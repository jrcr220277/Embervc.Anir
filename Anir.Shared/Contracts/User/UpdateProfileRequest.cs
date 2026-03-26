using Anir.Shared.Contracts.Common;
using Anir.Shared.Enums;


namespace Anir.Shared.Contracts.User;

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? ImagenId { get; set; }
    public string? ImagenUrl { get; set; }
    public ThemeMode ThemeMode { get; set; }

}
