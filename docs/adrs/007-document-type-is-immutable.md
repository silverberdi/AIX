# ADR-007: DocumentType is Immutable for Documents

## Status
Accepted

## Decision
A document cannot change its DocumentType after creation.

## Consequences
DocumentType is part of document identity. If classification is uncertain, an explicit temporary DocumentType must be used.
