# Sorting Skill
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Sorting in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

## Agent Invocation
<!-- token-budget: 40 words -->
- Paired custom agent: `/docs/ai-agents/custom-agents/sorting-agent.md`
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to Sorting. Do NOT load alongside other feature skills.
- One agent invocation = one feature skill maximum.

---

## Knowledge References
<!-- token-budget: 60 words -->
All content derived from reading these files — do NOT reproduce their content here:

- `docs/training/00-START-HERE.md`
- `docs/training/01-getting-started/architecture-overview.md`
- `docs/training/02-requirements-analysis/understanding-requirements.md`
- `docs/training/03-llm-best-practices/working-with-llms.md`
- `docs/training/04-code-processing/optimal-chunking-strategies.md`
- `docs/training/05-practical-examples/feature-implementation-walkthrough.md`
- `docs/training/06-reference/quick-reference-guides.md`
- `docs/architecture/system-architecture.md`
- `docs/architecture/component-architecture.md`
- `docs/tech-stack/tech-stack.md`
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `docs/code-guidelines/error-handling.md`

---

## Training Insights Applied
<!-- token-budget: 80 words -->

Key rules and edge cases from `/docs/training/` that directly govern Sorting work:

- **Operation order is mandatory**: Filter query is built BEFORE sort query in `DataGenerator<T>.GenerateQuery()`. Any change to query composition must preserve this order — sort must always be the last data-transformation step before paging.
- **`PropertyChanges` drives re-sort**: `AllowSorting`, `AllowMultiSorting`, and `SortSettings` changes are tracked via `UpdateProperty()`. A sort refresh only fires when these keys appear in `PropertyChanges`.
- **Multi-sort via Ctrl+Click**: `AllowMultiSorting = true` allows stacking sort predicates. Clearing the sort stack must reset ALL column sort indicators in the header renderers.
- **Programmatic API must fire event pipeline**: `SortColumnAsync()` must raise `OnActionBegin` → check `Cancel` → sort → raise `OnActionComplete`. Skipping this breaks developer-configured guards.
- **`SortedColumns` is read by sibling modules**: `Filter<T>`, `Group<T>`, and `DataGenerator<T>` all read `Parent.SortSettings.Columns` or `Parent.SortModule?.SortedColumns`. Never restructure the sort state without checking these consumers.
- **No direct `StateHasChanged()`**: Sort header re-render must go through grid's internal render scheduling, not `StateHasChanged()` directly.
- **Zero-warning build is mandatory**: Any new or modified `public` member on `Sort<T>` must carry XML `/// <summary>` comments.
- **`ConfigureAwait(true)` on every await**: All async calls in `Sort<T>` must use `.ConfigureAwait(true)` for Blazor context continuity.

---

## Code Location Map
<!-- token-budget: 80 words -->

All Sorting-related code lives at these paths (no code reproduced — paths + one-line purpose only):

| Path | Purpose |
|------|---------|
| `Internal/Actions/Sort.cs` | Primary module: `Sort<TValue>` — sort state, multi-sort, header click handler, programmatic sort |
| `Internal/Actions/Data.cs` | `DataGenerator<T>.GenerateQuery()` — applies `SortQuery` via `.SortBy()` after filter/search predicates |
| `SfGrid.Properties.cs` | `AllowSorting`, `AllowMultiSorting`, `SortSettings` parameter declarations |
| `SfGrid.Methods.cs` | `SortColumnAsync(columnName, direction)` — public API entry point |
| `SfGrid.Lifecycle.cs` | `OnParametersSetAsync` — detects `AllowSorting`/`SortSettings` changes, triggers `RefreshColumnHeader` |
| `Internal/Renderer/GridHeaderCell.razor` | Renders sort icon, applies `e-ascending`/`e-descending`/`e-sorted` CSS classes |
| `Enumeration/GridsEnumerations.cs` | `SortDirection` enum: `Ascending`, `Descending`, `None` |
| `EventModels/Grids.cs` | `SortEventArgs<T>` — carries `ColumnName`, `Direction`, `Cancel` for `OnActionBegin`/`OnActionComplete` |
| `Internal/Base/GridJSInteropAdaptor.cs` | Receives `onHeaderClick` from JS, dispatches to `SortColumnAsync` |
| `sf-grid.js` | `onHeaderClick` — captures column header click and calls .NET callback |

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live feature cross-reference + `/docs/training/` risk tables.  
> Omitted pairs have no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|--------------|------|
| Sorting + Filtering | Filter predicates applied first in `DataGenerator.GenerateQuery()`; sort comes after. Changing query build order breaks filtered+sorted result sets. | Critical |
| Sorting + Grouping | `Group<T>` reads `SortSettings` to determine sub-sort within groups. Sort state must not be cleared when grouping is active. | Critical |
| Sorting + Paging | Page must reset to page 1 when sort changes (`RequestType = "sorting"` triggers `PageQuery` reset in `DataGenerator`). Must NOT double-reset if sort fires from pager context. | High |
| Sorting + Virtualization | Virtual scroll row range is recalculated after every `DataBound`. Sort triggers `DataBound` via `EventAggregator`. Ensure `VirtualScroll<T>` scroll offset resets to 0 after sort. | High |
| Sorting + Editing | Editing a row while sort is active: `BeginEdit` locates the row by index in `CurrentViewData`. After a sort change while edit is open, row index may shift — sort must close open edit before re-sorting. | High |
| Sorting + Selection | `Selection<T>` listens to `DataBound` and re-applies persistent selection. Sort triggers `DataBound`. Ensure selection indices map to the new sort order, not old. | High |
| Sorting + Aggregates | `ReactiveAggregate<T>` listens to `DataBound`. Sort triggers `DataBound`. Aggregate values must remain correct (they are data-independent, so no risk of wrong value — only risk is a missed render). | Medium |
| Sorting + Column Reorder | `Reorder<T>` changes column order in `GridUtils.GetColumns()` flat list. `Sort<T>` identifies columns by `Field` name (not index). No risk as long as sort identifies columns by `Field`, not position. | Medium |
| Sorting + ForeignKey | `ForeignKey<T>` populates display text from a secondary data source. Sort must sort by the underlying field value, not the display text, unless `ForeignKeyValue` sort override is active. | Medium |
| Sorting + Export | Export reads `SortSettings.Columns` to reproduce the sort order in the exported file. Sort state must be serializable and reflect the current UI sort indicators. | Medium |
| Sorting + Persistence | `EnablePersistence = true` persists `SortSettings` to `localStorage`. On reload, sort must be re-applied from persisted state via `PersistProperties()` before first `DataProcess()`. | Medium |
| Sorting + Frozen Columns | Frozen columns are sorted the same as movable columns — sort is data-level, not DOM-level. No special handling required unless `FreezeDirection` affects column visibility for header click routing. | Low |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: Sorting
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid Sorting feature.
Scope: Internal/Actions/Sort.cs and the surfaces listed below only.

=== WHAT YOU MUST KNOW (pre-loaded) ===
- Sort module class: Sort<T> in Internal/Actions/Sort.cs
- Public entry point: SfGrid.SortColumnAsync(columnName, direction) in SfGrid.Methods.cs
- Sort state: SfGrid.SortSettings.Columns (List of SortDescriptor)
- Query application: DataGenerator<T>.GenerateQuery() — SortQuery applied AFTER FilterQuery
- EventAggregator event fired after sort: "DataBound" (triggers ReactiveAggregate, Selection, FocusHandler)
- Header click routing: JS onHeaderClick → GridJSInteropAdaptor → SortColumnAsync
- CSS indicators: e-ascending / e-descending / e-sorted — applied in GridHeaderCell.razor
- Enum: SortDirection { Ascending, Descending, None } in GridsEnumerations.cs
- EventArgs: SortEventArgs<T> in EventModels/Grids.cs
- Action pipeline: OnActionBegin(Cancel check) → sort → OnActionComplete

=== BEFORE YOU MAKE ANY CHANGE ===
1. Consult /docs/training/02-requirements-analysis/understanding-requirements.md for edge cases.
2. Consult /docs/training/06-reference/quick-reference-guides.md §5 for risk combos.
3. Confirm the fix/feature does NOT alter the filter-before-sort query order in Data.cs.
4. Confirm Sort<T> accesses siblings only via Parent.<Module>? (null-conditional).

=== CONSTRAINTS (all mandatory) ===
- No behavior change outside stated scope
- No new public API without explicit task authorization
- Zero analyzer warnings; XML comments on all modified public members
- All await calls use .ConfigureAwait(true)
- No direct StateHasChanged() — use grid internal render scheduling
- No direct JSRuntime.InvokeAsync — route through GridJSInteropAdaptor<T>
- No direct module-to-module calls — use EventAggregator for cross-module events
- Follow naming-conventions.md (PascalCase methods, _camelCase private fields, TValue public / T internal)
- Follow error-handling.md (guard clauses first, no silent catch blocks)

=== SCOPE ===
{Describe the exact method(s) to implement or fix — one method per task}

=== INPUT ===
{Paste the extracted code chunk — see training/04-code-processing/optimal-chunking-strategies.md}

=== OUTPUT ===
1. Root cause / design rationale (3–5 sentences)
2. Modified method(s) only — no full-file reproduction
3. Interaction risk flags (reference Interaction Matrix above)
4. Required test cases (Given-When-Then format)

After implementation, run regression verification using:
/docs/ai-agents/prompts/regression-verification-prompt.md
```
