using System.Fabric;
using Microsoft.Data.SqlClient;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SharingService.Configuration;
using SharingService.Data;
using SharingService.Models;
using TravelPlanner.Contracts.Activities;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Destinations;
using TravelPlanner.Contracts.Enums;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Sharing;
using TravelPlanner.Contracts.Trips;

namespace SharingService
{
    internal sealed class SharingService : StatefulService, ISharingService
    {
        private readonly ISharingRepository repository;

        public SharingService(StatefulServiceContext context)
            : base(context)
        {
            var settings = FabricConfigurationProvider.Load(context);
            repository = new SharingRepository(settings.DefaultConnection);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task<ShareTokenDto?> CreateShareAsync(CreateShareRequestDto request)
        {
            if (!IsValidCreateShareRequest(request))
            {
                return null;
            }

            try
            {
                if (!await repository.UserOwnsTripPlanAsync(request.TripPlanId, request.CreatedByUserId))
                {
                    return null;
                }

                var shareToken = new ShareTokenModel
                {
                    Id = Guid.NewGuid(),
                    TripPlanId = request.TripPlanId,
                    Token = Guid.NewGuid().ToString("N"),
                    AccessLevel = ToStoredAccessLevel(request.AccessLevel),
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = request.ExpiresAt,
                    IsRevoked = false
                };

                var created = await repository.CreateShareTokenAsync(shareToken);
                return ToDto(created);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Share token create failed", exception);
                return null;
            }
        }

        public async Task<ShareTokenDto?> GetShareAsync(string token)
        {
            try
            {
                var shareToken = await GetActiveShareTokenAsync(token);
                return shareToken is null ? null : ToDto(shareToken);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Share token lookup failed", exception);
                return null;
            }
        }

        public async Task<List<ShareTokenDto>> GetSharesByTripPlanAsync(Guid tripPlanId, Guid userId)
        {
            if (tripPlanId == Guid.Empty || userId == Guid.Empty)
            {
                return new List<ShareTokenDto>();
            }

            try
            {
                if (!await repository.UserOwnsTripPlanAsync(tripPlanId, userId))
                {
                    return new List<ShareTokenDto>();
                }

                var shareTokens = await repository.GetShareTokensByTripPlanIdAsync(tripPlanId);
                return shareTokens.Select(ToDto).ToList();
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Share token list failed", exception);
                return new List<ShareTokenDto>();
            }
        }

        public async Task<OperationResultDto> RevokeShareAsync(Guid tripPlanId, Guid shareId, Guid userId)
        {
            if (tripPlanId == Guid.Empty || shareId == Guid.Empty || userId == Guid.Empty)
            {
                return Failure("Share token request is invalid.");
            }

            try
            {
                if (!await repository.UserOwnsTripPlanAsync(tripPlanId, userId))
                {
                    return Failure("Trip plan was not found.");
                }

                var revoked = await repository.RevokeShareTokenAsync(tripPlanId, shareId);
                return revoked ? Success("Share token revoked.") : Failure("Share token was not found.");
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Share token revoke failed", exception);
                return Failure(exception.Message);
            }
        }

        public async Task<SharedTripPlanDto?> GetSharedTripPlanAsync(string token)
        {
            try
            {
                var shareToken = await GetActiveShareTokenAsync(token);
                return shareToken is null ? null : await BuildSharedTripPlanAsync(shareToken);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Shared trip plan lookup failed", exception);
                return null;
            }
        }

        public async Task<SharedTripPlanDto?> UpdateSharedTripPlanAsync(string token, UpdateTripPlanRequestDto request)
        {
            if (!IsValidTripPlan(request.Title, request.StartDate, request.EndDate, request.PlannedBudget))
            {
                return null;
            }

            try
            {
                var shareToken = await GetActiveShareTokenAsync(token);
                if (shareToken is null || ParseAccessLevel(shareToken.AccessLevel) != ShareAccessLevel.Edit)
                {
                    return null;
                }

                var tripPlan = await repository.GetTripPlanByIdAsync(shareToken.TripPlanId);
                if (tripPlan is null)
                {
                    return null;
                }

                tripPlan.Title = request.Title.Trim();
                tripPlan.Description = NormalizeOptionalText(request.Description);
                tripPlan.StartDate = request.StartDate.Date;
                tripPlan.EndDate = request.EndDate.Date;
                tripPlan.PlannedBudget = request.PlannedBudget;
                tripPlan.Notes = NormalizeOptionalText(request.Notes);
                tripPlan.UpdatedAt = DateTime.UtcNow;

                var updated = await repository.UpdateTripPlanAsync(tripPlan);
                return updated ? await BuildSharedTripPlanAsync(shareToken) : null;
            }
            catch (Exception exception) when (exception is InvalidOperationException or SqlException)
            {
                LogDatabaseError("Shared trip plan update failed", exception);
                return null;
            }
        }

        private async Task<ShareTokenModel?> GetActiveShareTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var shareToken = await repository.GetShareTokenByTokenAsync(token.Trim());
            if (shareToken is null || shareToken.IsRevoked)
            {
                return null;
            }

            if (shareToken.ExpiresAt.HasValue && shareToken.ExpiresAt.Value <= DateTime.UtcNow)
            {
                return null;
            }

            return shareToken;
        }

        private async Task<SharedTripPlanDto?> BuildSharedTripPlanAsync(ShareTokenModel shareToken)
        {
            var tripPlan = await repository.GetTripPlanByIdAsync(shareToken.TripPlanId);
            if (tripPlan is null)
            {
                return null;
            }

            var destinations = await repository.GetDestinationsByTripPlanIdAsync(shareToken.TripPlanId);
            var activities = await repository.GetActivitiesByTripPlanIdAsync(shareToken.TripPlanId);

            return new SharedTripPlanDto
            {
                Share = ToDto(shareToken),
                TripPlan = ToDto(tripPlan),
                Destinations = destinations.Select(ToDto).ToList(),
                Activities = activities.Select(ToDto).ToList()
            };
        }

        private static bool IsValidCreateShareRequest(CreateShareRequestDto request)
        {
            return request.TripPlanId != Guid.Empty
                && request.CreatedByUserId != Guid.Empty
                && Enum.IsDefined(typeof(ShareAccessLevel), request.AccessLevel)
                && (!request.ExpiresAt.HasValue || request.ExpiresAt.Value > DateTime.UtcNow);
        }

        private static bool IsValidTripPlan(string title, DateTime startDate, DateTime endDate, decimal plannedBudget)
        {
            return !string.IsNullOrWhiteSpace(title)
                && endDate.Date >= startDate.Date
                && plannedBudget >= 0;
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string ToStoredAccessLevel(ShareAccessLevel accessLevel)
        {
            return accessLevel == ShareAccessLevel.Edit ? "EDIT" : "VIEW";
        }

        private static ShareAccessLevel ParseAccessLevel(string accessLevel)
        {
            return string.Equals(accessLevel, "EDIT", StringComparison.OrdinalIgnoreCase)
                ? ShareAccessLevel.Edit
                : ShareAccessLevel.View;
        }

        private static ShareTokenDto ToDto(ShareTokenModel shareToken)
        {
            return new ShareTokenDto
            {
                Id = shareToken.Id,
                TripPlanId = shareToken.TripPlanId,
                Token = shareToken.Token,
                AccessLevel = ParseAccessLevel(shareToken.AccessLevel),
                CreatedByUserId = shareToken.CreatedByUserId,
                CreatedAt = shareToken.CreatedAt,
                ExpiresAt = shareToken.ExpiresAt,
                IsRevoked = shareToken.IsRevoked
            };
        }

        private static TripPlanDto ToDto(TripPlanModel tripPlan)
        {
            return new TripPlanDto
            {
                Id = tripPlan.Id,
                OwnerUserId = tripPlan.OwnerUserId,
                Title = tripPlan.Title,
                Description = tripPlan.Description,
                StartDate = tripPlan.StartDate,
                EndDate = tripPlan.EndDate,
                PlannedBudget = tripPlan.PlannedBudget,
                Notes = tripPlan.Notes,
                CreatedAtUtc = tripPlan.CreatedAt,
                UpdatedAtUtc = tripPlan.UpdatedAt
            };
        }

        private static DestinationDto ToDto(DestinationModel destination)
        {
            return new DestinationDto
            {
                Id = destination.Id,
                TripPlanId = destination.TripPlanId,
                Name = destination.Name,
                Location = destination.Location,
                ArrivalDate = destination.ArrivalDate,
                DepartureDate = destination.DepartureDate,
                Description = destination.Description,
                CreatedAtUtc = destination.CreatedAt,
                UpdatedAtUtc = destination.UpdatedAt
            };
        }

        private static ActivityDto ToDto(ActivityModel activity)
        {
            return new ActivityDto
            {
                Id = activity.Id,
                TripPlanId = activity.TripPlanId,
                Title = activity.Title,
                ActivityDate = activity.ActivityDate,
                ActivityTime = activity.ActivityTime,
                Location = activity.Location,
                Description = activity.Description,
                EstimatedCost = activity.EstimatedCost,
                Status = ParseStatus(activity.Status),
                CreatedAtUtc = activity.CreatedAt,
                UpdatedAtUtc = activity.UpdatedAt
            };
        }

        private static ActivityStatus ParseStatus(string status)
        {
            return Enum.TryParse<ActivityStatus>(status, ignoreCase: true, out var parsed)
                && Enum.IsDefined(typeof(ActivityStatus), parsed)
                    ? parsed
                    : ActivityStatus.Planned;
        }

        private static OperationResultDto Success(string message)
        {
            return new OperationResultDto
            {
                Succeeded = true,
                Message = message
            };
        }

        private static OperationResultDto Failure(string message)
        {
            return new OperationResultDto
            {
                Succeeded = false,
                Message = message
            };
        }

        private void LogDatabaseError(string message, Exception exception)
        {
            ServiceEventSource.Current.ServiceMessage(Context, "{0}: {1}", message, exception.Message);
        }
    }
}
