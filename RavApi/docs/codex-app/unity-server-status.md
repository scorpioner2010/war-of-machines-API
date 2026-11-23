# Unity Dedicated Server Registry

## Поточна поведінка
Registry Dedicated Servers зберігається в PostgreSQL. Кожен сервер має стабільний `ServerId` і окремий credential. API перевіряє credential через окрему `DedicatedServer` authentication scheme; player JWT для цих операцій не підходить.

`GET /unity-server/status` залишається anonymous для discovery і повертає лише зареєстровані сервери з heartbeat у межах `DedicatedServers:HeartbeatTimeoutSeconds`. API restart не стирає registry, а неактивні записи автоматично виключаються з відповіді.

## Власники
- `Controllers/UnityServerStatusController.cs` — register, heartbeat, unregister і discovery.
- `Infrastructure/DedicatedServerAuthenticationHandler.cs` — server-only headers/policy.
- `Infrastructure/DedicatedServerCredentialValidator.cs` — hash validation і bootstrap provisioning.
- `Models/DedicatedServer.cs` — persisted identity, address, capacity, activity та heartbeat.
- `Data/AppDbContext.cs` — PostgreSQL mapping/indexes.

## Конфігурація
- `DedicatedServers:Provisioned` — початковий список `ServerId` + credential у захищеній конфігурації. Реальні credentials не комітяться.
- `DedicatedServers:HeartbeatTimeoutSeconds` — expiration в discovery; допустимий діапазон 5–300 секунд.
- `X-Server-Id` і `X-Server-Credential` — обов'язкові headers для server mutations.
- Address береться з body сервера, а не з `X-Forwarded-For`; forwarded headers приймаються тільки від `ForwardedHeaders:KnownProxies`.

## Контракти
- `POST /unity-server/register` — server policy; сервер може створити/оновити лише власний запис. Body: `{ region, address, port, maxPlayers }`.
- `PUT /unity-server/{serverId}/heartbeat` — server policy; route `serverId` мусить збігатися з authenticated server claim. Body містить address, port, players, capacity, active matches, status і message.
- `POST /unity-server/{serverId}/unregister` — server policy; позначає власний запис offline.
- `GET /unity-server/status` — anonymous; повертає `{ servers: [...] }` з `serverId`, region, address, port, capacity, players, active matches і last heartbeat.

## Безпека
Credential у PostgreSQL зберігається лише як SHA-256 verification hash; порівняння виконується constant-time. Сервер не може heartbeat/unregister чужий `ServerId`. Public discovery не приймає update payload.

## Перевірки
Локально перевірені реєстрація, discovery, підроблений credential і heartbeat чужого server id. Повний dedicated runtime залежить від налаштування server environment variables.
