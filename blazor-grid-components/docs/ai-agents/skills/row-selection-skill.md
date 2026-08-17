---
name: row-selection-skill
description: Expert knowledge for the Row Selection feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to row selection behaviour, including persist selection across pages, checkbox selection with header checkbox, programmatic selection APIs (SelectRowAsync, SelectRowsAsync, SelectRowsByRangeAsync, ClearSelectionAsync), and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Expert knowledge for Row Selection in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

**Agent Invocation**
<!-- token-budget: 40 words -->
- Paired custom agent: `/docs/ai-agents/custom-agents/row-selection-agent.md`
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to Row Selection. Do NOT load alongside other feature skills.
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

Key rules and edge cases from `/docs/training/` that directly govern Row Selection work:

- **Primary key is mandatory for persist selection**: `PersistSelection = true` requires at least one column with `IsPrimaryKey = true`. `Selection<T>.PrimaryKey` is computed lazily from `GridUtils.GetColumns(Parent)`. If the primary key column is missing or null at resolve time, persist dictionaries silently stay empty — this is the most common persist bug.
- **`_persistedData` and `DeSelectedPersistData` are complementary**: `_persistedData` stores selected rows keyed by primary key value; `DeSelectedPersistData` stores rows explicitly deselected after a Select-All. For remote data (`DataSource == null`), deselect tracking is the canonical source of truth for header checkbox state.
- **`IsHeaderCheckboxChecked` is a tri-state gate**: Once the header checkbox is clicked into a "checked" state and filtering/searching is active, `IsHeaderCheckboxChecked = true` keeps the persist collection in "filter-active" mode. Clearing filters or search does NOT automatically reset this flag — code must call `ResetPersistSelection()` or update `IsSelectFilteredField` / `IsSelectSearchKey`.
- **Selection mode switches require `PropertyChanges` keys**: Switching `SelectionMode` programmatically injects `"CellSelectionModeChanged"`, `"RowSelectionModeChanged"`, or `"BothSelectionModeChanged"` into `PropertyChanges` so `ClearCellSelection` can distinguish a mode-change clear from a regular deselect.
- **`SelectRow`/`SelectRows` skips `Cell` mode unless Batch editing**: `Selection<T>.SelectRow()` returns early if `Mode == SelectionMode.Cell` and edit mode is not Batch. Any feature touching mode-switching must preserve this guard.
- **Ctrl+A respects `SelectionType.Single`**: `KeyPressed("CtrlA")` returns immediately when `Type == SelectionType.Single`. Never allow select-all for a grid in single-selection mode.
- **Checkbox-only mode** (`CheckboxOnly = true`): Row click outside the checkbox column is ignored in `RowSelectionClickHandler`. Any change to click routing must preserve this guard check before processing row state.
- **`CheckboxMode = ResetOnRowClick`**: A plain row click (no Ctrl/Shift) behaves like single-select — it deselects all others. `HasCheckBoxColumn(IsFromCheckBox, e)` returns `false` for plain row clicks in this mode, routing through single-select logic instead.
- **`VirtualScrollModule.SelectRowsMethodIndexes`**: When virtualization is active and `SelectRowsAsync` is called programmatically, the indexes array must be stored in `VirtualScrollModule.SelectRowsMethodIndexes` so virtual row rendering can mark the correct rows as selected when they scroll into view.
- **Adaptive/AdaptiveUI toolbar sync**: After any selection change when `EnableAdaptiveUI` is true and delete/edit is enabled, `EventAggregator.Trigger("ToolbarStateChanged", null!)` must be fired. Missing this call leaves Edit/Delete toolbar buttons in wrong enabled state on mobile.
- **`SoftRefresh = true` not `StateHasChanged()`**: Row state changes must set `Parent.SoftRefresh = true` and trigger `"RowStateChanged"` via `EventAggregator`. Never call `StateHasChanged()` directly from `Selection<T>`.
- **`ConfigureAwait(true)` on every await**: All async calls in `Selection<T>` must use `.ConfigureAwait(true)` for Blazor context continuity.
- **Zero-warning build is mandatory**: Any new or modified `public` or `internal` member on `Selection<T>` must carry XML `/// <summary>` comments.

---

## Code Location Map
<!-- token-budget: 80 words -->

All Row Selection-related code lives at these paths (no code reproduced — paths + one-line purpose only):

| Path | Purpose |
|------|---------|
| `Internal/Actions/Selection.cs` | Primary module: `Selection<T>` — all row/cell/checkbox selection logic, persist, header checkbox state, keyboard handler, click handlers |
| `GridSelectionSettings.cs` | Public `[Parameter]` properties: `Mode`, `Type`, `PersistSelection`, `CheckboxOnly`, `CheckboxMode`, `EnableSimpleMultiRowSelection`, `EnableToggle`, `CellSelectionMode`, `AllowDragSelection` |
| `SfGrid.Properties.cs` | `AllowSelection` (bool, default true), `SelectedRowIndex` (int), `SelectionSettings` (GridSelectionSettings) on the root grid component |
| `SfGrid.Methods.cs` | Public async API: `SelectRowAsync`, `SelectRowsAsync`, `SelectRowsByRangeAsync`, `SelectCellAsync`, `SelectCellsAsync`, `SelectCellsByRangeAsync`, `ClearSelectionAsync`, `GetSelectedRecordsAsync`, `GetSelectedRowIndexesAsync`, `GetSelectedRowCellIndexesAsync` |
| `EventModels/Grids.cs` | Event args: `RowSelectingEventArgs<T>`, `RowSelectEventArgs<T>`, `RowDeselectEventArgs<T>`, `CellSelectingEventArgs<T>`, `CellSelectEventArgs<T>`, `CellDeselectEventArgs<T>` |
| `GridEvents.cs` | Event callbacks declared on `GridEvents<TValue>`: `RowSelecting`, `RowSelected`, `RowDeselecting`, `RowDeselected`, `CellSelecting`, `CellSelected`, `CellDeselecting`, `CellDeselected` |
| `Enumeration/GridsEnumerations.cs` | `SelectionMode` (Row/Cell/Both), `SelectionType` (Single/Multiple), `CheckboxSelectionType` (Default/ResetOnRowClick), `CellSelectionMode` (Flow/Box), `CheckState` (Check/UnCheck/Intermediate) |
| `Internal/Actions/VirtualScroll.cs` | `SelectRowsMethodIndexes`, `ShiftSelectionRowIndexes`, `IsSelAllChangedByRowClick`, `IsSelectAllWithFilter` — virtual scroll selection state holders |
| `Internal/Actions/FocusHandler.cs` | `SelectedRowIndex`, `SelectedCellIndex` — focus sync after programmatic selection |
| `Internal/Base/GridJSInteropAdaptor.cs` | Routes JS click events that trigger selection via `ClickHandler`; no direct selection JS calls |
| `sf-grid.js` | Fires click/keyboard events back to .NET; no selection state held in JS |

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live feature cross-reference + `/docs/training/` risk tables.
> Omitted pairs have no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|--------------|------|
| Selection + Paging | `RefreshSelectionOnPaging()` clears `_lastSelectedRow/_lastSelectedCell` on page change. When `PersistSelection = false`, `CheckBoxState` resets to `UnCheck`. Changing page logic must call `RefreshSelectionOnPaging()` before re-rendering rows. | Critical |
| Selection + Persist + Filter/Search | `GetCurrentFilterData(requestType)` tracks which rows are filtered/searched — used to keep `_filteredOrSearchedData` in sync with `_persistedData`. Any filter/search clear that does not call `GetCurrentFilterData("ClearFiltering")` or `GetCurrentFilterData("ClearSearch")` will leave the persist dictionary stale, causing phantom selections. | Critical |
| Selection + Virtualization | `GetRowsObject()` switches between `Parent.Rows` and `VirtualScrollModule.GeneratedRows` based on `EnableVirtualization`. Programmatic `SelectRowsAsync` must write to `VirtualScrollModule.SelectRowsMethodIndexes` so rows selected off-screen are restored when they scroll into view. `ShiftSelectionRowIndexes` must be reset to `(-1,-1)` on clear. | Critical |
| Selection + Editing | `Edit<T>` sets `EditModule.ClearSelection = true` to indicate the grid is editing. `SelectRow()` checks `EditModule.ClearSelection` and calls `ClearRowSelection` before selecting the new row. Persist selection must not clear `_persistedData` when edit is in progress — guard: `Parent.SelectionSettings.PersistSelection && Parent.IsEdit`. | High |
| Selection + Grouping | Grouped virtual grids use `Parent.CurrentGroupedData` items; `SelectByRow` and `ClearSelectionByRow` must update both `Parent.Rows` state AND `CurrentGroupedData[i].IsSelected` for the correct Uid match. Forgetting the `CurrentGroupedData` sync causes header checkbox to show wrong state. | High |
| Selection + Sorting | `Selection<T>` listens to `DataBound` (fired after sort) and re-applies persist selection. Row indexes shift after sort — reselection must use primary key (from `_persistedData`) not the stale index from before sort. | High |
| Selection + Checkbox Column | `HasCheckBoxColumn()` checks `GridUtils.GetColumns(Parent)` for a column with `Type == ColumnType.CheckBox`. Header checkbox `CheckState` is a tri-state: `Check`, `UnCheck`, `Intermediate`. `SetHeaderCheckState()` logic is complex — modifications must re-run all 6 boolean conditions that determine the final state. | High |
| Selection + Detail Row | `SelectRows` explicitly filters out `RowType == "DetailRow"` rows. `ClearRowSelection` also skips detail rows for standard deselect but handles them for `IsDirty && IsExpand` cleanup. Any new selection code must replicate this guard. | Medium |
| Selection + Row Drag/Drop | When `AllowDragSelection = true` and `PersistSelection = true`, `ClearRowSelection` removes the row's primary key from `_persistedData` on drag deselect. Drag module must not call `ClearRowSelection` on drop target selection refresh — use selection API instead. | Medium |
| Selection + Aggregates | `ReactiveAggregate<T>` listens to `DataBound`. Selection does NOT trigger `DataBound` — it triggers `RowStateChanged`. Aggregates are not recalculated on selection. No risk unless future code incorrectly ties aggregate refresh to row state change. | Medium |
| Selection + Export | Export reads `SelectedRecords` (populated from `Parent.Rows` where `IsSelected == true`). Export does not require any selection module method call. Risk: if export is called during persist-mode with remote data, `SelectedRecords` may only reflect the current page, not the full persist dictionary. | Medium |
| Selection + Frozen Columns | Selection is data-row–level, not cell-column–level for row mode. Frozen column boundaries do not affect row selection state. Cell selection in `Box` mode that spans a frozen boundary may produce incorrect `startCellIndex/endCellIndex` if frozen column indices are not contiguous. | Low |
| Selection + FocusHandler | After `SelectByRow` is called programmatically with `isSelectionMethodInvoked = true`, `FocusModule.Focus()` is invoked to move keyboard focus to the selected row. If `FocusModule` is null (focus disabled), the focus call is guarded by null-conditional — no crash, but focus does not move. | Low |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: Row Selection
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid Row Selection feature.
Scope: Internal/Actions/Selection.cs and the surfaces listed in Code Location Map only.

=== WHAT YOU MUST KNOW (pre-loaded) ===
- Selection module class: Selection<T> in Internal/Actions/Selection.cs
- Public entry points (SfGrid.Methods.cs):
    SelectRowAsync(index, isToggle, selectAcrossPages)
    SelectRowsAsync(int[] rowIndexes)
    SelectRowsByRangeAsync(startIndex, endIndex?)
    SelectCellAsync(cellIndex, isToggle)
    SelectCellsAsync(rowCellIndexes[])
    SelectCellsByRangeAsync(startIndex, endIndex)
    ClearSelectionAsync()
    GetSelectedRecordsAsync() → List<TValue>
    GetSelectedRowIndexesAsync() → List<int>
    GetSelectedRowCellIndexesAsync() → List<ValueTuple<int,int>>
- Selection settings: GridSelectionSettings — Mode, Type, PersistSelection, CheckboxOnly,
  CheckboxMode, EnableSimpleMultiRowSelection, EnableToggle, CellSelectionMode, AllowDragSelection
- Root grid params: AllowSelection (default true), SelectedRowIndex, SelectionSettings
- Persist dictionaries: _persistedData (selected keys→data), DeSelectedPersistData (deselected keys→data)
- Header checkbox state: Parent.CheckBoxState (CheckState enum); SetHeaderCheckState() is the authoritative recalculator
- Primary key resolution: Selection<T>.PrimaryKey — lazy, reads IsPrimaryKey column via GridUtils.GetColumns
- EventAggregator events fired by Selection:
    "RowStateChanged" — after every row select/deselect
    "ContentStateChanged" / "VirtualComponentUpdate" — after header checkbox bulk changes
    "ToolbarStateChanged" — after selection change when AdaptiveUI is active
- Virtual scroll integration: VirtualScrollModule.SelectRowsMethodIndexes, ShiftSelectionRowIndexes
- EventArgs types: RowSelectingEventArgs<T>, RowSelectEventArgs<T>, RowDeselectEventArgs<T>
- Action pipeline for each select/deselect: fire RowSelecting (check Cancel) → update state → fire RowSelected

=== BEFORE YOU MAKE ANY CHANGE ===
1. Consult /docs/training/02-requirements-analysis/understanding-requirements.md for edge cases.
2. Consult /docs/training/06-reference/quick-reference-guides.md for risk combos.
3. Confirm PersistSelection guard: if changing _persistedData, check PersistSelection == true first.
4. Confirm virtual scroll guard: if EnableVirtualization, use GetRowsObject() not Parent.Rows directly.
5. Confirm GroupedData sync: if grouping is active, update CurrentGroupedData[i].IsSelected by Uid match.

=== CONSTRAINTS (all mandatory) ===
- No behavior change outside stated scope
- No new public API without explicit task authorization
- Zero analyzer warnings; XML comments on all new/modified public and internal members
- All await calls use .ConfigureAwait(true)
- No direct StateHasChanged() — set SoftRefresh = true and trigger "RowStateChanged" via EventAggregator
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
