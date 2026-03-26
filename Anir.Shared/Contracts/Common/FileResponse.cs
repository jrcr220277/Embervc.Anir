namespace Anir.Shared.Contracts.Common;

public class FileResponse
{
    public string Id { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string Name { get; set; } = default!;
    public long Size { get; set; }
    public string Type { get; set; } = default!;
}
