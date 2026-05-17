# TravelPlanner.Persistence

Ovaj projekat sadrzi EF Core model baze:

- `TravelPlannerDbContext`
- entity klase u `Entities`
- EF Core migracije u `Migrations`

IdentityService, TripPlanningService, BudgetService i SharingService koriste ovaj projekat kroz EF Core. SQL Server provider i migracije ostaju centralizovani ovdje, dok servisni projekti zavise od opsteg EF Core API-ja.

## Komande

Iz root foldera repozitorijuma:

```powershell
dotnet tool restore
```

Primjena migracija:

```powershell
dotnet dotnet-ef database update `
  --project backend/TravelPlanner.Persistence `
  --startup-project backend/TravelPlanner.Persistence `
  --context TravelPlannerDbContext
```

Lista dostupnih migracija:

```powershell
dotnet dotnet-ef migrations list `
  --project backend/TravelPlanner.Persistence `
  --startup-project backend/TravelPlanner.Persistence `
  --context TravelPlannerDbContext
```

Dodavanje nove migracije:

```powershell
dotnet dotnet-ef migrations add ImeMigracije `
  --project backend/TravelPlanner.Persistence `
  --startup-project backend/TravelPlanner.Persistence `
  --context TravelPlannerDbContext `
  --output-dir Migrations
```

Generisanje idempotentne SQL skripte:

```powershell
dotnet dotnet-ef migrations script --idempotent `
  --project backend/TravelPlanner.Persistence `
  --startup-project backend/TravelPlanner.Persistence `
  --context TravelPlannerDbContext
```

Ako lokalni SQL Server nije `SQLEXPRESS`, postaviti connection string prije komande:

```powershell
$env:TRAVELPLANNER_CONNECTION_STRING="Server=localhost;Database=TravelPlannerDb;Trusted_Connection=True;TrustServerCertificate=True;"
```
