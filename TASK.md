## 2026-11-26 — Netcup VPS staging deployment — IN_PROGRESS
- User request: deploy the RAV staging API and PostgreSQL integration to the newly active Netcup VPS alongside the Unity Dedicated Server; preserve production and all pre-existing changes.
- Scope: verify current API image/config and staging migrations, deploy only staging, keep PostgreSQL and API internal port private, preserve TLS and secrets handling.
- Acceptance: reachable HTTPS staging API, database readiness and auth/ticket/server registration/heartbeat integration, with limits documented.
- Current checkpoint: VPS SSH is reachable after host fingerprint match; Debian 13.8 x86_64, 4 vCPU, 8 GiB RAM; no API deployed. Existing working-tree changes must be preserved.
- Status: IN_PROGRESS; next inspect git state and current staging compose/image config before selecting deployment method.

### Ubuntu post-install access checkpoint — 2026-11-26
- Verified a password-authenticated SSH session to `root@62.83.120.40` using strict host-key checking and the dedicated known_hosts file.
- Remote output confirms `root`, `x86_64`, Ubuntu 24.04.5 LTS. Post-install SCP fingerprint had already matched the scanned ED25519 host key.
- Local Netcup Ed25519 private/public key pair is present inside the DPAPI vault JSON (`NetcupSshPrivateKey` / `NetcupSshPublicKey`); values were not displayed. This key is available for bootstrap admin access.
- No application deployment has run. Before host changes: verify keypair, patch the known placeholder in `prepare-host.sh` to consume the actual staging address/region, validate package/compose, then bootstrap and verify `ravadmin` key login before SSH hardening.
- **Status:** IN_PROGRESS.

### Runtime smoke cleanup checkpoint — 2026-11-26
- Dedicated Server now runs from the verified x86-64 staging release and listens on UDP 7770; service startup and initial health validation were separated to diagnose the first deployment rollback.
- Public HTTPS live/ready, PostgreSQL TLS, server register/heartbeat/status, temporary auth/profile/starter loadout, ticket issue/redeem/replay rejection, and backend match creation all passed.
- The smoke script EXIT cleanup then failed because it referenced undefined `RAV_STAGING_BOOTSTRAP_USER` and `RAV_STAGING_POSTGRES_DB` shell variables. The test user/match may still exist; remove only rows matching the generated `rav-smoke-<UTC timestamp>-<random>` pattern, verify zero remain, then fix script to use validated staging DB settings and rerun a cleanup validation.
- Remaining explicit skips: compatible FishNet DTLS probe and automated Unity gameplay client harness.
- Status: IN_PROGRESS.
- Follow-up: confirmed the failed smoke EXIT cleanup left exactly one `rav-smoke-*` test player and one match; removed only that temporary account's dependent rows and its match in a transaction. Post-cleanup database counts returned to Players=0 and Matches=0. The API/catalog tables remain.
- Fixed `smoke-test.sh` cleanup to derive the bootstrap role/database from staging configuration and to identify test match rows through the temporary player's participant rows, so cleanup does not depend on parsed match ID.
- Unity logger still reports a write failure for its relative `Logs/GameplayRuntimeDiagnostics.log` under the read-only release tree. Next route `Logs` to the existing writable staging log area and make the Unity release installer create this link for future releases.
- Fixed smoke test now passes end-to-end, including automatic cleanup: liveness/readiness HTTPS, PostgreSQL TLS, Unity UDP socket check, API server register/heartbeat/status, temporary account registration/login/JWT/starter profile, tickets and replay rejection, backend match creation, and player/match row removal. Explicit skips remain DTLS handshake and Unity client gameplay harness.
- Unity service is active after its release logs were routed through `/var/log/rav-staging/unity`; startup diagnostics now write successfully. The process is stable with UDP 7770 bound and no systemd restarts.
- Release-script validation caught a Windows CRLF line ending in the SCP copy: bash received a trailing carriage return in an executable argument and aborted before moving/switching the release. The active current symlink and running server were left untouched; normalize the shell script to LF, re-upload, and rerun the same-release deployment check.
- Release rollout with the LF-normalized script correctly staged a writable `Logs` symlink, but its health check queried UDP immediately after systemd restart and rolled back before Unity finished starting. The rollback restored the prior symlink and restarted the prior known-good release. Update `health-check-unity.sh` to wait a bounded interval for the UDP socket while failing immediately if systemd stops; then retry release validation.
- Backup/restore check passed: took a TLS-verified custom-format dump of the live staging DB, restored it with PostgreSQL 17 into a `--network none` container backed only by tmpfs, verified 13 public tables, Vehicles=21, Factions=2, Players=0, Matches=0, then allowed the disposable container to be removed.
- External UDP reachability is confirmed at the VPS NIC: a one-off uniquely marked datagram from the Windows host to 62.83.120.40:7770 was captured on eth0. This is not a DTLS handshake.
- The newly deployed Unity build emitted `gameplay_spawner_server_started` with active scene `Server`; `UnityTransport` bound 0.0.0.0:7770. After the process started, the public API status showed the real Unity server's `Unity server running` heartbeat. Map/gameplay client flow remains unverified.
- A 30-minute passive stability monitor is running; next prepare the client launcher and documentation, then collect the monitor result and final service/database state.
- Prepared a staging-only Unity Editor launcher that supplies only the verified HTTPS API URL, server certificate common name, and public CA path. Created a client guide; no Unity source/gameplay files or private credentials were changed.
- The bundle packager still writes `Runtime deployment: NOT STARTED` into generated BUNDLE-INFO. Since this staging environment is now live, make the generated note state that deployment status is tracked in the external progress document before creating the final refreshed bundle.
- Refreshed the full staging deployment archive with updated smoke/release/health-check scripts. Packager validated the 189-file Linux Unity payload, exact API source/snapshot hashes, archive manifest extraction, and SHA-256; sensitive-filename scan passed. Archive: `deploy/staging/artifacts/RAV-Staging-3B-20261125T194757Z.tar.gz` (SHA-256 recorded in Desktop docs).
- Existing API repo deployment plan/progress/checklist still end with the historical Altare-only state; append a dated Netcup live checkpoint so repository docs no longer report that no runtime is deployed.

## 2026-11-26 — Netcup staging deployment closeout — COMPLETED WITH CLIENT TESTS PENDING

- Completed isolated Netcup staging deployment: Ubuntu 24.04.5 x86-64, key-only ravadmin, firewall, Docker/systemd/logging/monitoring, HTTPS API, PostgreSQL 17, Unity Linux Dedicated Server.
- Created a fresh staging database with the current 9 EF Core migrations and system/game catalog seed only. No legacy players, profiles, progression, balances, vehicles, match history, or statistics were imported. Disposable test account and match rows were removed and zero counts verified.
- Verified PostgreSQL TLS/private network, API live/ready, server registration/heartbeat, auth/JWT/starter profile, connection ticket issue/redemption/replay rejection, backend match record, PG17 backup/restore, external UDP datagram arrival, 30-minute stability, and recovery after a controlled reboot.
- PostgreSQL/API ports are not public. No production endpoint/database or credentials were used. No paid add-on or destructive operation on an old database occurred.
- Remaining NOT VERIFIED: FishNet DTLS handshake, real Unity client ticket connection, live Map/match/gameplay (vehicles/bots/physics), and 20-slot performance. Server is left running.
- Bundle: deploy/staging/artifacts/RAV-Staging-3B-20261125T194757Z.tar.gz; SHA-256 d2b30525f7e940bf9aa887e1d3177613ffcef7e9a5e62b3fd520ce469ae595ee.
- Final operational status: deployment completed; staging runtime active; client/gameplay acceptance still requires an owner Unity Play test.

## 2026-11-26 — Commit readiness review — IN_PROGRESS
- User asks whether the current pending API and Unity changes can be committed manually.
- Scope: inspect both working trees and diff contents for intended staging changes, secrets, generated artifacts, and whitespace issues; do not commit, stage, reset, clean, or edit implementation.
- Acceptance: identify safe commit scope and any files that should be excluded or split; report tests/checks already available and remaining caveats.- Review result: branch main has no staged changes; 28 tracked paths are modified, with untracked deploy/staging/api/entrypoint.sh (required by the staging Dockerfile) and root TASK.md (local task journal). High-confidence secret-pattern scan found no matches. Production Compose uses the same secret filenames required by the shared root Dockerfile entrypoint, but that Dockerfile changes future production image builds and should be included only intentionally.
- Recommendation: commit the API staging implementation as a reviewed set, including deploy/staging/api/entrypoint.sh; omit root TASK.md unless the journal should be versioned. Build, Compose validation and staging smoke checks are recorded as passed. No commit or staging action was performed.
- Status: COMPLETED. Remaining caution: preserve review of shared Dockerfile behavior before any production image release.
## 2026-11-26 — Reinitialize local RAV database from zero — IN_PROGRESS
- User request: completely clear the local RAV development database and make it a fresh start comparable to a production first install.
- Scope: only the local PostgreSQL database used by the local RAV API. Recreate current schema through the supported EF Core migration path and retain only required catalog/system seed data; remove old local user/game records. No production, Netcup, other Docker databases/volumes, or source/config secrets may be touched.
- Acceptance: confirm exact local container/database/API target, initialize from current migrations and required system seeds, verify user/player/match tables are empty, and keep local API connected to that database. Do not launch Unity Client.
- Start: 2026-11-26 (Asia/Manila).
- Status: IN_PROGRESS. Next read database system docs, inspect local Compose/API startup and migration/seed behavior without displaying secret values, then reset only the verified local database.
- Discovery: the exact local Compose project is `war-of-machines-local`, service `postgres`, bound only to `127.0.0.1:5433`, and backed by named volume `war-of-machines-local_war_of_machines_postgres_data`. The current RAV API Compose file resolves to the same project name and volume. The old container is stopped; local API/server processes and local listeners are absent. Separate exited `rav-local-*` staging containers exist and will remain untouched.
- Seed finding: local Development currently enables migrations, catalog seeding, and the predictable demo fixture account/match. To keep a clean production-like reset after API restart, disable only `SeedDevelopmentFixtures`; migrations and required system catalog seed remain enabled.
- Configuration decision implemented: changed only the local `Database:SeedDevelopmentFixtures` default to false; preserved Development migrations and catalog seeding. Updated startup, database and configuration system docs so a restart will not recreate a demo account or match.
- Before reset: API and Dedicated Server processes were absent; local Postgres container was exited. Current RAV Compose config confirms the exact named volume identity of that local DB and loopback-only host binding. The separate `rav-local-*` staging containers/volumes are explicitly excluded.
- Next: start the verified local Compose PostgreSQL service, verify database/role identity and existing schema, then drop/recreate only `war_of_machines_local`, start the local API with current Development migrations/catalog seed, and verify zero user/game rows.
### Completion checkpoint — 2026-11-26 (Asia/Manila)
- Recreated only local `war_of_machines_local` using the existing loopback-only Compose PostgreSQL 16.15 service. The existing Docker volume was retained; no other database/container was stopped, deleted, or changed.
- API startup applied all 9 migrations present in the current `RavApi/Migrations` source and ran only the normal system/game catalog seed. Final counts: Factions=2, Maps=2, Vehicles=21, VehicleResearchRequirements=19; Players=0, Matches=0, MatchParticipants=0, UserVehicles=0, UserVehicleResearches=0, UserSessions=0, ConnectionTickets=0, DedicatedServers=0.
- Disabled `Database:SeedDevelopmentFixtures` in local Development so later API restarts do not recreate the demo user/match. Updated the API startup/database/configuration system docs; migration and catalog seeding remain enabled.
- The local API remains running from `Start-LocalPostgresApi.ps1`; `/health/live` and `/health/ready` both returned HTTPS 200 without skipping TLS validation. PostgreSQL is healthy and published only on `127.0.0.1:5433`.
- Validation: current config JSON parsed; current migration count matches 9 source migrations; final row-count queries passed; `git diff --check` passed (only existing line-ending notices). The first EF query for `__EFMigrationsHistory` logged an expected failure because the freshly created DB had no migration table yet; migrations then completed and readiness returned 200.
- Netcup/production and separate exited `rav-local-*` containers were not touched. Unity Dedicated Server and Client were not started. No tests, commit, or push were run.
- Status: COMPLETED.

## 2026-11-26 — Local API shared Netcup PostgreSQL tunnel configuration — IN_PROGRESS
- User request: keep Local API and Netcup API on the same `rav_staging` PostgreSQL database; Local API connects through a protected SSH tunnel using the existing SSH identity/secure vault, keeps PostgreSQL TLS verification enabled, and never auto-migrates or seeds development fixtures.
- Scope: only local API Development configuration and its system documentation if required by the panel integration. Never change the remote database or Netcup services; preserve all current dirty changes and secrets outside Git.
- Acceptance: local API config targets loopback end of the tunnel, migrations and fixtures disabled, API reports an incompatible schema instead of altering it, TLS verification remains on, no credentials/log secrets, and status can verify both local API and shared Netcup DB.
- Start: 2026-11-26 (Asia/Manila).
- Status: IN_PROGRESS. Next inspect existing API config/docs and panel's secure tunnel helper, then make only configuration/documentation changes needed for this behavior.
### Completion checkpoint — 2026-11-26 (Asia/Manila)
- Implemented the tunnel-only local database override with PostgreSQL `VerifyFull`, server-name validation, and disabled local startup migrations/seeds/fixtures. Added clear pending-schema failure handling and updated the API system docs.
- Validation: API build passed in an isolated output folder (0 warnings/errors); staging Compose validation passed and PostgreSQL has no published host ports.
- Local runtime is not verified: an unmanaged `RavApi.exe` owns localhost ports 7216/5220 and does not answer HTTPS readiness. It was left untouched; no local API process was replaced.
- Status: PARTIALLY COMPLETED — code/build verified; runtime requires the existing owner to release or deliberately relocate the occupied Local API ports.

## 2026-11-26 — Complete Local/Netcup control panel runtime validation — IN_PROGRESS
- User authorizes stopping PID 18232 only after confirming it is the old project `RavApi.exe` occupying localhost 7216/5220; user also authorizes project process/service start/stop and fixes. Preserve all uncommitted work; never touch PostgreSQL data, production, or Git history.
- Scope: verify/stop only that confirmed Local API process; start Local API through the panel using the verified Netcup PostgreSQL SSH tunnel; start Local Dedicated Server; verify status/heartbeat, Stop All, restart without rebuild, and Local/Netcup switching; remove only proven-dead panel code.
- Acceptance: practical component checks pass, code remains minimal/stable, existing API/Dedicated Server buttons function; no client launch, commit, or push.
- Prior checkpoint: API isolated build passed; Compose validation passed; API shared-DB tunnel/TLS and migration-disable code was prepared; current local runtime was blocked by the process on the API ports.
- Next: verify the current PID/path/ports anew; use the existing isolated build if intact; fix only concrete runtime blockers and record validation.
- Status: IN_PROGRESS.

## 2026-11-26 — Resume Local/Netcup control-panel runtime validation — IN_PROGRESS
- User confirmed the Windows Firewall prompt was dismissed with Cancel and authorized completing Stop All, restart without rebuild, and Local/Netcup switching; leave panel in Local with API and Dedicated Server running. No Client launch, commit, or push.
- Resume from the existing verified Local state. Do not repeat successful health checks/builds; use the panel, verify stopped state, restart with Build disabled, switch Local → Netcup → Local, then leave Local API and Dedicated Server Ready.
- Start: 2026-11-26 (Asia/Manila). Status: IN_PROGRESS.
### Completion checkpoint — 2026-11-26 (Asia/Manila)
- Completed panel-driven Stop All. The operation reported the Local API and Dedicated Server stop sequence completed; it also stopped only the loopback SSH tunnel helper and explicitly left Netcup PostgreSQL running. API status refreshed to Stopped; no data/service changes were made remotely.
- Re-ran Execute All in Local with Build unchecked and Start checked. Live Log explicitly said the API build was skipped and the existing build was used. The protected loopback PostgreSQL tunnel restarted, API reached Ready, and the Windows Dedicated Server reached Ready with registration/heartbeat verification. No Client launch or rebuild occurred.
- Switched Local → Netcup → Local. Netcup refresh reported PostgreSQL, API, and Dedicated Server Ready; returning to Local applied the Local API/DTLS settings and the fresh panel status showed PostgreSQL, Local API, and Local Dedicated Server Ready. Unity Editor was not restarted.
- Final state: Local selected; API Ready; Dedicated Server Ready; PostgreSQL Netcup Ready; Build checkbox off; Client not launched. No implementation code changed in this continuation. No commit or push.
- Status: COMPLETED.

## 2026-11-26 — Clearer registration validation and server errors — IN_PROGRESS
- User request: set minimum login and password lengths to 4 characters, and make server responses explain the reason for failures in the Unity client instead of showing raw/unhelpful responses.
- Scope: trace registration DTO validation and Unity registration response handling; preserve existing auth/security behavior except the requested length floor; return/display safe, actionable validation messages. Do not expose secrets or change unrelated flows.
- Acceptance: login and password each accept 4+ characters and reject shorter values with a clear reason; API returns explanatory validation errors; Unity UI displays a human-readable cause for failed registration/login responses. Run focused API/client compilation or tests available without launching a multiplayer client.
- Start: 2026-11-26 (Asia/Manila). Status: IN_PROGRESS.
- Implementation checkpoint: API `RegisterRequest` and `LoginRequest` now require Login length 4–32 and Password length 4–128, with explicit validation messages; applied to canonical AuthController and staging API snapshot while preserving existing unrelated starter-data edits in the snapshot.
- Unity `RegisterManager` now converts ASP.NET validation `errors`, ProblemDetails `detail/message/title`, plain-text auth failures, and empty/network responses into concise human-readable messages instead of forwarding raw JSON or blank text to the popup.
- Changed files: `Assets/Game/Scripts/API/Endpoints/RegisterManager.cs`; `RavApi/Controllers/AuthController.cs`; `deploy/staging/api/RavApi/Controllers/AuthController.cs`. No schema, database, service, or gameplay changes.
- Next: inspect the focused diff, compile the API into an isolated output and confirm Unity Editor recompilation, then record results and remaining deployment limitation.
### Completion checkpoint — 2026-11-26 (Asia/Manila)
- Registration and login now enforce Login 4–32 characters and Password 4–128 characters; API validation text names the field and accepted range. The canonical API controller and staging source snapshot are aligned for validation; unrelated starter balances in the staging snapshot were preserved.
- Unity auth handling now extracts ASP.NET validation messages and ProblemDetails details, keeps plain server messages readable, and supplies concise HTTP/network explanations when the response has no usable body. Raw validation JSON is no longer passed to the auth popup.
- Validation: canonical API `dotnet build --no-restore` succeeded with 0 warnings/errors; `git diff --check` passed in both repositories with only existing line-ending notices. Unity Editor compilation and live registration were not verified: the Editor log predates the client edit and the active Windows view is a Google identity check, which was left untouched. No API deployment/restart or database write was performed; the running service needs the updated API build to use the new server-side minimum.
- Status: COMPLETED (source implementation complete; Unity compile/live activation remain unverified).

### Resume checkpoint — registration still fails to connect — IN_PROGRESS
- User reports the Unity Play Mode popup now says “Could not connect to the server. Check your connection and try again.” Screenshot shows Local mode, API/Server statuses Ready, and Play Mode prevents environment/service changes.
- Scope: diagnose the active Unity API URL/TLS/network path and distinguish a stale status indicator from an API transport/endpoint error. Do not submit registration, create accounts, change credentials, or stop/restart services unless a concrete safe fix requires it.
- Start: 2026-11-26 (Asia/Manila). Next inspect the configured API address without secrets, current listeners/health endpoints, and recent Unity/API logs; then fix the smallest root cause.
- Diagnostic finding: current Unity `Editor.log` records libcurl `Curl error 28` after exactly 2000 ms. `RegisterManager` configured a 2-second local auth timeout. Local API HTTPS `/health/live` and `/health/ready` return 200 (108 ms / 330 ms); a deliberately invalid registration is rejected with HTTP 400 in 21 ms, and a nonexistent-user login returns HTTP 401 in 676 ms without creating an account. The failure therefore occurs only on slower real registration work (player plus starter-data database calls through the remote PostgreSQL tunnel), not at API reachability or TLS health.
- Decision: raise only the local authentication request timeout to 20 seconds and map UnityWebRequest's timeout/certificate errors to clear popup messages. The local API is still running an older build, so rebuild/restart only the Local API through the existing panel after exiting Play Mode; do not touch Netcup services or create a user.

### 2026-11-26 follow-up — Local API activation pending
- Registration connection error was traced to Unity's 2-second Local request timeout (Editor.log Curl error 28 at 2,000 ms); API health endpoints themselves returned HTTPS 200.
- API auth DTOs in canonical source and staging snapshot enforce login 4–32 and password 4–128 with explicit validation messages. A deliberate invalid short registration payload returns HTTP 400 quickly; no account or database write was created.
- The running Local API is an older build and must be rebuilt/restarted through the Unity Development Control Panel to activate the new validation rules. Unity is currently in Play Mode; its panel blocks service changes until Play Mode exits. CUA could not attach to the native Unity window, so no service was restarted.
- No Netcup service or database was changed. Next: after Play Mode exits, Build API and Start API in Local mode; verify health and only invalid-payload validation, never a valid registration.
- Status: IN PROGRESS; no commit/push.
### 2026-11-26 follow-up — Local API rebuilt and running
- Unity Development Control Panel Build API and Start API both completed in Local mode. Active `RavApi.dll` came from the isolated build directory `%LOCALAPPDATA%\RAV\DevelopmentControlPanel\Logs\LocalApiBuilds\api-20261126T143121313`.
- Panel reports Local API Ready; protected PostgreSQL tunnel remains loopback-only on port 15432. Startup explicitly disabled migrations and development fixtures.
- Strict-TLS checks: `/health/live` HTTP 200 (95 ms), `/health/ready` HTTP 200 (287 ms).
- A short invalid registration payload returned HTTP 400 with clear 4-character minimum messages for login and password. It did not create an account or write a valid registration.
- No Netcup services or database data were changed; no commit/push. Account creation remains untested by design.
- Status: local API validation fix active.