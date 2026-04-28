namespace TravelPlanner.Contracts.DTOs.Trips;

public sealed class TripPlanDto
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PlannedBudget { get; set; }
    public string? Notes { get; set; }
}
