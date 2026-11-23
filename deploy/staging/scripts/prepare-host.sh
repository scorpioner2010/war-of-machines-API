#!/usr/bin/env bash
set -Eeuo pipefail
if [[ "$(id -u)" -ne 0 || $# -ne 1 || ! -s "$1" ]]; then echo "Usage: sudo $0 <ravadmin-authorized_keys.pub>" >&2; exit 2; fi
key_file="$(realpath -- "$1")"
source_root="$(cd -- "$(dirname -- "$0")/.." && pwd)"
chmod 0755 "$source_root"/scripts/*.sh

[[ -f /etc/os-release ]] || { echo "Cannot identify Ubuntu release." >&2; exit 2; }
. /etc/os-release
[[ "$ID" == ubuntu && "$VERSION_ID" == 24.04 ]] || { echo "This provisioner supports Ubuntu 24.04 only." >&2; exit 2; }
[[ "$(dpkg --print-architecture)" == amd64 ]] || { echo "Expected the VPS amd64 architecture." >&2; exit 2; }

if command -v docker >/dev/null; then
  if systemctl is-active --quiet docker && [[ -n "$(docker ps -aq)" ]]; then echo "Existing Docker containers found; preserving them and refusing host/firewall changes." >&2; exit 3; fi
  if ! systemctl is-active --quiet docker && compgen -G '/var/lib/docker/containers/*/config.v2.json' >/dev/null; then echo "Existing stopped Docker containers found; preserving them and refusing host changes." >&2; exit 3; fi
  if systemctl is-active --quiet docker; then
    existing_staging_volumes="$(docker volume ls --format '{{.Name}}' | grep -E '^rav_staging_' || true)"
    existing_staging_network="$(docker network ls --format '{{.Name}}' | grep -E '^rav-staging_backend$' || true)"
    if [[ -n "$existing_staging_volumes" || -n "$existing_staging_network" ]]; then
      echo "Existing RAV staging Docker volumes/network found; preserving them and refusing to reuse this host." >&2
      exit 3
    fi
  elif compgen -G '/var/lib/docker/volumes/rav_staging_*' >/dev/null; then
    echo "Existing RAV staging Docker volume data found while Docker is stopped; preserving it and refusing host changes." >&2
    exit 3
  fi
else
  for package in docker.io docker-compose docker-compose-v2 podman-docker containerd runc; do
    status="$(dpkg-query -W -f='${Status}' "$package" 2>/dev/null || true)"
    if [[ "$status" == "install ok installed" ]]; then echo "Conflicting package $package is installed; no package will be removed." >&2; exit 3; fi
  done
  if [[ -e /etc/docker/daemon.json ]] || { [[ -d /var/lib/docker ]] && find /var/lib/docker -mindepth 1 -maxdepth 1 -print -quit | grep -q .; }; then
    echo "Existing Docker configuration or data found without a Docker CLI; preserving it and refusing to install over it." >&2
    exit 3
  fi
fi
if command -v docker >/dev/null && ! docker compose version >/dev/null 2>&1; then
  echo "An existing Docker CLI lacks the Compose plugin; preserving it and stopping for manual review." >&2
  exit 3
fi
if ! command -v docker >/dev/null && { [[ -e /etc/apt/keyrings/docker.asc ]] || [[ -e /etc/apt/sources.list.d/docker.sources ]]; }; then
  echo "Docker repository configuration already exists without a Docker CLI; preserving it and stopping for manual review." >&2
  exit 3
fi
if command -v ufw >/dev/null && ufw status | grep -q 'Status: active'; then
  echo "UFW is already active; preserving its existing policy and stopping before host changes." >&2
  exit 3
fi
if systemctl is-active --quiet firewalld; then
  echo "firewalld is active; preserving its policy and stopping for manual review." >&2
  exit 3
fi

export DEBIAN_FRONTEND=noninteractive
apt-get update
apt-get upgrade -y
apt-get install -y ca-certificates curl gnupg jq openssl ufw iptables sysstat
if ! command -v docker >/dev/null; then
  install -m 0755 -d /etc/apt/keyrings
  curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
  chmod a+r /etc/apt/keyrings/docker.asc
  architecture="$(dpkg --print-architecture)"
  codename="$VERSION_CODENAME"
  printf 'Types: deb\nURIs: https://download.docker.com/linux/ubuntu\nSuites: %s\nComponents: stable\nArchitectures: %s\nSigned-By: /etc/apt/keyrings/docker.asc\n' "$codename" "$architecture" > /etc/apt/sources.list.d/docker.sources
  apt-get update
  apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
fi
docker --version
docker compose version
systemctl enable --now docker

if ! id -u ravadmin >/dev/null 2>&1; then useradd --create-home --shell /bin/bash ravadmin; fi
usermod -aG sudo ravadmin
sudoers_target=/etc/sudoers.d/90-ravadmin
sudoers_temporary="$(mktemp /etc/sudoers.d/.90-ravadmin.XXXXXX)"
cat > "$sudoers_temporary" <<'SUDOERS'
ravadmin ALL=(ALL:ALL) NOPASSWD:ALL
SUDOERS
chmod 0440 "$sudoers_temporary"
if [[ -e "$sudoers_target" || -L "$sudoers_target" ]]; then
  [[ -f "$sudoers_target" && ! -L "$sudoers_target" ]] && cmp -s "$sudoers_temporary" "$sudoers_target" || { rm -f -- "$sudoers_temporary"; echo "Existing ravadmin sudo policy differs; preserving it and stopping." >&2; exit 3; }
  rm -f -- "$sudoers_temporary"
else
  mv -- "$sudoers_temporary" "$sudoers_target"
fi
visudo -cf "$sudoers_target"
install -d -o ravadmin -g ravadmin -m 0700 /home/ravadmin/.ssh
authorized_keys=/home/ravadmin/.ssh/authorized_keys
if [[ -e "$authorized_keys" || -L "$authorized_keys" ]]; then
  [[ -f "$authorized_keys" && ! -L "$authorized_keys" ]] || { echo "Existing authorized_keys is not a regular file; preserving it and stopping." >&2; exit 3; }
  candidate_key="$(cat "$key_file")"
  grep -Fxq -- "$candidate_key" "$authorized_keys" || { echo "Existing ravadmin authorized_keys differs; preserving it. Review and add the requested key manually." >&2; exit 3; }
else
  install -o ravadmin -g ravadmin -m 0600 "$key_file" "$authorized_keys"
fi

if ! id -u rav-staging-server >/dev/null 2>&1; then useradd --system --home-dir /var/lib/rav-staging --create-home --shell /usr/sbin/nologin rav-staging-server; fi
install -d -o root -g root -m 0755 /opt/rav-staging
install -d -o root -g rav-staging-server -m 0750 /opt/rav-staging/unity/releases
install -d -o root -g root -m 0750 /etc/rav-staging
install -d -o root -g rav-staging-server -m 0750 /etc/rav-staging/credentials
install -d -o rav-staging-server -g rav-staging-server -m 0750 /var/lib/rav-staging /var/log/rav-staging
install -d -o root -g root -m 0700 /var/backups/rav-staging/postgres

env_target=/etc/rav-staging/staging.env
if [[ ! -e "$env_target" ]]; then install -o root -g root -m 0600 "$source_root/staging.env.example" "$env_target"; fi
if [[ ! -s /etc/rav-staging/secrets/server_id ]]; then "$source_root/scripts/generate-staging-secrets.sh" /etc/rav-staging/secrets; fi
if [[ ! -e /etc/rav-staging/unity-server.env ]]; then
  server_id="$(cat /etc/rav-staging/secrets/server_id)"
  server_credential="$(cat /etc/rav-staging/secrets/server_credential)"
  api_host="$(sed -n 's/^RAV_STAGING_API_HOST=//p' "$env_target" | tail -n 1)"
  cat > /etc/rav-staging/unity-server.env <<EOF
RAV_API_BASE=https://$api_host
RAV_SERVER_ID=$server_id
RAV_SERVER_CREDENTIAL=$server_credential
RAV_SERVER_REGION=EU-STAGING
RAV_SERVER_ADDRESS=REPLACE_WITH_VPS_PUBLIC_IPV4
RAV_SERVER_PORT=7770
RAV_UTP_SERVER_COMMON_NAME=rav-staging-server
EOF
  chown root:rav-staging-server /etc/rav-staging/unity-server.env
  chmod 0640 /etc/rav-staging/unity-server.env
fi
install -d -o root -g root -m 0700 /etc/rav-staging/credentials
install_managed_file() {
  local source_file="$1" target_file="$2" mode="$3"
  if [[ -e "$target_file" || -L "$target_file" ]]; then
    [[ -f "$target_file" && ! -L "$target_file" ]] && cmp -s "$source_file" "$target_file" || { echo "Existing managed configuration differs ($target_file); preserving it and stopping." >&2; exit 3; }
  else
    install -D -o root -g root -m "$mode" "$source_file" "$target_file"
  fi
}
install_managed_file /etc/rav-staging/secrets/dtls_server.crt /etc/rav-staging/credentials/dtls_server.crt 0600
install_managed_file /etc/rav-staging/secrets/dtls_server.key /etc/rav-staging/credentials/dtls_server.key 0600
install_managed_file /etc/rav-staging/secrets/dtls_ca.crt /etc/rav-staging/credentials/dtls_ca.crt 0600
install -d -o root -g root -m 0755 /opt/rav-staging/unity/client-trust
install_managed_file /etc/rav-staging/secrets/dtls_ca.crt /opt/rav-staging/unity/client-trust/dtls-ca.crt 0644

install_managed_file "$source_root/unity/rav-staging-dedicated-server.service" /etc/systemd/system/rav-staging-dedicated-server.service 0644
install_managed_file "$source_root/systemd/rav-staging-compose.service" /etc/systemd/system/rav-staging-compose.service 0644
install_managed_file "$source_root/systemd/rav-staging-monitor.service" /etc/systemd/system/rav-staging-monitor.service 0644
install_managed_file "$source_root/systemd/rav-staging-monitor.timer" /etc/systemd/system/rav-staging-monitor.timer 0644
install_managed_file "$source_root/unity/rav-staging-server.logrotate" /etc/logrotate.d/rav-staging-server 0644
install_managed_file "$source_root/systemd/docker.service.d/20-rav-staging-forward-guard.conf" /etc/systemd/system/docker.service.d/20-rav-staging-forward-guard.conf 0644
systemctl daemon-reload

mapfile -t ssh_ports < <(sshd -T | awk '$1 == "port" {print $2}' | sort -nu)
if [[ -n "${SSH_CONNECTION:-}" ]]; then read -r _ _ _ detected_ssh_port <<< "$SSH_CONNECTION"; ssh_ports+=("$detected_ssh_port"); fi
if [[ -n "${RAV_SSH_PORT:-}" ]]; then ssh_ports+=("$RAV_SSH_PORT"); fi
mapfile -t ssh_ports < <(printf '%s\n' "${ssh_ports[@]}" | sort -nu)
((${#ssh_ports[@]} > 0)) || { echo "Could not determine SSH listener ports; firewall not changed." >&2; exit 2; }
for ssh_port in "${ssh_ports[@]}"; do
  [[ "$ssh_port" =~ ^[0-9]{1,5}$ ]] && (( ssh_port > 0 && ssh_port <= 65535 )) || { echo "Invalid SSH port; firewall not changed." >&2; exit 2; }
done
for ssh_port in "${ssh_ports[@]}"; do ufw allow "$ssh_port/tcp" comment 'RAV staging SSH'; done
ufw allow 80/tcp comment 'RAV staging HTTP'
ufw allow 443/tcp comment 'RAV staging HTTPS'
ufw allow 443/udp comment 'RAV staging HTTP3'
ufw allow 7770/udp comment 'RAV staging FishNet DTLS'
ufw default deny incoming
ufw default allow outgoing
ufw --force enable
"$source_root/scripts/install-docker-forward-guard.sh"
echo "Ubuntu host preparation completed. Docker and firewall are configured; Compose and Unity services are installed but not enabled or started."
echo "Verify a fresh SSH login as ravadmin before running scripts/harden-ssh.sh."
