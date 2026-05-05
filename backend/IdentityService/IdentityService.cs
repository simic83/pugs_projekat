using System.Fabric;
using IdentityService.Configuration;
using IdentityService.Data;
using IdentityService.Mapping;
using IdentityService.Security;
using IdentityService.Validation;
using Microsoft.Data.SqlClient;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Contracts.Auth;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Users;

namespace IdentityService
{
    internal sealed class IdentityService : StatelessService, IIdentityService
    {
        private readonly IUserRepository userRepository;
        private readonly PasswordHasher passwordHasher;
        private readonly JwtTokenService jwtTokenService;

        public IdentityService(StatelessServiceContext context)
            : base(context)
        {
            var settings = FabricConfigurationProvider.Load(context);
            userRepository = new SqlUserRepository(settings.DefaultConnection);
            passwordHasher = new PasswordHasher();
            jwtTokenService = new JwtTokenService(settings);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
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
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
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

                var user = await userRepository.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());
                if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Failure("Invalid email or password.");
                }

                return Success(user);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
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
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
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
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                ServiceEventSource.Current.ServiceMessage(Context, "Identity user list failed: {0}", exception.Message);
                return new List<UserDto>();
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
    }
}
