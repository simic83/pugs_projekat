using Microsoft.ServiceFabric.Services.Remoting;
using TravelPlanner.Contracts.Common;
using TravelPlanner.Contracts.Sharing;
using TravelPlanner.Contracts.Trips;

namespace TravelPlanner.Contracts.Interfaces;

public interface ISharingService : IService
{
    Task<ShareTokenDto?> CreateShareAsync(CreateShareRequestDto request);

    Task<ShareTokenDto?> GetShareAsync(string token);

    Task<List<ShareTokenDto>> GetSharesByTripPlanAsync(Guid tripPlanId, Guid userId);

    Task<OperationResultDto> RevokeShareAsync(Guid tripPlanId, Guid shareId, Guid userId);

    Task<SharedTripPlanDto?> GetSharedTripPlanAsync(string token);

    Task<SharedTripPlanDto?> UpdateSharedTripPlanAsync(string token, UpdateTripPlanRequestDto request);
}
