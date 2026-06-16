## 1. Project references

- [x] 1.1 Add `AIX.Documents.Contracts` project reference to `backend/src/AIX.Documents/AIX.Documents.csproj`
- [x] 1.2 Add `AIX.Documents.Contracts` project reference to `backend/tests/AIX.Documents.Tests/AIX.Documents.Tests.csproj`

## 2. Documents-owned value types

- [x] 2.1 Add `DocumentCapturedMetadataGroupInstance` in `AIX.Documents.Domain` with `GroupCode`, `InstanceKey`, and keyword-code-keyed `Values`
- [x] 2.2 Add `DocumentCapturedMetadata` sealed class/record with `StandaloneValues`, `GroupInstances`, and `From(CapturedMetadataPayload payload)` factory with defensive copies

## 3. Domain event

- [x] 3.1 Add `DocumentMetadataCaptured` sealed record in `AIX.Documents.Events` with document id, correlation, and captured metadata snapshot fields

## 4. Document aggregate

- [x] 4.1 Add private `_capturedMetadata` field and public `CapturedMetadata` get-only property (`null` until first set)
- [x] 4.2 Implement `SetCapturedMetadata(CapturedMetadataPayload payload, IClock clock)` — reject when Complete, map to value object, replace state, emit event
- [x] 4.3 Ensure `DocumentTypeId` and `DocumentTypeVersionId` are not modified by metadata operations

## 5. Behavior tests (`AIX.Documents.Tests`)

- [x] 5.1 `new_document_metadata_is_null_until_set`
- [x] 5.2 `draft_accepts_first_metadata_capture`
- [x] 5.3 `draft_replaces_existing_metadata`
- [x] 5.4 `complete_document_rejects_metadata_update`
- [x] 5.5 `metadata_capture_emits_domain_event`
- [x] 5.6 `metadata_update_preserves_type_version_binding`
- [x] 5.7 `complete_document_file_rules_still_enforced` (regression: file attach still rejected on Complete)

## 6. Verification and handoff

- [x] 6.1 Run `cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln` — all tests pass; no changes to `AIX.Metadata` or `AIX.Documents.Contracts` behavior
- [x] 6.2 Update `ai/backlog/mvp/index.md`, `ai/context/memory.md`, and `ai/context/current-state.md` for slice-017 completion
- [x] 6.3 Point `ai/tasks/current-task.md` at slice-018
