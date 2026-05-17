using Microsoft.EntityFrameworkCore;
using TravelPlanner.Persistence.Entities;

namespace TravelPlanner.Persistence;

public sealed class TravelPlannerDbContext : DbContext
{
    public TravelPlannerDbContext(DbContextOptions<TravelPlannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<RoleEntity> Roles => Set<RoleEntity>();

    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();

    public DbSet<TripPlanEntity> TripPlans => Set<TripPlanEntity>();

    public DbSet<DestinationEntity> Destinations => Set<DestinationEntity>();

    public DbSet<ActivityEntity> Activities => Set<ActivityEntity>();

    public DbSet<ChecklistItemEntity> ChecklistItems => Set<ChecklistItemEntity>();

    public DbSet<NoteEntity> Notes => Set<NoteEntity>();

    public DbSet<ReminderEntity> Reminders => Set<ReminderEntity>();

    public DbSet<ExpenseEntity> Expenses => Set<ExpenseEntity>();

    public DbSet<ShareTokenEntity> ShareTokens => Set<ShareTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureIdentity(modelBuilder);
        ConfigureTripPlanning(modelBuilder);
        ConfigureBudget(modelBuilder);
        ConfigureSharing(modelBuilder);
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id).HasName("PK_Users");
            entity.Property(user => user.Id).HasColumnName("UserId").ValueGeneratedNever();
            entity.Property(user => user.Name).HasMaxLength(200).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(user => user.CreatedAtUtc).HasColumnType("datetime2(7)");
            entity.HasIndex(user => user.Email).IsUnique().HasDatabaseName("UQ_Users_Email");
        });

        modelBuilder.Entity<RoleEntity>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(role => role.Id).HasName("PK_Roles");
            entity.Property(role => role.Id).HasColumnName("RoleId");
            entity.Property(role => role.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(role => role.Name).IsUnique().HasDatabaseName("UQ_Roles_Name");
        });

        modelBuilder.Entity<UserRoleEntity>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId }).HasName("PK_UserRoles");

            entity.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserRoles_Users");

            entity.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserRoles_Roles");
        });
    }

    private static void ConfigureTripPlanning(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TripPlanEntity>(entity =>
        {
            entity.ToTable("TripPlans", table =>
            {
                table.HasCheckConstraint(
                    "CK_TripPlans_DatesRequired",
                    "StartDate > CONVERT(date, '00010101', 112) AND EndDate > CONVERT(date, '00010101', 112)");
                table.HasCheckConstraint("CK_TripPlans_DateRange", "EndDate >= StartDate");
                table.HasCheckConstraint("CK_TripPlans_PlannedBudget", "PlannedBudget >= 0");
            });

            entity.HasKey(tripPlan => tripPlan.Id).HasName("PK_TripPlans");
            entity.Property(tripPlan => tripPlan.Id).ValueGeneratedNever();
            entity.Property(tripPlan => tripPlan.Title).HasMaxLength(150).IsRequired();
            entity.Property(tripPlan => tripPlan.Description).HasColumnType("nvarchar(max)");
            entity.Property(tripPlan => tripPlan.StartDate).HasColumnType("date");
            entity.Property(tripPlan => tripPlan.EndDate).HasColumnType("date");
            entity.Property(tripPlan => tripPlan.PlannedBudget).HasColumnType("decimal(18,2)");
            entity.Property(tripPlan => tripPlan.Notes).HasColumnType("nvarchar(max)");
            entity.Property(tripPlan => tripPlan.CreatedAt).HasColumnType("datetime2");
            entity.Property(tripPlan => tripPlan.UpdatedAt).HasColumnType("datetime2");
            entity.HasIndex(tripPlan => tripPlan.OwnerUserId).HasDatabaseName("IX_TripPlans_OwnerUserId");

            entity.HasOne(tripPlan => tripPlan.Owner)
                .WithMany(user => user.TripPlans)
                .HasForeignKey(tripPlan => tripPlan.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TripPlans_Users");
        });

        modelBuilder.Entity<DestinationEntity>(entity =>
        {
            entity.ToTable("Destinations", table =>
            {
                table.HasCheckConstraint(
                    "CK_Destinations_DatesRequired",
                    "ArrivalDate > CONVERT(date, '00010101', 112) AND DepartureDate > CONVERT(date, '00010101', 112)");
                table.HasCheckConstraint("CK_Destinations_DateRange", "DepartureDate >= ArrivalDate");
            });

            entity.HasKey(destination => destination.Id).HasName("PK_Destinations");
            entity.Property(destination => destination.Id).ValueGeneratedNever();
            entity.Property(destination => destination.Name).HasMaxLength(150).IsRequired();
            entity.Property(destination => destination.Location).HasMaxLength(200);
            entity.Property(destination => destination.ArrivalDate).HasColumnType("date");
            entity.Property(destination => destination.DepartureDate).HasColumnType("date");
            entity.Property(destination => destination.Description).HasColumnType("nvarchar(max)");
            entity.Property(destination => destination.CreatedAt).HasColumnType("datetime2");
            entity.Property(destination => destination.UpdatedAt).HasColumnType("datetime2");

            entity.HasOne(destination => destination.TripPlan)
                .WithMany(tripPlan => tripPlan.Destinations)
                .HasForeignKey(destination => destination.TripPlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Destinations_TripPlans");
        });

        modelBuilder.Entity<ActivityEntity>(entity =>
        {
            entity.ToTable("Activities", table =>
            {
                table.HasCheckConstraint(
                    "CK_Activities_DateRequired",
                    "ActivityDate > CONVERT(date, '00010101', 112)");
                table.HasCheckConstraint("CK_Activities_EstimatedCost", "EstimatedCost >= 0");
            });

            entity.HasKey(activity => activity.Id).HasName("PK_Activities");
            entity.Property(activity => activity.Id).ValueGeneratedNever();
            entity.Property(activity => activity.Title).HasMaxLength(150).IsRequired();
            entity.Property(activity => activity.ActivityDate).HasColumnType("date");
            entity.Property(activity => activity.ActivityTime).HasColumnType("time");
            entity.Property(activity => activity.Location).HasMaxLength(200);
            entity.Property(activity => activity.Description).HasColumnType("nvarchar(max)");
            entity.Property(activity => activity.EstimatedCost).HasColumnType("decimal(18,2)");
            entity.Property(activity => activity.Status).HasMaxLength(30).IsRequired();
            entity.Property(activity => activity.CreatedAt).HasColumnType("datetime2");
            entity.Property(activity => activity.UpdatedAt).HasColumnType("datetime2");

            entity.HasOne(activity => activity.TripPlan)
                .WithMany(tripPlan => tripPlan.Activities)
                .HasForeignKey(activity => activity.TripPlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Activities_TripPlans");
        });

        modelBuilder.Entity<ChecklistItemEntity>(entity =>
        {
            entity.ToTable("ChecklistItems");
            entity.HasKey(checklistItem => checklistItem.Id).HasName("PK_ChecklistItems");
            entity.Property(checklistItem => checklistItem.Id).ValueGeneratedNever();
            entity.Property(checklistItem => checklistItem.Title).HasMaxLength(150).IsRequired();
            entity.Property(checklistItem => checklistItem.CreatedAt).HasColumnType("datetime2");
            entity.Property(checklistItem => checklistItem.UpdatedAt).HasColumnType("datetime2");

            entity.HasOne(checklistItem => checklistItem.TripPlan)
                .WithMany(tripPlan => tripPlan.ChecklistItems)
                .HasForeignKey(checklistItem => checklistItem.TripPlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ChecklistItems_TripPlans");
        });

        modelBuilder.Entity<NoteEntity>(entity =>
        {
            entity.ToTable("Notes");
            entity.HasKey(note => note.Id).HasName("PK_Notes");
            entity.Property(note => note.Id).ValueGeneratedNever();
            entity.Property(note => note.Title).HasMaxLength(150).IsRequired();
            entity.Property(note => note.Content).HasColumnType("nvarchar(max)");
            entity.Property(note => note.CreatedAt).HasColumnType("datetime2");
            entity.Property(note => note.UpdatedAt).HasColumnType("datetime2");

            entity.HasOne(note => note.TripPlan)
                .WithMany(tripPlan => tripPlan.NotesList)
                .HasForeignKey(note => note.TripPlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Notes_TripPlans");
        });

        modelBuilder.Entity<ReminderEntity>(entity =>
        {
            entity.ToTable("Reminders");
            entity.HasKey(reminder => reminder.Id).HasName("PK_Reminders");
            entity.Property(reminder => reminder.Id).ValueGeneratedNever();
            entity.Property(reminder => reminder.Title).HasMaxLength(150).IsRequired();
            entity.Property(reminder => reminder.Description).HasColumnType("nvarchar(max)");
            entity.Property(reminder => reminder.ReminderAt).HasColumnType("datetime2");
            entity.Property(reminder => reminder.CreatedAt).HasColumnType("datetime2");
            entity.Property(reminder => reminder.UpdatedAt).HasColumnType("datetime2");

            entity.HasOne(reminder => reminder.TripPlan)
                .WithMany(tripPlan => tripPlan.Reminders)
                .HasForeignKey(reminder => reminder.TripPlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Reminders_TripPlans");
        });
    }

    private static void ConfigureBudget(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpenseEntity>(entity =>
        {
            entity.ToTable(
                "Expenses",
                table => table.HasCheckConstraint("CK_Expenses_Amount", "Amount >= 0"));

            entity.HasKey(expense => expense.Id).HasName("PK_Expenses");
            entity.Property(expense => expense.Id).ValueGeneratedNever();
            entity.Property(expense => expense.Title).HasMaxLength(150).IsRequired();
            entity.Property(expense => expense.Category).HasMaxLength(50).IsRequired();
            entity.Property(expense => expense.Amount).HasColumnType("decimal(18,2)");
            entity.Property(expense => expense.ExpenseDate).HasColumnType("date");
            entity.Property(expense => expense.Description).HasColumnType("nvarchar(max)");
            entity.Property(expense => expense.CreatedAt).HasColumnType("datetime2");
            entity.Property(expense => expense.UpdatedAt).HasColumnType("datetime2");

            entity.HasOne(expense => expense.TripPlan)
                .WithMany(tripPlan => tripPlan.Expenses)
                .HasForeignKey(expense => expense.TripPlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Expenses_TripPlans");
        });
    }

    private static void ConfigureSharing(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShareTokenEntity>(entity =>
        {
            entity.ToTable(
                "ShareTokens",
                table => table.HasCheckConstraint(
                    "CK_ShareTokens_AccessLevel",
                    "AccessLevel IN (N'VIEW', N'EDIT')"));

            entity.HasKey(shareToken => shareToken.Id).HasName("PK_ShareTokens");
            entity.Property(shareToken => shareToken.Id).ValueGeneratedNever();
            entity.Property(shareToken => shareToken.Token).HasMaxLength(200).IsRequired();
            entity.Property(shareToken => shareToken.AccessLevel).HasMaxLength(20).IsRequired();
            entity.Property(shareToken => shareToken.CreatedAt).HasColumnType("datetime2");
            entity.Property(shareToken => shareToken.ExpiresAt).HasColumnType("datetime2");
            entity.HasIndex(shareToken => shareToken.Token).IsUnique().HasDatabaseName("UQ_ShareTokens_Token");

            entity.HasOne(shareToken => shareToken.TripPlan)
                .WithMany(tripPlan => tripPlan.ShareTokens)
                .HasForeignKey(shareToken => shareToken.TripPlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ShareTokens_TripPlans");
        });
    }
}
