using Microsoft.ServiceFabric.Services.Remoting;
using TravelPlanner.Contracts.DTOs.Auth;

namespace TravelPlanner.Contracts.Interfaces;

public interface IIdentityService : IService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    Task<UserDto?> GetUserByIdAsync(Guid userId);

    Task<TokenValidationResultDto> ValidateTokenAsync(string token);

    Task<TokenValidationResultDto> ValidateTokenRoleAsync(string token, string requiredRole);
}
