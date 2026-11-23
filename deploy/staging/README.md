# RAV staging deployment bundle

This directory is the source for an isolated RAV staging bundle. It contains the Compose stack, API source snapshot, Ubuntu 24.04 bootstrap, generated-at-provisioning secret workflow, systemd units, Unity release package, smoke checks, rollback scripts and runbooks.

## Safety and current state

- This is **staging-only**. The Compose project, network, volumes, users, database and credentials use `rav-staging` names.
- PostgreSQL has no published host port and sits on an internal Docker network. Network database connections require TLS certificate verification.
- The bundle contains templates, not real credentials. `generate-staging-secrets.sh` creates random credentials and staging-only certificates on the VPS; never copy production secrets here.
- Compose profiles for migration and seed are explicit. Normal service startup does not run migrations or seed data.
- No VPS deployment is performed by packaging or local validation. Stage 3B runtime is **NOT STARTED**.
- Full first-run procedure: [`docs/FIRST_DEPLOY.md`](docs/FIRST_DEPLOY.md).

## Build and verify the bundle

Run PowerShell from this directory:

```powershell
./package-staging-bundle.ps1
```

The packager checks the source Unity build against the staged Unity copy, creates the release tar expected by `deploy-unity-release.sh`, extracts and hashes it, builds a self-contained top-level `rav-staging/` archive, verifies its internal SHA-256 manifest, and writes an adjacent archive checksum. It refuses an output collision and leaves prior artifacts untouched.

The archive is created under `artifacts/`. Its environment file is a template with `.invalid` host and IP placeholders; fill real staging values only on the verified VPS. No database, server credential, private key, certificate private key, or production secret is included.

## Validation limits

Local checks validate Compose syntax and package integrity without starting containers. Linux service execution, Docker image builds, PostgreSQL TLS, HTTPS issuance, Ubuntu bootstrap, Unity ELF startup, UDP 7770, FishNet DTLS, API registration/heartbeat and gameplay tests require the provisioned staging VPS and are deferred. The smoke script reports DTLS and game-client harness checks as `SKIP` when those dedicated test clients are absent; a skip is not a pass.
