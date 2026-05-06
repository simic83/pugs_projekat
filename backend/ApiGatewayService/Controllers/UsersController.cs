using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Auth;
using TravelPlanner.Contracts.Common;
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

    [HttpPut("{userId:guid}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OperationResultDto>> ChangeUserRole(Guid userId, ChangeUserRoleRequest request)
    {
        var identityService = GatewayServiceProxyFactory.CreateStateless<IIdentityService>(ServiceNames.IdentityServiceUri);
        var result = await identityService.ChangeUserRoleAsync(userId, request);

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OperationResultDto>> DeleteUser(Guid userId)
    {
        if (userId == ControllerUserContext.GetUserId(this))
        {
            return BadRequest(Failure("Admin cannot delete their own account."));
        }

        var identityService = GatewayServiceProxyFactory.CreateStateless<IIdentityService>(ServiceNames.IdentityServiceUri);
        var result = await identityService.DeleteUserAsync(userId);

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    private static OperationResultDto Failure(string message)
    {
        return new OperationResultDto
        {
            Succeeded = false,
            Message = message
        };
    }
}
