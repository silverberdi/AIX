# Slice 017 — Document Metadata Attachment

## Goal

Enable the `Document` aggregate to hold captured metadata while in Draft state, emit domain events on capture, and reject metadata mutations after Complete.

## Scope

- Documents-owned value type (e.g. `DocumentCapturedMetadata`) aligned with `CapturedMetadataPayload` contract shape
- `Document` stores optional captured metadata (null until first set)
- `SetCapturedMetadata` (or equivalent) on Draft documents — replace semantics on subsequent calls
- Reject metadata changes when `State == Complete` with deterministic error
- Emit `DocumentMetadataCaptured` domain event with document id, correlation, and capture snapshot
- Metadata does not alter immutable `DocumentTypeId` / `DocumentTypeVersionId`
- No validation gate in this slice (validation added in slice 018)

## Out of Scope

- Version-scoped validation (slice 018)
- Completion readiness (slice 019)
- Persistence, EF Core, repositories
- HTTP APIs, MediatR
- Angular UI, PrimeNG, Avalon
- Runtime multi-tenancy, storage providers
- OCR, search, retention, workflows

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Documents` (Domain, Events) | Yes |
| `AIX.Documents.Tests` | Yes |
| `AIX.Documents.Contracts` | Read contract types for alignment |
| `AIX.Metadata` | No |

## Dependencies

- **slice-016** — `CapturedMetadataPayload` contract shape

## Acceptance Criteria

- Draft document accepts first metadata capture
- Draft document replaces metadata on subsequent capture
- New document may have null metadata until explicitly set
- Complete document rejects metadata update with `CannotModifyWhenComplete` or dedicated metadata error
- Successful capture emits `DocumentMetadataCaptured` event
- `DocumentTypeId` and `DocumentTypeVersionId` unchanged after metadata operations
- Existing file attachment and create behaviors unchanged

## Testing Expectations

- Minimum scenarios:
  - draft_accepts_first_metadata_capture
  - draft_replaces_existing_metadata
  - new_document_metadata_is_null_until_set
  - complete_document_rejects_metadata_update
  - metadata_capture_emits_domain_event
  - metadata_update_preserves_type_version_binding
  - complete_document_file_rules_still_enforced
- Full regression: all Wave 0–2 + slice 016 tests

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`; behavior tests pass; no validation port yet.

## Post-Slice Memory / Backlog Sync

1. `index.md` — slice 017 **Done**
2. `memory.md` — Document metadata attachment summary
3. `current-state.md` — Wave 3 progress
4. `current-task.md` — point to slice-018

## Patterns Used

- Aggregate Root
- Value Object
- Domain Event

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
