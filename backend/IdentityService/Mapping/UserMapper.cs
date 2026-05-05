using IdentityService.Models;
using TravelPlanner.Contracts.Enums;
using TravelPlanner.Contracts.Users;

namespace IdentityService.Mapping;

internal static class UserMapper
{
    public static UserDto ToDto(UserRecord user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAtUtc = user.CreatedAtUtc,
            Roles = user.Roles.Select(ToDto).ToList()
        };
    }

    private static RoleDto ToDto(RoleRecord role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = Enum.TryParse<UserRole>(role.Name, ignoreCase: true, out var parsedRole)
                ? parsedRole
                : UserRole.User
        };
    }
}
