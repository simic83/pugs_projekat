using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Budget;
using TravelPlanner.Contracts.Interfaces;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/trip-plans/{tripPlanId:guid}/budget")]
public sealed class BudgetController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BudgetSummaryDto>> GetBudgetSummary(Guid tripPlanId)
    {
        var budgetService = GatewayServiceProxyFactory.CreateStateful<IBudgetService>(ServiceNames.BudgetServiceUri);
        var summary = await budgetService.GetBudgetSummaryAsync(tripPlanId, ControllerUserContext.GetUserId(this));

        return summary is null ? NotFound() : Ok(summary);
    }
}
