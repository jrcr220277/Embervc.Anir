using Anir.Shared.Contracts.User;

namespace Anir.Shared.Contracts.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UserResponse User { get; set; } = new();
}
