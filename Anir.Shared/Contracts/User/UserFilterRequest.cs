namespace Anir.Shared.Contracts.User;

public class UserFilterRequest
{
    public string? SearchTerm { get; set; }
    public bool? Active { get; set; }
    public string? SortField { get; set; }
    public bool SortDescending { get; set; }
    public int PageSize { get; set; } = 10;
    public int CurrentPage { get; set; } = 1;
}
