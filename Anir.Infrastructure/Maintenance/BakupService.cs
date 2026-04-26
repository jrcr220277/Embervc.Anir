// Anir.Infrastructure\Maintenance\BackupService.cs
using Anir.Data;
using Anir.Infrastructure.Settings;
using Anir.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace Anir.Infrastructure.Maintenance;

public class BackupService : IBackupService
{
    private readonly ApplicationDbContext _db;
    private readonly string _storageRootPath;
    private readonly string _backupsRootPath;
    private readonly ILogger<BackupService> _logger;

    public BackupService(
        ApplicationDbContext db,
        IOptions<FileStorageSettings> fileSettings,
        ILogger<BackupService> logger)
    {
        _db = db;
        _logger = logger;

        var storagePath = fileSettings.Value.RootPath;
        _storageRootPath = Path.IsPathRooted(storagePath) ? storagePath : Path.Combine(Directory.GetCurrentDirectory(), storagePath);

        _backupsRootPath = Path.Combine(Directory.GetCurrentDirectory(), fileSettings.Value.BackupsFolder);
        if (!Directory.Exists(_backupsRootPath)) Directory.CreateDirectory(_backupsRootPath);
    }

    public async Task<BackupResult> GetSystemInfoAsync(CancellationToken ct = default)
    {
        var currentMigration = (await _db.Database.GetAppliedMigrationsAsync(ct)).LastOrDefault() ?? "Unknown";
        var appVersion = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";

        return new BackupResult
        {
            AppVersion = appVersion,
            LastMigration = currentMigration
        };
    }

    public async Task<BackupResult> CreateBackupAsync(CancellationToken ct = default)
    {
        var settings = await _db.SystemSettings.FirstOrDefaultAsync(ct);
        if (settings == null || string.IsNullOrWhiteSpace(settings.BackupToolPath))
            throw new InvalidOperationException("No se ha configurado la ruta de la herramienta de respaldo en el sistema.");

        if (!File.Exists(settings.BackupToolPath))
            throw new FileNotFoundException($"No se encontró la herramienta de respaldo en: {settings.BackupToolPath}");
        // ============================================================
        // NUEVO: GENERAR MANIFIESTO
        // ============================================================
        var currentMigration = (await _db.Database.GetAppliedMigrationsAsync(ct)).LastOrDefault() ?? "Unknown";
        var appVersion = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupName = $"ANIR_Backup_{timestamp}_v{appVersion}";
        var tempFolder = Path.Combine(Path.GetTempPath(), backupName);
        var sqlFilePath = Path.Combine(tempFolder, "database_backup.sql");
        var finalZipPath = Path.Combine(_backupsRootPath, $"{backupName}.zip");

        try
        {
            Directory.CreateDirectory(tempFolder);
            await ExportDatabaseAsync(settings.BackupToolPath, sqlFilePath, ct);

            if (Directory.Exists(_storageRootPath))
            {
                var filesDestPath = Path.Combine(tempFolder, "AnirStorage");
                foreach (string dir in Directory.GetDirectories(_storageRootPath, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dir.Replace(_storageRootPath, filesDestPath));
                foreach (string file in Directory.GetFiles(_storageRootPath, "*", SearchOption.AllDirectories))
                    File.Copy(file, file.Replace(_storageRootPath, filesDestPath), true);
            }

            
            var manifest = new BackupManifest
            {
                AppVersion = appVersion,
                LastMigration = currentMigration,
                CreatedAt = DateTime.UtcNow
            };

            var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(Path.Combine(tempFolder, "manifest.json"), manifestJson, ct);
            // ============================================================

            ZipFile.CreateFromDirectory(tempFolder, finalZipPath);
            ApplyBackupRotation(settings.MaxBackupFiles);

            var fileInfo = new FileInfo(finalZipPath);
            return new BackupResult
            {
                FileName = fileInfo.Name,
                SizeBytes = fileInfo.Length,
                AppVersion = appVersion,           
                LastMigration = currentMigration   
            };
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                try { Directory.Delete(tempFolder, true); }
                catch { /* Ignorar */ }
            }
        }
    }

    public Task DeleteBackupAsync(string fileName, CancellationToken ct = default)
    {
        if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
            throw new UnauthorizedAccessException("Nombre de archivo inválido.");

        var filePath = Path.Combine(_backupsRootPath, fileName);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Archivo no encontrado.");

        File.Delete(filePath);
        return Task.CompletedTask;
    }
    public async Task<string> RestoreBackupAsync(Stream zipStream, string fileName, CancellationToken ct = default)
    {
        var settings = await _db.SystemSettings.FirstOrDefaultAsync(ct);
        if (settings == null || string.IsNullOrWhiteSpace(settings.BackupToolPath))
            throw new InvalidOperationException("No se ha configurado la ruta de la herramienta de respaldo.");

        var psqlPath = settings.BackupToolPath.Replace("pg_dump.exe", "psql.exe", StringComparison.OrdinalIgnoreCase);
        if (!File.Exists(psqlPath))
            throw new FileNotFoundException($"No se encontró la herramienta de restauración (psql) en: {psqlPath}");

        var tempFolder = Path.Combine(Path.GetTempPath(), $"ANIR_Restore_{DateTime.Now:yyyyMMdd_HHmmss}");
        string warningMessage = string.Empty;

        try
        {
            Directory.CreateDirectory(tempFolder);
            var zipFilePath = Path.Combine(tempFolder, fileName);

            using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
            {
                await zipStream.CopyToAsync(fileStream, ct);
            }

            ZipFile.ExtractToDirectory(zipFilePath, tempFolder);

            // ============================================================
            // NUEVO: VALIDAR MANIFIESTO
            // ============================================================
            var manifestPath = Path.Combine(tempFolder, "manifest.json");
            if (!File.Exists(manifestPath))
                throw new InvalidOperationException("El archivo ZIP no es un respaldo válido de ANIR (falta manifest.json).");

            var manifestJson = await File.ReadAllTextAsync(manifestPath, ct);
            var manifest = JsonSerializer.Deserialize<BackupManifest>(manifestJson);

            if (manifest?.SystemId != BackupManifest.SystemSignature)
                throw new InvalidOperationException("El archivo ZIP no es un respaldo válido de ANIR.");

            var currentMigration = (await _db.Database.GetAppliedMigrationsAsync(ct)).LastOrDefault() ?? "Unknown";

            if (manifest.LastMigration != currentMigration)
            {
                _logger.LogError("Intento de restauración BLOQUEADO por incompatibilidad de versión. Respaldo: {BackupVersion}, Sistema: {CurrentVersion}", manifest.LastMigration, currentMigration);

                // BLOQUEO DURO: Lanzamos excepción para detener la restauración inmediatamente.
                // La base de datos actual queda INTACTA.
                throw new InvalidOperationException(
                    $"🚫 OPERACIÓN BLOQUEADA: El respaldo pertenece a la versión App v{manifest.AppVersion} (Migración: {manifest.LastMigration}), " +
                    $"pero tu sistema actual está en la Migración: {currentMigration}. " +
                    $"Restaurar este archivo destruiría la estructura de la base de datos actual. " +
                    $"Solo puedes restaurar respaldos generados por esta misma versión del sistema."
                );
            }
            // ============================================================

            var sqlFilePath = Directory.GetFiles(tempFolder, "database_backup.sql").FirstOrDefault();
            if (sqlFilePath != null)
            {
                await ImportDatabaseAsync(psqlPath, sqlFilePath, ct);
            }

            var extractedStoragePath = Directory.GetDirectories(tempFolder, "AnirStorage").FirstOrDefault();
            if (extractedStoragePath != null && Directory.Exists(_storageRootPath))
            {
                foreach (var file in Directory.GetFiles(_storageRootPath, "*", SearchOption.AllDirectories))
                    File.Delete(file);

                foreach (string dir in Directory.GetDirectories(extractedStoragePath, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dir.Replace(extractedStoragePath, _storageRootPath));
                foreach (string file in Directory.GetFiles(extractedStoragePath, "*", SearchOption.AllDirectories))
                    File.Copy(file, file.Replace(extractedStoragePath, _storageRootPath), true);
            }

            return warningMessage; // Devuelve advertencia o vacío
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                try { Directory.Delete(tempFolder, true); }
                catch { /* Ignorar */ }
            }
        }
    }
 
    private async Task ImportDatabaseAsync(string psqlPath, string sqlFilePath, CancellationToken ct)
    {
        var connString = _db.Database.GetConnectionString();
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connString);

        var startInfo = new ProcessStartInfo
        {
            FileName = psqlPath,
            Arguments = $"--host={builder.Host} --port={builder.Port} --username={builder.Username} --dbname={builder.Database} --no-password --file=\"{sqlFilePath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        startInfo.EnvironmentVariables["PGPASSWORD"] = builder.Password;

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        string errors = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            _logger.LogError("Error ejecutando psql: {Error}", errors);
            throw new Exception($"Error en psql: {errors}");
        }
    }

    private async Task ExportDatabaseAsync(string pgDumpPath, string outputSqlPath, CancellationToken ct)
    {
        var connString = _db.Database.GetConnectionString();
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connString);

        var startInfo = new ProcessStartInfo
        {
            FileName = pgDumpPath,
            Arguments = $"--clean --host={builder.Host} --port={builder.Port} --username={builder.Username} --dbname={builder.Database} --no-password --file=\"{outputSqlPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        // Inyectar la contraseña de forma segura por variable de entorno
        startInfo.EnvironmentVariables["PGPASSWORD"] = builder.Password;

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        // Leer output para evitar deadlocks
        string errors = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            _logger.LogError("Error ejecutando pg_dump: {Error}", errors);
            throw new Exception($"Error en pg_dump: {errors}");
        }
    }

    private void ApplyBackupRotation(int maxFiles)
    {
        var backupFiles = new DirectoryInfo(_backupsRootPath)
            .GetFiles("ANIR_Backup_*.zip")
            .OrderBy(f => f.CreationTime)
            .ToList();

        // Si hay más archivos de los permitidos, borramos los más viejos
        while (backupFiles.Count > maxFiles)
        {
            var oldest = backupFiles.First();
            _logger.LogInformation("Rotación de respaldos: Eliminando archivo antiguo {File}", oldest.Name);
            oldest.Delete();
            backupFiles.Remove(oldest);
        }
    }

    public Task<List<BackupResult>> GetBackupsAsync(CancellationToken ct = default)
    {
        if (!Directory.Exists(_backupsRootPath)) return Task.FromResult(new List<BackupResult>());

        var files = new DirectoryInfo(_backupsRootPath)
            .GetFiles("ANIR_Backup_*.zip")
            .OrderByDescending(f => f.CreationTime) // Los más nuevos primero
            .Select(f => new BackupResult
            {
                FileName = f.Name,
                SizeBytes = f.Length
            })
            .ToList();

        return Task.FromResult(files);
    }

    public Task<(Stream Stream, string FileName)?> DownloadBackupAsync(string fileName, CancellationToken ct = default)
    {
        // Seguridad básica: evitar path traversal
        if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
            return Task.FromResult<(Stream Stream, string FileName)?>(null);

        var filePath = Path.Combine(_backupsRootPath, fileName);
        if (!File.Exists(filePath))
            return Task.FromResult<(Stream Stream, string FileName)?>(null);

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<(Stream Stream, string FileName)?>((stream, fileName));
    }
}