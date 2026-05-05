using System.Runtime.Serialization;
using TravelPlanner.Contracts.Enums;

namespace TravelPlanner.Contracts.Sharing;

[DataContract]
public sealed class ShareTokenDto
{
    [DataMember(Order = 1)]
    public string Token { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 3)]
    public ShareAccessLevel AccessLevel { get; set; }

    [DataMember(Order = 4)]
    public DateTime CreatedAtUtc { get; set; }

    [DataMember(Order = 5)]
    public DateTime? ExpiresAtUtc { get; set; }
}
