# Active Project Decisions

Locked choices for agents and reviewers. Change only via explicit user decision and doc/ADR update.

## Repository and AI workflow

- Use **`ai/`** for agent context and instructions — **never** create **`.ai/`**
- Test-first execution is **mandatory** for behavioral changes
- Slices are the unit of delivery; verify Definition of Done before marking **Done**

## Frontend

- **Angular 21** is the official SPA framework
- **PrimeNG 21** is the official UI component library
- **Avalon** is the official UI theme/template
- **Nx** monorepo for frontend; backend lives under `backend/` (not in Nx graph)
- **No UI implementation** before a UI slice explicitly requires it

## Backend

- **.NET 9** is the official backend runtime
- **PostgreSQL** is the canonical database
- **Modular monolith** before microservices
- **No GraphQL** during MVP unless explicitly decided later
- **No event sourcing** during MVP
- **No generic repository pattern** by default — introduce persistence only when a slice requires it
- **No persistence work** before the corresponding slice
- Bounded contexts reference **`AIX.SharedKernel` only** for shared primitives

## Integration and patterns

- Cross-BC integration via **contracts and events**, not project references between BCs
- **SharedKernel must remain minimal** — no business rules or BC-specific types
- Vertical Slice Architecture with explicit handoff in `ai/tasks/current-task.md`
