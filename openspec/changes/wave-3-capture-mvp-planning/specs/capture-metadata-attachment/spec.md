## ADDED Requirements

### Requirement: Document holds captured metadata in Draft

A Document in Draft state SHALL hold the captured metadata payload associated with its bound `DocumentTypeVersionId`. The payload SHALL be replaceable while the document remains in Draft.

#### Scenario: Attach metadata to draft document

- **WHEN** a Draft document receives a captured metadata payload accepted by the current slice rules
- **THEN** the document SHALL expose the current payload through the aggregate

#### Scenario: Replace metadata on draft document

- **WHEN** a Draft document already has metadata and receives a new captured payload
- **THEN** the document SHALL replace the prior payload with the new payload

#### Scenario: Document starts without metadata

- **WHEN** a document is created
- **THEN** it MAY have no captured metadata until explicitly set

### Requirement: Metadata is immutable after Complete

A Document in Complete state SHALL NOT accept metadata changes. Attempts to modify metadata on a Complete document SHALL fail with a deterministic domain error.

#### Scenario: Reject metadata update on complete document

- **WHEN** a Complete document receives a metadata update
- **THEN** the operation SHALL fail and the existing metadata SHALL remain unchanged

### Requirement: Metadata changes emit domain events

When captured metadata is set or replaced on a Draft document, the system SHALL emit a structured domain event suitable for audit and downstream projections.

#### Scenario: Metadata update emits event

- **WHEN** captured metadata is successfully set or replaced on a Draft document
- **THEN** a domain event SHALL be recorded on the aggregate's domain event list

### Requirement: Metadata remains bound to document type version

Captured metadata on a document SHALL NOT change the document's bound `DocumentTypeId` or `DocumentTypeVersionId`. Version binding is immutable from document creation.

#### Scenario: Metadata update does not alter version binding

- **WHEN** metadata is updated on a Draft document
- **THEN** the document's `DocumentTypeId` and `DocumentTypeVersionId` SHALL remain unchanged
