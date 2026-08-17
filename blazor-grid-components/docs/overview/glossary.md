# Glossary — Syncfusion Blazor DataGrid

> **Audience**: All developers and AI agents working on the SfGrid component  
> **Purpose**: Single source of terminology for consistent communication across code, docs, and reviews  
> **Last Updated**: March 11, 2026

All terms are listed alphabetically. Code symbols use `monospace`. Types use **Bold**.

---

## A

| Term | Definition |
|------|------------|
| **ActionArgs** | Internal model class (`ActionArgs`) that carries request type, row data, and action metadata during grid operations (sort, filter, edit, refresh, etc.). |
| **ActionEventArgs\<TValue\>** | The event argument type raised by grid actions. Contains `RequestType` (enum `Action`), `Data`, `RowData`, and cancel flag. |
| **Adaptive UI** | A full-screen layout mode activated by `EnableAdaptiveUI = true` that renders grid dialogs and actions optimized for mobile/tablet devices. |
| **AdaptiveMode** | Enum controlling which device types receive the adaptive layout: `Both`, `Mobile`, `Desktop`. |
| **Adaptor** | An abstraction layer in `SfDataManager` that translates grid query objects into HTTP requests or in-memory operations. Examples: `WebApiAdaptor`, `ODataV4Adaptor`, `BlazorAdaptor`, `CustomAdaptor`. |
| **Aggregate** | A summary computation (Sum, Average, Count, Min, Max, TrueCount, FalseCount, Custom) displayed in footer or group rows via `GridAggregate` / `GridAggregateColumn`. |
| **AllowFiltering** | Grid-level `[Parameter]` (`bool`) that enables the filter UI. Default: `false`. |
| **AllowGrouping** | Grid-level `[Parameter]` (`bool`) that enables column grouping via drag-and-drop. Default: `false`. |
| **AllowMultiSorting** | Grid-level `[Parameter]` (`bool`) that enables Shift+Click multi-column sorting. Default: `true`. |
| **AllowPaging** | Grid-level `[Parameter]` (`bool`) that enables the pager at the grid footer. Default: `false`. |
| **AllowResizing** | Grid-level `[Parameter]` (`bool`) that enables column resize by dragging column borders. Default: `false`. |
| **AllowSelection** | Grid-level `[Parameter]` (`bool`) that enables row/cell selection. Default: `true`. |
| **AllowSorting** | Grid-level `[Parameter]` (`bool`) that enables single-click column sort. Default: `false`. |
| **AllowTextWrap** | Grid-level `[Parameter]` (`bool`) that enables text wrap in cells when content exceeds column width. Default: `false`. |
| **AltRow** | Alternate row styling applied via `e-altrow` CSS class when `EnableAltRow = true`. |
| **Annotation** | Blazor component metadata system in `Internal/Annotation/` that reads data annotation attributes (`[DisplayName]`, `[Required]`, etc.) from `TValue` model for auto-configuration. |
| **ARIA** | Accessible Rich Internet Applications — W3C standard attributes (`aria-sort`, `aria-selected`, `aria-expanded`) applied to grid DOM elements for screen reader support. |
| **AutoFill** | Feature allowing users to copy cell values by dragging the AutoFill icon in Box cell selection mode. Requires `EnableAutoFill = true`, cell selection mode = Box, edit mode = Batch. |
| **AutoFit** | Grid-level `[Parameter]` (`bool`) that enforces column widths as defined without stretching. Default: `false`. |
| **AutoSpan** | Grid-level `[Parameter]` (`AutoSpanMode`) that controls automatic cell merging for identical adjacent values. Default: `AutoSpanMode.None`. |
| **AutoSpanMode** | Enum with values: `None`, `Row` (horizontal), `Column` (vertical), `HorizontalAndVertical`. |

---

## B

| Term | Definition |
|------|------------|
| **Batch Edit** | Edit mode where multiple cell changes are queued locally and submitted together. Activated by `GridEditSettings.Mode = EditMode.Batch`. |
| **BlazorAdaptor** | The default `SfDataManager` adaptor that processes `IEnumerable<TValue>` in-memory. Used when `DataSource` is a local collection. |
| **BUnit** | xUnit-based Blazor component testing framework used for unit and integration tests of grid rendering and behavior. |

---

## C

| Term | Definition |
|------|------------|
| **CaptionSummaryRenderer** | Internal renderer (`CaptionSummaryRenderer.cs`) that generates aggregate cells in group caption rows. |
| **Cell** | A single data cell in the grid, represented by the `Cell<T>` model class in `Internal/Models/`. |
| **CellRender** | Razor component (`CellRender.razor`) that renders individual grid data cells, applying format, templates, and clip mode. |
| **CheckBoxRenderer** | Razor component (`CheckBoxRenderer.razor`) that renders boolean columns as checkboxes using `DisplayAsCheckbox`. |
| **ChildContent** | `[Parameter]` on `SfGrid<TValue>` that accepts child Razor components (columns, settings, events) as `RenderFragment`. |
| **ClipMode** | Enum controlling overflow behavior: `Clip` (truncate), `Ellipsis` (show `…`), `EllipsisWithTooltip` (ellipsis + hover tooltip). Default: `Ellipsis`. |
| **Column** | See **GridColumn**. |
| **ColumnChooser** | A dialog allowing users to dynamically show/hide columns. Activated by `ShowColumnChooser = true`. |
| **ColumnMenu** | A per-column dropdown menu with actions (AutoFit, Sort, Group, Filter). Activated by `ShowColumnMenu = true`. |
| **ColumnQueryMode** | Enum that controls which fields are included in the server query: `All`, `Schema` (all defined columns), `ExcludeHidden` (only visible columns). |
| **ColumnVirtualization** | Feature that renders only horizontally visible columns in the DOM. Activated by `EnableColumnVirtualization = true`. |
| **CommandColumn** | A column containing action buttons (Edit, Delete, Save, Cancel, custom). Configured via `GridCommandColumn`. |
| **ContextMenu** | A right-click menu on rows/cells. Configured via `ContextMenuItems` property. |
| **CurrentViewData** | Internal property on `SfGrid<TValue>` holding the currently displayed data rows after paging/filtering/sorting. |

---

## D

| Term | Definition |
|------|------------|
| **DataAnnotations** | C# attribute namespace (`System.ComponentModel.DataAnnotations`) providing `[Required]`, `[StringLength]`, `[Range]`, `[DisplayName]` etc. The grid's `Annotation` system reads these to auto-configure column headers, validation rules, and editor constraints. |
| **DataBound** | Event (`GridEvents.DataBound`) fired after the grid has finished rendering its data for the first time or after a refresh. |
| **DataGenerator\<T\>** | Internal class (`Internal/Actions/Data.cs`) that builds the `Query` object for each data request, composing filter, sort, page, group, search, and aggregate queries. |
| **DataManager** | See **SfDataManager**. |
| **DataSource** | Grid-level `[Parameter]` (`IEnumerable<TValue>?`) for binding local data. Set to `null` when using `SfDataManager` child component. |
| **DetailRow** | Expandable child row below a parent row. Rendered via `GridDetailRow.razor`. Managed by `DetailRow<T>` module. |
| **Dialog Edit** | Edit mode where a modal dialog opens for record editing. Activated by `GridEditSettings.Mode = EditMode.Dialog`. |
| **DOM Measurement** | The process by which the JS module reads client-side layout values — `offsetHeight`, `offsetWidth`, `scrollTop`, `scrollLeft`, `getBoundingClientRect()` — and returns them to .NET. Used by virtualization, column resize, and freeze line positioning. |
| **DOMRect** | Browser object returned by `getBoundingClientRect()`, providing `top`, `left`, `width`, `height`, `right`, `bottom` of an element. Used by the JS module for column and row offset calculations. |
| **DragDropService** | Internal service class managing drag initiation, movement delta tracking, boundary enforcement, and drop completion for both row drag-and-drop and column reorder operations. Receives `pointermove` and `pointerup` events from the JS module. |
| **DynamicInfo** | Internal base class in `Internal/Base/DynamicInfo.cs` supporting dynamic data scenarios. |

---

## E

| Term | Definition |
|------|------------|
| **Edit Module** | `Edit<T>` class in `Internal/Actions/Edit.cs` that orchestrates all CRUD operations including inline, dialog, and batch editing. |
| **EditCell** | A cell in edit mode rendering an editor component (text, dropdown, date picker, checkbox). |
| **EditMode** | Enum for editing mode: `Normal` (inline row), `Dialog`, `Batch`, `Normal` with `ShowAddNewRow`. |
| **EditSettings** | See **GridEditSettings**. |
| **EnableAdaptiveUI** | `[Parameter]` (`bool`) that enables the adaptive full-screen UI layout. Default: `false`. |
| **EnableAltRow** | `[Parameter]` (`bool`) that enables alternate row highlighting. Default: `true`. |
| **EnableAutoFill** | `[Parameter]` (`bool`) that enables the AutoFill drag handle in box cell selection. Default: `false`. |
| **EnableColumnVirtualization** | `[Parameter]` (`bool`) that enables horizontal virtual scrolling of columns. Default: `false`. |
| **EnableHover** | `[Parameter]` (`bool`) that enables the `e-hover` CSS class on hovered rows. Default: `true`. |
| **EnableInfiniteScrolling** | `[Parameter]` (`bool`) that enables infinite scroll (load-on-demand at scroll end). Default: `false`. |
| **EnablePersistence** | `[Parameter]` (`bool`) that stores grid state in `window.localStorage`. Default: `false`. |
| **EnableRtl** | `[Parameter]` (`bool`) that renders the grid in right-to-left layout. Default: `false`. |
| **EnableStickyHeader** | `[Parameter]` (`bool`) that fixes column headers during vertical scroll. Default: `false`. |
| **EnableVirtualization** | `[Parameter]` (`bool`) that enables row virtualization (renders only visible rows). Default: `false`. |
| **EnableVirtualMaskRow** | `[Parameter]` (`bool`) that shows loading placeholder rows during virtual scroll data fetch. Default: `false`. |
| **EventAggregator** | Internal pub-sub service used for cross-module communication (e.g., `Trigger("InitialLoad", ...)`, `NotifyAsync("DataBoundMock", ...)`). |

---

## F

| Term | Definition |
|------|------------|
| **Filter Module** | `Filter<T>` class in `Internal/Actions/Filter.cs` managing all filter types (FilterBar, Excel, Menu, Checkbox). |
| **FilterBar** | Default filter mode showing a text input row below column headers. Activated by `GridFilterSettings.Type = FilterType.FilterBar`. |
| **FilterQuery** | Method on `DataGenerator<T>` that appends filter predicates to the `Query` object. |
| **FilterSettings** | See **GridFilterSettings**. |
| **FocusHandler\<T\>** | Internal module (`Internal/Actions/FocusHandler.cs`) managing keyboard navigation, focus trapping, and ARIA focus announcements. |
| **FocusService** | Internal service (within `FocusHandler<T>`) that handles programmatic focus requests. Receives `.NET → JS` calls to set DOM focus on a specific cell or element, and receives `JS → .NET` callbacks when focus changes due to user keyboard interaction. |
| **ForeignKey Column** | A column bound to a separate data source for display value lookup. Configured via `GridForeignColumn` with `ForeignDataSource` and `ForeignKeyValue`. |
| **FreezeDirection** | Enum controlling per-column freeze side: `Left`, `Right`, `None`. Used on `GridColumn.FreezeDirection`. Works in combination with the global `FrozenColumns`/`FrozenRows` parameters. |
| **FrozenColumns** | `[Parameter]` (`int`) specifying the count of columns frozen to the left. Default: `0`. |
| **FrozenRows** | `[Parameter]` (`int`) specifying the count of rows frozen at the top. Default: `0`. |

---

## G

| Term | Definition |
|------|------------|
| **Grid Content** | The scrollable body area of the grid rendered by `GridContent.razor`. |
| **Grid Header** | The fixed header row area rendered by `GridHeader.razor` containing column headers. |
| **GridAggregate** | Child component (`GridAggregate.razor`) defining an aggregate row with one or more `GridAggregateColumn` instances. |
| **GridAggregateColumn** | Child component defining the field, type, and template for a single aggregate cell. |
| **GridColumn** | Core column configuration class (`GridColumn.cs`, `Internal/Base/GridColumnBase.cs`) that defines Field, HeaderText, Width, Format, Template, IsPrimaryKey, AllowSorting, AllowFiltering, etc. |
| **GridColumnBase** | Partial base class (`Internal/Base/GridColumnBase.cs`) for `GridColumn` providing internal state like `ValueType`, `ActualType`, `IsGridForeignColumn`. |
| **GridEditSettings** | Child component / property class defining editing behavior: `AllowAdding`, `AllowEditing`, `AllowDeleting`, `Mode`, `ShowAddNewRow`, `NewRowPosition`. |
| **GridEvents\<TValue\>** | Child component (`GridEvents.cs`) exposing all grid event callbacks: `OnLoad`, `Created`, `DataBound`, `OnActionBegin`, `OnActionComplete`, `OnToolbarClick`, etc. |
| **GridFilterSettings** | Child component / property class for filter configuration: `Type` (FilterBar/Excel/Menu/Checkbox), `Columns`, `ShowFilterBarStatus`, `EnableCaseSensitivity`. |
| **GridGroupSettings** | Child component / property class for grouping: `Columns`, `ShowGroupedColumn`, `EnableLazyLoading`, `ShowToggleButton`, `ShowUngroupButton`. |
| **GridInfiniteScrollSettings** | Property class for infinite scroll configuration: `InitialBlocks`, `MaximumBlocks`, `EnableCache`. |
| **GridJSInteropAdaptor\<T\>** | Internal class (`Internal/Base/GridJSInteropAdaptor.cs`) bridging Blazor C# code with the `sfBlazor.Grid.*` JavaScript module. |
| **GridKeySettings** | Property class (`GridKeySettings.cs`) for customizing keyboard shortcuts for grid cell movement. |
| **GridLine** | Enum for border visibility: `Both`, `None`, `Horizontal`, `Vertical`, `Default`. Default: `Default`. |
| **GridPageSettings** | Child component for pager configuration: `PageSize`, `PageCount`, `CurrentPage`, `PageSizes`, `Template`. |
| **GridRow** | Internal model class representing a single data row, containing `Cells`, `Data`, `IsSelected`, `RowIndex`. |
| **GridRowDropSettings** | Property class for row drag-and-drop configuration: `TargetID`. |
| **GridSearchSettings** | Property class for global search configuration: `Key`, `Fields`, `Operator`, `IgnoreCase`. |
| **GridSelectionSettings** | Child component for selection configuration: `Mode` (Row/Cell/Both), `Type` (Single/Multiple), `PersistSelection`, `CellSelectionMode`, `CheckboxMode`. |
| **GridSortSettings** | Child component for sort configuration: `Columns` (list of `GridSortColumn`). |
| **GridSortColumn** | Class defining a sorted column: `Field` and `Direction` (Ascending/Descending). |
| **GridTemplates** | Property class (`GridTemplates.cs`) for template definitions: `ToolbarTemplate`, `TooltipTemplate`, `PagerTemplate`, `DetailTemplate`. |
| **GridTextWrapSettings** | Child component for text wrap configuration: `WrapMode` (Both/Header/Content). |
| **Grouping Module** | `Grouping<T>` class in `Internal/Actions/Group.cs` managing drag-group, programmatic group, collapse/expand, and lazy-load grouping. |
| **GroupDropArea** | The visual drop zone at the top of the grid where columns are dragged to group. Rendered by `GroupDropArea.razor`. |

---

## H

| Term | Definition |
|------|------------|
| **HeaderCell** | A single column header cell rendered by `GridHeaderCell.razor`, containing sort icons, column menu, filter icon. |
| **HierarchyGrid** | A parent grid containing detail rows that are themselves full grids (master-detail pattern). |
| **HierarchyPrintMode** | Enum controlling how hierarchy grids are printed: `Expanded`, `All`, `None`. Default: `Expanded`. |

---

## I

| Term | Definition |
|------|------------|
| **IGrid** | Public interface (`Interfaces/IGrid.cs`) that `SfGrid<TValue>` implements. Defines all public properties accessible via the interface contract. |
| **InfiniteScroll\<T\>** | Internal module (`Internal/Actions/InfiniteScroll.cs`) managing load-on-demand scrolling behavior. |
| **ISfCircularComponent** | Syncfusion base interface for components supporting circular/parent-child component relationships. |

---

## J

| Term | Definition |
|------|------------|
| **JS Interop** | JavaScript interoperability — the mechanism by which Blazor C# code calls `sfBlazor.Grid.*` JavaScript functions for DOM measurement, scroll positioning, focus management, drag/resize tracking, and browser-level event capture. All data logic, filtering, sorting, and state management remain in C#. |
| **JS Interop Dispatcher** | The generic `execute(action, payload)` JavaScript function exposed by the `sfBlazor.Grid` module. All grid features invoke the same entry point with a string action name and JSON payload, rather than separate feature-specific JS functions. This keeps the JS surface minimal and version-stable. |
| **JS Interop Lifecycle** | The standard four-phase lifecycle of the grid's JS module: **Initialize** (import module, create grid JS instance, attach DOM listeners, set up `ResizeObserver`) → **Observe** (listen for scroll, pointer, keyboard, resize events) → **Interact** (handle user actions, call back to .NET, apply DOM patches) → **Dispose** (detach listeners, clear observers, release JS object references on component teardown). |
| **JS → .NET Callback** | A JavaScript call back into the Blazor component using `DotNetObjectReference`. The grid routes all JS callbacks through a single unified .NET endpoint on `GridJSInteropAdaptor<T>`, which dispatches to `ScrollService`, `FocusService`, `DragDropService`, or `ResizeService` as appropriate. |
| **JS Module** | The `sfBlazor.Grid` client-side JavaScript module (`sf-grid.js`) imported once during `OnAfterRenderAsync(firstRender)`. It handles all DOM-dependent operations for the grid and is disposed when the component is torn down. |

---

## L

| Term | Definition |
|------|------------|
| **Lazy Loading (Group)** | Feature where grouped data is loaded from the server on expand, rather than all upfront. Enabled via `GridGroupSettings.EnableLazyLoading`. |
| **Locale** | Translated string set for grid UI labels (e.g., "EmptyRecord", "True", "False"). Configured via Syncfusion locale service. |

---

## M

| Term | Definition |
|------|------------|
| **MergeHandler\<T\>** | Internal module (`Internal/Actions/MergeHandler.cs`) managing automatic and manual cell spanning logic (`AutoSpan` feature). |
| **MergeModule** | The `SfGrid<TValue>` instance of `MergeHandler<T>`, initialized in `OnInitializedAsync`. |
| **Model** | The `TValue` generic type parameter — the C# class representing a single row of data in the grid. |
| **Module** | An internal action class (e.g., `Sort<T>`, `Filter<T>`, `Selection<T>`) encapsulating feature logic. Each module holds a reference to the parent `SfGrid<TValue>`. |

---

## N

| Term | Definition |
|------|------------|
| **NewRowPosition** | Enum on `GridEditSettings` controlling where a newly added row is inserted: `Top` (default), `Bottom`. Applies to both toolbar Add and `ShowAddNewRow` scenarios. |
| **NormalEdit** | Edit mode where the entire row becomes editable inline. Rendered by `NormalEdit.razor`. |
| **NullDisplayText** | Internal `GridColumn` property specifying text to display when a cell value is `null`. |

---

## O

| Term | Definition |
|------|------------|
| **ObservableCollection** | `System.Collections.ObjectModel.ObservableCollection<TValue>` — a collection that fires change notifications. The grid subscribes to these to auto-refresh when items are added/removed/modified. |
| **OnAfterRenderAsync** | Blazor lifecycle hook overridden in `SfGrid.Lifecycle.cs` to handle post-render tasks (JS initialize, selection, EditComplete). |
| **OnInitializedAsync** | Blazor lifecycle hook overridden in `SfGrid.Lifecycle.cs` to construct all module instances. |
| **OnParametersSetAsync** | Blazor lifecycle hook overridden in `SfGrid.Lifecycle.cs` to detect property changes and trigger re-renders. |
| **OverscanCount** | `[Parameter]` (`int`) specifying extra rows to pre-render above/below the virtual viewport. Default: `0`. |

---

## P

| Term | Definition |
|------|------------|
| **PageQuery** | Method on `DataGenerator<T>` that applies `Skip` and `Take` to the query for paging. |
| **Pager** | The navigation control at the grid footer showing page numbers and navigation buttons. |
| **PageSettings** | See **GridPageSettings**. |
| **Persistence** | Saving and restoring grid state (sort, filter, group, column order) across page reloads via `localStorage`. |
| **Playwright** | End-to-end browser automation testing framework used for grid integration/E2E tests. |
| **PointerEvents** | Browser pointer API events (`pointerdown`, `pointermove`, `pointerup`) captured by the JS module for drag-and-drop (row reorder, column reorder) and column resize operations. Preferred over mouse events for unified touch and mouse support. |
| **PrimaryKey** | `GridColumn.IsPrimaryKey = true` marks the column as the unique row identifier. Required for editing and row drag-and-drop. |
| **PropertyChanges** | Internal `Dictionary<string, object>` on `SfDataBoundComponent` tracking which `[Parameter]` values changed in the current `OnParametersSetAsync` cycle. |
| **PropHelper** | Internal `PropertyInfoHelper<TValue>` instance for reflection-based property access on `TValue`. |

---

## Q

| Term | Definition |
|------|------------|
| **Query** | `Syncfusion.Blazor.Data.Query` — a composable query builder supporting `Where`, `SortBy`, `Page`, `Group`, `Select`, `AddParams`. Used by `DataGenerator<T>` to build all data requests. |

---

## R

| Term | Definition |
|------|------------|
| **ReactiveAggregate\<T\>** | Internal module (`Internal/Actions/ReactiveAggregate.cs`) that recomputes aggregate values reactively when data changes without a full grid refresh. |
| **Reorder\<T\>** | Internal module (`Internal/Actions/Reorder.cs`) managing column drag-and-drop reordering. |
| **Renderer** | A Razor component in `Internal/Renderer/` responsible for rendering a specific part of the grid DOM (rows, cells, headers, editors, filters). |
| **ResizeObserver** | Browser API used by the JS module to monitor changes in the grid container's dimensions. When the grid is resized (e.g., window resize, panel resize), the `ResizeObserver` callback notifies .NET to trigger column width recalculation and layout update. |
| **ResizeService** | Internal service class that handles column resize operations. Receives `pointermove` deltas from the JS module, computes new column widths, and triggers Blazor re-render with updated `GridColumn.Width` values. |
| **Row** | A single data row in the grid. See **GridRow**. |
| **RowHeight** | `[Parameter]` (`double`) for fixed row height in pixels. Required for row virtualization accuracy. |
| **RowReorder\<T\>** | Internal module (`Internal/Actions/RowReorder.cs`) managing row drag-and-drop operations. |
| **RowRenderingMode** | `[Parameter]` (`RowDirection`) controlling layout: `Horizontal` (default table) or `Vertical` (mobile-friendly stacked layout). |
| **RTL** | Right-to-Left — layout mode for Arabic, Hebrew, and other right-to-left languages. Activated by `EnableRtl`. |

---

## S

| Term | Definition |
|------|------------|
| **ScriptModules** | Internal enum value `SfScriptModules.SfGrid` identifying which JS module file to load for the grid. |
| **ScrollService** | Internal service class that processes scroll offset updates received from the JS module (`JS → .NET`). It translates raw `scrollTop`/`scrollLeft` values into virtual row/column index ranges and triggers Blazor rendering of the correct data window. |
| **Selection\<T\>** | Internal module (`Internal/Actions/Selection.cs`) managing row, cell, and checkbox selection, including persist selection across pages. |
| **SelectionSettings** | See **GridSelectionSettings**. |
| **SfDataBoundComponent** | Syncfusion base class for data-aware Blazor components, providing `DataManager`, `PropertyChanges`, `UpdateProperty`, and lifecycle integration. |
| **SfDataManager** | Syncfusion data service component (`<SfDataManager>`) that abstracts data fetching from REST APIs, OData, GraphQL, and other sources. |
| **SfGrid\<TValue\>** | The main grid component class. Partial class split across `SfGrid.Properties.cs`, `SfGrid.Lifecycle.cs`, `SfGrid.Methods.cs`, `SfGrid.razor.cs`. |
| **sfBlazor.Grid** | The JavaScript module namespace (`sfBlazor.Grid.*`) within `sf-grid.js` that provides all client-side DOM helpers. Exposes a generic `execute(action, payload)` dispatcher and a `.NET`-facing callback endpoint. Imported once per grid instance on `OnAfterRenderAsync(firstRender)`. |
| **ShowAddNewRow** | `GridEditSettings` property (`bool`) that permanently displays an empty row at the top (or bottom, per `NewRowPosition`) for adding new records — independent of the toolbar Add button. **Regression note**: clearing the add-new-row container via `innerHTML = ''` destroys its DOM wrapper; under virtualization this causes flicker. The correct approach is to remove only the form element, preserving the wrapper node. |
| **ShowColumnChooser** | `[Parameter]` (`bool`) enabling the column chooser toolbar button and dialog. Default: `false`. |
| **ShowColumnMenu** | `[Parameter]` (`bool`) enabling the per-column dropdown menu icon. Default: `false`. |
| **ShowTooltip** | `[Parameter]` (`bool`) enabling hover tooltips on grid cells and headers. Default: `false`. |
| **Sort\<T\>** | Internal module (`Internal/Actions/Sort.cs`) managing single and multi-column sort operations. |
| **SortSettings** | See **GridSortSettings**. |
| **Stacked Header** | Multi-level column headers created by nesting `GridColumn` inside another `GridColumn`. |
| **SuppressAutoSpanning** | Internal `bool` on `SfGrid<TValue>` used to temporarily disable auto-spanning during internal re-renders. |

---

## T

| Term | Definition |
|------|------------|
| **Template Column** | A `GridColumn` with a `Template` render fragment for custom cell rendering. |
| **TValue** | The generic type parameter of `SfGrid<TValue>` representing the C# class for each data row. |
| **Toolbar** | `[Parameter]` (`object?`) accepting built-in item names (string list) or `ToolbarItem` objects for the grid toolbar. |
| **ToolbarTemplate** | Custom Razor template for the grid toolbar via `GridTemplates.ToolbarTemplate`. |

---

## U

| Term | Definition |
|------|------------|
| **UnifiedCallback** | The single `.NET` method on `GridJSInteropAdaptor<T>` that receives all `JS → .NET` event callbacks. It inspects the action name and routes to the appropriate internal service (`ScrollService`, `FocusService`, `DragDropService`, `ResizeService`). This pattern prevents the `.NET` object reference surface from growing with each new JS-driven feature. |
| **UpdateProperty** | Internal async method on `SfDataBoundComponent` that compares old/new parameter values and records changes in `PropertyChanges`. |

---

## V

| Term | Definition |
|------|------------|
| **Virtual Content** | The scrollable container used in virtualization mode, rendered by `GridVirtualContent.razor`. |
| **Virtual Header** | The column header container in column virtualization, rendered by `GridVirtualHeader.razor`. |
| **VirtualBuffer** | The pre-rendered rows above and below the visible viewport, controlled by `OverscanCount`. A buffer of extra rows prevents blank flashes during fast scroll before the next data fetch completes. |
| **VirtualScroll\<T\>** | Internal module (`Internal/Actions/VirtualScroll.cs`) managing row and column virtual scrolling, row index computation, scroll offset, and DOM translation. |
| **VirtualMaskRow** | Placeholder skeleton rows shown while virtual scroll is fetching data. Controlled by `EnableVirtualMaskRow`. |
| **Viewport** | The visible area of the grid content at any given scroll position. Virtualization renders only rows/columns within (and slightly beyond) the viewport. |

---

## W

| Term | Definition |
|------|------------|
| **WCAG** | Web Content Accessibility Guidelines — international accessibility standard. The DataGrid targets WCAG 2.0 Level AA. |
| **Width** | `[Parameter]` (`string`) for the overall grid width. Default: `"auto"`. |
| **WrapMode** | Enum in `GridTextWrapSettings`: `Both` (header + content), `Header`, `Content`. |

---

## Z

| Term | Definition |
|------|------------|
| **Zero-config** | The ability to use `SfGrid` with just `DataSource` and no other configuration — columns and features are auto-configured from the model type. |

---

*Total terms defined: 105+*  
*For architecture context, see [`architecture/system-architecture.md`](../architecture/system-architecture.md).*  
*For feature details, see [`overview/product-overview.md`](./product-overview.md).*  
*For JS-Interop depth, see [`architecture/component-architecture.md`](../architecture/component-architecture.md) and [`architecture/dependency-map.md`](../architecture/dependency-map.md).*
