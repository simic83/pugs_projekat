using System.Text;

namespace ApiGatewayService.Configuration;

internal sealed class ApiGatewaySettings
{
    public string JwtSecret { get; init; } = string.Empty;

    public string JwtIssuer { get; init; } = string.Empty;

    public string JwtAudience { get; init; } = string.Empty;

    public bool AllowDevUserHeaderFallback { get; init; }

    public void EnsureJwtConfigured()
    {
        if (string.IsNullOrWhiteSpace(JwtSecret))
        {
            throw new InvalidOperationException("Jwt:Secret is not configured.");
        }

        if (Encoding.UTF8.GetByteCount(JwtSecret) < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be at least 32 bytes.");
        }

        if (string.IsNullOrWhiteSpace(JwtIssuer))
        {
            throw new InvalidOperationException("Jwt:Issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(JwtAudience))
        {
            throw new InvalidOperationException("Jwt:Audience is not configured.");
        }
    }
}
