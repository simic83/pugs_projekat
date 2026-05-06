# Travel Planner

## Opis projekta

Travel Planner je web aplikacija za planiranje putovanja. Projekat je radjen kao studentski PUGS projekat, uz Microsoft Service Fabric backend i React frontend.

Aplikacija omogucava:

- registraciju i login korisnika,
- kreiranje planova putovanja,
- upravljanje destinacijama,
- upravljanje aktivnostima,
- vodjenje troskova i budzeta,
- checklist / packing listu,
- deljenje plana preko linka/tokena,
- VIEW i EDIT pristup deljenom planu.

## Tehnologije

- React frontend
- ASP.NET Core Web API u `ApiGatewayService`
- Microsoft Service Fabric
- Service Fabric Remoting
- Microsoft SQL Server
- Microsoft.Data.SqlClient
- JWT autentifikacija

## Struktura projekta

- `backend/` - Service Fabric solution, backend servisi, remoting contracts i SQL migracije.
- `frontend/` - React aplikacija sa stranicama, API servisima, kontekstima, modelima i stilovima.
- `docs/` - dokumentacija za predaju projekta.

Najbitniji backend delovi:

- `backend/ApiGatewayService` - HTTP ulaz za frontend i ASP.NET Core kontroleri.
- `backend/IdentityService` - registracija, login, JWT i korisnici.
- `backend/TripPlanningService` - planovi putovanja, destinacije, aktivnosti i checklist.
- `backend/BudgetService` - troskovi i budzet.
- `backend/SharingService` - share tokeni i VIEW/EDIT pristup.
- `backend/Contracts` - DTO klase i Service Fabric Remoting interfejsi.
- `backend/database/migrations` - SQL skripte za kreiranje tabela.
- `backend/TravelPlanner` - Service Fabric application project.

Najbitniji frontend delovi:

- `frontend/src/api` - HTTP pozivi ka backend API-ju.
- `frontend/src/pages` - stranice za login, registraciju, planove i deljeni plan.
- `frontend/src/context` - React context za autentifikaciju i osnovni app context.
- `frontend/src/styles` - globalni stilovi i bela/plava tema.
- `frontend/src/models` - frontend konstante i pomocni modeli.

## Mikroservisi

| Servis | Uloga |
| --- | --- |
| `ApiGatewayService` | HTTP ulaz za frontend. Validira JWT i poziva unutrasnje servise preko Service Fabric Remoting-a. |
| `IdentityService` | Registracija, login, hashovanje lozinki, JWT tokeni, korisnici i uloge. |
| `TripPlanningService` | Planovi putovanja, destinacije, aktivnosti i checklist stavke. |
| `BudgetService` | Troskovi putovanja i obracun ukupnog/preostalog budzeta. |
| `SharingService` | Kreiranje, pregled i opoziv share tokena, kao i VIEW/EDIT pristup deljenom planu. |

## Implementirane funkcionalnosti

- Auth: registracija, login i JWT autentifikacija.
- Uloge: `User` i `Admin`.
- Planovi putovanja: kreiranje, pregled, izmena i brisanje.
- Destinacije: dodavanje, pregled, izmena i brisanje.
- Aktivnosti: dodavanje, pregled, izmena i brisanje.
- Troskovi i budzet: troskovi po planu i summary budzeta.
- Checklist: dodavanje, cekiranje, izmena i brisanje stavki.
- Beleske: dodavanje, pregled, izmena i brisanje beleski za vlasnika plana.
- Deljenje plana: token/link, VIEW i EDIT pristup, opoziv tokena.
- Frontend stranice: login, register, moji planovi i javni shared prikaz.
- Osnovne validacije na backendu i frontend formama.
- Ownership provera: korisnik vidi i menja samo svoje planove i povezane podatke.

## Funkcionalnosti koje nisu radjene

- LDAP/SSO nije radjen.
- Prikaz rute na mapi nije radjen.
- Calendar view nije uradjen kao pravi kalendarski prikaz. Aktivnosti se prikazuju kao lista/grupisanje po datumu.
- Admin funkcionalnosti su minimalne. Backend ima endpoint za listu korisnika za Admin rolu, ali nema posebnog kompletnog admin panela.

## Podesavanje baze

SQL migracije se nalaze u:

- `backend/database/migrations/identity`
- `backend/database/migrations/trip-planning`
- `backend/database/migrations/budget`
- `backend/database/migrations/sharing`

Za lokalni development moze se koristiti objedinjena skripta:

- `backend/database/01_run_all_migrations.sql`

Ona kreira bazu `TravelPlannerDb` ako ne postoji i pokrece sve migracije redom. Ako treba samo kreirati bazu, pokrenuti:

- `backend/database/00_create_database.sql`

Preporuceni redosled pokretanja migracija:

1. `backend/database/migrations/identity/001_create_identity_schema.sql`
2. `backend/database/migrations/trip-planning/001_create_trip_planning_schema.sql`
3. `backend/database/migrations/trip-planning/002_create_checklist_items.sql`
4. `backend/database/migrations/trip-planning/003_create_notes.sql`
5. `backend/database/migrations/budget/001_create_budget_schema.sql`
6. `backend/database/migrations/sharing/001_create_share_tokens.sql`

Migracije se pokrecu nad Microsoft SQL Server bazom. Budget i sharing tabele imaju veze ka `TripPlans`, zato se trip-planning migracije pokrecu pre njih.

## Podesavanje konfiguracije

Za lokalno pokretanje treba podesiti:

- connection string,
- JWT Secret,
- JWT Issuer,
- JWT Audience,
- JWT expiration.

Service Fabric application manifest je:

- `backend/TravelPlanner/ApplicationPackageRoot/ApplicationManifest.xml`

Service Fabric parameter fajlovi su:

- `backend/TravelPlanner/ApplicationParameters/Local.1Node.xml`
- `backend/TravelPlanner/ApplicationParameters/Local.5Node.xml`
- `backend/TravelPlanner/ApplicationParameters/Cloud.xml`

Service `Settings.xml` fajlovi su:

- `backend/IdentityService/PackageRoot/Config/Settings.xml` - `ConnectionStrings/DefaultConnection`, `Jwt/Secret`, `Jwt/Issuer`, `Jwt/Audience`, `Jwt/ExpirationMinutes`.
- `backend/ApiGatewayService/PackageRoot/Config/Settings.xml` - `Jwt/Secret`, `Jwt/Issuer`, `Jwt/Audience`, `Authentication/AllowDevUserHeaderFallback`.
- `backend/TripPlanningService/PackageRoot/Config/Settings.xml` - `ConnectionStrings/DefaultConnection`.
- `backend/BudgetService/PackageRoot/Config/Settings.xml` - `ConnectionStrings/DefaultConnection`.
- `backend/SharingService/PackageRoot/Config/Settings.xml` - `ConnectionStrings/DefaultConnection`.

`ApplicationManifest.xml` mapira ove parametre u `Settings.xml`:

- `Identity_DefaultConnection`
- `Identity_JwtSecret`
- `Identity_JwtIssuer`
- `Identity_JwtAudience`
- `Identity_JwtExpirationMinutes`
- `ApiGateway_JwtSecret`
- `ApiGateway_JwtIssuer`
- `ApiGateway_JwtAudience`
- `ApiGateway_AllowDevUserHeaderFallback`
- `TripPlanning_DefaultConnection`
- `Budget_DefaultConnection`
- `Sharing_DefaultConnection`

Za lokalni rad, `Local.1Node.xml` i `Local.5Node.xml` trenutno koriste isti primer SQL connection string-a za sve module:

```text
Server=localhost;Database=TravelPlannerDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Ako koristite vise baza, podesite odvojene vrednosti za `Identity_DefaultConnection`, `TripPlanning_DefaultConnection`, `Budget_DefaultConnection` i `Sharing_DefaultConnection`. Ako koristite jednu bazu, isti connection string je dovoljan za sve servise.

JWT vrednosti u `IdentityService` i `ApiGatewayService` moraju biti iste, jer `IdentityService` izdaje token, a `ApiGatewayService` ga validira. Konkretno, uskladiti:

- `Identity_JwtSecret` i `ApiGateway_JwtSecret`
- `Identity_JwtIssuer` i `ApiGateway_JwtIssuer`
- `Identity_JwtAudience` i `ApiGateway_JwtAudience`

JWT Secret mora imati najmanje 32 karaktera/bajta. Pravi secret se ne cuva u repozitorijumu; vrednosti `CHANGE_ME_...` su samo placeholder-i i treba ih zameniti lokalnim development vrednostima pre pokretanja.

`ApiGateway_AllowDevUserHeaderFallback` je podesen na `false` i treba da ostane `false` za normalan lokalni/proizvodni rad.

## Pokretanje backend-a

Studentsko lokalno pokretanje:

1. Otvoriti `backend/TravelPlanner.sln` u Visual Studio.
2. Pokrenuti Visual Studio kao Administrator ako Service Fabric local cluster to zahteva.
3. Proveriti da je Service Fabric Local Cluster pokrenut.
4. Pokrenuti SQL migracije iz sekcije "Podesavanje baze".
5. Podesiti SQL Server connection string u konfiguraciji servisa.
6. Podesiti JWT secret, issuer, audience i expiration.
7. Deploy/pokrenuti Service Fabric aplikaciju `TravelPlanner` iz Visual Studio okruzenja.

## Pokretanje frontend-a

U terminalu:

```powershell
cd frontend
npm install
npm run dev
```

`npm install` je potreban samo ako `node_modules` ne postoji ili zavisnosti nisu instalirane.

Frontend koristi Vite `.env` podesavanje. Primer postoji u:

- `frontend/.env.example`

Potrebno je napraviti lokalni `.env` prema primeru i podesiti:

```env
VITE_API_BASE_URL=http://localhost:8080
```

Ako lokalni Service Fabric cluster dodeli drugi port za `ApiGatewayService` endpoint, uskladiti port u `VITE_API_BASE_URL` sa stvarnim ApiGateway HTTP endpoint-om.

Za proveru produkcionog build-a:

```powershell
cd frontend
npm run build
```

## Test nalozi

U migracijama postoje seed vrednosti za role `User` i `Admin`, ali ne postoje seed korisnici. Korisnik se kreira kroz Register formu. Registracija novom korisniku dodeljuje `User` rolu.

## Napomene

- Backend URL mora biti upisan u `VITE_API_BASE_URL`.
- HTTP pozivi su organizovani u `frontend/src/api` servisima.
- Lozinke se cuvaju hash-ovane.
- JWT mora imati validan secret.
- Deljeni VIEW link sluzi za pregled plana.
- Deljeni EDIT link omogucava izmenu osnovnog plana, destinacija, aktivnosti, troskova, checklist stavki i beleski.
- QR kod je dostupan za share linkove.
- Mapa/ruta i pravi calendar view nisu deo trenutne implementacije.
