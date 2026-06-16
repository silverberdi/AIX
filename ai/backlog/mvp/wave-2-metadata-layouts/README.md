# Wave 2 — Metadata & Layouts

**Theme:** Transform metadata into declarative UX (per `docs/roadmap/aix-mvp-sequencing.md`).

**Prerequisite:** Wave 1 complete; Wave 1 Domain Review **PASS** (`docs/reviews/wave-1-domain-review.md`).

**Goal:** Compose version-scoped field schema, bind keywords and groups, model layout separately from composition, prepare renderer-facing contracts, and integrate validation against the version schema.

## Slices (execution order)

| Slice | File | Depends on |
|-------|------|------------|
| 010 | `slice-010-field-schema-model.md` | 007, 009 |
| 011 | `slice-011-version-schema-composition.md` | 006, 010 |
| 012 | `slice-012-version-keyword-group-assignment.md` | 008, 011 |
| 013 | `slice-013-layout-schema-model.md` | 011, 012 |
| 014 | `slice-014-renderer-contract-preparation.md` | 013 |
| 015 | `slice-015-schema-validation-integration.md` | 009, 012, 014 |

## Canon references

- `docs/domain/metadata-model.md`
- `docs/domain/document-type-versioning.md`
- `docs/architecture/renderer-runtime.md`
- `docs/adrs/010-renderer-is-declarative.md`
- `artifacts/schemas/README.md`

## Deferred (not in Wave 2 slices)

- Persistence, EF Core, repositories, HTTP APIs
- Angular renderer, PrimeNG, capture UI
- Reference Data BC / dataset runtime (capability board “Dataset Abstraction” — stub only in contracts slice if needed)
- Rules, policies, tables (repeatable structures beyond group assignment), file requirements
- Cross-BC existence checks from `AIX.Documents`

## Post-wave

When all slices 010–015 are **Done**, run **Wave 2 Domain Review** (same pattern as Wave 0/1) before Wave 3 Capture MVP.
