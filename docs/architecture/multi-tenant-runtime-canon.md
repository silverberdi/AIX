# Multi-tenant Runtime Canon

## Platform Runtime (`AIX.Platform.Api`)

- authentication
- tenant registry
- provisioning
- feature enablement
- user identity
- routing / secret references

## Business Runtime (`AIX.Business.Api`)

- authorization
- document operations
- governance
- workflows
- search

## Separation

- Authentication belongs to **Platform**.
- Authorization belongs to **Business**.
- Tokens transport identity and context, not infrastructure.
- Tokens **never** contain secrets, connection strings, storage credentials, or provider internals.

## Canonical resolution flow

```
Frontend
→ Platform Auth
→ JWT containing tenant_id, user_id, session_id, minimal claims
→ Business API
→ TenantRuntimeResolver
→ Platform Runtime Registry / secure cache
→ Secret Store
→ dynamic DB + Storage resolution
```

Business resolves tenant runtime **server-side** using `tenant_id` from the validated JWT. Clients must not send DB or storage paths in requests.

## Data boundaries

**Platform DB** holds users, tenant registry, auth identity, subscriptions, provisioning, feature flags, routing keys, secret references, and platform audit. It does **not** hold documents, document metadata, workflows, document events, or tenant business records.

**Tenant DB** (one per tenant) holds documents, types, metadata, policies, roles, workflows, business events, search projections, and reference data. Tenant-domain tables do not require `tenant_id` because each database is tenant-scoped.
