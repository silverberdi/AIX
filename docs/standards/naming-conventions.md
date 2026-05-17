# Naming Conventions

Canonical naming rules for the AIX monorepo. New code and refactors should follow these conventions. Existing names are not renamed unless an explicit migration task says otherwise.

Related: [coding-standards.md](./coding-standards.md), [backend-architecture.md](../architecture/backend-architecture.md).

---

## 1. Backend projects

Use **PascalCase** with the **`AIX.`** prefix.

| Project | Role |
|---------|------|
| `AIX.Business.Api` | Business HTTP surface |
| `AIX.Platform.Api` | Platform HTTP surface |
| `AIX.Documents` | Documents bounded context |
| `AIX.Metadata` | Metadata / document types BC |
| `AIX.Governance` | Policies, retention BC |
| `AIX.Storage` | Storage providers BC |
| `AIX.Search` | Indexing and retrieval BC |
| `AIX.Workflow` | States, approvals BC |
| `AIX.Security` | Business authorization BC |
| `AIX.ReferenceData` | Lookup datasets BC |
| `AIX.Integrations` | External adapters BC |
| `AIX.Platform` | Tenants, registry, provisioning BC |
| `AIX.SharedKernel` | Cross-cutting primitives only |
| `AIX.Infrastructure` | Composition and shared infrastructure adapters only (not a BC) |

Test projects mirror the source name with a `.Tests` suffix, for example `AIX.Documents.Tests`, `AIX.Business.Api.Tests`.

---

## 2. Backend namespaces

Namespaces must match **project name** and **folder structure**.

Example for `AIX.Documents`:

```
AIX.Documents.Domain
AIX.Documents.Application
AIX.Documents.Contracts
AIX.Documents.Events
AIX.Documents.Infrastructure
```

Deeper folders extend the namespace (for example `AIX.Documents.Application.Commands.CreateDocument`).

---

## 3. Classes

Use **PascalCase** for all types.

| Kind | Examples |
|------|----------|
| Entities / value objects | `Document`, `DocumentType`, `DocumentTypeVersion`, `DocumentFile` |
| Events | `DocumentCompletedEvent` |
| Commands | `CreateDocumentCommand` |
| Handlers | `CreateDocumentHandler` |
| API / contract DTOs | `DocumentResponse` |
| Infrastructure | `DocumentRepository` |

---

## 4. Interfaces

Use the **`I`** prefix **only for abstractions** (ports, providers, clocks, repositories).

Examples:

- `IDocumentRepository`
- `ITenantRuntimeResolver`
- `IStorageProvider`
- `IClock`

Do **not** create interfaces for every concrete class by default. Prefer interfaces at boundaries (Application ports, Infrastructure adapters, cross-cutting abstractions).

---

## 5. Commands and queries

**Commands** mutate state. Name with an imperative verb + aggregate/resource:

- `CreateDocumentCommand`
- `CompleteDocumentCommand`
- `AttachDocumentFileCommand`

**Queries** read state. Name with `Get`, `Search`, `List`, or similar:

- `GetDocumentByIdQuery`
- `SearchDocumentsQuery`
- `GetDocumentTimelineQuery`

**Handlers** pair one-to-one with the command or query:

- `CreateDocumentHandler`
- `SearchDocumentsHandler`

---

## 6. DTOs and API contracts

HTTP and public contract types live in **`Contracts/`** (or API-specific request/response types that map to contracts). Do **not** expose domain entities directly through APIs.

**Requests**

- `CreateDocumentRequest`
- `AttachFileRequest`
- `SearchDocumentsRequest`

**Responses**

- `DocumentResponse`
- `DocumentFileResponse`
- `SearchDocumentsResponse`

Map between domain models and DTOs in Application or API mapping layers.

---

## 7. Events

**Domain / internal events** — past tense, PascalCase:

- `DocumentCreated`
- `DocumentFileAttached`
- `DocumentCompleted`
- `DocumentTypeVersionCreated`
- `PolicyActivated`

**Integration events** exposed outside a BC — versioned when published externally:

- `DocumentCreatedV1`
- `DocumentCompletedV1`

**Serialized event type names** (message bus, outbox, telemetry) may use **SCREAMING_SNAKE_CASE** constants:

- `DOCUMENT_CREATED`
- `DOCUMENT_COMPLETED`

---

## 8. Policies

**Policy types** — PascalCase:

- `RetentionPolicy`
- `OcrPolicy`
- `SearchVisibilityPolicy`
- `VersioningPolicy`
- `SecurityPolicy`

**Serialized policy type identifiers** — SCREAMING_SNAKE_CASE:

- `RETENTION`
- `OCR`
- `SEARCH_VISIBILITY`
- `VERSIONING`
- `SECURITY`

Evaluators and handlers use specific names (for example `RetentionPolicyEvaluator`), not generic `PolicyProcessor`.

---

## 9. Enums

**C# enum members** — PascalCase:

```csharp
Draft
Active
Disabled
Archived
```

**Serialized API / contract values** — SCREAMING_SNAKE_CASE when part of an external contract:

```
DRAFT
ACTIVE
DISABLED
ARCHIVED
```

Use explicit serialization attributes or mappers; do not rely on default enum numeric values in public APIs.

---

## 10. Database naming

Use **snake_case** for tables and columns.

**Tables**

- `documents`
- `document_files`
- `document_types`
- `document_type_versions`

**Columns**

- `created_at`
- `created_by`
- `document_type_id`

EF Core configurations map PascalCase domain properties to snake_case columns in Infrastructure.

---

## 11. JSON fields

API JSON payloads use **camelCase**:

- `documentId`
- `documentTypeId`
- `createdAt`
- `businessDocumentNumber`

Configure ASP.NET Core JSON options for camelCase. OpenAPI schemas should match.

---

## 12. Angular / Nx libraries

**Nx project names** — kebab-case:

- `shared-core`
- `shared-ui`
- `shared-data-access`
- `documents-feature`
- `documents-data-access`
- `documents-ui`

**TypeScript symbols** — PascalCase for types/classes, camelCase for functions/variables/signals:

| Kind | Examples |
|------|----------|
| Components | `DocumentListComponent` |
| Stores | `DocumentSearchStore` |
| Clients | `DocumentApiClient` |
| Renderers | `DocumentFormRendererComponent` |

Path aliases follow `@aix/<lib-name>` (see [coding-standards.md](./coding-standards.md)).

---

## 13. Routes

Use **kebab-case** URL segments:

- `/documents`
- `/document-types`
- `/search`
- `/admin/policies`

Route parameters: `:documentId`, `:documentTypeId` (camelCase in route config where Angular expects it).

---

## 14. Files

**C#** — file name matches the primary type (one main type per file when practical):

- `Document.cs`
- `CreateDocumentCommand.cs`
- `CreateDocumentHandler.cs`

**Angular** — kebab-case file names with standard suffixes:

- `document-list.component.ts`
- `document-search.store.ts`
- `document-api.client.ts`

Spec files: `document-list.component.spec.ts`, `document-search.store.spec.ts`.

---

## 15. Tests

**Backend** — test class names describe the unit under test:

- `DocumentTests`
- `CompleteDocumentTests`
- `CreateDocumentHandlerTests`
- `DocumentAuthorizationTests`

Test methods: `MethodName_Scenario_ExpectedResult` or a clear descriptive phrase (see [coding-standards.md](./coding-standards.md)).

**Frontend** — follow Angular/Nx defaults (Jest/Vitest/Playwright); spec files sit beside the source with `.spec.ts` suffix.

---

## 16. Artifacts

Versioned contract files under `artifacts/`:

**OpenAPI**

- `artifacts/openapi/business-api.v1.openapi.json`
- `artifacts/openapi/platform-api.v1.openapi.json`

**JSON Schema**

- `artifacts/schemas/document-schema.v1.json`
- `artifacts/schemas/layout-schema.v1.json`
- `artifacts/schemas/field-schema.v1.json`

Pattern: `{artifact-name}.v{major}.openapi.json` or `{artifact-name}-schema.v{major}.json`. Commit released versions explicitly; see `artifacts/README.md`.

---

## 17. Forbidden naming

Avoid vague, overloaded suffixes:

| Avoid | Prefer |
|-------|--------|
| `Manager` | `DocumentCompletionHandler`, `TenantRuntimeResolver` |
| `Helper` | Named function or small type in the right layer |
| `Util` / `Utils` | Feature-specific module (for example `CorrelationHeaderParser`) |
| `Processor` | `RetentionPolicyEvaluator`, `DocumentIndexProjector` |
| `Service` (generic) | `DocumentApiClient`, `StorageProviderRegistry`, `TenantRuntimeResolver` |

`Service` is acceptable only when it matches a well-known framework concept (for example ASP.NET Core `IServiceCollection` extension methods in `DependencyInjection.cs`), not as a catch-all for business logic.

---

## Quick reference

| Surface | Convention | Example |
|---------|------------|---------|
| .NET project | `AIX.PascalCase` | `AIX.Documents` |
| Namespace | Matches project + folder | `AIX.Documents.Application` |
| C# type | PascalCase | `CreateDocumentCommand` |
| Interface | `I` + PascalCase | `IDocumentRepository` |
| DB table/column | snake_case | `document_files`, `created_at` |
| JSON property | camelCase | `documentId` |
| API enum (external) | SCREAMING_SNAKE_CASE | `DRAFT` |
| Event type (serialized) | SCREAMING_SNAKE_CASE | `DOCUMENT_CREATED` |
| Nx lib / route / file | kebab-case | `documents-feature`, `/document-types` |
| TS class/component | PascalCase | `DocumentListComponent` |
