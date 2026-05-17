# JSON Schemas

JSON Schema documents for declarative document types, field definitions, renderer configuration, and runtime validation.

## Purpose

- **Renderer**: validate and interpret dynamic form/document definitions at runtime.
- **Backend**: align ingestion and metadata validation with the same structural rules where applicable.
- **Tooling**: design-time validation, tests, and future low-code editors.

## Ownership

- Canonical **semantic** rules remain in `docs/domain/` (human canon).
- **Executable** structure for runtime lives here as versioned schema files.

## Generation flow (planned)

```
Document type definition (source of truth in backend/domain)
        │
        ▼
  Publish / export JSON Schema
        │
        ▼
  artifacts/schemas/{domain}/{name}-v{version}.schema.json
        │
        ▼
  Angular renderer + optional AOT validators
```

## Usage — frontend

- Load schemas for runtime validation (AJV or equivalent).
- Map schema `$id` / version to document type version in the UI.

## Usage — backend

- Validate payloads against the same schema revision before persistence when enforcing structure at the edge.

## Contract-first rule

If UI and API disagree, **the schema revision in this folder** (and OpenAPI for HTTP) wins until an ADR changes the process.

## Git

Generated schema files are ignored by default; commit tagged releases when schemas are stable for a product version.
