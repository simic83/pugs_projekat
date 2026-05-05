using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Trips;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/trip-plans")]
public sealed class TripPlansController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TripPlanDto>>> GetTripPlans()
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var tripPlans = await tripPlanningService.GetTripPlansAsync(ControllerUserContext.GetUserId(this));

        return Ok(tripPlans);
    }

    [HttpGet("{tripPlanId:guid}")]
    public async Task<ActionResult<TripPlanDto>> GetTripPlan(Guid tripPlanId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var tripPlan = await tripPlanningService.GetTripPlanByIdAsync(tripPlanId, ControllerUserContext.GetUserId(this));

        return tripPlan is null ? NotFound() : Ok(tripPlan);
    }

    [HttpPost]
    public async Task<ActionResult<TripPlanDto>> CreateTripPlan(CreateTripPlanRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        request.OwnerUserId = ControllerUserContext.GetUserId(this);
        var tripPlan = await tripPlanningService.CreateTripPlanAsync(request);

        if (tripPlan is null)
        {
            return BadRequest(new OperationResultDto
            {
                Succeeded = false,
                Message = "Trip plan request is invalid."
            });
        }

        return Created($"/api/trip-plans/{tripPlan.Id}", tripPlan);
    }

    [HttpPut("{tripPlanId:guid}")]
    public async Task<ActionResult<TripPlanDto>> UpdateTripPlan(Guid tripPlanId, UpdateTripPlanRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var tripPlan = await tripPlanningService.UpdateTripPlanAsync(
            tripPlanId,
            ControllerUserContext.GetUserId(this),
            request);

        return tripPlan is null ? NotFound() : Ok(tripPlan);
    }

    [HttpDelete("{tripPlanId:guid}")]
    public async Task<ActionResult<OperationResultDto>> DeleteTripPlan(Guid tripPlanId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var result = await tripPlanningService.DeleteTripPlanAsync(tripPlanId, ControllerUserContext.GetUserId(this));

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
