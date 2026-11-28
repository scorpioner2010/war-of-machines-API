# Vehicles And Research

## Current Behavior
The vehicle catalog is public and stores vehicles by unique code with faction, branch, class, level, purchase cost, combat stats, armor strings, and visibility. `EnginePower` is a positive conditional engine-output stat exposed to clients; it is balanced independently from `Speed` and `Acceleration`. `TraverseAcceleration` controls how quickly the robot body reaches `TraverseSpeed` when it starts turning. Research links model predecessor-to-successor unlocks with XP cost on the predecessor. The catalog currently contains 21 seeded robots: 4 visible robots per faction plus 6 hidden Iron Alliance robots and 7 hidden Nova Syndicate robots at levels 3 and 4. Hidden robots remain in the public catalog and graph for the development tree, but cannot be researched, bought, or debug-added.

Seeded vehicle combat stats are owned by `Data/SeedData.cs` and are written idempotently on startup. Current seeded `Accuracy` values are balanced from `0.74` to `2.0`, where lower values are more accurate. `ShellSpeed` is balanced from `90` to `150` for the seeded catalog. `Speed` ranges from `1.25` to `5.95`: the global 50% reduction remains in effect, while `TZ-23 101F` has a 30% increase from its reduced value. All seeded `Acceleration` values are reduced by half again and range from `0.27` to `1.0`; `EnginePower` ranges from `700` to `1700`, with `+1000` applied to all 11 Nova Syndicate biped robots while Iron Alliance tracked robots retain their existing values. All `TraverseAcceleration` values are 100 times their corresponding `Acceleration` and range from `26` to `100`. Iron Alliance `TurretTraverseSpeed` values range from `10.5` to `25`; Nova Syndicate turret-turn values range from `44` to `78`. `TZ-23 101F` damage remains reduced by 30% to `25.2`–`37.8`.

`robot-characteristics.md` is the Ukrainian player-facing reference for all currently seeded robots. It groups the same catalog values by role, survivability, weapon, and movement, and must be updated when seeded vehicle stats or research costs change.

The 13 hidden T3/T4 concepts use a role-aware tier curve rather than a flat multiplier. Sustained DPS targets are 39.5–45 for T3 scouts, 45 for T3 guardians, and 45–48 for T3 colossi; T4 targets are 54 for scouts, 56.25 for guardians, and 54 for colossi. HP, armor, penetration, and sight range scale with tier inside each role. Scouts retain weaker armor and stronger mobility/vision; colossi retain the strongest durability and shot damage with slower handling. The reversible `20261128150000_RebalanceHiddenRobotStats` migration updates only these hidden catalog stats and refuses to run if the expected 13 codes, levels, classes, visibility, or prior stat values have drifted.

## Current Development Tree

The catalog has 21 robots and 19 research links. Required XP values are unchanged and remain attached to each destination robot's research link.

### Iron Alliance

- TZ-18 (T1) → TZ-23 101F (T2) → TZ-42 (T2) → YUG Titan (T2)
- TZ-18 (T1) → BR-160 (T3) → BR-174 Deadlock (T3) → R30-Halberd (T3)
- BR-160 (T3) → R42-Glaive 7B (T3) → V-88/R Northstar (T4) → Blackline-09 Mk. II (T4)

### Nova Syndicate

- N-16 Whitecap (T1) → N-17 (T2) → ARGUS-7B (T2) → Cormorant 8C (T2) → ZR-22 Knifepoint (T3)
- N-16 Whitecap (T1) → P-8 Null Compass (T3) → VX-44/2 (T3) → M-03 Sundown (T4)
- VX-44/2 (T3) → 77-V Seeker (T4)
- N-16 Whitecap (T1) → A-93 Graveward (T3) → S-404 Firebreak (T3)

Vehicle names are the current machine designations. Stable vehicle `Code` values remain unchanged and continue to identify ownership, research progress, and Unity prefab mappings. The API remains the source of truth for each vehicle's name, level, class, price, and combat characteristics; Unity resolves playable prefabs by `Code` and reads displayed catalog data from the API.

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
- Check constraints: positive `ShellSpeed`, positive `ViewRange`, positive `EnginePower`, non-negative `ShellsCount`, `DamageMin <= DamageMax`.
- Purchase depends on player `Bolts`; research depends on predecessor ownership and XP.

## Routes / Contracts
- `GET /vehicles?faction=iron_alliance&branch=tracked` returns catalog DTOs, including `enginePower`, `traverseAcceleration`, and `isVisible`; hidden catalog robots are intentionally included.
- `GET /vehicles/{id:int}` and `GET /vehicles/by-code/{code}` return a vehicle DTO, including `enginePower` and `traverseAcceleration`, or `404`.
- `GET /vehicles/{id:int}/research-from` returns predecessor IDs and required XP for a successor.
- `POST /vehicles/links` creates a research link for an authenticated admin and rejects self-links, missing vehicles, and duplicates.
- `DELETE /vehicles/links/{id:int}` deletes a research link for an authenticated admin.
- `GET /vehicles/graph?faction=iron_alliance` returns visible and hidden `nodes`, including `enginePower`, `traverseAcceleration`, and `isVisible`, plus `edges` for the development tree.
- `GET /user-vehicles/me` and `GET /user-vehicles/xp` return owned vehicle XP/research state, `enginePower`, `traverseAcceleration`, and player Free XP.
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
- Vehicle link mutations require the `admin` policy; normal player JWTs cannot change the research graph.
- Update `database-ef-core.md` whenever vehicle stats, research relationships, indexes, or seed data change.
