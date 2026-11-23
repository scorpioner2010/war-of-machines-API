#!/usr/bin/env bash
set -Eeuo pipefail
RAV_STAGING_ROOT="${RAV_STAGING_ROOT:-$(cd -- "$(dirname -- "$0")/.." && pwd)}"
RAV_STAGING_ENV_FILE="${RAV_STAGING_ENV_FILE:-/etc/rav-staging/staging.env}"
RAV_STAGING_SECRETS_DIR="${RAV_STAGING_SECRETS_DIR:-/etc/rav-staging/secrets}"
if [[ ! -f "$RAV_STAGING_ENV_FILE" ]]; then echo "Missing staging config: $RAV_STAGING_ENV_FILE" >&2; return 2 2>/dev/null || exit 2; fi
if [[ ! -d "$RAV_STAGING_SECRETS_DIR" ]]; then echo "Missing isolated staging secrets directory." >&2; return 2 2>/dev/null || exit 2; fi
staging_env_value() { local key="$1"; sed -n "s/^$key=//p" "$RAV_STAGING_ENV_FILE" | tail -n 1; }
RAV_STAGING_API_HOST="$(staging_env_value RAV_STAGING_API_HOST)"
RAV_STAGING_DB="$(staging_env_value RAV_STAGING_POSTGRES_DB)"
if [[ -z "$RAV_STAGING_API_HOST" || "$RAV_STAGING_API_HOST" == "war-of-machines-api.onrender.com" ]]; then echo "Refusing an empty or known production API host." >&2; return 2 2>/dev/null || exit 2; fi
if [[ ! "$RAV_STAGING_API_HOST" =~ ^[A-Za-z0-9]([A-Za-z0-9.-]*[A-Za-z0-9])?$ || ( "$RAV_STAGING_API_HOST" != *".staging."* && "$RAV_STAGING_API_HOST" != staging.* ) ]]; then echo "Use a valid staging-specific DNS hostname containing a staging label." >&2; return 2 2>/dev/null || exit 2; fi
if [[ "$RAV_STAGING_DB" != "rav_staging" ]]; then echo "Refusing to operate unless the database is explicitly named rav_staging." >&2; return 2 2>/dev/null || exit 2; fi
rav_compose() { docker compose --project-name rav-staging --project-directory "$RAV_STAGING_ROOT" --env-file "$RAV_STAGING_ENV_FILE" --file "$RAV_STAGING_ROOT/compose.yaml" "$@"; }
