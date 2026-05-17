# Review Agent Instructions

You review pull requests and diffs for the AIX monorepo.

## Checklist

### Architecture

- [ ] Backend changes respect bounded-context boundaries (no global `AIX.Application`)
- [ ] BC `Domain/` and `Application/` do not reference `AIX.Infrastructure` or other BC projects directly
- [ ] `AIX.Infrastructure` used only for composition/shared adapters — not as a domain dumping ground
- [ ] Controllers/endpoints remain thin; use cases live in BC `Application/`
- [ ] No NestJS or new Node backend apps introduced

### Naming

- [ ] New names follow [naming-conventions.md](../../docs/standards/naming-conventions.md)
- [ ] Backend projects/namespaces use `AIX.<Context>.<Layer>`; no vague `Manager`, `Helper`, `Util`, `Processor`, or generic `Service` types
- [ ] Commands/queries/handlers and DTOs use the prescribed suffixes; domain entities are not exposed on APIs
- [ ] Events: past tense internally; `V1` suffix for external integration events; SCREAMING_SNAKE_CASE only for serialized type constants
- [ ] DB snake_case, JSON camelCase, routes and Angular files kebab-case
- [ ] Artifact paths match `artifacts/openapi/*.v*.openapi.json` and `artifacts/schemas/*-schema.v*.json` when contracts are added

### Quality

- [ ] Tests added or updated for behavior changes
- [ ] No secrets or connection strings committed
- [ ] Nullable/types used correctly in C#; no `any` in TypeScript
- [ ] Tenant isolation considered for data access changes

### Frontend

- [ ] OnPush and signals patterns where applicable
- [ ] No duplicated server validation logic presented as authoritative in UI only

### Docs

- [ ] ADR or doc update if architectural decision changed
- [ ] `ai/tasks/current-task.md` aligned if task scope shifted

## Output format

1. **Summary** — one paragraph
2. **Blockers** — must fix before merge
3. **Suggestions** — optional improvements
4. **Verification** — commands reviewer should run

## Commands to recommend

```bash
cd backend && dotnet build && dotnet test
pnpm install && nx build aix-ui && nx test aix-ui
```
