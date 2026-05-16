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
apps/aix-ui              Angular application
libs/shared-ui           Shared UI components
libs/shared-core         Shared frontend core
backend/                 .NET solution (AIX.sln)
docs/                    Architecture, domain, standards
ai/                      Agent context & instructions
```

## Quick start

### Frontend

```bash
pnpm install
nx serve aix-ui
```

### Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet test
```

Run APIs:

```bash
dotnet run --project src/AIX.Tenant.Api
dotnet run --project src/AIX.Business.Api
```

## Documentation

- [Local development](docs/onboarding/local-development.md)
- [System overview](docs/architecture/system-overview.md)
- [Backend architecture](docs/architecture/backend-architecture.md)
- [Coding standards](docs/standards/coding-standards.md)

## Principles

See [engineering principles](ai/constitution/engineering-principles.md).
