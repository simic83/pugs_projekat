namespace IdentityService.Models;

internal sealed class UserRecord
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public List<RoleRecord> Roles { get; set; } = new();
}
