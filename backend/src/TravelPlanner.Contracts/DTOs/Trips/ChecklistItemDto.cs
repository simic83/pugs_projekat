namespace TravelPlanner.Contracts.DTOs.Trips;

public sealed class ChecklistItemDto
{
    public Guid Id { get; set; }
    public Guid TripPlanId { get; set; }
    public string? Title { get; set; }
    public bool IsCompleted { get; set; }
}

