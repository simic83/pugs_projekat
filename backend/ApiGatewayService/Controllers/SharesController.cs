using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Activities;
using TravelPlanner.Contracts.Budget;
using TravelPlanner.Contracts.Checklist;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Destinations;
using TravelPlanner.Contracts.Enums;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Notes;
using TravelPlanner.Contracts.Reminders;
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
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var sharedTripPlan = await sharingService.UpdateSharedTripPlanAsync(token, request);

        return sharedTripPlan is null ? InvalidEditRequest("Shared trip plan request is invalid.") : Ok(sharedTripPlan);
    }

    [HttpPost("api/shares/{token}/destinations")]
    [AllowAnonymous]
    public async Task<ActionResult<DestinationDto>> CreateSharedDestination(
        string token,
        CreateDestinationRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var destination = await sharingService.CreateSharedDestinationAsync(token, request);
        return destination is null
            ? InvalidEditRequest("Destination request is invalid or trip plan was not found.")
            : Created($"/api/shares/{token}/destinations/{destination.Id}", destination);
    }

    [HttpPut("api/shares/{token}/destinations/{destinationId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<DestinationDto>> UpdateSharedDestination(
        string token,
        Guid destinationId,
        UpdateDestinationRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var destination = await sharingService.UpdateSharedDestinationAsync(token, destinationId, request);
        return destination is null ? NotFound() : Ok(destination);
    }

    [HttpDelete("api/shares/{token}/destinations/{destinationId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResultDto>> DeleteSharedDestination(string token, Guid destinationId)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var result = await sharingService.DeleteSharedDestinationAsync(token, destinationId);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("api/shares/{token}/activities")]
    [AllowAnonymous]
    public async Task<ActionResult<ActivityDto>> CreateSharedActivity(string token, CreateActivityRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var activity = await sharingService.CreateSharedActivityAsync(token, request);
        return activity is null
            ? InvalidEditRequest("Activity request is invalid or trip plan was not found.")
            : Created($"/api/shares/{token}/activities/{activity.Id}", activity);
    }

    [HttpPut("api/shares/{token}/activities/{activityId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ActivityDto>> UpdateSharedActivity(
        string token,
        Guid activityId,
        UpdateActivityRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var activity = await sharingService.UpdateSharedActivityAsync(token, activityId, request);
        return activity is null ? NotFound() : Ok(activity);
    }

    [HttpDelete("api/shares/{token}/activities/{activityId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResultDto>> DeleteSharedActivity(string token, Guid activityId)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var result = await sharingService.DeleteSharedActivityAsync(token, activityId);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("api/shares/{token}/expenses")]
    [AllowAnonymous]
    public async Task<ActionResult<ExpenseDto>> CreateSharedExpense(string token, CreateExpenseRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var expense = await sharingService.CreateSharedExpenseAsync(token, request);
        return expense is null
            ? InvalidEditRequest("Expense request is invalid or trip plan was not found.")
            : Created($"/api/shares/{token}/expenses/{expense.Id}", expense);
    }

    [HttpPut("api/shares/{token}/expenses/{expenseId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ExpenseDto>> UpdateSharedExpense(
        string token,
        Guid expenseId,
        UpdateExpenseRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var expense = await sharingService.UpdateSharedExpenseAsync(token, expenseId, request);
        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpDelete("api/shares/{token}/expenses/{expenseId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResultDto>> DeleteSharedExpense(string token, Guid expenseId)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var result = await sharingService.DeleteSharedExpenseAsync(token, expenseId);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("api/shares/{token}/checklist-items")]
    [AllowAnonymous]
    public async Task<ActionResult<ChecklistItemDto>> CreateSharedChecklistItem(
        string token,
        CreateChecklistItemRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var checklistItem = await sharingService.CreateSharedChecklistItemAsync(token, request);
        return checklistItem is null
            ? InvalidEditRequest("Checklist item request is invalid or trip plan was not found.")
            : Created($"/api/shares/{token}/checklist-items/{checklistItem.Id}", checklistItem);
    }

    [HttpPut("api/shares/{token}/checklist-items/{checklistItemId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ChecklistItemDto>> UpdateSharedChecklistItem(
        string token,
        Guid checklistItemId,
        UpdateChecklistItemRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var checklistItem = await sharingService.UpdateSharedChecklistItemAsync(token, checklistItemId, request);
        return checklistItem is null ? NotFound() : Ok(checklistItem);
    }

    [HttpDelete("api/shares/{token}/checklist-items/{checklistItemId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResultDto>> DeleteSharedChecklistItem(string token, Guid checklistItemId)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var result = await sharingService.DeleteSharedChecklistItemAsync(token, checklistItemId);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("api/shares/{token}/notes")]
    [AllowAnonymous]
    public async Task<ActionResult<NoteDto>> CreateSharedNote(string token, CreateNoteRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var note = await sharingService.CreateSharedNoteAsync(token, request);
        return note is null
            ? InvalidEditRequest("Note request is invalid or trip plan was not found.")
            : Created($"/api/shares/{token}/notes/{note.Id}", note);
    }

    [HttpPut("api/shares/{token}/notes/{noteId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<NoteDto>> UpdateSharedNote(string token, Guid noteId, UpdateNoteRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var note = await sharingService.UpdateSharedNoteAsync(token, noteId, request);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpDelete("api/shares/{token}/notes/{noteId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResultDto>> DeleteSharedNote(string token, Guid noteId)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var result = await sharingService.DeleteSharedNoteAsync(token, noteId);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("api/shares/{token}/reminders")]
    [AllowAnonymous]
    public async Task<ActionResult<ReminderDto>> CreateSharedReminder(string token, CreateReminderRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var reminder = await sharingService.CreateSharedReminderAsync(token, request);
        return reminder is null
            ? InvalidEditRequest("Reminder request is invalid or trip plan was not found.")
            : Created($"/api/shares/{token}/reminders/{reminder.Id}", reminder);
    }

    [HttpPut("api/shares/{token}/reminders/{reminderId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ReminderDto>> UpdateSharedReminder(
        string token,
        Guid reminderId,
        UpdateReminderRequestDto request)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var reminder = await sharingService.UpdateSharedReminderAsync(token, reminderId, request);
        return reminder is null ? NotFound() : Ok(reminder);
    }

    [HttpDelete("api/shares/{token}/reminders/{reminderId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<OperationResultDto>> DeleteSharedReminder(string token, Guid reminderId)
    {
        var sharingService = GatewayServiceProxyFactory.CreateStateful<ISharingService>(ServiceNames.SharingServiceUri);
        var accessError = await ValidateSharedEditAccessAsync(sharingService, token);
        if (accessError is not null)
        {
            return accessError;
        }

        var result = await sharingService.DeleteSharedReminderAsync(token, reminderId);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    private static async Task<ActionResult?> ValidateSharedEditAccessAsync(ISharingService sharingService, string token)
    {
        var share = await sharingService.GetShareAsync(token);
        if (share is null)
        {
            return new NotFoundObjectResult(new OperationResultDto
            {
                Succeeded = false,
                Message = "Share link is invalid, expired or revoked."
            });
        }

        if (share.AccessLevel != ShareAccessLevel.Edit)
        {
            return new ObjectResult(new OperationResultDto
            {
                Succeeded = false,
                Message = "VIEW share link is read-only and cannot change trip plan data."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return null;
    }

    private static BadRequestObjectResult InvalidEditRequest(string message)
    {
        return new BadRequestObjectResult(new OperationResultDto
        {
            Succeeded = false,
            Message = message
        });
    }
}
