# Travel Planner

Initial scaffold for the travel planning project described in `pugs.pdf` and the Service Fabric practice material in `vezbe.pdf`.

The repository is split into:

- `backend/` - Microsoft Service Fabric application, services, remoting contracts, common helpers, and SQL migration folders.
- `frontend/` - React application scaffold with API services, models, context, routes, pages, components, and utilities.

Some business logic, service persistence, and UI implementation are intentionally still scaffolded.

## Local auth configuration

IdentityService reads its auth settings from the Service Fabric `Config` package:

- `ConnectionStrings/DefaultConnection`
- `Jwt/Secret`
- `Jwt/Issuer`
- `Jwt/Audience`
- `Jwt/ExpirationMinutes`

For local Service Fabric runs, set the application parameter values in `backend/TravelPlanner.ServiceFabric/ApplicationParameters/Local.1Node.xml` before deploying:

- `Identity_DefaultConnection` - your local SQL Server connection string for the identity database.
- `Identity_JwtSecret` - a real local development JWT signing secret, at least 32 characters long.
- `Identity_JwtIssuer`, `Identity_JwtAudience`, and `Identity_JwtExpirationMinutes` - adjust only if your local client/server settings need different values.

The checked-in `Identity_JwtSecret` value is intentionally an unusable placeholder; replace it before running auth locally.

`backend/TravelPlanner.ServiceFabric/ApplicationPackageRoot/ApplicationManifest.xml` maps those application parameters into the IdentityService `Settings.xml` values. Do not put production secrets in source-controlled XML files; use deployment-specific Service Fabric application parameter files or your secret-management process for real environments. If a connection string contains XML special characters such as `&`, escape them in the parameter file.
