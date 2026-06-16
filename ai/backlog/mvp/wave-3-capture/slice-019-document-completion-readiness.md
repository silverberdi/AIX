# Slice 019 — Document Completion Readiness

## Goal

Introduce capture completion readiness evaluation that determines whether a Draft document satisfies MVP capture rules (validated metadata present + primary file attached) before allowing completion.

## Scope

- `DocumentCaptureReadiness` evaluator (static class or domain service) in `AIX.Documents`
- Readiness checks:
  - Captured metadata is present (not null/empty per defined policy)
  - Metadata passes version-scoped validation via injected port
  - Primary file attached (`DocumentFileRole.Primary` exists per slice 004)
- Expose `EvaluateCaptureReadiness(document, validator, ...)` returning `Result` or dedicated readiness value
- New domain errors: e.g. `MetadataRequired`, `MetadataInvalid`, `PrimaryFileRequired`, `CaptureNotReady`
- Does **not** change `Complete()` behavior yet (slice 020)

## Out of Scope

- Changing `Complete()` to enforce readiness (slice 020)
- Supporting file requirements schema (deferred)
- Persistence, EF Core, repositories
- HTTP APIs, MediatR
- Angular UI, PrimeNG, Avalon
- Runtime multi-tenancy, storage providers
- OCR, search, retention, workflows

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Documents` (Domain) | Yes |
| `AIX.Documents.Tests` | Yes |
| `AIX.Documents.Contracts` | Read port types |
| `AIX.Metadata` | Test adapter only |

## Dependencies

- **slice-004** — primary file attachment rules
- **slice-017** — metadata on Document
- **slice-018** — validation port

## Acceptance Criteria

- Ready when valid metadata + primary file present
- Not ready when metadata missing
- Not ready when metadata fails validation
- Not ready when primary file missing (even if metadata valid)
- Not ready when only supporting files attached (no primary)
- Complete documents: readiness evaluation returns not-ready or is not applicable (document already complete)
- Readiness is pure evaluation — no state mutation

## Testing Expectations

- Minimum scenarios:
  - ready_when_valid_metadata_and_primary_file
  - not_ready_when_metadata_missing
  - not_ready_when_metadata_invalid
  - not_ready_when_primary_file_missing
  - not_ready_when_only_supporting_files
  - not_ready_when_no_files
  - readiness_does_not_mutate_document_state
- Full regression: all prior Wave 3 slice tests + Wave 0–2

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`; readiness evaluator tested; `Complete()` behavior unchanged from slice 018 baseline.

## Post-Slice Memory / Backlog Sync

1. `index.md` — slice 019 **Done**
2. `memory.md` — completion readiness rules
3. `current-state.md` — Wave 3 progress
4. `current-task.md` — point to slice-020

## Patterns Used

- Specification / policy object
- Pure domain evaluation

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
