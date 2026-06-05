# Codex App Documentation Index

Future Codex sessions must read `AGENTS.md` first, then this index, then every relevant system doc below before editing code. Use these docs as a navigation map, then verify behavior in the source files.

## System Docs
- `startup-and-di.md` - ASP.NET Core startup, middleware order, DI registrations, Swagger, startup migration, and seed execution.
- `api-endpoints.md` - Public controller and minimal API route map with auth requirements.
- `authentication-authorization.md` - Register/login behavior, JWT issuance/validation, BCrypt password handling, and protected route ownership.
- `player-profiles.md` - Current player profile, active vehicle selection, player currencies, and owned/researched vehicle DTOs.
- `vehicles-and-research.md` - Vehicle catalog, faction/branch filtering, tech tree links, research unlocks, ownership purchase/sell/debug flows, and vehicle XP conversion.
- `matches-and-rewards.md` - Match creation, result submission, anti-cheat clamps, XP/Bolts/MMR/free-XP reward calculations, and participant history.
- `maps-and-leaderboards.md` - Map listing and leaderboard endpoints for MMR, free XP, and vehicle XP.
- `database-ef-core.md` - `AppDbContext`, EF Core entities, relationships, indexes, migrations, PostgreSQL setup, and seed data.
- `configuration-settings.md` - `appsettings`, `.env.local`, connection string resolution, JWT key config, launch profiles, logging levels, and CORS policy.
- `minimal-apis.md` - Static admin pages, `/admin` minimal endpoints, log viewer routes, and root redirect.
- `logging-observability.md` - In-memory log store, admin log stream/snapshot/clear behavior, startup log events, and missing diagnostics.
- `unity-server-status.md` - Unity server heartbeat/status endpoints and singleton in-memory status store.
- `validation-and-error-handling.md` - Manual validation, `[ApiController]` behavior, route-specific error responses, and current exception handling gaps.
- `deployment-hosting.md` - Local/production hosting files, startup migration implications, static file serving, Swagger exposure, and solution/project layout.

## Systems Not Present
- No test project or automated test files were found.
- No background services, hosted workers, queues, or scheduled jobs were found.
- No SignalR hubs were found.
- No server-side Razor Pages, MVC views, or Blazor UI were found.
- No distributed cache, memory cache registration, response caching, or session state was found.
- No OpenTelemetry, health checks, Serilog/NLog, rate limiting, or custom global exception middleware was found.
