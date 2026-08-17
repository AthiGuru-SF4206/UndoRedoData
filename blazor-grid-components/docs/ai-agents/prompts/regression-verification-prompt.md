# Regression Verification Prompt
<!-- token-budget: 20 words -->

**Purpose**  
Standard prompt template used by ALL feature custom agents (both modes) to verify that a change does not introduce regressions across the Syncfusion Blazor DataGrid.

> **Loaded by**: Every custom agent, in both `feature-implementation` and `bug-fix` modes.  
> **Loaded after**: The feature-specific skill file.  
> **Must NOT be skipped.**

---

## Regression Verification Prompt Template
<!-- token-budget: 280 words -->

Use this verbatim when invoking the regression verification step:

```
ROLE: Code Review AI — Regression Verifier
COMPONENT: SfGrid<TValue> — Syncfusion.Blazor.Grids
FEATURE UNDER CHANGE: {feature-name}
MODE: {feature-implementation | bug-fix}

TASK:
Review the proposed code change below and verify it does NOT introduce a regression
in any of the listed feature combinations. For each combination, state:
  (a) whether the change touches the shared code path
  (b) what the expected behaviour is
  (c) whether a manual or automated test is required

CHANGE SUMMARY:
{Paste a 3–5 sentence description of what was changed and why.}

MODIFIED FILES:
{List each modified file and the method(s) changed.}

REGRESSION VERIFICATION CHECKLIST — run through all that apply:

1. SORTING
   - [ ] Single-column sort still works after this change
   - [ ] Multi-column sort (Ctrl+click) still works
   - [ ] Programmatic SortColumnAsync() still functions correctly

2. FILTERING
   - [ ] FilterBar input still triggers correct data reduction
   - [ ] Excel / Menu / Checkbox filter dialog still opens and applies correctly
   - [ ] Filter + Sort operation order preserved (filter first, sort after)

3. GROUPING
   - [ ] Group drag-drop to GroupDropArea still works
   - [ ] Expand/collapse group rows still works
   - [ ] Paging across group boundaries behaves correctly

4. EDITING
   - [ ] Normal edit: BeginEdit, SaveEdit, CancelEdit function correctly
   - [ ] Dialog edit: modal opens, saves, closes correctly
   - [ ] Batch edit: cell-level changes accumulate correctly
   - [ ] ShowAddNewRow persistent row remains stable during CRUD
   - [ ] Tab key navigation in edit mode functions correctly

5. SELECTION
   - [ ] Row selection by click functions correctly
   - [ ] Checkbox selection column functions correctly
   - [ ] Selection state is not corrupted on DataSource change

6. VIRTUALIZATION
   - [ ] Virtual scroll renders correct row range
   - [ ] Frozen + virtual scroll containers stay in sync
   - [ ] Edit mode works correctly with virtual DOM

7. PAGING
   - [ ] Page navigation (next/prev/first/last) functions correctly
   - [ ] PageSize change re-fetches correct data slice

8. AGGREGATES
   - [ ] Footer aggregates display correct values after sort/filter/page
   - [ ] Batch edit triggers aggregate recalculation on each cell change

9. ACCESSIBILITY
   - [ ] Keyboard navigation (Tab, Arrow keys) unaffected
   - [ ] ARIA attributes correct on modified DOM elements
   - [ ] Focus management unaffected in non-edit scenarios

10. PERSISTENCE
    - [ ] EnablePersistence = true: state reload after browser refresh unaffected
    - [ ] localStorage read/write unaffected

CONSTRAINTS:
- No behavior change outside the stated bug/feature scope
- No new public API added without explicit task authorization
- Zero analyzer warnings in the output
- Follow naming-conventions.md and coding-standards.md
- All await calls use .ConfigureAwait(true)
- No direct StateHasChanged() calls in action modules
- No direct JSRuntime.InvokeAsync calls outside GridJSInteropAdaptor<T>

OUTPUT FORMAT:
For each checklist item:
  ✅ Not affected — code path does not intersect
  ⚠️ Potentially affected — manual test required: [describe test]
  ❌ Regression risk — must fix before merge: [describe issue]

Final verdict: APPROVED | NEEDS FIXES
```

---

## How Agents Must Use This Prompt
<!-- token-budget: 60 words -->

1. Complete the feature skill work (implementation or fix).
2. Fill in `{feature-name}`, `{mode}`, `CHANGE SUMMARY`, and `MODIFIED FILES`.
3. Submit the completed prompt to the **Code Review AI**.
4. Do NOT submit a PR until Code Review AI returns `Final verdict: APPROVED`.
5. If `NEEDS FIXES` is returned — address all `❌` items and re-run verification.

---

*This prompt is mandatory for ALL feature agents.*  
*Never skip regression verification, even for "small" changes.*  
*See: `/docs/training/02-requirements-analysis/understanding-requirements.md` §3 for risk context.*
