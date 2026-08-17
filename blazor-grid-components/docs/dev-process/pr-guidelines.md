# PR Guidelines — Syncfusion Blazor DataGrid

> **Audience**: All developers, AI agents submitting PRs
> **Prerequisite**: [`development-workflow.md`](./development-workflow.md)
> **Last Updated**: March 11, 2026

---

## PR Template

Every pull request targeting the Blazor DataGrid must use the following template exactly.

```
### Bug / Feature Description

[One-line summary]
Task: https://dev.azure.com/EssentialStudio/Ej2-Web/_workitems/edit/<ID>
Ticket: https://es-testingportal.bolddesk.com/agent/tickets/<ID>

### Root Cause (Bug fixes only)

[Why the defect existed — specific class, method, condition]

### Solution Description

[What was changed and why it solves the root cause]

### AI Log Details (if Code Studio was used)

Root Cause Identification: ...
Why This is Wrong: ...
The Fix: ...

### Code Studio Usage (Mandatory)

* Code Studio used in this PR?
    - [ ] Yes
    - [ ] No
* Primary use (choose one):
    - [ ] Generate new code
    - [ ] Refactor/improve existing code
    - [ ] Tests
    - [ ] Bug fix / debugging help
    - [ ] Docs / comments
    - [ ] Review assistance
* Outcome:
    - [ ] Saved time
    - [ ] Neutral
    - [ ] Cost time

### Impact Assessment

* [ ] Low  — Single feature, minimal user impact
* [ ] Medium — Multiple features, moderate user impact
* [ ] High  — Critical functionality, significant user impact

### Areas Tested

* [ ] Tested using standard test cases
* [ ] Tested against feature matrix
* [ ] NA

### Breaking Changes

* [ ] Yes (Tag `breaking-issue`, provide migration guidance)
* [ ] No

### Regression Testing

* [ ] Verified fix does not reintroduce previous bugs
* [ ] Checked edge cases and error scenarios

### Action to Prevent Recurrence

* [ ] Added/updated unit tests (BUnit)
* [ ] Added Playwright automation
* [ ] Other (specify):
* [ ] NA

### Cross-Platform Verification

* [ ] Blazor Server
* [ ] Blazor WebAssembly
* [ ] NA

### Related Issues

* [ ] Resolved in EJ2 (PR link: ___)
* [ ] Created task for EJ2 (Task link: ___)
* [ ] Needs attention in other components
* [ ] NA

### API Changes

* [ ] New API added (API Review task link: ___)
* [ ] Existing API renamed/modified (API Review task link: ___)
* [ ] No API changes

### Performance Verification

* [ ] Verified no memory leaks introduced
* [ ] Verified no performance degradation
* [ ] Not applicable

### Reviewer Checklist

* [ ] Code Studio usage information reviewed
* [ ] Code changes follow component guidelines
* [ ] All provided information reviewed and verified
* [ ] Solution addresses the root cause effectively
```

---

## Approval Criteria Gates

Every PR must satisfy all gates before merge approval.

| Gate | Requirement |
|------|-------------|
| **Correctness** | Reported defect or feature fully addressed |
| **No API Break** | Zero public API signature changes without review task |
| **Build** | Zero Analyzer warning errors in CI |
| **Test Coverage** | BUnit or Playwright test linked or NA justified |
| **No Regression** | Cross-feature combinations verified |
| **CSS Contract** | No unintended class name or style changes |
| **Accessibility** | Keyboard navigation and ARIA attributes unaffected |
| **Memory** | No new disposable objects left undisposed |

---

## Code Review Checklist

Reviewers must verify each item before approving.

### Correctness
- [ ] Root cause is accurately identified
- [ ] Fix addresses the root cause, not just the symptom
- [ ] Edge cases are handled (null, empty, large dataset)
- [ ] No logic errors or off-by-one issues

### Code Quality
- [ ] Follows [`coding-standards.md`](../code-guidelines/coding-standards.md)
- [ ] Follows [`naming-conventions.md`](../code-guidelines/naming-conventions.md)
- [ ] No `any`-equivalent usage (no untyped `object` casts without justification)
- [ ] Async methods use `ConfigureAwait(true)` consistently
- [ ] XML comments accurate and complete for all public members
- [ ] No orphaned comment lines or debug traces

### Architecture
- [ ] Change is scoped to correct module(s)
- [ ] No new circular dependencies introduced
- [ ] JS-interop changes follow the unified dispatcher pattern
- [ ] Disposal implemented for any new `IDisposable` objects

### Performance
- [ ] No new synchronous blocking calls in render path
- [ ] No unnecessary `StateHasChanged` triggers
- [ ] Virtualization row recycling not broken
- [ ] No memory leaks (event subscriptions unsubscribed on dispose)

### Accessibility
- [ ] Keyboard navigation unaffected (Tab, Arrow, Enter, Escape)
- [ ] Focus management via `FocusHandler` not bypassed
- [ ] ARIA roles and attributes preserved

### Regression Risk Areas
When any of the following are touched, extended review applies:

| Module Touched | Verify These Combinations |
|---------------|--------------------------|
| `VirtualScroll.cs` | + Edit, + Selection, + InfiniteScroll |
| `Edit.cs` | + Virtualization (add-new-row DOM stability) |
| `FocusHandler.cs` | + Group, + Batch Edit, + Tab sequence |
| `Selection.cs` | + InfiniteScroll, + PersistSelection, + CheckBox |
| `GridJSInteropAdaptor.cs` | Disposal, callback routing, null-ref on teardown |
| `DataGenerator.cs` | + Filter + Sort + Group + Page query compositing |
| Freeze columns | + Reorder, + Resize, column list integrity |

---

## Responding to Review Feedback

1. **Address every comment** — do not close threads without a response
2. **Re-request review** after all comments resolved
3. **Do not force-push** after review approval without re-review
4. **Breaking feedback** — if a reviewer flags a regression risk, halt and re-analyze root cause

---

## Merge Strategy

| Scenario | Strategy |
|----------|----------|
| Feature branch → develop | **Squash merge** (clean history) |
| Bugfix branch → develop | **Squash merge** |
| Hotfix branch → main | **Merge commit** (traceability) |
| Develop → main (release) | **Merge commit** (preserve full history) |

---

## Common Review Issues

| Issue | Resolution |
|-------|-----------|
| Missing XML comment on new public member | Add `<summary>`, `<value>`, `<remarks>` per standard |
| `object` cast without type guard | Add null check + type validation before cast |
| `StateHasChanged` called directly | Use `CallStateHasChangedAsync()` wrapper |
| JS interop not disposed | Implement `IDisposable.Dispose()`, call `_dotnetRef?.Dispose()` |
| New `[Parameter]` without `_backing` field | Add private backing field + `UpdateProperty` call |
| innerHTML cleared on DOM wrapper | Use targeted child removal, preserve wrapper node |
