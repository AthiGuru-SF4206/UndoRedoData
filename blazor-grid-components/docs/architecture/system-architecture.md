# System Architecture — Syncfusion Blazor DataGrid

> **Audience**: Architects, Senior Developers, AI Agents  
> **Prerequisite**: [`overview/product-overview.md`](../overview/product-overview.md)  
> **Last Updated**: March 18, 2026

---

## Architectural Overview

The Syncfusion Blazor DataGrid (`SfGrid<TValue>`) is a **layered, module-injected, hybrid Blazor component**. It operates across three execution environments simultaneously:

1. **.NET (Server/WASM)** — all data operations, state management, rendering decisions
2. **Blazor Render Tree** — DOM diffing and Razor component hierarchy
3. **Browser JavaScript** — scroll measurement, DOM layout, keyboard coordination

The architecture is organized into four horizontal layers:

```
┌─────────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                               │
│  Internal/Renderer/*.razor  — CellRender, GridRow, GridHeader, ...  │
│  Internal/Editors/*.razor   — NormalEdit, DialogEdit, BatchEdit      │
│  SfGrid.razor (root shell)                                          │
├─────────────────────────────────────────────────────────────────────┤
│                    BUSINESS / ACTION LAYER                          │
│  Internal/Actions/          — 14 Feature Modules                    │
│  Sort<T>  Filter<T>  Group<T>  Edit<T>  Selection<T>                │
│  VirtualScroll<T>  InfiniteScroll<T>  FocusHandler<T>               │
│  Reorder<T>  RowReorder<T>  ForeignKey<T>  DetailRow<T>             │
│  ReactiveAggregate<T>  MergeHandler<T>                              │
├─────────────────────────────────────────────────────────────────────┤
│                    DATA LAYER                                       │
│  Internal/Actions/Data.cs   — DataGenerator<T>                      │
│  Syncfusion.Blazor.Data     — SfDataManager, Query, Adaptors        │
│  Internal/Base/Utils.cs     — GridUtils (column flattening, helpers) │
├─────────────────────────────────────────────────────────────────────┤
│                    INFRASTRUCTURE LAYER                             │
│  SfDataBoundComponent (base class from ej2-base)                    │
│  GridJSInteropAdaptor<T>    — JS Interop bridge                     │
│  EventAggregator            — internal pub-sub messaging            │
│  PropertyInfoHelper<T>      — reflection-based model access         │
│  Annotation/                — data annotation reading               │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Layer 1: Infrastructure Layer

### SfDataBoundComponent (Base Class)
Every grid property that accepts external data or settings inherits from `SfDataBoundComponent` — a Syncfusion base class providing:

- **`PropertyChanges`** — `Dictionary<string, object>` tracking which `[Parameter]` values changed in the current render cycle
- **`UpdateProperty<T>`** — async method comparing old/new parameter values, recording diffs
- **`DataManager`** — internal `SfDataManager` instance for data fetching
- **`SetDataManager<T>`** — configures the correct adaptor for the current data source
- **`IsRendered`** — flag indicating the component has completed first render

This base class is the foundation for Blazor's change detection integration. Without it, the grid cannot know which parameters changed and which modules to notify.

### GridJSInteropAdaptor\<T\>
**File**: `Internal/Base/GridJSInteropAdaptor.cs`

The interop bridge between C# and the `sfBlazor.Grid.*` JavaScript module. Key responsibilities:
- Provides a stable `DotNetObjectReference` for JS-to-.NET callbacks via `GetRef()`
- Wraps all `InvokeMethod` calls with grid-specific context (`DataId`)
- Initialized once via `_jsAdaptor.Init()` in `OnAfterScriptRendered`
- Handles scroll events, DOM size measurements, and keyboard state from JS
- Manages initialization of client-side grid state via `sfBlazor.Grid.initialize`
- Disposed on component teardown, detaching all JS listeners and releasing the `DotNetObjectReference`

### EventAggregator
An internal pub-sub service used for **cross-module communication** without tight coupling. Examples:
- `EventAggregator.Trigger("InitialLoad", this)` — fired when data is first needed
- `EventAggregator.Trigger("VirtualComponentUpdate", null)` — fired to update virtual scroll after row height detection
- `EventAggregator.Trigger("InternalDataBound", null)` — fired after data render completes

### PropertyInfoHelper\<T\>
Reflection helper for accessing `TValue` model properties by name without repeated `typeof(T).GetProperty()` calls. Caches `PropertyInfo` instances for performance.

---

## Layer 2: Data Layer

### DataGenerator\<T\>
**File**: `Internal/Actions/Data.cs`

The central query builder. Called once per data request to produce a complete `Query` object:

```
DataGenerator<T>.GenerateQuery()
  ├── ColumnQuery   → Select() based on ColumnQueryMode
  ├── FilterQuery   → Where() from FilterSettings.Columns
  ├── SearchQuery   → Search() from SearchSettings.Key
  ├── AggregateQuery → Aggregate() from GridAggregates
  ├── SortQuery     → SortBy() from SortSettings.Columns
  ├── PageQuery     → Skip() + Take() for current page
  └── GroupQuery    → Group() from GroupSettings.Columns
```

This query is then passed to `SfDataManager` which routes it to the appropriate adaptor (in-memory, REST API, OData, etc.).

### SfDataManager + Adaptors
`Syncfusion.Blazor.Data.SfDataManager` is the data abstraction layer. It accepts a `Query` and produces `IEnumerable<object>` results via:

| Adaptor | Scenario |
|---------|----------|
| `BlazorAdaptor` | Local `IEnumerable<TValue>` |
| `WebApiAdaptor` | ASP.NET Core Web API |
| `ODataV4Adaptor` | OData v4 endpoints |
| `CustomAdaptor` | Developer-defined fetch logic |
| `GraphQLAdaptor` | GraphQL queries |
| `UrlAdaptor` | Generic REST endpoint |

### GridUtils
**File**: `Internal/Base/Utils.cs`

Static utility class providing:
- `GetColumns(SfGrid<T>)` — flattens stacked/nested columns to a flat list
- `GetForeignKeyColumns(List<GridColumn>)` — returns all foreign key columns
- `IsRefreshable(string propertyName)` — determines if a property change triggers a data reload

---

## Layer 3: Business / Action Layer

### Module Injection Pattern

All **15 feature modules** are **instantiated in `OnInitializedAsync`** and held as instance fields on `SfGrid<TValue>`. The `_jsAdaptor` and `PropHelper` infrastructure objects are also constructed at this point:

```csharp
// From SfGrid.Lifecycle.cs — OnInitializedAsync (actual source order)
DetailRowModule         = new DetailRow<TValue>(this);
ReorderModule           = new Reorder<TValue>(this);
DataModule              = new DataGenerator<TValue>(this);
ReactiveAggregateModule = new ReactiveAggregate<TValue>(this);
ForeignKeyModule        = new ForeignKey<TValue>(this);
VirtualScrollModule     = new VirtualScroll<TValue>(this);
InfiniteScrollModule    = new InfiniteScroll<TValue>(this);
SortModule              = new Sort<TValue>(this);
GroupModule             = new Grouping<TValue>(this);
FilterModule            = new Filter<TValue>(this);
SelectionModule         = new Selection<TValue>(this);
EditModule              = new Edit<TValue>(this);
FocusModule             = new FocusHandler<TValue>(this);
RowReorderModule        = new RowReorder<TValue>(this);
_jsAdaptor              = new GridJSInteropAdaptor<TValue>(this);
PropHelper              = new PropertyInfoHelper<TValue>();
ScriptModules           = SfScriptModules.SfGrid;
MergeModule             = new MergeHandler<TValue>(this);
```

Each module receives `this` (the `SfGrid<TValue>` instance) as its `Parent` reference. This is the **service locator pattern** — modules access sibling modules via `Parent.SortModule`, `Parent.FilterModule`, etc.

### Module Responsibilities

| Module | File | Responsibility |
|--------|------|----------------|
| `DataGenerator<T>` | `Actions/Data.cs` | Query building, data fetch orchestration |
| `Sort<T>` | `Actions/Sort.cs` | Sort state management, multi-sort, programmatic sort |
| `Filter<T>` | `Actions/Filter.cs` | Filter UI, filter query building, Excel/Menu/Checkbox filter |
| `Grouping<T>` | `Actions/Group.cs` | Group drag-drop, lazy loading, expand/collapse |
| `Edit<T>` | `Actions/Edit.cs` | CRUD operations, form management, validation |
| `Selection<T>` | `Actions/Selection.cs` | Row/cell/checkbox selection, persist selection |
| `VirtualScroll<T>` | `Actions/VirtualScroll.cs` | Row and column virtualization, scroll offset math |
| `InfiniteScroll<T>` | `Actions/InfiniteScroll.cs` | Load-on-demand pagination at scroll end |
| `FocusHandler<T>` | `Actions/FocusHandler.cs` | Keyboard navigation, ARIA focus management |
| `Reorder<T>` | `Actions/Reorder.cs` | Column drag-and-drop reorder |
| `RowReorder<T>` | `Actions/RowReorder.cs` | Row drag-and-drop within/across grids |
| `ForeignKey<T>` | `Actions/ForeignKey.cs` | Foreign key display value resolution |
| `DetailRow<T>` | `Actions/DetailRow.cs` | Expand/collapse detail rows |
| `ReactiveAggregate<T>` | `Actions/ReactiveAggregate.cs` | Live aggregate recomputation |
| `MergeHandler<T>` | `Actions/MergeHandler.cs` | AutoSpan cell merging logic |

---

## Layer 4: Presentation Layer

### Razor Component Tree

```
SfGrid<TValue>  (root — SfGrid.razor)
├── GridToolbar.razor
├── GroupDropArea.razor
├── GridHeader.razor
│   ├── GroupedHeader.razor        (stacked/grouped headers)
│   └── GridHeaderCell.razor       (per-column header)
│       ├── ColumnMenu.razor
│       └── [SortIcon, FilterIcon]
├── GridContent.razor              (standard scroll container)
│   ├── GridRow.razor              (per data row)
│   │   └── CellRender.razor       (per cell)
│   │       ├── CheckBoxRenderer.razor
│   │       ├── CommandColumnRenderer.cs
│   │       ├── ExpandCellRenderer.cs
│   │       └── [Template rendering]
│   ├── FooterContent.razor        (aggregate footer rows)
│   └── RefreshAggregate.razor
├── GridVirtualContent.razor       (virtual scroll container — only when EnableVirtualization)
│   └── GridVirtualHeader.razor
├── GridDetailRow.razor            (detail row container)
├── [Edit Renderers]
│   ├── NormalEdit.razor
│   ├── DialogEdit.razor
│   ├── BatchEdit.razor
│   └── GridAddNewRow.razor
├── [Dialogs and Overlays]
│   ├── ColumnChooser.razor
│   ├── ContextMenu.razor
│   ├── GridTooltip.razor
│   ├── ValidationDialog.razor
│   └── ValidationTooltip.razor
├── [Adaptive]
│   └── AdaptiveDialogRenderer.razor
├── [Filter UI]
│   └── Filter/  (filter bar, Excel filter, menu filter, checkbox filter)
├── WidthController.razor          (column width enforcement)
├── Preloader.razor                (loading spinner)
├── EventRegister.razor            (JS event binding)
├── EventRegisterAsync.razor
└── PrintLayout.razor              (print-mode DOM)
```

### Rendering Strategy

The grid uses **server-driven rendering** — Blazor generates HTML server-side (Server) or in WASM, and the browser DOM is updated via Blazor's diffing algorithm. This means:

1. **All data logic runs in .NET** — no client-side data filtering or sorting
2. **DOM updates are minimal** — Blazor only patches changed nodes
3. **JS handles scroll and layout** — `sfBlazor.Grid.js` measures DOM, computes scroll offsets, and notifies .NET
4. **Virtual scroll coordinates** — JS fires scroll events → .NET computes new visible row range → .NET re-renders rows

---

## Event Flow Architecture

```
User Action (click sort header)
        │
        ▼
[JS Event: sfBlazor.Grid.onHeaderClick]
        │
        ▼
[GridJSInteropAdaptor.InvokeFromJS("sortColumn", args)]
        │
        ▼
[SfGrid.SortColumn(field, direction)]
        │
        ▼
[Sort<T>.SortColumn → updates SortSettings]
        │
        ▼
[GridEvents.OnActionBegin raised → user handler]
        │
        ▼ (if not cancelled)
[DataGenerator<T>.GenerateQuery() → SortQuery applied]
        │
        ▼
[SfDataManager.ExecuteQuery()]
        │
        ▼
[Data returned → SfGrid.CurrentViewData updated]
        │
        ▼
[StateHasChanged() → Blazor re-render triggered]
        │
        ▼
[GridContent.razor + GridRow.razor re-render]
        │
        ▼
[GridEvents.OnActionComplete raised → user handler]
```

---

## Module Communication Patterns

### Pattern 1: Parent Reference (Synchronous)
Modules access sibling state through `Parent`:
```csharp
// Inside Filter<T>
var sortedColumns = Parent.SortModule?.SortedColumns;
```

### Pattern 2: EventAggregator (Asynchronous)
Cross-cutting concerns use pub-sub:
```csharp
// Inside VirtualScroll<T>
Parent.EventAggregator.Trigger("VirtualComponentUpdate", null!);
```

### Pattern 3: Lifecycle Hooks (Orchestrated)
The grid lifecycle (`OnInitializedAsync` → `OnParametersSetAsync` → `OnAfterRenderAsync`) coordinates module initialization and updates in a defined order.

### Pattern 4: ActionArgs Pipeline
Grid actions pass through an `ActionArgs` pipeline allowing pre-action cancellation via `GridEvents.OnActionBegin`:
```
OnActionBegin(args) → if (!args.Cancel) → Execute → OnActionComplete(args)
```

---

## JavaScript Integration Architecture

```
Blazor C# (SfGrid<T>)
    │
    │ InvokeMethod("sfBlazor.Grid.initialize", ...)
    ▼
sfBlazor.Grid.js (client)
    │
    │ DotNetObjectReference callbacks
    │ (scroll events, key events, DOM measurements)
    ▼
GridJSInteropAdaptor<T>
    │
    │ Dispatches to appropriate module
    ▼
SfGrid<T> action handling
```

Key JS functions called:
| Function | Caller | Purpose |
|----------|--------|---------|
| `sfBlazor.Grid.initialize` | `OnAfterScriptRendered` | Initial grid setup — returns `rowHeight`, `indentWidth`, `isMac` |
| `sfBlazor.Grid.virtualDisconnect` | `OnParametersSetAsync` | Clean up virtual scroll DOM when `EnableVirtualization` toggled off |
| `sfBlazor.Grid.autoFitColumns` | `OnAfterScriptRendered` (persistence) | Column width auto-calculation after persistence restore |
| `sfBlazor.Grid.refreshGridPageSize` | `OnAfterScriptRendered` | Pager height recalculation when `Height = "100%"` |
| `window.localStorage.getItem` | `OnAfterScriptRendered` | State persistence read (key: `"grid{ID}"`) |

---

## TypeScript File Configuration

All client-side logic lives in `scripts/`. TypeScript is compiled to `scripts/.tmp/` by `tsc` then bundled by Rollup into `scripts/modules/sf-grid.js` (the file loaded by the Blazor component).

### File Map

| File | Role | Imports |
|------|------|---------|
| `index.ts` | Entry point — re-exports everything from `sf-grid.ts` | `sf-grid.ts` |
| `sf-grid.ts` | Public `sfBlazor.Grid.*` API surface (the `Grid` object registered with Blazor) | `sf-grid-fn.ts`, `util.ts`, `interfaces.ts`, `width-controller.ts` |
| `sf-grid-fn.ts` | `SfGrid` client class — owns all module instances, lifecycle, scroll wiring | All feature modules |
| `interfaces.ts` | Shared TypeScript interfaces: `IGridOptions`, `Column`, `BlazorGridElement`, `InitModulesResults`, `VirtualInfo`, `InterSection`, `FreezeLineMovingClientOptions`, etc. | `virtual-scroll.ts` (for `SentinelType`, `Offsets`) |
| `util.ts` | Pure utility functions: `parentsUntil`, `iterateArrayOrObject`, `getRowHeight`, `getScrollBarWidth`, `getCellByRowUidAndColIndex`, etc. | — |
| `scroll.ts` | Scroll event handling, frozen scroll sync, padding calculation | `interfaces.ts`, `util.ts` |
| `freeze.ts` | Frozen column height sync, text-wrap row height refresh, resize handler | `interfaces.ts`, `util.ts` |
| `virtual-scroll.ts` | `VirtualContentRenderer` + `VirtualHeaderRenderer` — viewport offset math, translateY transform, column virtual scroll, mask row | `interfaces.ts`, `intersection-observer.ts`, `util.ts` |
| `intersection-observer.ts` | IntersectionObserver wrapper used by virtual scroll sentinel detection | — |
| `infinite-scroll.ts` | Infinite scroll block cache, scroll-end detection, lazy group child loading | `interfaces.ts`, `util.ts` |
| `selection.ts` | Row/cell selection, auto-fill border, box selection | `interfaces.ts`, `util.ts` |
| `edit.ts` | Edit tooltip creation, cell focus after save/cancel | `interfaces.ts`, `util.ts` |
| `filter.ts` | Filter popup positioning | `interfaces.ts`, `util.ts` |
| `group.ts` | Group drag-drop, group drop area interactions | `interfaces.ts`, `util.ts` |
| `reorder.ts` | Column reorder drag-drop, index-based reorder, field-based reorder | `interfaces.ts`, `util.ts` |
| `resize.ts` | Column resize drag, auto-fit columns, resize cursor helper | `interfaces.ts`, `util.ts`, `width-controller.ts` |
| `width-controller.ts` | `ColumnWidthService` — sets pixel/percentage widths on `<table>` and `<col>` elements, persisted width restore | `interfaces.ts` |
| `header-drag-drop.ts` | Header cell drag-drop coordination for column reorder | `interfaces.ts`, `util.ts` |
| `content-drag-drop.ts` | Content area drag-drop (row drag) | `interfaces.ts`, `util.ts` |
| `frozen-drag-drop.ts` | Freeze-line drag to reposition the frozen column boundary | `interfaces.ts`, `util.ts` |
| `row-reorder.ts` | Row drag-and-drop (`RowDD`) — within-grid and cross-grid | `interfaces.ts`, `util.ts` |
| `column-chooser.ts` | Column chooser dialog position calculation, media column management | `interfaces.ts`, `util.ts` |
| `column-menu.ts` | Column menu popup positioning | `interfaces.ts`, `util.ts` |
| `clipboard.ts` | Copy-to-clipboard (with/without header), paste action | `interfaces.ts`, `util.ts` |
| `tooltip.ts` | `CustomToolTip` — cell/header tooltip positioning and display | `interfaces.ts`, `util.ts` |

### Module Dependency Tree

```
index.ts
  └── sf-grid.ts  (sfBlazor.Grid.* public API)
        ├── sf-grid-fn.ts  (SfGrid class)
        │     ├── scroll.ts           (scrollModule)
        │     ├── freeze.ts           (freezeModule)
        │     ├── virtual-scroll.ts   (virtualContentModule, virtualHeaderModule)
        │     │     └── intersection-observer.ts
        │     ├── infinite-scroll.ts  (infiniteScrollModule)
        │     ├── selection.ts        (selectionModule)
        │     ├── edit.ts             (editModule)
        │     ├── filter.ts           (filterModule)
        │     ├── group.ts            (groupModule)
        │     ├── reorder.ts          (reorderModule)
        │     ├── resize.ts           (resizeModule)
        │     │     └── width-controller.ts
        │     ├── header-drag-drop.ts (headerDragDrop)
        │     ├── content-drag-drop.ts(contentDragDrop)
        │     ├── frozen-drag-drop.ts (frozenDragDropModule)
        │     ├── row-reorder.ts      (rowDragAndDropModule)
        │     ├── column-chooser.ts   (columnChooserModule)
        │     ├── column-menu.ts      (columnMenuModule)
        │     ├── clipboard.ts        (clipboardModule)
        │     ├── tooltip.ts          (toolTipModule)
        │     └── interfaces.ts
        ├── util.ts
        ├── interfaces.ts
        └── width-controller.ts
```
---

## State Management Model

The grid does not use a centralized state store (no Redux/Flux pattern). Instead, state lives in:

| State Type | Location | Scope |
|------------|----------|-------|
| Parameter values | `SfGrid<TValue>` properties | Grid lifetime |
| Property change tracking | `PropertyChanges` dictionary | Per render cycle |
| Module-local state | Each module's private fields | Module lifetime |
| Current view data | `SfGrid.CurrentViewData` | Per data request |
| Virtual scroll offsets | `VirtualScroll<T>` private fields | Scroll session |
| Selection state | `Selection<T>` internal collections | Grid lifetime |
| Edit state | `Edit<T>` internal state | Edit session |

---

## Initialization Sequence

```
1. OnInitializedAsync()
   ├── OnHybridInitialized()
   │     ├── base.OnInitializedAsync()
   │     ├── Copy all [Parameter] values → private backing fields (_sort, _filter, ...)
   │     ├── Generate auto ID if not provided (sfgrid + random filename token)
   │     └── Set IsAutoGeneratedColumns = true if Columns != null
   ├── Construct all 15 action modules (in source order — see Module Injection Pattern)
   ├── Construct _jsAdaptor and PropHelper
   ├── Set ScriptModules = SfScriptModules.SfGrid
   ├── Init ColumnMenuClass CSS string
   └── _isLoaded = true

2. OnParametersSetAsync()
   ├── Handle UnMatchedAttributes → _cachedAttributes (when TableClass = false)
   ├── Wire/unwire ObservableCollection events on DataSource reference change
   ├── OnHybridParametersSet()
   │     ├── base.OnParametersSetAsync()
   │     ├── UpdateProperty() for every [Parameter] → records in PropertyChanges
   │     └── SetDataManager<TValue>() → configures adaptor
   ├── Query equality check → removes false "Query" change if Query.IsEqual()
   ├── DataSource change → reset IsSelected, CheckBoxState, persist selection
   ├── Detect refreshable property changes
   │     ├── headerRef = AllowGrouping/GroupSettings/AllowSorting/SortSettings/
   │     │               AllowRowDragAndDrop/Columns/AllowFiltering/ShowColumnMenu changed
   │     ├── FrozenColumns/Width/Height → RefreshFrozenHeader = true
   │     ├── EnableVirtualization toggle off → InvokeMethod("sfBlazor.Grid.virtualDisconnect")
   │     ├── SelectedRowIndex → _rowIndexPropertyChanged = true
   │     └── SelectionMode change → ClearRowSelectionAsync / ClearCellSelectionAsync
   ├── PropertyChanges.Clear()
   └── If refreshable → ModelChanged(RequestType = Refresh)

3. OnAfterRenderAsync(firstRender)
   ├── If isGridModelRefresh → InvokeAsync(ModelChanged(Refresh))
   ├── If _requireDataBoundInvoke && IsClientInitialized → DataBound event
   ├── If AddOrDeleteArgs != null → EditModule.EditComplete()
   ├── Reset flags: HasColumnChanges, IsColumnHideOrShow, SoftRefresh
   ├── SetColumnValueType()
   ├── Handle grouped template column visibility (ShowGroupedColumn path)
   ├── FirstRender: capture _originalProp via SerializeModel(this)
   └── base.OnAfterRenderAsync() → triggers OnAfterScriptRendered() on firstRender

4. OnAfterScriptRendered()
   ├── _jsAdaptor.Init()                              ← establish DotNetObjectReference
   ├── _hasSpinner = true
   ├── EventAggregator.Trigger("InitialLoad", this)
   ├── GridEvents.OnLoad.InvokeAsync()
   ├── InvokeMethod("sfBlazor.Grid.initialize", DataId, element, options, ref, focusArgs)
   │     Returns: { RowHeight, IndentWidth, IsRowDragCell, IsMacDevice }
   ├── Apply initializeResults:
   │     ├── IsMacDevice = result.IsMacDevice
   │     ├── RefreshIndentWidth(IndentWidth) if present
   │     └── VirtualScrollModule.RHeight + "VirtualComponentUpdate" if virtualizing
   ├── EnablePersistence → read localStorage["grid{ID}"] → PersistProperties()
   ├── DataProcess()                                   ← FIRST DATA LOAD
   ├── SetColumnValueType()
   ├── EventAggregator.Trigger("InternalDataBound")
   ├── GridEvents.Created.InvokeAsync()
   ├── GridEvents.DataBound.InvokeAsync() (if pending)
   ├── IsClientInitialized = true
   ├── RefreshPivotRowHeight → InvokeMethod("sfBlazor.Grid.refreshPivotRowHeight")
   └── Height="100%" + PageSizes → InvokeMethod("sfBlazor.Grid.refreshGridPageSize")
```

---

## Thread Safety and Async Patterns

- All Blazor lifecycle methods are `async Task` and run on the Blazor synchronization context
- All `InvokeMethod` calls use `.ConfigureAwait(true)` to ensure continuation on the Blazor context
- `await Task.Yield()` is used before `DataBound` to give the client time to initialize
- `await Task.Run(() => { }).ConfigureAwait(true)` used in Grid-rendered path of `OnAfterRenderAsync` to allow server-side async continuation
- Observable collection handlers dispatch back to the Blazor context via `InvokeAsync`
- `isGridModelRefresh` deferred flag ensures `ModelChanged(Refresh)` runs via `InvokeAsync` on the next tick (avoids re-entrancy in the render cycle)

---

## Key Internal Flags

| Flag | Type | Set When | Cleared When |
|------|------|----------|-------------|
| `IsClientInitialized` | `bool` | End of `OnAfterScriptRendered` | Never (lifetime flag) |
| `IsDataLoaded` | `bool` | First `DataProcess()` call | Never |
| `IsRendered` | `bool` | Base class after first render | Never |
| `_requireDataBoundInvoke` | `bool` | After `DataProcess()` completes | After `DataBound` event fires |
| `isGridModelRefresh` | `bool` | Property change needing deferred refresh | Start of next `OnAfterRenderAsync` |
| `RefreshColumnHeader` | `bool` | Column/group/sort/filter property change | After render cycle |
| `RefreshFrozenHeader` | `bool` | Frozen/virtualization/size property change | After render cycle |
| `HasColumnChanges` | `bool` | Columns parameter changed | End of `OnAfterRenderAsync` |
| `SoftRefresh` | `bool` | Lightweight re-render (no data reload) | End of `OnAfterRenderAsync` |
| `_rowIndexPropertyChanged` | `bool` | `SelectedRowIndex` parameter change | After `SelectRow()` in `OnAfterRenderAsync` |
| `VirtualScrollModule.IsDataSourceChanged` | `bool` | `DataSource` changed while rendered | After virtual scroll reset |

---

*For component hierarchy details, see [`architecture/component-architecture.md`](./component-architecture.md).*  
*For data flow diagrams, see [`architecture/data-flow.md`](./data-flow.md).*  
*For module dependencies, see [`architecture/dependency-map.md`](./dependency-map.md).*
