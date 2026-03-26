using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Anir.Client.Services.Company;

public class CompanyService : ICompanyService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public CompanyService(HttpClient http)
    {
        _http = http;

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
    public async Task<ProcessResponse<PagedResult<CompanyDto>>> GetPagedAsync(
        CompanyQueryDto query,
        CancellationToken ct = default)
    {
        using var content = ToJsonContent(query);
        using var response = await _http.PostAsync("/api/company/getpaged", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<PagedResult<CompanyDto>>>(response, ct);
            return body ?? ProcessResponse<PagedResult<CompanyDto>>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<PagedResult<CompanyDto>>>(response, ct);
        return result ?? ProcessResponse<PagedResult<CompanyDto>>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    public async Task<ProcessResponse<CompanyDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"/api/company/{id}", ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<CompanyDto>>(response, ct);
            return body ?? ProcessResponse<CompanyDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<CompanyDto>>(response, ct);
        return result ?? ProcessResponse<CompanyDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // CREATE
    // ============================================================
    public async Task<ProcessResponse<CompanyDto>> CreateAsync(CompanyDto dto, CancellationToken ct = default)
    {
        using var content = ToJsonContent(dto);
        using var response = await _http.PostAsync("/api/company", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<CompanyDto>>(response, ct);
            return body ?? ProcessResponse<CompanyDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<CompanyDto>>(response, ct);
        return result ?? ProcessResponse<CompanyDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // UPDATE
    // ============================================================
    public async Task<ProcessResponse<CompanyDto>> UpdateAsync(int id, CompanyDto dto, CancellationToken ct = default)
    {
        using var content = ToJsonContent(dto);
        using var response = await _http.PutAsync($"/api/company/{id}", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<CompanyDto>>(response, ct);
            return body ?? ProcessResponse<CompanyDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<CompanyDto>>(response, ct);
        return result ?? ProcessResponse<CompanyDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // DELETE
    // ============================================================
    public async Task<ProcessResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"/api/company/{id}", ct);

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
        using var response = await _http.PostAsync("/api/company/batch-delete", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<int>>(response, ct);
            return body ?? ProcessResponse<int>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<int>>(response, ct);
        return result ?? ProcessResponse<int>.Fail("Respuesta inválida del servidor.");
    }
}
