using Microsoft.ServiceFabric.Services.Remoting;
using TravelPlanner.Contracts.Sharing;

namespace TravelPlanner.Contracts.Interfaces;

public interface ISharingService : IService
{
    Task<ShareTokenDto> CreateShareAsync(CreateShareRequestDto request);

    Task<ShareTokenDto?> GetShareAsync(string token);

    Task<SharedTripPlanDto?> GetSharedTripPlanAsync(string token);
}
