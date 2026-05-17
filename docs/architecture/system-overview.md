# System Overview

## Vision

AIX (Axioma) is an Intelligent Document Processing (IDP) platform where the document is the central business entity.

The platform supports:

- Dynamic document composition
- Declarative form rendering
- Hybrid electronic forms and scanned documents
- Multi-tenant architecture (one database per tenant)
- Storage abstraction
- Intelligent search
- Automation and integrations

OCR and AI are optional future capability layers. AIX v1.0 must be **AI-ready but not AI-dependent** (see [MVP scope](./mvp-scope-v1.md)).

## Repository layout

| Path | Role |
|------|------|
| `apps/aix-ui` | Angular SPA (Nx) |
| `libs/shared-ui`, `libs/shared-core` | Shared frontend libraries |
| `backend/` | .NET 9 modular monolith (APIs, bounded contexts, SharedKernel, Infrastructure) |
| `docs/` | Architecture, domain, standards, ADRs |
| `ai/` | Agent context and instructions |

## Runtime topology

```
┌─────────────┐     HTTPS/REST      ┌──────────────────┐
│  aix-ui     │ ──────────────────► │ AIX.Business.Api │
│  (Angular)  │   JWT (tenant_id,   └────────┬─────────┘
└─────────────┘    user_id, …)              │
       │                                    ▼
       │                             Bounded contexts
       │                             (Documents, Security, …)
       │                                    │
       └────────── Platform Auth ──► ┌──────────────────┐
           (login, registry)         │ AIX.Platform.Api │
                                      └────────┬─────────┘
                                               ▼
                                      AIX.Platform BC,
                                      Platform DB, registry,
                                      secret references
                                               │
                                      Business runtime resolves
                                      tenant DB + storage via
                                      TenantRuntimeResolver
```

## Tenant runtime (summary)

```
Frontend → Platform Auth → JWT → Business API → TenantRuntimeResolver
→ Platform Runtime Registry / secure cache → Secret Store → tenant DB + storage
```

Authentication is Platform; authorization is Business. Tokens carry identity/context only—never secrets or connection strings.

## Architecture style

- **Frontend:** Nx monorepo, Angular 21, declarative UI
- **Backend:** Event-driven modular monolith, bounded contexts, minimal SharedKernel
- **Async:** Event-driven workers for indexing, notifications (OCR/AI in future iterations)
- **Tenancy:** One database per tenant; Platform DB for registry and identity

## Core product layers

| Layer | Responsibility |
|-------|----------------|
| Keywords | Atomic data definitions |
| Groups | Logical field grouping |
| Tables | Repeating structures |
| Document Types | Form orchestration |
| Form Renderer | Dynamic execution engine |

## Capture modes

- Upload
- Scan
- Electronic forms
