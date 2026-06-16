# AIX MVP Slice Registry

| Slice | Wave | Status | Dependencies |
|---|---|---|---|
| slice-001-document-aggregate | Wave 0 | Done | none |
| slice-002-sharedkernel-primitives | Wave 0 | Done | none |
| slice-003-document-state-transitions | Wave 0 | Done | slice-001 |
| slice-004-document-files | Wave 0 | Done | slice-001 |
| slice-005-document-type-aggregate | Wave 1 | Done | slice-001 |
| slice-006-document-type-versioning | Wave 1 | Done | slice-005 |
| slice-007-keyword-registry | Wave 1 | Done | slice-005 |
| slice-008-keyword-groups | Wave 1 | Done | slice-007 |
| slice-009-keyword-validation | Wave 1 | Done | slice-007, slice-008 |
| slice-010-field-schema-model | Wave 2 | Done | slice-007, slice-009 |
| slice-011-version-schema-composition | Wave 2 | Done | slice-006, slice-010 |
| slice-012-version-keyword-group-assignment | Wave 2 | Done | slice-008, slice-011 |
| slice-013-layout-schema-model | Wave 2 | Done | slice-011, slice-012 |
| slice-014-renderer-contract-preparation | Wave 2 | Done | slice-013 |
| slice-015-schema-validation-integration | Wave 2 | Done | slice-009, slice-012, slice-014 |
| slice-016-capture-metadata-contracts | Wave 3 | Done | slice-015 |
| slice-017-document-metadata-attachment | Wave 3 | Done | slice-016 |
| slice-018-capture-validation-port | Wave 3 | Pending | slice-016, slice-017, slice-015 |
| slice-019-document-completion-readiness | Wave 3 | Pending | slice-004, slice-017, slice-018 |
| slice-020-complete-with-capture-enforcement | Wave 3 | Pending | slice-019 |
| slice-021-capture-context-contract | Wave 3 | Pending (optional, last priority) | slice-014, slice-016 |

## Wave folders

| Wave | Folder |
|------|--------|
| Wave 0 — Foundation | `ai/backlog/mvp/wave-0-foundation/` |
| Wave 1 — Document Type System | `ai/backlog/mvp/wave-1-document-type-system/` |
| Wave 2 — Metadata & Layouts | `ai/backlog/mvp/wave-2-metadata-layouts/` |
| Wave 3 — Capture MVP | `ai/backlog/mvp/wave-3-capture/` |

**Wave 3 notes:** Core capture path is slices 016–020. Slice 021 is last-priority and optional within Wave 3; required only before future API/UI renderer work.
