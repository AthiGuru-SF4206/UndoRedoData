# Component Architecture — Syncfusion Blazor DataGrid

> **Audience**: Developers working on renderers, editors, and child components  
> **Prerequisite**: [`architecture/system-architecture.md`](./system-architecture.md)  
> **Last Updated**: March 11, 2026

---

## Overview

The DataGrid is composed of **one root component** (`SfGrid<TValue>`) and **~40 child Razor components**, all organized into logical rendering zones. The component hierarchy follows Blazor's tree model where child components receive data via `[CascadingParameter]` or direct binding from the parent.

The component is implemented as a **partial class** split across five files for maintainability:

| File | Responsibility |
|------|---------------|
| `SfGrid.Properties.cs` | All `[Parameter]` declarations with XML documentation and backing fields |
| `SfGrid.Lifecycle.cs` | `OnInitializedAsync`, `OnParametersSetAsync`, `OnAfterRenderAsync`, `OnAfterScriptRendered`, `OnHybridInitialized`, `OnHybridParametersSet` |
| `SfGrid.Methods.cs` | All public async API methods: `SortColumnAsync`, `FilterByColumnAsync`, `SelectRowAsync`, etc. |
| `SfGrid.razor.cs` | Internal properties, module field declarations, helper methods, internal flags |
| `Internal/SfGrid.razor` | Root Razor markup assembling all renderer components via `CascadingValue` |

---

## Component Hierarchy Tree

```
SfGrid<TValue>                              [Root — SfGrid.razor]
│
├── WidthController.razor                   [Column width enforcement CSS]
├── Preloader.razor                         [Loading spinner overlay]
│
├── GridToolbar.razor                       [Toolbar with built-in/custom items]
│   └── ForeignKeySubComponents.razor       [Sub-grids for FK filter dropdowns]
│
├── GroupDropArea.razor                     [Drag-to-group drop zone header]
│
├── ── HEADER ZONE ──
├── GridHeader.razor                        [Fixed header container]
│   ├── GroupedHeader.razor                 [Stacked / multi-level header rows]
│   └── GridHeaderCell.razor               [Per-column header cell]
│       ├── ColumnMenu.razor               [Dropdown menu icon + items]
│       └── [sort indicator, filter icon, resize handle]
│
├── ── CONTENT ZONE (Standard) ──
├── GridContent.razor                       [Main scrollable data area]
│   ├── GridRow.razor                      [Per data row]
│   │   ├── GridRow.CellRenderer.cs        [Cell render logic base]
│   │   └── [per-cell → CellRender.razor]
│   │       ├── CheckBoxRenderer.razor     [Boolean columns]
│   │       ├── CommandColumnRenderer.cs   [Command button cells]
│   │       ├── ExpandCellRenderer.cs      [Detail row expand/collapse button]
│   │       ├── IndentCellRenderer.cs      [Group/detail indent cells]
│   │       ├── RowDragCellRenderer.cs     [Row drag handle cell]
│   │       ├── GroupCaptionRenderer.cs    [Group caption row cell]
│   │       ├── SummaryCellRenderer.cs     [Aggregate footer cell]
│   │       └── CaptionSummaryRenderer.cs  [Group caption aggregate cell]
│   └── FooterContent.razor                [Aggregate footer row container]
│
├── ── CONTENT ZONE (Virtual) ──
├── GridVirtualContent.razor               [Virtual scroll container]
│   └── GridVirtualHeader.razor            [Column virtual scroll header]
│
├── ── DETAIL ROW ZONE ──
├── GridDetailRow.razor                    [Detail row expand container]
│
├── ── FILTER ZONE ──
├── Filter/                                [Filter UI components]
│   ├── [FilterBar cells per column]
│   ├── [Excel filter dialog]
│   ├── [Menu filter panel]
│   └── [Checkbox filter panel]
│
├── ── EDIT ZONE ──
├── NormalEdit.razor                       [Inline row edit form]
├── DialogEdit.razor                       [Modal dialog edit form]
├── BatchEdit.razor                        [Batch cell edit handler]
├── GridAddNewRow.razor                    [ShowAddNewRow form]
├── GridCellRegister.razor                 [Cell editor mount point]
│
├── ── EDITOR CELLS (mounted inside edit rows) ──
├── Internal/Editors/
│   ├── BooleanEditCell.razor              [Checkbox editor]
│   ├── DatePickerEditCell.razor           [Date/DateTime picker editor]
│   ├── TimePickerEditCell.razor           [Time picker editor]
│   ├── NumericEditCell.razor              [Numeric text box editor]
│   ├── TextBoxEditCell.razor              [Plain text box editor]
│   ├── DropDownEditCell.razor             [Dropdown editor]
│   ├── ForeignKeyEditCell.razor           [Foreign key dropdown editor]
│   ├── ColumnsValidator.razor             [Validation display per column]
│   └── EditorCellBase.cs                 [Base class for all editor cells]
│
├── ── OVERLAY ZONE ──
├── ColumnChooser.razor                    [Show/hide columns dialog]
├── ContextMenu.razor                      [Right-click context menu]
├── GridTooltip.razor                      [Cell/header tooltip]
├── ValidationDialog.razor                 [Edit validation error dialog]
├── ValidationTooltip.razor                [Inline validation tooltip]
├── AdaptiveDialogRenderer.razor           [Mobile adaptive dialog]
│
├── ── AGGREGATE ZONE ──
├── RefreshAggregate.razor                 [Aggregate recompute trigger]
│
├── ── EVENT / JS BINDING ZONE ──
├── EventRegister.razor                    [Sync JS event bindings]
├── EventRegisterAsync.razor              [Async JS event bindings]
│
└── PrintLayout.razor                      [Print-mode DOM structure]
```

---

## Core Class Hierarchy and Inheritance

```
System.Object
└── Microsoft.AspNetCore.Components.ComponentBase
    └── Syncfusion.Blazor.Internal.SfBaseComponent
        └── Syncfusion.Blazor.Internal.SfDataBoundComponent
            └── SfGrid<TValue>
                implements IGrid                  (Interfaces/IGrid.cs)
                implements ISfCircularComponent

GridColumn : SfDataBoundComponent
    partial base: GridColumnBase      (Internal/Base/GridColumnBase.cs)
    partial impl: GridColumn.cs       (public parameters — top-level file)
    internal:     Internal/GridColumn.razor   (internal Razor wrapper)
    foreign key:  Internal/GridForeignColumn.razor

GridAggregate          : SfDataBoundComponent   (GridAggregate.razor.cs)
GridAggregateColumn    : SfDataBoundComponent   (GridAggregateColumn.razor.cs)
GridEditSettings       : SfDataBoundComponent   (GridEditSettings.cs)
GridFilterSettings     : SfDataBoundComponent   (GridFilterSettings.razor.cs)
GridGroupSettings      : SfDataBoundComponent   (GridGroupSettings.razor.cs)
GridPageSettings       : SfDataBoundComponent   (GridPageSettings.razor.cs)
GridSelectionSettings  : SfDataBoundComponent   (GridSelectionSettings.cs)
GridSortSettings       : SfDataBoundComponent   (GridSortSettings.razor.cs)
GridInfiniteScrollSettings : SfDataBoundComponent (GridInfiniteScrollSettings.razor.cs)
GridTextWrapSettings   : SfDataBoundComponent   (GridTextWrapSettings.razor.cs)

// Internal base classes (not public API)
Internal/Base/GridColumnBase.cs      — internal column state and resolved types
Internal/Base/GridJSInteropAdaptor.cs — JS interop bridge
Internal/Base/Utils.cs               — static utilities (GridUtils)
Internal/Base/InternalClass.cs       — shared internal model types
Internal/Base/DynamicInfo.cs         — dynamic data support helpers
Internal/Base/Enums.cs               — internal-only enumerations
```

---

## Lifecycle Hooks and Phases

### Phase 1: Initialization (`OnInitializedAsync`)

**Triggered**: Once per component lifetime, on first render.

```
OnInitializedAsync()
  │
  ├── OnHybridInitialized()
  │     ├── base.OnInitializedAsync()          ← SfDataBoundComponent init
  │     ├── Copy all [Parameter] → backing fields (_sort, _filter, ...)
  │     ├── Generate ID if not provided
  │     └── Set IsAutoGeneratedColumns flag
  │
  ├── Construct all 15 action modules
  ├── Set ScriptModules = SfScriptModules.SfGrid
  └── Set _isLoaded = true
```

**Regression risks**: Any module initialization failure here leaves the grid in a broken state. All modules must be null-safe when used before `OnAfterScriptRendered`. The `_jsAdaptor` and `PropHelper` are also created here — their absence will null-ref in `OnAfterScriptRendered`.

---

### Phase 2: Parameter Changes (`OnParametersSetAsync`)

**Triggered**: On every parent component re-render that may have changed parameters.

```
OnParametersSetAsync()
  │
  ├── Handle UnMatchedAttributes → _cachedAttributes (TableClass = false guard)
  ├── Check DataSource reference change → wire/unwire ObservableCollection events
  │
  ├── OnHybridParametersSet()
  │     ├── base.OnParametersSetAsync()
  │     ├── UpdateProperty() for every [Parameter] → records in PropertyChanges
  │     └── SetDataManager<TValue>() → configures adaptor; if BlazorAdaptor + null → set Json=[]
  │
  ├── Query equality check → PropertyChanges.Remove("Query") if Query.IsEqual()
  │
  ├── DataSource change:
  │     ├── Reset all Rows.IsSelected = false and Cells.IsSelected = false
  │     ├── CheckBoxState = UnCheck
  │     ├── SelectionModule.IsHeaderCheckboxChecked = false
  │     └── _rowIndexPropertyChanged = false
  │
  ├── Wire ObservableCollection on first non-null DataSource (_isObservableWired guard)
  ├── AllowRowDragAndDrop change → RowReorderModule.RowReorderIndentWidth = ""
  │
  ├── Detect refreshable property changes (when PropertyChanges.Count > 0):
  │     ├── VirtualScrollModule.IsDataSourceChanged = DataSource changed while IsRendered
  │     ├── headerRef = AllowGrouping/GroupSettings/AllowSorting/SortSettings/
  │     │               AllowRowDragAndDrop/Columns/AllowFiltering/ShowColumnMenu
  │     ├── Width/Height + FrozenColumns → RefreshFrozenHeader + isNeedClientFrozenHeight
  │     ├── EnableVirtualization=False toggle → InvokeMethod("sfBlazor.Grid.virtualDisconnect")
  │     ├── FrozenColumns/RowHeight + virtual → _isRerendered = true
  │     ├── ColumnWidth → _isColumnWidthChanged = true
  │     ├── ColumnClipMode → _isColumnClipModeChanged = true
  │     ├── SelectedRowIndex (not Delete action) → _rowIndexPropertyChanged = true
  │     ├── RowSelectionModeChanged → ClearRowSelectionAsync
  │     ├── CellSelectionModeChanged → ClearCellSelectionAsync
  │     ├── BothSelectionModeChanged → both clear methods
  │     └── PersistGroupState → GroupStates.Clear()
  │
  ├── PropertyChanges.Clear()
  └── If refreshable keys or isNeedClientFrozenHeight → ModelChanged(RequestType = Refresh)
```

**Critical**: `PropertyChanges` is a **per-cycle** dictionary. It is **cleared at the end** of each `OnParametersSetAsync` cycle. Any logic that needs to act on property changes must do so before the `PropertyChanges.Clear()` call.

---

### Phase 3: After Render (`OnAfterRenderAsync`)

**Triggered**: After every Blazor render cycle completes.

```
OnAfterRenderAsync(firstRender)
  │
  ├── If isGridModelRefresh → InvokeAsync(ModelChanged(Refresh)) — deferred re-entry safe
  │
  ├── If _requireDataBoundInvoke && IsClientInitialized
  │     ├── await Task.Yield()         ← give client time before firing DataBound
  │     └── GridEvents.DataBound.InvokeAsync()
  │
  ├── If AddOrDeleteArgs != null:
  │     ├── IsDeleteAction = (Action == "Delete")
  │     ├── AddOrDeleteArgs = null
  │     └── EditModule.EditComplete(addDeleteArgs)
  │
  ├── Reset flags: HasColumnChanges=false, IsColumnHideOrShow=false, _shouldRender=true
  ├── ReorderModule.IsColumnReordered = false
  ├── EditModule.ClearSelection = !SelectionSettings.PersistSelection
  ├── EnsureFeaturesCompatibility()
  ├── If SoftRefresh → SoftRefresh = false
  ├── SetColumnValueType()
  │
  ├── Grouped template column visibility fix (ShowGroupedColumn=false path)
  │     └── Hide template columns that are in GroupSettings.Columns
  │
  ├── FirstRender only:
  │     └── _originalProp = SerializeModel(this)  ← baseline for reset
  │
  ├── Handle initial zero-data state: SelectionModule.IsInitialSelectionCompleted = false
  │
  ├── If SelectedRowIndex != -1 && data exists && (firstRender || rowIndexChanged || !initialSelectionDone):
  │     └── SelectionModule.SelectRow(SelectedRowIndex)
  │
  └── base.OnAfterRenderAsync(firstRender) → triggers OnAfterScriptRendered() on firstRender
```

---

### Phase 4: Script Initialized (`OnAfterScriptRendered`)

**Triggered**: Once after the JavaScript module is loaded and the component DOM is ready. This is the **point of JS initialization**.

```
OnAfterScriptRendered()
  ├── _jsAdaptor.Init()                              ← establish DotNet reference
  ├── _hasSpinner = true
  ├── Trigger GridEvents.OnLoad
  │
  ├── InvokeMethod("sfBlazor.Grid.initialize", ...)
  │     Returns: row height, indent width, mac device flag
  │
  ├── Handle EnablePersistence → read localStorage
  │
  ├── DataProcess()                                   ← FIRST DATA LOAD
  │
  ├── Trigger GridEvents.Created
  │
  └── If _requireDataBoundInvoke → Invoke GridEvents.DataBound
      IsClientInitialized = true
```

---

## Dependency Injection Pattern

The grid does **not** use ASP.NET Core's DI container for its internal modules. Instead it uses a **manual constructor injection** pattern where each module receives the parent grid instance:

```csharp
// Constructor pattern for all modules
internal class Sort<T>
{
    public SfGrid<T> Parent;
    public Sort(SfGrid<T> parent) => Parent = parent;
}
```

This makes module instantiation explicit, testable, and avoids service locator anti-patterns at the DI container level.

---

## Child Component Communication

### Cascading Parameters
Child Razor components (renderers) receive the parent grid instance via `[CascadingParameter]`:

```razor
<!-- In GridHeader.razor or GridRow.razor -->
@typeparam TValue
[CascadingParameter]
public SfGrid<TValue> Grid { get; set; }
```

This allows any renderer to access grid state without explicit prop-drilling.

### Parameter Binding
Settings child components (like `GridPageSettings`, `GridSortSettings`) bind to their parent via Blazor's standard `[Parameter]` / `[CascadingParameter]` system. They push their state up to the parent `SfGrid<TValue>` through property assignment in their `OnParametersSetAsync`.

---

## Column Model Architecture

```
GridColumn (public API — GridColumn.cs)
    │
    ├── [Parameters]: Field, HeaderText, Width, Format, Type, Visible,
    │                 AllowSorting, AllowFiltering, AllowGrouping,
    │                 IsPrimaryKey, IsFrozen, FreezeDirection,
    │                 Template, HeaderTemplate, EditTemplate, FilterTemplate,
    │                 AutoSpan, DisplayAsCheckbox, etc.
    │
    └── Internal state (GridColumnBase.cs):
          ├── ValueType          ← resolved .NET type of Field
          ├── ActualType         ← unwrapped nullable type
          ├── IsGridForeignColumn ← true if ForeignKeyValue is set
          ├── ColumnVisible      ← current effective visibility
          ├── FrozenMovableLabel ← "Left Frozen" / "Right Frozen" / "Movable"
          └── ConvertEmptyStringToNull ← UI null display behavior
```

Columns are stored as `List<GridColumn>` on `SfGrid<TValue>.Columns`. Stacked headers nest `GridColumn` children inside parent `GridColumn` instances.

`GridUtils.GetColumns(Parent)` flattens this tree into a flat `List<GridColumn>` for query building and rendering.

---

## Editor Architecture

```
EditCell request (user clicks Edit / enters Batch cell)
        │
        ▼
Edit<T>.GetEditor(column)
        │
        ▼ based on column.EditType
  ┌────────────────────────────────────────────────────────────┐
  │ DefaultEdit    → TextBoxEditCell.razor (string / default)  │
  │ NumericEdit    → NumericEditCell.razor                     │
  │ DropDownEdit   → DropDownEditCell.razor                    │
  │ BooleanEdit    → BooleanEditCell.razor                     │
  │ DatePickerEdit → DatePickerEditCell.razor                  │
  │ DateTimeEdit   → DatePickerEditCell.razor (DateTime mode)  │
  │ TimePickerEdit → TimePickerEditCell.razor                  │
  │ [ForeignKey]   → ForeignKeyEditCell.razor                  │
  │ [Custom]       → GridColumn.EditTemplate (RenderFragment)  │
  └────────────────────────────────────────────────────────────┘
        │
        ▼
EditorCellBase.cs (base class for all editors)
  ├── Handles value binding (two-way via EventCallback)
  ├── Applies validation rules (data annotations + GridColumn rules)
  └── Notifies Edit<T> of value changes via callback
```

---

## Renderer Naming Conventions

All Razor renderer components follow this naming pattern:

| Pattern | Example | Purpose |
|---------|---------|---------|
| `Grid[Area].razor` | `GridHeader.razor`, `GridContent.razor` | Zone-level containers |
| `Grid[Element].razor` | `GridRow.razor`, `GridHeaderCell.razor` | Individual element renderers |
| `[Type]Renderer.cs` | `CommandColumnRenderer.cs`, `SummaryCellRenderer.cs` | C# renderer helpers |
| `[Feature]Edit.razor` | `NormalEdit.razor`, `DialogEdit.razor` | Edit mode containers |
| `[Type]EditCell.razor` | `BooleanEditCell.razor`, `DropDownEditCell.razor` | Editor cell components |

---

## Internal vs. Public Boundary

```
Public Surface (API contract — NEVER break)
├── SfGrid<TValue>                (SfGrid.Properties.cs, SfGrid.Methods.cs)
├── IGrid                         (Interfaces/IGrid.cs)
├── GridColumn                    (GridColumn.cs)
├── GridEvents<TValue>            (GridEvents.cs)
├── GridEditSettings              (GridEditSettings.cs)
├── GridFilterSettings            (GridFilterSettings.razor.cs)
├── GridGroupSettings             (GridGroupSettings.razor.cs)
├── GridPageSettings              (GridPageSettings.razor.cs)
├── GridSortSettings              (GridSortSettings.razor.cs)
├── GridSelectionSettings         (GridSelectionSettings.cs)
├── GridInfiniteScrollSettings    (GridInfiniteScrollSettings.razor.cs)
├── GridTextWrapSettings          (GridTextWrapSettings.razor.cs)
├── GridSearchSettings            (GridSearchSettings.cs)
├── GridRowDropSettings           (GridRowDropSettings.cs)
├── GridColumnChooserSettings     (GridColumnChooserSettings.cs)
├── GridKeySettings               (GridKeySettings.cs)
├── GridCommandColumn             (GridCommandColumn.cs)
├── GridAggregate / GridAggregateColumn (GridAggregate*.cs)
├── GridTemplates                 (GridTemplates.cs)
├── All EventArgs                 (EventModels/Grids.cs)
└── All Enumerations              (Enumeration/GridsEnumerations.cs)

Internal Implementation (no stability guarantee — breaking changes allowed in patch)
├── Internal/Actions/*            (all 15 action modules)
├── Internal/Renderer/*           (all renderer Razor components)
├── Internal/Editors/*            (all editor cell components)
├── Internal/Base/*               (utilities, interop, column base)
├── Internal/Generators/          (export and code generation helpers)
├── Internal/Models/              (internal data model types)
├── Internal/Annotation/          (data annotation reading)
└── Internal/Export/              (Excel/PDF export internals)
```

> **Rule**: Any type, method, or property under `Internal/` may be changed without a deprecation notice. All top-level public files constitute the stable API.

---

## Renderer File Reference

| Renderer File | Zone | Purpose |
|--------------|------|---------|
| `GridHeader.razor` | Header | Fixed header container with column cells |
| `GroupedHeader.razor` | Header | Stacked / multi-level header row rendering |
| `GridHeaderCell.razor` | Header | Per-column header: sort icon, filter icon, resize handle, column menu |
| `ColumnMenu.razor` | Header | Dropdown column menu items |
| `GridContent.razor` | Content | Main scrollable data area |
| `GridRow.razor` / `GridRow.razor.cs` | Content | Per data row rendering with row-level state |
| `GridRow.CellRenderer.cs` | Content | Cell render logic base (cell type dispatch) |
| `CellRender.razor` | Content | Per-cell renderer; delegates to specialized renderers |
| `CheckBoxRenderer.razor` | Content | Boolean / checkbox selection column |
| `CommandColumnRenderer.cs` | Content | Command button cells |
| `ExpandCellRenderer.cs` | Content | Detail row expand/collapse button |
| `IndentCellRenderer.cs` | Content | Group/detail indent spacer cells |
| `RowDragCellRenderer.cs` | Content | Row drag handle cell |
| `GroupCaptionRenderer.cs` | Content | Group caption row |
| `SummaryCellRenderer.cs` | Content | Aggregate footer cell |
| `CaptionSummaryRenderer.cs` | Content | Group caption aggregate cell |
| `DetailCellRenderer.cs` | Content | Detail row wrapper cell |
| `FooterContent.razor` | Content | Aggregate footer row container |
| `GridVirtualContent.razor` | Virtual | Virtual scroll viewport container |
| `GridVirtualHeader.razor` | Virtual | Column virtual scroll header sync |
| `GridDetailRow.razor` | Detail | Detail row expand container |
| `GridToolbar.razor` | Toolbar | Toolbar with built-in / custom items |
| `GroupDropArea.razor` | Group | Drag-to-group drop zone |
| `ForeignKeySubComponents.razor` | FK | Sub-grid for FK filter dropdowns |
| `NormalEdit.razor` | Edit | Inline row edit form |
| `DialogEdit.razor` | Edit | Modal dialog edit form |
| `BatchEdit.razor` | Edit | Batch cell edit handler |
| `GridAddNewRow.razor` | Edit | ShowAddNewRow persistent form row |
| `GridCellRegister.razor` | Edit | Editor cell mount point |
| `ColumnChooser.razor` | Overlay | Show/hide columns dialog |
| `ContextMenu.razor` | Overlay | Right-click context menu |
| `GridTooltip.razor` | Overlay | Cell / header tooltip |
| `ValidationDialog.razor` | Overlay | Edit validation error dialog |
| `ValidationTooltip.razor` | Overlay | Inline validation tooltip |
| `AdaptiveDialogRenderer.razor` | Adaptive | Mobile-responsive adaptive dialog |
| `RefreshAggregate.razor` | Aggregate | Aggregate recompute trigger component |
| `EventRegister.razor` | JS | Sync JS event binding registrations |
| `EventRegisterAsync.razor` | JS | Async JS event binding registrations |
| `WidthController.razor` | Layout | Column width enforcement CSS injection |
| `Preloader.razor` | Layout | Loading spinner overlay |
| `PrintLayout.razor` | Print | Print-mode DOM structure |

---

*For data flow details, see [`architecture/data-flow.md`](./data-flow.md).*  
*For module dependency graph, see [`architecture/dependency-map.md`](./dependency-map.md).*
