using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiGatewayService.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace ApiGatewayService.Infrastructure;

internal static class ControllerUserContext
{
    private const string DevUserHeaderName = "X-User-Id";

    public static Guid GetUserId(ControllerBase controller)
    {
        var claimValue = controller.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? controller.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? controller.User.FindFirstValue("nameid");

        if (Guid.TryParse(claimValue, out var authenticatedUserId))
        {
            return authenticatedUserId;
        }

        // Dev-only compatibility path for old local calls; it is disabled by default.
        return IsDevUserHeaderFallbackEnabled(controller)
            && controller.Request.Headers.TryGetValue(DevUserHeaderName, out StringValues value)
            && Guid.TryParse(value.FirstOrDefault(), out var userId)
                ? userId
                : Guid.Empty;
    }

    public static IReadOnlyList<string> GetRoles(ControllerBase controller)
    {
        return controller.User.Claims
            .Where(claim => claim.Type is ClaimTypes.Role or "role")
            .Select(claim => claim.Value)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsDevUserHeaderFallbackEnabled(ControllerBase controller)
    {
        var settings = controller.HttpContext.RequestServices.GetService<ApiGatewaySettings>();
        return settings?.AllowDevUserHeaderFallback == true;
    }
}
