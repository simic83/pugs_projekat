using System.Runtime.Serialization;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Users;

namespace TravelPlanner.Contracts.Auth;

[DataContract]
public sealed class AuthResponseDto
{
    [DataMember(Order = 1)]
    public OperationResultDto Result { get; set; } = new();

    [DataMember(Order = 2)]
    public string? AccessToken { get; set; }

    [DataMember(Order = 3)]
    public DateTime? ExpiresAtUtc { get; set; }

    [DataMember(Order = 4)]
    public UserDto? User { get; set; }
}
