using System.Net.Mail;
using TravelPlanner.Contracts.DTOs.Auth;

namespace TravelPlanner.IdentityService.Auth;

internal static class AuthRequestValidator
{
    private const int MinimumPasswordLength = 8;
    private const int MaximumNameLength = 200;
    private const int MaximumEmailLength = 256;

    public static AuthValidationResult<ValidatedRegisterRequest> ValidateRegister(RegisterRequestDto? request)
    {
        var name = request?.Name?.Trim() ?? string.Empty;
        var email = NormalizeEmail(request?.Email);
        var password = request?.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            return AuthValidationResult<ValidatedRegisterRequest>.Fail("Name is required.");
        }

        if (name.Length > MaximumNameLength)
        {
            return AuthValidationResult<ValidatedRegisterRequest>.Fail($"Name must be at most {MaximumNameLength} characters.");
        }

        var emailError = ValidateEmail(email);
        if (emailError is not null)
        {
            return AuthValidationResult<ValidatedRegisterRequest>.Fail(emailError);
        }

        var passwordError = ValidatePassword(password);
        if (passwordError is not null)
        {
            return AuthValidationResult<ValidatedRegisterRequest>.Fail(passwordError);
        }

        return AuthValidationResult<ValidatedRegisterRequest>.Ok(
            new ValidatedRegisterRequest(name, email, password));
    }

    public static AuthValidationResult<ValidatedLoginRequest> ValidateLogin(LoginRequestDto? request)
    {
        var email = NormalizeEmail(request?.Email);
        var password = request?.Password ?? string.Empty;

        var emailError = ValidateEmail(email);
        if (emailError is not null)
        {
            return AuthValidationResult<ValidatedLoginRequest>.Fail(emailError);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AuthValidationResult<ValidatedLoginRequest>.Fail("Password is required.");
        }

        return AuthValidationResult<ValidatedLoginRequest>.Ok(
            new ValidatedLoginRequest(email, password));
    }

    private static string NormalizeEmail(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static string? ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required.";
        }

        if (email.Length > MaximumEmailLength)
        {
            return $"Email must be at most {MaximumEmailLength} characters.";
        }

        try
        {
            var parsed = new MailAddress(email);
            return string.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase)
                ? null
                : "Email format is invalid.";
        }
        catch (FormatException)
        {
            return "Email format is invalid.";
        }
    }

    private static string? ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password is required.";
        }

        return password.Length < MinimumPasswordLength
            ? $"Password must be at least {MinimumPasswordLength} characters."
            : null;
    }
}

internal sealed record ValidatedRegisterRequest(string Name, string Email, string Password);

internal sealed record ValidatedLoginRequest(string Email, string Password);

internal sealed class AuthValidationResult<T>
{
    public bool IsValid { get; private init; }
    public string? ErrorMessage { get; private init; }
    public T? Value { get; private init; }

    public static AuthValidationResult<T> Ok(T value)
    {
        return new AuthValidationResult<T>
        {
            IsValid = true,
            Value = value,
        };
    }

    public static AuthValidationResult<T> Fail(string errorMessage)
    {
        return new AuthValidationResult<T>
        {
            IsValid = false,
            ErrorMessage = errorMessage,
        };
    }
}
