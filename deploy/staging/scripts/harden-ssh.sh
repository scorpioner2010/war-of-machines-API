#!/usr/bin/env bash
set -Eeuo pipefail
if [[ "$(id -u)" -ne 0 || "${RAV_SSH_VERIFIED:-}" != "YES" ]]; then echo "Run as root with RAV_SSH_VERIFIED=YES only after opening a separate successful ravadmin SSH session." >&2; exit 2; fi
[[ -s /home/ravadmin/.ssh/authorized_keys ]] || { echo "ravadmin authorized_keys is missing." >&2; exit 2; }
dropin=/etc/ssh/sshd_config.d/90-rav-staging-hardening.conf
[[ ! -e "$dropin" ]] || { echo "SSH hardening drop-in already exists; review it manually rather than overwrite." >&2; exit 1; }
temporary="$(mktemp /etc/ssh/sshd_config.d/.90-rav-staging.XXXXXX)"
trap 'rm -f -- "$temporary"' EXIT
cat > "$temporary" <<'CONF'
PermitRootLogin no
PasswordAuthentication no
KbdInteractiveAuthentication no
PubkeyAuthentication yes
AuthenticationMethods publickey
AllowUsers ravadmin
CONF
chmod 0600 "$temporary"
mv -- "$temporary" "$dropin"
trap - EXIT
if ! sshd -t; then rm -f -- "$dropin"; echo "sshd config validation failed; drop-in removed." >&2; exit 1; fi
systemctl reload ssh
echo "SSH key-only hardening applied. Verify a second new ravadmin key login before ending the existing session."
