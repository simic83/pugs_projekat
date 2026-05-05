using System.Runtime.Serialization;
using TravelPlanner.Contracts.Enums;

namespace TravelPlanner.Contracts.Budget;

[DataContract]
public sealed class CreateExpenseRequestDto
{
    [DataMember(Order = 1)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 2)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public ExpenseCategory? Category { get; set; }

    [DataMember(Order = 4)]
    public decimal Amount { get; set; }

    [DataMember(Order = 5)]
    public DateTime ExpenseDate { get; set; }

    [DataMember(Order = 6)]
    public string? Description { get; set; }
}
