// Anir.Api\Controllers\Administration\BackupController.cs
using Anir.Infrastructure.Maintenance;
using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace Anir.Api.Controllers.Administration;

[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly ILogger<BackupController> _logger;

    public BackupController(IBackupService backupService, ILogger<BackupController> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    /// <summary>
    /// Informacion de la version BD
    /// </summary>
    [HttpGet("info")]
    public async Task<ActionResult<ProcessResponse<BackupResult>>> GetSystemInfo(CancellationToken ct = default)
    {
        var result = await _backupService.GetSystemInfoAsync(ct);
        return Ok(ProcessResponse<BackupResult>.Success(result));
    }

    /// <summary>
    /// Genera un respaldo completo de la base de datos y los archivos físicos.
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<ProcessResponse<BackupResult>>> CreateBackup(CancellationToken ct = default)
    {
        try
        {
            var result = await _backupService.CreateBackupAsync(ct);
            return Ok(ProcessResponse<BackupResult>.Success(result, result.Message));
        }
        catch (InvalidOperationException ex)
        {
            // Error de configuración (falta la ruta de pg_dump) -> Esto es un 400, no un 500
            _logger.LogWarning(ex, "Intento de respaldo sin configurar herramienta");
            return BadRequest(ProcessResponse<BackupResult>.Fail(ex.Message));
        }
        // NOTA: Eliminamos el catch (Exception ex). 
        // Si pg_dump falla, o hay un error de IO, subirá al GlobalExceptionMiddleware 
        // que lo logueará en Serilog y devolverá un 500 limpio.
    }


    /// <summary>
    /// Elimina un respaldo completo de la base de datos y los archivos físicos.
    /// </summary>
    [HttpDelete("{fileName}")]
    public async Task<ActionResult<ProcessResponse<bool>>> DeleteBackup(string fileName, CancellationToken ct = default)
    {
        try
        {
            await _backupService.DeleteBackupAsync(fileName, ct);
            return Ok(ProcessResponse<bool>.Success(true, "Respaldo eliminado correctamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar respaldo {FileName}", fileName);
            return BadRequest(ProcessResponse<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Restaura de la base de datos y los archivos físicos.
    /// </summary>
    [HttpPost("restore")]
    [RequestSizeLimit(200 * 1024 * 1024)] // 200 MB
    public async Task<ActionResult<ProcessResponse<bool>>> RestoreBackup(IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ProcessResponse<bool>.Fail("No se recibió ningún archivo."));

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ProcessResponse<bool>.Fail("Solo se permiten archivos .zip."));

        try
        {
            var warningMsg = await _backupService.RestoreBackupAsync(file.OpenReadStream(), file.FileName, ct);

            var successMsg = string.IsNullOrEmpty(warningMsg)
                ? "Sistema restaurado correctamente."
                : warningMsg;

            return Ok(ProcessResponse<bool>.Success(true, successMsg));
        }
        catch (InvalidOperationException ex)
        {
            // Error de configuración o ZIP inválido (manifest incorrecto) -> 400
            _logger.LogWarning(ex, "Intento de restauración fallido por configuración o ZIP inválido");
            return BadRequest(ProcessResponse<bool>.Fail(ex.Message));
        }
        // NOTA: Eliminamos el catch genérico aquí también.
        // Si psql falla, o hay un error copiando archivos, el Middleware lo captura y devuelve 
        // ProcessResponse<string>.Fail("Error inesperado en el servidor.") sin filtrar datos sensibles.
    }

    /// <summary>
    /// Obtiene la lista de respaldos .zip existentes en el servidor.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ProcessResponse<List<BackupResult>>>> GetBackups(CancellationToken ct = default)
    {
        var result = await _backupService.GetBackupsAsync(ct);
        return Ok(ProcessResponse<List<BackupResult>>.Success(result));
    }

    /// <summary>
    /// Descarga un archivo de respaldo específico por su nombre.
    /// </summary>
    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadBackup(string fileName, CancellationToken ct = default)
    {
        var result = await _backupService.DownloadBackupAsync(fileName, ct);

        if (result is null)
            return NotFound("Archivo de respaldo no encontrado.");

        var (stream, name) = result.Value;

        return File(stream, "application/zip", name);
    }
}