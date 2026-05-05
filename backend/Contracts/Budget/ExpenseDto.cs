using System.Runtime.Serialization;
using TravelPlanner.Contracts.Enums;

namespace TravelPlanner.Contracts.Budget;

[DataContract]
public sealed class ExpenseDto
{
    [DataMember(Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Order = 2)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 3)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public ExpenseCategory Category { get; set; }

    [DataMember(Order = 5)]
    public decimal Amount { get; set; }

    [DataMember(Order = 6)]
    public DateTime ExpenseDate { get; set; }

    [DataMember(Order = 7)]
    public string? Description { get; set; }

    [DataMember(Order = 8)]
    public DateTime CreatedAt { get; set; }

    [DataMember(Order = 9)]
    public DateTime? UpdatedAt { get; set; }
}
