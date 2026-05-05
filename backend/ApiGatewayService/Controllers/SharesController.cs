using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Sharing;

namespace ApiGatewayService.Controllers;

[ApiController]
public sealed class SharesController : ControllerBase
{
    [HttpGet("api/trip-plans/{tripPlanId:guid}/shares")]
    [Authorize]
    public ActionResult<List<object>> GetShares(Guid tripPlanId)
    {
        return Ok(new List<object>());
    }

    [HttpPost("api/trip-plans/{tripPlanId:guid}/shares")]
    [Authorize]
    public async Task<ActionResult<ShareTokenDto>> CreateShare(Guid tripPlanId, CreateShareRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        request.TripPlanId = tripPlanId;
        request.CreatedByUserId = ControllerUserContext.GetUserId(this);
        var share = await sharingService.CreateShareAsync(request);

        return Created($"/api/shares/{share.Token}/trip-plan", share);
    }

    [HttpGet("api/shares/{token}/trip-plan")]
    [AllowAnonymous]
    public async Task<ActionResult<SharedTripPlanDto>> GetSharedTripPlan(string token)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var sharedTripPlan = await sharingService.GetSharedTripPlanAsync(token);

        return sharedTripPlan is null ? NotFound() : Ok(sharedTripPlan);
    }
}
