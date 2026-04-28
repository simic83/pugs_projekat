namespace TravelPlanner.Contracts.DTOs.Auth;

public sealed class TokenValidationResultDto
{
    public bool IsValid { get; set; }
    public bool IsExpired { get; set; }
    public bool IsAuthorized { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? UserId { get; set; }
    public string? Role { get; set; }
    public UserDto? User { get; set; }
}
