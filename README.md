<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=0:0EA5E9,50:22C55E,100:F59E0B&height=190&section=header&text=Travel%20Planner&fontColor=FFFFFF&fontSize=48&fontAlignY=36&desc=Web%20aplikacija%20za%20planiranje%20putovanja&descAlignY=58&animation=fadeIn" alt="Travel Planner animated header" />
</p>

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Inter&weight=600&size=20&duration=2800&pause=900&color=0EA5E9&center=true&vCenter=true&width=820&lines=Planovi+putovanja+na+jednom+mjestu;Destinacije%2C+aktivnosti%2C+troskovi+i+budzet;Checklist%2C+biljeske%2C+podsjetnici+i+QR+dijeljenje" alt="Animated Travel Planner description" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=111827" alt="React 19" />
  <img src="https://img.shields.io/badge/Vite-7-646CFF?style=for-the-badge&logo=vite&logoColor=FFFFFF" alt="Vite 7" />
  <img src="https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=FFFFFF" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Service%20Fabric-Microservices-0EA5E9?style=for-the-badge" alt="Microsoft Service Fabric" />
  <img src="https://img.shields.io/badge/SQL%20Server-TravelPlannerDb-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=FFFFFF" alt="SQL Server" />
</p>

---

# Travel Planner

Travel Planner je web aplikacija za planiranje putovanja. Projekat je radjen za predmet **Primena veb programiranja u infrastrukturnim sistemima**.

Ideja aplikacije je da korisnik na jednom mjestu moze da napravi plan putovanja i uz njega vodi destinacije, aktivnosti, troskove, checklistu, biljeske i podsjetnike. Plan se moze podijeliti preko linka ili QR koda.

## Funkcionalnosti

- registracija i logovanje korisnika
- kreiranje, izmjena i brisanje planova putovanja
- dodavanje destinacija za jedno putovanje
- organizacija aktivnosti po danima
- pregled aktivnosti kroz kalendar
- evidencija troskova i pracenje budzeta
- checklist stavke za putovanje
- biljeske i podsjetnici
- dijeljenje plana preko linka i QR koda
- admin pregled korisnika i sadrzaja

Use case dijagram je dodat u fajlu:

![Use case dijagram](usecase.png)

## Arhitektura

Aplikacija je podijeljena na frontend, backend servise i bazu podataka.

- **Frontend** je React aplikacija. Sluzi za korisnicki interfejs i salje zahtjeve backend-u.
- **Backend** je napravljen u .NET-u kroz Service Fabric servise. Tu se nalazi poslovna logika, validacija i rad sa podacima.
- **SQL Server** cuva podatke aplikacije.

Frontend ne pristupa bazi direktno. Komunikacija ide ovako:

```text
React frontend -> ApiGatewayService -> interni servisi -> SQL Server
```

Backend servisi:

- `ApiGatewayService` - prima HTTP zahtjeve sa frontenda
- `IdentityService` - registracija, login, korisnici i role
- `TripPlanningService` - planovi, destinacije, aktivnosti, checklist, biljeske i podsjetnici
- `BudgetService` - troskovi i budzet
- `SharingService` - dijeljenje plana preko tokena/linka

## Baza podataka

Za bazu se koristi SQL Server. Entity Framework Core je koristen za `DbContext` i migracije.

Persistence sloj je izdvojen u:

```text
backend/TravelPlanner.Persistence
```

U tom projektu se nalaze entity klase, `TravelPlannerDbContext` i migracije. Connection string se podesava u Service Fabric parametrima, npr. u:

```text
backend/TravelPlanner/ApplicationParameters/Local.1Node.xml
backend/TravelPlanner/ApplicationParameters/Local.5Node.xml
```

## Struktura projekta

```text
pugs_projekat/
|-- backend/
|   |-- ApiGatewayService/
|   |-- IdentityService/
|   |-- TripPlanningService/
|   |-- BudgetService/
|   |-- SharingService/
|   |-- Contracts/
|   |-- TravelPlanner.Persistence/
|   |-- TravelPlanner/
|   `-- TravelPlanner.sln
|
|-- frontend/
|   |-- src/
|   |-- package.json
|   `-- .env
|
|-- usecase.png
`-- README.md
```

## Pokretanje projekta

Potrebno je imati Visual Studio sa Service Fabric alatima, .NET 8 SDK, SQL Server, Node.js i npm.

Prvo pokrenuti SQL Server i primijeniti migracije:

```powershell
dotnet tool restore
dotnet dotnet-ef database update --project backend/TravelPlanner.Persistence --startup-project backend/TravelPlanner.Persistence --context TravelPlannerDbContext
```

Zatim otvoriti backend solution:

```text
backend/TravelPlanner.sln
```

U `Local.1Node.xml` ili `Local.5Node.xml` provjeriti connection string i JWT vrijednosti, pa pokrenuti Service Fabric aplikaciju iz Visual Studio okruzenja.

Frontend se pokrece iz foldera `frontend`:

```powershell
npm install
npm run dev
```

Frontend se lokalno otvara na:

```text
http://localhost:5173
```

Backend adresa za frontend je u fajlu:

```text
frontend/.env
```

## Test nalog

Migracije dodaju osnovne role. Ako u bazi ne postoji admin, kreira se pocetni admin nalog:

```text
login: admin
email: admin@travelplanner.local
lozinka: admin123
```

Za stvarnu upotrebu ove vrijednosti treba promijeniti.

<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=0:F59E0B,50:22C55E,100:0EA5E9&height=110&section=footer" alt="Travel Planner footer wave" />
</p>
