# Database EF Core

## Current Behavior
The app uses EF Core 8 with PostgreSQL through Npgsql. `Program.cs` resolves the connection string, registers `AppDbContext` with retry-on-failure, and applies migrations/seed data only when explicit startup flags are enabled. `AppDbContextFactory` mirrors connection resolution for design-time migrations. Entities include Dedicated Server registry, user sessions, one-time connection tickets, match ownership/roster, and concurrency versions in addition to gameplay data.

`SeedData.Initialize` updates existing vehicles by `Code`, so seeded balance changes are applied only when the explicit seed switch is enabled. The security migration is additive and does not alter gameplay formulas.

## Owner Files
- `Data/AppDbContext.cs` - DbSets, relationships, indexes, check constraints, delete behavior.
- `Data/AppDbContextFactory.cs` - design-time DbContext creation for migrations.
- `Data/SeedData.cs` - factions, maps, vehicles, research links, default test user, starter vehicle/research, demo match seed.
- `Infrastructure/DatabaseConnectionStringFactory.cs` - database connection resolution and `DATABASE_URL` conversion.
- `Infrastructure/LocalEnvFileLoader.cs` - `.env.local` loader for local database/JWT settings.
- `Models/DedicatedServer.cs`, `Models/UserSession.cs`, `Models/ConnectionTicket.cs` - persisted security state.
- `Models/*.cs` - EF Core entity classes.
- `Migrations/20251009190022_InitialCreate.cs` - creates initial tables and indexes.
- `Migrations/20260512084921_AddVehicleProjectileStats.cs` - replaces vehicle `Damage` with projectile stat columns and constraints.
- `Migrations/20260512114908_AddUserVehicleResearches.cs` - adds researched unlock table.
- `Migrations/20260517071952_AddVehicleViewRange.cs` - adds `ViewRange` and positive check constraint.
- `Migrations/20261014120000_AddVehicleTraverseAcceleration.cs` - adds `TraverseAcceleration` and copies the current `Acceleration` into it for existing vehicles.
- `Migrations/20261103120000_AddVehicleEnginePower.cs` - adds positive `EnginePower` and initializes the balanced values for seeded robot codes.
- `Migrations/20261120120000_IncreaseNovaBipedEnginePower.cs` - increases `EnginePower` by `1000` for the 11 seeded Nova Syndicate biped robots without changing Iron Alliance tracked robots.
- `Migrations/20261119085129_SecurityHardeningFoundation.cs` - adds Dedicated Server registry, sessions, tickets, match ownership/version, finalized participant state, indexes, and constraints.
- `Migrations/20261018120000_AddMatchParticipantRewardAmounts.cs` - adds persisted per-match `BoltsEarned` and `FreeXpEarned` values. Existing historical participant rows receive `0`; the match API marks those rows as not reward-calculated so clients do not present the default values as a valid reward.
- `Migrations/AppDbContextModelSnapshot.cs` - current EF Core model snapshot.

## Important Configuration
- `DATABASE_URL` - preferred connection value; PostgreSQL URL values default to `SslMode=Require` unless overridden by query string.
- `ConnectionStrings:DefaultConnection` / `ConnectionStrings__DefaultConnection` - fallback connection value.
- `appsettings.json` has an empty `ConnectionStrings:DefaultConnection`; local values should come from `.env.local` or environment variables.

## Dependencies
- Database provider: `Npgsql.EntityFrameworkCore.PostgreSQL`.
- Design-time package: `Microsoft.EntityFrameworkCore.Tools`.
- Tables/entities: `Players`, `Vehicles`, `UserVehicles`, `UserVehicleResearches`, `Matches`, `MatchParticipants`, `DedicatedServers`, `UserSessions`, `ConnectionTickets`, `Factions`, `Maps`, `VehicleResearchRequirements`.
- Key indexes/constraints: unique `Factions.Code`, `Maps.Code`, `Vehicles.Code`; unique user/vehicle ownership; unique researched unlock; one active vehicle per user via filtered unique index; unique predecessor/successor research link; unique refresh-token and ticket hashes; unique `(MatchId, UserId)` roster; match server/end-state and ticket expiry lookup indexes.
- Delete behavior: player deletion cascades to owned/researched vehicles and match participants; vehicle deletion is restricted from ownership/match participants/predecessor links, while successor research links cascade from `Vehicle.ResearchFrom`.

## Routes / Contracts
- Database state is used by all controller routes. Unity Server registry is PostgreSQL-backed, so API restarts do not erase records; discovery filters stale heartbeats.
- Startup calls `db.Database.Migrate()` before `SeedData.Initialize(db)` only when the corresponding explicit flags are enabled. Production must apply reviewed migrations as a controlled deployment step.

## Tests
- No automated database or migration tests were found.

## Notes For Future Changes
- Any entity, relationship, index, migration, or seed data change must update this doc and the affected domain doc. The Nova biped engine-power balance is defined consistently in `SeedData.cs` and the additive `20261120120000_IncreaseNovaBipedEnginePower` migration.
- Startup auto-migration can affect production deploys; update `deployment-hosting.md` if this behavior changes.
- Keep `AppDbContextFactory` connection behavior aligned with runtime startup.
