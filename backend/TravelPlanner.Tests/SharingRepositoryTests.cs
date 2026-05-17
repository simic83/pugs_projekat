using SharingService;
using SharingService.Data;
using SharingService.Models;
using TravelPlanner.Contracts.Enums;
using TravelPlanner.Persistence.Entities;
using Xunit;

namespace TravelPlanner.Tests;

public sealed class SharingRepositoryTests
{
    [Fact]
    public async Task ShareTokenMethods_PersistViewAndEditTokensWithEf()
    {
        var tripPlanId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var factory = TestTravelPlannerDbContextFactory.Create();
        await SeedTripPlanAsync(factory, tripPlanId, ownerUserId);

        var repository = new SharingRepository(factory);
        var viewToken = CreateShareToken(tripPlanId, ownerUserId, "view-token", "VIEW", new DateTime(2026, 5, 17, 9, 0, 0));
        var editToken = CreateShareToken(tripPlanId, ownerUserId, "edit-token", "EDIT", new DateTime(2026, 5, 17, 10, 0, 0));

        await repository.CreateShareTokenAsync(viewToken);
        await repository.CreateShareTokenAsync(editToken);

        var loadedViewToken = await repository.GetShareTokenByTokenAsync(viewToken.Token);
        var loadedEditToken = await repository.GetShareTokenByTokenAsync(editToken.Token);
        var listedTokens = await repository.GetShareTokensByTripPlanIdAsync(tripPlanId);
        var now = new DateTime(2026, 5, 17, 11, 0, 0);

        Assert.NotNull(loadedViewToken);
        Assert.NotNull(loadedEditToken);
        Assert.True(ShareTokenAccessPolicy.AllowsView(loadedViewToken, now));
        Assert.False(ShareTokenAccessPolicy.AllowsEdit(loadedViewToken, now));
        Assert.True(ShareTokenAccessPolicy.AllowsView(loadedEditToken, now));
        Assert.True(ShareTokenAccessPolicy.AllowsEdit(loadedEditToken, now));
        Assert.Equal(ShareAccessLevel.View, ShareTokenAccessPolicy.ParseAccessLevel(loadedViewToken.AccessLevel));
        Assert.Equal("EDIT", ShareTokenAccessPolicy.ToStoredAccessLevel(ShareAccessLevel.Edit));
        Assert.Collection(
            listedTokens,
            first => Assert.Equal(editToken.Id, first.Id),
            second => Assert.Equal(viewToken.Id, second.Id));
        Assert.True(await repository.UserOwnsTripPlanAsync(tripPlanId, ownerUserId));
        Assert.False(await repository.UserOwnsTripPlanAsync(tripPlanId, Guid.NewGuid()));

        Assert.True(await repository.RevokeShareTokenAsync(tripPlanId, viewToken.Id));
        Assert.True(await repository.RevokeShareTokenAsync(tripPlanId, viewToken.Id));

        var revokedToken = await repository.GetShareTokenByTokenAsync(viewToken.Token);

        Assert.NotNull(revokedToken);
        Assert.False(ShareTokenAccessPolicy.AllowsView(revokedToken, now));
        Assert.False(ShareTokenAccessPolicy.AllowsEdit(revokedToken, now));
    }

    [Fact]
    public void ShareTokenAccessPolicy_DeniesExpiredRevokedAndInvalidTokens()
    {
        var tripPlanId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTime(2026, 5, 17, 12, 0, 0);
        var expiredEditToken = CreateShareToken(tripPlanId, userId, "expired", "EDIT", now.AddHours(-2));
        expiredEditToken.ExpiresAt = now.AddMinutes(-1);
        var revokedEditToken = CreateShareToken(tripPlanId, userId, "revoked", "EDIT", now.AddHours(-2));
        revokedEditToken.IsRevoked = true;
        var invalidToken = CreateShareToken(tripPlanId, userId, "invalid", "ADMIN", now.AddHours(-2));

        Assert.False(ShareTokenAccessPolicy.AllowsView(expiredEditToken, now));
        Assert.False(ShareTokenAccessPolicy.AllowsEdit(expiredEditToken, now));
        Assert.False(ShareTokenAccessPolicy.AllowsView(revokedEditToken, now));
        Assert.False(ShareTokenAccessPolicy.AllowsEdit(revokedEditToken, now));
        Assert.False(ShareTokenAccessPolicy.AllowsView(invalidToken, now));
        Assert.False(ShareTokenAccessPolicy.AllowsEdit(invalidToken, now));
    }

    [Fact]
    public async Task SharedReadMethods_ReturnOnlyRequestedTripPlanGraph()
    {
        var tripPlanId = Guid.NewGuid();
        var otherTripPlanId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var factory = TestTravelPlannerDbContextFactory.Create();
        await SeedTripPlanAsync(factory, tripPlanId, ownerUserId, plannedBudget: 1_000m);
        await SeedTripPlanAsync(factory, otherTripPlanId, ownerUserId, plannedBudget: 500m);

        await using (var context = await factory.CreateDbContextAsync())
        {
            context.Destinations.AddRange(
                new DestinationEntity
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = tripPlanId,
                    Name = "Sarajevo",
                    ArrivalDate = new DateTime(2026, 6, 1),
                    DepartureDate = new DateTime(2026, 6, 3),
                    CreatedAt = new DateTime(2026, 5, 17, 8, 0, 0)
                },
                new DestinationEntity
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = otherTripPlanId,
                    Name = "Other destination",
                    ArrivalDate = new DateTime(2026, 6, 1),
                    DepartureDate = new DateTime(2026, 6, 2),
                    CreatedAt = new DateTime(2026, 5, 17, 8, 0, 0)
                });
            context.Activities.Add(new ActivityEntity
            {
                Id = Guid.NewGuid(),
                TripPlanId = tripPlanId,
                Title = "Museum",
                ActivityDate = new DateTime(2026, 6, 2),
                EstimatedCost = 25m,
                Status = "Planned",
                CreatedAt = new DateTime(2026, 5, 17, 8, 30, 0)
            });
            context.Expenses.AddRange(
                new ExpenseEntity
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = tripPlanId,
                    Title = "Hotel",
                    Category = "Accommodation",
                    Amount = 100m,
                    ExpenseDate = new DateTime(2026, 6, 2),
                    CreatedAt = new DateTime(2026, 5, 17, 9, 0, 0)
                },
                new ExpenseEntity
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = otherTripPlanId,
                    Title = "Other expense",
                    Category = "Other",
                    Amount = 999m,
                    ExpenseDate = new DateTime(2026, 6, 2),
                    CreatedAt = new DateTime(2026, 5, 17, 9, 0, 0)
                });
            context.ChecklistItems.Add(new ChecklistItemEntity
            {
                Id = Guid.NewGuid(),
                TripPlanId = tripPlanId,
                Title = "Passport",
                CreatedAt = new DateTime(2026, 5, 17, 9, 30, 0)
            });
            context.Notes.Add(new NoteEntity
            {
                Id = Guid.NewGuid(),
                TripPlanId = tripPlanId,
                Title = "Ideas",
                CreatedAt = new DateTime(2026, 5, 17, 10, 0, 0)
            });
            context.Reminders.Add(new ReminderEntity
            {
                Id = Guid.NewGuid(),
                TripPlanId = tripPlanId,
                Title = "Check in",
                ReminderAt = new DateTime(2026, 6, 1, 12, 0, 0),
                CreatedAt = new DateTime(2026, 5, 17, 10, 30, 0)
            });
            await context.SaveChangesAsync();
        }

        var repository = new SharingRepository(factory);

        Assert.Single(await repository.GetDestinationsByTripPlanIdAsync(tripPlanId));
        Assert.Single(await repository.GetActivitiesByTripPlanIdAsync(tripPlanId));
        Assert.Single(await repository.GetExpensesByTripPlanIdAsync(tripPlanId));
        Assert.Single(await repository.GetChecklistItemsByTripPlanIdAsync(tripPlanId));
        Assert.Single(await repository.GetNotesByTripPlanIdAsync(tripPlanId));
        Assert.Single(await repository.GetRemindersForTripPlanAsync(tripPlanId));
        Assert.Equal(100m, await repository.GetTotalExpensesByTripPlanIdAsync(tripPlanId));
    }

    [Fact]
    public async Task SharedEditMethods_CreateUpdateAndDeleteRelatedEntitiesThroughEf()
    {
        var tripPlanId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var factory = TestTravelPlannerDbContextFactory.Create();
        await SeedTripPlanAsync(factory, tripPlanId, ownerUserId);
        var repository = new SharingRepository(factory);
        var updatedAt = new DateTime(2026, 5, 17, 12, 0, 0);

        var destination = await repository.CreateDestinationAsync(new DestinationModel
        {
            Id = Guid.NewGuid(),
            TripPlanId = tripPlanId,
            Name = "Old destination",
            ArrivalDate = new DateTime(2026, 6, 1),
            DepartureDate = new DateTime(2026, 6, 2),
            CreatedAt = new DateTime(2026, 5, 17, 8, 0, 0)
        });
        destination.Name = "New destination";
        destination.UpdatedAt = updatedAt;
        Assert.True(await repository.UpdateDestinationAsync(destination));
        Assert.Equal("New destination", (await repository.GetDestinationByIdForTripPlanAsync(tripPlanId, destination.Id))?.Name);
        Assert.False(await repository.DeleteDestinationAsync(Guid.NewGuid(), destination.Id));
        Assert.True(await repository.DeleteDestinationAsync(tripPlanId, destination.Id));

        var activity = await repository.CreateActivityAsync(new ActivityModel
        {
            Id = Guid.NewGuid(),
            TripPlanId = tripPlanId,
            Title = "Old activity",
            ActivityDate = new DateTime(2026, 6, 1),
            EstimatedCost = 10m,
            Status = "Planned",
            CreatedAt = new DateTime(2026, 5, 17, 8, 30, 0)
        });
        activity.Title = "New activity";
        activity.UpdatedAt = updatedAt;
        Assert.True(await repository.UpdateActivityAsync(activity));
        Assert.Equal("New activity", (await repository.GetActivityByIdForTripPlanAsync(tripPlanId, activity.Id))?.Title);
        Assert.True(await repository.DeleteActivityAsync(tripPlanId, activity.Id));

        var expense = await repository.CreateExpenseAsync(new ExpenseModel
        {
            Id = Guid.NewGuid(),
            TripPlanId = tripPlanId,
            Title = "Old expense",
            Category = "Other",
            Amount = 10m,
            ExpenseDate = new DateTime(2026, 6, 1),
            CreatedAt = new DateTime(2026, 5, 17, 9, 0, 0)
        });
        expense.Title = "New expense";
        expense.Amount = 12m;
        expense.UpdatedAt = updatedAt;
        Assert.True(await repository.UpdateExpenseAsync(expense));
        Assert.Equal(12m, (await repository.GetExpenseByIdForTripPlanAsync(tripPlanId, expense.Id))?.Amount);
        Assert.True(await repository.DeleteExpenseAsync(tripPlanId, expense.Id));

        var checklistItem = await repository.CreateChecklistItemAsync(new ChecklistItemModel
        {
            Id = Guid.NewGuid(),
            TripPlanId = tripPlanId,
            Title = "Old item",
            CreatedAt = new DateTime(2026, 5, 17, 9, 30, 0)
        });
        checklistItem.Title = "New item";
        checklistItem.IsCompleted = true;
        checklistItem.UpdatedAt = updatedAt;
        Assert.True(await repository.UpdateChecklistItemAsync(checklistItem));
        Assert.True((await repository.GetChecklistItemByIdForTripPlanAsync(tripPlanId, checklistItem.Id))?.IsCompleted);
        Assert.True(await repository.DeleteChecklistItemAsync(tripPlanId, checklistItem.Id));

        var note = await repository.CreateNoteAsync(new NoteModel
        {
            Id = Guid.NewGuid(),
            TripPlanId = tripPlanId,
            Title = "Old note",
            CreatedAt = new DateTime(2026, 5, 17, 10, 0, 0)
        });
        note.Title = "New note";
        note.UpdatedAt = updatedAt;
        Assert.True(await repository.UpdateNoteAsync(note));
        Assert.Equal("New note", (await repository.GetNoteByIdForTripPlanAsync(tripPlanId, note.Id))?.Title);
        Assert.True(await repository.DeleteNoteAsync(tripPlanId, note.Id));

        var reminder = await repository.CreateReminderAsync(new ReminderModel
        {
            Id = Guid.NewGuid(),
            TripPlanId = tripPlanId,
            Title = "Old reminder",
            ReminderAt = new DateTime(2026, 6, 1, 12, 0, 0),
            CreatedAt = new DateTime(2026, 5, 17, 10, 30, 0)
        });
        reminder.Title = "New reminder";
        reminder.IsCompleted = true;
        reminder.UpdatedAt = updatedAt;
        Assert.True(await repository.UpdateReminderAsync(reminder));
        Assert.True((await repository.GetReminderByIdForTripPlanAsync(tripPlanId, reminder.Id))?.IsCompleted);
        Assert.True(await repository.DeleteReminderAsync(tripPlanId, reminder.Id));
    }

    private static ShareTokenModel CreateShareToken(
        Guid tripPlanId,
        Guid userId,
        string token,
        string accessLevel,
        DateTime createdAt)
    {
        return new ShareTokenModel
        {
            Id = Guid.NewGuid(),
            TripPlanId = tripPlanId,
            Token = token,
            AccessLevel = accessLevel,
            CreatedByUserId = userId,
            CreatedAt = createdAt
        };
    }

    private static async Task SeedTripPlanAsync(
        TestTravelPlannerDbContextFactory factory,
        Guid tripPlanId,
        Guid ownerUserId,
        decimal plannedBudget = 1_000m)
    {
        await using var context = await factory.CreateDbContextAsync();
        context.TripPlans.Add(new TripPlanEntity
        {
            Id = tripPlanId,
            OwnerUserId = ownerUserId,
            Title = "Shared trip",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 10),
            PlannedBudget = plannedBudget,
            CreatedAt = new DateTime(2026, 5, 17, 8, 0, 0)
        });
        await context.SaveChangesAsync();
    }
}
