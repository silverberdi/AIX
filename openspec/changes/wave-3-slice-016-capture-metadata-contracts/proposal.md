## Why

Wave 3 Capture MVP planning is complete and slice-016 is the first implementation step. `AIX.Documents` must accept and validate version-scoped captured metadata without referencing `AIX.Metadata.Domain`. Passive cross-bounded-context contract types in a new `AIX.Documents.Contracts` project establish the anti-corruption boundary before attachment (017), validation port (018), readiness (019), and completion enforcement (020).

## What Changes

- Create `backend/src/AIX.Documents.Contracts` and add it to `backend/AIX.sln`.
- Define passive capture contract types only:
  - `CapturedMetadataPayload`
  - `CapturedMetadataGroupInstance`
  - `CaptureValidationResult`
  - `CaptureValidationError`
- Ensure contract types are immutable or init-only after construction.
- Add `backend/tests/AIX.Documents.Contracts.Tests` with shape, immutability, validation result, and forbidden-reference tests.
- Add the test project to `backend/AIX.sln`.

**Out of scope for this change:** `ICaptureMetadataValidator`, Document aggregate changes, Metadata domain changes, mapping adapters, persistence, APIs, MediatR, UI, multi-tenancy, storage, OCR, search, retention, workflows.

## Capabilities

### New Capabilities

- `capture-contracts`: Cross-bounded-context passive DTO and validation result types enabling Documents to integrate with Metadata validation without direct domain references.

### Modified Capabilities

- _(none — no stable specs exist under `openspec/specs/` yet; requirements are net-new for slice 016)_

## Impact

| Area | Impact |
|------|--------|
| **New projects** | `AIX.Documents.Contracts`, `AIX.Documents.Contracts.Tests` |
| **Solution** | `backend/AIX.sln` — two new project entries |
| **Bounded contexts** | Contracts layer only; no changes to `AIX.Documents` or `AIX.Metadata` |
| **Dependencies** | `AIX.Documents.Contracts` has zero project references (no `AIX.SharedKernel`, `AIX.Metadata`, `AIX.Metadata.Contracts`, or `AIX.Documents`) |
| **Future slices** | 017–018 consume these types; 018 maps Metadata validation results into `CaptureValidationResult` |

**Validation:** `cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln`
