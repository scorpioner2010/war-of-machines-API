# Configuration Settings

## Current Behavior
Configuration comes from standard ASP.NET Core providers plus `.env.local`, which is loaded into environment variables before `WebApplication.CreateBuilder`. Database configuration prefers `DATABASE_URL`, then `ConnectionStrings:DefaultConnection`. JWT signing key is required at startup. JSON output uses camelCase, null values are omitted, CORS is fully permissive, and Swagger is always enabled.

## Owner Files
- `Program.cs` - reads database and JWT config, configures JSON, CORS, Swagger, logging-related startup events, middleware.
- `Infrastructure/LocalEnvFileLoader.cs` - parses `.env.local` as simple `KEY=value` lines.
- `Infrastructure/DatabaseConnectionStringFactory.cs` - parses `DATABASE_URL` or Npgsql keyword connection strings.
- `appsettings.json` - base connection string placeholder, logging levels, allowed hosts, default JWT key.
- `appsettings.Development.json` - development logging levels.
- `appsettings.Production.json` - production logging levels and `AllowedHosts`.
- `.env.local.example` - local environment template.
- `Properties/launchSettings.json` - local HTTP/HTTPS/IIS Express launch URLs and development environment.
- `docker-compose.local.yml` - opt-in PostgreSQL 16 local-development container, bound only to `127.0.0.1:5433` with a persistent Docker volume.
- `tools/Start-LocalPostgresApi.ps1` - starts the local database and API with a process-scoped local connection string.
- `tools/Stop-LocalPostgres.ps1` - stops the local database while retaining its data volume.

## Important Configuration
- `DATABASE_URL` - PostgreSQL URL or keyword connection string; URL values can include `sslmode`.
- `ConnectionStrings:DefaultConnection` / `ConnectionStrings__DefaultConnection` - fallback Npgsql connection string.
- `Jwt:Key` / `Jwt__Key` - required signing key.
- `Logging:LogLevel:Default` - application default log level.
- `Logging:LogLevel:Microsoft.AspNetCore` - ASP.NET Core log level.
- `Logging:LogLevel:Microsoft.EntityFrameworkCore` - production EF Core log level.
- `AllowedHosts` - currently `*`.
- Launch URLs: `http://localhost:5220`, `https://localhost:7216`, IIS Express `http://localhost:43606` with SSL port `44377`.
- Rider/Visual Studio profile `local-postgres` runs Kestrel on ports `7216`/`5220` with the Docker local database. Select it instead of `IIS Express`, which uses `.env.local` and may target the remote database.
- Docker local database: `Host=127.0.0.1;Port=5433;Database=war_of_machines_local;Username=wom_local;Password=wom_local_dev_password`.

## Dependencies
- Npgsql connection string builder handles PostgreSQL URL conversion.
- `LocalEnvFileLoader` treats pre-existing process environment variables as higher priority than `.env.local`; the env file supplies only missing values.
- `Start-LocalPostgresApi.ps1` sets `DATABASE_URL` only for its own API process, so stopping it and running `dotnet run --launch-profile https` restores the existing `.env.local` database target without editing that file.
- The local scripts use `docker` from `PATH`, or the per-user Docker Desktop CLI path under `%LOCALAPPDATA%\Programs\DockerDesktop\resources\bin` when Docker was installed for the current user; in the latter case they add that directory to the process `PATH` so Docker's credential helper can run.
- JWT auth depends on `Jwt:Key` at startup and in `AuthController`.
- CORS policy `any` is used before static files and auth.

## Routes / Contracts
- Swagger UI is served at `/swagger`.
- Root `/` redirects to `/admin`.

## Tests
- No automated configuration tests were found.

## Notes For Future Changes
- Do not add or rename configuration keys without updating this file and the affected system doc.
- Prefer typed options for new configuration sections.
- Do not commit real `.env.local` secrets.
