using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using TravelPlanner.Common.ServiceFabric;
using TravelPlanner.Contracts.DTOs.Auth;
using TravelPlanner.Contracts.Interfaces;

namespace TravelPlanner.ApiGatewayService.Controllers;

[ApiController]
[Route("api")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("users")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await CreateIdentityServiceProxy().RegisterAsync(request);
        if (!result.Succeeded)
        {
            return ToAuthErrorResult(result);
        }

        return Created("/api/users/me", result);
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await CreateIdentityServiceProxy().LoginAsync(request);
        if (!result.Succeeded)
        {
            return ToAuthErrorResult(result);
        }

        return Ok(result);
    }

    [HttpGet("users/me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var token = GetBearerToken();
        if (token is null)
        {
            return UnauthorizedProblem("Bearer token is required.", "MissingToken");
        }

        var validation = await CreateIdentityServiceProxy().ValidateTokenAsync(token);
        if (!validation.IsValid)
        {
            return ToTokenErrorResult(validation);
        }

        return Ok(validation.User);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var token = GetBearerToken();
        if (token is null)
        {
            return UnauthorizedProblem("Bearer token is required.", "MissingToken");
        }

        var validation = await CreateIdentityServiceProxy().ValidateTokenRoleAsync(token, "Admin");
        if (!validation.IsValid || !validation.IsAuthorized)
        {
            return ToTokenErrorResult(validation);
        }

        return StatusCode(
            StatusCodes.Status501NotImplemented,
            new ProblemDetails
            {
                Status = StatusCodes.Status501NotImplemented,
                Title = "Admin user listing is not implemented yet.",
            });
    }

    private static IIdentityService CreateIdentityServiceProxy()
    {
        return ServiceProxy.Create<IIdentityService>(ServiceNames.IdentityServiceUri);
    }

    private string? GetBearerToken()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        const string bearerPrefix = "Bearer ";
        return authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[bearerPrefix.Length..].Trim()
            : null;
    }

    private IActionResult ToAuthErrorResult(AuthResponseDto result)
    {
        return result.ErrorCode switch
        {
            "ValidationError" => BadRequestProblem(result.ErrorMessage, result.ErrorCode),
            "DuplicateEmail" => ConflictProblem(result.ErrorMessage, result.ErrorCode),
            "InvalidCredentials" => UnauthorizedProblem(result.ErrorMessage, result.ErrorCode),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                CreateProblem(StatusCodes.Status500InternalServerError, result.ErrorMessage, result.ErrorCode)),
        };
    }

    private IActionResult ToTokenErrorResult(TokenValidationResultDto result)
    {
        if (result.IsValid && !result.IsAuthorized)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                CreateProblem(StatusCodes.Status403Forbidden, result.ErrorMessage, result.ErrorCode));
        }

        return UnauthorizedProblem(result.ErrorMessage, result.ErrorCode);
    }

    private BadRequestObjectResult BadRequestProblem(string? message, string? errorCode)
    {
        return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, message, errorCode));
    }

    private ConflictObjectResult ConflictProblem(string? message, string? errorCode)
    {
        return Conflict(CreateProblem(StatusCodes.Status409Conflict, message, errorCode));
    }

    private UnauthorizedObjectResult UnauthorizedProblem(string? message, string? errorCode)
    {
        return Unauthorized(CreateProblem(StatusCodes.Status401Unauthorized, message, errorCode));
    }

    private static ProblemDetails CreateProblem(int statusCode, string? message, string? errorCode)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = message ?? "Authentication request failed.",
            Detail = errorCode,
        };
    }
}
