#!/usr/bin/env bash
set -Eeuo pipefail
source "$(cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
if [[ "${RAV_STAGING_MIGRATION_CONFIRM:-}" != "APPLY_MIGRATIONS_TO_RAV_STAGING" ]]; then
  echo "Set RAV_STAGING_MIGRATION_CONFIRM=APPLY_MIGRATIONS_TO_RAV_STAGING after verifying the isolated staging VPS." >&2
  exit 2
fi
rav_compose --profile migration run --rm --build migrator
