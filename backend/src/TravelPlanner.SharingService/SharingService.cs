using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Contracts.DTOs.Sharing;
using TravelPlanner.Contracts.Interfaces;

namespace TravelPlanner.SharingService;

internal sealed class SharingService : StatefulService, ISharingService
{
    public SharingService(StatefulServiceContext context)
        : base(context)
    {
    }

    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        return this.CreateServiceRemotingReplicaListeners();
    }

    public Task<ShareAccessDto> CreateShareAccessAsync(ShareRequestDto request)
    {
        throw new NotImplementedException("Scaffold only. Share token creation is not implemented yet.");
    }

    public Task<ShareAccessDto?> ValidateShareTokenAsync(Guid tripPlanId, string token)
    {
        throw new NotImplementedException("Scaffold only. Share token signature and expiration validation are not implemented yet.");
    }

    public Task DeleteShareAccessForTripPlanAsync(Guid tripPlanId)
    {
        throw new NotImplementedException("Scaffold only. Share token cleanup is not implemented yet.");
    }
}
