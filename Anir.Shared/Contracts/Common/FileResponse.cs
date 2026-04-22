namespace Anir.Shared.Contracts.Common;

public class FileResponse
{
    public int Id { get; set; }
    public string Url { get; set; } = default!;
    public string Name { get; set; } = default!;
    public long Size { get; set; }
    public string Type { get; set; } = default!;
}