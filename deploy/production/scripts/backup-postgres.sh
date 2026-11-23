#!/usr/bin/env bash
set -Eeuo pipefail
umask 077

script_dir="$(cd -- "$(dirname -- "$0")" && pwd)"
deploy_dir="$(cd -- "$script_dir/.." && pwd)"
compose_file="$deploy_dir/compose.yaml"
env_file="$deploy_dir/.env.production"
if [[ -v COMPOSE_ENV_FILE && -n "$COMPOSE_ENV_FILE" ]]; then env_file="$COMPOSE_ENV_FILE"; fi
backup_dir="/var/backups/war-of-machines/postgres"
if [[ $# -gt 1 ]]; then echo "Usage: $0 [backup-directory]" >&2; exit 2; fi
if [[ $# -ge 1 ]]; then backup_dir="$1"; fi
if [[ ! -f "$env_file" ]]; then echo "Compose environment file is missing." >&2; exit 2; fi
if [[ ! -d "$backup_dir" ]]; then install -d -m 0700 "$backup_dir"; fi
if [[ ! -w "$backup_dir" ]]; then echo "Backup directory is not writable." >&2; exit 2; fi

stamp="$(date -u +%Y%m%dT%H%M%SZ)"
destination="$backup_dir/rav-$stamp.dump"
if [[ -e "$destination" || -L "$destination" ]]; then echo "Backup destination already exists." >&2; exit 1; fi
temporary="$(mktemp "$backup_dir/.rav-$stamp.XXXXXX")"
trap 'rm -f -- "$temporary"' EXIT

docker compose --env-file "$env_file" -f "$compose_file" exec -T postgres \
  sh -eu -c '
    export PGPASSWORD="$(cat /run/secrets/db_bootstrap_password)"
    exec pg_dump --host=127.0.0.1 --username="$POSTGRES_USER" \
      --format=custom --no-owner --no-privileges --dbname="$POSTGRES_DB"
  ' > "$temporary"

pg_restore --list "$temporary" >/dev/null
chmod 0600 "$temporary"
if [[ -e "$destination" || -L "$destination" ]]; then echo "Backup destination appeared during write." >&2; exit 1; fi
mv -- "$temporary" "$destination"
trap - EXIT
echo "Created PostgreSQL custom-format backup: $destination"
