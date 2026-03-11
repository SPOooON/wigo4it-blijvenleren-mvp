# BlijvenLeren MVP

This repository contains my implementation of the BlijvenLeren programming case.

The goal of this project is not to fully productize the platform, but to demonstrate how I approach:
- application design
- DevOps practices
- security-conscious development
- documentation and technical decision making

## Assignment intent

The original brief is intentionally open-ended and expects pragmatic choices rather than a perfect or complete implementation. This repository therefore focuses on:
- a working MVP where feasible
- clear technical trade-offs
- traceable documentation of functional requirements and architectural decisions
- a local, containerized setup for application, database and identity provider
- diagrams that explain the architecture and authentication model

## What is included

- .NET application code
- API for the main use cases
- Web application for the main use cases
- Persistent database
- Identity provider for authentication
- Containerized local runtime
- Documentation for requirements, architecture, security, testing and decisions
- Diagrams for system context, containers and authentication

## What is intentionally out of scope

- Deployment to a real cloud environment
- Infrastructure provisioning and deployment automation (including Terraform)
- Full production hardening
- Full implementation of every possible edge case
- Enterprise-grade social login integration beyond what is needed to demonstrate the direction

See `docs/scope-and-assumptions.md` for details.

## Quick start

### Prerequisites

- Docker Desktop
- .NET SDK
- Optional: `just` or `make`

### Run locally

```bash
docker compose up --build
```

## Why Terraform is not included in this MVP

Infrastructure provisioning is deliberately out of scope for this assignment iteration.
The available time is used to maximize demonstrable value in:
- core product behavior
- authentication and authorization flow
- data model and moderation flow
- testability and reviewable technical decisions

Terraform and cloud deployment design are treated as deferred work and can be added once the core MVP behavior is validated.

## Review process for demonstration

For demonstration purposes, changes are reviewed through GitHub rather than only through local diffs.
Work is done on feature branches and submitted as pull requests so interviewers can review:
- commit history and scope
- rationale in PR descriptions
- evolution of decisions over time
