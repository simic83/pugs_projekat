using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SharingService.Configuration;
using SharingService.Data;
using SharingService.Models;
using TravelPlanner.Persistence;
using TravelPlanner.Contracts.Activities;
using TravelPlanner.Contracts.Budget;
using TravelPlanner.Contracts.Checklist;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Destinations;
using TravelPlanner.Contracts.Enums;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Notes;
using TravelPlanner.Contracts.Reminders;
using TravelPlanner.Contracts.Sharing;
using TravelPlanner.Contracts.Trips;

namespace SharingService
{
    internal sealed class SharingService : StatefulService, ISharingService
    {
        private readonly ServiceProvider serviceProvider;
        private readonly ISharingRepository repository;

        public SharingService(StatefulServiceContext context)
            : base(context)
        {
            var settings = FabricConfigurationProvider.Load(context);
            serviceProvider = ConfigureServices(settings);
            repository = serviceProvider.GetRequiredService<ISharingRepository>();
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        private static ServiceProvider ConfigureServices(SharingServiceSettings settings)
        {
            var services = new ServiceCollection();
            services.AddSingleton(settings);
            services.AddTravelPlannerPersistence(settings.DefaultConnection);
            services.AddSingleton<ISharingRepository, SharingRepository>();

            return services.BuildServiceProvider();
        }

        public async Task<ShareTokenDto?> CreateShareAsync(CreateShareRequestDto request)
        {
            if (!IsValidCreateShareRequest(request))
            {
                return null;
            }

            try
            {
                if (!await repository.UserOwnsTripPlanAsync(request.TripPlanId, request.CreatedByUserId))
                {
                    return null;
                }

                var shareToken = new ShareTokenModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = request.TripPlanId,
                    Token = Guid.NewGuid().ToString("N"),
                    AccessLevel = ShareTokenAccessPolicy.ToStoredAccessLevel(request.AccessLevel),
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = request.ExpiresAt,
                    IsRevoked = false
                };

                var created = await repository.CreateShareTokenAsync(shareToken);
                return ToDto(created);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Share token create failed", exception);
                return null;
            }
        }

        public async Task<ShareTokenDto?> GetShareAsync(string token)
        {
            try
            {
                var shareToken = await ValidateShareTokenForViewAsync(token);
                return shareToken is null ? null : ToDto(shareToken);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Share token lookup failed", exception);
                return null;
            }
        }

        public async Task<List<ShareTokenDto>> GetSharesByTripPlanAsync(Guid tripPlanId, Guid userId)
        {
            if (tripPlanId == Guid.Empty || userId == Guid.Empty)
            {
                return new List<ShareTokenDto>();
            }

            try
            {
                if (!await repository.UserOwnsTripPlanAsync(tripPlanId, userId))
                {
                    return new List<ShareTokenDto>();
                }

                var shareTokens = await repository.GetShareTokensByTripPlanIdAsync(tripPlanId);
                return shareTokens.Select(ToDto).ToList();
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Share token list failed", exception);
                return new List<ShareTokenDto>();
            }
        }

        public async Task<OperationResultDto> RevokeShareAsync(Guid tripPlanId, Guid shareId, Guid userId)
        {
            if (tripPlanId == Guid.Empty || shareId == Guid.Empty || userId == Guid.Empty)
            {
                return Failure("Share token request is invalid.");
            }

            try
            {
                if (!await repository.UserOwnsTripPlanAsync(tripPlanId, userId))
                {
                    return Failure("Trip plan was not found.");
                }

                var revoked = await repository.RevokeShareTokenAsync(tripPlanId, shareId);
                return revoked ? Success("Share token revoked.") : Failure("Share token was not found.");
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Share token revoke failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<SharedTripPlanDto?> GetSharedTripPlanAsync(string token)
        {
            try
            {
                var shareToken = await ValidateShareTokenForViewAsync(token);
                return shareToken is null ? null : await BuildSharedTripPlanAsync(shareToken);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared trip plan lookup failed", exception);
                return null;
            }
        }

        public async Task<SharedTripPlanDto?> UpdateSharedTripPlanAsync(string token, UpdateTripPlanRequestDto request)
        {
            if (!IsValidTripPlan(request.Title, request.StartDate, request.EndDate, request.PlannedBudget))
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return null;
                }

                var tripPlan = await repository.GetTripPlanByIdAsync(shareToken.TripPlanId);
                if (tripPlan is null)
                {
                    return null;
                }

                if (!await ExistingScheduledItemsFitTripPlanAsync(shareToken.TripPlanId, request.StartDate, request.EndDate))
                {
                    return null;
                }

                tripPlan.Title = request.Title.Trim();
                tripPlan.Description = NormalizeOptionalText(request.Description);
                tripPlan.StartDate = request.StartDate.Date;
                tripPlan.EndDate = request.EndDate.Date;
                tripPlan.PlannedBudget = request.PlannedBudget;
                tripPlan.Notes = NormalizeOptionalText(request.Notes);
                tripPlan.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateTripPlanAsync(tripPlan);
                return updated ? await BuildSharedTripPlanAsync(shareToken) : null;
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared trip plan update failed", exception);
                return null;
            }
        }

        public async Task<DestinationDto?> CreateSharedDestinationAsync(string token, CreateDestinationRequestDto request)
        {
            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return null;
                }

                var tripPlan = await repository.GetTripPlanByIdAsync(shareToken.TripPlanId);
                if (tripPlan is null || !IsValidDestination(request.Name, request.ArrivalDate, request.DepartureDate, tripPlan))
                {
                    return null;
                }

                var destination = new DestinationModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = shareToken.TripPlanId,
                    Name = request.Name.Trim(),
                    Location = NormalizeOptionalText(request.Location),
                    ArrivalDate = request.ArrivalDate.Date,
                    DepartureDate = request.DepartureDate.Date,
                    Description = NormalizeOptionalText(request.Description),
                    CreatedAt = DateTime.UtcNow
                };

                var created = await repository.CreateDestinationAsync(destination);
                return ToDto(created);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared destination create failed", exception);
                return null;
            }
        }

        public async Task<DestinationDto?> UpdateSharedDestinationAsync(
            string token,
            Guid destinationId,
            UpdateDestinationRequestDto request)
        {
            if (destinationId == Guid.Empty)
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return null;
                }

                var tripPlan = await repository.GetTripPlanByIdAsync(shareToken.TripPlanId);
                if (tripPlan is null || !IsValidDestination(request.Name, request.ArrivalDate, request.DepartureDate, tripPlan))
                {
                    return null;
                }

                var destination = await repository.GetDestinationByIdForTripPlanAsync(shareToken.TripPlanId, destinationId);
                if (destination is null)
                {
                    return null;
                }

                destination.Name = request.Name.Trim();
                destination.Location = NormalizeOptionalText(request.Location);
                destination.ArrivalDate = request.ArrivalDate.Date;
                destination.DepartureDate = request.DepartureDate.Date;
                destination.Description = NormalizeOptionalText(request.Description);
                destination.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateDestinationAsync(destination);
                return updated ? ToDto(destination) : null;
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared destination update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteSharedDestinationAsync(string token, Guid destinationId)
        {
            if (destinationId == Guid.Empty)
            {
                return Failure("Destination was not found.");
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return Failure("Share token does not allow editing.");
                }

                var deleted = await repository.DeleteDestinationAsync(shareToken.TripPlanId, destinationId);
                return deleted ? Success("Destination deleted.") : Failure("Destination was not found.");
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared destination delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<ActivityDto?> CreateSharedActivityAsync(string token, CreateActivityRequestDto request)
        {
            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return null;
                }

                var tripPlan = await repository.GetTripPlanByIdAsync(shareToken.TripPlanId);
                if (tripPlan is null || !IsValidActivity(request.Title, request.ActivityDate, request.EstimatedCost, request.Status, tripPlan))
                {
                    return null;
                }

                var activity = new ActivityModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = shareToken.TripPlanId,
                    Title = request.Title.Trim(),
                    ActivityDate = request.ActivityDate.Date,
                    ActivityTime = request.ActivityTime,
                    Location = NormalizeOptionalText(request.Location),
                    Description = NormalizeOptionalText(request.Description),
                    EstimatedCost = request.EstimatedCost,
                    Status = request.Status.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                var created = await repository.CreateActivityAsync(activity);
                return ToDto(created);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared activity create failed", exception);
                return null;
            }
        }

        public async Task<ActivityDto?> UpdateSharedActivityAsync(
            string token,
            Guid activityId,
            UpdateActivityRequestDto request)
        {
            if (activityId == Guid.Empty)
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return null;
                }

                var tripPlan = await repository.GetTripPlanByIdAsync(shareToken.TripPlanId);
                if (tripPlan is null || !IsValidActivity(request.Title, request.ActivityDate, request.EstimatedCost, request.Status, tripPlan))
                {
                    return null;
                }

                var activity = await repository.GetActivityByIdForTripPlanAsync(shareToken.TripPlanId, activityId);
                if (activity is null)
                {
                    return null;
                }

                activity.Title = request.Title.Trim();
                activity.ActivityDate = request.ActivityDate.Date;
                activity.ActivityTime = request.ActivityTime;
                activity.Location = NormalizeOptionalText(request.Location);
                activity.Description = NormalizeOptionalText(request.Description);
                activity.EstimatedCost = request.EstimatedCost;
                activity.Status = request.Status.ToString();
                activity.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateActivityAsync(activity);
                return updated ? ToDto(activity) : null;
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared activity update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteSharedActivityAsync(string token, Guid activityId)
        {
            if (activityId == Guid.Empty)
            {
                return Failure("Activity was not found.");
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return Failure("Share token does not allow editing.");
                }

                var deleted = await repository.DeleteActivityAsync(shareToken.TripPlanId, activityId);
                return deleted ? Success("Activity deleted.") : Failure("Activity was not found.");
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared activity delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<ExpenseDto?> CreateSharedExpenseAsync(string token, CreateExpenseRequestDto request)
        {
            if (!IsValidExpense(request.Title, request.Category, request.Amount, request.ExpenseDate))
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null || await repository.GetTripPlanByIdAsync(shareToken.TripPlanId) is null)
                {
                    return null;
                }

                var expense = new ExpenseModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = shareToken.TripPlanId,
                    Title = request.Title.Trim(),
                    Category = request.Category.GetValueOrDefault().ToString(),
                    Amount = request.Amount,
                    ExpenseDate = request.ExpenseDate.Date,
                    Description = NormalizeOptionalText(request.Description),
                    CreatedAt = DateTime.UtcNow
                };

                var created = await repository.CreateExpenseAsync(expense);
                return ToDto(created);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared expense create failed", exception);
                return null;
            }
        }

        public async Task<ExpenseDto?> UpdateSharedExpenseAsync(
            string token,
            Guid expenseId,
            UpdateExpenseRequestDto request)
        {
            if (expenseId == Guid.Empty || !IsValidExpense(request.Title, request.Category, request.Amount, request.ExpenseDate))
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return null;
                }

                var expense = await repository.GetExpenseByIdForTripPlanAsync(shareToken.TripPlanId, expenseId);
                if (expense is null)
                {
                    return null;
                }

                expense.Title = request.Title.Trim();
                expense.Category = request.Category.GetValueOrDefault().ToString();
                expense.Amount = request.Amount;
                expense.ExpenseDate = request.ExpenseDate.Date;
                expense.Description = NormalizeOptionalText(request.Description);
                expense.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateExpenseAsync(expense);
                return updated ? ToDto(expense) : null;
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared expense update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteSharedExpenseAsync(string token, Guid expenseId)
        {
            if (expenseId == Guid.Empty)
            {
                return Failure("Expense was not found.");
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return Failure("Share token does not allow editing.");
                }

                var deleted = await repository.DeleteExpenseAsync(shareToken.TripPlanId, expenseId);
                return deleted ? Success("Expense deleted.") : Failure("Expense was not found.");
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared expense delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<ChecklistItemDto?> CreateSharedChecklistItemAsync(
            string token,
            CreateChecklistItemRequestDto request)
        {
            if (!IsValidChecklistTitle(request.Title))
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null || await repository.GetTripPlanByIdAsync(shareToken.TripPlanId) is null)
                {
                    return null;
                }

                var checklistItem = new ChecklistItemModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = shareToken.TripPlanId,
                    Title = request.Title.Trim(),
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await repository.CreateChecklistItemAsync(checklistItem);
                return ToDto(created);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared checklist item create failed", exception);
                return null;
            }
        }

        public async Task<ChecklistItemDto?> UpdateSharedChecklistItemAsync(
            string token,
            Guid checklistItemId,
            UpdateChecklistItemRequestDto request)
        {
            if (checklistItemId == Guid.Empty || !IsValidChecklistTitle(request.Title))
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return null;
                }

                var checklistItem = await repository.GetChecklistItemByIdForTripPlanAsync(
                    shareToken.TripPlanId,
                    checklistItemId);
                if (checklistItem is null)
                {
                    return null;
                }

                checklistItem.Title = request.Title.Trim();
                checklistItem.IsCompleted = request.IsCompleted;
                checklistItem.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateChecklistItemAsync(checklistItem);
                return updated ? ToDto(checklistItem) : null;
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared checklist item update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteSharedChecklistItemAsync(string token, Guid checklistItemId)
        {
            if (checklistItemId == Guid.Empty)
            {
                return Failure("Checklist item was not found.");
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return Failure("Share token does not allow editing.");
                }

                var deleted = await repository.DeleteChecklistItemAsync(shareToken.TripPlanId, checklistItemId);
                return deleted ? Success("Checklist item deleted.") : Failure("Checklist item was not found.");
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared checklist item delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<NoteDto?> CreateSharedNoteAsync(string token, CreateNoteRequestDto request)
        {
            if (!IsValidNoteTitle(request.Title))
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null || await repository.GetTripPlanByIdAsync(shareToken.TripPlanId) is null)
                {
                    return null;
                }

                var note = new NoteModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = shareToken.TripPlanId,
                    Title = request.Title.Trim(),
                    Content = NormalizeOptionalText(request.Content),
                    CreatedAt = DateTime.UtcNow
                };

                var created = await repository.CreateNoteAsync(note);
                return ToDto(created);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared note create failed", exception);
                return null;
            }
        }

        public async Task<NoteDto?> UpdateSharedNoteAsync(string token, Guid noteId, UpdateNoteRequestDto request)
        {
            if (noteId == Guid.Empty || !IsValidNoteTitle(request.Title))
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return null;
                }

                var note = await repository.GetNoteByIdForTripPlanAsync(shareToken.TripPlanId, noteId);
                if (note is null)
                {
                    return null;
                }

                note.Title = request.Title.Trim();
                note.Content = NormalizeOptionalText(request.Content);
                note.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateNoteAsync(note);
                return updated ? ToDto(note) : null;
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared note update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteSharedNoteAsync(string token, Guid noteId)
        {
            if (noteId == Guid.Empty)
            {
                return Failure("Note was not found.");
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return Failure("Share token does not allow editing.");
                }

                var deleted = await repository.DeleteNoteAsync(shareToken.TripPlanId, noteId);
                return deleted ? Success("Note deleted.") : Failure("Note was not found.");
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared note delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<ReminderDto?> CreateSharedReminderAsync(string token, CreateReminderRequestDto request)
        {
            if (!IsValidReminder(request.Title, request.ReminderAt))
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null || await repository.GetTripPlanByIdAsync(shareToken.TripPlanId) is null)
                {
                    return null;
                }

                var reminder = new ReminderModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = shareToken.TripPlanId,
                    Title = request.Title.Trim(),
                    Description = NormalizeOptionalText(request.Description),
                    ReminderAt = request.ReminderAt,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await repository.CreateReminderAsync(reminder);
                return ToDto(created);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared reminder create failed", exception);
                return null;
            }
        }

        public async Task<ReminderDto?> UpdateSharedReminderAsync(
            string token,
            Guid reminderId,
            UpdateReminderRequestDto request)
        {
            if (reminderId == Guid.Empty || !IsValidReminder(request.Title, request.ReminderAt))
            {
                return null;
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return null;
                }

                var reminder = await repository.GetReminderByIdForTripPlanAsync(shareToken.TripPlanId, reminderId);
                if (reminder is null)
                {
                    return null;
                }

                reminder.Title = request.Title.Trim();
                reminder.Description = NormalizeOptionalText(request.Description);
                reminder.ReminderAt = request.ReminderAt;
                reminder.IsCompleted = request.IsCompleted;
                reminder.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateReminderAsync(reminder);
                return updated ? ToDto(reminder) : null;
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared reminder update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteSharedReminderAsync(string token, Guid reminderId)
        {
            if (reminderId == Guid.Empty)
            {
                return Failure("Reminder was not found.");
            }

            try
            {
                var shareToken = await ValidateShareTokenForEditAsync(token);
                if (shareToken is null)
                {
                    return Failure("Share token does not allow editing.");
                }

                var deleted = await repository.DeleteReminderAsync(shareToken.TripPlanId, reminderId);
                return deleted ? Success("Reminder deleted.") : Failure("Reminder was not found.");
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                LogDatabaseError("Shared reminder delete failed", exception);
                return Failure(exception.Message);
            }
        }

        private async Task<ShareTokenModel?> ValidateShareTokenForViewAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var shareToken = await repository.GetShareTokenByTokenAsync(token.Trim());
            return ShareTokenAccessPolicy.AllowsView(shareToken, DateTime.UtcNow) ? shareToken : null;
        }

        private async Task<ShareTokenModel?> ValidateShareTokenForEditAsync(string token)
        {
            var shareToken = await ValidateShareTokenForViewAsync(token);
            return ShareTokenAccessPolicy.AllowsEdit(shareToken, DateTime.UtcNow) ? shareToken : null;
        }

        private async Task<SharedTripPlanDto?> BuildSharedTripPlanAsync(ShareTokenModel shareToken)
        {
            var tripPlan = await repository.GetTripPlanByIdAsync(shareToken.TripPlanId);
            if (tripPlan is null)
            {
                return null;
            }

            var destinations = await repository.GetDestinationsByTripPlanIdAsync(shareToken.TripPlanId);
            var activities = await repository.GetActivitiesByTripPlanIdAsync(shareToken.TripPlanId);
            var expenses = await repository.GetExpensesByTripPlanIdAsync(shareToken.TripPlanId);
            var totalExpenses = await repository.GetTotalExpensesByTripPlanIdAsync(shareToken.TripPlanId);
            var checklistItems = await repository.GetChecklistItemsByTripPlanIdAsync(shareToken.TripPlanId);
            var notes = await repository.GetNotesByTripPlanIdAsync(shareToken.TripPlanId);
            var reminders = await repository.GetRemindersForTripPlanAsync(shareToken.TripPlanId);
            var accessLevel = ShareTokenAccessPolicy.ParseAccessLevel(shareToken.AccessLevel);

            return new SharedTripPlanDto
            {
                Share = ToDto(shareToken),
                TripPlan = ToDto(tripPlan),
                AccessLevel = accessLevel,
                Destinations = destinations.Select(ToDto).ToList(),
                Activities = activities.Select(ToDto).ToList(),
                Expenses = expenses.Select(ToDto).ToList(),
                BudgetSummary = new BudgetSummaryDto
                {
                    TripPlanId = tripPlan.Id,
                    PlannedBudget = tripPlan.PlannedBudget,
                    TotalExpenses = totalExpenses,
                    RemainingBudget = tripPlan.PlannedBudget - totalExpenses
                },
                ChecklistItems = checklistItems.Select(ToDto).ToList(),
                Notes = notes.Select(ToDto).ToList(),
                Reminders = reminders.Select(ToDto).ToList()
            };
        }

        private static bool IsValidCreateShareRequest(CreateShareRequestDto request)
        {
            return request.TripPlanId != Guid.Empty
                && request.CreatedByUserId != Guid.Empty
                && Enum.IsDefined(typeof(ShareAccessLevel), request.AccessLevel)
                && (!request.ExpiresAt.HasValue || request.ExpiresAt.Value > DateTime.UtcNow);
        }

        private async Task<bool> ExistingScheduledItemsFitTripPlanAsync(Guid tripPlanId, DateTime startDate, DateTime endDate)
        {
            var destinations = await repository.GetDestinationsByTripPlanIdAsync(tripPlanId);
            if (destinations.Any(destination => !IsDateRangeWithinRange(destination.ArrivalDate, destination.DepartureDate, startDate, endDate)))
            {
                return false;
            }

            var activities = await repository.GetActivitiesByTripPlanIdAsync(tripPlanId);
            return activities.All(activity => IsDateWithinRange(activity.ActivityDate, startDate, endDate));
        }

        private static bool IsValidTripPlan(string title, DateTime startDate, DateTime endDate, decimal plannedBudget)
        {
            return !string.IsNullOrWhiteSpace(title)
                && IsValidDateRange(startDate, endDate)
                && plannedBudget >= 0;
        }

        private static bool IsValidDestination(string name, DateTime arrivalDate, DateTime departureDate, TripPlanModel tripPlan)
        {
            return !string.IsNullOrWhiteSpace(name)
                && IsDateRangeWithinTrip(arrivalDate, departureDate, tripPlan);
        }

        private static bool IsValidActivity(string title, DateTime activityDate, decimal estimatedCost, ActivityStatus status, TripPlanModel tripPlan)
        {
            return !string.IsNullOrWhiteSpace(title)
                && IsDateWithinTrip(activityDate, tripPlan)
                && estimatedCost >= 0
                && Enum.IsDefined(typeof(ActivityStatus), status);
        }

        private static bool IsValidDateRange(DateTime startDate, DateTime endDate)
        {
            return IsRequiredDate(startDate)
                && IsRequiredDate(endDate)
                && endDate.Date >= startDate.Date;
        }

        private static bool IsDateRangeWithinTrip(DateTime startDate, DateTime endDate, TripPlanModel tripPlan)
        {
            return IsDateRangeWithinRange(startDate, endDate, tripPlan.StartDate, tripPlan.EndDate);
        }

        private static bool IsDateRangeWithinRange(DateTime startDate, DateTime endDate, DateTime rangeStart, DateTime rangeEnd)
        {
            return IsValidDateRange(startDate, endDate)
                && IsValidDateRange(rangeStart, rangeEnd)
                && startDate.Date >= rangeStart.Date
                && endDate.Date <= rangeEnd.Date;
        }

        private static bool IsDateWithinTrip(DateTime date, TripPlanModel tripPlan)
        {
            return IsDateWithinRange(date, tripPlan.StartDate, tripPlan.EndDate);
        }

        private static bool IsDateWithinRange(DateTime date, DateTime rangeStart, DateTime rangeEnd)
        {
            return IsRequiredDate(date)
                && IsValidDateRange(rangeStart, rangeEnd)
                && date.Date >= rangeStart.Date
                && date.Date <= rangeEnd.Date;
        }

        private static bool IsRequiredDate(DateTime date)
        {
            return date.Date != default;
        }

        private static bool IsValidExpense(
            string title,
            ExpenseCategory? category,
            decimal amount,
            DateTime expenseDate)
        {
            return !string.IsNullOrWhiteSpace(title)
                && category.HasValue
                && Enum.IsDefined(typeof(ExpenseCategory), category.Value)
                && amount >= 0
                && expenseDate != default;
        }

        private static bool IsValidChecklistTitle(string title)
        {
            return !string.IsNullOrWhiteSpace(title)
                && title.Trim().Length <= 150;
        }

        private static bool IsValidNoteTitle(string title)
        {
            return !string.IsNullOrWhiteSpace(title)
                && title.Trim().Length <= 150;
        }

        private static bool IsValidReminder(string title, DateTime reminderAt)
        {
            return !string.IsNullOrWhiteSpace(title)
                && title.Trim().Length <= 150
                && reminderAt != default;
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static ShareTokenDto ToDto(ShareTokenModel shareToken)
        {
            return new ShareTokenDto
            {
                Id = shareToken.Id,
                TripPlanId = shareToken.TripPlanId,
                Token = shareToken.Token,
                AccessLevel = ShareTokenAccessPolicy.ParseAccessLevel(shareToken.AccessLevel),
                CreatedByUserId = shareToken.CreatedByUserId,
                CreatedAt = shareToken.CreatedAt,
                ExpiresAt = shareToken.ExpiresAt,
                IsRevoked = shareToken.IsRevoked
            };
        }

        private static TripPlanDto ToDto(TripPlanModel tripPlan)
        {
            return new TripPlanDto
            {
                Id = tripPlan.Id,
                OwnerUserId = tripPlan.OwnerUserId,
                Title = tripPlan.Title,
                Description = tripPlan.Description,
                StartDate = tripPlan.StartDate,
                EndDate = tripPlan.EndDate,
                PlannedBudget = tripPlan.PlannedBudget,
                Notes = tripPlan.Notes,
                CreatedAtUtc = tripPlan.CreatedAt,
                UpdatedAtUtc = tripPlan.UpdatedAt
            };
        }

        private static DestinationDto ToDto(DestinationModel destination)
        {
            return new DestinationDto
            {
                Id = destination.Id,
                TripPlanId = destination.TripPlanId,
                Name = destination.Name,
                Location = destination.Location,
                ArrivalDate = destination.ArrivalDate,
                DepartureDate = destination.DepartureDate,
                Description = destination.Description,
                CreatedAtUtc = destination.CreatedAt,
                UpdatedAtUtc = destination.UpdatedAt
            };
        }

        private static ActivityDto ToDto(ActivityModel activity)
        {
            return new ActivityDto
            {
                Id = activity.Id,
                TripPlanId = activity.TripPlanId,
                Title = activity.Title,
                ActivityDate = activity.ActivityDate,
                ActivityTime = activity.ActivityTime,
                Location = activity.Location,
                Description = activity.Description,
                EstimatedCost = activity.EstimatedCost,
                Status = ParseStatus(activity.Status),
                CreatedAtUtc = activity.CreatedAt,
                UpdatedAtUtc = activity.UpdatedAt
            };
        }

        private static ExpenseDto ToDto(ExpenseModel expense)
        {
            return new ExpenseDto
            {
                Id = expense.Id,
                TripPlanId = expense.TripPlanId,
                Title = expense.Title,
                Category = ParseCategory(expense.Category),
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Description = expense.Description,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt
            };
        }

        private static ChecklistItemDto ToDto(ChecklistItemModel checklistItem)
        {
            return new ChecklistItemDto
            {
                Id = checklistItem.Id,
                TripPlanId = checklistItem.TripPlanId,
                Title = checklistItem.Title,
                IsCompleted = checklistItem.IsCompleted,
                CreatedAt = checklistItem.CreatedAt,
                UpdatedAt = checklistItem.UpdatedAt
            };
        }

        private static NoteDto ToDto(NoteModel note)
        {
            return new NoteDto
            {
                Id = note.Id,
                TripPlanId = note.TripPlanId,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }

        private static ReminderDto ToDto(ReminderModel reminder)
        {
            return new ReminderDto
            {
                Id = reminder.Id,
                TripPlanId = reminder.TripPlanId,
                Title = reminder.Title,
                Description = reminder.Description,
                ReminderAt = reminder.ReminderAt,
                IsCompleted = reminder.IsCompleted,
                CreatedAt = reminder.CreatedAt,
                UpdatedAt = reminder.UpdatedAt
            };
        }

        private static ActivityStatus ParseStatus(string status)
        {
            return Enum.TryParse<ActivityStatus>(status, ignoreCase: true, out var parsed)
                && Enum.IsDefined(typeof(ActivityStatus), parsed)
                    ? parsed
                    : ActivityStatus.Planned;
        }

        private static ExpenseCategory ParseCategory(string category)
        {
            return Enum.TryParse<ExpenseCategory>(category, ignoreCase: true, out var parsed)
                && Enum.IsDefined(typeof(ExpenseCategory), parsed)
                    ? parsed
                    : ExpenseCategory.Other;
        }

        private static bool IsPersistenceException(Exception exception)
        {
            return PersistenceExceptionClassifier.IsPersistenceException(exception);
        }

        private static OperationResultDto Success(string message)
        {
            return new OperationResultDto
            {
                Succeeded = true,
                Message = message
            };
        }

        private static OperationResultDto Failure(string message)
        {
            return new OperationResultDto
            {
                Succeeded = false,
                Message = message
            };
        }

        private void LogDatabaseError(string message, Exception exception)
        {
            ServiceEventSource.Current.ServiceMessage(Context, "{0}: {1}", message, exception.Message);
        }
    }
}
