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
