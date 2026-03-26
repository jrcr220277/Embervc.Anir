using Anir.Shared.Enums;

namespace Anir.Shared.Contracts.Auth;

public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool Active { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? ImagenId { get; set; }
    public string? ImagenUrl { get; set; }
    public ThemeMode ThemeMode { get; set; }
}
