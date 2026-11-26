# Deployment Hosting

## Поточний порядок
Проєкт — .NET 10 ASP.NET Core API з PostgreSQL. Production запуск не повинен автоматично застосовувати міграції або seed: базова конфігурація має `Database:ApplyMigrationsOnStartup=false` і `Database:RunSeedOnStartup=false`. Міграція `20261119085129_SecurityHardeningFoundation` застосовується окремо після review у контрольованій БД.

## Обов'язкові secrets/config
- `Jwt__Key` — випадковий signing key не менше 32 байт; production rotation виконується окремою погодженою процедурою.
- `Jwt__Issuer`, `Jwt__Audience` — стабільні значення для конкретного середовища.
- `DATABASE_URL` або `ConnectionStrings__DefaultConnection` — production PostgreSQL з TLS-параметрами провайдера.
- `DedicatedServers__Provisioned__*` — bootstrap identity server-ів; credentials не зберігати в Git.
- `ForwardedHeaders__KnownProxies__*` — реальні reverse-proxy IP, якщо API працює за proxy.
- `Cors__AllowedOrigins__*` — лише потрібні браузерні origins; Unity не використовує CORS для auth.
- `Swagger__Enabled=false` у production.

## HTTPS і proxy
Production API має бути доступний клієнтам лише через HTTPS. Unity production build приймає лише allowlisted HTTPS API host і не має HTTP fallback. Reverse proxy повинен завершувати TLS, передавати proto/for тільки з довіреного proxy та не дозволяти прямий internet HTTP ingress до API. `ForwardedHeaders:KnownProxies` не можна залишати порожнім у deployment, де потрібна proxy-aware схема.

## Порядок deployment
1. Provision secrets і Dedicated Server credentials через secret manager.
2. Перевірити backup/rollback plan та застосувати additive EF migration до staging.
3. Deploy API з migrations/seed switches disabled або виконати migration окремою job.
4. Перевірити JWT issuer/audience, HTTPS, rate limit, admin policy, server register/heartbeat і ticket replay.
5. Deploy сумісні Unity Client/Dedicated Server builds із DTLS certificate secrets.
6. Увімкнути server registry credentials і перевірити reconnect/heartbeat expiration.
7. Лише після smoke/regression перевірок допустити public test traffic.

## Обмеження
Production deployment, ротація реальних ключів і production DB migration цим task не виконувалися. `/admin` і Swagger потребують окремого network access control, навіть якщо вони role-protected.

## Staging container packaging
- GHCR/Altare staging images must be built from the final `runtime` stage only; never publish the `migrate` stage because it contains API source and EF tooling.
- Runtime publish disables debug symbols and removes `appsettings.Development.json`. The image sets `ASPNETCORE_ENVIRONMENT=Production`, disables migrations/catalog seed/development fixtures by default, and uses `/data/protection` for a persistent Altare Data folder mounted at `/data`. For the first deployment to an empty, explicitly verified staging database only, set `Database:ApplyMigrationsOnStartup=true` and `Database:RunSeedOnStartup=true`; keep `Database:SeedDevelopmentFixtures=false`. After successful migration/catalog seed, turn the two startup switches off and redeploy. Never point these switches at production.
- The staging entrypoint requires an encrypted runtime Data Protection PFX and password in Production, decodes the PFX into a mode-0600 temporary file, and fails closed when either value is missing. A provider PostgreSQL CA may be supplied as a base64 environment value and is written to a restricted temporary file; configure `Database:SslMode=VerifyFull` and `Database:RootCertificate` to validate the DB certificate and hostname.
- `.dockerignore` excludes `.env` files, PFX/P12/private-key/PEM files, production secret directories, VCS metadata, and build artifacts. Before publishing, inspect the final runtime filesystem for environment files, certificates/private keys, debug/source files, and embedded runtime configuration secrets.

## Оновлення аудиту 2026-11-22

Повний Stage 1 readiness audit і послідовність наступних етапів описані в [deployment plan](../deployment/DEPLOYMENT_PLAN.md), [progress](../deployment/DEPLOYMENT_PROGRESS.md) і [checklist](../deployment/DEPLOYMENT_CHECKLIST.md). Перевірка підтвердила: API та Dockerfile поки на .NET 8, підтримка якого завершилася 2026-11-10; перед публічним запуском потрібен перехід на підтримуваний .NET LTS. Встановлений Unity Editor не має Linux Dedicated Server module/profile, а production API host allowlist і ASP.NET AllowedHosts досі прив'язані до Render. API ще не має окремих health/readiness endpoints, а довірені proxy IP не задані.

Локальна база була тимчасовою тестовою конфігурацією за уточненням користувача. Її поточний вміст цього аудиту не перевірявся; production DB schema/status лишається невідомим. Користувач дозволив майбутні зміни конфігурації/коду для online setup, якщо вони знадобляться. У межах Stage 1 жодне production з'єднання, перемикання БД, міграція чи deployment не виконувалися. Перед cutover потрібно окремо з'ясувати фактичного DB provider/owner, зробити перевірений backup і спершу пройти staging migration/restore.


## Оновлення Stage 2 — 2026-11-22

API project and Docker images now target .NET 10 with compatible pinned dependencies. Added GET /health/live and database-aware GET /health/ready. Compose uses private PostgreSQL networking, a separate explicit migration job, migrations/seed disabled during API startup, Caddy TLS, key-per-file secrets, and a private API proxy address. Persistent ASP.NET Data Protection keys use a named volume and require a PFX encryption certificate/password secret. Local Compose file-backed secret files are bind mounts; their host ownership/mode must allow the container UID/GID to read them.

The Unity repository contains a Linux Dedicated Server profile/build command and a successful local Mono Linux ELF build. The matching systemd/release/rollback/health/logrotate templates are documented in the deployment plan. Linux runtime, DTLS/UDP handshake and script syntax were unverified at that point. The subsequent owner-approved closeout reclassified Stage 2 as local preparation and deferred those tests to Stage 3; this does not mean they passed. No production endpoint/database was contacted.
## Final Stage 2 closeout — 2026-11-22

The owner-approved scope defines Stage 2 as **COMPLETED — LOCAL PREPARATION**: Unity Linux build/profile, .NET 10 API/health routes, security-preserving settings, Compose/Caddy/PostgreSQL templates, Unity systemd/deployment and PostgreSQL backup/restore templates, plus a NuGet audit reporting no vulnerable packages from configured sources. This status is not runtime acceptance.

Linux startup/scenes/shutdown, UDP 7770, DTLS/FishNet authentication, API/PostgreSQL containers, Compose runtime, migrations, backup/restore, deployment-script execution, server registration/heartbeat, tickets, matchmaking, bots, 20-machine match, performance and concurrency are **NOT TESTED — DEFERRED TO STAGE 3**.

Stage 3 is **ISOLATED STAGING DEPLOYMENT AND RUNTIME VERIFICATION**. First check current OVHcloud price/availability and Codex SSH/network access. Provision the proposed Europe VPS only after separate owner approval of the exact paid offer. Use Ubuntu 24.04 AMD64 and test-only secrets/database; keep Render production untouched. No VPS was purchased or provisioned. Production readiness remains **NO**.


## RAV branding update — 2026-11-22

The API project is now RavApi and its Docker build output uses rav-api image tags. Compose service IDs, project name, PostgreSQL volume names, Data Protection storage path, production hostname, and JWT issuer/audience remain stable for compatibility. RAV_SECRETS_DIR is preferred, with WOM_SECRETS_DIR supported as a fallback. No deployment, production database operation, or migration was run.
## RAV final Linux build validation — 2026-11-22

The manual Unity 6000.3.13f1 Linux Dedicated Server build completed successfully in the Editor on 2026-11-22 19:55:57–19:56:46 local time. The Editor log reports `Build Finished, Result: Success.`; the build loaded `Assets/Game/Scenes/Server.unity` and `Map.unity`. The original Desktop output is `C:\Users\TorasPC\Desktop\LinuxServer`; it contains the ELF64 x86_64 launcher, `UnityPlayer.so`, `LinuxServer_Data`, Mono runtime, and native plugin libraries. A separate prepared copy is at `D:\Programing\UnityProjects\war-of-machines-local-builds\RAVServer-20261122-1956`. That copy renames the launcher and its matching data folder to `RAVServer.x86_64` / `RAVServer_Data`, omits the explicitly non-shipping Burst debug directory, and has 189/189 source file hashes verified. It is a local staging artifact only; no tarball, host deployment, or Linux runtime test was performed.

The build log records 47 warnings and zero build errors: 37 CS0618 obsolete API warnings, four CS0414 unused-field warnings in FishNet utilities/demos, one CS0162 unreachable vendor-effect code warning, and five FishNet 3-to-4 upgrade-helper advisories. These are nonfatal third-party/editor/demo warnings; they do not block the successful build. Runtime shared-library resolution remains unverified without executing on Linux.

The final API build passed `dotnet build RavApi.csproj --no-restore --nologo -v:minimal` with zero warnings and errors. Stage 3A remains in progress; Stage 3B and production deployment remain not started.
