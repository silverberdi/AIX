# Slice 012 — Version Keyword & Group Assignment

## Goal

Bind **keyword groups** and optional standalone keyword references into the version schema composition, with registry/group integrity checks and version-scoped required semantics aligned with `KeywordGroup.IsRequired`.

## Scope

- Version composition entries for `KeywordGroupId` (group placement) with order and instance key
- Repeatable groups: honor `KeywordGroup.IsRepeatable` in composition model (min/max instance rules at domain level if MVP needs bounds; default single instance if not specified)
- Required semantics: version-scoped required flag derived from group definition and/or explicit override where canon allows
- Validate group exists and keyword membership still valid at assignment time (point-in-time integrity, same pattern as slice 008)
- Standalone keywords not in a group may remain via slice 010 field entries only — no duplicate representation of same keyword in group + standalone field
- Immutability: assignments frozen on version; new version for structural changes
- Domain event update for version creation including group assignments snapshot

## Out of Scope

- Layout placement of groups (slice 013)
- Value validation of captured document data (slice 015)
- Keyword registry mutation, group edit after create
- Tables as first-class repeating structures (beyond group repeatability)
- `AIX.ReferenceData` / datasets on SELECT fields
- Persistence, APIs, renderer

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Metadata` | Yes |
| `AIX.Metadata.Tests` | Yes |
| `AIX.SharedKernel` | Avoid |
| Other BCs | No |

## Dependencies

- **slice-008** — `KeywordGroup`, registry integrity at create
- **slice-011** — version composition container

## Acceptance Criteria

- Version can include zero or more group assignments with valid `KeywordGroupId`
- Unregistered group id or duplicate illegal placement fails with `Result`
- Repeatable flag affects allowed instance count rules (document behavior in tests)
- Required flag on group flows into version schema required metadata (testable without slice 015 capture)
- Cannot assign same keyword twice via group + standalone field in one version
- New version does not alter prior version group assignments
- Events expose group assignment snapshot for projections

## Testing Expectations

- Minimum scenarios:
  - assign_keyword_group_to_version_successfully
  - rejects_unregistered_keyword_group_id
  - rejects_duplicate_group_placement_when_not_repeatable
  - repeatable_group_allows_multiple_instances_per_rules
  - required_group_surfaces_required_in_version_schema
  - rejects_keyword_duplicated_in_group_and_standalone_field
  - prior_version_group_assignments_immutable
- Regression: keyword group tests (008), versioning (006, 011)

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`.

## Post-Slice Memory / Backlog Sync

1. `index.md` — slice 012 **Done**
2. `memory.md` — document three-root composition: type + version snapshot references groups by id
3. `current-state.md` — active slice → 013
4. `current-task.md` — slice 013

## Patterns Used

- Aggregate composition
- Integrity validation at version boundary
- Domain Event

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
