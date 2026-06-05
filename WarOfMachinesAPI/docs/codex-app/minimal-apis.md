# Admin Minimal APIs

## Current Behavior
The app serves static admin HTML from `wwwroot/admin` and maps minimal routes for the admin landing page and log viewer. The root route redirects to `/admin`. Admin routes are public and do not require authentication. The logs page uses server-sent events to stream in-memory log entries.

## Owner Files
- `Program.cs` - maps `/`, `/admin`, `/admin/logs`, `/admin/logs/stream`, `/admin/logs/snapshot`, `/admin/logs/clear`.
- `wwwroot/admin/index.html` - static admin landing page; fetches `/unity-server/status`.
- `wwwroot/admin/logs.html` - static log viewer page; opens `EventSource('/admin/logs/stream')`.
- `Logging/InMemoryLogSink.cs` - in-memory log event store used by admin log routes.

## Important Configuration
- Static files are enabled by `app.UseStaticFiles()`.
- Admin routes use `app.Environment.WebRootPath` or `AppContext.BaseDirectory/wwwroot` fallback to locate HTML files.
- No auth, role check, CORS restriction, or anti-forgery is configured for admin routes.

## Dependencies
- `InMemoryLogStore` supplies log snapshots and deltas.
- Browser `EventSource` consumes `/admin/logs/stream`.
- Admin landing page depends on `/unity-server/status` for server status display.

## Routes / Contracts
- `GET /` redirects to `/admin`.
- `GET /admin` serves `wwwroot/admin/index.html` or returns `404` text if missing.
- `GET /admin/logs` serves `wwwroot/admin/logs.html` or returns `404` text if missing.
- `GET /admin/logs/stream` returns `text/event-stream`; sends recent 200 logs once, then deltas every second.
- `GET /admin/logs/snapshot` returns recent 200 logs as JSON.
- `POST /admin/logs/clear` clears the buffer, records a "Log buffer cleared." event, and returns `{ ok = true, cleared = true }`.

## Tests
- No automated admin UI or minimal API tests were found.

## Notes For Future Changes
- If admin routes become protected, update auth, endpoint, and deployment docs.
- Keep log JSON camelCase expectations aligned with `logging-observability.md`.
