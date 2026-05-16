# Backend Architecture

## Stack

- .NET 9
- ASP.NET Core Web API
- xUnit, FluentAssertions, NSubstitute
- PostgreSQL (planned)
- MediatR, FluentValidation (planned)

## Solution structure

```
backend/
  AIX.sln
  src/
    AIX.Tenant.Api          # Tenant administration & resolution
    AIX.Business.Api        # Document & business operations
    AIX.Application         # Use cases, orchestration
    AIX.Documents.Domain    # Document bounded context
    AIX.Security.Domain     # Security bounded context
    AIX.Infrastructure      # Persistence, external providers
    AIX.SharedKernel        # Minimal cross-cutting primitives
  tests/
    *.Tests                 # One test project per layer under test
```

## Layer responsibilities

### Api (`AIX.Tenant.Api`, `AIX.Business.Api`)

- HTTP entry points and composition root
- Middleware, authentication, OpenAPI
- **No business logic** — delegates to Application via DI
- References: Application, Infrastructure, SharedKernel

### Application (`AIX.Application`)

- Use cases, commands/queries, DTO mapping
- Application services and validation orchestration
- References: Documents.Domain, Security.Domain, SharedKernel
- **Must not** reference Infrastructure

### Domain (`AIX.Documents.Domain`, `AIX.Security.Domain`)

- Entities, value objects, domain services, domain events
- Business invariants and rules
- References: SharedKernel only
- **Must not** reference Application or Infrastructure

### Infrastructure (`AIX.Infrastructure`)

- EF Core, repositories, external APIs, storage, messaging
- Implements interfaces defined in Application/Domain
- References: Application, both Domain projects, SharedKernel

### SharedKernel (`AIX.SharedKernel`)

- Minimal shared primitives (e.g. base types, result patterns)
- Keep small — avoid becoming a dumping ground

## Dependency rules

```
Api ──────────► Application ──────► Domain ──────► SharedKernel
 │                  ▲                ▲
 │                  │                │
 └──────► Infrastructure ─────────────┘
```

| Rule | Enforced |
|------|----------|
| Domain → no Infrastructure | Yes |
| Domain → no Application | Yes |
| Application → no Infrastructure | Yes |
| Api → no direct Domain logic | Convention |

## APIs

| API | Purpose |
|-----|---------|
| `AIX.Tenant.Api` | Tenant lifecycle, schema provisioning, tenant context |
| `AIX.Business.Api` | Documents, metadata, search, workflows |

## Planned cross-cutting

- MediatR for request dispatch
- FluentValidation at Application boundaries
- Schema-per-tenant via `x-tenant-id` middleware
- Async workers for OCR, indexing, notifications

## Testing

- Unit tests per project under `backend/tests/`
- Run from `backend/`: `dotnet test`
