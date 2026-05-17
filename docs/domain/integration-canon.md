# Integration Canon

## Principle

AIX does not replace external systems.

AIX integrates with them to:
- receive documents
- enrich metadata
- expose evidence
- automate document-driven interactions

## Integration Types

### Inbound Integrations
External systems create/send documents to AIX.

### Outbound Integrations
AIX emits notifications or delivers evidence externally.

### Lookup Integrations
AIX retrieves external reference data for metadata enrichment.

### Automation Integrations
AIX triggers external actions using document events.

## MVP Integrations

The MVP supports:
- email ingestion
- external APIs
- scanners
- storage providers
- identity providers

Future iterations may include:
- ERP integrations
- BPM integrations
- BI integrations
- advanced automation

## Integration Architecture

Integrations are adapters, not core domain logic.

Core Domain
↑
Integration Contracts
↑
Adapters

## API Philosophy

AIX APIs are document-centric.

Examples:
- POST /documents
- GET /documents/{id}
- POST /documents/{id}/files
- POST /documents/{id}/relations

## Async vs Sync

Synchronous:
- uploads
- metadata queries
- auth validation

Asynchronous:
- OCR
- notifications
- webhooks
- future ERP synchronization

## Event Exposure

Internal events are not exposed directly.

Internal Events
↓
Integration Mapping
↓
External Webhooks

## Tenant Scope

Integrations are tenant-scoped.

Each tenant manages:
- credentials
- providers
- mailboxes
- tokens
- integration endpoints
