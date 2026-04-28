namespace TravelPlanner.Contracts.DTOs.Auth;

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}
