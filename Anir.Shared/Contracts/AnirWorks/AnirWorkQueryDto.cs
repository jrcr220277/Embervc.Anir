using Anir.Shared.Contracts.Common;

namespace Anir.Shared.Contracts.AnirWorks;

public class AnirWorkQueryDto : BaseQuery
{
    public string? Search { get; set; }

    public int? CompanyId { get; set; }

    public bool? IsPaid { get; set; }

    public bool? IsGeneralized { get; set; }

    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}
