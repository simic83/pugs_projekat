using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Activities;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Interfaces;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/trip-plans/{tripPlanId:guid}/activities")]
public sealed class ActivitiesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ActivityDto>>> GetActivities(Guid tripPlanId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var activities = await tripPlanningService.GetActivitiesAsync(tripPlanId, ControllerUserContext.GetUserId(this));

        return Ok(activities);
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> CreateActivity(Guid tripPlanId, CreateActivityRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var activity = await tripPlanningService.CreateActivityAsync(
            tripPlanId,
            ControllerUserContext.GetUserId(this),
            request);

        if (activity is null)
        {
            return BadRequest(new OperationResultDto
            {
                Succeeded = false,
                Message = "Activity request is invalid or trip plan was not found."
            });
        }

        return Created($"/api/trip-plans/{tripPlanId}/activities/{activity.Id}", activity);
    }

    [HttpPut("{activityId:guid}")]
    public async Task<ActionResult<ActivityDto>> UpdateActivity(
        Guid tripPlanId,
        Guid activityId,
        UpdateActivityRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var activity = await tripPlanningService.UpdateActivityAsync(
            tripPlanId,
            activityId,
            ControllerUserContext.GetUserId(this),
            request);

        return activity is null ? NotFound() : Ok(activity);
    }

    [HttpDelete("{activityId:guid}")]
    public async Task<ActionResult<OperationResultDto>> DeleteActivity(Guid tripPlanId, Guid activityId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var result = await tripPlanningService.DeleteActivityAsync(
            tripPlanId,
            activityId,
            ControllerUserContext.GetUserId(this));

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
