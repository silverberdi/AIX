# Wave 3 — Capture MVP

**Theme:** Move from configurable document schemas to controlled document capture (per `docs/roadmap/aix-mvp-sequencing.md` and `docs/roadmap/aix-mvp-capability-board.md`).

**Prerequisite:** Wave 2 complete; Wave 2 Domain Review **PASS** (`docs/reviews/wave-2-domain-review.md`, 2026-05-23).

**Goal:** Enable draft document capture against a bound document type version — store captured metadata on the Document aggregate, validate against the version schema, and enforce completion readiness before Draft → Complete.

## Slices (execution order)

| Slice | File | Depends on | Priority |
|-------|------|------------|----------|
| 016 | `slice-016-capture-metadata-contracts.md` | 015 | Core |
| 017 | `slice-017-document-metadata-attachment.md` | 016 | Core |
| 018 | `slice-018-capture-validation-port.md` | 016, 017, 015 | Core |
| 019 | `slice-019-document-completion-readiness.md` | 004, 017, 018 | Core |
| 020 | `slice-020-complete-with-capture-enforcement.md` | 019 | Core |
| 021 | `slice-021-capture-context-contract.md` | 014, 016 | **Last / optional** |

**Core path:** slices 016–020 deliver the capture completion loop (contracts → attachment → validation → readiness → enforce on `Complete()`).

**Slice 021** prepares a capture context contract for future renderer and API work. It is last-priority within Wave 3 and optional if scope must be reduced. It is required only before future API/UI renderer implementation begins.

## Canon references

- `docs/domain/document-canon.md`
- `docs/domain/document-type-versioning.md`
- `docs/domain/metadata-model.md`
- `docs/architecture/renderer-runtime.md`
- `docs/reviews/wave-2-domain-review.md`
- `openspec/changes/wave-3-capture-mvp-planning/design.md`

## Deferred (not in Wave 3 slices)

- Persistence, EF Core, repositories, outbox
- HTTP APIs, controllers, MediatR
- Angular capture UI, PrimeNG 21, Avalon theme
- Runtime multi-tenancy implementation
- Physical storage providers, upload pipelines, antivirus, OCR
- Search, retention, workflows, governance policies
- `RuleSchema`, `FileRequirementSchema`, dataset runtime
- Full field catalog types (SELECT, TABLE, FILE, RICH_TEXT)
- Cross-BC integration test project (optional until API slice)

## Post-wave

When core slices 016–020 are **Done**, run **Wave 3 Domain Review** (same pattern as Wave 0/1/2) before Wave 4 Search MVP. Slice 021 may be completed before or after the domain review; it is not blocking for the core capture path but is required before future API/UI renderer work.
