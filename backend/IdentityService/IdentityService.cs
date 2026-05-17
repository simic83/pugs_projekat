using System.Fabric;
using IdentityService.Configuration;
using IdentityService.Data;
using IdentityService.Mapping;
using IdentityService.Security;
using IdentityService.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Contracts.Auth;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Enums;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Users;
using TravelPlanner.Persistence;

namespace IdentityService
{
    internal sealed class IdentityService : StatelessService, IIdentityService
    {
        private const string BootstrapAdminAlias = "admin";
        private const string BootstrapAdminEmail = "admin@travelplanner.local";
        private readonly ServiceProvider serviceProvider;
        private readonly IUserRepository userRepository;
        private readonly PasswordHasher passwordHasher;
        private readonly JwtTokenService jwtTokenService;

        public IdentityService(StatelessServiceContext context)
            : base(context)
        {
            var settings = FabricConfigurationProvider.Load(context);
            serviceProvider = ConfigureServices(settings);
            userRepository = serviceProvider.GetRequiredService<IUserRepository>();
            passwordHasher = new PasswordHasher();
            jwtTokenService = new JwtTokenService(settings);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        private static ServiceProvider ConfigureServices(IdentityServiceSettings settings)
        {
            var services = new ServiceCollection();
            services.AddSingleton(settings);
            services.AddTravelPlannerPersistence(settings.DefaultConnection);
            services.AddSingleton<IUserRepository, EfUserRepository>();

            return services.BuildServiceProvider();
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var errors = RegisterRequestValidator.Validate(request);
            if (errors.Count > 0)
            {
                return Failure("Registration request is invalid.", errors);
            }

            try
            {
                jwtTokenService.EnsureConfigured();

                var normalizedEmail = request.Email.Trim().ToLowerInvariant();
                errors.AddRange(RegisterRequestValidator.ValidateDuplicateEmail(
                    await userRepository.EmailExistsAsync(normalizedEmail)));

                if (errors.Count > 0)
                {
                    return Failure("Registration request is invalid.", errors);
                }

                var user = await userRepository.CreateUserAsync(new Models.UserRecord
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name.Trim(),
                    Email = normalizedEmail,
                    PasswordHash = passwordHasher.HashPassword(request.Password),
                    CreatedAtUtc = DateTime.UtcNow
                });

                return Success(user);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                return Failure(exception.Message);
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var errors = LoginRequestValidator.Validate(request);
            if (errors.Count > 0)
            {
                return Failure("Login request is invalid.", errors);
            }

            try
            {
                jwtTokenService.EnsureConfigured();

                var user = await userRepository.FindByEmailAsync(NormalizeLoginIdentifier(request.Email));
                if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Failure("Invalid email or password.");
                }

                return Success(user);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                return Failure(exception.Message);
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await userRepository.GetByIdAsync(userId);
                return user is null ? null : UserMapper.ToDto(user);
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                ServiceEventSource.Current.ServiceMessage(Context, "Identity user lookup failed: {0}", exception.Message);
                return null;
            }
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            try
            {
                var users = await userRepository.GetUsersAsync();
                return users.Select(UserMapper.ToDto).ToList();
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                ServiceEventSource.Current.ServiceMessage(Context, "Identity user list failed: {0}", exception.Message);
                return new List<UserDto>();
            }
        }

        public async Task<OperationResultDto> ChangeUserRoleAsync(Guid userId, ChangeUserRoleRequest request)
        {
            if (request is null)
            {
                return ResultFailure("Role request is required.");
            }

            if (userId == Guid.Empty)
            {
                return ResultFailure("User id is invalid.");
            }

            var roleName = NormalizeRoleName(request.Role);
            if (roleName is null)
            {
                return ResultFailure("Role must be User or Admin.");
            }

            try
            {
                var user = await userRepository.GetByIdAsync(userId);
                if (user is null)
                {
                    return ResultFailure("User was not found.");
                }

                if (!await userRepository.RoleExistsAsync(roleName))
                {
                    return ResultFailure("Requested role does not exist.");
                }

                var userIsAdmin = user.Roles.Any(role =>
                    string.Equals(role.Name, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase));

                if (userIsAdmin && roleName != UserRole.Admin.ToString())
                {
                    var adminCount = await userRepository.CountUsersInRoleAsync(UserRole.Admin.ToString());
                    if (adminCount <= 1)
                    {
                        return ResultFailure("Cannot remove the last admin role.");
                    }
                }

                var changed = await userRepository.SetUserRoleAsync(userId, roleName);
                return changed ? ResultSuccess("User role changed.") : ResultFailure("User role was not changed.");
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                ServiceEventSource.Current.ServiceMessage(Context, "Identity user role change failed: {0}", exception.Message);
                return ResultFailure(exception.Message);
            }
        }

        public async Task<OperationResultDto> DeleteUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return ResultFailure("User id is invalid.");
            }

            try
            {
                var user = await userRepository.GetByIdAsync(userId);
                if (user is null)
                {
                    return ResultFailure("User was not found.");
                }

                var userIsAdmin = user.Roles.Any(role =>
                    string.Equals(role.Name, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase));

                if (userIsAdmin)
                {
                    var adminCount = await userRepository.CountUsersInRoleAsync(UserRole.Admin.ToString());
                    if (adminCount <= 1)
                    {
                        return ResultFailure("Cannot delete the last admin user.");
                    }
                }

                var deleted = await userRepository.DeleteUserAsync(userId);
                return deleted ? ResultSuccess("User deleted.") : ResultFailure("User was not deleted.");
            }
            catch (Exception exception) when (IsPersistenceException(exception))
            {
                ServiceEventSource.Current.ServiceMessage(Context, "Identity user delete failed: {0}", exception.Message);
                return ResultFailure(exception.Message);
            }
        }

        private AuthResponseDto Success(Models.UserRecord user)
        {
            var token = jwtTokenService.CreateToken(user);

            return new AuthResponseDto
            {
                Result = new OperationResultDto
                {
                    Succeeded = true,
                    Message = "Authentication succeeded."
                },
                AccessToken = token.AccessToken,
                ExpiresAtUtc = token.ExpiresAtUtc,
                User = UserMapper.ToDto(user)
            };
        }

        private static AuthResponseDto Failure(string message, List<ValidationErrorDto>? errors = null)
        {
            return new AuthResponseDto
            {
                Result = new OperationResultDto
                {
                    Succeeded = false,
                    Message = message,
                    Errors = errors ?? new List<ValidationErrorDto>()
                }
            };
        }

        private static string? NormalizeRoleName(string? role)
        {
            var trimmedRole = role?.Trim();
            if (string.Equals(trimmedRole, UserRole.User.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return UserRole.User.ToString();
            }

            if (string.Equals(trimmedRole, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return UserRole.Admin.ToString();
            }

            return null;
        }

        private static string NormalizeLoginIdentifier(string value)
        {
            var trimmedValue = value.Trim();
            return string.Equals(trimmedValue, BootstrapAdminAlias, StringComparison.OrdinalIgnoreCase)
                ? BootstrapAdminEmail
                : trimmedValue.ToLowerInvariant();
        }

        private static bool IsPersistenceException(Exception exception)
        {
            return PersistenceExceptionClassifier.IsPersistenceException(exception);
        }

        private static OperationResultDto ResultSuccess(string message)
        {
            return new OperationResultDto
            {
                Succeeded = true,
                Message = message
            };
        }

        private static OperationResultDto ResultFailure(string message)
        {
            return new OperationResultDto
            {
                Succeeded = false,
                Message = message
            };
        }
    }
}
