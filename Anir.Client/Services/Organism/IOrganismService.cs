using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Organisms;

namespace Anir.Client.Services.Organism
{
    public interface IOrganismService
    {
        Task<ProcessResponse<PagedResponse<OrganismDto>>> GetPagedAsync(OrganismQueryDto query, CancellationToken ct = default);
        Task<ProcessResponse<OrganismDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ProcessResponse<OrganismDto>> CreateAsync(OrganismDto dto, CancellationToken ct = default);
        Task<ProcessResponse<OrganismDto>> UpdateAsync(int id, OrganismDto dto, CancellationToken ct = default);
        Task<ProcessResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
        Task<ProcessResponse<int>> DeleteBatchAsync(BulkSelectionRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ExportPdfAsync(BulkSelectionRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ExportExcelAsync(BulkSelectionRequest request, CancellationToken ct = default);
    }
}
