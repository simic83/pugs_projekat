# Identity Migrations

Run `001_create_identity_schema.sql` against the SQL Server database used by `IdentityService`.

The migration creates `Users`, `Roles`, and `UserRoles`, enforces unique user email addresses, and seeds the `User` and `Admin` roles.
