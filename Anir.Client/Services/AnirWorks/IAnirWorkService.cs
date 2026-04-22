using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.AnirWorks;

namespace Anir.Client.Services.AnirWorks;

public interface IAnirWorkService
{
    // CRUD Básico
    Task<ProcessResponse<PagedResponse<AnirWorkDto>>> GetPagedAsync(AnirWorkQueryDto query, CancellationToken ct = default);
    Task<ProcessResponse<AnirWorkDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProcessResponse<AnirWorkDto>> CreateAsync(AnirWorkDto dto, CancellationToken ct = default);
    Task<ProcessResponse<AnirWorkDto>> UpdateAsync(int id, AnirWorkDto dto, CancellationToken ct = default);
    Task<ProcessResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
    Task<ProcessResponse<int>> DeleteBatchAsync(BulkSelectionRequest request, CancellationToken ct = default);
    Task<byte[]> ExportPdfListAsync(BulkSelectionRequest request, CancellationToken ct = default);
    Task<byte[]> ExportExcelListAsync(BulkSelectionRequest request, CancellationToken ct = default);
}