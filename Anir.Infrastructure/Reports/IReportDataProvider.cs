using Anir.Shared.Contracts.Common;

namespace Anir.Infrastructure.Reports;

public interface IReportDataProvider
{
    Task<ReportConfigDto> GetConfigAsync(CancellationToken ct = default);
}