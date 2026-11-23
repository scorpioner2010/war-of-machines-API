#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
output_dir="${1:-/etc/rav-staging/secrets}"
if [[ "$output_dir" != /* ]]; then echo "Use an absolute secrets path." >&2; exit 2; fi
if [[ "$output_dir" == "/etc/rav-staging/secrets" && "$(id -u)" -ne 0 ]]; then echo "Run provisioning as root to create host secrets." >&2; exit 2; fi
if [[ -e "$output_dir" || -L "$output_dir" ]]; then echo "Secrets directory already exists; refusing to overwrite or rotate credentials." >&2; exit 1; fi
parent="$(dirname -- "$output_dir")"
install -d -m 0700 "$parent"
temporary="$(mktemp -d "$parent/.rav-staging-secrets.XXXXXX")"
trap 'rm -rf -- "$temporary"' EXIT
chmod 0700 "$temporary"
secret() { local name="$1" value="$2"; printf '%s' "$value" > "$temporary/$name"; chmod 0444 "$temporary/$name"; }
random_token() { openssl rand -base64 "$1" | tr '/+' '_-' | tr -d '=\n'; }
db_bootstrap="$(random_token 48)"; db_runtime="$(random_token 48)"; db_migrator="$(random_token 48)"
server_credential="$(random_token 48)"; jwt_key="$(random_token 64)"; dp_password="$(random_token 36)"
server_id="rav-staging-$(openssl rand -hex 6)"
secret db_bootstrap_password "$db_bootstrap"
secret db_runtime_password "$db_runtime"
secret db_migrator_password "$db_migrator"
secret server_id "$server_id"
secret server_credential "$server_credential"
secret jwt_key "$jwt_key"
secret jwt_issuer 'RAV.Staging.Api'
secret jwt_audience 'RAV.Staging.Client'
secret data_protection_password "$dp_password"
secret api_database_url "Host=postgres;Port=5432;Username=rav_staging_runtime;Password=$db_runtime;Database=rav_staging;SSL Mode=VerifyFull;Root Certificate=/run/secrets/postgres-ca.crt;"
secret migration_database_url "Host=postgres;Port=5432;Username=rav_staging_migrator;Password=$db_migrator;Database=rav_staging;SSL Mode=VerifyFull;Root Certificate=/run/secrets/postgres-ca.crt;"

openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:3072 -out "$temporary/postgres_ca.key" >/dev/null 2>&1
openssl req -x509 -new -sha256 -days 3650 -key "$temporary/postgres_ca.key" -out "$temporary/postgres_ca.crt" -subj '/CN=RAV Staging PostgreSQL CA' -addext 'basicConstraints=critical,CA:TRUE' -addext 'keyUsage=critical,keyCertSign,cRLSign'
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:3072 -out "$temporary/postgres_server.key" >/dev/null 2>&1
openssl req -new -sha256 -key "$temporary/postgres_server.key" -out "$temporary/postgres_server.csr" -subj '/CN=postgres'
cat > "$temporary/postgres_server.ext" <<'EXT'
basicConstraints=critical,CA:FALSE
keyUsage=critical,digitalSignature,keyEncipherment
extendedKeyUsage=serverAuth
subjectAltName=DNS:postgres
EXT
openssl x509 -req -sha256 -days 825 -in "$temporary/postgres_server.csr" -CA "$temporary/postgres_ca.crt" -CAkey "$temporary/postgres_ca.key" -CAcreateserial -out "$temporary/postgres_server.crt" -extfile "$temporary/postgres_server.ext" >/dev/null 2>&1
openssl verify -CAfile "$temporary/postgres_ca.crt" -verify_hostname postgres "$temporary/postgres_server.crt" >/dev/null

openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:3072 -out "$temporary/dtls_ca.key" >/dev/null 2>&1
openssl req -x509 -new -sha256 -days 3650 -key "$temporary/dtls_ca.key" -out "$temporary/dtls_ca.crt" -subj '/CN=RAV Staging DTLS CA' -addext 'basicConstraints=critical,CA:TRUE' -addext 'keyUsage=critical,keyCertSign,cRLSign'
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:3072 -out "$temporary/dtls_server.key" >/dev/null 2>&1
openssl req -new -sha256 -key "$temporary/dtls_server.key" -out "$temporary/dtls_server.csr" -subj '/CN=rav-staging-server'
cat > "$temporary/dtls_server.ext" <<'EXT'
basicConstraints=critical,CA:FALSE
keyUsage=critical,digitalSignature,keyEncipherment
extendedKeyUsage=serverAuth
subjectAltName=DNS:rav-staging-server
EXT
openssl x509 -req -sha256 -days 825 -in "$temporary/dtls_server.csr" -CA "$temporary/dtls_ca.crt" -CAkey "$temporary/dtls_ca.key" -CAcreateserial -out "$temporary/dtls_server.crt" -extfile "$temporary/dtls_server.ext" >/dev/null 2>&1
openssl verify -CAfile "$temporary/dtls_ca.crt" -verify_hostname rav-staging-server "$temporary/dtls_server.crt" >/dev/null

openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:3072 -out "$temporary/dp.key" >/dev/null 2>&1
openssl req -x509 -new -sha256 -days 1825 -key "$temporary/dp.key" -out "$temporary/dp.crt" -subj '/CN=RAV Staging Data Protection'
openssl pkcs12 -export -out "$temporary/data_protection.pfx" -inkey "$temporary/dp.key" -in "$temporary/dp.crt" -passout "pass:$dp_password" >/dev/null 2>&1
openssl pkcs12 -in "$temporary/data_protection.pfx" -noout -passin "pass:$dp_password" >/dev/null 2>&1
rm -f -- "$temporary/postgres_ca.key" "$temporary/postgres_server.csr" "$temporary/postgres_server.ext" "$temporary/postgres_ca.srl" "$temporary/dtls_ca.key" "$temporary/dtls_server.csr" "$temporary/dtls_server.ext" "$temporary/dtls_ca.srl" "$temporary/dp.key" "$temporary/dp.crt"
for filename in postgres_ca.crt postgres_server.key postgres_server.crt dtls_ca.crt dtls_server.key dtls_server.crt data_protection.pfx; do chmod 0444 "$temporary/$filename"; done
if [[ "$(id -u)" -eq 0 ]]; then chown -R root:root "$temporary"; fi
mv -- "$temporary" "$output_dir"
trap - EXIT
echo "Generated isolated RAV staging credentials and certificates in $output_dir (values are not printed)."
