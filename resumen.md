# AIX — Resumen consolidado (`ai/` + `docs/`)

Documento condensado a partir de la documentación en `ai/` y `docs/`. No sustituye los archivos origen; sirve como vista única del producto, arquitectura, dominio y reglas de trabajo.

---

## 1. Qué es AIX (Axioma)

**AIX** es una plataforma de gestión documental inteligente (**IDP**), orientada a **gobierno** y **PYMEs**. El **documento** es la entidad central de negocio: evidencia gobernada, no un simple archivo.

**Capacidades del producto:** composición dinámica de documentos, renderizado declarativo de formularios, formularios electrónicos y escaneo, multi-tenant, abstracción de almacenamiento, búsqueda, automatización e integraciones. OCR y AI son capas opcionales futuras; v1.0 es AI-ready pero no AI-dependent.

**Constitución del producto (18 principios):** ver `docs/domain/product-constitution.md` (gobierno primero, document-centric, policy-driven, Platform/Business separados, AI-ready not AI-dependent, one DB per tenant, etc.).

**Modelo de negocio:** suscripción + proyectos de implementación guiada.

---

## 2. Estado actual del repositorio

**Completado:** refactor de infraestructura; monorepo con Angular 21 (Nx) y backend .NET 9 (Clean Architecture); eliminación de APIs NestJS; documentación y contexto `ai/` para agentes.

**Pendiente (backlog):** implementar dominio y APIs según `docs/domain/` y `docs/business/`.

---

## 3. Estructura del monorepo

```
/apps/aix-ui              → Angular 21 SPA
/apps/aix-ui-e2e          → Playwright E2E
/libs/shared-ui           → Componentes UI compartidos
/libs/shared-core         → Utilidades frontend
/backend/                 → Solución .NET 9 (fuera del grafo Nx)
/artifacts/               → OpenAPI + JSON Schema (contratos)
/docs/                    → Documentación humana
/ai/                      → Instrucciones y contexto para agentes
```

**Híbrido:** un solo repo, dos workspaces (Nx+pnpm para frontend; .NET SDK para backend). CI puede construir y desplegar por separado.

**Contratos (`artifacts/`):** contract-first; OpenAPI desde APIs .NET; schemas para renderer; frontend y backend consumen artefactos, no DTOs paralelos.

---

## 4. Stack tecnológico

| Capa | Tecnología |
|------|------------|
| Frontend | Angular 21, Nx, PrimeNG Avalon, signals, standalone, OnPush |
| Backend | .NET 9, ASP.NET Core Web API, xUnit, FluentAssertions, NSubstitute |
| Previsto | PostgreSQL, MediatR, FluentValidation, Testcontainers |
| Infra local opcional | Docker: PostgreSQL, MinIO, Redis, OpenSearch |

**ADRs aceptados:** Nx (frontend), .NET 9 (backend), una BD por tenant, monolito modular primero, eventos de documento, seguridad policy-first, DocumentType inmutable en documentos, DocumentTypes versionados, documentos multi-archivo, renderer declarativo.

---

## 5. Arquitectura de runtime

```
aix-ui (Angular) ──HTTPS/REST──► AIX.Business.Api
                                      │
                                      ▼
                              Application + Domain
                                      │
aix-ui ──────────────────────► AIX.Platform.Api
                                      ▼
                              Infrastructure (DB, storage, search)
```

**Estilo:** monolito modular event-driven internamente; DDD ligero / Clean Architecture en backend; workers async para OCR, indexación, notificaciones.

**Multi-tenant:** una **base de datos por tenant** (no `tenant_id` en tablas de dominio del tenant). Resolución server-side: JWT (`tenant_id`, `user_id`, `session_id`) → Business API → `TenantRuntimeResolver` → Platform registry/cache → Secret Store → DB y storage dinámicos. Los tokens no transportan secretos ni connection strings.

**Runtimes separados:**
- **Platform:** auth, registro de tenants, provisioning, features, secretos/routing
- **Business:** autorización, documentos, gobierno, workflows, búsqueda

Auth en Platform; autorización en Business. Los tokens no transportan secretos.

---

## 6. Backend (.NET)

**Proyectos:**

| Proyecto | Rol |
|----------|-----|
| `AIX.Platform.Api` | Platform: auth, registry, provisioning (~5184) |
| `AIX.Business.Api` | Documentos y negocio (~5169) |
| `AIX.Documents` … `AIX.Integrations` | Bounded contexts de negocio (cada uno: Domain, Application, Infrastructure, Contracts, Events) |
| `AIX.Platform` | BC platform: tenants, registry, provisioning |
| `AIX.Infrastructure` | Composición / adapters compartidos (no reglas de dominio) |
| `AIX.SharedKernel` | Primitivos mínimos compartidos |

**Reglas de dependencia:** cada BC referencia solo `AIX.SharedKernel`. APIs delgadas; lógica en `Application/` del BC. `AIX.Infrastructure` solo bootstrap/composición; persistencia por BC en su carpeta `Infrastructure/`. Comunicación entre BCs por contratos/eventos, no referencias directas.

**Modularización:** organización por bounded contexts y capacidades; sin HTTP interno entre módulos; comunicación por eventos/contratos; kernel compartido mínimo; tablas propiedad de cada contexto; preparado para extracción futura a microservicios.

---

## 7. Frontend

- Signals-first, componentes tontos, arquitectura por features, lazy loading.
- Sin lógica de negocio en componentes UI; validación autoritativa en backend.
- UX: workspace-driven, por rol, search-first; priorizar patrones PrimeNG/Avalon.
- **Renderer declarativo:** esquema → runtime Angular (MVP); contratos estables: DocumentSchema, LayoutSchema, FieldSchema, RuleSchema, FileRequirementSchema.
- Catálogo de campos: TEXT, TEXTAREA, NUMBER, DECIMAL, DATE, DATETIME, BOOLEAN, SELECT, MULTISELECT, TABLE, FILE, RICH_TEXT.
- Modo MVP: CAPTURE_MODE; futuros: READ, COMPARE, AUDIT, APPROVAL.

---

## 8. Bounded contexts

Document Management · Metadata/Schema · Governance · Storage · Search · Workflow · Security · Reference Data · Integration · AI Capability Layer · Platform.

Comunicación entre contextos: contratos, eventos, APIs; acoplamiento directo mínimo.

---

## 9. Modelo de dominio (resumen)

### Lenguaje ubicuo
- **Keyword:** campo atómico reutilizable
- **Group / Table:** agrupación y estructuras repetibles
- **Document Type:** definición composable versionada
- **Document Instance:** datos capturados
- **Rule / Renderer / Capture Mode:** UPLOAD, SCAN, EFORM

### Documento (canon)
- Evidencia de negocio gobernada; agregado, no archivo.
- Identidad: `document_id`, `document_type_id`, `document_type_version_id`, `taxonomy_node_id`, `created_at/by`; opcional `business_document_number`.
- **DocumentType inmutable** tras creación (ADR-007).
- **Versiones de DocumentType**; cada documento enlaza la versión usada al crear (ADR-008).
- **Multi-archivo:** primario, soporte, derivados (OCR, thumbnails, PDF); requisitos por DocumentType (ADR-009).
- Metadatos en JSONB validado contra DocumentTypeVersion.
- Origen: DIGITAL_NATIVE, SCANNED, EMAIL, ERP, API, SYSTEM; pipelines policy-driven.
- Relaciones dirigidas y tipadas: RELATED_TO, ATTACHED_TO, GENERATED_FROM, BELONGS_TO, SUPERSEDES.
- **Expediente:** contenedor operativo (agrupa documentos); no sustituye identidad/auditoría/retención del documento.

### Gobierno, políticas y eventos
- Identidad inmutable vs contexto operativo mutable.
- **Políticas** declarativas (no código): RETENTION, IMMUTABILITY, OCR, SEARCH_VISIBILITY, VERSIONING, SECURITY, FILE_REQUIREMENT; estados DRAFT/ACTIVE/DISABLED/ARCHIVED.
- **Eventos** de negocio y auditoría: append-only, estructurados; campos estándar (event_id, type, aggregate, actor, occurred_at, correlation, causation, payload).
- Retención archivística; tiers HOT/COOL/COLD/ARCHIVE; sin borrado físico automático permanente; destrucción con revisión, aprobación y auditoría.

### Seguridad
- Policy-first; RBAC base insuficiente; **deny > allow**; autorización en backend; UI solo adapta experiencia.
- MVP: roles, permisos, permisos por taxonomía y DocumentType, overrides usuario.

### Búsqueda
- Híbrida: taxonomía, metadatos estructurados, full-text, expedientes, relaciones; semántica/IA futuro.
- MVP: filtros estructurados, taxonomía, DocumentType, estado, full-text básico, resultados filtrados por políticas.
- Visibilidad y OCR configurables por DocumentType/política.

### Storage
- Capacidades abstractas (upload, download, versioning, tiering, legal hold, signed URLs).
- Antes de estado **COMPLETE:** archivos reemplazables; después: gobernados e inmutables.
- Documentos versionables no sobrescriben evidencia física.

### Workflow (MVP)
- Transiciones de estado, tareas, aprobaciones humanas, triggers; tipos: aprobación documento, revisión disposición, validación metadata, cierre expediente, legal hold.
- Flujos destructivos requieren aprobación humana.

### Integración
- Adaptadores, no lógica de dominio; APIs document-centric; webhooks para async; integraciones por tenant.
- MVP: email, APIs externas, scanners, storage, IdP; futuro ERP/BPM/BI.

### Referencia y escáner
- Datasets manuales, externos o híbridos; cascading declarativo; keywords con `datasource_id`.
- Escáner: Bridge → Scanner Service → API AIX; gobierno solo en backend.

### IA
- Capa opcional, provider-agnostic; sugiere, no gobierna; no modifica metadata automáticamente; artefactos derivados regenerables/eliminables.

### Taxonomía y metadatos
- Taxonomía jerárquica por tenant; keywords reutilizables semánticamente.

---

## 10. MVP v1.0 — alcance

**Incluye:** Platform Auth, Tenant Registry, Angular 21 + PrimeNG + Avalon, renderer declarativo, DocumentTypes versionados, Keywords, taxonomía, estado COMPLETE, documentos versionables, políticas declarativas, REST + OpenAPI, webhooks, abstracción de providers, testing behavior-first.

**Excluye:** IA runtime avanzada, búsqueda semántica, OCR avanzado, BPM, integraciones ERP, SSO/passkeys, microservicios.

**Auth MVP:** usuario/contraseña local; autorización separada y policy-first.

---

## 11. API y testing

**API:** contract-first, REST + OpenAPI, APIs internas/externas separadas, document-centric, governance-aware; uploads por backend en MVP; futuro signed direct-to-storage.

**Testing:** behavior-first. Backend: xUnit, FluentAssertions, NSubstitute, Testcontainers, WebApplicationFactory, ArchUnitNET. Frontend: Vitest/Jest, Angular Testing Library, Playwright. Áreas críticas: aislamiento tenant, políticas, retención, inmutabilidad, autorización, workflows.

---

## 12. Principios de ingeniería

- UI declarativa; API como fuente de verdad
- Sin validación duplicada
- Sin fuga entre tenants
- Tipado fuerte obligatorio
- Abstracción de almacenamiento
- Procesamiento async event-driven
- Versionado inmutable

---

## 13. Estándares de código (resumen)

**C#:** .NET 9, nullable, capas respetadas, EF solo en Infrastructure, async en I/O, PascalCase / `_camelCase` privados.

**Angular:** signals, OnPush, PrimeNG, smart/dumb, alias `@aix/shared-core`, `@aix/shared-ui`.

**Comandos habituales:**
```bash
pnpm install && nx serve aix-ui
cd backend && dotnet restore && dotnet build && dotnet test
```

**Prohibido:** NestJS u otro backend Node en Nx; lógica de negocio en controllers; Infrastructure referenciado desde Domain/Application.

---

## 14. Guía para agentes de código (`ai/`)

1. Leer `project-overview` y `architecture-map`.
2. Cambio mínimo por capa; tests con cada cambio de comportamiento.
3. No ampliar alcance ni commitear sin pedido del usuario.
4. Verificar: `dotnet test` (backend), `nx test` / `nx build` (frontend).

**Checklist de revisión:** reglas de capas, sin secretos, aislamiento tenant, OnPush/signals, ADR/docs si cambia arquitectura, alinear `current-task.md`.

---

## 15. Capas del producto (composición)

Keywords → Groups → Tables → Document Types → Form Renderer.

**Modos de captura:** Upload, Scan, Electronic forms.

---

## 16. Índice de fuentes

| Área | Rutas principales |
|------|-------------------|
| Agentes | `ai/context/`, `ai/instructions/`, `ai/constitution/`, `ai/tasks/` |
| Arquitectura | `docs/architecture/` |
| Dominio | `docs/domain/` |
| ADRs | `docs/adrs/` |
| Estándares | `docs/standards/coding-standards.md` |
| Onboarding | `docs/onboarding/local-development.md` |
| Negocio | `docs/business/implementation-model.md` |

---

*Generado como resumen de `ai/` y `docs/`. Para detalle normativo, consultar el archivo fuente correspondiente.*
