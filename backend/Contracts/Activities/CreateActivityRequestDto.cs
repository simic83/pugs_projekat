using System.Runtime.Serialization;
using TravelPlanner.Contracts.Enums;

namespace TravelPlanner.Contracts.Activities;

[DataContract]
public sealed class CreateActivityRequestDto
{
    [DataMember(Order = 1)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public DateTime ActivityDate { get; set; }

    [DataMember(Order = 3)]
    public TimeSpan? ActivityTime { get; set; }

    [DataMember(Order = 4)]
    public string? Location { get; set; }

    [DataMember(Order = 5)]
    public string? Description { get; set; }

    [DataMember(Order = 6)]
    public decimal EstimatedCost { get; set; }

    [DataMember(Order = 7)]
    public ActivityStatus Status { get; set; }
}
