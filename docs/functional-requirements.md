# Functional Requirements

This document translates the assignment brief into an implementation-oriented requirement set.

## Source

Requirements are derived from the BlijvenLeren assignment brief.

## Core domain entity: Learning Resource

A learning resource has at minimum:
- Title
- Description
- Link to a website or video
- List of user comments

## User roles

### Internal user
Can:
- view learning resources
- create learning resources
- edit learning resources
- delete learning resources
- add comments that are directly visible
- approve or reject external comments

### External contributor
Can:
- view public or shared learning resources (depending on implementation scope)
- add comments that require approval before becoming visible

## Functional requirements

### FR-01 List learning resources
The system shall provide a list of learning resources.

### FR-02 View learning resource details
The system shall provide details of a selected learning resource.

### FR-03 Add learning resource
The system shall allow internal users to add a new learning resource.

### FR-04 Edit learning resource
The system shall allow internal users to edit an existing learning resource.

### FR-05 Delete learning resource
The system shall allow internal users to delete an existing learning resource.

### FR-06 Add comment to learning resource
The system shall allow users to add comments to a learning resource.

### FR-07 Auto-publish internal comments
Comments from internal users shall be visible immediately.

### FR-08 Moderate external comments
Comments from external contributors shall require approval before becoming visible.

### FR-09 Review pending comments
The system shall provide a list of pending external comments for internal users.

### FR-10 Approve or reject pending comments
The system shall allow internal users to approve or reject pending comments.

### FR-11 Authentication
Users shall be required to log in.

### FR-12 API access
The main functionality shall be accessible through an API.

### FR-13 Browser access
The main functionality shall be accessible through a browser-based application.

## Current implementation coverage

- FR-01 is implemented through `GET /api/v1/learning-resources` and `/LearningResources`.
- FR-02 is implemented through `GET /api/v1/learning-resources/{id}` and `/LearningResources/Details/{id}`.
- FR-03 is implemented through `POST /api/v1/learning-resources` and `/LearningResources/Create` for internal users.
- FR-04 is implemented through `PUT /api/v1/learning-resources/{id}` and `/LearningResources/Edit/{id}` for internal users.
- FR-05 is implemented through `DELETE /api/v1/learning-resources/{id}` and the delete action on `/LearningResources/Details/{id}` for internal users.
- FR-06 is implemented through `POST /api/v1/learning-resources/{id}/comments` and the comment form on `/LearningResources/Details/{id}` for authenticated users.
- FR-07 is implemented by auto-approving internal-user comments on submission so they appear immediately in the normal resource detail views.
- FR-08 is implemented by storing external comments as `Pending` and excluding them from the normal resource detail views until they are approved.
- FR-09 is implemented through `GET /api/v1/comments/pending` and `/Moderation/Comments` for internal users.
- FR-10 is implemented through `POST /api/v1/comments/{id}/moderation` and the approve/reject actions on `/Moderation/Comments` for internal users.
- FR-11 is implemented through the local Keycloak-backed browser login flow, prewired social-login brokers for external contributors, and bearer-token validation.
- FR-12 is partially implemented through the health/auth/resource/comment/moderation API slice plus local OpenAPI documentation.
- FR-13 is partially implemented through the current Razor Pages landing, protected, and learning-resource CRUD pages.

## Traceability matrix

| Requirement | Status | Current implementation | Deferred notes |
| --- | --- | --- | --- |
| FR-01 List learning resources | Implemented | `GET /api/v1/learning-resources`, `/LearningResources` | Pagination/filtering/sorting are deferred. |
| FR-02 View learning resource details | Implemented | `GET /api/v1/learning-resources/{id}`, `/LearningResources/Details/{id}` | Detail view stays intentionally simple. |
| FR-03 Add learning resource | Implemented | `POST /api/v1/learning-resources`, `/LearningResources/Create` | Internal-user only in the MVP. |
| FR-04 Edit learning resource | Implemented | `PUT /api/v1/learning-resources/{id}`, `/LearningResources/Edit/{id}` | No concurrency protection yet. |
| FR-05 Delete learning resource | Implemented | `DELETE /api/v1/learning-resources/{id}`, delete action on `/LearningResources/Details/{id}` | No soft-delete or recovery flow. |
| FR-06 Add comment to learning resource | Implemented | `POST /api/v1/learning-resources/{id}/comments`, browser comment form on details page | No comment edit/delete flow yet. |
| FR-07 Auto-publish internal comments | Implemented | Internal comments are stored as `Approved` immediately | Current behavior is role-driven on the server. |
| FR-08 Moderate external comments | Implemented | External comments start as `Pending` and stay hidden until approved | Moderation audit history is deferred. |
| FR-09 Review pending comments | Implemented | `GET /api/v1/comments/pending`, `/Moderation/Comments` | Queue filtering and richer review context are deferred. |
| FR-10 Approve or reject pending comments | Implemented | `POST /api/v1/comments/{id}/moderation`, browser approve/reject actions | Bulk moderation is deferred. |
| FR-11 Authentication | Implemented | Local Keycloak OIDC login, prewired GitHub/Google broker support, and bearer validation | Social providers need manual credential setup in Keycloak before they work. |
| FR-12 API access | Partially implemented | Health/auth/resource/comment/moderation endpoints plus `/openapi/v1.json` and `/docs` | Broader API completeness remains deferred. |
| FR-13 Browser access | Partially implemented | Razor Pages landing, protected area, CRUD pages, moderation page | UI polish and broader flows remain deferred. |

## Notes and implementation choices

The brief prefers social login, so the local Keycloak realm now includes placeholder social providers.
The current MVP still treats real provider credentials as manual local setup rather than checked-in configuration.

See:
- `docs/scope-and-assumptions.md`
- `docs/technical-decisions.md`
