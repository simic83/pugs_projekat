namespace TravelPlanner.Contracts.DTOs.Trips;

public sealed class CreateDestinationRequestDto
{
    public Guid TripPlanId { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public DateTime ArrivalDate { get; set; }
    public DateTime DepartureDate { get; set; }
    public string? Note { get; set; }
}
