# Matches And Rewards

## Current Behavior
Authenticated clients can start a match and later submit final participant results once. Ending a match validates participants, rejects duplicate users, rejects unknown users, prevents double-submission, clamps kills/damage, computes XP, Bolts, MMR delta, and Free XP, records `MatchParticipant` rows, updates player currencies/MMR, and adds XP to the submitted owned vehicle if found.

## Owner Files
- `Controllers/MatchesController.cs` - match start/end/history routes, reward constants, participant validation, reward calculation, transactions.
- `Models/Match.cs` - match map and start/end timestamps.
- `Models/MatchParticipant.cs` - per-user match result, vehicle, team, kills, damage, XP, MMR delta.
- `Models/Player.cs` - MMR, Bolts, Free XP updated by match results.
- `Models/UserVehicle.cs` - owned vehicle XP updated by match results.
- `Data/AppDbContext.cs` - match participant relationships and delete behavior.
- `Data/SeedData.cs` - seeds one demo match and participant when the database is empty.

## Important Configuration
- JWT auth is required for all `/matches` routes.
- Reward constants live in `MatchesController`, not configuration.

## Dependencies
- Database entities: `Matches`, `MatchParticipants`, `Players`, `Vehicles`, `UserVehicles`.
- EF Core execution strategy and transaction are used when ending a match.
- Current user claim is available but `EndMatch` trusts participant `UserId` values from the request body.

## Routes / Contracts
- `POST /matches/start` body `{ map }`; empty map becomes `default_map`; returns `{ matchId }`.
- `POST /matches/{matchId:int}/end` body `{ participants: [{ userId, vehicleId, team, result, kills, damage }] }`; returns `{ ok = true }`.
- `GET /matches/{matchId:int}/participants` returns stored participant result DTOs with username and vehicle name.

## Tests
- No automated match/reward tests were found.

## Notes For Future Changes
- Reward and anti-cheat constants are embedded in `MatchesController`; changing economy numbers requires updating this doc.
- Add tests before changing reward math, double-submit behavior, participant validation, or MMR calculation.
- If result submission should be limited to admins/servers, update auth docs because current JWT users can submit arbitrary participant IDs.
