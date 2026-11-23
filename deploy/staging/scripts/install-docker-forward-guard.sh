#!/usr/bin/env bash
set -Eeuo pipefail
command -v iptables >/dev/null || { echo "iptables is required for the Docker forwarding guard." >&2; exit 2; }
iptables -nL DOCKER-USER >/dev/null 2>&1 || { echo "Docker DOCKER-USER chain is unavailable; guard was not installed." >&2; exit 2; }
if iptables -nL RAV_STAGING_DOCKER_GUARD >/dev/null 2>&1; then
  echo "An existing RAV staging Docker guard chain was found; preserving it and refusing to flush or replace its rules." >&2
  exit 3
fi
existing="$(iptables -S DOCKER-USER | sed -n '/^-A DOCKER-USER/p' | grep -vE -- '(-j RETURN$|-j RAV_STAGING_DOCKER_GUARD$)' || true)"
if [[ -n "$existing" ]]; then echo "Existing DOCKER-USER policy found; preserving it and refusing to reorder rules." >&2; exit 3; fi
wan_if="$(ip -o route get 1.1.1.1 | awk '{for (i=1; i<=NF; i++) if ($i=="dev") {print $(i+1); exit}}')"
[[ -n "$wan_if" ]] || { echo "Could not identify the external network interface." >&2; exit 2; }
iptables -N RAV_STAGING_DOCKER_GUARD 2>/dev/null || true
iptables -F RAV_STAGING_DOCKER_GUARD
iptables -A RAV_STAGING_DOCKER_GUARD -i "$wan_if" -m conntrack --ctstate ESTABLISHED,RELATED -j RETURN
iptables -A RAV_STAGING_DOCKER_GUARD -i "$wan_if" -p tcp -m conntrack --ctorigdstport 80 -j RETURN
iptables -A RAV_STAGING_DOCKER_GUARD -i "$wan_if" -p tcp -m conntrack --ctorigdstport 443 -j RETURN
iptables -A RAV_STAGING_DOCKER_GUARD -i "$wan_if" -p udp -m conntrack --ctorigdstport 443 -j RETURN
iptables -A RAV_STAGING_DOCKER_GUARD -i "$wan_if" -j DROP
iptables -C DOCKER-USER -j RAV_STAGING_DOCKER_GUARD 2>/dev/null || iptables -I DOCKER-USER 1 -j RAV_STAGING_DOCKER_GUARD
