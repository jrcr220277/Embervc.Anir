using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Anir.Client.Services.Auth;

public class AuthenticationProviderJWT : AuthenticationStateProvider
{
    private readonly ILocalStorageService _storage;
    private readonly HttpClient _http;
    private const string TOKEN_KEY = "authToken";

    private readonly AuthenticationState _anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public AuthenticationProviderJWT(ILocalStorageService storage, HttpClient http)
    {
        _storage = storage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _storage.GetItemAsync<string>(TOKEN_KEY);
        if (string.IsNullOrWhiteSpace(token))
            return _anonymous;

        JwtSecurityToken jwt;
        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
        catch
        {
            await LogoutAsync();
            return _anonymous;
        }

        if (jwt.ValidTo <= DateTime.UtcNow)
        {
            await LogoutAsync();
            return _anonymous;
        }

        return BuildAuthenticationState(token);
    }

    public async Task LoginAsync(string token)
    {
        await _storage.SetItemAsync(TOKEN_KEY, token);
        var authState = BuildAuthenticationState(token);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public async Task LogoutAsync()
    {
        await _storage.RemoveItemAsync(TOKEN_KEY);
        _http.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    private AuthenticationState BuildAuthenticationState(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}
