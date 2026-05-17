-- LEGACY SQL ALTERNATIVE
-- Primarni V7 tok je EF Core: backend/TravelPlanner.Persistence/Migrations.
-- Ne pokretati nad bazom koja je vec inicijalizovana EF Core migracijama.

IF DB_ID(N'TravelPlannerDb') IS NULL
BEGIN
    CREATE DATABASE TravelPlannerDb;
END
GO

USE TravelPlannerDb;
GO

-- 1. Identity
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

-- 2. Trip planning osnovne tabele
IF OBJECT_ID(N'dbo.TripPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TripPlans
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        OwnerUserId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(150) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NOT NULL,
        PlannedBudget DECIMAL(18,2) NOT NULL,
        Notes NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_TripPlans PRIMARY KEY (Id),
        CONSTRAINT FK_TripPlans_Users FOREIGN KEY (OwnerUserId)
            REFERENCES dbo.Users (UserId)
            ON DELETE CASCADE,
        CONSTRAINT CK_TripPlans_DatesRequired CHECK
            (StartDate > CONVERT(date, '00010101', 112) AND EndDate > CONVERT(date, '00010101', 112)),
        CONSTRAINT CK_TripPlans_DateRange CHECK (EndDate >= StartDate),
        CONSTRAINT CK_TripPlans_PlannedBudget CHECK (PlannedBudget >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.TripPlans', N'U') IS NOT NULL
    AND OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
    AND OBJECT_ID(N'dbo.FK_TripPlans_Users', N'F') IS NULL
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM dbo.TripPlans tripPlan
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM dbo.Users [user]
            WHERE [user].UserId = tripPlan.OwnerUserId
        )
    )
    BEGIN
        THROW 50001, 'Cannot add FK_TripPlans_Users because orphaned trip plans exist.', 1;
    END;

    ALTER TABLE dbo.TripPlans WITH CHECK
    ADD CONSTRAINT FK_TripPlans_Users FOREIGN KEY (OwnerUserId)
        REFERENCES dbo.Users (UserId)
        ON DELETE CASCADE;
END;
GO

IF OBJECT_ID(N'dbo.TripPlans', N'U') IS NOT NULL
    AND NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.TripPlans')
            AND name = N'IX_TripPlans_OwnerUserId'
    )
BEGIN
    CREATE INDEX IX_TripPlans_OwnerUserId ON dbo.TripPlans (OwnerUserId);
END;
GO

IF OBJECT_ID(N'dbo.Destinations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Destinations
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        TripPlanId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(150) NOT NULL,
        Location NVARCHAR(200) NULL,
        ArrivalDate DATE NOT NULL,
        DepartureDate DATE NOT NULL,
        Description NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_Destinations PRIMARY KEY (Id),
        CONSTRAINT FK_Destinations_TripPlans FOREIGN KEY (TripPlanId)
            REFERENCES dbo.TripPlans (Id)
            ON DELETE CASCADE,
        CONSTRAINT CK_Destinations_DatesRequired CHECK
            (ArrivalDate > CONVERT(date, '00010101', 112) AND DepartureDate > CONVERT(date, '00010101', 112)),
        CONSTRAINT CK_Destinations_DateRange CHECK (DepartureDate >= ArrivalDate)
    );
END;
GO

IF OBJECT_ID(N'dbo.Activities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Activities
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        TripPlanId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(150) NOT NULL,
        ActivityDate DATE NOT NULL,
        ActivityTime TIME NULL,
        Location NVARCHAR(200) NULL,
        Description NVARCHAR(MAX) NULL,
        EstimatedCost DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(30) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_Activities PRIMARY KEY (Id),
        CONSTRAINT FK_Activities_TripPlans FOREIGN KEY (TripPlanId)
            REFERENCES dbo.TripPlans (Id)
            ON DELETE CASCADE,
        CONSTRAINT CK_Activities_DateRequired CHECK (ActivityDate > CONVERT(date, '00010101', 112)),
        CONSTRAINT CK_Activities_EstimatedCost CHECK (EstimatedCost >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.TripPlans', N'U') IS NOT NULL
    AND OBJECT_ID(N'dbo.CK_TripPlans_DatesRequired', N'C') IS NULL
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM dbo.TripPlans
        WHERE StartDate <= CONVERT(date, '00010101', 112)
            OR EndDate <= CONVERT(date, '00010101', 112)
    )
    BEGIN
        THROW 50006, 'Cannot add CK_TripPlans_DatesRequired because trip plans with default dates exist.', 1;
    END;

    ALTER TABLE dbo.TripPlans WITH CHECK
    ADD CONSTRAINT CK_TripPlans_DatesRequired CHECK
        (StartDate > CONVERT(date, '00010101', 112) AND EndDate > CONVERT(date, '00010101', 112));
END;
GO

IF OBJECT_ID(N'dbo.Destinations', N'U') IS NOT NULL
    AND OBJECT_ID(N'dbo.CK_Destinations_DatesRequired', N'C') IS NULL
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM dbo.Destinations
        WHERE ArrivalDate <= CONVERT(date, '00010101', 112)
            OR DepartureDate <= CONVERT(date, '00010101', 112)
    )
    BEGIN
        THROW 50007, 'Cannot add CK_Destinations_DatesRequired because destinations with default dates exist.', 1;
    END;

    ALTER TABLE dbo.Destinations WITH CHECK
    ADD CONSTRAINT CK_Destinations_DatesRequired CHECK
        (ArrivalDate > CONVERT(date, '00010101', 112) AND DepartureDate > CONVERT(date, '00010101', 112));
END;
GO

IF OBJECT_ID(N'dbo.Activities', N'U') IS NOT NULL
    AND OBJECT_ID(N'dbo.CK_Activities_DateRequired', N'C') IS NULL
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM dbo.Activities
        WHERE ActivityDate <= CONVERT(date, '00010101', 112)
    )
    BEGIN
        THROW 50008, 'Cannot add CK_Activities_DateRequired because activities with default dates exist.', 1;
    END;

    ALTER TABLE dbo.Activities WITH CHECK
    ADD CONSTRAINT CK_Activities_DateRequired CHECK (ActivityDate > CONVERT(date, '00010101', 112));
END;
GO

-- 3. Checklist
IF OBJECT_ID(N'dbo.ChecklistItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChecklistItems
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        TripPlanId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(150) NOT NULL,
        IsCompleted BIT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_ChecklistItems PRIMARY KEY (Id),
        CONSTRAINT FK_ChecklistItems_TripPlans FOREIGN KEY (TripPlanId)
            REFERENCES dbo.TripPlans (Id)
            ON DELETE CASCADE
    );
END;
GO

-- 4. Notes
IF OBJECT_ID(N'dbo.Notes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notes
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        TripPlanId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(150) NOT NULL,
        Content NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_Notes PRIMARY KEY (Id),
        CONSTRAINT FK_Notes_TripPlans FOREIGN KEY (TripPlanId)
            REFERENCES dbo.TripPlans (Id)
            ON DELETE CASCADE
    );
END;
GO

-- 5. Reminders
IF OBJECT_ID(N'dbo.Reminders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reminders
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        TripPlanId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(150) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        ReminderAt DATETIME2 NOT NULL,
        IsCompleted BIT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_Reminders PRIMARY KEY (Id),
        CONSTRAINT FK_Reminders_TripPlans FOREIGN KEY (TripPlanId)
            REFERENCES dbo.TripPlans (Id)
            ON DELETE CASCADE
    );
END;
GO

-- 6. Budget
IF OBJECT_ID(N'dbo.Expenses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Expenses
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        TripPlanId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(150) NOT NULL,
        Category NVARCHAR(50) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        ExpenseDate DATE NOT NULL,
        Description NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_Expenses PRIMARY KEY (Id),
        CONSTRAINT FK_Expenses_TripPlans FOREIGN KEY (TripPlanId)
            REFERENCES dbo.TripPlans (Id)
            ON DELETE CASCADE,
        CONSTRAINT CK_Expenses_Amount CHECK (Amount >= 0)
    );
END;
GO

-- 7. Sharing
IF OBJECT_ID(N'dbo.ShareTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShareTokens
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        TripPlanId UNIQUEIDENTIFIER NOT NULL,
        Token NVARCHAR(200) NOT NULL,
        AccessLevel NVARCHAR(20) NOT NULL,
        CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        ExpiresAt DATETIME2 NULL,
        IsRevoked BIT NOT NULL,
        CONSTRAINT PK_ShareTokens PRIMARY KEY (Id),
        CONSTRAINT FK_ShareTokens_TripPlans FOREIGN KEY (TripPlanId)
            REFERENCES dbo.TripPlans (Id)
            ON DELETE CASCADE,
        CONSTRAINT CK_ShareTokens_AccessLevel CHECK (AccessLevel IN (N'VIEW', N'EDIT')),
        CONSTRAINT UQ_ShareTokens_Token UNIQUE (Token)
    );
END;
GO
