# BlijvenLeren MVP

This repository contains my implementation of the BlijvenLeren programming case.

The goal of this project is not to fully productize the platform, but to demonstrate:
- pragmatic MVP scoping
- application and security design
- reviewable technical decisions
- local runtime and CI validation

## Review guide

If you want the fastest review path, start here:
- [Reviewer walkthrough](docs/reviewer-walkthrough.md) for a short guided demo
- [Functional requirements](docs/functional-requirements.md) for requirement coverage
- [Traceability map](docs/traceability-map.md) for where to find code and tests
- [Architecture](docs/architecture.md) for runtime, auth, and moderation structure
- [Testing strategy](docs/testing-strategy.md) for automated versus manual validation
- [Security](docs/security.md) for demo shortcuts and safeguards
- [Technical decisions](docs/technical-decisions.md) for key trade-offs
- [Scope and assumptions](docs/scope-and-assumptions.md) for explicit boundaries
- [Backlog](docs/backlog.md) for deferred work and impact

Current automation note:
- pull requests get a hosted GitHub Actions Release build-and-test check
- compose runtime behavior and the real OIDC login redirect remain manual review paths

## Quick start

### Prerequisites

- .NET SDK 10
- Docker Desktop

### Supported local runtime

The primary supported local runtime is the full compose stack:

```bash
docker compose up --build
```

Then open:
- `http://localhost:8080/` for the landing page
- `http://localhost:8080/LearningResources` for the main browser flow
- `http://localhost:8080/docs` for the interactive API docs
- `http://localhost:8080/api/health/dependencies` for the dependency probe
- `http://localhost:8081/` for the Keycloak admin console

Compose brings up:
- `app` on `http://localhost:8080`
- `db` on `localhost:5432`
- `idp` on `http://localhost:8081`

Demo accounts:
- internal user: `internal.demo / Passw0rd!`
- fallback external contributor: `external.demo / Passw0rd!`

Social login note:
- Keycloak now includes placeholder `GitHub` and `Google` social providers
- `GitHub` is the preferred external-user path in the app when you want a brokered login review
- these providers do not work out of the box because the checked-in realm import uses dummy client credentials
- to make them work, open the Keycloak admin console at `http://localhost:8081/`, sign in with `admin / admin`, open the `blijvenleren` realm, go to `Identity providers`, and replace the dummy client ID and client secret values
- until you do that, use `external.demo` as the fallback external reviewer account

If you want a guided review after startup, use the [reviewer walkthrough](docs/reviewer-walkthrough.md).

### Advanced app-only startup

You can run the ASP.NET Core app directly, but that is not the main review path:

```bash
dotnet run --no-launch-profile --project src/BlijvenLeren.App/BlijvenLeren.App.csproj --urls http://127.0.0.1:5078
```

Important:
- this assumes PostgreSQL is already available on `localhost:5432`
- this assumes Keycloak is already available on `localhost:8081`
- the checked-in app configuration points to those dependencies by default
- without those services, browser data flows, login, and dependency checks will not work correctly

Use this path only if you deliberately want to run the app outside compose with equivalent local infrastructure or overridden configuration.

## Build and test

Build the solution:

```bash
dotnet build BlijvenLeren.sln
```

Run the full automated suite:

```bash
dotnet test BlijvenLeren.sln -c Release
```

Run the browser smoke path only:

```bash
dotnet test test/BlijvenLeren.App.Tests/BlijvenLeren.App.Tests.csproj -c Release --filter FullyQualifiedName~BrowserSmoke
```

Hosted validation:
- GitHub Actions runs the same Release build-and-test path for pull requests and pushes to `main`

For deeper testing detail, see the [testing strategy](docs/testing-strategy.md).

## MVP summary

The current MVP includes:
- one ASP.NET Core app hosting Razor Pages and minimal APIs
- PostgreSQL persistence with compose-time migrations and demo-data seeding
- local Keycloak-based browser sign-in and bearer-token API auth
- prewired social-login broker placeholders in Keycloak for preferred external-user sign-in
- learning-resource CRUD, comments, moderation, health checks, and local API docs

Current intentional deferrals:
- cloud deployment and Terraform
- production hardening
- richer list ergonomics such as filtering, sorting, and pagination

See the [scope and assumptions](docs/scope-and-assumptions.md) and [backlog](docs/backlog.md) for the explicit boundaries and follow-up work.

## Notes

- If `docker compose up --build` fails on image pulls, retry after Docker Desktop is fully ready.
- If ports `8080`, `8081`, or `5432` are already in use, adjust the local port mappings in `compose.yaml`.
- If login fails immediately after startup, wait for the Keycloak realm import to finish and retry.
- If `GitHub` or `Google` login fails immediately, check the Keycloak `Identity providers` credentials first; the checked-in values are placeholders only.
- If the app starts before `db` or `idp` is ready, recheck `http://localhost:8080/api/health/dependencies` after a few seconds.

## Review process

Work is tracked through GitHub issues and reviewed through pull requests so interviewers can inspect:
- change scope
- commit history
- rationale in PR descriptions
