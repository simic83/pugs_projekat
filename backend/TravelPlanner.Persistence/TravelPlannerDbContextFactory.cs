using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TravelPlanner.Persistence;

public sealed class TravelPlannerDbContextFactory : IDesignTimeDbContextFactory<TravelPlannerDbContext>
{
    private const string DefaultLocalConnection =
        "Server=localhost\\SQLEXPRESS;Database=TravelPlannerDb;Trusted_Connection=True;TrustServerCertificate=True;";

    public TravelPlannerDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("TRAVELPLANNER_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DefaultLocalConnection;
        }

        var options = new DbContextOptionsBuilder<TravelPlannerDbContext>()
            .UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.EnableRetryOnFailure())
            .Options;

        return new TravelPlannerDbContext(options);
    }
}
