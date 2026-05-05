using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Trips;

[DataContract]
public sealed class TripPlanDto
{
    [DataMember(Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Order = 2)]
    public Guid OwnerUserId { get; set; }

    [DataMember(Order = 3)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string? Description { get; set; }

    [DataMember(Order = 5)]
    public DateTime StartDate { get; set; }

    [DataMember(Order = 6)]
    public DateTime EndDate { get; set; }

    [DataMember(Order = 7)]
    public decimal PlannedBudget { get; set; }

    [DataMember(Order = 8)]
    public string? Notes { get; set; }

    [DataMember(Order = 9)]
    public DateTime CreatedAtUtc { get; set; }

    [DataMember(Order = 10)]
    public DateTime? UpdatedAtUtc { get; set; }
}
