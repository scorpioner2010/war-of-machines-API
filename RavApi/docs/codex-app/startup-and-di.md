# Startup And DI

## Current Behavior
`Program.cs` builds a .NET 10 ASP.NET Core API. Startup loads `.env.local`, resolves PostgreSQL, registers EF Core retry-on-failure, camelCase controllers, restrictive configured CORS, ProblemDetails, forwarded-header trust, built-in rate limiting, JWT bearer auth, Dedicated Server auth, policies, and persisted sessions/tickets/server registry. Migrations and seed data run only when their explicit configuration flags are enabled; production defaults are disabled.

## Owner Files
- `Program.cs` - startup pipeline, DI registrations, middleware order, Swagger, JWT validation setup, admin minimal endpoints, startup migration and seed execution.
- `Infrastructure/LocalEnvFileLoader.cs` - loads `.env.local` into environment variables before configuration values are used.
- `Infrastructure/DatabaseConnectionStringFactory.cs` - resolves `DATABASE_URL` or `ConnectionStrings:DefaultConnection` for Npgsql.
- `Data/SeedData.cs` - idempotent startup seed data.
- `Infrastructure/DedicatedServerAuthenticationHandler.cs` - server-only header authentication scheme.
- `Infrastructure/JwtTokenService.cs` - configured JWT issuance for access and delegated server-session tokens.
- `Infrastructure/ApiExceptionHandler.cs` - generic ProblemDetails response for unhandled exceptions.

## Important Configuration
- `DATABASE_URL` - preferred database setting; accepts PostgreSQL URL or Npgsql keyword connection string.
- `ConnectionStrings:DefaultConnection` / `ConnectionStrings__DefaultConnection` - fallback database setting.
- `Jwt:Key` / `Jwt__Key` - required at startup; missing/blank throws `InvalidOperationException`.
- `Logging:LogLevel` - framework logging levels in `appsettings*.json`.
- `Cors:AllowedOrigins` - explicit browser origins; an empty list allows no CORS origins. Unity is not authenticated by CORS.
- `ForwardedHeaders:KnownProxies` - only these proxy IPs may supply forwarded address/proto values.
- `Database:ApplyMigrationsOnStartup` and `Database:RunSeedOnStartup` - explicit startup switches, both false in base configuration.

## Dependencies
- `Microsoft.EntityFrameworkCore` with `Npgsql.EntityFrameworkCore.PostgreSQL`; retry-on-failure is enabled with 5 retries and 10 second max delay.
- Middleware order in `Program.cs`: exception handler, trusted forwarded headers, configured CORS, rate limiter, static files, conditional Swagger, authentication, authorization, controllers, and protected admin minimal routes.
- Startup calls `db.Database.Migrate()` only when `Database:ApplyMigrationsOnStartup=true`, then seeds only when `Database:RunSeedOnStartup=true`.
- Dedicated Server registry, user sessions, and connection tickets are EF/PostgreSQL entities, not in-memory state.

## Routes / Contracts
- Controllers are mapped by attributes via `app.MapControllers()`.
- Minimal routes are documented in `minimal-apis.md`.

## Tests
- No checked-in test project exists; local security smoke scenarios are recorded in `SECURITY_HARDENING_REPORT.md`.

## Notes For Future Changes
- Any startup/middleware/DI change must update this file and any affected system doc.
- Do not add service registrations blindly; follow the explicit `Program.cs` pattern.
- Changing startup migration/seed behavior also requires updating `database-ef-core.md`.


## Stage 2 additions

When DataProtection:KeyDirectory is configured, Data Protection keys are persisted to that directory. Production fails closed unless an encryption PFX and password are configured; Compose mounts them as secrets and stores the key ring in its own writable volume. Liveness and database-readiness minimal routes are registered anonymously. Readiness returns only a generic status and HTTP 503 when its database check fails.