using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.BudgetService;

await ServiceRuntime.RegisterServiceAsync("BudgetServiceType", context => new BudgetService(context));
await Task.Delay(Timeout.InfiniteTimeSpan);

