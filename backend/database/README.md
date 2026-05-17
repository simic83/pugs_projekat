# Database Legacy SQL

Projekat koristi Microsoft SQL Server bazu:

- `TravelPlannerDb`

Primarni V7/EF Core tok je sada u projektu `backend/TravelPlanner.Persistence`, kroz `TravelPlannerDbContext` i folder `Migrations`.

Ovaj folder je legacy SQL arhiva i rucna alternativa. Ne pokretati ove skripte nad bazom koja je vec inicijalizovana EF Core migracijama, jer EF i legacy SQL kreiraju istu semu.

Ako EF Core `database update` prijavi da tabela kao `Roles` vec postoji, baza je vjerovatno nastala iz ovih legacy skripti. Za cist EF start koristiti novu/praznu lokalnu bazu ili svjesno ukloniti staru lokalnu bazu prije EF migracija.

Service Fabric connection string za lokalni development treba da pokazuje na ovu bazu, na primer:

```text
Server=localhost;Database=TravelPlannerDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Ako je SQL Server instaliran kao named instance `SQLEXPRESS`, koristi:

```text
Server=localhost\SQLEXPRESS;Database=TravelPlannerDb;Trusted_Connection=True;TrustServerCertificate=True;
```

## Kreiranje baze bez EF-a

Ako se namjerno ne koristi EF Core tok i treba samo rucno kreirati bazu, otvoriti i pokrenuti:

```text
backend/database/00_create_database.sql
```

Skripta proverava da li baza `TravelPlannerDb` postoji, kreira je ako ne postoji i zatim prebacuje SQL kontekst na tu bazu.

## Sve legacy migracije odjednom

Ako se namjerno koristi legacy SQL fallback, najjednostavnije je otvoriti u SQL Server Management Studio i pokrenuti:

```text
backend/database/01_run_all_migrations.sql
```

Ova skripta:

- kreira bazu `TravelPlannerDb` ako ne postoji,
- pokrece `USE TravelPlannerDb`,
- kreira sve tabele redom,
- seed-uje role `User` i `Admin` bez dupliranja,
- kreira prvi admin nalog ako ne postoji nijedan admin u bazi.

Podrazumevani bootstrap admin za cistu instalaciju:

```text
login: admin
email: admin@travelplanner.local
lozinka: admin123
```

## Pojedinacne legacy migracije

Ako se legacy migracije pokrecu pojedinacno, redosled je:

1. `backend/database/migrations/identity/001_create_identity_schema.sql`
2. `backend/database/migrations/trip-planning/001_create_trip_planning_schema.sql`
3. `backend/database/migrations/trip-planning/002_create_checklist_items.sql`
4. `backend/database/migrations/trip-planning/003_create_notes.sql`
5. `backend/database/migrations/trip-planning/004_create_reminders.sql`
6. `backend/database/migrations/trip-planning/005_add_trip_plan_owner_cascade.sql`
7. `backend/database/migrations/trip-planning/006_add_required_date_checks.sql`
8. `backend/database/migrations/budget/001_create_budget_schema.sql`
9. `backend/database/migrations/sharing/001_create_share_tokens.sql`

SQL Server Management Studio ne pokrece automatski druge `.sql` fajlove iz foldera. Ako se ne koristi objedinjena skripta, svaki migracioni fajl treba rucno otvoriti i pokrenuti navedenim redosledom.

## Napomena za Service Fabric

Parametri u Service Fabric konfiguraciji treba da koriste connection string ciji je `Database=TravelPlannerDb`. Ako svi servisi koriste jednu lokalnu bazu, isti connection string moze biti postavljen za identity, trip planning, budget i sharing servis.

Lokalni Service Fabric servisi se cesto konektuju kao `NT AUTHORITY\NETWORK SERVICE`. Ako se koristi Windows authentication i API vraca login gresku za taj nalog, dodati mu pristup bazi:

```text
backend/database/02_grant_service_fabric_network_service.sql
```

```sql
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'NT AUTHORITY\NETWORK SERVICE')
    CREATE LOGIN [NT AUTHORITY\NETWORK SERVICE] FROM WINDOWS;

USE TravelPlannerDb;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'NT AUTHORITY\NETWORK SERVICE')
    CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE];

ALTER ROLE db_datareader ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
ALTER ROLE db_datawriter ADD MEMBER [NT AUTHORITY\NETWORK SERVICE];
```
