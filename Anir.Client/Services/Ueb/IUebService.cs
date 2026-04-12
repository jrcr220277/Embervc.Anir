using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Companies;
using Anir.Shared.Contracts.Uebs;

namespace Anir.Client.Services.Ueb
{
    public interface IUebService
    {
        Task<List<UebDto>> GetAllAsync(CancellationToken ct = default);
        Task<ProcessResponse<PagedResponse<UebDto>>> GetPagedAsync(UebQueryDto query, CancellationToken ct = default);
        Task<ProcessResponse<UebDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ProcessResponse<UebDto>> CreateAsync(UebDto dto, CancellationToken ct = default);
        Task<ProcessResponse<UebDto>> UpdateAsync(int id, UebDto dto, CancellationToken ct = default);
        Task<ProcessResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
        Task<ProcessResponse<int>> DeleteBatchAsync(BulkSelectionRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ExportPdfAsync(BulkSelectionRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ExportExcelAsync(BulkSelectionRequest request, CancellationToken ct = default);
    }
}
