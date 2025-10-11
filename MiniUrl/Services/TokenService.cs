using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MiniUrl.Configs;
using MiniUrl.Entities;

namespace MiniUrl.Services;

public class TokenService : ITokenService
{
    private readonly JwtConfig _jwtConfig;

    public TokenService(JwtConfig jwtConfig)
    {
        _jwtConfig = jwtConfig;
    }

    public string CreateToken(User user)
    {
        // 1. Create Claims
        var claims = new List<Claim>()
        {
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new (ClaimTypes.Name, user.Username),
            new (ClaimTypes.Email, user.Email),
            new (ClaimTypes.Role, nameof(user.Role))
        };
        // 2. Generate Token
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiresInMinutes);
        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
