using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.SystemSettings;

namespace Anir.Client.Services.SystemSettings;

public class SystemSettingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly JsonSerializerOptions _jsonOptions;

    private SystemSettingDto? _current;
    public SystemSettingDto? Settings => _current;
    public event Action? OnChange;
    private void Notify() => OnChange?.Invoke();

    // Inyectamos el proveedor de servicios
    public SystemSettingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

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
        try { return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, ct); }
        catch { var raw = await response.Content.ReadAsStringAsync(ct); return JsonSerializer.Deserialize<T>(raw, _jsonOptions); }
    }

    public async Task<SystemSettingDto?> GetAsync(CancellationToken ct = default)
    {
        if (_current is not null) return _current;

        // Creamos un scope para obtener el HttpClient con la BaseAddress del backend correcta
        using var scope = _serviceProvider.CreateScope();
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();

        using var response = await http.GetAsync("/api/systemsetting", ct);

        if (!response.IsSuccessStatusCode) return null;

        var result = await ReadJsonAsync<ProcessResponse<SystemSettingDto>>(response, ct);

        if (result?.Value is not null)
            _current = result.Value;

        return _current;
    }

    public async Task<ProcessResponse<SystemSettingDto>?> UpdateAsync(SystemSettingDto dto, CancellationToken ct = default)
    {
        // Creamos un scope para obtener el HttpClient con la BaseAddress del backend correcta
        using var scope = _serviceProvider.CreateScope();
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();

        using var content = ToJsonContent(dto);
        using var response = await http.PutAsync("/api/systemsetting", content, ct);

        var result = await ReadJsonAsync<ProcessResponse<SystemSettingDto>>(response, ct);

        if (result?.Value is not null)
        {
            _current = result.Value;
            Notify();
        }

        return result;
    }

    public void Clear()
    {
        _current = null;
        Notify();
    }
}