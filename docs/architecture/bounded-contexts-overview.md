# AIX Bounded Contexts Overview

Each bounded context is a .NET project under `backend/src/AIX.<Name>/` with:

```
Domain/
Application/
Infrastructure/
Contracts/
Events/
```

## Core Bounded Contexts

| Context | Project | Responsibility |
|---------|---------|----------------|
| Document Management | `AIX.Documents` | Documents, files, relations and evidence |
| Metadata / Schema | `AIX.Metadata` | Keywords, groups, layouts and DocumentTypes |
| Governance | `AIX.Governance` | Policies, retention, immutability and disposition |
| Storage | `AIX.Storage` | Storage providers, lifecycle and file management |
| Search | `AIX.Search` | Indexing, search and retrieval |
| Workflow | `AIX.Workflow` | States, approvals, tasks and transitions |
| Security | `AIX.Security` | Authorization and policies (tenant business) |
| Reference Data | `AIX.ReferenceData` | Datasets and lookup structures |
| Integration | `AIX.Integrations` | External adapters, APIs and connectors |
| Platform | `AIX.Platform` | Tenants, provisioning and operational runtime |

### AI Capability Layer (future)

OCR, extraction, semantic search and AI services — not a separate project in v1; may become a BC when implemented.

## HTTP entry points

| API | References |
|-----|------------|
| `AIX.Platform.Api` | `AIX.Platform`, `AIX.Infrastructure`, `AIX.SharedKernel` |
| `AIX.Business.Api` | Business BCs (via Application/Contracts), `AIX.Infrastructure`, `AIX.SharedKernel` |

## Architectural Principle

Bounded contexts communicate through:

- contracts
- events
- APIs (external only — no internal HTTP between modules)

Direct coupling between contexts should be minimized.
