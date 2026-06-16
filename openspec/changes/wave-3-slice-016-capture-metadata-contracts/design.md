## Context

Wave 2 delivered version-scoped metadata validation in `AIX.Metadata` (`VersionMetadataPayload`, `MetadataGroupInstancePayload`, `SchemaValidationResult` using `AIX.SharedKernel.Primitives.Error`). `AIX.Metadata.Contracts` already exposes renderer/schema DTOs with no SharedKernel dependency. `AIX.Documents` exists but has no captured-metadata model yet.

Slice 016 is the first Wave 3 implementation change. It creates the passive contract layer so later slices can attach metadata on `Document` (017), define `ICaptureMetadataValidator` (018), and enforce capture readiness (019–020) without Documents referencing Metadata domain types.

**Reference shapes (Metadata domain — not referenced by contracts):**

```csharp
// AIX.Metadata.Domain — structural mirror only
VersionMetadataPayload(IReadOnlyDictionary<string, string?> StandaloneValues,
                       IReadOnlyList<MetadataGroupInstancePayload> GroupInstances)
MetadataGroupInstancePayload(string GroupCode, string? InstanceKey,
                               IReadOnlyDictionary<string, string?> Values)
SchemaValidationResult(bool IsValid, IReadOnlyList<Error> Errors)
```

## Goals / Non-Goals

**Goals:**

- Introduce `AIX.Documents.Contracts` as a dependency-minimal contracts assembly.
- Mirror capture payload structure from `VersionMetadataPayload` using Documents-owned type names.
- Provide validation result/error contracts suitable for cross-BC transfer and slice-018 adapter mapping.
- Enforce immutability via init-only or record types with defensive copies of mutable inputs.
- Verify via tests that the contracts project has no forbidden references and defines no behavioral ports.

**Non-Goals:**

- `ICaptureMetadataValidator` or any behavioral port (slice 018).
- Mapping from Metadata `SchemaValidationResult` to `CaptureValidationResult` (slice 018).
- `DocumentCapturedMetadata` value object or aggregate changes (slice 017).
- `DocumentCaptureContext` DTO (slice 021).
- JSON serialization attributes or API wire format.
- Persistence, EF Core, repositories, HTTP, MediatR, Angular/PrimeNG/Avalon.

## Decisions

### 1. Project layout and solution membership

| Path | Purpose |
|------|---------|
| `backend/src/AIX.Documents.Contracts/AIX.Documents.Contracts.csproj` | Contracts assembly |
| `backend/tests/AIX.Documents.Contracts.Tests/AIX.Documents.Contracts.Tests.csproj` | Contract shape and boundary tests |

Both projects are added to `backend/AIX.sln`, following the same `net9.0` SDK-style pattern as `AIX.Metadata.Contracts`.

**Rationale:** Dedicated test project keeps forbidden-reference checks isolated from `AIX.Documents.Tests`, which will grow with domain behavior in slices 017–020.

### 2. Zero project references on contracts

`AIX.Documents.Contracts.csproj` SHALL have no `<ProjectReference>` entries.

**Rationale:** Planning spec and slice backlog explicitly forbid references to `AIX.Documents`, `AIX.Metadata`, `AIX.Metadata.Contracts`, and `AIX.SharedKernel`. `AIX.Metadata.Contracts` follows the same zero-reference pattern.

**Alternative considered:** Reference `AIX.SharedKernel` for `Error` reuse. Rejected — contracts must stay dependency-minimal; slice 018 adapter can map `Error` → `CaptureValidationError`.

### 3. Type shapes and naming

| Contract type | Shape | Notes |
|---------------|-------|-------|
| `CapturedMetadataPayload` | `sealed class` with get-only properties | Constructor accepts optional `standaloneValues` and `groupInstances`; defaults to empty dictionary/list |
| `CapturedMetadataGroupInstance` | `sealed record` | `(string GroupCode, string? InstanceKey, IReadOnlyDictionary<string, string?> Values)` — mirrors `MetadataGroupInstancePayload` |
| `CaptureValidationError` | `sealed record` | `(string Code, string Message)` — structurally aligned with SharedKernel `Error` without referencing it |
| `CaptureValidationResult` | `sealed class` with private constructor | Factory methods `Success()` and `Failure(IReadOnlyList<CaptureValidationError> errors)`; `IsValid` + `Errors` get-only |

Namespace: `AIX.Documents.Contracts` (flat, matching `AIX.Metadata.Contracts`).

**Rationale:** Class + factory for validation result matches Metadata's `SchemaValidationResult` pattern and prevents invalid states (success with errors). Records for group instance and error provide value semantics and immutability.

### 4. Immutability strategy

- All contract properties are get-only (no public setters).
- `CapturedMetadataPayload` constructor copies incoming dictionaries into new `Dictionary<string, string?>` instances (or wraps with `ReadOnlyDictionary`) so callers cannot mutate after construction.
- `CaptureValidationResult.Failure` stores a defensive copy of the errors list (or wraps as read-only).
- `CapturedMetadataGroupInstance.Values` is assigned from a copied read-only dictionary in the record constructor if needed.

**Rationale:** Cross-BC DTOs must not allow post-construction mutation; defensive copy matches domain payload behavior in `VersionMetadataPayload`.

### 5. Error ordering contract

`CaptureValidationResult.Failure` SHALL preserve caller-supplied error order. Slice 018 adapter is responsible for mapping Metadata's deterministic ordering (`OrderErrorsDeterministically` in `VersionSchemaValidator`) when producing the contract result.

**Rationale:** Ordering logic belongs in the validation adapter (018), not in passive contracts (016).

### 6. Forbidden-reference and no-port tests

| Test | Approach |
|------|----------|
| `contracts_project_has_no_forbidden_references` | `Assembly.GetReferencedAssemblies()` on contracts assembly; assert none named `AIX.Documents`, `AIX.Metadata`, `AIX.Metadata.Contracts`, or `AIX.SharedKernel` |
| `contracts_project_defines_no_behavioral_ports` | Reflect over exported types; assert no type named `ICaptureMetadataValidator` and no public interface whose name contains `Validator` |

**Rationale:** Structural tests enforce slice boundary without requiring custom analyzers.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Duplicate error shape vs SharedKernel `Error` | Document mapping in slice 018; keep contracts dependency-free |
| Structural drift from `VersionMetadataPayload` | Mirror property names and types exactly; add shape tests comparing expected keys |
| Developers add SharedKernel reference for convenience | Forbidden-reference test fails CI |
| Validator port accidentally added to Contracts | Explicit no-port reflection test |

## Migration Plan

Not applicable — greenfield projects. After merge:

1. Run full backend build/test.
2. Update `ai/backlog/mvp/index.md`, `ai/context/memory.md`, `ai/context/current-state.md`.
3. Point `ai/tasks/current-task.md` at slice-017.

## Open Questions

- _(none — planning artifacts and slice backlog resolve all slice-016 decisions)_
