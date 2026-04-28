using TravelPlanner.Contracts.DTOs.Auth;

namespace TravelPlanner.IdentityService.Data;

internal sealed class UserRecord
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public required string Role { get; init; }

    public UserDto ToDto()
    {
        return new UserDto
        {
            Id = Id,
            Name = Name,
            Email = Email,
            Role = Role,
        };
    }
}
