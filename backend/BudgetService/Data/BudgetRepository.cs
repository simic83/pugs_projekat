using BudgetService.Models;
using Microsoft.EntityFrameworkCore;
using TravelPlanner.Persistence;
using TravelPlanner.Persistence.Entities;

namespace BudgetService.Data;

internal sealed class BudgetRepository : IBudgetRepository
{
    private readonly IDbContextFactory<TravelPlannerDbContext> dbContextFactory;

    public BudgetRepository(IDbContextFactory<TravelPlannerDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<ExpenseModel> CreateExpenseAsync(ExpenseModel expense)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        context.Expenses.Add(ToEntity(expense));
        await context.SaveChangesAsync();
        return expense;
    }

    public async Task<List<ExpenseModel>> GetExpensesByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var expenses = await context.Expenses
            .AsNoTracking()
            .Where(expense => expense.TripPlanId == tripPlanId)
            .OrderByDescending(expense => expense.ExpenseDate)
            .ThenByDescending(expense => expense.CreatedAt)
            .ToListAsync();

        return expenses.Select(ToModel).ToList();
    }

    public async Task<ExpenseModel?> GetExpenseByIdAsync(Guid expenseId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var expense = await context.Expenses
            .AsNoTracking()
            .SingleOrDefaultAsync(expense => expense.Id == expenseId);

        return expense is null ? null : ToModel(expense);
    }

    public async Task<bool> UpdateExpenseAsync(ExpenseModel expense)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Expenses
            .SingleOrDefaultAsync(current => current.Id == expense.Id);
        if (entity is null)
        {
            return false;
        }

        entity.Title = expense.Title;
        entity.Category = expense.Category;
        entity.Amount = expense.Amount;
        entity.ExpenseDate = expense.ExpenseDate;
        entity.Description = expense.Description;
        entity.UpdatedAt = expense.UpdatedAt;

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteExpenseAsync(Guid expenseId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Expenses
            .SingleOrDefaultAsync(expense => expense.Id == expenseId);
        if (entity is null)
        {
            return false;
        }

        context.Expenses.Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<decimal> GetTotalByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var total = await context.Expenses
            .Where(expense => expense.TripPlanId == tripPlanId)
            .Select(expense => (decimal?)expense.Amount)
            .SumAsync();

        return total ?? 0m;
    }

    public async Task<decimal?> GetPlannedBudgetForOwnerAsync(Guid tripPlanId, Guid ownerUserId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        return await context.TripPlans
            .AsNoTracking()
            .Where(tripPlan => tripPlan.Id == tripPlanId && tripPlan.OwnerUserId == ownerUserId)
            .Select(tripPlan => (decimal?)tripPlan.PlannedBudget)
            .SingleOrDefaultAsync();
    }

    private static ExpenseEntity ToEntity(ExpenseModel expense)
    {
        return new ExpenseEntity
        {
            Id = expense.Id,
            TripPlanId = expense.TripPlanId,
            Title = expense.Title,
            Category = expense.Category,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Description = expense.Description,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }

    private static ExpenseModel ToModel(ExpenseEntity expense)
    {
        return new ExpenseModel
        {
            Id = expense.Id,
            TripPlanId = expense.TripPlanId,
            Title = expense.Title,
            Category = expense.Category,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Description = expense.Description,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }
}
