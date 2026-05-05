using Microsoft.ServiceFabric.Services.Remoting;
using TravelPlanner.Contracts.Budget;
using TravelPlanner.Contracts.Common;

namespace TravelPlanner.Contracts.Interfaces;

public interface IBudgetService : IService
{
    Task<List<ExpenseDto>> GetExpensesAsync(Guid tripPlanId, Guid userId);

    Task<ExpenseDto?> GetExpenseByIdAsync(Guid tripPlanId, Guid expenseId, Guid userId);

    Task<ExpenseDto?> CreateExpenseAsync(Guid tripPlanId, Guid userId, CreateExpenseRequestDto request);

    Task<ExpenseDto?> UpdateExpenseAsync(Guid tripPlanId, Guid expenseId, Guid userId, UpdateExpenseRequestDto request);

    Task<OperationResultDto> DeleteExpenseAsync(Guid tripPlanId, Guid expenseId, Guid userId);

    Task<BudgetSummaryDto?> GetBudgetSummaryAsync(Guid tripPlanId, Guid userId);
}
