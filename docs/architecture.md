# Architecture

## Overview

The solution is designed as a locally runnable MVP with the following building blocks:

- Combined ASP.NET Core application hosting browser UI and API endpoints
- Database
- Identity provider

## Primary flow

1. User authenticates through the identity provider
2. Web application calls API
3. API validates identity and authorization
4. API reads/writes learning resources and comments in the database
5. Internal moderation flow controls visibility of external comments

## Container view

- `app`: browser-facing frontend and HTTP API hosted in one ASP.NET Core app for the bootstrap phase
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
- `app` applies pending EF Core migrations automatically in the compose runtime before serving requests
- `app` seeds demo data automatically in the compose runtime when the database is empty
- `idp` imports the local demo realm, roles, and test users on startup

## Current project structure

The runnable application code currently lives in:

- `src/BlijvenLeren.App`: single ASP.NET Core Razor Pages application with minimal API endpoints
- `BlijvenLeren.sln`: top-level solution file for local build and future expansion
- `test/BlijvenLeren.App.Tests`: unit and integration tests for the current learning-resource slice

Current bootstrap behavior:
- `/` serves a placeholder browser page
- `/api/health` serves a placeholder API health response

This keeps the startup slice small while still demonstrating both browser and API access paths.

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
- mapping and validation live in `Features/LearningResources`
- the first versioned API slice covers list, detail, and create behavior for learning resources

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
- the app completes the OIDC callback and stores an auth cookie for subsequent browser navigation
- API routes default to bearer-token authentication semantics so protected API calls do not depend on cookie redirects

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

See:
- `../diagrams/context.mmd`
- `../diagrams/container.mmd`
- `../diagrams/auth-sequence.mmd`
