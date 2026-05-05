using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Checklist;

[DataContract]
public sealed class ChecklistItemDto
{
    [DataMember(Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Order = 2)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 3)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public bool IsCompleted { get; set; }

    [DataMember(Order = 5)]
    public DateTime CreatedAt { get; set; }

    [DataMember(Order = 6)]
    public DateTime? UpdatedAt { get; set; }
}
