using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Auth;
using TravelPlanner.Contracts.Interfaces;

namespace ApiGatewayService.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/sessions")]
public sealed class AuthController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AuthResponseDto>> Create(LoginRequestDto request)
    {
        var identityService = GatewayServiceProxyFactory.CreateStateless<IIdentityService>(ServiceNames.IdentityServiceUri);
        var response = await identityService.LoginAsync(request);

        return response.Result.Succeeded ? Ok(response) : BadRequest(response);
    }
}
