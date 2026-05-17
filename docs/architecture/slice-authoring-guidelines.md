# Slice Authoring Guidelines

## Purpose

Define how implementation slices must be written in AIX.

The goal is:
- consistency
- architecture alignment
- AI-friendly execution
- controlled scope

---

# Required Structure

Every slice must contain:

## Goal
What business or architectural outcome the slice achieves.

---

## Scope
What the slice explicitly implements.

---

## Out of Scope
What the slice intentionally excludes.

This section is mandatory.

---

## Dependencies
Required slices or capabilities.

---

## Touched Projects
Projects/modules expected to change.

Example:
- AIX.Documents
- AIX.SharedKernel
- AIX.Documents.Tests

---

## Domain Behaviors
Expected behaviors or invariants.

---

## Patterns Used
Allowed architectural patterns applied by the slice.

Example:
- Aggregate Root
- Factory Method
- Strategy
- Adapter

---

## Tests Required
Required behavioral or integration tests.

---

## Acceptance Criteria
Conditions required to complete the slice.

---

## Validation Commands
Commands required before completion.

Example:
- dotnet build
- dotnet test
- nx build

---

## Future Considerations
Optional section for:
- future evolution
- deferred concerns
- scalability notes

---

# Authoring Rules

## Prefer behavioral language

Good:
- “Document becomes immutable after COMPLETE”

Bad:
- “Create service layer”

---

## Keep slices small

Preferred:
- one aggregate capability
- one architectural capability
- one clear behavior

Avoid:
- large multi-capability slices

---

## Avoid infrastructure leakage

Slices must not:
- leak EF Core into Domain
- leak HTTP into Domain
- leak Angular concerns into backend

---

## Avoid architecture drift

Slices must follow:
- naming conventions
- testing strategy
- application patterns canon
- bounded context ownership

---

# AI-Friendly Rules

Slices should:
- be explicit
- be deterministic
- define non-goals
- minimize ambiguity
- minimize hidden assumptions

The slice document becomes the primary execution context for AI-assisted implementation.

---

# Anti-Patterns

Avoid:
- vague slices
- implicit dependencies
- missing acceptance criteria
- oversized slices
- implementation-first wording
- technology-first wording
