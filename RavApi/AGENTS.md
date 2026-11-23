# Project Agent Rules

## Required orientation before coding
- Before editing API endpoints, controllers, services, middleware, authentication, authorization, database, EF Core, background jobs, SignalR, UI, configuration, caching, logging, or tests, read `docs/codex-app/README.md`.
- After reading the index, read every system file in `docs/codex-app/` that matches the user task.
- Use the system files as the first map of where to inspect code, then verify details in the actual source files before changing anything.
- If the user references a file but describes behavior owned elsewhere, follow the system docs and inspect the real owner files before editing.
- If no system file exists for the touched area, create one in `docs/codex-app/` and add it to `docs/codex-app/README.md`.

## Documentation maintenance
- Every behavior change must update the relevant file under `docs/codex-app/` in the same task.
- If a code change moves responsibility between classes, update the responsibility map in the relevant system file.
- If a configuration field is added, renamed, removed, or its meaning changes, update the relevant settings/configuration doc and affected system doc.
- If database schema, EF Core entity mapping, migrations, or seed data changes, update the relevant database/system doc.
- Documentation must be concrete: state current behavior, owner files, important settings, dependencies, and tests.
- Avoid vague notes such as "improved API" or "updated service".
- Keep system docs short enough to scan, but complete enough that future Codex can find the right files before coding.

## Git workflow
- Never run `git commit`, `git revert`, `git reset`, or discard changes automatically.
- The user handles all commit/revert/discard actions manually.
- If rollback/revert/discard is needed, explain what should be done and wait for the user to do it.
- Do not overwrite unrelated user changes.
- If dirty files already exist, work around them unless they conflict with the requested task.

## Code style
- Always use braces for code blocks, even for single-line `if`, `for`, `foreach`, `while`, and `using` blocks.
- Prefer clear explicit code over clever abstractions.
- Avoid reflection in runtime application code unless there is a strong reason.
- Avoid unnecessary LINQ in hot paths.
- Keep dependency injection explicit.
- Do not hide important behavior in extension methods unless it improves clarity.

## ASP.NET Core constraints
- Do not add service registrations blindly. Inspect existing DI patterns first.
- Do not create new middleware, filters, hosted services, or background workers without documenting ownership and execution order.
- Do not change authentication, authorization, CORS, rate limiting, cookies, JWT, or session behavior without updating docs.
- Do not change EF Core entities, migrations, relationships, indexes, query filters, or connection settings without updating docs.
- Do not change public API routes, request/response contracts, status codes, or validation behavior without updating API docs and tests.
- Prefer typed options for configuration.
- Validate required configuration at startup when appropriate.
- Keep logging structured and avoid logging secrets or PII.
- Do not introduce blocking I/O in async request paths.

## Verification workflow
Before final response after code changes, run the relevant checks when available:
- `dotnet build`
- `dotnet test`
- project-specific lint/format commands if present
- API/integration tests if the touched system has them

If a check cannot be run, state why.

## Performance / production diagnostics workflow
When the user reports latency, timeouts, high CPU, memory growth, database slowness, request spikes, failed requests, deadlocks, queue backlog, or production performance issues, do not guess first.

First inspect available diagnostics:
- application logs
- metrics
- tracing/OpenTelemetry setup
- health checks
- database query logs
- background worker logs
- reverse proxy / hosting logs if present

Classify the issue as one of:
- APP_CPU_BOUND
- DATABASE_BOUND
- NETWORK_BOUND
- MEMORY_GC_BOUND
- THREADPOOL_BOUND
- LOCK_CONTENTION
- EXTERNAL_SERVICE_BOUND
- CONFIGURATION_BOUND
- UNKNOWN

Cite concrete evidence before proposing a code change.
