# Wave 1 Domain Review

**Date:** 2026-05-22  
**Scope:** Slices 005–009 (DocumentType aggregate, versioning, keyword registry, keyword groups, keyword validation)  
**Reviewer role:** Architectural / domain review (no implementation)

---

## Executive summary

Wave 1 delivered four cohesive domain concepts in `AIX.Metadata`: `DocumentType` (with immutable `DocumentTypeVersion` records), `KeywordRegistry` (with `Keyword` entities), `KeywordGroup`, and centralized `KeywordValidator` with specification-based value rules and configuration-evolution guards. The work uses `Result`-based failures, four domain events, and **59 passing unit tests** in `AIX.Metadata.Tests` (96 total backend domain tests with Wave 0).

The model is **healthy for Wave 2** with deliberate gaps: versions are structural shells without schema/layout composition; keyword groups and the registry are not yet bound to `DocumentTypeVersion`; state changes and registry creation do not emit events. Continue with the same slice discipline; address composition, correlation propagation, and cross-aggregate integrity only when Wave 2 slices create concrete pressure.

---

## 1. Aggregate Health Review

### Evaluation

| Aggregate | Assessment |
|-----------|------------|
| `DocumentType` | **Strong** for slice scope — identity, code/name, active/inactive state, version collection |
| `KeywordRegistry` | **Strong** — registry root with `Keyword` child entities, duplicate guards, registration event |
| `KeywordGroup` | **Strong** — factory validates registry integrity at creation; immutable definition |
| Cross-aggregate cohesion | **Intentionally loose** — no `DocumentType` ↔ keyword wiring yet (Wave 2) |

### What is healthy?

- **Private constructors + factories** on all roots; no public setters on identity or keyword definitions.
- **`DocumentType`** owns `_versions` with immutable `DocumentTypeVersion` records (`Id`, `VersionNumber`, `CreatedAt`).
- **`LatestVersion`** derived from max version number — simple and test-covered.
- **`Keyword`** created via internal `Keyword.Create`; registry enforces duplicate code (case-insensitive) and name.
- **`KeywordGroup.Create`** validates non-empty keyword list, no duplicate IDs, and registry membership — correct integrity boundary for slice 008.
- **`Result` + BC-local error types** (`DocumentTypeErrors`, `KeywordErrors`, `KeywordGroupErrors`, `KeywordValidationErrors`) match Wave 0 patterns.
- **BC isolation** — `AIX.Metadata` references only `AIX.SharedKernel`; no project references to `AIX.Documents`.

### What is risky?

- **Three separate aggregate roots** (`DocumentType`, `KeywordRegistry`, `KeywordGroup`) with no explicit composition model yet. Wave 2 “field schema / layout” work must decide whether versions reference groups, embed schema, or introduce a new composed type — risk of ad-hoc linking under persistence pressure.
- **`Activate` / `Deactivate` do not guard `CreateVersion`** — an inactive document type can still receive new versions. Product may want “inactive = no new versions” later; not wrong for current slices but untested.
- **`IClock` on state transitions** is required but unused on `Activate`/`Deactivate` (same pattern as some Wave 0 methods) — harmless now; avoid assuming timestamps exist for state changes.
- **No document-type code uniqueness** across instances — multiple types can share the same `Code`; enforcement may belong in application layer or a future slice.
- **`KeywordRegistry.Create`** accepts `IClock` but does not set `CreatedAt` or raise an event — registry lifecycle is under-specified for audit.
- **Registry integrity is point-in-time** — `KeywordGroup` validates keywords at create only; deleting or changing registry entries later is not modeled (keywords are append-only today).

### What should NOT be changed yet?

- Do not merge `KeywordRegistry` / `KeywordGroup` into `DocumentType` without a Wave 2 slice that defines the composition model.
- Do not add EF mappings, repositories, or API endpoints.
- Do not validate `DocumentTypeId` existence from `AIX.Documents` domain code.
- Do not add layout/renderer/UI logic to domain aggregates.
- Do not move metadata types into `AIX.SharedKernel`.

### What may need future extraction?

| Concern | When / why |
|---------|------------|
| Version schema / layout graph | Wave 2 — versions need more than number + timestamp |
| `DocumentTypeVersion` as richer entity | When fields, groups, tables attach per versioning canon |
| Keyword configuration updates | Post-MVP — `ValidateConfigurationChange` exists but no `Keyword` mutation API yet |
| Application-level correlation | When command handlers orchestrate multi-step operations |
| Shared ID contracts | If duplicate `DocumentTypeId` CLR types become integration friction |

---

## 2. Versioning Review

### Evaluation

| Area | Assessment |
|------|------------|
| Immutability | **Correct** — `DocumentTypeVersion` is a sealed record; prior versions unchanged when new ones added |
| Version identity | **Correct** — `DocumentTypeVersionId`, monotonic auto-increment or explicit number |
| Duplicate prevention | **Correct** — duplicate version numbers rejected |
| Canon alignment | **Partial** — immutability and document binding intent supported; version payload is empty |
| Gap tolerance | **Monitor** — explicit version `3` allowed without `1` or `2` |

### What is healthy?

- Tests prove auto-increment, explicit numbering, duplicate rejection, invalid `<= 0` rejection, and `LatestVersion` behavior.
- `DocumentTypeVersionCreated` carries `DocumentTypeId`, `DocumentTypeVersionId`, and `VersionNumber` for projections.
- Aligns with ADR/versioning canon: documents reference a fixed version at creation (Documents BC already holds `DocumentTypeVersionId`).

### What is risky?

- **Empty version** — no fields, groups, rules, or policies on `DocumentTypeVersion`. Wave 2 must attach schema without breaking immutability rules.
- **Non-contiguous version numbers** — `CreateVersion(3, …)` succeeds on empty aggregate. May complicate UX and migrations; consider sequential-only policy in a Wave 2 slice if product requires it.
- **No “publish” or “draft version” state** — all versions are immediately materialized. Acceptable for MVP domain slices; layout slices may need lifecycle.
- **Structural change definition** not enforced in domain — creating a version does not require diffs from previous; any call adds a shell version.

### What should NOT be changed yet?

- Do not retroactively mutate existing `DocumentTypeVersion` records.
- Do not tie versioning to keyword registry mutations in Wave 1 code.
- Do not add migration or backfill logic in domain layer.

---

## 3. Keyword Model Review

### KeywordRegistry

| Area | Assessment |
|------|------------|
| Reusability | **Yes** — keywords registered once, referenced by ID from groups |
| Type immutability | **Enforced** — no public mutators; tests guard API surface |
| Length constraints | **At registration** — max length only for `Text`; optional null max |
| Events | `KeywordRegistered` on success |

**Healthy:** Case-insensitive duplicate code; duplicate name; trim on code/name; `KeywordDataType` enum covers MVP types.

**Gaps (acceptable for Wave 1):** No keyword deprecation, delete protection, or “in use” checks per versioning canon. No numeric range on `Number` type. Label/description changes not modeled (only `Name` at registration).

### KeywordGroup

| Area | Assessment |
|------|------------|
| Group integrity | **Good at creation** — registered keywords only, no duplicates in list |
| Repeatable / required flags | **Modeled** on group, not on individual keywords |
| Reusability | Groups are standalone aggregates — reusable in principle, not yet referenced by document types |

**Healthy:** `KeywordGroupCreated` includes full definition snapshot including `KeywordIds`.

**Risks:**

- **`IsRequired` on group vs `isRequired` on validation** — group-level required is stored but value validation uses `isRequired` passed into `ValidateKeywordValue`. Wiring group → field required is a Wave 2 / capture concern; risk of inconsistent required semantics if not centralized later.
- **No group code uniqueness** — multiple groups may share `Code` (unlike keywords in registry).
- **Snapshot of `KeywordIds` in event** — list is mutable reference type in event record; immutability relies on caller passing stable lists (factory passes the input list after validation).

### Cross-model gap (expected)

`DocumentType`, `DocumentTypeVersion`, `KeywordRegistry`, and `KeywordGroup` are **not linked**. Wave 2 “Field Schema Model” and “Layout Generation” must introduce composition (likely version-scoped schema referencing group/keyword IDs).

---

## 4. Validation Review

### Evaluation

| Area | Assessment |
|------|------------|
| Centralization | **Yes** — `KeywordValidator` static entry point |
| Specification pattern | **Yes** — `RequiredKeywordValueSpecification`, `KeywordDataTypeValueSpecification`, `TextMaxLengthValueSpecification` |
| Value rules | Required, type parse (invariant culture), text max length |
| Configuration evolution | Data type change forbidden; max-length reduction forbidden; increase allowed |
| Registry integration | `KeywordRegistry.ValidateKeywordValue` delegates to validator |

### What is healthy?

- Multiple validation failures collected in `KeywordValidationResult` for direct validator use (test: required + invalid boolean).
- Configuration rules align with **keyword compatibility** section of `document-type-versioning.md` (subset implemented: data type, max length).
- Optional empty values skip type parsing — correct for optional fields.

### What is risky?

- **`ValidateKeywordValue` returns only first error** — despite `KeywordValidationResult` supporting multiple errors, registry API maps to `Result.Failure(validation.Errors[0])`. API/consumers may under-report failures until addressed in application layer or a slice.
- **Configuration policy incomplete vs canon** — no numeric range, no “stricter validation” generalization, no keyword delete/in-use rules (not in scope).
- **`isRequired` is extrinsic** — not derived from `KeywordGroup.IsRequired`; capture/render layers must pass correct flag.
- **Date parsing** — `DateOnly.TryParse` with invariant culture only; locale-specific formats not supported (acceptable MVP).
- **Boolean parsing** — `bool.TryParse` only (`true`/`false`); no `1`/`0` (acceptable unless product demands).

### What should NOT be changed yet?

- Do not add renderer or external dataset validation.
- Do not add cross-BC validation from Documents to Metadata in domain code.

---

## 5. Domain Event Review

### Current events

| Event | Emitted when | Payload highlights |
|-------|----------------|-------------------|
| `DocumentTypeCreated` | `DocumentType.Create` | Id, name, code |
| `DocumentTypeVersionCreated` | Successful `CreateVersion` | Type id, version id, number |
| `KeywordRegistered` | Successful `Register` | Registry id, keyword definition |
| `KeywordGroupCreated` | Successful `KeywordGroup.Create` | Group definition + keyword ids |

### Evaluation

| Area | Assessment |
|------|------------|
| Naming | Consistent past-tense records |
| Granularity | One business fact per successful mutation — appropriate |
| Timing | Raised only after successful state change |
| Missing (deferred) | Activate/deactivate, registry created, validation failures |
| Noise level | Low |
| CorrelationId | New per event inside aggregate — same Wave 0 note |

### Alignment with event-model canon?

**Partially.** Core fields (`event_id`, `occurred_at`, `correlation_id`) are present via `DomainEvent`. Canon also lists `aggregate_type`, `aggregate_id`, `actor_id`, `causation_id` — appropriately deferred to application/infrastructure.

### Events that correctly do NOT exist yet

- `DocumentTypeActivated` / `DocumentTypeDeactivated`
- `KeywordRegistryCreated`
- Validation failure events (policy: modification audited, not every validation attempt)
- Schema/layout change events

**Verdict:** Event set is appropriate for Wave 1 scope; not over-evented.

---

## 6. SharedKernel Boundary Review

### Usage in `AIX.Metadata`

| Primitive | Usage | Verdict |
|-----------|--------|---------|
| `Entity<TId>` | All roots and `Keyword` | Appropriate |
| `StronglyTypedId<Guid>` | All ID records | Appropriate |
| `Result` / `Error` | All behavior methods | Appropriate |
| `DomainEvent` | Four event records | Appropriate |
| `IClock` | Factories and mutations | Appropriate |
| `CorrelationId` | Per event | Appropriate |

### Boundary health

- **No business vocabulary in SharedKernel** — confirmed.
- **No cross-BC project references** — Metadata → SharedKernel only.
- **Duplicate `DocumentTypeId`** — `AIX.Documents.Domain.DocumentTypeId` and `AIX.Metadata.Domain.DocumentTypeId` are separate CLR types with same GUID shape. Same GUID values work across BCs at runtime; integration via contracts/events remains correct. **Monitor** if Wave 2 introduces shared contract IDs.

### What should not expand yet

- No specification framework in SharedKernel (specifications stay in Metadata).
- No repository, MediatR, or validation pipeline types in SharedKernel.

---

## 7. Persistence Pressure Review

*Identification only — no fixes in this review.*

| Pressure point | Detail |
|----------------|--------|
| **`DocumentType._versions`** | Owned collection or child table; versions as records need mapping strategy |
| **Three aggregate roots** | Three repositories / units of work unless schema composition collapses boundaries in Wave 2 |
| **`KeywordGroup._keywordIds`** | List of GUIDs — join table or JSON column; no navigation to `Keyword` entity |
| **`KeywordRegistry._keywords`** | Child entities; case-insensitive uniqueness needs DB index strategy (CI collation or normalized column) |
| **Private constructors** | EF materialization ctor deferred to persistence slice |
| **`_domainEvents` on three types** | Outbox/dispatch + clear on save for each aggregate type |
| **Strongly typed IDs** | Value converters per BC — duplicate `DocumentTypeId` types need separate converters |
| **Empty `DocumentTypeVersion`** | Schema JSONB (per canon) will attach to version row — major Wave 2 mapping decision |
| **Immutability** | DB cannot enforce version immutability without append-only writes and app rules |
| **No `tenant_id`** | Per-tenant DB model — consistent with architecture doc |

---

## 8. Testing Review

### Test inventory

| File | Tests | Focus |
|------|-------|--------|
| `CreateDocumentTypeTests` | 6 | Create, validation, event |
| `DocumentTypeStateTests` | 4 | Activate/deactivate |
| `DocumentTypeVersioningTests` | 9 | Versioning + slice 005 regression |
| `KeywordRegistryTests` | 13 | Register, duplicates, immutability, event, regression |
| `KeywordGroupTests` | 10 | Group rules, event, regression |
| `KeywordValidationTests` | 17 | Validator, config policy, registry delegation, regression |
| **Total** | **59** | |

Wave 0 + Wave 1 backend tests: **25 + 12 + 59 = 96** (per `dotnet test` on Metadata project at review time).

### What is strong?

- Happy and failure paths for every public behavior introduced in slices 005–009.
- Snake_case test names describe behavior clearly.
- Domain events asserted with payload checks.
- Cross-slice regression tests (`slice_005_*`, `slice_005_through_008_*`) reduce drift.
- `FakeClock` for deterministic timestamps.

### Gaps (prioritized)

| Scenario | Priority |
|----------|----------|
| Inactive `DocumentType` still allows `CreateVersion` | Medium — document if intentional |
| Non-contiguous version numbers | Low — document or restrict in Wave 2 |
| `ValidateKeywordValue` multi-error surfacing | Medium — when API slice needs it |
| `KeywordGroup` duplicate code across groups | Low |
| Deactivate/activate events | Low — defer until audit slice |
| Integration: Documents `DocumentTypeId` vs Metadata type | Low — contract slice |

### What should NOT be tested yet?

- EF mappings, repositories, HTTP APIs.
- Renderer, layouts, datasets.
- Cross-BC existence checks at domain level.
- Performance tests on large keyword registries.

---

## 9. Wave 2 Readiness

### Is the foundation stable enough?

**Yes.** Wave 1 delivered reusable keywords, groups, validation, and version shells. Wave 2 capability board items (layout generation, field schema model, renderer contracts, dataset abstraction) can build on these primitives without Wave 1 rework.

### Architectural blockers?

**None.** No forbidden patterns (persistence, MediatR, cross-BC references, SharedKernel bloat) observed.

### Technical debt to address before Wave 2?

**None blocking.** Track in Wave 2 slices:

1. **Compose schema onto `DocumentTypeVersion`** (fields, groups, layout sections per canon).
2. **Decide aggregate boundaries** when versions reference groups — avoid three-root transactions without explicit design.
3. **Required semantics** — unify `KeywordGroup.IsRequired` with validation entry points.
4. **Correlation propagation** — application layer when handlers appear.
5. **Duplicate `DocumentTypeId` types** — contracts package if integration tests need shared types.

### What to monitor during Wave 2?

- Version immutability when attaching schema (add-only / deprecate-hide, never delete per canon).
- JSONB validation against version schema (canon) — likely application + infrastructure, not pure domain.
- Keyword compatibility rules when configuration mutation APIs appear.
- Event granularity as layout/schema changes grow.

---

## 10. Recommendations

### Safe to continue

- Current aggregate APIs and `Result` error patterns.
- Immutable `DocumentTypeVersion` records and append-only version list.
- Separate `KeywordRegistry` and `KeywordGroup` roots until composition slice defines wiring.
- `KeywordValidator` + specification layout for value rules.
- Four domain events at current granularity.
- Test-first slice workflow and `AIX.Metadata.Tests` structure.

### Monitor carefully

- Three aggregate roots → composition and persistence transaction boundaries in Wave 2.
- Empty versions → schema attachment without breaking immutability.
- `ValidateKeywordValue` first-error-only vs multi-error `KeywordValidationResult`.
- `isRequired` passed at validation time vs group-level `IsRequired`.
- Per-event `CorrelationId.New()` inside aggregates.
- Duplicate `DocumentTypeId` CLR types across BCs.
- Non-contiguous version numbers and inactive-type versioning policy.

### Avoid

- EF Core, repositories, APIs, UI during Wave 2 domain slices unless slice explicitly requires.
- Merging metadata concepts into SharedKernel or Documents domain.
- Validating document type existence from `Document` aggregate.
- Retroactive version mutation or keyword deletion without canon-compliant slices.
- Drive-by refactors of skeleton projects.

### Defer intentionally

- Persistence, outbox, `ClearDomainEvents`.
- Activate/deactivate and registry-created events.
- Full keyword compatibility canon (numeric ranges, delete protection, in-use checks).
- Renderer, datasets, capture pipeline.
- Cross-BC integration tests and shared contract packages.
- Numeric range and stricter-validation policies on keywords.

### Recommended fixes (optional, not blocking Wave 2)

| Item | Severity | Suggestion |
|------|----------|------------|
| Multi-error `ValidateKeywordValue` | Low | Future slice: return `Result` with aggregated errors or expose `KeywordValidationResult` on registry API |
| Unused `IClock` on Activate/Deactivate | Low | Use clock for future events or remove parameter when events added |
| Version number gaps | Low | Add domain rule in Wave 2 if product requires sequential-only |
| Document type `Code` normalization | Low | Trim/normalize in `Create` when API slice needs stable codes |

---

## 11. Final Verdict

| Dimension | Verdict |
|-----------|---------|
| **Wave 1 domain quality** | **Good** — focused aggregates, clear invariants for slice scope, aligned with versioning/keyword intent |
| **Architectural stability** | **Stable** — BC isolation preserved, SharedKernel minimal, no forbidden patterns |
| **AI execution discipline** | **Strong** — slices 005–009 respected scope, tests match acceptance criteria, regression tests present |
| **Readiness for Wave 2** | **Ready** — proceed with Metadata & Layouts wave per roadmap; no Wave 1 rework required |

### Review verdict

**PASS — Wave 1 domain review complete.** Foundation is sound for Wave 2 (field schema, layouts, renderer contracts).

### Risks found

1. **Composition gap** — versions, document types, registry, and groups are not yet linked (expected; Wave 2 must design explicitly).
2. **Three aggregate roots** — persistence and transactions need a deliberate composition model.
3. **Validation API asymmetry** — multi-error collection vs first-error `Result` on registry.
4. **Required semantics split** — group `IsRequired` vs per-call `isRequired` on validation.
5. **Duplicate `DocumentTypeId` types** across BCs (manageable; monitor for contracts).
6. **Correlation per event** — operation-scoped tracing deferred (inherited from Wave 0).

### Whether Wave 2 can start

**Yes.** Start Wave 2 domain slices when defined in backlog (field schema / layout composition). Do not block on Wave 1 code changes.

---

## Appendix: Review inputs

Documents and code reviewed:

- `ai/context/memory.md`, `current-state.md`, `active-decisions.md`, `known-pitfalls.md`, `ai/tasks/current-task.md`
- `ai/backlog/mvp/index.md`
- `docs/reviews/wave-0-domain-review.md`
- `docs/domain/metadata-model.md`, `document-type-versioning.md`, `event-model-canon.md`
- `docs/architecture/backend-architecture.md`, `slice-definition-of-done.md`
- `docs/roadmap/aix-mvp-sequencing.md`, `aix-mvp-capability-board.md`
- `backend/src/AIX.Metadata/Domain/*`, `Events/*`
- `backend/tests/AIX.Metadata.Tests/*`

Validation at review time:

```bash
cd backend && dotnet test tests/AIX.Metadata.Tests/AIX.Metadata.Tests.csproj
# Passed: 59/59
```
