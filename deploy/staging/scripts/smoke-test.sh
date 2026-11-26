#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
script_dir="$(cd -- "$(dirname -- "$0")" && pwd)"
source "$script_dir/lib.sh"
if [[ "${RAV_STAGING_SMOKE_WRITE_CONFIRM:-}" != "CREATE_TEST_USER_AND_MATCH_IN_RAV_STAGING" ]]; then
  echo "Set RAV_STAGING_SMOKE_WRITE_CONFIRM=CREATE_TEST_USER_AND_MATCH_IN_RAV_STAGING only after verifying the isolated staging host; this test registers the server and creates a disposable test player/match." >&2
  exit 2
fi
command -v curl >/dev/null && command -v jq >/dev/null && command -v python3 >/dev/null || { echo "Install curl, jq, and python3 before smoke testing." >&2; exit 2; }
api_base="https://$RAV_STAGING_API_HOST"
[[ "$RAV_STAGING_API_HOST" != "war-of-machines-api.onrender.com" ]] || { echo "Refusing the known production API host." >&2; exit 2; }
[[ "$RAV_STAGING_API_HOST" != *".invalid" ]] || { echo "Set a real staging DNS hostname before smoke testing." >&2; exit 2; }
for endpoint in health/live health/ready; do
  status="$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' --connect-timeout 5 --max-time 10 "$api_base/$endpoint")"
  [[ "$status" == 200 ]] || { echo "$endpoint returned HTTP $status." >&2; exit 1; }
  echo "PASS HTTPS $endpoint"
done
rav_compose exec -T postgres sh -eu -c '
  pg_isready -U rav_staging_bootstrap -d rav_staging >/dev/null
  export PGPASSWORD="$(cat /run/secrets/db_bootstrap_password)"
  ssl="$(psql "host=postgres port=5432 dbname=$POSTGRES_DB user=$POSTGRES_USER sslmode=verify-full sslrootcert=/var/lib/postgresql/tls/ca.crt" -Atq --set=ON_ERROR_STOP=1 --command="SELECT ssl FROM pg_stat_ssl WHERE pid=pg_backend_pid()")"
  [ "$ssl" = "t" ]
'
echo "PASS PostgreSQL connectivity with TLS certificate verification"
systemctl is-active --quiet rav-staging-dedicated-server.service
ss -H -lun "sport = :7770" | grep -q .
echo "PASS Unity systemd service and UDP 7770 socket"

server_id="$(tr -d '\r\n' < "$RAV_STAGING_SECRETS_DIR/server_id")"
server_credential="$(tr -d '\r\n' < "$RAV_STAGING_SECRETS_DIR/server_credential")"
bootstrap_user="$(staging_env_value RAV_STAGING_BOOTSTRAP_USER)"
bootstrap_user="${bootstrap_user:-rav_staging_bootstrap}"
[[ "$bootstrap_user" =~ ^[A-Za-z_][A-Za-z0-9_]*$ ]] || { echo "Invalid staging PostgreSQL bootstrap username." >&2; exit 2; }
server_address="$(staging_env_value RAV_STAGING_SERVER_ADDRESS)"
server_region="$(staging_env_value RAV_STAGING_REGION)"
if [[ -z "$server_address" || "$server_address" == "REPLACE_WITH_VPS_PUBLIC_IPV4" ]]; then echo "Set the actual VPS public IPv4 in staging.env before smoke testing." >&2; exit 2; fi
tmp="$(mktemp -d)"
username=""
user_id=""
match_id=""
cleanup() {
  local exit_code="$?"
  trap - EXIT
  if [[ -n "$username" && "$username" =~ ^rav-smoke-[0-9]{14}-[0-9]+$ ]]; then
    local cleanup_sql="BEGIN;
CREATE TEMP TABLE rav_smoke_test_user_ids ON COMMIT DROP AS
SELECT \"Id\" FROM \"Players\" WHERE \"Username\" = '$username';
CREATE TEMP TABLE rav_smoke_test_match_ids ON COMMIT DROP AS
SELECT DISTINCT m.\"Id\" FROM \"Matches\" m
INNER JOIN \"MatchParticipants\" mp ON mp.\"MatchId\" = m.\"Id\"
WHERE mp.\"UserId\" IN (SELECT \"Id\" FROM rav_smoke_test_user_ids);
DELETE FROM \"MatchParticipants\" WHERE \"MatchId\" IN (SELECT \"Id\" FROM rav_smoke_test_match_ids);
DELETE FROM \"Matches\" WHERE \"Id\" IN (SELECT \"Id\" FROM rav_smoke_test_match_ids);
DELETE FROM \"MatchParticipants\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Players\" WHERE \"Username\" = '$username');
DELETE FROM \"ConnectionTickets\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Players\" WHERE \"Username\" = '$username');
DELETE FROM \"UserSessions\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Players\" WHERE \"Username\" = '$username');
DELETE FROM \"UserVehicleResearches\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Players\" WHERE \"Username\" = '$username');
DELETE FROM \"UserVehicles\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"Players\" WHERE \"Username\" = '$username');
DELETE FROM \"Players\" WHERE \"Username\" = '$username';
COMMIT;"
    if rav_compose exec -T postgres psql --username="$bootstrap_user" --dbname="$RAV_STAGING_DB" --set=ON_ERROR_STOP=1 --command="$cleanup_sql" >/dev/null; then
      echo "PASS temporary smoke-test player and match rows removed"
    else
      echo "WARN temporary smoke-test cleanup failed; inspect staging test rows before owner access." >&2
      exit_code=1
    fi
  fi
  rm -rf -- "$tmp"
  exit "$exit_code"
}
trap cleanup EXIT
printf 'header = "X-Server-Id: %s"\nheader = "X-Server-Credential: %s"\n' "$server_id" "$server_credential" > "$tmp/server-auth.cfg"
chmod 0600 "$tmp/server-auth.cfg"
registration="$(jq -n --arg region "$server_region" --arg address "$server_address" '{region:$region,address:$address,port:7770,maxPlayers:20}')"
status="$(curl --silent --show-error --output "$tmp/register.json" --write-out '%{http_code}' --config "$tmp/server-auth.cfg" --header 'Content-Type: application/json' --data "$registration" "$api_base/unity-server/register")"
[[ "$status" == 200 ]] || { echo "Dedicated-server registration returned HTTP $status." >&2; exit 1; }
heartbeat="$(jq -n --arg region "$server_region" --arg address "$server_address" '{region:$region,address:$address,port:7770,playersOnline:0,maxPlayers:20,activeMatches:0,status:"online",message:"staging-smoke"}')"
status="$(curl --silent --show-error --output "$tmp/heartbeat.json" --write-out '%{http_code}' --config "$tmp/server-auth.cfg" --request PUT --header 'Content-Type: application/json' --data "$heartbeat" "$api_base/unity-server/$server_id/heartbeat")"
[[ "$status" == 200 ]] || { echo "Dedicated-server heartbeat returned HTTP $status." >&2; exit 1; }
curl --fail --silent --show-error "$api_base/unity-server/status" -o "$tmp/server-status.json"
jq -e --arg id "$server_id" '.servers | any(.serverId == $id and .isOnline == true)' "$tmp/server-status.json" >/dev/null
echo "PASS server registration, heartbeat, and public status"

username="rav-smoke-$(date -u +%Y%m%d%H%M%S)-$RANDOM"
password="$(openssl rand -base64 32 | tr -d '\n')"
register_body="$(jq -n --arg username "$username" --arg password "$password" '{username:$username,password:$password}')"
status="$(curl --silent --show-error --output "$tmp/auth.json" --write-out '%{http_code}' --header 'Content-Type: application/json' --data "$register_body" "$api_base/auth/register")"
[[ "$status" == 200 ]] || { echo "Staging test-player registration returned HTTP $status." >&2; exit 1; }
access_token="$(jq -er '.token' "$tmp/auth.json")"
login_body="$(jq -n --arg username "$username" --arg password "$password" '{username:$username,password:$password}')"
status="$(curl --silent --show-error --output "$tmp/login.json" --write-out '%{http_code}' --header 'Content-Type: application/json' --data "$login_body" "$api_base/auth/login")"
[[ "$status" == 200 ]] || { echo "Staging test-player login returned HTTP $status." >&2; exit 1; }
access_token="$(jq -er '.token' "$tmp/login.json")"
printf 'header = "Authorization: Bearer %s"\n' "$access_token" > "$tmp/player-auth.cfg"
chmod 0600 "$tmp/player-auth.cfg"
user_id="$(printf '%s' "$access_token" | python3 -c 'import base64,sys; s=sys.stdin.read().split(".")[1]; print(base64.urlsafe_b64decode(s + "="*((4-len(s)%4)%4)).decode())' | jq -er '."http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" // .nameid // .sub')"
[[ "$user_id" =~ ^[0-9]+$ ]] || { echo "Could not identify the staging test player for cleanup." >&2; exit 1; }
curl --fail --silent --show-error --config "$tmp/player-auth.cfg" "$api_base/players/me" -o "$tmp/profile.json"
jq -e '.bolts == 1000 and .freeXp == 500 and .adamant == 0 and .activeVehicleCode == "ia_l1_starter" and any(.ownedVehicles[]; .code == "ia_l1_starter" and .isActive == true and .isResearched == true)' "$tmp/profile.json" >/dev/null
echo "PASS player registration/login, JWT profile, starting resources, and IA starter vehicle"
ticket_request="$(jq -n --arg id "$server_id" '{serverId:$id}')"
status="$(curl --silent --show-error --output "$tmp/ticket.json" --write-out '%{http_code}' --config "$tmp/player-auth.cfg" --header 'Content-Type: application/json' --data "$ticket_request" "$api_base/connection-tickets")"
[[ "$status" == 200 ]] || { echo "Connection-ticket issue returned HTTP $status." >&2; exit 1; }
ticket="$(jq -er '.ticket' "$tmp/ticket.json")"
redeem_body="$(jq -n --arg ticket "$ticket" '{ticket:$ticket}')"
status="$(curl --silent --show-error --output "$tmp/redeemed.json" --write-out '%{http_code}' --config "$tmp/server-auth.cfg" --header 'Content-Type: application/json' --data "$redeem_body" "$api_base/connection-tickets/redeem")"
[[ "$status" == 200 ]] && jq -e '.accepted == true' "$tmp/redeemed.json" >/dev/null || { echo "Connection-ticket redemption failed (HTTP $status)." >&2; exit 1; }
status="$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' --config "$tmp/server-auth.cfg" --header 'Content-Type: application/json' --data "$redeem_body" "$api_base/connection-tickets/redeem")"
[[ "$status" == 401 ]] || { echo "One-time ticket replay check expected HTTP 401, got $status." >&2; exit 1; }
echo "PASS authentication, ticket issue/redemption, and replay rejection"

curl --fail --silent --show-error --config "$tmp/player-auth.cfg" "$api_base/user-vehicles/me" -o "$tmp/vehicles.json"
vehicle_id="$(jq -er '.vehicles[0].vehicleId' "$tmp/vehicles.json")"
match_body="$(jq -n --argjson userId "$user_id" --argjson vehicleId "$vehicle_id" '{map:"demo_map",participants:[{userId:$userId,vehicleId:$vehicleId,team:1}]}')"
status="$(curl --silent --show-error --output "$tmp/match.json" --write-out '%{http_code}' --config "$tmp/server-auth.cfg" --header 'Content-Type: application/json' --data "$match_body" "$api_base/matches/start")"
[[ "$status" == 200 ]] && jq -e '.matchId > 0' "$tmp/match.json" >/dev/null || { echo "Backend match-record creation failed (HTTP $status)." >&2; exit 1; }
match_id="$(jq -er '.matchId' "$tmp/match.json")"
[[ "$match_id" =~ ^[0-9]+$ ]] || { echo "Could not identify the staging test match for cleanup." >&2; exit 1; }
echo "PASS staging backend match-record creation"

dtls_probe="$(staging_env_value RAV_STAGING_DTLS_PROBE)"
if [[ -n "$dtls_probe" && -x "$dtls_probe" ]]; then
  "$dtls_probe" --host "$server_address" --port 7770 --ca "$RAV_STAGING_SECRETS_DIR/dtls_ca.crt" --server-name rav-staging-server
  echo "PASS DTLS probe"
else
  echo "SKIP DTLS handshake: no verified FishNet-compatible client probe is bundled."
fi
gameplay_harness="$(staging_env_value RAV_STAGING_GAMEPLAY_HARNESS)"
if [[ -n "$gameplay_harness" && -x "$gameplay_harness" ]]; then
  "$gameplay_harness" --api-base "$api_base" --server-address "$server_address" --server-port 7770 --vehicles 20 --bots
  echo "PASS 20-unit client matchmaking/bot gameplay harness"
else
  echo "SKIP matchmaking/bot spawning/20-unit simulation: no automated Unity client harness is present."
fi
echo "Backend staging smoke checks completed. Review the explicit SKIP lines; they are not passing tests."
