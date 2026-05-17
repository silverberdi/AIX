# Reference Data Canon

## Principles

1. Keywords may use datasets.
2. Datasets may be manual, external or hybrid.
3. Dataset relationships are declarative.
4. Cascading selections are configuration-driven.
5. Datasets are reusable.
6. Datasets are tenant-scoped.

## Dataset Types

### Manual Dataset
Tenant-managed data inside AIX.

Examples:
- cost centers
- offices
- internal categories

### External Dataset
AIX queries external systems.

Examples:
- medical catalogs
- external providers
- ERP lookups

### Hybrid Dataset
AIX synchronizes/caches external data locally.

Examples:
- ERP suppliers synchronized daily

## Keyword Integration

Keywords may reference datasets.

Example:
Keyword
- data_type
- ui_type
- datasource_id

## Cascading Relationships

Datasets may define cascading relationships.

Examples:
- country -> department -> city
- specialty -> diagnosis -> medication

## Dataset Metadata

Datasets may contain:
- source_type
- sync_strategy
- last_sync_at
- version
- status

## MVP Scope

The MVP prioritizes:
- manual datasets
- keyword datasource support
- basic cascading datasets

Advanced external synchronization may arrive later.
