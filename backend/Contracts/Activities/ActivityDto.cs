using System.Runtime.Serialization;
using TravelPlanner.Contracts.Enums;

namespace TravelPlanner.Contracts.Activities;

[DataContract]
public sealed class ActivityDto
{
    [DataMember(Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Order = 2)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 3)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public DateTime ActivityDate { get; set; }

    [DataMember(Order = 5)]
    public TimeSpan? ActivityTime { get; set; }

    [DataMember(Order = 6)]
    public string? Location { get; set; }

    [DataMember(Order = 7)]
    public string? Description { get; set; }

    [DataMember(Order = 8)]
    public decimal EstimatedCost { get; set; }

    [DataMember(Order = 9)]
    public ActivityStatus Status { get; set; }

    [DataMember(Order = 10)]
    public DateTime CreatedAtUtc { get; set; }

    [DataMember(Order = 11)]
    public DateTime? UpdatedAtUtc { get; set; }
}
