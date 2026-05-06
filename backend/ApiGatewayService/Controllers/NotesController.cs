using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Notes;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/trip-plans/{tripPlanId:guid}/notes")]
public sealed class NotesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<NoteDto>>> GetNotes(Guid tripPlanId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var notes = await tripPlanningService.GetNotesAsync(tripPlanId, ControllerUserContext.GetUserId(this));

        return Ok(notes);
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> CreateNote(Guid tripPlanId, CreateNoteRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        request.TripPlanId = tripPlanId;
        var note = await tripPlanningService.CreateNoteAsync(
            tripPlanId,
            ControllerUserContext.GetUserId(this),
            request);

        if (note is null)
        {
            return BadRequest(new OperationResultDto
            {
                Succeeded = false,
                Message = "Note request is invalid or trip plan was not found."
            });
        }

        return Created($"/api/trip-plans/{tripPlanId}/notes/{note.Id}", note);
    }

    [HttpPut("{noteId:guid}")]
    public async Task<ActionResult<NoteDto>> UpdateNote(Guid tripPlanId, Guid noteId, UpdateNoteRequestDto request)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var note = await tripPlanningService.UpdateNoteAsync(
            tripPlanId,
            noteId,
            ControllerUserContext.GetUserId(this),
            request);

        return note is null ? NotFound() : Ok(note);
    }

    [HttpDelete("{noteId:guid}")]
    public async Task<ActionResult<OperationResultDto>> DeleteNote(Guid tripPlanId, Guid noteId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var result = await tripPlanningService.DeleteNoteAsync(
            tripPlanId,
            noteId,
            ControllerUserContext.GetUserId(this));

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
