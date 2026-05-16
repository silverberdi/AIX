# Multi-Tenancy

## Strategy

Axioma uses schema-per-tenant isolation.

Each tenant has an independent database schema.

## Resolution

Tenant resolution is performed through:

- x-tenant-id header
- infrastructure middleware

## Principles

- No tenant logic in domain layer
- Physical isolation
- Shared infrastructure
- Schema-aware caching

## Implications

- Separate migrations per schema
- Tenant-aware cache keys
- Strong isolation guarantees
