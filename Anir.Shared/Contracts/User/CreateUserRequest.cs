using Anir.Shared.Contracts.Common;


namespace Anir.Shared.Contracts.User;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool Active { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? Password { get; set; }
    public string? ImagenId { get; set; }   // ⭐ SOLO ESTO
}

