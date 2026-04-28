namespace TravelPlanner.Common.Security;

public sealed class JwtValidationResult
{
    public bool IsValid { get; init; }
    public bool IsExpired { get; init; }
    public Guid? UserId { get; init; }
    public string? Role { get; init; }
}

