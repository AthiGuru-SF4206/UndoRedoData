# Row & Column Virtualization

## Summary
Row & Column Virtualization enables `SfGrid<TValue>` to render only the rows and columns currently visible in the viewport (plus a configurable overscan buffer), rather than materializing the full dataset as DOM nodes. This keeps frame rendering fast and memory consumption flat regardless of dataset size—tens of thousands of rows or hundreds of columns can scroll smoothly because the DOM node count stays proportional to the visible window, not the total data size. Infinite Scrolling is explicitly out of scope; it is handled by the separate `InfiniteScroll<T>` module.

---

## Motivation & Use Cases

- **Primary user goal:** Scroll through very large datasets (10 k–1 M rows, 50–500 columns) without a perceptible rendering lag or browser memory spike.
- **Key scenarios:**
  - Financial dashboards with high-frequency tick data updates across many columns.
  - ERP grids with hundreds of columns where only a horizontal slice is relevant at any moment.
  - Read-heavy reporting grids that must remain responsive while a background fetch refreshes data.
  - Editing a single row inside a virtualized grid without requiring a full re-render.
- **Success criteria / measurable outcomes:**
  - First meaningful paint of 10 k rows ≤ time to paint 50 rows without virtualization (DOM node count is bounded by viewport window).
  - Vertical/horizontal scroll frame rate ≥ 60 fps on reference hardware.
  - Memory footprint does not grow linearly with `TotalItemCount`.
  - Cache hit on repeat scroll to a previously visited position (no redundant data fetch).

---

## Inputs

- **Data inputs:**
  - `DataSource` / `DataManager` bound to `SfGrid<TValue>` — queried in pages whose size is derived from `GridPageSettings.PageSize` (auto-calculated when default).
  - `TotalItemCount` — total record count used to size the virtual scroll track and compute `RowEndIndex`.
  - `GroupGeneratedData` — pre-grouped virtual cache when `GridGroupSettings` is active.
- **User inputs:**
  - Vertical scroll event (mouse wheel, scrollbar drag, touch) → `HandleVerticalScrollAsync`.
  - Horizontal scroll event → `HandleHorizontalScrollAsync`.
  - Add / Edit / Delete actions that must reposition the viewport or invalidate the cache.
  - Keyboard navigation (arrow keys, Page Up / Down) inside a virtualized grid.
- **External triggers:**
  - `EnableVirtualization` toggled at runtime.
  - `DataSource` reference change → `IsDataSourceChanged` flag → `sfBlazor.Grid.refreshOnDataChange`.
  - Sorting, filtering, searching, grouping, row-reorder, freeze-line reorder — all trigger full cache invalidation via `ClearCacheData()`.

---

## Outputs

- **UI outputs:**
  - A viewport-sized DOM slice of `GridRow` / `CellRender` components, positioned via CSS `translateY` (vertical) and `translateX` (horizontal).
  - Skeleton placeholder rows when `EnableVirtualMaskRow = true`.
  - `GridVirtualContent.razor` — virtual scroll container rendered only when `EnableVirtualization = true`.
  - `GridVirtualHeader.razor` — virtual column header slice rendered only when `EnableColumnVirtualization = true`.
- **Events / callbacks:**
  - `VirtualComponentUpdate` — fired after vertical scroll cache resolution; signals Blazor to re-render the row slice.
  - `VirtualHeaderComponentUpdate` — fired after horizontal scroll; signals re-render of the column header slice.
- **Persisted artifacts:**
  - `GeneratedData` — in-memory row data cache (not persisted across page reload).
  - `GeneratedRows` — in-memory rendered row object cache.

---

## States

| State | Description |
|---|---|
| **Idle** | Grid rendered; scroll position stable; cache warm for current window. |
| **Scrolling-Vertical** | `HandleVerticalScrollAsync` active; evaluating cache hit or miss. |
| **Scrolling-Horizontal** | `HandleHorizontalScrollAsync` active; recalculating `StartColumnIndex`/`EndColumnIndex`. |
| **Fetching** | Cache miss; `DataProcess()` called; optional mask rows visible. |
| **Mask** | `EnableVirtualMaskRow = true` and fetch in flight; skeleton rows displayed. |
| **CRUD-Active** | Add/Edit/Delete form open inside virtual viewport. |
| **Cache-Invalid** | `ClearCacheData()` called; next scroll forces a full re-fetch. |
| **Column-Virt-Init** | `RefreshColOffsets()` building `_coffSets`; column index not yet resolved. |

**State-transition rules:**
- Idle → Scrolling-Vertical / Scrolling-Horizontal on JS scroll event.
- Scrolling → Idle (cache hit) or Fetching (cache miss).
- Fetching → Idle after data arrives and `SetCurrentViewData` + re-render complete.
- Any cache-invalidating action → Cache-Invalid → Fetching on next scroll or render trigger.
- `EnableVirtualization` toggled off → `sfBlazor.Grid.virtualDisconnect` → full non-virtual render.

---

## Configuration

| Property | Type | Default | Constraint | Description |
|---|---|---|---|---|
| `EnableVirtualization` | `bool` | `false` | — | Enables row virtualization. |
| `EnableColumnVirtualization` | `bool` | `false` | Requires `EnableVirtualization = true` | Enables column virtualization. |
| `EnableVirtualMaskRow` | `bool` | `false` | Only meaningful when `EnableVirtualization = true` | Shows skeleton placeholder rows during async fetch. |
| `OverscanCount` | `int` | `0` | ≥ 0 | Extra rows/columns pre-rendered beyond the visible boundary. |
| `GridPageSettings.PageSize` | `int` | `12` (auto) | Auto-overridden by `EnsurePageSize()` | Controls the data window size per virtual page. |
| `GridColumn.Width` | `string`/`int` | `"200px"` (fallback) | **Required** for column virtualization | All columns must have an explicit width when `EnableColumnVirtualization = true`. |

**Auto-calculation:**
- `EnsurePageSize()` overrides the default `PageSize` of 12 when the grid height and row height (`RHeight`) are known: `PageSize = (gridHeight / RHeight) × 2`.

**Examples:**
```razor
<SfGrid TValue="Order" DataSource="@Orders"
        EnableVirtualization="true"
        EnableColumnVirtualization="true"
        EnableVirtualMaskRow="true"
        OverscanCount="5" Height="600px">
    <GridPageSettings PageSize="40" />
    <GridColumns>
        <GridColumn Field="OrderID" Width="120" />
        <GridColumn Field="CustomerID" Width="150" />
        <!-- All columns must specify Width -->
    </GridColumns>
</SfGrid>
```

---

## Behaviors & Rules

- **Row virtualization invariant:** Only rows in the window `[RowStartIndex - OverscanCount, RowEndIndex + OverscanCount]` (clamped to `[0, TotalItemCount - 1]`) are present in the DOM.
- **Column virtualization invariant:** Only columns in the window `[StartColumnIndex, EndColumnIndex]` (a range spanning ≈ 2× the grid viewport width) are rendered. Frozen columns are always rendered regardless of horizontal scroll position.
- **Width requirement:** If a column has no explicit `Width` when `EnableColumnVirtualization = true`, it defaults to `200px`. Failure to set widths causes `RefreshColOffsets()` to produce incorrect `_coffSets` cumulative offset values.
- **CSS transform positioning:** Rendered rows are absolutely positioned using `transform: translateY(${TranslateY}px)` and columns using `translateX(${TranslateX}px)`, giving the illusion of the full virtual scroll height/width without materializing absent DOM nodes.
- **Cache precedence:** `GeneratedData[index]` is checked before any data fetch. Cache hit → re-render from memory; cache miss → `DataProcess()` query.
- **Cache invalidation:** Any of the following triggers `ClearCacheData()`, discarding all cached rows/columns: sorting, column reorder, searching, filtering, grid refresh, save, delete, grouping, ungrouping, freeze-line reorder, row drag-and-drop.
- **Grouping + Virtualization:** Uses `GroupGeneratedData` (keyed `Dictionary<int, GroupedDataItem>`). When `GridGroupSettings.EnableLazyLoading = true`, the path switches to `GetUiData()` + `GenerateLazyRowsobject()`.
- **CRUD — Add Top:** Edit row inserted at index 0 only when `RowStartIndex == 0`.
- **CRUD — Add Bottom:** `IsBottomAddForm()` returns `true` when `RowEndIndex == TotalItemCount`; edit row appended at bottom.
- **CRUD — Delete:** Uses `HasAddOrCancelAction` flag; calls `sfBlazor.Grid.clientTransformUpdate` with the `bottom` flag.
- **CRUD — Save:** `ScrollToEditedRowAsync()` scrolls the viewport to the edited row before persisting, ensuring the row is in the DOM.
- **OverscanCount math:**
  - `CurrentIndexes(start, end)` → `(start - overscan, end + overscan)` clamped to valid bounds.
  - `VirtualIndexes(start, end)` → full overscan-adjusted render window used for `GeneratedData` keying.

---

## Workflows

### Vertical Scroll (Row Virtualization)

```
1. JS scroll event fires on the virtual scroll container.
2. HandleVerticalScrollAsync receives new ScrollTop.
3. Compute new RowStartIndex / RowEndIndex from ScrollTop and RHeight.
4. Apply OverscanCount → call CurrentIndexes() / VirtualIndexes().
5. Check GeneratedData[RowStartIndex]:
   a. Cache HIT  → SetCurrentViewData(cachedRows)
                 → fire VirtualComponentUpdate
                 → Blazor re-renders row slice (no network call).
   b. Cache MISS → DataProcess() → async data fetch
                 → (if EnableVirtualMaskRow) show skeleton rows via
                   sfBlazor.Grid.clientTransformUpdate(maskFlag)
                 → on data arrival: store in GeneratedData[index]
                 → SetCurrentViewData → fire VirtualComponentUpdate
                 → Blazor re-renders.
6. sfBlazor.Grid.clientTransformUpdate(TranslateY) applied to
   reposition the DOM slice within the scroll track.
```

### Horizontal Scroll (Column Virtualization)

```
1. JS horizontal scroll event fires.
2. HandleHorizontalScrollAsync receives new scrollLeft.
3. GetColumnIndexes() computes new StartColumnIndex / EndColumnIndex
   from _coffSets and grid width × 2 viewport window.
4. Update TranslateX.
5. Fire VirtualHeaderComponentUpdate → re-render GridVirtualHeader.
6. Fire VirtualComponentUpdate → re-render row cells for new column slice.
7. sfBlazor.Grid.updateVirtualColumns(columnSlice) — pushes column
   metadata to JS side.
8. sfBlazor.Grid.clientTransformUpdate(TranslateX) for CSS offset.
```

### Initialization

```
1. SfGrid renders GridVirtualContent.razor (row virt) and/or
   GridVirtualHeader.razor (col virt).
2. sfBlazor.Grid.initialize called via JS interop.
3. Returns { RowHeight, IndentWidth } → stored as RHeight.
4. EnsurePageSize() recalculates PageSize if still at default 12.
5. RefreshColOffsets() builds _coffSets cumulative width dictionary
   (column virt only).
6. Initial DataProcess() fetches first window of rows.
7. GeneratedData[0] populated; VirtualComponentUpdate fires.
```

---

## Architecture

### Component Boundaries

| Layer | Responsibility |
|---|---|
| **`SfGrid<TValue>`** | Hosts public API properties; delegates virtual logic to `VirtualScroll<T>`. |
| **`VirtualScroll<T>`** (Internal/Actions) | Core virtualization engine — cache management, index calculation, scroll handling, JS interop orchestration. |
| **`GridVirtualContent.razor`** | Replaces `GridContent.razor` when `EnableVirtualization = true`; renders the viewport-sized DOM slice with `translateY` positioning. |
| **`GridVirtualHeader.razor`** | Replaces the standard header when `EnableColumnVirtualization = true`; renders visible column header cells with `translateX` positioning. |
| **`GridRow.razor` / `CellRender.razor`** | Standard row/cell renderers — reused unchanged; virtualization affects which rows/cells are passed to them. |
| **JS module (`sf-grid.js`)** | Owns DOM scroll listeners, measures row height, applies CSS transforms, manages virtual column DOM updates. |

### Internal Modules and Collaboration

```
SfGrid<TValue>
  └─ VirtualScroll<T>
       ├─ GeneratedData (row cache)
       ├─ GeneratedRows (row object cache)
       ├─ FrozenCachedData / FrozenCachedRowObject (frozen col cache)
       ├─ GroupGeneratedData (group virtual cache)
       ├─ _coffSets (column offset dictionary)
       └─ JS Interop ──► sf-grid.js
```

**Reference documents:**
- `docs/architecture/system-architecture.md` — VirtualScroll module role, component tree, initialization sequence, internal flags.
- `docs/data-flow.md` — Flow 6: Virtual Scroll (full step-by-step pipeline).

---

## Data Flow

**Source:** `DataSource` / `DataManager` → queried via `DataProcess()` with a virtual page window query (start index + page size).

**Transformation path:**
1. Raw data arrives → stored in `GeneratedData[RowStartIndex]` and `GeneratedRows[RowStartIndex]`.
2. `SetCurrentViewData()` extracts the visible slice from cache.
3. `VirtualComponentUpdate` event triggers Blazor diff/re-render of `GridVirtualContent`.
4. CSS `translateY` repositions the rendered slice to the correct scroll position within the full virtual track height.

**Column data path:**
1. All columns loaded upfront; `_coffSets` built once during init and invalidated on column state changes.
2. `GetColumnIndexes()` slices the column array on each horizontal scroll.
3. Only the sliced column objects are passed to `GridRow` / `CellRender`.

**Sync/Async:**
- Cache hits are synchronous (re-render from memory).
- Cache misses are async (data fetch); mask rows optionally bridge the wait.

**Batching / Pagination:** Fetch window = `PageSize` rows (auto-sized). No batching of multiple windows per fetch.

**Side-effects:**
- `IsDataSourceChanged = true` → `sfBlazor.Grid.refreshOnDataChange` wipes all caches and resets scroll position.
- Any cache-invalidating action (see Behaviors) → `ClearCacheData()`.

**Reference:** `docs/data-flow.md` — Flow 6: Virtual Scroll.

---

## Events & Integration Points

| Event | Direction | Payload | Purpose |
|---|---|---|---|
| `VirtualComponentUpdate` | Internal emit → `GridVirtualContent` | Current row slice + `TranslateY` | Triggers re-render of the visible row window. |
| `VirtualHeaderComponentUpdate` | Internal emit → `GridVirtualHeader` | Current column slice + `TranslateX` | Triggers re-render of the visible column header window. |
| JS scroll (vertical) | JS → .NET | `scrollTop` (float) | Initiates `HandleVerticalScrollAsync`. |
| JS scroll (horizontal) | JS → .NET | `scrollLeft` (float) | Initiates `HandleHorizontalScrollAsync`. |
| `sfBlazor.Grid.initialize` | .NET → JS | Grid element ref | Returns `{ RowHeight, IndentWidth }`. |
| `sfBlazor.Grid.virtualDisconnect` | .NET → JS | Grid element ref | Cleanup when virtualization disabled at runtime. |
| `sfBlazor.Grid.refreshColumnIndex` | .NET → JS | Column state | Recalculate column offsets after column change. |
| `sfBlazor.Grid.refreshOnDataChange` | .NET → JS | Grid element ref | Full virtual reset on `DataSource` change. |
| `sfBlazor.Grid.clientTransformUpdate` | .NET → JS | `{ translateY, translateX, maskFlag, bottomFlag }` | Apply CSS transform; toggle mask rows. |
| `sfBlazor.Grid.updateVirtualColumns` | .NET → JS | Column slice metadata | Push new column slice to JS-side DOM. |

---

## API Details

### Public Properties (on `SfGrid<TValue>`)

| Property | Type | Default | XML Doc Summary |
|---|---|---|---|
| `EnableVirtualization` | `bool` | `false` | Enables row virtualization for large datasets. |
| `EnableColumnVirtualization` | `bool` | `false` | Enables column virtualization for wide datasets. Requires `EnableVirtualization`. |
| `EnableVirtualMaskRow` | `bool` | `false` | Displays placeholder skeleton rows while data is being fetched during scroll. |
| `OverscanCount` | `int` | `0` | Number of extra rows/columns rendered beyond the visible boundary to reduce blank-flash on fast scroll. |

**Source:** `SfGrid.Properties.cs` lines 640–800.

### Supporting Configuration

| Property | Type | Location |
|---|---|---|
| `PageSize` | `int` | `GridPageSettings` |
| `Width` | `string` | `GridColumn` (required for column virtualization) |

### Internal Key Methods (on `VirtualScroll<T>`)

| Method | Purpose |
|---|---|
| `HandleVerticalScrollAsync(scrollTop)` | Main vertical scroll handler. |
| `HandleHorizontalScrollAsync(scrollLeft)` | Main horizontal scroll handler. |
| `ClearCacheData()` | Wipes `GeneratedData`, `GeneratedRows`, frozen caches, group caches. |
| `SetCurrentViewData(rows)` | Publishes the current row slice for Blazor re-render. |
| `EnsurePageSize()` | Auto-calculates `PageSize` from grid height ÷ row height × 2. |
| `RefreshColOffsets()` | Builds `_coffSets` cumulative column width dictionary. |
| `GetColumnIndexes(scrollLeft)` | Returns `(StartColumnIndex, EndColumnIndex)` for the current horizontal position. |
| `CurrentIndexes(start, end)` | Applies `OverscanCount` clamp for current render window. |
| `VirtualIndexes(start, end)` | Full overscan-adjusted window for cache keying. |
| `ScrollToEditedRowAsync()` | Scrolls viewport to the row under edit before CRUD save. |
| `IsBottomAddForm()` | Returns `true` when `RowEndIndex == TotalItemCount` (Add-Bottom guard). |

---

## Dependencies

### Internal Modules
- `SfGrid<TValue>` — host grid; provides `DataSource`, `TotalItemCount`, column definitions, `GridPageSettings`.
- `GridColumn` / `GridColumns` — column metadata and width information.
- `GridPageSettings` — page size configuration.
- `GridGroupSettings` — grouping state; activates `GroupGeneratedData` path.
- `GridEditSettings` — edit mode; interacts with CRUD-in-virtual-mode logic.
- `GridVirtualContent.razor` — virtual row container component.
- `GridVirtualHeader.razor` — virtual header component.

### External Libraries / Services
- **`sf-grid.js`** — Syncfusion JS module; owns scroll listeners, DOM transform application, row-height measurement.
- **Blazor JS Interop** (`IJSRuntime`) — communication bridge between `VirtualScroll<T>` and `sf-grid.js`.

### Feature Flags / Configuration Toggles
- `EnableVirtualization` — master toggle; when `false`, `GridVirtualContent` is not rendered and `sfBlazor.Grid.virtualDisconnect` is called.
- `EnableColumnVirtualization` — secondary toggle; requires `EnableVirtualization = true`.
- `GridGroupSettings.EnableLazyLoading` — switches the grouping path inside virtualization (lazy group load vs. eager group virtual cache).

---

## Edge Cases

| Scenario | Handling |
|---|---|
| **Fast scroll past cached window** | Cache miss → `DataProcess()` fetch. If `EnableVirtualMaskRow`, skeleton rows are shown immediately via `clientTransformUpdate(maskFlag)`. |
| **DataSource replaced at runtime** | `IsDataSourceChanged = true` → `sfBlazor.Grid.refreshOnDataChange` → full cache wipe + scroll reset to top. |
| **Column with no explicit Width** | Defaults to `200px`; `_coffSets` still computes, but layout may not match design intent. Documented as a requirement. |
| **OverscanCount = 0** | No buffer rows; visible blank rows on very fast scroll are possible. Minimum recommended: 2–5. |
| **Frozen + virtual columns** | Frozen columns always rendered; only movable columns participate in `StartColumnIndex`/`EndColumnIndex` windowing. |
| **Grouping + Virtualization** | Uses `GroupGeneratedData` cache. `EnableLazyLoading` switches code path to `GetUiData()` + `GenerateLazyRowsobject()`. |
| **CRUD Add at top when RowStartIndex > 0** | Add-Top is only allowed when `RowStartIndex == 0`; otherwise ignored / scrolled to top first. |
| **CRUD Add at bottom** | `IsBottomAddForm()` guard; `clientTransformUpdate` with `bottom` flag adjusts transform. |
| **Delete during scroll** | `HasAddOrCancelAction` flag prevents transform conflict; delete finalizes before next scroll is processed. |
| **Row height varies per row** | `RHeight` is a single uniform value measured at initialization. Variable row heights are not supported in virtual mode. |
| **Grid height not set** | `EnsurePageSize()` cannot compute auto `PageSize`; stays at configured/default value. Grid height (`Height` property) should be explicitly set. |
| **Keyboard navigation** | Arrow keys and Page Up/Down are intercepted; scroll position is programmatically adjusted to keep the focused row within the virtual window. |
| **Accessibility** | Virtual rows absent from DOM have no ARIA representation; screen-reader traversal is limited to visible window. |
| **Localization** | No locale-specific behavior; purely layout/data concern. RTL layouts require `TranslateX` sign inversion (handled in JS). |

---

## Non-Functional Requirements

### Performance
- DOM node count must remain O(viewport) — not O(dataset size).
- Scroll handler debounce/throttle managed in JS; `.NET` side processes one scroll event per animation frame.
- `GeneratedData` cache prevents redundant server round-trips for previously visited positions.
- Target: ≥ 60 fps vertical/horizontal scroll on a mid-range desktop with 100 k rows / 200 columns.

### Accessibility
- WCAG 2.1 AA: keyboard navigation (arrow, Page Up/Down, Home/End) must be functional within the virtual window.
- Focus must not be lost when re-render replaces the DOM slice.
- Virtual rows outside the DOM are not reachable by assistive technologies — this is a known architectural constraint; document in user-facing release notes.

### Security / Privacy
- No virtualization-specific security surface. Data handling follows standard `SfGrid<TValue>` data-access rules.
- No PII written to any virtualization cache that persists beyond the page session.

### Telemetry / Logging
- Cache hit/miss ratio should be traceable via internal diagnostic logging (DEBUG builds only).
- `HandleVerticalScrollAsync` / `HandleHorizontalScrollAsync` entry and cache decision logged at `Trace` level.
- No telemetry emitted to external endpoints.
