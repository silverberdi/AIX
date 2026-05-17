# Slice Status Model

## Purpose

Define the official lifecycle states for AIX implementation slices.

---

# Statuses

## Planned

The slice exists conceptually but:
- is not prioritized for immediate execution
- may still evolve

---

## Ready

The slice:
- has sufficient detail
- has dependencies resolved
- is ready for implementation

---

## In Progress

Implementation is actively occurring.

Only one active implementation owner should exist per slice at a time.

---

## Blocked

The slice cannot progress due to:
- missing dependencies
- unresolved architecture
- external constraints
- failed prerequisite slices

Blocked slices must document:
- blocker reason
- expected unblock condition

---

## Done

The slice:
- satisfies acceptance criteria
- passes tests
- respects architecture rules
- passes validation commands

Done does NOT mean:
- future-proof forever
- immune to future refactors

---

## Deferred

The slice is intentionally postponed.

Reasons may include:
- MVP prioritization
- scalability timing
- operational complexity
- unclear business value

---

## Rejected

The slice or idea has been explicitly discarded.

Rejected slices should preserve:
- rationale
- architectural concerns
- lessons learned

---

# Transition Rules

Typical flow:

Planned
→ Ready
→ In Progress
→ Done

Alternative flows:

Ready
→ Blocked

Planned
→ Deferred

Planned
→ Rejected
