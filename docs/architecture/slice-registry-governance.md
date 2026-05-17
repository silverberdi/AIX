# Slice Registry Governance

## Purpose

The Slice Registry governs how implementation slices are created, organized and executed in AIX.

Slices are:
- implementation-oriented
- dependency-aware
- behavior-focused
- testable
- AI-friendly

Slices are NOT:
- generic backlog tasks
- broad epics
- architecture-free coding prompts

---

# Core Rules

## Slice IDs never change

Example:

slice-001-document-aggregate

IDs are immutable even if:
- slice titles change
- slices move between waves
- scope evolves

---

## Slices must remain small

Preferred size:
- one behavior
OR
- one aggregate capability
OR
- one architectural capability

Avoid:
- “implement metadata system”
- “implement governance”

---

## Explicit Non-Goals are mandatory

Every slice must explicitly define:
- what it implements
- what it intentionally does NOT implement

This prevents:
- scope drift
- accidental architecture
- overengineering

---

## Dependencies are mandatory

Every slice must declare:
- prerequisite slices
- prerequisite capabilities
- blocked dependencies if applicable

---

## Slices must be testable

A slice cannot be considered valid if:
- behavior cannot be tested
- acceptance criteria are ambiguous
- architecture constraints are unclear

---

## Slices are architecture-aware

Each slice must respect:
- bounded context ownership
- naming conventions
- allowed patterns
- testing strategy
- architectural constraints

---

## Slice registry is canonical

The slice registry becomes the canonical execution model for MVP delivery.

Implementation outside the slice registry should be avoided.
