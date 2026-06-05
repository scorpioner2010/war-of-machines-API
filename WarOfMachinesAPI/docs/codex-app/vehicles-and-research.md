# Vehicles And Research

## Current Behavior
The vehicle catalog is public and stores vehicles by unique code with faction, branch, class, level, purchase cost, combat stats, armor strings, and visibility. Research links model predecessor-to-successor unlocks with XP cost on the predecessor. Authenticated users can research vehicles, buy researched vehicles with Bolts, sell vehicles for half purchase cost, debug-add vehicles for free, remove vehicles, set active vehicles, and convert Free XP to owned vehicle XP.

## Owner Files
- `Controllers/VehiclesController.cs` - public vehicle catalog, details, tech tree link management, and graph endpoints.
- `Controllers/UserVehiclesController.cs` - authenticated ownership, purchase, sell, research, debug add, remove, active vehicle, and XP conversion endpoints.
- `Models/Vehicle.cs` - vehicle catalog entity and combat/economy fields.
- `Models/VehicleClass.cs` - `Scout`, `Guardian`, `Colossus` enum.
- `Models/Faction.cs` - faction catalog entity.
- `Models/VehicleResearchRequirement.cs` - predecessor/successor XP requirement entity.
- `Models/UserVehicle.cs` - owned vehicle with active flag and XP.
- `Models/UserVehicleResearch.cs` - researched unlock record.
- `Data/SeedData.cs` - idempotently seeds factions, vehicles, and research links.
- `Data/AppDbContext.cs` - vehicle relationships, check constraints, indexes, and delete behavior.

## Important Configuration
- JWT auth is required for `/user-vehicles/*`; `/vehicles/*` is currently public.
- Database connection settings are required because all behavior is EF Core-backed.

## Dependencies
- Database tables/entities: `Vehicles`, `Factions`, `VehicleResearchRequirements`, `UserVehicles`, `UserVehicleResearches`, `Players`.
- Unique indexes: vehicle code, predecessor/successor link pair, user/vehicle ownership, user/vehicle research.
- Filtered unique index: one active `UserVehicle` per user where `IsActive = TRUE`.
- Check constraints: positive `ShellSpeed`, positive `ViewRange`, non-negative `ShellsCount`, `DamageMin <= DamageMax`.
- Purchase depends on player `Bolts`; research depends on predecessor ownership and XP.

## Routes / Contracts
- `GET /vehicles?faction=iron_alliance&branch=tracked` returns catalog DTOs.
- `GET /vehicles/{id:int}` and `GET /vehicles/by-code/{code}` return a vehicle DTO or `404`.
- `GET /vehicles/{id:int}/research-from` returns predecessor IDs and required XP for a successor.
- `POST /vehicles/links` creates a research link and rejects self-links, missing vehicles, and duplicates.
- `DELETE /vehicles/links/{id:int}` deletes a research link.
- `GET /vehicles/graph?faction=iron_alliance` returns `nodes` and `edges`.
- `GET /user-vehicles/me` and `GET /user-vehicles/xp` return owned vehicle XP/research state and player Free XP.
- `PUT /user-vehicles/me/active/{vehicleId:int}` sets active owned vehicle transactionally.
- `POST /user-vehicles/me/buy/{code}` buys a researched vehicle with Bolts.
- `POST /user-vehicles/me/sell/{vehicleId:int}` sells a vehicle for half purchase cost and prevents selling the last vehicle.
- `POST /user-vehicles/research/{vehicleId:int}` researches a vehicle, spending predecessor XP when requirements exist.
- `POST /user-vehicles/me/add-by-code/{code}` debug-adds a vehicle and marks it researched.
- `DELETE /user-vehicles/me/{vehicleId:int}` removes a vehicle without refund and prevents removing the last vehicle.
- `POST /user-vehicles/{vehicleId:int}/convert-freexp` moves positive Free XP amount to an owned vehicle.

## Tests
- No automated vehicle/research tests were found.

## Notes For Future Changes
- Keep active vehicle changes compatible with `IX_UserVehicles_UserId_IsActive`; clear old active state before setting new active state.
- `POST /vehicles/links` and `DELETE /vehicles/links/{id}` are currently public; add docs/tests if auth is introduced.
- Update `database-ef-core.md` whenever vehicle stats, research relationships, indexes, or seed data change.
