# RAV brand and compatibility map

## Canonical identity

- **Name:** RAV
- **Full name:** Robotic Assault Vehicles
- **Tagline:** Tactical Multiplayer Combat

The backend project, solution, assembly, output DLL/apphost, and owned namespaces use `RavApi` / `RavApi.*`. The first-party Unity namespaces use `RAV.*`; the Unity-owned `Assembly-CSharp` identity remains stable. Owned asmdefs use `RAV.Gameplay.Shared` and `RAV.PlayMode.Tests`, and serialized Unity type identifiers were updated without changing script GUIDs.

## Configuration aliases

Unity runtime configuration prefers the RAV variables. If a RAV variable is unset or blank, the previous WOM variable remains accepted:

| New key | Compatibility alias |
| --- | --- |
| `RAV_API_BASE` | `WOM_API_BASE` |
| `RAV_SERVER_ID` | `WOM_SERVER_ID` |
| `RAV_SERVER_CREDENTIAL` | `WOM_SERVER_CREDENTIAL` |
| `RAV_SERVER_REGION` | `WOM_SERVER_REGION` |
| `RAV_SERVER_ADDRESS` | `WOM_SERVER_ADDRESS` |
| `RAV_SERVER_PORT` | `WOM_SERVER_PORT` |
| `RAV_UTP_SERVER_CERTIFICATE` | `WOM_UTP_SERVER_CERTIFICATE` |
| `RAV_UTP_SERVER_PRIVATE_KEY` | `WOM_UTP_SERVER_PRIVATE_KEY` |
| `RAV_UTP_SERVER_COMMON_NAME` | `WOM_UTP_SERVER_COMMON_NAME` |
| `RAV_UTP_CA_CERTIFICATE` | `WOM_UTP_CA_CERTIFICATE` |

The local security initializer can carry a credential from an existing WOM-named local file into RAV-named output. It does not print credential values.

## Intentional legacy identifiers in active API files

| File | Remaining identifier | Why it stays |
| --- | --- | --- |
| `RavApi/appsettings.json` | `WarOfMachines.Api`, `WarOfMachines.Client` | Existing JWT issuer/audience values remain stable so previously issued tokens and current clients are not invalidated. |
| `RavApi/appsettings.Production.json`; Unity `Assets/Resources/production-api-hosts.txt`; Unity `Assets/Game/Scripts/API/HttpLink.cs` | `war-of-machines-api.onrender.com` | Existing externally controlled production HTTPS endpoint; no production endpoint or client allowlist was changed. |
| `deploy/production/compose.yaml` | `war-of-machines` project name; `wom_bootstrap`; `WOM_SECRETS_DIR`; `/var/lib/war-of-machines/...` | Existing production project, role, secret-path fallback, and persistent Data Protection path must remain stable. Service IDs and volume names were not changed. |
| `deploy/production/postgres/init-roles.sh`; `deploy/production/scripts/backup-postgres.sh`; `deploy/production/scripts/restore-postgres.sh`; `RavApi/docs/codex-app/database-ef-core.md` | `wom_runtime`, `wom_migrator`, and the existing backup path | Preserves existing database permissions, backup locations, and restore compatibility. No database was contacted or modified. |
| `RavApi/docker-compose.local.yml`; `RavApi/Properties/launchSettings.json`; `RavApi/tools/Start-LocalPostgresApi.ps1` | `war_of_machines_local`, `wom_local` | Keeps existing local database/role/volume identity and data rather than creating a new empty local database. |
| `RavApi/tools/Initialize-LocalSecurityEnvironment.ps1` | `WOM_SERVER_CREDENTIAL` | Imports an existing local credential as a fallback without displaying or replacing its value. |
| `RavApi/docs/codex-app/deployment-hosting.md` | `WOM_SECRETS_DIR` | Documents the supported legacy environment fallback. |
| `RavApi/docs/codex-app/branding-and-compatibility.md`; deployment plan/progress/checklist; historical task journals and logs | Legacy names and earlier build observations | The compatibility map documents active fallbacks; deployment history remains append-only and preserves the facts as recorded at the time. The pre-rebrand Unity Linux artifact is historical, not the current RAV build. |

## Database and deployment safety

C# namespaces and EF model metadata were renamed together. Migration IDs, migration operations, table/column names, schema, and the EF migrations-history contract remain unchanged. This work did not connect to a database, run migrations, or modify data. No production service or database was contacted or changed.

## Validation limits

Earlier checks included two NU1900 feed-metadata warnings and an `Assembly-CSharp.csproj` MSBuild project-reference resolution failure before C# diagnostics; these do not override the final Editor build evidence below. The latest API build passed with zero warnings/errors, and the manual Unity Editor Linux Server build passed after the RAV namespace conversion. PlayMode tests and Linux runtime tests were not run.
## Final build evidence — 2026-11-22

`dotnet build RavApi.csproj --no-restore --nologo -v:minimal` passed with zero warnings and errors. The earlier restore build's NU1900 warnings were feed-metadata access notices, not compiler errors. Unity's manual Linux Dedicated Server Build Profile also passed in 49 seconds with zero build errors; the log confirms the Server and Map scenes. It emitted 47 non-fatal warnings: 37 obsolete API calls, four unused fields, one unreachable vendor-effect code path, and five FishNet upgrade-helper advisories. No Linux runtime startup, shared-library resolution, UDP, DTLS, or API integration test was run.