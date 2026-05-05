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
