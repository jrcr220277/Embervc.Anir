using Anir.Infrastructure.Storage;
using Anir.Shared;
using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace Anir.Api.Controllers;

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
    public async Task<ActionResult<FileResponse>> Upload([FromForm] IFormFile file, [FromForm] string folder)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        string extension = Path.GetExtension(file.FileName);

        string id = await _storage.SaveAsync(bytes, extension, folder);

        return Ok(new FileResponse
        {
            Id = id,
            Url = $"{Request.Scheme}://{Request.Host}/{id}",
            Name = file.FileName,
            Size = file.Length,
            Type = file.ContentType
        });
    }

    [HttpDelete("{folder}/{id}")]
    public async Task<ActionResult> Delete(string folder, string id)
    {
        return await _storage.DeleteAsync(id, folder) ? Ok() : NotFound();
    }
}
