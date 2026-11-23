# Configuration Settings

## Current Behavior
Configuration comes from standard ASP.NET Core providers plus `.env.local`, which is loaded into environment variables before `WebApplication.CreateBuilder`. Database configuration prefers `DATABASE_URL`, then `ConnectionStrings:DefaultConnection`. JWT signing key and issuer/audience are required at startup. JSON output uses camelCase, null values are omitted, CORS requires explicit origins, and Swagger is disabled unless Development or explicitly enabled.

## Owner Files
- `Program.cs` - reads database and JWT config, configures JSON, CORS, Swagger, logging-related startup events, middleware.
- `Infrastructure/LocalEnvFileLoader.cs` - parses `.env.local` as simple `KEY=value` lines.
- `Infrastructure/DatabaseConnectionStringFactory.cs` - parses `DATABASE_URL` or Npgsql keyword connection strings.
- `appsettings.json` - base connection string placeholder, security defaults, logging levels, allowed hosts, and empty secret placeholders.
- `appsettings.Development.json` - development logging plus explicit local migration/seed switches.
- `appsettings.Production.json` - production logging levels and `AllowedHosts`.
- `.env.local.example` - local environment template.
- `Properties/launchSettings.json` - local HTTP/HTTPS/IIS Express launch URLs and development environment.
- `docker-compose.local.yml` - opt-in PostgreSQL 16 local-development container, bound only to `127.0.0.1:5433` with a persistent Docker volume.
- `tools/Start-LocalPostgresApi.ps1` - starts the local database and API with a process-scoped local connection string.
- `tools/Stop-LocalPostgres.ps1` - stops the local database while retaining its data volume.
- `tools/Initialize-LocalSecurityEnvironment.ps1` - creates process-scoped local Dedicated Server credentials and PEM transport material outside the repository.

## Important Configuration
- `DATABASE_URL` - PostgreSQL URL or keyword connection string; URL values can include `sslmode`.
- `ConnectionStrings:DefaultConnection` / `ConnectionStrings__DefaultConnection` - fallback Npgsql connection string.
- `Jwt:Key` / `Jwt__Key` - required signing key.
- `Jwt:Issuer` / `Jwt__Issuer` and `Jwt:Audience` / `Jwt__Audience` - required JWT validation values.
- `Jwt:AccessTokenMinutes` - bounded access-token lifetime (5–120 minutes).
- `Authentication:RefreshTokenLifetimeDays` - persisted refresh-session lifetime.
- `Authentication:ConnectionTicketLifetimeSeconds` - bounded one-time ticket lifetime (5–120 seconds).
- `Authentication:ServerDelegatedTokenMinutes` - bounded delegated profile-token lifetime (1–15 minutes).
- `DedicatedServers:Provisioned__*` - protected bootstrap server identities used before a server row exists; never commit real credentials.
- `DedicatedServers:HeartbeatTimeoutSeconds` - discovery expiration window.
- `Cors:AllowedOrigins` - explicit browser origins; Unity does not need CORS.
- `ForwardedHeaders:KnownProxies` - trusted reverse-proxy IPs for forwarded headers.
- `Database:ApplyMigrationsOnStartup` and `Database:RunSeedOnStartup` - explicit startup operations; production defaults are false.
- `Swagger:Enabled` - explicit non-development Swagger switch; default false.
- `Logging:LogLevel:Default` - application default log level.
- `Logging:LogLevel:Microsoft.AspNetCore` - ASP.NET Core log level.
- `Logging:LogLevel:Microsoft.EntityFrameworkCore` - production EF Core log level.
- `AllowedHosts` - host filtering configuration; set it to the production API host for deployment instead of leaving `*`.
- Launch URLs: `http://localhost:5220`, `https://localhost:7216`, IIS Express `http://localhost:43606` with SSL port `44377`.
- Rider/Visual Studio profile `local-postgres` runs Kestrel on ports `7216`/`5220` with the Docker local database. Select it instead of `IIS Express`, which uses `.env.local` and may target the remote database.
- Docker local database: `Host=127.0.0.1;Port=5433;Database=war_of_machines_local;Username=wom_local;Password=wom_local_dev_password`.

## Dependencies
- Npgsql connection string builder handles PostgreSQL URL conversion.
- `LocalEnvFileLoader` treats pre-existing process environment variables as higher priority than `.env.local`; the env file supplies only missing values.
- `Start-LocalPostgresApi.ps1` sets `DATABASE_URL` only for its own API process, so stopping it and running `dotnet run --launch-profile https` restores the existing `.env.local` database target without editing that file.
- The local scripts use `docker` from `PATH`, or the per-user Docker Desktop CLI path under `%LOCALAPPDATA%\Programs\DockerDesktop\resources\bin` when Docker was installed for the current user; in the latter case they add that directory to the process `PATH` so Docker's credential helper can run.
- JWT auth depends on `Jwt:Key` at startup and in `AuthController`.
- CORS policy `client` runs before static files/auth and is deny-by-default when no origins are configured.
- Forwarded headers are accepted only from configured known proxies; arbitrary `X-Forwarded-For` is ignored.
- Production Unity builds accept only the configured HTTPS API allowlist; local HTTPS candidates are development-only.
- The desktop `Start War of Machines.bat` runs `tools/Initialize-LocalSecurityEnvironment.ps1`, passes `https://localhost:7216`, and never enables an HTTP API fallback.
- Local server/client builds are Development builds only so the local HTTPS allowlist is available; production builds remain restricted to the production HTTPS host.

## Routes / Contracts
- Swagger UI is served at `/swagger`.
- Root `/` redirects to `/admin`.

## Tests
- No automated configuration tests were found.

## Notes For Future Changes
- Do not add or rename configuration keys without updating this file and the affected system doc.
- Prefer typed options for new configuration sections.
- Do not commit real `.env.local` secrets.


## Stage 2 production additions

- Production Compose mounts secret files under /run/secrets; the key-per-file provider is added after standard configuration so mounted values take precedence.
- DataProtection:KeyDirectory points to a writable persistent named volume. Production also requires DataProtection:CertificatePath and DataProtection:CertificatePassword; the PFX and its password come from separate secret files. The certificate protects the persisted key ring at rest.
- Production AllowedHosts and ForwardedHeaders:KnownProxies are set by Compose for the chosen API host and internal Caddy address.
- Database:ApplyMigrationsOnStartup, Database:RunSeedOnStartup and Swagger:Enabled stay false in production. EF migrations run only through the explicit migrator profile.

## RAV Identity and Compatibility

RAV_API_BASE is the primary Unity API endpoint setting; WOM_API_BASE remains a fallback for existing local launchers and server environments. RAV_SERVER_* and RAV_UTP_* settings follow the same RAV-first, WOM-fallback policy. The desktop Start War of Machines.bat filename remains a legacy shortcut; its contents launch the RAV API and builds. See branding-and-compatibility.md for the complete mapping.

The JWT issuer/audience and Render production HTTPS hostname remain unchanged for token and client compatibility. The local Compose project/database/role and volume identities also remain unchanged to preserve existing local PostgreSQL data.