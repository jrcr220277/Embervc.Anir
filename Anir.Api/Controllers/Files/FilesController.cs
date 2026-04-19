using Anir.Infrastructure.Storage;
using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileStorage _storage;
    private const long MaxImageSize = 10 * 1024 * 1024;
    private const long MaxDocumentSize = 20 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
    private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

    public FilesController(IFileStorage storage)
    {
        _storage = storage;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ProcessResponse<FileResponse>>> Upload([FromForm] IFormFile file, [FromQuery] string folder)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ProcessResponse<FileResponse>.Fail("No se ha seleccionado ningún archivo."));

        if (string.IsNullOrWhiteSpace(folder))
            return BadRequest(ProcessResponse<FileResponse>.Fail("La carpeta es obligatoria."));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (folder != "images" && folder != "docs")
            return BadRequest(ProcessResponse<FileResponse>.Fail("La carpeta debe ser 'images' o 'docs'."));

        if (folder == "images")
        {
            if (!AllowedImageExtensions.Contains(extension))
                return BadRequest(ProcessResponse<FileResponse>.Fail("Tipo de archivo no permitido para images."));
            if (file.Length > MaxImageSize)
                return BadRequest(ProcessResponse<FileResponse>.Fail($"La imagen no puede superar los {MaxImageSize / 1024 / 1024} MB."));
        }
        else
        {
            if (!AllowedDocumentExtensions.Contains(extension))
                return BadRequest(ProcessResponse<FileResponse>.Fail("Tipo de archivo no permitido para docs."));
            if (file.Length > MaxDocumentSize)
                return BadRequest(ProcessResponse<FileResponse>.Fail($"El documento no puede superar los {MaxDocumentSize / 1024 / 1024} MB."));
        }

        await using var stream = file.OpenReadStream();
        string fileId = await _storage.SaveAsync(stream, extension, folder);

        var url = Url.Action(nameof(Get), "Files", new { folder = folder, fileName = fileId }, Request.Scheme, Request.Host.Value);

        return Ok(ProcessResponse<FileResponse>.Success(new FileResponse
        {
            Id = fileId,
            Name = file.FileName,
            Size = file.Length,
            Type = file.ContentType,
            Url = url
        }, "Archivo subido correctamente."));
    }

    [HttpGet("{folder}/{fileName}")]
    public async Task<IActionResult> Get(string folder, string fileName)
    {
        if (!IsSafePathComponent(folder) || !IsSafePathComponent(fileName))
            return BadRequest("Ruta no válida.");

        if (folder != "images" && folder != "docs")
            return BadRequest("La carpeta debe ser 'images' o 'docs'.");

        var file = await _storage.GetAsync(fileName, folder);
        if (file == null)
            return NotFound();

        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }

    [HttpDelete("{folder}/{fileName}")]
    public async Task<ActionResult<ProcessResponse<bool>>> Delete(string folder, string fileName)
    {
        if (!IsSafePathComponent(folder) || !IsSafePathComponent(fileName))
            return BadRequest(ProcessResponse<bool>.Fail("Ruta no válida."));

        if (folder != "images" && folder != "docs")
            return BadRequest(ProcessResponse<bool>.Fail("La carpeta debe ser 'images' o 'docs'."));

        bool deleted = await _storage.DeleteAsync(fileName, folder);
        if (!deleted)
            return NotFound(ProcessResponse<bool>.Fail("Archivo no encontrado."));

        return Ok(ProcessResponse<bool>.Success(true, "Archivo eliminado."));
    }

    private static bool IsSafePathComponent(string input)
    {
        return !string.IsNullOrEmpty(input) &&
               input.IndexOfAny(Path.GetInvalidFileNameChars()) == -1 &&
               !input.Contains("..") &&
               !input.Contains('/') &&
               !input.Contains('\\');
    }
}
