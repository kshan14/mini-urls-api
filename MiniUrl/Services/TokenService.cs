using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
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
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
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

    public User GetUserFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.Key));
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtConfig.Issuer,
            ValidAudience = _jwtConfig.Audience,
            IssuerSigningKey = key,
        }, out _);

        return new User
        {
            Id = Guid.Parse(principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value),
            Username = principal.Claims.First(c => c.Type == ClaimTypes.Name).Value,
            Email = principal.Claims.First(c => c.Type == ClaimTypes.Email).Value,
            Role = Enum.Parse<Role>(principal.Claims.First(c => c.Type == ClaimTypes.Role).Value),
        };
    }
}
