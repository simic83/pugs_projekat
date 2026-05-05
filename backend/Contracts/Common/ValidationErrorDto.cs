using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Common;

[DataContract]
public sealed class ValidationErrorDto
{
    [DataMember(Order = 1)]
    public string Field { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string Message { get; set; } = string.Empty;
}
