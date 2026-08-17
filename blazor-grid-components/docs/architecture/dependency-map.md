# Dependency Map — Syncfusion Blazor DataGrid

> **Audience**: Architects, Senior Developers, AI Agents performing scoped edits  
> **Prerequisite**: [`architecture/system-architecture.md`](./system-architecture.md)  
> **Last Updated**: March 11, 2026

---

## Purpose

This document maps every module's dependencies — what it reads, what it writes, which modules it calls, and which modules depend on it. Use this map to:

1. **Scope a change safely** — know what else might be affected
2. **Identify regression risks** — find all callers before modifying a module
3. **Assign sub-agent work** — provide precise file and module context

---

## Top-Level Dependency Graph

```
                        SfGrid<TValue>
                            │
          ┌─────────────────┼──────────────────────────────┐
          │                 │                              │
    Infrastructure     Action Modules                 Presentation
          │                 │                              │
    ┌─────┴──────┐   ┌──────┴────────────────┐    ┌───────┴──────┐
    │            │   │                       │    │              │
  DataMgr    JsAdaptor  ┌─Sort ──────────────┤  Renderers   Editors
  PropHelper EventAgg   ├─Filter             │    │              │
                        ├─Grouping           │  GridRow      NormalEdit
                        ├─Edit               │  CellRender   DialogEdit
                        ├─Selection          │  GridHeader   BatchEdit
                        ├─VirtualScroll ─────┤  GridContent
                        ├─InfiniteScroll     │
                        ├─FocusHandler       │
                        ├─Reorder            │
                        ├─RowReorder         │
                        ├─ForeignKey ────────┤
                        ├─DetailRow          │
                        ├─ReactiveAggregate  │
                        └─MergeHandler───────┘
                                │
                         DataGenerator<T>
                                │
                         SfDataManager
                                │
                     [Adaptor: BlazorAdaptor /
                      WebApiAdaptor / ODataV4 /
                      CustomAdaptor / ...]
```

---

## Module Dependency Detail

### DataGenerator\<T\>
**File**: `Internal/Actions/Data.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.SortSettings.Columns` | `Query` (output) |
| `Parent.FilterSettings.Columns` | — |
| `Parent.SearchSettings` | — |
| `Parent.GroupSettings` | — |
| `Parent.PageSettings` | — |
| `Parent.ColumnQueryMode` | — |
| `Parent.AllowPaging` | — |
| `Parent.EnableVirtualization` | — |
| `Parent.EnableInfiniteScrolling` | — |
| `Parent.Aggregates` | — |
| `Parent.Columns` (via `GridUtils.GetColumns`) | — |

**Called by**: `DataProcess()` on `SfGrid<TValue>` (every data refresh)  
**Calls**: `SfDataManager.ExecuteQuery(query)`  
**Modules that affect its output**: Sort, Filter, Grouping, VirtualScroll, InfiniteScroll

**Regression risk**: Any module that modifies `SortSettings`, `FilterSettings`, `GroupSettings`, or `PageSettings` indirectly affects query output. Changes to `GenerateQuery()` affect ALL data operations.

---

### Sort\<T\>
**File**: `Internal/Actions/Sort.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.AllowSorting` | `Parent.SortSettings.Columns` |
| `Parent.AllowMultiSorting` | `Parent.PropertyChanges` (via UpdateProperty) |
| `Parent.SortSettings` | Grid header cell sort icon state |

**Called by**: `SfGrid.SortColumnAsync()`, `SfGrid.ClearSortingAsync()`, header click JS callback  
**Calls**: `Parent.ModelChanged()` → triggers DataGenerator  
**Dependencies**: DataGenerator (sort applied in SortQuery)

**Feature interactions**:
- Grouping: grouped columns always sort ascending by default
- Virtualization: sort triggers full re-virtualization
- Persistence: sort state serialized to localStorage

**Regression risk**: Changing SortSettings mutation order can break multi-sort with persistence.

---

### Filter\<T\>
**File**: `Internal/Actions/Filter.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.AllowFiltering` | `Parent.FilterSettings.Columns` |
| `Parent.FilterSettings.Type` | Filter UI state (icons, active indicators) |
| `Parent.Columns` | — |
| `Parent.ForeignKeyModule` | — |

**Called by**: `SfGrid.FilterByColumnAsync()`, `SfGrid.ClearFilteringAsync()`, filter UI events  
**Calls**: `Parent.ModelChanged()`, `ForeignKey<T>` (for FK column filter)  
**Dependencies**: DataGenerator, ForeignKey module

**Feature interactions**:
- Search: filter and search predicates are combined with AND logic
- Grouping: filter applied before grouping
- Paging: filter resets pager to page 1
- PersistSelection: filters can change visible rows, persist selection must reconcile

**Regression risk**: FilterSettings.Columns mutation while persistence is active can cause stale filter UI.

---

### Grouping\<T\>
**File**: `Internal/Actions/Group.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.AllowGrouping` | `Parent.GroupSettings.Columns` |
| `Parent.GroupSettings` | `Parent.GroupStates` (expand/collapse state) |
| `Parent.Columns` | Column `Visible` state (hide grouped columns) |

**Called by**: `SfGrid.GroupColumnAsync()`, `SfGrid.UngroupColumnAsync()`, drag-drop JS callback  
**Calls**: `Parent.ModelChanged()`, Sort module (sorted by group fields)  
**Dependencies**: DataGenerator, Sort

**Feature interactions**:
- Aggregates: group footer and caption aggregates use GroupGeneratedData
- Virtualization: group + virtual is handled by `GroupGeneratedData` in VirtualScroll
- Lazy loading: separate code path in DataGenerator when `EnableLazyLoading = true`
- ShowGroupedColumn: hides grouped column from data rows
- ForeignKey: grouped FK columns must show display value, not raw value

**Regression risk**: LazyLoading + Virtualization + Grouping is the most regression-sensitive combination. Changes to GroupQuery path must be tested against all three active simultaneously.

---

### Edit\<T\>
**File**: `Internal/Actions/Edit.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.EditSettings` | `Parent.DataSource` (via application event) |
| `Parent.Columns` (PrimaryKey, EditType) | `Parent.AddOrDeleteArgs` (signals `OnAfterRenderAsync`) |
| `Parent.SelectionModule` | Edit form state (NormalEdit/DialogEdit/BatchEdit) |

**Called by**: `SfGrid.AddRecordAsync()`, `SfGrid.StartEditAsync()`, `SfGrid.DeleteRecordAsync()`, `SfGrid.EndEditAsync()`, toolbar/command events  
**Calls**: `GridEvents.OnActionBegin`, `GridEvents.OnActionComplete`, `GridEvents.OnSave`, `GridEvents.OnDelete`  
**Dependencies**: Selection (cleared after save/cancel), ReactiveAggregate (aggregates update after edit)

**`EditComplete()` call path**:
```
OnAfterRenderAsync() detects AddOrDeleteArgs != null
  → IsDeleteAction = (Action == "Delete")
  → AddOrDeleteArgs = null
  → EditModule.EditComplete(args)
```

**Feature interactions**:
- ShowAddNewRow + Virtualization: **Critical combination** — Bug `1011415` flicker fix: destroy form **before** content re-renders; preserve wrapper element height via `style.height`
- Batch edit + AutoFill: requires Box cell selection mode
- Dialog edit + Validation: `ValidationDialog.razor` used for dialog mode errors
- Normal edit + FrozenColumns: edit row must span frozen and scrollable areas

**Regression risk**: Edit module has the most complex feature interactions. Any change to `EditComplete()` must be verified against: Normal+Frozen, Dialog+Validation, Batch+AutoFill, ShowAddNewRow+Virtual.

---

### Selection\<T\>
**File**: `Internal/Actions/Selection.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.AllowSelection` | `Parent.SelectedRowIndexes` |
| `Parent.SelectionSettings` | `Parent.SelectedRowIndex` |
| `Parent.CurrentViewData` | `GridRow.IsSelected` |
| `Parent.CheckBoxState` | `Parent.CheckBoxState` |

**Called by**: `SfGrid.SelectRowAsync()`, `SfGrid.SelectRowsAsync()`, `SfGrid.SelectCellAsync()`, click JS callback  
**Calls**: `GridEvents.OnRowSelecting`, `GridEvents.RowSelected`, `GridEvents.OnCellSelecting`, `GridEvents.CellSelected`

**Feature interactions**:
- PersistSelection: row keys stored and re-applied after filter/sort/page
- CheckboxMode: `Default` vs `ResetOnRowClick` changes selection behavior
- AutoFill: requires Box cell selection — changing CellSelectionMode clears selection (`ClearCellSelectionAsync`)
- RowDragAndDrop: drag requires selection; `Type = Multiple` needed for multi-row drag

**Regression risk**: `PersistSelection` + paging is regression-sensitive. Selected keys must survive DataSource change detection in `OnParametersSetAsync`.

---

### VirtualScroll\<T\>
**File**: `Internal/Actions/VirtualScroll.cs` (largest module in the codebase)

| Reads from | Writes to |
|-----------|----------|
| `Parent.EnableVirtualization` | `GeneratedData` (row data cache, keyed by page index) |
| `Parent.EnableColumnVirtualization` | `GeneratedRows` (row model cache) |
| `Parent.OverscanCount` | `FrozenCachedData`, `FrozenCachedRowObject` (frozen caches) |
| `Parent.PageSettings.PageSize` | `GroupGeneratedData` (group+virtual cache) |
| `Parent.RowHeight` | `RowStartIndex`, `RowEndIndex` (visible row range) |
| `Parent.GroupSettings` | `StartColumnIndex`, `EndColumnIndex` (column virtual) |
| `IsDataSourceChanged` (flag) | `VirtualizedColumns` (visible column subset) |
|  | CSS `translateY` transform on virtual content |

**Called by**: JS scroll events (via `GridJSInteropAdaptor`), `DataProcess()`, `ModelChanged()`  
**Calls**:
- `DataGenerator.GenerateQuery(VirtualStartIndex, VirtualEndIndex)` — fetch visible slice
- `EventAggregator.Trigger("VirtualComponentUpdate")` — notify after row height detection

**`IsDataSourceChanged` flag**:
Set in `OnParametersSetAsync` when `DataSource` changed while `IsRendered`. Consumed by `VirtualScroll` to reset scroll position and caches on next render.

**Feature interactions**:
- Frozen columns: `FrozenCachedRowObject` / `FrozenCachedData` caches — separate frozen vs. movable data pipelines
- Grouping: `GroupGeneratedData` dictionary separates group+virtual data from plain virtual
- InfiniteScroll: **mutually exclusive** — `EnsureFeaturesCompatibility()` enforces this
- RowHeight: JS measures it at `sfBlazor.Grid.initialize` if `RowHeight` not set by developer
- MaskRow: `EnableVirtualMaskRow` shows skeleton placeholder rows during async fetch

**Regression risk**: Highest-complexity module. Changes must be tested with all combinations:
- Row-only virtualization
- Column-only virtualization
- Row + Column simultaneous virtualization
- Row virtual + Frozen columns
- Row virtual + Grouping (with and without LazyLoading)
- Row virtual + OverscanCount > 0
- Row virtual + DataSource change (scroll reset path)

---

### InfiniteScroll\<T\>
**File**: `Internal/Actions/InfiniteScroll.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.EnableInfiniteScrolling` | Block data cache |
| `Parent.InfiniteScrollSettings` | Scroll position |

**Called by**: JS scroll-end callback  
**Calls**: `DataGenerator.GenerateQuery()` via `IntialInfinitePageQuery()`

**Mutual exclusion**: Cannot be enabled simultaneously with `EnableVirtualization`.

---

### FocusHandler\<T\>
**File**: `Internal/Actions/FocusHandler.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.KeySettings` | Focused cell/row DOM state |
| `Parent.EnableVirtualization` | JS focus callbacks |
| `Parent.EditSettings` | — |

**Called by**: JS keyboard events (Tab, Arrow keys, Enter, Escape, F2)  
**Calls**: `Selection<T>` (arrow key selection), `Edit<T>` (F2 edit, Enter save, Escape cancel)

**Feature interactions**: All keyboard-driven features route through FocusHandler. Changing key behavior in Edit or Selection without updating FocusHandler breaks keyboard navigation.

---

### ForeignKey\<T\>
**File**: `Internal/Actions/ForeignKey.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.Columns` (IsForeignKey) | Column display value cache |
| ForeignDataSource per column | — |

**Called by**: DataGenerator (for filter), CellRender (for display), DropDownEditCell (for edit)  
**Provides**: Display value lookup from foreign data source given a raw key value

---

### Reorder\<T\>
**File**: `Internal/Actions/Reorder.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.AllowReordering` | `Parent.Columns` order |
| `Parent.Columns` | `RefreshColumnHeader = true` |

**Called by**: JS column drag-drop callback

**Feature interactions**: Stacked headers limit reorder to same level. Frozen columns cannot cross the frozen boundary.

---

### RowReorder\<T\>
**File**: `Internal/Actions/RowReorder.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.AllowRowDragAndDrop` | `Parent.DataSource` (reordered) |
| `Parent.RowDropSettings` | Indent column width |
| `Parent.SelectionSettings` | — |

**Called by**: JS row drag-drop callback  
**Requires**: PrimaryKey column, Selection enabled, `Type = Multiple` for multi-row drag

---

### ReactiveAggregate\<T\>
**File**: `Internal/Actions/ReactiveAggregate.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.Aggregates` | Aggregate computed values |
| `Parent.CurrentViewData` | — |

**Called by**: `OnAfterRenderAsync` after edit/delete, Observable data changes  
**Purpose**: Recomputes aggregate rows without triggering a full data refresh

---

### MergeHandler\<T\>
**File**: `Internal/Actions/MergeHandler.cs`

| Reads from | Writes to |
|-----------|----------|
| `Parent.AutoSpan` | Cell `colspan`/`rowspan` attributes |
| `Parent.Columns[].AutoSpan` | `Parent.SuppressAutoSpanning` |
| `Parent.CurrentViewData` | — |

**Called by**: GridRow/CellRender during render, `ModelChanged()` after data load  
**Two-pass algorithm**: Horizontal (row) merge first, vertical (column) merge second

---

## Cross-Module Interaction Matrix

The following matrix shows which module changes affect which other modules:

| Changed Module → | DataGen | Sort | Filter | Group | Edit | Select | Virtual | InfScroll | Focus | Merge |
|-----------------|---------|------|--------|-------|------|--------|---------|-----------|-------|-------|
| **DataGenerator** | — | ✅ | ✅ | ✅ | — | ✅ | ✅ | ✅ | — | ✅ |
| **Sort** | ✅ | — | — | ✅ | — | — | ✅ | — | — | — |
| **Filter** | ✅ | — | — | — | — | ✅ | ✅ | — | — | — |
| **Grouping** | ✅ | ✅ | — | — | — | — | ✅ | — | — | — |
| **Edit** | ✅ | — | — | — | — | ✅ | ✅ | — | ✅ | — |
| **Selection** | — | — | ✅ | — | ✅ | — | — | — | ✅ | — |
| **VirtualScroll** | ✅ | — | — | ✅ | ✅ | — | — | ✗ | — | — |
| **FocusHandler** | — | — | — | — | ✅ | ✅ | — | — | — | — |

Legend: ✅ = Affects | ✗ = Mutually exclusive

---

## Internal vs. External Dependencies

### Internal Dependencies (within this repository)
```
Syncfusion.Blazor.Grids
  ├── Syncfusion.Blazor.Internal        (SfDataBoundComponent, SfBaseComponent, SfScriptModules)
  ├── Syncfusion.Blazor.Data            (SfDataManager, Query, Adaptors)
  └── Syncfusion.Blazor.Buttons         (checkbox in selection, toolbar buttons)
```

### External Dependencies
```
Syncfusion.Blazor.Grid.csproj references:
  ├── Microsoft.AspNetCore.Components   (Blazor component model — ComponentBase, RenderFragment)
  ├── System.Text.Json                  (JSON serialization for JS interop payloads)
  ├── System.ComponentModel             (DefaultValue attribute on Parameters)
  └── [See Syncfusion.Blazor.Grid.csproj for full NuGet reference list]
```

### Third-Party Integration Points
| Integration | Entry Point | Notes |
|-------------|-------------|-------|
| EJ2 JavaScript | `sfBlazor.Grid.js` (loaded via ScriptModules) | Handles scroll, DOM measurement, keyboard |
| OData services | `ODataV4Adaptor` in SfDataManager | Grid builds OData query strings automatically |
| GraphQL | `GraphQLAdaptor` | Custom query template required |
| SignalR (real-time) | `ObservableCollection<TValue>` | Grid subscribes to collection change events |

---

## Circular Dependency Prevention

The following patterns are **forbidden** to prevent circular dependencies:

| ❌ Forbidden | ✅ Correct |
|------------|-----------|
| Module A calls Module B calls Module A | Use EventAggregator for decoupled communication |
| Renderer directly mutating module state | Renderer raises event → module handles it |
| Module importing a renderer namespace | Modules have no reference to `Internal/Renderer/` |
| Child component accessing sibling child | All cross-child comms go through `SfGrid<TValue>` parent |

---

## File-Level Reference for Sub-Agent Work

When assigning a sub-agent, provide this table for the targeted module:

| Module | Primary File | Secondary Files | Public Surface Used |
|--------|-------------|-----------------|---------------------|
| Data | `Internal/Actions/Data.cs` | `Internal/Base/Utils.cs` | `SfGrid.DataModule` |
| Sort | `Internal/Actions/Sort.cs` | `GridSortSettings.razor.cs`, `GridSortColumn.cs`, `GridSortColumns.razor.cs` | `SfGrid.SortModule`, `SortColumnAsync()`, `ClearSortingAsync()` |
| Filter | `Internal/Actions/Filter.cs` | `GridFilterSettings.razor.cs`, `GridFilterColumn.cs`, `GridFilterColumns.razor.cs`, `Internal/Renderer/Filter/` | `SfGrid.FilterModule`, `FilterByColumnAsync()`, `ClearFilteringAsync()` |
| Edit | `Internal/Actions/Edit.cs` | `GridEditSettings.cs`, `Internal/Editors/*`, `Internal/Renderer/NormalEdit.razor`, `DialogEdit.razor`, `BatchEdit.razor`, `GridAddNewRow.razor` | `SfGrid.EditModule`, `AddRecordAsync()`, `StartEditAsync()`, `EndEditAsync()`, `DeleteRecordAsync()` |
| Selection | `Internal/Actions/Selection.cs` | `GridSelectionSettings.cs` | `SfGrid.SelectionModule`, `SelectRowAsync()`, `SelectRowsAsync()`, `SelectCellAsync()`, `ClearSelectionAsync()` |
| VirtualScroll | `Internal/Actions/VirtualScroll.cs` | `Internal/Renderer/GridVirtualContent.razor`, `GridVirtualHeader.razor` | `SfGrid.VirtualScrollModule` |
| InfiniteScroll | `Internal/Actions/InfiniteScroll.cs` | `GridInfiniteScrollSettings.razor.cs` | `SfGrid.InfiniteScrollModule` |
| Grouping | `Internal/Actions/Group.cs` | `GridGroupSettings.razor.cs`, `Internal/Renderer/GroupDropArea.razor`, `GroupedHeader.razor` | `SfGrid.GroupModule`, `GroupColumnAsync()`, `UngroupColumnAsync()` |
| FocusHandler | `Internal/Actions/FocusHandler.cs` | `GridKeySettings.cs` | `SfGrid.FocusModule` |
| Reorder | `Internal/Actions/Reorder.cs` | — | `SfGrid.ReorderModule`, `ReorderColumnsAsync()` |
| RowReorder | `Internal/Actions/RowReorder.cs` | `GridRowDropSettings.cs` | `SfGrid.RowReorderModule` |
| ForeignKey | `Internal/Actions/ForeignKey.cs` | `Internal/GridForeignColumn.razor`, `Internal/Renderer/Editors/ForeignKeyEditCell.razor` | `SfGrid.ForeignKeyModule` |
| DetailRow | `Internal/Actions/DetailRow.cs` | `Internal/Renderer/GridDetailRow.razor` | `SfGrid.DetailRowModule`, `ExpandAllDetailRowAsync()`, `CollapseAllDetailRowAsync()` |
| ReactiveAggregate | `Internal/Actions/ReactiveAggregate.cs` | `Internal/Renderer/RefreshAggregate.razor` | `SfGrid.ReactiveAggregateModule` |
| MergeHandler | `Internal/Actions/MergeHandler.cs` | `SfGrid.Properties.cs` (`AutoSpan`) | `SfGrid.MergeModule` |

---

## EventAggregator Message Reference

All cross-module and cross-component async messages that flow through `EventAggregator`:

| Message Key | Trigger Point | Consumer |
|-------------|--------------|---------|
| `"InitialLoad"` | `OnAfterScriptRendered` (first load) | Grid integration |
| `"VirtualComponentUpdate"` | After `initialize` returns `RowHeight` | `VirtualScroll<T>` — recalculate offsets |

---

*For complete data flow through these modules, see [`architecture/data-flow.md`](./data-flow.md).*  
*For component hierarchy, see [`architecture/component-architecture.md`](./component-architecture.md).*
