namespace IdentityService.Configuration;

internal sealed class IdentityServiceSettings
{
    public string DefaultConnection { get; init; } = string.Empty;

    public string JwtSecret { get; init; } = string.Empty;

    public string JwtIssuer { get; init; } = string.Empty;

    public string JwtAudience { get; init; } = string.Empty;

    public int JwtExpirationMinutes { get; init; } = 60;
}
