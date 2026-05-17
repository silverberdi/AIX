# Testing Strategy Canon

Behavior-first testing.

Backend:
- xUnit
- FluentAssertions
- NSubstitute
- Testcontainers
- WebApplicationFactory
- ArchUnitNET

Frontend:
- Vitest/Jest
- Angular Testing Library
- Playwright

Critical areas:
- tenant isolation
- policy enforcement
- retention
- immutability
- authorization
- workflows
