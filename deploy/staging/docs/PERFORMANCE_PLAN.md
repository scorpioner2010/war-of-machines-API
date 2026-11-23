# Staging performance plan: one match, 20 vehicles

This bundle prepares host-level telemetry; it does not include a Unity client swarm, bot-driving scenario, or FishNet load generator. A 20-vehicle performance result is therefore **NOT TESTED**.

## Required harness

Provide a staging-only Unity client harness able to:

1. Connect with a client build that trusts the staging DTLS CA and uses a valid staging ticket.
2. Start exactly one match and join 20 controlled vehicles/bots on the intended map.
3. Keep a fixed scenario for at least 10 minutes, then exit cleanly.
4. Record client count, packet loss/RTT, simulation/tick rate, server CPU/memory, GC pauses, network throughput, API latency and server logs with UTC timestamps.

The harness should receive the staging API/server endpoint and vehicle count as explicit arguments, fail closed on certificate validation or authentication errors, and never call production endpoints.

## Execution after authorization

First run one client, then 5, 10 and 20 clients to identify where latency or tick rate degrades. For each step, capture API `/health/live` and `/health/ready`, UDP 7770 socket state, `journalctl -u rav-staging-dedicated-server`, `pidstat` and `ss` before/during/after. The provided `performance-sample.sh [seconds]` records host CPU, memory, I/O, sockets and Unity logs only; it does not create clients or measure in-game tick rate.

Do not raise player caps or change gameplay/network settings just to make a test pass. Record build ID, map, client version, staging region, Unity build hash and harness version with each run.
