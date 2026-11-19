# Validation And Error Handling

## Current Behavior
Validation combines DataAnnotations/[ApiController] model validation with explicit controller checks. Request bodies are capped at 256 KB, auth requests are rate-limited, and unhandled exceptions are converted to generic ProblemDetails by `ApiExceptionHandler` without returning exception details to clients. Existing route-specific plain-string errors remain for compatibility.

## Owner Files
- `Program.cs` - configures controllers, request limits, rate limiting, and exception handling.
- `Infrastructure/ApiExceptionHandler.cs` - generic 500 ProblemDetails response and server-side structured logging.
- `Controllers/AuthController.cs` - separate registration and login username/password bounds, duplicate username conflict, credential failures.
- `Controllers/MatchesController.cs` - participant required checks, duplicate participant checks, missing user checks, stat clamps, result normalization.
- `Controllers/UserVehiclesController.cs` - ownership, currency, last-vehicle, research, and free XP amount checks.
- `Controllers/VehiclesController.cs` - research link self-link, missing vehicle, duplicate link, and delete checks.
- `Controllers/PlayersController.cs` - current-player lookup and active vehicle ownership checks.
- `Controllers/UnityServerStatusController.cs` - server id, address, port, capacity, heartbeat and status validation.
- `Controllers/ConnectionTicketsController.cs` - ticket length, session state, expiry, replay and transaction/concurrency validation.
- `Models/*.cs` - DataAnnotations for some required and range-constrained properties.

## Important Configuration
- `[ApiController]` is applied to every controller.
- JSON output is camelCase and omits null values.
- `UseExceptionHandler` is configured through `IExceptionHandler`; no external validation library is required.

## Dependencies
- Several endpoints rely on EF Core constraints after manual validation.
- Match ending and ticket redemption use EF execution strategies and transactions.
- Missing `Jwt:Key` or database connection string throws during startup before requests are served.
- Protected controllers parse `ClaimTypes.NameIdentifier` directly as an integer current-player id.

## Routes / Contracts
- Common statuses: `400` for invalid input, `401` for login failure, `404` for missing rows, `409` for duplicates/conflicts, `204` for successful research-link deletion.
- Error bodies are route-specific and often plain strings, not a shared DTO.

## Tests
- No checked-in validation test project exists. Local smoke tests cover invalid server credentials, replay/concurrency, player-JWT server denial, and session revocation.

## Notes For Future Changes
- If adding a standard error envelope or Problem Details behavior, update `api-endpoints.md`, affected domain docs, and clients.
- If adding validation attributes or validators, document whether behavior is automatic `[ApiController]` validation or manual action checks.
- Do not silently change status codes or plain-string error bodies; clients may depend on them.
