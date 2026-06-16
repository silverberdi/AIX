# Slice 016 — Capture Metadata Contracts

## Goal

Introduce cross-bounded-context contract types for captured metadata payloads and validation results so `AIX.Documents` can integrate with Metadata validation without referencing `AIX.Metadata.Domain`.

## Scope

- Create `AIX.Documents.Contracts` project and add it to `backend/AIX.sln`
- Define passive contract/data types only (no behavioral ports or validators):
  - `CapturedMetadataPayload` mirroring structural shape of `VersionMetadataPayload` (standalone keyword-code values + group instances)
  - `CapturedMetadataGroupInstance` (or equivalent) for repeatable group instances
  - `CaptureValidationResult` with success/failure and deterministic multiple errors
  - `CaptureValidationError` (or equivalent) for individual validation failures
- Strong typing; no `any`; JSON serialization conventions deferred to future API slices
- Unit tests for contract immutability and shape

## Out of Scope

- Document aggregate changes
- Metadata domain changes
- Behavioral ports or validators (slice 018)
- Mapping adapters (slice 018)
- Persistence, EF Core, repositories
- HTTP APIs, MediatR
- Angular UI, PrimeNG, Avalon
- Runtime multi-tenancy
- Storage providers, OCR, search, retention, workflows

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Documents.Contracts` (new) | Yes — primary |
| `AIX.Documents.Tests` or dedicated contracts tests | Yes |
| `AIX.Metadata`, `AIX.Documents` domain | No |
| `AIX.SharedKernel` | Avoid unless established contract primitive pattern requires it |

## Dependencies

- **slice-015** — `VersionMetadataPayload` shape and validation semantics defined in Metadata

## Acceptance Criteria

- `AIX.Documents.Contracts` is created, added to `backend/AIX.sln`, and builds without referencing `AIX.Metadata` or `AIX.Documents` domain projects
- `CapturedMetadataPayload` carries standalone values and group instances with keyword-code keys
- `CaptureValidationResult` supports success and multiple deterministic errors without exceptions for expected failures
- `CaptureValidationError` (or equivalent) carries individual error details in a deterministic structure
- No behavioral ports or validator interfaces are defined in this slice
- Contract types are immutable or init-only after construction
- Tests verify contract construction and error collection behavior

## Testing Expectations

- Minimum scenarios:
  - constructs_captured_metadata_payload_with_standalone_values
  - constructs_captured_metadata_payload_with_group_instances
  - capture_validation_result_success_has_no_errors
  - capture_validation_result_failure_carries_multiple_errors
  - capture_validation_error_carries_deterministic_details
  - contracts_project_has_no_forbidden_references
  - contracts_project_defines_no_behavioral_ports
- Full regression: all Wave 0–2 backend tests still pass (no domain behavior change yet)

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`; contracts build; tests pass; no forbidden project references.

## Post-Slice Memory / Backlog Sync

1. `index.md` — slice 016 **Done**
2. `memory.md` — Documents.Contracts introduced; capture contract summary
3. `current-state.md` — Wave 3 progress table
4. `current-task.md` — point to slice-017

## Patterns Used

- Anti-corruption layer (passive contracts)

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
