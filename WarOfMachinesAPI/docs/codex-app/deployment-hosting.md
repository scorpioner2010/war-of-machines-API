# Deployment Hosting

## Поточний порядок
Проєкт — .NET 8 ASP.NET Core API з PostgreSQL. Production запуск не повинен автоматично застосовувати міграції або seed: базова конфігурація має `Database:ApplyMigrationsOnStartup=false` і `Database:RunSeedOnStartup=false`. Міграція `20261119085129_SecurityHardeningFoundation` застосовується окремо після review у контрольованій БД.

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
