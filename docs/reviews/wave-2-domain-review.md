# Wave 2 Domain Review

**Date:** 2026-05-23  
**Scope:** Slices 010–015 (field schema, version composition, group assignment, layout schema, renderer contracts, schema validation integration)  
**Reviewer role:** Architectural / domain review (no implementation)

---

## Executive summary

Wave 2 completed the declarative metadata stack in `AIX.Metadata`: immutable `FieldSchema` and `VersionSchemaComposition`, group assignments with point-in-time snapshots, presentation-only `LayoutSchema` (with `DefaultLayoutGenerator`), renderer-facing DTOs in `AIX.Metadata.Contracts`, and version-scoped capture validation via `VersionSchemaValidator` / `DocumentType.ValidateMetadataAgainstVersion`. The work uses `Result`-based composition failures, rich `DocumentTypeVersionCreated` payloads, and **129 passing unit tests** in `AIX.Metadata.Tests` (**166** backend domain/API unit tests including Wave 0–1).

Composition vs layout separation matches renderer canon. Contract boundaries are correct (contracts depend on nothing; domain projects to contracts). Validation is centralized for capture semantics with deliberate delegation to `KeywordValidator` for value rules.

The model is **healthy for Wave 3** with tracked follow-ups: `DocumentType` orchestration surface growth, custom layouts that omit composition entries, Documents BC not yet storing validated metadata, and Wave 3 slice backlog not yet defined under `ai/backlog/mvp/`.

---

## 1. Aggregate Health Review

### Evaluation

| Area | Assessment |
|------|------------|
| `DocumentType` cohesion | **Strong** — still owns identity, state, versions, and version-scoped validation entry point |
| `DocumentType` size | **Growing** (~324 lines) — many `CreateVersion` overloads; logic delegated to composition/layout builders |
| `DocumentTypeVersion` | **Appropriate** — sealed record holding immutable `VersionSchemaComposition` + `LayoutSchema` |
| `KeywordRegistry` / `KeywordGroup` | **Unchanged roots** — still separate; version creation takes registry/groups as inputs (point-in-time integrity) |
| Cross-aggregate wiring | **Snapshot-based** — versions do not hold live references to `KeywordGroup` aggregates |
| BC isolation | **Preserved** — `AIX.Metadata` → `SharedKernel` + `Metadata.Contracts`; no `AIX.Documents` reference |

### What is healthy?

- **Version immutability** — `DocumentTypeVersion` is append-only on `DocumentType`; composition and layout fixed at creation.
- **Factory + private ctor** patterns unchanged on roots; `FieldSchema` is a value object with `Create` validation.
- **`ValidateMetadataAgainstVersion`** on `DocumentType` resolves version then delegates to `VersionSchemaValidator` — correct aggregate façade for capture validation.
- **Event richness** — `DocumentTypeVersionCreated` includes field, group, and layout snapshots for projections.
- **Inactive type policy unchanged** — `Activate`/`Deactivate` still do not block `CreateVersion` (documented Wave 1 behavior; still intentional unless product changes).

### What is risky?

- **`DocumentType` overload proliferation** — ten+ `CreateVersion` entry points increase maintenance cost; risk of inconsistent defaults if a new overload bypasses layout/composition pipeline.
- **Three aggregate roots at persistence time** — version creation still requires registry + groups in memory; repositories must load or snapshot consistently (no FK from version row to live `KeywordGroup` row required by domain, but ops need a story).
- **Registry integrity is still point-in-time** — keywords/groups validated at version creation and capture validation uses current registry for value rules; registry drift after version publish is a known MVP trade-off (keywords not versioned per canon).
- **Non-contiguous version numbers** — still allowed (inherited from Wave 1).

### What should NOT be changed yet?

- Do not merge `KeywordRegistry` / `KeywordGroup` into `DocumentType`.
- Do not add EF mappings, repositories, HTTP APIs, or MediatR.
- Do not attach metadata JSON to `Document` in Metadata BC — belongs in Documents + Wave 3 slices.
- Do not add workflow/publish states to versions without an explicit slice.
- Do not retroactively mutate `DocumentTypeVersion` composition or layout.

### Aggregate boundary verdict

**Still correct.** `DocumentType` remains the version aggregate root; composition semantics and layout presentation are composed value objects on `DocumentTypeVersion`, not separate aggregates. Keyword definitions stay in registry/group roots.

---

## 2. Schema Composition Review

### Evaluation

| Area | Assessment |
|------|------------|
| Field model | **Strong** — `FieldSchema` binds `KeywordId` + `FieldCatalogType` with compatibility matrix vs `KeywordDataType` |
| Version composition | **Strong** — ordered fields, group assignments, snapshots, duplicate/overlap rules |
| Group assignment | **Strong** — repeatable instance keys, non-repeatable duplicate rejection, keyword overlap with standalone fields rejected |
| Empty composition | **Allowed** — `VersionSchemaComposition.Empty` |
| Canon alignment | **Good** — add-only field model via deprecate/hide; no delete |

### What is healthy?

- `VersionSchemaComposition.FromDefinitions` builds fields via `FieldSchema.Create` and groups via assignment definitions with registry/group integrity checks.
- **Uniqueness** — duplicate `FieldSchemaId`, duplicate keyword in standalone fields, duplicate group placement/instance keys enforced.
- **`VersionSchemaFieldSnapshot` / `VersionSchemaGroupSnapshot`** — keyword codes and group flags snapshotted on version creation for stable projections and contracts.
- **MVP catalog subset** — `FieldCatalogType` implements Text through Boolean; incompatible pairings fail at field creation.

### What is risky?

- **No table/file field types in composition** — capability board mentions tables; correctly deferred but Wave 3+ capture may need `FileRequirementSchema` alignment with Documents files.
- **Group keyword list snapshotted as IDs only** — if registry keyword codes change, historical event payloads use snapshots for fields but group snapshots store IDs (codes resolved at projection time from registry in tests; production needs stored codes or immutable keyword codes).
- **`FromFields` path** — allows building composition from pre-built `FieldSchema` list without group path; useful for tests, secondary to main `FromDefinitions` pipeline.

### Composition vs layout separation

**Healthy and intentional.** `DocumentTypeVersion` holds both `SchemaComposition` and `LayoutSchema` as peer properties. Layout references composition keys (`KeywordId`, `KeywordGroupId` + instance key) and does not re-declare keywords. Renderer canon (“composition = what exists; layout = how presented”) is implemented in code, not only in docs.

---

## 3. Layout Model Review

### Evaluation

| Area | Assessment |
|------|------------|
| Separation from composition | **Correct** |
| Reference integrity | **Strong** — unknown field/group references fail; duplicate placements rejected |
| Default layout | **Slice 014** — `DefaultLayoutGenerator` places all standalone fields in “General” and each group in its own section |
| Custom layout | **Permissive** — explicit sections may omit composition entries (empty section allowed in tests) |
| Ordering | **Deterministic** — sections and placements sorted by order hints |

### What is healthy?

- `LayoutSchema.FromDefinitions` validates against composition map; `CreateDefault` delegates to `DefaultLayoutGenerator`.
- **Snapshots** — `LayoutSectionSnapshot` on `DocumentTypeVersionCreated` for event-driven projections.
- **Humanized group section titles** — `DefaultLayoutGenerator.HumanizeGroupCode` for default UX.

### What is risky?

- **Custom layout completeness not enforced** — a version may have fields/groups in composition that never appear in an explicit layout; renderer must fall back to composition-only rendering or treat layout as authoritative — document contract behavior for Angular slice.
- **`DocumentSchemaProjector.MapLayout` silently skips** placements when composition lookup fails (`when fieldById.ContainsKey`) — defensive for inconsistent state but could mask bugs; prefer invariant that version layout always matches composition after domain creation.
- **Layout not validated on capture** — correct per canon (presentation only); capture validation uses composition only.

---

## 4. Renderer Contract Review

### Evaluation

| Area | Assessment |
|------|------------|
| `AIX.Metadata.Contracts` dependency | **None on SharedKernel** — correct public surface |
| `DocumentSchemaContract` | **Complete for MVP subset** — type identity, binding key, fields, groups, nested layout |
| `RendererFieldCatalogType` | **Full canon enum** — domain maps MVP subset; Select/MultiSelect/Table/File/RichText reserved |
| `DocumentSchemaProjector` | **Read-only** — maps domain version to contract; `SchemaBindingKey` `{code}/v{n}` |
| `DatasetId` | **Placeholder null** — dataset abstraction on capability board, not implemented |

### Contract boundary verdict

**Correct.**

- Contracts use primitive `Guid` and strings — suitable for cross-BC and frontend consumption.
- Projection lives in `AIX.Metadata` (not Application folder yet) — acceptable for domain-only waves; move to Application when handlers appear.
- **No business rules in Contracts** — DTOs only.
- Documents BC does not reference Metadata contracts yet — expected until Wave 3 integration slice.

### Gaps vs renderer canon (deferred)

- `RuleSchema`, `FileRequirementSchema` not in contracts.
- Full field catalog types not producible from domain yet.
- No JSON Schema export (explicitly out of slice 014 scope).

---

## 5. Validation Integration Review

### Evaluation

| Area | Assessment |
|------|------------|
| Capture validation entry | `DocumentType.ValidateMetadataAgainstVersion` → `VersionSchemaValidator` |
| Value rules | **Delegated** to `KeywordValidator.ValidateValue` — no duplication of type/length/required parsing |
| Required semantics | **Unified for capture** — `FieldSchema.IsRequired` standalone; `VersionSchemaGroupAssignment.IsRequired` for group keywords |
| Multi-error | **`SchemaValidationResult`** collects multiple errors; deterministic ordering by code/message |
| Structural rules | Unknown keys, group payload shape, hidden/deprecated on capture, unexpected group instances |
| Registry API | **`ValidateKeywordValue` still first-error-only** — Wave 1 gap; schema path does not use this for multi-field capture |

### Is validation centralized enough?

**Yes for version-scoped capture validation.** Single static validator owns structural + required + capture policy; keyword value rules stay in `KeywordValidator`.

**Partial for ad-hoc keyword checks** — direct `KeywordRegistry.ValidateKeywordValue` callers still get one `Result` failure.

### What is healthy?

- `VersionMetadataPayload` — domain-friendly dictionary model (not JSON parser in domain).
- Group instance matching via normalized `GroupInstanceKey` (case-insensitive group code).
- Rejects standalone keys that belong to group keyword sets (`GroupKeywordMustUseGroupPayload`).
- Inactive fields (`IsDeprecated` / `IsHidden`) skip required value checks but reject provided values on capture.

### What is risky?

- **Capture validation uses current registry** for keyword definitions while composition was built at version time — aligns with “keywords not versioned” canon but stricter validation or label changes can affect old versions.
- **Repeatable group max cardinality** — assignment rules at composition time; payload validation rejects duplicate instances and unexpected instances; max repeat count beyond duplicate detection not modeled (only non-repeatable duplicate placement blocked at composition).
- **No cross-BC validation** — document existence, file requirements, governance — correctly absent.

---

## 6. SharedKernel Boundary Review

### Usage in `AIX.Metadata` (Wave 2 additions)

| Primitive | Usage | Verdict |
|-----------|--------|---------|
| `Result` / `Error` | Composition, layout, field, validation errors | Appropriate |
| `Entity<TId>` | Unchanged on roots | Appropriate |
| `StronglyTypedId<Guid>` | `FieldSchemaId`, `LayoutSectionId`, etc. | Appropriate |
| `DomainEvent` | Extended `DocumentTypeVersionCreated` payload | Appropriate |

### Boundary health

- **No business vocabulary added to SharedKernel.**
- **Metadata.Contracts does not reference SharedKernel** — frontend/integration friendly.
- **Specifications remain in Metadata** (`KeywordValueSpecifications`) — not promoted to SharedKernel.

### What should not expand yet

- No shared validation pipeline, MediatR, or repository abstractions in SharedKernel.
- No renderer or layout types in SharedKernel.

---

## 7. Persistence Pressure Review

*Identification only — no fixes in this review.*

| Pressure point | Detail |
|----------------|--------|
| **`DocumentTypeVersion` record** | Two owned graphs: `VersionSchemaComposition` + `LayoutSchema` — likely JSONB columns or owned tables per version row |
| **Snapshot columns** | Field/group/layout snapshots duplicated in event payload and embeddable on version — choose single source of truth for read models |
| **Rich version payload size** | Large document types → large JSONB per version; acceptable for MVP; monitor |
| **Three aggregate roots** | `DocumentType`, `KeywordRegistry`, `KeywordGroup` — three repositories; version creation transaction may span loaded aggregates in application layer |
| **No navigation from version to Keyword** | Keyword IDs in composition; FK optional; keyword code snapshots partially on field snapshots |
| **Strongly typed IDs** | Value converters per ID type in Metadata BC |
| **Private constructors** | EF materialization ctor deferred to persistence slice |
| **`_domainEvents`** | Outbox/dispatch + clear on save |
| **Immutability** | Append-only versions enforced in application rules; DB triggers optional later |
| **ValidateMetadataAgainstVersion** | Needs version row + registry at runtime — query pattern: load document type version + registry (tenant DB) |

---

## 8. Renderer/UI Pressure Review

| Topic | Assessment |
|-------|------------|
| Contract readiness | **Ready** for Angular contract-driven renderer slice |
| PrimeNG / Avalon | **Not started** — per project plan |
| Binding key | `SchemaBindingKey` on contract supports version-aware renderer selection |
| Default layout | Backend generates deterministic layout; UI can rely on contract `Layout` |
| UX validation | **Out of scope** — client may pre-validate; backend is authority per canon |
| Modes | CAPTURE_MODE only in domain; READ/COMPARE deferred |

### Pressure points for Wave 3 UI slice

- Map `RendererFieldCatalogType` to PrimeNG controls (MVP subset first).
- Decide renderer behavior when custom layout omits composed fields (show in default section vs hide).
- Wire capture payload assembly to `VersionMetadataPayload` shape.
- Do not implement full catalog (SELECT, TABLE, FILE) until domain supports them.

---

## 9. Testing Review

### Test inventory (`AIX.Metadata.Tests`)

| File | Facts | Focus |
|------|-------|--------|
| `FieldSchemaTests` | 8 (+ theories) | Catalog compatibility, deprecate/hide/active |
| `VersionSchemaCompositionTests` | 8 | Composition rules, snapshots, regression |
| `VersionSchemaGroupAssignmentTests` | 9 | Groups, repeatability, overlap |
| `LayoutSchemaTests` | 9 | Layout references, defaults, ordering |
| `RendererContractTests` | 7 | Projector, binding key, default layout export |
| `SchemaValidationTests` | 14 | Capture validation integration |
| Wave 1 files | 59 | Registry, groups, keyword validation, document type |
| **Total** | **129** | |

Backend totals at review time: **25** Documents + **12** SharedKernel + **129** Metadata + **2** API smoke = **168** passed (`dotnet test AIX.sln`).

### What is strong?

- Every Wave 2 slice has dedicated tests with failure paths.
- Regression tests from prior slices (`slice_005_*`, cross-slice helpers).
- Snake_case names; FluentAssertions; domain events and snapshots asserted.
- `SchemaValidationTests` cover multi-error, hidden/deprecated, group payload rules.

### Gaps (prioritized)

| Scenario | Priority |
|----------|----------|
| Custom layout with unplaced composition fields — projector/contract behavior | Medium — document before UI slice |
| `ValidateKeywordValue` multi-error on registry | Low — Wave 1 carryover |
| Inactive document type + `CreateVersion` | Low — document if intentional |
| Cross-BC: Document stores metadata validated against version | High — **Wave 3** |
| Integration test Documents ↔ Metadata contracts | Medium — Wave 3 |

### What should NOT be tested yet?

- EF mappings, repositories, HTTP endpoints.
- Angular renderer E2E.
- External datasets, OCR, storage pipelines.
- Performance tests on large schemas.

---

## 10. Wave 3 Readiness

### Is the foundation stable enough?

**Yes.** Wave 2 delivers composable version schema, layout, renderer contracts, and capture validation — the prerequisites listed on the capability board for metadata/layout work before operational capture.

### Architectural blockers?

**None** for starting Wave 3 **domain** slices. No forbidden patterns observed (persistence, MediatR, cross-BC domain references, SharedKernel bloat).

### Prerequisites before first Wave 3 slice

1. **Define Wave 3 slices** under `ai/backlog/mvp/` (roadmap lists Capture MVP capabilities; no slice files yet).
2. **Wave 2 Domain Review** (this document) — complete.
3. Keep **Documents** metadata storage and validation orchestration in explicit slices — do not assume persistence from Wave 2.

### What Wave 3 will likely need (not blockers)

- Attach validated metadata to `Document` (JSONB per versioning canon).
- Application-layer orchestration: load type version + registry, validate payload, persist document.
- Optional: shared contract package consumption from `AIX.Documents.Application`.
- File upload pipeline and capture UI (separate slices; PrimeNG still deferred globally).

### Is Wave 3 ready?

**Yes — to begin planning and domain/application slices once the backlog is written.** Wave 2 domain work does not require rework. **Do not start implementation without slice definitions** — same discipline as prior waves.

---

## 11. Recommendations

### Safe to continue

- Composition + layout split on `DocumentTypeVersion`.
- `VersionSchemaValidator` + `SchemaValidationResult` for capture.
- `DocumentSchemaProjector` and `AIX.Metadata.Contracts` DTOs.
- `DefaultLayoutGenerator` for versions without explicit layout.
- `KeywordValidator` delegation for all value rules.
- Test-first slice workflow; **129** Metadata tests as regression guard.

### Monitor carefully

- `DocumentType` `CreateVersion` overload surface — consider internal builder if overload count grows in Wave 3.
- Custom layout vs composition completeness for renderer.
- Registry drift vs frozen composition semantics.
- `DocumentSchemaProjector` silent skip of bad layout placements.
- Three-root persistence and version creation transactions.
- Duplicate `DocumentTypeId` CLR types across Documents and Metadata.
- Per-event `CorrelationId.New()` inside aggregates (inherited).

### Avoid

- EF Core, repositories, APIs, MediatR, Angular UI during domain-only slices unless explicit.
- Merging layout into composition or encoding presentation in `FieldSchema`.
- Duplicating keyword value parsing outside `KeywordValidator`.
- Full catalog types in domain without slice scope.
- Cross-BC domain references from `Document` to `DocumentType` internals.

### Defer intentionally

- Persistence, outbox, `ClearDomainEvents`.
- `RuleSchema`, `FileRequirementSchema`, dataset/`DatasetId` resolution.
- SELECT/MULTISELECT/TABLE/FILE/RICH_TEXT field types.
- Layout completeness validation for explicit layouts (unless product requires).
- `ValidateKeywordValue` multi-error registry API (until API slice needs it).
- Activate/deactivate / registry-created events.
- JSON Schema infrastructure validator.
- PrimeNG 21 and Avalon theme setup.
- Cross-BC integration tests until Wave 3 contracts slice.

### Recommended fixes (optional, not blocking Wave 3)

| Item | Severity | Suggestion |
|------|----------|------------|
| Registry `ValidateKeywordValue` first-error only | Low | Future slice: return aggregated errors or expose `KeywordValidationResult` |
| Projector skips unknown placements | Low | Assert invariant in tests; or fail projection in dev builds |
| `DocumentSchemaProjector` namespace / folder | Low | Move to `Application/` when application layer slice lands |
| Inactive type can create versions | Low | Product rule + slice if versioning should freeze on deactivate |

---

## 12. Final Verdict

| Dimension | Verdict |
|-----------|---------|
| **Wave 2 domain quality** | **Good** — clear composition/layout split, capture validation, contracts aligned with renderer canon |
| **Architectural stability** | **Stable** — BC isolation, minimal SharedKernel, no scope violations |
| **AI execution discipline** | **Strong** — slices 010–015 respected boundaries; tests match acceptance criteria |
| **Readiness for Wave 3** | **Ready** — proceed once Wave 3 slices are defined in backlog |

### Review verdict

**PASS — Wave 2 domain review complete.** Foundation is sound for Capture MVP (Wave 3). No Wave 2 rework required.

### Explicit review questions

| Question | Answer |
|----------|--------|
| Is composition vs layout separation healthy? | **Yes** — separate models on `DocumentTypeVersion`; layout references composition keys only. |
| Is the contract boundary correct? | **Yes** — `AIX.Metadata.Contracts` is dependency-free; projection stays in Metadata; primitives in contracts. |
| Is validation centralized enough? | **Yes for capture** via `VersionSchemaValidator` + `KeywordValidator`; registry single-error API remains a minor gap. |
| Is the metadata model becoming too large? | **Not yet** — complexity is factored into value objects/validators; `DocumentType` orchestration file is the main growth point. |
| Are aggregate boundaries still correct? | **Yes** — three roots; version holds immutable snapshots, not live group entities. |
| What should NOT be implemented yet? | Persistence, EF, APIs, MediatR, UI, datasets, full field catalog, rules/file requirement contracts, document metadata storage. |
| What should be deferred intentionally? | See §11 “Defer intentionally”. |
| Is Wave 3 ready? | **Yes**, after Wave 3 slice backlog is written; domain foundation does not block. |

### Risks found

1. **`DocumentType` orchestration growth** — many `CreateVersion` overloads; maintainability under future slices.
2. **Custom layout may omit composed fields** — renderer must define behavior; not enforced in domain.
3. **Registry vs version snapshot drift** — keyword rules apply from current registry at capture time.
4. **Three aggregate roots** — application/persistence must coordinate loads for version authoring.
5. **`ValidateKeywordValue` first-error-only** — carried from Wave 1; schema validation path is better.
6. **Documents BC has no metadata integration yet** — expected; Wave 3 must wire explicitly.
7. **Wave 3 backlog undefined** — process risk, not domain defect.

### Non-blocking recommendations

- Define `ai/backlog/mvp/wave-3-capture/` slices before coding.
- Document renderer behavior for unplaced composition fields when layout is explicit.
- Consider internal `VersionCreationBuilder` if more `CreateVersion` variants are added.
- Add cross-BC contract reference slice early in Wave 3 for `Document` + metadata payload.

### Whether Wave 3 can start

**Yes** — domain and contracts are ready. **Next step:** author Wave 3 slice files, then start the first slice (likely document metadata attachment or capture validation orchestration in Documents — per roadmap, not this review).

### Files changed (this review task)

| File | Action |
|------|--------|
| `docs/reviews/wave-2-domain-review.md` | Created |
| `ai/context/memory.md` | Updated |
| `ai/context/current-state.md` | Updated |
| `ai/tasks/current-task.md` | Updated |

### Architectural concerns requiring immediate attention

**None blocking.** The only urgent process item is **defining Wave 3 slices in the backlog** before implementation — not a domain defect.

---

## Appendix: Review inputs

Documents and code reviewed:

- `ai/context/memory.md`, `current-state.md`, `active-decisions.md`, `known-pitfalls.md`, `ai/tasks/current-task.md`
- `ai/backlog/mvp/index.md`, `wave-2-metadata-layouts/*`
- `docs/reviews/wave-0-domain-review.md`, `wave-1-domain-review.md`
- `docs/domain/metadata-model.md`, `document-type-versioning.md`
- `docs/architecture/renderer-runtime.md`, `backend-architecture.md`, `slice-definition-of-done.md`
- `docs/roadmap/aix-mvp-sequencing.md`, `aix-mvp-capability-board.md`
- `backend/src/AIX.Metadata/Domain/*`, `Events/*`, `DocumentSchemaProjector.cs`
- `backend/src/AIX.Metadata.Contracts/*`
- `backend/tests/AIX.Metadata.Tests/*`

Validation at review time:

```bash
cd backend && dotnet test tests/AIX.Metadata.Tests/AIX.Metadata.Tests.csproj
# Passed: 129/129

cd backend && dotnet test AIX.sln
# Passed: 168/168 (Documents 25, SharedKernel 12, Metadata 129, API smoke 2)
```
