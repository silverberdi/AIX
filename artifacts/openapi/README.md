# OpenAPI Contracts

Exported OpenAPI documents for AIX HTTP APIs.

## APIs

| Service | Source project | Typical export name |
|---------|----------------|---------------------|
| Tenant API | `backend/src/AIX.Tenant.Api` | `tenant-api.openapi.json` |
| Business API | `backend/src/AIX.Business.Api` | `business-api.openapi.json` |

## Generation (planned)

Export during build or CI from ASP.NET Core (Swashbuckle / built-in OpenAPI):

```bash
# Example — to be wired in backend/Directory.Build.targets or a script
dotnet run --project backend/src/AIX.Tenant.Api -- swagger export ...
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

## Git

Generated `*.json` / `*.yaml` files are ignored by default (see root `.gitignore`). Commit explicitly when a contract version is released.
