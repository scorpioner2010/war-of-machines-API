# Deployment Hosting

## Current Behavior
The repository contains one ASP.NET Core project and one solution. It runs as a .NET 8 web app, serves static files from `wwwroot`, exposes Swagger at `/swagger`, redirects `/` to `/admin`, and auto-applies EF Core migrations plus seed data during startup. Local launch profiles target `/admin`.

## Owner Files
- `WarOfMachines-api.sln` - single solution containing `WarOfMachinesAPI.csproj`.
- `WarOfMachinesAPI.csproj` - .NET 8 web project and package references.
- `Program.cs` - runtime pipeline, static files, Swagger, startup migration/seed, root/admin routes.
- `Properties/launchSettings.json` - local HTTP, HTTPS, and IIS Express profiles.
- `appsettings.Production.json` - production logging levels and `AllowedHosts`.
- `.env.local.example` - local environment template.
- `wwwroot/admin/index.html` and `wwwroot/admin/logs.html` - static admin assets.

## Important Configuration
- `ASPNETCORE_ENVIRONMENT` - launch profiles set `Development`.
- `ASPNETCORE_URLS` - optional in `.env.local.example`.
- `DATABASE_URL` or `ConnectionStrings__DefaultConnection` - required for startup.
- `Jwt__Key` - required for startup.
- `AllowedHosts` - currently `*`.

## Dependencies
- Runtime packages: BCrypt, EF Core Tools, JWT bearer auth, Npgsql EF Core provider, Swashbuckle, JWT token library.
- PostgreSQL must be reachable during startup because migration and seed run before the app finishes starting.
- Static admin files must exist under `wwwroot/admin` for `/admin` and `/admin/logs`.

## Routes / Contracts
- `/swagger` is exposed in all environments by current `Program.cs`.
- `/admin` and `/admin/logs` are public static/minimal routes.

## Tests
- No automated deployment or smoke tests were found.

## Notes For Future Changes
- Changing auto-migrate/seed startup behavior requires updating `database-ef-core.md` and `startup-and-di.md`.
- Adding Docker, reverse proxy, cloud hosting, health checks, or environment-specific Swagger behavior should update this file.
