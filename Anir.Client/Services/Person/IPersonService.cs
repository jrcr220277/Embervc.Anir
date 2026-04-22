using Anir.Shared.Contracts.Common;
using Anir.Shared.Contracts.Persons;

namespace Anir.Client.Services.Person
{
    public interface IPersonService
    {
        Task<List<PersonDto>> SearchAsync(string query, CancellationToken ct = default);
        Task<ProcessResponse<PagedResponse<PersonDto>>> GetPagedAsync(PersonQueryDto query, CancellationToken ct = default);
        Task<ProcessResponse<PersonDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ProcessResponse<PersonDto>> CreateAsync(PersonDto dto, CancellationToken ct = default);
        Task<ProcessResponse<PersonDto>> UpdateAsync(int id, PersonDto dto, CancellationToken ct = default);
        Task<ProcessResponse<bool>> DeleteAsync(int id, CancellationToken ct = default);
        Task<ProcessResponse<int>> DeleteBatchAsync(BulkSelectionRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ExportPdfAsync(BulkSelectionRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ExportExcelAsync(BulkSelectionRequest request, CancellationToken ct = default);
    }
}
