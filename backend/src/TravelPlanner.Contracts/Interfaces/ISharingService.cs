using Microsoft.ServiceFabric.Services.Remoting;
using TravelPlanner.Contracts.DTOs.Sharing;

namespace TravelPlanner.Contracts.Interfaces;

public interface ISharingService : IService
{
    Task<ShareAccessDto> CreateShareAccessAsync(ShareRequestDto request);

    Task<ShareAccessDto?> ValidateShareTokenAsync(Guid tripPlanId, string token);

    Task DeleteShareAccessForTripPlanAsync(Guid tripPlanId);
}

