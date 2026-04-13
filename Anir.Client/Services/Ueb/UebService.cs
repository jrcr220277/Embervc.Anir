using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;
using Anir.Shared.Contracts.Organisms;
using Anir.Shared.Contracts.Uebs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Anir.Client.Services.Ueb;

public class UebService : IUebService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public UebService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // ⭐ CAMBIO IMPORTANTE:
        // El backend usa PascalCase → NO usar camelCase aquí.
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null // ← PascalCase
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
    // GET PAGED (POST, profesional, limpio)
    // ============================================================
    public async Task<List<UebDto>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<List<UebDto>>("/api/ueb/all", ct);
        return response ?? new();
    }

    // ============================================================
    // GET PAGED (POST, profesional, limpio)
    // ============================================================
    public async Task<ProcessResponse<PagedResponse<UebDto>>> GetPagedAsync(
        UebQueryDto query,
        CancellationToken ct = default)
    {
        using var content = ToJsonContent(query);
        using var response = await _httpClient.PostAsync("/api/ueb/getpaged", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<PagedResponse<UebDto>>>(response, ct);
            return body ?? ProcessResponse<PagedResponse<UebDto>>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<PagedResponse<UebDto>>>(response, ct);
        return result ?? ProcessResponse<PagedResponse<UebDto>>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    public async Task<ProcessResponse<UebDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync($"/api/ueb/{id}", ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<UebDto>>(response, ct);
            return body ?? ProcessResponse<UebDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<UebDto>>(response, ct);
        return result ?? ProcessResponse<UebDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // CREATE
    // ============================================================
    public async Task<ProcessResponse<UebDto>> CreateAsync(UebDto dto, CancellationToken ct = default)
    {
        using var content = ToJsonContent(dto);
        using var response = await _httpClient.PostAsync("/api/ueb", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<UebDto>>(response, ct);
            return body ?? ProcessResponse<UebDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<UebDto>>(response, ct);
        return result ?? ProcessResponse<UebDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // UPDATE
    // ============================================================
    public async Task<ProcessResponse<UebDto>> UpdateAsync(int id, UebDto dto, CancellationToken ct = default)
    {
        using var content = ToJsonContent(dto);
        using var response = await _httpClient.PutAsync($"/api/ueb/{id}", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<UebDto>>(response, ct);
            return body ?? ProcessResponse<UebDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<UebDto>>(response, ct);
        return result ?? ProcessResponse<UebDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // DELETE
    // ============================================================
    public async Task<ProcessResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        using var response = await _httpClient.DeleteAsync($"/api/ueb/{id}", ct);

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
        using var response = await _httpClient.PostAsync("/api/ueb/batch-delete", content, ct);

        var raw = await response.Content.ReadAsStringAsync(ct);

        if (response.IsSuccessStatusCode)
        {
            // Éxito → value es int
            var ok = JsonSerializer.Deserialize<ProcessResponse<int>>(raw, _jsonOptions);
            return ok ?? ProcessResponse<int>.Fail("Respuesta inválida del servidor.");
        }
        else
        {
            // Error → value es string o null
            var error = JsonSerializer.Deserialize<ProcessResponse<string>>(raw, _jsonOptions);

            return ProcessResponse<int>.Fail(error?.ErrorMessage ?? $"Error HTTP {(int)response.StatusCode}");
        }
    }

    public async Task<HttpResponseMessage> ExportPdfAsync(BulkSelectionRequest request, CancellationToken ct = default)
    {
        using var content = ToJsonContent(request);
        return await _httpClient.PostAsync("/api/ueb/export-pdf", content, ct);
    }


    public async Task<HttpResponseMessage> ExportExcelAsync(BulkSelectionRequest request, CancellationToken ct = default)
    {
        using var content = ToJsonContent(request);
        return await _httpClient.PostAsync("/api/ueb/export-excel", content, ct);
    }


}
