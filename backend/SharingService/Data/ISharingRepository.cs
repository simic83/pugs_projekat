using SharingService.Models;

namespace SharingService.Data;

internal interface ISharingRepository
{
    Task<ShareTokenModel> CreateShareTokenAsync(ShareTokenModel shareToken);

    Task<ShareTokenModel?> GetShareTokenByTokenAsync(string token);

    Task<List<ShareTokenModel>> GetShareTokensByTripPlanIdAsync(Guid tripPlanId);

    Task<bool> RevokeShareTokenAsync(Guid tripPlanId, Guid shareId);

    Task<bool> UserOwnsTripPlanAsync(Guid tripPlanId, Guid userId);

    Task<TripPlanModel?> GetTripPlanByIdAsync(Guid tripPlanId);

    Task<bool> UpdateTripPlanAsync(TripPlanModel tripPlan);

    Task<DestinationModel> CreateDestinationAsync(DestinationModel destination);

    Task<List<DestinationModel>> GetDestinationsByTripPlanIdAsync(Guid tripPlanId);

    Task<DestinationModel?> GetDestinationByIdForTripPlanAsync(Guid tripPlanId, Guid destinationId);

    Task<bool> UpdateDestinationAsync(DestinationModel destination);

    Task<bool> DeleteDestinationAsync(Guid tripPlanId, Guid destinationId);

    Task<ActivityModel> CreateActivityAsync(ActivityModel activity);

    Task<List<ActivityModel>> GetActivitiesByTripPlanIdAsync(Guid tripPlanId);

    Task<ActivityModel?> GetActivityByIdForTripPlanAsync(Guid tripPlanId, Guid activityId);

    Task<bool> UpdateActivityAsync(ActivityModel activity);

    Task<bool> DeleteActivityAsync(Guid tripPlanId, Guid activityId);

    Task<ExpenseModel> CreateExpenseAsync(ExpenseModel expense);

    Task<List<ExpenseModel>> GetExpensesByTripPlanIdAsync(Guid tripPlanId);

    Task<ExpenseModel?> GetExpenseByIdForTripPlanAsync(Guid tripPlanId, Guid expenseId);

    Task<bool> UpdateExpenseAsync(ExpenseModel expense);

    Task<bool> DeleteExpenseAsync(Guid tripPlanId, Guid expenseId);

    Task<decimal> GetTotalExpensesByTripPlanIdAsync(Guid tripPlanId);

    Task<ChecklistItemModel> CreateChecklistItemAsync(ChecklistItemModel checklistItem);

    Task<List<ChecklistItemModel>> GetChecklistItemsByTripPlanIdAsync(Guid tripPlanId);

    Task<ChecklistItemModel?> GetChecklistItemByIdForTripPlanAsync(Guid tripPlanId, Guid checklistItemId);

    Task<bool> UpdateChecklistItemAsync(ChecklistItemModel checklistItem);

    Task<bool> DeleteChecklistItemAsync(Guid tripPlanId, Guid checklistItemId);

    Task<NoteModel> CreateNoteAsync(NoteModel note);

    Task<List<NoteModel>> GetNotesByTripPlanIdAsync(Guid tripPlanId);

    Task<NoteModel?> GetNoteByIdForTripPlanAsync(Guid tripPlanId, Guid noteId);

    Task<bool> UpdateNoteAsync(NoteModel note);

    Task<bool> DeleteNoteAsync(Guid tripPlanId, Guid noteId);

    Task<List<ReminderModel>> GetRemindersForTripPlanAsync(Guid tripPlanId);
}
