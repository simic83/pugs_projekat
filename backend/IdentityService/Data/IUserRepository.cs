using IdentityService.Models;

namespace IdentityService.Data;

internal interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);

    Task<UserRecord?> FindByEmailAsync(string email);

    Task<UserRecord?> GetByIdAsync(Guid userId);

    Task<List<UserRecord>> GetUsersAsync();

    Task<UserRecord> CreateUserAsync(UserRecord user);

    Task<bool> RoleExistsAsync(string roleName);

    Task<int> CountUsersInRoleAsync(string roleName);

    Task<bool> SetUserRoleAsync(Guid userId, string roleName);

    Task<bool> DeleteUserAsync(Guid userId);
}
