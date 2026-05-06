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

    Task<List<DestinationModel>> GetDestinationsByTripPlanIdAsync(Guid tripPlanId);

    Task<List<ActivityModel>> GetActivitiesByTripPlanIdAsync(Guid tripPlanId);

    Task<List<ExpenseModel>> GetExpensesByTripPlanIdAsync(Guid tripPlanId);

    Task<decimal> GetTotalExpensesByTripPlanIdAsync(Guid tripPlanId);

    Task<List<ChecklistItemModel>> GetChecklistItemsByTripPlanIdAsync(Guid tripPlanId);

    Task<List<NoteModel>> GetNotesByTripPlanIdAsync(Guid tripPlanId);
}
