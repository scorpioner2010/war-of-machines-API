#!/bin/sh
set -eu

read_secret() {
    secret_path="/run/secrets/$1"
    if [ ! -s "$secret_path" ]; then
        printf '%s\n' "Required staging secret is unavailable." >&2
        exit 78
    fi
    cat "$secret_path"
}

# Compose secrets are files, not environment variables. Load them only in the
# process environment consumed by ASP.NET Core; never print their values.
export DATABASE_URL="$(read_secret DATABASE_URL)"
export Jwt__Key="$(read_secret Jwt__Key)"
export Jwt__Issuer="$(read_secret Jwt__Issuer)"
export Jwt__Audience="$(read_secret Jwt__Audience)"
export DedicatedServers__Provisioned__0__ServerId="$(read_secret DedicatedServers__Provisioned__0__ServerId)"
export DedicatedServers__Provisioned__0__Credential="$(read_secret DedicatedServers__Provisioned__0__Credential)"

if [ "${ASPNETCORE_ENVIRONMENT:-Production}" = "Production" ]; then
    export DataProtection__CertificatePassword="$(read_secret DataProtection__CertificatePassword | tr -d '\r\n')"
    export DataProtection__CertificatePath="/run/credentials/data-protection.pfx"
    test -s "$DataProtection__CertificatePath"
fi

exec dotnet RavApi.dll
