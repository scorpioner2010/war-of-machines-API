#!/usr/bin/env bash
set -Eeuo pipefail
source "$(cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
if [[ "${RAV_STAGING_SEED_CONFIRM:-}" != "SEED_RAV_STAGING_ONLY" ]]; then
  echo "Set RAV_STAGING_SEED_CONFIRM=SEED_RAV_STAGING_ONLY after verifying the isolated staging VPS." >&2
  exit 2
fi
rav_compose --profile seed run --rm seed
