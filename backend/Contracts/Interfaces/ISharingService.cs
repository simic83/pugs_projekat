using Microsoft.ServiceFabric.Services.Remoting;
using TravelPlanner.Contracts.Activities;
using TravelPlanner.Contracts.Budget;
using TravelPlanner.Contracts.Checklist;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Destinations;
using TravelPlanner.Contracts.Sharing;
using TravelPlanner.Contracts.Notes;
using TravelPlanner.Contracts.Trips;

namespace TravelPlanner.Contracts.Interfaces;

public interface ISharingService : IService
{
    Task<ShareTokenDto?> CreateShareAsync(CreateShareRequestDto request);

    Task<ShareTokenDto?> GetShareAsync(string token);

    Task<List<ShareTokenDto>> GetSharesByTripPlanAsync(Guid tripPlanId, Guid userId);

    Task<OperationResultDto> RevokeShareAsync(Guid tripPlanId, Guid shareId, Guid userId);

    Task<SharedTripPlanDto?> GetSharedTripPlanAsync(string token);

    Task<SharedTripPlanDto?> UpdateSharedTripPlanAsync(string token, UpdateTripPlanRequestDto request);

    Task<DestinationDto?> CreateSharedDestinationAsync(string token, CreateDestinationRequestDto request);

    Task<DestinationDto?> UpdateSharedDestinationAsync(string token, Guid destinationId, UpdateDestinationRequestDto request);

    Task<OperationResultDto> DeleteSharedDestinationAsync(string token, Guid destinationId);

    Task<ActivityDto?> CreateSharedActivityAsync(string token, CreateActivityRequestDto request);

    Task<ActivityDto?> UpdateSharedActivityAsync(string token, Guid activityId, UpdateActivityRequestDto request);

    Task<OperationResultDto> DeleteSharedActivityAsync(string token, Guid activityId);

    Task<ExpenseDto?> CreateSharedExpenseAsync(string token, CreateExpenseRequestDto request);

    Task<ExpenseDto?> UpdateSharedExpenseAsync(string token, Guid expenseId, UpdateExpenseRequestDto request);

    Task<OperationResultDto> DeleteSharedExpenseAsync(string token, Guid expenseId);

    Task<ChecklistItemDto?> CreateSharedChecklistItemAsync(string token, CreateChecklistItemRequestDto request);

    Task<ChecklistItemDto?> UpdateSharedChecklistItemAsync(string token, Guid checklistItemId, UpdateChecklistItemRequestDto request);

    Task<OperationResultDto> DeleteSharedChecklistItemAsync(string token, Guid checklistItemId);

    Task<NoteDto?> CreateSharedNoteAsync(string token, CreateNoteRequestDto request);

    Task<NoteDto?> UpdateSharedNoteAsync(string token, Guid noteId, UpdateNoteRequestDto request);

    Task<OperationResultDto> DeleteSharedNoteAsync(string token, Guid noteId);
}
