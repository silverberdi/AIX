# Architecture Map (for agents)

## Monorepo layout

```
/apps/aix-ui              → Angular 21 SPA
/apps/aix-ui-e2e          → Playwright E2E
/libs/shared-ui           → Shared UI components
/libs/shared-core         → Shared frontend utilities
/backend/                 → .NET 9 solution (NOT in Nx graph)
/artifacts/               → OpenAPI + JSON Schema contracts
/docs/                    → Human documentation
/ai/                      → Agent instructions & context
```

## Backend (`backend/AIX.sln`)

| Project | Role |
|---------|------|
| AIX.Platform.Api | Platform HTTP: auth, tenant registry, provisioning (~5184) |
| AIX.Business.Api | Business HTTP: documents, governance, workflows (~5169) |
| AIX.SharedKernel | Minimal primitives only |
| AIX.Documents … AIX.Integrations | Business bounded contexts |
| AIX.Platform | Platform bounded context (tenants, registry) |
| AIX.Infrastructure | Composition / shared infrastructure adapters only |

Each BC project layout: `Domain/`, `Application/`, `Infrastructure/`, `Contracts/`, `Events/`.

**Dependency rules:** BCs reference SharedKernel only. APIs reference BCs via Application/Contracts (Business) or Platform BC (Platform). No business logic in Api controllers.

## Tenant runtime

```
Frontend → Platform Auth → JWT (tenant_id, user_id, session_id) → Business API
→ TenantRuntimeResolver → Platform registry / cache → Secret Store → tenant DB + storage
```

- Authentication: Platform (`AIX.Platform.Api`)
- Authorization: Business (`AIX.Business.Api`)
- Tokens carry identity/context only—never secrets or connection strings
- One database per tenant for business data; Platform DB for registry and identity

## Where to put new code

| Change | Location |
|--------|----------|
| HTTP endpoint | `src/AIX.*.Api` (thin) |
| Use case / handler | `src/AIX.<Context>/Application/` |
| Entity / rule | `src/AIX.<Context>/Domain/` |
| Public DTO / API contract | `src/AIX.<Context>/Contracts/` |
| Integration event | `src/AIX.<Context>/Events/` |
| BC persistence / adapter | `src/AIX.<Context>/Infrastructure/` |
| Shared bootstrap adapter | `src/AIX.Infrastructure` (sparingly) |
| Shared primitive | `src/AIX.SharedKernel` (sparingly) |
| Angular feature | `apps/aix-ui/src/app/...` |
| Reusable UI | `libs/shared-ui` |

## Commands

```bash
pnpm install && nx serve aix-ui
cd backend && dotnet restore && dotnet build && dotnet test
```

## Do not

- Add NestJS or Node backends to Nx
- Put business logic in Api controllers
- Add a global `AIX.Application` project
- Put domain rules in `AIX.Infrastructure` or `AIX.SharedKernel`
- Put connection strings or storage paths in JWTs or client requests
