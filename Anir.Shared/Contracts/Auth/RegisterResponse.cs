namespace Anir.Shared.Contracts.Auth;

public class RegisterResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool Active { get; set; }
}

