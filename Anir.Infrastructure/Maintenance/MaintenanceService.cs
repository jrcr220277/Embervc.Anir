// Anir.Infrastructure\Maintenance\MaintenanceService.cs
using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Settings;
using Anir.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Anir.Infrastructure.Maintenance;

public class MaintenanceService : IMaintenanceService
{
    private readonly ApplicationDbContext _db;
    private readonly string _rootPath;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(
        ApplicationDbContext db,
        IOptions<FileStorageSettings> settings,
        ILogger<MaintenanceService> logger)
    {
        _db = db;
        _logger = logger;

        var path = settings.Value.RootPath;
        if (!Path.IsPathRooted(path))
            path = Path.Combine(Directory.GetCurrentDirectory(), path);

        _rootPath = path;
    }

    public async Task<CleanupResult> CleanOrphansAsync(CancellationToken ct = default)
    {
        var result = new CleanupResult();

        if (!Directory.Exists(_rootPath))
        {
            _logger.LogWarning("El directorio de almacenamiento no existe: {Path}", _rootPath);
            return result;
        }

        // ============================================================
        // PASO 1: Cargar todos los registros de la BD en memoria (solo metadata)
        // ============================================================
        var dbFiles = await _db.StoredFiles
            .ToDictionaryAsync(f => f.Id, f => f, ct);

        // ============================================================
        // PASO 2: Escanear disco vs BD (Archivos Huérfanos)
        // ============================================================
        var physicalFiles = Directory.GetFiles(_rootPath, "*", SearchOption.AllDirectories);
        var validDbFileNames = new HashSet<string>(dbFiles.Values.Select(f => f.FileName), StringComparer.OrdinalIgnoreCase);
        var foundDbFileIds = new HashSet<int>();

        foreach (var physicalPath in physicalFiles)
        {
            var fileName = Path.GetFileName(physicalPath);

            if (validDbFileNames.Contains(fileName))
            {
                // El archivo físico SÍ está en la BD. Lo marcamos como "encontrado".
                var dbFile = dbFiles.Values.First(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                foundDbFileIds.Add(dbFile.Id);
            }
            else
            {
                // ARCHIVO HUÉRFANO: Está en disco pero NO en BD -> Borrar de disco
                try
                {
                    var fileInfo = new FileInfo(physicalPath);
                    result.SpaceFreedBytes += fileInfo.Length;
                    fileInfo.Delete();
                    result.OrphanFilesDeleted++;
                    _logger.LogInformation("Archivo huérfano eliminado: {File}", fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al eliminar archivo huérfano: {File}", fileName);
                }
            }
        }

        // ============================================================
        // PASO 3: Detectar Registros Fantasma (En BD pero NO en disco)
        // ============================================================
        var phantomIds = dbFiles.Keys.Except(foundDbFileIds).ToList();

        if (phantomIds.Any())
        {
            // MAGIA DE EF CORE: Buscar dinámicamente todas las FK que apuntan a StoredFile
            var storedFileEntity = _db.Model.FindEntityType(typeof(StoredFile));
            if (storedFileEntity != null)
            {
                var referencingForeignKeys = storedFileEntity.GetReferencingForeignKeys();

                foreach (var fk in referencingForeignKeys)
                {
                    var principalEntityType = fk.DeclaringEntityType; // Ej: AnirWork
                    var fkProperty = fk.Properties.First();           // Ej: ImageFileId

                    var table = StoreObjectIdentifier.Table(
                        principalEntityType.GetTableName()!,
                        principalEntityType.GetSchema());

                    string tableName = table.Name;
                    string columnName = fkProperty.GetColumnName(table)!;

                    // Ejecutar: UPDATE "AnirWorks" SET "ImageFileId" = NULL WHERE "ImageFileId" IN (1, 2, 3...)
                    // Lo hacemos iterando para que el log sea claro de qué ID se limpió
                    foreach (var phantomId in phantomIds)
                    {
                        try
                        {
                            // PostgreSQL usa comillas dobles para identificadores
                            int rowsAffected = await _db.Database.ExecuteSqlRawAsync(
                                $"UPDATE \"{tableName}\" SET \"{columnName}\" = NULL WHERE \"{columnName}\" = {{0}}",
                                phantomId, ct);

                            if (rowsAffected > 0)
                            {
                                result.BrokenReferencesFixed += rowsAffected;
                                _logger.LogInformation("Referencia rota reparada: {Table}.{Column} = {Id} puesto a NULL", tableName, columnName, phantomId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al limpiar FK {Table}.{Column} para StoredFile {Id}", tableName, columnName, phantomId);
                        }
                    }
                }
            }

            // Finalmente, borramos los registros fantasma de la tabla StoredFile
            var phantomRecords = dbFiles.Values.Where(f => phantomIds.Contains(f.Id)).ToList();
            _db.StoredFiles.RemoveRange(phantomRecords);
            result.PhantomRecordsDeleted = await _db.SaveChangesAsync(ct);
        }

        return result;
    }
}