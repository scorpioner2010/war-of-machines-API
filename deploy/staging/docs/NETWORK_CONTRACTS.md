# RAV staging network and configuration contracts

| Component | Address/configuration | Exposure |
| --- | --- | --- |
| Unity Dedicated Server | `RAV_SERVER_ADDRESS=<VPS IPv4>`, `RAV_SERVER_PORT=7770` | Host UDP 7770, FishNet DTLS; UFW allows only the declared game port |
| API | `https://<RAV_STAGING_API_HOST>`; Caddy proxies to `api:8080` | TCP 80/443 and UDP 443 on Caddy only; API has no host port |
| PostgreSQL | Compose service `postgres:5432`; `DATABASE_URL` is mounted as an API secret | No `ports`; internal backend network only; all network DB clients use TLS with verified staging CA and hostname `postgres` |
| Caddy | `RAV_STAGING_API_HOST` | Automatic HTTPS; certificate verification stays enabled |
| Compose | Project `rav-staging`; isolated named volumes `rav_staging_*`; backend network `internal: true` | No production project/volume names are referenced |
| systemd | `rav-staging-compose.service`, `rav-staging-dedicated-server.service` | Dedicated server starts only after Compose service; Unity runs as `rav-staging-server` |

## Environment source of truth

- `/etc/rav-staging/staging.env` defines the real API hostname, generated secret directory, DB name, reserved Docker backend subnet/Caddy IP, image tags, region and advertised server address.
- `/etc/rav-staging/unity-server.env` is the Unity `EnvironmentFile`. The dedicated-server unit reads exactly this file. Its API host, server ID, credential and public address must match the staging environment and generated secrets.
- Compose mounts database URLs, JWT values, the server identity, PostgreSQL CA, and Data Protection certificate/password from files under `/etc/rav-staging/secrets`. API loads these through its `/run/secrets` key-per-file configuration provider. No credential is set inline in Compose or committed to Git.
- `RAV_UTP_SERVER_CERTIFICATE`, `RAV_UTP_SERVER_PRIVATE_KEY`, and `RAV_UTP_CA_CERTIFICATE` point to systemd-provided credentials for the FishNet DTLS transport. The client must trust the separately distributed staging CA; the server common name is `rav-staging-server`.

## Ports and controls

- Public inbound: configured SSH TCP port, TCP 80/443, UDP 443 for HTTP/3, UDP 7770 for FishNet DTLS.
- Postgres 5432 and API 8080 are not published. Docker `DOCKER-USER` guard denies other inbound published container ports; it refuses to reorder an existing unrelated policy.
- `pg_hba.conf` rejects non-TLS network sessions and uses SCRAM authentication for TLS sessions. The API connection string uses `SSL Mode=VerifyFull` and the staging CA.
- Do not change `SSL Mode`, trust the server without a certificate, disable HTTPS/DTLS, use the staging CA in production, or add a public PostgreSQL port.
