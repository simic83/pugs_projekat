namespace TravelPlanner.Common.Validation;

public sealed class ValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();
}

