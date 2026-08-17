# Data Flow — Syncfusion Blazor DataGrid

> **Audience**: Developers working on data binding, actions, and rendering  
> **Prerequisite**: [`architecture/system-architecture.md`](./system-architecture.md)  
> **Last Updated**: March 11, 2026

---

## Overview

The DataGrid processes data through a **unidirectional pipeline**:

```
External Data Source
       │
       ▼
   DataGenerator<T>.GenerateQuery()    ← compose Query
       │
       ▼
   SfDataManager.ExecuteQuery()        ← fetch data
       │
       ▼
   SfGrid.CurrentViewData              ← store results
       │
       ▼
   Blazor Render Tree                  ← render rows/cells
       │
       ▼
   Browser DOM                         ← display
```

Every user action (sort, filter, page, group) triggers a re-run of some or all of this pipeline.

---

## Flow 1: Initial Data Load

```
App sets DataSource="@myList" on SfGrid
        │
        ▼
OnInitializedAsync() → OnHybridInitialized()
  └── _dataSource = DataSource  (backing field set)
      All 15 modules instantiated
        │
        ▼
OnParametersSetAsync() → OnHybridParametersSet()
  └── UpdateProperty() for all parameters
      SetDataManager<TValue>(DataSource) → adaptor configured
        │
        ▼
OnAfterRenderAsync(firstRender=true)
  └── base.OnAfterRenderAsync() → OnAfterScriptRendered()
        │
        ▼
OnAfterScriptRendered()
  ├── _jsAdaptor.Init()
  ├── EventAggregator.Trigger("InitialLoad", this)
  ├── GridEvents.OnLoad.InvokeAsync()
  │
  ├── InvokeMethod("sfBlazor.Grid.initialize", DataId, element, options, ref, focusArgs)
  │     Returns: ActionArgs { RowHeight, IndentWidth, IsRowDragCell, IsMacDevice }
  │
  ├── VirtualScrollModule.RHeight = result.RowHeight  (if virtualizing)
  ├── EventAggregator.Trigger("VirtualComponentUpdate")
  │
  ├── EnablePersistence → localStorage["grid{ID}"] → PersistProperties()
  │
  └── DataProcess()                                    ← FIRST DATA LOAD

        │
        ▼
DataProcess()
  ├── DataModule.GenerateQuery()       ← build full Query
  │     ├── ColumnQuery   → Select() based on ColumnQueryMode
  │     ├── FilterQuery   → Where() from FilterSettings.Columns
  │     ├── SearchQuery   → Search() from SearchSettings.Key
  │     ├── AggregateQuery → Aggregate() from GridAggregates
  │     ├── SortQuery     → SortBy() from SortSettings.Columns
  │     ├── PageQuery     → Skip() + Take() for current page
  │     └── GroupQuery    → Group() from GroupSettings.Columns
  │
  └── DataManager.ExecuteQuery(query)
        │
        ▼
Adaptor processes query:
  [BlazorAdaptor]     → in-memory LINQ on IEnumerable<TValue>
  [WebApiAdaptor]     → HTTP GET with serialized query params
  [ODataV4Adaptor]    → OData $filter/$orderby/$top/$skip/$count
  [CustomAdaptor]     → developer-defined Read/ReadAsync logic
  [GraphQLAdaptor]    → custom GraphQL query template
  [UrlAdaptor]        → generic REST POST/GET endpoint
        │
        ▼
Result: { result: IEnumerable<object>, count: int }
        │
        ▼
SfGrid processes result:
  ├── CurrentViewData = result
  ├── TotalRecordsCount = count
  ├── _requireDataBoundInvoke = true
  └── StateHasChanged()  ← Blazor diff + re-render
        │
        ▼
GridContent.razor renders GridRow.razor × N
  └── Each GridRow renders CellRender.razor × M (columns)
        │
        ▼
SetColumnValueType()
EventAggregator.Trigger("InternalDataBound")
GridEvents.Created.InvokeAsync()
GridEvents.DataBound.InvokeAsync()   ← notify application
IsClientInitialized = true
```

---

## Flow 2: Sort Action

```
User clicks column header
        │
        ▼
[JS: sfBlazor.Grid.onHeaderClick → DotNet callback]
        │
        ▼
SfGrid.SortColumnAsync(field, direction, isMultiSort)
        │
        ▼
Sort<T>.SortColumn(field, direction)
  ├── Updates SortSettings.Columns (adds/updates/removes GridSortColumn)
  └── Sets PropertyChanges["SortSettings"] indirectly

        │
        ▼
GridEvents.OnActionBegin.InvokeAsync(ActionEventArgs {
    RequestType = Action.Sorting,
    ColumnName  = field,
    Direction   = direction
})

  If args.Cancel == true → STOP
        │
        ▼ (not cancelled)
ModelChanged(ActionEventArgs { RequestType = Action.Sorting })
  └── DataProcess()  ← re-run full pipeline with new SortQuery

        │
        ▼
DataGenerator<T>.SortQuery(query)
  └── foreach SortSettings.Columns:
        query.SortBy(column.Field, column.Direction)

        │
        ▼
[Data fetched, re-rendered]

        │
        ▼
GridEvents.OnActionComplete.InvokeAsync(...)
```

---

## Flow 3: Filter Action

```
User types in FilterBar / applies Excel filter
        │
        ▼
Filter<T>.FilterByColumn(field, operator, value, ...)
  ├── Updates FilterSettings.Columns (adds/updates/removes GridFilterColumn)
  └── Calls ModelChanged()

        │
        ▼
GridEvents.OnActionBegin.InvokeAsync(ActionEventArgs {
    RequestType = Action.Filtering,
    CurrentFilterObject = filterColumn
})

  If args.Cancel == true → STOP
        │
        ▼
DataGenerator<T>.FilterQuery(query)
  └── foreach FilterSettings.Columns:
        query.Where(field, operator, value, ignoreCase)

  [Foreign key filter → resolves display value → applies on FK field]

        │
        ▼
[Data fetched, pager reset to page 1, re-rendered]

        │
        ▼
GridEvents.OnActionComplete.InvokeAsync(...)
```

---

## Flow 4: Paging Action

```
User clicks page number in pager
        │
        ▼
SfGrid.GoToPageAsync(pageNumber)
  └── PageSettings.CurrentPage = pageNumber

        │
        ▼
ModelChanged(ActionEventArgs { RequestType = Action.Paging })

        │
        ▼
DataGenerator<T>.PageQuery(query, skipPage=false)
  └── query.Page(pageNumber, pageSize)
      → translates to: Skip = (page-1) * pageSize, Take = pageSize

        │
        ▼
[Data fetched for the new page, re-rendered]
```

---

## Flow 5: Grouping Action

```
User drags column to GroupDropArea
        │
        ▼
[JS: drag event → DotNet callback]
        │
        ▼
Grouping<T>.GroupColumn(field)
  └── GroupSettings.Columns = [...existing, field]

        │
        ▼
GridEvents.OnActionBegin.InvokeAsync(ActionEventArgs {
    RequestType = Action.Grouping,
    ColumnName  = field
})

  If args.Cancel == true → STOP
        │
        ▼
DataGenerator<T>.GroupQuery(query)
  └── foreach GroupSettings.Columns:
        query.Group(field)

  [With EnableLazyLoading = true]
  └── Only grouped structure returned, child rows loaded on expand

        │
        ▼
[Data fetched as grouped tree, GroupCaptionRenderer renders captions]
[Aggregates applied per group via CaptionSummaryRenderer]

        │
        ▼
GridEvents.OnActionComplete.InvokeAsync(...)
```

---

## Flow 6: Virtual Scroll (Row Virtualization)

```
User scrolls grid vertically
        │
        ▼
[JS: scroll event → sfBlazor.Grid.onScroll → DotNet]
        │
        ▼
VirtualScroll<T>.OnScroll(scrollTop)
  ├── Compute visible row range:
  │     RowStartIndex = Floor(scrollTop / RHeight)
  │     RowEndIndex   = RowStartIndex + PageSize + OverscanCount * 2
  │
  ├── Check if new range is within GeneratedData cache
  │     If cached → use GeneratedData[page] → NO network call
  │     If not cached → trigger DataProcess() for new range
  │
  └── Generate CSS transform: translateY(RowStartIndex * RHeight)
      [Virtual rows appear at correct scroll position]

        │
        ▼
DataGenerator<T>.GenerateQuery(
    VirtualStartIndex = RowStartIndex,
    VirtualEndIndex   = RowEndIndex)
  └── PageQuery → Skip(RowStartIndex), Take(RowEndIndex - RowStartIndex)

        │
        ▼
[New rows fetched, stored in GeneratedData[pageIndex]]
[Only viewport rows rendered in DOM]
[CSS translateY applied to position virtual content]

  With EnableVirtualMaskRow:
  └── Mask rows shown immediately during fetch
      → replaced with actual rows on data return
```

---

## Flow 7: Infinite Scroll

```
User scrolls to bottom of grid
        │
        ▼
[JS: scroll event → sfBlazor.Grid.onInfiniteScroll → DotNet]
        │
        ▼
InfiniteScroll<T>.OnScrollEnd()
  ├── Increment current block
  ├── Check MaximumBlocks (cache mode):
  │     If blocks > MaximumBlocks → remove oldest block from DOM
  │
  └── IntialInfinitePageQuery(query)
      → query.Page(nextBlock, pageSize)

        │
        ▼
[New data appended to existing rows]
[No full re-render — only new rows added]
```

---

## Flow 8: CRUD — Add Record

```
User clicks toolbar Add button
        │
        ▼
SfGrid.AddRecordAsync()
  └── EditModule.AddRecord()

        │
        ▼
GridEvents.OnActionBegin.InvokeAsync(ActionEventArgs {
    RequestType = Action.Add,
    Data        = new TValue() (default instance)
})

  If args.Cancel == true → STOP
        │
        ▼
Edit mode rendering:
  [Normal]  → GridAddNewRow.razor or NormalEdit.razor (new row at top/bottom)
  [Dialog]  → DialogEdit.razor (modal opens with empty form)
  [Batch]   → new row appears in batch buffer

        │
        ▼
User fills in values → each editor calls back Edit<T>

        │
        ▼
User clicks Save / Update
        │
        ▼
GridEvents.OnActionBegin.InvokeAsync(ActionEventArgs {
    RequestType = Action.Save,
    Data        = filled TValue object
})

        │
        ▼
Edit<T>.SaveRecord()
  ├── Validate via ColumnsValidator (data annotations + custom rules)
  ├── If validation fails → show ValidationTooltip / ValidationDialog → STOP
  │
  └── GridEvents.OnSave.InvokeAsync(args)
        → Application should persist to backend here

        │
        ▼
DataSource updated (application responsibility)
Grid refreshes current page
GridEvents.OnActionComplete.InvokeAsync(...)
```

---

## Flow 9: CRUD — Delete Record

```
User clicks Delete (toolbar / command column / context menu)
        │
        ▼
SfGrid.DeleteRecordAsync()
  └── EditModule.DeleteRecord(rowData)

        │
        ▼
GridEvents.OnActionBegin.InvokeAsync(ActionEventArgs {
    RequestType = Action.Delete,
    Data        = selected row TValue
})

  If args.Cancel == true → STOP
        │
        ▼
Edit<T>.DeleteRecord()
  └── GridEvents.OnDelete.InvokeAsync(args)
      → Application removes record from DataSource

        │
        ▼
AddOrDeleteArgs = { Action = "Delete", ... }   ← signal OnAfterRenderAsync
StateHasChanged()
        │
        ▼
OnAfterRenderAsync()
  └── EditModule.EditComplete(addDeleteArgs)
        │
        ▼
[With ShowAddNewRow + Virtualization — Bug 1011415 fix]
  Form is destroyed BEFORE content re-renders:
  ├── Capture addedRowElement.style.height = addedRowElement.offsetHeight + "px"
  ├── Remove formObj.element from DOM (parentElement.removeChild)
  └── Preserve addedRowElement wrapper — prevents flicker on virtualization re-render

        │
        ▼
Grid refreshes → DataProcess() called
GridEvents.OnActionComplete.InvokeAsync(...)
```

---

## Flow 10: Selection

```
User clicks a row
        │
        ▼
[JS: click event → DotNet callback via GridJSInteropAdaptor]
        │
        ▼
Selection<T>.SelectRow(rowIndex)
  ├── Check SelectionSettings.Mode (Row / Cell / Both)
  ├── Check SelectionSettings.Type (Single / Multiple)
  │
  ├── GridEvents.OnRowSelecting.InvokeAsync(args)
  │     If args.Cancel == true → STOP
  │
  ├── Update internal SelectedRowIndexes collection
  ├── Apply IsSelected = true on GridRow model
  │
  └── GridEvents.RowSelected.InvokeAsync(args)

  [With PersistSelection = true]
  └── SelectedRowIndexes persisted in localStorage
      Re-applied after filter / paging / sort
```

---

## State Transition Flows

### Property Change Detection Flow

```
Parent component re-renders
        │
        ▼
Blazor passes new [Parameter] values to SfGrid
        │
        ▼
OnParametersSetAsync()
  └── OnHybridParametersSet()
        └── UpdateProperty(nameof(AllowSorting), AllowSorting, _allowSorting)
              │
              ├── If AllowSorting != _allowSorting:
              │     PropertyChanges["AllowSorting"] = AllowSorting
              │     _allowSorting = AllowSorting
              │
              └── Returns new value

        │
        ▼
PropertyChanges analyzed:
  ├── IsRefreshable property changed? → ModelChanged(Refresh)
  ├── Columns changed?               → RefreshColumnHeader = true
  ├── GroupSettings changed?         → RefreshColumnHeader = true
  ├── SelectedRowIndex changed?      → _rowIndexPropertyChanged = true
  └── DataSource changed?            → clear selections, reset scroll
```

---

## Data Consistency Checks

### 1. Query Equality Check (Query parameter)
The `Query` parameter uses `Query.IsEqual()` comparison to avoid false refreshes when the same query object is re-passed. A local snapshot is captured **before** `OnHybridParametersSet()` to compare against the incoming value:
```csharp
// Captured before OnHybridParametersSet() executes
var query = _query ?? new Query();

// After OnHybridParametersSet() updates _query:
if (PropertyChanges.ContainsKey("Query"))
{
    if (Query.IsEqual(query, Query))
    {
        PropertyChanges.Remove("Query");  // Not actually changed — avoid spurious refresh
    }
}
```

### 2. DataSource Reference Check
ObservableCollection wiring uses reference equality to prevent double-subscribing:
```csharp
if (!object.ReferenceEquals(DataSource, _dataSource))
{
    UpdateObservableEvents(nameof(DataSource), _dataSource, true);  // unwire old
    UpdateObservableEvents(nameof(DataSource), DataSource);          // wire new
}
```

### 3. Virtual Scroll Cache Hit
Before fetching data, `VirtualScroll<T>` checks `GeneratedData` (a page-indexed dictionary) to avoid redundant server calls when the user scrolls back to already-rendered pages.

### 4. Selection Consistency After DataSource Change
When `DataSource` changes, all row/cell `IsSelected` flags are reset and `CheckBoxState` is set to `UnCheck`. The header checkbox and persist data are also cleared:
```csharp
if (PropertyChanges.ContainsKey(nameof(DataSource)))
{
    Rows?.ForEach(_ =>
    {
        _.IsSelected = false;
        _.Cells?.ForEach(c => c.IsSelected = false);
    });
    CheckBoxState = CheckState.UnCheck;
    if (SelectionModule != null)
    {
        SelectionModule.IsHeaderCheckboxChecked = false;
        SelectionModule.SetPersistData(state: CheckBoxState);
    }
    _rowIndexPropertyChanged = false;
}
```

---

## Caching Mechanisms

| Cache | Location | Key | Purpose |
|-------|----------|-----|---------|
| Virtual row data | `VirtualScroll<T>.GeneratedData` | page index (`int`) | Avoid re-fetching scrolled-past pages |
| Virtual row objects | `VirtualScroll<T>.GeneratedRows` | page index | Avoid re-constructing row models |
| Virtual frozen row data | `VirtualScroll<T>.FrozenCachedData` | page index | Frozen column virtual scroll data |
| Virtual frozen row objects | `VirtualScroll<T>.FrozenCachedRowObject` | page index | Frozen row model cache |
| Virtual group data | `VirtualScroll<T>.GroupGeneratedData` | page index | Group+virtual combo |
| Column info cache | `PropHelper` (PropertyInfoHelper) | property name | Avoid repeated `typeof(T).GetProperty()` |
| Persist state | `window.localStorage["grid{ID}"]` | grid ID | Cross-session state restore |
| Infinite scroll blocks | `InfiniteScroll<T>` internal blocks | block index | Append-only data blocks |
| Original properties | `SfGrid._originalProp` | — | Baseline for `ResetPersistDataAsync` |

---

## Error Handling in Data Flow

| Scenario | Behavior |
|----------|----------|
| DataSource is null | Grid renders empty state ("No records to display") |
| Remote fetch fails | Adaptor throws; grid shows error or empty (application should handle `OnActionFailure`) |
| Validation fails on save | `ColumnsValidator` shows inline tooltip; save is blocked |
| Edit action cancelled | `OnActionBegin` with `Cancel = true` stops the pipeline silently |
| Column Field mismatch | `PropertyInfoHelper` returns null; cell renders empty (no exception) |
| Virtual scroll out of range | `RowStartIndex`/`RowEndIndex` clamped to data bounds |

---

## Flow 11: ObservableCollection Live Updates

```
Application modifies ObservableCollection<TValue>
  (Add / Remove / Reset)
        │
        ▼
CollectionChanged event fires on background thread
        │
        ▼
SfGrid.UpdateObservableEvents() handler
  └── InvokeAsync(() => DataProcess())   ← marshal to Blazor sync context
        │
        ▼
DataProcess() → adaptor reads updated collection
[ReactiveAggregateModule re-runs aggregates]
StateHasChanged()  ← re-render with new data
```

**Guard**: `_isObservableWired` flag prevents double-subscribing when `DataSource` reference is re-set to the same collection.

---


*For module dependency relationships, see [`architecture/dependency-map.md`](./dependency-map.md).*  
*For component hierarchy, see [`architecture/component-architecture.md`](./component-architecture.md).*
