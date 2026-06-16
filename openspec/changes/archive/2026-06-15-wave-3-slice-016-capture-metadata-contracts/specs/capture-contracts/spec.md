## ADDED Requirements

### Requirement: Capture metadata payload contract

The system SHALL expose a bounded-context-neutral contract type for version-scoped captured metadata that mirrors the structural shape of `VersionMetadataPayload` (standalone keyword values keyed by keyword code, and group instances with group code, optional instance key, and keyword values) without requiring `AIX.Documents` to reference `AIX.Metadata.Domain`.

#### Scenario: Contract represents standalone field values

- **WHEN** a capture payload contains standalone keyword values
- **THEN** `CapturedMetadataPayload` SHALL carry keyword-code-keyed string values via a read-only `StandaloneValues` property

#### Scenario: Contract represents repeatable group instances

- **WHEN** a capture payload contains group instances
- **THEN** `CapturedMetadataPayload` SHALL carry them via a read-only `GroupInstances` property where each `CapturedMetadataGroupInstance` includes group code, optional instance key, and keyword-code-keyed values

#### Scenario: Payload is immutable after construction

- **WHEN** a caller constructs `CapturedMetadataPayload` with standalone values or group instances
- **THEN** subsequent mutation of the caller's dictionary or list inputs SHALL NOT change the contract instance's exposed values

### Requirement: Capture validation result contract

The system SHALL expose a contract type for version-scoped metadata validation outcomes that supports success, deterministic multiple errors, and no exception-based control flow for expected validation failures.

#### Scenario: Validation success

- **WHEN** metadata satisfies the bound document type version schema
- **THEN** `CaptureValidationResult.Success()` SHALL indicate success with `IsValid` true and an empty errors collection

#### Scenario: Validation failure with multiple errors

- **WHEN** metadata violates multiple schema rules
- **THEN** `CaptureValidationResult.Failure(...)` SHALL indicate failure with `IsValid` false and SHALL carry all supplied errors in caller order without loss

#### Scenario: Success result has no errors

- **WHEN** a successful validation result is created
- **THEN** the result's errors collection SHALL be empty

### Requirement: Capture validation error contract

The system SHALL expose a contract type for individual validation errors that can be collected within `CaptureValidationResult`.

#### Scenario: Error carries deterministic details

- **WHEN** a validation failure occurs
- **THEN** each `CaptureValidationError` instance SHALL carry a stable `Code` and human-readable `Message` suitable for cross-boundary transfer

#### Scenario: Multiple errors are distinct instances

- **WHEN** a failure result is created with multiple errors
- **THEN** each error SHALL be represented as its own `CaptureValidationError` instance in the result's errors collection

### Requirement: Contracts are passive data types only

Capture contract types in slice 016 SHALL be passive DTOs and result types only. Behavioral ports and validator interfaces SHALL NOT be defined in the contracts slice.

#### Scenario: No validator port in contracts slice

- **WHEN** slice 016 is implemented
- **THEN** `AIX.Documents.Contracts` SHALL contain payload and validation result types but SHALL NOT define `ICaptureMetadataValidator` or other behavioral ports

#### Scenario: No validator interfaces exported

- **WHEN** the contracts assembly is inspected
- **THEN** no public interface whose name contains `Validator` SHALL be exported from `AIX.Documents.Contracts`

### Requirement: Contracts remain dependency-minimal

Capture contract types SHALL live in `AIX.Documents.Contracts`, which SHALL NOT reference `AIX.SharedKernel`, `AIX.Metadata`, `AIX.Metadata.Contracts`, or `AIX.Documents`.

#### Scenario: Contracts project has no forbidden references

- **WHEN** `AIX.Documents.Contracts` is built
- **THEN** its assembly references SHALL NOT include `AIX.Documents`, `AIX.Metadata`, `AIX.Metadata.Contracts`, or `AIX.SharedKernel`

#### Scenario: Documents domain consumes contracts only

- **WHEN** future slices attach captured metadata on the Document aggregate
- **THEN** they SHALL use contract or Documents-owned value types derived from these capture contracts, not Metadata domain types

### Requirement: Contracts build and test in solution

The contracts project and its tests SHALL be part of the backend solution and pass full regression.

#### Scenario: Solution builds and tests pass

- **WHEN** `dotnet restore`, `dotnet build`, and `dotnet test` are run against `backend/AIX.sln`
- **THEN** `AIX.Documents.Contracts` and `AIX.Documents.Contracts.Tests` SHALL build successfully and all tests SHALL pass without modifying `AIX.Documents` or `AIX.Metadata` behavior
