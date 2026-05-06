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
        CONSTRAINT CK_Activities_EstimatedCost CHECK (EstimatedCost >= 0)
    );
END;
GO
