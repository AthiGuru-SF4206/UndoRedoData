# Architecture Overview — Syncfusion Blazor DataGrid

> **Audience**: New developers, freshers  
> **Module**: 01 — Getting Started  
> **Time Required**: 60 minutes  
> **Prerequisites**: [`../00-START-HERE.md`](../00-START-HERE.md), [`../../overview/product-overview.md`](../../overview/product-overview.md)  
> **Deep Reference**: [`../../architecture/system-architecture.md`](../../architecture/system-architecture.md)  
> **Last Updated**: March 12, 2026

---

## Overview

The Syncfusion Blazor DataGrid is not a simple table component. It is a **layered, module-injected, hybrid Blazor component** that operates across three execution environments simultaneously:

- **.NET (Server or WASM)** — all data operations, state management, rendering decisions
- **Blazor Render Tree** — component hierarchy, parameter diffing, incremental DOM updates
- **Browser JavaScript** — scroll measurement, DOM layout, keyboard coordination, focus management

Understanding how these three environments collaborate is the most important architectural concept for any DataGrid developer.

---

## The 4-Layer Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│  LAYER 4 — PRESENTATION LAYER                                       │
│  Internal/Renderer/*.razor — CellRender, GridRow, GridHeader, ...   │
│  Internal/Editors/*.razor  — NormalEdit, DialogEdit, BatchEdit       │
│  Internal/SfGrid.razor     — root render shell                      │
├─────────────────────────────────────────────────────────────────────┤
│  LAYER 3 — BUSINESS / ACTION LAYER                                  │
│  Internal/Actions/ — 14 feature modules                             │
│  Sort  Filter  Group  Edit  Selection  VirtualScroll                │
│  InfiniteScroll  FocusHandler  Reorder  RowReorder                  │
│  ForeignKey  DetailRow  ReactiveAggregate  MergeHandler             │
├─────────────────────────────────────────────────────────────────────┤
│  LAYER 2 — DATA LAYER                                               │
│  Internal/Actions/Data.cs — DataGenerator<T>                        │
│  Syncfusion.Blazor.Data   — SfDataManager, Query, Adaptors          │
│  Internal/Base/Utils.cs   — GridUtils (column helpers)              │
├─────────────────────────────────────────────────────────────────────┤
│  LAYER 1 — INFRASTRUCTURE LAYER                                     │
│  SfDataBoundComponent     — base class, property change tracking    │
│  GridJSInteropAdaptor<T>  — JS-interop bridge                       │
│  EventAggregator          — internal pub-sub messaging              │
│  PropertyInfoHelper<T>    — reflection-based model access           │
└─────────────────────────────────────────────────────────────────────┘
```

Each layer has a strict responsibility boundary. Code in the Presentation Layer does not call data operations. Code in the Data Layer does not call JS-interop. Violations of this boundary are architectural bugs.

---

## Layer 1 — Infrastructure Layer

### SfDataBoundComponent (Base Class)
Every grid parameter component inherits from `SfDataBoundComponent`. This Syncfusion base class provides:

- **`PropertyChanges`**: `Dictionary<string, object>` — tracks which `[Parameter]` values changed in the current render cycle
- **`UpdateProperty<T>`**: async method that compares old and new parameter values and records diffs
- **`IsRendered`**: `bool` — `true` after the first render cycle completes

This is how the grid implements **incremental updates** — instead of re-executing all 14 action modules on every `StateHasChanged`, only the modules whose relevant parameters changed are notified.

### GridJSInteropAdaptor\<T\> — The JS Bridge
**File**: `Internal/Base/GridJSInteropAdaptor.cs`

This is the **single entry point** for all communication between C# and the browser. No action module calls `JSRuntime.InvokeAsync` directly. All JS calls go through this adaptor.

Responsibilities:
- Manages a stable `DotNetObjectReference` for JS-to-.NET callbacks
- Wraps all outbound calls with grid-specific context (`DataId`)
- Handles inbound data from JS: scroll position, DOM sizes, keyboard events
- Initializes the client-side grid state once via `sfBlazor.Grid.initialize`
- Disposes all JS listeners and observers on component teardown

> **Key principle**: JS-interop is scoped exclusively to DOM-dependent operations. Data, sorting, filtering, grouping, paging — all pure C#.

### EventAggregator — Internal Pub-Sub
Enables cross-module communication without direct coupling. Action modules never call each other's methods directly. Instead:

```csharp
// Sort module fires an event after sorting completes
EventAggregator.Trigger("DataBound", this);

// Aggregate module listens for this event and recalculates
EventAggregator.On("DataBound", OnDataBound);
```

This is how 14 feature modules coexist without forming a dependency web.

---

## Layer 2 — Data Layer

### DataGenerator\<T\>
**File**: `Internal/Actions/Data.cs`

The central data pipeline. When the grid needs to display data, `DataGenerator<T>` is responsible for:

1. Accepting the raw `DataSource` or `SfDataManager` reference
2. Applying the current `Query` (sort, filter, group, page, search predicates)
3. Executing the query against the local collection or remote adaptor
4. Returning a typed result object with `Result` (rows) and `Count` (total)

The data pipeline flow:
```
User action (e.g., sort click)
    → Action module updates Query predicates
    → DataGenerator<T>.GenerateQuery()
    → SfDataManager.ExecuteQuery(query)
    → Adaptor executes (local Array adaptor or remote OData/REST adaptor)
    → Result<T> returned
    → DataGenerator fires "DataBound" via EventAggregator
    → Renderers re-render with new data
```

---

## Layer 3 — Business / Action Layer

The 14 action modules in `Internal/Actions/` are the heart of the grid's feature set. Each module is responsible for exactly one feature area.

| Module Class | Feature | Key File |
|-------------|---------|---------|
| `Sort<T>` | Single and multi-column sorting | `Actions/Sort.cs` |
| `Filter<T>` | Filter bar, Excel, Menu, Checkbox | `Actions/Filter.cs` |
| `Group<T>` | Grouping, collapse/expand, lazy group | `Actions/Group.cs` |
| `Edit<T>` | Inline, Dialog, Batch editing, CRUD | `Actions/Edit.cs` |
| `Selection<T>` | Row, cell, checkbox selection | `Actions/Selection.cs` |
| `VirtualScroll<T>` | Row and column virtualization | `Actions/VirtualScroll.cs` |
| `InfiniteScroll<T>` | Infinite scroll on-demand loading | `Actions/InfiniteScroll.cs` |
| `FocusHandler<T>` | Keyboard focus coordination | `Actions/FocusHandler.cs` |
| `Reorder<T>` | Column reorder (drag-and-drop) | `Actions/Reorder.cs` |
| `RowReorder<T>` | Row drag-and-drop within/between grids | `Actions/RowReorder.cs` |
| `ForeignKey<T>` | Foreign key column data lookup | `Actions/ForeignKey.cs` |
| `DetailRow<T>` | Expandable detail rows | `Actions/DetailRow.cs` |
| `ReactiveAggregate<T>` | Live aggregate recalculation | `Actions/ReactiveAggregate.cs` |
| `MergeHandler<T>` | Auto cell spanning (row/col merge) | `Actions/MergeHandler.cs` |

### Module Injection Pattern
Modules are not instantiated unconditionally. They are injected via the internal `ServiceLocator` only when the corresponding feature is enabled:

```csharp
// Example: Sort module is only injected when AllowSorting = true
if (AllowSorting)
{
    ServiceLocator.RegisterService<Sort<TValue>>(new Sort<TValue>(this));
}
```

This is why a grid with only `AllowPaging = true` has a smaller memory footprint than a fully-featured grid — unused modules are never allocated.

---

## Layer 4 — Presentation Layer

The `Internal/Renderer/` folder contains 30+ Razor components that each render one piece of the grid UI. They receive data via `[CascadingParameter]` or direct property binding from the parent grid and have no business logic.

Key renderers:
- `GridHeaderCellRenderer` — renders `<th>` elements for column headers
- `GridRowRenderer` — renders a single `<tr>` data row
- `GridCellRenderer` — renders a single `<td>` cell with template or value
- `GridPagerRenderer` — renders the pager component
- `GridToolbarRenderer` — renders the toolbar
- `GridGroupDropAreaRenderer` — renders the group drop zone
- `GridVirtualContentRenderer` — renders the virtual scroll container

---

## The Rendering Lifecycle

```
OnInitializedAsync
    → Build column model (GridColumn list)
    → Register modules via ServiceLocator
    → Initialize DataGenerator<T>

OnAfterRenderAsync (firstRender = true)
    → Import sf-grid.js module via IJSRuntime
    → Call sfBlazor.Grid.initialize (JS creates scroll/focus listeners)
    → Trigger "InitialLoad" via EventAggregator
    → DataGenerator fetches first page of data

OnAfterRenderAsync (subsequent renders)
    → Check PropertyChanges dictionary
    → Notify only affected modules
    → Modules update their state
    → Re-render only affected renderer subtrees

Dispose
    → Call sfBlazor.Grid.destroy (JS removes all listeners)
    → Dispose GridJSInteropAdaptor (releases DotNetObjectReference)
    → Unsubscribe all EventAggregator subscriptions
```

---

## JS-Interop: What Goes to JavaScript

Only operations that require DOM measurement or direct browser interaction use JS-interop. Examples:

| Operation | Why JS Is Needed |
|-----------|-----------------|
| Column auto-fit | Must measure rendered `<th>` cell widths |
| Scroll position tracking | Must read `scrollLeft` / `scrollTop` from DOM |
| Virtual row height calculation | Must measure actual rendered row heights |
| Keyboard focus | Must call `element.focus()` for specific cells |
| Drag/resize tracking | Must listen to `pointermove` and `pointerup` |
| Frozen column line moving | Must track pointer delta for line position |

Everything else — sorting, filtering, paging, grouping, editing logic, validation — is pure C# in the action modules.

---

## How Parameters Flow

```
User sets AllowSorting = true in .razor page
    → SfGrid<TValue>.SetParametersAsync called
    → SfGrid<TValue>.UpdateProperty("AllowSorting", true) called
    → Stored in PropertyChanges dictionary
    → OnParametersSetAsync fires
    → Grid checks PropertyChanges for "AllowSorting"
    → If changed: re-initialize or reconfigure Sort<T> module
    → Sort<T>.PropertyChanged() called with new value
    → Render cycle triggered for affected column headers only
```

This incremental approach is what keeps the grid performant — most parameter changes do not require a full re-render.

---

## What You Must Understand Before Writing Code

Before making any change to the DataGrid source:

1. **Which layer does the change belong to?** (Infrastructure / Data / Business / Presentation)
2. **Which action module owns the feature?** (see module table above)
3. **Does the change affect the JS-interop bridge?** If yes, changes are required in both `GridJSInteropAdaptor.cs` and `sf-grid.js`
4. **Does the change affect `PropertyChanges` detection?** If yes, ensure the property name string matches exactly
5. **Is there an existing `EventAggregator` event for this flow?** Use it instead of creating a direct dependency

---

## Navigation

**Previous**: [`../00-START-HERE.md`](../00-START-HERE.md)  
**Next**: [`project-setup-guide.md`](./project-setup-guide.md)  
**Deep Dive**: [`../../architecture/system-architecture.md`](../../architecture/system-architecture.md)
