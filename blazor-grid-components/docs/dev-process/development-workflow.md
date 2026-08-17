# Development Workflow  Syncfusion Blazor DataGrid

> **Audience**: All developers, AI agents, Scrum Master  
> **Prerequisite**: [`architecture/system-architecture.md`](../architecture/system-architecture.md)  
> **Last Updated**: March 11, 2026

---

## Overview

All changes to `SfGrid<TValue>` follow a strict **7-phase lifecycle** to ensure correctness, performance, and backward compatibility. Every phase has a gate that must be cleared before proceeding.

---

## Phase 1  Requirements

**Goal**: Establish a clear, testable specification before any code is written.

### Inputs
- Azure DevOps task link
- BoldDesk ticket (customer issue) or internal feature request
- Existing API reference for affected parameters

### Actions
1. Create `docs/requirements/features/<feature-name>/` or `docs/requirements/bugs/<bug-id>/`
2. Write `feature-requirement.md` / `description.md` using the backlog template
3. Identify affected modules from `docs/architecture/dependency-map.md`
4. Identify regression-sensitive feature combinations

### Gate: Scrum Master Approval
- [ ] User story / bug description complete
- [ ] Acceptance criteria defined and measurable
- [ ] Affected modules listed
- [ ] Regression risk level assigned (Low / Medium / High / Critical)

---

## Phase 2  Architecture Review

**Goal**: Validate the solution approach against the component's architectural constraints.

### Actions
1. Review `docs/architecture/system-architecture.md` for layering compliance
2. Confirm no public API breaking changes (`IGrid.cs`, `SfGrid.Properties.cs`)
3. Identify JS-Interop touch points in `sf-grid.js` / `GridJSInteropAdaptor.cs`
4. Write `fix-approach.md` or `functional-spec.md` with solution description

### Gate: Architect AI Approval
- [ ] Solution stays within the correct architectural layer
- [ ] No new circular dependencies introduced
- [ ] JS-Interop disposal handled if new listeners are added
- [ ] Public API contract unchanged or API review task created

---

## Phase 3  Unit Test Case Design

**Goal**: Define the test cases *before* writing code (TDD approach for fixes).

### Actions
1. List all positive, negative, and boundary test cases
2. Identify which BUnit test file covers the affected module
3. For bugs: write a failing test reproducing the issue
4. For features: write tests for all acceptance criteria

### Gate: Test Lead Approval
- [ ] Failing test created for bug fix
- [ ] All acceptance criteria have at least one test
- [ ] Edge cases and browser-specific behaviors covered
- [ ] Playwright test plan created for UI interactions

---

## Phase 4  Development

**Goal**: Implement the fix or feature following all code guidelines.

### Actions
1. Create branch: `feature/<description>` or `bugfix/<description>` from `develop`
2. Implement changes  scope is strictly limited to the identified module
3. Follow `docs/code-guidelines/coding-standards.md`
4. Follow `docs/code-guidelines/naming-conventions.md`
5. Add/update XML comments for any changed public members
6. Zero analyzer warnings  verified before commit

### Constraints
-  Do NOT change public API signatures without API Review task
-  Do NOT modify unrelated files in the same PR
-  Do NOT comment out code  delete unused code
-  DO keep changes scoped to the minimum necessary files
-  DO update `docs/requirements/bugs/<id>/fix-approach.md` with AI log details

### Gate: Self-Review Checklist
- [ ] Build passes with zero analyzer warnings
- [ ] No unintended file changes in diff
- [ ] XML comments updated for changed public members
- [ ] Code follows naming conventions

---

## Phase 5  Testing

**Goal**: Validate the fix/feature against the full feature matrix.

### Actions
1. Run existing BUnit test suite  zero regressions
2. Run Playwright tests for affected UI interactions
3. Test against cross-feature combinations from `docs/architecture/dependency-map.md`
4. Test on Blazor Server and Blazor WASM if UI behavior is involved
5. Test with 10K+ rows if the change touches rendering or virtualization

### Regression Risk Checklist by Module
| Module Changed | Must Also Test |
|---------------|---------------|
| `VirtualScroll` | Edit (add-new-row), Selection, InfiniteScroll |
| `Edit` | Validation, Batch, Dialog, Normal modes; Virtualization |
| `Selection` | InfiniteScroll, PersistSelection, ForeignKey |
| `FocusHandler` | Group, Edit, Tab order, Keyboard navigation |
| `DataGenerator` | Sort + Filter + Group + Page compositing |
| `GridJSInteropAdaptor` | Memory: verify disposal on teardown |
| `Filter` | ForeignKey columns, Excel filter, CheckBox filter |
| `Group` | LazyLoad grouping, Aggregate, Caption row rendering |

### Gate: QA Approval
- [ ] All existing BUnit tests pass
- [ ] Playwright tests pass for affected features
- [ ] Tested against the Syncfusion Feature Matrix
- [ ] No memory leaks detected in dispose path

---

## Phase 6  Code Review

**Goal**: Independent validation of correctness, style, and regression safety.

### Actions
1. Create PR using template from `docs/dev-process/pr-guidelines.md`
2. Assign to Code Review AI and one human reviewer
3. Address all feedback  no unresolved comments at merge
4. Re-run tests after any code changes from review feedback

### Gate: Reviewer Approval
- [ ] Solution addresses root cause (not symptoms)
- [ ] No performance regression
- [ ] No memory leak introduced
- [ ] Accessibility (keyboard navigation) unaffected
- [ ] Code Studio usage section completed in PR template

---

## Phase 7  Merge & Release

**Goal**: Integrate safely and deliver to production.

### Actions
1. Squash-merge feature/bugfix branch into `develop`
2. Tag release: `v32.x.x` following semver convention
3. For hotfixes: also cherry-pick to `main` and create patch tag
4. Update `docs/README.md` "Last Updated" timestamp
5. Close Azure DevOps task and BoldDesk ticket with resolution notes

### Gate: Scrum Master Final Approval
- [ ] PR template fully completed
- [ ] All gates from phases 16 cleared
- [ ] Release tag follows `v[MAJOR].[MINOR].[PATCH]` convention
- [ ] Documentation updated

---

## Definition of Done

A task is **Done** when ALL of the following are true:

- [ ] Code merged to `develop` (or `main` for hotfix)
- [ ] All tests pass (BUnit + Playwright)
- [ ] Zero analyzer warnings in build
- [ ] PR template completely filled
- [ ] Related docs updated
- [ ] Azure task and ticket closed
- [ ] No open reviewer comments

---

## Risk Checkpoints Summary

| Phase | Risk Check |
|-------|-----------|
| Requirements | Regression risk level assigned |
| Architecture | No API breaking, no circular deps |
| Test Design | Failing test written before code |
| Development | Zero analyzer warnings |
| Testing | Feature matrix tested |
| Review | Root cause addressed, not symptoms |
| Merge | All gates cleared |
