# Codex App Documentation Index

Future Codex sessions must read `AGENTS.md` first, then this index, then every relevant system doc below before editing code. Use these docs as a navigation map, then verify behavior in the source files.

## System Docs
- `startup-and-di.md` - ASP.NET Core startup, middleware order, DI registrations, Swagger, startup migration, and seed execution.
- `api-endpoints.md` - Public controller and minimal API route map with auth requirements, server policies, and ticket contracts.
- `authentication-authorization.md` - Login/session behavior, JWT issuance/validation, refresh rotation, server credentials, tickets, and protected route ownership.
- `player-profiles.md` - Current player profile, active vehicle selection, player currencies, and owned/researched vehicle DTOs.
- `vehicles-and-research.md` - Vehicle catalog, faction/branch filtering, tech tree links, research unlocks, ownership purchase/sell/debug flows, and vehicle XP conversion.
- `robot-characteristics.md` - Ukrainian player-facing reference for every seeded robot, with grouped statistics, roles, and research costs.
- `matches-and-rewards.md` - Match creation, result submission, anti-cheat clamps, XP/Bolts/MMR/free-XP reward calculations, and participant history.
- `maps-and-leaderboards.md` - Map listing and leaderboard endpoints for MMR, free XP, and vehicle XP.
- `database-ef-core.md` - `AppDbContext`, EF Core entities, relationships, indexes, migrations, PostgreSQL setup, and seed data.
- `configuration-settings.md` - `appsettings`, `.env.local`, connection string resolution, JWT/session/server credential config, launch profiles, logging levels, and CORS policy.
- `minimal-apis.md` - Static admin pages, `/admin` minimal endpoints, log viewer routes, and root redirect.
- `logging-observability.md` - In-memory log store, admin log stream/snapshot/clear behavior, startup log events, and missing diagnostics.
- `unity-server-status.md` - PostgreSQL-backed Dedicated Server identity, registration, heartbeat, expiration, and discovery endpoints.
- `validation-and-error-handling.md` - DTO validation, rate limiting, ProblemDetails handling, route-specific errors, and transactional conflict handling.
- `deployment-hosting.md` - Local/production hosting files, explicit migration/seed controls, proxy trust, HTTPS, Swagger exposure, and deployment order.

## Systems Not Present
- No checked-in test project currently exists; local API smoke/integration checks are documented in `SECURITY_HARDENING_REPORT.md`.
- No background services, hosted workers, queues, or scheduled jobs were found.
- No SignalR hubs were found.
- No server-side Razor Pages, MVC views, or Blazor UI were found.
- No distributed cache, response caching, or session state was found. Refresh sessions, tickets, and server registry are persisted in PostgreSQL.
- Built-in rate limiting and `IExceptionHandler`/ProblemDetails are configured in `Program.cs`; no external tracing or hosted worker is present.
