# Backlog

## Deferred from issue #4

- Split browser UI and API into separate projects if later scope, hosting or team boundaries justify it.
- Add a shared contracts project if DTO reuse starts to create coupling or duplication.
- Add coding standards enforcement such as `.editorconfig` and CI validation once the codebase has more than the initial bootstrap slice.
- Add automated tests as part of issue `#12`, which already tracks the MVP test suite.

## Deferred from issue #5

- Add Docker Compose profiles for a faster inner-loop setup once more services exist.
- Add explicit healthchecks and readiness ordering for all services instead of relying on startup timing.
- Replace the Keycloak development-mode defaults with stronger local configuration once the authentication flow is implemented.

## Deferred from issue #6

- Add seed data for a richer local demo once the CRUD flow exists.
- Add more targeted indexes after the real list and moderation query patterns are implemented.
- Split the persistence smoke path into automated integration tests once issue `#12` is implemented.
