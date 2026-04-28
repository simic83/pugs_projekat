namespace TravelPlanner.Contracts.DTOs.Trips;

public sealed class CreateTripPlanRequestDto
{
    public Guid OwnerUserId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PlannedBudget { get; set; }
    public string? Notes { get; set; }
}
