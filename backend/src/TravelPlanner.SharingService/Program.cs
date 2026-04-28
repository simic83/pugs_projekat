using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.SharingService;

await ServiceRuntime.RegisterServiceAsync("SharingServiceType", context => new SharingService(context));
await Task.Delay(Timeout.InfiniteTimeSpan);

