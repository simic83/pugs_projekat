namespace TravelPlanner.Common.Security;

public interface IJwtTokenValidator
{
    JwtValidationResult Validate(string token);
}

