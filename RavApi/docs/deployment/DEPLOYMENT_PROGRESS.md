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
- **Setup gate:** Do not begin VPS configuration yet. No confirmed active endpoint or public IP is available, so SSH remains **NOT TESTED**. Resume Stage 3A after OVH confirms payment and provisions the service; then verify the service details and SSH fingerprint before host changes. Stage 3B remains **NOT STARTED**.## 2026-11-24 — Provider switch checkpoint: Hetzner CX33

OVHcloud order #23370514 was canceled per the owner; its $14.50 refund remains separate. Hetzner is the new VPS candidate for RAV staging. The official Hetzner public page currently marks CX33 unavailable and lists its eu-central locations as NBG1 (Germany) and HEL1 (Finland). The Console account-specific availability is not verified yet.

CX33 is 4 shared x86 vCPU, 8 GB RAM and 80 GB NVMe. Current published price is €8.49/month or €0.0136/hour excluding VAT. A required public Primary IPv4 adds €0.50/month or €0.0008/hour, for €8.99/month before VAT. At Germany’s 19% VAT, the illustrative total is €10.70/month; final tax depends on the account’s truthful billing profile. The Helsinki price reduction announced for November 2026 covers CPX and CCX, not CX.

The owner completed the browser security check and the Hetzner individual contact form was submitted with Ukrainian profile details. EUR is selected; Hetzner says account currency is irreversible after registration. The account setup page offers English and German only. Credit card is the only payment method currently displayed. No card details or top-up were entered, no final account completion/payment was made, and no VPS was created.

Hetzner may request ID verification or credit-card prepayment. Its published refund rule says unused verification credit is generally refunded only when the account is fully closed and all invoices are settled; top-ups have no invoice and cannot unlock unavailable capacity. CX23 is also shown unavailable. No x86 option within budget is confirmed available; do not substitute a more expensive plan without the owner’s decision.

**Current gate:** owner must complete payment-method/account setup personally. Afterward, recheck CX33 availability in Germany and Finland, VAT-inclusive checkout price, IPv4 and billing. Stage 3B runtime remains NOT STARTED; no paid resource, production, or database operation occurred.
## 2026-11-24 — Hetzner Console follow-up

The owner can access Hetzner Console; the Default project shows 0 resources. The account has no invoices or current transactions and the credit balance is USD $0.00. A pending card authorization was not checked. The Console’s 2FA message is a recommendation, not a block. The account/mail page is a contact-settings reminder to keep an independent email reachable, not a rejection notice.

Account billing is currently in USD, although the signup form previously showed EUR selected. Hetzner states account currency is fixed after registration; do not create a duplicate account or attempt a currency change without the owner’s decision. The Ukrainian profile is not the apparent blocker.

The unsubmitted server wizard confirms Cost-Optimized x86 plans are disabled for this account in HEL1, NBG1, and FSN1, including CX33. Regular Performance x86 plans are selectable: HEL1 CPX02 / CPX12 / CPX22 / CPX32 at $5.99 / $10.99 / $19.49 / $35.49 per month; Germany FSN1/NBG1 at $7.49 / $13.49 / $22.99 / $41.99. These are before VAT and public IPv4; the account UI lists IPv4 as $0.60/month. CPX12 is under-resourced for the planned stack; CPX32 matches 4 vCPU / 8 GB RAM but exceeds budget substantially. No server was created and Create & Buy now was not pressed.

**Current gate:** owner direction is needed on the fixed USD currency and whether to wait for CX availability or consider the costlier CPX32. No payment, provisioning, or Stage 3B runtime was started.


## 2026-11-25 — Altare free hosting feasibility review

### Account and platform
- The authenticated Default team is on Always Free. Observed quotas: 4 GiB RAM, 125% CPU, 40 GiB disk, 2 app slots; usage is zero and no apps exist. The 50-Bolt balance was left untouched.
- Workstations beta is full (30 seats; 2,408 on the waitlist). The documented beta allowance is up to two workstations, 2 shared vCPU, 4 GB RAM and 60 GB disk, in London only; this account has no beta seat.
- Workstations documentation supports raw TCP/UDP ports. An external port is assigned from 46000–47999 and forwarded to a selected internal port on a workstation hostname. The endpoint/restart stability was not runtime-verified.
- Altare Docker Apps support Linux/amd64 images but expose TCP only. GitHub App Hosting is web/API/worker oriented. Neither provides the raw inbound UDP required for FishNet.
- Sandboxes are not full VMs and have no systemd or nested Docker; additional public ports have Bolt costs. Altare Go is limited to 12 hours and HTTPS exposure. These options do not meet the $0, persistent UDP requirement.
- The Altare status/dashboard reported a heavy-demand incident affecting deployments, sandboxes, Go and Console during this audit. No resources were created.

### RAV static compatibility
- The NetworkManager prefab sets FishyUnityTransport to UDP (m_ProtocolType: 0), port 7770, encryption enabled, and a configured maximum of 20 clients. This is configuration only; it does not establish actual capacity or successful network operation.
- StartServerButtons accepts RAV_SERVER_PORT and applies that value both to the FishNet bind port and to the port registered/advertised to the API. There is no separate external advertised-port setting. If a Workstation maps a different external UDP port to internal 7770, the current registration/client discovery path needs a separate external-port setting or a same-number mapping before it can work.
- Static server/API integration is present: RAV server ID/credential/region/address/port variables, API base URL, registration/heartbeat/unregister routes, connection-ticket flow, and API liveness/readiness routes. No live API, DB or ticket flow was tested.
- DTLS encryption remains enabled. The server certificate/private-key environment variables and client common-name/CA variables are read by the existing secure transport configuration; no DTLS handshake was run.
- The available RAVServer-20261122-1956 Linux build contains the executable, data folder, UnityPlayer.so and runtime files, but predates Unity HEAD 3a050ee43598d5ff6aea16f17cd243e677043534 from 2026-11-24. It is therefore not confirmed to match the current source and was not packaged for deployment.

### Validation and decision
- No Altare app/workstation/sandbox/Go resource was created; no Bolt, payment method, upgrade, or production service was used.
- Docker CLI is present locally, but Docker Desktop's Linux engine is not running. No image was built. Headless startup, scene loading, UDP listening, DTLS, API/database connectivity, registration/heartbeat, tickets, shutdown, and load/performance tests remain NOT TESTED. Do not claim support for 20-player matches.
- No Altare Dockerfile was added because the available Apps/GitHub path is TCP-only and the only documented free UDP path is unavailable to this account pending the Workstations beta waitlist. No project/runtime code was changed.
- Decision for the current account and strict $0 requirement: **NOT SUITABLE**. Reconsider only if the account receives Workstations access and the external/internal port behavior can be made compatible; runtime and load tests would still be required.

Official references:
- Workstations beta: https://docs.altare.gg/workstations/beta
- Workstation limits: https://docs.altare.gg/workstations/pricing
- Workstation networking: https://docs.altare.gg/workstations/networking
- Docker Apps: https://docs.altare.gg/apps/docker
- App Hosting: https://docs.altare.gg/apps
- Sandboxes: https://docs.altare.gg/sandboxes
- Altare Go: https://docs.altare.gg/cli/go
- Bolts: https://docs.altare.gg/platform/bolts
- Status incident: https://status.altare.gg/history#incident-20261120-demand-capacity


## AlienX Cloud Free Lite assessment — 2026-11-25

### Account and platform

The owner authenticated in the AlienX panel. The account has no game server or Cloud VM. One free Cloud access request was submitted and the panel confirmed ticket **#93**, status **waiting for staff**. No duplicate request, VM, payment method, paid boost, or other paid resource was created.

The panel displayed Lite as free with 1 vCPU, 2 GB RAM, 30 GB disk, 2 projects, 10 WAN ports and 10 Mbit/s. Ubuntu 24.04 LTS was listed. VM CPU architecture, actual allocation, root access and the assigned endpoint cannot be confirmed before VM access. The observed 10 Mbit/s differs from the 50 Mbps estimate in the task brief.

### Networking and policy

AlienX's official Cloud networking guide confirms a shared public IPv4, assigned WAN port ranges and direct same-number forwarding to the VM; the provider does not translate external UDP 12345 to internal 7770. Port rules support UDP independently. The provider may change assigned ports; no static public-IP or endpoint guarantee was found. SSH consumes a WAN port. Both the AlienX panel firewall and guest Linux firewall must permit traffic.

RAV FishyUnityTransport is UDP with DTLS enabled. RAV_SERVER_PORT overrides the FishNet bind and the port advertised to the API, and RAV_SERVER_ADDRESS supplies the public address. Therefore the documented same-number mapping can work by binding/registering the actual assigned UDP port; 7770 must not be assumed available. No source/gameplay change was needed. No actual port has been assigned yet, so compatibility is static only.

Published terms list Russia among blocked IP locations and do not list Ukraine. Do not bypass region restrictions. AlienX Cloud is a hobby beta on residential/shared infrastructure with no uptime/SLA guarantee; its endpoint may change. Lite provides limited capacity and bandwidth and is suitable only for cautious low-scale tests. Twenty-player performance remains unverified.

### Unity artifact and deployment preparation

- Unity HEAD: 3a050ee43598d5ff6aea16f17cd243e677043534; repository worktree has no task-specific source changes.
- A fresh Linux Dedicated Server build completed with Unity 6000.3.13f1 using the existing RAV.Editor.LocalNetworkBuild.BuildLinuxDedicatedServer pipeline. The output is D:\Programing\UnityProjects\war-of-machines-local-builds\LinuxServer-20261124-192541, created 2026-11-25 03:27 Manila time. Build configuration is Development, suitable for limited staging validation.
- Build pipeline configures Server.unity and Map.unity; ELF header confirms 64-bit little-endian x86-64. Required files were present: RAVServer.x86_64, RAVServer_Data/globalgamemanagers, RAVServer_Data/Managed/Assembly-CSharp.dll, RAVServer_Data/MonoBleedingEdge/x86_64/libmonobdwgc-2.0.so, and UnityPlayer.so.
- Unity returned a successful build result and batch-mode exit code 0. The log had nonfatal FishNet/Unity obsolete API and FishNet upgrade-helper warnings, plus a URP non-RenderGraph warning; no compilation/build failure was reported. The license log had an unavailable access-token refresh, but Unity resolved the installed Personal entitlement and completed the build.
- Packaged a separate Linux release archive at D:\Programing\UnityProjects\war-of-machines-local-builds\AlienX-RAV-Lite-Release-20261125\RAVServer-AlienX-Lite-20261125.tar.gz. It contains 324 files (246,296,261 source bytes; 110,360,879 archive bytes), excludes Burst debug information, and passed required-file, path/type and executable-mode checks. Archive SHA-256: 80216b46ac3e520bd6100543e9517ce26bba9dc81e4f88eff9414dc20c9bd030. File manifest SHA-256: 7fe431539c3cc16b3fa65adfffae1eb17439a8770b884e5562747e5a3dcfc259.
- Added the provider-specific native deployment runbook at D:\Programing\UnityProjects\rav-unity\deploy\alienx\README.md; it reuses existing systemd/Linux scripts and does not duplicate them. The older generic Stage 3B archive was not changed or treated as current for AlienX.

### Unverified / blocked

No VM exists, so Ubuntu/architecture, host fingerprint, assigned UDP port, firewall, Linux runtime dependencies, headless start, scene runtime loading, UDP listener/external reachability, DTLS handshake, CPU/RAM/bandwidth, API/PostgreSQL connectivity, registration/heartbeat/tickets, shutdown and 20-player performance are **NOT TESTED**. No active isolated HTTPS staging API endpoint was available; keep production completely out of the test path. The existing package/deploy shell scripts were not executed on a Linux host.

**Current gate:** wait for staff's response to ticket #93; do not poll repeatedly or create a VM until access and the zero-cost Lite allocation are confirmed. Then verify the actual architecture, endpoint and port before any deployment. Platform decision: **READY FOR LIMITED TESTING ONLY after access is granted**; not ready for full multiplayer testing or production.

Official references: [Cloud networking](https://www.alienxservers.co.uk/knowledgebase/cloud/how-cloud-networking-works), [port mapping](https://www.alienxservers.co.uk/knowledgebase/cloud/open-a-port), [Terms](https://www.alienxservers.co.uk/terms).


## RAV API and PostgreSQL on Altare Always Free — 2026-11-25

### Account and cost controls
- Authenticated Default team: Always Free; current account-specific pool is 4 GiB RAM, 125% CPU, 40 GiB storage and 2 app slots. The console showed zero app use. Its quotas differ from the public plan page snapshot reviewed, so the account console is the planning source.
- Managed PostgreSQL 17 is available in the console with 1 included database, 256 MiB memory, 1 GiB disk and 3-day backups; the creation form listed 25 connections at 256 MiB / 50% CPU. European choices are Frankfurt, Karlskrona and London; location is capacity-selected.
- Billing showed $0 spent/no invoice, Auto top-up Off, and no card saved; the last observed wallet balance was 50 Bolts. The storage spend cap was saved at $0 and the console confirmed it. That cap applies to storage overages only; it does not protect app usage. No resources were created and no Bolts were consumed.
- No app was created, no DB was created, and no payment or plan change occurred.

### Compatibility and blockers
- Docker Apps accept Linux/amd64 images, provide TCP ingress and platform HTTPS, support encrypted variables, and retain the configured data folder across image redeploys. Account limit is two app slots. This can host an HTTPS API, but it does not establish private connectivity to Managed PostgreSQL.
- The managed DB documentation advertises TLS and an individual db.altare.global address; the UI offers “Connect an app,” but neither the reviewed docs nor creation form established private-only managed DB ingress or an allowlist. Do not assume an internet-facing endpoint meets the no-public-PostgreSQL requirement. No managed DB was provisioned.
- The official docs describe private networking for apps and a PostgreSQL database run in an app. A self-hosted Docker DB could be considered only after verifying app-to-app routing, TLS certificate setup, persistence across restarts/redeploys, resource allocation and zero-Bolt overage behavior. The image check for postgres:17-alpine returned a 112 MB multi-platform image exposing 5432; the setup wizard was closed and its draft discarded.
- Existing RAV API Release build passed locally: dotnet build RavApi/rav-api.sln --configuration Release --no-restore, 0 warnings/errors. Existing Dockerfile uses .NET 10, HTTP 8080, non-root execution and keeps migrations/seeding disabled. Docker CLI is present, but the Docker Desktop Linux Engine was not running, so Docker build/runtime validation did not happen.
- To use Altare's persistent app data folder, the API Data Protection key directory must be overridden into that folder. The API accepts only configured ForwardedHeaders:KnownProxies; the reviewed provider docs did not supply a stable proxy IP/CIDR, so do not relax proxy trust. No private API image/source was uploaded or linked.
- The provider status page reported an active heavy-demand incident affecting app deployments and the Console. No SLA, app idle-shutdown/expiry policy or numerical HTTP request allowance was established by the docs reviewed. A 2-second heartbeat implies about 1,296,000 requests per 30 days; load and request capacity remain untested.

### Result
**BLOCKED — deployment not started.** Required gates are private-only database connectivity (or a validated private app-to-app DB design), trusted HTTPS proxy range, verified zero-Bolt app overage stop, working local Docker Engine for image validation, and owner approval before private source/image transfer. No API URL, migrations, health/auth tests, database backup/restore, persistence test or resource measurements exist. Do not mark the service ready for AlienX integration.
Official references: [Altare PostgreSQL](https://docs.altare.gg/databases/postgres), [Docker Apps](https://docs.altare.gg/apps/docker), [networking](https://docs.altare.gg/networking), [plans](https://docs.altare.gg/platform/plans), [resource limits](https://docs.altare.gg/platform/resources), [Bolts and overages](https://docs.altare.gg/platform/bolts), [service status](https://status.altare.gg/history#incident-20261120-demand-capacity).

## 2026-11-25 — Private GHCR staging image preparation

- The existing Altare managed database `rav-staging` is present and Running. The current Console shows one included PostgreSQL allocation at 256 MB / 256 MB; the Apps page shows no app resources used (0/4 GiB memory, 0/125% CPU, 0/40 GiB disk, 0/2 app slots) and 50 Bolts. No second database, Docker App, paid resource or Bolt use was created in this checkpoint.
- The API Release solution builds successfully after staging hardening: 0 warnings and 0 errors.
- Built Linux/amd64 runtime image `rav-api:staging-20261125` from the current repository Dockerfile. Image ID: `sha256:a09c756fc6908192c19b9b669fbde5a33f715ff48fb2621fd76d461b7c26fc16`; size 397,904,758 bytes; runtime UID 1654; port 8080.
- Runtime image audit inspected 117 files and found no `.env` files, private keys/certificates, source/project/solution files, PDB symbols, PEM certificate markers or Development configuration. The Production entrypoint fails closed when the staging Data Protection certificate secret is absent. Entrypoint shell syntax passed.
- Staging-only hardening added TLS mode/root-certificate overrides for managed PostgreSQL, including `VerifyFull`; development fixture account `testuser/test123` and demo match are gated off by default; container startup expects the Data Protection PFX and password as encrypted runtime values, and accepts the public database CA as a runtime value, writing all material only to restricted temporary files. The Data Protection key ring uses Altare's persistent `/data` folder.
- Exact destination remains `ghcr.io/scorpioner2010/rav-api:staging-20261125`. Remote push and digest verification are pending entry of the owner's classic `write:packages` token in the masked terminal. Do not record the local image ID as a registry digest.
- Docker App creation, PULL_TOKEN entry, DB attachment, migrations, public HTTPS/API tests, local Unity-to-cloud registration and client configuration have not yet occurred. Keep them pending until the private image is available. No GitHub App was installed and no source repository was connected to Altare.

## 2026-11-25 — Local isolated staging and Unity integration verification

This checkpoint supersedes earlier entries that said Docker/runtime checks had not been run. Those were accurate at their timestamps; the following local-only tests were completed afterward.

- Compose static validation passed with the isolated staging environment. The API runtime and migration Docker targets built successfully. The migration target now restores Microsoft.EntityFrameworkCore.Design before EF tooling runs.
- PostgreSQL initialization script was normalized to LF after a Windows bind-mount CRLF caused the first initialization attempt to report a shell here-document parse error. The isolated database recovered; no existing volume or production data was removed. EF migrations and staging seed completed.
- API and PostgreSQL containers became healthy. PostgreSQL client connection with TLS verify-full succeeded. No API or PostgreSQL host port was published; staging services used a uniquely named internal-only Docker network and volumes.
- A fresh Unity Linux Dedicated Server Development build succeeded. Artifact path: D:\Programing\UnityProjects\war-of-machines-local-builds\LinuxServer-20261124-203532. Archive: D:\Programing\UnityProjects\war-of-machines-local-builds\AlienX-RAV-Lite-Release-20261125-v2\RAVServer-AlienX-Lite-20261125-v2.tar.gz. SHA-256: 939ac7e6ef5134605f677760e3ad309e05ea7414d18ab546b0c4190efb95abd6; 110,793,355 bytes, 333 entries, executable 0755, required files present, no detected secrets or unsafe paths.
- In isolated Ubuntu 24.04 Docker runtime, Unity headless startup, Server scene, FishNet UDP 7770 inside the container, API registration and heartbeat passed. The host UDP mapping was only 127.0.0.1:7770/udp. Graceful stop exited 0 and an HTTPS registry query confirmed unregister.
- API smoke passed HTTPS certificate validation, health/live and health/ready, PostgreSQL-backed player registration/login/profile/starter vehicle, two connection-ticket issues/redemptions, replay rejection, and creation of a two-player backend match record.
- Source-only staging hardening: an explicit RAV_API_BASE is now authoritative; invalid/disallowed explicit endpoints no longer fall through to the legacy production URL. Runtime diagnostics accept RAV_GAMEPLAY_DIAGNOSTICS_PATH, and the Linux unit points it to /var/log/rav. No production request or credential was used.
- Non-blocking API runtime log warning: optional libgssapi_krb5.so.2 is absent; API health and TLS/SCRAM database connection passed.

Still NOT TESTED: AlienX VM deployment/Ubuntu systemd/firewall; Altare cloud API or managed PostgreSQL; public UDP; an actual Unity client/FishNet DTLS handshake; runtime Map load; in-game matchmaking/vehicle and bot spawning; saved match results/profile update/reconnect; PostgreSQL backup/restore; and 20-player performance. No cloud resource was created, no money or Bolts spent, and no production service/data was used. Private source/image transfer to Altare and any non-reversible cloud deployment action require the account owner to explicitly authorize that concrete operation.

## 2026-11-25 — Altare Docker App variables and verified private image

- Altare successfully checked private image `ghcr.io/scorpioner2010/rav-api:staging-20261125`; reported Linux/amd64 multi-platform image, 106 MB registry size, UID 1654, TCP port 8080. Registry digest: `sha256:a09c756fc6908192c19b9b669fbde5a33f715ff48fb2621fd76d461b7c26fc16`. Image remains private; no GitHub App/source access was granted.
- The image's own entrypoint is `/app/entrypoint.sh`; leave Altare Start command blank so the image ENTRYPOINT runs. Set the Docker App port to TCP `8080`. Keep Data folder `/data`; the image persists the Data Protection key ring at `/data/protection`. Altare documents that this folder survives image redeploys. Do not set `DataProtection__CertificatePath` manually; the entrypoint creates it from the PFX variable.
- Required/runtime configuration for the Altare app (values must be entered through encrypted Altare Variables, not stored in Git):
  - `DATABASE_URL`: attach the existing `rav-staging` from its Postgres Overview → Connect to an app. Altare supplies this secret connection string and also `PGHOST`, `PGPORT`, `PGUSER`, `PGPASSWORD`, `PGDATABASE`, and `PGSSLMODE`; the API reads `DATABASE_URL`, so do not copy credentials manually.
  - `Database__SslMode=VerifyFull`: use certificate-verified PostgreSQL TLS.
  - `RAV_DATABASE_ROOT_CA_BASE64`: base64 of the public CA certificate downloaded from the `rav-staging` Overview. The entrypoint writes it to a restricted temp file and sets `Database__RootCertificate`.
  - `Jwt__Key`: JWT signing secret from the current-user DPAPI-protected local bundle. It exceeds the code's 32-character minimum. Never place its value in documentation or chat.
  - `Jwt__Issuer=WarOfMachines.Api` and `Jwt__Audience=WarOfMachines.Client`: existing compatibility identifiers; they are nonsecret and already have these defaults, but can be explicitly set in Altare.
  - `AllowedHosts`: the exact Altare app hostname only (no scheme or path); must be filled after the app name/hostname is known because `appsettings.Production.json` still contains the former Render host.
  - `RAV_DP_CERT_PFX_BASE64` and `DataProtection__CertificatePassword`: required Production-only secrets from the DPAPI-protected local bundle. The entrypoint writes the PFX to a restricted temp file and exports `DataProtection__CertificatePath` itself.
  - Keep `Database__ApplyMigrationsOnStartup=false`, `Database__RunSeedOnStartup=false`, and `Database__SeedDevelopmentFixtures=false` during initial deployment preparation. Do not enable migrations until a fresh backup and existing schema/data have been reviewed: migration `20260512084921_AddVehicleProjectileStats` drops `Vehicles.Damage` before adding the replacement stat fields. Normal catalog seeding may be enabled only after migrations have been safely applied; development fixture seeding stays false.
  - For later Dedicated Server registration only: `DedicatedServers__Provisioned__0__ServerId` (nonsecret) and `DedicatedServers__Provisioned__0__Credential` (secret) come from the protected bundle and must match the local Dedicated Server configuration. Omit them until that server is ready to register.
- Image defaults already set in the Dockerfile; no need to duplicate them in Altare Variables: `ASPNETCORE_ENVIRONMENT=Production`, `ASPNETCORE_URLS=http://+:8080`, `ASPNETCORE_HTTP_PORTS=8080`, `DataProtection__KeyDirectory=/data/protection`, migrations/seeding off, and `Swagger__Enabled=false`. Do not configure internal HTTPS; Altare terminates external HTTPS and the app speaks HTTP on TCP 8080.
- Do not set `ConnectionStrings__DefaultConnection`, `Database__RootCertificate`, or `DataProtection__CertificatePath` in Altare: the attached `DATABASE_URL` and entrypoint populate the needed paths. Do not set `RAV_API_BASE` on the API; it belongs in the local Unity client/server configuration. `Cors__AllowedOrigins` is unnecessary for the native Unity client; set it only if a browser client needs CORS. Do not set `ForwardedHeaders__KnownProxies__0` until Altare provides a verified trusted proxy address.
- Altare's current Docker Apps documentation limits app variables to 200 entries and 32 KB total; the generated PFX base64 is 3.2 KB and the generated local bundle contains no registry token. Store the public DB CA value and the other runtime settings in the encrypted Altare Variables interface. Local generated secrets are outside both repositories at `C:\Users\TorasPC\AppData\Local\RAV\AltareStagingSecrets\Generated-20261124T230116Z-e320b14c\staging-secrets.dpapi.txt`; the file is protected with current-user Windows DPAPI and an explicit user-only ACL.
- The local Docker Compose staging config still uses mounted file secrets and older `/var/lib/rav-staging` paths, while the Altare image entrypoint expects PFX/CA base64 variables and `/data/protection`. Do not copy that Compose environment verbatim to Altare. It is not used by the currently verified remote image; reconcile it before any later local Compose run.
- Current step remains **preparation only**: no Altare Docker App has been created, the database has not been attached to this app, and cloud migrations/health/auth/ticket/server-registration/Unity tests have not run. Select Frankfurt when creating the app; the wizard previously defaulted to Stockholm. Recheck resource/price/quota display before creating.
- Official platform references: [Altare Docker Apps](https://docs.altare.gg/apps/docker) and [Altare Postgres](https://docs.altare.gg/databases/postgres).
- A user-local helper `Copy-AltareStagingValue.ps1` was added beside the DPAPI bundle (outside Git). It copies one selected variable to the clipboard without printing it, then clears the clipboard after the owner presses Enter. It can also base64-encode the public PostgreSQL CA certificate from a chosen local file.

## 2026-11-25 — Final deployment status and credential recovery

- Altare Console current read-only snapshot: rav-staging is Running (PostgreSQL 17, 256 MB RAM, 1 GB disk, 25 connections, 7.34 MB data). UI exposes host rav-staging-g2iw.db.altare.global, port 25121, database/user rav_staging; UI region is only stated as Europe, not a city. The database currently reports zero connections and no connected apps.
- Altare Home shows 0 apps, the Always Free pool unused (4 GiB RAM, 125% CPU, 40 GiB disk, 2 slots), 50 Bolts, and an active heavy-demand incident; Frankfurt node was shown as in outage. The API image setup wizard still has the verified private GHCR image and PULL credentials, port 8080, /data, blank Start command, but default region is Stockholm. No app was created.
- Security incident: a database password appeared in browser-derived tool output due a separate label/value line escaping the sanitizer. The value is not recorded here, repeated or stored. Treat the old password as compromised; owner password reset is required before connecting the DB to an app. Manual browser takeover requested; wait for owner to return.
- Desktop token source was inspected without printing values. Existing Git Credential Manager supplied a GitHub credential; GH CLI is not installed. Existing GHCR push/pull tokens were copied into a verified current-user DPAPI vault together with generated API staging secrets. The source Desktop/token.txt remains because the automatic delete operation was rejected; do not retry/evade the policy. User must remove the plaintext file manually after verifying access to the vault.
- Created C:\Users\TorasPC\Desktop\War of Machines Deployment with all requested documents, backups, logs, artifacts, scripts, and secure-vault. Central vault round-trip passed; ACL allows only the current account. The encrypted vault intentionally omits the compromised DB password.
- Copied Unity archive to the Desktop artifacts folder; SHA-256 verified as 939ac7e6ef5134605f677760e3ad309e05ea7414d18ab546b0c4190efb95abd6 (110,793,355 bytes).
- Added nine PowerShell automation/credential helpers under Desktop scripts. PowerShell parser reports syntax OK for all nine. Scripts do not print secret values; UDP probe reports inconclusive states without representing them as DTLS proof.
- Docker Desktop Engine is available locally (29.8.2). Local image rav-api:staging-20261125 is present, non-root UID 1654 and TCP 8080; remote digest matches the recorded GHCR digest. No rebuild was needed.
- Current cloud API, migration, health/auth/ticket, public UDP, DTLS, heartbeat, server registration, Unity client and performance checks remain NOT TESTED. AlienX current access was not inspected in this checkpoint because its authenticated tab is not present and browser work is paused for the owner credential reset.
- Next: Owner resets the staging database password and signals completion. Import it by copying the new password to clipboard and running the local helper; then make one current provider check, back up, review the destructive migration, deploy the free Altare app in the DB's available Europe region if possible, and proceed with staging checks. Do not use the old database password.

## 2026-11-25 — Continued Altare staging after owner deferred password rotation
- Owner instruction: defer rav-staging credential rotation until after test deployment and integration checks. The DB is test-only. Do not reset now; use Altare's supported Connect to an app injection path where possible. Do not echo or document any credential.
- Current Altare Postgres list was readable: rav-staging is Running, PostgreSQL 17, Europe, 256 MB RAM, 1 GB disk, about 7.34 MB; the Console showed 50 Bolts. No app or Bolt was created/used during this continuation.
- Clicked Back up now once. Altare returned “Backup started.” A Nightly 1.05 KB Done row was visible, but the new manual backup was not observed in a completed state. Backup readiness remains NOT VERIFIED; no migration/seed ran.
- Altare Settings, Tables and Apps/New routes stayed on loading skeletons while the page displayed the existing high-demand incident notice. No API app, endpoint, database attachment, or cloud test was created.
- Reviewed AddVehicleProjectileStats source: it drops Vehicles.Damage and adds DamageMin/DamageMax/ShellSpeed/ShellsCount. Before applying, inspect current tables, applied migration history and data; preserve user data.
- Official free x86 candidate research: Oracle Always Free AMD micro is 1/8 OCPU, 1 GB RAM, 50 Mbps and includes one public IP; Ubuntu supported. OCI may reclaim idle resources and signup requires valid card details for verification, with a temporary authorization hold possible. No account, card or resource was created. Candidate is not validated for RAV runtime or 20-player use.
- **Next:** Once Altare Console data routes are available, verify the requested backup, schema, migration history and zero-cost App options; create and attach only the authorized free App, then continue HTTPS/API tests. Check AlienX approval and actual UDP hosting separately. Keep Unity Cloud config unchanged until real API and UDP endpoints exist.

## 2026-11-25 — Altare private image verified; API App request pending

- Owner instruction remains to defer the test database password rotation until after staging configuration and testing. No reset was performed. The protected clipboard helper was resumed after the owner pasted the GHCR pull token into Altare; the helper confirmed the OS clipboard was cleared. The token value is not recorded.
- Altare accepted the private registry login and verified ghcr.io/scorpioner2010/rav-api:staging-20261125: digest prefix a09c756fc690 (matches recorded sha256:a09c756fc6908192c19b9b669fbde5a33f715ff48fb2621fd76d461b7c26fc16), 106 MB, Linux/amd64, TCP 8080, runtime UID 1654.
- Frankfurt, Karlskrona, and Helsinki were unavailable for new Apps. Stockholm was the available standard European region; London Plus was selectable but not chosen. Created the authorized free rav-api-staging app request for Stockholm with 1 GiB RAM, 50% CPU, 2 GiB disk, port 8080, Data folder /data, image command/entrypoint unchanged, manual redeploy, and encrypted private-registry login. This reserves only a portion of the team's free pool and no Bolt usage was indicated.
- The create operation has not yet returned a success page or app identifier; UI still shows Creating… under Altare's heavy-demand incident. Do not click Create again. App existence, endpoint, current allocation, and Bolt usage remain unverified until the Console responds.
- Docker app environment variables have not been entered yet. Expected next steps after the app exists: attach the existing rav-staging through Altare's Connect to an app flow (inject DATABASE_URL), set the verified TLS root CA and existing staging-only JWT/Data Protection secrets using the protected vault and encrypted Variables UI, set AllowedHosts to the assigned app hostname, keep migration/seeding switches false until backup completion and schema/migration-history review, then run migrations once against this staging database only.
- The Postgres Tables view showed “No tables yet.” This does not prove migration history or backup completion. One on-demand backup returned “Backup started”; its completion was not observed. No migration, seed, API health/auth/ticket test, server registration, heartbeat, public UDP/DTLS, Unity Cloud config, or production change occurred.
- Local code review confirms DatabaseConnectionStringFactory prefers Altare-injected DATABASE_URL; Dockerfile defaults ASPNETCORE_ENVIRONMENT=Production, ASPNETCORE_URLS=http://+:8080, migrations/seeding off, and /data/protection. Entrypoint /app/entrypoint.sh is the correct image command and writes the encrypted Data Protection certificate and optional PostgreSQL CA into restricted temp files. No manual Start command is needed.
- No API rebuild was done. Frankfurt outage/Altare high demand still limits progress. Continue from the pending create request without duplicating it.

## 2026-11-25 — Free Altare app placement blocked

- The first automatic placement attempt returned “No allocations satisfying the requirements for automatic deployment were found.” Explicitly pinning the app to two different Stockholm nodes, each marked Fits for 1 GiB memory / 2 GiB disk, returned “The node has no free ports.” No app was created. The visible Console remained under the heavy-demand incident.
- Other standard listed regions showed No capacity. London Plus and Singapore Plus were not selected because the user prohibited paid resources/Bolt usage. The app wizard and Altare database remain separate: no app attachment or database variables were configured.
- The unsaved private-registry token was cleared from the app wizard after the failed attempts; the protected DPAPI vault was not changed. The owner had pasted the token directly; the protected helper cleared the system clipboard.
- A second read-only visit to the Postgres overview exposed a credential in the page's accessibility text; it was not copied to repository/Desktop documents, repeated in this entry, or used. Per owner instruction the test database password rotation remains deferred until staging setup/tests. Continue avoiding raw database overview accessibility output.
- The Backups route still showed Loading on two observations, so the on-demand backup remains unverified. The Tables page previously showed No tables yet, but migration history was not inspected. No EF migration, seed, API endpoint, health/auth/ticket test, Unity config, UDP/DTLS, or production change occurred.
- Current blocker: Altare has no successful free TCP port allocation for this app in the available standard European region. Stop placement retries unless Altare availability materially changes. Continue independent checks only; no Plus region, Bolts, or paid resource.

## Netcup VPS live staging — 2026-11-26 (current checkpoint; supersedes historical Altare pending status)

- Netcup VPS is active in Nuremberg: Ubuntu 24.04.5 LTS x86-64, 4 vCPU, 8 GiB RAM, 160 GiB disk, IPv4 62.83.120.40.
- `ravadmin` SSH key-only access, sudo, UFW, Docker Engine/Compose, Docker forwarding guard, systemd and staging monitor timer are configured.
- Fresh PostgreSQL 17 staging volume was created and migrated. No legacy users/profiles/progression were imported; development fixture seeding is off.
- Latest DB state: Players=0, Matches=0, UserVehicles=0, tickets=0, sessions=0; Vehicles=21, Factions=2, VehicleResearchRequirements=19, Maps=2, DedicatedServers=1 (the real staging server).
- API/Caddy HTTPS: `https://api.staging.62-83-120-40.sslip.io`; `/health/live` and `/health/ready` return 200. PostgreSQL and API ports are not published on the host.
- Smoke test passed PostgreSQL verified TLS, server API register/heartbeat/status, temporary account register/login/JWT/profile/starter values, tickets/replay rejection, and backend match creation. The generated test account/match and related rows were removed and zero Players/Matches rechecked.
- Unity x86-64 dedicated server currently runs in scene `Server`, Fishy UnityTransport listens at 0.0.0.0:7770, and API status records an actual Unity heartbeat (`Unity server running`). A uniquely marked external UDP datagram from the Windows host was captured on the VPS NIC. This is not a DTLS handshake.
- A staging PostgreSQL dump was restored successfully into an isolated PostgreSQL 17 container using `--network none` and tmpfs; restored state had 13 public tables, Players=0, Matches=0, Vehicles=21, Factions=2.
- Current Linux bundle: `deploy/staging/artifacts/RAV-Staging-3B-20261125T194757Z.tar.gz`; SHA-256 `d2b30525f7e940bf9aa887e1d3177613ffcef7e9a5e62b3fd520ce469ae595ee`.
- Pending after final checks: compatible FishNet DTLS handshake; real client connection/live ticket use; Map/match/vehicle/bot/physics tests; 20-slot load. Stability and reboot recovery passed.

No production endpoint/database was used or changed. No old database was deleted or modified. No extra paid service was purchased.

## Netcup final deployment checkpoint — 2026-11-26

- 30-minute passive observation passed (2026-11-25 19:44:45–20:14:50 UTC): sampled HTTPS live/ready, UDP socket, and systemd health remained good; Dedicated Server NRestarts=0. Idle sample was about 195 MiB RSS / 2.3% CPU; this is not a load test.
- Controlled VPS reboot recovery passed: strict key-only SSH reconnected on attempt 2; staging Compose and Dedicated Server units enabled/active; PostgreSQL, API, Caddy healthy; UDP 7770 listening.
- Post-reboot public /health/live, /health/ready, and /unity-server/status returned HTTP 200; server was ready and heartbeat fresh at 2026-11-25T20:17:55Z.
- Fresh PostgreSQL 17 staging database: 9 EF migrations applied; system catalog only. Clean state after test cleanup verified with zero Players, Matches, UserVehicles, ConnectionTickets, and UserSessions; catalog counts Vehicles=21, Factions=2, research requirements=19, Maps=2, plus one real DedicatedServers registry.
- PostgreSQL backup/restore passed in isolated disposable PG17; API/auth/starter-profile/ticket/replay/backend match smoke passed and disposable user/match were cleaned up.
- Deployment status: COMPLETED for infrastructure/bootstrap and staging API/DB/Unity service. Not equivalent to full gameplay acceptance.
- Remaining NOT VERIFIED: FishNet DTLS client handshake, live Unity client/ticket connection, Map load through live match, vehicles/bots/physics/match completion, and performance at 20 slots.
