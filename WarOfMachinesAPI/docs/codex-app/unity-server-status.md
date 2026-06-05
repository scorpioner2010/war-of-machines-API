# Unity Server Status

## Current Behavior
Unity server status is stored in memory as the latest heartbeat snapshot. `GET /unity-server/status` returns offline when no heartbeat exists or when the last heartbeat is older than 6 seconds. `POST /unity-server/status` records a heartbeat, normalizes status text, address, and port, and returns receipt metadata. The admin landing page polls this endpoint.

## Owner Files
- `Controllers/UnityServerStatusController.cs` - public status get/post routes, request address normalization, port validation.
- `Infrastructure/UnityServerStatusStore.cs` - singleton in-memory status snapshot, offline threshold, heartbeat update.
- `Program.cs` - registers `UnityServerStatusStore` as singleton.
- `wwwroot/admin/index.html` - fetches `/unity-server/status`.

## Important Configuration
- No appsettings-backed configuration exists for this system.
- Offline threshold is hard-coded to 6 seconds in `UnityServerStatusStore`.
- Routes are public and do not require JWT auth.

## Dependencies
- `UnityServerStatusStore` uses a lock around the latest snapshot.
- Address fallback reads `X-Forwarded-For`, then `X-Real-IP`, then `HttpContext.Connection.RemoteIpAddress`.
- Valid port range is 1 through 65535; invalid ports are stored as `null`.

## Routes / Contracts
- `GET /unity-server/status` returns `UnityServerStatusSnapshot` fields: `isOnline`, `status`, `address`, `port`, `lastHeartbeatUtc`, `secondsSinceHeartbeat`, `playersOnline`, `maxPlayers`, `activeMatches`, `message`.
- `POST /unity-server/status` accepts optional `status`, `address`, `port`, `playersOnline`, `maxPlayers`, `activeMatches`, `message`; returns `{ ok, receivedAtUtc, isOnline }`.

## Tests
- No automated Unity server status tests were found.

## Notes For Future Changes
- Status is not persisted; process restart loses heartbeat state.
- If this endpoint becomes trusted server-only input, update auth docs and add tests for forwarded address handling.
