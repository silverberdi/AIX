## 1. Wave 3 backlog structure

- [x] 1.1 Create `ai/backlog/mvp/wave-3-capture/README.md` with wave theme, slice order, and deferred items
- [x] 1.2 Verify slice dependency graph matches `design.md`

## 2. Slice definition files (016–021)

- [x] 2.1 Create `ai/backlog/mvp/wave-3-capture/slice-016-capture-metadata-contracts.md`
- [x] 2.2 Create `ai/backlog/mvp/wave-3-capture/slice-017-document-metadata-attachment.md`
- [x] 2.3 Create `ai/backlog/mvp/wave-3-capture/slice-018-capture-validation-port.md`
- [x] 2.4 Create `ai/backlog/mvp/wave-3-capture/slice-019-document-completion-readiness.md`
- [x] 2.5 Create `ai/backlog/mvp/wave-3-capture/slice-020-complete-with-capture-enforcement.md`
- [x] 2.6 Create `ai/backlog/mvp/wave-3-capture/slice-021-capture-context-contract.md`

## 3. Registry and task pointer updates

- [x] 3.1 Update `ai/backlog/mvp/index.md` with Wave 3 slice rows (016–021, status Pending)
- [x] 3.2 Update `ai/tasks/current-task.md` to point at slice-016 as the next active slice

## 4. Planning verification

- [x] 4.1 Confirm each slice file includes Goal, Scope, Out of Scope, Dependencies, Allowed Bounded Context, Acceptance Criteria, Testing Expectations, Definition of Done, and Validation Commands
- [x] 4.2 Confirm no slice includes persistence, EF Core, repositories, APIs, MediatR, Angular UI, PrimeNG, Avalon, runtime multi-tenancy, storage providers, OCR, search, retention, or workflows
- [x] 4.3 Confirm OpenSpec delta specs under `openspec/changes/wave-3-capture-mvp-planning/specs/` align with slice acceptance criteria

## 5. Handoff

- [x] 5.1 Run `openspec status --change wave-3-capture-mvp-planning` and confirm apply-ready
- [x] 5.2 Do **not** run `/opsx:apply` for this planning change — implementation begins with a new OpenSpec change per slice (starting with slice-016)

**Note:** This tasks list governs the planning change itself. Individual slice implementation tasks will be created when each slice becomes its own OpenSpec apply change.
