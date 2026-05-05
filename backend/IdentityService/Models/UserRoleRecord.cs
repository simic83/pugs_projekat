namespace IdentityService.Models;

internal sealed class UserRoleRecord
{
    public Guid UserId { get; set; }

    public int RoleId { get; set; }
}
