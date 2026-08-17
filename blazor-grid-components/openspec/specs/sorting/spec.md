# Sorting

## Summary
The Sorting feature in `SfGrid<TValue>` enables users to reorder grid rows based on one or more column values. It supports both single-column and multi-column sorting, ascending and descending directions, programmatic control, and cancellable events — all executed server-side in .NET with the `Sort<T>` action module orchestrating state and query generation.

---

## Motivation & Use Cases
- **Primary user goal**: Quickly find records by ordering data along a chosen column (e.g., sort orders by date or freight).
- **Key scenarios**:
  - Single-column sort by clicking a column header.
  - Multi-column sort by holding **Shift** (or **Ctrl** on Mac/mobile) while clicking additional headers.
  - Remove sort on a column by clicking its header a third time (unsort) when `AllowUnsort = true`.
  - Pre-sort the grid at initial load via declarative `GridSortColumns` child components.
  - Programmatically sort/unsort columns via `SortColumnAsync`, `SortColumnsAsync`, and `ClearSortingAsync` methods.
  - Prevent sorting on specific columns via `GridColumn.AllowSorting = false`.
  - Cancel a pending sort via `SortingEventArgs.Cancel = true` in the `Sorting` event.
- **Success criteria**:
  - Column headers display ascending/descending directional icons reflecting current sort state.
  - Grid data re-renders in the sorted order after each sort action.
  - `OnActionBegin` and `OnActionComplete` fire reliably; `Sorting` event cancels correctly.
  - Sort state persists across page navigation when `EnablePersistence = true`.

---

## Inputs
- **Data inputs**: `SfGrid.DataSource` (`IEnumerable<TValue>`) or remote `SfDataManager` result set.
- **User inputs**:
  - Left-click on a column header → single-column sort.
  - Shift+Click (Windows/Linux) or Meta+Click (Mac) on a column header → multi-column sort.
  - Third click on a sorted column header (when `AllowUnsort = true`) → removes sort.
  - Column menu items `SortAscending` / `SortDescending` → sort via menu.
  - Context menu items `SortAscending` / `SortDescending`.
- **External triggers**:
  - `SortColumnAsync(string columnName, SortDirection direction, bool? isMultiSort)` method call.
  - `SortColumnsAsync(List<SortColumn> columns, bool clearPreviousSort)` method call.
  - `ClearSortingAsync()` / `ClearSortingAsync(List<string> fieldNames)` method calls.
  - Declarative initial sort via `GridSortSettings > GridSortColumns > GridSortColumn` child components.
  - State persistence restore from `localStorage` on first render (when `EnablePersistence = true`).

---

## Outputs
- **UI outputs**:
  - Sort direction icon (`e-ascending` / `e-descending` CSS class) on the active column header cell.
  - Sort priority number badge on each sorted column header when multi-sorting is active.
  - Grid rows re-rendered in the new sorted order.
  - Column header re-rendered (`RefreshColumnHeader = true`) to reflect updated icons.
- **Events / callbacks**:

  | Event | Type | Moment |
  |-------|------|--------|
  | `GridEvents.Sorting` | `EventCallback<SortingEventArgs>` | Before sort is applied; cancellable |
  | `GridEvents.Sorted` | `EventCallback<SortedEventArgs>` | After sort is applied |
  | `GridEvents.OnActionBegin` | `EventCallback<ActionEventArgs<TValue>>` | Before any grid action, including sort |
  | `GridEvents.OnActionComplete` | `EventCallback<ActionEventArgs<TValue>>` | After any grid action, including sort |

- **Persisted artifacts**:
  - When `EnablePersistence = true`, sort state (`SortSettings.Columns`) is serialised into `localStorage` key `"grid{ID}"` on dispose.

---

## States
- **Unsorted** — no sort columns in `SortSettings.Columns`; no icons shown.
- **Single-sorted** — one entry in `SortSettings.Columns`; one header shows a direction icon.
- **Multi-sorted** — multiple entries in `SortSettings.Columns`; each sorted header shows its direction icon and a numeric priority badge.
- **Sort-pending** — `Sorting` event is fired but `Cancel` has not yet been evaluated; data has not changed.
- **Loading** — `ModelChanged(RequestType = Sorting)` dispatched; `SfDataManager` executes query; spinner displayed.
- **Sort-complete** — data returned, rows re-rendered, `Sorted` / `OnActionComplete` events fired.

**State transitions**:
- Unsorted → Single-sorted: user clicks a sortable column header (ascending direction applied first).
- Single-sorted → Single-sorted: user clicks the same column (direction toggles Ascending ↔ Descending).
- Single-sorted → Unsorted: user clicks the same column again when it is Descending and `AllowUnsort = true`, OR calls `ClearSortingAsync()`.
- Any state → Multi-sorted: user Shift/Ctrl-clicks an additional column while `AllowMultiSorting = true`.
- Multi-sorted → reduced multi-sorted: user Shift/Ctrl-clicks an already-sorted column to remove it from the sort set.
- Any state → Unsorted: `ClearSortingAsync()` removes all non-grouped sort columns.

---

## Configuration

### `SfGrid<TValue>` properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AllowSorting` | `bool` | `false` | Master switch — enables column header click-to-sort. |
| `AllowMultiSorting` | `bool` | `true` | When `true` (and `AllowSorting = true`), Shift/Ctrl+Click adds columns to the sort set. |
| `SortSettings` | `GridSortSettings?` | `null` | Container for sort behaviour configuration. |

### `GridSortSettings` properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AllowUnsort` | `bool` | `true` | When `true`, clicking a Descending column removes it from sort. When `false`, the column cycles Asc → Desc only. |
| `Columns` | `List<GridSortColumn>?` | `null` | Initial (and live) sorted column list. |

### `GridSortColumn` properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Field` | `string` | `""` | Column field name to sort by. Must match `GridColumn.Field`. |
| `Direction` | `SortDirection` | `Ascending` | `SortDirection.Ascending` or `SortDirection.Descending`. |
| `IsFromGroup` | `bool` | `false` | Internal flag; `true` when the sort entry was created by a grouping operation. |

### `GridColumn` properties related to sorting

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AllowSorting` | `bool` | `true` | Set `false` to disable sorting for this specific column. |

### Constraints
- `AllowSorting` on `SfGrid` must be `true` for any column-level or programmatic sort to take effect.
- Template columns and command columns have `AllowSorting` automatically set to `false` (no `Field` binding).
- `AllowMultiSorting` has no effect if `AllowSorting = false`.
- `GridSortColumn.Direction` must be `Ascending` or `Descending`; `SortDirection.None` is only used in event payloads to signal removal.

### Example — declarative initial sort

```razor
<SfGrid DataSource="@Orders" AllowSorting="true" AllowMultiSorting="true">
    <GridSortSettings AllowUnsort="true">
        <GridSortColumns>
            <GridSortColumn Field="@nameof(Order.OrderDate)" Direction="SortDirection.Descending" />
            <GridSortColumn Field="@nameof(Order.Freight)"   Direction="SortDirection.Ascending" />
        </GridSortColumns>
    </GridSortSettings>
    <GridColumns>
        <GridColumn Field="@nameof(Order.OrderID)"   HeaderText="Order ID"   IsPrimaryKey="true" Width="120" />
        <GridColumn Field="@nameof(Order.OrderDate)" HeaderText="Order Date"  Format="d"          Width="130" />
        <GridColumn Field="@nameof(Order.Freight)"   HeaderText="Freight"     Format="C2"          Width="120" />
        <GridColumn Field="@nameof(Order.CustomerName)" HeaderText="Customer" Width="150"
                    AllowSorting="false" />
    </GridColumns>
</SfGrid>
```

---

## Behaviors & Rules

### Invariants
- A sort action is a no-op if `SfGrid.AllowSorting = false` or `GridColumn.AllowSorting = false`.
- `SortColumn` direction cycles: Unsorted → Ascending → Descending → (Unsorted if `AllowUnsort = true` else stays Descending).
- When `AllowMultiSorting = false`, clicking a new column replaces the existing sort (grouped columns are preserved in-place).
- Grouped columns are always included in `SortSettings.Columns` and cannot be removed by `ClearSortingAsync` — only ungrouping removes them.
- Sort state in `SortSettings.Columns` is the single source of truth; the `Sort<T>` module reads and mutates this list directly.
- Selection is cleared after a sort action unless `GridSelectionSettings.PersistSelection = true`.

### Ordering / priority rules
- In multi-sort, columns appear in the `SortSettings.Columns` list in the order they were sorted; the `DataGenerator` applies them as `SortBy()` clauses in list order.
- When `AllowMultiSorting = false`, only one non-grouped column can exist in the sort list at any time.
- Group columns always precede user-sorted columns in the `SortSettings.Columns` list.

### Error handling rules
- If `Field` supplied to `SortColumnAsync` does not resolve to a `GridColumn`, the call returns silently (no exception).
- If `SortingEventArgs.Cancel = true`, `SortSettings.Columns` is **not** updated and `OnActionComplete` is **not** fired.

---

## Workflows

### Workflow 1 — User clicks a column header
```
1. JS fires header click → GridJSInteropAdaptor routes to Sort<T>.SortClickHandler(column, cssClass, mouseArgs)
2. Sort<T>.SortClickHandler checks:
   - AllowSorting == true
   - column.Type != CheckBox
3. Sort<T>.InitiateSort() determines direction from current CSS class:
   - Not "e-ascending" → Ascending
   - Is "e-ascending" → Descending
4. If Shift/Ctrl held and column already sorted → RemoveSortColumn()
   Else → SortColumn(field, direction, isMultiSort)
5. Sort<T>.SortColumn():
   a. Resolve column; guard AllowSorting / AllowMultiSorting
   b. UpdateModel() → mutates SortSettings.Columns
   c. Clear selection (if PersistSelection = false)
   d. ModelChanged(RequestType = Sorting) → fires OnActionBegin / Sorting event
   e. If Cancel == false: DataGenerator builds SortQuery → SfDataManager executes
   f. CurrentViewData updated → StateHasChanged() → Blazor re-render
   g. Fires Sorted / OnActionComplete events
```

### Workflow 2 — Programmatic sort via `SortColumnAsync`
```
1. Caller: await grid.SortColumnAsync("Freight", SortDirection.Descending)
2. SfGrid.SortColumnAsync delegates to Sort<T>.SortColumn(..., invokedByMethod: true)
3. Same pipeline as Workflow 1, step 5 onwards
```

### Workflow 3 — Sort multiple columns via `SortColumnsAsync`
```
1. Caller: await grid.SortColumnsAsync(columns, clearPreviousSort: true)
2. If clearPreviousSort == true → each non-grouped column is removed from SortSettings.Columns
3. For each SortColumn in list: Sort<T>.SortColumn(..., multipleCols: true) — skips ModelChanged
4. After all columns processed: single ModelChanged(RequestType = Sorting) with full SortedColumns list
```

### Workflow 4 — Clear all sorting
```
1. Caller: await grid.ClearSortingAsync()
2. For each non-grouped column in SortSettings.Columns: Sort<T>.RemoveSortColumn(field, multipleCols: true)
3. ModelChanged(RequestType = Sorting, Action = Reset, Direction = None)
4. DataGenerator rebuilds query without SortQuery → SfDataManager re-fetches
5. Grid re-renders without sorted row order; all sort icons removed
```

---

## Architecture

### Component boundaries
- **`SfGrid<TValue>`** — hosts `SortModule` (instance of `Sort<T>`), `SortSettings` property (`GridSortSettings`), `AllowSorting`, and `AllowMultiSorting` properties.
- **`Sort<T>`** (`Internal/Actions/Sort.cs`) — business logic: click handling, `UpdateModel`, query dispatch.
- **`GridSortSettings`** (`GridSortSettings.razor.cs`) — Razor child component holding `AllowUnsort` and `Columns` list; communicates changes to parent via `UpdateChildProperties`.
- **`GridSortColumns`** (`GridSortColumns.razor.cs`) — container for declarative `GridSortColumn` children; accumulates them into a `List<GridSortColumn>`.
- **`GridSortColumn`** (`GridSortColumn.cs`) — per-column sort descriptor with `Field`, `Direction`, `IsFromGroup`.
- **`DataGenerator<T>`** (`Internal/Actions/Data.cs`) — consumes `SortSettings.Columns` to produce `SortBy()` query clauses.
- **`GridHeader.razor` / `GridHeaderCell.razor`** — renders sort icons based on `SortSettings.Columns` state.

### Internal modules and collaboration
```
User click
  └─► Sort<T>.SortClickHandler
        └─► Sort<T>.SortColumn / RemoveSortColumn
              ├─► Sort<T>.UpdateModel  → mutates SortSettings.Columns
              └─► SfGrid.ModelChanged(RequestType = Sorting)
                    ├─► Fires OnActionBegin / Sorting event (cancellable)
                    └─► DataGenerator<T>.GenerateQuery()
                          └─► SfDataManager.ExecuteQuery()
                                └─► CurrentViewData updated
                                      └─► StateHasChanged() → re-render
                                            └─► Fires Sorted / OnActionComplete
```

### Client/server responsibilities
- **All sort logic is .NET-side** — `Sort<T>` module, `DataGenerator`, and `SfDataManager` run fully in .NET (Server or WASM).
- **JS role is header click dispatch only** — `sfBlazor.Grid.js` captures the header click DOM event and calls `.NET` via `DotNetObjectReference`; no client-side sort computation occurs.

### Links to internal docs
- System architecture: `docs/architecture/system-architecture.md` — "Module Injection Pattern", "Event Flow Architecture"
- Data flow: `docs/architecture/system-architecture.md` — "DataGenerator<T>" section

---

## Data Flow

### Source and path
```
SortSettings.Columns (List<GridSortColumn>)
    │
    ▼
DataGenerator<T>.GenerateQuery()
    └─ SortQuery: foreach column → query.SortBy(field, direction)
    │
    ▼
SfDataManager.ExecuteQuery(query)
    ├─ Local (BlazorAdaptor): LINQ OrderBy / ThenBy on IEnumerable<TValue>
    └─ Remote (WebApiAdaptor / ODataV4Adaptor / etc.): $orderby query parameter
    │
    ▼
IEnumerable<object> result
    │
    ▼
SfGrid.CurrentViewData  →  Blazor re-render
```

### Async notes
- `SortColumn` / `RemoveSortColumn` are `async Task` and run on the Blazor synchronization context.
- `ModelChanged` uses `.ConfigureAwait(true)` throughout to stay on the Blazor context.
- Remote data sources: sort is applied server-side via query string parameters; the grid does not sort the returned page locally.

### Event emission sequence
```
Sort<T>.SortColumn()
  │  fires ──► OnActionBegin(RequestType=Sorting)   [cancellable via args.Cancel]
  │  fires ──► Sorting(SortingEventArgs)             [cancellable via args.Cancel]
  │  (if not cancelled)
  │  executes data fetch
  │  fires ──► Sorted(SortedEventArgs)
  └──fires ──► OnActionComplete(RequestType=Sorting)
```

---

## Events & Integration Points

### Emitted events

| Event | Class | Key Payload Properties |
|-------|-------|------------------------|
| `Sorting` | `SortingEventArgs` | `ColumnName`, `Direction`, `Action` (`Add`/`Replace`/`Remove`), `Cancel` (settable), `IsCtrlKeyPressed` |
| `Sorted` | `SortedEventArgs` | `ColumnName`, `Direction`, `Action`, `SortedColumns` (populated by `SortColumnsAsync`) |
| `OnActionBegin` | `ActionEventArgs<TValue>` | `RequestType = Action.Sorting`, `ColumnName`, `Direction`, `Cancel` |
| `OnActionComplete` | `ActionEventArgs<TValue>` | `RequestType = Action.Sorting`, `ColumnName`, `Direction` |

### `SortingEventArgs.Action` values

| Value | Meaning |
|-------|---------|
| `Add` | New column added to the sort set |
| `Replace` | Existing column's direction toggled |
| `Remove` | Column removed from the sort set |
| `Reset` | All sort columns cleared (`ClearSortingAsync`) |

### Consumed events / commands
- `SfGrid.AllowSorting` parameter change → triggers `RefreshColumnHeader = true` (header re-render).
- `SfGrid.SortSettings` parameter change → calls `OnHybridParametersSet`, recorded in `PropertyChanges`, triggers `ModelChanged(Refresh)`.

### External services / APIs
- `SfDataManager` (Syncfusion.Blazor.Data) — translates `SortQuery` into adaptor-specific sort parameters.
- `sfBlazor.Grid.js` — routes DOM header click events to `Sort<T>.SortClickHandler` via the `DotNetObjectReference`.

---

## API Details

### `SfGrid<TValue>` methods

| Method | Signature | Description |
|--------|-----------|-------------|
| `SortColumnAsync` | `Task SortColumnAsync(string columnName, SortDirection direction, bool? isMultiSort = null)` | Programmatically sorts a single column. `isMultiSort = true` appends to the existing sort; `false` replaces it. |
| `SortColumnsAsync` | `Task SortColumnsAsync(List<SortColumn> columns, bool clearPreviousSort)` | Sorts multiple columns in one call. Pass `clearPreviousSort = true` to clear existing non-grouped sorts first. |
| `ClearSortingAsync` | `Task ClearSortingAsync()` | Removes all non-grouped sort columns and triggers a single data refresh. |
| `ClearSortingAsync(fields)` | `Task ClearSortingAsync(List<string> fieldNames)` | Removes only the named columns from the sort set. |

### `SfGrid<TValue>` properties (sorting-related)

| Property | Type | Default |
|----------|------|---------|
| `AllowSorting` | `bool` | `false` |
| `AllowMultiSorting` | `bool` | `true` |
| `SortSettings` | `GridSortSettings?` | `null` |

### `GridSortSettings` properties

| Property | Type | Default |
|----------|------|---------|
| `AllowUnsort` | `bool` | `true` |
| `Columns` | `List<GridSortColumn>?` | `null` |

### `GridSortColumn` properties

| Property | Type | Default |
|----------|------|---------|
| `Field` | `string` | `""` |
| `Direction` | `SortDirection` | `Ascending` |
| `IsFromGroup` | `bool` | `false` |

### `GridColumn` sorting property

| Property | Type | Default |
|----------|------|---------|
| `AllowSorting` | `bool` | `true` |

### `SortDirection` enum values

| Value | Description |
|-------|-------------|
| `Ascending` | Sorts A → Z / smallest → largest |
| `Descending` | Sorts Z → A / largest → smallest |
| `None` | No sort applied (used in event payloads for removal) |

---

## Dependencies

### Internal modules
- `DataGenerator<T>` — consumes `SortSettings.Columns` to build `SortBy()` query clauses.
- `Grouping<T>` — group columns are included in `SortSettings.Columns` with `IsFromGroup = true`; sort module preserves these on clear.
- `Selection<T>` — selection is cleared after sort unless `PersistSelection = true`.
- `FocusHandler<T>` — focus state may shift after row re-render following sort.
- `GridJSInteropAdaptor<T>` — delivers header click events from browser JS to `Sort<T>`.
- `EventAggregator` — used by related modules for cross-module communication during render cycle.

### External libraries / services
- `Syncfusion.Blazor.Data` — `SfDataManager`, `Query`, adaptors for translating sort queries to remote endpoints.
- `Microsoft.AspNetCore.Components` — Blazor lifecycle (`[Parameter]`, `EventCallback`, `CascadingParameter`).

### Feature flags / toggles
- `SfGrid.AllowSorting` — master enable/disable.
- `SfGrid.AllowMultiSorting` — enables/disables multi-column sort.
- `GridSortSettings.AllowUnsort` — enables/disables the unsort (third-click removal) gesture.
- `GridColumn.AllowSorting` — per-column opt-out.
- `SfGrid.EnablePersistence` — persists sort state across page reloads.
- `SfGrid.ShowColumnMenu` — exposes `SortAscending` / `SortDescending` column menu items.

---

## Edge Cases

- **Empty dataset**: Sort action completes normally; `CurrentViewData` remains empty; no error.
- **Single-column sort with grouping active**: Grouped columns remain in `SortSettings.Columns` with `IsFromGroup = true`; `ClearSortingAsync` skips them; user-clicked sort is appended after grouped entries.
- **`AllowUnsort = false` with Descending sort**: Clicking the Descending header again toggles back to Ascending rather than removing the sort.
- **Remote data with multi-sort**: All sort columns are forwarded as `$orderby` / `sortBy` parameters in the single data request; no partial fetches.
- **`EnablePersistence = true` with stale column set**: On restore, if a persisted `Field` no longer exists in the current column definition, `GridUtils.GetColumnByField` returns `null` and the sort entry is silently ignored.
- **`SortColumnsAsync` with `clearPreviousSort = false`**: New columns are appended to existing sort set; duplicate fields are replaced (the old entry is removed and the new one appended at the end).
- **Sort cancelled via `Sorting` event**: `SortSettings.Columns` is already mutated by `UpdateModel` before the event fires; the cancellation reverts the model update and prevents the data fetch.
  - ⚠️ Implementors must ensure model rollback on cancel (tracked via `LastSortedCols`).
- **Virtual scrolling active**: Sort causes `VirtualScroll<T>` to reset scroll position to top; `VirtualComponentUpdate` event is triggered after data reload.
- **`AllowMultiSorting = false` on mobile / `IsDeviceMode = true`**: The device mode flag also enables multi-sort behaviour; `AllowMultiSorting = false` overrides the device-mode multi-sort.
- **Large dataset (100K+ rows), local data**: LINQ `OrderBy / ThenBy` operates on the full in-memory collection; for very large local sets, use remote data + `WebApiAdaptor` to push sorting to the database.
- **RTL layout**: Sort icon positions are mirrored; directional logic is unchanged.
- **Accessibility**: Sort state communicated via `aria-sort` attribute on `<th>` elements (`ascending` / `descending` / `none`).

---

## Non-Functional Requirements

### Performance
- For local data, sorting is a LINQ in-memory operation; latency is proportional to dataset size.
- For remote data, the sort parameter is appended to the outbound query; no additional round-trips beyond the normal page fetch.
- `RefreshColumnHeader = true` triggers only a header sub-tree re-render (not a full grid re-render) when column/sort settings change.
- Multi-sort with `SortColumnsAsync` issues a single `ModelChanged` call regardless of the number of sort columns, minimising re-renders.

### Accessibility
- WCAG 2.0 AA target: `aria-sort` attribute updated on each sorted header.
- Keyboard: column header focusable via Tab; Enter / Space activates sort (handled by `FocusHandler<T>`).
- Screen reader announces sort direction change after re-render.

### Security & privacy
- Sort parameters sent to remote APIs are derived from `GridColumn.Field` names; no user-supplied raw strings reach the query without validation against the column definition.

### Telemetry / logging
- No built-in telemetry hooks; `OnActionBegin` / `OnActionComplete` events provide all observable sort lifecycle information for consumer-level instrumentation.
