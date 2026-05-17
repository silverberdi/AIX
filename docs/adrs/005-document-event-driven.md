# ADR-005: Internal Document Event-Driven Architecture

## Status
Accepted

## Context

AIX revolves around governed document operations.

Document lifecycle actions naturally produce events such as:
- document uploaded
- document indexed
- document approved
- document locked
- document destroyed

These events support:
- automation
- OCR
- indexing
- notifications
- integrations
- future workflows

## Decision

AIX adopts an internal document-event-driven architecture.

The MVP uses an internal event bus inside the modular monolith.

## Consequences

### Positive
- loose coupling
- scalable async processing
- future microservice readiness
- cleaner automation model

### Negative
- eventual consistency management
- async debugging complexity

## Rules

- important lifecycle actions emit events
- events are machine-readable
- handlers should be idempotent when possible

## Alternatives Considered

### CRUD-Centric Architecture
Rejected because it limits automation and scalability.
