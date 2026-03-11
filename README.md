# BlijvenLeren MVP

This repository contains my implementation of the BlijvenLeren programming case.

The goal of this project is not to fully productize the platform, but to demonstrate how I approach:
- application design
- DevOps practices
- security-conscious development
- documentation and technical decision making

## Assignment intent

The original brief is intentionally open-ended and expects pragmatic choices rather than a perfect or complete implementation. This repository therefore focuses on:
- a working MVP where feasible
- clear technical trade-offs
- traceable documentation of functional requirements and architectural decisions
- a local, containerized setup for application, database and identity provider
- diagrams that explain the architecture and authentication model

## What is included

- .NET application code
- Combined browser UI and API in one ASP.NET Core app for the MVP
- Persistent database
- Identity provider for authentication
- Containerized local runtime
- Documentation for requirements, architecture, security, testing and decisions
- Diagrams for system context, containers and authentication

## What is intentionally out of scope

- Deployment to a real cloud environment
- Infrastructure provisioning and deployment automation (including Terraform)
- Full production hardening
- Full implementation of every possible edge case
- Enterprise-grade social login integration beyond what is needed to demonstrate the direction

See `docs/scope-and-assumptions.md` for details.

## Quick start

### Prerequisites

- .NET SDK 10
- Docker Desktop

### Run locally

```bash
dotnet run --no-launch-profile --project src/BlijvenLeren.App/BlijvenLeren.App.csproj --urls http://127.0.0.1:5078
```

Then open:
- `http://127.0.0.1:5078/` for the placeholder browser page
- `http://127.0.0.1:5078/api/health` for the placeholder API health endpoint

Local URL note:
- the checked-in launch profile also uses port `5078`
- the compose runtime uses port `8080`
- if you see `http://localhost:5114`, you are not on the compose-hosted app

You can also build the current solution with:

```bash
dotnet build BlijvenLeren.sln
```

### Run the container runtime

```bash
docker compose up --build
```

Container ports:
- app: `http://localhost:8080`
- db: `localhost:5432`
- idp: `http://localhost:8081`

Useful endpoints after startup:
- app landing page: `http://localhost:8080/`
- app health: `http://localhost:8080/api/health`
- dependency probe: `http://localhost:8080/api/health/dependencies`
- versioned list route: `GET http://localhost:8080/api/v1/learning-resources`
- versioned detail route: `GET http://localhost:8080/api/v1/learning-resources/{id}`
- versioned create route: `POST http://localhost:8080/api/v1/learning-resources`
- persistence smoke path: `POST http://localhost:8080/api/health/persistence-smoke`
- demo data reseed path: `POST http://localhost:8080/api/demo/seed-data?reset=true`
- current-user route: `GET http://localhost:8080/api/auth/me`
- internal-only route: `GET http://localhost:8080/api/auth/internal`
- external-only route: `GET http://localhost:8080/api/auth/external`
- Keycloak admin console: `http://localhost:8081/` with `admin` / `admin`

## Current bootstrap scope

Issue `#4` establishes a runnable starting point rather than full feature coverage. The current implementation:
- uses one ASP.NET Core project to host both Razor Pages and minimal API endpoints
- provides a placeholder landing page for the browser experience
- exposes a placeholder health endpoint for API connectivity checks
- intentionally leaves domain logic, persistence, authentication and containers for follow-up issues

Issue `#5` adds the first container runtime around that bootstrap:
- `app` hosts both the browser UI and the API surface
- `db` provides a PostgreSQL instance for later persistence work
- `idp` provides a local Keycloak instance for later authentication work
- the application exposes a dependency probe so container-network reachability is visible during review

Issue `#6` adds the first persistence slice:
- EF Core with PostgreSQL for the initial relational schema
- tables for learning resources and moderation-ready comments
- an initial migration under `src/BlijvenLeren.App/Data/Migrations`
- a persistence smoke endpoint that writes and reads temporary data inside a transaction

Issue `#18` adds predictable demo data:
- three learning resources with review-friendly titles and descriptions
- internal comments that are immediately visible
- external comments in `Pending` and `Rejected` moderation states
- a reseed endpoint for resetting the local walkthrough data

Issue `#7` adds the first authentication slice:
- a local Keycloak realm import with `internal-user` and `external-contributor` roles
- one test account per role
- browser login through the app against the local identity provider
- protected routes and role-gated API endpoints
- bearer-token validation for API access

Issue `#8` adds the first domain-contract slice:
- versioned request/response contracts under `Contracts/V1`
- list and detail responses for learning resources and comments
- an internal-only create route with basic validation errors
- mapping and validation tests for the new contract baseline

### Database migration workflow

Restore the local EF tool:

```bash
dotnet tool restore
```

Apply the migration from a clean local database:

```bash
dotnet ef database update --configuration Release --project src/BlijvenLeren.App/BlijvenLeren.App.csproj --startup-project src/BlijvenLeren.App/BlijvenLeren.App.csproj
```

Compose note:
- `compose.yaml` sets `Runtime__Database__ApplyMigrationsOnStartup=true` for the `app` service, so the container runtime applies pending migrations automatically against the local PostgreSQL instance.
- `compose.yaml` also sets `Runtime__Database__SeedDemoDataOnStartup=true`, so a clean local container runtime starts with representative review data.

### Local login setup

The local Keycloak realm is imported automatically in the compose runtime.

Demo accounts:
- internal user: `internal.demo / Passw0rd!`
- external contributor: `external.demo / Passw0rd!`

Browser flow:
- open `http://localhost:8080/`
- select `Login via local OIDC`
- complete the Keycloak sign-in screen with one of the demo accounts
- open `http://localhost:8080/protected`

Flow notes:
- browser sign-in uses the OIDC authorization-code flow with the local Keycloak realm
- the app stores a local authentication cookie after the OIDC callback completes
- API routes under `/api/*` validate bearer tokens from the same realm instead of relying on the browser cookie

Bearer-token flow example:

```bash
curl -X POST "http://localhost:8081/realms/blijvenleren/protocol/openid-connect/token" ^
  -H "Content-Type: application/x-www-form-urlencoded" ^
  -d "client_id=blijvenleren-app&grant_type=password&username=internal.demo&password=Passw0rd!"
```

Use the returned access token with:

```bash
curl "http://localhost:8080/api/auth/me" -H "Authorization: Bearer <access_token>"
```

API docs note:
- interactive API docs are deferred to follow-up issue `#20`
- for now, use the documented curl examples or your preferred HTTP client against the local endpoints

Learning-resource create example:

```bash
curl -X POST "http://localhost:8080/api/v1/learning-resources" ^
  -H "Authorization: Bearer <internal_access_token>" ^
  -H "Content-Type: application/json" ^
  -d "{\"title\":\"Intro to REST APIs\",\"description\":\"Short MVP sample resource.\",\"url\":\"https://example.com/rest-api\"}"
```

### Demo data workflow

The container runtime seeds demo data automatically when the database is empty.

To reset demo data manually:

```bash
curl -X POST "http://localhost:8080/api/demo/seed-data?reset=true"
```

Expected seeded shape:
- 3 learning resources
- internal comments in `Approved`
- external comments in `Pending`
- external comments in `Rejected`

### Container troubleshooting

- If `docker compose up --build` fails on image pulls, retry after Docker Desktop finishes starting and registry access is available.
- If port `8080`, `8081`, or `5432` is already in use, stop the conflicting service or change the port mapping in `compose.yaml`.
- If the app starts before `db` or `idp` is fully ready, refresh `http://localhost:8080/api/health/dependencies` after a few seconds. Full healthchecks are deferred follow-up work.
- If you are running the app outside Docker and do not want startup migrations, leave `Runtime__Database__ApplyMigrationsOnStartup` unset or `false`.
- If you are running the app outside Docker and want startup seeding, set `Runtime__Database__SeedDemoDataOnStartup=true`.
- If login fails immediately after startup, wait for Keycloak realm import to finish and try again a few seconds later.

## Why Terraform is not included in this MVP

Infrastructure provisioning is deliberately out of scope for this assignment iteration.
The available time is used to maximize demonstrable value in:
- core product behavior
- authentication and authorization flow
- data model and moderation flow
- testability and reviewable technical decisions

Terraform and cloud deployment design are treated as deferred work and can be added once the core MVP behavior is validated.

## Review process for demonstration

For demonstration purposes, changes are reviewed through GitHub rather than only through local diffs.
Work is done on feature branches and submitted as pull requests so interviewers can review:
- commit history and scope
- rationale in PR descriptions
- evolution of decisions over time
