namespace ApiGatewayService.Configuration;

internal static class ServiceNames
{
    public static readonly Uri IdentityServiceUri = new("fabric:/TravelPlanner/IdentityService");

    public static readonly Uri TripPlanningServiceUri = new("fabric:/TravelPlanner/TripPlanningService");

    public static readonly Uri BudgetServiceUri = new("fabric:/TravelPlanner/BudgetService");

    public static readonly Uri SharingServiceUri = new("fabric:/TravelPlanner/SharingService");
}
