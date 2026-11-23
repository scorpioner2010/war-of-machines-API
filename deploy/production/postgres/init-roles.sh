#!/usr/bin/env bash
set -Eeuo pipefail

runtime_password="$(< /run/secrets/db_runtime_password)"
migrator_password="$(< /run/secrets/db_migrator_password)"

psql --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" --set=ON_ERROR_STOP=1 \
  --set=database_name="$POSTGRES_DB" \
  --set=runtime_password="$runtime_password" \
  --set=migrator_password="$migrator_password" <<'SQL'
CREATE ROLE wom_runtime LOGIN PASSWORD :'runtime_password';
CREATE ROLE wom_migrator LOGIN PASSWORD :'migrator_password';
GRANT CONNECT ON DATABASE :"database_name" TO wom_runtime, wom_migrator;
GRANT USAGE ON SCHEMA public TO wom_runtime;
GRANT USAGE, CREATE ON SCHEMA public TO wom_migrator;
ALTER DEFAULT PRIVILEGES FOR ROLE wom_migrator IN SCHEMA public
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO wom_runtime;
ALTER DEFAULT PRIVILEGES FOR ROLE wom_migrator IN SCHEMA public
  GRANT USAGE, SELECT ON SEQUENCES TO wom_runtime;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO wom_runtime;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO wom_runtime;
SQL
