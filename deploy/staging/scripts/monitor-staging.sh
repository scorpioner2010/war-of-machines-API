#!/usr/bin/env bash
set -Eeuo pipefail
source "$(cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
api_base="https://$RAV_STAGING_API_HOST"
curl --fail --silent --show-error --max-time 8 "$api_base/health/ready" >/dev/null
rav_compose exec -T postgres pg_isready -U rav_staging_bootstrap -d rav_staging >/dev/null
systemctl is-active --quiet rav-staging-dedicated-server.service
ss -H -lun "sport = :7770" | grep -q .
echo "RAV staging API, PostgreSQL, Unity service, and UDP socket are healthy."
