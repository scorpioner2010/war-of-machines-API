# План розгортання RAV

Оновлено: 2026-11-22
Поточний етап: Stage 2 — COMPLETED: LOCAL PREPARATION; Stage 3A — IN PROGRESS; Stage 3B не починався.
Статус: SSH egress preflight passed; account registration pending; no VPS order/payment; actual VPS access and specs NOT VERIFIED.
Примітка: перші розділи нижче зберігають dated Stage 1 audit snapshots; остаточні Stage 2/Stage 3 межі наведені в closeout наприкінці.

## Висновок

Ubuntu 24.04 LTS підходить як ціль для Unity Linux Dedicated Server: Unity 6 перелічує Ubuntu 22.04 і 24.04 як серверні платформи без вимоги GPU. Але поточна машина Unity 6000.3.13f1 не має встановленого Linux Dedicated Server Build Support, а в проєкті є лише Windows build profiles і Windows-only локальний build script. Код не містить знайдених Windows-only runtime API чи native plugin, тому явного кодового блокера Linux не виявлено. Фактичний Linux build і запуск ще не перевірені.

Один VPS-3 архітектурно може вмістити API, PostgreSQL, reverse proxy та один Unity Dedicated Server. Це придатна ціль для початкового закритого тесту в одному регіоні після проходження Linux build, security і load gates. Продуктивність 20-ботового матчу, паралельних матчів, відмовостійкість і production database стан не підтверджені. До навантажувального тесту ліміт — один активний матч на серверний процес.

Є три основні блокери перед публічним deployment:

1. Linux build support/profile/script відсутні в поточній Unity-інсталяції/проєкті.
2. API ще цілиться в .NET 8; підтримка .NET 8 закінчилася 2026-11-10, а Dockerfile також використовує .NET 8.
3. Production host allowlist у Unity та ASP.NET конфігурації досі вказує на Render; новий HTTPS host не прийматиметься поточним production build.

## Поточний стан — перевірені факти

### Unity Client і Dedicated Server

- Проєкт використовує Unity 6000.3.13f1, FishNet 4.6.4, Dedicated Server package 2.0.2, FishyUnityTransport з pinned commit 08756d9733a556018041e52fcb9d2a6035346aeb і Unity Transport 2.7.2.
- Встановлено Windows Standalone support; директорії LinuxStandaloneSupport і Linux server Mono/IL2CPP варіантів у цій Unity-інсталяції немає. У проєкті є Windows та Windows Server build profiles, Linux profile відсутній.
- Windows Server profile включає Server.unity і Map.unity, використовує Dedicated Server subtarget; scripting backend для Server налаштований на Mono. Assets/Editor/LocalNetworkBuild.cs створює лише Windows 64-bit Development builds з .exe іменами.
- Статичний пошук у власному runtime/editor коді не знайшов DllImport, Win32/Windows Forms/Registry API або нативних .so/.dylib плагінів. Знайдені DLL — керовані бібліотеки/редакторські analyzers. Server scene все ще містить camera/render компоненти; тільки Linux Dedicated Server build покаже фактичне visual stripping і сумісність сцени.
- У NetworkManager.prefab підключені ConnectionTicketAuthenticator і SecureFishNetTransportConfiguration. Encryption увімкнено. Конфігурація бере server certificate/private key та client CA/common name з environment/file paths і fail-closed при відсутніх DTLS параметрах.
- API transport і DTLS — окремі TLS контури: API має працювати через HTTPS за reverse proxy; FishyUnityTransport шифрує і автентифікує UDP-сесії через DTLS. Для self-signed DTLS server certificate клієнтам потрібен довірений CA; без CA транспорт перевіряє системне сховище сертифікатів.
- Server scene автоматично запускає Server mode. Порт за замовчуванням — UDP 7770; його та advertised address можна задати через runtime environment/command-line налаштування. Heartbeat відправляється приблизно раз на 2 секунди, API позначає сервер неактивним після 15 секунд без heartbeat.
- FishNet TimeManager має default 30 Hz; у сценах не знайдено іншого serialized tick rate. Діагностика сервера збирає останній tick, p95/max за 10 секунд, physics/AI/visibility час, кількість bot/entity/projectile, Unity allocated memory і GC collection count. Поле GC allocated bytes/sec зараз явно встановлюється в null; процесний RSS треба вимірювати окремо через Linux/cgroup monitoring.
- Немає Linux build чи runtime evidence. Попередній запис про один гравець плюс 19 ботів у локальному тесті не є доказом поточної Linux продуктивності.

### Паралельні матчі

Один Unity Dedicated Server process архітектурно підтримує кілька матчів: LobbyRooms зберігає процесний room registry, кожен ServerRoom має власний match state, а GameplaySpawner завантажує map scenes additive і призначає scene handle/slot/offset. MatchBotPopulationService заповнює кімнату до maxPlayersForFindRoom=20 ботами. Отже, для кожного матчу окремий процес не потрібен.

Усі кімнати залишаються в одному process, мережевому стеку, CPU та спільному physics/AI бюджеті. Кількість паралельних матчів не встановлюється лише архітектурним фактом; її потрібно визначити load test-ом. Перший deployment має рекламувати один матч, доки два одночасні 20-slot матчі не пройдуть тест.

### Backend і база

- WarOfMachinesAPI.csproj цілиться в net8.0. Dockerfile використовує SDK/runtime 8.0 та ASPNETCORE_URLS з Render-style PORT.
- На локальній машині доступні SDK .NET 8 і .NET 10. API build з --no-restore завершився успішно: 0 warnings, 0 errors.
- Поточний Production config вимикає automatic migrations, seed і Swagger. App запускається лише з зовнішнім JWT key довжиною щонайменше 32 символи та налаштованими issuer/audience.
- JWT підпис, issuer, audience і lifetime перевіряються; кожен bearer session звіряється з активним PostgreSQL UserSession. Refresh tokens обертаються, logout відкликає session. Dedicated Server використовує окрему authentication scheme й credential hash. Connection tickets зберігаються як hash, прив'язані до user/session/server, короткоживучі й одноразові.
- Dedicated Server register/heartbeat/unregister, match start/end та ticket redemption вимагають Dedicated Server policy. GET /unity-server/status навмисно лишається anonymous для server discovery.
- У поточному Unity HTTP коді не знайдено TLS certificate bypass. CORS обмежений порожнім allowlist-ом; Unity клієнту CORS не потрібен.
- Production AllowedHosts зараз дорівнює war-of-machines-api.onrender.com; ForwardedHeaders:KnownProxies порожній. Program.cs довіряє лише явно заданим proxy IP. У коді немає dedicated health/readiness endpoint і немає UseHttpsRedirection; публічний TLS має завершувати reverse proxy, а API port слід закрити від прямого Internet ingress.
- Клас InMemoryLoggerProvider існує, але не реєструється. ASP.NET стандартний logger пише в console; /admin/logs показує обмежений in-memory ring buffer (до 2000 подій), який зникає при restart. OpenTelemetry/tracing/зовнішнього metrics sink не знайдено.
- Локальний Compose містить лише PostgreSQL 16, прив'язаний до 127.0.0.1:5433. Production Compose/reverse proxy конфігурації немає.

### Міграція і production endpoint

Міграція 20261119085129_SecurityHardeningFoundation присутня. Вона додає UserSessions, ConnectionTickets, DedicatedServers, match ownership/version та індекси, і змінює participant index. Її Down() видаляє нові security таблиці та колонки, тому production rollback не можна робити сліпим застосуванням Down().

Історичний API TASK.md підтверджує, що міграцію застосовували до ізольованої локальної PostgreSQL 2026-11-19; наступний локальний startup у записі від 2026-11-20 пройшов з наступною migration. На момент цього аудиту Docker daemon зупинений, 127.0.0.1:5433 недоступний, тому сучасний стан локальної БД повторно не прочитаний. Користувач підтвердив, що локальна БД була тимчасовим тестовим налаштуванням. Production migration state лишається невідомим; історичні записи вказують, що production migration не виконувалась. Не вважати production schema оновленою до окремої перевірки в наступному етапі.

Production endpoint у поточному Unity коді/configuration — https://war-of-machines-api.onrender.com. Це статична перевірка рядків і allowlist, не live availability/DNS перевірка; жодного запиту до production не надсилали.

## Рекомендована архітектура VPS

- OS: Ubuntu 24.04 LTS AMD64. Вона входить до Unity 6 server support matrix і має підтримку до 2029 року. Ubuntu 26.04 є LTS, але не входить до перевіреної Unity 6 server platform list у джерелі цього аудиту.
- API + PostgreSQL: Docker Compose, pinned image tags. Оновити API до .NET 10 LTS і сумісних ASP.NET Core/EF Core/Npgsql пакетів до production. PostgreSQL volume постійний; DB не публікувати на host port і не відкривати у firewall.
- Reverse proxy: Caddy або Nginx на зовнішньому HTTPS host. Варіант із Caddy у Compose має використовувати сталий private IP для ForwardedHeaders:KnownProxies, бо код не довіряє довільним X-Forwarded-For. API не має бути доступним напряму з Internet.
- Unity: Linux Dedicated Server build як окремий native process під systemd service user. Увімкнути Restart=on-failure, журналювання в journald, обмежити доступ до бінарників і credential files. На першому кроці — одна активна 20-slot match room.
- UDP: відкрити тільки фактичний game port (7770 за замовчуванням) до Dedicated Server. API доступний через TCP 443; TCP 80 потрібен для HTTP redirect/ACME challenge. SSH обмежити адміністративними IP. PostgreSQL 5432 не відкривати публічно.
- TLS: API — домен під контролем користувача та автоматичний HTTPS сертифікат reverse proxy. DTLS — окремий CA/server certificate/private key; перевірка common name та CA має залишитися увімкненою. Не копіювати ключі в Git, Unity assets, Docker image чи build artifact.
- DNS: потрібен власний домен на новому API host. Render hostname не можна перепризначити на VPS. Для старих клієнтів передбачити період сумісності або обов'язкове оновлення клієнта.
- Secrets: для Unity systemd передавати шлях до root-owned PEM files через WOM_UTP_SERVER_CERTIFICATE, WOM_UTP_SERVER_PRIVATE_KEY, WOM_UTP_CA_CERTIFICATE і WOM_UTP_SERVER_COMMON_NAME. WOM_SERVER_CREDENTIAL та API JWT/database credentials зберігати як systemd credentials або захищені host files, права лише потрібним сервісним користувачам.

## Configuration variables без значень

API:
- ASPNETCORE_ENVIRONMENT, ASPNETCORE_URLS
- ConnectionStrings__DefaultConnection або DATABASE_URL
- Jwt__Key, Jwt__Issuer, Jwt__Audience
- AllowedHosts
- ForwardedHeaders__KnownProxies__0
- Cors__AllowedOrigins__0 — тільки якщо потрібен browser origin; Unity не вимагає CORS
- Database__ApplyMigrationsOnStartup=false, Database__RunSeedOnStartup=false, Swagger__Enabled=false
- DedicatedServers__Provisioned__0__ServerId та DedicatedServers__Provisioned__0__Credential
- DedicatedServers__HeartbeatTimeoutSeconds, Logging__LogLevel__Default

Unity Dedicated Server:
- WOM_API_BASE
- WOM_SERVER_ID, WOM_SERVER_CREDENTIAL, WOM_SERVER_REGION
- WOM_SERVER_ADDRESS, WOM_SERVER_PORT
- WOM_UTP_SERVER_CERTIFICATE, WOM_UTP_SERVER_PRIVATE_KEY
- WOM_UTP_SERVER_COMMON_NAME, WOM_UTP_CA_CERTIFICATE

Production client:
- HTTPS API host має бути доданий у production allowlist. Поточного env/command-line override недостатньо: HttpLink відкидає будь-який host, крім Render host, тому потрібне узгоджене source/build change та новий клієнтський build.

## Ресурси та критерії навантаження

OVHcloud VPS-3 відповідає плану 6 vCore, 12 GB RAM, 100 GB NVMe. Публічний сайт OVHcloud на момент перевірки показував нову VPS-3 конфігурацію від $12.32/місяць без податків; розташування, строк оплати і фактичний checkout price залежать від вибору регіону. Це узгоджується з орієнтиром $15 до податків, але не є підтвердженням остаточної ціни або закупівлею.

Єдині фактичні ресурсні значення зараз — характеристики плану. Вимоги API/DB/Unity на цій машині не вимірювалися. До розширення concurrency прийняти такі тестові gates:
- Стабільний 30–60 хвилинний Linux Dedicated Server test з повними 20 slots, включно з 19 ботами; зібрати tick p95/max, active bots/entities, physics/AI time, GC counters, process RSS і CPU.
- Peak total RAM під час одного 20-slot матчу не вище 9 GB на VPS 12 GB, щоб лишити щонайменше 3 GB для ОС, контейнерів, cache і коротких піків. Це плановий acceptance threshold, не поточний результат.
- CPU sustained нижче 70% у типовому матчі; server tick p95 нижче половини 33.3 ms бюджету 30 Hz, а max не перевищує одного tick interval у контрольованому steady-state тесті.
- Перевірити два одночасні матчі окремим тестом; до нього не обіцяти та не рекламувати два матчі на процес.
- На диску залишати щонайменше 20 GB вільними; розмір Linux build, log growth, PostgreSQL data growth і backup storage виміряти до запуску.
- Після тесту зафіксувати RAM/CPU/tick/packet loss/error rates; якщо будь-який gate не проходить, зменшити concurrency або обрати більший VPS до public traffic.

## PostgreSQL migration, backup і rollback

1. Спершу з'ясувати реальне production DB provider/version/schema та отримати погоджений backup. Не використовувати локальний connection string для цієї перевірки.
2. Відновити копію production backup в ізольовану staging PostgreSQL тієї ж major version. Перевірити дублікати (MatchId, UserId), обсяг таблиць, extensions і доступні диски.
3. Згенерувати і review idempotent SQL migration script до target migration; застосувати лише на staging, виміряти блокування/час, перевірити __EFMigrationsHistory, auth/ticket/match workflows.
4. Перед production maintenance зробити новий DB backup і виконати restore drill. У production лишити ApplyMigrationsOnStartup=false та RunSeedOnStartup=false; застосувати migration окремою контрольованою job з окремими правами.
5. Відкат застосунку робити на попередній версії API/image. Не запускати автоматично Down() для security migration: він видаляє session/ticket/server дані. Для невідновлюваної schema/data помилки — відновлювати перевірену DB backup за погодженим recovery plan.
6. Щодня робити encrypted logical PostgreSQL backup поза VPS і регулярно тестувати відновлення. OVH daily backup — додатковий шар, не єдиний backup. Включити rotation Docker/journald logs і моніторинг вільного диску.

## Наступні етапи

### Етап 2 — локальні code/build/config blockers
- Встановити Linux Dedicated Server Build Support для Unity 6000.3.13f1.
- Додати Linux Server profile/build target/script; зберегти Server subtarget, Mono backend і потрібні Server/Map scenes.
- Зібрати Linux artifact; виправити лише доведені compile/build blockers, потім перевірити headless startup, UDP port, DTLS, API auth, register/heartbeat/shutdown.
- Оновити API на .NET 10 LTS і сумісний package set; додати health/readiness checks, prod host configuration і release Compose/service files.
- Узгодити HTTPS API allowlist для server/client, зберігши строгий certificate validation. Не перемикати production DB у межах цього етапу без погодженого config/secret plan.

### Етап 3 — ізольований staging і capacity proof
- Підняти тимчасовий Ubuntu 24.04 staging host/VM або ізольований еквівалент; не використовувати production credentials/data.
- Перевірити build/release rollback, systemd signals, DTLS certificate rotation, connection ticket/replay, heartbeat expiry, API health, private PostgreSQL, backup/restore.
- Провести 20-slot бот-тест, потім два паралельні матчі. Зафіксувати CPU, RSS, tick p95/max, API DB latency, packet loss і disk growth.

### Етап 4 — production migration і cutover за окремим дозволом
- Підтвердити Render API/DB ownership, data export/restore path, DNS/domain і план сумісності старих клієнтів.
- Створити verified backup, підготувати VPS secrets/firewall/monitoring і застосувати production migration окремо від API startup.
- Розгорнути API/Unity versioned releases, перевірити readiness/auth/DTLS/heartbeat, після цього вручну переключити DNS/client rollout. Не робити цю фазу автоматично після Stage 1.

### Етап 5 — обмежений public test і операційна підтримка
- Відкрити обмежену хвилю користувачів з concurrency cap, чергуванням і визначеними rollback triggers.
- Перевірити daily backup/restore, alerting, журналювання, restart/crash recovery, certificate expiry, DB capacity та network latency.
- Знімати cap лише після кількох стабільних навантажувальних вікон; якщо один VPS не має headroom або потрібна HA, винести API/database/game servers окремо.

## Зовнішні першоджерела

- Unity 6 [Server platform requirements](https://docs.unity3d.com/6000.0/Manual/system-requirements.html) і [Dedicated Server requirements](https://docs.unity3d.com/6000.0/Manual/dedicated-server-requirements.html).
- Microsoft [.NET lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-and-net-core) і [.NET on Ubuntu](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install).
- Canonical [Ubuntu release cycle](https://ubuntu.com/about/release-cycle).
- OVHcloud [VPS plans/configurations](https://www.ovhcloud.com/en/vps/).

## Межі аудиту

Перевірено лише локальні репозиторії/configuration, Git diffs, локальний API compile і публічні офіційні product/support pages. Production endpoint, DNS, production database, server purchase, VPS login, deployment, migrations, Unity Linux build, runtime networking і VPS capacity не перевірялися або не виконувалися.



## Stage 2 — local implementation result (2026-11-22)

The dated Stage 1 findings above describe the state at audit time. Stage 2 updated the local implementation without connecting to production.

### Unity

The current Unity 6000.3.13f1 installation has LinuxStandaloneSupport available. The project now has a Linux Server build profile scoped to Server.unity and Map.unity and a Mono Dedicated Server build command. The local build succeeded and produced an ELF executable at D:\Programing\UnityProjects\war-of-machines-local-builds\LinuxServer-20261121-192956. Unity systemd, credential, package, deploy, rollback, health and logrotate templates are under the Unity repository deploy/linux directory.

A Linux runtime proof is still required. WSL returned Wsl/E_ACCESSDENIED and Docker Desktop had no Linux daemon, so the build was not started on Linux. UDP, headless scene loading, DTLS/client handshake, heartbeat and clean shutdown have not been established.

### API, database and edge

The API now targets net10.0 with pinned compatible package versions. Docker SDK/runtime images target .NET 10. Production Compose pins PostgreSQL and Caddy images, publishes only Caddy HTTP/HTTPS/TLS-QUIC ports, keeps API/PostgreSQL on an internal network, fixes the private proxy address, separates runtime/migrator credentials, and keeps startup migrations and seed disabled. EF migration remains an explicit migrator job.

Added GET /health/live and GET /health/ready. Production uses key-per-file secrets, a mounted PostgreSQL CA, TLS, and configurable host allowlists. Data Protection key persistence uses a named writable volume and requires a mounted PFX certificate/password so the persisted key ring is encrypted at rest. Local Compose file-backed secrets are bind mounts; source-file ownership/mode must grant the container identity read access, since Compose cannot remap uid/gid/mode for file sources. Stage 3 must verify the .NET app UID/GID (1654) and PostgreSQL UID/GID (999) against the host secret files. Caddy terminates public TLS; the API port is not published.

PostgreSQL backup/restore scripts use a custom-format archive and refuse restore into a non-empty public schema. They do not drop objects or apply EF migrations. Unity and PostgreSQL scripts are templates only; no production host, real secret, image build, database migration, or deploy was used.

### Stage 2 verification gate

API build succeeded with zero errors; NU1900 remains because vulnerability-feed/cache access was denied. A local fake-DB API smoke returned 200 for liveness and 503 with a generic response for readiness. A temporary local Data Protection certificate produced a key file with no startup warning. Compose static config validation passed. No checked-in test project exists, and dotnet test executed no test cases.

**Historical pre-closeout runtime gate:** BLOCKED before the owner scope decision. Runtime, network and script checks below remain untested and are explicitly deferred to Stage 3.

## Stage 2B — WSL/Docker access and remaining runtime gate (2026-11-22)

**Historical pre-closeout diagnostic status:** WSL/Docker access from Codex was blocked. This did not establish that WSL itself was broken; the later owner-approved scope change moved these tests to Stage 3.

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

## Stage 2 final closeout — owner-approved local preparation scope (2026-11-22)

The owner approved a scope change: Stage 2 is **COMPLETED — LOCAL PREPARATION**. The earlier blocked-runtime gate is a historical snapshot, not a claim that runtime tests passed.

### Verified local artifacts

- Unity Linux Server profile and existing Mono ELF: `D:\Programing\UnityProjects\war-of-machines-local-builds\LinuxServer-20261121-192956\WarOfMachinesServer.x86_64` (ELF `7F-45-4C-46`); prior Editor report records a successful build.
- .NET 10 API project/output, health routes, Dockerfile and prior successful build report.
- Production Compose/Caddy/PostgreSQL TLS, roles, health probe, secret/volume config, explicit migration job and prior static config validation.
- Unity systemd, environment/credential, logrotate, package/deploy/rollback/health templates; PostgreSQL backup/restore scripts.
- NuGet audit using a task-local writable cache reported no vulnerable packages from configured sources. No production endpoint, credentials, DB, migration or data were used.

### Stage 3 — ISOLATED STAGING DEPLOYMENT AND RUNTIME VERIFICATION (not started)

Proposed target from the owner: OVHcloud VPS-3 in Germany or France, Ubuntu 24.04 AMD64, planned 6 vCore / 12 GB RAM / 100 GB NVMe and about $15/month. Price, availability, taxes and actual region configuration are **not verified**.

Stage 3 order: (1) check current price/availability and approved Codex SSH/network path; (2) present the exact offer and obtain separate owner approval before paid provisioning; (3) after provisioning verify Ubuntu, firewall, disk and access; (4) use test-only credentials and an empty/test DB; (5) deploy privately and run all deferred runtime, integration, restore, script and capacity tests. Keep Render production and production data/secrets untouched. Stage 3 is not automatically started by this closeout.

All runtime/integration checks are **NOT TESTED — DEFERRED TO STAGE 3**; see the exhaustive list in DEPLOYMENT_CHECKLIST.md. Production cutover remains separate.

**Stage 2:** COMPLETED — LOCAL PREPARATION. **Ready for Stage 3 staging:** YES, subject to VPS access and provisioning. **Production ready:** NO.

## Stage 3A — OVHcloud VPS provisioning and secure access (IN PROGRESS; 2026-11-22)

Stage 3A has started; Stage 3B remains NOT STARTED. No VPS has been ordered, paid for, provisioned, or accessed. No API, PostgreSQL, or Unity server has been deployed, and production has not been touched.

### Verified preflight

- Windows OpenSSH 9.5p2 and SCP are available to the Codex command runner. A non-authenticated TCP/22 check to github.com succeeded; this confirms outbound port access only. SSH authentication and commands against a future VPS remain NOT VERIFIED.
- The official OVHcloud VPS-3 2027 configurator shows 6 vCore, 12 GB RAM, 100 GB NVMe, unlimited traffic at 2 Gbps and daily backup included. With no commitment selected, the current display is $14.50/month excluding tax. The $12.32/month figure is the discounted 12-month price, not the selected billing option. Source: https://www.ovhcloud.com/en/vps/configurator/
- Europe — Poland, Warsaw is selected and shown as available now. Germany (Limburg) and the listed France locations are unavailable for this exact configuration.
- Ubuntu 24.04 is selectable and selected. The configurator did not expose CPU architecture, so AMD64 remains NOT VERIFIED.
- Public IPv4, exact tax-inclusive recurring total, renewal/auto-renewal details and any setup fees remain NOT VERIFIED. Paid add-ons are off; daily backup is included.

### Account security and remaining gates

OVH account signup is complete and the account console is authenticated. The account type is individual; contact values are intentionally omitted from project documentation. The account security page was inspected read-only and shows that 2FA is disabled. The user must enable 2FA directly; no password, OTP or recovery code is handled here.

No order has been submitted, no payment has been made, and no VPS has been provisioned. Before the user submits an order, enable 2FA, verify the exact tax-inclusive recurring total, renewal/auto-renewal terms, setup fees, public IPv4 and AMD64 architecture, and confirm the total is at or below the $15/month limit. The current $14.50 amount excludes tax, so the budget limit is not yet confirmed. If the final total is over $15/month, stop for the user's revised budget approval. Germany and France are unavailable for this plan; Warsaw is the currently selected European option. Keep paid extras off. The user handles final order and payment. Stage 3B has not started.
## Current rebrand amendment — 2026-11-22

The earlier Stage 1 observations above describe the pre-Stage-2 state. The active API project now targets .NET 10 and is located at D:/Programing/ServerProjects/rav-api/RavApi/RavApi.csproj. The new API namespace and assembly are RavApi. Current Unity deployment settings use RAV_* names first and retain WOM_* aliases for existing installations. Production deployment and database operations remain outside this amendment.
## RAV rebrand validation amendment — 2026-11-22

The Unity project now lives at `D:/Programing/UnityProjects/rav-unity`. First-party C# namespaces use `RAV.*`; the custom assemblies are `RAV.Gameplay.Shared` and `RAV.PlayMode.Tests`. The API remains `RavApi`. Database schema, migration IDs, JWT issuer/audience, production hostname, production service/volume IDs, local DB names, and secrets were preserved.

The API build completed with zero errors and two NU1900 warnings because NuGet vulnerability metadata was unreachable. Docker Compose static configuration validation passed for local and production files; local validation used the checked-in example env file, and production validation used a `.invalid` host plus empty temporary secret stubs. No container was started and no production secret was read.

The two Unity-generated RAV assemblies compile with zero warnings/errors through their generated `.csproj` files. `Assembly-CSharp.csproj` did not reach C# compilation: MSBuild project-reference target resolution exited with zero diagnostics. Unity's Linux Dedicated Server batch build was stopped after repeated Licensing Client IPC reconnect failures; the log also reports `com.unity.editor.headless was not found`. No fresh RAV Linux artifact was produced. The existing `WarOfMachinesServer.x86_64` is a pre-rebrand historical artifact, not a current RAV build.

Stage 3A remains in progress, Stage 3B has not started, and no production or database operation occurred.

## RAV final local build evidence — 2026-11-22

The Unity Editor completed the active Linux Server Build Profile successfully on 2026-11-22 19:55:57–19:56:46 local time. The log confirms `Server.unity` and `Map.unity` loads; output contains a 64-bit x86_64 ELF, UnityPlayer, `LinuxServer_Data`, Mono runtime, native plugins, and RAV managed assemblies. The source code/project changes predate the build. The user-facing Console reported 0 errors and 47 warnings; the Editor log confirms build success and the warnings are nonfatal: obsolete API calls, unused vendor fields, one unreachable CFXR vendor-effect branch, and FishNet migration-helper advisories.

For consistent future deployment, a separate local copy at `D:\Programing\UnityProjects\war-of-machines-local-builds\RAVServer-20261122-1956` uses `RAVServer.x86_64` together with the matching `RAVServer_Data`. It retains the Unity runtime/data/plugins, excludes `RAV_BurstDebugInformation_DoNotShip`, and all 189 retained files match the Desktop build by SHA-256. The package script now checks required Mono/data/runtime files and excludes that debug directory; the deploy script rejects archives missing those required files. Linux script execution and runtime linking remain unverified.

The API rebrand build passed with zero warnings/errors. Linux runtime, UDP 7770, DTLS, database/container integration and all Stage 3B deployment checks remain **NOT TESTED**. Stage 3A is still in progress; Stage 3B has not started and production was not touched.
## RAV final local build evidence — 2026-11-22

The Unity Editor completed the active Linux Server Build Profile successfully on 2026-11-22 19:55:57–19:56:46 local time. The log confirms `Server.unity` and `Map.unity` loads; output contains a 64-bit x86_64 ELF, UnityPlayer, `LinuxServer_Data`, Mono runtime, native plugins, and RAV managed assemblies. The source code/project changes predate the build. The user-facing Console reported 0 errors and 47 warnings; the Editor log confirms build success and the warnings are nonfatal: obsolete API calls, unused vendor fields, one unreachable CFXR vendor-effect branch, and FishNet migration-helper advisories.

For consistent future deployment, a separate local copy at `D:\Programing\UnityProjects\war-of-machines-local-builds\RAVServer-20261122-1956` uses `RAVServer.x86_64` together with the matching `RAVServer_Data`. It retains the Unity runtime/data/plugins, excludes `RAV_BurstDebugInformation_DoNotShip`, and all 189 retained files match the Desktop build by SHA-256. The package script now checks required Mono/data/runtime files and excludes that debug directory; the deploy script rejects archives missing those required files. Linux script execution and runtime linking remain unverified.

The API rebrand build passed with zero warnings/errors. Linux runtime, UDP 7770, DTLS, database/container integration and all Stage 3B deployment checks remain **NOT TESTED**. Stage 3A is still in progress; Stage 3B has not started and production was not touched.
## RAV Stage 3B local preparation closeout — 2026-11-22

This is the current local preparation record. The owner says VPS order #23370514 is still under OVH review and there is no active IP or SSH endpoint. OVH was not rechecked during this task. Stage 3A provisioning remains pending; Stage 3B runtime deployment is NOT STARTED.

### Verified build and bundle

- Unity 6000.3.13f1 built the active Linux Server profile in the Editor at 2026-11-22 23:31 local time. Editor.log reports Build Finished, Result: Success, with Server.unity and Map.unity present in the build section and zero compiler/build errors. The FishNet 3-to-4 upgrade-helper advisory was repeated; it is a compile-time notice, not a build failure.
- The fresh build is D:\Programing\UnityProjects\war-of-machines-local-builds\RAVServer-20261122-staging-hostallowlist. The ELF launcher is 64-bit little-endian x86_64. The matching launcher and data directory were renamed together to RAVServer.x86_64 and RAVServer_Data to match the systemd unit and release scripts.
- The staging copy contains 189 retained runtime files (157,876,244 bytes); the explicitly non-shipping RAV_BurstDebugInformation_DoNotShip folder is excluded. Source and payload hashes match. The compiled Assembly-CSharp.dll contains IsExplicitStagingBase. Staging API host validation requires HTTPS and a dedicated staging DNS label.
- API Release build passed: 0 warnings, 0 errors.
- Docker Compose static validation passed with staging.env.example. Structural checks confirm PostgreSQL and API have no published ports, the backend network is internal, all named volumes use the rav_staging_ prefix, and only Caddy publishes TCP 80/443 and UDP 443. No container was started.
- The Unity systemd EnvironmentFile path, bootstrap output path, RAV_API_BASE, RAV_SERVER_ADDRESS and UDP 7770 configuration align. PostgreSQL connection strings use VerifyFull and the generated staging CA; Caddy automatic HTTPS remains enabled.
- Backup creates a custom-format dump. Restore targets a new rav_restore_* database and contains no DROP DATABASE or destructive schema-drop step.
- The package script passed PowerShell parsing, retained-file SHA comparison, nested Unity tar extraction verification, and archive manifest verification.

Final bundle: D:\Programing\ServerProjects\rav-api\deploy\staging\artifacts\RAV-Staging-3B-20261122T154549Z.tar.gz
Outer SHA-256: 712af20149fd1ec75484f8a7dc984b02761d998972c88c3ebf9ba76522e39c3f

### Safety and deferred verification

No real staging secrets, production credentials, production database, OVH endpoint, or migrations were used. No Docker services or deployment were started. The deployment scripts now refuse existing Docker containers, staging volumes/network, incomplete or conflicting Docker configuration, active firewall policy, and existing RAV guard chains instead of replacing them.

Bash syntax was not parsed locally: Git Bash had already returned E_ACCESSDENIED and was not retried; Docker Desktop image inventory returned permission denied. The runbook now performs bash -n on every bundled shell script after extraction and before host changes. The packager also checks Bash shebangs, LF endings, and trailing whitespace.

Linux host bootstrap, SSH/fingerprint verification, Docker image builds and startup, PostgreSQL TLS, Caddy certificate issuance, API health, Unity headless runtime, UDP 7770, FishNet initialization, DTLS, registration/heartbeat, tickets/authentication, match creation, restore execution, and deployment/rollback scripts remain NOT TESTED until a staging VPS exists. The DTLS client probe and Unity gameplay harness are not bundled; matchmaking, bot spawning, and one match with 20 machines cannot be verified by the host-level performance sampler alone.

Local package preparation is complete. Stage 3A remains pending; run only the syntax-only preflight first after access is available, then wait for explicit Stage 3B authorization before any host mutation.

## 2026-11-23 — Stage 3B preparation final verification

- **Authoritative bundle:** `D:\Programing\ServerProjects\rav-api\deploy\staging\artifacts\RAV-Staging-3B-20261122T154549Z.tar.gz`.
- **Integrity:** Outer SHA-256 is `712af20149fd1ec75484f8a7dc984b02761d998972c88c3ebf9ba76522e39c3f`; it matches the saved handoff. The archive lists 119 safe relative paths and contains Compose/Caddy, the staging environment example, API payload, the RAV Unity release archive, host bootstrap/deploy/rollback/backup/restore/smoke/performance scripts, systemd units, and first-deploy documentation. The filename scan found no `.env.local`, bare `.env`, `id_rsa`/`id_ed25519`, `.pem`, `.key`, `.p12`, or `.pfx` entries; secrets are generated by the included provisioning script.
- **Previously passed local checks:** API Release build (0 warnings, 0 errors) and Docker Compose static config validation passed during the original preparation. They were not rerun during this recovery closeout. API and Unity `git diff --check` passed for tracked changes on 2026-11-23. The three deployment documents are untracked, so a separate whitespace scan was used; it found trailing spaces in checklist lines 3-4, now removed, and the final scan passed. Existing working-tree changes remain uncommitted and preserved.
- **Local validation limit:** Bash syntax parsing was not available in this Windows session. The earlier Git Bash `E_ACCESSDENIED` was not retried. The bundle's first-deploy runbook requires `bash -n` on all bundled shell scripts after extraction and before host changes.
- **Deferred:** The last owner-provided VPS state was still OVH review pending with no active endpoint/IP or SSH access; OVH was not rechecked. Host bootstrap, Docker startup, PostgreSQL TLS/backup-restore execution, Caddy certificates, API health, Unity headless runtime, UDP 7770, FishNet, DTLS, registration/heartbeat, tickets, match/gameplay/load and deployment/rollback execution remain **NOT TESTED** until an isolated VPS is active.
- **Status:** Deployment bundle **READY**. Local static validation **PARTIAL**. **READY FOR VPS DEPLOYMENT: NO** until an active VPS endpoint/SSH path is available and the bundled `bash -n` preflight passes; this reflects the last owner-provided VPS state (OVH was not rechecked). Stage 3B runtime **NOT STARTED**. Do not mutate a host until access is verified, the preflight passes, and Stage 3B is explicitly authorized.