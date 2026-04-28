namespace TravelPlanner.Common.ServiceFabric;

public static class ServiceNames
{
    public const string ApplicationName = "fabric:/TravelPlanner";
    public const string ApiGatewayServiceName = "ApiGatewayService";
    public const string IdentityServiceName = "IdentityService";
    public const string TripPlanningServiceName = "TripPlanningService";
    public const string BudgetServiceName = "BudgetService";
    public const string SharingServiceName = "SharingService";

    public static readonly Uri IdentityServiceUri = new($"{ApplicationName}/{IdentityServiceName}");
    public static readonly Uri TripPlanningServiceUri = new($"{ApplicationName}/{TripPlanningServiceName}");
    public static readonly Uri BudgetServiceUri = new($"{ApplicationName}/{BudgetServiceName}");
    public static readonly Uri SharingServiceUri = new($"{ApplicationName}/{SharingServiceName}");
}

