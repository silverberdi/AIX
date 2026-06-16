# Slice 014 — Renderer Contract Preparation & Default Layout Generation

## Goal

Prepare **renderer-facing contracts**: aggregate `DocumentSchema` (composition + layout + version identity), default layout generation when layout omitted, and serializable contract/DTO shapes aligned with `docs/architecture/renderer-runtime.md` and `artifacts/schemas/README.md`.

## Scope

- `AIX.Metadata.Contracts`: versioned DTOs for `DocumentSchema`, `FieldSchema`, `LayoutSchema` (names aligned with canon)
- Mapper or factory on domain side to build contract from `DocumentType` + specific `DocumentTypeVersionId` (read-only export, no mutation)
- **Default layout generation** when version created without explicit layout: deterministic layout from standalone fields, groups, and section heuristics per renderer-runtime canon
- Contract includes: document type id, version id, version number, field catalog metadata, group placements, layout sections
- Stub or documented placeholder for `dataset_id` on SELECT-like fields without implementing Reference Data
- Optional: initial JSON Schema skeleton under `artifacts/schemas/metadata/` (documented `$id` convention) — **documentation/comments only** if generation is manual; no CI code gen required in slice
- Version-aware contract identity (type + version number) for renderer binding

## Out of Scope

- Angular renderer implementation, PrimeNG, `apps/aix-ui`
- Runtime AJV validation in frontend
- OpenAPI HTTP endpoints exposing schemas
- Full `RuleSchema`, `FileRequirementSchema` (future slices)
- Dataset resolution, cascading selects, `AIX.ReferenceData`
- Persistence, EF Core
- Captured document payload validation (slice 015)

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Metadata` (Domain — export/mapping only) | Yes |
| `AIX.Metadata.Contracts` | Yes |
| `AIX.Metadata.Tests` | Yes (domain + mapping tests) |
| `artifacts/schemas/` | README or single example schema file only if stable enough to commit |
| `AIX.SharedKernel` | Avoid |
| Other BCs | No |

## Dependencies

- **slice-013** — `LayoutSchema` model
- **slice-011**, **slice-012** — full version snapshot

## Acceptance Criteria

- Given a document type with a version, contract export produces complete `DocumentSchema` DTO
- Default layout generation produces valid layout referencing all composition entries
- Explicit layout preserved; default generation skipped when layout provided
- Contract mapping is deterministic (same domain state → equal contract)
- No Angular or Api project references required to compile contracts
- Tests verify default layout covers all fields and groups
- Renderer canon field catalog types represented in contract enum/DTO

## Testing Expectations

- Domain tests for default layout generator (pure functions)
- Contract mapping tests: round-trip structural equality on key fields
- Minimum scenarios:
  - generates_default_layout_for_field_only_composition
  - generates_default_layout_including_keyword_groups
  - preserves_explicit_layout_when_provided
  - exports_document_schema_contract_for_version
  - contract_includes_version_identity_for_renderer_binding
- No frontend tests in this slice

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`; contracts project builds and is referenced only from Metadata tests and Metadata domain mapping (no Api wiring).

## Post-Slice Memory / Backlog Sync

1. `index.md` — slice 014 **Done**
2. `memory.md` — note Contracts project usage and default layout rule
3. `current-state.md` — active slice → 015
4. `current-task.md` — slice 015
5. If example JSON Schema added, mention path in `memory.md` (not required)

## Patterns Used

- Anti-corruption / export mapper
- Factory (default layout)
- Contract-first DTOs

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
