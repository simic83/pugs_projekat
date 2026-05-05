using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Checklist;

[DataContract]
public sealed class CreateChecklistItemRequestDto
{
    [DataMember(Order = 1)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 2)]
    public string Title { get; set; } = string.Empty;
}
