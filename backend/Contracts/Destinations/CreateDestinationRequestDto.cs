using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Destinations;

[DataContract]
public sealed class CreateDestinationRequestDto
{
    [DataMember(Order = 1)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string? Location { get; set; }

    [DataMember(Order = 3)]
    public DateTime ArrivalDate { get; set; }

    [DataMember(Order = 4)]
    public DateTime DepartureDate { get; set; }

    [DataMember(Order = 5)]
    public string? Description { get; set; }
}
