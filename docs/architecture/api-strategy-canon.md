# API Strategy Canon

## Principles

- contract-first APIs
- REST + OpenAPI
- separated internal/external APIs
- document-centric APIs
- governance-aware APIs

## Internal vs External APIs

| Surface | Consumers | Rules |
|---------|-----------|-------|
| **Internal APIs** | Angular SPA, first-party services | May evolve with the monorepo; still contract-first via OpenAPI |
| **External APIs** | Customers, integrators, third parties | Versioned from day one; scope-based; stable published contracts |

### External API rules

- External APIs are **versioned from day one** (e.g. `/v1/...`).
- External APIs are **scope-based** (integrator permissions, not full admin surface).
- External APIs must **not** expose internal domain entities or persistence shapes.
- External APIs must **not** expose raw OpenSearch/Elastic DSL or internal query languages.
- Published external contracts live under `artifacts/openapi/`.
- JSON Schemas for renderer and structural contracts live under `artifacts/schemas/`.

### Internal API rules

- Used by `apps/aix-ui` and first-party backend workers.
- OpenAPI export still required for client generation and review.
- Must not leak secrets, connection strings, or tenant runtime configuration in responses.

## Async integrations

- webhooks
- async jobs

## MVP uploads

- backend upload

## Future

- signed direct-to-storage uploads
