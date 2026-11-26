#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
source "$(cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
if [[ "$RAV_STAGING_ROOT" != "/opt/rav-staging" ]]; then echo "Backup is intended only for /opt/rav-staging." >&2; exit 2; fi
if [[ $# -gt 1 ]]; then echo "Usage: $0 [backup-directory-under-/var/backups/rav-staging]" >&2; exit 2; fi
backup_dir="${1:-/var/backups/rav-staging/postgres}"
case "$backup_dir" in /var/backups/rav-staging/postgres|/var/backups/rav-staging/postgres/*) ;; *) echo "Backups must stay under /var/backups/rav-staging/postgres." >&2; exit 2 ;; esac
[[ ! -L "$backup_dir" ]] || { echo "Refusing a symlinked backup destination." >&2; exit 2; }
install -d -m 0700 "$backup_dir"
stamp="$(date -u +%Y%m%dT%H%M%SZ)"
destination="$backup_dir/rav-staging-$stamp.dump"
[[ ! -e "$destination" && ! -L "$destination" ]] || { echo "Backup destination exists." >&2; exit 1; }
temporary="$(mktemp "$backup_dir/.rav-staging-$stamp.XXXXXX")"
trap 'rm -f -- "$temporary"' EXIT
rav_compose exec -T postgres sh -eu -c 'export PGPASSWORD="$(cat /run/secrets/db_bootstrap_password)" PGHOST=postgres PGPORT=5432 PGSSLMODE=verify-full PGSSLROOTCERT=/var/lib/postgresql/tls/ca.crt; exec pg_dump --username="$POSTGRES_USER" --format=custom --no-owner --no-privileges --dbname="$POSTGRES_DB"' > "$temporary"
rav_compose exec -T postgres pg_restore --list < "$temporary" >/dev/null
chmod 0600 "$temporary"
mv -- "$temporary" "$destination"
trap - EXIT
echo "Created and validated staging-only PostgreSQL backup: $destination"
