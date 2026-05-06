using Microsoft.Data.SqlClient;
using SharingService.Models;

namespace SharingService.Data;

internal sealed class SharingRepository : ISharingRepository
{
    private readonly string connectionString;

    public SharingRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<ShareTokenModel> CreateShareTokenAsync(ShareTokenModel shareToken)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            INSERT INTO dbo.ShareTokens
                (Id, TripPlanId, Token, AccessLevel, CreatedByUserId, CreatedAt, ExpiresAt, IsRevoked)
            VALUES
                (@Id, @TripPlanId, @Token, @AccessLevel, @CreatedByUserId, @CreatedAt, @ExpiresAt, @IsRevoked);
            """,
            connection);

        AddShareTokenParameters(command, shareToken);
        await command.ExecuteNonQueryAsync();
        return shareToken;
    }

    public async Task<ShareTokenModel?> GetShareTokenByTokenAsync(string token)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(ShareTokenSelectSql + " WHERE Token = @Token;", connection);
        command.Parameters.AddWithValue("@Token", token);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadShareToken(reader) : null;
    }

    public async Task<List<ShareTokenModel>> GetShareTokensByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            ShareTokenSelectSql + " WHERE TripPlanId = @TripPlanId ORDER BY CreatedAt DESC;",
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);

        await using var reader = await command.ExecuteReaderAsync();
        var shareTokens = new List<ShareTokenModel>();

        while (await reader.ReadAsync())
        {
            shareTokens.Add(ReadShareToken(reader));
        }

        return shareTokens;
    }

    public async Task<bool> RevokeShareTokenAsync(Guid tripPlanId, Guid shareId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            UPDATE dbo.ShareTokens
            SET IsRevoked = 1
            WHERE Id = @Id AND TripPlanId = @TripPlanId;
            """,
            connection);
        command.Parameters.AddWithValue("@Id", shareId);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> UserOwnsTripPlanAsync(Guid tripPlanId, Guid userId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            SELECT COUNT(1)
            FROM dbo.TripPlans
            WHERE Id = @TripPlanId AND OwnerUserId = @OwnerUserId;
            """,
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);
        command.Parameters.AddWithValue("@OwnerUserId", userId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<TripPlanModel?> GetTripPlanByIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(TripPlanSelectSql + " WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", tripPlanId);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadTripPlan(reader) : null;
    }

    public async Task<bool> UpdateTripPlanAsync(TripPlanModel tripPlan)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            UPDATE dbo.TripPlans
            SET Title = @Title,
                Description = @Description,
                StartDate = @StartDate,
                EndDate = @EndDate,
                PlannedBudget = @PlannedBudget,
                Notes = @Notes,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """,
            connection);

        AddTripPlanParameters(command, tripPlan);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<DestinationModel>> GetDestinationsByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            DestinationSelectSql + " WHERE TripPlanId = @TripPlanId ORDER BY ArrivalDate, Name;",
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);

        await using var reader = await command.ExecuteReaderAsync();
        var destinations = new List<DestinationModel>();

        while (await reader.ReadAsync())
        {
            destinations.Add(ReadDestination(reader));
        }

        return destinations;
    }

    public async Task<List<ActivityModel>> GetActivitiesByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            ActivitySelectSql + " WHERE TripPlanId = @TripPlanId ORDER BY ActivityDate, ActivityTime, Title;",
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);

        await using var reader = await command.ExecuteReaderAsync();
        var activities = new List<ActivityModel>();

        while (await reader.ReadAsync())
        {
            activities.Add(ReadActivity(reader));
        }

        return activities;
    }

    public async Task<List<ExpenseModel>> GetExpensesByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            ExpenseSelectSql + " WHERE TripPlanId = @TripPlanId ORDER BY ExpenseDate DESC, CreatedAt DESC;",
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);

        await using var reader = await command.ExecuteReaderAsync();
        var expenses = new List<ExpenseModel>();

        while (await reader.ReadAsync())
        {
            expenses.Add(ReadExpense(reader));
        }

        return expenses;
    }

    public async Task<decimal> GetTotalExpensesByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            "SELECT COALESCE(SUM(Amount), 0) FROM dbo.Expenses WHERE TripPlanId = @TripPlanId;",
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);

        var result = await command.ExecuteScalarAsync();
        return result is decimal total ? total : 0m;
    }

    public async Task<List<ChecklistItemModel>> GetChecklistItemsByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            ChecklistItemSelectSql + " WHERE TripPlanId = @TripPlanId ORDER BY IsCompleted, CreatedAt, Title;",
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);

        await using var reader = await command.ExecuteReaderAsync();
        var checklistItems = new List<ChecklistItemModel>();

        while (await reader.ReadAsync())
        {
            checklistItems.Add(ReadChecklistItem(reader));
        }

        return checklistItems;
    }

    public async Task<List<NoteModel>> GetNotesByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            NoteSelectSql + " WHERE TripPlanId = @TripPlanId ORDER BY CreatedAt DESC, Title;",
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);

        await using var reader = await command.ExecuteReaderAsync();
        var notes = new List<NoteModel>();

        while (await reader.ReadAsync())
        {
            notes.Add(ReadNote(reader));
        }

        return notes;
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

    private static void AddShareTokenParameters(SqlCommand command, ShareTokenModel shareToken)
    {
        command.Parameters.AddWithValue("@Id", shareToken.Id);
        command.Parameters.AddWithValue("@TripPlanId", shareToken.TripPlanId);
        command.Parameters.AddWithValue("@Token", shareToken.Token);
        command.Parameters.AddWithValue("@AccessLevel", shareToken.AccessLevel);
        command.Parameters.AddWithValue("@CreatedByUserId", shareToken.CreatedByUserId);
        command.Parameters.AddWithValue("@CreatedAt", shareToken.CreatedAt);
        command.Parameters.AddWithValue("@ExpiresAt", ToDbValue(shareToken.ExpiresAt));
        command.Parameters.AddWithValue("@IsRevoked", shareToken.IsRevoked);
    }

    private static void AddTripPlanParameters(SqlCommand command, TripPlanModel tripPlan)
    {
        command.Parameters.AddWithValue("@Id", tripPlan.Id);
        command.Parameters.AddWithValue("@OwnerUserId", tripPlan.OwnerUserId);
        command.Parameters.AddWithValue("@Title", tripPlan.Title);
        command.Parameters.AddWithValue("@Description", ToDbValue(tripPlan.Description));
        command.Parameters.AddWithValue("@StartDate", tripPlan.StartDate.Date);
        command.Parameters.AddWithValue("@EndDate", tripPlan.EndDate.Date);
        command.Parameters.AddWithValue("@PlannedBudget", tripPlan.PlannedBudget);
        command.Parameters.AddWithValue("@Notes", ToDbValue(tripPlan.Notes));
        command.Parameters.AddWithValue("@CreatedAt", tripPlan.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", ToDbValue(tripPlan.UpdatedAt));
    }

    private static ShareTokenModel ReadShareToken(SqlDataReader reader)
    {
        return new ShareTokenModel
        {
            Id = reader.GetGuid(0),
            TripPlanId = reader.GetGuid(1),
            Token = reader.GetString(2),
            AccessLevel = reader.GetString(3),
            CreatedByUserId = reader.GetGuid(4),
            CreatedAt = reader.GetDateTime(5),
            ExpiresAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            IsRevoked = reader.GetBoolean(7)
        };
    }

    private static TripPlanModel ReadTripPlan(SqlDataReader reader)
    {
        return new TripPlanModel
        {
            Id = reader.GetGuid(0),
            OwnerUserId = reader.GetGuid(1),
            Title = reader.GetString(2),
            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
            StartDate = reader.GetDateTime(4),
            EndDate = reader.GetDateTime(5),
            PlannedBudget = reader.GetDecimal(6),
            Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
            CreatedAt = reader.GetDateTime(8),
            UpdatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
        };
    }

    private static DestinationModel ReadDestination(SqlDataReader reader)
    {
        return new DestinationModel
        {
            Id = reader.GetGuid(0),
            TripPlanId = reader.GetGuid(1),
            Name = reader.GetString(2),
            Location = reader.IsDBNull(3) ? null : reader.GetString(3),
            ArrivalDate = reader.GetDateTime(4),
            DepartureDate = reader.GetDateTime(5),
            Description = reader.IsDBNull(6) ? null : reader.GetString(6),
            CreatedAt = reader.GetDateTime(7),
            UpdatedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
        };
    }

    private static ActivityModel ReadActivity(SqlDataReader reader)
    {
        return new ActivityModel
        {
            Id = reader.GetGuid(0),
            TripPlanId = reader.GetGuid(1),
            Title = reader.GetString(2),
            ActivityDate = reader.GetDateTime(3),
            ActivityTime = reader.IsDBNull(4) ? null : reader.GetTimeSpan(4),
            Location = reader.IsDBNull(5) ? null : reader.GetString(5),
            Description = reader.IsDBNull(6) ? null : reader.GetString(6),
            EstimatedCost = reader.GetDecimal(7),
            Status = reader.GetString(8),
            CreatedAt = reader.GetDateTime(9),
            UpdatedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10)
        };
    }

    private static ExpenseModel ReadExpense(SqlDataReader reader)
    {
        return new ExpenseModel
        {
            Id = reader.GetGuid(0),
            TripPlanId = reader.GetGuid(1),
            Title = reader.GetString(2),
            Category = reader.GetString(3),
            Amount = reader.GetDecimal(4),
            ExpenseDate = reader.GetDateTime(5),
            Description = reader.IsDBNull(6) ? null : reader.GetString(6),
            CreatedAt = reader.GetDateTime(7),
            UpdatedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
        };
    }

    private static ChecklistItemModel ReadChecklistItem(SqlDataReader reader)
    {
        return new ChecklistItemModel
        {
            Id = reader.GetGuid(0),
            TripPlanId = reader.GetGuid(1),
            Title = reader.GetString(2),
            IsCompleted = reader.GetBoolean(3),
            CreatedAt = reader.GetDateTime(4),
            UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
        };
    }

    private static NoteModel ReadNote(SqlDataReader reader)
    {
        return new NoteModel
        {
            Id = reader.GetGuid(0),
            TripPlanId = reader.GetGuid(1),
            Title = reader.GetString(2),
            Content = reader.IsDBNull(3) ? null : reader.GetString(3),
            CreatedAt = reader.GetDateTime(4),
            UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
        };
    }

    private static object ToDbValue(object? value)
    {
        return value ?? DBNull.Value;
    }

    private const string ShareTokenSelectSql =
        """
        SELECT Id, TripPlanId, Token, AccessLevel, CreatedByUserId, CreatedAt, ExpiresAt, IsRevoked
        FROM dbo.ShareTokens
        """;

    private const string TripPlanSelectSql =
        """
        SELECT Id, OwnerUserId, Title, Description, StartDate, EndDate, PlannedBudget, Notes, CreatedAt, UpdatedAt
        FROM dbo.TripPlans
        """;

    private const string DestinationSelectSql =
        """
        SELECT Id, TripPlanId, Name, Location, ArrivalDate, DepartureDate, Description, CreatedAt, UpdatedAt
        FROM dbo.Destinations
        """;

    private const string ActivitySelectSql =
        """
        SELECT Id, TripPlanId, Title, ActivityDate, ActivityTime, Location, Description, EstimatedCost, Status, CreatedAt, UpdatedAt
        FROM dbo.Activities
        """;

    private const string ExpenseSelectSql =
        """
        SELECT Id, TripPlanId, Title, Category, Amount, ExpenseDate, Description, CreatedAt, UpdatedAt
        FROM dbo.Expenses
        """;

    private const string ChecklistItemSelectSql =
        """
        SELECT Id, TripPlanId, Title, IsCompleted, CreatedAt, UpdatedAt
        FROM dbo.ChecklistItems
        """;

    private const string NoteSelectSql =
        """
        SELECT Id, TripPlanId, Title, Content, CreatedAt, UpdatedAt
        FROM dbo.Notes
        """;
}
