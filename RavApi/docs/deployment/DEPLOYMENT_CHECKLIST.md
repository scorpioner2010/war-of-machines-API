# Deployment readiness checklist

Перевірено: 2026-11-22
Current phase: Stage 3A — OVHcloud provisioning and secure access IN PROGRESS; Stage 3B not started
Rule: етапи 2–5 не запускаються автоматично

## Stage 1 — Audit and plan

- [x] Перевірити Unity і API repository paths та інструкції.
- [x] Перевірити Unity Editor/package versions і Linux build module state.
- [x] Перевірити FishNet authenticator, DTLS config, runtime port/address, server startup/shutdown.
- [x] Перевірити room registry/additive scenes, 20-slot bot-fill code, diagnostics owners.
- [x] Перевірити API JWT/session/ticket/server auth, routes, migration, host/proxy/CORS/logging config.
- [x] Перевірити production endpoint лише статично; не надсилати production request.
- [x] Запустити API compile без запуску API/DB: 0 warnings, 0 errors.
- [x] Зафіксувати current local DB limitation, historical migration evidence та відсутність production verification.
- [x] Створити план, прогрес і checklist.

## Stage 2 — Local code/build/config readiness

### Unity Linux Dedicated Server
- [x] Перевірити Linux Dedicated Server Build Support у Unity 6000.3.13f1; модуль уже встановлений у поточній editor installation.
- [x] Додати Linux Server profile/script на основі Server.unity + Map.unity і Dedicated Server subtarget.
- [x] Зберегти server Mono backend до майбутньої FishNet handshake перевірки; IL2CPP не обрано.
- [x] Виконати Linux build; Unity створив ELF output. Headless startup, scene load і shutdown — NOT TESTED — DEFERRED TO STAGE 3; див. Stage 3 list.
- UDP 7770 default і overrides через RAV_SERVER_PORT / command line — NOT TESTED — DEFERRED TO STAGE 3; див. Stage 3 list.
- Runtime-перевірка API host allowlist у dedicated server/client build — NOT TESTED — DEFERRED TO STAGE 3.

### API/DB host configuration
- [x] Перевести API target і Docker SDK/runtime images на .NET 10 LTS.
- [x] Підняти сумісні ASP.NET Core/EF Core/Npgsql dependencies; build пройшов, але повного regression/security test plan немає.
- [x] Додати liveness/readiness checks; fake-DB smoke підтвердив 200/503 з узагальненим body.
- [x] Створити production Compose для API/PostgreSQL/Caddy з pinned tags, healthchecks і restart policy; config validation passed.
- [x] PostgreSQL має private network/volume без public 5432 port та окремі runtime/migrator roles.
- [x] Додати Compose host/proxy параметри; реальний hostname/IP заповнюється під час staging provisioning.
- [x] Залишити production API migration/seed startup flags false; міграція — окрема job.
- [x] Додати secret file mounts і імена у docs без реальних значень; Data Protection PFX/password також винесені у secrets.

Historical pre-closeout diagnostic: WSL/Docker access from Codex was blocked and runtime tests were unperformed. The owner later redefined Stage 2 as local preparation; final scope and Stage 3 deferred tests are recorded at the end of this checklist.

## Stage 3 — Isolated staging proof
- [ ] Verify file-backed Compose secret source ownership and read modes against the non-root API UID/GID 1654 and PostgreSQL UID/GID 999; local Compose does not remap file-source uid/gid/mode.

- [ ] Підготувати Ubuntu 24.04 AMD64 staging host/VM без production data/secrets.
- [ ] Перевірити HTTPS certificate/renewal, HTTP redirect, HSTS/proxy headers та API auth.
- [ ] Перевірити DTLS server certificate, client CA/common name validation і certificate renewal. Відхилення TLS не допускаються.
- [ ] Перевірити register, heartbeat, discovery, connection-ticket issue/redeem/replay, match start/end authorization і logout/revocation.
- [ ] Пройти PostgreSQL backup → restore → migration → __EFMigrationsHistory verification.
- [ ] Перевірити systemd SIGTERM/restart, API/DB container restart, 15s heartbeat expiry, user result persistence і reconnect.
- [ ] Прогнати один повний 20-slot match з 19 bots 30–60 хвилин.
- [ ] Виміряти API/DB latency, Unity process RSS, host memory, CPU, disk, packet loss, server tick p95/max, GC count, bot/entity count.
- [ ] Capacity gate: peak total RAM ≤9 GB з 3 GB headroom на 12 GB host; sustained CPU <70%; tick p95 < половини 33.3 ms 30 Hz budget і max < 1 tick interval у steady state.
- [ ] Окремо прогнати два одночасні матчі; не збільшувати capacity, доки вони не пройдуть.
- [ ] Перевірити log rotation, alert thresholds, disk growth і off-host backup restore.

## Stage 4 — Production migration and cutover (потребує окремого початку етапу)

- [ ] Підтвердити власника/конфіг Render API та PostgreSQL, версію БД, data retention і старі client compatibility.
- [ ] Створити і перевірити encrypted backup/restore plan; зафіксувати point-in-time rollback target.
- [ ] Підготувати Europe VPS, DNS, firewall, domain TLS, private Docker network, systemd account, certificates, secrets і monitoring.
- [ ] Перевірити production migration SQL проти staging copy; preflight duplicate roster rows і таблиці.
- [ ] Зупинити або обмежити match creation на контрольованому вікні, зберегти final backup.
- [ ] Застосувати міграцію окремою контрольованою job. Не вмикати automatic migration/seed при API startup.
- [ ] Перевірити __EFMigrationsHistory, таблиці/session/tickets/server identity, API readiness, auth, DTLS і heartbeat.
- [ ] Переключити API domain та client rollout з планом для старих builds; перевірити через власний test account.
- [ ] Перевірити попередній release rollback; не запускати migration Down(), якщо він видаляє session/ticket/server data.

## Stage 5 — Limited public test/operations

- [ ] Увімкнути обмежену хвилю доступу та один-match concurrency cap.
- [ ] Відстежувати 5xx/login failures, API/DB latency, Unity tick p95/max, process RSS, GC, CPU, disk, heartbeat status і UDP loss.
- [ ] Підтвердити щоденні off-site PostgreSQL backups та періодичний restore drill; OVH backup лишити додатковим шаром.
- [ ] Перевірити сертифікат expiry/renewal, secret rotation runbook, log rotation, restart/crash recovery.
- [ ] Поступово збільшувати concurrency тільки після стабільних навантажувальних вікон; при перевищенні resource gates перейти на більший/розділений хост.

## Required runtime variables — names only

API:
- ASPNETCORE_ENVIRONMENT, ASPNETCORE_URLS
- ConnectionStrings__DefaultConnection або DATABASE_URL
- Jwt__Key, Jwt__Issuer, Jwt__Audience
- AllowedHosts, ForwardedHeaders__KnownProxies__0
- Cors__AllowedOrigins__0 (лише для browser clients)
- Database__ApplyMigrationsOnStartup, Database__RunSeedOnStartup, Swagger__Enabled
- DedicatedServers__Provisioned__0__ServerId, DedicatedServers__Provisioned__0__Credential
- DedicatedServers__HeartbeatTimeoutSeconds, Logging__LogLevel__Default

Unity server:
- RAV_API_BASE
- RAV_SERVER_ID, RAV_SERVER_CREDENTIAL, RAV_SERVER_REGION
- RAV_SERVER_ADDRESS, RAV_SERVER_PORT
- RAV_UTP_SERVER_CERTIFICATE, RAV_UTP_SERVER_PRIVATE_KEY
- RAV_UTP_SERVER_COMMON_NAME, RAV_UTP_CA_CERTIFICATE

Жодне поле вище не містить реальне значення secret. Production JWT key, database credential, Dedicated Server credential, DTLS private key і CA мають бути новими у своїх середовищах і передаватися поза Git.

## Stage 2B diagnostic status — 2026-11-22

- [x] WSL package version identified; `wsl --status` / `wsl --list --verbose` denial recorded without assuming WSL is broken.
- [x] Docker CLI/Compose versions and selected Linux context recorded; Docker Desktop start failed because its installation registry key is missing.
- [x] NuGet endpoints respond; vulnerability audit completed with an isolated writable cache and reported no vulnerable packages from configured sources.
- [ ] Read WSL distro registration, Windows feature state, and hypervisor state from host PowerShell outside Codex.
- [ ] Start a working Linux engine; build/run isolated Compose and verify container health, persistence, restart, secret permissions, and backup/restore.
- [ ] Launch the existing Unity Linux build; verify dependencies, headless scene startup/shutdown, UDP 7770, and API registration.
- [ ] Verify DTLS/client handshake with an authorized test client; otherwise keep it NOT VERIFIED.
- [ ] Run `bash -n`, ShellCheck if installed, executable-mode and safe script checks in Linux.

Historical pre-closeout Stage 2B note: WSL/Docker diagnostics were blocked from Codex. The owner-approved final scope below supersedes the previous Stage 2 status; no Windows features or Docker data were changed.
## Stage 2B recovery update — host Ubuntu not reachable from Codex (2026-11-22)

- [x] Record the user's host-side confirmation of Ubuntu 24.04.5 LTS, WSL 2, distro `Ubuntu-24.04`, user `toraspc`.
- [x] Attempt `wsl.exe -d Ubuntu-24.04 --exec whoami` once from Codex; it returned `Wsl/E_ACCESSDENIED`.
- [ ] Run the existing Linux Dedicated Server and verify scenes, shutdown, logs, UDP 7770, API integration and DTLS — blocked until an approved host-shell route is available.
- [ ] Test Docker Engine inside Ubuntu, or start/register Docker Desktop; the current Desktop start attempt reports its registry key missing.
- [ ] Run Compose containers, healthchecks, database persistence/restart, backup/restore, and Linux script checks — blocked by the missing accessible engine/runtime.
- [ ] Continue Stage 3 only after Stage 2 runtime acceptance is complete.

**Historical next action before the owner scope change:** obtain an approved host-shell route. The closeout below supersedes this Stage 2 blocker; runtime tests remain deferred. No Ubuntu reinstall or Docker data reset is required for Stage 2 closeout.

## Final Stage 2 status — owner-approved scope change (2026-11-22)

Stage 2 is **COMPLETED — LOCAL PREPARATION** by the owner’s explicit decision. This does not mean Linux runtime or multiplayer tests passed. Older “BLOCKED” diagnostics are historical. Stage 3 is **ISOLATED STAGING DEPLOYMENT AND RUNTIME VERIFICATION**, not started.

### Stage 3 deferred verification

Each item is **NOT TESTED — DEFERRED TO STAGE 3**:

- [ ] Linux Dedicated Server headless startup — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Server and Map scene loading — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Linux dependencies, graceful shutdown and crash-log review — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Production client/server API host allowlist runtime behavior — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] UDP 7770 listener and port/address overrides — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] DTLS handshake and certificate/CA validation — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] FishNet client authentication and connection tickets — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] API/PostgreSQL container startup — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Compose runtime healthchecks, persistence and restart — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] PostgreSQL migration tests on isolated test DB — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Test-database backup and restore — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Unity Linux deployment-script syntax, permissions and execution — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Server registration and heartbeat — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Matchmaking — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Bot spawning — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Full match with 20 machines — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] CPU/RAM/tick, API/DB latency and packet-loss measurements — NOT TESTED — DEFERRED TO STAGE 3.
- [ ] Multiple-match concurrency — NOT TESTED — DEFERRED TO STAGE 3.

### Stage 3 prerequisites

First check current OVHcloud VPS-3 price/availability and an approved SSH/network route for Codex. The proposed Germany/France, Ubuntu 24.04 AMD64, 6 vCore/12 GB/100 GB and ~$15/month are unverified planning values. Create/pay for a VPS only after the owner separately approves the exact offer. Use test-only credentials and a test DB; do not copy production data/secrets or touch Render.

**Stage 3 gate:** YES to begin staging preflight, subject to VPS access and provisioning. **Production ready: NO.**

## Stage 3A — OVHcloud VPS provisioning and secure access (IN PROGRESS)

### Preflight and order gates
- [x] Confirm Windows OpenSSH 9.5p2 and SCP are available.
- [x] Verify outbound TCP/22 from Codex with a non-authenticated probe; actual VPS login and host fingerprint remain NOT VERIFIED.
- [x] Confirm OVH account signup is complete and the account console is authenticated; no personal address values are recorded.
- [x] Inspect security settings read-only: 2FA is disabled and must be enabled by the user before ordering.
- [x] Select no commitment in the configurator; current display is $14.50/month excluding tax. The $12.32/month amount is the discounted 12-month price.
- [x] Select Europe — Poland, Warsaw, shown available now; Germany and France are unavailable for this exact VPS-3 configuration.
- [x] Select Ubuntu 24.04; AMD64 architecture is not displayed and remains NOT VERIFIED.
- [x] Keep paid add-ons off; daily backup is included.
- [ ] User enables 2FA directly; do not request or handle OTP/recovery codes.
- [ ] Verify public IPv4, exact tax-inclusive monthly total, renewal/auto-renewal terms and setup fees.
- [ ] Confirm the final recurring total is at or below $15/month; stop for user approval if higher.
- [ ] User reviews and submits any order/payment themselves; no order has been placed.

### After the owner confirms a VPS order
- [ ] Record only non-secret VPS facts: plan, selected European datacentre, Ubuntu version, IP, hostname, provisioning status, resource specs, billing period/renewal and total.
- [ ] Verify provider console recovery access before changing SSH/firewall or rebooting.
- [ ] Verify host key fingerprint out-of-band, then connect with Ed25519 SSH key and confirm Linux version, CPU, RAM, disk, IP/network and sudo.
- [ ] Harden Ubuntu with updates, non-root sudo admin, SSH key-only/root-login restrictions, minimal UFW rules for SSH/80/443 only, time sync, brute-force protection, log rotation and monitoring; maintain SSH access while changing firewall.
- [ ] Install Docker Engine/Compose and prepare staging directories/accounts only after SSH and sudo are verified; do not deploy API, PostgreSQL, or Unity in Stage 3A.
- [ ] Verify restart/recovery and document installed components/firewall/resource state.
- [ ] Stop before Stage 3B.

Current state: OVH account signup is complete, but 2FA is disabled. No VPS order, payment, provisioning, SSH connection, firewall change, hardening, or Stage 3B work has occurred. The no-commit VPS-3 offer is configured for Warsaw with Ubuntu 24.04 at $14.50/month excluding tax; AMD64, public IPv4 and the exact tax-inclusive total remain unverified.

## RAV rebranding update — 2026-11-22

- RAV API namespaces, assembly, project file, solution, Docker entrypoint, and Docker migrator project are renamed.
- Unity runtime and deployment templates use RAV names; WOM variables remain supported as compatibility aliases.
- Database schema/data, migration IDs, JWT issuer/audience, HTTPS host, Compose project/service/volume identifiers remain unchanged.
- Production deployment, production DB access, migrations, and secret rotation were not performed.
## RAV rebrand validation — 2026-11-22

- [x] Update first-party Unity namespaces/asmdefs to RAV and serialized type identifiers; preserve script GUIDs.
- [x] Update generated Unity project/solution paths and first-party assembly names to RAV.
- [x] Build `RAV.Gameplay.Shared` and `RAV.PlayMode.Tests`: both passed with zero warnings/errors.
- [x] Build API project: passed with zero errors; two NU1900 feed-access warnings.
- [x] Validate local and production Compose syntax with `docker compose config --quiet`; no containers started and no production secrets used.
- [ ] Full Unity `Assembly-CSharp` compile: blocked before compiler diagnostics by generated MSBuild project-reference target resolution.
- [ ] Fresh RAV Linux Dedicated Server build: blocked by Unity Licensing Client IPC reconnect failures and missing `com.unity.editor.headless` entitlement; no new artifact was produced.
- [ ] Unity Editor scene/prefab import and PlayMode test execution remain unverified.

The existing `WarOfMachinesServer.x86_64` artifact recorded earlier in this checklist predates the RAV build. Do not treat it as the new RAV artifact. Stage 3B remains NOT STARTED.

## Final RAV rebranding validation — authoritative update (2026-11-22)

This section supersedes the earlier checklist line saying the fresh RAV Linux build was blocked. The manually initiated Unity Editor build has now been verified from `Editor.log`.

- [x] Unity Linux Dedicated Server build succeeded; build log confirms Server and Map scenes.
- [x] Confirm the Linux ELF64 x86_64 launcher, UnityPlayer, Data folder, Mono runtime, and native plugin libraries are present.
- [x] Prepare a separate RAV-named staging copy with matching `RAVServer.x86_64` and `RAVServer_Data`; SHA-256 matched all 189 retained files, and the DoNotShip Burst debug folder was excluded.
- [x] Unity Editor build has 0 errors; 47 nonfatal third-party/editor warnings are classified in the deployment-hosting report.
- [x] API build succeeds with 0 warnings and 0 errors.
- [ ] Linux runtime startup, host shared-library resolution, UDP 7770, DTLS, FishNet registration, API/PostgreSQL integration, and deployment-script execution — **NOT TESTED**.
- [ ] Stage 3B deployment — **NOT STARTED**.

Stage 3A VPS/account preflight remains in progress; do not infer VPS provisioning or production readiness from the local build.
## Final RAV rebranding validation — authoritative update (2026-11-22)

This section supersedes the earlier checklist line saying the fresh RAV Linux build was blocked. The manually initiated Unity Editor build has now been verified from `Editor.log`.

- [x] Unity Linux Dedicated Server build succeeded; build log confirms Server and Map scenes.
- [x] Confirm the Linux ELF64 x86_64 launcher, UnityPlayer, Data folder, Mono runtime, and native plugin libraries are present.
- [x] Prepare a separate RAV-named staging copy with matching `RAVServer.x86_64` and `RAVServer_Data`; SHA-256 matched all 189 retained files, and the DoNotShip Burst debug folder was excluded.
- [x] Unity Editor build has 0 errors; 47 nonfatal third-party/editor warnings are classified in the deployment-hosting report.
- [x] API build succeeds with 0 warnings and 0 errors.
- [ ] Linux runtime startup, host shared-library resolution, UDP 7770, DTLS, FishNet registration, API/PostgreSQL integration, and deployment-script execution — **NOT TESTED**.
- [ ] Stage 3B deployment — **NOT STARTED**.

Stage 3A VPS/account preflight remains in progress; do not infer VPS provisioning or production readiness from the local build.
## Stage 3A OVHcloud live order review - authoritative update (2026-11-22)

This section supersedes earlier Stage 3A order notes. No VPS has been purchased or provisioned.

- [x] OVH console confirms 2FA and authenticator app are enabled.
- [x] Verify preferred Warsaw VPS-3 offer against the payment flow; checkout reports `out of stock in region WAW`.
- [x] Review a Europe/United Kingdom VPS-3 2027 alternative: 6 vCore, 12 GB RAM, 100 GB NVMe, Ubuntu 24.04, no commitment, unlimited 2 Gbps traffic shown in configurator.
- [x] Confirm the displayed UK order summary: $14.50/month before tax, $0.00 displayed tax, $14.50/month with displayed tax. This is under the $15 current-display budget, but the future renewal amount is not verified.
- [x] Keep optional Snapshot backup, Premium automatic backup and additional storage unselected; Local Storage is included.
- [x] Record that Automated Backup Standard appears at +$0.80 with a matching -$0.80 offer/discount; promotion duration and post-promotion price are NOT VERIFIED.
- [x] Reach Step 2 Payment for the UK alternative without accepting Terms or selecting/submitting a payment method.
- [ ] Owner confirms the UK alternative is acceptable because Warsaw is out of stock.
- [ ] Owner verifies the account's billing/tax details are accurate before accepting legal terms. The account currently displays Ukraine billing details, while the user previously said they currently live in the Philippines; exact personal values are intentionally omitted.
- [ ] Verify public IPv4 allocation, setup/initial charges, renewal cadence/amount and post-promotion pricing.
- [ ] Owner personally accepts any Terms and submits payment if they approve the exact offer. No order has been placed.
- [ ] After a successful owner purchase, verify VPS identity, resource specs, IPv4, host fingerprint and SSH login over the previously checked TCP/22 path before any hardening.
- [ ] Stage 3B deployment - NOT STARTED; no production access or changes.

**Current gate:** Owner review and payment are pending. Do not claim Stage 3A complete until a VPS is purchased and the authorized secure-access/hardening checks are verified. Keep production and database untouched.

### Owner billing confirmation - 2026-11-22

- [x] Owner confirmed the Ukrainian address already on the OVH profile is genuine and current as their registration/billing address; no profile change was made.
- [x] OVH payment page displays the owner-confirmed profile address without a validation error.
- [ ] Verify compatibility with the owner's payment provider after they choose their payment method; no method has been selected.
- [ ] Confirm public IPv4, any setup/initial charges, and the renewal amount/promotion duration; these are not shown in the current checkout summary.
- [ ] Owner personally reviews/accepts Terms and authorizes payment. No Terms or payment were submitted.

## Stage 3A — post-purchase order review (authoritative current state, 2026-11-22)

- [x] Owner completed order **#23370514**; OVH confirmation page says the order was processed. No second order was attempted.
- [x] Order tracker opened in the authenticated OVH Control Panel; current history status is **Checking your order**. OVH says additional processing is required, usually around 48 hours, and support may contact the account owner.
- [x] VPS services page checked; it currently says **You have no VPS**, so there is no active/provisioned service to administer.
- [x] Order summary checked: VPS-3 2027 monthly, Linux, 6 vCPU, 12 GB RAM, 100 GB disk, Local Storage and Automated Backup Standard.
- [ ] Payment completion / invoice — **NOT INDEPENDENTLY VERIFIED**; payment tracking and invoice list showed no result at inspection.
- [ ] Actual datacentre/region after provisioning — **NOT VERIFIED**. Pre-purchase selection was UK (configurator showed Erith); order tracker summary does not confirm the allocated site.
- [ ] Installed Ubuntu 24.04 — selected before purchase, but actual deployed image is **NOT VERIFIED** while order review is pending.
- [ ] Public IPv4, hostname, next billing date, renewal amount and post-promotion amount — **NOT AVAILABLE / NOT VERIFIED**.
- [ ] SSH host fingerprint and login — **NOT TESTED**; no active endpoint/IP exists. No SSH key was generated or installed.
- [ ] Ubuntu updates, non-root sudo user, SSH hardening, firewall, Docker Engine/Compose, systemd, directories/logging — **NOT STARTED**; do these only after provisioning and verified access.
- [ ] Stage 3B deployment — **NOT STARTED**.

**Current gate:** Wait for OVH order review/provisioning (typically around 48 hours per the tracker). If OVH requests verification, the account owner must respond. Resume Stage 3A once the VPS appears in services; do not create another order. Production and database remain untouched.

### Stage 3A — payment validation recheck (2026-11-22)

- [x] Re-opened order #23370514; expanded history shows **Your payment is being validated** at 21:53 on 2026-11-22 and latest order state **Checking your order**.
- [x] Confirmed the VPS services view still says **You have no VPS**.
- [ ] Payment accepted/cleared — **NOT CONFIRMED**; OVH’s order tracker explicitly says payment is being validated.
- [ ] Provisioning/active service, actual datacentre, installed Ubuntu 24.04, IPv4, hostname, next billing date and renewal price — **NOT AVAILABLE / NOT VERIFIED**.
- [ ] SSH, key, host fingerprint, Ubuntu hardening, firewall, Docker, systemd and logging — **BLOCKED** until a service endpoint exists.
- [ ] Stage 3B — **NOT STARTED**.

**Next action:** wait for OVH payment/order validation, usually around 48 hours per the order page. If OVH contacts the account owner, respond directly in the OVH account. Do not place another order.
## RAV Stage 3B local preparation — 2026-11-22

- [x] Build fresh Unity Linux Dedicated Server with active Linux Server profile; Editor log reports success and includes Server and Map scenes.
- [x] Verify ELF64 x86_64, UnityPlayer, matching RAVServer_Data, Mono runtime and compiled staging API-host allowlist fix.
- [x] Prepare 189-file staging payload, exclude the DoNotShip Burst debug directory, and verify source/payload SHA-256.
- [x] Build API Release with 0 warnings and 0 errors.
- [x] Validate Compose statically and assert PostgreSQL/API have no published ports, backend is internal, named volumes are staging-specific, and Caddy ports are limited to web TLS/HTTP.
- [x] Align Unity service EnvironmentFile, bootstrap output, API host, server address and UDP 7770 values.
- [x] Verify generated DB connection templates use PostgreSQL VerifyFull and a generated staging CA; keep HTTPS/DTLS verification enabled.
- [x] Prepare secret generation, Ubuntu bootstrap, SSH hardening, Docker guard, systemd, backup/restore, smoke, monitoring, deployment and rollback procedures.
- [x] Package current artifacts; validate nested Unity archive, manifest and outer SHA-256. Bundle path and hash are recorded in DEPLOYMENT_PLAN.md.
- [ ] Run the bundled bash -n syntax preflight after extracting on the verified VPS and before host changes. Local shell syntax parsing was unavailable: Git Bash returned E_ACCESSDENIED and was not retried; Docker Desktop image inventory returned permission denied.
- [ ] Stage 3B VPS deployment and runtime tests — NOT STARTED; await active VPS access and explicit owner authorization.
- [ ] DTLS client handshake, matchmaking/bot spawning and 20-machine match — harnesses are not bundled and remain unverified.
- [ ] SSH, Ubuntu bootstrap, Docker image execution, PostgreSQL TLS/backup/restore, Caddy HTTPS, API health, Unity runtime, UDP 7770, FishNet, registration/heartbeat, connection tickets/authentication, match creation and rollback — VPS-only tests deferred.

Local bundle preparation is complete; OVH provisioning remains pending per the owner, with no status recheck in this task.

## 2026-11-23 — Stage 3B preparation final verification

- **Authoritative bundle:** `D:\Programing\ServerProjects\rav-api\deploy\staging\artifacts\RAV-Staging-3B-20261122T154549Z.tar.gz`.
- **Integrity:** Outer SHA-256 is `712af20149fd1ec75484f8a7dc984b02761d998972c88c3ebf9ba76522e39c3f`; it matches the saved handoff. The archive lists 119 safe relative paths and contains Compose/Caddy, the staging environment example, API payload, the RAV Unity release archive, host bootstrap/deploy/rollback/backup/restore/smoke/performance scripts, systemd units, and first-deploy documentation. The filename scan found no `.env.local`, bare `.env`, `id_rsa`/`id_ed25519`, `.pem`, `.key`, `.p12`, or `.pfx` entries; secrets are generated by the included provisioning script.
- **Previously passed local checks:** API Release build (0 warnings, 0 errors) and Docker Compose static config validation passed during the original preparation. They were not rerun during this recovery closeout. API and Unity `git diff --check` passed for tracked changes on 2026-11-23. The three deployment documents are untracked, so a separate whitespace scan was used; it found trailing spaces in checklist lines 3-4, now removed, and the final scan passed. Existing working-tree changes remain uncommitted and preserved.
- **Local validation limit:** Bash syntax parsing was not available in this Windows session. The earlier Git Bash `E_ACCESSDENIED` was not retried. The bundle's first-deploy runbook requires `bash -n` on all bundled shell scripts after extraction and before host changes.
- **Deferred:** The last owner-provided VPS state was still OVH review pending with no active endpoint/IP or SSH access; OVH was not rechecked. Host bootstrap, Docker startup, PostgreSQL TLS/backup-restore execution, Caddy certificates, API health, Unity headless runtime, UDP 7770, FishNet, DTLS, registration/heartbeat, tickets, match/gameplay/load and deployment/rollback execution remain **NOT TESTED** until an isolated VPS is active.
- **Status:** Deployment bundle **READY**. Local static validation **PARTIAL**. **READY FOR VPS DEPLOYMENT: NO** until an active VPS endpoint/SSH path is available and the bundled `bash -n` preflight passes; this reflects the last owner-provided VPS state (OVH was not rechecked). Stage 3B runtime **NOT STARTED**. Do not mutate a host until access is verified, the preflight passes, and Stage 3B is explicitly authorized.
## 2026-11-23 — resumed local validation checkpoint

- The existing 119-entry staging bundle remains unchanged and its SHA-256 still matches the recorded handoff.
- Tracked API and Unity changes pass `git diff --check`. These three deployment Markdown files are untracked in the API repository, so they were scanned separately; the final scan found zero trailing whitespace after removing the two trailing spaces in checklist lines 3-4.
- This Windows session has no `shellcheck`, `checkbashisms`, or `sh`; the global npm packages have no Bash parser. The only `bash.exe` found is the WindowsApps alias, which was not invoked. The earlier Git Bash `E_ACCESSDENIED` was not retried, and WSL diagnostics were not repeated.
- **Remaining gate:** Run the bundle's non-mutating `bash -n` preflight on the verified Linux VPS before any host changes. No VPS, production, Docker, or database operation was performed in this resume check.
- Local staging preparation remains complete; local static validation remains **PARTIAL** until that Linux syntax preflight. VPS deployment remains **NOT STARTED**.
## 2026-11-23 — Stage 3A OVH order status check

- **Order #23370514:** One read-only check of the existing OVH order was made. The tracker step says “Your order has been processed,” while the newest order-history event is **“Checking your order”** at 2026-11-22 21:53:48 and says additional processing is required, usually around 48 hours; the previous event at 21:53:31 says **“Your payment is being validated.”** Treat the order as **PENDING**. This does not confirm payment clearance, provisioning, or an active VPS.
- **Order summary:** VPS-3 2027 monthly fees, Linux distribution, 6 vCPU, 12 GB RAM, 100 GB disk, Local Storage, and Automated Backup Standard. The active service, installed Ubuntu version, allocated region, hostname, public IPv4, invoice/payment clearance, and renewal amount were not confirmed by this order-page check.
- **SSH/host setup:** SSH is **NOT TESTED** because no active endpoint was confirmed. No SSH key/user, firewall, hardening, updates, Docker, systemd, or monitoring changes were made. No duplicate order, payment, production, or database operation occurred.
- **Existing bundle:** Rechecked without rebuilding. `RAV-Staging-3B-20261122T154549Z.tar.gz` remains 119 entries with SHA-256 `712af20149fd1ec75484f8a7dc984b02761d998972c88c3ebf9ba76522e39c3f`; required deployment files are present.
- **Current gate:** Stage 3A remains **IN PROGRESS / PENDING OVH ORDER REVIEW**; wait for an OVH status change. Do not repeat checks in a loop. Stage 3B remains **NOT STARTED** and requires separate owner authorization.
## 2026-11-23 — Stage 3A order refresh (latest owner request)

- Reloaded the existing OVH order #23370514 page once. Its progress indicator says “Your order has been processed,” but the newest order-history entry remains **“Checking your order”** at 2026-11-22 21:53:48 and says additional processing is required, usually around 48 hours. The page does not confirm cleared payment or an active/provisioned VPS; the order remains **PENDING**.
- Because the order is still pending, hostname, public IPv4, allocated region, installed Ubuntu, actual service resources, billing completion, and SSH access remain unverified. SSH was **NOT TESTED** and no host changes were made. No second order or payment action occurred.
- Rechecked the existing deployment bundle without rebuilding: 119 entries, all required staging files present, SHA-256 `712af20149fd1ec75484f8a7dc984b02761d998972c88c3ebf9ba76522e39c3f`.
- **Gate:** Stage 3A remains **IN PROGRESS / PENDING OVH REVIEW**. Wait for the existing order’s status to change; do not poll repeatedly. Stage 3B remains **NOT STARTED** and requires separate owner authorization.
## 2026-11-23 — OVH payment status follow-up

- [ ] Payment clearance for order #23370514 — **NOT CONFIRMED**. Tracker still shows “Checking your order”; prior event says “Your payment is being validated.”
- [ ] VPS provisioning/active service, hostname, IP, region, Ubuntu and SSH — **NOT CONFIRMED / NOT TESTED**.
- [ ] Stage 3A server configuration — **NOT STARTED**; wait for OVH payment review and provisioning.
- Checkout page showed an out-of-stock error for the selected UK region; no payment action was taken.
- Stage 3B remains **NOT STARTED**.