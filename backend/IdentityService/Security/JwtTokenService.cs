using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Configuration;
using IdentityService.Models;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Security;

internal sealed class JwtTokenService
{
    private readonly IdentityServiceSettings settings;

    public JwtTokenService(IdentityServiceSettings settings)
    {
        this.settings = settings;
    }

    public void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(settings.JwtSecret))
        {
            throw new InvalidOperationException("Jwt:Secret is not configured.");
        }

        if (Encoding.UTF8.GetByteCount(settings.JwtSecret) < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be at least 32 bytes.");
        }

        if (string.IsNullOrWhiteSpace(settings.JwtIssuer))
        {
            throw new InvalidOperationException("Jwt:Issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.JwtAudience))
        {
            throw new InvalidOperationException("Jwt:Audience is not configured.");
        }
    }

    public JwtTokenResult CreateToken(UserRecord user)
    {
        EnsureConfigured();

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(settings.JwtExpirationMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Name)));

        var token = new JwtSecurityToken(
            issuer: settings.JwtIssuer,
            audience: settings.JwtAudience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
