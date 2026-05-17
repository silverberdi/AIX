# Slice 006 — DocumentType Versioning

## Goal
Implement immutable document type versioning.

## Scope
- create new versions
- preserve previous versions
- immutable version references

## Out of Scope
- migrations
- retroactive updates

## Patterns Used
- Aggregate Root
- Factory Method

## Acceptance Criteria
- versions immutable
- documents preserve referenced versions
