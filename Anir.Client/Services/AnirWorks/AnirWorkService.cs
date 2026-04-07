using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.AnirWorks;

namespace Anir.Client.Services.AnirWorks;

public class AnirWorkService : IAnirWorkService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AnirWorkService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null // PascalCase
        };
    }

    private StringContent ToJsonContent<T>(T model)
    {
        var json = JsonSerializer.Serialize(model, _jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, ct);
        }
        catch
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<T>(raw, _jsonOptions);
        }
    }

    // ============================================================
    // GET PAGED
    // ============================================================
    public async Task<ProcessResponse<PagedResponse<AnirWorkDto>>> GetPagedAsync(
        AnirWorkQueryDto query,
        CancellationToken ct = default)
    {
        using var content = ToJsonContent(query);
        using var response = await _httpClient.PostAsync("/api/anirwork/getpaged", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<PagedResponse<AnirWorkDto>>>(response, ct);
            return body ?? ProcessResponse<PagedResponse<AnirWorkDto>>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<PagedResponse<AnirWorkDto>>>(response, ct);
        return result ?? ProcessResponse<PagedResponse<AnirWorkDto>>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    public async Task<ProcessResponse<AnirWorkDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync($"/api/anirwork/{id}", ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<AnirWorkDto>>(response, ct);
            return body ?? ProcessResponse<AnirWorkDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<AnirWorkDto>>(response, ct);
        return result ?? ProcessResponse<AnirWorkDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // CREATE
    // ============================================================
    public async Task<ProcessResponse<AnirWorkDto>> CreateAsync(AnirWorkDto dto, CancellationToken ct = default)
    {
        using var content = ToJsonContent(dto);
        using var response = await _httpClient.PostAsync("/api/anirwork", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<AnirWorkDto>>(response, ct);
            return body ?? ProcessResponse<AnirWorkDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<AnirWorkDto>>(response, ct);
        return result ?? ProcessResponse<AnirWorkDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // UPDATE
    // ============================================================
    public async Task<ProcessResponse<AnirWorkDto>> UpdateAsync(int id, AnirWorkDto dto, CancellationToken ct = default)
    {
        using var content = ToJsonContent(dto);
        using var response = await _httpClient.PutAsync($"/api/anirwork/{id}", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<AnirWorkDto>>(response, ct);
            return body ?? ProcessResponse<AnirWorkDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<AnirWorkDto>>(response, ct);
        return result ?? ProcessResponse<AnirWorkDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // DELETE
    // ============================================================
    public async Task<ProcessResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        using var response = await _httpClient.DeleteAsync($"/api/anirwork/{id}", ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<bool>>(response, ct);
            return body ?? ProcessResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<bool>>(response, ct);
        return result ?? ProcessResponse<bool>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // BATCH DELETE
    // ============================================================
    public async Task<ProcessResponse<int>> DeleteBatchAsync(BulkSelectionRequest request, CancellationToken ct = default)
    {
        using var content = ToJsonContent(request);
        using var response = await _httpClient.PostAsync("/api/anirwork/batch-delete", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<int>>(response, ct);
            return body ?? ProcessResponse<int>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<int>>(response, ct);
        return result ?? ProcessResponse<int>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // EXPORT PDF LIST
    // ============================================================
    public async Task<HttpResponseMessage> ExportPdfListAsync(
        BulkSelectionRequest request,
        CancellationToken ct = default)
    {
        using var content = ToJsonContent(request);
        return await _httpClient.PostAsync("/api/anirwork/export-pdf", content, ct);
    }

    // ============================================================
    // EXPORT PDF DETAIL
    // ============================================================
    public async Task<HttpResponseMessage> ExportPdfDetailAsync(
        int id,
        CancellationToken ct = default)
    {
        return await _httpClient.GetAsync($"/api/anirwork/export-pdf/{id}", ct);
    }
}
