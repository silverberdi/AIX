# Multi-Tenancy

## Strategy

AIX uses **one database per tenant** for business data (see [ADR-003](../adrs/003-one-database-per-tenant.md)).

Each tenant has an independent database. Platform metadata lives in a separate **Platform DB**.

## Runtime resolution

Tenant runtime is resolved **server-side** in the Business API. Clients do not send connection strings, storage paths, or provider credentials.

```
Frontend
→ Platform Auth (AIX.Platform.Api)
→ JWT with tenant_id, user_id, session_id, minimal claims
→ Business API (AIX.Business.Api)
→ TenantRuntimeResolver
→ Platform Runtime Registry / secure cache
→ Secret Store
→ dynamic DB + Storage resolution
```

### Rules

- **Authentication** belongs to Platform.
- **Authorization** belongs to Business.
- Tokens transport **identity and context** (`tenant_id`, `user_id`, `session_id`), not infrastructure.
- Tokens **never** contain secrets, connection strings, storage credentials, or provider internals.
- Business resolves tenant runtime using `tenant_id` from the validated JWT.

## Platform DB vs Tenant DB

### Platform DB owns

- users
- tenant registry
- authentication identity
- subscriptions / payment state
- provisioning state
- feature flags
- routing keys
- secret references (not secret values in the DB when avoidable)
- platform audit

Platform DB does **not** contain documents, document metadata, workflows, document events, or other tenant business records.

### Tenant DB owns

- documents
- document types
- metadata
- policies
- roles / authorization data
- workflows
- audit / business events
- search projections (where applicable)
- reference data

### Rule

No `tenant_id` column is required in tenant-domain tables because each tenant has its own database.

## Principles

- No tenant resolution logic in the domain layer
- Physical isolation per tenant database
- Shared platform infrastructure for registry and secrets
- Tenant-aware cache keys where caching is used

## Implications

- Separate migrations per tenant database
- Platform DB migrations independent of tenant DBs
- Strong isolation guarantees at the database boundary
