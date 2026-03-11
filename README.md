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

## Review guide

If you want the fastest walkthrough of what is built versus deferred, start here:
- [Reviewer walkthrough](docs/reviewer-walkthrough.md) for a short guided demo path through the MVP
- [Functional requirements](docs/functional-requirements.md) for requirement-to-implementation traceability
- [Traceability map](docs/traceability-map.md) for where to find the main code and tests behind each implemented slice
- [Architecture](docs/architecture.md) for the current runtime, auth, and moderation design
- [Testing strategy](docs/testing-strategy.md) for what is covered automatically and what is still manual
- [Security](docs/security.md) for demo shortcuts and active safeguards
- [Backlog](docs/backlog.md) for intentionally deferred work and its impact

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
- `http://127.0.0.1:5078/` for the MVP landing page and browser entry point
- `http://127.0.0.1:5078/LearningResources` for the main browser learning-resource flow
- `http://127.0.0.1:5078/api/health` for the application health endpoint
- `http://127.0.0.1:5078/docs` for the local interactive API docs

Local URL note:
- the checked-in launch profile also uses port `5078`
- the compose runtime uses port `8080`
- if you see `http://localhost:5114`, you are not on the compose-hosted app

You can also build the current solution with:

```bash
dotnet build BlijvenLeren.sln
```

### Run tests

Run the full automated suite:

```bash
dotnet test BlijvenLeren.sln -c Release
```

Hosted validation:
- GitHub Actions runs the same Release build-and-test path for pull requests and pushes to `main`

Run the browser smoke path only:

```bash
dotnet test test/BlijvenLeren.App.Tests/BlijvenLeren.App.Tests.csproj -c Release --filter FullyQualifiedName~BrowserSmoke
```

Current scope:
- unit tests cover request validation and contract-mapping rules
- integration tests cover the main API and Razor Pages flows against an in-memory app host
- the browser smoke path verifies the main list-to-details Razor Pages journey without adding a separate browser-automation stack
- pull requests now get a hosted GitHub Actions build-and-test check using the same main command path

### Run the container runtime

```bash
docker compose up --build
```

If you want a compact guided demo after startup, use the [reviewer walkthrough](docs/reviewer-walkthrough.md).

Container ports:
- app: `http://localhost:8080`
- db: `localhost:5432`
- idp: `http://localhost:8081`

Useful endpoints after startup:
- app landing page: `http://localhost:8080/`
- app health: `http://localhost:8080/api/health`
- dependency probe: `http://localhost:8080/api/health/dependencies`
- OpenAPI document: `http://localhost:8080/openapi/v1.json`
- API docs UI: `http://localhost:8080/docs`
- browser resource list: `http://localhost:8080/LearningResources`
- browser resource details: `http://localhost:8080/LearningResources/{id}`
- browser resource create route: `http://localhost:8080/LearningResources/Create`
- browser resource edit route: `http://localhost:8080/LearningResources/Edit/{id}`
- versioned list route: `GET http://localhost:8080/api/v1/learning-resources`
- versioned detail route: `GET http://localhost:8080/api/v1/learning-resources/{id}`
- versioned create route: `POST http://localhost:8080/api/v1/learning-resources`
- versioned update route: `PUT http://localhost:8080/api/v1/learning-resources/{id}`
- versioned delete route: `DELETE http://localhost:8080/api/v1/learning-resources/{id}`
- comment create route: `POST http://localhost:8080/api/v1/learning-resources/{id}/comments`
- pending comment list route: `GET http://localhost:8080/api/v1/comments/pending`
- comment moderation route: `POST http://localhost:8080/api/v1/comments/{id}/moderation`
- persistence smoke path: `POST http://localhost:8080/api/health/persistence-smoke`
- demo data reseed path: `POST http://localhost:8080/api/demo/seed-data?reset=true`
- current-user route: `GET http://localhost:8080/api/auth/me`
- internal-only route: `GET http://localhost:8080/api/auth/internal`
- external-only route: `GET http://localhost:8080/api/auth/external`
- Keycloak admin console: `http://localhost:8081/` with `admin` / `admin`

## Current MVP scope

Issue `#4` established the initial runnable starting point. The current implementation now:
- uses one ASP.NET Core project to host both Razor Pages and minimal API endpoints
- provides a reviewer-facing landing page plus learning-resource browser flows
- exposes health, auth, resource, comment, moderation, demo-data, and API-docs endpoints
- keeps the combined-app structure as an MVP trade-off rather than a missing implementation step

Issue `#5` adds the local container runtime around that MVP:
- `app` hosts both the browser UI and the API surface
- `db` provides the PostgreSQL persistence used by the current app
- `idp` provides the local Keycloak identity provider used by the current auth flow
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

Issue `#9` completes the first learning-resource CRUD slice:
- browser list and details pages for seeded learning resources
- internal-only browser create, edit, and delete flows
- versioned update and delete API routes alongside the existing list, detail, and create routes
- role checks enforced consistently across browser and API paths
- integration coverage for API and browser happy paths using an in-memory test host

Issue `#10` adds the first comment-submission slice:
- authenticated users can add comments through the API and browser details page
- internal-user comments are auto-approved and show up immediately in normal resource views
- external-contributor comments are stored as pending for the later moderation workflow
- comment owner identity is now stored alongside display metadata for future moderation decisions

Issue `#11` adds the moderation workflow:
- internal users can review pending external comments through the API and an internal browser page
- pending external comments can be approved or rejected with server-side transition rules
- approved external comments become visible in the normal resource detail views
- rejected external comments remain hidden from the normal resource detail views

Issue `#12` formalizes the MVP automated test suite:
- one test project now covers validation, mapping, API integration, and browser-facing Razor Pages checks
- the suite includes an explicit `BrowserSmoke` path for the main list-to-details journey
- README and testing docs now describe the reproducible local test commands

Issue `#20` adds local API docs:
- the app now generates a built-in ASP.NET Core OpenAPI document at `/openapi/v1.json`
- Scalar serves a separate interactive docs UI at `/docs`
- protected API endpoints are annotated with bearer-token requirements and `401`/`403` response metadata
- the local docs flow uses the same bearer tokens already documented for API testing

## Traceability summary

Current implemented requirement coverage:
- FR-01 through FR-11 are implemented in the current MVP slice
- FR-12 and FR-13 are implemented as an MVP subset rather than a fully polished product surface

Current notable deferrals:
- browser/API list ergonomics such as pagination, filtering, and sorting remain deferred
- production hardening, richer moderation auditability, and cloud/IaC concerns remain intentionally out of MVP scope

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
- the built-in OpenAPI document is available at `http://localhost:8080/openapi/v1.json`
- the interactive local docs UI is available at `http://localhost:8080/docs`
- for protected endpoints, first obtain a bearer token from the local Keycloak realm, then use the docs UI auth panel or your preferred HTTP client

Learning-resource create example:

```bash
curl -X POST "http://localhost:8080/api/v1/learning-resources" ^
  -H "Authorization: Bearer <internal_access_token>" ^
  -H "Content-Type: application/json" ^
  -d "{\"title\":\"Intro to REST APIs\",\"description\":\"Short MVP sample resource.\",\"url\":\"https://example.com/rest-api\"}"
```

Learning-resource update example:

```bash
curl -X PUT "http://localhost:8080/api/v1/learning-resources/<resource_id>" ^
  -H "Authorization: Bearer <internal_access_token>" ^
  -H "Content-Type: application/json" ^
  -d "{\"title\":\"Updated REST APIs intro\",\"description\":\"Updated MVP sample resource.\",\"url\":\"https://example.com/rest-api-updated\"}"
```

Learning-resource delete example:

```bash
curl -X DELETE "http://localhost:8080/api/v1/learning-resources/<resource_id>" ^
  -H "Authorization: Bearer <internal_access_token>"
```

Comment create example:

```bash
curl -X POST "http://localhost:8080/api/v1/learning-resources/<resource_id>/comments" ^
  -H "Authorization: Bearer <access_token>" ^
  -H "Content-Type: application/json" ^
  -d "{\"body\":\"Useful follow-up context from the API demo flow.\"}"
```

Comment visibility note:
- internal-user comments are returned immediately in the normal resource detail responses
- external-contributor comments are stored as `Pending` and are not shown in the normal detail responses until an internal moderator approves them

Moderation examples:

```bash
curl "http://localhost:8080/api/v1/comments/pending" ^
  -H "Authorization: Bearer <internal_access_token>"
```

```bash
curl -X POST "http://localhost:8080/api/v1/comments/<comment_id>/moderation" ^
  -H "Authorization: Bearer <internal_access_token>" ^
  -H "Content-Type: application/json" ^
  -d "{\"action\":\"approve\"}"
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
