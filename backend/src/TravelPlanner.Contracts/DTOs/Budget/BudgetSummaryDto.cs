namespace TravelPlanner.Contracts.DTOs.Budget;

public sealed class BudgetSummaryDto
{
    public Guid TripPlanId { get; set; }
    public decimal PlannedBudget { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal RemainingBudget { get; set; }
}

