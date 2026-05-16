# System Overview

## Vision

AIX (Axioma) is an Intelligent Document Processing (IDP) platform where the document is the central business entity.

The platform supports:

- Dynamic document composition
- Declarative form rendering
- OCR and AI extraction
- Hybrid electronic forms and scanned documents
- Multi-tenant architecture
- Storage abstraction
- Intelligent search
- Automation and integrations

## Repository layout

| Path | Role |
|------|------|
| `apps/aix-ui` | Angular SPA (Nx) |
| `libs/shared-ui`, `libs/shared-core` | Shared frontend libraries |
| `backend/` | .NET 9 solution (APIs, domain, application, infrastructure) |
| `docs/` | Architecture, domain, standards, ADRs |
| `ai/` | Agent context and instructions |

## Runtime topology

```
┌─────────────┐     HTTPS/REST      ┌──────────────────┐
│  aix-ui     │ ──────────────────► │ AIX.Business.Api │
│  (Angular)  │                     └────────┬─────────┘
└─────────────┘                              │
       │                                     ▼
       │                              Application + Domain
       │                                     │
       └────────────────────────────► ┌──────────────────┐
                                      │ AIX.Tenant.Api   │
                                      └────────┬─────────┘
                                               ▼
                                      Infrastructure (DB, storage, search)
```

## Architecture style

- **Frontend:** Nx monorepo, Angular 21, declarative UI
- **Backend:** Modular monolith, Clean Architecture / light DDD
- **Async:** Event-driven workers for OCR, indexing, notifications
- **Tenancy:** Schema-per-tenant with tenant resolution middleware

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
