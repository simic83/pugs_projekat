namespace IdentityService.Security;

internal sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAtUtc);
