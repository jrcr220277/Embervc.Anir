// Anir.Client\Services\SystemSettings\BackupService.cs
using Anir.Shared.Contracts.Common;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using System.Text.Json;

namespace Anir.Client.Services.Administration;


public class BackupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly JsonSerializerOptions _jsonOptions;

    public BackupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null
        };
    }

    public async Task<ProcessResponse<BackupResult>?> GetSystemInfoAsync(CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();

        // IMPORTANTE: La ruta debe coincidir con tu Controller
        using var response = await http.GetAsync("/api/backup/info", ct);

        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<ProcessResponse<BackupResult>>(_jsonOptions, ct);
    }

    public async Task<ProcessResponse<BackupResult>?> CreateBackupAsync(CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();

        using var response = await http.PostAsync("/api/backup/create", null, ct);

        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<ProcessResponse<BackupResult>>(_jsonOptions, ct);
    }

    public async Task<ProcessResponse<bool>?> DeleteBackupAsync(string fileName, CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();

        using var response = await http.DeleteAsync($"/api/backup/{fileName}", ct);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<ProcessResponse<bool>>(_jsonOptions, ct);
    }

    public async Task<ProcessResponse<bool>?> RestoreBackupAsync(IBrowserFile file, CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();

        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream(200 * 1024 * 1024, ct); // 200MB Max
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        content.Add(streamContent, "file", file.Name);

        using var response = await http.PostAsync("/api/backup/restore", content, ct);

        return await response.Content.ReadFromJsonAsync<ProcessResponse<bool>>(_jsonOptions, ct);
    }

    public async Task<ProcessResponse<List<BackupResult>>?> GetBackupsAsync(CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();

        using var response = await http.GetAsync("/api/backup", ct);

        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<ProcessResponse<List<BackupResult>>>(_jsonOptions, ct);
    }

    // Para descargar, devolvemos los bytes y usamos JS Interop como haces con los Excel
    public async Task<byte[]?> DownloadBackupAsync(string fileName, CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();

        using var response = await http.GetAsync($"/api/backup/download/{fileName}", ct);

        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadAsByteArrayAsync(ct);
    }
}