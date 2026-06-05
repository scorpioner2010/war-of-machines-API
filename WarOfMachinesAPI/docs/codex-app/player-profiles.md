# Player Profiles

## Current Behavior
Authenticated players can read their own profile and set an active owned vehicle. The profile response includes account flags, currencies, active vehicle identity/stats, owned vehicles with XP and research status, and researched vehicle list. Active vehicle changes are transactional and clear the previous active vehicle before setting the new one to avoid the filtered unique active-vehicle index.

## Owner Files
- `Controllers/PlayersController.cs` - `GET /players/me`, `PUT /players/me/active/{vehicleId}`, profile DTOs.
- `Models/Player.cs` - player identity, admin flag, MMR, Bolts, Adamant, Free XP, owned vehicle navigation.
- `Models/UserVehicle.cs` - ownership, active flag, per-vehicle XP.
- `Models/UserVehicleResearch.cs` - researched vehicle unlock records.
- `Data/AppDbContext.cs` - user vehicle uniqueness and active-vehicle filtered unique index.

## Important Configuration
- JWT auth must be configured because all `/players` routes require `[Authorize]`.

## Dependencies
- Database entities: `Players`, `UserVehicles`, `UserVehicleResearches`, `Vehicles`.
- Claim dependency: `ClaimTypes.NameIdentifier` is parsed as the current player ID.
- EF Core execution strategy and transaction are used when switching active vehicle.

## Routes / Contracts
- `GET /players/me` returns profile fields including `id`, `username`, `isAdmin`, `mmr`, `bolts`, `adamant`, `freeXp`, active vehicle fields, `ownedVehicles`, and `researchedVehicles`.
- `PUT /players/me/active/{vehicleId}` returns `{ ok = true, activeVehicleId }` or `404` when the user does not own the vehicle.

## Tests
- No automated profile tests were found.

## Notes For Future Changes
- `UserVehiclesController` has a second active-vehicle endpoint with similar behavior; keep both routes consistent or intentionally consolidate them.
- Any change to profile response shape must update `api-endpoints.md` and any client/admin docs that consume it.
