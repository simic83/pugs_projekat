using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Reminders;

[DataContract]
public sealed class CreateReminderRequestDto
{
    [DataMember(Order = 1)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 2)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string? Description { get; set; }

    [DataMember(Order = 4)]
    public DateTime ReminderAt { get; set; }
}
