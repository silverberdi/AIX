# Slice 011 — Document Type Version Schema Composition

## Goal

Attach an immutable **version schema composition** to each new `DocumentTypeVersion`: the structural snapshot of standalone fields that defines what data exists for documents bound to that version.

## Scope

- Extend `DocumentTypeVersion` (or companion immutable record owned by version) with composition payload: ordered standalone `FieldSchema` entries
- Composition supplied at `DocumentType.CreateVersion` time (auto or explicit version number flows)
- Immutability: existing versions unchanged when new version created; composition frozen on version record
- Empty composition allowed for backward-compatible tests; non-empty composition validated
- Duplicate field keys / ids within a version rejected
- `DocumentTypeVersionCreated` event extended or companion event with composition snapshot (ids + stable field keys, not full registry entities)
- Align with `docs/domain/document-type-versioning.md`: structural change = new version; no retroactive mutation

## Out of Scope

- `KeywordGroup` references on version (slice 012)
- Layout sections and presentation (slice 013)
- Default layout generation (slice 013/014)
- Sequential-only version number policy (optional future rule — document if gaps remain allowed)
- Inactive `DocumentType` blocking `CreateVersion` (monitor only unless product rule added here)
- JSONB storage, persistence, APIs
- Documents BC validation of type existence

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Metadata` (Domain, Events) | Yes |
| `AIX.Metadata.Tests` | Yes |
| `AIX.SharedKernel` | Avoid |
| Other BCs / Api | No |

## Dependencies

- **slice-006** — `DocumentType`, `DocumentTypeVersion`, `CreateVersion`
- **slice-010** — `FieldSchema` primitives

## Acceptance Criteria

- `CreateVersion` accepts composition (or builder) and persists snapshot on new version record
- Prior versions retain original composition unchanged
- Duplicate field identity within one version fails
- Invalid field schema entries fail before version append
- `LatestVersion` reflects new version with its composition
- Event payload sufficient for downstream projection of version schema (field ids + keyword ids + catalog types)
- All Metadata tests pass including versioning regression tests

## Testing Expectations

- Extend `DocumentTypeVersioningTests` or dedicated composition test class
- Minimum scenarios:
  - create_version_with_empty_composition_succeeds
  - create_version_with_field_composition_succeeds
  - prior_version_composition_unchanged_after_new_version
  - rejects_duplicate_field_in_composition
  - rejects_invalid_field_schema_in_composition
  - version_created_event_includes_composition_snapshot
- Regression: slices 005–010 behaviors unchanged

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md` — build green, tests green, no forbidden patterns, immutability preserved.

## Post-Slice Memory / Backlog Sync

1. `ai/backlog/mvp/index.md` — slice 011 **Done**
2. `ai/context/memory.md` — note version composition model and event shape
3. `ai/context/current-state.md` — test counts, active slice → 012
4. `ai/tasks/current-task.md` — point to slice 012 file

## Patterns Used

- Aggregate Root (extend `DocumentType`)
- Immutable snapshot on version record
- Domain Event

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
