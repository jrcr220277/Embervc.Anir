using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Persons;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Anir.Client.Services.Person;

public class PersonService : IPersonService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public PersonService(HttpClient httpClient)
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
    public async Task<ProcessResponse<PagedResponse<PersonDto>>> GetPagedAsync(PersonQueryDto query, CancellationToken ct = default)
    {
        using var content = ToJsonContent(query);
        using var response = await _httpClient.PostAsync("/api/person/getpaged", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<PagedResponse<PersonDto>>>(response, ct);
            return body ?? ProcessResponse<PagedResponse<PersonDto>>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<PagedResponse<PersonDto>>>(response, ct);
        return result ?? ProcessResponse<PagedResponse<PersonDto>>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    public async Task<ProcessResponse<PersonDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync($"/api/person/{id}", ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<PersonDto>>(response, ct);
            return body ?? ProcessResponse<PersonDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<PersonDto>>(response, ct);
        return result ?? ProcessResponse<PersonDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // CREATE
    // ============================================================
    public async Task<ProcessResponse<PersonDto>> CreateAsync(PersonDto dto, CancellationToken ct = default)
    {
        using var content = ToJsonContent(dto);
        using var response = await _httpClient.PostAsync("/api/person", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<PersonDto>>(response, ct);
            return body ?? ProcessResponse<PersonDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<PersonDto>>(response, ct);
        return result ?? ProcessResponse<PersonDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // UPDATE
    // ============================================================
    public async Task<ProcessResponse<PersonDto>> UpdateAsync(int id, PersonDto dto, CancellationToken ct = default)
    {
        using var content = ToJsonContent(dto);
        using var response = await _httpClient.PutAsync($"/api/person/{id}", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<PersonDto>>(response, ct);
            return body ?? ProcessResponse<PersonDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<PersonDto>>(response, ct);
        return result ?? ProcessResponse<PersonDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // DELETE
    // ============================================================
    public async Task<ProcessResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        using var response = await _httpClient.DeleteAsync($"/api/person/{id}", ct);

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
        using var response = await _httpClient.PostAsync("/api/person/batch-delete", content, ct);

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
        return await _httpClient.PostAsync("/api/person/export-pdf", content, ct);
    }


    public async Task<HttpResponseMessage> ExportExcelAsync(BulkSelectionRequest request, CancellationToken ct = default)
    {
        using var content = ToJsonContent(request);
        return await _httpClient.PostAsync("/api/person/export-excel", content, ct);
    }


}
