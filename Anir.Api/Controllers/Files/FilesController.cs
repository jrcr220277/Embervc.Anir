using Anir.Infrastructure.Storage;
using Anir.Shared.Contracts.Common;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileStorage _storage;

    public FilesController(IFileStorage storage)
    {
        _storage = storage;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<FileResponse>> Upload([FromForm] IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        string extension = Path.GetExtension(file.FileName);

        string folder = file.ContentType.StartsWith("image")
            ? "images"
            : "docs";

        // ⭐ SaveAsync devuelve SOLO el nombre del archivo
        string fileName = await _storage.SaveAsync(bytes, extension, folder);

        return Ok(new FileResponse
        {
            Id = fileName, // ⭐ SOLO el nombre
            Url = $"{Request.Scheme}://{Request.Host}/{folder}/{fileName}",
            Name = file.FileName,
            Size = file.Length,
            Type = file.ContentType
        });
    }

    [HttpDelete("{folder}/{fileName}")]
    public async Task<ActionResult> Delete(string folder, string fileName)
    {
        return await _storage.DeleteAsync(fileName, folder)
            ? Ok()
            : NotFound();
    }
}
