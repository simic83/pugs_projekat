namespace TripPlanningService.Models;

internal sealed class ActivityModel
{
    public Guid Id { get; set; }

    public Guid TripPlanId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime ActivityDate { get; set; }

    public TimeSpan? ActivityTime { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public decimal EstimatedCost { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
