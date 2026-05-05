using Microsoft.ServiceFabric.Services.Remoting;
using TravelPlanner.Contracts.Auth;
using TravelPlanner.Contracts.Users;

namespace TravelPlanner.Contracts.Interfaces;

public interface IIdentityService : IService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    Task<UserDto?> GetUserByIdAsync(Guid userId);

    Task<List<UserDto>> GetUsersAsync();
}
