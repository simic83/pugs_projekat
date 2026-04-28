using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using TravelPlanner.Common.ServiceFabric;
using TravelPlanner.Contracts.DTOs.Budget;
using TravelPlanner.Contracts.DTOs.Sharing;
using TravelPlanner.Contracts.DTOs.Trips;
using TravelPlanner.Contracts.Interfaces;

namespace TravelPlanner.ApiGatewayService.Controllers;

[ApiController]
[Route("api/trip-plans")]
public sealed class TripPlansController : ControllerBase
{
    [HttpGet]
    public IActionResult GetTripPlans()
    {
        _ = CreateTripPlanningServiceProxy();

        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpGet("{tripPlanId:guid}")]
    public IActionResult GetTripPlan(Guid tripPlanId)
    {
        _ = CreateTripPlanningServiceProxy();

        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost]
    public IActionResult CreateTripPlan([FromBody] CreateTripPlanRequestDto request)
    {
        _ = CreateTripPlanningServiceProxy();

        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPut("{tripPlanId:guid}")]
    public IActionResult UpdateTripPlan(Guid tripPlanId, [FromBody] TripPlanDto request)
    {
        _ = CreateTripPlanningServiceProxy();

        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpDelete("{tripPlanId:guid}")]
    public IActionResult DeleteTripPlan(Guid tripPlanId)
    {
        _ = CreateTripPlanningServiceProxy();
        _ = CreateBudgetServiceProxy();
        _ = CreateSharingServiceProxy();

        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("{tripPlanId:guid}/destinations")]
    public IActionResult CreateDestination(Guid tripPlanId, [FromBody] CreateDestinationRequestDto request)
    {
        _ = CreateTripPlanningServiceProxy();

        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("{tripPlanId:guid}/activities")]
    public IActionResult CreateActivity(Guid tripPlanId, [FromBody] CreateActivityRequestDto request)
    {
        _ = CreateTripPlanningServiceProxy();

        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("{tripPlanId:guid}/expenses")]
    public IActionResult CreateExpense(Guid tripPlanId, [FromBody] CreateExpenseRequestDto request)
    {
        _ = CreateBudgetServiceProxy();

        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpGet("{tripPlanId:guid}/budget")]
    public IActionResult GetBudget(Guid tripPlanId)
    {
        _ = CreateBudgetServiceProxy();

        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("{tripPlanId:guid}/shares")]
    public IActionResult CreateShareAccess(Guid tripPlanId, [FromBody] ShareRequestDto request)
    {
        _ = CreateSharingServiceProxy();

        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    private static ITripPlanningService CreateTripPlanningServiceProxy()
    {
        return ServiceProxy.Create<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
    }

    private static IBudgetService CreateBudgetServiceProxy()
    {
        return ServiceProxy.Create<IBudgetService>(ServiceNames.BudgetServiceUri);
    }

    private static ISharingService CreateSharingServiceProxy()
    {
        return ServiceProxy.Create<ISharingService>(ServiceNames.SharingServiceUri);
    }
}

