using Microsoft.ServiceFabric.Services.Remoting;
using TravelPlanner.Contracts.Activities;
using TravelPlanner.Contracts.Checklist;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Destinations;
using TravelPlanner.Contracts.Trips;

namespace TravelPlanner.Contracts.Interfaces;

public interface ITripPlanningService : IService
{
    Task<List<TripPlanDto>> GetTripPlansAsync(Guid userId);

    Task<TripPlanDto?> GetTripPlanByIdAsync(Guid tripPlanId, Guid userId);

    Task<TripPlanDto?> CreateTripPlanAsync(CreateTripPlanRequestDto request);

    Task<TripPlanDto?> UpdateTripPlanAsync(Guid tripPlanId, Guid userId, UpdateTripPlanRequestDto request);

    Task<OperationResultDto> DeleteTripPlanAsync(Guid tripPlanId, Guid userId);

    Task<List<DestinationDto>> GetDestinationsAsync(Guid tripPlanId, Guid userId);

    Task<DestinationDto?> CreateDestinationAsync(Guid tripPlanId, Guid userId, CreateDestinationRequestDto request);

    Task<DestinationDto?> UpdateDestinationAsync(Guid tripPlanId, Guid destinationId, Guid userId, UpdateDestinationRequestDto request);

    Task<OperationResultDto> DeleteDestinationAsync(Guid tripPlanId, Guid destinationId, Guid userId);

    Task<List<ActivityDto>> GetActivitiesAsync(Guid tripPlanId, Guid userId);

    Task<ActivityDto?> CreateActivityAsync(Guid tripPlanId, Guid userId, CreateActivityRequestDto request);

    Task<ActivityDto?> UpdateActivityAsync(Guid tripPlanId, Guid activityId, Guid userId, UpdateActivityRequestDto request);

    Task<OperationResultDto> DeleteActivityAsync(Guid tripPlanId, Guid activityId, Guid userId);

    Task<List<ChecklistItemDto>> GetChecklistItemsAsync(Guid tripPlanId, Guid userId);

    Task<ChecklistItemDto?> CreateChecklistItemAsync(Guid tripPlanId, Guid userId, CreateChecklistItemRequestDto request);

    Task<ChecklistItemDto?> UpdateChecklistItemAsync(Guid tripPlanId, Guid checklistItemId, Guid userId, UpdateChecklistItemRequestDto request);

    Task<OperationResultDto> DeleteChecklistItemAsync(Guid tripPlanId, Guid checklistItemId, Guid userId);
}
