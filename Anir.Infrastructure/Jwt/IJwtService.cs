using Anir.Data.Identity;

namespace Anir.Infrastructure.Jwt;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
