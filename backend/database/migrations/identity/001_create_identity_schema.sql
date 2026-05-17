-- LEGACY SQL ALTERNATIVE
-- Primarni V7 tok su EF Core migracije u backend/TravelPlanner.Persistence/Migrations.

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Email NVARCHAR(320) NOT NULL,
        PasswordHash NVARCHAR(512) NOT NULL,
        CreatedAtUtc DATETIME2(7) NOT NULL,
        CONSTRAINT PK_Users PRIMARY KEY (UserId),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
END;
GO

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        RoleId INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        CONSTRAINT PK_Roles PRIMARY KEY (RoleId),
        CONSTRAINT UQ_Roles_Name UNIQUE (Name)
    );
END;
GO

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId INT NOT NULL,
        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (UserId)
            ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId)
            REFERENCES dbo.Roles (RoleId)
            ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'User')
BEGIN
    INSERT INTO dbo.Roles (Name) VALUES (N'User');
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Admin')
BEGIN
    INSERT INTO dbo.Roles (Name) VALUES (N'Admin');
END;
GO

DECLARE @BootstrapAdminEmail NVARCHAR(320) = N'admin@travelplanner.local';
DECLARE @BootstrapAdminUserId UNIQUEIDENTIFIER;
DECLARE @AdminRoleId INT;

SELECT @AdminRoleId = RoleId
FROM dbo.Roles
WHERE Name = N'Admin';

IF @AdminRoleId IS NOT NULL
    AND NOT EXISTS
    (
        SELECT 1
        FROM dbo.UserRoles userRole
        INNER JOIN dbo.Roles roleRecord ON roleRecord.RoleId = userRole.RoleId
        WHERE roleRecord.Name = N'Admin'
    )
BEGIN
    SELECT @BootstrapAdminUserId = UserId
    FROM dbo.Users
    WHERE Email = @BootstrapAdminEmail;

    IF @BootstrapAdminUserId IS NULL
    BEGIN
        SET @BootstrapAdminUserId = CAST(N'11111111-1111-1111-1111-111111111111' AS UNIQUEIDENTIFIER);

        INSERT INTO dbo.Users (UserId, Name, Email, PasswordHash, CreatedAtUtc)
        VALUES
        (
            @BootstrapAdminUserId,
            N'admin',
            @BootstrapAdminEmail,
            N'PBKDF2-SHA256$100000$QWRtaW5TZWVkU2FsdDEyMw==$wC8VScJnqu3wE4lxE2vuRCKf5+N12yg4803P6pamDi4=',
            SYSUTCDATETIME()
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.UserRoles
        WHERE UserId = @BootstrapAdminUserId
            AND RoleId = @AdminRoleId
    )
    BEGIN
        INSERT INTO dbo.UserRoles (UserId, RoleId)
        VALUES (@BootstrapAdminUserId, @AdminRoleId);
    END;
END;
GO
