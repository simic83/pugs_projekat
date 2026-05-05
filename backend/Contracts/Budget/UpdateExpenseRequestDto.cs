using System.Runtime.Serialization;
using TravelPlanner.Contracts.Enums;

namespace TravelPlanner.Contracts.Budget;

[DataContract]
public sealed class UpdateExpenseRequestDto
{
    [DataMember(Order = 1)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public ExpenseCategory? Category { get; set; }

    [DataMember(Order = 3)]
    public decimal Amount { get; set; }

    [DataMember(Order = 4)]
    public DateTime ExpenseDate { get; set; }

    [DataMember(Order = 5)]
    public string? Description { get; set; }
}
