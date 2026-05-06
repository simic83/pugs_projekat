using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Reminders;

[DataContract]
public sealed class UpdateReminderRequestDto
{
    [DataMember(Order = 1)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string? Description { get; set; }

    [DataMember(Order = 3)]
    public DateTime ReminderAt { get; set; }

    [DataMember(Order = 4)]
    public bool IsCompleted { get; set; }
}
