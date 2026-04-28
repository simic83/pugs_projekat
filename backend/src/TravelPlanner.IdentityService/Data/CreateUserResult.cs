namespace TravelPlanner.IdentityService.Data;

internal sealed class CreateUserResult
{
    public bool Succeeded { get; private init; }
    public bool DuplicateEmail { get; private init; }
    public UserRecord? User { get; private init; }

    public static CreateUserResult Success(UserRecord user)
    {
        return new CreateUserResult
        {
            Succeeded = true,
            User = user,
        };
    }

    public static CreateUserResult EmailAlreadyExists()
    {
        return new CreateUserResult
        {
            Succeeded = false,
            DuplicateEmail = true,
        };
    }
}
