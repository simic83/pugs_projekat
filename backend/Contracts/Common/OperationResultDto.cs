using System.Runtime.Serialization;

namespace TravelPlanner.Contracts.Common;

[DataContract]
public sealed class OperationResultDto
{
    [DataMember(Order = 1)]
    public bool Succeeded { get; set; }

    [DataMember(Order = 2)]
    public string? Message { get; set; }

    [DataMember(Order = 3)]
    public List<ValidationErrorDto> Errors { get; set; } = new();
}
