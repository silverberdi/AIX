# Coding Agent Instructions

You implement features in the AIX monorepo. Read `ai/context/project-overview.md` and `ai/context/architecture-map.md` first.

## Scope

- **Frontend:** Angular 21 in `apps/aix-ui`, shared code in `libs/shared-ui` and `libs/shared-core`
- **Backend:** .NET 9 in `backend/` — never add NestJS

## Rules

1. Respect modular monolith boundaries (see `docs/architecture/backend-architecture.md`).
2. No business logic in API controllers — use the bounded context `Application/` layer.
3. Do not add a global `AIX.Application` project; each BC owns `Domain/`, `Application/`, `Infrastructure/`, `Contracts/`, `Events/`.
4. BC projects reference only `AIX.SharedKernel`; cross-BC integration via contracts/events, not project references.
5. `AIX.Infrastructure` is for composition and shared adapters only — not BC-specific domain rules or repositories.
6. Follow [naming-conventions.md](../../docs/standards/naming-conventions.md) for all new types, files, routes, APIs, events, policies, and artifacts. Match existing names in touched code unless a task explicitly renames them.
7. Do not expand scope: no drive-by refactors or unrelated docs.
8. Add or update tests with every behavioral change.
9. Do not commit unless the user asks.

## Workflow

1. Identify bounded context(s) and layer(s) affected.
2. Implement smallest change that satisfies the task.
3. Run `dotnet test` in `backend/` for backend changes.
4. Run `nx test` / `nx build` for frontend changes.
5. Summarize what changed and how to verify.

## References

- `docs/standards/naming-conventions.md` (required for naming)
- `docs/standards/coding-standards.md`
- `ai/constitution/engineering-principles.md`
- `ai/tasks/current-task.md` (active work item)
