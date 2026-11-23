# Matches And Rewards

## Поточна поведінка
Матчі створює та завершує тільки authenticated Dedicated Server. При старті API перевіряє authoritative roster: існування користувачів, ownership vehicle, унікальність користувачів і коректні team/vehicle ids. Match зберігає `DedicatedServerId`, а `MatchParticipant` фіксує roster до початку бою.

При завершенні API звіряє server ownership, точний склад roster, team і vehicle. Kills/damage обмежуються існуючими anti-cheat clamp, а формули XP/Bolts/MMR/free-XP не змінені. Операція виконується в транзакції з concurrency token на `Match.Version`; повторний `EndMatch` повертає вже збережений результат і не нараховує нагороди вдруге.

## Власники
- `Controllers/MatchesController.cs` — server-only start/end/history routes, roster validation, transaction і reward calculations.
- `Models/Match.cs` — map, timestamps, `DedicatedServerId`, concurrency `Version`.
- `Models/MatchParticipant.cs` — authoritative roster, result, rewards, `IsFinalized`.
- `Data/AppDbContext.cs` — relationships, unique `(MatchId, UserId)` constraint та match indexes.

## Контракти
- `POST /matches/start` body `{ map, participants: [{ userId, vehicleId, team }] }`; повертає `{ matchId }`.
- `POST /matches/{matchId}/end` body `{ participants: [{ userId, vehicleId, team, result, kills, damage }] }`; повертає `{ ok, participants }` або idempotent `{ alreadyProcessed: true, participants }`.
- `GET /matches/{matchId}/participants` повертає persisted participant/reward DTO і залишається authenticated read endpoint.

Reward constants знаходяться в `MatchesController` і не змінювалися в hardening. Локально перевірені player-JWT denial для start, server start/end, roster validation і повторний end без подвійних нагород. Повний multiplayer gameplay runtime не запускався в цьому середовищі.
