# First VPS setup and staging deployment

This is a deferred execution runbook. Do not run it against production. Current owner-provided state says OVH order #23370514 is still under review and there is no VPS IP or SSH endpoint, so no command below was executed.

## 1. Verify access and copy the bundle

After the service is active and the owner explicitly starts Stage 3B, verify Ubuntu 24.04 x86_64 and the SSH host key fingerprint through the provider console before sending any file. Use the adjacent `.sha256` file to verify the uploaded archive on the VPS. Upload only the public `ravadmin` key; do not upload any private key.

Extract the archive to a temporary directory first and confirm it contains `rav-staging/`. Before installing it, inspect `/opt/rav-staging`. If it already contains any files, stop and review them; do not overwrite an existing installation.

```sh
sha256sum -c RAV-Staging-3B-<UTC>.tar.gz.sha256
mkdir -m 0700 /tmp/rav-staging-unpack
tar -xzf RAV-Staging-3B-<UTC>.tar.gz -C /tmp/rav-staging-unpack
test -s /tmp/rav-staging-unpack/rav-staging/MANIFEST.sha256
(cd /tmp/rav-staging-unpack/rav-staging && sha256sum -c MANIFEST.sha256)
find /tmp/rav-staging-unpack/rav-staging/scripts -maxdepth 1 -type f -name '*.sh' -print0 | xargs -0 -n1 bash -n
```

The final command is a syntax-only check; it does not run the scripts. Stop if it reports an error. For a first install only, create `/opt/rav-staging` if absent and confirm it is empty before copying the verified contents. Do not extract over an existing installation. Preserve the archive and its checksum outside the deployment directory as the recovery source.

```sh
sudo install -d -o root -g root -m 0755 /opt/rav-staging
sudo test -z "$(sudo find /opt/rav-staging -mindepth 1 -maxdepth 1 -print -quit)"
sudo cp -a /tmp/rav-staging-unpack/rav-staging/. /opt/rav-staging/
```

## 2. Prepare Ubuntu and SSH access

From a verified root console or current SSH session, run the bootstrap with the public key for the non-root administrator:

```sh
sudo bash /opt/rav-staging/scripts/prepare-host.sh /path/to/ravadmin-authorized_keys.pub
```

The script requires Ubuntu 24.04 x86_64, checks for conflicting Docker/Podman packages and existing Docker containers, applies security updates, installs Docker Engine/Compose if Docker is absent, creates `ravadmin` and `rav-staging-server`, installs staging service files, configures UFW and a Docker published-port guard. It refuses existing Docker containers, `rav_staging_*` volumes or the `rav-staging_backend` network, conflicting runtime packages, incomplete Docker installations, existing Docker repository/configuration/data without its CLI, active UFW/firewalld policies, existing RAV guard chains, or pre-existing Docker forwarding rules rather than deleting or rewriting them. Existing admin keys, sudo policy, TLS files, or systemd configuration with different contents are preserved and cause a safe stop for review.

Open a separate SSH connection as `ravadmin` and verify sudo before changing SSH policy. Only then run:

```sh
sudo env RAV_SSH_VERIFIED=YES bash /opt/rav-staging/scripts/harden-ssh.sh
```

The hardening script validates `sshd` configuration before reload and leaves the current session open. Verify a second independent key login before ending it. The script refuses to overwrite an existing hardening drop-in.

## 3. Configure isolated staging values

On the VPS, set `/etc/rav-staging/staging.env` to a real HTTPS API hostname with an exact `staging` DNS label and the actual VPS IPv4. The Unity client accepts only an explicitly configured HTTPS staging hostname; the API host must contain a dedicated `staging` label. Keep the file root-owned mode `0600`. Confirm the DNS A record points to that VPS. Review the staging subnet and Caddy proxy IP against all existing Docker networks before creating the Compose network.

Secrets are generated once during host preparation under `/etc/rav-staging/secrets`. The generator refuses to overwrite an existing secrets directory, does not print values, and removes temporary CA signing keys. Keep the resulting secret directory private and outside the bundle/Git. Do not regenerate credentials over an existing staging database without a reviewed rotation plan.

Review `/etc/rav-staging/unity-server.env`: its API base, generated server ID/credential and advertised address must match `staging.env` and the generated secrets. The TLS and DTLS verification settings remain enabled. Install the DTLS CA certificate in the staging client build's trust configuration before trying a client handshake.

Then validate configuration without starting any service:

```sh
sudo /opt/rav-staging/scripts/check-config.sh
sudo /opt/rav-staging/scripts/check-network-subnet.sh
```

## 4. Stage the API stack and Unity server

Only after the owner explicitly authorizes Stage 3B, make a verified VPS snapshot/backup if available, review the exact host and staging values, then run the deployment with all confirmations deliberately provided:

```sh
sudo env RAV_STAGING_DEPLOY_CONFIRM=START_RAV_STAGING_SERVICES \
  RAV_STAGING_MIGRATION_CONFIRM=APPLY_MIGRATIONS_TO_RAV_STAGING \
  RAV_STAGING_SEED_CONFIRM=SEED_RAV_STAGING_ONLY \
  bash /opt/rav-staging/scripts/deploy-staging.sh <release-id>
```

This explicitly builds staging API images, waits for PostgreSQL, creates and validates a pre-migration custom-format database backup, applies the staged EF migrations, seeds staging, then waits for API/Caddy health. It does not contact production. It does not automatically reverse a database schema if application health fails; the pre-migration backup is retained for a separate reviewed recovery.

Deploy the Unity package from the extracted bundle:

```sh
sudo /opt/rav-staging/scripts/deploy-unity-release.sh \
  RAVServer-20261122-staging-hostallowlist \
  /opt/rav-staging/unity/build/RAVServer-20261122-staging-hostallowlist.tar.gz \
  7770 https://<staging-api-host>/health/ready
```

The Unity script rejects unsafe archive paths and special files, verifies required Linux build/runtime files, atomically switches the `current` symlink and restores the prior symlink if the service check fails. It never deletes old releases.

## 5. Smoke checks and observations

After both services are approved and running, execute the write-confirmed staging smoke test. It creates a staging-only test player and match that are not automatically deleted, and tests API liveness/readiness, verified PostgreSQL TLS, UDP socket, server registration/heartbeat, ticket issuance/redemption/replay rejection and match record creation. Its write action is limited to staging.

```sh
sudo env RAV_STAGING_SMOKE_WRITE_CONFIRM=CREATE_TEST_USER_AND_MATCH_IN_RAV_STAGING \
  bash /opt/rav-staging/scripts/smoke-test.sh
```

Read every `SKIP` result. DTLS requires a FishNet-compatible client probe; matchmaking, bot spawning and 20-vehicle load require a Unity gameplay harness. Neither harness is included in this bundle, so those tests remain unverified until provided.

## 6. Rollback

- **API code:** first confirm the prior release is schema-compatible, then run `rollback-staging-api.sh` with `RAV_STAGING_API_ROLLBACK_CONFIRM=ROLLBACK_RAV_STAGING_API` and `RAV_STAGING_DB_ROLLBACK_COMPATIBLE=YES`. It switches only the image tag, leaves PostgreSQL data/schema intact and requires local prior image.
- **Unity:** use `rollback-unity-release.sh <existing-release-id> [7770] [https-health-url]`. It switches the current symlink and reverts the link if health fails. Keep old release directories.
- **Database:** do not automatically roll back migrations. `backup-staging-postgres.sh` creates a TLS-verified custom-format staging dump. Restore goes to a new, randomly named `rav_restore_*` database and never drops/overwrites an existing database. Review data, object counts and compatibility before any later manual promotion or cleanup.
- If health fails, stop the affected staging services, preserve logs and backups, and inspect before further changes. Never point these scripts at production.
