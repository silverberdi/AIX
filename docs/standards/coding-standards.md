# Coding Standards

Initial conventions for AIX. Refine via ADRs as the codebase grows.

## General

- Strict typing; no `any` (TypeScript) or unjustified `dynamic` (C#)
- API is the source of truth for contracts
- No duplicated validation between UI and backend
- No tenant data leakage across boundaries
- Prefer small, focused types and functions

## C# / Backend

- Target **.NET 9**; nullable reference types enabled
- Follow layer boundaries (see [backend-architecture.md](../architecture/backend-architecture.md))
- Controllers/endpoints: thin — no business logic
- Use cases live in **Application**; persistence in **Infrastructure**
- Domain entities: no EF attributes; map in Infrastructure
- Validation: FluentValidation at Application boundaries (when introduced)
- Async all I/O-bound paths; avoid `.Result` / `.Wait()`
- Naming: `PascalCase` types/methods; `_camelCase` private fields
- Tests: xUnit + FluentAssertions + NSubstitute; one assertion focus per test when practical

## Angular / Frontend

- **Signals-first** state; avoid unnecessary RxJS for local UI state
- `ChangeDetectionStrategy.OnPush` on components
- PrimeNG for UI primitives; theme aligned with Avalon
- Smart/dumb component split: containers fetch; presentational components receive inputs
- No business rules in templates — delegate to services or backend
- Path aliases: `@aix/shared-core`, `@aix/shared-ui`
- Tests: Jest (app), Vitest (libs) via Nx targets

## Tests

| Area | Runner | Location |
|------|--------|----------|
| Backend | `dotnet test` | `backend/tests/` |
| Angular app | `nx test aix-ui` | `apps/aix-ui` |
| Shared libs | `nx test shared-ui` etc. | `libs/` |
| E2E | `nx e2e aix-ui-e2e` | Playwright |

- Placeholder tests are acceptable in scaffold phase; replace with real specs when implementing features
- Name tests: `MethodName_Scenario_ExpectedResult` or descriptive sentence style

## Git & reviews

- Conventional, imperative commit subjects
- PRs must not break `nx build aix-ui` or `dotnet test` in `backend/`
