# Project Overview

Axioma is an intelligent document management platform with dynamic document composition, multi-tenancy, and hybrid storage capabilities.

The system focuses on structured data and declarative form rendering.

OCR and AI are optional future capability layers. **AIX v1.0 is AI-ready but not AI-dependent**—governance, capture, and structured search are in scope; advanced AI runtime, semantic search, and advanced OCR are not.

## Backend shape

- **APIs:** `AIX.Platform.Api` (platform), `AIX.Business.Api` (tenant business)
- **Bounded contexts:** `AIX.Documents`, `AIX.Metadata`, `AIX.Governance`, `AIX.Storage`, `AIX.Search`, `AIX.Workflow`, `AIX.Security`, `AIX.ReferenceData`, `AIX.Integrations`, `AIX.Platform`
- **Shared:** `AIX.SharedKernel` (primitives only), `AIX.Infrastructure` (composition / shared adapters)

Each bounded context uses `Domain/`, `Application/`, `Infrastructure/`, `Contracts/`, `Events/`.
