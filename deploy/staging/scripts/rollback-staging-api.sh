#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
source "$(cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
if [[ "$RAV_STAGING_ROOT" != "/opt/rav-staging" || "${RAV_STAGING_API_ROLLBACK_CONFIRM:-}" != "ROLLBACK_RAV_STAGING_API" || "${RAV_STAGING_DB_ROLLBACK_COMPATIBLE:-}" != "YES" ]]; then
  echo "Rollback requires /opt/rav-staging plus explicit API and schema-compatibility confirmations." >&2
  exit 2
fi
deployments=/var/lib/rav-staging/deployments
previous_file="$deployments/previous-api-image"
[[ -s "$previous_file" ]] || { echo "No previous image tag is recorded." >&2; exit 2; }
previous="$(cat "$previous_file")"
[[ "$previous" =~ ^rav-api:staging-[A-Za-z0-9][A-Za-z0-9._-]{0,63}$ ]] || { echo "Recorded image tag is invalid." >&2; exit 2; }
docker image inspect "$previous" >/dev/null || { echo "Previous image is not present locally; no image was pulled." >&2; exit 2; }
current="$(staging_env_value RAV_STAGING_API_IMAGE)"
export RAV_STAGING_API_IMAGE="$previous"
if ! rav_compose up -d --no-build --wait api caddy; then echo "API rollback health check failed; DB and volumes remain unchanged." >&2; exit 1; fi
curl --fail --silent --show-error --max-time 10 "https://$RAV_STAGING_API_HOST/health/ready" >/dev/null

file="$RAV_STAGING_ENV_FILE"; temporary="$(mktemp "$(dirname -- "$file")/.staging-env.XXXXXX")"
awk -v key=RAV_STAGING_API_IMAGE -v value="$previous" 'BEGIN{found=0} index($0,key"=")==1 {if(!found) print key"="value; found=1; next} {print} END{if(!found) print key"="value}' "$file" > "$temporary"
chown root:root "$temporary"; chmod 0600 "$temporary"; mv -- "$temporary" "$file"
if [[ -n "$current" ]]; then printf '%s' "$current" > "$deployments/previous-api-image.next"; chown root:rav-staging-server "$deployments/previous-api-image.next"; chmod 0640 "$deployments/previous-api-image.next"; mv -f -- "$deployments/previous-api-image.next" "$deployments/previous-api-image"; fi
printf '%s' "$previous" > "$deployments/current-api-image.next"; chown root:rav-staging-server "$deployments/current-api-image.next"; chmod 0640 "$deployments/current-api-image.next"; mv -f -- "$deployments/current-api-image.next" "$deployments/current-api-image"
echo "RAV staging API rolled back to $previous. PostgreSQL schema/data were not changed or reversed."
