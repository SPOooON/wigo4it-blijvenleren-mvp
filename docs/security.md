# Security

## Current posture

This MVP uses a local Keycloak instance to demonstrate authentication and role-based behavior while still allowing brokered social login for external contributors.

## Local authentication approach

- Browser users authenticate against the local identity provider through the OIDC authorization-code flow.
- The local realm also includes placeholder GitHub and Google identity brokers for external-user sign-in.
- The application stores a local authentication cookie after the OIDC callback completes.
- API endpoints can also validate bearer tokens issued by the same Keycloak realm.
- Two realm roles are currently used:
  - `internal-user`
  - `external-contributor`

## Demo-only shortcuts

- Test account credentials are committed because they are only intended for the disposable local demo environment.
- Social-provider broker credentials are intentionally committed as dummy placeholders only; real client secrets must be entered manually in the local Keycloak admin console.
- HTTP is used locally for simplicity; production deployment should require HTTPS and stricter token validation settings.
- The compose runtime uses a backchannel-authority override so the containerized app can reach Keycloak while the browser still signs in against `http://localhost:8081`.
- The demo reseed endpoint is protected by the internal-user role, but it is still a convenience endpoint that would need tighter operational controls in a real system.

## Immediate safeguards still in place

- Protected routes require authentication before access.
- Role checks are enforced in the application for internal-only and external-only behavior.
- Brokered social users are intended to land in the existing `external-contributor` role through Keycloak role mapping rather than through app-specific special cases.
- API bearer tokens are validated against the local identity provider before protected API access is granted.
- Browser create, edit, and delete routes for learning resources are restricted to the `internal-user` role.
- API create, update, and delete routes for learning resources are restricted to the `internal-user` role.
- Comment submission requires an authenticated browser session or bearer token.
- External comments are stored as pending instead of being published immediately in the normal resource views.
- Pending-comment review and moderation actions are restricted to the `internal-user` role.
- Server-side moderation rules reject attempts to moderate internal comments or already-moderated comments.
- The local docs UI only documents bearer-token use for protected API endpoints; it does not bypass the app's normal authorization checks.

## API docs auth behavior

- `/openapi/v1.json` and `/docs` are local review aids, not separate auth surfaces.
- Protected endpoints in the generated document are annotated with bearer-token guidance and `401` / `403` responses.
- For local review, obtain a bearer token from the demo Keycloak realm and use that token in the docs UI when calling protected endpoints.

## Deferred hardening

- Enforce stronger cookie settings and production-grade HTTPS-only behavior.
- Move test account provisioning out of committed realm-import data for non-demo environments.
- Replace manual social-provider credential entry with environment-specific secret management if this ever moves beyond local demo scope.

## Review traceability

- Authentication and authorization requirements are mapped in `docs/functional-requirements.md`.
- Current protected boundaries are listed in `docs/architecture.md`.
- Deferred hardening work is tracked in `docs/backlog.md`.
