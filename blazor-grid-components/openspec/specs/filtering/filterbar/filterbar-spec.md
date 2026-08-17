# FilterBar

## Summary
The FilterBar feature renders a row of text-input cells directly beneath the column headers, one cell per data column. As the user types into a cell or presses **Enter**, the grid applies a column-level predicate to the active dataset and re-renders only the matching rows. It is the default filter mode (`FilterType.FilterBar`) of `SfGrid<TValue>` and is designed for fast, always-visible single-column filtering without opening any dialog.

---

## Motivation & Use Cases
- **Primary user goal**: Narrow down large, tabular datasets inline without interrupting the reading flow.
- **Key scenarios**:
  - Type-ahead filtering with a configurable debounce delay (`Immediate` mode).
  - Explicit, press-Enter-to-filter workflow (`OnEnter` mode).
  - Keyboard navigation between filter inputs and data rows.
  - Programmatically pre-filtering columns at grid initialisation via `GridFilterSettings.Columns`.
  - Displaying the active filter summary in the pager status bar.
- **Success criteria**:
  - Filter results appear within `ImmediateModeDelay` ms in `Immediate` mode or on Enter in `OnEnter` mode.
  - The pager `ExternalMessage` reflects the active predicates whenever `ShowFilterBarStatus = true`.
  - Clearing a cell (Escape or clear-icon click) removes the column's predicate and re-fetches all rows.

---

## Inputs
- **User inputs**: Characters typed into a `<input type="search">` filter cell; keyboard shortcuts (Enter, Escape, Tab, Shift+Tab); clear-icon click.
- **Data inputs**: `GridFilterSettings.Columns` — pre-populated `List<GridFilterColumn>` at initialisation or programmatic update.
- **External triggers**: Calls to `SfGrid.FilterByColumnAsync()` or `SfGrid.ClearFilteringAsync()` from application code.

---

## Outputs
- **UI outputs**:
  - `FilterBarRenderer.razor` — a `<tr class="e-filterbar">` row beneath the header, one `<td>` per column.
  - `FilterInput.razor` — each `<td>` contains a `<span class="e-input-group">` wrapping a native `<input type="search">` and an `e-clear-icon` span.
  - Clear icon (`e-clear-icon`) becomes visible when the input has a value; hidden (`e-clear-icon-hide`) otherwise.
  - Pager `ExternalMessage` shows the active filter expression (e.g., `"Name: John && Age: 30"`).
- **Events / callbacks**:
  - `GridEvents.OnActionBegin` — `RequestType = Action.Filtering` (cancellable).
  - `GridEvents.OnActionComplete` — `RequestType = Action.Filtering`.
  - `GridEvents.OnActionBegin` / `OnActionComplete` — `RequestType = Action.ClearFiltering`.
- **Persisted artifacts**: `GridFilterSettings.Columns` list is updated in-place and fed back into `DataGenerator<T>.FilterQuery`.

---

## States
| State | Description |
|-------|-------------|
| **Idle** | All filter cells are empty; full dataset displayed. |
| **Filtering** | One or more cells contain values; `DataGenerator` has applied `Where()` predicates. |
| **Debouncing** | `Immediate` mode only: a `System.Timers.Timer` is running; filter has not fired yet. |
| **Cleared** | Escape key or clear-icon clicked; cell is empty; predicate removed from `FilterSettings.Columns`. |
| **Disabled per column** | `GridColumn.AllowFiltering = false`; input rendered with `disabled` attribute. |

**State transitions (brief rules)**:
- `Idle → Filtering`: user types + timer fires OR user presses Enter.
- `Filtering → Filtering`: subsequent keystrokes restart the timer.
- `Filtering → Cleared`: Escape key, clear-icon click, or `ClearFilteringAsync()` call.
- `Cleared → Idle`: after `RemoveFilterColumnByField` completes and grid re-renders.

---

## Configuration

### `GridFilterSettings` properties relevant to FilterBar

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Type` | `FilterType` | `FilterType.FilterBar` | Must be `FilterBar` to activate this feature. |
| `Mode` | `FilterBarMode` | `FilterBarMode.OnEnter` | `OnEnter` — filter on Enter key. `Immediate` — filter after `ImmediateModeDelay` ms. |
| `ImmediateModeDelay` | `int` | `1500` | Milliseconds to wait before triggering a filter in `Immediate` mode. |
| `EnableCaseSensitivity` | `bool` | `false` | When `true`, string matching is case-sensitive. |
| `IgnoreAccent` | `bool` | `false` | When `true`, diacritic characters are ignored during string comparison. |
| `ShowFilterBarStatus` | `bool` | `true` | Shows the active filter expression in the pager `ExternalMessage`. |
| `Columns` | `List<GridFilterColumn>` | `null` | Pre-populated predicates applied at grid initialisation. |
| `Operators` | `object` | `null` | Override default operators per column type in the filter UI. |
| `AllowTextWrap` | `bool` | `false` | Not used by FilterBar (applies to CheckBox/Excel types only). |

### Per-column overrides (`GridColumn.FilterSettings`)
| Property | Effect |
|----------|--------|
| `Operator` | Default operator for this column's filter bar input. |
| `Type` | Can override the filter type per column (e.g., `Menu` on one column while the grid uses `FilterBar`). |

### Operator inference from typed prefix (string/numeric columns)
| Prefix in input | Resolved operator |
|-----------------|-------------------|
| `*value` | `startswith` |
| `%value` | `endswith` |
| `>value` | `greaterthan` |
| `>=value` | `greaterthanorequal` |
| `<value` | `lessthan` |
| `<=value` | `lessthanorequal` |
| `=value` | `equal` |
| `!=value` | `notequal` |
| (no prefix, string column) | `startswith` |
| (no prefix, non-string column) | `equal` |

---

## Behaviors & Rules
- **One predicate per column**: applying a new value for an already-filtered column replaces (not appends) the existing predicate for that column.
- **Collection values**: if `FilterValue` is `IEnumerable`, multiple `GridFilterColumn` entries are created with `Predicate = "or"` (unless the operator is `NotEqual`, in which case `"and"`).
- **IsEmpty / IsNotEmpty operators**: `FilterValue` is coerced to `""` regardless of user input.
- **Foreign key columns**: the `field` used in the predicate is `GridColumn.ForeignKeyValue`, not `GridColumn.Field`.
- **Frozen columns**: `FilterBarRenderer` splits cells into frozen-left, movable, and frozen-right sections via `FilterBarParameters.IsFrozen` / `IsFrozenRight`. Each section renders its own `<tr>`.
- **Column virtualization**: only cells whose `column.Index` falls within `[StartColumnIndex-1, EndColumnIndex+1]` are rendered in the movable section.
- **Grouped columns**: indent cells (`e-indent`, `e-detailindent`, `e-rowdrag`) are prepended before data cells to keep alignment with the header.
- **Template override**: if `GridColumn.FilterTemplate` is set, it replaces the default `<input>` entirely; `FilterInput` renders the template with a `PredicateModel` context object.
- **Selection clearing**: when a filter is applied and `SelectionSettings.PersistSelection = false`, current selection is cleared before executing the filter query.
- **`OnActionBegin` cancellation**: if `args.Cancel = true` is set in the handler, `ModelChanged` is not called and the grid data is not reloaded.

---

## Workflows

### Apply filter (OnEnter mode)
1. User types in a filter input cell.
2. `UpdateValue` stores the typed string in `FilteredValue`; shows the clear icon.
3. User presses **Enter**.
4. `KeyDownHandler` calls `StopTimer()` then `StartTimer(args, column)` with `Interval = 1 ms`.
5. Timer fires → `ProcessFilter(column)` is called via `InvokeAsync`.
6. `GetActualFilterValue` parses any operator prefix and casts the value to the column's `ValueType`.
7. `Filter<T>.FilterByColumn(field, operator, value, ...)` is called.
8. Existing predicates for this column's `Uid` are removed from `FilterSettings.Columns`; new ones are added.
9. `FilterSettings.UpdateProperties("Columns", ...)` propagates the change.
10. `UpdateFilterMessage` updates the pager `ExternalMessage`.
11. `ModelChanged(RequestType = Filtering)` raises `OnActionBegin`; if not cancelled, `DataGenerator.GenerateQuery()` runs.
12. `SfDataManager.ExecuteQuery()` returns filtered results.
13. `SfGrid.CurrentViewData` updated → `StateHasChanged()` → grid re-renders.
14. `OnActionComplete` raised.

### Apply filter (Immediate mode)
Steps 1–2 same as above.
3. `UpdateValue` detects `Mode == Immediate`, calls `StartTimer` with `Interval = ImmediateModeDelay`.
4. If the user types again before the timer fires, `StopTimer()` + `StartTimer()` restarts the countdown.
5. Timer fires → same as step 5 onwards above.

### Clear filter (clear-icon click)
1. User clicks `e-clear-icon` span.
2. `CancelIconClick` sets `FilteredValue = ""`.
3. `Filter<T>.RemoveFilterColumnByField(field, uid)` removes all `GridFilterColumn` entries matching the column's `Uid`.
4. `FilterSettings.UpdateProperties("Columns", ...)` propagated.
5. `ModelChanged(RequestType = ClearFiltering)` → data reload → grid re-renders.
6. `sfBlazor.Grid.searchClear` JS call clears the native input DOM value.

### Clear filter (Escape key)
1. `KeyDownHandler` detects `args.Key == "Escape"`.
2. `FilterClearIcon` set to `""`, `FilteredValue = ""`, `FilteredColumn = null`, `Parent.FilteredColumns` cleared.
3. No immediate server call; the filter is cleared in-memory. A subsequent blur or Enter would trigger `ProcessFilter` with empty value, which calls `RemoveFilterColumnByField`.

### Keyboard navigation between filter cells
1. Tab / Shift+Tab detected in `KeyDownHandler`.
2. `Parent.InvokeMethod("sfBlazor.Grid.focusFilterBar", [DataId, keyCombination])` delegates focus movement to JS.

---

## Architecture

### Component boundaries
```
SfGrid<TValue>  (root)
└── GridHeader.razor
    └── FilterBarRenderer.razor   [Renderer — Presentation Layer]
        └── FilterInput.razor     [Renderer — Presentation Layer, one per column]
```

### Internal modules
| Module | Role |
|--------|------|
| `Filter<T>` (`Internal/Actions/Filter.cs`) | Business logic: builds and mutates `FilterSettings.Columns`, calls `ModelChanged`, manages pager message. |
| `FilterBarRenderer.razor` | Renders the `<tr class="e-filterbar">` row; computes which columns to show per frozen section. |
| `FilterInput.razor` | Renders one `<td>` per column; handles user input, debounce timer, operator parsing, type conversion, and clear-icon interaction. |
| `DataGenerator<T>` | Reads `FilterSettings.Columns` and appends `Where()` predicates to the query. |
| `GridFilterSettings` | Parameter model; propagates changes via `UpdateProperty` into `Parent.FilterSettings`. |

### Client/server responsibilities
- All filtering logic (predicate building, type parsing, query generation) runs in **.NET** (server or WASM).
- JavaScript (`sfBlazor.Grid.focusFilterBar`, `sfBlazor.Grid.searchClear`, `sfBlazor.Grid.updateFilterBarCell`) handles only DOM focus management and native input value reset — no data logic.

---

## Data Flow

```
User Input (keydown / oninput)
        │
        ▼
FilterInput.razor — UpdateValue() / KeyDownHandler()
        │  starts/restarts System.Timers.Timer
        ▼
ProcessFilter(column)   [on timer elapsed, via InvokeAsync]
        │
        ▼
GetActualFilterValue()  — parse prefix operators, cast to ValueType
        │
        ▼
Filter<T>.FilterByColumn(field, operator, value, uid, ...)
        │  mutates FilterSettings.Columns
        │  calls UpdateFilterMessage → Pager.ExternalMessage
        ▼
ModelChanged(RequestType = Filtering)
        │  raises OnActionBegin (cancellable)
        ▼
DataGenerator<T>.GenerateQuery()
        │  FilterQuery: Where(field, operator, value) per GridFilterColumn
        ▼
SfDataManager.ExecuteQuery()
        │
        ▼
SfGrid.CurrentViewData updated
        │
        ▼
StateHasChanged() → Blazor re-render
        │
        ▼
FilterBarRenderer + FilterInput re-render (existing values preserved)
        │
        ▼
OnActionComplete raised
```

- **Sync/async**: `FilterByColumn` and `RemoveFilterColumnByField` are `async Task`; timer callback uses `.GetAwaiter()` (fire-and-forget) and re-enters the Blazor context via `InvokeAsync`.
- **Batching**: multiple simultaneous column filters are not batched — each column update is an independent call. Multi-column re-sync after filter uses `sfBlazor.Grid.updateFilterBarCell` to patch other cell DOM values.
- **Pagination**: after a filter operation, `UpdatePageSizes` checks whether the current page size exceeds `TotalItemCount` and trims it.

---

## Events & Integration Points

### Emitted events
| Event | `RequestType` | Payload | Cancellable |
|-------|--------------|---------|-------------|
| `OnActionBegin` | `Action.Filtering` | `ActionEventArgs<T>` with `CurrentFilterObject` (`PredicateModel<object>`), `CurrentFilteringColumn` | Yes (`args.Cancel = true`) |
| `OnActionComplete` | `Action.Filtering` | `ActionEventArgs<T>`, `FilteringEventArgs` with `FilterPredicates`, `ColumnName` | No |
| `OnActionBegin` | `Action.ClearFiltering` | `ActionEventArgs<T>` with `CurrentFilterObject` | Yes |
| `OnActionComplete` | `Action.ClearFiltering` | `ActionEventArgs<T>`, `FilteringEventArgs` with `ColumnName` | No |

### Consumed JS functions
| JS Function | Caller | Purpose |
|-------------|--------|---------|
| `sfBlazor.Grid.focusFilterBar` | `FilterInput.KeyDownHandler` | Move DOM focus to next/previous filter cell on Tab/Shift+Tab |
| `sfBlazor.Grid.searchClear` | `FilterInput.CancelIconClick` | Reset native `<input>` DOM value after clear-icon click |
| `sfBlazor.Grid.updateFilterBarCell` | `FilterInput.ProcessFilter` | Sync other filter bar cells' displayed values after multi-column filter re-apply |

---

## API Details

### `GridFilterSettings` public properties
| Property | Type | Default |
|----------|------|---------|
| `Type` | `FilterType` | `FilterType.FilterBar` |
| `Mode` | `FilterBarMode` | `FilterBarMode.OnEnter` |
| `ImmediateModeDelay` | `int` | `1500` |
| `EnableCaseSensitivity` | `bool` | `false` |
| `IgnoreAccent` | `bool` | `false` |
| `ShowFilterBarStatus` | `bool` | `true` |
| `Columns` | `List<GridFilterColumn>` | `null` |
| `Operators` | `object` | `null` |
| `AllowTextWrap` | `bool` | `false` |

### `GridFilterSettings` internal methods
| Method | Signature | Purpose |
|--------|-----------|---------|
| `UpdateChildProperties` | `(string key, List<GridFilterColumn> value)` | Called by `GridFilterColumns` child component to push column list changes. |
| `UpdateProperties` | `async Task (string key, List<GridFilterColumn> value)` | Propagates `Columns` change via `UpdateProperty` diff tracking. |
| `Initialize` | `static async Task<GridFilterSettings> (SfDataBoundComponent)` | Auto-constructs instance when `<GridFilterSettings>` is absent from markup. |

### `Filter<T>` internal methods
| Method | Signature | Purpose |
|--------|-----------|---------|
| `FilterByColumn` | `async Task (string field, Operator op, object value, string? predicate, bool? matchCase, bool? ignoreAccent, object? actualValue, object? actualOp, string? uid, string? inputField)` | Core filter application. |
| `RemoveFilterColumnByField` | `async Task (string field, string uid, string foreignKeyField)` | Removes predicates for a column and triggers `ClearFiltering` action. |
| `ClearFiltering` | `async Task (object fields)` | Clears all or specified column filters. |
| `UpdateFilterMessage` | `async void (List<GridFilterColumn>, string predicate)` | Updates pager `ExternalMessage` string. |
| `GetOperator` | `static Operator (string value)` | Maps operator string to `Operator` enum. |
| `GetColumnType` | `static string? (ColumnType?)` | Returns column type string for `GridFilterColumn.ColumnType`. |
| `UpdateFilterModel` | `static void (object model, GridFilterColumn col, ...)` | Applies filter column state onto a typed model instance via reflection. |

### `FilterBarMode` enum
| Value | Behaviour |
|-------|-----------|
| `OnEnter` | Filter triggered only when Enter key is pressed. |
| `Immediate` | Filter triggered automatically after `ImmediateModeDelay` ms of inactivity. |

### `FilterType` enum (relevant value)
| Value | Description |
|-------|-------------|
| `FilterBar` | Default. Inline row of filter inputs below the column header. |
| `Menu` | Column header icon opens a filter dialog (separate feature). |
| `CheckBox` | Dropdown checkbox list (separate feature). |
| `Excel` | Excel-style dialog (separate feature). |

---

## Dependencies

### Internal modules
- `DataGenerator<T>` — consumes `FilterSettings.Columns` for query building.
- `SfGrid<T>.FilterModule` — `Filter<T>` instance held as `Parent.FilterModule`.
- `SelectionModule` — selection is cleared on filter when `PersistSelection = false`.
- `GridPageSettings` / `PagerRef` — pager `ExternalMessage` is updated by `UpdateFilterMessage`.
- `GridUtils.GetColumns` — flattens stacked/nested column tree for filter cell generation.
- `VirtualScrollModule` — provides `StartColumnIndex`/`EndColumnIndex` for column virtualization windowing.

### External libraries / services
- `Syncfusion.Blazor.Data.SfDataManager` — executes the query after filter predicates are applied.
- `Syncfusion.Blazor.Inputs.SfTextBox` — imported but not used directly in `FilterInput`; the native `<input>` element is used instead.
- `System.Timers.Timer` — debounce timer in `FilterInput` for `Immediate` mode.
- `Microsoft.AspNetCore.Components.Web.KeyboardEventArgs` — key event detection.

### Feature flags / toggles
- `SfGrid.AllowFiltering` — master switch; must be `true` for the filter bar row to render.
- `GridColumn.AllowFiltering` — per-column disable; renders input with `disabled` attribute when `false`.
- `SfGrid.EnableColumnVirtualization` — activates column windowing in `FilterBarRenderer`.

---

## Edge Cases

- **Empty `FilteredValue`**: `ProcessFilter` calls `RemoveFilterColumnByField` instead of `FilterByColumn`, avoiding a spurious empty-string predicate.
- **`JsonElement` values from persistence**: `FilterBarRenderer` detects `JsonElement` typed `Col.Value` and calls `SfBaseUtils.ChangeType` before displaying; same pattern repeated in `UpdateFilterMessage`.
- **`DateOnly` format display**: `FilterInput` re-parses `DateOnly` strings through `ConvertToDesiredDateFormat` to match the column's configured `Format` for the displayed value, while sending ISO `MM/dd/yyyy` internally to the data layer.
- **Enum column values**: `FilterInput` resolves display names via `MetadataExtension.GetDisplayName` before populating the cell.
- **`DateTimeOffset` columns**: special-cased in both `FilterInput` and `Filter<T>` to use `DateTimeOffset.TryParse` rather than generic `SfBaseUtils.ChangeType`.
- **Stacked headers**: `FilterBarRenderer` uses `GridUtils.GetColumns(Parent)` to flatten nested column trees; `UpdateFilterBarColumns` applies the correct frozen partition.
- **Multi-column concurrent filter (Immediate mode)**: after each filter, `sfBlazor.Grid.updateFilterBarCell` is called to re-sync all other filter cells' displayed values, preventing stale DOM state.
- **Timer leak**: `StopTimer()` is called at the start of every `StartTimer()` and on every new keydown, ensuring only one active timer exists per cell at any time.
- **Race condition (Immediate mode + navigation)**: `InvokeAsync` marshals `ProcessFilter` back onto the Blazor synchronization context before mutating `FilterSettings.Columns`, preventing cross-thread state corruption.
- **Large number of filtered columns**: `UpdateFilterMessage` iterates all `FilterSettings.Columns` on every change; no lazy/incremental message building — potential performance concern for grids with many simultaneously filtered columns.
- **`ShowGroupedColumn = false`**: hidden grouped columns still receive indent cells; they are inserted by `GetActualColumns` to maintain column alignment.
- **Column virtualization + frozen panes**: only the movable filter bar section participates in virtual windowing; frozen-left and frozen-right sections always render all their cells.

---

## Non-Functional Requirements

- **Performance**: timer-based debounce (`ImmediateModeDelay`, default 1500 ms) prevents over-querying remote data sources. Column-virtual rendering limits DOM nodes proportionally to the visible column window.
- **Accessibility**:
  - Each filter input has `role="searchbox"` and `aria-label="<field>_FilterBar"`.
  - `aria-disabled="false"` is set on enabled inputs; `disabled` attribute on `AllowFiltering = false` columns.
  - `title` attribute on each input is set to `"{HeaderText} filter bar"` (localized via `GridLocaleKeys.FilterbarTitle`).
  - Tab / Shift+Tab navigation is delegated to `sfBlazor.Grid.focusFilterBar` for consistent cross-browser focus management.
- **Localization**: `FilterbarTitle`, `FilterBar`, and `ClearButton` strings are resolved via `ISyncfusionStringLocalizer` (injected in `FilterBarRenderer.OnInitialized`).
- **Security**: filter values are passed as typed .NET objects through `SfDataManager` adaptors — no raw SQL or query string construction on the client.
- **Telemetry / logging**: no explicit telemetry hooks; standard Blazor component lifecycle exceptions propagate to the host application's error boundary.
