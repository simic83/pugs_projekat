using IdentityService.Models;
using Microsoft.Data.SqlClient;

namespace IdentityService.Data;

internal sealed class SqlUserRepository : IUserRepository
{
    private readonly string connectionString;

    public SqlUserRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            "SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email) THEN 1 ELSE 0 END AS bit);",
            connection);
        command.Parameters.AddWithValue("@Email", email.Trim().ToLowerInvariant());

        var result = await command.ExecuteScalarAsync();
        return result is bool exists && exists;
    }

    public async Task<UserRecord?> FindByEmailAsync(string email)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(UserWithRolesSql + " WHERE u.Email = @Email ORDER BY r.RoleId;", connection);
        command.Parameters.AddWithValue("@Email", email.Trim().ToLowerInvariant());

        await using var reader = await command.ExecuteReaderAsync();
        return await ReadSingleUserAsync(reader);
    }

    public async Task<UserRecord?> GetByIdAsync(Guid userId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(UserWithRolesSql + " WHERE u.UserId = @UserId ORDER BY r.RoleId;", connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync();
        return await ReadSingleUserAsync(reader);
    }

    public async Task<List<UserRecord>> GetUsersAsync()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(UserWithRolesSql + " ORDER BY u.CreatedAtUtc DESC, r.RoleId;", connection);

        await using var reader = await command.ExecuteReaderAsync();
        var users = new Dictionary<Guid, UserRecord>();

        while (await reader.ReadAsync())
        {
            var userId = reader.GetGuid(0);
            if (!users.TryGetValue(userId, out var user))
            {
                user = CreateUserFromReader(reader);
                users.Add(userId, user);
            }

            AddRoleFromReader(reader, user);
        }

        return users.Values.ToList();
    }

    public async Task<UserRecord> CreateUserAsync(UserRecord user)
    {
        await using var connection = await CreateOpenConnectionAsync();
        using var transaction = connection.BeginTransaction();

        await using (var command = new SqlCommand(
            """
            INSERT INTO dbo.Users (UserId, Name, Email, PasswordHash, CreatedAtUtc)
            VALUES (@UserId, @Name, @Email, @PasswordHash, @CreatedAtUtc);

            INSERT INTO dbo.UserRoles (UserId, RoleId)
            SELECT @UserId, RoleId
            FROM dbo.Roles
            WHERE Name = @DefaultRoleName;
            """,
            connection,
            transaction))
        {
            command.Parameters.AddWithValue("@UserId", user.Id);
            command.Parameters.AddWithValue("@Name", user.Name);
            command.Parameters.AddWithValue("@Email", user.Email.Trim().ToLowerInvariant());
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@CreatedAtUtc", user.CreatedAtUtc);
            command.Parameters.AddWithValue("@DefaultRoleName", "User");

            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();

        var created = await GetByIdAsync(user.Id);
        return created ?? user;
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            "SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = @RoleName) THEN 1 ELSE 0 END AS bit);",
            connection);
        command.Parameters.AddWithValue("@RoleName", roleName);

        var result = await command.ExecuteScalarAsync();
        return result is bool exists && exists;
    }

    public async Task<int> CountUsersInRoleAsync(string roleName)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            SELECT COUNT(DISTINCT ur.UserId)
            FROM dbo.UserRoles ur
            INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
            WHERE r.Name = @RoleName;
            """,
            connection);
        command.Parameters.AddWithValue("@RoleName", roleName);

        var result = await command.ExecuteScalarAsync();
        return result is int count ? count : Convert.ToInt32(result);
    }

    public async Task<bool> SetUserRoleAsync(Guid userId, string roleName)
    {
        await using var connection = await CreateOpenConnectionAsync();
        using var transaction = connection.BeginTransaction();

        await using (var deleteCommand = new SqlCommand(
            "DELETE FROM dbo.UserRoles WHERE UserId = @UserId;",
            connection,
            transaction))
        {
            deleteCommand.Parameters.AddWithValue("@UserId", userId);
            await deleteCommand.ExecuteNonQueryAsync();
        }

        int inserted;
        await using (var insertCommand = new SqlCommand(
            """
            INSERT INTO dbo.UserRoles (UserId, RoleId)
            SELECT @UserId, RoleId
            FROM dbo.Roles
            WHERE Name = @RoleName;
            """,
            connection,
            transaction))
        {
            insertCommand.Parameters.AddWithValue("@UserId", userId);
            insertCommand.Parameters.AddWithValue("@RoleName", roleName);
            inserted = await insertCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        return inserted > 0;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand("DELETE FROM dbo.Users WHERE UserId = @UserId;", connection);
        command.Parameters.AddWithValue("@UserId", userId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    private async Task<SqlConnection> CreateOpenConnectionAsync()
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<UserRecord?> ReadSingleUserAsync(SqlDataReader reader)
    {
        UserRecord? user = null;

        while (await reader.ReadAsync())
        {
            user ??= CreateUserFromReader(reader);
            AddRoleFromReader(reader, user);
        }

        return user;
    }

    private static UserRecord CreateUserFromReader(SqlDataReader reader)
    {
        return new UserRecord
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            CreatedAtUtc = reader.GetDateTime(4)
        };
    }

    private static void AddRoleFromReader(SqlDataReader reader, UserRecord user)
    {
        if (reader.IsDBNull(5))
        {
            return;
        }

        user.Roles.Add(new RoleRecord
        {
            Id = reader.GetInt32(5),
            Name = reader.GetString(6)
        });
    }

    private const string UserWithRolesSql =
        """
        SELECT
            u.UserId,
            u.Name,
            u.Email,
            u.PasswordHash,
            u.CreatedAtUtc,
            r.RoleId,
            r.Name AS RoleName
        FROM dbo.Users u
        LEFT JOIN dbo.UserRoles ur ON ur.UserId = u.UserId
        LEFT JOIN dbo.Roles r ON r.RoleId = ur.RoleId
        """;
}
