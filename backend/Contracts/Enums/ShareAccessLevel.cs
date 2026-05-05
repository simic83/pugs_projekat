using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Enums;

[DataContract]
public enum ShareAccessLevel
{
    [EnumMember]
    View = 0,

    [EnumMember]
    Edit = 1
}
