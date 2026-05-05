using BudgetService.Models;
using Microsoft.Data.SqlClient;

namespace BudgetService.Data;

internal sealed class BudgetRepository : IBudgetRepository
{
    private readonly string connectionString;

    public BudgetRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<ExpenseModel> CreateExpenseAsync(ExpenseModel expense)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            INSERT INTO dbo.Expenses
                (Id, TripPlanId, Title, Category, Amount, ExpenseDate, Description, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @TripPlanId, @Title, @Category, @Amount, @ExpenseDate, @Description, @CreatedAt, @UpdatedAt);
            """,
            connection);

        AddExpenseParameters(command, expense);
        await command.ExecuteNonQueryAsync();
        return expense;
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

    public async Task<ExpenseModel?> GetExpenseByIdAsync(Guid expenseId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(ExpenseSelectSql + " WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", expenseId);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadExpense(reader) : null;
    }

    public async Task<bool> UpdateExpenseAsync(ExpenseModel expense)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            UPDATE dbo.Expenses
            SET Title = @Title,
                Category = @Category,
                Amount = @Amount,
                ExpenseDate = @ExpenseDate,
                Description = @Description,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """,
            connection);

        AddExpenseParameters(command, expense);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteExpenseAsync(Guid expenseId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand("DELETE FROM dbo.Expenses WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", expenseId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<decimal> GetTotalByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            "SELECT COALESCE(SUM(Amount), 0) FROM dbo.Expenses WHERE TripPlanId = @TripPlanId;",
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);

        var result = await command.ExecuteScalarAsync();
        return result is decimal total ? total : 0m;
    }

    public async Task<decimal?> GetPlannedBudgetForOwnerAsync(Guid tripPlanId, Guid ownerUserId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = new SqlCommand(
            """
            SELECT PlannedBudget
            FROM dbo.TripPlans
            WHERE Id = @TripPlanId AND OwnerUserId = @OwnerUserId;
            """,
            connection);
        command.Parameters.AddWithValue("@TripPlanId", tripPlanId);
        command.Parameters.AddWithValue("@OwnerUserId", ownerUserId);

        var result = await command.ExecuteScalarAsync();
        return result is decimal plannedBudget ? plannedBudget : null;
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

    private static void AddExpenseParameters(SqlCommand command, ExpenseModel expense)
    {
        command.Parameters.AddWithValue("@Id", expense.Id);
        command.Parameters.AddWithValue("@TripPlanId", expense.TripPlanId);
        command.Parameters.AddWithValue("@Title", expense.Title);
        command.Parameters.AddWithValue("@Category", expense.Category);
        command.Parameters.AddWithValue("@Amount", expense.Amount);
        command.Parameters.AddWithValue("@ExpenseDate", expense.ExpenseDate.Date);
        command.Parameters.AddWithValue("@Description", ToDbValue(expense.Description));
        command.Parameters.AddWithValue("@CreatedAt", expense.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", ToDbValue(expense.UpdatedAt));
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

    private static object ToDbValue(object? value)
    {
        return value ?? DBNull.Value;
    }

    private const string ExpenseSelectSql =
        """
        SELECT Id, TripPlanId, Title, Category, Amount, ExpenseDate, Description, CreatedAt, UpdatedAt
        FROM dbo.Expenses
        """;
}
