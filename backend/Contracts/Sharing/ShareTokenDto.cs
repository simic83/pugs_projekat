using System.Runtime.Serialization;
using TravelPlanner.Contracts.Enums;

namespace TravelPlanner.Contracts.Sharing;

[DataContract]
public sealed class ShareTokenDto
{
    [DataMember(Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Order = 2)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 3)]
    public string Token { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public ShareAccessLevel AccessLevel { get; set; }

    [DataMember(Order = 5)]
    public Guid CreatedByUserId { get; set; }

    [DataMember(Order = 6)]
    public DateTime CreatedAt { get; set; }

    [DataMember(Order = 7)]
    public DateTime? ExpiresAt { get; set; }

    [DataMember(Order = 8)]
    public bool IsRevoked { get; set; }
}
