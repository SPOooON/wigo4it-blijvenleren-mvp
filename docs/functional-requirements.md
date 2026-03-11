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
- FR-11 is implemented through the local Keycloak-backed browser login flow and bearer-token validation.
- FR-12 is partially implemented through the health/auth/resource API slice.
- FR-13 is partially implemented through the current Razor Pages landing, protected, and learning-resource CRUD pages.

## Notes and implementation choices

The brief mentions social login as a preference rather than an absolute requirement.  
This implementation may choose a local identity provider and document social login as a future extension.

See:
- `docs/scope-and-assumptions.md`
- `docs/technical-decisions.md`
