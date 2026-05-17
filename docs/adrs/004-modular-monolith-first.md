# ADR-004: Modular Monolith First

## Status
Accepted

## Context

AIX is expected to evolve into a large governance and document processing platform.

Starting directly with microservices would introduce excessive complexity during the MVP stage.

## Decision

AIX starts as a modular monolith.

Modules communicate through contracts and internal events.

The architecture must allow future extraction into microservices.

## Consequences

### Positive
- simpler development
- faster iteration
- lower infrastructure complexity
- easier transactional consistency

### Negative
- requires strong modular discipline
- future extraction still requires planning

## Architectural Rules

- modules own their domain
- modules communicate through contracts/events
- avoid direct cross-module persistence access

## Alternatives Considered

### Full Microservices
Rejected because complexity is unjustified during MVP.
