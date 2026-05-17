namespace TravelPlanner.Persistence.Entities;

public sealed class TripPlanEntity
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal PlannedBudget { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public UserEntity? Owner { get; set; }

    public ICollection<DestinationEntity> Destinations { get; set; } = new List<DestinationEntity>();

    public ICollection<ActivityEntity> Activities { get; set; } = new List<ActivityEntity>();

    public ICollection<ChecklistItemEntity> ChecklistItems { get; set; } = new List<ChecklistItemEntity>();

    public ICollection<NoteEntity> NotesList { get; set; } = new List<NoteEntity>();

    public ICollection<ReminderEntity> Reminders { get; set; } = new List<ReminderEntity>();

    public ICollection<ExpenseEntity> Expenses { get; set; } = new List<ExpenseEntity>();

    public ICollection<ShareTokenEntity> ShareTokens { get; set; } = new List<ShareTokenEntity>();
}
