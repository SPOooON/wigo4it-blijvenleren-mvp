# Scope and Assumptions

## Goal

Deliver a pragmatic MVP that demonstrates sound engineering and DevOps practices within the time constraints of the assignment.

## In scope

- .NET-based application
- Browser-accessible web application
- API for core use cases
- Authentication via local identity provider
- Persistent database
- Containerized local environment
- Documentation and diagrams
- Automated tests for key behavior

## Out of scope

- Real cloud deployment
- Infrastructure provisioning and deployment automation (including Terraform)
- Full production-grade HA setup
- Full social login implementation
- Advanced tenant isolation
- Full observability platform
- Full performance validation for 500 real concurrent users

## Assumptions

- A local/demo identity provider is acceptable to demonstrate the authentication architecture.
- The API and web app do not need every edge case implemented to show approach and quality.
- Deployment strategy can be documented conceptually without implementing IaC artifacts in this MVP.
- Performance and scalability can be addressed through design notes and basic measurements, even if not fully benchmarked.

## Deliberate trade-offs

- Prefer demonstrable application behavior over partial infrastructure automation.
- Excluding Terraform reduces setup overhead and keeps scope focused on functional requirements and security-critical flows.
- Prefer clarity and maintainability over maximum feature count.
- Prefer documented decisions over implicit assumptions.
