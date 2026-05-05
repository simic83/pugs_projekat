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
