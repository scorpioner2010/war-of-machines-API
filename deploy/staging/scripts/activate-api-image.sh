#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
if [[ $# -ne 1 ]]; then echo "Usage: $0 <staging-api-image>" >&2; exit 2; fi
image="$1"
[[ "$image" =~ ^rav-api:staging-[A-Za-z0-9][A-Za-z0-9._-]{0,63}$ ]] || { echo "Invalid staging API image tag." >&2; exit 2; }
script_dir="/opt/rav-staging/scripts"
source "$script_dir/lib.sh"
[[ "$RAV_STAGING_ROOT" == "/opt/rav-staging" && "$RAV_STAGING_DB" == "rav_staging" ]] || { echo "Refusing to activate outside the RAV staging deployment." >&2; exit 2; }
docker image inspect "$image" >/dev/null || { echo "Staged API image is not present; no service changed." >&2; exit 2; }
current="$(staging_env_value RAV_STAGING_API_IMAGE)"
[[ "$current" =~ ^rav-api:staging-[A-Za-z0-9][A-Za-z0-9._-]{0,63}$ ]] || { echo "Current API image is invalid; refusing activation." >&2; exit 2; }
if [[ "$current" == "$image" ]]; then
  curl --fail --silent --show-error --max-time 10 "https://$RAV_STAGING_API_HOST/health/ready" >/dev/null
  echo "The requested staging API image is already active and healthy."
  exit 0
fi

deployments=/var/lib/rav-staging/deployments
install -d -o root -g rav-staging-server -m 0750 "$deployments"
printf '%s' "$current" > "$deployments/previous-api-image.next"
chown root:rav-staging-server "$deployments/previous-api-image.next"
chmod 0640 "$deployments/previous-api-image.next"
mv -f -- "$deployments/previous-api-image.next" "$deployments/previous-api-image"

set_env_value() {
  local key="$1" value="$2" file="$RAV_STAGING_ENV_FILE" temporary
  temporary="$(mktemp "$(dirname -- "$file")/.staging-env.XXXXXX")"
  awk -v key="$key" -v value="$value" 'BEGIN{found=0} index($0,key"=")==1 {if(!found) print key"="value; found=1; next} {print} END{if(!found) print key"="value}' "$file" > "$temporary"
  chown root:root "$temporary"
  chmod 0600 "$temporary"
  mv -- "$temporary" "$file"
}

set_env_value RAV_STAGING_API_IMAGE "$image"
export RAV_STAGING_API_IMAGE="$image"
if rav_compose up -d --no-build --no-deps --wait api \
    && curl --fail --silent --show-error --max-time 10 "https://$RAV_STAGING_API_HOST/health/live" >/dev/null \
    && curl --fail --silent --show-error --max-time 10 "https://$RAV_STAGING_API_HOST/health/ready" >/dev/null; then
  printf '%s' "$image" > "$deployments/current-api-image.next"
  chown root:rav-staging-server "$deployments/current-api-image.next"
  chmod 0640 "$deployments/current-api-image.next"
  mv -f -- "$deployments/current-api-image.next" "$deployments/current-api-image"
  echo "Staging API image $image is active and healthy. PostgreSQL, Caddy, and schema were not changed."
  exit 0
fi

if RAV_STAGING_API_ROLLBACK_CONFIRM=ROLLBACK_RAV_STAGING_API \
   RAV_STAGING_DB_ROLLBACK_COMPATIBLE=YES \
   bash "$script_dir/rollback-staging-api.sh"; then
  echo "API activation failed; the previous staging API image was restored."
else
  echo "API activation and automatic rollback both failed; inspect staging API status before further changes." >&2
fi
exit 1
