# Task Journal

## 2026-11-03 — Додати потужність двигуна роботам

- **Запит:** додати новий параметр потужності двигуна для роботів і збалансувати його для всіх роботів.
- **Обсяг:** EF Core модель/схема, seed-каталог, API DTO/видача та документація характеристик і систем.
- **Критерії приймання:** усі 21 роботи мають збережене позитивне значення потужності двигуна; значення повертається каталогом та пов'язаними профільними endpoint'ами; seed оновлює існуючі записи і є ідемпотентним; балансування та зміни задокументовані; доступні перевірки проходять.
- **Статус:** `IN_PROGRESS`
- **Початок:** 2026-11-03 (Asia/Manila)
- **Знахідки:** джерелом числових характеристик є `Data/SeedData.cs`; каталог містить 21 робота, а `docs/codex-app/robot-characteristics.md` і `vehicles-and-research.md` мають бути синхронізовані.
- **Знахідки:** `Vehicle` є єдиною EF Core сутністю каталогу; seed ідемпотентно оновлює записи за `Code`. Параметри робота повертаються через `VehiclesController`, а вибрані/придбані роботи — також через `PlayersController` і `UserVehiclesController`.
- **Рішення:** додати `EnginePower` як позитивний `float` в умовних одиницях. Для tracked-гілки використати діапазон `700–1320`, для biped — `360–700`; у кожній лінії дослідження потужність не зменшується.
- **Зміни:** додано `Vehicle.EnginePower`, позитивний EF Core check constraint і міграцію `20261103120000_AddVehicleEnginePower`; `SeedData` оновлює поле і для нових, і для існуючих записів за `Code`.
- **Зміни:** `enginePower` додано до каталогу/деталей/графа роботів, профілю гравця та endpoint'ів `/user-vehicles`; довідник роботів і системні docs синхронізовано.
- **Перевірки:** `git diff --check` — успішно; `dotnet build WarOfMachinesAPI.csproj --no-restore` — успішно, 0 помилок і 7 наявних попереджень; `dotnet test WarOfMachinesAPI.csproj --no-restore --no-build` — успішно, тестових проєктів немає.
- **Обмеження:** `EnginePower` зберігається й повертається API як балансний параметр; окрему формулу фізики руху в API не додано, бо її власником є клієнт.
- **Статус:** `COMPLETED`
- **Завершення:** 2026-11-03 (Asia/Manila)

## 2026-11-18 — Реалізація MVP взводу на двох гравців

- **Статус:** `IN_PROGRESS`
- **Запит:** додати friend requests, прямі повідомлення, запрошення у взвод і party matchmaking для максимум двох гравців.
- **Обсяг:** EF Core сутності/міграція та авторизовані social endpoint'и; без voice chat, push-повідомлень і multi-server party queue.
- **Критерії:** користувачі можуть знайти один одного, прийняти дружбу, обмінятися повідомленнями та безпечно запросити друга у party.
- **Прогрес:** реалізацію ще не розпочато; API architecture і поточні match contracts перевірені.
- **Рішення щодо власників:** friendship/message persistence належать `SocialController` та EF Core models; realtime party state не зберігається в API для цього MVP і належить Unity/FishNet `PartyManager`. API надає лише authenticated social contracts і не приймає клієнтський party state як authoritative matchmaking input.
- **Зміни:** додано `Models/Friendship.cs`, `Models/DirectMessage.cs`, `Controllers/SocialController.cs`, DbSet/relationship/index configuration і міграцію `20261118145753_AddSocialAndDirectMessaging`.
- **Перевірка:** `dotnet build WarOfMachinesAPI.csproj --no-restore` успішний, 0 помилок; залишилися сім наявних warnings у старих файлах/Program.
- **Checkpoint:** Unity social proxy і server-authoritative two-player party flow підключені до backend contracts. Backend source/migration і Unity compilation валідовані; залишилися документація, фінальний static review та runtime two-client verification.
- **Результат:** Backend social contracts, EF Core migration, and API documentation are complete for the two-player platoon MVP.
- **Фінальна перевірка:** `dotnet build WarOfMachinesAPI.csproj --no-restore` — 0 warnings, 0 errors; `dotnet test WarOfMachinesAPI.csproj --no-restore --no-build` завершено. Автоматизованих test projects у репозиторії немає.
- **Обмеження:** Live two-user API smoke test і перевірка production database migration не запускалися; Unity server handles the in-memory party state for this MVP.
- **Статус:** COMPLETED

## 2026-11-19 — Відкат останніх змін API та Unity

- **Статус:** `IN_PROGRESS`
- **Запит:** відкинути останні напрацювання, реалізовані в API та Unity, і коротко описати, що було зроблено.
- **Обсяг:** зафіксувати поточний uncommitted стан API/Unity, відкинути лише поточні зміни відносно HEAD, зберегти журнали задач і committed history.
- **Критерії:** social/platoon implementation, пов’язані docs/tests/build artifacts і Unity wiring видалені з робочого дерева; журнали та історія Git збережені; короткий підсумок підготовлено.
- **Результат:** усі перелічені uncommitted зміни API та Unity відкинуті; `TASK.md` і committed history збережені.
- **Перевірка:** `git status --short` та `git ls-files --others --exclude-standard` показують лише цей `TASK.md`; untracked API implementation/log files відсутні.
- **Обмеження:** окремий desktop launcher і `D:\Programing\war-of-machines-local-builds` не змінювалися.
- **Статус:** `COMPLETED`
- **Завершення:** 2026-11-19 (Asia/Manila)

## 2026-11-19 — Phase 1 security audit and hardening plan

- **Status:** IN_PROGRESS
- **Запит:** Провести read-only аудит безпеки ASP.NET Core Backend API у складі спільного War of Machines security hardening; перевірити TLS/API authorization, server-only heartbeat та match endpoints, JWT/secrets, validation/rate limiting/CORS/forwarded headers, залежності й database contract; не редагувати код до підтвердження Phase 1.
- **Обсяг:** `WarOfMachinesAPI` source, `docs/codex-app`, configuration, migrations, deployment files, Git state; без production deployment, secret rotation, database writes або code changes.
- **Приймання:** Підтверджені факти з точними файлами/методами, API contract і migration impact, compatibility risks, тестовий план та порядок реалізації українською.
- **Початковий стан:** Root/nested `AGENTS.md`, API docs index і релевантні system docs прочитані. Робоче дерево містить попередню зміну `WarOfMachinesAPI/TASK.md`; implementation files не змінюються.
- **Перший крок:** Перевірити фактичні `Program.cs`, контролери, моделі, EF context/migrations, конфігурацію та server/client contracts.
- Оновлення аудиту: підтверджено публічний `/unity-server/status` з in-memory last-writer-wins registry, довірою до forwarded headers, відсутністю server credentials; `/matches/start` і `/matches/end` мають лише user `[Authorize]`, без server ownership/idempotency та з довірою до participant payload; admin/log routes і vehicle link mutations публічні.
- Підтверджено JWT/config проблеми: issuer/audience не валідуюються, `RequireHttpsMetadata=false`, 7-денний token, немає logout/revocation/rate limiting/DTO bounds/global error/forwarded-header configuration; `AuthController` має hardcoded fallback key і логування register request із password; signing key присутній у tracked config та Git history.
- Заплановано additive PostgreSQL model для dedicated-server identity/credentials, registry/heartbeat, connection tickets/session lifecycle і match ownership/idempotency. Міграції не створювалися та не застосовувалися. Server-only auth буде окремою policy/credential scheme, не player JWT.
- Поточна перевірка є статичною: runtime, build, database та integration/security tests не запускалися. Наступний крок після підтвердження — Phase 2 без production deployment або ротації секретів.

## 2026-11-19 — Implement security hardening Phases 2-5

- Status: IN_PROGRESS
- Запит: Реалізувати затверджений Phase 1 план для TLS/API, Dedicated Server identity/registry, match security, session/ticket authentication, FishNet admission, RPC hardening, tests і фінального security report.
- Обсяг: локальні зміни API та Unity Client/Dedicated Server; без production deployment, ротації реальних secrets, production DB migration або соціальних/Steam змін.
- Початковий стан: до implementation змінені лише task journals; API implementation files чисті від цього завдання; Phase 1 findings зафіксовані вище.
- Приймання: інкрементальна реалізація з build/test після фаз, мінімальний вплив на gameplay/network sync, documented manual configuration і чіткі runtime limitations.
- Перший крок: targeted inspection API contracts і конфігурації для Phase 2.

### Phase 2-5 implementation checkpoint
- **Phase 2:** JWT issuer/audience/signature/lifetime validation, external key configuration, auth rate limiting, DTO/body limits, ProblemDetails, restrictive CORS, trusted proxy handling and server-only policies implemented.
- **Phase 3:** PostgreSQL-backed Dedicated Server identity/heartbeat registry, hashed server credentials, ownership checks, heartbeat expiration, match server binding, roster validation and idempotent transactional match finalization implemented.
- **Phase 4:** User sessions, refresh rotation/revocation, hashed one-time connection tickets, dedicated-server authentication scheme and FishNet admission contracts implemented across API and Unity.
- **Phase 5 validation:** API build 0 warnings/0 errors; additive migration applied only to isolated local PostgreSQL; API smoke covered ticket replay/concurrency, spoofed/foreign heartbeat, player-JWT denial, match ownership/idempotency, refresh rotation/logout; rate-limit smoke passed.
- **Changed files:** API controllers/models/infrastructure/EF migration/config/docs plus coordinated Unity client/Dedicated Server contracts.
- **Limitations:** production deployment, real secret rotation, production DB migration and full Unity runtime/two-client multiplayer regression were not performed. API has no pre-existing checked-in automated test project; local smoke was used.
- **Result:** local API security hardening and cross-component contracts complete; staging/runtime acceptance and deployment checklist remain before public testing.
- **Status:** COMPLETED
- **Completed:** 2026-11-19.

## 2026-11-19 — Локальний launcher після security hardening

- **Статус:** `COMPLETED`
- **Запит:** зробити локальне тестування API/Dedicated Server сумісним із HTTPS, server credentials та DTLS без HTTP fallback.
- **Зміни:** додано `tools/Initialize-LocalSecurityEnvironment.ps1`, який генерує process-scoped server credential і PEM certificate/key поза репозиторієм; оновлено `.env.local.example` та configuration docs; launcher отримує `DedicatedServers__Provisioned` і transport variables через успадковане середовище.
- **Перевірка:** повний `C:\Users\TorasPC\Desktop\Start War of Machines.bat` завершився з exit code 0; API HTTPS `7216`, server UDP `7770`, local registry online; register/heartbeat повернули 200/200.
- **Обмеження:** production deployment/secrets/database не змінювалися; двоклієнтний gameplay runtime test не виконувався.
- **Завершено:** 2026-11-19 (Asia/Manila)

## 2026-11-22 — Deployment readiness audit, stage 1

- **Status:** IN_PROGRESS
- **Started:** 2026-11-22 02:21 Asia/Manila
- **User request:** Виконати повний технічний аудит і підготувати перевірений deployment plan для War of Machines на OVHcloud VPS-3; не купувати сервер, не підключатися до production, не виконувати deployment або production migrations, не змінювати gameplay.
- **Scope:** API/.NET/PostgreSQL security, authentication, migrations, configuration, health/HTTPS/logging and production readiness; coordinated Unity dedicated-server and deployment plan. Expected documentation deliverables are under the Unity project `docs/deployment/` unless an existing shared location is discovered.
- **Acceptance criteria:** Validate against current API code/configuration and both repositories' working trees; distinguish facts, assumptions, and unperformed tests; document blockers and safe stages 2–5; do not expose secrets or contact production.
- **Initial state:** API repo path exists and has project instructions and task journal. Its existing journal records 2026-11-19 local security hardening and confirms production migration/secrets were not touched then. Current working-tree state has not yet been inspected.
- **Next step:** Inspect Git status, API documentation map, .NET/EF configuration, auth/health/migration owners and local migration state without connecting to production.

### Audit checkpoint — current API and database readiness
- **Git state:** `main...origin/main`; only `WarOfMachinesAPI/TASK.md` is modified, from this audit journal entry. No implementation/configuration changes are present in the current diff.
- **Runtime:** API targets `net8.0`, and Dockerfile uses .NET 8 SDK/ASP.NET runtime images. Local SDK 8.0.411 and runtime 8.0.17 are installed; .NET 10 is also installed. Microsoft lifecycle confirms .NET 8 support ended 2026-11-10, so a public deployment on net8 is blocked pending a supported target/runtime.
- **Security:** Current code requires an external JWT key (minimum 32 characters) plus issuer/audience; validates signature, issuer, audience and lifetime; verifies active sessions; rotates/revokes refresh sessions; uses a distinct Dedicated Server credential scheme and hashed credentials; issues hashed single-use server-bound tickets; and protects registration/heartbeat/match endpoints by server identity. No hardcoded signing fallback appears in current source. Production config keeps automatic migration/seed and Swagger disabled.
- **Hosting gaps:** Production `AllowedHosts` and Unity's production API host remain Render-specific. API has no dedicated liveness/readiness endpoint. Production `ForwardedHeaders:KnownProxies` is empty; the reverse proxy's stable private address must be configured. Existing `Dockerfile` is Render/`PORT`-oriented; local Compose is PostgreSQL-only and loopback-bound.
- **Migration status:** `20261119085129_SecurityHardeningFoundation` exists. The historical API journal records it applied to isolated local PostgreSQL on 2026-11-19; the later local migration/startup record corroborates that local DB progressed afterward. Current state cannot be re-queried: Docker Desktop is not running and TCP `127.0.0.1:5433` is closed. No production connection or migration was attempted.
- **Next step:** Complete safe configuration/health/logging/route review, determine final migration caveats and stage order, then write deployment documents and close both journals after validation.
- **Documentation location decision:** The requested `docs/deployment/` files will live beside the existing API deployment guide in this project. That guide will receive a short append-only audit update so its previous instructions remain intact.

### Підсумок Stage 1 deployment audit — 2026-11-22
- **Результат:** створено `docs/deployment/DEPLOYMENT_PLAN.md`, `DEPLOYMENT_PROGRESS.md` і `DEPLOYMENT_CHECKLIST.md`; `docs/codex-app/deployment-hosting.md` доповнено актуальними blockers і посиланням на план. Документація містить підтверджені факти, невідомі/неперевірені умови, Ubuntu/VPS рекомендацію, resource gates, backup/rollback і контрольовані Stage 2–5.
- **Валідація:** `dotnet build WarOfMachinesAPI.csproj --no-restore --nologo -v:minimal` завершився успішно: 0 warnings, 0 errors. Це compile check, API tests не запускалися. Unity Linux build, runtime, навантаження та DB state не перевірялися.
- **User clarification:** тестова DB була тимчасово локальною; користувач дозволив майбутні зміни для online setup за потреби. Production DB switch/connect не виконувався у межах Stage 1.
- **Git state:** до build змінені лише журнали задач; build додатково змінив tracked `bin/Debug/net8.0` і `obj/Debug/net8.0` generated outputs. Їх залишено як є згідно з правилом не відкидати зміни; точний список доступний у `git status --short`. Source/config code не змінювалися.
- **Наступний крок:** Stage 2 може початися за окремою командою, з Linux build support/profile та локальної .NET 10/hosting готовності; Stage 4 production work не розпочинати без окремого preflight і погодженого sequencing.
- **Статус:** COMPLETED
- **Завершено:** 2026-11-22 (Asia/Manila)

## 2026-11-22 — Deployment readiness, stage 2

- **Status:** IN_PROGRESS
- **Started:** 2026-11-22 (Asia/Manila)
- **User request:** Виконати Stage 2 локальної Linux build і deployment readiness підготовки для War of Machines за вкладеним запитом.
- **Scope:** API .NET 10 LTS migration, health/readiness, HTTPS host configuration, production Compose/PostgreSQL/Caddy templates, DB roles/secrets/proxy settings, server deployment scripts/templates, testing and docs coordinated with Unity. No production connection/deployment/migration, paid service, real secret, destructive migration, gameplay/security weakening, automatic commit, or Stage 3.
- **Acceptance criteria:** Complete available local changes in the attachment; preserve auth/API/data semantics; run restore/build/tests/Compose validation where available; report Unity build/runtime separately; preserve existing uncommitted/generated changes; update deployment docs and close journal with truthful status/readiness.
- **Initial state:** Stage 1 docs and current Git status are to be reviewed before implementation. Stage 1 records tracked API bin/obj modifications from its build and no production DB state verification; preserve all current changes.
- **Next step:** Read Stage 1 docs and inspect current API Git state, docs map, target/package/security owners, local SDK/runtime/Docker availability and deployment-related files.


### Stage 2 final checkpoint — 2026-11-22
- **Status:** BLOCKED; Stage 3 readiness: NO.
- **Completed:** API target/runtime moved to .NET 10 with pinned packages; added generic liveness and database-aware readiness routes, production Data Protection key persistence/encryption requirements and configurable HTTPS host settings. Added Production Compose/Caddy, private Postgres network/roles/TLS, explicit migration job, health probe, secret templates, and PostgreSQL backup/guarded restore scripts. Deployment, progress and checklist docs record Stage 2 results and the staging gate.
- **Validation:** API `dotnet build --no-restore` succeeded with 0 errors; one NU1900 warning remains because vulnerability-feed/cache access was denied. HealthProbe build succeeded with 0 warnings/errors. Compose static config validation passed. Local smoke against a fake unreachable DB returned 200 from `/health/live` and generic 503 from `/health/ready`; temporary local PFX produced a persistent encrypted Data Protection key without startup warning. `git diff --check` passed. `dotnet test` exited 0 but found no checked-in test project and ran no test cases.
- **Blockers:** Docker Desktop Linux daemon is unavailable and WSL returns `Wsl/E_ACCESSDENIED`; therefore the container image was not built or run, Linux server runtime/scene/shutdown and UDP/DTLS/client handshake were not tested, and deployment shell scripts were not syntax-checked or executed on Linux. No staging host/network/capacity proof exists.
- **Safety boundary:** No online/production DB connection, production endpoint, migration, real credential/certificate, host purchase or deployment was used. The user's permission to change an online DB was not needed for this local readiness stage.
- **Next step:** Stage 3 must begin only with an isolated Linux staging environment and complete the runtime/network, script, database restore/migration, and capacity gates before public or production work.
- **Completed locally:** 2026-11-22 (Asia/Manila); readiness remains blocked pending external Linux runtime evidence.
## 2026-11-22 — Stage 2B unblock Linux runtime testing

- **Status:** IN_PROGRESS
- **Started:** 2026-11-22 (Asia/Manila)
- **User request:** Continue War of Machines Stage 2B: diagnose WSL/Docker access, use the existing Linux Dedicated Server build, run isolated local runtime/Compose/script checks where possible, investigate NU1900, and update deployment docs without starting Stage 3.
- **Scope:** Read-only Windows/WSL/Docker diagnostics first; then only non-destructive authorized fixes and isolated local tests. No production connection, destructive WSL/Docker repair, data-volume deletion, Windows feature changes or reboot without user confirmation, real secrets, VPS purchase, or commit.
- **Acceptance:** Truthfully establish WSL/Docker cause and status; test Unity Linux server, UDP, API/Postgres/Compose, scripts and NuGet audit where accessible; document blockers and stop if access requires a user/admin action.
- **Initial state:** Prior Stage 2 journal says local API build/health probe/Compose static config passed, but Linux runtime and script checks were blocked. Existing tracked/generated changes must be preserved.
- **Next step:** Read project instructions/journals and the Stage 2 deployment docs, then gather read-only WSL/Windows feature/hypervisor/Docker diagnostics before attempting any repair.
### Stage 2B checkpoint — diagnostic results (2026-11-22)
- **Status:** BLOCKED pending one host-side read-only diagnostic; Stage 3 not started.
- **Docker:** CLI 29.8.2/Compose 5.5.1 work, but the active `desktop-linux` pipe is absent. `docker desktop start` fails because the Docker Desktop registry key is missing. No image/container/volume was touched; API/PostgreSQL image and health tests could not run.
- **NuGet:** NuGet service endpoints returned HTTP 200. Default audit still fails on sandbox path access; using a task-local HTTP cache/temp override, the vulnerability audit completed and found no vulnerable packages from configured sources.
- **Host boundary:** WSL 3.0.5 version info is readable, while status/list and Windows feature/CIM/registry reads are denied. Codex rejected the requested host-level elevation because `sandbox_approval` is disabled. This is not evidence that WSL is damaged; host distro/feature/hypervisor state is unknown.
- **Runtime/scripts:** Unity ELF was not launched, UDP 7770 and DTLS remain NOT VERIFIED. Git Bash `bash -n` fails before parsing with `NtCreateDirectoryObject ... 0xC0000022`; ShellCheck is unavailable. Deployment scripts were not executed.
- **Documentation:** Added Stage 2B evidence, checklist and recovery action to the three deployment documents. No source or production configuration changes were made.
- **Next step:** Ask the user to run the documented read-only WSL/Windows-feature/hypervisor block in Administrator PowerShell outside Codex and provide the output; then resume with the smallest safe correction.
### Stage 2B recovery update — Ubuntu available per user (2026-11-22)
- **Status:** IN_PROGRESS.
- **User-provided environment evidence:** User reports Ubuntu 24.04.5 LTS is installed under WSL 2, distro name `Ubuntu-24.04`, Linux account `toraspc`; they verified `whoami` and `/etc/os-release` on the Windows host. This is recorded as user-provided until Codex independently connects.
- **Decision:** Do not reinstall WSL or the distro. Attempt the exact requested read-only `wsl.exe -d Ubuntu-24.04 --exec whoami` once from the current execution environment; if sandbox-denied, stop host attempts and request the minimum needed output.
### Stage 2B resume checkpoint — user reports Ubuntu; Codex still blocked (2026-11-22)
- **Status:** BLOCKED; Stage 3 not started.
- **User update:** User states host WSL 2 Ubuntu 24.04.5 LTS (`Ubuntu-24.04`) is installed and verified; Linux username is `toraspc`.
- **Validation:** Codex ran `wsl.exe -d Ubuntu-24.04 --exec whoami` once; it returned `Wsl/E_ACCESSDENIED`. No repeated attempts. Codex's host-shell escalation request was rejected by the granular approval policy. The computer-use workflow forbids automating Windows terminal apps, so no alternate authorized terminal route is available.
- **Remaining:** No Unity Linux runtime, UDP 7770, API/PostgreSQL containers, server registration/tickets, backup/restore, DTLS, or Linux script execution was possible. Docker Desktop CLI start also reports its installation registry key missing; Docker Engine status inside Ubuntu is unknown.
- **Documentation:** Updated deployment progress/checklist to distinguish user-reported host Ubuntu from independently verified Codex access.
- **Next step:** User must provide an approved host-shell execution route to this task. Keep Ubuntu and Docker data intact; do not start Stage 3.
## 2026-11-22 — Stage 2 final closeout under owner-approved scope

- **Status:** IN_PROGRESS
- **Started:** 2026-11-22 (Asia/Manila)
- **User decision:** Close Stage 2 as “COMPLETED — LOCAL PREPARATION SCOPE”; move all unperformed Linux/Docker/UDP/DTLS/multiplayer runtime verification to Stage 3. Do not start Stage 3 automatically.
- **Scope:** Verify current Stage 2 artifacts/evidence; update deployment plan, progress, checklist and deployment-hosting guide with the accepted scope, deferred tests, Stage 3 prerequisites and OVHcloud staging gate. No VPS purchase/provisioning, production change, test execution, commit, or discard.
- **Acceptance:** Mark local preparation complete only; list every deferred test as NOT TESTED — DEFERRED TO STAGE 3; state Stage 3 as isolated staging; explain approval, access, pricing and test-credential prerequisites; keep production readiness NO.
- **Initial state:** Prior Stage 2B logs record user-confirmed Ubuntu 24.04.5 LTS/WSL2, but Codex's WSL invocation returns E_ACCESSDENIED. Docker Desktop is not registered/available to this sandbox. Preserve all existing source/generated changes.
- **Next step:** Read current repository instructions and deployment docs, verify artifact/config existence and source owners, then update docs without running runtime tests.


### Stage 2 closeout completed — 2026-11-22
- **Status:** COMPLETED — LOCAL PREPARATION SCOPE (per explicit owner decision).
- **Changed:** Updated `docs/deployment/DEPLOYMENT_PLAN.md`, `DEPLOYMENT_PROGRESS.md`, `DEPLOYMENT_CHECKLIST.md`, and `docs/codex-app/deployment-hosting.md`. Historical pre-closeout blocked diagnostics remain identified as historical; Stage 2 is now complete only for local preparation.
- **Verified artifacts:** Existing Unity ELF/profile; .NET 10 API target/output and health route definitions; Compose/Caddy/PostgreSQL, health probe, secret, migration-job, Unity systemd/deploy and PostgreSQL backup/restore templates. NuGet audit with a task-local cache reported no vulnerable packages from configured sources.
- **Deferred:** Linux startup/scenes/UDP/DTLS, API/PostgreSQL containers, Compose runtime, migrations, test DB backup/restore, Linux script execution, registration/heartbeat, tickets, matchmaking, bots, 20-machine match, performance and concurrency are NOT TESTED — DEFERRED TO STAGE 3.
- **Stage 3:** ISOLATED STAGING DEPLOYMENT AND RUNTIME VERIFICATION. Ready to begin preflight subject to approved SSH/network access and owner-approved VPS provisioning. First action: verify current OVHcloud price/availability. No VPS purchase/provisioning, Stage 3 deployment, production access, production migration, or real secrets.
- **Validation:** Re-verified artifact existence/ELF header and deployment-file presence; API and Unity `git diff --check` passed. Builds were not repeated because existing successful reports/artifacts were verified.
- **Production readiness:** NO; runtime tests remain required.


## 2026-11-22 — Stage 3A OVHcloud VPS provisioning and secure access

- **Status:** IN_PROGRESS
- **Started:** 2026-11-22 (Asia/Manila)
- **User request:** Begin only Stage 3A: assess permitted remote administration, prepare/acquire an OVHcloud Europe VPS under the specified limits, establish secure SSH access and initial Ubuntu hardening. Do not deploy API/PostgreSQL/Unity, begin Stage 3B, or touch production.
- **Scope:** Inspect current deployment docs and project state; verify local SSH/SCP and allowed remote-command/network path before any purchase; check live official pricing, taxes, billing term, renewal, fees, region and availability; support owner-led login/2FA/payment; configure only the purchased staging host after provisioning. Update Stage 3A deployment docs without recording secrets.
- **Acceptance:** Do not purchase if Codex lacks a realistic permitted admin path; do not submit payment or legal acceptance for the owner; disclose exact total and ask for confirmation if above $15/month; verify VPS identity/specs/access/security only when actually available; mark every unverified item accurately; close at Stage 3A and stop before Stage 3B.
- **Initial state:** Stage 2 final docs report local preparation complete and runtime tests deferred. Repository instructions and prior task journal were read. Current working-tree state has not yet been inspected.
- **Next step:** Inspect existing Git changes and deployment documentation, determine whether OpenSSH/SCP plus permitted outbound SSH is viable, then decide whether browser purchase preparation can proceed safely. No VPS order, payment, remote connection, or hardening action has occurred.

### Stage 3A checkpoint — administration path and registration handoff (2026-11-22)
- **Status:** IN_PROGRESS; no VPS has been ordered or provisioned.
- **Remote administration path:** Windows OpenSSH 9.5p2 and SCP are installed. With the task-scoped network permission granted, a non-authenticated TCP connection to github.com:22 succeeded (TcpTestSucceeded=True). PowerShell emitted a CIM access warning for optional source/interface details, but the TCP handshake succeeded. This is a viable shell-based SSH path to verify against the actual VPS after provisioning; there is no dedicated SSH connector.
- **Official offer found:** OVHcloud's live configurator identifies VPS-3 2027 as 6 vCore, 12 GB RAM, 100 GB NVMe, unlimited traffic at 2 Gbps, daily backup included, starting at $12.32/month excluding tax. Region availability, no-commit renewal price and tax-inclusive total still need verification in the exact order configuration.
- **Registration:** User confirmed tax residence Ukraine. Account settings were set to Ukraine, USD, and OVHcloud World (English) and saved. The visible next page is the OVHcloud account creation form requesting email/password and acceptance of Terms. These credentials and legal acceptance are for the user to enter/complete; marketing opt-in remains unchecked. Browser handoff is pending.
- **Unverified:** Exact tax-inclusive recurring cost, commitment/renewal terms, Germany/France availability, Ubuntu 24.04 image selection, final order total, VPS creation/status, IP/hostname, host fingerprint, SSH login, sudo, firewall, hardening and resource specs.
- **Next step:** User completes account creation, email verification and 2FA directly in the visible official browser, then notifies Codex. Do not submit a VPS order or payment until the exact configuration/total has been verified and the user performs the payment confirmation. Continue only Stage 3A.

- **Progress update:** OVH account creation has advanced to the account-type selection screen. User was advised to select Private individual only if the account/billing is personal, or Company only if a legal entity will own the account; the user should make and confirm this classification. Credentials, Terms acceptance, email verification and 2FA remain user-operated. Browser handoff is pending; no order or payment has been submitted.
- **Address-country clarification:** User confirmed that Ukraine is also the correct postal/billing residence country for this personal OVH account. The next form requests personal name/address/phone and an accuracy attestation; these remain user-entered/submitted directly in the OVH browser. No personal details, credentials or payment information were requested in chat or recorded in this journal. Continue only after the user completes the form.
- **Correction and pause:** User clarified that the current living/mailing address is in the Philippines, although tax residence was answered as Ukraine. The signup page locks country of residence to Ukraine. Do not enter a Philippines address against a Ukraine country selection, invent a Ukrainian address, or submit this page. Official OVHcloud guidance on reconciling tax residence with current address must be checked; account registration remains pending and no personal address/phone was copied into the journal.
- **Address-form update:** At the user's direct request, entered the supplied Ukraine street/postcode/city values into the OVH account form; the postal-format error cleared and Confirm became enabled. Exact address values were intentionally omitted from this journal. The user must verify the fields, personal contact details, accuracy declaration, and optional SMS marketing consent; Codex did not submit the form. Account creation, Terms acceptance, email verification, 2FA, order, payment, VPS provisioning, and remote access remain incomplete.
- **Account setup checkpoint:** OVHcloud console is authenticated and the account profile page confirms signup details were saved after the owner reviewed/submitted them. The individual account profile is present; no service, order, invoice, or payment is present. Personal profile values are not copied into this journal. Next, inspect whether 2FA is enabled without changing account security settings.
- **Security check:** The authenticated OVHcloud profile reports 2FA disabled and no authenticator app or security key configured. This was a read-only check; no security setting was changed. The user must enable 2FA directly before account purchase/provisioning. The console shows no existing service/order/invoice.
- **VPS configurator checkpoint (2026-11-22):** Signup is complete and the account console is authenticated. Read-only account security inspection shows 2FA disabled; the user must enable it directly. In the no-commit VPS-3 2027 configurator, Poland/Warsaw is selected and shown available now; Germany and France are unavailable for this configuration. Ubuntu 24.04 is selected, but AMD64 and public IPv4 are not verified. The display is $14.50/month excluding tax; $12.32/month is the discounted 12-month price. Paid add-ons are off and daily backup is included. The tax-inclusive recurring total, renewal/auto-renewal terms and setup fees are unverified. No order, payment, VPS, SSH access, or Stage 3B deployment exists. Do not proceed to order until 2FA is enabled and the final price/details are checked; stop if the total exceeds $15/month without the user's revised budget approval. No personal address, credential, OTP, or recovery code is recorded.
## 2026-11-22 — Backend/API rebranding to RAV
- **Status:** IN_PROGRESS
- **Started:** 2026-11-22 (Asia/Manila)
- **User request:** Rebrand the backend as RAV API / RavApi, align safe code/project/config/docs/container/deployment names, preserve database and production compatibility, and audit old-name references.
- **Scope:** Backend-owned API source, project/solution metadata, local/staging configuration and documentation; no production changes, database data/schema destruction, real secret changes, destructive Git operations, or Stage 3B.
- **Acceptance criteria:** Apply only dependency-aware safe renames; keep API contracts/auth/database behavior intact; validate build, EF migration discovery, Compose/config/deployment references where available; document unavoidable legacy identifiers.
- **Initial state:** API root rules and latest journal entries read. Existing API task history contains prior deployment-stage work; repository identity and Git status still need a reliable check because the first Git invocation failed despite the supplied path contents being readable. No implementation files changed for this task yet.
- **Next step:** Verify the exact API repository root and complete its Git snapshot; inspect nested instructions/docs index and map owned brand identifiers before edits.
### Discovery checkpoint — brand and compatibility map (2026-11-22)
- Repository root is this directory; the source project is a nested folder. Pre-existing modified API source/docs, old net8 build outputs and untracked deployment templates are present. None are attributable to this task and all must be preserved.
- Owned C# namespace and EF model metadata strings use WarOfMachines.*. They can move to RavApi.* if migration IDs, SQL operations, table/column names and history remain unchanged; verify migration discovery/model snapshot after the rename.
- Local and production deployment templates contain WOM-prefixed variables, legacy local DB names/roles, service/container/volume names and an old external production hostname. Do not change actual .env.local, secrets, production endpoint, database identifiers, or production service/volume IDs; provide RAV-prefixed staging variables with explicit legacy-variable fallback.
- No implementation files changed for this task yet.
- **Next step:** Inspect dirty diffs and exact config/script consumers before editing; then apply only consistent source/config/documentation renames.
### RAV rebrand recovery checkpoint — 2026-11-22
- **Status:** IN_PROGRESS
- **Recovery finding:** Backend rebrand changes are present in this `RavApi` project; earlier discovery notes saying no implementation had started are historical and were superseded by later changes.
- **Changes present:** API project/solution/assembly and owned C#/EF metadata namespaces use RavApi; Dockerfile/Compose and deployment documentation reference the renamed project where safe. Production endpoints, auth identifiers, database names, volumes, and local data remain compatibility-preserved.
- **Validation already recorded:** Isolated `dotnet build` completed with 0 errors; two NU1900 warnings occurred because NuGet vulnerability metadata could not be reached. Original `obj` directory was restored. Tests, Compose runtime, production connectivity, and database operations were not run.
- **Working tree:** The directory rename appears as the old tracked `WarOfMachinesAPI` tree deleted plus the new `RavApi` tree untracked until Git stages/detects renames; changes remain unstaged and uncommitted. No user changes were discarded.
- **Next step:** After Unity Linux artifact verification, record the final coordinated rebrand status and legacy compatibility limits in both repository journals.

### RAV backend rebrand final audit checkpoint — 2026-11-22
- **Status:** BLOCKED for coordinated rebrand closeout pending Unity's final RAV build; API implementation and available checks are complete.
- **Validation:** `RavApi.csproj` / `rav-api.sln` are present; owned C# namespaces use `RavApi.*` (48 files) and no old `WarOfMachines.*` namespace imports/declarations remain. API build completed with zero errors and two NU1900 vulnerability-metadata warnings. Local and production Compose static configs passed with example/dummy validation inputs; no container or database was started or contacted. API `git diff --check` passed.
- **Compatibility:** Existing JWT issuer/audience, production HTTPS host, DB roles/schema/migration identifiers, local DB/volume names, persistent paths, and WOM credential/environment fallbacks remain documented for compatibility. `.env.local` was not read or changed; no production credential, endpoint, database, data, or service was touched.
- **Limitations:** No API/PostgreSQL runtime, migration command, or PlayMode tests were executed. Unity's only Editor build attempt predated the final namespace conversion and was blocked by Licensing Client IPC/headless entitlement errors, so the rebranded Linux artifact remains unverified.
- **Next step:** Restore Unity license/headless build access and run `RAV.Editor.LocalNetworkBuild.BuildLinuxDedicatedServer`; then close the coordinated rebrand only after that output is verified. Stage 3B remains not ready pending this and the independent Stage 3A gate.
## 2026-11-22 — RAV final Linux artifact validation (API coordination)
- **Status:** IN_PROGRESS
- **User request:** Support final RAV rebranding validation for the newly built Unity Linux Dedicated Server; verify API/rebrand/deployment references and update the coordinated report. Do not run WSL/runtime tests, start Stage 3B, alter production, or buy a VPS.
- **Scope:** Read-only API build/reference/config/deployment audit and documentation update; no API runtime or database connection.
- **Acceptance:** Confirm API build evidence and safe compatibility identifiers; synchronize final status with the Unity artifact evidence; keep Linux runtime as NOT TESTED.
- **Initial state:** API RAV namespace/project migration and successful API build were previously recorded; preserve all existing dirty and untracked files. No new API implementation changes are planned.
- **Next step:** Inspect current Git state and relevant RAV/deployment documentation before synchronizing the report.
### RAV backend rebrand final closeout — 2026-11-22
- **Status:** COMPLETED as a coordinated rebrand; deployment/runtime checks are explicitly not included.
- **API build:** `dotnet build RavApi.csproj --no-restore --nologo -v:minimal` succeeded with zero warnings and errors. No test project/database/migration command was run.
- **Unity evidence:** Editor log confirms successful Linux Dedicated Server build after RAV namespace changes, with Server/Map scenes, zero errors and 47 classified nonfatal warnings. The separate RAV-named staging artifact was hash-verified; no Linux runtime test was run.
- **Compatibility:** Existing database schema/migration IDs, roles, local DB/volume identifiers, JWT issuer/audience, production HTTPS endpoint, and WOM aliases remain preserved. No production endpoint/database/secrets were accessed or changed.
- **Deployment boundary:** Stage 3A is still in progress; Stage 3B remains NOT STARTED. Linux startup, networking/DTLS, API/PostgreSQL runtime, script execution, backups/restores and PlayMode tests are NOT TESTED.
### Stage 3A live order review — 2026-11-22
- **Status:** BLOCKED pending owner review; no VPS purchased or provisioned.
- **OVH account security:** Authenticated control panel shows 2FA and the authenticator app enabled; this supersedes the earlier stale journal note that 2FA was disabled.
- **Preferred region:** The saved VPS-3 2027 configuration matched Warsaw, Poland, Ubuntu 24.04, 6 vCore, 12 GB RAM, 100 GB NVMe and no commitment. The checkout reported HTTP 400, “out of stock in region WAW”; no order was created.
- **Available alternative reviewed:** A fresh Europe/UK configuration with the same VPS-3 specs and Ubuntu 24.04 reached the payment page. Its summary showed no commitment, $14.50/month before tax, $0.00 tax and $14.50/month with tax. The checkout showed an Automated Backup Standard line at +$0.80 with a matching -$0.80 offer/discount; Local Storage is included. Optional Snapshot backup, Premium automatic backup and extra storage were left unselected. Whether the backup discount expires and what the recurring renewal price becomes are not disclosed in the observed summary.
- **Order boundary:** The UK checkout is at Step 2 Payment only. No Terms of Service were accepted, no payment method was selected, and Pay was not activated. Do not submit payment.
- **Billing-data consistency:** The authenticated profile displays a Ukraine address while the phone uses a Philippines country code; the user previously said they live in the Philippines. No address or phone values are recorded here. The owner must ensure the billing/tax residence and address are accurate before accepting terms or paying.
- **Still unverified:** WAW availability, tax treatment after accurate billing details, recurring price after any offer, IPv4 allocation, initial/setup/renewal charges, actual VPS provisioning, host fingerprint and SSH login. Windows OpenSSH/SCP exists and a general TCP/22 path was previously verified, but no VPS endpoint is available to test.
- **Next step:** Owner reviews the UK alternative, corrects/confirms accurate billing/tax details directly in OVH, and confirms the purchase at the payment step. Continue only Stage 3A after successful owner payment; do not begin Stage 3B.
- **Warsaw availability clarification:** The checkout confirmed WAW is out of stock for this attempt; only future restock timing is unknown.

### Stage 3A owner billing confirmation - 2026-11-22
- **User confirmation:** The owner confirms the Ukraine address already saved in the OVH profile is genuine and current as their registration/billing address. Do not change it or substitute a Philippines address.
- **Checkout:** The current Payment page displays the confirmed profile address and the UK VPS-3 2027 order at $14.50/month including $0 displayed tax. No Terms were accepted, payment method selected, or payment submitted.
- **Still not verified:** Payment-provider address compatibility (no method selected), public IPv4, setup/initial charges, and renewal amount/promotion duration for the Automated Backup Standard line (+$0.80 offset by -$0.80 currently).
- **Boundary:** The user must personally accept Terms and authorize payment. Continue Stage 3A after successful purchase only; Stage 3B remains NOT STARTED.

## 2026-11-22 — RAV Stage 3A post-order VPS verification
- **Status:** IN_PROGRESS
- **Started:** 2026-11-22 (Asia/Manila)
- **Request:** After the owner completes OVHcloud order #23370514, verify payment, provisioning, plan, region, OS, resources, IPv4, next billing and renewal cost; continue Stage 3A hardening and Docker setup only if the VPS is active and authorized SSH is available; update deployment docs; do not start Stage 3B.
- **Scope:** Read-only OVH order/account inspection until a VPS endpoint exists; if provisioned and SSH is permitted, establish verified Ed25519 access, safely harden Ubuntu, configure firewall/security updates/Docker/systemd/logging, and verify access. No production credentials/database, no public PostgreSQL, and no weakening SSH before alternative access is verified.
- **Acceptance:** Truthfully report payment and provisioning status, actual VPS details, SSH/access status and remaining blockers; update deployment documentation with evidence and limitations.
- **Findings:** Confirmation page states order #23370514 was processed. Order tracking history at 2026-11-22 21:53 (Asia/Manila) says “Checking your order”; additional processing is required, usually around 48 hours, and support may contact the account owner. The control panel's VPS service view says “You have no VPS.”
- **Findings:** Order summary lists VPS-3 2027 monthly fees, Linux monthly fees, 6 vCPU / 12 GB RAM / 100 GB disk, Local Storage and Automated Backup Standard. Ubuntu 24.04 was selected on the pre-purchase summary, but the installed image is not yet confirmed because provisioning has not completed. The payment-tracking list has no result and invoice history had no invoice result at inspection; payment completion is therefore not independently confirmed by the billing panel. No IP, active service, SSH endpoint, billing date, or renewal price is available yet.
- **Security decision:** Do not generate/install a persistent SSH credential or attempt SSH without a provisioned host/IP. No attempt to bypass sandbox/network controls.
- **Next step:** Record the order-review hold in deployment docs and wait for OVH processing / service provisioning; then re-check the service, billing and endpoint before Stage 3A setup.
- **Documentation update:** Appended the post-purchase checkpoint to `docs/deployment/DEPLOYMENT_PROGRESS.md` and `docs/deployment/DEPLOYMENT_CHECKLIST.md`, including the order-review hold, observed plan summary, payment uncertainty, missing host/IP, security boundary and next step. The latest tails were reread to verify these additions.
- **Working-tree preservation:** The pre-existing RAV rename state (old `WarOfMachinesAPI` paths deleted in Git view, new `RavApi/` and `deploy/` untracked, plus existing root changes) was observed and left untouched; no staging or commit.
- **Validation:** OVH order tracker visibly shows “Checking your order” with the usual ~48-hour processing notice; authenticated VPS services view says “You have no VPS.” Payment tracking and invoice table showed no result. Documentation tail verification succeeded. No SSH, key, firewall, OS, Docker or deployment action was possible or attempted without an active endpoint.
- **Final status:** `BLOCKED` pending OVH order review/provisioning. Resume Stage 3A when the VPS appears in the service list or OVH requests an owner action. Stage 3B remains NOT STARTED.

## 2026-11-22 — RAV Stage 3A order status recheck
- **Status:** IN_PROGRESS
- **Started:** 2026-11-22 (Asia/Manila)
- **Request:** Recheck order #23370514 in OVH and continue secure VPS setup only if it is active and has an endpoint; refresh deployment documentation; do not place a second order or begin Stage 3B.
- **Scope:** Read-only account/order status, then proceed only with supported SSH/network access to the already purchased VPS. Preserve all prior working-tree state and avoid production resources/secrets.
- **Acceptance:** Determine whether the prior review hold changed; verify payment, service, plan, region, OS, resources, IP and renewal facts where visible; if still pending, record the exact wait/owner action and leave Stage 3A blocked.
- **Recovery state:** Previous checkpoint found order “Checking your order” (additional processing usually around 48 hours), no VPS service, no payment/invoice result, no IP or SSH endpoint. The prior checkpoint updated progress/checklist and stopped safely.
- **Next step:** Recheck the existing OVH order and VPS service state before any technical setup.
- **Recheck finding:** Fresh order tracker still reports “Checking your order”; expanded history adds “Your payment is being validated” (2026-11-22 21:53). Fresh service view still reports “You have no VPS.” No newer progress is visible.
- **Documentation update:** Appended this payment-validation checkpoint to `docs/deployment/DEPLOYMENT_PROGRESS.md` and `docs/deployment/DEPLOYMENT_CHECKLIST.md`.
- **Current status:** `BLOCKED` on OVH payment validation/order review and service provisioning. Wait for OVH or an owner verification request; then re-check before SSH. No second order, SSH, host changes or Stage 3B.

## 2026-11-22 — RAV Stage 3B deployment readiness preparation
- **Status:** IN_PROGRESS
- **Started:** 2026-11-22 (Asia/Manila)
- **Request:** While OVH order #23370514 is pending, prepare and verify the complete RAV Linux deployment bundle and automated Stage 3B runbook. Do not recheck OVH, deploy to a VPS, contact production, run real DB migrations, add paid services, or start Stage 3B.
- **Scope:** Inspect existing Unity/API artifacts, Docker Compose/PostgreSQL/Caddy/systemd/configuration/deployment/rollback/backup scripts; verify rename paths and Unity/API/game-client networking contracts; improve staging-only deployment automation and documentation where safe; add static/local tests and performance test plan without production secrets.
- **Acceptance:** Bundle is self-contained and path-consistent; secrets are generated only during provisioning and remain outside Git; scripts have static checks; rollback and DB backup/restore procedures are safe; smoke/performance tests are prepared with explicit untested items; deployment progress/checklist updated.
- **Initial state:** Rebranding and builds are recorded complete; VPS provisioning is pending and this task must not revisit its status. Repository has pre-existing uncommitted rename work (`WarOfMachinesAPI` paths deleted, `RavApi/` and `deploy/` untracked, and existing root changes); preserve all of it, do not stage/commit/reset.
- **Next step:** Capture current Git state and inventory the deployment bundle and prepared Unity artifact before deciding which files need edits.

## 2026-11-23 — RAV concise Game Design Document
- **Status:** IN_PROGRESS
- **Started:** 2026-11-23 22:39 (Asia/Manila)
- **User request:** Fully analyze the RAV Unity game and backend API, then write a concise, professional game design document for the user to review and correct.
- **Scope:** Read-only audit of Unity client/dedicated server and RAV API source, project documentation, and available configuration. Create one user-facing document under the current task's `outputs` directory. Do not change game/API implementation, production, database, deployment, or runtime state.
- **Acceptance criteria:** Describe the actual player experience, core loop, roles/vehicles, match flow, progression, multiplayer architecture, and current limits using verified project evidence; separate implemented behavior from planned or unverified behavior; keep the GDD concise and readable; render and inspect the final document.
- **Initial state:** RAV Unity project is `D:\Programing\UnityProjects\rav-unity`; API source is nested at `D:\Programing\ServerProjects\rav-api\RavApi`. Existing local task journals contain unrelated deployment tasks and user changes; preserve all existing entries and project state.
- **Next step:** Inspect repo status, nested instructions, source/docs inventories and the existing gameplay/API contract; then draft, render, visually inspect, and close this task entry.
### Source inspection checkpoint — 2026-11-23
- Both requested projects are present at the RAV paths. The Unity working tree contains extensive pre-existing modified and untracked gameplay, asset, API-client, and rebrand/deployment files; the API working tree contains extensive pre-existing rebrand/deployment changes. All are preserved and treated as the current source of truth.
- Unity documentation index maps the relevant menu/progression, combat, bots, matchmaking/spawn, maps, and FishNet transport owners. API documentation maps profile, catalog/research, server discovery, authentication, match result/reward, maps and leaderboard contracts.
- The only edits so far are this task's append-only journal entries in each repository. No game/API implementation, assets, scenes, configuration, or deployment state has been changed.
### Design audit findings — 2026-11-23
- Current Unity match configuration: 20 total slots, bots enabled to fill open slots, 300-second match timer, two MMR-balanced teams, and one playable map scene (`Map`). Current end rule awards victory when only one team has living vehicles; timeout currently ends as a draw even though the end screen reports team kill totals.
- Current API seed catalog: two factions (Iron Alliance tracked and Nova Syndicate biped), three vehicle classes (Scout, Guardian, Colossus), and a multi-level research tree. Twenty-one vehicle records are seeded; eight are marked visible and thirteen unavailable. New API registrations initialize 0 MMR, 50,000 Bolts, 5,000 Free XP, and an IA Skirmisher starter vehicle.
- API owns player/session authentication, one-time server-bound connection tickets, dedicated-server registry/discovery, catalog/research/ownership, match records, rewards, profile statistics, and leaderboards. `Adamant` is present in profile data but no active earn/spend loop was found in current controllers.
- Integration questions for the draft: API seeds `Demo Yard` and `Steel Arena`, while Unity currently lists only the `Map` gameplay scene; timeout-result policy needs owner confirmation. No RAV API social, campaign/lore, monetization, or release-platform system was found in the inspected current scope.
- No tests or runtime processes were launched. Source and project documentation remain unchanged; only append-only journals were updated.

#### Завершення — 2026-11-23
- Створено професійний короткий GDD українською: `C:\Users\TorasPC\Documents\Codex\2026-11-23\new-chat-4\outputs\RAV_Game_Design_Document_Draft_v0_1.docx`.
- Зафіксовано перевірені поточним кодом правила бою, бойову систему, фракції та класи, прогресію/економіку й межі інтеграції Unity–Dedicated Server–API; окремо винесено авторські рішення для підтвердження.
- Через відсутність LibreOffice DOCX експортовано у PDF встановленим Microsoft Word; обидві сторінки переглянуто як зображення. Після компонування фінальна версія має 2 сторінки без відірваного останнього пункту.
- Зміни у вихідний код гри й API не вносилися; тести не запускалися.
- Статус: COMPLETED.

### Vehicle list verification checkpoint — 2026-11-23
- Read `docs/codex-app/README.md`, `vehicles-and-research.md`, and `robot-characteristics.md` before source review.
- Verified current `Data/SeedData.cs` entries with `IsVisible = true`: Iron Alliance — IA Skirmisher, IA Strider, IA Bulwark, IA Juggernaut; Nova Syndicate — Nova Wisp, Nova Flicker, Nova Aegis, Nova Titan. One level-I starter plus three level-II vehicles per faction; exact names/classes agree with the Ukrainian characteristics reference.
- Next: replace the GDD's aggregate catalog paragraph with only these eight names grouped by faction, then render and inspect the revised document. API implementation remains read-only.

### Scope/source checkpoint — 2026-11-23
- The user confirmed API scope. Normal registration in `RavApi/Controllers/AuthController.cs` currently grants 50,000 Bolts and 5,000 Free XP; the empty-database `testuser` in `RavApi/Data/SeedData.cs` is a separate default fixture with 10,000 / 0 and MMR 1000.
- Planned grant change: both future registrations and a newly created default test profile on an empty database will start with 1,000 Bolts and 500 Free XP. Keep the test fixture's MMR 1000. Do not retroactively edit existing database rows. Update authentication, player-profile and seed docs. Do not touch production/runtime/database.
- Next: patch exact grant initializers and docs; record static verification only, with no tests.

### Implementation checkpoint — 2026-11-23
- `AuthController.Register` now initializes future registered players with `Bolts = 1000` and `FreeXp = 500`. `SeedData.Initialize` uses the same currency grant for the development-only `testuser` it creates when the player table is empty; its MMR remains 1000.
- Updated `authentication-authorization.md`, `player-profiles.md`, and `database-ef-core.md` to describe registration/default-seed grants and state that persisted accounts are not retroactively changed.
- No database/runtime was opened; no schema or migration changed. Next: targeted static checks and close this entry.


### Task closeout — 2026-11-23 23:26 (Asia/Manila)
- Future API registrations now receive Bolts = 1000 and FreeXp = 500. The empty-database development test-user seed uses the same balances; its MMR remains 1000.
- Updated authentication-authorization.md, player-profiles.md, and database-ef-core.md. Existing persisted accounts are unchanged; no migration or database operation was performed.
- Static scans confirm the new initializers and no remaining old starting balances in API C# source. No tests/build/runtime were run.
- Pre-existing rebrand/deployment working-tree state remains preserved.
- Status: COMPLETED.

### Git fork sync after repository rename — 2026-11-23
- **Status:** IN_PROGRESS.
- **User request:** Restore/sync the Unity and API repositories in the fork after the repositories were renamed.
- **Safety scope:** Inspect existing branch/upstream/remotes and working-tree changes first. Preserve all local modifications. Do not commit, reset, rebase, clean, or discard anything. Fetch remote refs; only fast-forward a local branch if its upstream and working tree make that safe.
- **Acceptance:** Identify the configured fork/upstream repositories, determine whether the local clones are behind or point to obsolete names, sync both repos safely where possible, and report any unresolved fork-hosting issue.
- **Next:** Inspect Git status/remotes/current branch tracking without changing source files.
#### Fork sync findings — 2026-11-23
- GitHub HTTPS fetch succeeded for the existing origin repositories without changing the checked-out source. Both local main branches are exactly aligned with fetched origin/main (0 commits ahead, 0 behind); there was nothing to merge into either working tree.
- GitHub metadata identifies scorpioner2010/war-of-machines-unity and scorpioner2010/war-of-machines-API as ordinary repositories (fork=false), and the current canonical names are still those old slugs. Candidate names scorpioner2010/rav-unity and scorpioner2010/rav-api were not found.
- SSH fetch failed because this session has no usable GitHub public key. HTTPS fetch worked. No origin URL was changed.
- Existing worktrees already had 313 (Unity) and 523 (API) changed/untracked entries. No checkout, merge, rebase, commit, reset, clean, or source-file change was made; only the append-only task journal was updated.
- Status: BLOCKED for the requested fork sync. No fork remote is configured, and the intended fork URLs/owner are not identifiable from the available remotes. Recovery: provide the GitHub URL(s) or owner/name of the intended RAV fork(s); then fetch and fast-forward only if safe.
#### Interpretation correction and Fork app handoff — 2026-11-23
- The user meant the desktop Fork Git client, not a GitHub fork relationship. The earlier GitHub metadata check is not the intended outcome.
- Fork is open on the unrelated VR-Demo repository; the RAV Unity and API local clones are not among the open tabs observed.
- A UI interaction was interrupted by repeated “user input was detected in this window” notices. Stopped UI control to avoid interfering with the user's active Fork session. No repository was added or removed in the app.
- Existing local clones remain present and their main branches are synchronized with their configured origin/main. The only repo changes by this task are the task-journal additions; the fetch updated Git remote-tracking refs without touching source or worktree files.
- **Status: BLOCKED on the Fork window being free for UI control.** Resume by adding the existing folders D:\Programing\UnityProjects\rav-unity and D:\Programing\ServerProjects\rav-api as existing repositories in Fork; do not clone over them or replace their working directories.
### Completed — RAV repositories added to Fork — 2026-11-24
- Correctly interpreted “Fork” as the desktop Git client. Opened the existing local repositories using File > Open Repository; the Fork window now shows separate tabs for rav-unity and rav-api alongside the pre-existing VR-Demo tab.
- Verified the displayed repositories are the existing paths D:\Programing\UnityProjects\rav-unity and D:\Programing\ServerProjects\rav-api. No duplicate clones were created and no repository source files were edited.
- HTTPS fetch updated/checks the existing origin refs; both local main branches are at 0 ahead / 0 behind origin/main, so no commit needed merging. Existing working-tree changes remain preserved.
- **Status: COMPLETED** for making both RAV repositories visible/open in Fork.