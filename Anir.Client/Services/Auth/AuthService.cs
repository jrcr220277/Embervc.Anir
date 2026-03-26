using Anir.Shared.Contracts.Auth;
using Anir.Shared.Contracts.User;
using Anir.Shared.Enums;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace Anir.Client.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly AuthenticationProviderJWT _authProvider;
    private readonly ILocalStorageService _storage;
    private readonly UserState _userState;
    private readonly NavigationManager _nav;

    private const string TOKEN_KEY = "authToken";

    public AuthService(
        HttpClient http,
        AuthenticationProviderJWT authProvider,
        ILocalStorageService storage,
        UserState userState,
        NavigationManager nav)
    {
        _http = http;
        _authProvider = authProvider;
        _storage = storage;
        _userState = userState;
        _nav = nav;
    }

    // ============================================================
    // LOGIN
    // ============================================================
    public async Task<ProcessResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", request);

            var wrapper = await response.Content.ReadFromJsonAsync<ProcessResponse<LoginResponse>>();

            if (wrapper == null)
                return ProcessResponse<LoginResponse>.Fail("Respuesta inválida del servidor.");

            if (wrapper.Result == ResponseStatus.Failed)
                return wrapper;

            var data = wrapper.Value!;

            await _storage.SetItemAsync(TOKEN_KEY, data.Token);

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", data.Token);

            await _authProvider.LoginAsync(data.Token);

            if (data.User != null)
                _userState.Set(data.User);

            return wrapper;
        }
        catch
        {
            return ProcessResponse<LoginResponse>.Fail("Error de conexión con el servidor.");
        }
    }

    // ============================================================
    // REGISTER
    // ============================================================
    public async Task<ProcessResponse<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", request);

            var wrapper = await response.Content.ReadFromJsonAsync<ProcessResponse<RegisterResponse>>();

            return wrapper ?? ProcessResponse<RegisterResponse>.Fail("Respuesta inválida del servidor.");
        }
        catch
        {
            return ProcessResponse<RegisterResponse>.Fail("Error de conexión con el servidor.");
        }
    }

    // ============================================================
    // ME
    // ============================================================
    public async Task<ProcessResponse<UserResponse>> MeAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/auth/me");

            var wrapper = await response.Content.ReadFromJsonAsync<ProcessResponse<UserResponse>>();

            if (wrapper == null)
                return ProcessResponse<UserResponse>.Fail("Respuesta inválida del servidor.");

            if (wrapper.Result == ResponseStatus.Success && wrapper.Value != null)
                _userState.Set(wrapper.Value);

            return wrapper;
        }
        catch
        {
            return ProcessResponse<UserResponse>.Fail("Error de conexión con el servidor.");
        }
    }

    // ============================================================
    // FORGOT PASSWORD
    // ============================================================
    public async Task<ProcessResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/forgot-password", request);

            var wrapper = await response.Content.ReadFromJsonAsync<ProcessResponse<bool>>();

            return wrapper ?? ProcessResponse<bool>.Fail("Respuesta inválida del servidor.");
        }
        catch
        {
            return ProcessResponse<bool>.Fail("Error de conexión con el servidor.");
        }
    }

    // ============================================================
    // RESET PASSWORD
    // ============================================================
    public async Task<ProcessResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/reset-password", request);

            var wrapper = await response.Content.ReadFromJsonAsync<ProcessResponse<bool>>();

            return wrapper ?? ProcessResponse<bool>.Fail("Respuesta inválida del servidor.");
        }
        catch
        {
            return ProcessResponse<bool>.Fail("Error de conexión con el servidor.");
        }
    }

    // ============================================================
    // CHANGE PASSWORD
    // ============================================================
    public async Task<ProcessResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/change-password", request);

            var wrapper = await response.Content.ReadFromJsonAsync<ProcessResponse<bool>>();

            return wrapper ?? ProcessResponse<bool>.Fail("Respuesta inválida del servidor.");
        }
        catch
        {
            return ProcessResponse<bool>.Fail("Error de conexión con el servidor.");
        }
    }

    // ============================================================
    // UPDATE PROFILE
    // ============================================================
    public async Task<ProcessResponse<bool>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            var response = await _http.PutAsJsonAsync("api/auth/profile", request);

            var wrapper = await response.Content.ReadFromJsonAsync<ProcessResponse<bool>>();

            return wrapper ?? ProcessResponse<bool>.Fail("Respuesta inválida del servidor.");
        }
        catch
        {
            return ProcessResponse<bool>.Fail("Error de conexión con el servidor.");
        }
    }

    // ============================================================
    // LOGOUT
    // ============================================================
    public async Task LogoutAsync()
    {
        await _authProvider.LogoutAsync();
        await _storage.RemoveItemAsync(TOKEN_KEY);
        _userState.Clear();
        _nav.NavigateTo("/login", true);
    }
}
