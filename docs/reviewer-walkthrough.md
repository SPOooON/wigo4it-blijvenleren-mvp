# Reviewer Walkthrough

This document gives reviewers a short, repeatable path through the current MVP.

## Before you start

Recommended runtime:
- start the local container runtime with `docker compose up --build`
- open the app at `http://localhost:8080/`
- use the local API docs at `http://localhost:8080/docs`

Demo accounts:
- internal user: `internal.demo / Passw0rd!`
- fallback external contributor: `external.demo / Passw0rd!`

Social login setup note:
- the local Keycloak realm includes placeholder `GitHub` and `Google` identity providers
- they do not work out of the box because their client credentials are dummy values in the checked-in realm import
- to activate them, open `http://localhost:8081/`, sign in with `admin / admin`, open the `blijvenleren` realm, go to `Identity providers`, and replace the dummy client ID and client secret values
- if you do not configure that, use `external.demo` as the fallback external reviewer account

Expected seeded data:
- 3 learning resources
- internal comments already visible
- external comments in `Pending`
- external comments in `Rejected`

## Scenario 1: Anonymous browser review

Goal:
- confirm the browser surface is discoverable without logging in

Steps:
1. Open `http://localhost:8080/`.
2. Open `http://localhost:8080/LearningResources`.
3. Open one of the seeded resource detail pages.

Expected result:
- the landing page points clearly to the main browser and API review paths
- the learning-resource list shows seeded resources
- the details page shows approved comments only
- create, edit, moderation, and comment actions are not available to anonymous users

## Scenario 2: Internal user creates a learning resource

Goal:
- confirm the internal-user CRUD path works in the browser

Steps:
1. From the landing page, select `Login via local OIDC`.
2. Sign in as `internal.demo`.
3. Open `http://localhost:8080/LearningResources`.
4. Select `Add resource`.
5. Create a resource with a recognizable title.

Expected result:
- the internal user can reach the create page
- the new resource is saved and redirected to its details page
- the new resource appears in the resource list
- internal-only navigation such as moderation is available after login

## Scenario 3: External user submits a comment

Goal:
- confirm external comments are stored but not published immediately

Steps:
1. Sign out if needed.
2. Sign in through the preferred social path if you configured the broker credentials in Keycloak; otherwise sign in as `external.demo`.
3. Open any seeded learning-resource detail page.
4. Submit a new comment.

Expected result:
- comment submission succeeds
- the UI confirms the comment is waiting for moderation
- the newly submitted comment does not appear in the normal details view yet
- the external user cannot access internal-only routes

## Scenario 4: Internal user moderates the pending comment

Goal:
- confirm the moderation workflow exposes external comments after approval

Steps:
1. Sign out if needed.
2. Sign in as `internal.demo`.
3. Open `http://localhost:8080/Moderation/Comments`.
4. Find the pending comment from Scenario 3.
5. Approve it.
6. Return to the related learning-resource details page.

Expected result:
- the moderation queue shows pending external comments
- approval succeeds
- the approved comment is visible on the normal resource details page
- rejected comments remain hidden from the normal resource details page

## Scenario 5: Protected API review through the docs UI

Goal:
- confirm the documented API surface is usable for protected routes

Steps:
1. Obtain a bearer token from the local Keycloak realm using the token flow documented in `README.md`.
2. Open `http://localhost:8080/docs`.
3. Authorize the docs UI with the bearer token.
4. Call `GET /api/auth/me`.
5. If using an internal token, also call `GET /api/v1/comments/pending`.

Expected result:
- the docs UI loads the generated OpenAPI document
- protected endpoints accept the bearer token
- `GET /api/auth/me` returns the current username and mapped roles
- internal-only endpoints reject external tokens and succeed for internal tokens

## Notes

- The automated suite covers the main API and Razor Pages behavior, but not the full OIDC login redirect against the local Keycloak runtime.
- The prewired social-login brokers also remain manual-only because real provider credentials are not stored in the repo.
- If you want a smaller automated verification step, run `dotnet test test/BlijvenLeren.App.Tests/BlijvenLeren.App.Tests.csproj -c Release --filter FullyQualifiedName~BrowserSmoke`.
- If the local identity provider is still starting up, wait a few seconds and retry the login flow.
