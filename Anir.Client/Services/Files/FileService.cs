using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace Anir.Web.Client.Services;

public class FileService
{
    private readonly HttpClient _http;
    private const long MaxFileSize = 30 * 1024 * 1024; // 30 MB

    public FileService(HttpClient http) => _http = http;

    /// <summary>
    /// Sube un archivo al servidor y retorna sus datos.
    /// </summary>
    /// <param name="file">Archivo desde MudFileUpload o InputFile</param>
    /// <param name="folder">Carpeta destino (ej: "images/persons", "docs/works")</param>
    public async Task<FileResponse?> UploadAsync(IBrowserFile file, string folder, CancellationToken ct = default)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException("La carpeta es obligatoria.", nameof(folder));

        using var content = new MultipartFormDataContent();

        await using var stream = file.OpenReadStream(MaxFileSize, ct);
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        content.Add(streamContent, "file", file.Name);

        using var response = await _http.PostAsync($"api/files/upload?folder={folder}", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error al subir el archivo: {response.StatusCode}");
        }

        // CORREGIDO: Leemos directamente FileResponse, porque el controlador devuelve Ok(FileResponse)
        return await response.Content.ReadFromJsonAsync<FileResponse>(cancellationToken: ct);
    }

    /// <summary>
    /// Elimina un archivo del servidor usando su ID.
    /// </summary>
    public async Task<bool> DeleteAsync(int fileId, CancellationToken ct = default)
    {
        if (fileId <= 0) return false;

        using var response = await _http.DeleteAsync($"api/files/{fileId}", ct);
        return response.IsSuccessStatusCode;
    }
}