// Anir.Shared\Contracts\Common\BackupResult.cs
namespace Anir.Shared.Contracts.Common;

public class BackupResult
{
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string FormattedSize =>
        SizeBytes < 1024 ? $"{SizeBytes} B" :
        SizeBytes < 1024 * 1024 ? $"{SizeBytes / 1024.0:0.0} KB" :
        $"{SizeBytes / (1024.0 * 1024.0):0.1} MB";

    public string Message => $"Respaldo '{FileName}' ({FormattedSize}) generado correctamente.";

    // ============================================================
    // INFO DEL SISTEMA (Se usan para mostrar la versión actual en la UI)
    // ============================================================
    public string AppVersion { get; set; } = string.Empty;
    public string LastMigration { get; set; } = string.Empty;
}