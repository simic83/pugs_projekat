using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Auth;

[DataContract]
public sealed class RegisterRequestDto
{
    [DataMember(Order = 1)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string Email { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string Password { get; set; } = string.Empty;
}
