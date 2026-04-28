IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(512) NOT NULL,
        CONSTRAINT PK_Users PRIMARY KEY (Id),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
END;

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_Roles PRIMARY KEY (Id),
        CONSTRAINT UQ_Roles_Name UNIQUE (Name)
    );
END;

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users_UserId
            FOREIGN KEY (UserId)
            REFERENCES dbo.Users (Id)
            ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles_RoleId
            FOREIGN KEY (RoleId)
            REFERENCES dbo.Roles (Id)
            ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.foreign_keys
       WHERE name = N'FK_UserRoles_Users_UserId'
   )
BEGIN
    ALTER TABLE dbo.UserRoles
    ADD CONSTRAINT FK_UserRoles_Users_UserId
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users (Id)
        ON DELETE CASCADE;
END;

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.foreign_keys
       WHERE name = N'FK_UserRoles_Roles_RoleId'
   )
BEGIN
    ALTER TABLE dbo.UserRoles
    ADD CONSTRAINT FK_UserRoles_Roles_RoleId
        FOREIGN KEY (RoleId)
        REFERENCES dbo.Roles (Id)
        ON DELETE CASCADE;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'User')
BEGIN
    INSERT INTO dbo.Roles (Id, Name)
    VALUES ('11111111-1111-1111-1111-111111111111', N'User');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Admin')
BEGIN
    INSERT INTO dbo.Roles (Id, Name)
    VALUES ('22222222-2222-2222-2222-222222222222', N'Admin');
END;
