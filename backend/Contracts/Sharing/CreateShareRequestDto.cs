using System.Runtime.Serialization;
using TravelPlanner.Contracts.Enums;

namespace TravelPlanner.Contracts.Sharing;

[DataContract]
public sealed class CreateShareRequestDto
{
    [DataMember(Order = 1)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 2)]
    public Guid CreatedByUserId { get; set; }

    [DataMember(Order = 3)]
    public ShareAccessLevel AccessLevel { get; set; }

    [DataMember(Order = 4)]
    public DateTime? ExpiresAt { get; set; }
}
