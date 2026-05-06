using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Enums;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Sharing;
using TravelPlanner.Contracts.Trips;

namespace ApiGatewayService.Controllers;

[ApiController]
public sealed class SharesController : ControllerBase
{
    [HttpGet("api/trip-plans/{tripPlanId:guid}/shares")]
    [Authorize]
    public async Task<ActionResult<List<ShareTokenDto>>> GetShares(Guid tripPlanId)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var shares = await sharingService.GetSharesByTripPlanAsync(tripPlanId, ControllerUserContext.GetUserId(this));

        return Ok(shares);
    }

    [HttpPost("api/trip-plans/{tripPlanId:guid}/shares")]
    [Authorize]
    public async Task<ActionResult<ShareTokenDto>> CreateShare(Guid tripPlanId, CreateShareRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        request.TripPlanId = tripPlanId;
        request.CreatedByUserId = ControllerUserContext.GetUserId(this);
        var share = await sharingService.CreateShareAsync(request);

        if (share is null)
        {
            return BadRequest(new OperationResultDto
            {
                Succeeded = false,
                Message = "Share token request is invalid or trip plan was not found."
            });
        }

        return Created($"/api/shares/{share.Token}/trip-plan", share);
    }

    [HttpDelete("api/trip-plans/{tripPlanId:guid}/shares/{shareId:guid}")]
    [Authorize]
    public async Task<ActionResult<OperationResultDto>> RevokeShare(Guid tripPlanId, Guid shareId)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var result = await sharingService.RevokeShareAsync(
            tripPlanId,
            shareId,
            ControllerUserContext.GetUserId(this));

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpGet("api/shares/{token}/trip-plan")]
    [AllowAnonymous]
    public async Task<ActionResult<SharedTripPlanDto>> GetSharedTripPlan(string token)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var sharedTripPlan = await sharingService.GetSharedTripPlanAsync(token);

        return sharedTripPlan is null ? NotFound() : Ok(sharedTripPlan);
    }

    [HttpPut("api/shares/{token}/trip-plan")]
    [AllowAnonymous]
    public async Task<ActionResult<SharedTripPlanDto>> UpdateSharedTripPlan(string token, UpdateTripPlanRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var share = await sharingService.GetShareAsync(token);
        if (share is null)
        {
            return NotFound();
        }

        if (share.AccessLevel != ShareAccessLevel.Edit)
        {
            return Forbid();
        }

        var sharedTripPlan = await sharingService.UpdateSharedTripPlanAsync(token, request);

        return sharedTripPlan is null ? BadRequest() : Ok(sharedTripPlan);
    }
}
