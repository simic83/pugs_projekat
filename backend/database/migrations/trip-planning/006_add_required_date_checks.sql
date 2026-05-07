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
