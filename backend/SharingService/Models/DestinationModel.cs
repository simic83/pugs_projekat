namespace SharingService.Models;

internal sealed class DestinationModel
{
    public Guid Id { get; set; }

    public Guid TripPlanId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public DateTime ArrivalDate { get; set; }

    public DateTime DepartureDate { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
