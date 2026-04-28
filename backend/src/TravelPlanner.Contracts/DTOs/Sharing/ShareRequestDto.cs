namespace TravelPlanner.Contracts.DTOs.Sharing;

public sealed class ShareRequestDto
{
    public Guid TripPlanId { get; set; }
    public Guid OwnerUserId { get; set; }
    public ShareAccessLevel AccessLevel { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

