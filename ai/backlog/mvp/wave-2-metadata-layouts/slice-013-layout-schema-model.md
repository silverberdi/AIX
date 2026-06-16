# Slice 013 — Layout Schema Model

## Goal

Model **layout** separately from **composition**: `LayoutSchema` with sections and field/group placement so presentation can evolve within versioning rules without redefining semantic structure.

## Scope

- `LayoutSchema` immutable model attached to `DocumentTypeVersion` (or paired snapshot created with version)
- Layout sections (identifiers, titles, order)
- Placement references: field keys and group instance keys from composition (not re-declaring keywords)
- Separation principle from renderer canon: composition = what data exists; layout = how it is presented
- Basic layout constraints: every placed reference must exist in version composition; no orphan placements
- Optional default single-section layout when none supplied (minimal pass-through layout)
- Domain events include layout snapshot on version creation if not already covered

## Out of Scope

- Default layout **generation** algorithm from composition (slice 014)
- JSON Schema export (slice 014)
- Angular renderer, CSS, PrimeNG
- Rules-driven conditional layout
- READ_MODE / compare / audit modes
- Persistence, APIs

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Metadata` | Yes |
| `AIX.Metadata.Tests` | Yes |
| Other BCs | No |

## Dependencies

- **slice-011** — version composition (fields)
- **slice-012** — group assignments on version

## Acceptance Criteria

- Version can be created with explicit `LayoutSchema` or acceptable default
- Layout references only composition keys; unknown reference fails
- Layout immutable for existing versions
- Sections ordered deterministically; tests assert stable ordering
- Composition without layout still valid (default layout path tested)
- Event/projection payload includes layout section structure

## Testing Expectations

- Minimum scenarios:
  - create_version_with_layout_sections_successfully
  - rejects_layout_reference_to_unknown_field
  - rejects_layout_reference_to_unknown_group_instance
  - layout_immutable_on_published_version
  - default_layout_covers_all_composition_entries_when_auto_generated_flag_off (explicit layout only in 013)
  - section_order_is_stable
- Regression: composition and group assignment tests

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`.

## Post-Slice Memory / Backlog Sync

1. `index.md` — slice 013 **Done**
2. `memory.md` — composition vs layout documented for agents
3. `current-state.md` — active slice → 014
4. `current-task.md` — slice 014

## Patterns Used

- Value Object / immutable record
- Declarative schema (domain representation)

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
