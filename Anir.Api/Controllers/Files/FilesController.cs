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

        // ⭐ Folder automático según tipo MIME
        string folder = file.ContentType.StartsWith("image")
            ? "images"
            : "docs";

        string id = await _storage.SaveAsync(bytes, extension, folder);

        return Ok(new FileResponse
        {
            Id = id, // images/xxxx.jpg o docs/xxxx.pdf
            Url = $"{Request.Scheme}://{Request.Host}/{id}",
            Name = file.FileName,
            Size = file.Length,
            Type = file.ContentType
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var folder = id.Split('/')[0];
        var fileName = id.Split('/')[1];

        return await _storage.DeleteAsync(fileName, folder)
            ? Ok()
            : NotFound();
    }
}
