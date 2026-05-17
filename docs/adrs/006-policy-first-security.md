# ADR-006: Policy-First Security Model

## Status
Accepted

## Context

Governance-oriented document systems require more than traditional RBAC.

Permissions may depend on:
- role
- user restrictions
- taxonomy
- document type
- metadata
- document state
- operational context

## Decision

AIX adopts a policy-first security model.

RBAC is foundational but not sufficient.

Roles grant capabilities while policies and restrictions condition or reduce effective access.

Authorization is enforced in backend.

## Consequences

### Positive
- flexible governance-aware security
- metadata-aware permissions
- contextual authorization support

### Negative
- more complex authorization evaluation
- policy debugging complexity

## Security Rules

- deny overrides allow
- frontend adapts UX only
- backend enforces authorization

## MVP Scope

- roles
- permissions
- taxonomy permissions
- DocumentType permissions
- user allow/deny overrides

## Alternatives Considered

### Pure RBAC
Rejected because it is too rigid for governance scenarios.
