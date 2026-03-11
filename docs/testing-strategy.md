# Testing Strategy

## Current approach

The MVP currently uses automated tests at two levels:

- focused unit tests for learning-resource contract mapping and request validation
- integration tests for the first CRUD vertical slice across both API and browser routes

Issue `#12` keeps that structure intentionally small rather than introducing a separate browser-automation stack.

## Test stack

- `xUnit` for the test runner
- `Microsoft.AspNetCore.Mvc.Testing` for in-process application hosting
- `Microsoft.EntityFrameworkCore.InMemory` for isolated integration-test persistence

## Current coverage

Issue `#8` coverage:
- request validation for create requests
- contract mapping for list, detail, and create behavior

Issue `#9` coverage:
- seeded resource list/detail API responses
- internal-user create, update, and delete API happy paths
- role-based API denial for external contributors
- seeded browser list rendering
- internal-user create, edit, and delete browser happy paths through Razor Pages forms

Issue `#10` coverage:
- comment request validation
- internal-user API comments becoming visible immediately
- external-user API comments being stored as pending and hidden from normal detail reads
- internal-user browser comment submission showing the new comment immediately
- external-user browser comment submission succeeding without surfacing the pending comment yet

Issue `#11` coverage:
- internal-user pending-comment list API reads
- internal-user approve and reject moderation API paths
- invalid moderation attempts against non-pending or internal comments
- internal-user browser moderation flow approving a pending comment and making it visible in the normal detail page

Issue `#12` suite formalization:
- the `BrowserSmoke` test trait marks the smallest review-friendly end-to-end Razor Pages path
- `dotnet test BlijvenLeren.sln -c Release` runs the full suite reproducibly
- `dotnet test test/BlijvenLeren.App.Tests/BlijvenLeren.App.Tests.csproj -c Release --filter FullyQualifiedName~BrowserSmoke` runs only the smoke path

## Deliberate limits

- The integration tests replace PostgreSQL with EF Core InMemory, so they verify application behavior rather than provider-specific SQL behavior.
- The browser coverage is server-rendered Razor Pages exercised through HTTP, not full browser automation.
- This repo intentionally does not add a second UI-test framework yet because the current UI is still simple server-rendered pages and the assignment favors pragmatic scope.
- The OIDC login redirect against local Keycloak is not part of the automated suite yet; it is still verified manually in the compose runtime.

## Follow-up direction

- Add provider-realistic persistence tests if query complexity increases.
- Add browser automation if the UI becomes more interactive than basic Razor Pages forms.
- Add compose-level smoke checks once CI/runtime setup becomes part of the assignment scope.

## Review traceability

- Requirement coverage lives in `docs/functional-requirements.md`.
- Main code and test entry points for each implemented slice live in `docs/traceability-map.md`.
- Security-sensitive behavior under test is summarized in `docs/security.md`.
- Remaining testing gaps are tracked in `docs/backlog.md`.
