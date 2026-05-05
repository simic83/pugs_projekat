using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Checklist;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Interfaces;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/trip-plans/{tripPlanId:guid}/checklist-items")]
public sealed class ChecklistController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ChecklistItemDto>>> GetChecklistItems(Guid tripPlanId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var checklistItems = await tripPlanningService.GetChecklistItemsAsync(
            tripPlanId,
            ControllerUserContext.GetUserId(this));

        return Ok(checklistItems);
    }

    [HttpPost]
    public async Task<ActionResult<ChecklistItemDto>> CreateChecklistItem(
        Guid tripPlanId,
        CreateChecklistItemRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        request.TripPlanId = tripPlanId;
        var checklistItem = await tripPlanningService.CreateChecklistItemAsync(
            tripPlanId,
            ControllerUserContext.GetUserId(this),
            request);

        if (checklistItem is null)
        {
            return BadRequest(new OperationResultDto
            {
                Succeeded = false,
                Message = "Checklist item request is invalid or trip plan was not found."
            });
        }

        return Created($"/api/trip-plans/{tripPlanId}/checklist-items/{checklistItem.Id}", checklistItem);
    }

    [HttpPut("{checklistItemId:guid}")]
    public async Task<ActionResult<ChecklistItemDto>> UpdateChecklistItem(
        Guid tripPlanId,
        Guid checklistItemId,
        UpdateChecklistItemRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var checklistItem = await tripPlanningService.UpdateChecklistItemAsync(
            tripPlanId,
            checklistItemId,
            ControllerUserContext.GetUserId(this),
            request);

        return checklistItem is null ? NotFound() : Ok(checklistItem);
    }

    [HttpDelete("{checklistItemId:guid}")]
    public async Task<ActionResult<OperationResultDto>> DeleteChecklistItem(Guid tripPlanId, Guid checklistItemId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var result = await tripPlanningService.DeleteChecklistItemAsync(
            tripPlanId,
            checklistItemId,
            ControllerUserContext.GetUserId(this));

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
