# Architecture

## Overview

The solution is designed as a locally runnable MVP with the following building blocks:

- Combined ASP.NET Core application hosting browser UI and API endpoints
- Database
- Identity provider

## Primary flow

1. User authenticates through the identity provider
2. Web application calls API
3. API validates identity and authorization
4. API reads/writes learning resources and comments in the database
5. Internal moderation flow controls visibility of external comments

## Container view

- `app`: browser-facing frontend and HTTP API hosted in one ASP.NET Core app for the bootstrap phase
- `db`: persistent relational database
- `idp`: authentication and identity management

## Current project structure

The runnable application code currently lives in:

- `src/BlijvenLeren.App`: single ASP.NET Core Razor Pages application with minimal API endpoints
- `BlijvenLeren.sln`: top-level solution file for local build and future expansion

Current bootstrap behavior:
- `/` serves a placeholder browser page
- `/api/health` serves a placeholder API health response

This keeps the startup slice small while still demonstrating both browser and API access paths.

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

## Bootstrap scope trade-off

This repository currently uses one ASP.NET Core app instead of separate web and API projects.

Reasoning:
- The current load and feature scope do not justify an early split.
- A combined app reduces setup overhead for a sample assignment.
- The repository still demonstrates both browser and API entry points without premature structure.

Impact:
- Reviewers can run one process to see both surfaces.
- A later split remains possible if the application grows or deployment concerns become stricter.
- Architecture documentation must explicitly record that this is an MVP simplification rather than a final scaling choice.

## Diagrams

See:
- `../diagrams/context.mmd`
- `../diagrams/container.mmd`
- `../diagrams/auth-sequence.mmd`
