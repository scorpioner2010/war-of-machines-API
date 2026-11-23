#!/usr/bin/env bash
set -Eeuo pipefail
script_dir="$(cd -- "$(dirname -- "$0")" && pwd)"
source "$script_dir/lib.sh"
for name in db_bootstrap_password db_runtime_password db_migrator_password api_database_url migration_database_url jwt_key jwt_issuer jwt_audience server_id server_credential postgres_ca.crt postgres_server.crt postgres_server.key dtls_ca.crt dtls_server.crt dtls_server.key data_protection.pfx data_protection_password; do
  [[ -s "$RAV_STAGING_SECRETS_DIR/$name" ]] || { echo "Missing required secret file: $name" >&2; exit 2; }
done
docker compose --project-name rav-staging --project-directory "$RAV_STAGING_ROOT" --env-file "$RAV_STAGING_ENV_FILE" --file "$RAV_STAGING_ROOT/compose.yaml" config --quiet
echo "RAV staging Compose configuration is valid; no containers were started."
