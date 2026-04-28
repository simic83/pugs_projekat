using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Contracts.DTOs.Auth;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.IdentityService.Auth;
using TravelPlanner.IdentityService.Data;

namespace TravelPlanner.IdentityService;

internal sealed class IdentityService : StatelessService, IIdentityService
{
    private readonly AuthOptions _options;
    private readonly IdentityRepository _repository;
    private readonly JwtTokenService _jwtTokenService;
    private readonly PasswordHasher _passwordHasher = new();

    public IdentityService(StatelessServiceContext context)
        : base(context)
    {
        _options = AuthOptions.FromServiceContext(context);
        _repository = new IdentityRepository(_options.ConnectionString);
        _jwtTokenService = new JwtTokenService(_options);
    }

    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
    {
        return this.CreateServiceRemotingInstanceListeners();
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var validation = AuthRequestValidator.ValidateRegister(request);
        if (!validation.IsValid)
        {
            return Fail(AuthErrorCodes.ValidationError, validation.ErrorMessage!);
        }

        _options.EnsureUsableForDatabase();

        var validatedRequest = validation.Value!;
        var passwordHash = _passwordHasher.Hash(validatedRequest.Password);
        var createResult = await _repository.CreateUserAsync(
            Guid.NewGuid(),
            validatedRequest.Name,
            validatedRequest.Email,
            passwordHash);

        if (createResult.DuplicateEmail)
        {
            return Fail(AuthErrorCodes.DuplicateEmail, "Email is already registered.");
        }

        var user = createResult.User!;
        var token = _jwtTokenService.IssueToken(user);

        return Success(token, user.ToDto());
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var validation = AuthRequestValidator.ValidateLogin(request);
        if (!validation.IsValid)
        {
            return Fail(AuthErrorCodes.ValidationError, validation.ErrorMessage!);
        }

        _options.EnsureUsableForDatabase();

        var validatedRequest = validation.Value!;
        var user = await _repository.GetUserByEmailAsync(validatedRequest.Email);
        if (user is null || !_passwordHasher.Verify(validatedRequest.Password, user.PasswordHash))
        {
            return Fail(AuthErrorCodes.InvalidCredentials, "Email or password is incorrect.");
        }

        var token = _jwtTokenService.IssueToken(user);
        return Success(token, user.ToDto());
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        _options.EnsureUsableForDatabase();

        var user = await _repository.GetUserByIdAsync(userId);
        return user?.ToDto();
    }

    public async Task<TokenValidationResultDto> ValidateTokenAsync(string token)
    {
        var tokenValidation = _jwtTokenService.ValidateToken(token);
        if (!tokenValidation.IsValid)
        {
            return InvalidTokenResult(tokenValidation);
        }

        _options.EnsureUsableForDatabase();

        var user = await _repository.GetUserByIdAsync(tokenValidation.UserId!.Value);
        if (user is null)
        {
            return new TokenValidationResultDto
            {
                IsValid = false,
                IsAuthorized = false,
                ErrorCode = AuthErrorCodes.UserNotFound,
                ErrorMessage = "Token user no longer exists.",
            };
        }

        return new TokenValidationResultDto
        {
            IsValid = true,
            IsAuthorized = true,
            UserId = user.Id,
            Role = user.Role,
            User = user.ToDto(),
        };
    }

    public async Task<TokenValidationResultDto> ValidateTokenRoleAsync(string token, string requiredRole)
    {
        var validation = await ValidateTokenAsync(token);
        if (!validation.IsValid)
        {
            return validation;
        }

        if (!string.Equals(validation.Role, requiredRole, StringComparison.OrdinalIgnoreCase))
        {
            validation.IsAuthorized = false;
            validation.ErrorCode = AuthErrorCodes.Forbidden;
            validation.ErrorMessage = $"Role '{requiredRole}' is required.";
        }

        return validation;
    }

    private static AuthResponseDto Success(IssuedToken token, UserDto user)
    {
        return new AuthResponseDto
        {
            Succeeded = true,
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresAt,
            User = user,
        };
    }

    private static AuthResponseDto Fail(string errorCode, string errorMessage)
    {
        return new AuthResponseDto
        {
            Succeeded = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };
    }

    private static TokenValidationResultDto InvalidTokenResult(JwtTokenValidation validation)
    {
        return new TokenValidationResultDto
        {
            IsValid = false,
            IsExpired = validation.IsExpired,
            IsAuthorized = false,
            ErrorCode = validation.ErrorCode,
            ErrorMessage = validation.ErrorMessage,
        };
    }
}
