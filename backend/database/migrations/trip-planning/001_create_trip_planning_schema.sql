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
        CONSTRAINT CK_TripPlans_DateRange CHECK (EndDate >= StartDate),
        CONSTRAINT CK_TripPlans_PlannedBudget CHECK (PlannedBudget >= 0)
    );
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
