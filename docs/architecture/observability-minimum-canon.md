# Observability Minimum Canon

All logs must be structured.

Minimum contextual fields:
- correlationId
- tenantId
- userId
- eventId
- timestamp

Rules:
- no secrets in logs
- no connection strings in logs
- no storage credentials in logs
- tenant isolation must be observable

Initial focus:
- operational debugging
- audit traceability
- runtime diagnostics
