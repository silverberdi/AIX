## ADDED Requirements

### Requirement: Document holds captured metadata in Draft

A Document in Draft state SHALL hold the captured metadata payload associated with its bound `DocumentTypeVersionId`. The payload SHALL be replaceable while the document remains in Draft.

#### Scenario: Attach metadata to draft document

- **WHEN** a Draft document receives a captured metadata payload via `SetCapturedMetadata`
- **THEN** the document SHALL expose the current payload through `CapturedMetadata`

#### Scenario: Replace metadata on draft document

- **WHEN** a Draft document already has captured metadata and receives a new captured payload via `SetCapturedMetadata`
- **THEN** the document SHALL replace the prior payload with the new payload

#### Scenario: Document starts without metadata

- **WHEN** a document is created via `Create`
- **THEN** `CapturedMetadata` SHALL be null until explicitly set

### Requirement: Metadata is immutable after Complete

A Document in Complete state SHALL NOT accept metadata changes. Attempts to modify metadata on a Complete document SHALL fail with a deterministic domain error.

#### Scenario: Reject metadata update on complete document

- **WHEN** a Complete document receives a metadata update via `SetCapturedMetadata`
- **THEN** the operation SHALL fail with `DocumentErrors.CannotModifyWhenComplete` and the existing captured metadata SHALL remain unchanged

### Requirement: Metadata changes emit domain events

When captured metadata is set or replaced on a Draft document, the system SHALL emit a structured domain event suitable for audit and downstream projections.

#### Scenario: Metadata update emits event

- **WHEN** captured metadata is successfully set or replaced on a Draft document
- **THEN** a `DocumentMetadataCaptured` domain event SHALL be recorded on the aggregate's domain event list

### Requirement: Metadata remains bound to document type version

Captured metadata on a document SHALL NOT change the document's bound `DocumentTypeId` or `DocumentTypeVersionId`. Version binding is immutable from document creation.

#### Scenario: Metadata update does not alter version binding

- **WHEN** metadata is updated on a Draft document via `SetCapturedMetadata`
- **THEN** the document's `DocumentTypeId` and `DocumentTypeVersionId` SHALL remain unchanged

### Requirement: Captured metadata uses Documents-owned value types

The aggregate SHALL store captured metadata as Documents-owned value types structurally aligned with `CapturedMetadataPayload` from `AIX.Documents.Contracts`, not Metadata domain types.

#### Scenario: Domain value object mirrors contract shape

- **WHEN** `SetCapturedMetadata` accepts a `CapturedMetadataPayload`
- **THEN** the aggregate SHALL persist the data as `DocumentCapturedMetadata` with standalone keyword values and group instances matching the contract structure

### Requirement: No schema validation in attachment slice

Metadata attachment in slice 017 SHALL NOT validate payload contents against the bound document type version schema.

#### Scenario: Unvalidated payload is accepted on Draft

- **WHEN** a Draft document receives a `CapturedMetadataPayload` that has not been validated against Metadata schema
- **THEN** `SetCapturedMetadata` SHALL succeed and store the payload without invoking a validation port

### Requirement: Existing document behaviors unchanged

Slice 017 SHALL NOT alter `Create`, `AttachFile`, or `Complete` behavior beyond adding captured metadata state and `SetCapturedMetadata`.

#### Scenario: Complete document still rejects file attachment

- **WHEN** a Complete document attempts to attach a file after metadata attachment is implemented
- **THEN** `AttachFile` SHALL still fail with `DocumentErrors.CannotModifyWhenComplete`
