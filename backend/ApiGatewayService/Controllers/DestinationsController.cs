using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Destinations;
using TravelPlanner.Contracts.Interfaces;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/trip-plans/{tripPlanId:guid}/destinations")]
public sealed class DestinationsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DestinationDto>>> GetDestinations(Guid tripPlanId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var destinations = await tripPlanningService.GetDestinationsAsync(tripPlanId, ControllerUserContext.GetUserId(this));

        return Ok(destinations);
    }

    [HttpPost]
    public async Task<ActionResult<DestinationDto>> CreateDestination(Guid tripPlanId, CreateDestinationRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var destination = await tripPlanningService.CreateDestinationAsync(
            tripPlanId,
            ControllerUserContext.GetUserId(this),
            request);

        if (destination is null)
        {
            return BadRequest(new OperationResultDto
            {
                Succeeded = false,
                Message = "Destination request is invalid or trip plan was not found."
            });
        }

        return Created($"/api/trip-plans/{tripPlanId}/destinations/{destination.Id}", destination);
    }

    [HttpPut("{destinationId:guid}")]
    public async Task<ActionResult<DestinationDto>> UpdateDestination(
        Guid tripPlanId,
        Guid destinationId,
        UpdateDestinationRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var destination = await tripPlanningService.UpdateDestinationAsync(
            tripPlanId,
            destinationId,
            ControllerUserContext.GetUserId(this),
            request);

        return destination is null ? NotFound() : Ok(destination);
    }

    [HttpDelete("{destinationId:guid}")]
    public async Task<ActionResult<OperationResultDto>> DeleteDestination(Guid tripPlanId, Guid destinationId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var result = await tripPlanningService.DeleteDestinationAsync(
            tripPlanId,
            destinationId,
            ControllerUserContext.GetUserId(this));

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
