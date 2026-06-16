# Slice 010 — Field Schema Model

## Goal

Introduce reusable **field schema** domain primitives: standalone field definitions that bind semantic keywords to presentation/control metadata (field catalog types per renderer canon), without yet attaching them to `DocumentTypeVersion`.

## Scope

- `FieldSchema` (or equivalent domain name aligned with canon) as immutable value object / record
- Field identity (`FieldSchemaId` or stable field key within a future version scope)
- Binding to `KeywordId` from `KeywordRegistry`
- Field catalog / control type enum aligned with `docs/architecture/renderer-runtime.md` fixed field catalog (MVP subset: TEXT, TEXTAREA, NUMBER, DECIMAL, DATE, DATETIME, BOOLEAN — defer SELECT/MULTISELECT/TABLE/FILE/RICH_TEXT unless trivial)
- Optional display metadata: label override, help text, order hint (non-layout)
- Deprecation/hide flags for fields (add-only evolution prep; no delete)
- Factory/validation via `Result` and BC-local errors
- Domain events only if a new aggregate root is introduced; prefer value objects owned by future version composition

## Out of Scope

- Attaching fields to `DocumentTypeVersion` (slice 011)
- `KeywordGroup` placement on versions (slice 012)
- `LayoutSchema`, sections, default layout generation (slices 013–014)
- JSON Schema files under `artifacts/schemas/` (slice 014)
- Document instance metadata / JSONB persistence
- Renderer Angular code, UI, datasets / `AIX.ReferenceData`
- Persistence, APIs, EF Core, MediatR
- Rules, policies, tables, file requirements

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Metadata` (Domain, Events if needed) | Yes |
| `AIX.Metadata.Tests` | Yes |
| `AIX.SharedKernel` | Only if a primitive is genuinely missing (avoid) |
| `AIX.Metadata.Contracts` | No — defer serializable DTOs to slice 014 |
| All other BCs and Api projects | No |

## Dependencies

- **slice-007** — `KeywordRegistry`, `Keyword`, `KeywordId`
- **slice-009** — `KeywordValidator` (reference for data-type alignment; do not duplicate value validation here)

## Acceptance Criteria

- Field definitions can be constructed with a registered `KeywordId` and valid catalog/control type
- Invalid keyword reference (unregistered id) fails with `Result` failure
- Incompatible control type vs keyword `KeywordDataType` fails (explicit matrix or documented rules in tests)
- Field identity and definition are immutable after creation
- Deprecate/hide semantics are modeled (hidden fields excluded from “active” schema queries if exposed)
- Build passes; new tests pass; existing `AIX.Metadata.Tests` regression count unchanged or increased only by new tests
- No forbidden patterns (persistence, cross-BC references, SharedKernel bloat)

## Testing Expectations

- Test-first: add tests in `AIX.Metadata.Tests` before or alongside domain types
- Naming: `snake_case` behavior descriptions (match Wave 0/1)
- Minimum scenarios:
  - creates_field_schema_with_registered_keyword_successfully
  - rejects_unregistered_keyword_id
  - rejects_incompatible_control_type_for_keyword_data_type
  - field_definition_is_immutable
  - deprecate_or_hide_flag_prevents_active_inclusion (if API exposes active filter)
- Regression: run full `AIX.Metadata.Tests` and solution test command

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`:

- Behavior implemented in allowed projects only
- All new and existing Metadata tests pass
- `dotnet build` / `dotnet test` on `backend/AIX.sln` succeeds
- No placeholder logic, no unauthorized TODOs
- Architecture rules respected (BC isolation, minimal SharedKernel)

## Post-Slice Memory / Backlog Sync

1. Mark slice **Done** in `ai/backlog/mvp/index.md`
2. Update `ai/context/memory.md` — Wave 2 active slice, field schema primitives summary, test count
3. Update `ai/context/current-state.md` — slice status, backend test totals
4. Point `ai/tasks/current-task.md` at **slice-011** (or next pending Wave 2 slice)
5. Do **not** edit `docs/` unless slice discovered a canon gap (then minimal doc fix only)

## Patterns Used

- Value Object
- Factory Method
- Specification (optional, for control-type vs data-type rules)

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
