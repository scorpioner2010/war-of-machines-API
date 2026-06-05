# API Endpoints

## Current Behavior
The API uses attribute-routed controllers plus a few minimal admin routes. JSON output is camelCase and ignores null values. JWT auth protects most player/progression routes; catalog, maps, auth, Unity server status, Swagger, and admin pages are public.

## Owner Files
- `Program.cs` - maps controllers, root redirect, admin minimal routes, Swagger UI.
- `Controllers/AuthController.cs` - `/auth` registration and login.
- `Controllers/PlayersController.cs` - `/players` authenticated profile and active vehicle routes.
- `Controllers/UserVehiclesController.cs` - `/user-vehicles` authenticated ownership, research, purchase, sell, XP routes.
- `Controllers/VehiclesController.cs` - `/vehicles` catalog and tech tree link routes.
- `Controllers/MatchesController.cs` - `/matches` authenticated match start/end/history routes.
- `Controllers/MapsController.cs` - `/maps` public map listing.
- `Controllers/LeaderboardController.cs` - `/leaderboard` authenticated rankings.
- `Controllers/UnityServerStatusController.cs` - `/unity-server` public heartbeat/status routes.

## Important Configuration
- `Jwt:Key` - required for protected endpoints.
- CORS policy `any` - public cross-origin access for all methods/headers.
- Swagger is always enabled at `/swagger`.

## Dependencies
- Controllers depend directly on `AppDbContext` except `UnityServerStatusController`, which depends on `UnityServerStatusStore`.
- `[ApiController]` provides automatic model binding behavior for controller actions.
- `[Authorize]` is set at controller level for players, user vehicles, matches, and leaderboard.

## Routes / Contracts
- `POST /auth/register` - anonymous; body `{ username, password }`; creates player, starter vehicle/research, returns `{ token }`.
- `POST /auth/login` - anonymous; body `{ username, password }`; verifies password, auto-heals starter vehicle, returns `{ token }`.
- `GET /players/me` - JWT required; returns player currencies, active vehicle, owned vehicles, researched vehicles.
- `PUT /players/me/active/{vehicleId}` - JWT required; sets active owned vehicle.
- `GET /user-vehicles/me` - JWT required; returns free XP and owned vehicles.
- `PUT /user-vehicles/me/active/{vehicleId}` - JWT required; sets active owned vehicle.
- `POST /user-vehicles/me/buy/{code}` - JWT required; buys researched vehicle by catalog code using Bolts.
- `POST /user-vehicles/me/sell/{vehicleId}` - JWT required; sells owned vehicle for half purchase cost.
- `POST /user-vehicles/research/{vehicleId}` - JWT required; unlocks a vehicle, spending predecessor XP when required.
- `POST /user-vehicles/me/add-by-code/{code}` - JWT required; debug/free add by code.
- `DELETE /user-vehicles/me/{vehicleId}` - JWT required; removes owned vehicle without refund.
- `GET /user-vehicles/xp` - JWT required; returns owned vehicle XP and free XP.
- `POST /user-vehicles/{vehicleId}/convert-freexp` - JWT required; body `{ amount }`; moves free XP to an owned vehicle.
- `GET /vehicles?faction=&branch=` - public; vehicle catalog filter.
- `GET /vehicles/{id}` - public; vehicle details.
- `GET /vehicles/by-code/{code}` - public; vehicle details by code.
- `GET /vehicles/{id}/research-from` - public; predecessor requirements for successor vehicle.
- `POST /vehicles/links` - public; creates tech tree link.
- `DELETE /vehicles/links/{id}` - public; deletes tech tree link.
- `GET /vehicles/graph?faction=` - public; tech tree nodes and edges.
- `POST /matches/start` - JWT required; body `{ map }`; creates match.
- `POST /matches/{matchId}/end` - JWT required; submits participants and awards progression.
- `GET /matches/{matchId}/participants` - JWT required; lists stored participant results.
- `GET /maps` - public; lists maps.
- `GET /leaderboard/mmr?top=10` - JWT required; top players by MMR.
- `GET /leaderboard/free-xp?top=10` - JWT required; top players by free XP.
- `GET /leaderboard/vehicle-xp?top=10` - JWT required; top user vehicles by XP.
- `GET /unity-server/status` - public; returns current Unity server status.
- `POST /unity-server/status` - public; updates Unity server heartbeat/status.
- `GET /`, `GET /admin`, `GET /admin/logs`, `GET /admin/logs/stream`, `GET /admin/logs/snapshot`, `POST /admin/logs/clear` - public admin/minimal routes.

## Tests
- No automated API tests were found.
- `WarOfMachines-api.http` contains manual register/login examples only.

## Notes For Future Changes
- Public route changes require updating this file and the matching domain doc.
- Several mutating catalog/admin routes are currently public; changing auth behavior requires updating `authentication-authorization.md`.
