-- Grants local Service Fabric services access to the TravelPlannerDb database
-- when Windows authentication is used.
--
-- Local Service Fabric usually runs application services as
-- NT AUTHORITY\NETWORK SERVICE. Without a database user for that login,
-- API calls fail with SqlServerRetryingExecutionStrategy after several retries.

USE [master];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.server_principals
    WHERE name = N'NT AUTHORITY\NETWORK SERVICE'
)
BEGIN
    CREATE LOGIN [NT AUTHORITY\NETWORK SERVICE] FROM WINDOWS;
END;
GO

USE [TravelPlannerDb];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_principals
    WHERE name = N'NT AUTHORITY\NETWORK SERVICE'
)
BEGIN
    CREATE USER [NT AUTHORITY\NETWORK SERVICE]
    FOR LOGIN [NT AUTHORITY\NETWORK SERVICE];
END;
GO

IF IS_ROLEMEMBER(N'db_datareader', N'NT AUTHORITY\NETWORK SERVICE') <> 1
BEGIN
    ALTER ROLE db_datareader ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
END;
GO

IF IS_ROLEMEMBER(N'db_datawriter', N'NT AUTHORITY\NETWORK SERVICE') <> 1
BEGIN
    ALTER ROLE db_datawriter ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
END;
GO
