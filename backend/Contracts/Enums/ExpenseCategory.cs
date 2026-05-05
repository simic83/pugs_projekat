using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Enums;

[DataContract]
public enum ExpenseCategory
{
    [EnumMember]
    Transport = 0,

    [EnumMember]
    Accommodation = 1,

    [EnumMember]
    Food = 2,

    [EnumMember]
    Tickets = 3,

    [EnumMember]
    Shopping = 4,

    [EnumMember]
    Other = 5
}
