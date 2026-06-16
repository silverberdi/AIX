# AIX Operational Memory

Concise context for coding and review agents. Load this before executing any slice.

## Stage

| Item | Value |
|------|--------|
| **Wave** | Wave 3 — Capture MVP (**in progress** — slices 016–017 done; slice 018 next) |
| **Slices 001–009** | **Done** (Wave 1 complete; Slice 009 verified 2026-05-22) |
| **Slices 010–015** | **Done** (Wave 2 complete; Slice 015 verified 2026-05-23) |
| **Wave 0 Domain Review** | **Done** — `docs/reviews/wave-0-domain-review.md` (2026-05-17) |
| **Wave 1 Domain Review** | **Done** — `docs/reviews/wave-1-domain-review.md` (2026-05-22) |
| **Wave 2 Domain Review** | **Done** — `docs/reviews/wave-2-domain-review.md` (2026-05-23) — **PASS** |
| **Next target** | slice-018 — Capture validation port (`ICaptureMetadataValidator`) |

## Official stack

- **Frontend:** Angular 21, PrimeNG 21, Avalon theme, Nx monorepo (`apps/aix-ui`, `libs/shared-ui`, `libs/shared-core`)
- **Backend:** .NET 9 modular monolith (`backend/AIX.sln`)
- **Database:** PostgreSQL (canonical; one DB per tenant for business data)

## Architecture rules

- **Modular monolith first** — no microservices during MVP
- **Vertical Slice Architecture** — deliver behavior in thin vertical slices with explicit DoD
- **Bounded Context isolation** — BCs reference `AIX.SharedKernel` only; integrate via contracts/events
- **SharedKernel minimal** — primitives only (`Result`, `Entity`, IDs, clock, domain event base)
- **Test-first execution** — behavior tests before or alongside implementation
- **AI-first slice/handoff workflow** — read `ai/tasks/current-task.md` and the active slice file every session

## Current implementation reality

- Repository is still **mostly skeleton/placeholders** outside completed slices
- **Slice 001** — `Document` aggregate with create factory, typed IDs, `DocumentCreated` (7 tests)
- **Slice 002** — SharedKernel primitives audited; `AIX.SharedKernel.Tests` (12 tests)
- **Slice 003** — `Document.Complete(IClock)` transitions **Draft → Complete**; `DocumentCompleted` event; re-complete fails via `Result` (`DocumentErrors.AlreadyComplete`); **Complete** documents are immutable for completed-state transitions (5 tests)
- **Slice 004** — `Document.AttachFile` adds primary/supporting file references (domain metadata only); `DocumentFile` value object, `DocumentFileId`, `DocumentFileRole`; attachment rules enforced; `DocumentFileAttached` event; complete documents reject file mutation (13 tests)
- **Slice 005** — `DocumentType` aggregate in `AIX.Metadata` with `DocumentTypeId`, `Name`, `Code`, `DocumentTypeState` (Active/Inactive), `Create` factory, `Activate`/`Deactivate`, `DocumentTypeCreated` event; validation via `Result` (`DocumentTypeErrors`); **10 tests** in `AIX.Metadata.Tests`
- **Slice 006** — `DocumentType.CreateVersion` on `DocumentType` with immutable `DocumentTypeVersion` records (`DocumentTypeVersionId`, `VersionNumber`, `CreatedAt`); auto-increment or explicit version number; duplicate/invalid version failures; `LatestVersion`; `DocumentTypeVersionCreated` event; **19 tests** in `AIX.Metadata.Tests`
- **Slice 007** — `KeywordRegistry` aggregate in `AIX.Metadata` with `KeywordRegistryId`, `Keyword` entities (`KeywordId`, `Code`, `Name`, `KeywordDataType`, `MaxLength`); `Register` factory; duplicate code/name rejection; length constraints for text types; immutable `DataType`; `KeywordRegistered` event; `KeywordErrors`; **32 tests** in `AIX.Metadata.Tests`
- **Slice 008** — `KeywordGroup` aggregate in `AIX.Metadata` with `KeywordGroupId`, `Code`, `Name`, `IsRepeatable`, `IsRequired`, registered `KeywordId` references; `Create` factory validates integrity against `KeywordRegistry`; duplicate/unregistered keyword rejection; `KeywordGroupCreated` event; `KeywordGroupErrors`; **42 tests** in `AIX.Metadata.Tests`
- **Slice 009** — Centralized `KeywordValidator` with specification-based value rules (required, data type, text max length) and configuration evolution policy (immutable data type, no max-length reduction); `KeywordValidationResult` for multiple failures; `KeywordRegistry.ValidateKeywordValue`; `KeywordValidationErrors`; **59 tests** in `AIX.Metadata.Tests`
- **Slice 010** — `FieldSchema` immutable value object in `AIX.Metadata` with `FieldSchemaId`, `FieldCatalogType` (MVP subset: Text, TextArea, Number, Decimal, Date, DateTime, Boolean), `KeywordId` binding via `KeywordRegistry`, optional label/help/order metadata, `IsDeprecated`/`IsHidden`/`IsActive`, `IsRequired`, `SelectActive` filter; `FieldCatalogTypeCompatibility` matrix vs `KeywordDataType`; `FieldSchemaErrors`; **82 tests** in `AIX.Metadata.Tests`
- **Slice 011** — `VersionSchemaComposition` immutable snapshot on `DocumentTypeVersion` with ordered `FieldSchema` entries; `VersionSchemaFieldDefinition` + `FromDefinitions` at `CreateVersion`; duplicate keyword/field rejection (`VersionSchemaCompositionErrors`); empty composition allowed; `VersionSchemaFieldSnapshot` on `DocumentTypeVersionCreated.FieldSnapshots` (field id, keyword id/code, catalog type); **90 tests** in `AIX.Metadata.Tests`
- **Slice 012** — `VersionSchemaGroupAssignment` on `VersionSchemaComposition` with `VersionSchemaGroupAssignmentDefinition` at `CreateVersion` (registry + keyword groups + fields + groups); point-in-time group/registry integrity; non-repeatable duplicate placement and repeatable unique instance keys; keyword overlap group+standalone rejected; `IsRequired`/`IsRepeatable` snapshotted from `KeywordGroup`; `VersionSchemaGroupSnapshot` on `DocumentTypeVersionCreated.GroupSnapshots`; **99 tests** in `AIX.Metadata.Tests`
- **Slice 013** — `LayoutSchema` immutable presentation model on `DocumentTypeVersion` (separate from `VersionSchemaComposition`); `LayoutSection` + field/group placements referencing composition keys (`KeywordId`, `KeywordGroupId` + instance key); default layout when none supplied; duplicate/unknown reference rejection via `LayoutSchemaErrors`; `LayoutSectionSnapshot` on `DocumentTypeVersionCreated.LayoutSnapshots`; composition vs layout separation enforced
- **Slice 014** — `AIX.Metadata.Contracts` renderer DTOs (`DocumentSchemaContract`, `FieldSchemaContract`, `LayoutSchemaContract`, full `RendererFieldCatalogType` enum); `DocumentSchemaProjector` read-only export from `DocumentType` + version id; `SchemaBindingKey` (`{code}/v{n}`); `DatasetId` placeholder on fields; `DefaultLayoutGenerator` heuristic sections (General + per keyword group); **115 tests** in `AIX.Metadata.Tests`
- **Slice 015** — `VersionMetadataPayload` + `MetadataGroupInstancePayload`; `VersionSchemaValidator` + `DocumentType.ValidateMetadataAgainstVersion`; `SchemaValidationResult` (multiple deterministic errors, no throw on expected failures); required semantics unified: `FieldSchema.IsRequired` for standalone fields, `VersionSchemaGroupAssignment.IsRequired` for group keywords via `KeywordValidator` delegation; capture rejects hidden/deprecated/unknown keys and unexpected group instances; **129 tests** in `AIX.Metadata.Tests`
- **Slice 016** — `AIX.Documents.Contracts` passive capture contracts (`CapturedMetadataPayload`, `CapturedMetadataGroupInstance`, `CaptureValidationResult`, `CaptureValidationError`); zero project references (no SharedKernel/Metadata/Documents); defensive copies for immutability; forbidden-reference and no-port boundary tests; **8 tests** in `AIX.Documents.Contracts.Tests`
- **Slice 017** — `Document` captured metadata attachment while **Draft**; Documents-owned `DocumentCapturedMetadata` + `DocumentCapturedMetadataGroupInstance` (mapped from `CapturedMetadataPayload` via defensive `From` factory); `SetCapturedMetadata` replace semantics; Complete rejects with `DocumentErrors.CannotModifyWhenComplete`; `DocumentMetadataCaptured` event; immutable `DocumentTypeId`/`DocumentTypeVersionId`; **no schema validation** (slice 018); **7 new tests** (**32 total** in `AIX.Documents.Tests`)
- **Wave 2 Domain Review** — **PASS** (2026-05-23); composition/layout separation healthy; Wave 3 in progress
- **Do not add** persistence, APIs, EF Core, MediatR, or UI during domain-only slices unless the active slice explicitly requires it
- PrimeNG 21 and Avalon theme setup are **deferred** to dedicated UI tasks

## Agent rules

1. Read `ai/tasks/current-task.md` first, then the active slice under `ai/backlog/mvp/`
2. Read `ai/context/current-state.md`, `ai/context/active-decisions.md`, and `ai/context/known-pitfalls.md` before code changes
3. Read relevant `docs/` and `ai/context/project-overview.md` / `architecture-map.md` before editing
4. Stay inside bounded contexts and projects listed in the slice
5. Do not create duplicate docs, parallel folder trees, or a `.ai/` directory — use **`ai/`** only
6. Do not mark a slice **Done** without Definition of Done verification and passing tests
7. After every completed slice, update operational memory, current state, backlog status, and current task

## Related context files

| File | Purpose |
|------|---------|
| `ai/context/current-state.md` | Execution health and blockers |
| `ai/context/active-decisions.md` | Locked architectural/product choices |
| `ai/context/known-pitfalls.md` | Anti-patterns agents must avoid |
| `ai/context/project-overview.md` | Product and BC overview |
| `ai/context/architecture-map.md` | Repo layout and where to put code |
