using Anir.Shared.Contracts.Common;

namespace Anir.Shared.Contracts.Organisms;

public class OrganismQueryDto : BaseQuery
{
    public string? Search { get; set; }
    public bool? ActiveFilter { get; set; }
}
