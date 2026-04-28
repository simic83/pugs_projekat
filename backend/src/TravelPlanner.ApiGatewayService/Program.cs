using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.ApiGatewayService;

await ServiceRuntime.RegisterServiceAsync("ApiGatewayServiceType", context => new ApiGatewayService(context));
await Task.Delay(Timeout.InfiniteTimeSpan);

