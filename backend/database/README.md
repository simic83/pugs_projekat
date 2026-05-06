# Database

Projekat koristi Microsoft SQL Server bazu:

- `TravelPlannerDb`

Service Fabric connection string za lokalni development treba da pokazuje na ovu bazu, na primer:

```text
Server=localhost;Database=TravelPlannerDb;Trusted_Connection=True;TrustServerCertificate=True;
```

## Kreiranje baze

Ako treba samo kreirati bazu, otvoriti i pokrenuti:

```text
backend/database/00_create_database.sql
```

Skripta proverava da li baza `TravelPlannerDb` postoji, kreira je ako ne postoji i zatim prebacuje SQL kontekst na tu bazu.

## Sve migracije odjednom

Za lokalni development najjednostavnije je otvoriti u SQL Server Management Studio i pokrenuti:

```text
backend/database/01_run_all_migrations.sql
```

Ova skripta:

- kreira bazu `TravelPlannerDb` ako ne postoji,
- pokrece `USE TravelPlannerDb`,
- kreira sve tabele redom,
- seed-uje role `User` i `Admin` bez dupliranja.

## Pojedinacne migracije

Ako se migracije pokrecu pojedinacno, redosled je:

1. `backend/database/migrations/identity/001_create_identity_schema.sql`
2. `backend/database/migrations/trip-planning/001_create_trip_planning_schema.sql`
3. `backend/database/migrations/trip-planning/002_create_checklist_items.sql`
4. `backend/database/migrations/trip-planning/003_create_notes.sql`
5. `backend/database/migrations/trip-planning/004_create_reminders.sql`
6. `backend/database/migrations/trip-planning/005_add_trip_plan_owner_cascade.sql`
7. `backend/database/migrations/budget/001_create_budget_schema.sql`
8. `backend/database/migrations/sharing/001_create_share_tokens.sql`

SQL Server Management Studio ne pokrece automatski druge `.sql` fajlove iz foldera. Ako se ne koristi objedinjena skripta, svaki migracioni fajl treba rucno otvoriti i pokrenuti navedenim redosledom.

## Napomena za Service Fabric

Parametri u Service Fabric konfiguraciji treba da koriste connection string ciji je `Database=TravelPlannerDb`. Ako svi servisi koriste jednu lokalnu bazu, isti connection string moze biti postavljen za identity, trip planning, budget i sharing servis.
