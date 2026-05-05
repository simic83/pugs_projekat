using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Destinations;

[DataContract]
public sealed class DestinationDto
{
    [DataMember(Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Order = 2)]
    public Guid TripPlanId { get; set; }

    [DataMember(Order = 3)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string? Location { get; set; }

    [DataMember(Order = 5)]
    public DateTime ArrivalDate { get; set; }

    [DataMember(Order = 6)]
    public DateTime DepartureDate { get; set; }

    [DataMember(Order = 7)]
    public string? Description { get; set; }

    [DataMember(Order = 8)]
    public DateTime CreatedAtUtc { get; set; }

    [DataMember(Order = 9)]
    public DateTime? UpdatedAtUtc { get; set; }
}
