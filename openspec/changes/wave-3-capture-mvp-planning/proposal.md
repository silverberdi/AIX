## Why

AIX has completed Waves 0–2: the Document aggregate, DocumentType versioning, and version-scoped metadata schema with capture validation exist in `AIX.Metadata`, but Documents cannot yet hold captured metadata, enforce validation at capture time, or gate completion on metadata correctness. Wave 2 Domain Review (**PASS**, 2026-05-23) confirmed the domain foundation is ready and explicitly requires Wave 3 slices to be defined under `ai/backlog/mvp/` before any implementation begins.

This planning change defines Wave 3 — Capture MVP as a set of small, dependency-ordered, domain-first slices so each slice can later become its own atomic OpenSpec change and implementation session without scope drift into persistence, APIs, UI, or infrastructure.

## What Changes

- Author the Wave 3 — Capture MVP product and technical scope as OpenSpec planning artifacts (`proposal.md`, `design.md`, `tasks.md`, delta specs).
- Define **six** implementable slices (016–021) under `ai/backlog/mvp/wave-3-capture/` with explicit scope, dependencies, acceptance criteria, and Definition of Done. Slices 016–020 form the core capture completion path; slice 021 is last-priority and optional within Wave 3.
- Update `ai/backlog/mvp/index.md` with Wave 3 slice registry entries.
- Select **slice-016-capture-metadata-contracts** as the first active slice and point `ai/tasks/current-task.md` at it.
- Produce delta specs describing observable capture behaviors that future implementation slices must satisfy.

**No production code, persistence, APIs, UI, or infrastructure changes in this change.**

## Capabilities

### New Capabilities

- `capture-contracts`: Cross-bounded-context contract types for captured metadata payloads and validation results, enabling Documents to integrate with Metadata validation without direct domain references.
- `capture-metadata-attachment`: Document aggregate holds version-scoped captured metadata while in Draft; metadata is immutable after Complete.
- `capture-metadata-validation`: Metadata updates and completion paths delegate to version-scoped schema validation; validation failures block capture mutations deterministically.
- `capture-completion-rules`: Completion readiness evaluates metadata validity and file attachment rules before allowing Draft → Complete transition.

### Modified Capabilities

- _(none — no stable specs exist under `openspec/specs/` yet; Wave 3 behaviors are net-new requirements captured as planning deltas)_

## Impact

| Area | Impact |
|------|--------|
| **Planning artifacts** | `openspec/changes/wave-3-capture-mvp-planning/*` |
| **Backlog** | `ai/backlog/mvp/wave-3-capture/*`, `ai/backlog/mvp/index.md` |
| **Task pointer** | `ai/tasks/current-task.md` → slice-016 |
| **Bounded contexts (future slices)** | `AIX.Documents`, `AIX.Documents.Contracts` (new), `AIX.Metadata` (validation adapter only) |
| **Out of scope** | Persistence, EF Core, repositories, HTTP APIs, MediatR, Angular/PrimeNG/Avalon, runtime multi-tenancy, storage providers, OCR, search, retention, workflows |

**Validation strategy for this planning change:** Review artifact completeness against Wave 2 Domain Review recommendations and capability board; confirm slice dependency graph is acyclic; confirm each slice has testable acceptance criteria and explicit out-of-scope boundaries. No backend build/test required for planning-only delivery.

**Risks to track in implementation slices:**

- Duplicate `DocumentTypeId` CLR types across Documents and Metadata — integrate via contracts/events only.
- Registry drift vs frozen composition at capture validation time (known MVP trade-off from Wave 2).
- `Complete()` today has no metadata or file readiness checks — Wave 3 slices must evolve behavior without breaking immutability invariants from Wave 0.
