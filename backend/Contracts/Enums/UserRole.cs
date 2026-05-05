using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Enums;

[DataContract]
public enum UserRole
{
    [EnumMember]
    User = 0,

    [EnumMember]
    Admin = 1
}
