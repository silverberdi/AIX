# Slice 003 — Document State Transitions

## Goal
Implement controlled document state transitions.

## Scope
- Draft → Complete
- immutability after COMPLETE

## Out of Scope
- workflows
- approvals
- policies

## Patterns Used
- Aggregate Root
- Domain Behavior

## Acceptance Criteria
- COMPLETE documents become immutable
- transitions validated inside aggregate
