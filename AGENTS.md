# AGENTS.md

## Purpose

This repository is used for a job application assignment.  
All work should be transparent and reviewable by interviewers.

The repository must therefore show not only the code, but also:
- what was built
- what was not built
- why decisions were made
- how the solution evolved

## Working principles

- Prefer clear, reviewable progress over large opaque changes.
- Keep all important documentation in the repository.
- Update docs whenever code changes alter scope, behavior or architecture.
- Favor pragmatic MVP solutions over unnecessary completeness.
- Do not introduce complexity unless it clearly supports a requirement from the brief.
- Make trade-offs explicit.

## Documentation requirements

For every meaningful change, keep the following in sync where relevant:
- `README.md`
- `docs/functional-requirements.md`
- `docs/technical-decisions.md`
- `docs/architecture.md`
- `docs/testing-strategy.md`
- `docs/security.md`
- `docs/backlog.md`

When making a decision, document:
- the decision
- the reason
- the impact
- rejected alternatives if relevant

## Functional scope handling

When implementing a feature:
1. Map it to a requirement in `docs/functional-requirements.md`
2. Implement the smallest coherent version that demonstrates the idea
3. Document any simplifications or omissions
4. Add deferred work to `docs/backlog.md` if needed

## Code change rules

- Keep changes scoped to the task at hand.
- Avoid unrelated refactors.
- Prefer straightforward code over clever code.
- Keep naming and structure boring and predictable.
- Add tests for behavior that is implemented.
- If something is not tested, state why.

## Architecture and infrastructure rules

- Keep the application runnable locally.
- Use containers for app, database and identity provider.
- Keep Terraform in the repo and treat it as part of the solution.
- Do not require real cloud deployment for the solution to be understandable or demonstrable.
- Favor a structure that can later be migrated to real cloud infrastructure.

## Security expectations

- Treat authentication and authorization as first-class concerns.
- Do not store secrets in source control.
- Use safe defaults in configuration.
- Document security shortcuts taken for local/demo purposes.

## Diagrams

- Keep diagram source files in `diagrams/`.
- Keep diagrams aligned with implementation and docs.
- Prefer Mermaid unless there is a strong reason not to.

## Commits and pull requests

- Use small, descriptive commits.
- Reference the requirement or decision being addressed where useful.
- Summaries should explain both the change and the reason.
- For demonstration, open a GitHub pull request for each feature branch so reviewers can inspect the change set and rationale.

## Work management

Work should be tracked through GitHub issues.

When starting work:
1. Pick an open issue.
2. If the issue is unclear, incomplete, or contains conflicting requirements, ask clarifying questions **in the issue comments before starting implementation**.
3. Wait for clarification before proceeding if the missing information could affect the design or implementation.
4. Once the issue is sufficiently clear, create a feature branch.
5. Implement the task.
6. Open a pull request referencing the issue.

### Clarifications

If requirements or scope are unclear:

- Ask questions in the **issue comments**, not in pull requests.
- Keep questions concrete and actionable.
- Do not guess architectural decisions if they are not documented.

## Definition of done for a task

A task is only done when:
- the code is updated
- relevant tests are updated or added
- relevant docs are updated
- relevant diagrams are updated if architecture or deployment changed
- deferred work is captured explicitly
