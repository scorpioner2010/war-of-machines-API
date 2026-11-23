#!/usr/bin/env bash
set -Eeuo pipefail
umask 027
if [[ $# -lt 1 || $# -gt 3 ]]; then echo "Usage: $0 <existing-release-id> [udp-port] [https-api-health-url]" >&2; exit 2; fi
release_id="$1"; port="${2:-7770}"; api_health_url="${3:-}"
if [[ ! "$release_id" =~ ^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$ || "$release_id" == "." || "$release_id" == ".." ]]; then echo "Invalid release ID." >&2; exit 2; fi
[[ "$port" =~ ^[0-9]{1,5}$ ]] && (( port > 0 && port <= 65535 )) || { echo "Invalid UDP port." >&2; exit 2; }
if [[ -n "$api_health_url" && "$api_health_url" != https://* ]]; then echo "API health URL must use HTTPS." >&2; exit 2; fi
release_root=/opt/rav-staging/unity/releases; release_dir="$release_root/$release_id"; current_link=/opt/rav-staging/unity/current; next_link="/opt/rav-staging/unity/.rollback-$$"
service=rav-staging-dedicated-server.service; health=/opt/rav-staging/scripts/health-check-unity.sh
[[ -d "$release_dir" && ! -L "$release_dir" && -x "$release_dir/RAVServer.x86_64" ]] || { echo "Requested immutable release is unavailable." >&2; exit 1; }
[[ -f "$release_dir/RAVServer_Data/globalgamemanagers" && -f "$release_dir/RAVServer_Data/Managed/Assembly-CSharp.dll" && -f "$release_dir/RAVServer_Data/MonoBleedingEdge/x86_64/libmonobdwgc-2.0.so" && -f "$release_dir/UnityPlayer.so" ]] || { echo "Requested release is incomplete." >&2; exit 1; }
[[ ! -e "$current_link" || -L "$current_link" ]] || { echo "Current path is not a symlink." >&2; exit 1; }
previous_target=""
if [[ -L "$current_link" ]]; then
  previous_target="$(readlink -f -- "$current_link")"
  [[ "$previous_target" == "$release_root/"* && -d "$previous_target" ]] || { echo "Current symlink target invalid." >&2; exit 1; }
fi
ln -s "$release_dir" "$next_link"; mv -Tf -- "$next_link" "$current_link"
if systemctl restart "$service" && "$health" "$port" "$api_health_url"; then echo "Rolled back to $release_id."; exit 0; fi
echo "Rollback health check failed; restoring the previous symlink." >&2
if [[ -n "$previous_target" ]]; then ln -s "$previous_target" "$next_link"; mv -Tf -- "$next_link" "$current_link"; systemctl restart "$service" || true; else rm -f -- "$current_link"; systemctl stop "$service" || true; fi
exit 1
