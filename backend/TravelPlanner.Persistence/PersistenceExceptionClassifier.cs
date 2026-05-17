using Microsoft.EntityFrameworkCore;

namespace TravelPlanner.Persistence;

public static class PersistenceExceptionClassifier
{
    private const string SqlServerExceptionFullName = "Microsoft.Data.SqlClient.SqlException";

    public static bool IsPersistenceException(Exception exception)
    {
        return exception is InvalidOperationException or DbUpdateException
            || HasSqlServerProviderException(exception);
    }

    private static bool HasSqlServerProviderException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (string.Equals(current.GetType().FullName, SqlServerExceptionFullName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
