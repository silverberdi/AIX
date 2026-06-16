# Current Task

## Before you start

Load operational memory (read in order):

1. `ai/context/memory.md`
2. `ai/context/current-state.md`
3. `ai/context/active-decisions.md`
4. `ai/context/known-pitfalls.md`

Then continue with this file.

## Active slice

**slice-017-document-metadata-attachment** — Wave 3 — Capture MVP

- Slice file: `ai/backlog/mvp/wave-3-capture/slice-017-document-metadata-attachment.md`
- Planning reference: `openspec/changes/wave-3-capture-mvp-planning/design.md`
- Wave 3 folder: `ai/backlog/mvp/wave-3-capture/`

## Context

Wave 2 and **Wave 2 Domain Review** are **Done** (review verified 2026-05-23, **PASS**). Slice 016 introduced passive capture contracts in `AIX.Documents.Contracts`. Core path is 016–020; slice 021 is last-priority and optional.

### Wave 3 slice order

| Slice | Status |
|-------|--------|
| slice-016-capture-metadata-contracts | **Done** |
| slice-017-document-metadata-attachment | **Next** |
| slice-018-capture-validation-port | Pending |
| slice-019-document-completion-readiness | Pending |
| slice-020-complete-with-capture-enforcement | Pending |
| slice-021-capture-context-contract | Pending (optional, last priority) |

## Execution rules

1. **Test-first** — add or extend behavior tests before or alongside domain code.
2. **Read before edit** — memory files above, slice file, and relevant `docs/`.
3. **Bounded scope** — touch only projects listed in the active slice.
4. **No drift** — no persistence, APIs, EF Core, MediatR, UI, or unrelated BCs unless the slice requires it.
5. **Minimal change** — satisfy the slice only.

## Validation

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```

## After each Wave 3 slice

1. Update `ai/context/memory.md`, `ai/context/current-state.md`, and `ai/backlog/mvp/index.md`.
2. Point this file at the next Wave 3 slice.
