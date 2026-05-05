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
