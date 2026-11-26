#!/usr/bin/env bash
set -Eeuo pipefail
port="${1:-7770}"; api_health_url="${2:-}"
[[ "$port" =~ ^[0-9]{1,5}$ ]] && (( port > 0 && port <= 65535 )) || { echo "Invalid UDP port." >&2; exit 2; }
systemctl is-active --quiet rav-staging-dedicated-server.service || { echo "RAV staging Unity service is inactive." >&2; exit 1; }
startup_deadline=$((SECONDS + 45))
while ! ss -H -lun "sport = :$port" | grep -q .; do
  systemctl is-active --quiet rav-staging-dedicated-server.service || { echo "RAV staging Unity service stopped before UDP port $port became ready." >&2; exit 1; }
  if (( SECONDS >= startup_deadline )); then
    echo "No UDP socket listening on $port after 45 seconds." >&2
    exit 1
  fi
  sleep 1
done
if [[ -n "$api_health_url" ]]; then
  [[ "$api_health_url" == https://* ]] || { echo "API health URL must use HTTPS." >&2; exit 2; }
  curl --fail --silent --show-error --max-time 8 "$api_health_url" >/dev/null
fi
echo "RAV staging Unity service and UDP port $port are healthy."
