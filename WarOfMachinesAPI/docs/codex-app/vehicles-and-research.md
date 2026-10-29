# Vehicles And Research

## Current Behavior
The vehicle catalog is public and stores vehicles by unique code with faction, branch, class, level, purchase cost, combat stats, armor strings, and visibility. `TraverseAcceleration` controls how quickly the robot body reaches `TraverseSpeed` when it starts turning. Research links model predecessor-to-successor unlocks with XP cost on the predecessor. The catalog currently contains 21 seeded robots: 4 visible robots per faction plus 6 hidden Iron Alliance robots and 7 hidden Nova Syndicate robots at levels 3 and 4. Hidden robots remain in the public catalog and graph for the development tree, but cannot be researched, bought, or debug-added.

Seeded vehicle combat stats are owned by `Data/SeedData.cs` and are written idempotently on startup. Current seeded `Accuracy` values are balanced from `0.8` to `2.0`, where lower values are more accurate. Current seeded `Penetration` values are balanced from `90` to `135`. `ShellSpeed` is linearly balanced from `90` to `150` while preserving the robots' original order. `Speed` is balanced from `1.26` to `5.616`: the global 50% reduction remains in effect, while `IA Strider` has a 30% increase from its reduced value. All seeded `Acceleration` values are reduced by half again and now range from `0.275` to `1.0`; all `TraverseAcceleration` values are 100 times their corresponding `Acceleration` and range from `27.5` to `100`. Iron Alliance `TurretTraverseSpeed` values are the average of their original and reduced values, ranging from `11.25` to `24`; Nova Syndicate turret-turn values are reduced by 15% to `51.85`–`68`. `IA Strider` damage is reduced by 30% to `25.2`–`37.8`.

`robot-characteristics.md` is the Ukrainian player-facing reference for all currently seeded robots. It groups the same catalog values by role, survivability, weapon, and movement, and must be updated when seeded vehicle stats or research costs change.

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
- `GET /vehicles?faction=iron_alliance&branch=tracked` returns catalog DTOs, including `traverseAcceleration` and `isVisible`; hidden catalog robots are intentionally included.
- `GET /vehicles/{id:int}` and `GET /vehicles/by-code/{code}` return a vehicle DTO, including `traverseAcceleration`, or `404`.
- `GET /vehicles/{id:int}/research-from` returns predecessor IDs and required XP for a successor.
- `POST /vehicles/links` creates a research link and rejects self-links, missing vehicles, and duplicates.
- `DELETE /vehicles/links/{id:int}` deletes a research link.
- `GET /vehicles/graph?faction=iron_alliance` returns visible and hidden `nodes`, including `traverseAcceleration` and `isVisible`, plus `edges` for the development tree.
- `GET /user-vehicles/me` and `GET /user-vehicles/xp` return owned vehicle XP/research state, `traverseAcceleration`, and player Free XP.
- `PUT /user-vehicles/me/active/{vehicleId:int}` sets active owned vehicle transactionally.
- `POST /user-vehicles/me/buy/{code}` buys a researched visible vehicle with Bolts; hidden vehicles return `400 Vehicle is not available.`
- `POST /user-vehicles/me/sell/{vehicleId:int}` sells a vehicle for half purchase cost and prevents selling the last vehicle.
- `POST /user-vehicles/research/{vehicleId:int}` researches a visible vehicle, spending predecessor XP when requirements exist; hidden vehicles return `400 Vehicle is not available.`
- `POST /user-vehicles/me/add-by-code/{code}` debug-adds a visible vehicle and marks it researched; hidden vehicles are rejected as unavailable.
- `DELETE /user-vehicles/me/{vehicleId:int}` removes a vehicle without refund and prevents removing the last vehicle.
- `POST /user-vehicles/{vehicleId:int}/convert-freexp` moves positive Free XP amount to an owned vehicle.

## Tests
- No automated vehicle/research tests were found.

## Notes For Future Changes
- Keep active vehicle changes compatible with `IX_UserVehicles_UserId_IsActive`; clear old active state before setting new active state.
- `POST /vehicles/links` and `DELETE /vehicles/links/{id}` are currently public; add docs/tests if auth is introduced.
- Update `database-ef-core.md` whenever vehicle stats, research relationships, indexes, or seed data change.
