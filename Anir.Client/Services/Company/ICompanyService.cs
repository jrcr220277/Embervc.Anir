using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;

namespace Anir.Client.Services.Company
{
    public interface ICompanyService
    {
        Task<ProcessResponse<PagedResult<CompanyDto>>> GetPagedAsync(CompanyQueryDto query, CancellationToken ct = default);
        Task<ProcessResponse<CompanyDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ProcessResponse<CompanyDto>> CreateAsync(CompanyDto dto, CancellationToken ct = default);
        Task<ProcessResponse<CompanyDto>> UpdateAsync(int id, CompanyDto dto, CancellationToken ct = default);
        Task<ProcessResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
        Task<ProcessResponse<int>> DeleteBatchAsync(BulkSelectionRequest request, CancellationToken ct = default);
    }
}
