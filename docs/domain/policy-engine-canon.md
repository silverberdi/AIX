# Policy Engine Canon

Policies are declarative, typed, reusable, auditable and versionable.

Policies are not executable code.

Policy types:
- RETENTION
- IMMUTABILITY
- OCR
- SEARCH_VISIBILITY
- VERSIONING
- SECURITY
- FILE_REQUIREMENT

Policy states:
- DRAFT
- ACTIVE
- DISABLED
- ARCHIVED

Only ACTIVE policies may be assigned to new DocumentTypes.

Base system policies are immutable templates.
Tenants cannot modify base policies directly.

Documents preserve references/snapshots to the policies applied when governed.
