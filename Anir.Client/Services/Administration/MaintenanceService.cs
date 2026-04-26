// Anir.Client\Services\SystemSettings\MaintenanceService.cs
using System.Net.Http.Json;
using System.Text.Json;
using Anir.Shared.Contracts.Common;

namespace Anir.Client.Services.Administration;


public class MaintenanceService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly JsonSerializerOptions _jsonOptions;

    public MaintenanceService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null // PascalCase
        };
    }

    public async Task<ProcessResponse<CleanupResult>?> CleanOrphansAsync(CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();

        using var response = await http.PostAsync("/api/maintenance/clean-orphans", null, ct);

        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<ProcessResponse<CleanupResult>>(_jsonOptions, ct);
    }
}