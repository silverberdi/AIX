# Runtime Architecture

AIX is a **modular monolith first** and **internally event-driven**.

## Process model

- Two ASP.NET Core hosts: `AIX.Platform.Api` (identity, registry) and `AIX.Business.Api` (tenant business).
- Bounded contexts run in-process as libraries; they do not call each other over HTTP.
- Cross-context integration uses **contracts** and **integration events** (async handlers/workers planned).

## Request flow (business)

```
Client → AIX.Business.Api → BC Application layer → BC Domain
                              ↓
                    BC Infrastructure (tenant DB, storage)
```

Platform auth issues JWT; `TenantRuntimeResolver` (planned) resolves tenant DB and storage server-side.

## Shared infrastructure

`AIX.Infrastructure` wires composition (DI bootstrap, shared adapters). BC-specific persistence lives under each `AIX.<Context>/Infrastructure/`.

## Related

- [Backend architecture](./backend-architecture.md)
- [Multi-tenant runtime](./multi-tenant-runtime-canon.md)
- [Modularization canon](./modularization-canon.md)
