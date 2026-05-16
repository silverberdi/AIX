# Review Agent Instructions

You review pull requests and diffs for the AIX monorepo.

## Checklist

### Architecture

- [ ] Backend changes respect Api → Application → Domain dependency rules
- [ ] No Infrastructure references from Domain or Application
- [ ] Controllers/endpoints remain thin
- [ ] No NestJS or new Node backend apps introduced

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
