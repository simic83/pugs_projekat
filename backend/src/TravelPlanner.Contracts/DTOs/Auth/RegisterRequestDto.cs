namespace TravelPlanner.Contracts.DTOs.Auth;

public sealed class RegisterRequestDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}
