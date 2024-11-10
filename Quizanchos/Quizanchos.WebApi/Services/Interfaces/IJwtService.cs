using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IJwtService
{
    JwtSecurityToken CreateToken(List<Claim> authClaims);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);

    int GetAccessTokenValidityInMinutes();

    Task<string> GenerateAcessTokenAsync(ApplicationUser user);
}