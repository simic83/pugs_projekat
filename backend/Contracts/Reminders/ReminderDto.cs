using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Reminders;

[DataContract]
public sealed class ReminderDto
{
    [DataMember(Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Order = 2)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 3)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string? Description { get; set; }

    [DataMember(Order = 5)]
    public DateTime ReminderAt { get; set; }

    [DataMember(Order = 6)]
    public bool IsCompleted { get; set; }

    [DataMember(Order = 7)]
    public DateTime CreatedAt { get; set; }

    [DataMember(Order = 8)]
    public DateTime? UpdatedAt { get; set; }
}
