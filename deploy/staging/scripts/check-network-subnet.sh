#!/usr/bin/env bash
set -Eeuo pipefail
source "$(cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
subnet="$(staging_env_value RAV_STAGING_BACKEND_SUBNET)"
caddy_ip="$(staging_env_value RAV_STAGING_CADDY_IP)"
[[ -n "$subnet" && -n "$caddy_ip" ]] || { echo "Staging network settings are missing." >&2; exit 2; }
python3 - "$subnet" "$caddy_ip" <<'PY'
import ipaddress
import json
import subprocess
import sys

target = ipaddress.ip_network(sys.argv[1], strict=True)
proxy = ipaddress.ip_address(sys.argv[2])
if proxy not in target or proxy in (target.network_address, target.broadcast_address):
    raise SystemExit("Caddy static address must be a usable host address inside the staging subnet.")

names = subprocess.check_output(
    ["docker", "network", "ls", "--format", "{{.Name}}"], text=True
).splitlines()
for name in names:
    inspected = json.loads(subprocess.check_output(["docker", "network", "inspect", name], text=True))[0]
    for config in inspected.get("IPAM", {}).get("Config", []) or []:
        value = config.get("Subnet")
        if not value:
            continue
        existing = ipaddress.ip_network(value, strict=False)
        if name == "rav-staging_backend":
            if existing != target:
                raise SystemExit(f"Existing rav-staging backend network uses {existing}; expected {target}. No network was changed.")
            continue
        if target.version == existing.version and target.overlaps(existing):
            raise SystemExit(f"Staging subnet {target} overlaps existing Docker network {name} ({existing}). No network was changed.")
print(f"Staging network preflight passed: {target}, Caddy {proxy}.")
PY
