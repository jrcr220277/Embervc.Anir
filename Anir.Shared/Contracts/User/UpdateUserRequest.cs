using Anir.Shared.Contracts.Common;


namespace Anir.Shared.Contracts.User;

public class UpdateUserRequest
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool Active { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? NewPassword { get; set; }
    public string? ImagenId { get; set; }   // ⭐ SOLO ESTO
}

