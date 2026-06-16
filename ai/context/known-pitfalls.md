# Known Pitfalls

Things agents must **avoid**. Reviewers should flag violations as blockers when they break slice scope or architecture.

## Repository and documentation

- Creating **`.ai/`** instead of using **`ai/`**
- **Duplicating** existing documentation (new files that repeat `docs/` or `ai/context/` content)
- **Rewriting** architecture docs unnecessarily during implementation tasks
- Marking a slice **Done** without Definition of Done verification and passing tests

## Scope creep during slices

- **Implementing APIs** during domain-only slices
- **Adding EF Core** or persistence during domain-only slices
- **Installing PrimeNG or Avalon** outside the dedicated UI setup task
- **Touching unrelated bounded contexts** not listed in the active slice
- Adding **MediatR**, global `AIX.Application`, or NestJS backends

## Design anti-patterns

- **Generic CRUD services** or vague `Manager` / `Helper` / `Util` types
- **Broad abstractions** before concrete slice pressure exists (e.g. generic repositories, catch-all base classes)
- Putting **business logic in API controllers** or **domain rules in `AIX.SharedKernel`**
- Treating **skeleton/placeholder code** as legacy code to preserve or refactor wholesale

## Execution mistakes

- Skipping **`ai/context/memory.md`** and **`ai/tasks/current-task.md`** at session start
- Expanding scope with drive-by refactors, extra docs, or unrelated features
- Assuming Wave 0 needs persistence/API/UI because “the app will need them eventually”
