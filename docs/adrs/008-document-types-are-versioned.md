# ADR-008: DocumentTypes are Versioned

## Status
Accepted

## Decision
DocumentTypes are versioned and each document references the exact DocumentTypeVersion used when it was created.

## Consequences
DocumentType changes create new versions. Existing documents remain bound to their original version. Fields are not deleted; they may be deprecated.
