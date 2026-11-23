#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
if [[ $# -gt 1 || ! "${1:-600}" =~ ^[0-9]{2,5}$ ]]; then echo "Usage: $0 [duration-seconds (60..86400)]" >&2; exit 2; fi
duration="${1:-600}"
if (( duration < 60 || duration > 86400 )); then echo "Duration must be 60..86400 seconds." >&2; exit 2; fi
if [[ "$(id -u)" -ne 0 ]]; then echo "Run as root to sample the systemd process and journald." >&2; exit 2; fi
command -v pidstat >/dev/null || { echo "Install sysstat before sampling; no packages were changed." >&2; exit 2; }
service="rav-staging-dedicated-server.service"
pid="$(systemctl show --property=MainPID --value "$service")"
[[ "$pid" =~ ^[1-9][0-9]*$ && -r "/proc/$pid/status" ]] || { echo "RAV staging dedicated server is not running." >&2; exit 2; }
output_dir="/var/log/rav-staging/perf-$(date -u +%Y%m%dT%H%M%SZ)"
install -d -o root -g rav-staging-server -m 0750 "$output_dir"
printf 'utc_start,pid,duration_seconds\n%s,%s,%s\n' "$(date -u +%FT%TZ)" "$pid" "$duration" > "$output_dir/summary.csv"
grep -E '^(Name|State|VmPeak|VmSize|VmRSS|Threads):' "/proc/$pid/status" > "$output_dir/process-before.txt"
ss -s > "$output_dir/sockets-before.txt"
journalctl -u "$service" --since '-5 minutes' --no-pager > "$output_dir/unity-log-before.txt"
pidstat -h -u -r -d -p "$pid" 1 "$duration" > "$output_dir/pidstat.txt"
grep -E '^(Name|State|VmPeak|VmSize|VmRSS|Threads):' "/proc/$pid/status" > "$output_dir/process-after.txt"
ss -s > "$output_dir/sockets-after.txt"
journalctl -u "$service" --since "-$((duration + 60)) seconds" --no-pager > "$output_dir/unity-log-window.txt"
echo "Collected host-level RAV staging performance sample: $output_dir"
echo "This does not create 20 clients or measure Unity simulation tick time; use docs/PERFORMANCE_PLAN.md."
