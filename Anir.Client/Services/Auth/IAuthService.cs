using Anir.Shared.Contracts.Auth;
using Anir.Shared.Contracts.User;

namespace Anir.Client.Services.Auth;

public interface IAuthService
{
    Task<ProcessResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ProcessResponse<RegisterResponse>> RegisterAsync(RegisterRequest request);
    Task<ProcessResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ProcessResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ProcessResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request);
    Task<ProcessResponse<UserResponse>> MeAsync();
    Task<ProcessResponse<bool>> UpdateProfileAsync(UpdateProfileRequest request);
    Task LogoutAsync();
}
