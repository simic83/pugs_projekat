using IdentityService.Models;
using Microsoft.EntityFrameworkCore;
using TravelPlanner.Persistence;
using TravelPlanner.Persistence.Entities;

namespace IdentityService.Data;

internal sealed class EfUserRepository : IUserRepository
{
    private const string DefaultRoleName = "User";
    private readonly IDbContextFactory<TravelPlannerDbContext> dbContextFactory;

    public EfUserRepository(IDbContextFactory<TravelPlannerDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var normalizedEmail = NormalizeEmail(email);

        return await context.Users
            .AsNoTracking()
            .AnyAsync(user => user.Email == normalizedEmail);
    }

    public async Task<UserRecord?> FindByEmailAsync(string email)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var normalizedEmail = NormalizeEmail(email);

        var user = await UsersWithRoles(context)
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail);

        return user is null ? null : ToRecord(user);
    }

    public async Task<UserRecord?> GetByIdAsync(Guid userId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var user = await UsersWithRoles(context)
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId);

        return user is null ? null : ToRecord(user);
    }

    public async Task<List<UserRecord>> GetUsersAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var users = await UsersWithRoles(context)
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAtUtc)
            .ToListAsync();

        return users.Select(ToRecord).ToList();
    }

    public async Task<UserRecord> CreateUserAsync(UserRecord user)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var normalizedEmail = NormalizeEmail(user.Email);
        var defaultRoleId = await context.Roles
            .Where(role => role.Name == DefaultRoleName)
            .Select(role => (int?)role.Id)
            .SingleOrDefaultAsync();

        var entity = new UserEntity
        {
            Id = user.Id,
            Name = user.Name,
            Email = normalizedEmail,
            PasswordHash = user.PasswordHash,
            CreatedAtUtc = user.CreatedAtUtc
        };

        if (defaultRoleId is null)
        {
            throw new InvalidOperationException("Default user role is not configured.");
        }

        entity.UserRoles.Add(new UserRoleEntity
        {
            UserId = entity.Id,
            RoleId = defaultRoleId.Value
        });

        context.Users.Add(entity);
        await context.SaveChangesAsync();

        return await GetByIdAsync(entity.Id) ?? ToRecord(entity);
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        return await context.Roles
            .AsNoTracking()
            .AnyAsync(role => role.Name == roleName);
    }

    public async Task<int> CountUsersInRoleAsync(string roleName)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        return await context.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.Role != null && userRole.Role.Name == roleName)
            .Select(userRole => userRole.UserId)
            .Distinct()
            .CountAsync();
    }

    public async Task<bool> SetUserRoleAsync(Guid userId, string roleName)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var roleId = await context.Roles
            .Where(role => role.Name == roleName)
            .Select(role => (int?)role.Id)
            .SingleOrDefaultAsync();

        if (roleId is null)
        {
            return false;
        }

        var existingRoles = await context.UserRoles
            .Where(userRole => userRole.UserId == userId)
            .ToListAsync();

        context.UserRoles.RemoveRange(existingRoles);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRoleEntity
        {
            UserId = userId,
            RoleId = roleId.Value
        });

        var inserted = await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return inserted > 0;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var user = await context.Users
            .Include(user => user.UserRoles)
            .SingleOrDefaultAsync(user => user.Id == userId);
        if (user is null)
        {
            return false;
        }

        context.Users.Remove(user);
        return await context.SaveChangesAsync() > 0;
    }

    private static IQueryable<UserEntity> UsersWithRoles(TravelPlannerDbContext context)
    {
        return context.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role);
    }

    private static UserRecord ToRecord(UserEntity user)
    {
        return new UserRecord
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            CreatedAtUtc = user.CreatedAtUtc,
            Roles = user.UserRoles
                .Where(userRole => userRole.Role is not null)
                .OrderBy(userRole => userRole.RoleId)
                .Select(userRole => new RoleRecord
                {
                    Id = userRole.Role!.Id,
                    Name = userRole.Role.Name
                })
                .ToList()
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
