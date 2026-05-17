namespace TravelPlanner.Persistence.Entities;

public sealed class ShareTokenEntity
{
    public Guid Id { get; set; }

    public Guid TripPlanId { get; set; }

    public string Token { get; set; } = string.Empty;

    public string AccessLevel { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public TripPlanEntity? TripPlan { get; set; }
}
