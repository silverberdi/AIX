# Repository Structure

## Rationale: hybrid monorepo

AIX uses a **single repository** with **two independent workspaces**:

| Workspace | Tooling | Scope |
|-----------|---------|--------|
| Frontend | Nx + pnpm | `apps/`, `libs/` |
| Backend | .NET SDK | `backend/` |

This keeps product code colocated while respecting different build graphs, lifecycles, and deployment units.

## Why Nx for frontend only

- Angular app (`aix-ui`) and shared TypeScript libraries benefit from Nx caching, dependency graph, and generators.
- E2E (Playwright) and unit tests (Jest/Vitest) are first-class Nx targets.
- No Node/Nest backend remains — Nx does **not** orchestrate .NET builds.

## Why backend stays independent

- .NET 9 solution (`backend/AIX.sln`) follows Clean Architecture with its own test runner (`dotnet test`).
- APIs, domain, application, and infrastructure evolve on the CLR stack without webpack or Nx executors.
- CI can build and deploy backend artifacts separately from the SPA.

## Top-level layout

```
apps/           Angular applications (Nx projects)
libs/           Shared frontend libraries (Nx projects)
backend/        .NET solution — outside Nx graph
artifacts/      OpenAPI + JSON Schema contracts (generated)
docs/           Human documentation (domain, architecture, ADRs)
ai/             Agent context and instructions
```

## Shared artifacts strategy

`artifacts/` holds **machine-readable contracts**:

- `artifacts/openapi/` — HTTP API surface exported from .NET
- `artifacts/schemas/` — JSON Schemas for renderer and structural validation

**Contract-first:** domain docs express intent; artifacts express what tools compile and validate against. Frontend and backend both consume artifacts; neither invents parallel DTO shapes.

## Future OpenAPI / schema generation

1. **OpenAPI**: export from `AIX.Tenant.Api` and `AIX.Business.Api` during build or CI → `artifacts/openapi/`.
2. **Schemas**: publish document-type and renderer schemas from domain/application pipelines → `artifacts/schemas/`.
3. **Frontend**: generate or sync clients/types into `libs/shared-data-access` (or successor lib).
4. **Versioning**: artifact filenames include API/schema version; breaking changes require explicit bumps.

Root `package.json` scripts wrap common commands; they delegate to `nx` or `dotnet` without merging the two graphs.

## Separation of concerns

| Concern | Location |
|---------|----------|
| UI composition | `apps/aix-ui`, `libs/shared-ui` |
| Frontend shared logic | `libs/shared-core`, `libs/shared-data-access` |
| HTTP APIs | `backend/src/AIX.*.Api` |
| Business rules | `backend/src/AIX.Application`, `*Domain` |
| Persistence / integrations | `backend/src/AIX.Infrastructure` |
| Product language & rules (human) | `docs/domain/` |
| Executable contracts | `artifacts/` |

## Related

- [Backend architecture](./backend-architecture.md)
- [Frontend architecture](./frontend-architecture.md)
- [Local development](../onboarding/local-development.md)
- [Artifacts README](../../artifacts/README.md)
