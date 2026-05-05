using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace ApiGatewayService.Infrastructure;

internal static class GatewayServiceProxyFactory
{
    private static readonly ServicePartitionKey DefaultStatefulPartitionKey = new(0);

    public static T CreateStateless<T>(Uri serviceUri)
        where T : IService
    {
        return ServiceProxy.Create<T>(serviceUri);
    }

    public static T CreateStateful<T>(Uri serviceUri)
        where T : IService
    {
        return ServiceProxy.Create<T>(serviceUri, DefaultStatefulPartitionKey);
    }
}
