## ADDED Requirements

### Requirement: Capture metadata is validated against bound version

Before a Draft document accepts a captured metadata payload, the system SHALL validate the payload against the document's bound `DocumentTypeVersionId` using the centralized version-scoped schema validation rules established in Wave 2.

#### Scenario: Valid payload is accepted

- **WHEN** a Draft document receives a metadata payload that passes version-scoped validation
- **THEN** the metadata SHALL be stored on the document

#### Scenario: Invalid payload is rejected

- **WHEN** a Draft document receives a metadata payload that fails version-scoped validation
- **THEN** the metadata SHALL NOT be stored and the operation SHALL return validation errors

#### Scenario: Validation rejects unknown keys

- **WHEN** a payload contains keys not defined on the bound version composition
- **THEN** validation SHALL fail with a deterministic error

#### Scenario: Validation rejects hidden or deprecated fields on capture

- **WHEN** a payload provides values for hidden or deprecated fields on the capture path
- **THEN** validation SHALL fail with a deterministic error

### Requirement: Validation ownership remains centralized

Keyword value rules SHALL be enforced through the existing `KeywordValidator` delegation path. Documents SHALL NOT reimplement keyword parsing or value rule logic.

#### Scenario: Value rule failures delegate to keyword validator

- **WHEN** a payload contains an invalid keyword value for a defined field
- **THEN** validation SHALL fail using the same value rules as `VersionSchemaValidator`

### Requirement: Validation port enables cross-bounded-context integration

Documents SHALL integrate validation through an explicit behavioral port (`ICaptureMetadataValidator`) introduced in slice 018, injected as a method parameter on aggregate operations — not through a direct project reference from `AIX.Documents` to `AIX.Metadata.Domain`. The port SHALL NOT be defined in `AIX.Documents.Contracts`.

#### Scenario: Validation port placement decided during slice 018

- **WHEN** slice 018 is implemented
- **THEN** `ICaptureMetadataValidator` SHALL be defined in `AIX.Documents.Application` (preferred if Application layer is used for ports) or `AIX.Documents.Domain` (only if the aggregate method requires a domain-level abstraction), accepting document type version identity and `CapturedMetadataPayload`, returning `CaptureValidationResult`

#### Scenario: Contracts project remains passive DTOs only

- **WHEN** slice 018 is implemented
- **THEN** `AIX.Documents.Contracts` SHALL contain only passive DTO/result types and SHALL NOT define `ICaptureMetadataValidator`

#### Scenario: Documents tests validate through adapter

- **WHEN** Documents behavior tests exercise metadata capture validation
- **THEN** they SHALL use a test adapter that implements the validation port and delegates to Metadata validation
