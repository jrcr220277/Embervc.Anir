using Anir.Shared.Contracts.Common;
using Anir.Shared.Enums;

namespace Anir.Shared.Contracts.AnirWorks;

public class AnirWorkQueryDto : BaseQuery
{
    public string? Search { get; set; }

    public int? CompanyId { get; set; }
    public int? UebId { get; set; }
    public bool? HasSocialEffect { get; set; }
    public bool? HasEconomicEffect { get; set; }

    public GeneralizationStatus? Generalization { get; set; }

    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}
