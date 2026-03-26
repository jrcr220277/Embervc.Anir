// Anir.Shared.Contracts.Companies/CompanyFilterDto.cs
using Anir.Shared.Contracts.Common;

namespace Anir.Shared.Contracts.Companies;

public class CompanyQueryDto : QueryParams
{
    public string? Search { get; set; }
    public bool? ActiveFilter { get; set; }
    public int? MunicipalityId { get; set; }
}
