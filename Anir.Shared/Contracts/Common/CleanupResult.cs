// Anir.Shared\Contracts\Common\CleanupResultDto.cs
namespace Anir.Shared.Contracts.Common;

public class CleanupResult
{
    // Archivos físicos borrados (estaban en disco pero no en la BD)
    public int OrphanFilesDeleted { get; set; }

    // Registros en tabla StoredFile borrados (estaban en BD pero no en disco)
    public int PhantomRecordsDeleted { get; set; }

    // Referencias limpiadas (Ej: AnirWork.ImageFileId puestos a NULL)
    public int BrokenReferencesFixed { get; set; }

    // Espacio liberado en disco en bytes
    public long SpaceFreedBytes { get; set; }

    // Mensaje legible
    public string Summary => $"Limpieza completada: {OrphanFilesDeleted} archivos huérfanos eliminados, " +
                             $"{PhantomRecordsDeleted} registros fantasma borrados, " +
                             $"{BrokenReferencesFixed} referencias rotas reparadas. " +
                             $"Espacio liberado: {FormatSize(SpaceFreedBytes)}";

    private static string FormatSize(long bytes) =>
        bytes < 1024 ? $"{bytes} B" :
        bytes < 1024 * 1024 ? $"{bytes / 1024.0:0.0} KB" :
        $"{bytes / (1024.0 * 1024.0):0.1} MB";
}