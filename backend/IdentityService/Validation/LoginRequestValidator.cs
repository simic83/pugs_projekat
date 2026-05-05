using System.ComponentModel.DataAnnotations;
using TravelPlanner.Contracts.Auth;
using TravelPlanner.Contracts.Common;

namespace IdentityService.Validation;

internal static class LoginRequestValidator
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public static List<ValidationErrorDto> Validate(LoginRequestDto request)
    {
        var errors = new List<ValidationErrorDto>();

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

        return errors;
    }
}
