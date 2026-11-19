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
