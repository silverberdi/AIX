## 1. Project scaffolding

- [x] 1.1 Create `backend/src/AIX.Documents.Contracts/AIX.Documents.Contracts.csproj` (`net9.0`, no project references)
- [x] 1.2 Add `AIX.Documents.Contracts` to `backend/AIX.sln`
- [x] 1.3 Create `backend/tests/AIX.Documents.Contracts.Tests/AIX.Documents.Contracts.Tests.csproj` referencing contracts project and xUnit/FluentAssertions
- [x] 1.4 Add `AIX.Documents.Contracts.Tests` to `backend/AIX.sln`

## 2. Capture payload contracts

- [x] 2.1 Add `CapturedMetadataGroupInstance` sealed record with `GroupCode`, `InstanceKey`, and keyword-code-keyed `Values`
- [x] 2.2 Add `CapturedMetadataPayload` sealed class with `StandaloneValues` and `GroupInstances` get-only properties and constructor defaults to empty collections
- [x] 2.3 Defensively copy incoming dictionaries/lists in `CapturedMetadataPayload` constructor so contract instances are immutable after construction

## 3. Validation result contracts

- [x] 3.1 Add `CaptureValidationError` sealed record with `Code` and `Message`
- [x] 3.2 Add `CaptureValidationResult` sealed class with private constructor, `IsValid`, `Errors`, and factory methods `Success()` and `Failure(IReadOnlyList<CaptureValidationError> errors)`
- [x] 3.3 Store a defensive copy of errors in `Failure(...)` so the result is immutable after construction

## 4. Contract tests

- [x] 4.1 `constructs_captured_metadata_payload_with_standalone_values`
- [x] 4.2 `constructs_captured_metadata_payload_with_group_instances`
- [x] 4.3 `captured_metadata_payload_is_immutable_after_construction` (mutating caller input does not change exposed values)
- [x] 4.4 `capture_validation_result_success_has_no_errors`
- [x] 4.5 `capture_validation_result_failure_carries_multiple_errors`
- [x] 4.6 `capture_validation_error_carries_deterministic_details`
- [x] 4.7 `contracts_project_has_no_forbidden_references` (no `AIX.Documents`, `AIX.Metadata`, `AIX.Metadata.Contracts`, or `AIX.SharedKernel`)
- [x] 4.8 `contracts_project_defines_no_behavioral_ports` (no `ICaptureMetadataValidator` or public `*Validator*` interfaces)

## 5. Verification and handoff

- [x] 5.1 Run `cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln` — all tests pass; no changes to `AIX.Documents` or `AIX.Metadata`
- [x] 5.2 Update `ai/backlog/mvp/index.md`, `ai/context/memory.md`, and `ai/context/current-state.md` for slice-016 completion
- [x] 5.3 Point `ai/tasks/current-task.md` at slice-017
