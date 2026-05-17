using SharingService.Models;
using TravelPlanner.Contracts.Enums;

namespace SharingService;

internal static class ShareTokenAccessPolicy
{
    public static bool AllowsView(ShareTokenModel? shareToken, DateTime utcNow)
    {
        return shareToken is not null
            && !shareToken.IsRevoked
            && (!shareToken.ExpiresAt.HasValue || shareToken.ExpiresAt.Value > utcNow)
            && IsValidStoredAccessLevel(shareToken.AccessLevel);
    }

    public static bool AllowsEdit(ShareTokenModel? shareToken, DateTime utcNow)
    {
        return AllowsView(shareToken, utcNow)
            && ParseAccessLevel(shareToken!.AccessLevel) == ShareAccessLevel.Edit;
    }

    public static string ToStoredAccessLevel(ShareAccessLevel accessLevel)
    {
        return accessLevel == ShareAccessLevel.Edit ? "EDIT" : "VIEW";
    }

    public static ShareAccessLevel ParseAccessLevel(string accessLevel)
    {
        return string.Equals(accessLevel, "EDIT", StringComparison.OrdinalIgnoreCase)
            ? ShareAccessLevel.Edit
            : ShareAccessLevel.View;
    }

    private static bool IsValidStoredAccessLevel(string accessLevel)
    {
        return string.Equals(accessLevel, "VIEW", StringComparison.OrdinalIgnoreCase)
            || string.Equals(accessLevel, "EDIT", StringComparison.OrdinalIgnoreCase);
    }
}
