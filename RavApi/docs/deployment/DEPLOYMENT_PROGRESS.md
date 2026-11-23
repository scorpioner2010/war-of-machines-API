# Статус підготовки deployment

Останнє оновлення: 2026-11-22
Етап: Stage 3A of 5 — IN PROGRESS; Stage 2 completed as LOCAL PREPARATION
Статус: Stage 3A account signup in progress; no VPS order/provisioning or server access. Stage 3B runtime remains NOT TESTED — DEFERRED TO STAGE 3B.
Примітка: scope змінено за прямим рішенням власника; цей статус не означає runtime acceptance.

## Підсумок

Запитаний Stage 1 завершено без production connection, покупки сервера, production migration чи deployment operation. Поточний код і config перевірені локально; підтверджені факти, припущення, blockers і Stage 2–5 записані у [DEPLOYMENT_PLAN.md](DEPLOYMENT_PLAN.md).

## Що перевірено

- Обидва repositories існують; Unity та API Git trees були чисті від source/config changes перед аудитом. Поточні task journal зміни є append-only.
- Unity Editor version, Packages manifest/lock, FishNet/transport/DS versions, Windows build profiles, local build script і наявність відсутності Linux Standalone module.
- NetworkManager authenticator/DTLS component wiring, runtime API host allowlist, UDP port/environment overrides, server startup/heartbeat/unregister, additive room scenes, bot cap і server diagnostics.
- API csproj/Dockerfile, startup/auth/migration controls, appsettings production keys без виведення secret values, migrations, endpoint authorization, forwarded headers, CORS, host allowlist, health/logging setup.
- Git status/diff обох repositories: до API compile змінені лише обидва task journals; після build API tracked build outputs також стали modified (перелік див. нижче).
- Поточна production URL лише за статичними посиланнями у коді/config. Live availability/DNS probe не виконувався.

## Перевірки і результати

| Перевірка | Результат |
|---|---|
| API compile | Успішно: 0 warnings, 0 errors, target net8.0 |
| Unity Linux build | Не виконано: Linux Dedicated Server Build Support не встановлений і Linux profile/script відсутні |
| API tests | Не запускалися; у документаційній карті API немає checked-in test project |
| Local PostgreSQL current state | Не підключалися: Docker Desktop недоступний і 127.0.0.1:5433 закритий |
| Migration 20261119085129 | Історичний TASK.md запис підтверджує застосування до isolated local PostgreSQL 2026-11-19; сучасну DB state цього запуску не перевірено |
| Production DB / endpoint | Не підключалися за обмеженням Stage 1 |
| VPS/Ubuntu runtime, network, DTLS, 20 bots | Не виконувалися |

## Підтверджені blockers

1. Installed Unity Editor не має LinuxStandaloneSupport. У Unity project є Windows/Windows Server profiles, але немає Linux Server profile; LocalNetworkBuild.cs будує лише Windows64 Development outputs.
2. Backend target net8.0 і Docker images 8.0. Microsoft завершила support .NET 8 2026-11-10; перед public deployment перейти на supported LTS runtime.
3. Unity HttpLink production allowlist і API Production AllowedHosts обмежені Render hostname. Поточний build не може прийняти інший public API host через простий environment override.
4. API не має окремого live/readiness health endpoint. ForwardedHeaders:KnownProxies порожній, тому deployment має задати сталий trusted proxy IP.
5. Production database version, актуальна __EFMigrationsHistory, backup/data export і стан Render API не підтверджені. Історичні записи кажуть, що production migration не виконувалась; до окремого read-only preflight вважати production schema незміненою.
6. Немає Linux 20-slot match, двох simultaneous matches або VPS CPU/RSS/tick measurements. Один VPS можна рекомендувати тільки як capacity-gated initial beta target.
7. Чинний API/DB secret values не показувалися або не копіювалися у звіт. Реальні production keys/credentials/certificates треба створити окремо, не reuse локальні значення.

## Важлива зміна локального Git state під час аудиту

API build успішно пройшов, але проект зберігає build outputs під Git tracking. Після збірки Git позначає modified:
- WarOfMachinesAPI/bin/Debug/net8.0/WarOfMachines.dll, WarOfMachines.exe, WarOfMachines.pdb
- WarOfMachinesAPI/obj/Debug/net8.0/WarOfMachines.dll, WarOfMachines.pdb, WarOfMachinesAPI.AssemblyInfo.cs, cache/source-link/ref outputs та generated apphost

Ці артефакти не змінюють source behavior. У них немає секретів. Їх залишено як є, щоб не відкидати/відновлювати tracked changes без дотримання repo Git policy. Source/config змін від аудиту немає, окрім запитаних deployment docs і task journals.

## Наступні дії

- **Stage 2:** локально підготувати Linux server build, .NET 10 upgrade, health/hosting files і production HTTPS allowlist; production config/DB поки не торкати.
- **Stage 3:** staging build/network/auth/migration/restore/capacity tests.
- **Stage 4:** тільки після явного початку етапу — production data/backup preflight, migration і cutover.
- **Stage 5:** обмежений public test, monitoring, restore drill і поступове збільшення concurrency.

Користувач повідомив, що локальна БД була тимчасово встановлена для тестів і дозволив майбутні config зміни для online setup. Це не змінює межі Stage 1; production connection/config restoration не виконувалося.



## Stage 2 — local build and deployment readiness (2026-11-22)

**Historical pre-closeout status:** BLOCKED pending Linux runtime proof. The owner later redefined Stage 2 as local preparation; the final closeout below supersedes that Stage 2 status. Runtime checks remain untested and are deferred to Stage 3.

### Completed work

- Unity 6000.3.13f1 currently has LinuxStandaloneSupport and Linux Server Mono/IL2CPP variations installed. No editor module installation was needed.
- Created Assets/Settings/Build Profiles/Linux Server.asset with only Server.unity and Map.unity enabled, and added the Mono Dedicated Server build menu command.
- Build succeeded at D:\Programing\UnityProjects\war-of-machines-local-builds\LinuxServer-20261121-192956. The executable begins with ELF magic 7F-45-4C-46. Mono was selected; output was produced by Unity's Linux Dedicated Server subtarget.
- Added a configurable exact HTTPS production API host allowlist resource for Unity. Localhost remains Development-only; TLS verification behavior is unchanged.
- Migrated the API project and Docker build stages to .NET 10 and compatible pinned dependencies. Added anonymous GET /health/live and database-aware GET /health/ready.
- Added Production Compose/Caddy configuration with a private Postgres network, private database ports, TLS, a trusted proxy address, separate runtime/migrator roles, explicit migration job, secret file mounts, and migrations/seed disabled during API startup.
- Production Data Protection keys use a persistent writable volume and require a mounted PFX encryption certificate plus password. File-backed Compose secrets preserve source-file access constraints; Stage 3 must verify container UID/GID and set source permissions (API UID/GID 1654, Postgres 999) before startup.
- Added Unity systemd, credentials, release packaging, atomic switch, rollback, health-check and logrotate templates; added PostgreSQL custom-format backup and empty-schema restore scripts.

### Validation

| Check | Result |
|---|---|
| Unity scripts compile | Passed in the open Unity Editor; no compiler errors. |
| Linux Dedicated Server build | Passed; ELF executable and expected Linux runtime files exist. |
| API build | Passed: 0 errors. One NU1900 warning remains because the package vulnerability feed/cache returned Access denied. |
| API health smoke | With a fake local DB connection to 127.0.0.1:1 and a temporary local Data Protection certificate, live returned 200 and ready returned 503 with only status=not_ready. One encrypted key file was created and no Data Protection warning appeared. |
| Compose static validation | Passed with docker compose config --quiet using the example environment file. |
| Automated test project | No test project is checked in; dotnet test returned 0 without executing test cases. |
| Docker image/container runtime | Not run: Docker CLI is present but the Docker Desktop Linux daemon pipe is missing. |
| Linux executable runtime | Not run: WSL command returns Wsl/E_ACCESSDENIED. No external Linux host was contacted. |
| Shell script syntax check | Not run: WSL is inaccessible and Git Bash cannot create its MSYS named object in this environment. Scripts were manually reviewed only. |

### Runtime gates from the pre-closeout diagnosis — deferred to Stage 3

1. Run the built server on Linux staging and verify headless startup, Server/Map scene load, graceful shutdown, UDP 7770/overrides, API registration/heartbeat, and DTLS client handshake.
2. Build the Docker images and exercise Compose health/readiness and a staging database backup/restore after Docker/Linux runtime becomes available.
3. Run shell syntax/lint checks for the deployment scripts in a Linux shell.
4. Review the NuGet vulnerability audit feed after the local access-denied condition is resolved.

No production endpoint or database was contacted; no production migration, deployment, secret creation, or gameplay change was performed. The readiness statement in this historical snapshot is superseded by the final owner-approved closeout below.

## Stage 2B — WSL/Docker access and remaining runtime gate (2026-11-22)

**Historical pre-closeout diagnostic status:** BLOCKED from the Codex execution environment. These observations do not establish that WSL itself was broken; the owner later moved runtime acceptance to Stage 3.

### Confirmed evidence

- `wsl.exe --version` succeeds and reports WSL 3.0.5.0, kernel 6.1.18.58.1-1, WSLg 1.0.79, and Windows 10.0.26200.9974.
- `wsl.exe --status` and `wsl.exe --list --verbose` both return `Wsl/E_ACCESSDENIED`. Reading the per-user WSL registration key is also denied. Windows optional-feature status requires elevation; CIM queries are denied. A request to run those read-only checks outside the sandbox was rejected by the Codex granular approval policy (`sandbox_approval: false`). Therefore distro registration, feature state, virtualization availability, and the host-side cause of the WSL denial remain **NOT VERIFIED**. No WSL reinstall or repair was attempted.
- Docker CLI 29.8.2 and Compose 5.5.1 are present; `desktop-linux` is the selected context and points to `npipe:////./pipe/dockerDesktopLinuxEngine`. The pipe is absent. `docker desktop status` says Desktop is not running; `docker desktop start` fails because `SOFTWARE\Docker Inc.\Docker Desktop` is missing. Docker Desktop could not be started from this environment; no image, container, volume, or database was changed.
- NuGet service endpoints returned HTTP 200. The normal package audit still hit `Access to the path is denied` in this sandbox. With a task-local writable HTTP cache/temp directory, `dotnet list ... package --vulnerable --include-transitive --no-restore` completed and reported no vulnerable packages from the configured sources. The earlier default-cache `NU1900` warning is a local path-access issue, not a confirmed package vulnerability.
- Git Bash exists, but each `bash -n` attempt fails before parsing with `NtCreateDirectoryObject ... 0xC0000022`. ShellCheck is not available. Deployment shell syntax, executable permissions, and script behavior remain **NOT VERIFIED**; scripts were not executed.
- The existing Unity ELF was not launched because no accessible Linux runtime/engine is available. UDP 7770, Linux dependencies, headless initialization, scene load/shutdown, API-to-server registration, and DTLS/client handshake are all **NOT VERIFIED**.
- No API/PostgreSQL containers were started. The prior Stage 2 API smoke and Compose static-config validation remain the only local evidence; container build, Compose healthchecks, persistence, restart, secret mount permissions, backup/restore, and Unity-to-API integration remain **NOT VERIFIED**.

### Smallest recovery action

Run this read-only block in **PowerShell opened as Administrator outside Codex**, then send its output. Do not enable/disable Windows features, install/reinstall WSL or Docker, reset Docker, or reboot yet.

```powershell
wsl --status
wsl --list --verbose
Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux
Get-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform
Get-CimInstance Win32_ComputerSystem | Select-Object Manufacturer, Model, HypervisorPresent
docker desktop status
```

Expected output will show whether Ubuntu is registered and running, whether the WSL/virtual-machine features are enabled, whether a hypervisor is available, and Docker Desktop's host-side status. Once that separates host configuration from Codex sandbox restrictions, the minimum safe correction can be selected.

If this host cannot provide Linux locally, the deferred alternative is an isolated Ubuntu 24.04 AMD64 staging VM/host using test-only credentials and no production data. No staging machine was provisioned, no VPS was purchased, and Stage 3 was not started.
## Stage 2B recovery — Ubuntu confirmed on host, WSL still blocked in Codex (2026-11-22)

**Historical pre-closeout status:** BLOCKED before the owner scope decision; current status is stated in the final closeout below.

The user reports that Windows-host WSL 2 launches Ubuntu 24.04.5 LTS (`Ubuntu-24.04`), with Linux user `toraspc`; host-side `whoami` and `/etc/os-release` were verified by the user. From the Codex execution environment, the requested command `wsl.exe -d Ubuntu-24.04 --exec whoami` still returns `Wsl/E_ACCESSDENIED`. It was attempted once and not repeated.

There is no authorized host-terminal path available in this session: the explicit host-level shell escalation request was automatically rejected by the Codex approval policy (`sandbox_approval: false`), and the computer-use instructions prohibit running terminal commands through Windows UI automation. Therefore Codex cannot install Linux dependencies, launch the Unity ELF, inspect Linux UDP processes/logs, test Docker Engine inside Ubuntu, or run Linux deployment scripts.

Docker CLI/Compose are present, but Docker Desktop is not registered: `docker desktop start` reports the missing `SOFTWARE\Docker Inc.\Docker Desktop` registry key. The earlier Linux engine pipe is absent. Whether a separate Docker Engine is already installed inside the user-reported Ubuntu distro is **NOT VERIFIED**. No Docker data or WSL installation was changed.

| Stage 2B check | Result |
|---|---|
| Host Ubuntu/WSL 2 | User-reported verified; not reachable from Codex |
| Codex WSL invocation | Blocked: `Wsl/E_ACCESSDENIED` |
| Unity Linux runtime / Server and Map scenes / shutdown | NOT VERIFIED |
| UDP 7770 | NOT VERIFIED |
| API/PostgreSQL containers and Compose healthchecks | NOT VERIFIED |
| Registration/heartbeat, tickets, backup/restore | NOT VERIFIED |
| DTLS handshake | NOT VERIFIED |
| Linux deployment scripts | NOT VERIFIED; Git Bash cannot initialize in sandbox |

**Minimum recovery action:** provide this task an approved host-shell execution route that can invoke the user's WSL distro outside the current Codex sandbox. Keep the existing Ubuntu installation; no reinstall or Windows feature changes are indicated by the available evidence. The Stage 2B runtime tests can resume once that route is available. No Stage 3, production, or VPS action was taken.

## Stage 2 final closeout — LOCAL PREPARATION SCOPE (2026-11-22)

**Authoritative status: COMPLETED — LOCAL PREPARATION SCOPE.** The project owner explicitly revised Stage 2 to cover local build, code and deployment-configuration readiness. This status does not claim runtime tests passed; earlier Stage 2B “BLOCKED” records are pre-closeout snapshots.

### Verified local preparation

- Unity profile exists for Server/Map scenes. Existing Mono Dedicated Server binary: `D:\Programing\UnityProjects\war-of-machines-local-builds\LinuxServer-20261121-192956\WarOfMachinesServer.x86_64`; read header is ELF `7F-45-4C-46`. Prior Unity Editor build report records success.
- API targets `net10.0`; output DLL exists. Prior API build report records success with zero errors. Normal build context had NU1900 path-access warning.
- `GET /health/live` and database-aware `GET /health/ready` exist. Prior isolated process smoke returned live 200 and ready 503 against a deliberately unreachable test DB.
- Dockerfile, Compose, Caddy, PostgreSQL private-network/roles/TLS templates, health probe, secrets documentation and explicit migration job are present. Prior static Compose validation passed; containers were not started.
- Unity systemd/environment/logrotate templates and package/deploy/rollback/health scripts exist. PostgreSQL backup/restore scripts exist; their Linux execution is deferred.
- NuGet audit with a task-local writable cache reported no vulnerable packages from configured sources. Normal sandbox cache may still produce NU1900 due local path access.
- HTTPS/TLS checks, JWT, server authorization and tickets were not weakened. Production migration/seed startup remains disabled. No production endpoint, credentials, database or data were used.

### NOT TESTED — DEFERRED TO STAGE 3

Linux headless startup; Server/Map scene loading; Linux dependencies; shutdown/crash logs; UDP 7770; DTLS; FishNet authentication; API/PostgreSQL containers; Compose runtime health/persistence/restart; PostgreSQL migration tests; test DB backup/restore; Unity deployment-script execution; server registration/heartbeat; tickets; matchmaking; bot spawning; full 20-machine match; CPU/RAM/tick performance; multiple-match concurrency.

### Stage 3 — ISOLATED STAGING DEPLOYMENT AND RUNTIME VERIFICATION

The owner’s proposed target is OVHcloud VPS-3 in Germany or France, Ubuntu 24.04 LTS AMD64, planned 6 vCore/12 GB/100 GB NVMe. These are planning values, not a current vendor offer. Stage 3 must first check current price/availability and confirm an approved SSH/network route for Codex. Do not create or charge a VPS until the owner separately approves the exact offer. After approval, use only test credentials and a test database; install/configure Linux dependencies and Docker Engine as needed, deploy privately, then run the deferred checks in the checklist. No production secrets/data or Render changes.

**Ready for Stage 3 staging:** YES, SUBJECT TO VPS ACCESS AND PROVISIONING. No VPS was bought or provisioned. **Production ready: NO.**

## Stage 3A — current progress (2026-11-22)

**Status: IN PROGRESS.** Windows OpenSSH/SCP are available; outbound TCP/22 was verified with a non-authenticated probe only. The OVH account signup is complete, but read-only inspection of account security showed 2FA is disabled. The user must enable it directly.

The current VPS-3 2027 configurator is set to no commitment at $14.50/month excluding tax, with 6 vCore, 12 GB RAM, 100 GB NVMe, unlimited 2 Gbps traffic and daily backup included. The selected location is Europe — Poland, Warsaw, shown available now. Germany (Limburg) and the listed France locations are unavailable for this exact configuration. Ubuntu 24.04 is selected; AMD64 architecture is not displayed and remains unverified. Paid add-ons are off.

Public IPv4, the exact tax-inclusive recurring total, renewal/auto-renewal details and setup fees remain unverified. The $12.32/month public base figure is the discounted 12-month price; no commitment is selected at $14.50/month before tax. Because tax has not been shown, the user's $15/month cap is not yet verified.

No order, payment, VPS provisioning, SSH login, server hardening, Stage 3B deployment or production operation has occurred. Before the user submits an order, enable 2FA, verify AMD64, public IPv4, final tax-inclusive recurring price and renewal terms. Stop if the total exceeds $15/month unless the user approves a revised budget. The user handles the final order and payment. Stage 3B remains NOT STARTED.

## RAV rebranding — 2026-11-22

Unity repository: D:/Programing/UnityProjects/rav-unity. API Git root: D:/Programing/ServerProjects/rav-api. API project: RavApi/RavApi.csproj; namespace/assembly: RavApi. The production Dockerfile and Compose migrator now point at the renamed project, and new build image tags use rav-api. Compose service IDs, project name, volume names, local database/role, Data Protection path, JWT issuer/audience, and production HTTPS host remain unchanged. Production and database were not contacted.
## RAV rebrand validation amendment — 2026-11-22

- Source identity is RAV / Robotic Assault Vehicles / Tactical Multiplayer Combat. Unity first-party namespaces/asmdefs are RAV; backend project/assembly/namespace are RavApi. The current API build passed with zero errors and two NU1900 warnings for unreachable NuGet vulnerability metadata.
- Unity-generated `RAV.Gameplay.Shared` and `RAV.PlayMode.Tests` projects compiled with zero warnings/errors. The generated `Assembly-CSharp.csproj` build stopped during MSBuild project-reference resolution with exit code 1 and no compiler diagnostics.
- Unity Linux batch build was attempted and stopped after repeated Licensing Client connection failures; its log also reports the missing `com.unity.editor.headless` entitlement. No new RAV Linux artifact was created. The existing WarOfMachines binary above remains pre-rebrand historical evidence only.
- Local and production Docker Compose files both passed `docker compose config --quiet`; local used `.env.local.example`, production used dummy `.invalid` host and empty temporary secret files. No containers, production secrets, database, or production endpoint were accessed.
- The Linux build, Windows Client/Server builds, Unity Editor scene/prefab load, and PlayMode tests remain unverified. Stage 3B remains NOT STARTED.

## Final RAV rebranding validation — 2026-11-22 (authoritative; supersedes the prior build blocker)

- **Unity build: PASSED.** Unity Editor log records a successful Linux Dedicated Server Build Profile build from 19:55:57 to 19:56:46 local time. `Server.unity` and `Map.unity` were loaded. Build output included `Assembly-CSharp.dll`, `RAV.Gameplay.Shared.dll`, 64-bit x86_64 ELF launcher/runtime and Mono/native plugin dependencies.
- **Warnings:** 47 nonfatal warnings, zero build errors: 37 obsolete API, four unused-field, one unreachable-code warning in vendor/third-party code, and five FishNet upgrade-helper advisories.
- **Prepared artifact:** `D:\Programing\UnityProjects\war-of-machines-local-builds\RAVServer-20261122-1956`. Original Desktop output was not modified. Staging copy has matched `RAVServer.x86_64` / `RAVServer_Data` names, excludes `RAV_BurstDebugInformation_DoNotShip`, and all 189 retained files passed SHA-256 comparison.
- **API build: PASSED.** `dotnet build RavApi.csproj --no-restore --nologo -v:minimal` returned 0 warnings and 0 errors.
- **Runtime:** Linux startup/dependencies, UDP 7770, DTLS, FishNet registration, API/PostgreSQL runtime and deployment script execution remain **NOT TESTED**. No WSL command, production access, VPS purchase, or Stage 3B action occurred.
- **Rebranding:** COMPLETED for project/API names, namespaces, assembly references, RAV-first environment variables, deployment names and documentation. Intentional compatibility identifiers remain documented.
## Final RAV rebranding validation — 2026-11-22 (authoritative; supersedes the prior build blocker)

- **Unity build: PASSED.** Unity Editor log records a successful Linux Dedicated Server Build Profile build from 19:55:57 to 19:56:46 local time. `Server.unity` and `Map.unity` were loaded. Build output included `Assembly-CSharp.dll`, `RAV.Gameplay.Shared.dll`, 64-bit x86_64 ELF launcher/runtime and Mono/native plugin dependencies.
- **Warnings:** 47 nonfatal warnings, zero build errors: 37 obsolete API, four unused-field, one unreachable-code warning in vendor/third-party code, and five FishNet upgrade-helper advisories.
- **Prepared artifact:** `D:\Programing\UnityProjects\war-of-machines-local-builds\RAVServer-20261122-1956`. Original Desktop output was not modified. Staging copy has matched `RAVServer.x86_64` / `RAVServer_Data` names, excludes `RAV_BurstDebugInformation_DoNotShip`, and all 189 retained files passed SHA-256 comparison.
- **API build: PASSED.** `dotnet build RavApi.csproj --no-restore --nologo -v:minimal` returned 0 warnings and 0 errors.
- **Runtime:** Linux startup/dependencies, UDP 7770, DTLS, FishNet registration, API/PostgreSQL runtime and deployment script execution remain **NOT TESTED**. No WSL command, production access, VPS purchase, or Stage 3B action occurred.
- **Rebranding:** COMPLETED for project/API names, namespaces, assembly references, RAV-first environment variables, deployment names and documentation. Intentional compatibility identifiers remain documented.
## Stage 3A OVHcloud live offer review - 2026-11-22 (authoritative current status)

**Status: IN PROGRESS; owner review and purchase remain pending. No VPS has been bought or provisioned.** This update supersedes earlier Stage 3A notes that said 2FA was disabled and Warsaw was still orderable.

- **Account security:** The authenticated OVH console now shows 2FA and the authenticator app enabled.
- **Preferred offer:** The saved VPS-3 2027 offer matched 6 vCore, 12 GB RAM, 100 GB NVMe, Ubuntu 24.04, Europe/Poland-Warsaw and no commitment. On continuing checkout, OVH returned HTTP 400: `out of stock in region WAW`. No Warsaw order was created.
- **Available alternative:** A new Europe/United Kingdom VPS-3 2027 configuration with 6 vCore, 12 GB RAM, 100 GB NVMe, Ubuntu 24.04 and no commitment reached the Payment step. The order summary showed $14.50/month before tax, $0.00 tax and $14.50/month including the displayed tax. Unlimited traffic at 2 Gbps was shown in the configurator. This is an alternative to the preferred Warsaw location and has not been purchased.
- **Billing and extras:** Local Storage is included. Optional Snapshot backup, Premium automatic backup and additional storage were left unselected. The summary included an Automated Backup Standard line priced at +$0.80 with a matching -$0.80 offer/discount. The offer duration and post-promotion recurring amount were not shown; do not treat the current $14.50 display as a verified future renewal price.
- **Billing period:** No commitment is selected and the order is priced per month. The exact monthly renewal amount, promotion expiry, any setup fee and public IPv4 allocation remain NOT VERIFIED.
- **Billing profile gate:** The checkout displays a Ukraine billing address while the user previously said they currently live in the Philippines. The user selected Ukraine tax residence. The owner must ensure the tax residence and billing details are accurate before accepting the terms or paying. Exact contact/address values are intentionally omitted.
- **Payment boundary:** The UK offer is at Step 2 Payment. Terms are not accepted, no payment method is selected, and no payment/order was submitted. The user must review and perform any legal acceptance and payment themselves.
- **Access/deployment:** Windows OpenSSH/SCP and a general outbound TCP/22 route were verified earlier, but no VPS address/host key exists. SSH login, IPv4, host setup, firewall, hardening and all Stage 3B work remain NOT TESTED.

**Next gate:** Owner reviews the UK alternative, verifies accurate billing/tax data and the recurring post-offer price, and personally completes any terms/payment decision. Continue only Stage 3A after a successful owner purchase. Stage 3B remains NOT STARTED; production is untouched.

### Owner billing confirmation and final checkout hold - 2026-11-22

The owner confirmed that the Ukrainian address already saved in OVH is genuine and current as their registration/billing address. Do not replace it with a Philippines address or edit personal profile data. The OVH payment page displays this profile address and proceeds to the payment-method section without an address-validation error. Because no payment method is selected, compatibility with the owner's card/provider billing rules remains NOT VERIFIED.

The order remains VPS-3 2027, Europe/United Kingdom, Ubuntu 24.04, 6 vCore, 12 GB RAM, 100 GB NVMe, no commitment. It displays $14.50/month including the displayed $0 tax. Automated Backup Standard still shows +$0.80 offset by -$0.80; promotion duration and post-offer renewal amount are not disclosed. Public IPv4 and any additional setup/initial charges are not displayed. No Terms were accepted and no payment method or payment was submitted. Stage 3A is not complete; Stage 3B remains NOT STARTED.

## Stage 3A post-purchase provisioning checkpoint — 2026-11-22 (authoritative current status)

**Stage 3A: IN PROGRESS / BLOCKED on OVH order review. Stage 3B: NOT STARTED.** The owner completed OVH order **#23370514**. The confirmation page says the order was processed, but the control panel order tracker still says **“Checking your order”** and that additional processing is usually around 48 hours; OVH support may contact the owner. The VPS service list currently says **“You have no VPS.”** No duplicate order was created.

- **Order summary evidence:** VPS-3 2027, monthly fees, Linux distribution, 6 vCPU, 12 GB RAM, 100 GB disk, Local Storage and Automated Backup Standard. The earlier checkout selected Ubuntu 24.04 and the UK region (the configurator showed Erith), but neither the installed Ubuntu image nor the actual assigned datacentre has been confirmed after provisioning.
- **Payment:** OVH’s payment-tracking table showed no result; the invoice list also had no result at inspection. The order submission is confirmed, but completed payment is **NOT INDEPENDENTLY VERIFIED** in the billing panel.
- **Still unavailable:** active VPS, public IPv4, hostname, host fingerprint, next billing date, actual renewal amount and post-offer price. No SSH attempt or key generation was made because there is no provisioned endpoint.
- **Security boundary:** no production credentials/database or data were used; no security settings were weakened; PostgreSQL was not exposed; no deployment or Stage 3B work started.
- **Next step:** wait for OVH’s order review/provisioning (the page says it usually takes around 48 hours). If OVH requests verification, the owner should respond in the OVH account. Once the service appears, verify payment/invoice, actual plan/region/image/IP and renewal details, then continue Stage 3A secure SSH setup.

### Stage 3A payment-validation recheck — 2026-11-22

A fresh read-only check of order #23370514 in OVH shows no progress beyond the prior review checkpoint. Expanding the order history reveals **“Your payment is being validated”** at 2026-11-22 21:53; the latest order entry remains **“Checking your order”**, with additional processing usually around 48 hours and possible contact from OVH support. The VPS services view still says **“You have no VPS.”**

Payment is therefore **NOT COMPLETE / NOT CONFIRMED** by OVH’s order tracker. There is still no active service, assigned IP/hostname, installed OS confirmation, exact datacentre, next billing date or renewal amount. SSH and all host setup remain blocked until order/payment validation finishes and the service appears. No second order was created; no key/network attempt, production access, system configuration or Stage 3B action occurred. Keep the existing order tracker as the handoff page and wait for OVH review or a request to the account owner.
## RAV Stage 3B preparation resumed — 2026-11-22

The owner-provided status is that OVH order #23370514 is still under review with no active IP or SSH endpoint. This task did not recheck OVH. No VPS deployment, production access, database migration, or container startup occurred.

The Unity Editor completed a fresh Linux Dedicated Server build with Server and Map scenes. The verified staging build is D:\Programing\UnityProjects\war-of-machines-local-builds\RAVServer-20261122-staging-hostallowlist. It contains a 64-bit x86_64 ELF, Unity runtime, Mono runtime and matching RAVServer_Data; 189 retained files hash-match the staging payload. The staging API hostname allowlist fix is compiled into Assembly-CSharp.dll. The API Release build passed with 0 warnings and 0 errors.

Docker Compose static config and structural checks passed: no published API/PostgreSQL ports, internal backend network, staging-specific volumes and Caddy-only web ports. The final self-contained bundle and SHA-256 are recorded in DEPLOYMENT_PLAN.md. The package manifest, nested Unity archive and outer checksum were verified. The first-run guide includes a non-mutating Bash syntax gate before VPS setup.

Remaining tests need a provisioned staging VPS: SSH and host fingerprint, Ubuntu bootstrap/firewall, Docker/API/PostgreSQL/Caddy startup, PostgreSQL TLS and backup/restore, Unity headless startup and UDP 7770, FishNet/DTLS, server registration/heartbeat, tickets/authentication, match creation and rollback. The optional FishNet DTLS probe and Unity gameplay harness are unavailable, so those integration checks and a real 20-machine match remain deferred.

Bash syntax was not parsed locally: Git Bash had returned E_ACCESSDENIED and was not retried; Docker Desktop image inventory returned permission denied. No shell script was executed. The bundle includes a bash -n preflight to run after extraction and before host changes. Local static validation is therefore partial, while the package and Compose validation passed.

**Status:** local preparation complete; Stage 3A pending OVH review; Stage 3B runtime NOT STARTED. Do not deploy until the VPS endpoint is independently verified and the owner explicitly authorizes Stage 3B.

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

- Read the current checkout page and the existing order tracker for order #23370514. The tracker still says “Checking your order”; the latest history event says more processing is required (usually around 48 hours), and the preceding event says “Your payment is being validated.” The tracker does **not** confirm a successful/cleared payment or an active VPS. “Your order has been processed” is the order workflow step, not proof of payment clearance.
- The checkout page displayed an HTTP 400 message that the selected UK region was out of stock. No payment was submitted. The checkout error does not establish that the payment succeeded.
- **Setup gate:** Do not begin VPS configuration yet. No confirmed active endpoint or public IP is available, so SSH remains **NOT TESTED**. Resume Stage 3A after OVH confirms payment and provisions the service; then verify the service details and SSH fingerprint before host changes. Stage 3B remains **NOT STARTED**.