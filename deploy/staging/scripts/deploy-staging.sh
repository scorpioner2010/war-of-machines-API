#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
script_dir="$(cd -- "$(dirname -- "$0")" && pwd)"
source "$script_dir/lib.sh"
if [[ "$RAV_STAGING_ROOT" != "/opt/rav-staging" ]]; then echo "Deployment is intended only from /opt/rav-staging." >&2; exit 2; fi
if [[ "${RAV_STAGING_DEPLOY_CONFIRM:-}" != "START_RAV_STAGING_SERVICES" || "${RAV_STAGING_MIGRATION_CONFIRM:-}" != "APPLY_MIGRATIONS_TO_RAV_STAGING" || "${RAV_STAGING_SEED_CONFIRM:-}" != "SEED_RAV_STAGING_ONLY" ]]; then
  echo "Set the three explicit RAV staging deployment/migration/seed confirmations after verifying the isolated VPS." >&2
  exit 2
fi
if [[ "$RAV_STAGING_API_HOST" == *".invalid" || "$RAV_STAGING_API_HOST" == "war-of-machines-api.onrender.com" ]]; then echo "Set a real staging DNS hostname; refusing placeholders or the known production host." >&2; exit 2; fi
server_address="$(staging_env_value RAV_STAGING_SERVER_ADDRESS)"
server_region="$(staging_env_value RAV_STAGING_REGION)"
[[ -n "$server_address" && "$server_address" != "REPLACE_WITH_VPS_PUBLIC_IPV4" ]] || { echo "Set the provisioned public IPv4 in staging.env first." >&2; exit 2; }
[[ -n "$server_region" ]] || { echo "Set RAV_STAGING_REGION in staging.env first." >&2; exit 2; }
python3 - "$server_address" <<'PY'
import ipaddress
import sys
try:
    ipaddress.IPv4Address(sys.argv[1])
except ipaddress.AddressValueError:
    raise SystemExit("RAV_STAGING_SERVER_ADDRESS must be the actual VPS public IPv4 address.")
PY
unity_env=/etc/rav-staging/unity-server.env
[[ -s "$unity_env" ]] || { echo "Missing Unity staging EnvironmentFile." >&2; exit 2; }
grep -Fxq "RAV_API_BASE=https://$RAV_STAGING_API_HOST" "$unity_env" || { echo "Unity API base does not match the staging host." >&2; exit 2; }
grep -Fxq "RAV_SERVER_ADDRESS=$server_address" "$unity_env" || { echo "Unity advertised address does not match staging.env." >&2; exit 2; }
grep -Fxq "RAV_SERVER_REGION=$server_region" "$unity_env" || { echo "Unity region does not match staging.env." >&2; exit 2; }
grep -Fxq "RAV_SERVER_PORT=7770" "$unity_env" || { echo "Unity game port must remain UDP 7770." >&2; exit 2; }
expected_server_id="$(tr -d '\r\n' < "$RAV_STAGING_SECRETS_DIR/server_id")"
grep -Fxq "RAV_SERVER_ID=$expected_server_id" "$unity_env" || { echo "Unity server ID does not match generated staging secret." >&2; exit 2; }
[[ -s /etc/rav-staging/credentials/dtls_server.crt && -s /etc/rav-staging/credentials/dtls_server.key && -s /etc/rav-staging/credentials/dtls_ca.crt ]] || { echo "DTLS staging credentials are missing." >&2; exit 2; }
"$script_dir/check-config.sh"
"$script_dir/check-network-subnet.sh"

release_id="${1:-$(date -u +%Y%m%dT%H%M%SZ)}"
[[ "$release_id" =~ ^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$ ]] || { echo "Invalid release ID." >&2; exit 2; }
api_image="rav-api:staging-$release_id"
migrator_image="rav-api-migrator:staging-$release_id"
export RAV_STAGING_API_IMAGE="$api_image"
export RAV_STAGING_MIGRATOR_IMAGE="$migrator_image"

deployments=/var/lib/rav-staging/deployments
install -d -o root -g rav-staging-server -m 0750 "$deployments"
previous_image="$(staging_env_value RAV_STAGING_API_IMAGE)"
if [[ -n "$previous_image" ]]; then printf '%s' "$previous_image" > "$deployments/previous-api-image.next"; chown root:rav-staging-server "$deployments/previous-api-image.next"; chmod 0640 "$deployments/previous-api-image.next"; mv -f -- "$deployments/previous-api-image.next" "$deployments/previous-api-image"; fi

set_env_value() {
  local key="$1" value="$2" file="$RAV_STAGING_ENV_FILE" temporary
  temporary="$(mktemp "$(dirname -- "$file")/.staging-env.XXXXXX")"
  awk -v key="$key" -v value="$value" 'BEGIN{found=0} index($0,key"=")==1 {if(!found) print key"="value; found=1; next} {print} END{if(!found) print key"="value}' "$file" > "$temporary"
  chown root:root "$temporary"
  chmod 0600 "$temporary"
  mv -- "$temporary" "$file"
}
rav_compose --profile migration --profile seed build api migrator
rav_compose up -d --wait postgres
"$script_dir/backup-staging-postgres.sh"
RAV_STAGING_MIGRATION_CONFIRM=APPLY_MIGRATIONS_TO_RAV_STAGING "$script_dir/migrate-staging.sh"
RAV_STAGING_SEED_CONFIRM=SEED_RAV_STAGING_ONLY "$script_dir/seed-staging.sh"
if ! rav_compose up -d --wait api caddy; then
  rav_compose stop api caddy || true
  echo "API/Caddy readiness failed. PostgreSQL and the pre-migration backup remain intact; no automatic schema rollback was attempted." >&2
  exit 1
fi
api_base="https://$RAV_STAGING_API_HOST"
curl --fail --silent --show-error --max-time 10 "$api_base/health/live" >/dev/null
curl --fail --silent --show-error --max-time 10 "$api_base/health/ready" >/dev/null
set_env_value RAV_STAGING_API_IMAGE "$api_image"
set_env_value RAV_STAGING_MIGRATOR_IMAGE "$migrator_image"
systemctl enable --now rav-staging-compose.service
printf '%s' "$api_image" > "$deployments/current-api-image.next"
chown root:rav-staging-server "$deployments/current-api-image.next"
chmod 0640 "$deployments/current-api-image.next"
mv -f -- "$deployments/current-api-image.next" "$deployments/current-api-image"
echo "RAV staging API/PostgreSQL/Caddy deployment completed as release $release_id."
echo "Unity server package deployment and game-level DTLS/match smoke remain a separate, explicit step."
