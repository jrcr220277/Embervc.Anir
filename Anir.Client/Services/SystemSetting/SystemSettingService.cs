// Anir.Client\Services\SystemSettings\SystemSettingService.cs
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.SystemSettings;

namespace Anir.Client.Services.SystemSettings;

public class SystemSettingService : ISystemSettingService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public SystemSettingService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null
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
    // GET
    // ============================================================
    public async Task<ProcessResponse<SystemSettingDto>> GetAsync(CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync("/api/systemsetting", ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<SystemSettingDto>>(response, ct);
            return body ?? ProcessResponse<SystemSettingDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<SystemSettingDto>>(response, ct);
        return result ?? ProcessResponse<SystemSettingDto>.Fail("Respuesta inválida del servidor.");
    }

    // ============================================================
    // UPDATE
    // ============================================================
    public async Task<ProcessResponse<SystemSettingDto>> UpdateAsync(SystemSettingDto dto, CancellationToken ct = default)
    {
        using var content = ToJsonContent(dto);
        using var response = await _httpClient.PutAsync("/api/systemsetting", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadJsonAsync<ProcessResponse<SystemSettingDto>>(response, ct);
            return body ?? ProcessResponse<SystemSettingDto>.Fail($"Error HTTP {(int)response.StatusCode}");
        }

        var result = await ReadJsonAsync<ProcessResponse<SystemSettingDto>>(response, ct);
        return result ?? ProcessResponse<SystemSettingDto>.Fail("Respuesta inválida del servidor.");
    }
}