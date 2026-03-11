# Architecture

## Overview

The solution is designed as a locally runnable MVP with the following building blocks:

- Combined ASP.NET Core application hosting browser UI and API endpoints
- Database
- Identity provider

## Primary flow

1. User authenticates through the identity provider
2. Browser requests and API calls both terminate in the same ASP.NET Core app
3. The app validates identity and authorization based on route type and role requirements
4. Razor Pages handlers and minimal API endpoints read/write learning resources and comments through the same DbContext
5. Internal moderation rules control visibility of external comments

## Container view

- `app`: browser-facing frontend and HTTP API hosted in one ASP.NET Core app for the current MVP
- `db`: persistent relational database
- `idp`: authentication and identity management

## Local container runtime

`compose.yaml` defines the current local runtime with these services:

- `app`: built from `src/BlijvenLeren.App/Dockerfile`, exposed on host port `8080`
- `db`: PostgreSQL 16, exposed on host port `5432`
- `idp`: Keycloak in development mode, exposed on host port `8081`

Current runtime behavior:
- browser traffic goes to `app`
- browser and future client-side API calls terminate in the same `app` service
- `app` can reach `db` through the compose network name `db`
- `app` can reach `idp` through the compose network name `idp`
- `app` exposes `/api/health/dependencies` to show whether those dependencies are reachable from inside the runtime
- `app` exposes `/openapi/v1.json` for the generated API document and `/docs` for the local interactive docs UI
- `app` applies pending EF Core migrations automatically in the compose runtime before serving requests
- `app` seeds demo data automatically in the compose runtime when the database is empty
- `idp` imports the local demo realm, roles, test users, and placeholder social identity providers on startup

Current hosted validation behavior:
- GitHub Actions runs the main Release build-and-test path on pull requests and pushes to `main`
- hosted validation currently covers build and automated tests, not the compose runtime or the real OIDC redirect flow

## Current project structure

The runnable application code currently lives in:

- `src/BlijvenLeren.App`: single ASP.NET Core Razor Pages application with minimal API endpoints
- `BlijvenLeren.sln`: top-level solution file for local build and future expansion
- `test/BlijvenLeren.App.Tests`: unit and integration tests covering learning resources, comments, moderation, browser flows, and OpenAPI behavior

API route registration is now grouped by feature under `src/BlijvenLeren.App/Features/*/*EndpointRouteBuilderExtensions.cs`, while `Program.cs` stays focused on startup and top-level composition.

Current entry-point behavior:
- `/` serves the current MVP landing page with auth and runtime guidance
- `/api/health` serves a simple app-health response

This keeps the runtime easy to review while still demonstrating both browser and API access paths.

Current CRUD behavior:
- `/LearningResources` lists the seeded learning resources in the browser
- `/LearningResources/Details/{id}` shows resource details and currently visible approved comments
- `/LearningResources/Create` and `/LearningResources/Edit/{id}` are restricted to internal users
- `DELETE` browser behavior is triggered from the details page and enforced server-side
- `/api/v1/learning-resources` now covers list, detail, create, update, and delete operations

Current comment behavior:
- authenticated users can add comments from the resource details page or through `POST /api/v1/learning-resources/{id}/comments`
- comment owner identity is stored separately from the display name so later moderation work has stable ownership metadata
- internal-user comments are saved as `Approved` immediately
- external-contributor comments are saved as `Pending`
- normal resource detail views currently expose only approved comments

Current moderation behavior:
- internal users can review pending external comments through `GET /api/v1/comments/pending`
- the browser UI exposes the same queue at `/Moderation/Comments`
- moderation actions go through `POST /api/v1/comments/{id}/moderation` or the browser approve/reject forms
- only pending external comments can transition to `Approved` or `Rejected`
- moderation timestamps are recorded when an internal user makes the decision

Current API docs behavior:
- the built-in ASP.NET Core OpenAPI stack generates a local `v1` document
- the document is filtered to `/api/*` paths so browser-login endpoints do not clutter the API review surface
- Scalar provides the local interactive docs UI without becoming a long-term API-stack decision by itself
- protected endpoints include explicit bearer-token guidance plus `401` and `403` response metadata

Current test/runtime notes:
- the automated suite runs both locally and in hosted CI through `dotnet test BlijvenLeren.sln -c Release`
- the suite uses an in-process test host with EF Core InMemory rather than a containerized PostgreSQL runtime
- compose startup behavior and the real Keycloak login redirect remain part of the manual review path

## Data layer

The current persistence implementation uses:

- EF Core in the same ASP.NET Core app
- PostgreSQL as the relational store
- one `learning_resources` table for the main domain entity
- one `comments` table with explicit moderation state values
- an initial migration in `src/BlijvenLeren.App/Data/Migrations`

Schema notes:
- comments belong to a learning resource through a required foreign key
- comment moderation is stored explicitly as `Pending`, `Approved`, or `Rejected`
- comment author type is stored explicitly as `Internal` or `External`
- the first index targets `(LearningResourceId, Status)` to support resource detail and moderation queries

## Domain and contract baseline

The current domain/API baseline intentionally keeps modeling straightforward:

- EF Core entities remain the current domain model for the MVP
- versioned transport contracts live separately under `Contracts/V1`
- endpoint registration, mapping, and validation live under `Features/*`
- the current versioned API slice covers list, detail, create, update, and delete behavior for learning resources plus comment and moderation endpoints

This keeps the current layer count low while still separating persistence entities from external request/response contracts.

## Demo data

The current demo-data approach lives inside the application runtime:

- `DemoDataSeeder` inserts a small representative dataset through the same DbContext used by the app
- startup seeding is opt-in through configuration
- the compose runtime enables startup seeding for local review convenience
- `POST /api/demo/seed-data?reset=true` clears existing records and reloads the demo dataset

The seeded data intentionally demonstrates:
- multiple learning resources
- internal comments that are already approved
- external comments awaiting moderation
- external comments that were rejected

## Authentication layer

The current authentication setup uses:

- Keycloak as the local identity provider
- one local realm imported through the compose runtime
- two realm roles:
  - `internal-user`
  - `external-contributor`
- prewired `GitHub` and `Google` identity brokers in Keycloak with placeholder credentials
- OIDC authorization-code flow for browser sign-in
- cookie auth for browser sessions
- JWT bearer validation for protected API endpoints

Current protected boundaries:
- `/protected` requires authentication
- `/LearningResources/Create` requires the internal-user role
- `/LearningResources/Edit/{id}` requires the internal-user role
- delete behavior on `/LearningResources/Details/{id}` requires the internal-user role
- comment submission on `/LearningResources/Details/{id}` requires authentication
- `/Moderation/Comments` requires the internal-user role
- `/api/demo/seed-data` requires the internal-user role
- `/api/auth/internal` requires the internal-user role
- `/api/auth/external` requires the external-contributor role
- `/api/auth/me` accepts authenticated browser sessions or bearer tokens
- `POST`, `PUT`, and `DELETE` on `/api/v1/learning-resources` require the internal-user role
- `POST /api/v1/learning-resources/{id}/comments` requires authentication
- `GET /api/v1/comments/pending` requires the internal-user role
- `POST /api/v1/comments/{id}/moderation` requires the internal-user role

Flow notes:
- browser requests challenge through the app and are redirected to Keycloak
- the app can suggest a preferred external identity provider to Keycloak through `kc_idp_hint`
- the preferred external path currently points at the `GitHub` broker when that provider is configured
- the app completes the OIDC callback and stores an auth cookie for subsequent browser navigation
- API routes default to bearer-token authentication semantics so protected API calls do not depend on cookie redirects
- social-provider credentials are intentionally not valid out of the box; reviewers must replace the placeholder values in the Keycloak admin console before brokered login will succeed

## Design goals

- local reproducibility
- traceable security boundaries
- separation of concerns
- easy interview walkthrough
- realistic path toward cloud deployment later

## Deployment scope decision

Terraform and actual deployment automation are excluded from this MVP.

Reasoning:
- The assignment is time-boxed and prioritizes demonstrable functional behavior.
- Core value for review is in domain logic, moderation flow, authentication boundaries and testability.
- Partial or shallow IaC would add complexity without improving confidence in core requirements.

Impact:
- No in-repo infrastructure provisioning scripts for this version.
- Cloud deployment remains a documented follow-up step.

## Bootstrap scope trade-off

This repository currently uses one ASP.NET Core app instead of separate web and API projects.

Reasoning:
- The current load and feature scope do not justify an early split.
- A combined app reduces setup overhead for a sample assignment.
- The repository still demonstrates both browser and API entry points without premature structure.

Impact:
- Reviewers can run one process to see both surfaces.
- A later split remains possible if the application grows or deployment concerns become stricter.
- Architecture documentation must explicitly record that this is an MVP simplification rather than a final scaling choice.

## Compose scope trade-off

The original container runtime issue described separate `web` and `api` services. The implemented compose runtime keeps a single `app` service instead.

Reasoning:
- The current application is still a combined ASP.NET Core app.
- Splitting the container topology before the code splits would add operational structure that the application does not yet need.

Impact:
- The compose runtime matches the implementation rather than the earlier aspirational architecture wording.
- Future issues can introduce a separate `web` and `api` runtime if the application is intentionally split later.

## Diagrams

The Mermaid diagrams are intended to stay aligned with the implementation and this document.

See:
- `../diagrams/context.mmd`
- `../diagrams/container.mmd`
- `../diagrams/auth-sequence.mmd`
