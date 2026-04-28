using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Contracts.DTOs.Trips;
using TravelPlanner.Contracts.Interfaces;

namespace TravelPlanner.TripPlanningService;

internal sealed class TripPlanningService : StatefulService, ITripPlanningService
{
    public TripPlanningService(StatefulServiceContext context)
        : base(context)
    {
    }

    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        return this.CreateServiceRemotingReplicaListeners();
    }

    public Task<IReadOnlyCollection<TripPlanDto>> GetTripPlansForUserAsync(Guid userId)
    {
        throw new NotImplementedException("Scaffold only. Trip query logic is not implemented yet.");
    }

    public Task<TripPlanDto?> GetTripPlanByIdAsync(Guid tripPlanId, Guid requesterUserId)
    {
        throw new NotImplementedException("Scaffold only. Trip lookup logic is not implemented yet.");
    }

    public Task<TripPlanDto> CreateTripPlanAsync(CreateTripPlanRequestDto request)
    {
        throw new NotImplementedException("Scaffold only. Trip creation logic is not implemented yet.");
    }

    public Task DeleteTripPlanAsync(Guid tripPlanId, Guid requesterUserId)
    {
        throw new NotImplementedException("Scaffold only. Cascading delete orchestration is not implemented yet.");
    }

    public Task<DestinationDto> CreateDestinationAsync(CreateDestinationRequestDto request)
    {
        throw new NotImplementedException("Scaffold only. Destination creation logic is not implemented yet.");
    }

    public Task<ActivityDto> CreateActivityAsync(CreateActivityRequestDto request)
    {
        throw new NotImplementedException("Scaffold only. Activity creation logic is not implemented yet.");
    }

    public Task DeleteChildEntitiesForTripPlanAsync(Guid tripPlanId)
    {
        throw new NotImplementedException("Scaffold only. Child entity cleanup is not implemented yet.");
    }
}
