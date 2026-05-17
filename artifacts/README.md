# Shared Artifacts

Cross-cutting **contracts** consumed by frontend, backend, and runtime tooling. This folder is the single place for machine-readable API and schema definitions.

## Ownership

| Path | Owner | Consumers |
|------|-------|-----------|
| `openapi/` | Backend (export from .NET APIs) | Frontend data-access, codegen, docs |
| `schemas/` | Platform / renderer (JSON Schema) | Angular renderer, validation, tooling |

Human-readable product rules live in `docs/domain/`. **Artifacts are the executable contract layer.**

## Contract-first philosophy

1. **Backend** exposes HTTP APIs; OpenAPI describes the public surface.
2. **Schemas** describe document types, fields, and renderer structure independent of UI code.
3. **Frontend** implements against generated or hand-synced types derived from artifacts — not duplicated ad hoc DTOs.
4. Breaking changes require version bumps and coordinated updates across consumers.

## Generation flow (planned)

```
.NET APIs  ──export──►  artifacts/openapi/
Renderer / domain  ──publish──►  artifacts/schemas/
                              │
                              ▼
                    Frontend libs / runtime validation
```

Commands will be added to `package.json` and backend tooling as pipelines mature. Until then, directories are reserved with README placeholders.

## Boundaries

- Do **not** put application source code here.
- Do **not** commit secrets or environment-specific URLs.
- Generated files may be committed when stable, or ignored via root `.gitignore` until CI publishes them.

## Related documentation

- [Repository structure](../docs/architecture/repository-structure.md)
