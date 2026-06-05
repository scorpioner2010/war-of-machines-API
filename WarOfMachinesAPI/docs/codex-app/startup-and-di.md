# Startup And DI

## Current Behavior
`Program.cs` builds a .NET 8 ASP.NET Core API. Startup loads `.env.local`, resolves a PostgreSQL connection string, registers EF Core, controllers with camelCase JSON, permissive CORS, Swagger, JWT bearer auth, authorization, and a singleton Unity server status store. At runtime it uses CORS, static files, Swagger UI, authentication, authorization, controller routes, root redirect, admin minimal endpoints, then auto-migrates and seeds the database before serving requests.

## Owner Files
- `Program.cs` - startup pipeline, DI registrations, middleware order, Swagger, JWT validation setup, admin minimal endpoints, startup migration and seed execution.
- `Infrastructure/LocalEnvFileLoader.cs` - loads `.env.local` into environment variables before configuration values are used.
- `Infrastructure/DatabaseConnectionStringFactory.cs` - resolves `DATABASE_URL` or `ConnectionStrings:DefaultConnection` for Npgsql.
- `Data/SeedData.cs` - idempotent startup seed data.
- `Infrastructure/UnityServerStatusStore.cs` - registered as singleton.

## Important Configuration
- `DATABASE_URL` - preferred database setting; accepts PostgreSQL URL or Npgsql keyword connection string.
- `ConnectionStrings:DefaultConnection` / `ConnectionStrings__DefaultConnection` - fallback database setting.
- `Jwt:Key` / `Jwt__Key` - required at startup; missing/blank throws `InvalidOperationException`.
- `Logging:LogLevel` - framework logging levels in `appsettings*.json`.
- CORS policy name `any` - allows any origin, header, and method.

## Dependencies
- `Microsoft.EntityFrameworkCore` with `Npgsql.EntityFrameworkCore.PostgreSQL`; retry-on-failure is enabled with 5 retries and 10 second max delay.
- Middleware order in `Program.cs`: `UseCors("any")`, `UseStaticFiles()`, `UseSwagger()`, `UseSwaggerUI()`, `UseAuthentication()`, `UseAuthorization()`, `MapControllers()`, root/admin minimal route mappings.
- Startup database action calls `db.Database.Migrate()` and then `SeedData.Initialize(db)`.
- `UnityServerStatusStore` is a singleton in-memory dependency.

## Routes / Contracts
- Controllers are mapped by attributes via `app.MapControllers()`.
- Minimal routes are documented in `minimal-apis.md`.

## Tests
- No automated tests were found.

## Notes For Future Changes
- Any startup/middleware/DI change must update this file and any affected system doc.
- Do not add service registrations blindly; follow the explicit `Program.cs` pattern.
- Changing startup migration/seed behavior also requires updating `database-ef-core.md`.
