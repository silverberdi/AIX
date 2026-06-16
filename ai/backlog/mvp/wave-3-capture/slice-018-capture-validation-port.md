# Slice 018 — Capture Validation Port

## Goal

Wire captured metadata updates on `Document` to version-scoped schema validation through an explicit port, delegating all validation rules to Metadata without a Documents → Metadata domain reference.

## Scope

- Define `ICaptureMetadataValidator` behavioral port interface (slice 018 ownership) accepting document type version identity + `CapturedMetadataPayload`, returning `CaptureValidationResult` from `AIX.Documents.Contracts`
- Port placement: final namespace/folder selected during implementation based on existing `AIX.Documents` structure — **SHALL NOT** live in `AIX.Documents.Contracts`
  - **Preferred:** `AIX.Documents.Application` if the Application layer is used for ports
  - **Alternative:** `AIX.Documents.Domain` only if the aggregate method requires a domain-level abstraction
- `SetCapturedMetadata` (or dedicated method) requires successful validation before storing payload
- Accept `ICaptureMetadataValidator` as method parameter — domain-friendly injection, same pattern as `IClock`
- Test adapter in `AIX.Documents.Tests` implementing the port by delegating to `DocumentType.ValidateMetadataAgainstVersion` / `VersionSchemaValidator`
- Map Metadata `SchemaValidationResult` to `CaptureValidationResult` contract
- Reject invalid payloads: unknown keys, missing required fields/groups, invalid keyword values, hidden/deprecated on capture, unexpected group instances
- Surface multiple deterministic validation errors where Metadata provides them
- Do not duplicate keyword value parsing in Documents

## Out of Scope

- Application-layer DI registration (future slice)
- Completion readiness (slice 019)
- Persistence, EF Core, repositories
- HTTP APIs, MediatR
- Angular UI, PrimeNG, Avalon
- Runtime multi-tenancy, storage providers
- OCR, search, retention, workflows
- Production Metadata adapter project reference from `AIX.Documents` domain assembly

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Documents` (Domain) | Yes — validation gate on aggregate; port in Domain only if domain-level abstraction required |
| `AIX.Documents.Application` | Yes — preferred home for `ICaptureMetadataValidator` if Application layer exists or is introduced |
| `AIX.Documents.Contracts` | Read passive DTO types only (`CapturedMetadataPayload`, `CaptureValidationResult`, etc.) — **no port interface** |
| `AIX.Documents.Tests` | Yes — test adapter may reference `AIX.Metadata` |
| `AIX.Metadata` | Test adapter only; no change to validation core unless bugfix required |

## Dependencies

- **slice-016** — `CapturedMetadataPayload`, `CaptureValidationResult`, `CaptureValidationError`
- **slice-017** — metadata attachment on Document
- **slice-015** — `VersionSchemaValidator`, `ValidateMetadataAgainstVersion`

## Acceptance Criteria

- `ICaptureMetadataValidator` port is defined during slice 018 in `AIX.Documents.Application` or `AIX.Documents.Domain` (implementation choice) — **not** in `AIX.Documents.Contracts`
- Valid payload passes validation and is stored on Draft document
- Invalid payload is rejected; metadata not stored; validation errors returned
- Unknown metadata key fails validation
- Hidden/deprecated field values rejected on capture path
- Missing required standalone field fails validation
- Missing required group instance fails validation
- Invalid keyword value fails via KeywordValidator delegation
- Multiple errors returned in one validation call where applicable
- No keyword value validation logic duplicated in Documents domain

## Testing Expectations

- Minimum scenarios:
  - valid_metadata_passes_validation_and_is_stored
  - invalid_metadata_rejected_and_not_stored
  - fails_unknown_metadata_key
  - rejects_hidden_field_on_capture
  - fails_missing_required_field
  - fails_missing_required_group_instance
  - fails_invalid_keyword_value
  - returns_multiple_validation_errors_when_applicable
  - complete_document_still_rejects_metadata_regardless_of_validation
- Full regression: Metadata 129 tests + Documents tests

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`; validation port integrated; adapter tests pass.

## Post-Slice Memory / Backlog Sync

1. `index.md` — slice 018 **Done**
2. `memory.md` — capture validation port summary
3. `current-state.md` — Wave 3 progress
4. `current-task.md` — point to slice-019

## Patterns Used

- Port and adapter
- Delegation to existing validator
- Anti-corruption layer

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
