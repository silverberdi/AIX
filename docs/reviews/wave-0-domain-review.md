# Wave 0 Domain Review

**Date:** 2026-05-17  
**Scope:** Slices 001–004 (Document aggregate, SharedKernel primitives, state transitions, document files)  
**Reviewer role:** Architectural / domain review (no implementation)

---

## Executive summary

Wave 0 delivered a small, coherent `Document` aggregate with explicit factory and mutation methods, `Result`-based failure paths, three domain events, and **37 passing unit tests** (25 `AIX.Documents.Tests`, 12 `AIX.SharedKernel.Tests`). The foundation aligns with project canon (document ≠ file, type identity immutable, event-driven intent) and respects slice boundaries (no persistence, APIs, or cross-BC coupling).

The model is **healthy for Wave 1** with conservative follow-through: defer policy-heavy rules to DocumentType slices, avoid premature extraction or infrastructure, and monitor correlation/event plumbing and cross-BC ID conventions as `AIX.Metadata` grows.

---

## 1. Aggregate Health Review

### Evaluation

| Area | Assessment |
|------|------------|
| Cohesion | **Strong** — create, complete, and attach-file behaviors live on one root with shared invariants |
| Responsibility boundaries | **Appropriate** — document identity, draft/complete lifecycle, and file references belong together per document canon |
| Size | **Not too large** (~160 lines) — readable; growth expected in later waves (metadata values, keywords) |
| Files inside aggregate | **Correct** — canon treats the document as the aggregate, not individual files |
| Invariant protection | **Good** — private ctor, factory-only creation, readonly type refs, complete-state guard |
| Mutation rules | **Explicit** — failures via `DocumentErrors` and `Result`; no public setters on identity |
| State transitions | **Controlled** — only `Draft → Complete`; re-complete fails; complete blocks file attach |
| Method clarity | **High** — `Create`, `Complete`, `AttachFile` are easy to follow |

### What is healthy?

- Immutable `DocumentTypeId` / `DocumentTypeVersionId` after creation (private readonly + tests).
- `DocumentState` is minimal (`Draft`, `Complete`) and enforced inside the aggregate.
- File rules match slice 004 scope: one primary, no duplicate file IDs, metadata validation, no mutation when complete.
- `DocumentFile` as an immutable record keeps the file model simple and persistence-friendly.
- In-memory `DomainEvents` collection supports future dispatch without forcing infrastructure now.
- `IClock` injection supports deterministic tests and future application-layer orchestration.

### What is risky?

- **No existence validation** for `TaxonomyNodeId` or `UserId` on create (empty GUIDs are allowed). Type/version empty checks exist; other identity fields are asymmetric.
- **Complete has no composition rules** — a document can complete with zero files. Canon says file structure is DocumentType-configurable; enforcing “primary required” belongs in a later slice, but the gap should be tracked.
- **Single immutability gate** — only `Complete` blocks `AttachFile`. Future mutations (metadata, keywords) must repeat the complete guard or centralize it to avoid drift.
- **CorrelationId per event** — each operation calls `CorrelationId.New()` inside the aggregate, so a single user operation spanning create + attach + complete will not share correlation until an application layer owns it.
- **Document will grow** — Wave 1+ will add cross-cutting references (keywords, type-driven validation). The aggregate is not at risk today but is the natural accumulation point.

### What should NOT be changed yet?

- Do not extract `DocumentFile` or file attachment into a separate aggregate or service.
- Do not add workflow states, approval gates, or policy engines to `Document`.
- Do not add persistence constructors, repositories, or EF mappings.
- Do not introduce cross-BC calls from `Document.Create` to validate `DocumentTypeId` against `AIX.Metadata`.
- Do not add detach/replace/derived-file APIs until a slice requires them.

### What may need future extraction?

| Concern | When / why |
|---------|------------|
| DocumentType-driven file policy | Slice 006+ — primary required, allowed MIME types, supporting file rules |
| Keyword / metadata value objects | Slices 007–009 — may stay on aggregate but could become composed types |
| Derived files | Later wave — separate role/category per file model canon |
| Correlation / causation propagation | Application or infrastructure slice — not aggregate-internal |
| Event dispatch / outbox | Persistence slice — `ClearDomainEvents` or equivalent |

---

## 2. Domain Event Review

### Current events

| Event | Emitted when | Payload |
|-------|----------------|---------|
| `DocumentCreated` | Successful `Create` | Identity snapshot (type, version, taxonomy, creator) |
| `DocumentCompleted` | Successful `Complete` | `DocumentId` |
| `DocumentFileAttached` | Successful `AttachFile` | File metadata + role |

### Evaluation

| Area | Assessment |
|------|------------|
| Naming | Consistent past-tense, aggregate-prefixed records |
| Granularity | One meaningful business change per event — appropriate |
| Timing | Raised only after successful state change |
| CorrelationId | Present but **not operation-scoped** (new ID per event) |
| Noise level | Low (3 events for 3 operations) |
| Missing events | None required for Wave 0 scope |
| Over-eventing | Not a concern at current scale |

### Are events aligned with the domain?

**Yes.** Creation, completion, and file attachment are distinct, auditable facts that match the event-model canon’s intent (structured, immutable, machine-readable). `DocumentCreated` carries enough context for downstream projections; `DocumentFileAttached` duplicates file metadata suitable for audit trails.

### Are events too granular?

**No.** Finer events (e.g. per-field validation failures) would be noise. Coarser events (e.g. one “DocumentChanged”) would lose audit precision.

### Are events missing?

**Not for Wave 0.** The following are correctly **absent** until product slices demand them:

- File detached / replaced
- State reverted or workflow transitions
- Read/access events (canon: policy-driven, not domain aggregate concern)
- DocumentType or taxonomy change on existing document (forbidden by canon)

### Should any event NOT exist yet?

**No.** All three events correspond to implemented, tested behavior and future audit/integration needs.

### Correlation and causation notes

- `DomainEvent` supports optional `CausationId`; document events do not pass it — acceptable now.
- Event canon lists `actor_id`, `aggregate_type`, `causation_id` — only partially modeled at SharedKernel level. Filling these belongs to application/infrastructure layers, not Wave 0 aggregate code.
- **Recommendation for Wave 1+:** pass `CorrelationId` (and optionally `CausationId`) from the application command into aggregate methods rather than generating inside the aggregate.

---

## 3. SharedKernel Review

### Primitives reviewed

| Primitive | Role | Verdict |
|-----------|------|---------|
| `Error` / `Result` / `Result<T>` | Operation outcomes without exceptions | **Appropriate** |
| `Entity<TId>` | Identity root base | **Appropriate**, minimal |
| `StronglyTypedId<TValue>` | GUID wrappers | **Appropriate** |
| `DomainEvent` | Event metadata base | **Appropriate** |
| `CorrelationId` / `CausationId` | Tracing | **Appropriate**, causation unused |
| `IClock` / `SystemClock` | Time abstraction | **Appropriate** |
| `ValueObject` | Equality-based VOs | **Appropriate**; `DocumentFile` uses `record` instead (acceptable) |

### What is appropriate?

- SharedKernel stays small (6 source files under `Primitives/`) with no business vocabulary.
- `Result` pattern is used consistently in `Document` behavior.
- `DomainEvent` record hierarchy is extensible without forcing a specific dispatch mechanism.
- Tests cover primitives in isolation (12 tests) — good guard against accidental SharedKernel bloat.

### What is premature?

- **Nothing critical.** `CausationId` on the base event is forward-looking but low cost.
- `ValueObject` base is tested but unused by `DocumentFile` — not harmful; avoid forcing all VOs through the base unless equality semantics need it.

### What should remain stable?

- `Result` / `Error` shape and failure semantics.
- `StronglyTypedId<Guid>` pattern for BC-local ID records.
- `IClock` for testability.
- `DomainEvent` core fields: `EventId`, `OccurredOn`, `CorrelationId`.

### What should not expand yet?

- No aggregate base class beyond `Entity<TId>`.
- No repository interfaces, unit of work, or MediatR types in SharedKernel.
- No tenant, actor, or audit envelope types in SharedKernel — keep those in application/infrastructure or BC contracts.
- No generic “domain service” or specification framework.

---

## 4. Persistence Pressure Review

*Identification only — no fixes in this review.*

| Pressure point | Detail |
|----------------|--------|
| **Private collection + `IReadOnlyList`** | `_files` exposed read-only — EF will map as owned collection or child table; pattern is standard |
| **`DocumentFile` record** | EF Core owned entities / complex types support records; role enum maps cleanly |
| **No parameterless constructor** | Required for EF materialization later — add only in persistence slice, keep domain ctor closed |
| **In-memory `_domainEvents`** | Needs outbox or dispatch + clear on save; risk of duplicate publish if not cleared |
| **Strongly typed ID records** | Value converters per ID type — manageable, watch duplicate converters across BCs |
| **Unbounded `_files` list** | Large documents with many supporting files could bloat aggregate load; acceptable for MVP; monitor if pagination/summary views are needed |
| **Immutability after complete** | DB must enforce via application rules initially; optional DB constraints later |
| **Event payload duplication** | `DocumentFileAttached` repeats metadata also stored on entity — normal for audit; watch storage volume |
| **Cross-table references** | `DocumentTypeId` in Documents without FK to Metadata until integration — by design for modular monolith |

---

## 5. Testing Review

### Test inventory

| Project | Tests | Focus |
|---------|-------|--------|
| `AIX.Documents.Tests` | 25 | Create (7), Complete (5), AttachFile (13) |
| `AIX.SharedKernel.Tests` | 12 | Primitives |

### What test areas are strong?

- Happy paths and failure paths for all public behaviors.
- Snake_case names describe behavior clearly (`draft_document_can_attach_primary_file`).
- Domain events asserted by type and, for attach, payload fields.
- Event ordering preserved (`DocumentCreated` then later events).
- Complete-state immutability tested for both re-complete and failed attach.
- Immutability of type references verified (reflection on property writability).
- `FakeClock` enables deterministic `CreatedAt` / event timestamps.

### What important behavior may still be missing?

| Scenario | Priority |
|----------|----------|
| Mixed primary + supporting files on same document | Low — implied by separate tests |
| Complete after attach — files and events preserved | Low — worthwhile when persistence lands |
| Empty `TaxonomyNodeId` / `UserId` on create | Low — document if intentional or add validation in a slice |
| `DocumentCompleted` event payload / timestamp from clock | Low |
| Failed attach does not append events | Low — implicit, could be explicit |

### What should NOT be tested yet?

- EF mappings, repositories, API controllers.
- Cross-BC DocumentType existence.
- Storage, upload, antivirus, OCR.
- Workflow, approval, or policy engines.
- Performance or load tests on file collections.

### Over-testing / under-testing

- **Reflection tests** for type immutability are slightly brittle but document intent; acceptable at this stage.
- **Under-testing:** cross-field validation symmetry (taxonomy/user vs type/version) — acceptable deferral unless product requires it in Wave 1.

---

## 6. Wave 1 Readiness (Slice 005 — Document Type Aggregate)

### Is the current foundation stable enough?

**Yes.** Document aggregate references `DocumentTypeId` and `DocumentTypeVersionId` without owning type definition behavior — correct boundary for implementing `DocumentType` in `AIX.Metadata`.

### Are there architectural blockers?

**None.** Slice 005 depends on slice 001 only (per backlog). Documents BC does not need to change for a minimal DocumentType aggregate in Metadata.

### Technical debt that must be addressed first?

**None blocking.** Track but do not fix before slice 005:

- Duplicate `DocumentTypeId` types may appear in `AIX.Metadata.Domain` vs `AIX.Documents.Domain` — use contracts or shared ID types only when a slice explicitly requires cross-BC sharing.
- Per-event `CorrelationId.New()` inside aggregate.

### What to monitor carefully during Wave 1?

1. **BC boundaries** — DocumentType rules stay in `AIX.Metadata`; Documents keeps references only.
2. **ID duplication** — same GUID, different CLR types across BCs is fine; avoid merging BCs via project references.
3. **Policy vs aggregate** — file-required-on-complete and keyword rules belong to type/version slices, not premature changes to `Document.Complete`.
4. **SharedKernel discipline** — resist moving metadata concepts into SharedKernel.
5. **Test placement** — new tests in `AIX.Metadata.Tests`, not expanding Documents tests unless Document behavior changes.

---

## 7. Recommendations

### Safe to continue

- Current `Document` API surface (`Create`, `Complete`, `AttachFile`).
- `Result` + `DocumentErrors` pattern for domain failures.
- Three domain events at current granularity.
- SharedKernel primitive set as implemented.
- Test-first slice workflow and per-slice test projects.
- File references as value objects inside the document aggregate.

### Monitor carefully

- Aggregate size as keywords and metadata attach to documents.
- Correlation/causation propagation when application layer appears.
- `DocumentTypeId` definitions across `AIX.Documents` and `AIX.Metadata`.
- Centralizing “cannot modify when complete” when new mutating methods are added.
- Empty GUID validation consistency across identity fields.
- Domain event list lifecycle before persistence (clear/dispatch).

### Avoid

- EF Core, repositories, MediatR, or API endpoints during Wave 1 domain slices unless the slice says so.
- Validating DocumentType existence from inside `Document` domain code.
- Extracting file or type sub-aggregates without slice pressure.
- Adding workflow states or generic state machines early.
- Putting DocumentType business rules in `AIX.SharedKernel` or `AIX.Documents`.
- Drive-by refactors of skeleton projects.

### Defer intentionally

- Persistence, outbox, and `ClearDomainEvents`.
- Derived files, detach/replace, storage integration.
- Complete-time file composition rules (until DocumentType versioning / policy slices).
- Actor_id enrichment on events (application/infrastructure concern).
- Cross-BC integration tests and contract packages.
- CQRS read models, event sourcing, microservice splits.

---

## 8. Final Verdict

| Dimension | Verdict |
|-----------|---------|
| **Wave 0 domain quality** | **Good** — focused aggregate, clear invariants, aligned with canon, well-tested for scope |
| **Architectural stability** | **Stable** — BC isolation preserved, SharedKernel minimal, no forbidden patterns observed |
| **AI execution discipline** | **Strong** — slices respected scope, tests match acceptance criteria, context files updated per slice |
| **Readiness for Wave 1** | **Ready** — proceed with Slice 005 (`DocumentType` aggregate in `AIX.Metadata`) without Wave 0 rework |

**Overall:** Wave 0 established a conservative, evolvable domain core. Continue into Wave 1 with the same slice discipline; address correlation propagation and type-driven policies only when the relevant slices or application layer create concrete pressure.

---

## Appendix: Review inputs

Documents and code reviewed per task specification:

- `ai/context/*` (memory, current-state, active-decisions, known-pitfalls, current-task)
- `ai/backlog/mvp/index.md`
- `docs/domain/document-canon.md`, `document-lifecycle.md`, `document-file-model.md`, `event-model-canon.md`
- `docs/architecture/backend-architecture.md`, `slice-definition-of-done.md`
- `backend/src/AIX.Documents/Domain/*`, `Events/*`
- `backend/src/AIX.SharedKernel/Primitives/*`
- `backend/tests/AIX.Documents.Tests/*`, `AIX.SharedKernel.Tests/*`

Validation at review time:

```bash
cd backend && dotnet test tests/AIX.Documents.Tests/AIX.Documents.Tests.csproj
# Passed: 25/25
```
