using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Notes;

[DataContract]
public sealed class CreateNoteRequestDto
{
    [DataMember(Order = 1)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 2)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string? Content { get; set; }
}
