using Microsoft.ServiceFabric.Services.Remoting;
using TravelPlanner.Contracts.DTOs.Trips;

namespace TravelPlanner.Contracts.Interfaces;

public interface ITripPlanningService : IService
{
    Task<IReadOnlyCollection<TripPlanDto>> GetTripPlansForUserAsync(Guid userId);

    Task<TripPlanDto?> GetTripPlanByIdAsync(Guid tripPlanId, Guid requesterUserId);

    Task<TripPlanDto> CreateTripPlanAsync(CreateTripPlanRequestDto request);

    Task DeleteTripPlanAsync(Guid tripPlanId, Guid requesterUserId);

    Task<DestinationDto> CreateDestinationAsync(CreateDestinationRequestDto request);

    Task<ActivityDto> CreateActivityAsync(CreateActivityRequestDto request);

    Task DeleteChildEntitiesForTripPlanAsync(Guid tripPlanId);
}

