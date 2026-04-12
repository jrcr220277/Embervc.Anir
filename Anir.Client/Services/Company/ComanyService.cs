using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;
using Anir.Shared.Contracts.Organisms;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Anir.Client.Services.Company;

public class CompanyService : ICompanyService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public CompanyService(HttpClient httpClient)
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
    public async Task<List<CompanyDto>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<List<CompanyDto>>("/api/company/all", ct);
        return response ?? new();
    }

    // ============================================================
    // GET PAGED (POST, profesional, limpio)
    // ============================================================
    public async Task<ProcessResponse<PagedResponse<CompanyDto>>> GetPagedAsync(
        CompanyQueryDto query,
        CancellationToken ct = default)
    {
        using var content = ToJsonContent(query);
        using var response = await _httpClient.PostAsync("/api/company/getpaged", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<PagedResponse<CompanyDto>>>(response, ct);
            return body ?? ProcessResponse<PagedResponse<CompanyDto>>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<PagedResponse<CompanyDto>>>(response, ct);
        return result ?? ProcessResponse<PagedResponse<CompanyDto>>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    public async Task<ProcessResponse<CompanyDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync($"/api/company/{id}", ct);

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
        using var response = await _httpClient.PostAsync("/api/company", content, ct);

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
        using var response = await _httpClient.PutAsync($"/api/company/{id}", content, ct);

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
        using var response = await _httpClient.DeleteAsync($"/api/company/{id}", ct);

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
        using var response = await _httpClient.PostAsync("/api/company/batch-delete", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<int>>(response, ct);
            return body ?? ProcessResponse<int>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<int>>(response, ct);
        return result ?? ProcessResponse<int>.Fail("Respuesta inválida del servidor.");
    }

    public async Task<HttpResponseMessage> ExportPdfAsync(BulkSelectionRequest request, CancellationToken ct = default)
    {
        using var content = ToJsonContent(request);
        return await _httpClient.PostAsync("/api/company/export-pdf", content, ct);
    }


    public async Task<HttpResponseMessage> ExportExcelAsync(BulkSelectionRequest request, CancellationToken ct = default)
    {
        using var content = ToJsonContent(request);
        return await _httpClient.PostAsync("/api/company/export-excel", content, ct);
    }


}
