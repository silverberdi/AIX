## Why

Slice 016 delivered passive capture contracts in `AIX.Documents.Contracts`, but the `Document` aggregate still cannot hold captured metadata. Wave 3 Capture MVP requires Documents to store version-scoped metadata on Draft documents before validation (018), readiness (019), and completion enforcement (020) can proceed.

## What Changes

- Add `AIX.Documents.Contracts` project reference to `AIX.Documents` (and test project).
- Introduce Documents-owned value type `DocumentCapturedMetadata` aligned with `CapturedMetadataPayload` contract shape.
- Extend `Document` with optional captured metadata (`null` until first set) and `SetCapturedMetadata` with replace semantics on Draft.
- Reject metadata updates when `State == Complete` using deterministic domain error `DocumentErrors.CannotModifyWhenComplete` (same pattern as file attachment).
- Add `DocumentMetadataCaptured` domain event emitted on successful set/replace.
- Preserve immutable `DocumentTypeId` and `DocumentTypeVersionId` during metadata operations.
- Add behavior tests in `AIX.Documents.Tests` covering attach, replace, immutability after Complete, event emission, and version binding preservation.

**Out of scope for this change:** metadata schema validation (`ICaptureMetadataValidator`, slice 018), completion readiness (019), `Complete()` enforcement changes (020), `AIX.Metadata` changes, persistence, APIs, MediatR, UI, multi-tenancy, storage, OCR, search, retention, workflows.

## Capabilities

### New Capabilities

- `capture-metadata-attachment`: Document aggregate holds version-scoped captured metadata while in Draft; metadata is immutable after Complete; changes emit domain events.

### Modified Capabilities

- _(none — net-new domain behavior; no existing stable spec under `openspec/specs/` for metadata attachment)_

## Impact

| Area | Impact |
|------|--------|
| **Projects** | `AIX.Documents` (Domain, Events), `AIX.Documents.Tests` |
| **New dependency** | `AIX.Documents` → `AIX.Documents.Contracts` (read contract types only) |
| **Bounded contexts** | Documents domain only; contracts consumed read-only; `AIX.Metadata` unchanged |
| **Existing behavior** | `Create`, `AttachFile`, and `Complete` semantics unchanged (no validation gate on metadata yet) |
| **Future slices** | 018 adds validation port; 019–020 add readiness and `Complete()` enforcement |

**Validation:** `cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln`
