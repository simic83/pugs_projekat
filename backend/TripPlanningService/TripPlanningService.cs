using System.Fabric;
using Microsoft.Data.SqlClient;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Contracts.Activities;
using TravelPlanner.Contracts.Checklist;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Destinations;
using TravelPlanner.Contracts.Enums;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Notes;
using TravelPlanner.Contracts.Reminders;
using TravelPlanner.Contracts.Trips;
using TripPlanningService.Configuration;
using TripPlanningService.Data;
using TripPlanningService.Models;

namespace TripPlanningService
{
    internal sealed class TripPlanningService : StatefulService, ITripPlanningService
    {
        private readonly ITripPlanningRepository repository;

        public TripPlanningService(StatefulServiceContext context)
            : base(context)
        {
            var settings = FabricConfigurationProvider.Load(context);
            repository = new TripPlanningRepository(settings.DefaultConnection);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task<List<TripPlanDto>> GetTripPlansAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return new List<TripPlanDto>();
            }

            try
            {
                var tripPlans = await repository.GetTripPlansByOwnerAsync(userId);
                return tripPlans.Select(ToDto).ToList();
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Trip plan list failed", exception);
                return new List<TripPlanDto>();
            }
        }

        public async Task<List<TripPlanDto>> GetAllTripPlansForAdminAsync()
        {
            try
            {
                var tripPlans = await repository.GetAllTripPlansAsync();
                return tripPlans.Select(ToDto).ToList();
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Admin trip plan list failed", exception);
                return new List<TripPlanDto>();
            }
        }

        public async Task<TripPlanDto?> GetTripPlanByIdAsync(Guid tripPlanId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                return tripPlan is null ? null : ToDto(tripPlan);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Trip plan lookup failed", exception);
                return null;
            }
        }

        public async Task<TripPlanDto?> CreateTripPlanAsync(CreateTripPlanRequestDto request)
        {
            if (!IsValidTripPlan(request.OwnerUserId, request.Title, request.StartDate, request.EndDate, request.PlannedBudget))
            {
                return null;
            }

            var now = DateTime.UtcNow;
            var tripPlan = new TripPlanModel
            {
                Id = Guid.NewGuid(),
                OwnerUserId = request.OwnerUserId,
                Title = request.Title.Trim(),
                Description = NormalizeOptionalText(request.Description),
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate.Date,
                PlannedBudget = request.PlannedBudget,
                Notes = NormalizeOptionalText(request.Notes),
                CreatedAt = now
            };

            try
            {
                var created = await repository.CreateTripPlanAsync(tripPlan);
                return ToDto(created);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Trip plan create failed", exception);
                return null;
            }
        }

        public async Task<TripPlanDto?> UpdateTripPlanAsync(Guid tripPlanId, Guid userId, UpdateTripPlanRequestDto request)
        {
            if (!IsValidTripPlan(userId, request.Title, request.StartDate, request.EndDate, request.PlannedBudget))
            {
                return null;
            }

            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return null;
                }

                if (!await ExistingScheduledItemsFitTripPlanAsync(tripPlanId, request.StartDate, request.EndDate))
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
                return updated ? ToDto(tripPlan) : null;
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Trip plan update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteTripPlanAsync(Guid tripPlanId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return Failure("Trip plan was not found.");
                }

                var deleted = await repository.DeleteTripPlanAsync(tripPlanId);
                return deleted ? Success("Trip plan deleted.") : Failure("Trip plan was not deleted.");
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Trip plan delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<OperationResultDto> DeleteTripPlanForAdminAsync(Guid tripPlanId)
        {
            if (tripPlanId == Guid.Empty)
            {
                return Failure("Trip plan id is invalid.");
            }

            try
            {
                var deleted = await repository.DeleteTripPlanAsync(tripPlanId);
                return deleted ? Success("Trip plan deleted.") : Failure("Trip plan was not found.");
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Admin trip plan delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<List<DestinationDto>> GetDestinationsAsync(Guid tripPlanId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return new List<DestinationDto>();
                }

                var destinations = await repository.GetDestinationsByTripPlanIdAsync(tripPlanId);
                return destinations.Select(ToDto).ToList();
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Destination list failed", exception);
                return new List<DestinationDto>();
            }
        }

        public async Task<DestinationDto?> CreateDestinationAsync(Guid tripPlanId, Guid userId, CreateDestinationRequestDto request)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null || !IsValidDestination(request.Name, request.ArrivalDate, request.DepartureDate, tripPlan))
                {
                    return null;
                }

                var destination = new DestinationModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = tripPlanId,
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
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Destination create failed", exception);
                return null;
            }
        }

        public async Task<DestinationDto?> UpdateDestinationAsync(
            Guid tripPlanId,
            Guid destinationId,
            Guid userId,
            UpdateDestinationRequestDto request)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null || !IsValidDestination(request.Name, request.ArrivalDate, request.DepartureDate, tripPlan))
                {
                    return null;
                }

                var destination = await repository.GetDestinationByIdAsync(destinationId);
                if (destination is null || destination.TripPlanId != tripPlanId)
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
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Destination update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteDestinationAsync(Guid tripPlanId, Guid destinationId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return Failure("Trip plan was not found.");
                }

                var destination = await repository.GetDestinationByIdAsync(destinationId);
                if (destination is null || destination.TripPlanId != tripPlanId)
                {
                    return Failure("Destination was not found.");
                }

                var deleted = await repository.DeleteDestinationAsync(destinationId);
                return deleted ? Success("Destination deleted.") : Failure("Destination was not deleted.");
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Destination delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<List<ActivityDto>> GetActivitiesAsync(Guid tripPlanId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return new List<ActivityDto>();
                }

                var activities = await repository.GetActivitiesByTripPlanIdAsync(tripPlanId);
                return activities.Select(ToDto).ToList();
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Activity list failed", exception);
                return new List<ActivityDto>();
            }
        }

        public async Task<ActivityDto?> CreateActivityAsync(Guid tripPlanId, Guid userId, CreateActivityRequestDto request)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null || !IsValidActivity(request.Title, request.ActivityDate, request.EstimatedCost, request.Status, tripPlan))
                {
                    return null;
                }

                var activity = new ActivityModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = tripPlanId,
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
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Activity create failed", exception);
                return null;
            }
        }

        public async Task<ActivityDto?> UpdateActivityAsync(
            Guid tripPlanId,
            Guid activityId,
            Guid userId,
            UpdateActivityRequestDto request)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null || !IsValidActivity(request.Title, request.ActivityDate, request.EstimatedCost, request.Status, tripPlan))
                {
                    return null;
                }

                var activity = await repository.GetActivityByIdAsync(activityId);
                if (activity is null || activity.TripPlanId != tripPlanId)
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
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Activity update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteActivityAsync(Guid tripPlanId, Guid activityId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return Failure("Trip plan was not found.");
                }

                var activity = await repository.GetActivityByIdAsync(activityId);
                if (activity is null || activity.TripPlanId != tripPlanId)
                {
                    return Failure("Activity was not found.");
                }

                var deleted = await repository.DeleteActivityAsync(activityId);
                return deleted ? Success("Activity deleted.") : Failure("Activity was not deleted.");
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Activity delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<List<ChecklistItemDto>> GetChecklistItemsAsync(Guid tripPlanId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return new List<ChecklistItemDto>();
                }

                var checklistItems = await repository.GetChecklistItemsByTripPlanIdAsync(tripPlanId);
                return checklistItems.Select(ToDto).ToList();
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Checklist item list failed", exception);
                return new List<ChecklistItemDto>();
            }
        }

        public async Task<ChecklistItemDto?> CreateChecklistItemAsync(
            Guid tripPlanId,
            Guid userId,
            CreateChecklistItemRequestDto request)
        {
            if (tripPlanId == Guid.Empty
                || request.TripPlanId == Guid.Empty
                || request.TripPlanId != tripPlanId
                || !IsValidChecklistTitle(request.Title))
            {
                return null;
            }

            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return null;
                }

                var checklistItem = new ChecklistItemModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = tripPlanId,
                    Title = request.Title.Trim(),
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await repository.CreateChecklistItemAsync(checklistItem);
                return ToDto(created);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Checklist item create failed", exception);
                return null;
            }
        }

        public async Task<ChecklistItemDto?> UpdateChecklistItemAsync(
            Guid tripPlanId,
            Guid checklistItemId,
            Guid userId,
            UpdateChecklistItemRequestDto request)
        {
            if (!IsValidChecklistTitle(request.Title))
            {
                return null;
            }

            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return null;
                }

                var checklistItem = await repository.GetChecklistItemByIdAsync(checklistItemId);
                if (checklistItem is null || checklistItem.TripPlanId != tripPlanId)
                {
                    return null;
                }

                checklistItem.Title = request.Title.Trim();
                checklistItem.IsCompleted = request.IsCompleted;
                checklistItem.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateChecklistItemAsync(checklistItem);
                return updated ? ToDto(checklistItem) : null;
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Checklist item update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteChecklistItemAsync(Guid tripPlanId, Guid checklistItemId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return Failure("Trip plan was not found.");
                }

                var checklistItem = await repository.GetChecklistItemByIdAsync(checklistItemId);
                if (checklistItem is null || checklistItem.TripPlanId != tripPlanId)
                {
                    return Failure("Checklist item was not found.");
                }

                var deleted = await repository.DeleteChecklistItemAsync(checklistItemId);
                return deleted ? Success("Checklist item deleted.") : Failure("Checklist item was not deleted.");
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Checklist item delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<List<NoteDto>> GetNotesAsync(Guid tripPlanId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return new List<NoteDto>();
                }

                var notes = await repository.GetNotesByTripPlanIdAsync(tripPlanId);
                return notes.Select(ToDto).ToList();
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Note list failed", exception);
                return new List<NoteDto>();
            }
        }

        public async Task<NoteDto?> CreateNoteAsync(Guid tripPlanId, Guid userId, CreateNoteRequestDto request)
        {
            if (tripPlanId == Guid.Empty
                || request.TripPlanId == Guid.Empty
                || request.TripPlanId != tripPlanId
                || !IsValidNoteTitle(request.Title))
            {
                return null;
            }

            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return null;
                }

                var note = new NoteModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = tripPlanId,
                    Title = request.Title.Trim(),
                    Content = NormalizeOptionalText(request.Content),
                    CreatedAt = DateTime.UtcNow
                };

                var created = await repository.CreateNoteAsync(note);
                return ToDto(created);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Note create failed", exception);
                return null;
            }
        }

        public async Task<NoteDto?> UpdateNoteAsync(
            Guid tripPlanId,
            Guid noteId,
            Guid userId,
            UpdateNoteRequestDto request)
        {
            if (!IsValidNoteTitle(request.Title))
            {
                return null;
            }

            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return null;
                }

                var note = await repository.GetNoteByIdAsync(noteId);
                if (note is null || note.TripPlanId != tripPlanId)
                {
                    return null;
                }

                note.Title = request.Title.Trim();
                note.Content = NormalizeOptionalText(request.Content);
                note.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateNoteAsync(note);
                return updated ? ToDto(note) : null;
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Note update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteNoteAsync(Guid tripPlanId, Guid noteId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return Failure("Trip plan was not found.");
                }

                var note = await repository.GetNoteByIdAsync(noteId);
                if (note is null || note.TripPlanId != tripPlanId)
                {
                    return Failure("Note was not found.");
                }

                var deleted = await repository.DeleteNoteAsync(noteId);
                return deleted ? Success("Note deleted.") : Failure("Note was not deleted.");
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Note delete failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<List<ReminderDto>> GetRemindersAsync(Guid tripPlanId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return new List<ReminderDto>();
                }

                var reminders = await repository.GetRemindersByTripPlanIdAsync(tripPlanId);
                return reminders.Select(ToDto).ToList();
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Reminder list failed", exception);
                return new List<ReminderDto>();
            }
        }

        public async Task<ReminderDto?> CreateReminderAsync(
            Guid tripPlanId,
            Guid userId,
            CreateReminderRequestDto request)
        {
            if (tripPlanId == Guid.Empty
                || request.TripPlanId == Guid.Empty
                || request.TripPlanId != tripPlanId
                || !IsValidReminder(request.Title, request.ReminderAt))
            {
                return null;
            }

            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return null;
                }

                var reminder = new ReminderModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = tripPlanId,
                    Title = request.Title.Trim(),
                    Description = NormalizeOptionalText(request.Description),
                    ReminderAt = request.ReminderAt,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await repository.CreateReminderAsync(reminder);
                return ToDto(created);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Reminder create failed", exception);
                return null;
            }
        }

        public async Task<ReminderDto?> UpdateReminderAsync(
            Guid tripPlanId,
            Guid reminderId,
            Guid userId,
            UpdateReminderRequestDto request)
        {
            if (reminderId == Guid.Empty || !IsValidReminder(request.Title, request.ReminderAt))
            {
                return null;
            }

            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return null;
                }

                var reminder = await repository.GetReminderByIdAsync(reminderId);
                if (reminder is null || reminder.TripPlanId != tripPlanId)
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
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Reminder update failed", exception);
                return null;
            }
        }

        public async Task<OperationResultDto> DeleteReminderAsync(Guid tripPlanId, Guid reminderId, Guid userId)
        {
            try
            {
                var tripPlan = await GetOwnedTripPlanAsync(tripPlanId, userId);
                if (tripPlan is null)
                {
                    return Failure("Trip plan was not found.");
                }

                var reminder = await repository.GetReminderByIdAsync(reminderId);
                if (reminder is null || reminder.TripPlanId != tripPlanId)
                {
                    return Failure("Reminder was not found.");
                }

                var deleted = await repository.DeleteReminderAsync(reminderId);
                return deleted ? Success("Reminder deleted.") : Failure("Reminder was not deleted.");
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Reminder delete failed", exception);
                return Failure(exception.Message);
            }
        }

        private async Task<TripPlanModel?> GetOwnedTripPlanAsync(Guid tripPlanId, Guid userId)
        {
            if (tripPlanId == Guid.Empty || userId == Guid.Empty)
            {
                return null;
            }

            var tripPlan = await repository.GetTripPlanByIdAsync(tripPlanId);
            return tripPlan?.OwnerUserId == userId ? tripPlan : null;
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

        private static bool IsValidTripPlan(Guid ownerUserId, string title, DateTime startDate, DateTime endDate, decimal plannedBudget)
        {
            return ownerUserId != Guid.Empty
                && !string.IsNullOrWhiteSpace(title)
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
