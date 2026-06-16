# Slice 021 — Capture Context Contract

## Priority

**Last-priority within Wave 3. Optional if Wave 3 scope must be reduced.** Required only before future API or UI renderer work begins. Not part of the core capture completion path (016–020).

## Goal

Define a capture context contract that bundles document identity, projected document schema, and current captured metadata snapshot for future renderer and API slices — without implementing UI or HTTP endpoints.

## Scope

- `DocumentCaptureContext` DTO in `AIX.Documents.Contracts`
- Fields: document id, document type id, document type version id, taxonomy node id, projected `DocumentSchema` (from `AIX.Metadata.Contracts`), captured metadata snapshot
- Factory or projector in `AIX.Documents` (Application folder if created, or static mapper in Domain if slice pressure is small) assembling context from in-memory Document + projected schema
- Read `AIX.Metadata.Contracts` types only — no Metadata domain reference from Documents domain assembly
- Tests assemble context from test document + `DocumentSchemaProjector` output via test helper
- Document mode constant or enum reference: CAPTURE_MODE (align with renderer canon)

## Out of Scope

- Angular capture renderer, PrimeNG, Avalon
- HTTP APIs returning capture context
- Persistence loading of document or schema
- Runtime multi-tenancy
- Storage providers, OCR, search, retention, workflows
- Rule evaluation, file requirement schema

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Documents.Contracts` | Yes — `DocumentCaptureContext` |
| `AIX.Documents` | Yes — assembly helper/projector |
| `AIX.Documents.Tests` | Yes — may use Metadata projector in test adapter |
| `AIX.Metadata.Contracts` | Read-only reference from Documents.Contracts |

## Dependencies

- **slice-014** — `DocumentSchema` and renderer contract types
- **slice-016** — captured metadata contract types
- **slice-017** — metadata on Document (for snapshot in context)

## Acceptance Criteria

- `DocumentCaptureContext` builds with correct identity fields from Document
- Context includes projected `DocumentSchema` matching bound version
- Context includes current captured metadata snapshot (null if none)
- Documents domain assembly does not reference Metadata domain
- `AIX.Documents.Contracts` references `AIX.Metadata.Contracts` only (not Metadata domain)
- Tests verify context assembly for draft document with and without metadata

## Testing Expectations

- Minimum scenarios:
  - builds_capture_context_with_document_identity
  - includes_projected_document_schema
  - includes_metadata_snapshot_when_present
  - metadata_snapshot_null_when_not_captured
  - capture_context_contract_has_no_metadata_domain_reference
- Full regression: all backend tests

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`; contract and assembly tested. Does not block Wave 3 Domain Review after core slices 016–020 are complete.

## Post-Slice Memory / Backlog Sync

1. `index.md` — slice 021 **Done** (optional slice; core Wave 3 may already be reviewed after 016–020)
2. `memory.md` — Wave 3 complete summary; capture context contract notes
3. `current-state.md` — Wave 3 progress; suggest Wave 3 Domain Review
4. `current-task.md` — point to Wave 3 domain review prep
5. Schedule **Wave 3 Domain Review** doc in `docs/reviews/` (human/agent review task)

## Patterns Used

- DTO / read model
- Contract composition across BCs

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
