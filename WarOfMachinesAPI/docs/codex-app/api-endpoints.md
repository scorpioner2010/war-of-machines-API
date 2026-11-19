# API Endpoints

## Current Behavior
The API uses attribute-routed controllers plus protected minimal admin routes. JSON output is camelCase and ignores null values. Player JWT, Dedicated Server credentials, and admin role policies are separate. `GET /unity-server/status` remains public for discovery; server mutations and match mutations are server-only.

## Owner Files
- `Program.cs` - maps controllers, root redirect, admin minimal routes, Swagger UI.
- `Controllers/AuthController.cs` - `/auth` registration and login.
- `Controllers/PlayersController.cs` - `/players` authenticated profile, live history statistics, and active vehicle routes.
- `Controllers/UserVehiclesController.cs` - `/user-vehicles` authenticated ownership, research, purchase, sell, XP routes.
- `Controllers/VehiclesController.cs` - `/vehicles` catalog and tech tree link routes.
- `Controllers/MatchesController.cs` - `/matches` server-only match start/end and authenticated participant history.
- `Controllers/ConnectionTicketsController.cs` - player ticket issue and server-only ticket redemption.
- `Controllers/MapsController.cs` - `/maps` public map listing.
- `Controllers/LeaderboardController.cs` - `/leaderboard` authenticated rankings.
- `Controllers/UnityServerStatusController.cs` - PostgreSQL-backed server register/heartbeat/unregister and public discovery.

## Important Configuration
- `Jwt:Key` - required for protected endpoints.
- `Cors:AllowedOrigins` - explicit browser origins; empty by default.
- Swagger is available only in Development or when `Swagger:Enabled=true`.
- Login/register/refresh use the built-in `auth` rate-limit policy (10 requests/IP/minute).

## Dependencies
- Controllers depend directly on `AppDbContext`; server auth uses `DedicatedServerCredentialValidator` and ticket redemption uses `JwtTokenService`.
- `[ApiController]` provides automatic model binding behavior for controller actions.
- `[Authorize]` is set at controller level for players, user vehicles, matches, and leaderboard.

## Routes / Contracts
- `POST /auth/register` - anonymous/rate-limited; body `{ username, password }`; requires a 3–32 character username and an 8–128 character password, creates a player with starter state, and returns `{ token, refreshToken, expiresAtUtc }`.
- `POST /auth/login` - anonymous/rate-limited; body `{ username, password }`; accepts existing 1–32 character usernames for legacy account compatibility and a 1–128 character password, then returns the same session response.
- `POST /auth/refresh` - anonymous/rate-limited; rotates an opaque refresh token and returns a new access/refresh pair.
- `POST /auth/logout` - current player JWT; revokes the persisted session.
- `GET /players/me` - JWT required; returns player currencies, active vehicle, owned vehicles, researched vehicles, and active/owned vehicle `enginePower` and `traverseAcceleration` values.
- `GET /players/me/statistics` - JWT required; aggregates the current player's persisted `MatchParticipant` history into overall battles/wins/losses/draws, win rate, kills, damage, reward totals, current account values, and per-played-vehicle statistics.
- `PUT /players/me/active/{vehicleId}` - JWT required; sets active owned vehicle.
- `GET /user-vehicles/me` - JWT required; returns free XP, owned vehicles, and each vehicle's `enginePower` and `traverseAcceleration`.
- `PUT /user-vehicles/me/active/{vehicleId}` - JWT required; sets active owned vehicle.
- `POST /user-vehicles/me/buy/{code}` - JWT required; buys a researched visible vehicle by catalog code using Bolts; hidden development-tree vehicles return `400`.
- `POST /user-vehicles/me/sell/{vehicleId}` - JWT required; sells owned vehicle for half purchase cost.
- `POST /user-vehicles/research/{vehicleId}` - JWT required; unlocks a visible vehicle, spending predecessor XP when required; hidden development-tree vehicles return `400`.
- `POST /user-vehicles/me/add-by-code/{code}` - JWT required; debug/free add by code for visible vehicles; hidden development-tree vehicles return `400`.
- `DELETE /user-vehicles/me/{vehicleId}` - JWT required; removes owned vehicle without refund.
- `GET /user-vehicles/xp` - JWT required; returns owned vehicle XP, `enginePower`, `traverseAcceleration`, and free XP.
- `POST /user-vehicles/{vehicleId}/convert-freexp` - JWT required; body `{ amount }`; moves free XP to an owned vehicle.
- `GET /vehicles?faction=&branch=` - public; vehicle catalog filter, including `enginePower`, `traverseAcceleration`, and `isVisible`; hidden development-tree vehicles are included.
- `GET /vehicles/{id}` - public; vehicle details, including `enginePower` and `traverseAcceleration`.
- `GET /vehicles/by-code/{code}` - public; vehicle details by code, including `enginePower` and `traverseAcceleration`.
- `GET /vehicles/{id}/research-from` - public; predecessor requirements for successor vehicle.
- `POST /vehicles/links` - admin policy required; creates tech tree link.
- `DELETE /vehicles/links/{id}` - admin policy required; deletes tech tree link.
- `GET /vehicles/graph?faction=` - public; visible and hidden tech tree nodes, including `enginePower`, `traverseAcceleration`, and `isVisible`, plus edges.
- `POST /matches/start` - Dedicated Server policy required; body `{ map, participants: [{ userId, vehicleId, team }] }`; the API validates users/owned vehicles and binds the match to the authenticated server.
- `POST /matches/{matchId}/end` - Dedicated Server policy required; server ownership and the stored roster are checked, rewards are transactional, and repeated calls return the already-finalized result without double rewards.
- `GET /matches/{matchId}/participants` - JWT required; lists the same stored participant-result fields. A `rewardsCalculated: false` item has no authoritative per-match currency breakdown and must not be displayed as a zero reward.
- `GET /maps` - public; lists maps.
- `GET /leaderboard/mmr?top=10` - JWT required; top players by MMR.
- `GET /leaderboard/free-xp?top=10` - JWT required; top players by free XP.
- `GET /leaderboard/vehicle-xp?top=10` - JWT required; top user vehicles by XP.
- `GET /unity-server/status` - anonymous; returns registered servers whose heartbeat is within `DedicatedServers:HeartbeatTimeoutSeconds`.
- `POST /unity-server/register` - Dedicated Server policy; creates/updates only the authenticated server record.
- `PUT /unity-server/{serverId}/heartbeat` - Dedicated Server policy; route id must match the authenticated server id.
- `POST /unity-server/{serverId}/unregister` - Dedicated Server policy; marks only the authenticated server offline.
- `GET /`, `GET /admin`, `GET /admin/logs`, `GET /admin/logs/stream`, `GET /admin/logs/snapshot`, `POST /admin/logs/clear` - admin role required.

## Tests
- No checked-in test project exists. Local integration smoke checks cover the security-sensitive contracts and are listed in `SECURITY_HARDENING_REPORT.md`.

## Notes For Future Changes
- Public route changes require updating this file and the matching domain doc.
- `/players/me/statistics` is read-only and current-user scoped; it does not create an aggregate table because `MatchParticipant` is the source of truth.
- Unity is not a browser client; CORS is not an authentication mechanism. Keep server credentials and player tokens on their intended sides of the contract.
