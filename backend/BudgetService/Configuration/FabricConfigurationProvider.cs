using System.Fabric;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Services.Runtime;

namespace BudgetService.Configuration;

internal static class FabricConfigurationProvider
{
    public static BudgetServiceSettings Load(StatefulServiceContext context)
    {
        var package = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
        var settings = package.Settings;

        return new BudgetServiceSettings
        {
            DefaultConnection = Read(settings, "ConnectionStrings", "DefaultConnection")
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
}
