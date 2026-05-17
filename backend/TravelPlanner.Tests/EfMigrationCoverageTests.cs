using BudgetService.Data;
using BudgetService.Models;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TravelPlanner.Persistence;
using TravelPlanner.Persistence.Entities;
using TripPlanningService.Data;
using Xunit;

namespace TravelPlanner.Tests;

public sealed class EfUserRepositoryTests
{
    [Fact]
    public async Task CreateUserAsync_AssignsDefaultRoleAndNormalizesEmailForLoginLookup()
    {
        var repository = await CreateUserRepositoryAsync();
        var userId = Guid.NewGuid();

        var created = await repository.CreateUserAsync(new UserRecord
        {
            Id = userId,
            Name = "Ada Lovelace",
            Email = " ADA@EXAMPLE.COM ",
            PasswordHash = "password-hash",
            CreatedAtUtc = DateTime.UtcNow
        });

        Assert.Equal("ada@example.com", created.Email);
        Assert.Contains(created.Roles, role => role.Name == "User");
        Assert.True(await repository.EmailExistsAsync("ada@example.com"));

        var found = await repository.FindByEmailAsync(" ADA@example.com ");

        Assert.NotNull(found);
        Assert.Equal(userId, found.Id);
        Assert.Contains(found.Roles, role => role.Name == "User");
    }

    [Fact]
    public async Task SetUserRoleAsync_ReplacesDefaultRoleWithAdminRole()
    {
        var repository = await CreateUserRepositoryAsync();
        var created = await repository.CreateUserAsync(new UserRecord
        {
            Id = Guid.NewGuid(),
            Name = "Grace Hopper",
            Email = "grace@example.com",
            PasswordHash = "password-hash",
            CreatedAtUtc = DateTime.UtcNow
        });

        var changed = await repository.SetUserRoleAsync(created.Id, "Admin");
        var updated = await repository.GetByIdAsync(created.Id);

        Assert.True(changed);
        Assert.NotNull(updated);
        Assert.Contains(updated.Roles, role => role.Name == "Admin");
        Assert.DoesNotContain(updated.Roles, role => role.Name == "User");
        Assert.Equal(1, await repository.CountUsersInRoleAsync("Admin"));
    }

    [Fact]
    public async Task DeleteUserAsync_RemovesUserAndUserRoles()
    {
        var repository = await CreateUserRepositoryAsync();
        var created = await repository.CreateUserAsync(new UserRecord
        {
            Id = Guid.NewGuid(),
            Name = "Katherine Johnson",
            Email = "katherine@example.com",
            PasswordHash = "password-hash",
            CreatedAtUtc = DateTime.UtcNow
        });
        await repository.SetUserRoleAsync(created.Id, "Admin");

        var deleted = await repository.DeleteUserAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(await repository.GetByIdAsync(created.Id));
        Assert.Equal(0, await repository.CountUsersInRoleAsync("Admin"));
    }

    private static async Task<EfUserRepository> CreateUserRepositoryAsync()
    {
        var factory = CreateFactory();
        await using var context = await factory.CreateDbContextAsync();

        context.Roles.AddRange(
            new RoleEntity { Id = 1, Name = "User" },
            new RoleEntity { Id = 2, Name = "Admin" });
        await context.SaveChangesAsync();

        return new EfUserRepository(factory);
    }

    private static TestTravelPlannerDbContextFactory CreateFactory()
    {
        return TestTravelPlannerDbContextFactory.Create();
    }
}

public sealed class TripPlanningRepositoryTests
{
    [Fact]
    public async Task GetTripPlansByOwnerAsync_ReturnsOnlyOwnedPlansInDateOrder()
    {
        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var factory = TestTravelPlannerDbContextFactory.Create();
        await using var context = await factory.CreateDbContextAsync();
        context.TripPlans.AddRange(
            new TripPlanEntity
            {
                Id = Guid.NewGuid(),
                OwnerUserId = ownerUserId,
                Title = "Late plan",
                StartDate = new DateTime(2026, 7, 10),
                EndDate = new DateTime(2026, 7, 12),
                CreatedAt = new DateTime(2026, 1, 2)
            },
            new TripPlanEntity
            {
                Id = Guid.NewGuid(),
                OwnerUserId = otherUserId,
                Title = "Other owner",
                StartDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2026, 6, 3),
                CreatedAt = new DateTime(2026, 1, 3)
            },
            new TripPlanEntity
            {
                Id = Guid.NewGuid(),
                OwnerUserId = ownerUserId,
                Title = "Early plan",
                StartDate = new DateTime(2026, 5, 20),
                EndDate = new DateTime(2026, 5, 22),
                CreatedAt = new DateTime(2026, 1, 1)
            });
        await context.SaveChangesAsync();

        var repository = new TripPlanningRepository(factory);
        var plans = await repository.GetTripPlansByOwnerAsync(ownerUserId);

        Assert.Collection(
            plans,
            first => Assert.Equal("Early plan", first.Title),
            second => Assert.Equal("Late plan", second.Title));
        Assert.All(plans, plan => Assert.Equal(ownerUserId, plan.OwnerUserId));
    }
}

public sealed class BudgetRepositoryTests
{
    [Fact]
    public async Task CreateUpdateDeleteExpenseAsync_PersistsExpenseThroughEfContext()
    {
        var tripPlanId = Guid.NewGuid();
        var factory = TestTravelPlannerDbContextFactory.Create();
        await SeedTripPlanAsync(factory, tripPlanId, Guid.NewGuid(), 500m);

        var repository = new BudgetRepository(factory);
        var expense = new ExpenseModel
        {
            Id = Guid.NewGuid(),
            TripPlanId = tripPlanId,
            Title = "Train tickets",
            Category = "Transport",
            Amount = 120.50m,
            ExpenseDate = new DateTime(2026, 6, 2),
            CreatedAt = new DateTime(2026, 5, 17, 10, 0, 0)
        };

        await repository.CreateExpenseAsync(expense);
        var created = await repository.GetExpenseByIdAsync(expense.Id);

        Assert.NotNull(created);
        Assert.Equal("Train tickets", created.Title);

        expense.Title = "Updated train tickets";
        expense.Amount = 130.75m;
        expense.UpdatedAt = new DateTime(2026, 5, 17, 11, 0, 0);

        Assert.True(await repository.UpdateExpenseAsync(expense));
        var updated = await repository.GetExpenseByIdAsync(expense.Id);

        Assert.NotNull(updated);
        Assert.Equal("Updated train tickets", updated.Title);
        Assert.Equal(130.75m, updated.Amount);

        Assert.True(await repository.DeleteExpenseAsync(expense.Id));
        Assert.Null(await repository.GetExpenseByIdAsync(expense.Id));
    }

    [Fact]
    public async Task GetExpensesByTripPlanIdAsync_ReturnsOnlyTripExpensesInBudgetOrderAndTotal()
    {
        var tripPlanId = Guid.NewGuid();
        var otherTripPlanId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var factory = TestTravelPlannerDbContextFactory.Create();
        await SeedTripPlanAsync(factory, tripPlanId, ownerUserId, 1_000m);
        await SeedTripPlanAsync(factory, otherTripPlanId, ownerUserId, 1_000m);

        await using (var context = await factory.CreateDbContextAsync())
        {
            context.Expenses.AddRange(
                new ExpenseEntity
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = tripPlanId,
                    Title = "Museum",
                    Category = "Activities",
                    Amount = 25m,
                    ExpenseDate = new DateTime(2026, 6, 4),
                    CreatedAt = new DateTime(2026, 5, 17, 9, 0, 0)
                },
                new ExpenseEntity
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = tripPlanId,
                    Title = "Hotel",
                    Category = "Accommodation",
                    Amount = 75m,
                    ExpenseDate = new DateTime(2026, 6, 5),
                    CreatedAt = new DateTime(2026, 5, 17, 8, 0, 0)
                },
                new ExpenseEntity
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = otherTripPlanId,
                    Title = "Other trip",
                    Category = "Other",
                    Amount = 999m,
                    ExpenseDate = new DateTime(2026, 6, 6),
                    CreatedAt = new DateTime(2026, 5, 17, 7, 0, 0)
                });
            await context.SaveChangesAsync();
        }

        var repository = new BudgetRepository(factory);
        var expenses = await repository.GetExpensesByTripPlanIdAsync(tripPlanId);
        var total = await repository.GetTotalByTripPlanIdAsync(tripPlanId);
        var emptyTotal = await repository.GetTotalByTripPlanIdAsync(Guid.NewGuid());

        Assert.Collection(
            expenses,
            first => Assert.Equal("Hotel", first.Title),
            second => Assert.Equal("Museum", second.Title));
        Assert.All(expenses, expense => Assert.Equal(tripPlanId, expense.TripPlanId));
        Assert.Equal(100m, total);
        Assert.Equal(0m, emptyTotal);
    }

    [Fact]
    public async Task GetPlannedBudgetForOwnerAsync_UsesTripPlansOwnership()
    {
        var tripPlanId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var factory = TestTravelPlannerDbContextFactory.Create();
        await SeedTripPlanAsync(factory, tripPlanId, ownerUserId, 1_200m);

        var repository = new BudgetRepository(factory);

        Assert.Equal(1_200m, await repository.GetPlannedBudgetForOwnerAsync(tripPlanId, ownerUserId));
        Assert.Null(await repository.GetPlannedBudgetForOwnerAsync(tripPlanId, otherUserId));
        Assert.Null(await repository.GetPlannedBudgetForOwnerAsync(Guid.NewGuid(), ownerUserId));
    }

    private static async Task SeedTripPlanAsync(
        IDbContextFactory<TravelPlannerDbContext> factory,
        Guid tripPlanId,
        Guid ownerUserId,
        decimal plannedBudget)
    {
        await using var context = await factory.CreateDbContextAsync();
        context.TripPlans.Add(new TripPlanEntity
        {
            Id = tripPlanId,
            OwnerUserId = ownerUserId,
            Title = "Budget trip",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 10),
            PlannedBudget = plannedBudget,
            CreatedAt = new DateTime(2026, 5, 17, 8, 0, 0)
        });
        await context.SaveChangesAsync();
    }
}

public sealed class TravelPlannerDbContextModelTests
{
    [Fact]
    public void TripPlanningRelationships_AreConfiguredForCascadeDelete()
    {
        using var context = TestTravelPlannerDbContextFactory.Create().CreateDbContext();

        AssertCascade<TripPlanEntity, UserEntity>(context);
        AssertCascade<DestinationEntity, TripPlanEntity>(context);
        AssertCascade<ActivityEntity, TripPlanEntity>(context);
        AssertCascade<ChecklistItemEntity, TripPlanEntity>(context);
        AssertCascade<NoteEntity, TripPlanEntity>(context);
        AssertCascade<ReminderEntity, TripPlanEntity>(context);
    }

    private static void AssertCascade<TDependent, TPrincipal>(TravelPlannerDbContext context)
    {
        var dependentType = context.Model.FindEntityType(typeof(TDependent));
        Assert.NotNull(dependentType);

        var foreignKey = dependentType.GetForeignKeys()
            .Single(key => key.PrincipalEntityType.ClrType == typeof(TPrincipal));

        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }
}

internal sealed class TestTravelPlannerDbContextFactory : IDbContextFactory<TravelPlannerDbContext>
{
    private readonly DbContextOptions<TravelPlannerDbContext> options;

    private TestTravelPlannerDbContextFactory(DbContextOptions<TravelPlannerDbContext> options)
    {
        this.options = options;
    }

    public static TestTravelPlannerDbContextFactory Create()
    {
        var options = new DbContextOptionsBuilder<TravelPlannerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new TestTravelPlannerDbContextFactory(options);
    }

    public TravelPlannerDbContext CreateDbContext()
    {
        return new TravelPlannerDbContext(options);
    }

    public ValueTask<TravelPlannerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(CreateDbContext());
    }
}
