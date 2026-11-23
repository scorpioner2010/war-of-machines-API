#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
if [[ $# -ne 1 ]]; then echo "Usage: $0 <custom-format-backup.dump>" >&2; exit 2; fi
backup="$(realpath -- "$1")"
if [[ ! -f "$backup" || ! -r "$backup" ]]; then echo "Backup is not a readable regular file." >&2; exit 2; fi
pg_restore --list "$backup" >/dev/null

script_dir="$(cd -- "$(dirname -- "$0")" && pwd)"
deploy_dir="$(cd -- "$script_dir/.." && pwd)"
compose_file="$deploy_dir/compose.yaml"
env_file="$deploy_dir/.env.production"
if [[ -v COMPOSE_ENV_FILE && -n "$COMPOSE_ENV_FILE" ]]; then env_file="$COMPOSE_ENV_FILE"; fi
if [[ ! -f "$env_file" ]]; then echo "Compose environment file is missing." >&2; exit 2; fi

echo "Restore requires an empty public schema. It will not drop objects or apply EF migrations." >&2
docker compose --env-file "$env_file" -f "$compose_file" exec -T postgres \
  sh -eu -c '
    export PGPASSWORD="$(cat /run/secrets/db_migrator_password)"
    empty="$(psql --host=127.0.0.1 --username=wom_migrator --dbname="$POSTGRES_DB" --tuples-only --no-align --set=ON_ERROR_STOP=1 \
      --command="SELECT (SELECT count(*) FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname=chr(112)||chr(117)||chr(98)||chr(108)||chr(105)||chr(99) AND c.relkind IN (chr(114),chr(112),chr(83),chr(118),chr(109),chr(102))) + (SELECT count(*) FROM pg_proc p JOIN pg_namespace n ON n.oid=p.pronamespace WHERE n.nspname=chr(112)||chr(117)||chr(98)||chr(108)||chr(105)||chr(99)) + (SELECT count(*) FROM pg_type t JOIN pg_namespace n ON n.oid=t.typnamespace WHERE n.nspname=chr(112)||chr(117)||chr(98)||chr(108)||chr(105)||chr(99) AND t.typtype IN (chr(98),chr(99),chr(100),chr(101),chr(114)))")"
    if [ "$empty" != "0" ]; then
      echo "Restore refused: public schema is not empty." >&2
      exit 3
    fi
    pg_restore --exit-on-error --no-owner --no-privileges \
      --host=127.0.0.1 --username=wom_migrator --dbname="$POSTGRES_DB"
    psql --host=127.0.0.1 --username=wom_migrator --dbname="$POSTGRES_DB" --set=ON_ERROR_STOP=1 \
      --command="GRANT USAGE ON SCHEMA public TO wom_runtime; GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO wom_runtime; GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO wom_runtime;"
  ' < "$backup"

echo "Restore completed. Verify EF migration history and application behavior in isolated staging."
