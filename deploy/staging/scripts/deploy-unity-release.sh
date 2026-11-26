#!/usr/bin/env bash
set -Eeuo pipefail
umask 027
if [[ $# -lt 2 || $# -gt 4 ]]; then echo "Usage: $0 <release-id> <release.tar.gz> [udp-port] [https-api-health-url]" >&2; exit 2; fi
release_id="$1"; archive="$(realpath -- "$2")"; port="${3:-7770}"; api_health_url="${4:-}"
if [[ ! "$release_id" =~ ^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$ || "$release_id" == "." || "$release_id" == ".." ]]; then echo "Invalid release ID." >&2; exit 2; fi
[[ -f "$archive" && -r "$archive" ]] || { echo "Release archive is not readable." >&2; exit 2; }
[[ "$port" =~ ^[0-9]{1,5}$ ]] && (( port > 0 && port <= 65535 )) || { echo "Invalid UDP port." >&2; exit 2; }
if [[ -n "$api_health_url" && "$api_health_url" != https://* ]]; then echo "API health URL must use HTTPS." >&2; exit 2; fi
release_root=/opt/rav-staging/unity/releases; current_link=/opt/rav-staging/unity/current
service=rav-staging-dedicated-server.service; health=/opt/rav-staging/scripts/health-check-unity.sh
service_was_enabled="$(systemctl is-enabled "$service" 2>/dev/null || true)"
install -d -o root -g rav-staging-server -m 0750 "$release_root"
release_dir="$release_root/$release_id"; stage_dir="$release_root/.staging-$release_id-$$"; next_link="/opt/rav-staging/unity/.current-$release_id-$$"
[[ ! -e "$release_dir" && ! -L "$release_dir" && ! -e "$stage_dir" ]] || { echo "Release path exists; refusing overwrite." >&2; exit 1; }
[[ ! -e "$current_link" || -L "$current_link" ]] || { echo "Current path is not a symlink." >&2; exit 1; }
while IFS= read -r member; do
  [[ "$member" != /* && "$member" != *$'\r'* ]] || { echo "Archive has an unsafe path." >&2; exit 1; }
  IFS='/' read -r -a parts <<< "$member"
  for part in "${parts[@]}"; do [[ "$part" != ".." ]] || { echo "Archive path traversal refused." >&2; exit 1; }; done
done < <(tar -tzf "$archive")
while IFS= read -r entry; do
  kind="$(printf '%s' "$entry" | cut -c1)"
  [[ "$kind" == "-" || "$kind" == "d" ]] || { echo "Archive links/special files refused." >&2; exit 1; }
done < <(tar -tvzf "$archive")
mkdir -m 0750 "$stage_dir"
cleanup() { [[ "$stage_dir" == "$release_root"/.staging-* && -d "$stage_dir" ]] && rm -rf -- "$stage_dir"; }
trap cleanup EXIT
tar --extract --gzip --file "$archive" --directory "$stage_dir" --no-same-owner --no-same-permissions
if [[ ! -f "$stage_dir/RAVServer.x86_64" || ! -f "$stage_dir/RAVServer_Data/globalgamemanagers" || ! -f "$stage_dir/RAVServer_Data/Managed/Assembly-CSharp.dll" || ! -f "$stage_dir/RAVServer_Data/MonoBleedingEdge/x86_64/libmonobdwgc-2.0.so" || ! -f "$stage_dir/UnityPlayer.so" ]]; then
  echo "Release archive is missing required Linux Unity files." >&2; exit 1
fi
if [[ -e "$stage_dir/Logs" || -L "$stage_dir/Logs" ]]; then
  echo "Release already contains Logs; refusing to replace its contents." >&2; exit 1
fi
install -d -o rav-staging-server -g rav-staging-server -m 0750 /var/log/rav-staging/unity
ln -s /var/log/rav-staging/unity "$stage_dir/Logs"
chmod 0750 "$stage_dir/RAVServer.x86_64"
chown -R root:rav-staging-server "$stage_dir"
find "$stage_dir" -type d -exec chmod 0750 {} +
find "$stage_dir" -type f -exec chmod 0640 {} +
chmod 0750 "$stage_dir/RAVServer.x86_64"
mv -- "$stage_dir" "$release_dir"
trap - EXIT
previous_target=""
if [[ -L "$current_link" ]]; then
  previous_target="$(readlink -f -- "$current_link")"
  [[ "$previous_target" == "$release_root/"* && -d "$previous_target" ]] || { echo "Current symlink target invalid." >&2; exit 1; }
fi
ln -s "$release_dir" "$next_link"; mv -Tf -- "$next_link" "$current_link"
if systemctl restart "$service" && "$health" "$port" "$api_health_url" && systemctl enable "$service" && systemctl enable --now rav-staging-monitor.timer; then
  echo "Release $release_id is active; Unity service and staging monitor timer are enabled."
  exit 0
fi
echo "Release health check failed; restoring the previous symlink." >&2
if [[ -n "$previous_target" ]]; then
  ln -s "$previous_target" "$next_link"
  mv -Tf -- "$next_link" "$current_link"
  systemctl restart "$service" || true
else
  rm -f -- "$current_link"
  systemctl stop "$service" || true
  if [[ "$service_was_enabled" != "enabled" ]]; then systemctl disable "$service" || true; fi
fi
exit 1
