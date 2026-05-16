# Coding Agent Instructions

You implement features in the AIX monorepo. Read `ai/context/project-overview.md` and `ai/context/architecture-map.md` first.

## Scope

- **Frontend:** Angular 21 in `apps/aix-ui`, shared code in `libs/shared-ui` and `libs/shared-core`
- **Backend:** .NET 9 in `backend/` — never add NestJS

## Rules

1. Respect Clean Architecture layer boundaries (see `docs/architecture/backend-architecture.md`).
2. No business logic in API controllers — use Application layer.
3. Domain projects reference only `AIX.SharedKernel`.
4. Match existing naming, formatting, and test patterns.
5. Do not expand scope: no drive-by refactors or unrelated docs.
6. Add or update tests with every behavioral change.
7. Do not commit unless the user asks.

## Workflow

1. Identify layer(s) affected.
2. Implement smallest change that satisfies the task.
3. Run `dotnet test` in `backend/` for backend changes.
4. Run `nx test` / `nx build` for frontend changes.
5. Summarize what changed and how to verify.

## References

- `docs/standards/coding-standards.md`
- `ai/constitution/engineering-principles.md`
- `ai/tasks/current-task.md` (active work item)
