# AIX Capability Dependency Map

## Foundation Layer
- Platform Runtime
- Security & Authorization
- Document Core

## Metadata Layer
Metadata System depends on:
- Document Core

## UX Operational Layer
Renderer & UX Engine depends on:
- Metadata System
- Security

Capture System depends on:
- Document Core
- Metadata
- Renderer
- Storage

Search & Retrieval depends on:
- Document Core
- Metadata
- Security

## Governance Layer
Governance depends on:
- Document Core
- Metadata
- Security
- Search

## Integration Layer
Storage depends on:
- Platform Runtime
- Security

Integration Platform depends on:
- Security
- Documents
- Search
- Governance

## Intelligence Layer
AI Foundation depends on:
- Metadata
- Search
- Governance
- Storage
