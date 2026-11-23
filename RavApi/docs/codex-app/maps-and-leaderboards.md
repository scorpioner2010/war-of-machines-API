# Maps And Leaderboards

## Current Behavior
Maps are public catalog data returned as ID, code, name, and description. Leaderboards are authenticated and return top players by MMR, top players by Free XP, or top owned vehicles by vehicle XP.

## Owner Files
- `Controllers/MapsController.cs` - `GET /maps` public map list.
- `Controllers/LeaderboardController.cs` - authenticated MMR, Free XP, and vehicle XP rankings.
- `Models/Map.cs` - map entity.
- `Models/Player.cs` - MMR and Free XP leaderboard source.
- `Models/UserVehicle.cs` - vehicle XP leaderboard source.
- `Data/SeedData.cs` - seeds `demo_map` and `steel_arena` if no maps exist.
- `Data/AppDbContext.cs` - map unique code index and required fields.

## Important Configuration
- `/leaderboard/*` requires JWT auth.
- `/maps` is public.

## Dependencies
- Database entities: `Maps`, `Players`, `UserVehicles`, `Vehicles`.
- Leaderboard results depend on seeded/users-created player and vehicle data.

## Routes / Contracts
- `GET /maps` returns map DTOs with `id`, `code`, `name`, `description`.
- `GET /leaderboard/mmr?top=10` returns `userId`, `username`, `value`.
- `GET /leaderboard/free-xp?top=10` returns `userId`, `username`, `value`.
- `GET /leaderboard/vehicle-xp?top=10` returns anonymous DTO with `userId`, `username`, `vehicleName`, `xp`.

## Tests
- No automated map or leaderboard tests were found.

## Notes For Future Changes
- Add bounds validation for `top` if large leaderboard requests become a production concern.
- Update seed/database docs when map seed records or leaderboard source fields change.
