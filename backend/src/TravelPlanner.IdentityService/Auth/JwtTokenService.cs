using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TravelPlanner.IdentityService.Data;

namespace TravelPlanner.IdentityService.Auth;

internal sealed class JwtTokenService
{
    private readonly AuthOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtTokenService(AuthOptions options)
    {
        _options = options;
    }

    public IssuedToken IssueToken(UserRecord user)
    {
        _options.EnsureUsableForJwt();

        var issuedAt = DateTimeOffset.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_options.JwtExpirationMinutes);
        var signingKey = CreateSigningKey();
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _options.JwtIssuer,
            audience: _options.JwtAudience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new IssuedToken(_tokenHandler.WriteToken(token), expiresAt);
    }

    public JwtTokenValidation ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return JwtTokenValidation.Fail(AuthErrorCodes.MissingToken, "Bearer token is required.");
        }

        _options.EnsureUsableForJwt();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = CreateSigningKey(),
            ValidateIssuer = true,
            ValidIssuer = _options.JwtIssuer,
            ValidateAudience = true,
            ValidAudience = _options.JwtAudience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtToken ||
                !string.Equals(jwtToken.Header.Alg, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
            {
                return JwtTokenValidation.Fail(AuthErrorCodes.InvalidToken, "JWT signing algorithm is invalid.");
            }

            var userIdValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;

            if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(role))
            {
                return JwtTokenValidation.Fail(AuthErrorCodes.InvalidToken, "JWT claims are invalid.");
            }

            return JwtTokenValidation.Success(userId, role);
        }
        catch (SecurityTokenExpiredException)
        {
            return JwtTokenValidation.Fail(AuthErrorCodes.ExpiredToken, "JWT token has expired.", isExpired: true);
        }
        catch (SecurityTokenException)
        {
            return JwtTokenValidation.Fail(AuthErrorCodes.InvalidToken, "JWT token is invalid.");
        }
        catch (ArgumentException)
        {
            return JwtTokenValidation.Fail(AuthErrorCodes.InvalidToken, "JWT token is invalid.");
        }
    }

    private SymmetricSecurityKey CreateSigningKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtSecret));
    }
}

internal sealed record IssuedToken(string AccessToken, DateTimeOffset ExpiresAt);

internal sealed class JwtTokenValidation
{
    public bool IsValid { get; private init; }
    public bool IsExpired { get; private init; }
    public string? ErrorCode { get; private init; }
    public string? ErrorMessage { get; private init; }
    public Guid? UserId { get; private init; }
    public string? Role { get; private init; }

    public static JwtTokenValidation Success(Guid userId, string role)
    {
        return new JwtTokenValidation
        {
            IsValid = true,
            UserId = userId,
            Role = role,
        };
    }

    public static JwtTokenValidation Fail(string errorCode, string errorMessage, bool isExpired = false)
    {
        return new JwtTokenValidation
        {
            IsValid = false,
            IsExpired = isExpired,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };
    }
}
