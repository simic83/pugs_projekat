namespace TravelPlanner.Contracts.DTOs.Auth;

public sealed class AuthResponseDto
{
    public bool Succeeded { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AccessToken { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public UserDto? User { get; set; }
}
