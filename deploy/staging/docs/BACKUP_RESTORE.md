# Staging PostgreSQL backup and restore

These procedures target only the `rav-staging` Compose project and the explicitly named `rav_staging` database. They use verified TLS inside the Docker network. PostgreSQL has no public port.

## Backup

After Stage 3B is authorized and the staging database is running:

```sh
sudo bash /opt/rav-staging/scripts/backup-staging-postgres.sh
```

The script writes a timestamped custom-format dump under `/var/backups/rav-staging/postgres`, mode `0600`, checks its PostgreSQL archive table of contents, and refuses a destination collision. Copy backups to an approved protected off-host location only through a separately reviewed secure process. This prepared bundle does not configure an external backup service.

## Non-destructive restore test

Run the restore only against an approved staging backup:

```sh
sudo env RAV_STAGING_RESTORE_CONFIRM=RESTORE_TO_NEW_RAV_STAGING_DB \
  bash /opt/rav-staging/scripts/restore-staging-postgres.sh \
  /var/backups/rav-staging/postgres/<backup>.dump
```

The script creates a new randomly named `rav_restore_*` database using the bootstrap role, then restores into that new database using the staging migrator role. It never targets `rav_staging`, drops a database, deletes an existing object, or runs EF migrations. If restore fails, the partial test database is retained for inspection; cleanup is a separate manual action after review.

After restoration, record the test database name, compare expected table/row counts and inspect migration history. Do not direct API traffic to the restore test database. Data Protection and TLS/DTLS private credentials are not database backup contents and must be protected separately.
