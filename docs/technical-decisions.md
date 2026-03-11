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
