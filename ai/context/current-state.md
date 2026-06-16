# Current Execution State

Last updated: 2026-06-16 (slice-017 complete). Refresh after each slice completes or health changes.

## Wave 0 progress

| Slice | Status |
|-------|--------|
| slice-001-document-aggregate | **Done** |
| slice-002-sharedkernel-primitives | **Done** |
| slice-003-document-state-transitions | **Done** |
| slice-004-document-files | **Done** |
| **Wave 0 Domain Review** | **Done** — `docs/reviews/wave-0-domain-review.md` |

Wave 0 is complete.

## Wave 1 progress

| Slice | Status |
|-------|--------|
| slice-005-document-type-aggregate | **Done** |
| slice-006-document-type-versioning | **Done** |
| slice-007-keyword-registry | **Done** |
| slice-008-keyword-groups | **Done** |
| slice-009-keyword-validation | **Done** |
| **Wave 1 Domain Review** | **Done** — `docs/reviews/wave-1-domain-review.md` |

Wave 1 is complete.

## Wave 2 progress

| Slice | Status |
|-------|--------|
| slice-010-field-schema-model | **Done** |
| slice-011-version-schema-composition | **Done** |
| slice-012-version-keyword-group-assignment | **Done** |
| slice-013-layout-schema-model | **Done** |
| slice-014-renderer-contract-preparation | **Done** |
| slice-015-schema-validation-integration | **Done** |
| **Wave 2 Domain Review** | **Done** — `docs/reviews/wave-2-domain-review.md` (2026-05-23) — **PASS** |

Wave 2 is **complete**.

## Wave 3 progress

| Slice | Status |
|-------|--------|
| slice-016-capture-metadata-contracts | **Done** |
| slice-017-document-metadata-attachment | **Done** |
| slice-018-capture-validation-port | **Next** |
| slice-019-document-completion-readiness | Pending |
| slice-020-complete-with-capture-enforcement | Pending |
| slice-021-capture-context-contract | Pending (optional, last priority) |

Backlog: `ai/backlog/mvp/wave-3-capture/`

## Backend health

| Check | Status |
|-------|--------|
| .NET 9 solution (`backend/AIX.sln`) | Builds |
| Full solution tests | **191/191** pass |
| `AIX.Documents.Tests` | **32/32** pass |
| `AIX.SharedKernel.Tests` | **12/12** pass |
| `AIX.Metadata.Tests` | **129/129** pass |
| `AIX.Metadata.Contracts` | Builds (no SharedKernel dependency) |
| `AIX.Documents.Contracts` | Builds (no forbidden dependencies) |
| `AIX.Documents.Contracts.Tests` | **8/8** pass |

Validation command:

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```

## Frontend health

| Check | Status |
|-------|--------|
| Nx workspace | Exists |
| Angular 21 app | `apps/aix-ui` |
| Shared libs | `libs/shared-ui`, `libs/shared-core` |
| PrimeNG 21 | **Pending** — not installed yet |
| Avalon theme | **Pending** — not configured yet |

## Current slice

**slice-018-capture-validation-port** — wire `SetCapturedMetadata` to version-scoped schema validation via `ICaptureMetadataValidator`. See `ai/backlog/mvp/wave-3-capture/slice-018-capture-validation-port.md`.

## Slice 017 summary (complete)

- `Document` supports captured metadata while **Draft** (`CapturedMetadata` is `null` until first set).
- Metadata is stored as Documents-owned value types (`DocumentCapturedMetadata`, `DocumentCapturedMetadataGroupInstance`) mapped from `CapturedMetadataPayload`.
- `SetCapturedMetadata(CapturedMetadataPayload, IClock)` accepts contract payloads at the boundary; metadata can be **replaced** on subsequent calls while Draft.
- Metadata updates are **rejected after Complete** with `DocumentErrors.CannotModifyWhenComplete`; `DocumentTypeId` and `DocumentTypeVersionId` remain immutable.
- `DocumentMetadataCaptured` domain event is emitted on successful set/replace.
- **No schema validation** yet — slice 018 owns the validation port and gate.

## Blockers

- **None known**

## Deferred work (do not pull forward without a slice)

- PrimeNG 21 installation
- Avalon theme setup
- Persistence / repositories
- HTTP APIs and controllers beyond skeleton
- EF Core
- UI workflows and feature screens
