# Database

SQL Server is the required database engine for the project.

Entity/database models must stay separate from DTO models. Service-specific entity models belong under each owning service's `Data/Entities` folder. SQL migrations belong under service-specific `Data/Migrations` folders or the shared `migrations/` folder below when a migration spans the whole database.

