using System.Fabric;

namespace TravelPlanner.IdentityService.Auth;

internal sealed class AuthOptions
{
    private const int DefaultExpirationMinutes = 60;

    public required string ConnectionString { get; init; }
    public required string JwtSecret { get; init; }
    public required string JwtIssuer { get; init; }
    public required string JwtAudience { get; init; }
    public required int JwtExpirationMinutes { get; init; }

    public static AuthOptions FromServiceContext(StatelessServiceContext context)
    {
        var settings = context.CodePackageActivationContext
            .GetConfigurationPackageObject("Config")
            .Settings;

        return new AuthOptions
        {
            ConnectionString = GetSetting(settings, "ConnectionStrings", "DefaultConnection"),
            JwtSecret = GetSetting(settings, "Jwt", "Secret"),
            JwtIssuer = GetSetting(settings, "Jwt", "Issuer", "TravelPlanner"),
            JwtAudience = GetSetting(settings, "Jwt", "Audience", "TravelPlanner.Client"),
            JwtExpirationMinutes = GetIntSetting(settings, "Jwt", "ExpirationMinutes", DefaultExpirationMinutes),
        };
    }

    public void EnsureUsableForDatabase()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException("Identity database connection string is not configured.");
        }
    }

    public void EnsureUsableForJwt()
    {
        if (string.IsNullOrWhiteSpace(JwtSecret))
        {
            throw new InvalidOperationException("JWT secret is not configured.");
        }

        if (JwtSecret.Length < 32)
        {
            throw new InvalidOperationException("JWT secret must be at least 32 characters long.");
        }

        if (string.IsNullOrWhiteSpace(JwtIssuer))
        {
            throw new InvalidOperationException("JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(JwtAudience))
        {
            throw new InvalidOperationException("JWT audience is not configured.");
        }
    }

    private static string GetSetting(
        System.Fabric.Description.ConfigurationSettings settings,
        string sectionName,
        string parameterName,
        string defaultValue = "")
    {
        if (!settings.Sections.Contains(sectionName))
        {
            return defaultValue;
        }

        var section = settings.Sections[sectionName];
        return section.Parameters.Contains(parameterName)
            ? section.Parameters[parameterName].Value
            : defaultValue;
    }

    private static int GetIntSetting(
        System.Fabric.Description.ConfigurationSettings settings,
        string sectionName,
        string parameterName,
        int defaultValue)
    {
        var rawValue = GetSetting(settings, sectionName, parameterName);
        return int.TryParse(rawValue, out var parsedValue) && parsedValue > 0
            ? parsedValue
            : defaultValue;
    }
}
