// Anir.Infrastructure\Maintenance\IBackupService.cs
using Anir.Shared.Contracts.Common;

namespace Anir.Infrastructure.Maintenance;

public interface IBackupService
{
    Task<BackupResult> GetSystemInfoAsync(CancellationToken ct = default);
    Task<BackupResult> CreateBackupAsync(CancellationToken ct = default);
    Task DeleteBackupAsync(string fileName, CancellationToken ct = default);
    Task<List<BackupResult>> GetBackupsAsync(CancellationToken ct = default);
    Task<(Stream Stream, string FileName)?> DownloadBackupAsync(string fileName, CancellationToken ct = default);

    // Retorna un string: vacío si todo ok, o un mensaje de advertencia si hay diferencias de versión
    Task<string> RestoreBackupAsync(Stream zipStream, string fileName, CancellationToken ct = default);
}