using System.Runtime.Serialization;
using TravelPlanner.Contracts.Trips;

namespace TravelPlanner.Contracts.Sharing;

[DataContract]
public sealed class SharedTripPlanDto
{
    [DataMember(Order = 1)]
    public ShareTokenDto? Share { get; set; }

    [DataMember(Order = 2)]
    public TripPlanDto? TripPlan { get; set; }
}
