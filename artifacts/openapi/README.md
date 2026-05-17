# OpenAPI Contracts

Exported OpenAPI documents for AIX HTTP APIs.

## APIs

| Service | Source project | Typical export name |
|---------|----------------|---------------------|
| Platform API (internal) | `backend/src/AIX.Platform.Api` | `platform-api.openapi.json` |
| Business API (internal) | `backend/src/AIX.Business.Api` | `business-api.openapi.json` |
| External integrator API | Business API external surface | `external-v1.openapi.json` (versioned) |

External API contracts are published under this directory. JSON Schemas for renderer and structural contracts live under `artifacts/schemas/`.

## Generation (planned)

Export during build or CI from ASP.NET Core (Swashbuckle / built-in OpenAPI):

```bash
# Example — to be wired in backend/Directory.Build.targets or a script
dotnet run --project backend/src/AIX.Platform.Api -- swagger export ...
```

Outputs land in this directory for:

- Frontend client generation (`libs/shared-data-access` or dedicated clients lib)
- Contract review in PRs
- External integrators

## Usage — frontend

- Import types or clients generated from these specs.
- Do not hand-write request/response shapes that diverge from OpenAPI.

## Usage — backend

- OpenAPI reflects the **published** API surface only.
- Internal Application/Domain types are not duplicated here.
- External APIs must not expose internal domain entities or raw search DSL.

## Git

Generated `*.json` / `*.yaml` files are ignored by default (see root `.gitignore`). Commit explicitly when a contract version is released.
