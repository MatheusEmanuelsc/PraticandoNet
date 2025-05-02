using System.Security.Claims;

namespace Artigos.Entidades;

public interface ITokenService
{
    Task<string> GenerateAccessToken(ApplicationUser user);
    Task<string> GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}