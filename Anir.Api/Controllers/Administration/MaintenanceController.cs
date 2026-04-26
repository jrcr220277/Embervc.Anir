// Anir.Api\Controllers\MaintenanceController.cs
using Anir.Infrastructure.Maintenance;
using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace Anir.Api.Controllers.Administration;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    /// <summary>
    /// Ejecuta la limpieza de archivos huérfanos y referencias rotas en el sistema.
    /// </summary>
    [HttpPost("clean-orphans")]
    public async Task<ActionResult<ProcessResponse<CleanupResult>>> CleanOrphans(CancellationToken ct = default)
    {
        var result = await _maintenanceService.CleanOrphansAsync(ct);

        return Ok(ProcessResponse<CleanupResult>.Success(result, result.Summary));
    }
}