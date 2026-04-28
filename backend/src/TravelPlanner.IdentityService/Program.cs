using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.IdentityService;

await ServiceRuntime.RegisterServiceAsync("IdentityServiceType", context => new IdentityService(context));
await Task.Delay(Timeout.InfiniteTimeSpan);

