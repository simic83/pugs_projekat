using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Users;

[DataContract]
public sealed class UserDto
{
    [DataMember(Order = 1)]
    public Guid Id { get; set; }

    [DataMember(Order = 2)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string Email { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public List<RoleDto> Roles { get; set; } = new();

    [DataMember(Order = 5)]
    public DateTime CreatedAtUtc { get; set; }
}
