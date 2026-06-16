# Slice 020 — Complete With Capture Enforcement

## Goal

Evolve the Document `Complete` operation to enforce capture readiness so Draft → Complete only succeeds when metadata is valid and MVP file requirements are satisfied.

## Scope

- `Complete()` calls capture readiness evaluation before state transition
- Fail with deterministic errors when not capture-ready (no state change, no `DocumentCompleted` event)
- Succeed when capture-ready; emit `DocumentCompleted` as today
- Preserve existing behavior: already-Complete fails with `AlreadyComplete`
- Preserve post-Complete immutability for metadata and files
- Update existing `CompleteDocumentTests` to satisfy new readiness requirements in test helpers

## Out of Scope

- Persistence, EF Core, repositories
- HTTP APIs, MediatR
- Angular UI, PrimeNG, Avalon
- Runtime multi-tenancy, storage providers
- OCR, search, retention, workflows
- File requirement schema beyond primary file rule

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Documents` (Domain) | Yes |
| `AIX.Documents.Tests` | Yes |
| `AIX.Documents.Contracts` | Read port types |

## Dependencies

- **slice-019** — `DocumentCaptureReadiness` evaluator

## Acceptance Criteria

- Complete succeeds when document is capture-ready
- Complete fails when metadata missing
- Complete fails when metadata invalid
- Complete fails when primary file missing
- Complete fails when already Complete (unchanged)
- Failed complete does not emit `DocumentCompleted`
- Failed complete leaves document in Draft
- Complete document rejects metadata and file mutations (Wave 0 immutability preserved)
- Prior domain events preserved on successful complete

## Testing Expectations

- Minimum scenarios:
  - complete_succeeds_when_capture_ready
  - complete_fails_when_metadata_missing
  - complete_fails_when_metadata_invalid
  - complete_fails_when_primary_file_missing
  - complete_fails_when_already_complete
  - failed_complete_emits_no_completed_event
  - failed_complete_preserves_draft_state
  - complete_document_rejects_metadata_and_file_changes
- Full regression: all Documents + Metadata tests

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`; complete enforcement tested; no placeholder logic.

## Post-Slice Memory / Backlog Sync

1. `index.md` — slice 020 **Done**
2. `memory.md` — capture-enforced completion summary
3. `current-state.md` — Wave 3 progress
4. `current-task.md` — point to slice-021 (optional, last-priority) or Wave 3 Domain Review if 021 is deferred

## Patterns Used

- Aggregate invariant enforcement
- Policy gate on state transition

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
