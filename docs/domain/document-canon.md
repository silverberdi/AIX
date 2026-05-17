# AIX Document Canon

## Definition
A document in AIX is a governed business evidence record. A document is not the same thing as a file.

## Core Principles
1. Every document belongs to a DocumentType.
2. The DocumentType is part of the document identity and cannot change after creation.
3. A document may contain multiple physical files.
4. A document may have a primary file and supporting files.
5. The document is an aggregate, not a file.
6. Document identity is separated from operational context.
7. Document versioning and immutability are policy-driven.
8. Audit is structured and event-based.
9. AIX is document-event-driven.
10. Tenant isolation occurs at infrastructure/database level, not inside document entities.

## Identity
Required:
- document_id
- document_type_id
- document_type_version_id
- taxonomy_node_id
- created_at
- created_by

Optional:
- business_document_number

Because AIX uses one database per tenant, tenant_id should not be stored in tenant domain tables.
