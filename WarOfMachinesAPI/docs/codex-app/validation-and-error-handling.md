# Validation And Error Handling

## Current Behavior
Validation is mostly manual inside controllers, with `[ApiController]` providing normal ASP.NET Core model binding and model validation behavior. Many failure responses are plain strings returned with route-specific status codes. There is no global exception handler, custom filter, FluentValidation setup, or shared error response envelope.

## Owner Files
- `Program.cs` - configures controllers and JSON options; no custom global exception middleware is configured.
- `Controllers/AuthController.cs` - username/password checks, duplicate username conflict, credential failures.
- `Controllers/MatchesController.cs` - participant required checks, duplicate participant checks, missing user checks, stat clamps, result normalization.
- `Controllers/UserVehiclesController.cs` - ownership, currency, last-vehicle, research, and free XP amount checks.
- `Controllers/VehiclesController.cs` - research link self-link, missing vehicle, duplicate link, and delete checks.
- `Controllers/PlayersController.cs` - current-player lookup and active vehicle ownership checks.
- `Controllers/UnityServerStatusController.cs` - heartbeat address normalization and port range validation.
- `Models/*.cs` - DataAnnotations for some required and range-constrained properties.

## Important Configuration
- `[ApiController]` is applied to every controller.
- JSON output is camelCase and omits null values.
- No custom `UseExceptionHandler`, exception filter, validation library, or Problem Details customization was found.

## Dependencies
- Several endpoints rely on EF Core constraints after manual validation.
- Match ending and active-vehicle changes use EF execution strategies and transactions.
- Missing `Jwt:Key` or database connection string throws during startup before requests are served.
- Protected controllers parse `ClaimTypes.NameIdentifier` directly as an integer current-player id.

## Routes / Contracts
- Common statuses: `400` for invalid input, `401` for login failure, `404` for missing rows, `409` for duplicates/conflicts, `204` for successful research-link deletion.
- Error bodies are route-specific and often plain strings, not a shared DTO.

## Tests
- No automated validation or error-contract tests were found.

## Notes For Future Changes
- If adding a standard error envelope or Problem Details behavior, update `api-endpoints.md`, affected domain docs, and clients.
- If adding validation attributes or validators, document whether behavior is automatic `[ApiController]` validation or manual action checks.
- Do not silently change status codes or plain-string error bodies; clients may depend on them.
