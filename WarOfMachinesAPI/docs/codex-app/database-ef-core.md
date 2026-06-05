# Database EF Core

## Current Behavior
The app uses EF Core 8 with PostgreSQL through Npgsql. `Program.cs` resolves the connection string, registers `AppDbContext`, auto-applies migrations at startup, and runs idempotent seed data. `AppDbContextFactory` mirrors connection resolution for design-time migrations. Entities cover players, vehicles, factions, research links, owned vehicles, researched unlocks, matches, match participants, and maps.

## Owner Files
- `Data/AppDbContext.cs` - DbSets, relationships, indexes, check constraints, delete behavior.
- `Data/AppDbContextFactory.cs` - design-time DbContext creation for migrations.
- `Data/SeedData.cs` - factions, maps, vehicles, research links, default test user, starter vehicle/research, demo match seed.
- `Infrastructure/DatabaseConnectionStringFactory.cs` - database connection resolution and `DATABASE_URL` conversion.
- `Infrastructure/LocalEnvFileLoader.cs` - `.env.local` loader for local database/JWT settings.
- `Models/*.cs` - EF Core entity classes.
- `Migrations/20251009190022_InitialCreate.cs` - creates initial tables and indexes.
- `Migrations/20260512084921_AddVehicleProjectileStats.cs` - replaces vehicle `Damage` with projectile stat columns and constraints.
- `Migrations/20260512114908_AddUserVehicleResearches.cs` - adds researched unlock table.
- `Migrations/20260517071952_AddVehicleViewRange.cs` - adds `ViewRange` and positive check constraint.
- `Migrations/AppDbContextModelSnapshot.cs` - current EF Core model snapshot.

## Important Configuration
- `DATABASE_URL` - preferred connection value; PostgreSQL URL values default to `SslMode=Require` unless overridden by query string.
- `ConnectionStrings:DefaultConnection` / `ConnectionStrings__DefaultConnection` - fallback connection value.
- `appsettings.json` has an empty `ConnectionStrings:DefaultConnection`; local values should come from `.env.local` or environment variables.

## Dependencies
- Database provider: `Npgsql.EntityFrameworkCore.PostgreSQL`.
- Design-time package: `Microsoft.EntityFrameworkCore.Tools`.
- Tables/entities: `Players`, `Vehicles`, `UserVehicles`, `UserVehicleResearches`, `Matches`, `MatchParticipants`, `Factions`, `Maps`, `VehicleResearchRequirements`.
- Key indexes/constraints: unique `Factions.Code`, `Maps.Code`, `Vehicles.Code`; unique user/vehicle ownership; unique researched unlock; one active vehicle per user via filtered unique index; unique predecessor/successor research link; vehicle projectile/view range check constraints.
- Delete behavior: player deletion cascades to owned/researched vehicles and match participants; vehicle deletion is restricted from ownership/match participants/predecessor links, while successor research links cascade from `Vehicle.ResearchFrom`.

## Routes / Contracts
- Database state is used by all controller routes except admin static routes and Unity server status storage.
- Startup calls `db.Database.Migrate()` before `SeedData.Initialize(db)`.

## Tests
- No automated database or migration tests were found.

## Notes For Future Changes
- Any entity, relationship, index, migration, or seed data change must update this doc and the affected domain doc.
- Startup auto-migration can affect production deploys; update `deployment-hosting.md` if this behavior changes.
- Keep `AppDbContextFactory` connection behavior aligned with runtime startup.
