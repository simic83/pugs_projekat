using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/trip-plans/{tripPlanId:guid}/notes")]
public sealed class NotesController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<object>> GetNotes(Guid tripPlanId)
    {
        return Ok(new List<object>());
    }

    [HttpPost]
    public ActionResult CreateNote(Guid tripPlanId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            message = "Notes persistence will be added after the trip planning service is implemented."
        });
    }
}
