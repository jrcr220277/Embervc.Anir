using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace Anir.Client.Services.Files;

public class FileService
{
    private readonly HttpClient _http;

    public FileService(HttpClient http)
    {
        _http = http;
    }

    public async Task<FileResponse?> UploadAsync(IBrowserFile file)
    {
        var content = new MultipartFormDataContent();

        var streamContent = new StreamContent(file.OpenReadStream(10 * 1024 * 1024)); // 10 MB

        streamContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        content.Add(streamContent, "file", file.Name);

        var response = await _http.PostAsync("api/files/upload", content);

        return await response.Content.ReadFromJsonAsync<FileResponse>();
    }

    public async Task<bool> DeleteAsync(string fileId)
    {
        var response = await _http.DeleteAsync($"api/files/{fileId}");
        return response.IsSuccessStatusCode;
    }
}
