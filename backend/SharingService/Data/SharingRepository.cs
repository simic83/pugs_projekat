using Microsoft.EntityFrameworkCore;
using SharingService.Models;
using TravelPlanner.Persistence;
using TravelPlanner.Persistence.Entities;

namespace SharingService.Data;

internal sealed class SharingRepository : ISharingRepository
{
    private readonly IDbContextFactory<TravelPlannerDbContext> dbContextFactory;

    public SharingRepository(IDbContextFactory<TravelPlannerDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<ShareTokenModel> CreateShareTokenAsync(ShareTokenModel shareToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        context.ShareTokens.Add(ToEntity(shareToken));
        await context.SaveChangesAsync();
        return shareToken;
    }

    public async Task<ShareTokenModel?> GetShareTokenByTokenAsync(string token)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var shareToken = await context.ShareTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(shareToken => shareToken.Token == token);

        return shareToken is null ? null : ToModel(shareToken);
    }

    public async Task<List<ShareTokenModel>> GetShareTokensByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var shareTokens = await context.ShareTokens
            .AsNoTracking()
            .Where(shareToken => shareToken.TripPlanId == tripPlanId)
            .OrderByDescending(shareToken => shareToken.CreatedAt)
            .ToListAsync();

        return shareTokens.Select(ToModel).ToList();
    }

    public async Task<bool> RevokeShareTokenAsync(Guid tripPlanId, Guid shareId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var shareToken = await context.ShareTokens
            .SingleOrDefaultAsync(current => current.Id == shareId && current.TripPlanId == tripPlanId);
        if (shareToken is null)
        {
            return false;
        }

        if (shareToken.IsRevoked)
        {
            return true;
        }

        shareToken.IsRevoked = true;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UserOwnsTripPlanAsync(Guid tripPlanId, Guid userId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        return await context.TripPlans
            .AsNoTracking()
            .AnyAsync(tripPlan => tripPlan.Id == tripPlanId && tripPlan.OwnerUserId == userId);
    }

    public async Task<TripPlanModel?> GetTripPlanByIdAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var tripPlan = await context.TripPlans
            .AsNoTracking()
            .SingleOrDefaultAsync(tripPlan => tripPlan.Id == tripPlanId);

        return tripPlan is null ? null : ToModel(tripPlan);
    }

    public async Task<bool> UpdateTripPlanAsync(TripPlanModel tripPlan)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.TripPlans
            .SingleOrDefaultAsync(current => current.Id == tripPlan.Id);
        if (entity is null)
        {
            return false;
        }

        entity.Title = tripPlan.Title;
        entity.Description = tripPlan.Description;
        entity.StartDate = tripPlan.StartDate;
        entity.EndDate = tripPlan.EndDate;
        entity.PlannedBudget = tripPlan.PlannedBudget;
        entity.Notes = tripPlan.Notes;
        entity.UpdatedAt = tripPlan.UpdatedAt;

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<DestinationModel> CreateDestinationAsync(DestinationModel destination)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        context.Destinations.Add(ToEntity(destination));
        await context.SaveChangesAsync();
        return destination;
    }

    public async Task<List<DestinationModel>> GetDestinationsByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var destinations = await context.Destinations
            .AsNoTracking()
            .Where(destination => destination.TripPlanId == tripPlanId)
            .OrderBy(destination => destination.ArrivalDate)
            .ThenBy(destination => destination.Name)
            .ToListAsync();

        return destinations.Select(ToModel).ToList();
    }

    public async Task<DestinationModel?> GetDestinationByIdForTripPlanAsync(Guid tripPlanId, Guid destinationId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var destination = await context.Destinations
            .AsNoTracking()
            .SingleOrDefaultAsync(destination => destination.Id == destinationId && destination.TripPlanId == tripPlanId);

        return destination is null ? null : ToModel(destination);
    }

    public async Task<bool> UpdateDestinationAsync(DestinationModel destination)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Destinations
            .SingleOrDefaultAsync(current => current.Id == destination.Id && current.TripPlanId == destination.TripPlanId);
        if (entity is null)
        {
            return false;
        }

        entity.Name = destination.Name;
        entity.Location = destination.Location;
        entity.ArrivalDate = destination.ArrivalDate;
        entity.DepartureDate = destination.DepartureDate;
        entity.Description = destination.Description;
        entity.UpdatedAt = destination.UpdatedAt;

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteDestinationAsync(Guid tripPlanId, Guid destinationId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Destinations
            .SingleOrDefaultAsync(destination => destination.Id == destinationId && destination.TripPlanId == tripPlanId);
        if (entity is null)
        {
            return false;
        }

        context.Destinations.Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<ActivityModel> CreateActivityAsync(ActivityModel activity)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        context.Activities.Add(ToEntity(activity));
        await context.SaveChangesAsync();
        return activity;
    }

    public async Task<List<ActivityModel>> GetActivitiesByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var activities = await context.Activities
            .AsNoTracking()
            .Where(activity => activity.TripPlanId == tripPlanId)
            .OrderBy(activity => activity.ActivityDate)
            .ThenBy(activity => activity.ActivityTime)
            .ThenBy(activity => activity.Title)
            .ToListAsync();

        return activities.Select(ToModel).ToList();
    }

    public async Task<ActivityModel?> GetActivityByIdForTripPlanAsync(Guid tripPlanId, Guid activityId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var activity = await context.Activities
            .AsNoTracking()
            .SingleOrDefaultAsync(activity => activity.Id == activityId && activity.TripPlanId == tripPlanId);

        return activity is null ? null : ToModel(activity);
    }

    public async Task<bool> UpdateActivityAsync(ActivityModel activity)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Activities
            .SingleOrDefaultAsync(current => current.Id == activity.Id && current.TripPlanId == activity.TripPlanId);
        if (entity is null)
        {
            return false;
        }

        entity.Title = activity.Title;
        entity.ActivityDate = activity.ActivityDate;
        entity.ActivityTime = activity.ActivityTime;
        entity.Location = activity.Location;
        entity.Description = activity.Description;
        entity.EstimatedCost = activity.EstimatedCost;
        entity.Status = activity.Status;
        entity.UpdatedAt = activity.UpdatedAt;

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteActivityAsync(Guid tripPlanId, Guid activityId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Activities
            .SingleOrDefaultAsync(activity => activity.Id == activityId && activity.TripPlanId == tripPlanId);
        if (entity is null)
        {
            return false;
        }

        context.Activities.Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<ExpenseModel> CreateExpenseAsync(ExpenseModel expense)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        context.Expenses.Add(ToEntity(expense));
        await context.SaveChangesAsync();
        return expense;
    }

    public async Task<List<ExpenseModel>> GetExpensesByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var expenses = await context.Expenses
            .AsNoTracking()
            .Where(expense => expense.TripPlanId == tripPlanId)
            .OrderByDescending(expense => expense.ExpenseDate)
            .ThenByDescending(expense => expense.CreatedAt)
            .ToListAsync();

        return expenses.Select(ToModel).ToList();
    }

    public async Task<ExpenseModel?> GetExpenseByIdForTripPlanAsync(Guid tripPlanId, Guid expenseId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var expense = await context.Expenses
            .AsNoTracking()
            .SingleOrDefaultAsync(expense => expense.Id == expenseId && expense.TripPlanId == tripPlanId);

        return expense is null ? null : ToModel(expense);
    }

    public async Task<bool> UpdateExpenseAsync(ExpenseModel expense)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Expenses
            .SingleOrDefaultAsync(current => current.Id == expense.Id && current.TripPlanId == expense.TripPlanId);
        if (entity is null)
        {
            return false;
        }

        entity.Title = expense.Title;
        entity.Category = expense.Category;
        entity.Amount = expense.Amount;
        entity.ExpenseDate = expense.ExpenseDate;
        entity.Description = expense.Description;
        entity.UpdatedAt = expense.UpdatedAt;

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteExpenseAsync(Guid tripPlanId, Guid expenseId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Expenses
            .SingleOrDefaultAsync(expense => expense.Id == expenseId && expense.TripPlanId == tripPlanId);
        if (entity is null)
        {
            return false;
        }

        context.Expenses.Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<decimal> GetTotalExpensesByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var total = await context.Expenses
            .Where(expense => expense.TripPlanId == tripPlanId)
            .Select(expense => (decimal?)expense.Amount)
            .SumAsync();

        return total ?? 0m;
    }

    public async Task<ChecklistItemModel> CreateChecklistItemAsync(ChecklistItemModel checklistItem)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        context.ChecklistItems.Add(ToEntity(checklistItem));
        await context.SaveChangesAsync();
        return checklistItem;
    }

    public async Task<List<ChecklistItemModel>> GetChecklistItemsByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var checklistItems = await context.ChecklistItems
            .AsNoTracking()
            .Where(checklistItem => checklistItem.TripPlanId == tripPlanId)
            .OrderBy(checklistItem => checklistItem.IsCompleted)
            .ThenBy(checklistItem => checklistItem.CreatedAt)
            .ThenBy(checklistItem => checklistItem.Title)
            .ToListAsync();

        return checklistItems.Select(ToModel).ToList();
    }

    public async Task<ChecklistItemModel?> GetChecklistItemByIdForTripPlanAsync(Guid tripPlanId, Guid checklistItemId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var checklistItem = await context.ChecklistItems
            .AsNoTracking()
            .SingleOrDefaultAsync(checklistItem => checklistItem.Id == checklistItemId && checklistItem.TripPlanId == tripPlanId);

        return checklistItem is null ? null : ToModel(checklistItem);
    }

    public async Task<bool> UpdateChecklistItemAsync(ChecklistItemModel checklistItem)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.ChecklistItems
            .SingleOrDefaultAsync(current => current.Id == checklistItem.Id && current.TripPlanId == checklistItem.TripPlanId);
        if (entity is null)
        {
            return false;
        }

        entity.Title = checklistItem.Title;
        entity.IsCompleted = checklistItem.IsCompleted;
        entity.UpdatedAt = checklistItem.UpdatedAt;

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteChecklistItemAsync(Guid tripPlanId, Guid checklistItemId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.ChecklistItems
            .SingleOrDefaultAsync(checklistItem => checklistItem.Id == checklistItemId && checklistItem.TripPlanId == tripPlanId);
        if (entity is null)
        {
            return false;
        }

        context.ChecklistItems.Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<NoteModel> CreateNoteAsync(NoteModel note)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        context.Notes.Add(ToEntity(note));
        await context.SaveChangesAsync();
        return note;
    }

    public async Task<List<NoteModel>> GetNotesByTripPlanIdAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var notes = await context.Notes
            .AsNoTracking()
            .Where(note => note.TripPlanId == tripPlanId)
            .OrderByDescending(note => note.CreatedAt)
            .ThenBy(note => note.Title)
            .ToListAsync();

        return notes.Select(ToModel).ToList();
    }

    public async Task<NoteModel?> GetNoteByIdForTripPlanAsync(Guid tripPlanId, Guid noteId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var note = await context.Notes
            .AsNoTracking()
            .SingleOrDefaultAsync(note => note.Id == noteId && note.TripPlanId == tripPlanId);

        return note is null ? null : ToModel(note);
    }

    public async Task<bool> UpdateNoteAsync(NoteModel note)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Notes
            .SingleOrDefaultAsync(current => current.Id == note.Id && current.TripPlanId == note.TripPlanId);
        if (entity is null)
        {
            return false;
        }

        entity.Title = note.Title;
        entity.Content = note.Content;
        entity.UpdatedAt = note.UpdatedAt;

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteNoteAsync(Guid tripPlanId, Guid noteId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Notes
            .SingleOrDefaultAsync(note => note.Id == noteId && note.TripPlanId == tripPlanId);
        if (entity is null)
        {
            return false;
        }

        context.Notes.Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<List<ReminderModel>> GetRemindersForTripPlanAsync(Guid tripPlanId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var reminders = await context.Reminders
            .AsNoTracking()
            .Where(reminder => reminder.TripPlanId == tripPlanId)
            .OrderBy(reminder => reminder.IsCompleted)
            .ThenBy(reminder => reminder.ReminderAt)
            .ThenBy(reminder => reminder.CreatedAt)
            .ToListAsync();

        return reminders.Select(ToModel).ToList();
    }

    public async Task<ReminderModel> CreateReminderAsync(ReminderModel reminder)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        context.Reminders.Add(ToEntity(reminder));
        await context.SaveChangesAsync();
        return reminder;
    }

    public async Task<ReminderModel?> GetReminderByIdForTripPlanAsync(Guid tripPlanId, Guid reminderId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var reminder = await context.Reminders
            .AsNoTracking()
            .SingleOrDefaultAsync(reminder => reminder.Id == reminderId && reminder.TripPlanId == tripPlanId);

        return reminder is null ? null : ToModel(reminder);
    }

    public async Task<bool> UpdateReminderAsync(ReminderModel reminder)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Reminders
            .SingleOrDefaultAsync(current => current.Id == reminder.Id && current.TripPlanId == reminder.TripPlanId);
        if (entity is null)
        {
            return false;
        }

        entity.Title = reminder.Title;
        entity.Description = reminder.Description;
        entity.ReminderAt = reminder.ReminderAt;
        entity.IsCompleted = reminder.IsCompleted;
        entity.UpdatedAt = reminder.UpdatedAt;

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteReminderAsync(Guid tripPlanId, Guid reminderId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var entity = await context.Reminders
            .SingleOrDefaultAsync(reminder => reminder.Id == reminderId && reminder.TripPlanId == tripPlanId);
        if (entity is null)
        {
            return false;
        }

        context.Reminders.Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    private static ShareTokenEntity ToEntity(ShareTokenModel shareToken)
    {
        return new ShareTokenEntity
        {
            Id = shareToken.Id,
            TripPlanId = shareToken.TripPlanId,
            Token = shareToken.Token,
            AccessLevel = shareToken.AccessLevel,
            CreatedByUserId = shareToken.CreatedByUserId,
            CreatedAt = shareToken.CreatedAt,
            ExpiresAt = shareToken.ExpiresAt,
            IsRevoked = shareToken.IsRevoked
        };
    }

    private static DestinationEntity ToEntity(DestinationModel destination)
    {
        return new DestinationEntity
        {
            Id = destination.Id,
            TripPlanId = destination.TripPlanId,
            Name = destination.Name,
            Location = destination.Location,
            ArrivalDate = destination.ArrivalDate,
            DepartureDate = destination.DepartureDate,
            Description = destination.Description,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }

    private static ActivityEntity ToEntity(ActivityModel activity)
    {
        return new ActivityEntity
        {
            Id = activity.Id,
            TripPlanId = activity.TripPlanId,
            Title = activity.Title,
            ActivityDate = activity.ActivityDate,
            ActivityTime = activity.ActivityTime,
            Location = activity.Location,
            Description = activity.Description,
            EstimatedCost = activity.EstimatedCost,
            Status = activity.Status,
            CreatedAt = activity.CreatedAt,
            UpdatedAt = activity.UpdatedAt
        };
    }

    private static ExpenseEntity ToEntity(ExpenseModel expense)
    {
        return new ExpenseEntity
        {
            Id = expense.Id,
            TripPlanId = expense.TripPlanId,
            Title = expense.Title,
            Category = expense.Category,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Description = expense.Description,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }

    private static ChecklistItemEntity ToEntity(ChecklistItemModel checklistItem)
    {
        return new ChecklistItemEntity
        {
            Id = checklistItem.Id,
            TripPlanId = checklistItem.TripPlanId,
            Title = checklistItem.Title,
            IsCompleted = checklistItem.IsCompleted,
            CreatedAt = checklistItem.CreatedAt,
            UpdatedAt = checklistItem.UpdatedAt
        };
    }

    private static NoteEntity ToEntity(NoteModel note)
    {
        return new NoteEntity
        {
            Id = note.Id,
            TripPlanId = note.TripPlanId,
            Title = note.Title,
            Content = note.Content,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }

    private static ReminderEntity ToEntity(ReminderModel reminder)
    {
        return new ReminderEntity
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

    private static ShareTokenModel ToModel(ShareTokenEntity shareToken)
    {
        return new ShareTokenModel
        {
            Id = shareToken.Id,
            TripPlanId = shareToken.TripPlanId,
            Token = shareToken.Token,
            AccessLevel = shareToken.AccessLevel,
            CreatedByUserId = shareToken.CreatedByUserId,
            CreatedAt = shareToken.CreatedAt,
            ExpiresAt = shareToken.ExpiresAt,
            IsRevoked = shareToken.IsRevoked
        };
    }

    private static TripPlanModel ToModel(TripPlanEntity tripPlan)
    {
        return new TripPlanModel
        {
            Id = tripPlan.Id,
            OwnerUserId = tripPlan.OwnerUserId,
            Title = tripPlan.Title,
            Description = tripPlan.Description,
            StartDate = tripPlan.StartDate,
            EndDate = tripPlan.EndDate,
            PlannedBudget = tripPlan.PlannedBudget,
            Notes = tripPlan.Notes,
            CreatedAt = tripPlan.CreatedAt,
            UpdatedAt = tripPlan.UpdatedAt
        };
    }

    private static DestinationModel ToModel(DestinationEntity destination)
    {
        return new DestinationModel
        {
            Id = destination.Id,
            TripPlanId = destination.TripPlanId,
            Name = destination.Name,
            Location = destination.Location,
            ArrivalDate = destination.ArrivalDate,
            DepartureDate = destination.DepartureDate,
            Description = destination.Description,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }

    private static ActivityModel ToModel(ActivityEntity activity)
    {
        return new ActivityModel
        {
            Id = activity.Id,
            TripPlanId = activity.TripPlanId,
            Title = activity.Title,
            ActivityDate = activity.ActivityDate,
            ActivityTime = activity.ActivityTime,
            Location = activity.Location,
            Description = activity.Description,
            EstimatedCost = activity.EstimatedCost,
            Status = activity.Status,
            CreatedAt = activity.CreatedAt,
            UpdatedAt = activity.UpdatedAt
        };
    }

    private static ExpenseModel ToModel(ExpenseEntity expense)
    {
        return new ExpenseModel
        {
            Id = expense.Id,
            TripPlanId = expense.TripPlanId,
            Title = expense.Title,
            Category = expense.Category,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Description = expense.Description,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }

    private static ChecklistItemModel ToModel(ChecklistItemEntity checklistItem)
    {
        return new ChecklistItemModel
        {
            Id = checklistItem.Id,
            TripPlanId = checklistItem.TripPlanId,
            Title = checklistItem.Title,
            IsCompleted = checklistItem.IsCompleted,
            CreatedAt = checklistItem.CreatedAt,
            UpdatedAt = checklistItem.UpdatedAt
        };
    }

    private static NoteModel ToModel(NoteEntity note)
    {
        return new NoteModel
        {
            Id = note.Id,
            TripPlanId = note.TripPlanId,
            Title = note.Title,
            Content = note.Content,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }

    private static ReminderModel ToModel(ReminderEntity reminder)
    {
        return new ReminderModel
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
}
