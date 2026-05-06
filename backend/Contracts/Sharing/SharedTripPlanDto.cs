using System.Runtime.Serialization;
using TravelPlanner.Contracts.Activities;
using TravelPlanner.Contracts.Budget;
using TravelPlanner.Contracts.Checklist;
using TravelPlanner.Contracts.Destinations;
using TravelPlanner.Contracts.Enums;
using TravelPlanner.Contracts.Notes;
using TravelPlanner.Contracts.Trips;

namespace TravelPlanner.Contracts.Sharing;

[DataContract]
public sealed class SharedTripPlanDto
{
    [DataMember(Order = 1)]
    public ShareTokenDto? Share { get; set; }

    [DataMember(Order = 2)]
    public TripPlanDto? TripPlan { get; set; }

    [DataMember(Order = 3)]
    public ShareAccessLevel AccessLevel { get; set; }

    [DataMember(Order = 4)]
    public List<DestinationDto> Destinations { get; set; } = new();

    [DataMember(Order = 5)]
    public List<ActivityDto> Activities { get; set; } = new();

    [DataMember(Order = 6)]
    public List<ExpenseDto> Expenses { get; set; } = new();

    [DataMember(Order = 7)]
    public BudgetSummaryDto? BudgetSummary { get; set; }

    [DataMember(Order = 8)]
    public List<ChecklistItemDto> ChecklistItems { get; set; } = new();

    [DataMember(Order = 9)]
    public List<NoteDto> Notes { get; set; } = new();
}
