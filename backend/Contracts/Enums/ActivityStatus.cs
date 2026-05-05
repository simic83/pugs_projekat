using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Enums;

[DataContract]
public enum ActivityStatus
{
    [EnumMember]
    Planned = 0,

    [EnumMember]
    Reserved = 1,

    [EnumMember]
    Completed = 2,

    [EnumMember]
    Cancelled = 3
}
