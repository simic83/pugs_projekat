using System.Runtime.Serialization;
using TravelPlanner.Contracts.Activities;
using TravelPlanner.Contracts.Destinations;
using TravelPlanner.Contracts.Trips;

namespace TravelPlanner.Contracts.Sharing;

[DataContract]
public sealed class SharedTripPlanDto
{
    [DataMember(Order = 1)]
    public ShareTokenDto? Share { get; set; }

    [DataMember(Order = 2)]
    public TripPlanDto? TripPlan { get; set; }

    [DataMember(Order = 3)]
    public List<DestinationDto> Destinations { get; set; } = new();

    [DataMember(Order = 4)]
    public List<ActivityDto> Activities { get; set; } = new();
}
