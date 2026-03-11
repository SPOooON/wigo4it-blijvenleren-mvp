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
- persistence smoke path: `POST http://localhost:8080/api/health/persistence-smoke`
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

### Container troubleshooting

- If `docker compose up --build` fails on image pulls, retry after Docker Desktop finishes starting and registry access is available.
- If port `8080`, `8081`, or `5432` is already in use, stop the conflicting service or change the port mapping in `compose.yaml`.
- If the app starts before `db` or `idp` is fully ready, refresh `http://localhost:8080/api/health/dependencies` after a few seconds. Full healthchecks are deferred follow-up work.
- If you are running the app outside Docker and do not want startup migrations, leave `Runtime__Database__ApplyMigrationsOnStartup` unset or `false`.

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
