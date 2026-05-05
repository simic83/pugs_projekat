using ApiGatewayService.Configuration;
using ApiGatewayService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Contracts.Budget;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Interfaces;

namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/trip-plans/{tripPlanId:guid}/expenses")]
public sealed class ExpensesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ExpenseDto>>> GetExpenses(Guid tripPlanId)
    {
        var budgetService = GatewayServiceProxyFactory.CreateStateful<IBudgetService>(ServiceNames.BudgetServiceUri);
        var expenses = await budgetService.GetExpensesAsync(tripPlanId, ControllerUserContext.GetUserId(this));

        return Ok(expenses);
    }

    [HttpGet("{expenseId:guid}")]
    public async Task<ActionResult<ExpenseDto>> GetExpense(Guid tripPlanId, Guid expenseId)
    {
        var budgetService = GatewayServiceProxyFactory.CreateStateful<IBudgetService>(ServiceNames.BudgetServiceUri);
        var expense = await budgetService.GetExpenseByIdAsync(
            tripPlanId,
            expenseId,
            ControllerUserContext.GetUserId(this));

        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> CreateExpense(Guid tripPlanId, CreateExpenseRequestDto request)
    {
        var budgetService = GatewayServiceProxyFactory.CreateStateful<IBudgetService>(ServiceNames.BudgetServiceUri);
        request.TripPlanId = tripPlanId;
        var expense = await budgetService.CreateExpenseAsync(
            tripPlanId,
            ControllerUserContext.GetUserId(this),
            request);

        if (expense is null)
        {
            return BadRequest(new OperationResultDto
            {
                Succeeded = false,
                Message = "Expense request is invalid or trip plan was not found."
            });
        }

        return Created($"/api/trip-plans/{tripPlanId}/expenses/{expense.Id}", expense);
    }

    [HttpPut("{expenseId:guid}")]
    public async Task<ActionResult<ExpenseDto>> UpdateExpense(
        Guid tripPlanId,
        Guid expenseId,
        UpdateExpenseRequestDto request)
    {
        var budgetService = GatewayServiceProxyFactory.CreateStateful<IBudgetService>(ServiceNames.BudgetServiceUri);
        var expense = await budgetService.UpdateExpenseAsync(
            tripPlanId,
            expenseId,
            ControllerUserContext.GetUserId(this),
            request);

        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpDelete("{expenseId:guid}")]
    public async Task<ActionResult<OperationResultDto>> DeleteExpense(Guid tripPlanId, Guid expenseId)
    {
        var budgetService = GatewayServiceProxyFactory.CreateStateful<IBudgetService>(ServiceNames.BudgetServiceUri);
        var result = await budgetService.DeleteExpenseAsync(
            tripPlanId,
            expenseId,
            ControllerUserContext.GetUserId(this));

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
