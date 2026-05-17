# ADR-003: One Database Per Tenant

## Status
Accepted

## Context

AIX is designed as a governance-first SaaS platform for SMEs with strong document isolation requirements.

Different tenants may require:
- independent storage providers
- different retention policies
- sovereign infrastructure
- isolated backups
- hybrid cloud/on-premise deployment

## Decision

AIX uses one database per tenant.

Tenant resolution belongs to the platform/infrastructure layer, not the domain layer.

Tenant domain entities should not require tenant_id fields.

## Consequences

### Positive
- strong tenant isolation
- easier backup/restore
- reduced cross-tenant leakage risk
- better sovereignty support

### Negative
- more operational complexity
- more databases to manage
- higher infrastructure orchestration requirements

## Alternatives Considered

### Shared Database Multi-Tenant
Rejected because it complicates governance and isolation requirements.
