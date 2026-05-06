namespace SharingService.Models;

internal sealed class ChecklistItemModel
{
    public Guid Id { get; set; }

    public Guid TripPlanId { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
