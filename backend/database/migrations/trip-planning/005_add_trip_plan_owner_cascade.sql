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
