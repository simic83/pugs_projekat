namespace TravelPlanner.Persistence.Entities;

public sealed class UserEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();

    public ICollection<TripPlanEntity> TripPlans { get; set; } = new List<TripPlanEntity>();
}
