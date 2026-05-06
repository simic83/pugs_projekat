using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Notes;

[DataContract]
public sealed class UpdateNoteRequestDto
{
    [DataMember(Order = 1)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string? Content { get; set; }
}
