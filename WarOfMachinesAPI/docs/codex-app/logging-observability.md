# Logging Observability

## Current Behavior
The app has a static in-memory log store and admin routes for viewing it. Startup manually adds events for server starting, database migrated, seed data ensured, and server started. A logger provider implementation exists, but `Program.cs` does not register it as a global logging provider. Framework logging levels are configured in `appsettings*.json`; no health checks, tracing, OpenTelemetry, metrics, or external log sink were found.

## Owner Files
- `Logging/InMemoryLogSink.cs` - `LogEvent`, `InMemoryLogStore`, in-memory `ILoggerProvider`, ring buffer capacity, recent/since/clear APIs.
- `Program.cs` - manual startup log events and admin log routes.
- `wwwroot/admin/logs.html` - client-side log stream consumer.
- `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json` - framework log levels.

## Important Configuration
- `Logging:LogLevel:Default` - default application log level.
- `Logging:LogLevel:Microsoft.AspNetCore` - ASP.NET Core logging level.
- `Logging:LogLevel:Microsoft.EntityFrameworkCore` - production EF Core logging level.
- `InMemoryLogStore.Capacity` defaults to `2000` entries and is not configured through appsettings.

## Dependencies
- `ConcurrentQueue<LogEvent>` stores logs in memory.
- Admin routes in `Program.cs` read/write `InMemoryLogStore`.
- Server-sent events send log batches as JSON from `/admin/logs/stream`.

## Routes / Contracts
- `GET /admin/logs/stream` streams recent logs and new deltas.
- `GET /admin/logs/snapshot` returns recent logs.
- `POST /admin/logs/clear` clears logs and adds one clear event.

## Tests
- No automated logging or observability tests were found.

## Notes For Future Changes
- Registering `InMemoryLoggerProvider` globally would change runtime logging volume and memory use; document it before doing so.
- Do not log secrets or PII. Auth registration currently logs the request object, so inspect before expanding auth logging.
- Production performance investigations should first check available logs and database/provider diagnostics; tracing and health checks are not currently present.
