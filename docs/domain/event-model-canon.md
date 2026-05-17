# Event Model Canon

Business events drive behavior.
Audit events preserve evidence.

Read access auditing is policy-driven.
Modification events are always audited.

Events are:
- immutable
- append-only
- structured
- machine-readable

Event structure should include:
- event_id
- event_type
- aggregate_type
- aggregate_id
- actor_id
- occurred_at
- correlation_id
- causation_id
- payload

Tenant events live inside tenant databases.
