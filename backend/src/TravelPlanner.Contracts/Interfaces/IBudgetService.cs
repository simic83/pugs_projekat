using Microsoft.ServiceFabric.Services.Remoting;
using TravelPlanner.Contracts.DTOs.Budget;

namespace TravelPlanner.Contracts.Interfaces;

public interface IBudgetService : IService
{
    Task<IReadOnlyCollection<ExpenseDto>> GetExpensesAsync(Guid tripPlanId);

    Task<ExpenseDto> CreateExpenseAsync(CreateExpenseRequestDto request);

    Task<BudgetSummaryDto> GetBudgetSummaryAsync(Guid tripPlanId);

    Task DeleteExpensesForTripPlanAsync(Guid tripPlanId);
}

