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

| Project | Layer | Notes |
|---------|-------|-------|
| AIX.Tenant.Api | Api | Tenant admin; port ~5184 |
| AIX.Business.Api | Api | Business/documents; port ~5169 |
| AIX.Application | Application | Use cases only |
| AIX.Documents.Domain | Domain | Document BC |
| AIX.Security.Domain | Domain | Security BC |
| AIX.Infrastructure | Infrastructure | DB, storage, externals |
| AIX.SharedKernel | Shared | Minimal shared types |

**Dependency rules:** Domain ↛ Application/Infrastructure. Application ↛ Infrastructure.

## Where to put new code

| Change | Location |
|--------|----------|
| HTTP endpoint | `src/AIX.*.Api` (thin) |
| Use case / handler | `src/AIX.Application` |
| Entity / rule | `src/AIX.*.Domain` |
| Repository / EF | `src/AIX.Infrastructure` |
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
- Reference Infrastructure from Domain or Application
