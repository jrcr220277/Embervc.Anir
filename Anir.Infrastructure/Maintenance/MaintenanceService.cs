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
        // PASO 2: Escanear disco vs BD (Archivos Huérfanos Físicos)
        // ============================================================
        var physicalFiles = Directory.GetFiles(_rootPath, "*", SearchOption.AllDirectories);
        var validDbFileNames = new HashSet<string>(dbFiles.Values.Select(f => f.FileName), StringComparer.OrdinalIgnoreCase);
        var foundDbFileIds = new HashSet<int>();

        foreach (var physicalPath in physicalFiles)
        {
            var fileName = Path.GetFileName(physicalPath);

            if (validDbFileNames.Contains(fileName))
            {
                var dbFile = dbFiles.Values.First(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                foundDbFileIds.Add(dbFile.Id);
            }
            else
            {
                // Archivo en disco sin registro en BD
                try
                {
                    var fileInfo = new FileInfo(physicalPath);
                    result.SpaceFreedBytes += fileInfo.Length;
                    fileInfo.Delete();
                    result.OrphanFilesDeleted++;
                    _logger.LogInformation("Archivo huérfano (solo disco) eliminado: {File}", fileName);
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
            // Reparar referencias rotas (FK apuntando a StoredFile que ya no existe en disco)
            var storedFileEntity = _db.Model.FindEntityType(typeof(StoredFile));
            if (storedFileEntity != null)
            {
                var referencingForeignKeys = storedFileEntity.GetReferencingForeignKeys();

                foreach (var fk in referencingForeignKeys)
                {
                    var principalEntityType = fk.DeclaringEntityType;
                    var fkProperty = fk.Properties.First();
                    var table = StoreObjectIdentifier.Table(principalEntityType.GetTableName()!, principalEntityType.GetSchema());
                    string tableName = table.Name;
                    string columnName = fkProperty.GetColumnName(table)!;

                    // Optimización: un solo UPDATE con IN en lugar de muchos individuales
                    if (phantomIds.Any())
                    {
                        var idsList = string.Join(",", phantomIds);
                        try
                        {
                            int rowsAffected = await _db.Database.ExecuteSqlRawAsync(
                                $"UPDATE \"{tableName}\" SET \"{columnName}\" = NULL WHERE \"{columnName}\" IN ({idsList})",
                                ct);
                            if (rowsAffected > 0)
                            {
                                result.BrokenReferencesFixed += rowsAffected;
                                _logger.LogInformation("Referencias rotas reparadas en {Table}.{Column}: {Count} filas actualizadas", tableName, columnName, rowsAffected);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al limpiar FK {Table}.{Column} para los IDs {Ids}", tableName, columnName, idsList);
                        }
                    }
                }
            }

            // Eliminar los registros fantasma de StoredFile
            var phantomRecords = dbFiles.Values.Where(f => phantomIds.Contains(f.Id)).ToList();
            _db.StoredFiles.RemoveRange(phantomRecords);
            result.PhantomRecordsDeleted = await _db.SaveChangesAsync(ct);
            // Re-cargar dbFiles actualizado para el siguiente paso (opcional, pero limpia)
            dbFiles = await _db.StoredFiles.ToDictionaryAsync(f => f.Id, f => f, ct);
        }

        // ============================================================
        // PASO 4: ELIMINAR STOREDFILE NO REFERENCIADOS POR NINGUNA FK (HUÉRFANOS LÓGICOS)
        // ============================================================
        // Obtener todas las FKs que apuntan a StoredFile de forma dinámica
        var storedFileEntityType = _db.Model.FindEntityType(typeof(StoredFile));
        if (storedFileEntityType == null) return result;

        var referencingFks = storedFileEntityType.GetReferencingForeignKeys().ToList();
        if (!referencingFks.Any()) return result;

        // Construir una lista de SQL queries para obtener todos los IDs referenciados
        var referencedIds = new HashSet<int>();
        foreach (var fk in referencingFks)
        {
            var declaringType = fk.DeclaringEntityType;
            var tableName = declaringType.GetTableName();
            var schema = declaringType.GetSchema();
            var fkProperty = fk.Properties.First();
            var columnName = fkProperty.GetColumnName(StoreObjectIdentifier.Table(tableName!, schema!));

            // Consulta: SELECT DISTINCT {columnName} FROM {tableName} WHERE {columnName} IS NOT NULL
            var sql = $"SELECT DISTINCT \"{columnName}\" FROM \"{tableName}\" WHERE \"{columnName}\" IS NOT NULL";
            var ids = await _db.Database.SqlQueryRaw<int>(sql).ToListAsync(ct);
            referencedIds.UnionWith(ids);
        }

        // Los StoredFile cuyo Id no está en referencedIds son huérfanos lógicos
        var logicalOrphans = dbFiles.Values.Where(sf => !referencedIds.Contains(sf.Id)).ToList();
        foreach (var orphan in logicalOrphans)
        {
            // 1. Eliminar archivo físico si existe
            var fullPath = Path.Combine(_rootPath, orphan.Folder, orphan.FileName);
            if (File.Exists(fullPath))
            {
                try
                {
                    var fileInfo = new FileInfo(fullPath);
                    result.SpaceFreedBytes += fileInfo.Length;
                    fileInfo.Delete();
                    result.OrphanFilesDeleted++;
                    _logger.LogInformation("Archivo huérfano lógico (sin referencias) eliminado: {Path}", fullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al eliminar archivo huérfano lógico: {Path}", fullPath);
                }
            }
            else
            {
                // No existe en disco, es un registro fantasma (ya se habría tratado en paso 3)
                // Pero lo contamos como phantom (por si acaso)
                result.PhantomRecordsDeleted++;
            }

            // 2. Eliminar el registro de StoredFile
            _db.StoredFiles.Remove(orphan);
        }

        if (logicalOrphans.Any())
            await _db.SaveChangesAsync(ct);

        return result;
    }
}