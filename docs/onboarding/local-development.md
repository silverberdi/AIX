# Local Development

Monorepo with **Angular + Nx** (frontend) and **.NET 9** (backend) in separate physical roots.

## Prerequisites

- Node.js 20+ and [pnpm](https://pnpm.io/)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Docker (optional, for PostgreSQL, MinIO, Redis, OpenSearch)

## Frontend

From the repository root:

```bash
pnpm install
nx serve aix-ui
```

Other useful commands:

```bash
nx build aix-ui
nx test aix-ui
nx e2e aix-ui-e2e
```

The UI is served at `http://localhost:4200` by default.

## Backend

From the repository root:

```bash
cd backend
dotnet restore
dotnet build
dotnet test
```

Run APIs individually:

```bash
dotnet run --project src/AIX.Tenant.Api
dotnet run --project src/AIX.Business.Api
```

Default ports (see `launchSettings.json`):

| API | HTTP |
|-----|------|
| AIX.Tenant.Api | `http://localhost:5184` |
| AIX.Business.Api | `http://localhost:5169` |

## Infrastructure services (optional)

When local dependencies are required:

```bash
docker compose up
```

Expected services: PostgreSQL, MinIO, Redis, OpenSearch.
