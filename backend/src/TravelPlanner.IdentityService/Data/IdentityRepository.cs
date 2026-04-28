using System.Data;
using Microsoft.Data.SqlClient;
using TravelPlanner.IdentityService.Auth;

namespace TravelPlanner.IdentityService.Data;

internal sealed class IdentityRepository
{
    private readonly string _connectionString;

    public IdentityRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<CreateUserResult> CreateUserAsync(
        Guid userId,
        string name,
        string email,
        string passwordHash,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        if (await EmailExistsAsync(connection, email, cancellationToken: cancellationToken))
        {
            return CreateUserResult.EmailAlreadyExists();
        }

        using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var roleId = await GetRoleIdAsync(
                connection,
                AuthRoleNames.User,
                transaction,
                cancellationToken);

            if (roleId is null)
            {
                throw new InvalidOperationException("Default User role is missing. Run the identity database migration first.");
            }

            await InsertUserAsync(
                connection,
                transaction,
                userId,
                name,
                email,
                passwordHash,
                cancellationToken);

            await InsertUserRoleAsync(
                connection,
                transaction,
                userId,
                roleId.Value,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return CreateUserResult.Success(new UserRecord
            {
                Id = userId,
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
                Role = AuthRoleNames.User,
            });
        }
        catch (SqlException ex) when (IsUniqueConstraintViolation(ex))
        {
            await transaction.RollbackAsync(cancellationToken);
            return CreateUserResult.EmailAlreadyExists();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<UserRecord?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT TOP (1)
                u.Id,
                u.Name,
                u.Email,
                u.PasswordHash,
                COALESCE(r.Name, @FallbackRole) AS RoleName
            FROM dbo.Users AS u
            LEFT JOIN dbo.UserRoles AS ur ON ur.UserId = u.Id
            LEFT JOIN dbo.Roles AS r ON r.Id = ur.RoleId
            WHERE u.Email = @Email
            ORDER BY CASE WHEN r.Name = @AdminRole THEN 0 ELSE 1 END;
            """;

        using var command = CreateCommand(connection, sql);
        AddStringParameter(command, "@Email", email, 256);
        AddStringParameter(command, "@FallbackRole", AuthRoleNames.User, 50);
        AddStringParameter(command, "@AdminRole", AuthRoleNames.Admin, 50);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await ReadUserOrNullAsync(reader, cancellationToken);
    }

    public async Task<UserRecord?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT TOP (1)
                u.Id,
                u.Name,
                u.Email,
                u.PasswordHash,
                COALESCE(r.Name, @FallbackRole) AS RoleName
            FROM dbo.Users AS u
            LEFT JOIN dbo.UserRoles AS ur ON ur.UserId = u.Id
            LEFT JOIN dbo.Roles AS r ON r.Id = ur.RoleId
            WHERE u.Id = @UserId
            ORDER BY CASE WHEN r.Name = @AdminRole THEN 0 ELSE 1 END;
            """;

        using var command = CreateCommand(connection, sql);
        command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
        AddStringParameter(command, "@FallbackRole", AuthRoleNames.User, 50);
        AddStringParameter(command, "@AdminRole", AuthRoleNames.Admin, 50);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await ReadUserOrNullAsync(reader, cancellationToken);
    }

    private static async Task<bool> EmailExistsAsync(
        SqlConnection connection,
        string email,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.Users WHERE Email = @Email;";
        using var command = CreateCommand(connection, sql, transaction);
        AddStringParameter(command, "@Email", email, 256);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) > 0;
    }

    private static async Task<Guid?> GetRoleIdAsync(
        SqlConnection connection,
        string roleName,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = "SELECT Id FROM dbo.Roles WHERE Name = @RoleName;";
        using var command = CreateCommand(connection, sql, transaction);
        AddStringParameter(command, "@RoleName", roleName, 50);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is Guid roleId ? roleId : null;
    }

    private static async Task InsertUserAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid userId,
        string name,
        string email,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Users (Id, Name, Email, PasswordHash)
            VALUES (@Id, @Name, @Email, @PasswordHash);
            """;

        using var command = CreateCommand(connection, sql, transaction);
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = userId;
        AddStringParameter(command, "@Name", name, 200);
        AddStringParameter(command, "@Email", email, 256);
        AddStringParameter(command, "@PasswordHash", passwordHash, 512);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertUserRoleAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.UserRoles (UserId, RoleId)
            VALUES (@UserId, @RoleId);
            """;

        using var command = CreateCommand(connection, sql, transaction);
        command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
        command.Parameters.Add("@RoleId", SqlDbType.UniqueIdentifier).Value = roleId;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<UserRecord?> ReadUserOrNullAsync(SqlDataReader reader, CancellationToken cancellationToken)
    {
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserRecord
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            Role = reader.GetString(4),
        };
    }

    private static SqlCommand CreateCommand(
        SqlConnection connection,
        string commandText,
        SqlTransaction? transaction = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.Transaction = transaction;
        return command;
    }

    private static void AddStringParameter(SqlCommand command, string name, string value, int size)
    {
        command.Parameters.Add(name, SqlDbType.NVarChar, size).Value = value;
    }

    private static bool IsUniqueConstraintViolation(SqlException exception)
    {
        return exception.Number is 2601 or 2627;
    }
}
