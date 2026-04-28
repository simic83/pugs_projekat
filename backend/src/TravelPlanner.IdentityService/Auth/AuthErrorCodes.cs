namespace TravelPlanner.IdentityService.Auth;

internal static class AuthErrorCodes
{
    public const string ValidationError = "ValidationError";
    public const string DuplicateEmail = "DuplicateEmail";
    public const string InvalidCredentials = "InvalidCredentials";
    public const string MissingToken = "MissingToken";
    public const string InvalidToken = "InvalidToken";
    public const string ExpiredToken = "ExpiredToken";
    public const string Forbidden = "Forbidden";
    public const string UserNotFound = "UserNotFound";
}
