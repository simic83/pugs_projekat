using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Users;

[DataContract]
public sealed class ChangeUserRoleRequest
{
    [DataMember(Order = 1)]
    public string Role { get; set; } = string.Empty;
}
