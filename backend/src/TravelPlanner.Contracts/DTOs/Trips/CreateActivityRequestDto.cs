namespace TravelPlanner.Contracts.DTOs.Trips;

public sealed class CreateActivityRequestDto
{
    public Guid TripPlanId { get; set; }
    public Guid? DestinationId { get; set; }
    public string? Name { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? Time { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public decimal EstimatedCost { get; set; }
    public string? Status { get; set; }
}
