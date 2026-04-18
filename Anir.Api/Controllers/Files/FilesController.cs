using Anir.Infrastructure.Storage;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Files;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controlador responsable de manejar la subida, confirmación,
/// obtención y eliminación de archivos.
/// Implementa un flujo temporal → commit final.
/// </summary>
[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileStorage _storage;

    public FilesController(IFileStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Sube un archivo a la carpeta temporal.
    /// Se usa para previsualización antes de guardar el formulario.
    /// </summary>
    [HttpPost("upload-temp")]
    public async Task<ActionResult<FileResponse>> UploadTemp([FromForm] IFormFile file)
    {
        string extension = Path.GetExtension(file.FileName);

        // .NET 10 — forma moderna, eficiente y recomendada
        await using var uploadStream = new MemoryStream();
        await file.CopyToAsync(uploadStream);
        uploadStream.Position = 0;


        string tempId = await _storage.SaveTempAsync(uploadStream, extension);

        return Ok(new FileResponse
        {
            Id = tempId,
            Name = file.FileName,
            Size = file.Length,
            Type = file.ContentType
            // Url se llena solo cuando el archivo está en carpeta final
        });
    }

    /// <summary>
    /// Confirma un archivo temporal y lo mueve a su carpeta final.
    /// </summary>
    [HttpPost("commit")]
    public async Task<ActionResult<string>> Commit([FromBody] FileFinalizeRequest request)
    {
        string finalId = await _storage.CommitAsync(request.TempId, request.Folder);
        return Ok(finalId);
    }

    /// <summary>
    /// Devuelve un archivo como stream.
    /// No expone rutas físicas ni usa wwwroot.
    /// </summary>
    [HttpGet("{folder}/{fileName}")]
    public async Task<IActionResult> Get(string folder, string fileName)
    {
        var file = await _storage.GetAsync(fileName, folder);
        if (file == null)
            return NotFound();

        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }

    /// <summary>
    /// Elimina un archivo de cualquier carpeta.
    /// </summary>
    [HttpDelete("{folder}/{fileName}")]
    public async Task<IActionResult> Delete(string folder, string fileName)
    {
        bool ok = await _storage.DeleteAsync(fileName, folder);
        return ok ? Ok() : NotFound();
    }
}
