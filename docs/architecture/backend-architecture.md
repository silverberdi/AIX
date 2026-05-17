# Backend Architecture

## Stack

- .NET 9
- ASP.NET Core Web API
- xUnit, FluentAssertions, NSubstitute
- PostgreSQL (planned)
- MediatR, FluentValidation (planned)

## Solution structure

Event-driven modular monolith: bounded contexts as projects, minimal SharedKernel, thin APIs.

```
backend/
  AIX.sln
  src/
    AIX.Platform.Api        # Platform HTTP: auth, tenants, provisioning, features
    AIX.Business.Api        # Business HTTP: documents, governance, workflows, etc.
    AIX.SharedKernel          # Cross-cutting primitives only
    AIX.Documents             # Document management BC
    AIX.Metadata              # Schema / DocumentTypes BC
    AIX.Governance            # Policies, retention BC
    AIX.Storage               # Storage providers BC
    AIX.Search                # Indexing and retrieval BC
    AIX.Workflow              # States, approvals BC
    AIX.Security              # Authorization BC (business tenant)
    AIX.ReferenceData         # Lookup datasets BC
    AIX.Integrations          # External adapters BC
    AIX.Platform              # Tenants, registry, provisioning BC
    AIX.Infrastructure        # Composition / shared infrastructure adapters only
  tests/
    AIX.*.Tests               # One test project per bounded context
    AIX.Platform.Api.Tests
    AIX.Business.Api.Tests
```

Each bounded-context project starts with exactly these top-level folders:

```
Domain/
Application/
Infrastructure/
Contracts/
Events/
```

Every bounded context starts with Domain, Application, Infrastructure, Contracts and Events. Deeper folders are created only when there is real code requiring them. Do not add layer subfolders (for example `Domain/Entities/`) until that code exists.

## Layer responsibilities

### Api (`AIX.Platform.Api`, `AIX.Business.Api`)

- HTTP entry points and composition root
- Middleware, authentication (Platform), authorization (Business), OpenAPI
- **No business logic** — delegates to bounded contexts via DI
- `AIX.Business.Api` references business BCs (Application/Contracts surfaces)
- `AIX.Platform.Api` references `AIX.Platform` and shared infrastructure only

### Bounded contexts (`AIX.Documents`, `AIX.Metadata`, …)

- Own domain rules, use cases, contracts, events, and BC-specific infrastructure
- Reference `AIX.SharedKernel` only (not other BCs directly; use contracts/events)
- **Must not** be referenced by SharedKernel

### Infrastructure (`AIX.Infrastructure`)

- Cross-cutting **composition** and shared adapters (e.g. bootstrap, shared DB host wiring)
- **Not** a dumping ground for domain rules or BC-specific persistence
- BC-specific EF/repositories live under each BC’s `Infrastructure/` folder
- References: `AIX.SharedKernel` only at the shell layer; APIs pull in BC modules

### SharedKernel (`AIX.SharedKernel`)

- Minimal primitives: `Result`/`Error`, entity/value-object bases, `DomainEvent`, strongly-typed IDs, clock, correlation/causation IDs
- **No business rules**

## Platform DB vs Tenant DB

| Platform DB | Tenant DB (one per tenant) |
|-------------|----------------------------|
| Users, tenant registry, auth identity | Documents, document types, metadata |
| Subscriptions, provisioning, feature flags | Policies, roles, authorization |
| Routing keys, secret references | Workflows, business/audit events |
| Platform audit | Search projections, reference data |

Platform DB does not store tenant business records. Tenant-domain tables do not require `tenant_id` because each tenant has a dedicated database.

## Dependency rules

```
Api ──► Bounded context (Contracts/Application) ──► Domain ──► SharedKernel
 │              ▲
 └──► AIX.Infrastructure (composition / shared adapters only)
```

| Rule | Enforced |
|------|----------|
| BC Domain ↛ other BC projects | Convention (contracts/events) |
| BC ↛ Api | Yes |
| SharedKernel ↛ BC or Infrastructure | Yes |
| Api ↛ no business logic in controllers | Convention |
| Domain rules ↛ AIX.Infrastructure | Yes |

## APIs

| API | Purpose |
|-----|---------|
| `AIX.Platform.Api` | Authentication, tenant registry, provisioning, feature enablement, identity |
| `AIX.Business.Api` | Documents, metadata, search, workflows, authorization |

## Planned cross-cutting

- MediatR for request dispatch (per BC Application layer)
- FluentValidation at application boundaries
- JWT-based tenant context; `TenantRuntimeResolver` resolves tenant DB and storage server-side
- Async workers for indexing, notifications (OCR/AI in future iterations)

## Testing

- Unit tests per bounded context under `backend/tests/`
- API smoke tests: `AIX.Platform.Api.Tests`, `AIX.Business.Api.Tests`
- Run from `backend/`: `dotnet test`
