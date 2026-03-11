# Technical Decisions

## TD-001 Use .NET for application code
**Decision**
Use C# / .NET for the application.

**Why**
The brief explicitly requires C# / .NET.

**Impact**
Aligns with assignment requirements and keeps the stack focused.

---

## TD-002 Build both API and Web in the same solution
**Decision**
Implement API and browser-based functionality in one solution, with separate projects where useful.

**Why**
The brief requires both browser access and API access.

**Impact**
Allows shared models and faster MVP delivery.

---

## TD-003 Use a relational database
**Decision**
Use a relational database for persistence.

**Why**
The domain is CRUD-heavy and relational structure is straightforward for resources, comments and approval state.

**Impact**
Simple querying, clear schema and reliable persistence.

---

## TD-004 Use a local identity provider
**Decision**
Use a local IDP for authentication in the demo environment.

**Why**
The brief requires login and prefers social login, but the assignment does not require full external identity integration.

**Impact**
Demonstrates auth architecture without over-investing in third-party setup.

---

## TD-005 Containerize app, DB and IDP
**Decision**
Run the system through containers.

**Why**
Supports local reproducibility and demonstrates DevOps thinking.

**Impact**
Simplifies onboarding and demo setup.

---

## TD-006 Exclude Terraform and deployment automation from MVP
**Decision**
Do not include Terraform or real deployment automation in this iteration.

**Why**
The assignment is time-boxed. Prioritizing functional behavior, authentication/authorization, and reviewable application quality provides higher signal than partial IaC.

**Impact**
- Infrastructure provisioning is explicitly out of scope.
- Deployment strategy is documented as follow-up work instead of implemented.
- The repository stays focused on core requirements and testable behavior.

**Rejected alternatives**
- Include minimal Terraform for local/demo targets: rejected because it introduces setup and maintenance overhead without improving validation of core business requirements.
- Start cloud deployment directly: rejected because it reduces time available for core MVP completeness and security-critical application flows.

---

## TD-007 Document deferred work explicitly
**Decision**
Anything not implemented but relevant goes into backlog and scope docs.

**Why**
The brief allows selective implementation as long as choices are argued well.

**Impact**
Reduces ambiguity during review.

---

## TD-008 Use GitHub PRs as the primary review artifact
**Decision**
Use feature branches and GitHub pull requests as the main demonstration and review flow.

**Why**
Reviewers need transparent, auditable change history and rationale that is easy to inspect asynchronously.

**Impact**
- Changes are grouped into focused PRs with explicit summaries.
- Design trade-offs are visible both in commit history and PR discussion.
- Local work remains reproducible, while review happens in GitHub.

---

## TD-009 Use one ASP.NET Core app for browser and API bootstrap
**Decision**
Bootstrap the solution with one ASP.NET Core app that serves both the browser UI and the initial HTTP API endpoint.

**Why**
Issue `#4` needs a runnable skeleton, but a separate web and API project split adds ceremony without improving the sample app at the current scale.

**Impact**
- The repository gets a single runnable entry point for both browser and API access.
- Setup and review stay simpler during early implementation.
- If later issues introduce scaling, deployment or ownership concerns, the app can still be split into separate projects.

**Rejected alternatives**
- Start with separate web and API projects immediately: rejected because it adds structure before there is enough behavior to justify it in this MVP.

---

## TD-010 Use Docker Compose with app, db and idp services for local runtime
**Decision**
Use `compose.yaml` to run the MVP locally with three services: `app`, `db`, and `idp`.

**Why**
The repository needs a single-command runtime that reflects the documented container architecture, but the current codebase only has one combined ASP.NET Core application rather than separate web and API deployables.

**Impact**
- Local reviewers can start the runtime with one Docker Compose command.
- The runtime includes the infrastructure services needed for upcoming persistence and authentication work.
- The compose topology intentionally differs from the older `web`/`api` wording and must stay documented as an MVP simplification.

**Rejected alternatives**
- Force separate `web` and `api` containers immediately: rejected because the codebase still ships one application artifact.
- Delay container runtime work until persistence and authentication are implemented: rejected because local reproducibility is a stated architecture goal.

---

## TD-011 Use EF Core migrations for the initial PostgreSQL schema
**Decision**
Use EF Core with the Npgsql provider and store the initial schema as code-first migrations in the application project.

**Why**
Issue `#6` needs a reviewable relational schema plus a repeatable migration workflow. EF Core keeps the schema close to the application model and makes later schema changes easier to audit in pull requests.

**Impact**
- Schema changes are represented as checked-in migration files instead of ad hoc SQL only.
- The application can apply migrations automatically in the compose runtime for local demo convenience.
- A local `dotnet-ef` tool manifest is now part of the repo for repeatable migration commands.

**Rejected alternatives**
- Hand-write raw SQL migrations only: rejected because it adds manual mapping overhead at this stage without improving reviewer clarity.
- Delay ORM selection until CRUD features are implemented: rejected because the current issue explicitly requires a schema and migration workflow now.

---

## TD-012 Seed demo data through the application data layer
**Decision**
Seed review data through the application itself, using the existing DbContext and an opt-in runtime flag.

**Why**
Issue `#18` asks for predictable demo data aligned with the MVP runtime. Reusing the application data layer keeps the seeding path visible, easy to review, and consistent with the current single-app architecture.

**Impact**
- A clean compose runtime can start with representative review data automatically.
- Demo data can be reset through a simple HTTP endpoint instead of an extra external script.
- Seed logic stays close to the entity model and evolves with schema changes.

**Rejected alternatives**
- Maintain separate SQL seed scripts only: rejected because it duplicates model knowledge outside the app.
- Seed unconditionally on every startup: rejected because it would overwrite manual demo changes too aggressively.

---

## TD-013 Use Keycloak realm import plus OIDC code flow for MVP authentication
**Decision**
Use a local Keycloak realm import for users and roles, and authenticate browser users through the OIDC authorization-code flow while also validating bearer tokens from the same IDP.

**Why**
Issue `#7` needs local authentication and role mapping. Using the standard browser-oriented OIDC flow keeps the MVP aligned with expected identity boundaries while still staying small enough for a local demo runtime.

**Impact**
- The compose runtime can start with ready-to-use accounts and roles.
- Reviewers can test protected browser behavior through a normal identity-provider redirect flow.
- API endpoints can still validate real bearer tokens issued by the local identity provider.
- The runtime needs a small backchannel-host rewrite so the containerized app can talk to the host-exposed Keycloak authority while browsers still use `localhost`.

**Rejected alternatives**
- Delay authentication until CRUD features exist: rejected because the assignment explicitly requires login and roles.
- Use an app-managed password exchange shortcut: rejected because it is less representative of a real browser login boundary and was unnecessary once the local OIDC flow was wired.

---

## TD-014 Keep the MVP domain model simple and separate only the API contracts
**Decision**
Keep the current EF Core entities as the MVP domain baseline, but introduce separate versioned request/response contracts for the HTTP API.

**Why**
Issue `#8` needs a coherent model and contract baseline with validation, but a heavier domain layer would add ceremony before the CRUD workflow is implemented. Separating the transport contracts now gives versionable API boundaries without forcing a larger architectural split yet.

**Impact**
- HTTP contracts can evolve independently from the persistence entities.
- Validation rules are centralized for request handling instead of leaking through EF/database errors.
- The current codebase stays simple enough for a sample assignment while still showing deliberate API design.

**Rejected alternatives**
- Reuse EF entities directly as API contracts: rejected because it couples the wire format to persistence too early.
- Introduce a richer DDD-style domain layer immediately: rejected because the current scope does not yet justify the extra abstraction.

---

## TD-015 Implement the first CRUD slice through Razor Pages plus minimal APIs
**Decision**
Deliver the first learning-resource CRUD slice through the existing Razor Pages app and the versioned minimal API surface, with the same DbContext and validation rules underneath both.

**Why**
Issue `#9` requires both browser and API access, but the current MVP still favors one combined application over a split frontend/backend architecture. Reusing the same application slice keeps behavior reviewable and avoids inventing a second UI/API integration layer too early.

**Impact**
- Internal-user create, edit, and delete behavior is now available through both browser and API paths.
- Validation and mapping stay centralized instead of diverging between UI and API handlers.
- The browser UI remains intentionally simple server-rendered Razor Pages rather than a separate SPA.

**Rejected alternatives**
- Build a separate frontend client for CRUD now: rejected because it adds architecture that the sample scope does not require.
- Implement only the API and defer browser CRUD: rejected because the issue explicitly requires both access paths.

---

## TD-016 Store comment owner identity and keep external comments pending by default
**Decision**
When authenticated users submit comments, store a stable owner identity on the comment record and auto-approve only internal-user comments. External comments remain pending and are excluded from the normal resource detail reads for now.

**Why**
Issue `#10` needs role-aware publication behavior plus metadata that future moderation can act on. Storing only a display name would be too weak for later review decisions, while publishing all comments immediately would bypass the intended moderation direction.

**Impact**
- Internal users get immediate visible feedback when commenting.
- External comments are persisted without being treated as public content yet.
- Issue `#11` can build moderation behavior on top of stored ownership and status metadata instead of reworking the schema.

**Rejected alternatives**
- Publish all authenticated comments immediately: rejected because it conflicts with the later external moderation requirement.
- Delay ownership metadata until moderation is implemented: rejected because it would create avoidable schema churn and weaker auditability.

---

## TD-017 Keep moderation as an explicit internal workflow with server-side transition rules
**Decision**
Implement pending-comment review as an internal moderation queue with explicit approve/reject actions, and enforce state transitions on the server so only pending external comments can be moderated.

**Why**
Issue `#11` requires review and moderation flows, but the MVP still needs a simple, reviewable design. An explicit queue plus one moderation action endpoint/page keeps the behavior easy to reason about and prevents UI-only assumptions from becoming security bugs.

**Impact**
- Internal moderators now have one clear browser page and one API surface for moderation.
- Approved comments become visible through the existing resource detail reads without duplicating comment-query logic.
- Repeated or invalid moderation attempts return conflicts instead of silently mutating comment state.

**Rejected alternatives**
- Auto-approve external comments after submission: rejected because it bypasses the requirement entirely.
- Allow moderation from multiple ad hoc pages first: rejected because it spreads security-sensitive state transitions across the UI too early.

---
