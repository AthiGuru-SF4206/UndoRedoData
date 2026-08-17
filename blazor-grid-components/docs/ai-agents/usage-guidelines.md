# AI Agent Usage Guidelines — Syncfusion Blazor Data Grid

**Document Version**: 1.0.0  
**Last Updated**: March 12, 2026  
**Owner**: Architect AI  
**Audience**: Development Team, QA Engineers, Team Leads

---

## Table of Contents

1. [How to Request Work from Agents](#how-to-request-work-from-agents)
2. [Request Templates](#request-templates)
   - [Feature Request](#feature-request-template)
   - [Bug Fix Request](#bug-fix-request-template)
   - [Code Review Request](#code-review-request-template)
   - [Documentation Request](#documentation-request-template)
   - [Test Authoring Request](#test-authoring-request-template)
   - [Performance Optimization Request](#performance-optimization-request-template)
   - [Accessibility Review Request](#accessibility-review-request-template)
3. [Approval Workflow Diagrams](#approval-workflow-diagrams)
4. [Escalation Triggers & Process](#escalation-triggers--process)
5. [Common Issues & Solutions](#common-issues--solutions)
6. [Best Practices for Working with Agents](#best-practices-for-working-with-agents)

---

## How to Request Work from Agents

### General Principles

All agent requests must follow these rules before submission:

1. **Architect AI scopes the task first** — no agent receives an unscoped request.
2. **Provide exact file references** — agents work only on excerpts provided.
3. **Include task ID** — every request links to a DevOps task or BoldDesk ticket.
4. **Specify affected module** — helps agents avoid scope creep.
5. **List known regression risks** — agents validate but requester must list knowns.
6. **Attach source excerpts** — agents do not browse the full source independently.

### Request Routing Table

| Request Type | Primary Agent | Validation Agent | Approval Agent |
|---|---|---|---|
| New feature | Scrum Master → Dev | Code Review + Test | Architect AI |
| Bug fix | Bug Fix Agent | Code Review + Test | Scrum Master |
| Code review | Code Review Agent | Architect AI (API risk) | Scrum Master |
| Documentation | Documentation Agent | Code Review (XML) | Scrum Master |
| Test authoring | Test Agent | Code Review | Scrum Master |
| Performance issue | Performance Agent | Code Review | Architect AI |
| Accessibility issue | Accessibility Agent | Code Review | Scrum Master |

---

## Request Templates

---

### Feature Request Template

Use this template when requesting implementation of a new feature or enhancement.

```markdown
## Feature Request

### Task Reference
- DevOps Task  : https://dev.azure.com/EssentialStudio/Ej2-Web/_workitems/edit/[TASK-ID]
- Ticket       : https://es-testingportal.bolddesk.com/agent/tickets/[TICKET-ID]

### Feature Name
[Short descriptive name, e.g., "Column Reorder with Touch Support"]

### Affected Module
[e.g., Renderer/ColumnReorder, Actions/Reorder]

### Source Files to Modify
- `Internal/Actions/[FileName].cs` — [reason]
- `Internal/Renderer/[FileName].cs` — [reason]
- `sf-grid.js` — [reason if JS-interop change]

### Feature Description
[2-5 sentences describing the feature, user workflow, and expected outcome]

### Acceptance Criteria
- [ ] [Criterion 1]
- [ ] [Criterion 2]
- [ ] [Criterion 3]

### Ensured Related Features (Must Not Break)
- [Feature 1, e.g., "Column Freeze must still work after reorder"]
- [Feature 2]
- [Feature 3]

### Regression Risks
- [Risk 1 — e.g., "Column index mapping may affect Export module"]
- [Risk 2]

### Required Tests
- BUnit : [Test scenario description]
- Playwright : [UI interaction test description]

### Performance Expectations
- [e.g., "Reorder operation must complete in < 50 ms for 30 columns"]

### Accessibility Requirements
- [e.g., "Reorder via keyboard (Alt+Left/Right) must be supported"]

### API Changes
- [ ] New API added (requires Architect AI + API Review approval)
- [ ] Existing API modified (requires Architect AI approval)
- [X] No API changes
```

---

### Bug Fix Request Template

Use this template when requesting root cause analysis and a fix for a defect.

```markdown
## Bug Fix Request

### Task Reference
- DevOps Task  : https://dev.azure.com/EssentialStudio/Ej2-Web/_workitems/edit/[TASK-ID]
- Ticket       : https://es-testingportal.bolddesk.com/agent/tickets/[TICKET-ID]

### Bug Summary
[One-line description of the defect]

### Steps to Reproduce
1. [Step 1]
2. [Step 2]
3. [Step 3 — observe the issue]

### Expected Behavior
[What should happen]

### Actual Behavior
[What actually happens — include error messages, screenshots if available]

### Affected Module
[e.g., Internal/Actions/Edit.cs, Internal/Renderer/VirtualContent.cs]

### Source Excerpt
[Paste the relevant code excerpt — agents work only on provided excerpts]

### Known Regression Risks
- [Risk 1 — e.g., "Virtualization row recycling may be affected"]
- [Risk 2]

### EJ2 Parity Check
- [ ] Issue also present in EJ2 JavaScript version
- [ ] EJ2 fix already exists (link: ___)
- [ ] Not applicable

### Required Fix Constraints
- Must NOT modify public API
- Must NOT change default behavior without approval
- Fix must be minimal footprint (least code change)

### Required Tests
- BUnit  : [Regression test for this scenario]
- Playwright : [UI repro test if applicable]

### Impact Assessment
- [ ] Low — Single feature, minimal user impact
- [ ] Medium — Multiple features or moderate impact
- [ ] High — Critical functionality or major user impact
```

---

### Code Review Request Template

Use this template when requesting a code review of a completed change.

```markdown
## Code Review Request

### Task Reference
- DevOps Task  : https://dev.azure.com/EssentialStudio/Ej2-Web/_workitems/edit/[TASK-ID]
- PR Link      : [GitHub / Azure DevOps PR URL]

### Change Summary
[2-3 sentences describing what was changed and why]

### Files Modified
- `[File 1]` — [What changed]
- `[File 2]` — [What changed]

### Source Excerpts
[Paste modified code blocks — reviewer works on provided excerpts only]

### Checklist (Requester Self-Review)
- [ ] No public API changes
- [ ] XML comments added/updated for all modified public members
- [ ] No `.NET` analyzer warnings introduced
- [ ] Variable naming follows `coding-standards.md`
- [ ] Error handling patterns followed
- [ ] No commented-out code
- [ ] No magic numbers (constants used)
- [ ] JS-interop changes follow established patterns

### Known Regression Risks
- [Risk 1]
- [Risk 2]

### Review Focus Areas
[e.g., "Focus on the null-check in line 47 and the disposal pattern in line 89"]

### Urgency
- [ ] Blocking (hotfix / production issue)
- [ ] Normal (feature / scheduled bugfix)
- [ ] Low (cleanup / docs only)
```

---

### Documentation Request Template

Use this template when requesting documentation creation or updates.

```markdown
## Documentation Request

### Task Reference
- DevOps Task  : [Optional]
- Linked Feature/Bug : [Feature name or Bug ID]

### Documentation Type
- [ ] New feature guide (create `/docs/requirements/features/[feature-name]/`)
- [ ] Bug analysis docs (create `/docs/requirements/bugs/[bug-id]/`)
- [ ] Architecture update (update `/docs/architecture/`)
- [ ] API reference update
- [ ] XML comments update
- [ ] Training material update (`/docs/training/`)
- [ ] Glossary update

### Target Files
- `[docs/path/file.md]` — [What needs to be created/updated]

### Source Material
[Paste source code, PR description, or feature spec to base docs on]

### Audience
- [ ] Freshers / new developers
- [ ] Experienced developers
- [ ] Team leads / architects

### Documentation Standards
- File naming: kebab-case
- Code examples: ❌ WRONG vs ✅ CORRECT format
- Internal links: relative paths only
- All code examples must compile and run

### Validation Required
- [ ] Code Review Agent review of XML comments
- [ ] Architect AI review of architectural docs
- [ ] Scrum Master sign-off
```

---

### Test Authoring Request Template

Use this template when requesting test case authoring (BUnit or Playwright).

```markdown
## Test Authoring Request

### Task Reference
- DevOps Task  : https://dev.azure.com/EssentialStudio/Ej2-Web/_workitems/edit/[TASK-ID]
- Related Fix/Feature : [Bug ID or Feature Name]

### Test Type
- [ ] BUnit unit tests
- [ ] Playwright integration / UI tests
- [ ] Both

### Feature / Bug Under Test
[Description of the behavior being tested]

### Source Excerpt (Code Under Test)
[Paste the relevant implementation excerpt]

### Test Scenarios Required

#### Happy Path
- [ ] [Scenario 1 — e.g., "Sort ascending on string column with 1000 rows"]
- [ ] [Scenario 2]

#### Edge Cases
- [ ] [Edge 1 — e.g., "Sort with null values in column"]
- [ ] [Edge 2 — e.g., "Sort on grouped grid"]
- [ ] [Edge 3 — e.g., "Sort with frozen columns enabled"]

#### Regression Scenarios
- [ ] [Previously broken scenario 1]
- [ ] [Previously broken scenario 2]

### Test Naming Convention
`Should_[ExpectedBehavior]_When_[Condition]`

Example:
```csharp
// Should_SortAscending_When_ColumnHeaderClicked
// Should_PreserveSortOrder_When_FilterApplied
// Should_MaintainFocus_When_SortTriggeredViaKeyboard
```

### Coverage Expectations
- [ ] Happy path covered
- [ ] All edge cases listed above covered
- [ ] Regression scenarios covered
- [ ] Performance assertion included (if applicable)
- [ ] Accessibility assertion included (keyboard, ARIA)
```

---

### Performance Optimization Request Template

Use this template when requesting performance analysis or optimization.

```markdown
## Performance Optimization Request

### Task Reference
- DevOps Task  : [Optional]
- Issue Description : [What is slow or leaking]

### Performance Problem Statement
[Describe the observed performance issue with data: rows, columns, operations]

### Profiling Data (if available)
[Paste profiling output, trace, or benchmark results]

### Affected Module
[e.g., Internal/Renderer/VirtualContent.cs, sf-grid.js scroll handler]

### Source Excerpt
[Paste relevant code — agent works on provided excerpt only]

### Current vs Target Benchmark
| Metric | Current | Target |
|--------|---------|--------|
| Render time (500×10) | [X ms] | < 200 ms |
| Scroll FPS | [X FPS] | ≥ 60 FPS |
| Memory (after 1000 rows) | [X MB] | [Target MB] |

### Known Optimization Constraints
- [e.g., "Cannot change public virtualization API"]
- [e.g., "Must remain compatible with Server-side Blazor"]

### Regression Risks
- [Risk 1]
- [Risk 2]

### Validation Required
- [ ] Before/after profiling comparison
- [ ] No functional regression
- [ ] Memory leak check (DisposeAsync audit)
```

---

### Accessibility Review Request Template

Use this template when requesting accessibility validation.

```markdown
## Accessibility Review Request

### Task Reference
- DevOps Task  : [Optional]
- Related Feature/Fix : [Feature name or Bug ID]

### Review Scope
[Describe the UI area or feature to be reviewed]

### WCAG Target
- [X] WCAG 2.1 Level AA (required baseline)
- [ ] WCAG 2.1 Level AAA (enhanced)

### Source Excerpt
[Paste relevant Razor markup and C# code — agent works on provided excerpt]

### Areas to Validate
- [ ] ARIA roles and attributes (`role`, `aria-label`, `aria-describedby`)
- [ ] Keyboard navigation (Tab, Arrow, Enter, Escape, Space)
- [ ] Focus management (after open/close/edit/delete)
- [ ] Screen reader announcements (`aria-live`, dynamic content)
- [ ] Color contrast ratios
- [ ] Localized accessibility labels

### Known Issues (if any)
[List any known accessibility issues in this area]

### Regression Risks
- [e.g., "Tab order change may affect sequential navigation through filter row"]

### Test Scenarios Required
- [ ] Full keyboard-only navigation through the feature
- [ ] Screen reader announcement on state change
- [ ] Focus restoration after dialog close
- [ ] ARIA attribute validation (present and accurate)
```

---

## Approval Workflow Diagrams

### Feature Implementation Workflow

```
Developer submits Feature Request
            │
            ▼
    Architect AI scopes sub-tasks
            │
            ▼
    Scrum Master validates scope
    (Phase 1: Requirements ✅)
            │
            ▼
    Test Agent writes test cases first
    (Phase 3: Unit Test Cases ✅ — TDD gate)
            │
            ▼
    Developer implements feature
    (Phase 4: Development)
            │
            ▼
    ┌───────────────────────────────┐
    │   Parallel Validation         │
    │  • Code Review Agent          │
    │  • Performance Agent          │
    │  • Accessibility Agent        │
    └───────────────────────────────┘
            │
            ▼
    All agents APPROVE?
    ┌──────────────────────────────┐
    │ YES                 NO       │
    │  ▼                  ▼       │
    │ Scrum Master    Return to    │
    │ Phase 6 ✅      Developer    │
    └──────────────────────────────┘
            │ (if approved)
            ▼
    Documentation Agent updates docs
    (Phase 6: Review ✅)
            │
            ▼
    Architect AI final approval
            │
            ▼
    Scrum Master merge approval
    (Phase 7: Merge ✅)
```

### Bug Fix Workflow

```
Bug reported (DevOps Task / BoldDesk Ticket)
            │
            ▼
    Architect AI identifies affected module
            │
            ▼
    Bug Fix Agent: Root cause analysis
            │
    Creates: /docs/requirements/bugs/[id]/
    • description.md
    • root-cause.md
    • fix-approach.md
            │
            ▼
    Test Agent writes regression test
            │
            ▼
    Bug Fix Agent implements fix
            │
            ▼
    Code Review Agent validates
    (API safe? Standards? Minimal footprint?)
            │
            ▼
    APPROVED?
    ┌──────────────────────────────┐
    │ YES                 NO       │
    │  ▼                  ▼       │
    │ Scrum Master    Return to    │
    │ PR approval     Bug Fix      │
    │                 Agent        │
    └──────────────────────────────┘
            │ (if approved)
            ▼
    EJ2 parity check
    (Create task for EJ2 if needed)
            │
            ▼
    Merge to develop branch
```

---

## Escalation Triggers & Process

### When to Escalate

| Situation | Escalation Path | Action Required |
|---|---|---|
| API breaking change detected | Code Review → Architect AI | STOP — no merge until approved |
| Behavior change without test | Code Review → Test Agent + Scrum Master | Add test before proceeding |
| >10% performance regression | Performance Agent → Architect AI | Root cause analysis required |
| WCAG 2.1 AA violation | Accessibility Agent → Code Review | Fix before merge |
| Unknown architectural behavior | Any Agent → Architect AI | Request source docs |
| Scope creep in fix | Code Review → Scrum Master | Re-scope to minimal fix |
| Circular dependency introduced | Code Review → Architect AI | Dependency map review |
| JS-interop pattern deviation | Code Review → Architect AI | Pattern alignment required |
| Fix does not resolve root cause | Bug Fix Agent → Architect AI | Deeper analysis needed |
| Missing documentation | Documentation Agent → Scrum Master | Docs required before merge |

### Escalation Communication Format

```
ESCALATION NOTICE
-----------------
Agent       : [Agent Name / ID]
Task ID     : [DevOps/BoldDesk ID]
Trigger     : [One-line description of why escalating]
Affected    : [Module/File]
Risk Level  : Low | Medium | High | Blocking
Evidence    : [Code excerpt, benchmark data, or test output]
Requested   : [What decision or action is needed from escalation target]
```

---

## Common Issues & Solutions

### Issue 1: Agent Produces Fix Outside Scoped Excerpt

**Problem**: Bug Fix Agent modifies a file that was not in the scoped excerpt.  
**Solution**: Reject the output. Re-scope with explicit file boundaries. Remind agent: "Work only on the provided excerpt."

---

### Issue 2: Code Review Agent Misses an API Breaking Change

**Problem**: A parameter was renamed in a public method but review passed.  
**Solution**: Escalate immediately to Architect AI. Revert the change. All public method signatures require Architect AI sign-off before any modification.

---

### Issue 3: Test Agent Writes Tests After Code Is Merged

**Problem**: TDD gate was bypassed — tests written after implementation.  
**Solution**: Scrum Master blocks future PRs from this developer until TDD workflow is followed. Test Agent must write tests in Phase 3, before Phase 4 (Development).

---

### Issue 4: Documentation Agent Generates Inaccurate XML Comments

**Problem**: XML `<summary>` describes wrong behavior or references wrong parameters.  
**Solution**: Code Review Agent must validate XML comments. Documentation Agent re-generates with corrected source excerpt. Accuracy is mandatory — incorrect XML comments are a blocking issue.

---

### Issue 5: Performance Agent Reports Regression But Cause is Unknown

**Problem**: Benchmark shows 15% regression but source is unclear.  
**Solution**: Escalate to Architect AI with profiling data. Architect AI identifies the module. Performance Agent investigates the specific scoped area.

---

### Issue 6: Accessibility Agent Flags ARIA Issue in Third-Party Dependency

**Problem**: Missing `aria-label` is inside a Syncfusion base component, not the Grid.  
**Solution**: Document the issue. Raise a task for the affected base component team. Note in PR that this is a known limitation tracked separately.

---

### Issue 7: Multiple Agents Provide Conflicting Feedback

**Problem**: Code Review approves a fix but Performance Agent rejects it.  
**Solution**: Escalate to Scrum Master for conflict resolution. Scrum Master arbitrates and may escalate to Architect AI if architectural decision is needed.

---

### Issue 8: JS-Interop Module Disposed Before All Callbacks Complete

**Problem**: `DisposeAsync` is called but a pending JS-to-.NET callback fires after disposal.  
**Solution**: Bug Fix Agent must implement a disposal guard flag (`_isDisposed`). Performance Agent validates the guard is in place. Code Review validates the pattern matches `component-architecture.md`.

---

## Best Practices for Working with Agents

### ✅ DO

```
✅ Always provide exact file paths and line references
✅ Include the DevOps task ID or BoldDesk ticket ID in every request
✅ Paste the source excerpt — agents do not browse files autonomously
✅ List all known regression risks upfront
✅ Follow the TDD gate — write tests before submitting for development
✅ Use the structured request templates for every task type
✅ Escalate early when uncertainty arises — do not guess
✅ Reference the relevant /docs files when asking for behavior clarification
✅ Check EJ2 JavaScript parity for every Grid bug fix
✅ Validate accessibility keyboard navigation on every UI change
```

### ❌ DON'T

```
❌ Submit unscoped requests ("fix the grid's performance")
❌ Ask agents to browse source files independently
❌ Allow API changes without Architect AI approval
❌ Merge without all agent gates passing
❌ Bypass the TDD gate ("tests will come in the next PR")
❌ Ask multiple agents to work on the same file simultaneously
❌ Accept a fix that uses innerHTML = '' to clear wrapper elements
❌ Allow "temporary" commented-out code to enter the source
❌ Skip the EJ2 parity check on bug fixes
❌ Ignore performance regression signals ("it's only 12% slower")
```

### Working with the Architect AI

The Architect AI is the **single source of architectural truth**. Before any significant change:

1. **Confirm readiness** — Architect AI reviews and scopes the task
2. **Request source docs** — if behavior is uncertain, ask Architect AI before coding
3. **Never guess** — uncertain behavior = escalate, not assume
4. **API decisions are final** — Architect AI approvals cannot be overridden
5. **Regression-sensitive areas** — always listed by Architect AI in the task scope

### Providing Effective Source Excerpts

```
✅ Good Excerpt Request:
"Here is the excerpt from Internal/Actions/Edit.cs, lines 245-310,
specifically the SaveCell() method and its interaction with
VirtualContentRenderer.RefreshVirtualElement()..."

[paste excerpt]

❌ Bad Excerpt Request:
"The editing is broken. Can you fix it?"
```

### Task Sizing for Agents

| Task Size | Description | Max Lines Changed | Agents Involved |
|---|---|---|---|
| XS | Single null-check or guard | < 5 lines | Bug Fix + Code Review |
| S | Single method fix | 5–30 lines | Bug Fix + Test + Code Review |
| M | Single feature module | 30–150 lines | All 7 agents |
| L | Cross-module feature | 150–500 lines | All 7 agents + Architect AI review |
| XL | Architectural change | 500+ lines | Architect AI leads — full team |

---

## Quick Reference — Request Checklist

Before submitting any agent request, confirm:

- [ ] Task ID (DevOps / BoldDesk) included
- [ ] Architect AI has scoped the task
- [ ] Exact source file paths provided
- [ ] Source excerpt attached (not just a description)
- [ ] Affected module named
- [ ] Known regression risks listed
- [ ] Ensured related features listed
- [ ] Required tests described
- [ ] API change check done (✅ no API change OR ⚠️ escalated to Architect AI)
- [ ] Correct request template used

---

*This document is part of the Syncfusion Blazor Data Grid AI Collaboration Framework.*  
*See also: [agents-overview.md](./agents-overview.md) | [development-workflow.md](../dev-process/development-workflow.md) | [pr-guidelines.md](../dev-process/pr-guidelines.md)*
