---
name: feature-impact-analysis-skill
description: Expert knowledge for the Feature Impact Analysis process in the Syncfusion Blazor TreeGrid. Use this skill in any bug‑fix task to evaluate cross‑feature blast radius, dependency chains, module interactions, and risk conditions before any code change is made.
---

# Feature Impact Analysis
<!-- token-budget: 20 words -->

**Purpose**  
Reusable checklist invoked in `bug-fix` mode to assess cross-feature blast radius before any change is made.

---

## When to Use
<!-- token-budget: 30 words -->
Invoke this document in **`bug-fix` mode only**, after loading the feature-specific skill and before writing any code.  
Do NOT invoke in `feature-implementation` mode.

---

## Step 1 — Identify the Bug Origin Layer
<!-- token-budget: 50 words -->

| Layer | Indicator | Files to Check |
|-------|-----------|---------------|
| Infrastructure | JS-interop failure, DotNetRef error, disposal crash | `GridJSInteropAdaptor.cs`, `sf-grid.js` |
| Data | Wrong row count, wrong sort/filter result | `Internal/Actions/Data.cs` |
| Business | Wrong feature logic, incorrect state | `Internal/Actions/<Module>.cs` |
| Presentation | Wrong DOM, wrong CSS class, wrong ARIA | `Internal/Renderer/*.razor` |

---

## Step 2 — Module Blast Radius Table
<!-- token-budget: 120 words -->

For every module listed, mark whether it **shares code path** with the buggy flow.

| Module | File | Shares Path? | Action Required |
|--------|------|-------------|----------------|
| `Sort<T>` | `Actions/Sort.cs` | ☐ Yes ☐ No | |
| `Filter<T>` | `Actions/Filter.cs` | ☐ Yes ☐ No | |
| `Group<T>` | `Actions/Group.cs` | ☐ Yes ☐ No | |
| `Edit<T>` | `Actions/Edit.cs` | ☐ Yes ☐ No | |
| `Selection<T>` | `Actions/Selection.cs` | ☐ Yes ☐ No | |
| `VirtualScroll<T>` | `Actions/VirtualScroll.cs` | ☐ Yes ☐ No | |
| `InfiniteScroll<T>` | `Actions/InfiniteScroll.cs` | ☐ Yes ☐ No | |
| `FocusHandler<T>` | `Actions/FocusHandler.cs` | ☐ Yes ☐ No | |
| `Reorder<T>` | `Actions/Reorder.cs` | ☐ Yes ☐ No | |
| `RowReorder<T>` | `Actions/RowReorder.cs` | ☐ Yes ☐ No | |
| `ForeignKey<T>` | `Actions/ForeignKey.cs` | ☐ Yes ☐ No | |
| `DetailRow<T>` | `Actions/DetailRow.cs` | ☐ Yes ☐ No | |
| `ReactiveAggregate<T>` | `Actions/ReactiveAggregate.cs` | ☐ Yes ☐ No | |
| `MergeHandler<T>` | `Actions/MergeHandler.cs` | ☐ Yes ☐ No | |

---

## Step 3 — High-Risk Combination Check
<!-- token-budget: 80 words -->

Cross-reference the bug scenario against the known high-risk combination table.  
Source: `training/06-reference/quick-reference-guides.md` §5.

| Combination | Risk | Relevant to This Bug? |
|-------------|------|-----------------------|
| Virtualization + Editing | 🔴 High | ☐ Yes ☐ No |
| Grouping + Selection | 🔴 High | ☐ Yes ☐ No |
| Grouping + Editing | 🔴 High | ☐ Yes ☐ No |
| Frozen Columns + Virtualization | 🔴 High | ☐ Yes ☐ No |
| Frozen Columns + Column Reorder | 🔴 High | ☐ Yes ☐ No |
| Infinite Scroll + Editing | 🔴 High | ☐ Yes ☐ No |
| Batch Edit + Aggregates | 🔴 High | ☐ Yes ☐ No |
| Paging + Grouping | 🟡 Medium | ☐ Yes ☐ No |
| Sort + Filter | 🟡 Medium | ☐ Yes ☐ No |
| Export + Column Templates | 🟡 Medium | ☐ Yes ☐ No |
| Column Resize + Frozen | 🟡 Medium | ☐ Yes ☐ No |
| DetailRow + Selection | 🟡 Medium | ☐ Yes ☐ No |
| FilterBar + ForeignKey | 🟡 Medium | ☐ Yes ☐ No |

---

## Step 4 — EventAggregator Chain Audit
<!-- token-budget: 60 words -->

If the bug is in a module that fires `EventAggregator` events, list every module that subscribes to those events — they are all affected by the fix.

Reference event table: `training/06-reference/quick-reference-guides.md` §4.

| Event Fired | Subscribed By | Could Break? |
|-------------|--------------|-------------|
| `DataBound` | ReactiveAggregate, Selection, FocusHandler | ☐ Yes ☐ No |
| `EditBegin` | FocusHandler | ☐ Yes ☐ No |
| `EditComplete` | Selection, FocusHandler | ☐ Yes ☐ No |
| `ActionBegin` | SfGrid (public event) | ☐ Yes ☐ No |
| `ActionComplete` | SfGrid (public event) | ☐ Yes ☐ No |

---

## Step 5 — Regression Test Matrix Output
<!-- token-budget: 60 words -->

Before submitting the fix, produce a regression test matrix in this format:

| Scenario | Feature Combination | Expected | Test Status |
|----------|-------------------|----------|------------|
| Fix scenario | [This Feature] alone | [what should happen] | ☐ Pass ☐ Fail |
| Regression A | [This] + [Related] | [original behaviour preserved] | ☐ Pass ☐ Fail |
| Regression B | [This] + [Related] | [original behaviour preserved] | ☐ Pass ☐ Fail |

---

## Mandatory Final Gate
<!-- token-budget: 40 words -->

Before the agent writes any code:

- [ ] Step 1 layer identified  
- [ ] Step 2 blast radius complete  
- [ ] Step 3 high-risk combinations checked  
- [ ] Step 4 EventAggregator chain audited  
- [ ] Step 5 regression test matrix ready  

**If any checkbox is unchecked → STOP. Complete the analysis before proceeding.**

---

*Invoked exclusively in `bug-fix` mode by feature custom agents.*  
*See: `/docs/ai-agents/custom-agents/<feature>-agent.md` for invocation contract.*
