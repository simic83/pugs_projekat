using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Trips;
using TravelPlanner.Contracts.Users;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public sealed class AdminController : ControllerBase
{
    [HttpGet("trip-plans")]
    public async Task<ActionResult<List<AdminTripPlanDto>>> GetTripPlans()
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var identityService = GatewayServiceProxyFactory.CreateStateless<IIdentityService>(ServiceNames.IdentityServiceUri);

        var tripPlansTask = tripPlanningService.GetAllTripPlansForAdminAsync();
        var usersTask = identityService.GetUsersAsync();

        await Task.WhenAll(tripPlansTask, usersTask);

        var usersById = usersTask.Result.ToDictionary(user => user.Id);
        var response = tripPlansTask.Result
            .Select(tripPlan => ToAdminDto(tripPlan, usersById))
            .ToList();

        return Ok(response);
    }

    [HttpDelete("trip-plans/{tripPlanId:guid}")]
    public async Task<ActionResult<OperationResultDto>> DeleteTripPlan(Guid tripPlanId)
    {
        var tripPlanningService = GatewayServiceProxyFactory.CreateStateful<ITripPlanningService>(ServiceNames.TripPlanningServiceUri);
        var result = await tripPlanningService.DeleteTripPlanForAdminAsync(tripPlanId);

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    private static AdminTripPlanDto ToAdminDto(TripPlanDto tripPlan, Dictionary<Guid, UserDto> usersById)
    {
        usersById.TryGetValue(tripPlan.OwnerUserId, out var owner);

        return new AdminTripPlanDto
        {
            Id = tripPlan.Id,
            OwnerUserId = tripPlan.OwnerUserId,
            OwnerName = owner?.Name,
            OwnerEmail = owner?.Email,
            Title = tripPlan.Title,
            Description = tripPlan.Description,
            StartDate = tripPlan.StartDate,
            EndDate = tripPlan.EndDate,
            PlannedBudget = tripPlan.PlannedBudget,
            CreatedAtUtc = tripPlan.CreatedAtUtc
        };
    }
}
