using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Budget;

[DataContract]
public sealed class BudgetSummaryDto
{
    [DataMember(Order = 1)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 2)]
    public decimal PlannedBudget { get; set; }

    [DataMember(Order = 3)]
    public decimal TotalExpenses { get; set; }

    [DataMember(Order = 4)]
    public decimal RemainingBudget { get; set; }
}
