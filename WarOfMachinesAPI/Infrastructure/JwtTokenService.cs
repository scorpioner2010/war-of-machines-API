using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WarOfMachines.Models;

namespace WarOfMachines.Infrastructure;

public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly byte[] _signingKey;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        string key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured.");
        if (key.Length < 32)
        {
            throw new InvalidOperationException("JWT signing key is too short.");
        }

        _signingKey = Encoding.UTF8.GetBytes(key);
    }

    public AccessTokenResult CreateAccessToken(Player user, Guid sessionId, int? lifetimeMinutes = null)
    {
        string issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");
        string audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");
        int configuredLifetimeMinutes = lifetimeMinutes
            ?? _configuration.GetValue("Jwt:AccessTokenMinutes", 30);
        DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(Math.Clamp(configuredLifetimeMinutes, 5, 120));

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "admin" : "user"),
            new Claim("sid", sessionId.ToString("D")),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        });

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(_signingKey),
            SecurityAlgorithms.HmacSha256);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(
            subject: identity,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            issuer: issuer,
            audience: audience,
            signingCredentials: credentials);

        return new AccessTokenResult(handler.WriteToken(token), expiresAtUtc);
    }
}

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
