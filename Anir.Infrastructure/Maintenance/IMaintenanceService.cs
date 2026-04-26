// Anir.Infrastructure\Maintenance\IMaintenanceService.cs
using Anir.Shared.Contracts.Common;

namespace Anir.Infrastructure.Maintenance;

public interface IMaintenanceService
{
    /// <summary>
    /// Escanea el sistema en busca de archivos huérfanos y referencias rotas, 
    /// limpiándolos de forma segura sin borrar datos de negocio.
    /// </summary>
    Task<CleanupResult> CleanOrphansAsync(CancellationToken ct = default);
}