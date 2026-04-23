// Anir.Client\Services\SystemSettings\ISystemSettingService.cs
using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.SystemSettings;

namespace Anir.Client.Services.SystemSettings;

public interface ISystemSettingService
{
    Task<ProcessResponse<SystemSettingDto>> GetAsync(CancellationToken ct = default);
    Task<ProcessResponse<SystemSettingDto>> UpdateAsync(SystemSettingDto dto, CancellationToken ct = default);
}