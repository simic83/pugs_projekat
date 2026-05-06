using Microsoft.Data.SqlClient;
using TripPlanningService.Models;

namespace TripPlanningService.Data;

internal sealed class TripPlanningRepository : ITripPlanningRepository
{
    private readonly string connectionString;

    public TripPlanningRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<TripPlanModel> CreateTripPlanAsync(TripPlanModel tripPlan)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            INSERT INTO dbo.TripPlans
                (Id, OwnerUserId, Title, Description, StartDate, EndDate, PlannedBudget, Notes, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @OwnerUserId, @Title, @Description, @StartDate, @EndDate, @PlannedBudget, @Notes, @CreatedAt, @UpdatedAt);
            """,
            connection);

        AddTripPlanParameters(command, tripPlan);
        await command.ExecuteNonQueryAsync();
        return tripPlan;
    }

    public async Task<TripPlanModel?> GetTripPlanByIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(TripPlanSelectSql + " WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", tripPlanId);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadTripPlan(reader) : null;
    }

    public async Task<List<TripPlanModel>> GetTripPlansByOwnerAsync(Guid ownerUserId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            TripPlanSelectSql + " WHERE OwnerUserId = @OwnerUserId ORDER BY StartDate, CreatedAt;",
            connection);
        command.Parameters.AddWithValue("@OwnerUserId", ownerUserId);

        await using var reader = await command.ExecuteReaderAsync();
        var tripPlans = new List<TripPlanModel>();

        while (await reader.ReadAsync())
        {
            tripPlans.Add(ReadTripPlan(reader));
        }

        return tripPlans;
    }

    public async Task<List<TripPlanModel>> GetAllTripPlansAsync()
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            TripPlanSelectSql + " ORDER BY CreatedAt DESC, StartDate;",
            connection);

        await using var reader = await command.ExecuteReaderAsync();
        var tripPlans = new List<TripPlanModel>();

        while (await reader.ReadAsync())
        {
            tripPlans.Add(ReadTripPlan(reader));
        }

        return tripPlans;
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

    public async Task<bool> DeleteTripPlanAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand("DELETE FROM dbo.TripPlans WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", tripPlanId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<DestinationModel> CreateDestinationAsync(DestinationModel destination)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            INSERT INTO dbo.Destinations
                (Id, TripPlanId, Name, Location, ArrivalDate, DepartureDate, Description, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @TripPlanId, @Name, @Location, @ArrivalDate, @DepartureDate, @Description, @CreatedAt, @UpdatedAt);
            """,
            connection);

        AddDestinationParameters(command, destination);
        await command.ExecuteNonQueryAsync();
        return destination;
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

    public async Task<DestinationModel?> GetDestinationByIdAsync(Guid destinationId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(DestinationSelectSql + " WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", destinationId);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadDestination(reader) : null;
    }

    public async Task<bool> UpdateDestinationAsync(DestinationModel destination)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            UPDATE dbo.Destinations
            SET Name = @Name,
                Location = @Location,
                ArrivalDate = @ArrivalDate,
                DepartureDate = @DepartureDate,
                Description = @Description,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """,
            connection);

        AddDestinationParameters(command, destination);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteDestinationAsync(Guid destinationId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand("DELETE FROM dbo.Destinations WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", destinationId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<ActivityModel> CreateActivityAsync(ActivityModel activity)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            INSERT INTO dbo.Activities
                (Id, TripPlanId, Title, ActivityDate, ActivityTime, Location, Description, EstimatedCost, Status, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @TripPlanId, @Title, @ActivityDate, @ActivityTime, @Location, @Description, @EstimatedCost, @Status, @CreatedAt, @UpdatedAt);
            """,
            connection);

        AddActivityParameters(command, activity);
        await command.ExecuteNonQueryAsync();
        return activity;
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

    public async Task<ActivityModel?> GetActivityByIdAsync(Guid activityId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(ActivitySelectSql + " WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", activityId);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadActivity(reader) : null;
    }

    public async Task<bool> UpdateActivityAsync(ActivityModel activity)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            UPDATE dbo.Activities
            SET Title = @Title,
                ActivityDate = @ActivityDate,
                ActivityTime = @ActivityTime,
                Location = @Location,
                Description = @Description,
                EstimatedCost = @EstimatedCost,
                Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """,
            connection);

        AddActivityParameters(command, activity);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteActivityAsync(Guid activityId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand("DELETE FROM dbo.Activities WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", activityId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<ChecklistItemModel> CreateChecklistItemAsync(ChecklistItemModel checklistItem)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            INSERT INTO dbo.ChecklistItems
                (Id, TripPlanId, Title, IsCompleted, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @TripPlanId, @Title, @IsCompleted, @CreatedAt, @UpdatedAt);
            """,
            connection);

        AddChecklistItemParameters(command, checklistItem);
        await command.ExecuteNonQueryAsync();
        return checklistItem;
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

    public async Task<ChecklistItemModel?> GetChecklistItemByIdAsync(Guid checklistItemId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(ChecklistItemSelectSql + " WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", checklistItemId);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadChecklistItem(reader) : null;
    }

    public async Task<bool> UpdateChecklistItemAsync(ChecklistItemModel checklistItem)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            UPDATE dbo.ChecklistItems
            SET Title = @Title,
                IsCompleted = @IsCompleted,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """,
            connection);

        AddChecklistItemParameters(command, checklistItem);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteChecklistItemAsync(Guid checklistItemId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand("DELETE FROM dbo.ChecklistItems WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", checklistItemId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<NoteModel> CreateNoteAsync(NoteModel note)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            INSERT INTO dbo.Notes
                (Id, TripPlanId, Title, Content, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @TripPlanId, @Title, @Content, @CreatedAt, @UpdatedAt);
            """,
            connection);

        AddNoteParameters(command, note);
        await command.ExecuteNonQueryAsync();
        return note;
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

    public async Task<NoteModel?> GetNoteByIdAsync(Guid noteId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(NoteSelectSql + " WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", noteId);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadNote(reader) : null;
    }

    public async Task<bool> UpdateNoteAsync(NoteModel note)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            UPDATE dbo.Notes
            SET Title = @Title,
                Content = @Content,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """,
            connection);

        AddNoteParameters(command, note);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteNoteAsync(Guid noteId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand("DELETE FROM dbo.Notes WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", noteId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<ReminderModel> CreateReminderAsync(ReminderModel reminder)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            INSERT INTO dbo.Reminders
                (Id, TripPlanId, Title, Description, ReminderAt, IsCompleted, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @TripPlanId, @Title, @Description, @ReminderAt, @IsCompleted, @CreatedAt, @UpdatedAt);
            """,
            connection);

        AddReminderParameters(command, reminder);
        await command.ExecuteNonQueryAsync();
        return reminder;
    }

    public async Task<List<ReminderModel>> GetRemindersByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            ReminderSelectSql + " WHERE TripPlanId = @TripPlanId ORDER BY IsCompleted, ReminderAt, CreatedAt;",
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);

        await using var reader = await command.ExecuteReaderAsync();
        var reminders = new List<ReminderModel>();

        while (await reader.ReadAsync())
        {
            reminders.Add(ReadReminder(reader));
        }

        return reminders;
    }

    public async Task<ReminderModel?> GetReminderByIdAsync(Guid reminderId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(ReminderSelectSql + " WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", reminderId);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadReminder(reader) : null;
    }

    public async Task<bool> UpdateReminderAsync(ReminderModel reminder)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            UPDATE dbo.Reminders
            SET Title = @Title,
                Description = @Description,
                ReminderAt = @ReminderAt,
                IsCompleted = @IsCompleted,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """,
            connection);

        AddReminderParameters(command, reminder);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteReminderAsync(Guid reminderId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand("DELETE FROM dbo.Reminders WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", reminderId);

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

    private static void AddDestinationParameters(SqlCommand command, DestinationModel destination)
    {
        command.Parameters.AddWithValue("@Id", destination.Id);
        command.Parameters.AddWithValue("@TripPlanId", destination.TripPlanId);
        command.Parameters.AddWithValue("@Name", destination.Name);
        command.Parameters.AddWithValue("@Location", ToDbValue(destination.Location));
        command.Parameters.AddWithValue("@ArrivalDate", destination.ArrivalDate.Date);
        command.Parameters.AddWithValue("@DepartureDate", destination.DepartureDate.Date);
        command.Parameters.AddWithValue("@Description", ToDbValue(destination.Description));
        command.Parameters.AddWithValue("@CreatedAt", destination.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", ToDbValue(destination.UpdatedAt));
    }

    private static void AddActivityParameters(SqlCommand command, ActivityModel activity)
    {
        command.Parameters.AddWithValue("@Id", activity.Id);
        command.Parameters.AddWithValue("@TripPlanId", activity.TripPlanId);
        command.Parameters.AddWithValue("@Title", activity.Title);
        command.Parameters.AddWithValue("@ActivityDate", activity.ActivityDate.Date);
        command.Parameters.AddWithValue("@ActivityTime", ToDbValue(activity.ActivityTime));
        command.Parameters.AddWithValue("@Location", ToDbValue(activity.Location));
        command.Parameters.AddWithValue("@Description", ToDbValue(activity.Description));
        command.Parameters.AddWithValue("@EstimatedCost", activity.EstimatedCost);
        command.Parameters.AddWithValue("@Status", activity.Status);
        command.Parameters.AddWithValue("@CreatedAt", activity.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", ToDbValue(activity.UpdatedAt));
    }

    private static void AddChecklistItemParameters(SqlCommand command, ChecklistItemModel checklistItem)
    {
        command.Parameters.AddWithValue("@Id", checklistItem.Id);
        command.Parameters.AddWithValue("@TripPlanId", checklistItem.TripPlanId);
        command.Parameters.AddWithValue("@Title", checklistItem.Title);
        command.Parameters.AddWithValue("@IsCompleted", checklistItem.IsCompleted);
        command.Parameters.AddWithValue("@CreatedAt", checklistItem.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", ToDbValue(checklistItem.UpdatedAt));
    }

    private static void AddNoteParameters(SqlCommand command, NoteModel note)
    {
        command.Parameters.AddWithValue("@Id", note.Id);
        command.Parameters.AddWithValue("@TripPlanId", note.TripPlanId);
        command.Parameters.AddWithValue("@Title", note.Title);
        command.Parameters.AddWithValue("@Content", ToDbValue(note.Content));
        command.Parameters.AddWithValue("@CreatedAt", note.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", ToDbValue(note.UpdatedAt));
    }

    private static void AddReminderParameters(SqlCommand command, ReminderModel reminder)
    {
        command.Parameters.AddWithValue("@Id", reminder.Id);
        command.Parameters.AddWithValue("@TripPlanId", reminder.TripPlanId);
        command.Parameters.AddWithValue("@Title", reminder.Title);
        command.Parameters.AddWithValue("@Description", ToDbValue(reminder.Description));
        command.Parameters.AddWithValue("@ReminderAt", reminder.ReminderAt);
        command.Parameters.AddWithValue("@IsCompleted", reminder.IsCompleted);
        command.Parameters.AddWithValue("@CreatedAt", reminder.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", ToDbValue(reminder.UpdatedAt));
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

    private static ReminderModel ReadReminder(SqlDataReader reader)
    {
        return new ReminderModel
        {
            Id = reader.GetGuid(0),
            TripPlanId = reader.GetGuid(1),
            Title = reader.GetString(2),
            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
            ReminderAt = reader.GetDateTime(4),
            IsCompleted = reader.GetBoolean(5),
            CreatedAt = reader.GetDateTime(6),
            UpdatedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
        };
    }

    private static object ToDbValue(object? value)
    {
        return value ?? DBNull.Value;
    }

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

    private const string ReminderSelectSql =
        """
        SELECT Id, TripPlanId, Title, Description, ReminderAt, IsCompleted, CreatedAt, UpdatedAt
        FROM dbo.Reminders
        """;
}
