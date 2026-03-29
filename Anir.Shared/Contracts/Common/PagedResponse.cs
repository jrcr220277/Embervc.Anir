namespace Anir.Shared.Contracts.Common;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }

    public int TotalPages =>
        Size == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / Size);

    public bool HasMore =>
        (Page * Size) < TotalCount;
}
