# DocumentType Versioning Canon

## Principle
DocumentTypes are versioned.

Every document is linked to the exact DocumentType version used at creation time.

## Rules
- Structural changes create a new version.
- Existing documents remain bound to their original version.
- Documents do not point only to the current live DocumentType.
- The renderer must be version-aware.

## Allowed Evolution
DocumentTypes may add:
- fields
- groups
- tables
- layout sections
- rules
- policies

## Forbidden Evolution
DocumentTypes must not delete historical fields. Fields may be deprecated or hidden from new captures.

## Keyword Compatibility
Keywords are not versioned in the MVP.

Allowed:
- increasing text length
- increasing numeric ranges
- relaxing validations
- changing label or description

Forbidden:
- changing data_type
- reducing allowed length
- reducing numeric range
- stricter validation that breaks existing data
- deleting a keyword in use

## Metadata Storage
Document metadata is stored as JSONB validated against the DocumentTypeVersion.
