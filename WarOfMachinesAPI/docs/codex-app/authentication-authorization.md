# Authentication And Authorization

## Current Behavior
Users register and login with username/password over the HTTPS API. Passwords are hashed with BCrypt. Successful auth creates a persisted `UserSession`, returns a short-lived access JWT plus a rotating opaque refresh token, and includes a session id (`sid`). JWT validation requires signature, issuer, audience, lifetime, and an active matching PostgreSQL session; logout revokes the session. Refresh tokens are stored only as SHA-256 verification hashes.

Dedicated Servers use a separate `DedicatedServer` authentication scheme with `X-Server-Id` and `X-Server-Credential` headers. Server credentials are validated against a stored hash or protected bootstrap configuration and receive a `dedicatedServer` policy, never a player role. Connection tickets are opaque, 32-byte random values, stored only as hashes, bound to user/session/server, short-lived, and atomically redeemed once. Redemption returns a short-lived delegated player token to the server over HTTPS for existing profile reads; it does not grant server-only policy access.

## Owner Files
- `Program.cs` - JWT bearer registration/validation, active-session check, auth schemes and policies.
- `Controllers/AuthController.cs` - register/login, refresh rotation, logout, DTO bounds, BCrypt password handling, starter vehicle auto-heal.
- `Controllers/ConnectionTicketsController.cs` - issue and atomic redeem of server-bound tickets.
- `Infrastructure/DedicatedServerAuthenticationHandler.cs` and `DedicatedServerCredentialValidator.cs` - separate server identity validation.
- `Infrastructure/JwtTokenService.cs` - configured access/delegated JWT issuance.
- `Models/Player.cs` - user auth/account fields: `Username`, `PasswordHash`, `IsAdmin`, currencies, profile state.
- `Data/AppDbContext.cs` - `Players` DbSet.

## Important Configuration
- `Jwt:Key` / `Jwt__Key` - startup-required signing key in `Program.cs`.
- There is no hardcoded JWT fallback. Startup fails when `Jwt:Key` is absent or shorter than 32 characters.
- `RequireHttpsMetadata = true`; issuer, audience, signature, lifetime, and signing key are all validated with 30-second clock skew.
- `Authentication:RefreshTokenLifetimeDays`, `Authentication:ConnectionTicketLifetimeSeconds`, and `Authentication:ServerDelegatedTokenMinutes` bound session/ticket lifetimes.

## Dependencies
- `BCrypt.Net-Next` hashes and verifies passwords.
- `System.IdentityModel.Tokens.Jwt` creates tokens.
- `Microsoft.AspNetCore.Authentication.JwtBearer` validates bearer tokens.
- Database tables/entities: `Players`, `UserVehicles`, `UserVehicleResearches`, `Vehicles`.

## Routes / Contracts
- `POST /auth/register` and `POST /auth/login` return `{ token, refreshToken, expiresAtUtc }`; both are rate-limited and validate username/password bounds. Registration requires a 3–32 character username and an 8–128 character password; login accepts existing 1–32 character usernames for compatibility with legacy local accounts and requires a non-empty 1–128 character password.
- `POST /auth/refresh` rotates the refresh token; the previous value is rejected. `POST /auth/logout` revokes the current `sid` session.
- `POST /connection-tickets` requires a player JWT. `POST /connection-tickets/redeem` requires the Dedicated Server policy.
- `/matches/start`, `/matches/{id}/end`, and Dedicated Server register/heartbeat/unregister require the Dedicated Server policy; player JWTs are rejected.
- Admin pages/log endpoints require the `admin` role. Vehicle link mutations require the `admin` policy.

## Tests
- No checked-in test project exists. Local smoke coverage for register/profile, refresh rotation, logout revocation, server identity, ticket replay/concurrency, and server-only match access is recorded in `SECURITY_HARDENING_REPORT.md`.

## Notes For Future Changes
- Do not change JWT validation, token lifetime, claims, or protected/public route boundaries without updating this doc and `api-endpoints.md`.
- Login logs contain only the submitted username; passwords, JWTs, refresh tokens, tickets, and server credentials must never be logged.
