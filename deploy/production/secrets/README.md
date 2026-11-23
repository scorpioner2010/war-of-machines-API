# Production secret files

This directory is ignored by Git. Create the secret files on the deployment host; never add real values to Git.

File-backed Compose secrets are bind mounts: local Compose ignores secret uid/gid/mode remapping. Set host ownership/mode so the container process can read each source file. For this pinned setup, API secret files (including the Data Protection PFX and password) should be root:1654 mode 0440; Postgres init-password files should be root:999 mode 0440. Verify APP_UID/GID in the built .NET image and the postgres account before staging. The Postgres TLS key is separately root-controlled by the generation script as postgres UID/GID 999 mode 0400. Do not use root:root mode 0600 for files that non-root container processes must read.

- `db_bootstrap_password` — PostgreSQL initial superuser password, used only on first database initialization and controlled maintenance.
- `db_runtime_password` — password for the least-privilege `wom_runtime` role.
- `db_migrator_password` — password for the separate `wom_migrator` role.
- `api_database_url` — Npgsql connection string for `wom_runtime`, with `Host=postgres`, `SSL Mode=VerifyFull`, and `Root Certificate=/run/secrets/postgres-ca.crt`.
- `migration_database_url` — Npgsql connection string for `wom_migrator`, with the same TLS verification settings.
- `jwt_key`, `jwt_issuer`, `jwt_audience` — production JWT signing and validation configuration.
- `server_id`, `server_credential` — provisioned Dedicated Server bootstrap identity and credential.
- `postgres_ca.crt` — public certificate of the private CA used for the PostgreSQL server certificate.
- `postgres_server_certificate` and `postgres_server_private_key` — PostgreSQL TLS certificate/key with SAN `DNS:postgres`; key must be owned by container UID/GID `999:999` and mode `0400` on the host so PostgreSQL accepts it.

Generate the private CA and PostgreSQL certificate on the deployment host with `../scripts/generate-postgres-tls.sh`. Keep the CA private key outside the Compose secret directory after issuing the server certificate; never add it to Git.

Write text secret files without a trailing newline unless the value explicitly requires one; the API trims line endings from the Data Protection certificate password. Never use real production credentials in a local test. The initial database role script runs only when the persistent data directory is empty. Password rotation for an existing database is a separate reviewed operation.

- data_protection_certificate.pfx and data_protection_certificate_password — private X.509 key-encryption certificate and password for the ASP.NET Data Protection key ring. Keep the PFX restricted and retain the certificate for reading old keys during approved rotation.
