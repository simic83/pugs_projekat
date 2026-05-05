using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Trips;

[DataContract]
public sealed class CreateTripPlanRequestDto
{
    [DataMember(Order = 1)]
    public Guid OwnerUserId { get; set; }

    [DataMember(Order = 2)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string? Description { get; set; }

    [DataMember(Order = 4)]
    public DateTime StartDate { get; set; }

    [DataMember(Order = 5)]
    public DateTime EndDate { get; set; }

    [DataMember(Order = 6)]
    public decimal PlannedBudget { get; set; }

    [DataMember(Order = 7)]
    public string? Notes { get; set; }
}
