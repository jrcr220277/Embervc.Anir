using Anir.Shared;
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

    public async Task<FileResponse?> UploadAsync(IBrowserFile file, string folder)
    {
        var content = new MultipartFormDataContent();

        // Convertimos el archivo a StreamContent
        var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 1024));
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        content.Add(streamContent, "file", file.Name);
        content.Add(new StringContent(folder), "folder");

        var response = await _http.PostAsync("api/files/upload", content);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<FileResponse>();
    }

    public async Task<bool> DeleteAsync(string folder, string fileId)
    {
        var response = await _http.DeleteAsync($"api/files/{folder}/{fileId}");
        return response.IsSuccessStatusCode;
    }
}
