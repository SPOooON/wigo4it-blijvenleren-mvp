# Traceability Map

This document is a compact "where to look" guide for the current MVP.

Use it together with:
- `docs/functional-requirements.md` for requirement status and scope
- `docs/testing-strategy.md` for test intent and deliberate gaps
- `docs/reviewer-walkthrough.md` for a short guided demo path

## Learning-resource list and details

Requirements:
- `FR-01`
- `FR-02`

Primary docs:
- `docs/functional-requirements.md`
- `docs/architecture.md`

Main code entry points:
- `src/BlijvenLeren.App/Features/LearningResources/LearningResourceEndpointRouteBuilderExtensions.cs`
- `src/BlijvenLeren.App/Pages/LearningResources/Index.cshtml.cs`
- `src/BlijvenLeren.App/Pages/LearningResources/Details.cshtml.cs`
- `src/BlijvenLeren.App/Features/LearningResources/LearningResourceContractMapper.cs`
- `src/BlijvenLeren.App/Contracts/V1/LearningResourceContracts.cs`

Main automated tests:
- `test/BlijvenLeren.App.Tests/ApiResourceCrudIntegrationTests.cs`
- `test/BlijvenLeren.App.Tests/BrowserResourceCrudIntegrationTests.cs`
- `test/BlijvenLeren.App.Tests/LearningResourceContractMapperTests.cs`

## Learning-resource create, edit, and delete

Requirements:
- `FR-03`
- `FR-04`
- `FR-05`

Primary docs:
- `docs/functional-requirements.md`
- `docs/architecture.md`
- `docs/security.md`

Main code entry points:
- `src/BlijvenLeren.App/Features/LearningResources/LearningResourceEndpointRouteBuilderExtensions.cs`
- `src/BlijvenLeren.App/Pages/LearningResources/Create.cshtml.cs`
- `src/BlijvenLeren.App/Pages/LearningResources/Edit.cshtml.cs`
- `src/BlijvenLeren.App/Pages/LearningResources/Details.cshtml.cs`
- `src/BlijvenLeren.App/Features/LearningResources/LearningResourceRequestValidator.cs`
- `src/BlijvenLeren.App/Features/LearningResources/LearningResourceContractMapper.cs`

Main automated tests:
- `test/BlijvenLeren.App.Tests/ApiResourceCrudIntegrationTests.cs`
- `test/BlijvenLeren.App.Tests/BrowserResourceCrudIntegrationTests.cs`
- `test/BlijvenLeren.App.Tests/LearningResourceRequestValidatorTests.cs`

## Comment submission and visibility rules

Requirements:
- `FR-06`
- `FR-07`
- `FR-08`

Primary docs:
- `docs/functional-requirements.md`
- `docs/architecture.md`
- `docs/security.md`

Main code entry points:
- `src/BlijvenLeren.App/Features/Comments/CommentEndpointRouteBuilderExtensions.cs`
- `src/BlijvenLeren.App/Pages/LearningResources/Details.cshtml.cs`
- `src/BlijvenLeren.App/Features/Comments/CommentSubmissionFactory.cs`
- `src/BlijvenLeren.App/Features/Comments/CommentRequestValidator.cs`
- `src/BlijvenLeren.App/Features/LearningResources/LearningResourceContractMapper.cs`

Main automated tests:
- `test/BlijvenLeren.App.Tests/ApiResourceCrudIntegrationTests.cs`
- `test/BlijvenLeren.App.Tests/BrowserResourceCrudIntegrationTests.cs`
- `test/BlijvenLeren.App.Tests/CommentRequestValidatorTests.cs`

## Moderation queue and approve/reject flow

Requirements:
- `FR-09`
- `FR-10`

Primary docs:
- `docs/functional-requirements.md`
- `docs/architecture.md`
- `docs/security.md`

Main code entry points:
- `src/BlijvenLeren.App/Features/Comments/CommentEndpointRouteBuilderExtensions.cs`
- `src/BlijvenLeren.App/Pages/Moderation/Comments.cshtml.cs`
- `src/BlijvenLeren.App/Features/Comments/CommentModerationValidator.cs`
- `src/BlijvenLeren.App/Features/LearningResources/LearningResourceContractMapper.cs`

Main automated tests:
- `test/BlijvenLeren.App.Tests/ApiResourceCrudIntegrationTests.cs`
- `test/BlijvenLeren.App.Tests/BrowserResourceCrudIntegrationTests.cs`

## Authentication and authorization boundaries

Requirement:
- `FR-11`

Primary docs:
- `docs/functional-requirements.md`
- `docs/architecture.md`
- `docs/security.md`

Main code entry points:
- `src/BlijvenLeren.App/Features/Auth/AuthEndpointRouteBuilderExtensions.cs`
- `src/BlijvenLeren.App/Features/Auth/LoginRequestBuilder.cs`
- `src/BlijvenLeren.App/Security/ClaimsPrincipalFactory.cs`
- `src/BlijvenLeren.App/Security/AuthorityRewriteHandler.cs`
- `src/BlijvenLeren.App/Pages/Protected.cshtml.cs`
- `src/BlijvenLeren.App/Pages/Index.cshtml.cs`
- `infra/keycloak/realm-import/blijvenleren-realm.json`

Main automated tests:
- `test/BlijvenLeren.App.Tests/ApiResourceCrudIntegrationTests.cs`
- `test/BlijvenLeren.App.Tests/BrowserResourceCrudIntegrationTests.cs`
- `test/BlijvenLeren.App.Tests/LoginRequestBuilderTests.cs`
- `test/BlijvenLeren.App.Tests/Infrastructure/TestAuthHandler.cs`
- `test/BlijvenLeren.App.Tests/Infrastructure/TestApplicationFactory.cs`

## API surface and local docs

Requirements:
- `FR-12`

Primary docs:
- `docs/functional-requirements.md`
- `docs/architecture.md`
- `README.md`

Main code entry points:
- `src/BlijvenLeren.App/Features/Auth/AuthEndpointRouteBuilderExtensions.cs`
- `src/BlijvenLeren.App/Features/Runtime/RuntimeEndpointRouteBuilderExtensions.cs`
- `src/BlijvenLeren.App/Features/LearningResources/LearningResourceEndpointRouteBuilderExtensions.cs`
- `src/BlijvenLeren.App/Features/Comments/CommentEndpointRouteBuilderExtensions.cs`
- `src/BlijvenLeren.App/OpenApi/OpenApiDocumentConfiguration.cs`
- `src/BlijvenLeren.App/OpenApi/OpenApiEndpointConventionBuilderExtensions.cs`
- `src/BlijvenLeren.App/Contracts/V1/LearningResourceContracts.cs`

Main automated tests:
- `test/BlijvenLeren.App.Tests/OpenApiIntegrationTests.cs`
- `test/BlijvenLeren.App.Tests/ApiResourceCrudIntegrationTests.cs`

## Browser surface and reviewer entry points

Requirement:
- `FR-13`

Primary docs:
- `docs/functional-requirements.md`
- `docs/reviewer-walkthrough.md`
- `README.md`

Main code entry points:
- `src/BlijvenLeren.App/Pages/Index.cshtml`
- `src/BlijvenLeren.App/Pages/Index.cshtml.cs`
- `src/BlijvenLeren.App/Pages/Shared/_Layout.cshtml`
- `src/BlijvenLeren.App/Pages/LearningResources/Index.cshtml`
- `src/BlijvenLeren.App/Pages/Protected.cshtml`

Main automated tests:
- `test/BlijvenLeren.App.Tests/BrowserResourceCrudIntegrationTests.cs`

## Test infrastructure

If you want the shared automated-test host setup first, start here:
- `test/BlijvenLeren.App.Tests/Infrastructure/TestApplicationFactory.cs`
- `test/BlijvenLeren.App.Tests/Infrastructure/TestAuthHandler.cs`

These files explain how the suite boots the app, swaps authentication for controlled tests, and keeps the browser and API integration checks reproducible.
