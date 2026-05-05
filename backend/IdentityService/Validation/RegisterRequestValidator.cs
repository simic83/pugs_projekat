using System.ComponentModel.DataAnnotations;
using TravelPlanner.Contracts.Auth;
using TravelPlanner.Contracts.Common;

namespace IdentityService.Validation;

internal static class RegisterRequestValidator
{
    private const int MinimumPasswordLength = 8;
    private static readonly EmailAddressAttribute EmailValidator = new();

    public static List<ValidationErrorDto> Validate(RegisterRequestDto request)
    {
        var errors = new List<ValidationErrorDto>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add(new ValidationErrorDto { Field = nameof(request.Name), Message = "Name is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add(new ValidationErrorDto { Field = nameof(request.Email), Message = "Email is required." });
        }
        else if (!EmailValidator.IsValid(request.Email))
        {
            errors.Add(new ValidationErrorDto { Field = nameof(request.Email), Message = "Email must be valid." });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add(new ValidationErrorDto { Field = nameof(request.Password), Message = "Password is required." });
        }
        else if (request.Password.Length < MinimumPasswordLength)
        {
            errors.Add(new ValidationErrorDto
            {
                Field = nameof(request.Password),
                Message = $"Password must be at least {MinimumPasswordLength} characters."
            });
        }

        return errors;
    }

    public static List<ValidationErrorDto> ValidateDuplicateEmail(bool emailExists)
    {
        return emailExists
            ? new List<ValidationErrorDto>
            {
                new() { Field = nameof(RegisterRequestDto.Email), Message = "Email is already registered." }
            }
            : new List<ValidationErrorDto>();
    }
}
