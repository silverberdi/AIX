# Slice 015 — Schema Validation Integration

## Goal

Integrate **keyword value validation** with the **version schema**: validate captured metadata payloads against required fields, group instances, and keyword rules defined on a `DocumentTypeVersion` composition.

## Scope

- Version-scoped validation service or aggregate method (e.g. `ValidateMetadataAgainstVersion`) accepting key-value payload model (dictionary or structured value objects — domain-friendly, not JSON parser dependency)
- Required field resolution from composition + group `IsRequired` + field-level flags (unify extrinsic `isRequired` with schema — address Wave 1 review risk)
- Delegate keyword value rules to `KeywordValidator` / `KeywordRegistry.ValidateKeywordValue` per field
- Collect multiple validation failures where practical (`KeywordValidationResult` pattern); improve registry API only if slice scope requires
- Reject unknown keys, missing required keys, deprecated/hidden fields in capture context (policy: hidden not accepted on new capture)
- Validate repeatable group instance counts against composition rules
- No persistence: in-memory payload only

## Out of Scope

- Renderer UX validation (client-side)
- External dataset value checks
- Policy/governance rules, cross-BC document existence
- JSON Schema runtime validator in infrastructure (optional future; domain rules suffice here)
- HTTP APIs, EF, UI
- Full JSONB storage pipeline (Wave 3+)

## Allowed Bounded Context

| Project | Allowed |
|---------|---------|
| `AIX.Metadata` (Domain, optional Application folder if pure domain service does not fit) | Yes |
| `AIX.Metadata.Tests` | Yes |
| `AIX.Metadata.Contracts` | Read-only types for payload if needed |
| Other BCs | No |

## Dependencies

- **slice-009** — `KeywordValidator`, registry validation
- **slice-012** — group required/repeatable semantics on version
- **slice-014** — contract/schema identity clarity (tests may use contract keys)

## Acceptance Criteria

- Valid payload for a version passes with `Result` success
- Missing required standalone field fails
- Missing required group instance fails
- Invalid keyword value fails with appropriate error (type, length, required)
- Unknown field key fails
- Hidden/deprecated field rejected on capture validation path
- Multiple errors surfaced in one validation call where designed (document API shape in tests)
- No validation logic duplicated outside `KeywordValidator` for value rules

## Testing Expectations

- Minimum scenarios:
  - validates_complete_metadata_payload_successfully
  - fails_missing_required_field_from_schema
  - fails_missing_required_keyword_group_instance
  - fails_invalid_keyword_value_delegates_to_keyword_validator
  - fails_unknown_metadata_key
  - rejects_hidden_field_on_capture
  - returns_multiple_validation_errors_when_applicable
  - repeatable_group_enforces_instance_rules_on_payload
- Full regression: all Wave 1 + Wave 2 Metadata tests

## Definition of Done

Per `docs/architecture/slice-definition-of-done.md`; Wave 2 domain slices 010–015 complete after this slice.

## Post-Slice Memory / Backlog Sync

1. `index.md` — slice 015 **Done**; Wave 2 slices all **Pending → Done** as executed
2. `memory.md` — Wave 2 complete summary; validation integration notes; test totals
3. `current-state.md` — Wave 2 progress table; suggest Wave 2 Domain Review
4. `current-task.md` — point to Wave 2 domain review prep or first Wave 3 slice when defined
5. Schedule **Wave 2 Domain Review** doc in `docs/reviews/` (human/agent review task, not in this backlog slice)

## Patterns Used

- Policy / specification composition
- Delegation to existing `KeywordValidator`

## Validation Commands

```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```
