# Database EF Core

## Current Behavior
The app uses EF Core 8 with PostgreSQL through Npgsql. `Program.cs` resolves the connection string, registers `AppDbContext`, auto-applies migrations at startup, and runs idempotent seed data. `AppDbContextFactory` mirrors connection resolution for design-time migrations. Entities cover players, vehicles, factions, research links, owned vehicles, researched unlocks, matches, match participants, and maps.

`SeedData.Initialize` updates existing vehicles by `Code`, so seeded vehicle balance changes are applied on startup without a schema migration. The catalog now contains 21 robots: 8 existing visible entries plus 13 hidden development-tree entries split into 6 Iron Alliance and 7 Nova Syndicate robots across levels 3 and 4. Hidden entries use `IsVisible = false`; they are returned by catalog/graph queries but are rejected by research, purchase, and debug-add ownership flows. Current seeded vehicle accuracy spans `0.8` to `2.0`, penetration spans `90` to `158`, and projectile speed spans `90` to `150` after linear rebalancing that preserves the original robot order. Movement speed spans `1.2` to `5.95`, and acceleration spans `0.26` to `1.0`. `TraverseAcceleration` is a separate persisted body-turn acceleration field; every seeded value is 100 times the corresponding movement `Acceleration` and ranges from `26` to `100`. Iron Alliance turret-turn speed ranges from `10.5` to `25`; Nova Syndicate turret-turn speed ranges from `44` to `78`.

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
- `Migrations/20261018120000_AddMatchParticipantRewardAmounts.cs` - adds persisted per-match `BoltsEarned` and `FreeXpEarned` values. Existing historical participant rows receive `0`; the match API marks those rows as not reward-calculated so clients do not present the default values as a valid reward.
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
