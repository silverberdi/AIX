## ADDED Requirements

### Requirement: Completion readiness evaluates capture correctness

The system SHALL evaluate whether a Draft document is ready to transition to Complete based on capture rules before allowing completion. At minimum, readiness SHALL require valid captured metadata against the bound version and satisfaction of file attachment rules defined for MVP capture.

#### Scenario: Ready when metadata valid and primary file attached

- **WHEN** a Draft document has validated captured metadata and a primary file attached
- **THEN** completion readiness SHALL indicate the document can complete

#### Scenario: Not ready when metadata missing

- **WHEN** a Draft document has no captured metadata
- **THEN** completion readiness SHALL indicate the document cannot complete

#### Scenario: Not ready when metadata invalid

- **WHEN** a Draft document has metadata that fails version-scoped validation
- **THEN** completion readiness SHALL indicate the document cannot complete

#### Scenario: Not ready when primary file missing

- **WHEN** a Draft document has valid metadata but no primary file
- **THEN** completion readiness SHALL indicate the document cannot complete

### Requirement: Complete transition enforces readiness

The Document aggregate `Complete` operation SHALL enforce completion readiness. Completing a document that is not capture-ready SHALL fail with a deterministic domain error and SHALL NOT change document state.

#### Scenario: Complete succeeds when capture-ready

- **WHEN** a capture-ready Draft document completes
- **THEN** the document state SHALL become Complete and a `DocumentCompleted` event SHALL be emitted

#### Scenario: Complete fails when not capture-ready

- **WHEN** a Draft document that is not capture-ready attempts to complete
- **THEN** the operation SHALL fail, state SHALL remain Draft, and no `DocumentCompleted` event SHALL be emitted

### Requirement: Complete documents remain immutable

After a successful Complete transition, the document SHALL reject metadata mutations and file attachment mutations per existing Wave 0 immutability rules.

#### Scenario: Complete document rejects further capture changes

- **WHEN** a Complete document receives a metadata update or file attachment
- **THEN** the operation SHALL fail per existing immutability rules

### Requirement: Completion rules are evaluable without persistence

Completion readiness evaluation SHALL operate on in-memory aggregate state and injected validation collaborators without requiring databases, repositories, or HTTP APIs.

#### Scenario: Readiness evaluated in unit tests

- **WHEN** completion readiness is tested
- **THEN** tests SHALL construct document, metadata, and file state in memory and assert readiness outcomes

#### Scenario: Not ready when only supporting files are attached

- **WHEN** a Draft document has valid metadata and only supporting files attached
- **THEN** completion readiness SHALL indicate the document cannot complete
