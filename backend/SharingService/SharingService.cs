using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Contracts.Interfaces;
using TravelPlanner.Contracts.Sharing;

namespace SharingService
{
    internal sealed class SharingService : StatefulService, ISharingService
    {
        public SharingService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public Task<ShareTokenDto> CreateShareAsync(CreateShareRequestDto request)
        {
            var shareToken = new ShareTokenDto
            {
                Token = Guid.NewGuid().ToString("N"),
                TripPlanId = request.TripPlanId,
                AccessLevel = request.AccessLevel,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = request.ExpiresAtUtc
            };

            return Task.FromResult(shareToken);
        }

        public Task<ShareTokenDto?> GetShareAsync(string token)
        {
            return Task.FromResult<ShareTokenDto?>(null);
        }

        public Task<SharedTripPlanDto?> GetSharedTripPlanAsync(string token)
        {
            return Task.FromResult<SharedTripPlanDto?>(null);
        }
    }
}
