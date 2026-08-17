---
name: filtermenu-skill
description: Expert knowledge for the FilterMenu feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to FilterMenu behaviour, including per-column popup dialog rendering, operator list resolution, type-specific value input components, filter/clear action pipeline, adaptive fullscreen dialog mode, FilterTemplate support, and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Expert knowledge for FilterMenu (`FilterType.Menu`) in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

**Agent Invocation**
<!-- token-budget: 40 words -->
- Paired custom agent: `/docs/ai-agents/custom-agents/filtermenu-agent.md`
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to FilterMenu (`FilterType.Menu`). Do NOT load alongside other feature skills.
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
- `openspec/specs/filtermenu/spec.md`
- `GridFilterSettings.razor.cs`
- `GridFilterColumn.cs`
- `Internal/Actions/Filter.cs`
- `Internal/Renderer/Filter/FilterMenuRenderer.razor`
- `Internal/Renderer/Filter/FilterMenuRenderer.razor.cs`
- `Internal/Renderer/Filter/FilterType.razor`
- `Internal/Renderer/AdaptiveDialogRenderer.razor`
- `Internal/Renderer/ColumnMenu.razor`
- `SfGrid.Lifecycle.cs`
- `SfGrid.Methods.cs`
- `sf-grid.js`

---

## Training Insights Applied
<!-- token-budget: 80 words -->

Key rules and edge cases from `/docs/training/` that directly govern FilterMenu work:

- **Only one dialog visible at a time**: `Filter<T>` tracks `FilterIconIsClicked` and `FilterIconColumn`. Opening a second column's menu must close the first dialog before showing the new one. Never assume the previous dialog is already hidden.
- **Dialog open is a two-gate cancellable flow**: Both `FilterDialogOpening` (with `Cancel`) and `OnActionBegin(RequestType = FilterBeforeOpen)` (with `Cancel`) must be checked in sequence in `OpenFilterDialog()`. Skipping either gate breaks developer-configured guards.
- **`isnull` / `isnotnull` disable the value input**: When the operator dropdown resolves to `isnull` or `isnotnull`, the value input must be visually disabled and `FilterByColumn` must be called with `null` as the value. Do not add a value-required guard for these operators.
- **Foreign key columns use `ForeignKeyValue` as the predicate field**: `GridColumn.ForeignKeyValue` (not `GridColumn.Field`) must be used when constructing the predicate. The `SfAutoComplete` for foreign key string columns must use the column's own `DataManager`, not the grid's.
- **`FilterTemplate` replaces the entire dialog body**: When `GridColumn.FilterTemplate` is set, neither the operator dropdown nor the built-in value input is rendered. The template receives `PredicateModel<TValue>` as context. Do not render built-in controls alongside a `FilterTemplate`.
- **`ShouldRender` suppression is intentional**: `FilterMenuRenderer` suppresses Blazor re-renders while the dialog is closed. This is a deliberate performance optimisation — do not remove the `ShouldRender` override.
- **Action pipeline is mandatory**: Every `FilterByColumn` and `RemoveFilterColumnByField` call must honour `OnActionBegin` / `OnActionComplete` via the established event pipeline. Skipping this breaks developer-configured guards.
- **One feature per agent invocation** — if a bug spans FilterMenu and ColumnMenu, split into two tasks (per `training/03-llm-best-practices/working-with-llms.md` §Mistake 1).
- **Operator list has three resolution layers** (lowest to highest priority): built-in type defaults → `GridFilterSettings.Operators` global override → `FilterDialogOpeningEventArgs.FilterOperators` per-open override. Any change to operator resolution must preserve this precedence chain.
- **`ConfigureAwait(true)` on every await**: All async calls within `Filter<T>` and `FilterMenuRenderer.razor.cs` must use `.ConfigureAwait(true)`.
- **Zero analyzer warnings**: All `public` members require XML `/// <summary>` comments.

---

## Code Location Map
<!-- token-budget: 80 words -->

| Path | Purpose |
|------|---------|
| `Internal/Actions/Filter.cs` | `Filter<T>` module — `FilterByColumn`, `RemoveFilterColumnByField`, `ClearFiltering`, `GetOperator`, `GetColumnOperator`, `GetFilterType`; owns `FilterIconIsClicked` / `FilterIconColumn` flags; drives `DataGenerator` query rebuild |
| `Internal/Renderer/Filter/FilterMenuRenderer.razor` | Per-column popup dialog markup — `SfDialog` (desktop) + `SfDialog` adaptive (mobile); operator `SfDropDownList`; type-specific value inputs (`SfAutoComplete`, `SfNumericTextBox<T>`, `SfDatePicker<T>`, `SfDateTimePicker<T>`, `SfTimePicker<T>`, `SfDropDownList<bool?>`); `FilterTemplate` slot; Filter / Clear buttons |
| `Internal/Renderer/Filter/FilterMenuRenderer.razor.cs` | Code-behind for `FilterMenuRenderer<TContent>` — `OpenFilterDialog()`, `FilterBtnClick()`, `ClearBtnClick()`, `CloseHandler()` (adaptive), `ShouldRender` suppression; operator list population; pre-population of operator + value from existing `GridFilterColumn` |
| `Internal/Renderer/Filter/FilterType.razor` | Router component — selects `FilterMenuRenderer`, `FilterBarRenderer`, `ExcelBase`, or `FilterCheckBoxRenderer` based on resolved `FilterType` per column |
| `Internal/Renderer/AdaptiveDialogRenderer.razor` | Adaptive dialog container — activated when `EnableAdaptiveUI = true`; FilterMenu uses `customFilterDialog` JS call for fullscreen layout in adaptive mode |
| `Internal/Renderer/ColumnMenu.razor` | Renders "Filter" menu item when `ShowColumnMenu = true`; sets `IsColumnMenuFilter = true` on `Filter<T>` before triggering `OpenFilterDialog()` |
| `GridFilterSettings.razor.cs` | `GridFilterSettings` settings component — `Type`, `Columns`, `Operators`, `EnableCaseSensitivity`, `IgnoreAccent` |
| `GridFilterColumn.cs` | `GridFilterColumn` predicate model — `Field`, `Operator`, `Value`, `Predicate`, `MatchCase`, `IgnoreAccent`, `Uid`, `ActualValue`, `ColumnType` |
| `GridColumn.cs` (filter-menu–relevant) | `AllowFiltering`, `FilterSettings` (`FilterSettings.Type`, `FilterSettings.Operator`), `FilterEditorSettings` (typed params), `FilterTemplate` |
| `SfGrid.Lifecycle.cs` | Instantiates `Filter<T>` as `Parent.FilterModule = new Filter<TValue>(this)` |
| `SfGrid.Methods.cs` | `FilterByColumnAsync`, `ClearFilteringAsync` — public API entry points |
| `Internal/Actions/Data.cs` | `DataGenerator<T>.GenerateQuery()` — applies filter `Where()` predicates from `FilterSettings.Columns`; distinguishes `FilterBar` / `Menu` column types |
| `sf-grid.js` (`sfBlazor.Grid.filterPopupRender`) | Returns `[X, Y]` pixel position for desktop dialog placement |
| `sf-grid.js` (`sfBlazor.Grid.customFilterDialog`) | Positions fullscreen adaptive dialog; called in adaptive mode instead of `filterPopupRender` |

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live feature cross-reference + `/docs/training/` risk tables.
> Omitted pairs have no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|--------------|------|
| FilterMenu + Sorting | Filter `Where()` applied BEFORE `SortBy()` in `DataGenerator.GenerateQuery()`. Altering query composition order corrupts filtered+sorted result sets. | Critical |
| FilterMenu + ForeignKey | Predicate `Field` must be `GridColumn.ForeignKeyValue` (not `GridColumn.Field`) when `IsForeignColumn() == true`. `SfAutoComplete` in the dialog must use the column's own `DataManager`. Any change to `FilterByColumn` must preserve this field substitution. | Critical |
| FilterMenu + ColumnMenu | ColumnMenu "Filter" item sets `IsColumnMenuFilter = true` on `Filter<T>` before calling `OpenFilterDialog()`. After Filter/Clear, `HideColumnMenuPopup()` must be called when `IsColumnMenuFilter == true`. Removing either call breaks ColumnMenu filter close behaviour. | Critical |
| FilterMenu + Adaptive UI | When `EnableAdaptiveUI = true`, `sfBlazor.Grid.customFilterDialog` replaces `filterPopupRender`. Desktop dialog is hidden; fullscreen `SfDialog` with header close button is shown. `CloseHandler()` calls `MenuAdaptiveDialog.HideAsync()`. Both paths must trigger the same Filter / Clear logic. | Critical |
| FilterMenu + FilterTemplate | When `GridColumn.FilterTemplate` is set, the operator dropdown and built-in value input must NOT render. Template receives `PredicateModel<TValue>` context. `FilterBtnClick()` must read state from the template's bound model, not from internal operator/value fields. | High |
| FilterMenu + Paging | Page resets to 1 when filter changes — `RequestType = "Filtering"` triggers `PageQuery` reset in `DataGenerator`. `RequestType = "ClearFiltering"` must also reset paging. Do not bypass this reset. | High |
| FilterMenu + Grouping | Grouped grid still shows per-column filter icons. `FilterMenuRenderer` is rendered per filterable column inside the column header renderer. Group header rows must not receive filter dialog triggers. | High |
| FilterMenu + Selection (Checkbox) | `Filter<T>.FilterByColumn` calls `Parent.ClearSelectionAsync()` when `SelectedRecords.Count > 0` and `PersistSelection = false`. Do not remove this call — it preserves selection consistency after filter. | High |
| FilterMenu + Persistence | `EnablePersistence = true` serializes `GridFilterSettings.Columns` to `localStorage["grid{ID}"]`. On restore, each `GridFilterColumn` (including `Uid`, `ColumnType`) is used to pre-populate dialogs on re-open and to apply predicates on first data load. `ColumnType` must always be set on `GridFilterColumn` in `FilterByColumn`. | High |
| FilterMenu + Virtualization (Column) | Column virtualization renders a subset of columns. `FilterMenuRenderer` is only instantiated for visible columns. Ensure the `FilterIconIsClicked` / `FilterIconColumn` state does not reference a column that has been virtualised out of the render tree. | High |
| FilterMenu + Frozen Columns | Frozen and movable column filter icons are rendered in separate header sections. `FilterMenuRenderer` receives the correct column reference regardless of freeze position. Dialog position (`filterPopupRender`) is computed relative to the clicked icon's DOM element — do not pass a frozen column offset into a movable header zone. | High |
| FilterMenu + Aggregates | `ReactiveAggregate<T>` listens for `DataBound` after filter reload. FilterMenu triggers `DataBound` via `EventAggregator` through the same pipeline as FilterBar. Aggregate recalculation must fire after every filter/clear — no risk of wrong values, only missed render if the event chain is broken. | Medium |
| FilterMenu + Editing | An open FilterMenu dialog should not block edit actions. If `BeginEdit` fires while a filter dialog is open, the dialog must be dismissed (hidden) before the edit row renders, to avoid z-index overlap. | Medium |
| FilterMenu + Export | Export reads `FilterSettings.Columns` to reproduce filter predicates. `ColumnType` on `GridFilterColumn` must be populated by `FilterByColumn` for correct OData / query serialisation in the exported file. | Medium |
| FilterMenu + FilterBar | Only one filter type is active per column (resolved by `Filter<T>.GetFilterType(column)`). When the grid-level `Type = FilterType.Menu` but a column overrides to `FilterSettings.Type = FilterType.FilterBar`, the correct renderer must be selected. Do not allow both dialog and bar to render for the same column simultaneously. | Medium |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: FilterMenu
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid FilterMenu feature.
Scope: FilterMenu-related surfaces only (FilterMenuRenderer, Filter<T> menu paths).
Do NOT modify FilterBar, Excel, or CheckBox filter paths.

=== WHAT YOU MUST KNOW (pre-loaded) ===
- Activation: AllowFiltering = true + GridFilterSettings.Type = FilterType.Menu
- Filter module class: Filter<T> in Internal/Actions/Filter.cs
- Dialog renderer: FilterMenuRenderer<TContent> in Internal/Renderer/Filter/FilterMenuRenderer.razor(.cs)
- Dialog IDs: desktop = {Column.Uid}-flmdlg  |  operator dropdown = {Column.Uid}-floptr
- Dialog open flow: FilterIconIsClicked flag set → FilterMenuRenderer.Rendered() → JS filterPopupRender → OpenFilterDialog() → FilterDialogOpening (cancellable) → OnActionBegin(FilterBeforeOpen, cancellable) → MenuDialog.ShowAsync() → OnActionComplete(FilterAfterOpen) → FilterDialogOpened
- Adaptive flow replaces filterPopupRender with customFilterDialog; close via CloseHandler → MenuAdaptiveDialog.HideAsync()
- Operator resolution order (lowest → highest): type defaults → GridFilterSettings.Operators → FilterDialogOpeningEventArgs.FilterOperators
- Default operator lists by column type:
    String:  startswith, doesnotstartwith, endswith, doesnotendwith, contains, doesnotcontain, equal, notequal, isempty, isnotempty, like
    Number / Date / DateTime / DateOnly / TimeOnly / Decimal / Long:  equal, notequal, greaterthan, greaterthanorequal, lessthan, lessthanorequal, isnull, isnotnull
    Boolean:  equal, notequal
- Value input disabled when operator is isnull or isnotnull; FilterByColumn called with null value
- FilterBtnClick: MenuDialog.HideAsync → Filter<T>.FilterByColumn(field, op, value, matchCase, ignoreAccent, uid, actualValue)
- ClearBtnClick: MenuDialog.HideAsync → Filter<T>.RemoveFilterColumnByField(field, uid)
- Foreign key: predicate Field = GridColumn.ForeignKeyValue; SfAutoComplete uses column's own DataManager
- FilterTemplate: replaces operator dropdown + value input entirely; receives PredicateModel<TValue>
- ShouldRender suppression in FilterMenuRenderer is intentional — do not remove
- If IsColumnMenuFilter == true → call HideColumnMenuPopup() after Filter/Clear
- Persistence: GridFilterSettings.Columns serialized to localStorage["grid{ID}"] when EnablePersistence = true
- Public API: SfGrid.FilterByColumnAsync / ClearFilteringAsync in SfGrid.Methods.cs

=== BEFORE YOU MAKE ANY CHANGE ===
1. Check training/02-requirements-analysis/understanding-requirements.md for ForeignKey + FilterMenu edge cases.
2. Confirm the two cancellation gates (FilterDialogOpening + OnActionBegin FilterBeforeOpen) are both preserved.
3. Confirm isnull/isnotnull operator disables the value input and passes null to FilterByColumn.
4. Confirm FilterTemplate columns do NOT render built-in operator dropdown or value input.
5. Confirm IsColumnMenuFilter → HideColumnMenuPopup() is called after Filter/Clear.
6. Confirm ColumnType is always set on GridFilterColumn in FilterByColumn.

=== CONSTRAINTS (all mandatory) ===
- No behavior change outside stated scope
- No new public API without explicit task authorization
- Zero analyzer warnings; XML comments on all modified public members
- All await calls use .ConfigureAwait(true)
- No direct StateHasChanged() — use InvokeAsync / grid internal render scheduling
- No direct JSRuntime.InvokeAsync — route through GridJSInteropAdaptor<T>
- Preserve ShouldRender suppression in FilterMenuRenderer
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
