# Database EF Core

## Current Behavior
The app uses EF Core 8 with PostgreSQL through Npgsql. `Program.cs` resolves the connection string, registers `AppDbContext`, auto-applies migrations at startup, and runs idempotent seed data. `AppDbContextFactory` mirrors connection resolution for design-time migrations. Entities cover players, vehicles, factions, research links, owned vehicles, researched unlocks, matches, match participants, and maps.

`SeedData.Initialize` updates existing vehicles by `Code`, so seeded vehicle balance changes are applied on startup without a schema migration. Current seeded vehicle accuracy spans `0.8` to `2.0`, penetration spans `90` to `135`, and projectile speed spans `90` to `150` after linear rebalancing that preserves the original robot order. Movement speed spans `1.26` to `5.616` after the global 50% reduction and `IA Strider`'s 30% increase, and acceleration spans `0.275` to `1.0` after its second global 50% reduction. `TraverseAcceleration` is a separate persisted body-turn acceleration field; every seeded value is 100 times the corresponding movement `Acceleration` and ranges from `27.5` to `100`. Iron Alliance turret-turn speed is the average of its original and reduced values, ranging from `11.25` to `24`; Nova Syndicate turret-turn speed is reduced by 15% and ranges from `51.85` to `68`.

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
- `Migrations/20261014120000_AddVehicleTraverseAcceleration.cs` - adds `TraverseAcceleration` and copies the current `Acceleration` into it for existing vehicles.
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
