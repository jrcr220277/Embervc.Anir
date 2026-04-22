using Anir.Data.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Anir.Infrastructure.Jwt;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var jwt = _config.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim("fullName", user.FullName ?? ""),
            new Claim("active", user.Active ? "1" : "0"),
            new Claim("themeMode", user.ThemeMode.ToString())
            
            // ⚠️ ELIMINADO: new Claim("imagenId", user.ImagenId ?? "")
            // Razón: El JWT vive 60 minutos. Si el usuario cambia su foto de perfil, 
            // el token quedaría con el ID viejo hasta que haga logout.
            // La imagen se obtiene dinámicamente desde el endpoint /api/auth/me (UserResponse.ImageFile).
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"]!)),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}