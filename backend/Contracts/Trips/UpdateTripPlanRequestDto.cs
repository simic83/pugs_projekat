using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Trips;

[DataContract]
public sealed class UpdateTripPlanRequestDto
{
    [DataMember(Order = 1)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string? Description { get; set; }

    [DataMember(Order = 3)]
    public DateTime StartDate { get; set; }

    [DataMember(Order = 4)]
    public DateTime EndDate { get; set; }

    [DataMember(Order = 5)]
    public decimal PlannedBudget { get; set; }

    [DataMember(Order = 6)]
    public string? Notes { get; set; }
}
