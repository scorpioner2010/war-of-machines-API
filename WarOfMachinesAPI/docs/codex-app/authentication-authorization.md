# Authentication And Authorization

## Current Behavior
Users register and login with username/password. Passwords are hashed with BCrypt. Successful auth returns a 7 day JWT containing name identifier, username, and role (`admin` or `user`). JWT bearer validation checks lifetime and signing key but does not validate issuer or audience. Protected controllers use `[Authorize]`; anonymous routes are explicitly allowed for `/auth/register` and `/auth/login`.

## Owner Files
- `Program.cs` - JWT bearer registration, signing key creation, auth/authorization middleware order.
- `Controllers/AuthController.cs` - register/login request DTOs, BCrypt password hashing/verification, JWT issuance, starter vehicle auto-heal.
- `Models/Player.cs` - user auth/account fields: `Username`, `PasswordHash`, `IsAdmin`, currencies, profile state.
- `Data/AppDbContext.cs` - `Players` DbSet.

## Important Configuration
- `Jwt:Key` / `Jwt__Key` - startup-required signing key in `Program.cs`.
- `AuthController` also has a fallback string if config is missing, but normal startup prevents missing `Jwt:Key`.
- `RequireHttpsMetadata = false` in JWT bearer options.
- `ValidateIssuer = false` and `ValidateAudience = false`.

## Dependencies
- `BCrypt.Net-Next` hashes and verifies passwords.
- `System.IdentityModel.Tokens.Jwt` creates tokens.
- `Microsoft.AspNetCore.Authentication.JwtBearer` validates bearer tokens.
- Database tables/entities: `Players`, `UserVehicles`, `UserVehicleResearches`, `Vehicles`.

## Routes / Contracts
- `POST /auth/register` with `{ username, password }`.
- `POST /auth/login` with `{ username, password }`.
- Both return `{ token }` on success.
- Protected controllers currently include `PlayersController`, `UserVehiclesController`, `MatchesController`, and `LeaderboardController`.
- Public controllers/routes currently include `VehiclesController`, `MapsController`, `UnityServerStatusController`, Swagger, and admin minimal endpoints.

## Tests
- No automated auth tests were found.
- `WarOfMachines-api.http` has manual register/login examples.

## Notes For Future Changes
- Do not change JWT validation, token lifetime, claims, or protected/public route boundaries without updating this doc and `api-endpoints.md`.
- Register/login log messages must not include passwords; currently registration logs the request object, so inspect logging risk before expanding auth logs.
