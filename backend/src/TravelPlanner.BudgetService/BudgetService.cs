using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Contracts.DTOs.Budget;
using TravelPlanner.Contracts.Interfaces;

namespace TravelPlanner.BudgetService;

internal sealed class BudgetService : StatefulService, IBudgetService
{
    public BudgetService(StatefulServiceContext context)
        : base(context)
    {
    }

    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        return this.CreateServiceRemotingReplicaListeners();
    }

    public Task<IReadOnlyCollection<ExpenseDto>> GetExpensesAsync(Guid tripPlanId)
    {
        throw new NotImplementedException("Scaffold only. Expense query logic is not implemented yet.");
    }

    public Task<ExpenseDto> CreateExpenseAsync(CreateExpenseRequestDto request)
    {
        throw new NotImplementedException("Scaffold only. Expense creation logic is not implemented yet.");
    }

    public Task<BudgetSummaryDto> GetBudgetSummaryAsync(Guid tripPlanId)
    {
        throw new NotImplementedException("Scaffold only. Budget calculation is not implemented yet.");
    }

    public Task DeleteExpensesForTripPlanAsync(Guid tripPlanId)
    {
        throw new NotImplementedException("Scaffold only. Expense cleanup is not implemented yet.");
    }
}
