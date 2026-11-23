#!/usr/bin/env bash
set -Eeuo pipefail

secrets_dir="${1:-./secrets}"
mkdir -p "$secrets_dir"
chmod 0700 "$secrets_dir"

for file in postgres_ca.key postgres_ca.crt postgres_server_private_key postgres_server_certificate; do
  if [[ -e "$secrets_dir/$file" ]]; then
    printf 'Refusing to overwrite existing file: %s\n' "$secrets_dir/$file" >&2
    exit 1
  fi
done

work_dir="$(mktemp -d)"
trap 'rm -rf "$work_dir"' EXIT

openssl genrsa -out "$secrets_dir/postgres_ca.key" 4096
openssl req -x509 -new -sha256 -days 1825 \
  -key "$secrets_dir/postgres_ca.key" \
  -out "$secrets_dir/postgres_ca.crt" \
  -subj "/CN=RAV-PostgreSQL-CA"
openssl req -new -newkey rsa:3072 -nodes \
  -keyout "$secrets_dir/postgres_server_private_key" \
  -out "$work_dir/postgres-server.csr" \
  -subj "/CN=postgres"
printf 'subjectAltName=DNS:postgres\nkeyUsage=digitalSignature,keyEncipherment\nextendedKeyUsage=serverAuth\n' > "$work_dir/server.ext"
openssl x509 -req -sha256 -days 825 \
  -in "$work_dir/postgres-server.csr" \
  -CA "$secrets_dir/postgres_ca.crt" \
  -CAkey "$secrets_dir/postgres_ca.key" \
  -CAcreateserial -out "$secrets_dir/postgres_server_certificate" \
  -extfile "$work_dir/server.ext"

chmod 0600 "$secrets_dir/postgres_ca.key"
chmod 0644 "$secrets_dir/postgres_ca.crt" "$secrets_dir/postgres_server_certificate"
sudo chown 999:999 "$secrets_dir/postgres_server_private_key"
sudo chmod 0400 "$secrets_dir/postgres_server_private_key"
printf 'PostgreSQL TLS files created in %s. Keep postgres_ca.key offline after issuance.\n' "$secrets_dir"
