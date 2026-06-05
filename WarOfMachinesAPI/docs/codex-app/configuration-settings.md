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

## Important Configuration
- `DATABASE_URL` - PostgreSQL URL or keyword connection string; URL values can include `sslmode`.
- `ConnectionStrings:DefaultConnection` / `ConnectionStrings__DefaultConnection` - fallback Npgsql connection string.
- `Jwt:Key` / `Jwt__Key` - required signing key.
- `Logging:LogLevel:Default` - application default log level.
- `Logging:LogLevel:Microsoft.AspNetCore` - ASP.NET Core log level.
- `Logging:LogLevel:Microsoft.EntityFrameworkCore` - production EF Core log level.
- `AllowedHosts` - currently `*`.
- Launch URLs: `http://localhost:5220`, `https://localhost:7216`, IIS Express `http://localhost:43606` with SSL port `44377`.

## Dependencies
- Npgsql connection string builder handles PostgreSQL URL conversion.
- `LocalEnvFileLoader` does not override existing environment variables with blank values.
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
