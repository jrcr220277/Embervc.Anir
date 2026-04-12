using Anir.Shared.Contracts.Common;

namespace Anir.Shared.Contracts.Uebs;

public class UebQueryDto : BaseQuery
{
    public string? Search { get; set; }
    public bool? ActiveFilter { get; set; }
   
}

