# Document Relations Canon

## Principle
Relations between documents are directed, typed and configurable by tenant.

## Model
DocumentRelation:
- source_document_id
- relation_type_id
- target_document_id
- created_at
- created_by

## Direction Matters
Invoice GENERATED_FROM PurchaseOrder is not equivalent to PurchaseOrder GENERATED_FROM Invoice.

## MVP Relation Types
- RELATED_TO
- ATTACHED_TO
- GENERATED_FROM
- BELONGS_TO
- SUPERSEDES

## Future
Tenants may create relation types such as SUPPORTS, REPLACES, REFERENCES, EVIDENCES, PART_OF.
Future versions may support AI-suggested relations and semantic document graphs.
