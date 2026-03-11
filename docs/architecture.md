# Architecture

## Overview

The solution is designed as a locally runnable MVP with the following building blocks:

- Web application
- API
- Database
- Identity provider

## Primary flow

1. User authenticates through the identity provider
2. Web application calls API
3. API validates identity and authorization
4. API reads/writes learning resources and comments in the database
5. Internal moderation flow controls visibility of external comments

## Container view

- `web`: browser-facing frontend
- `api`: backend application exposing HTTP API
- `db`: persistent relational database
- `idp`: authentication and identity management

## Design goals

- local reproducibility
- traceable security boundaries
- separation of concerns
- easy interview walkthrough
- realistic path toward cloud deployment later

## Deployment scope decision

Terraform and actual deployment automation are excluded from this MVP.

Reasoning:
- The assignment is time-boxed and prioritizes demonstrable functional behavior.
- Core value for review is in domain logic, moderation flow, authentication boundaries and testability.
- Partial or shallow IaC would add complexity without improving confidence in core requirements.

Impact:
- No in-repo infrastructure provisioning scripts for this version.
- Cloud deployment remains a documented follow-up step.

## Diagrams

See:
- `../diagrams/context.mmd`
- `../diagrams/container.mmd`
- `../diagrams/auth-sequence.mmd`
