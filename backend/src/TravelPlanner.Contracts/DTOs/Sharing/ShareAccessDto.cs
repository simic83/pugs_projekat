namespace TravelPlanner.Contracts.DTOs.Sharing;

public sealed class ShareAccessDto
{
    public Guid TripPlanId { get; set; }
    public string? Token { get; set; }
    public ShareAccessLevel AccessLevel { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

