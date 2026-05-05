using IdentityService.Models;

namespace IdentityService.Data;

internal interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);

    Task<UserRecord?> FindByEmailAsync(string email);

    Task<UserRecord?> GetByIdAsync(Guid userId);

    Task<List<UserRecord>> GetUsersAsync();

    Task<UserRecord> CreateUserAsync(UserRecord user);
}
