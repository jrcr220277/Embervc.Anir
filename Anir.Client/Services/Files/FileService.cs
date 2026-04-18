using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Files;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

public class FileService
{
    private readonly HttpClient _http;

    public FileService(HttpClient http)
    {
        _http = http;
    }

    // ============================================================
    // UPLOAD TEMPORAL
    // ============================================================
    public async Task<FileResponse?> UploadTempAsync(IBrowserFile file)
    {
        var content = new MultipartFormDataContent();

        var streamContent = new StreamContent(file.OpenReadStream(10 * 1024 * 1024));
        streamContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        content.Add(streamContent, "file", file.Name);

        var response = await _http.PostAsync("api/files/upload-temp", content);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<FileResponse>();
    }

    // ============================================================
    // COMMIT FINAL
    // ============================================================
    public async Task<string?> CommitAsync(string tempId, string folder)
    {
        var request = new FileFinalizeRequest
        {
            TempId = tempId,
            Folder = folder
        };

        var response = await _http.PostAsJsonAsync("api/files/commit", request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<string>();
    }

    // ============================================================
    // DELETE
    // ============================================================
    public async Task<bool> DeleteAsync(string folder, string fileName)
    {
        if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(fileName))
            return false;

        var response = await _http.DeleteAsync($"api/files/{folder}/{fileName}");
        return response.IsSuccessStatusCode;
    }
}
