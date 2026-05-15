using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

[assembly: FabricTransportServiceRemotingProvider(
    RemotingClientVersion = RemotingClientVersion.V2,
    RemotingListenerVersion = RemotingListenerVersion.V2)]
