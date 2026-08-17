---
name: filterbar-skill
description: Expert knowledge for the FilterBar feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to FilterBar behaviour, including filter input rendering, Immediate/OnEnter mode timing, operator inference from input prefixes, multi-column filter state management, filter status message in pager, and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Expert knowledge for FilterBar in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

**Agent Invocation**
<!-- token-budget: 40 words -->
- Paired custom agent: `/docs/ai-agents/custom-agents/filterbar-agent.md`
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to FilterBar (`FilterType.FilterBar`). Do NOT load alongside other feature skills.
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
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `GridFilterSettings.razor.cs`
- `GridFilterColumn.cs`
- `Internal/Actions/Filter.cs`
- `Internal/Renderer/Filter/FilterBarRenderer.razor`
- `Internal/Renderer/Filter/FilterInput.razor`
- `Internal/Actions/FocusHandler.cs` (filter bar keyboard nav paths)
- `Internal/Actions/Edit.cs` (Shift+Tab back into filter bar)
- `Internal/Base/InternalClass.cs` (`FilterBarParameters`, locale keys)

---

## Training Insights Applied
<!-- token-budget: 80 words -->

Key rules and edge cases from `/docs/training/` that directly govern FilterBar work:

- **FilterBar + ForeignKey is a documented high-regression area** (`training/02-requirements-analysis/understanding-requirements.md` §Part 3). ForeignKey columns use `GridColumn.ForeignKeyValue` as the predicate field, not `GridColumn.Field`. Any change to `FilterByColumn` must preserve this substitution.
- **Operator inference from typed prefixes**: `FilterInput.razor` infers filter operators from input prefix characters (`*` → startswith, `%` → endswith, `!=` → notequal, `<` / `<=` / `>` / `>=` → numeric comparisons). Changes to `GetActualFilterValue` or `GetOperator(string)` must preserve all prefix mappings.
- **One feature per agent invocation** — if a bug spans FilterBar and Editing keyboard interaction, split into two tasks (per `training/03-llm-best-practices/working-with-llms.md` §Mistake 1).
- **Action pipeline is mandatory**: Every `FilterByColumn` and `RemoveFilterColumnByField` call must honour `OnActionBegin` / `OnActionComplete` via `Parent.ModelChanged(...)`. Skipping this breaks developer-configured guards.
- **Never call `StateHasChanged()` directly**: Use grid internal render scheduling. `FilterInput.razor` uses `InvokeAsync` to return to Blazor context from the `System.Timers.Timer` elapsed callback — this pattern is intentional and must be preserved.
- **Chunking rule**: `Filter.cs` is ~580 lines. Never provide the full file to a sub-agent. Chunk to the relevant method(s) only (see `training/04-code-processing/optimal-chunking-strategies.md`).
- **`ConfigureAwait(true)` on every await**: All async calls within `Filter<T>` and `FilterInput.razor.cs` must use `.ConfigureAwait(true)`.
- **Zero analyzer warnings**: All `public` members require XML `/// <summary>` comments. `#pragma warning disable BL0005` is used deliberately to set child component properties programmatically — do not remove these suppressions.
- **`FilterBarMode.Immediate` uses a `System.Timers.Timer`**: The timer delay comes from `Parent.FilterSettings.ImmediateModeDelay` (default 1500 ms). `FilterBarMode.OnEnter` fires only on Enter key. Do not conflate the two modes when fixing timing bugs.

---

## Code Location Map
<!-- token-budget: 80 words -->

| Path | Purpose |
|------|---------|
| `Internal/Actions/Filter.cs` | `Filter<T>` module — `FilterByColumn`, `RemoveFilterColumnByField`, `ClearFiltering`, `GetOperator`, `GetColumnOperator`, `GetFilterType`, `UpdateFilterMessage`, `UpdateFilterModel`, operator string↔enum maps |
| `Internal/Renderer/Filter/FilterBarRenderer.razor` | Renders the `<tr class="e-filterbar">` row; iterates filterable columns; passes `FilterInputParameters` to `FilterInput`; handles frozen/virtual column slicing |
| `Internal/Renderer/Filter/FilterInput.razor` | Per-cell `<td class="e-filterbarcell">`; renders `<input type="search">` or `FilterTemplate`; handles `@oninput`, `@onkeydown`, `@onblur`, `@onfocus`; contains `ProcessFilter`, `GetActualFilterValue`, timer logic, `CancelIconClick` |
| `GridFilterSettings.razor.cs` | `GridFilterSettings` settings component — `Type`, `Mode`, `ImmediateModeDelay`, `EnableCaseSensitivity`, `IgnoreAccent`, `ShowFilterBarStatus`, `Columns` |
| `GridFilterColumn.cs` | `GridFilterColumn` predicate model — `Field`, `Operator`, `Value`, `Predicate`, `MatchCase`, `IgnoreAccent`, `Uid`, `ActualValue`, `RawInputValue`, `ColumnType` |
| `GridColumn.cs` (filter-relevant properties) | `AllowFiltering`, `FilterSettings` (`FilterSettings.Operator`, `FilterSettings.Type`), `FilterTemplate`, `FilterClearIcon`, `FilterEditorSettings` |
| `Internal/Actions/Data.cs` lines ~410–435 | `DataGenerator<T>.GenerateQuery()` — applies filter `Where()` predicates from `FilterSettings.Columns`; distinguishes `FilterBar`/`Menu` column types |
| `Internal/Actions/FocusHandler.cs` lines ~211–285 | FilterBar keyboard navigation — Arrow-Up from first data row focuses filter bar; Tab/Shift+Tab routing via `sfBlazor.Grid.focusFilterBar` |
| `Internal/Actions/Edit.cs` lines ~361–398 | Shift+Tab from first edit cell when `isFilterBar = true` — calls `sfBlazor.Grid.focusFilterBar` |
| `Internal/Base/InternalClass.cs` line ~359 | `FilterBarParameters` class — `IsFrozen`, `IsFrozenRight` for frozen column slicing in `FilterBarRenderer` |
| `Internal/Base/InternalClass.cs` lines ~483, ~705 | Locale keys: `FilterbarTitle`, `FilterBar` — used for ARIA labels and pager status text |
| `SfGrid.Lifecycle.cs` | Instantiates `Filter<T>` as `Parent.FilterModule = new Filter<TValue>(this)` |
| `SfGrid.Methods.cs` | `FilterByColumnAsync`, `ClearFilteringAsync` — public API entry points |
| `sf-grid.js` (`sfBlazor.Grid.focusFilterBar`) | JS function that focuses the correct filter bar input cell by field index |
| `sf-grid.js` (`sfBlazor.Grid.updateFilterBarCell`) | JS function that updates filter bar cell display values for multi-column filter state sync |
| `sf-grid.js` (`sfBlazor.Grid.searchClear`) | JS function called by `CancelIconClick` to clear the input DOM value |

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live feature cross-reference + `/docs/training/02-requirements-analysis/understanding-requirements.md` §Part 3 regression risk tables.
> Omitted pairs have no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|--------------|------|
| FilterBar + Sorting | Filter `Where()` applied BEFORE `SortBy()` in `DataGenerator.GenerateQuery()`. Changing query composition order corrupts filtered+sorted result sets. | Critical |
| FilterBar + ForeignKey | Predicate `Field` must be `GridColumn.ForeignKeyValue`, not `GridColumn.Field`, when `IsForeignColumn() == true`. `UpdateFilterMessage` must resolve the foreign key column's `HeaderText` for pager status. | Critical |
| FilterBar + Editing (Shift+Tab) | `Edit<T>` detects `isFilterBar` flag; Shift+Tab from first edit cell must focus the filter bar via `sfBlazor.Grid.focusFilterBar`. Removing the `isFilterBar` check breaks keyboard navigation out of edit mode. | Critical |
| FilterBar + FocusHandler | `FocusHandler<T>` routes Arrow-Up from first data row and Tab from last header cell into the filter bar via `sfBlazor.Grid.focusFilterBar`. Both paths check `isFilterBar` before routing. Any change to `FilterType` detection here must preserve both paths. | Critical |
| FilterBar + Paging | Page must reset to 1 when filter changes (`RequestType = "Filtering"` triggers `PageQuery` reset in `DataGenerator`). `ShowFilterBarStatus = true` updates `Parent.PagerRef.ExternalMessage` with active filter summary. Clear must blank the message. | High |
| FilterBar + Grouping | Grouped columns emit indent cells in `FilterBarRenderer` — `GetActualColumns()` prepends indent/group cells before data cells. Changing column collection iteration must preserve indent cell insertion for grouped columns. `ShowGroupedColumn = false` adds `e-hide` CSS to the grouped column's filter bar cell. | High |
| FilterBar + Virtualization (Column) | `FilterBarRenderer` uses `Parent.VirtualScrollModule!.GetVirtualColumns()` for the movable header slice when `EnableColumnVirtualization = true`. Changes to column slicing logic must preserve the virtual column range check. | High |
| FilterBar + Frozen Columns | `FilterBarRenderer.UpdateFilterBarColumns()` slices columns into frozen-left / frozen-right / movable sets using `FilterBarParameters.IsFrozen` and `IsFrozenRight`. `FilterBarParameters` is populated per render pass. Do not flatten frozen slices into a single pass. | High |
| FilterBar + Selection (Checkbox) | `Filter<T>.FilterByColumn` calls `Parent.ClearSelectionAsync()` when `SelectedRecords.Count > 0` and `PersistSelection = false`. This preserves selection consistency after filter. Do not remove this call. | High |
| FilterBar + Persistence | `EnablePersistence = true` serializes `GridFilterSettings.Columns` to `localStorage["grid{ID}"]`. On restore, `FilterBarRenderer` reads `FilterSettings.Columns` to pre-populate input values. `RawInputValue` must be preserved on persisted `GridFilterColumn` so formatted date inputs restore correctly. | High |
| FilterBar + Aggregates | `ReactiveAggregate<T>` listens for `DataBound` after filter data reload. Filter triggers `DataBound` via `EventAggregator`. Aggregate recalculation must always fire after filter — no risk of wrong values, only missed render if event chain is broken. | Medium |
| FilterBar + RowDragAndDrop | `FilterBarRenderer.GetActualColumns()` prepends a `RowDrag` indent cell when `AllowRowDragAndDrop = true`. Do not skip this indent cell. | Medium |
| FilterBar + DetailRow | `FilterBarRenderer.GetActualColumns()` prepends a `DetailIndent` cell when `DetailTemplate != null` and `!IsRenderedFromTreeGrid`. | Medium |
| FilterBar + Column Menu | Column Menu renders a "Filter" item only when `FilterType != FilterType.FilterBar` (`ColumnMenu.razor` line ~383). FilterBar type suppresses the Column Menu filter option — do not change this guard. | Medium |
| FilterBar + Adaptive UI | `AdaptiveDialogRenderer.razor` explicitly excludes `FilterType.FilterBar` from the adaptive filter dialog path. FilterBar never opens a dialog on mobile — it stays as an always-visible row. | Medium |
| FilterBar + Export | Export reads `FilterSettings.Columns` for query reproduction. FilterBar must keep `GridFilterColumn.ColumnType` populated (set in `FilterByColumn`) for correct export predicate serialization. | Low |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: FilterBar
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid FilterBar feature.
Scope: FilterBar-related surfaces only. Do NOT modify Menu, Excel, or CheckBox filter paths.

=== WHAT YOU MUST KNOW (pre-loaded) ===
- Activation: AllowFiltering = true + GridFilterSettings.Type = FilterType.FilterBar (default type)
- Filter module class: Filter<T> in Internal/Actions/Filter.cs
- Public API: SfGrid.FilterByColumnAsync / ClearFilteringAsync in SfGrid.Methods.cs
- Filter bar UI: FilterBarRenderer.razor (row) + FilterInput.razor (per-cell input)
- Input element ID: {Column.Field}_filterBarcell
- Timing modes: FilterBarMode.OnEnter (Enter key fires filter) | FilterBarMode.Immediate (System.Timers.Timer, delay = ImmediateModeDelay ms)
- Operator inference from typed prefixes in GetActualFilterValue() and GetOperator(string):
    * → startswith, % → endswith, != → notequal, < / <= / > / >= → numeric
    Column.FilterSettings.Operator overrides default per-type operator
- Filter state: Parent.FilterSettings.Columns (List<GridFilterColumn>); pre-populates input on Columns restore
- RawInputValue: stored on GridFilterColumn for formatted date/display round-trip
- Multi-column filter update: sfBlazor.Grid.updateFilterBarCell called after multi-filter in ProcessFilter
- Pager status: Filter<T>.UpdateFilterMessage → Parent.PagerRef.ExternalMessage (only when ShowFilterBarStatus = true + FilterType.FilterBar)
- Foreign key predicate field: GridColumn.ForeignKeyValue (not Field) when IsForeignColumn() == true
- Keyboard nav: FocusHandler<T> + Edit<T> both check isFilterBar flag for Arrow-Up / Shift+Tab routing
- Clear icon: CancelIconClick → RemoveFilterColumnByField + sfBlazor.Grid.searchClear
- Frozen columns: FilterBarRenderer.UpdateFilterBarColumns() slices by IsFrozen / IsFrozenRight
- Virtual columns: VirtualScrollModule.GetVirtualColumns() when EnableColumnVirtualization = true
- Column type detected via: FilterInputParameters.Column.Type (ColumnType enum)
- FilterTemplate: replaces <input> entirely; receives PredicateModel context

=== BEFORE YOU MAKE ANY CHANGE ===
1. Check training/02-requirements-analysis/understanding-requirements.md for ForeignKey + FilterBar edge case.
2. Confirm GetActualFilterValue prefix parsing is NOT affected by your change.
3. Confirm FilterBarRenderer column-slicing (frozen, virtual, grouped indent) is NOT affected.
4. Confirm UpdateFilterMessage is called after every FilterByColumn and RemoveFilterColumnByField.

=== CONSTRAINTS (all mandatory) ===
- No behavior change outside stated scope
- No new public API without explicit task authorization
- Zero analyzer warnings; XML comments on all modified public members
- All await calls use .ConfigureAwait(true)
- No direct StateHasChanged() — use InvokeAsync / grid internal render scheduling
- No direct JSRuntime.InvokeAsync — route through GridJSInteropAdaptor<T>
- Preserve #pragma warning disable BL0005 where deliberately setting child component properties
- No direct module-to-module calls — use EventAggregator for cross-module events
- Follow naming-conventions.md (PascalCase methods, _camelCase private fields, TValue public / T internal)
- Follow error-handling.md (guard clauses first, no silent catch blocks)
- FilterBarMode.Immediate timer must use System.Timers.Timer, not Task.Delay

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
