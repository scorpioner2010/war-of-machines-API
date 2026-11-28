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
## 2026-11-26 — Rewrite recent commit subjects (requested)
**Request:** Review the latest 10 commits in each of the RAV API and Unity repositories, identify what each commit changed, and replace vague subjects with clear, accurate names.
**Scope:** API repo and Unity repo; preserve all file/tree changes and commit order. Do not create new content commits. Inspect local working-tree changes before rewriting. Do not rewrite commits outside the most recent 10 per repository. Check remote divergence before deciding whether local rewording can be safely synchronized.
**Acceptance:** 20 recent commits reviewed; each gets a concise, evidence-based subject; branch trees remain unchanged; final status and any remote synchronization limitation reported. No unrelated edits.
**Status:** IN_PROGRESS.
### Commit subject review checkpoint — 2026-11-26
- Reviewed the latest 10 first-parent commits, including source/config/docs diffs and generated-only commits. All are unsigned, single-parent commits. `main` exactly matches `origin/main`; only this task's uncommitted `TASK.md` journal edit is present.
- Proposed subjects, newest to oldest:
  1. `Prepare RAV staging API startup and release tooling` — 4d506d8
  2. `Rebrand the API as RavApi and move to .NET 10` — 8fde90d
  3. `Increase Nova biped vehicle engine power` — d7ef0a1
  4. `Harden API authentication, tickets, and server credentials` — 906eab0
  5. `Add Codex local API log placeholders` — f24f966
  6. `Add vehicle engine power data and migration` — 9228d6d
  7. `Refresh checked-in API build artifacts` — eea3f49
  8. `Add player statistics and vehicle roster endpoints` — c87cc27
  9. `Persist match rewards in participant results` — ce82830
  10. `Add vehicle traverse acceleration data` — 9b0fde2
- Rewriting local subjects changes commit IDs. Because all 10 are already on `origin/main`, synchronizing them would require a force update of remote `main`; do not push without explicit approval. Next: rewrite local commit objects while preserving tree, author, and committer metadata; verify exact tree equality and keep working-tree changes.
- Status: IN_PROGRESS.
### Commit subject rewrite — completed locally — 2026-11-26
- Rewrote subjects for the latest 10 commits on local main after reviewing each commit's changes.
- Verified each rewritten commit retains the original tree, parent sequence, author/committer metadata, and commit message body; only the first subject line changed.
- Preserved existing uncommitted work. No content files were changed by the history rewrite.
- origin/main remains untouched. Local main is now 10 commits ahead and 10 behind because rewritten history has new hashes; publishing it would require a force update of origin/main.
- No push performed. Status: LOCAL REWRITE COMPLETED; REMOTE SYNCHRONIZATION NOT DONE.

## 2026-11-27 — Diagnose and repair Netcup deployment workflow — IN_PROGRESS
- **Request:** Local mode works; Netcup/server workflow fails. Reproduce and fix the failures so the server deployment workflow completes reliably, using the Development Control Panel and existing deployment tooling.
- **Scope:** Diagnose the screenshot failure at Netcup API image build, trace Execute All/API/server sequencing, correct root causes, and run focused real staging checks. Preserve user changes and do not alter the database, production, security settings, or paid resources.
- **Acceptance:** Identify exact build/deploy/start failure; fix the smallest relevant cause; verify API image build and the selected remote workflow through real panel logs/status checks; report any parts not safely or technically verifiable.
- **Start:** 2026-11-27 (Asia/Manila). **Status:** IN_PROGRESS.
- **Next:** Inspect current worktree, active panel/full logs and deployment/build configuration; reproduce only the failing Netcup step before making changes.

### Diagnostic checkpoint — 2026-11-27 (Asia/Manila)
- Screenshot/live operation log confirms Netcup API image build failed before any upload or remote change: Docker could not open the desktop-linux named pipe because it did not exist.
- Local Docker Desktop 29.8.2 was already installed but not running. Its configured context is desktop-linux. Started the existing Docker Desktop application; the Linux engine now responds to docker info.
- Inspected Docker containers after startup: all listed containers were already exited, including old local RAV services. No container was started/stopped, no volumes/data were touched, and no remote service changed.
- The panel currently has no daemon startup path before running docker build. Plan: add a bounded preflight that starts the already-installed Docker Desktop only when the Linux engine is unavailable, waits for docker info to report linux, and returns a specific error if the engine cannot become ready. Then build the image via the panel and continue the selected staging workflow.
- Status: IN_PROGRESS; API image build and remote workflow have not yet been retried.
### Implementation checkpoint — 2026-11-27 (Asia/Manila)
- Changed Unity Editor panel: before Netcup API image build, it now probes Docker for an active Linux engine, starts the existing Docker Desktop only if needed, and polls readiness with a 120-second bound. Failure messages now identify missing/unready Docker Desktop rather than silently treating every engine absence as an API build error.
- Source diff is limited to Assets/Editor/RAVDevelopmentControlPanel.cs; focused git diff --check passed. Unity Editor is open and not in Play Mode; Editor.log has no C# compilation errors after the reload.
- The Linux Docker engine is currently running, so the next non-destructive check is to build and inspect the staging API image. No deployment or remote service change will occur during that check.
- Status: IN_PROGRESS.
### Validation checkpoint — 2026-11-27 (Asia/Manila)
- Docker Linux engine 29.8.2 became ready on desktop-linux. The staging API image built successfully from the API repository using the same Dockerfile/build context as the panel: rav-api:staging-20261126172515, linux/amd64, manifest digest sha256:8b8c0fae3744c726144842489e09ef0137b044e4d003ec29e0b758b29d586ab7. The Dockerfile is multi-stage; runtime only copies published output, health probe, and entrypoint. .dockerignore excludes env files, private keys, certificates, and production secrets.
- Panel's 01:23:28 Netcup refresh passed HTTPS API health, PostgreSQL SSH health, and fresh Dedicated Server heartbeat.
- Strict-host-checked SSH read-only check returned the Dedicated Server systemd service active; remote ss showed RAVServer.x86_64 bound to UDP 7770 on 0.0.0.0.
- No staging API/server service was restarted or deployed. No database writes, container starts, or volume changes occurred; existing containers remain exited.
- The Unity panel source change compiled/reloaded without reported C# errors and focused git diff --check passed. The full Execute All button was not safely retried: Computer Use detected simultaneous user input and an unrelated browser sign-in surface became foreground, so UI input was stopped to avoid touching that session.
- Remaining: actual post-fix panel Build/API + Deploy/Start orchestration has not been observed end-to-end. The underlying Docker build and current remote readiness are verified; keep task IN_PROGRESS until the panel operation is safely rerun.
- Additional image configuration check: the API appsettings connection string is empty; runtime image does not inherit production credentials from appsettings. No secret values were printed or uploaded.
- Current result: the original failure cause is handled in source, a real Linux/amd64 API image build passed, and the live staging server/API/database are healthy. Remaining acceptance item is an actual post-fix Execute All panel run; no active deployment was needed to resolve the local Docker-engine prerequisite.

### Completion checkpoint — 2026-11-27 (Asia/Manila)
- Fixed the staging API activation guard in deploy/staging/scripts/activate-api-image.sh: the helper is invoked from the restricted incoming directory, so it now explicitly establishes /opt/rav-staging before loading lib.sh. The explicit rav_staging database guard remains active.
- Bash syntax validation and focused diff checks passed.
- Verified through the Unity panel workflow: fresh API image built, staged, and activated with rollback protection; HTTPS health/readiness passed. PostgreSQL and Caddy were not restarted and the database schema/data were not changed.
- Result: the Netcup API activation step and complete staging workflow passed. No commit or push.

## 2026-11-27 — Add correlated diagnostics for post-battle API failures — IN_PROGRESS
- **User request:** Instrument the backend API to help diagnose a Unity `Server API is not available!` popup after ~10-minute battles, in coordination with Unity client and Dedicated Server logs.
- **Scope:** Trace existing ASP.NET request logging, exception handling, authentication/session expiry, and match result/profile endpoints. Add safe structured diagnostics correlated by request trace and match/session identifiers where available. Never log authorization headers, tokens, passwords, connection strings, or request bodies. Do not change API behavior, schema, production, or Netcup services.
- **Acceptance:** API logs record method/route, status, duration, trace ID, authenticated user identifier where safe, and sanitized exception category; endpoint/service logs identify match/result completion failures. Keep expected client validation/auth responses at appropriate levels and avoid high-volume body logging. Build passes; no commit/push.
- **Start:** 2026-11-27 (Asia/Manila). **Status:** IN_PROGRESS.
- **Next:** Inspect API logging setup and the match completion/result/profile endpoints after reading this journal.

- 2026-11-27 CHECKPOINT: Added focused API request and auth failure diagnostics with safe request/flow IDs, and match-finalization outcome logs. Compile and diff validation remain.

- 2026-11-27 COMPLETE: API and Unity assemblies compile successfully; git diff --check passes in both repositories. No live API/server, database, Netcup service, or Git history was changed. Match-end diagnostic code is ready for the next rebuilt/restarted run.

## 2026-11-27 — Investigate API failure on post-match disconnect — COMPLETE
- **Request:** Correlate the newly instrumented API logs with the user's latest Disconnect-after-battle failure. Inspect API-side recent logs and any confirmed endpoint/auth/DB failure before proposing a fix.
- **Scope:** Read-only diagnostics first; change API source only if evidence establishes API ownership. No data/schema, production, or Netcup service changes; preserve existing dirty work; no commit/push.
- **Acceptance:** Root cause supported by current logs; if API bug exists, fix and build; otherwise identify client/network lifecycle owner and report exact next action.
- **Start:** 2026-11-27 (Asia/Manila). **Status:** COMPLETE.
- **Result:** Correlated requests show `/players/me` returned HTTP 401 shortly after match completion while the match-end endpoint returned HTTP 200; API health stayed up. The configured delegated access-token lifetime was five minutes, shorter than a normal match plus return-to-menu/profile refresh. Raised the default/configured lifetime to 30 minutes (clamped to 5–60); active player-session checks remain on every request, preserving immediate logout revocation.
- **Validation:** API build passed with 0 warnings and 0 errors. Local API was rebuilt through the existing panel workflow, restarted, and both HTTPS health endpoints returned 200. The panel confirmed fresh Dedicated Server registration/heartbeat. The staging deployment copy was updated in source but Netcup was not redeployed or restarted; no database/schema changes or commit/push.

## 2026-11-27 — Set match delegated token to 20 minutes — IN PROGRESS (local gameplay validation pending)
- **User request:** Reduce the Dedicated Server delegated access-token lifetime used for battles to 20 minutes while Unity automatically refreshes the player's separate main-menu token.
- **Scope:** Keep the existing ticket/session validation and rotating refresh-token behavior. Update the API default, local/staging configuration, and deployment docs. Do not touch live Netcup services or database data.
- **Acceptance:** New delegated tokens use 20 minutes with safe bounds, API builds cleanly, client refresh contract remains supported; no commit/push.
- **Start:** 2026-11-27 (Asia/Manila). **Status:** IN_PROGRESS.
- **Validation checkpoint:** API build passed (0 warnings/errors); the updated Local API is running and strict-TLS live/ready health checks return 200. The renewal route is registered and denies unauthenticated requests with 401. No environment override for the delegated token lifetime is set; source and appsettings default/clamp it to 20 minutes. The Unity server build has the renewal client, and local Dedicated Server heartbeat is fresh. A successful authenticated renewal and 20-minute elapsed-time test remain unverified without an active player session. The staging snapshot build was skipped after `--no-restore` found no assets file; no deployment or live Netcup service change.
- **Checkpoint:** Confirmed the player access token and server delegated token are separate. Client receives a rotating player refresh token but never calls `/auth/refresh`; server connection persists through ReturnToMainMenu and uses the ticket delegated token for menu profile/vehicle operations. A fixed 20-minute delegated TTL without server renewal would break menu operations after 20 minutes. Add a dedicated-server-authenticated renewal endpoint bound to an active user session with a redeemed ticket for that server; update Unity server session renewal scheduling.
- **Implementation checkpoint:** Added `POST /connection-tickets/renew`, requiring dedicated-server auth, an active UserSession, and a redeemed ticket bound to that server. Redemption now returns token expiry; delegated token default and maximum are 20 minutes. Mirrored API source and staging deployment snapshot; updated auth/config docs. No migration or live deploy.

## 2026-11-27 — Commit and publish API/Unity work — COMPLETED
- **User request:** Commit the API and Unity work with clear commit subjects, then push both repositories.
- **Scope:** Inspect each repository's branch/upstream, staged and unstaged changes, and recent history. Commit only coherent requested project work with separate descriptive subjects. Never discard changes; do not force-push or overwrite remote history without explicit approval.
- **Acceptance:** API and Unity changes are committed with clear names and pushed to their configured remotes if normal fast-forward publication is safe. Report commit IDs and any remaining unpublished work.
- **Start:** 2026-11-27 (Asia/Manila). **Status:** IN_PROGRESS.
- **Result:** API commit `604ef33 Add match diagnostics and 20-minute token renewal` and Unity commit `4a9445db Stabilize RAV multiplayer and local workflows` were created and pushed to their GitHub `main` branches using normal fast-forward pushes.
- **Validation:** In each repository, local `HEAD` matches `origin/main`; staged diff checks passed before commit. No force push was used.
- **Excluded local files:** This task journal remains uncommitted. The API repository also has generated `deploy/staging/api/RavApi/obj/` output untracked and excluded from the commit.

## 2026-11-28 — Visualize the current robot progression tree — COMPLETED
- **User request:** Analyze the current robot research tree and produce a visual representation showing which robot unlocks which, including concept/ghost robots, so the user can design a revised progression later.
- **Scope:** Inspect API catalog/graph and any progression seed data as needed to corroborate the client view; create a current-state diagram only. Do not alter migrations, schema, catalog data, player data, or API behavior.
- **Acceptance:** The artifact lists progression nodes with their current tier/status and depicts every prerequisite edge, distinguishing playable robots from concept entries. Verify node and edge counts against authoritative source data.
- **Start:** 2026-11-28 (Asia/Manila). **Status:** IN_PROGRESS.
- **Result:** Captured the current graph from the read-only staging API endpoint `GET /vehicles/graph` and cross-checked it against Unity's graph presentation and API seed data. The inline diagram in the task response shows all current unlock paths, costs, tiers, and playable/concept status. No database, catalog, migration, or runtime files were changed.
- **Validation:** 21 nodes and 19 unique edges; 8 playable (`isVisible=true`) and 13 concept/unavailable (`isVisible=false`). Iron Alliance: 10 nodes (4 playable, 6 concepts); Nova Syndicate: 11 (4 playable, 7 concepts). There are two level-1 roots, one per faction; every node is reachable and all edge endpoints resolve.
- **Limitation:** This records the currently served staging progression. The requested redesign and database migration have not started.
- **Completed:** 2026-11-28 (Asia/Manila).
## 2026-11-28 — Rename robot catalog entries in API and staging database — IN_PROGRESS
- **User request:** Apply the user's 22 selected technical designations to existing robots in the RAV API/catalog and database, preserving the agreed research-tree placement; report when complete.
- **Scope:** Inspect current API vehicle catalog, progression records, migrations, and database target; rename catalog records in source and staging data only. Preserve vehicle IDs, stats, faction/tier/research relationships unless a name-specific reference must be updated. Never modify production. Do not commit/push.
- **Acceptance:** Establish the exact existing record count and mapping, update API source and staging database consistently, use a reversible/safe migration or data update as appropriate, verify every name and unchanged relationships, build/validate API and run `git diff --check`.
- **Start:** 2026-11-28 15:31 Asia/Manila. **Status:** IN_PROGRESS.
- **Initial constraint:** The prior verified staging graph journal records 21 robot nodes (Iron Alliance 10, Nova Syndicate 11), while the user's selected list has 22 names. Inspect the current catalog before applying names; do not silently create/delete a robot or discard a selected name.
- **Read-only validation:** API seed source and live staging `GET /vehicles/graph` both contain 21 vehicle records (10 Iron Alliance, 11 Nova Syndicate). The user's selected list contains 22 names. The extra `N-19` name was used for a T3 concept placeholder in the prior diagram, but no such vehicle currently exists in source or staging. The current database also still has the original 19 progression links; the new diagram's topology has not been applied.
- **Working tree before implementation:** pre-existing `deploy/staging/api/RavApi/obj/` untracked; this task added only its required journal entry so far. No source/catalog/migration/database changes made.
- **Status:** BLOCKED — need the owner to choose whether to leave `N-19` unused or create a new T3 concept (which requires a source for its stats), and whether this pass should also replace current progression links with the approved diagram. Next: wait for that clarification, then update the same task entry and continue without repeating completed read-only checks.

### Resume checkpoint — 2026-11-28 — owner clarified mapping scope
- Owner authorized selecting one name to omit and expanded scope to Unity resource/file names, with no broken references and generic/data-driven vehicle metadata.
- Decision: omit `N-19 Brimstone`; it was only the speculative extra concept node and is not an existing API record. Use the remaining 21 names for the 21 existing robot IDs. Apply the previously approved research-tree arrangement with the speculative node omitted (21 vehicles remain: 10 IA / 11 Nova). Preserve every vehicle `Code`/ID, stat, ownership/progression FK, and tier unless the approved arrangement explicitly changes edge rows; leave purchases/user history untouched.
- Resume status: IN_PROGRESS. Next inspect Unity system docs and code/data references, then implement source + reviewed reversible data migration and safe Unity asset rename strategy.
- **Owner clarification received:** remove `N-19 Brimstone` rather than create a 22nd concept robot. Resume with 21 designations for 21 current records. The new research layout is approved; omit the speculative extra concept and preserve the existing 21 vehicle identities and all stats.
- **Status:** IN_PROGRESS. No source, Unity asset, or database writes have been made yet.

## 2026-11-28 — Vehicle catalog rename continuation — COMPLETED
- Correction to the earlier task entry: the approved research tree has 19 edges (9 Iron Alliance and 10 Nova Syndicate), not 17.
- Updated all 21 API seed names and made explicit seed reconciliation match the approved 19-edge tree. Stable vehicle codes/IDs and all stats/prices/classes remain unchanged.
- Added reversible guarded migration `20261128120000_RenameVehicleCatalogAndResearchTree`; it changes only catalog display names and research links. User ownership, player research state, and other records remain untouched.
- Applied only this migration to the verified `rav_staging` database after checking all previous migrations were current. Saved and validated backup at `/var/backups/rav-staging/postgres/rav-staging-20261128T075320Z.dump`.
- Live staging verification: 21/21 names and IDs match, all 19 target links and XP costs match, HTTPS `/health/live` and `/health/ready` both return 200; API, Caddy, and PostgreSQL are healthy. No API/PostgreSQL service restart or production change.
- API `dotnet build --no-restore`: passed with 0 warnings/errors. Mirrored the updated seed and migration into the staging build context. Existing untracked `deploy/staging/api/RavApi/obj/` remains untouched.
## 2026-11-28 — Balance hidden T3/T4 robot characteristics — IN_PROGRESS
- **User request:** Balance the hidden robots so their stats fit their levels.
- **Scope:** Inspect the 13 hidden catalog robots, current stat curve/roles, and prior balancing decisions; preserve names, IDs/codes, levels, visibility, prices, ownership/research data, and the approved progression graph. Change only combat characteristics that need level-appropriate tuning. Do not touch production.
- **Acceptance:** A defensible, consistent T3/T4 progression curve; matching API seed and staging `rav_staging` data; no unrelated data changes; relevant build/validation and diff checks.
- **Start:** 2026-11-28 (Asia/Manila). **Status:** IN_PROGRESS.
- **Next:** Read API system documentation and current task history; compare hidden robots against visible tier references and existing balance rules before deciding concrete stat changes.
### Balance review — 2026-11-28
- Compared every hidden T3/T4 robot against its same-class predecessor and the documented visible T1/T2 baselines.
- The current catalog is monotonic for core progression: hidden units generally gain HP, armor, DPS, penetration, and/or role utility. Colossus T4s trade a smaller DPS increase for substantial survivability; scouts retain lighter HP/armor for view and mobility.
- No numerical balance target or tier-power formula exists in the project docs. No seed, migration, database, or Unity values changed pending confirmation of the desired power curve.
- Proposed default: preserve class/role identity and target roughly +20–30% combined combat effectiveness from T2 to T3 and +15–25% from T3 to T4, allowing a deliberate scout/guardian/colossus tradeoff. Rebalance only outliers against that curve.
- Status: AWAITING BALANCE CURVE CONFIRMATION.

## 2026-11-28 — Full hidden T3/T4 robot rebalance — IN_PROGRESS
- User clarified that all 13 hidden robots need a complete re-balance from scratch.
- Established role-aware DPS targets by tier; revised hidden HP, armor, projectile damage/reload, penetration, view, aim/accuracy, ammunition, and selected movement values. Kept IDs, names, levels, classes, prices, visibility, research graph/XP, and player progress unchanged.
- Updated `RavApi/Data/SeedData.cs`, both balance references, and added guarded reversible `20261128150000_RebalanceHiddenRobotStats` migration. Mirrored seed/migration into the staging Docker build context.
- Local .NET build succeeded with 0 warnings/errors. Staging DB identity and old migration state were confirmed; pre-change DB backup and guarded staging migration remain pending.
- Completion: applied the guarded data migration to `rav_staging` after creating and validating `/var/backups/rav-staging/postgres/rav-staging-20261128T111710Z.dump`.
- Verified the public staging catalog returns all 13 hidden robots with the new HP/DPS/penetration/view values; `health/live` and `health/ready` both returned healthy. Catalog remains 13 hidden + 8 visible, with 19 research links.
- API, Caddy, and PostgreSQL container IDs stayed unchanged; no service restart, production change, Unity edit, or user progression change was performed. Local API build and `git diff --check` passed. Gameplay feel remains for the owner's Unity playtest.
- Status: COMPLETE.
