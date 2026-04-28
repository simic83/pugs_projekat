using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.TripPlanningService;

await ServiceRuntime.RegisterServiceAsync("TripPlanningServiceType", context => new TripPlanningService(context));
await Task.Delay(Timeout.InfiniteTimeSpan);

