using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Auth;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Users;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Create(RegisterRequestDto request)
    {
        var identityService = GatewayServiceProxyFactory.CreateStateless<IIdentityService>(ServiceNames.IdentityServiceUri);
        var response = await identityService.RegisterAsync(request);

        return response.Result.Succeeded ? Created("/api/users/me", response) : BadRequest(response);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var identityService = GatewayServiceProxyFactory.CreateStateless<IIdentityService>(ServiceNames.IdentityServiceUri);
        var user = await identityService.GetUserByIdAsync(ControllerUserContext.GetUserId(this));

        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var identityService = GatewayServiceProxyFactory.CreateStateless<IIdentityService>(ServiceNames.IdentityServiceUri);
        var users = await identityService.GetUsersAsync();

        return Ok(users);
    }
}
