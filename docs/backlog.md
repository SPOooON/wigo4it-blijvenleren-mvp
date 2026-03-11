# Backlog

This file captures work that is intentionally deferred, not forgotten.

Reading guide:
- each section groups deferred work by the issue that made the trade-off
- items stay here only if they still matter after later issues land
- the impact line explains what reviewers should assume is missing today

## Deferred from issue #4

- Split browser UI and API into separate projects if later scope, hosting or team boundaries justify it.
- Add a shared contracts project if DTO reuse starts to create coupling or duplication.
- Add coding standards enforcement such as `.editorconfig` and CI validation once the codebase has more than the initial bootstrap slice.
Impact:
The current repo favors one combined app and lightweight project structure over earlier separation.

## Deferred from issue #5

- Add Docker Compose profiles for a faster inner-loop setup once more services exist.
- Add explicit healthchecks and readiness ordering for all services instead of relying on startup timing.
- Replace the Keycloak development-mode defaults with stronger local configuration once the authentication flow is implemented.
Impact:
The local runtime is reproducible, but not yet optimized or hardened.

## Deferred from issue #6

- Add more targeted indexes after the real list and moderation query patterns are implemented.
Impact:
Persistence is reviewable and tested at the app level, but database tuning is still minimal.

## Deferred from issue #18

- Add environment-specific seed sets once reviewer and developer workflows diverge.
- Add richer personas, more comments per resource, and moderation-history examples once the UI can surface them.
- Replace the HTTP reseed endpoint with a more role-aware or operator-only flow once authentication and authorization are implemented.
Impact:
The demo data is predictable and useful, but still optimized for one shared MVP walkthrough.

## Deferred from issue #7

- Tighten token validation and cookie settings for production-like environments.
- Move committed demo credentials and realm provisioning out of the default runtime for non-demo environments.
- Add built-in OpenAPI generation and a local interactive API docs UI in issue `#20`.
Impact:
Authentication is suitable for local review, not for production deployment.

## Deferred from issue #8

- Add explicit API versioning strategy documentation once there is more than one versioned slice.
- Revisit whether the current EF-entity-as-domain baseline should be split further once business rules become richer.
Impact:
The current API boundary is deliberate, but not yet a broader long-term versioning strategy.

## Deferred from issue #9

- Add pagination, filtering, and sorting to the resource list in browser and API views.
- Add optimistic concurrency handling for concurrent edits and deletes.
- Add browser automation if the UI grows beyond the current server-rendered forms and navigation.
Impact:
CRUD behavior exists, but list ergonomics and conflict handling are still MVP-level.

## Deferred from issue #10

- Add edit and delete behavior for comments once comment ownership UX is defined.
- Add anti-spam or rate-limit controls for comment submission.
Impact:
Comment submission works, but community-facing resilience and management features remain light.

## Deferred from issue #11

- Add moderation audit history beyond the current status and timestamp fields.
- Add bulk approval or rejection operations if the queue grows.
- Add richer moderator context such as linked resource excerpts or filters once the queue becomes larger.
Impact:
Moderation decisions are enforceable, but not yet optimized for higher-volume reviewer workflows.

## Deferred from issue #12

- Add CI workflow or GitHub PR checks for automatic execution outside local development.
- Add more failure-path coverage if business rules become more complex.
Impact:
The automated suite is reproducible locally, but not yet enforced in hosted automation.
