#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
if [[ $# -ne 1 || "${RAV_STAGING_RESTORE_CONFIRM:-}" != "RESTORE_TO_NEW_RAV_STAGING_DB" ]]; then
  echo "Usage: RAV_STAGING_RESTORE_CONFIRM=RESTORE_TO_NEW_RAV_STAGING_DB $0 <custom-format-staging-backup.dump>" >&2
  exit 2
fi
source "$(cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
if [[ "$RAV_STAGING_ROOT" != "/opt/rav-staging" ]]; then echo "Restore is intended only for /opt/rav-staging." >&2; exit 2; fi
backup="$(realpath -- "$1")"
[[ -f "$backup" && -r "$backup" ]] || { echo "Backup must be a readable regular file." >&2; exit 2; }

restore_db="rav_restore_$(date -u +%Y%m%d%H%M%S)_${RANDOM}"
export RAV_RESTORE_DB="$restore_db"
echo "Restore target: new isolated database $restore_db. No existing database will be dropped or overwritten."
rav_compose exec -T -e RAV_RESTORE_DB="$restore_db" postgres sh -eu -c '
  export PGPASSWORD="$(cat /run/secrets/db_bootstrap_password)" PGHOST=postgres PGPORT=5432 PGSSLMODE=verify-full PGSSLROOTCERT=/var/lib/postgresql/tls/ca.crt
  case "$RAV_RESTORE_DB" in rav_restore_[0-9]*_[0-9]*) ;; *) echo "Invalid restore database name." >&2; exit 2 ;; esac
  createdb --username="$POSTGRES_USER" --owner=rav_staging_migrator --template=template0 "$RAV_RESTORE_DB"
  psql --username="$POSTGRES_USER" --dbname=postgres --set=ON_ERROR_STOP=1 --set=database_name="$RAV_RESTORE_DB" --command="GRANT CONNECT ON DATABASE :\"database_name\" TO rav_staging_runtime"
'

rav_compose exec -T postgres pg_restore --list < "$backup" >/dev/null
rav_compose exec -T -e RAV_RESTORE_DB="$restore_db" postgres sh -eu -c '
  export PGPASSWORD="$(cat /run/secrets/db_migrator_password)" PGHOST=postgres PGPORT=5432 PGSSLMODE=verify-full PGSSLROOTCERT=/var/lib/postgresql/tls/ca.crt
  pg_restore --exit-on-error --no-owner --no-privileges --username=rav_staging_migrator --dbname="$RAV_RESTORE_DB"
  psql --username=rav_staging_migrator --dbname="$RAV_RESTORE_DB" --set=ON_ERROR_STOP=1 --command="GRANT USAGE ON SCHEMA public TO rav_staging_runtime; GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO rav_staging_runtime; GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO rav_staging_runtime;"
' < "$backup"
echo "Restore test completed into $restore_db. Verify object counts and readiness; retain or remove this test database only through a separately reviewed manual action."
