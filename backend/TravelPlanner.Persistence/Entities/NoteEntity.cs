namespace TravelPlanner.Persistence.Entities;

public sealed class NoteEntity
{
    public Guid Id { get; set; }

    public Guid TripPlanId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public TripPlanEntity? TripPlan { get; set; }
}
