using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Reminders;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/trip-plans/{tripPlanId:guid}/reminders")]
public sealed class RemindersController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ReminderDto>>> GetReminders(Guid tripPlanId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var reminders = await tripPlanningService.GetRemindersAsync(tripPlanId, ControllerUserContext.GetUserId(this));

        return Ok(reminders);
    }

    [HttpPost]
    public async Task<ActionResult<ReminderDto>> CreateReminder(Guid tripPlanId, CreateReminderRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        request.TripPlanId = tripPlanId;
        var reminder = await tripPlanningService.CreateReminderAsync(
            tripPlanId,
            ControllerUserContext.GetUserId(this),
            request);

        if (reminder is null)
        {
            return BadRequest(new OperationResultDto
            {
                Succeeded = false,
                Message = "Reminder request is invalid or trip plan was not found."
            });
        }

        return Created($"/api/trip-plans/{tripPlanId}/reminders/{reminder.Id}", reminder);
    }

    [HttpPut("{reminderId:guid}")]
    public async Task<ActionResult<ReminderDto>> UpdateReminder(
        Guid tripPlanId,
        Guid reminderId,
        UpdateReminderRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var reminder = await tripPlanningService.UpdateReminderAsync(
            tripPlanId,
            reminderId,
            ControllerUserContext.GetUserId(this),
            request);

        return reminder is null ? NotFound() : Ok(reminder);
    }

    [HttpDelete("{reminderId:guid}")]
    public async Task<ActionResult<OperationResultDto>> DeleteReminder(Guid tripPlanId, Guid reminderId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var result = await tripPlanningService.DeleteReminderAsync(
            tripPlanId,
            reminderId,
            ControllerUserContext.GetUserId(this));

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
