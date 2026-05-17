-- LEGACY SQL ALTERNATIVE
-- Primarni V7 tok je EF Core: dotnet dotnet-ef database update.
-- Ova skripta postoji kao arhiva/rucni fallback kada se ne koristi EF Core tok.

IF DB_ID(N'TravelPlannerDb') IS NULL
BEGIN
    CREATE DATABASE TravelPlannerDb;
END
GO

USE TravelPlannerDb;
GO
