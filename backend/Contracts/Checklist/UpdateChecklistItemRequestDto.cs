using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Checklist;

[DataContract]
public sealed class UpdateChecklistItemRequestDto
{
    [DataMember(Order = 1)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public bool IsCompleted { get; set; }
}
