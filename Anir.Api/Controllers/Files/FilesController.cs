using Anir.Application.Common.Interfaces;
using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anir.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;

    public FilesController(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>
    /// Sube un archivo al servidor.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB máximo
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromQuery] string folder = "general",
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No se recibió ningún archivo.");

        var storedFile = await _fileStorage.SaveAsync(file.OpenReadStream(), file.FileName, folder, ct);

        var response = new FileResponse
        {
            Id = storedFile.Id,
            // ANTES: Url = $"/api/files/{storedFile.Id}",
            // AHORA: Url Absoluta para que Blazor WASM pueda verla
            Url = $"{Request.Scheme}://{Request.Host}/api/files/{storedFile.Id}",
            Name = storedFile.OriginalName,
            Size = storedFile.SizeBytes,
            Type = storedFile.MimeType
        };

        return Ok(response);
    }

    /// <summary>
    /// Obiene o descarga un archivo por su ID.
    /// Las imágenes se muestran inline, los PDFs se descargan.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct = default)
    {
        var result = await _fileStorage.ReadAsync(id, ct);
        if (result is null)
            return NotFound("Archivo no encontrado.");

        var (stream, mimeType, originalName) = result.Value;

        // Si es imagen o PDF, forzamos que se abra en el navegador (inline)
        if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
            mimeType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            Response.Headers.Append("Content-Disposition", "inline");
            return File(stream, mimeType);
        }

        // El resto (Word, Excel, etc) se descargan con su nombre original
        return File(stream, mimeType, originalName);
    }

    /// <summary>
    /// Elimina un archivo del servidor y la base de datos.
    /// </summary>
    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _fileStorage.DeleteAsync(id, ct);
        return NoContent();
    }
}