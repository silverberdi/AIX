# Slice 001 — Document Aggregate

## Goal
Implement the foundational Document aggregate.

## Scope
- Document aggregate
- immutable document type references
- draft initial state
- createdAt handling
- strongly typed IDs

## Out of Scope
- persistence
- APIs
- EF Core
- MediatR
- UI
- uploads

## Touched Projects
- AIX.Documents
- AIX.SharedKernel
- AIX.Documents.Tests

## Patterns Used
- Aggregate Root
- Value Object
- Factory Method

## Tests Required
- creates_document_successfully
- document_initial_state_is_draft
- document_has_created_at_from_clock
- document_requires_document_type
- document_requires_document_type_version
- document_type_cannot_change
- document_type_version_cannot_change

## Validation Commands
```bash
cd backend && dotnet restore AIX.sln && dotnet build AIX.sln && dotnet test AIX.sln
```

## Acceptance Criteria
- build passes
- tests pass
- no public setters on aggregate identity fields
