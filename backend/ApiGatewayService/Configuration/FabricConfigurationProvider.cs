using System.Fabric;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ApiGatewayService.Configuration;

internal static class FabricConfigurationProvider
{
    public static ApiGatewaySettings Load(StatelessServiceContext context)
    {
        var package = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
        var settings = package.Settings;

        return new ApiGatewaySettings
        {
            JwtSecret = Read(settings, "Jwt", "Secret"),
            JwtIssuer = Read(settings, "Jwt", "Issuer"),
            JwtAudience = Read(settings, "Jwt", "Audience"),
            AllowDevUserHeaderFallback = ReadBool(settings, "Authentication", "AllowDevUserHeaderFallback")
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

    private static bool ReadBool(ConfigurationSettings settings, string sectionName, string parameterName)
    {
        return bool.TryParse(Read(settings, sectionName, parameterName), out var parsed) && parsed;
    }
}
