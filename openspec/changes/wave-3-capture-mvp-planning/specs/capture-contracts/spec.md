## ADDED Requirements

### Requirement: Capture metadata payload contract

The system SHALL expose a bounded-context-neutral contract type for version-scoped captured metadata that mirrors the structural shape of `VersionMetadataPayload` (standalone keyword values keyed by keyword code, and group instances with group code, optional instance key, and keyword values) without requiring `AIX.Documents` to reference `AIX.Metadata.Domain`.

#### Scenario: Contract represents standalone field values

- **WHEN** a capture payload contains standalone keyword values
- **THEN** the contract type SHALL carry keyword-code-keyed string values suitable for cross-boundary transfer

#### Scenario: Contract represents repeatable group instances

- **WHEN** a capture payload contains group instances
- **THEN** the contract type SHALL carry group code, optional instance key, and keyword-code-keyed values per instance

### Requirement: Capture validation result contract

The system SHALL expose a contract type for version-scoped metadata validation outcomes that supports success, deterministic multiple errors, and no exception-based control flow for expected validation failures.

#### Scenario: Validation success

- **WHEN** metadata satisfies the bound document type version schema
- **THEN** the validation result contract SHALL indicate success with no errors

#### Scenario: Validation failure with multiple errors

- **WHEN** metadata violates multiple schema rules
- **THEN** the validation result contract SHALL carry all applicable errors in a deterministic order

### Requirement: Capture validation error contract

The system SHALL expose a contract type for individual validation errors that can be collected within `CaptureValidationResult`.

#### Scenario: Error carries deterministic details

- **WHEN** a validation failure occurs
- **THEN** each error contract instance SHALL carry sufficient detail to identify the failing rule or field in a deterministic structure

### Requirement: Contracts are passive data types only

Capture contract types in slice 016 SHALL be passive DTOs and result types only. Behavioral ports and validator interfaces SHALL NOT be defined in the contracts slice.

#### Scenario: No validator port in contracts slice

- **WHEN** slice 016 is implemented
- **THEN** `AIX.Documents.Contracts` SHALL contain payload and validation result types but SHALL NOT define `ICaptureMetadataValidator` or other behavioral ports

### Requirement: Contracts remain dependency-minimal

Capture contract types SHALL live in a contracts project that SHALL NOT reference `AIX.SharedKernel`, `AIX.Metadata.Domain`, or `AIX.Documents.Domain` unless an explicit documented contract convention requires it.

#### Scenario: Documents domain consumes contracts only

- **WHEN** the Document aggregate accepts captured metadata
- **THEN** it SHALL use contract or Documents-owned value types derived from the capture contracts, not Metadata domain types
