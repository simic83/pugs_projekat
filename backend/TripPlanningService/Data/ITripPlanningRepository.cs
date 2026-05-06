using TripPlanningService.Models;

namespace TripPlanningService.Data;

internal interface ITripPlanningRepository
{
    Task<TripPlanModel> CreateTripPlanAsync(TripPlanModel tripPlan);

    Task<TripPlanModel?> GetTripPlanByIdAsync(Guid tripPlanId);

    Task<List<TripPlanModel>> GetTripPlansByOwnerAsync(Guid ownerUserId);

    Task<List<TripPlanModel>> GetAllTripPlansAsync();

    Task<bool> UpdateTripPlanAsync(TripPlanModel tripPlan);

    Task<bool> DeleteTripPlanAsync(Guid tripPlanId);

    Task<DestinationModel> CreateDestinationAsync(DestinationModel destination);

    Task<List<DestinationModel>> GetDestinationsByTripPlanIdAsync(Guid tripPlanId);

    Task<DestinationModel?> GetDestinationByIdAsync(Guid destinationId);

    Task<bool> UpdateDestinationAsync(DestinationModel destination);

    Task<bool> DeleteDestinationAsync(Guid destinationId);

    Task<ActivityModel> CreateActivityAsync(ActivityModel activity);

    Task<List<ActivityModel>> GetActivitiesByTripPlanIdAsync(Guid tripPlanId);

    Task<ActivityModel?> GetActivityByIdAsync(Guid activityId);

    Task<bool> UpdateActivityAsync(ActivityModel activity);

    Task<bool> DeleteActivityAsync(Guid activityId);

    Task<ChecklistItemModel> CreateChecklistItemAsync(ChecklistItemModel checklistItem);

    Task<List<ChecklistItemModel>> GetChecklistItemsByTripPlanIdAsync(Guid tripPlanId);

    Task<ChecklistItemModel?> GetChecklistItemByIdAsync(Guid checklistItemId);

    Task<bool> UpdateChecklistItemAsync(ChecklistItemModel checklistItem);

    Task<bool> DeleteChecklistItemAsync(Guid checklistItemId);

    Task<NoteModel> CreateNoteAsync(NoteModel note);

    Task<List<NoteModel>> GetNotesByTripPlanIdAsync(Guid tripPlanId);

    Task<NoteModel?> GetNoteByIdAsync(Guid noteId);

    Task<bool> UpdateNoteAsync(NoteModel note);

    Task<bool> DeleteNoteAsync(Guid noteId);

    Task<ReminderModel> CreateReminderAsync(ReminderModel reminder);

    Task<List<ReminderModel>> GetRemindersByTripPlanIdAsync(Guid tripPlanId);

    Task<ReminderModel?> GetReminderByIdAsync(Guid reminderId);

    Task<bool> UpdateReminderAsync(ReminderModel reminder);

    Task<bool> DeleteReminderAsync(Guid reminderId);
}
