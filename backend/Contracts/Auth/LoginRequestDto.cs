using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Auth;

[DataContract]
public sealed class LoginRequestDto
{
    [DataMember(Order = 1)]
    public string Email { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string Password { get; set; } = string.Empty;
}
