#!/usr/bin/env bash
set -Eeuo pipefail
runtime_password="$(< /run/secrets/db_runtime_password)"
migrator_password="$(< /run/secrets/db_migrator_password)"
psql --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" --set=ON_ERROR_STOP=1 --set=database_name="$POSTGRES_DB" --set=runtime_password="$runtime_password" --set=migrator_password="$migrator_password" <<'SQL'
CREATE ROLE rav_staging_runtime LOGIN PASSWORD :'runtime_password';
CREATE ROLE rav_staging_migrator LOGIN PASSWORD :'migrator_password';
GRANT CONNECT ON DATABASE :"database_name" TO rav_staging_runtime, rav_staging_migrator;
GRANT USAGE ON SCHEMA public TO rav_staging_runtime;
GRANT USAGE, CREATE ON SCHEMA public TO rav_staging_migrator;
ALTER DEFAULT PRIVILEGES FOR ROLE rav_staging_migrator IN SCHEMA public
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO rav_staging_runtime;
ALTER DEFAULT PRIVILEGES FOR ROLE rav_staging_migrator IN SCHEMA public
  GRANT USAGE, SELECT ON SEQUENCES TO rav_staging_runtime;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO rav_staging_runtime;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO rav_staging_runtime;
SQL
