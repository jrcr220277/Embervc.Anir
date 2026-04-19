using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

public class FileService
{
    private readonly HttpClient _http;
    private const long DefaultMaxStream = 30 * 1024 * 1024; // 30 MB por seguridad

    public FileService(HttpClient http) => _http = http;

    public async Task<FileResponse?> UploadAsync(IBrowserFile file, string folder, CancellationToken ct = default)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException("folder required", nameof(folder));
        folder = folder.ToLowerInvariant();
        if (folder != "images" && folder != "docs") throw new ArgumentException("folder must be 'images' or 'docs'");

        var content = new MultipartFormDataContent();

        long maxStream = folder == "images" ? 10 * 1024 * 1024 : 20 * 1024 * 1024;
        maxStream = Math.Max(maxStream, DefaultMaxStream);

        var streamContent = new StreamContent(file.OpenReadStream(maxStream, ct));
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.Name);

        using var response = await _http.PostAsync($"api/files/upload?folder={folder}", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Upload failed: {response.StatusCode} - {text}");
        }

        return await response.Content.ReadFromJsonAsync<FileResponse>(cancellationToken: ct);
    }

    public async Task<bool> DeleteAsync(string folder, string fileName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(fileName)) return false;
        using var response = await _http.DeleteAsync($"api/files/{folder}/{fileName}", ct);
        return response.IsSuccessStatusCode;
    }
}
