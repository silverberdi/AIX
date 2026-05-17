# AIX (Axioma)

Intelligent document management platform: structured composition, dynamic forms, OCR/AI extraction, multi-tenancy, and hybrid storage.

## Stack

| Area | Technology |
|------|------------|
| Frontend | Angular 21, Nx, PrimeNG |
| Backend | .NET 9, ASP.NET Core |
| Data (planned) | PostgreSQL, MinIO, OpenSearch |

Monorepo with **physical separation**: frontend under `apps/` and `libs/`, backend under `backend/`.

## Repository structure

```
apps/aix-ui              Angular application (Nx)
libs/                    Shared frontend libraries (Nx)
backend/                 .NET solution (AIX.sln) — independent of Nx
artifacts/               OpenAPI + JSON Schema contracts
docs/                    Architecture, domain, standards
ai/                      Agent context & instructions
```

## Quick start

```bash
pnpm install
pnpm run serve:ui
```

Backend (from repo root):

```bash
pnpm run build:backend
pnpm run test:backend
pnpm run serve:tenant-api
pnpm run serve:business-api
```

See [repository structure](docs/architecture/repository-structure.md) for the hybrid monorepo model.

## Documentation

- [Local development](docs/onboarding/local-development.md)
- [System overview](docs/architecture/system-overview.md)
- [Backend architecture](docs/architecture/backend-architecture.md)
- [Repository structure](docs/architecture/repository-structure.md)
- [Coding standards](docs/standards/coding-standards.md)

## Principles

See [engineering principles](ai/constitution/engineering-principles.md).
