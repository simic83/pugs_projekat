using BudgetService.Models;

namespace BudgetService.Data;

internal interface IBudgetRepository
{
    Task<ExpenseModel> CreateExpenseAsync(ExpenseModel expense);

    Task<List<ExpenseModel>> GetExpensesByTripPlanIdAsync(Guid tripPlanId);

    Task<ExpenseModel?> GetExpenseByIdAsync(Guid expenseId);

    Task<bool> UpdateExpenseAsync(ExpenseModel expense);

    Task<bool> DeleteExpenseAsync(Guid expenseId);

    Task<decimal> GetTotalByTripPlanIdAsync(Guid tripPlanId);

    Task<decimal?> GetPlannedBudgetForOwnerAsync(Guid tripPlanId, Guid ownerUserId);
}
