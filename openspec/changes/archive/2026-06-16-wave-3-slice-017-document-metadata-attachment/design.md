## Context

Slice 016 created `AIX.Documents.Contracts` with `CapturedMetadataPayload` and `CapturedMetadataGroupInstance` mirroring Metadata's `VersionMetadataPayload` shape. The `Document` aggregate (`AIX.Documents`) supports Draft creation bound to `DocumentTypeVersionId`, file attachment, and unconditional `Complete()` — but has no captured-metadata state or operations.

Slice 017 is the second Wave 3 implementation change. It stores captured metadata on the aggregate using Documents-owned types derived from the contract shape, without validating against the Metadata schema (deferred to slice 018).

**Current aggregate surface (relevant):**

- `Document.Create(...)` → Draft, emits `DocumentCreated`
- `Document.AttachFile(...)` → rejects when Complete via `DocumentErrors.CannotModifyWhenComplete`
- `Document.Complete(IClock)` → unconditional Draft → Complete (unchanged in this slice)
- No `AIX.Documents.Contracts` reference on `AIX.Documents` yet

## Goals / Non-Goals

**Goals:**

- Hold optional captured metadata on `Document` while in Draft (null until first `SetCapturedMetadata`).
- Replace metadata on subsequent calls while Draft.
- Reject metadata mutations on Complete documents with deterministic error.
- Emit `DocumentMetadataCaptured` domain event on successful set/replace.
- Expose current captured metadata from the aggregate for tests and future slices.
- Preserve immutable `DocumentTypeId` and `DocumentTypeVersionId`.
- Full backend regression passes.

**Non-Goals:**

- `ICaptureMetadataValidator` or schema validation (slice 018).
- Completion readiness evaluation (slice 019).
- Changing `Complete()` to require metadata or files (slice 020).
- Modifying `AIX.Metadata` or `AIX.Documents.Contracts` (unless a tiny contract gap is discovered and justified).
- Persistence, EF Core, repositories, HTTP, MediatR, Angular/PrimeNG/Avalon.

## Decisions

### 1. Project reference: Documents → Contracts

Add `<ProjectReference Include="..\AIX.Documents.Contracts\AIX.Documents.Contracts.csproj" />` to `AIX.Documents.csproj`.

Add the same reference to `AIX.Documents.Tests.csproj` so tests can construct `CapturedMetadataPayload` instances.

**Rationale:** Slice 016 established contracts as the anti-corruption boundary. The aggregate operation accepts `CapturedMetadataPayload` at the boundary and maps to a Documents-owned value object internally.

**Alternative considered:** Duplicate payload shape in Domain without referencing Contracts. Rejected — planning and slice backlog require contract alignment; duplication risks drift.

### 2. Documents-owned value object: `DocumentCapturedMetadata`

| Member | Type | Notes |
|--------|------|-------|
| `StandaloneValues` | `IReadOnlyDictionary<string, string?>` | Keyword-code-keyed values |
| `GroupInstances` | `IReadOnlyList<DocumentCapturedMetadataGroupInstance>` | Documents-owned group instance type |

Add nested or sibling type `DocumentCapturedMetadataGroupInstance` with `GroupCode`, `InstanceKey`, and `Values` — structurally aligned with `CapturedMetadataGroupInstance`.

Factory: `DocumentCapturedMetadata.From(CapturedMetadataPayload payload)` performs defensive copies (same immutability discipline as the contract).

**Rationale:** Domain should not expose contract types as persistent aggregate state. Value object keeps Documents BC autonomous while mirroring contract shape for slice 018/019 mapping.

**Alternative considered:** Store `CapturedMetadataPayload` directly on the aggregate. Rejected — contracts are cross-BC DTOs; domain state should be Documents-owned.

### 3. Aggregate API

| Addition | Detail |
|----------|--------|
| Property | `DocumentCapturedMetadata? CapturedMetadata { get; }` — `null` when never set |
| Operation | `Result SetCapturedMetadata(CapturedMetadataPayload payload, IClock clock)` |

Behavior:

1. If `State == Complete` → `Result.Failure(DocumentErrors.CannotModifyWhenComplete)`; existing metadata unchanged.
2. Map `payload` → `DocumentCapturedMetadata` via `From`.
3. Replace `_capturedMetadata` (first call sets; subsequent calls replace).
4. Emit `DocumentMetadataCaptured` with `EventId`, `OccurredOn` (from clock), `CorrelationId.New()`, `DocumentId`, and the captured snapshot (standalone values + group instances sufficient for audit/projections).
5. Return `Result.Success()`.

No validation of payload contents in this slice — any structurally valid contract instance is accepted.

**Rationale:** Matches `AttachFile` guard pattern and `IClock` injection used elsewhere on the aggregate. Reusing `CannotModifyWhenComplete` avoids proliferating errors before slice 020.

### 4. Domain event: `DocumentMetadataCaptured`

`sealed record` in `AIX.Documents.Events`, following `DocumentFileAttached` conventions:

- `Guid EventId`, `DateTimeOffset OccurredOn`, `CorrelationId CorrelationId`
- `DocumentId DocumentId`
- Snapshot fields: standalone values dictionary and group instances (or embed `DocumentCapturedMetadata` if events layer can reference Domain types — prefer explicit primitives on the event record matching existing event style)

**Rationale:** Past-tense event name per Wave 0 conventions; enables audit and future projections without coupling to Metadata.

### 5. Version binding immutability

`SetCapturedMetadata` SHALL NOT assign to `_documentTypeId` or `_documentTypeVersionId`. No new APIs to change version binding.

**Rationale:** Version binding is fixed at `Create` per Wave 0/1 design; metadata is scoped to the bound version.

### 6. Unchanged behaviors (regression guard)

- `Create`, `AttachFile`, and `Complete` logic unchanged.
- Existing `AIX.Documents.Tests` scenarios continue to pass; add one regression test that Complete documents still reject file attachment after metadata work lands.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Unvalidated metadata stored on Document | Explicit slice boundary; slice 018 adds validation gate |
| Contract ↔ domain mapping drift | `From` factory tested; structural parity with `CapturedMetadataPayload` |
| Event payload too large for future persistence | Snapshot matches contract shape; JSONB storage deferred |
| Developers add validation in slice 017 | Tasks and spec explicitly forbid `ICaptureMetadataValidator` |

## Migration Plan

Not applicable — additive domain behavior. After implementation:

1. Run full backend build/test.
2. Update `ai/backlog/mvp/index.md`, `ai/context/memory.md`, `ai/context/current-state.md`.
3. Point `ai/tasks/current-task.md` at slice-018.

## Open Questions

- _(none — planning artifacts and slice backlog resolve slice-017 decisions)_
