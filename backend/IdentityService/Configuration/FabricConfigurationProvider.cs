using System.Fabric;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Services.Runtime;

namespace IdentityService.Configuration;

internal static class FabricConfigurationProvider
{
    public static IdentityServiceSettings Load(StatelessServiceContext context)
    {
        var package = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
        var settings = package.Settings;

        return new IdentityServiceSettings
        {
            DefaultConnection = Read(settings, "ConnectionStrings", "DefaultConnection"),
            JwtSecret = Read(settings, "Jwt", "Secret"),
            JwtIssuer = Read(settings, "Jwt", "Issuer"),
            JwtAudience = Read(settings, "Jwt", "Audience"),
            JwtExpirationMinutes = ReadInt(settings, "Jwt", "ExpirationMinutes", 60)
        };
    }

    private static string Read(ConfigurationSettings settings, string sectionName, string parameterName)
    {
        if (!settings.Sections.Contains(sectionName))
        {
            return string.Empty;
        }

        var section = settings.Sections[sectionName];
        return section.Parameters.Contains(parameterName)
            ? section.Parameters[parameterName].Value
            : string.Empty;
    }

    private static int ReadInt(ConfigurationSettings settings, string sectionName, string parameterName, int fallback)
    {
        var value = Read(settings, sectionName, parameterName);
        return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
    }
}
