# Filter Menu

## Summary
The Filter Menu feature provides a column-level popup dialog that allows users to select a filter operator and enter a filter value to narrow grid data. It is activated when `SfGrid.AllowFiltering = true` and `GridFilterSettings.Type = FilterType.Menu`. Each column renders its own `FilterMenuRenderer<TContent>` dialog with type-appropriate input controls. The feature integrates with the grid's `Filter<T>` module to apply, clear, and persist filter predicates.

---

## Motivation & Use Cases
- **Primary goal:** Enable users to filter grid data per column using operator-driven dialogs rather than an always-visible filter bar.
- **Key scenarios:**
  - Filtering string columns with partial-match operators (Contains, StartsWith, Like, etc.)
  - Filtering numeric or date columns with range-style operators (GreaterThan, LessThan, etc.)
  - Filtering boolean columns via a True/False dropdown
  - Using custom value editors via `FilterEditorSettings` (e.g., custom date picker params)
  - Providing a fully custom filter UI via `FilterTemplate`
  - Applying filters on mobile devices via adaptive fullscreen dialogs
  - Opening the filter dialog from the column menu
- **Success criteria:**
  - Dialog opens/closes without layout jitter
  - Correct operator list rendered per column data type
  - Filter applied to data source on "Filter" button click; cleared on "Clear" button click
  - Active filter state pre-populates dialog on re-open
  - All events fire in the documented sequence with correct payloads

---

## Inputs

### Data inputs
- Grid data source (in-memory `IEnumerable<T>` or remote via `SfDataManager`)
- `GridFilterSettings.Columns` — existing `List<GridFilterColumn>` (restored state)
- `GridColumn.Field` / `GridColumn.ForeignKeyValue` (foreign key columns)
- `GridColumn.ColumnType` — drives operator list and value input component selection

### User inputs
- Filter icon click on column header
- Column menu "Filter" item click (`ShowColumnMenu = true`)
- Operator selection in `SfDropDownList` (`{Column.Uid}-floptr`)
- Value entry in the type-specific input control
- "Filter" button click (`e-flmenu-okbtn`)
- "Clear" button click (`e-flmenu-cancelbtn`)
- Adaptive mode: dialog close via header close button

### External triggers
- `EnablePersistence` → restores `GridFilterSettings.Columns` from `localStorage["grid{ID}"]` on grid init
- `FilterDialogOpening` event handler → may cancel dialog open or modify operator list
- `OnActionBegin` handler with `RequestType = FilterBeforeOpen` → may cancel dialog open

---

## Outputs

### UI outputs
- `FilterMenuRenderer` popup dialog positioned near the column header
- Operator `SfDropDownList` pre-selected to current or default operator
- Type-appropriate value input pre-populated from active filter state
- Filter icon state (active/inactive visual indicator on column header)
- Adaptive mode: fullscreen `SfDialog` with header close button

### Events / callbacks
| Event | Type | Notes |
|---|---|---|
| `FilterDialogOpening` | `FilterDialogOpeningEventArgs` | Cancellable; exposes `FilterOperators` (mutable) |
| `FilterDialogOpened` | `FilterDialogOpenedEventArgs` | Fires after dialog is visible |
| `OnActionBegin` | `ActionEventArgs<T>` | `RequestType = FilterBeforeOpen` (cancellable), `Filtering`, `ClearFiltering` |
| `OnActionComplete` | `ActionEventArgs<T>` | `RequestType = FilterAfterOpen`, `Filtering`, `ClearFiltering` |
| `Filtering` | `FilteringEventArgs` | Before filter predicate applied to data |
| `Filtered` | `FilteredEventArgs` | After filtered data rendered |

### Persisted artifacts
- `GridFilterSettings.Columns` — updated `List<GridFilterColumn>` after each filter/clear operation
- `localStorage["grid{ID}"]` — serialized filter state when `EnablePersistence = true`

---

## States

| State | Description |
|---|---|
| **Idle** | No filter dialog open; column headers show filter icons (filled if filter active) |
| **DialogOpening** | Filter icon clicked; position computed; `FilterDialogOpening` / `OnActionBegin(FilterBeforeOpen)` events raised |
| **DialogOpen** | Dialog visible; operator and value inputs ready for interaction |
| **Filtering** | "Filter" button clicked; dialog hidden; `Filter<T>.FilterByColumn()` executing |
| **Filtered** | Data reload complete; header filter icon shows active state |
| **Clearing** | "Clear" button clicked; `Filter<T>.RemoveFilterColumnByField()` executing |
| **Cleared** | Filter predicate removed; data reloaded to unfiltered state |
| **AdaptiveOpen** | Mobile fullscreen dialog open (superset of DialogOpen) |

### State transitions
- Idle → DialogOpening: filter icon or column menu "Filter" clicked
- DialogOpening → Idle: `FilterDialogOpening.Cancel = true` or `OnActionBegin.Cancel = true`
- DialogOpening → DialogOpen: `MenuDialog.ShowAsync()` resolves
- DialogOpen → Filtering: "Filter" button clicked
- DialogOpen → Clearing: "Clear" button clicked
- DialogOpen → Idle: dialog dismissed without action
- Filtering → Filtered: data reload complete
- Clearing → Cleared: predicate removed, data reload complete
- Filtered / Cleared → Idle: ready for next interaction

---

## Configuration

### `GridFilterSettings` properties (filter-menu–relevant)

| Property | Type | Default | Description |
|---|---|---|---|
| `Type` | `FilterType` | `FilterType.FilterBar` | Must be `FilterType.Menu` to enable this feature |
| `Mode` | `FilterBarMode` | `FilterBarMode.OnEnter` | Not applicable to Menu type |
| `EnableCaseSensitivity` | `bool` | `false` | Case-sensitive string matching |
| `IgnoreAccent` | `bool` | `false` | Ignore diacritics in string comparison |
| `ImmediateModeDelay` | `int` | `1500` | Not applicable to Menu type |
| `ShowFilterBarStatus` | `bool` | `true` | Shows active filter summary in pager area |
| `AllowTextWrap` | `bool` | `false` | Allows text wrap in filter bar cells |
| `Columns` | `List<GridFilterColumn>` | `[]` | Pre-set or restored filter predicates |
| `Operators` | `IEnumerable<object>` | `null` | Global override of operator lists per type |

### `GridColumn` filter-menu–relevant properties

| Property | Type | Default | Description |
|---|---|---|---|
| `AllowFiltering` | `bool` | `true` | Enables filter icon on this column |
| `FilterSettings` | `FilterSettings` | — | Per-column overrides (`Type`, `Operator`) |
| `FilterSettings.Type` | `FilterType` | inherits grid | Override filter type for this column only |
| `FilterSettings.Operator` | `string` | type default | Default operator for this column's dialog |
| `FilterEditorSettings` | `IFilterSettings` | `null` | Editor component parameter overrides (see types below) |
| `FilterTemplate` | `RenderFragment<object>` | `null` | Fully custom filter value UI |

### `FilterEditorSettings` implementations

| Column type | Implementation class | Key parameter |
|---|---|---|
| String | `AutoCompleteFilterParams` | `AutoCompleteParams: AutoCompleteModel` |
| Integer / Long / Double / Decimal | `NumericFilterParams` | `NumericTextBoxParams` |
| Date / DateOnly | `DateFilterParams` | `DatePickerParams` |
| DateTime | `DateTimeFilterParams` | `DateTimePickerParams` |
| TimeOnly | `TimeFilterParams` | `TimePickerParams` |
| Boolean | `DropDownFilterParams` | `DropDownListParams` |

### `GridFilterColumn` predicate model

| Property | Type | Description |
|---|---|---|
| `Field` | `string` | Column field name |
| `Operator` | `string` | Filter operator string (e.g., `"contains"`) |
| `Value` | `object` | Filter value |
| `Predicate` | `string` | `"and"` or `"or"` |
| `MatchCase` | `bool` | Case-sensitive match |
| `IgnoreAccent` | `bool` | Ignore diacritics |
| `Uid` | `string` | Column UID — used for dialog ID pattern and state lookup |
| `ActualValue` | `object` | Resolved value after type coercion |
| `ColumnType` | `string` | Serialized column data type |

---

## Behaviors & Rules

### Invariants
- `SfGrid.AllowFiltering = true` AND `GridFilterSettings.Type = FilterType.Menu` are both required for the feature to activate.
- Only one filter menu dialog is visible at a time; opening a second column's dialog closes the first.
- The dialog ID is `{Column.Uid}-flmdlg`; the operator dropdown ID is `{Column.Uid}-floptr`.
- When `GridColumn.AllowFiltering = false`, no filter icon is rendered and no dialog is created.

### Operator list rules
- Default operator lists are determined by `ColumnType`:
  - **String:** `startswith`, `doesnotstartwith`, `endswith`, `doesnotendwith`, `contains`, `doesnotcontain`, `equal`, `notequal`, `isempty`, `isnotempty`, `like`
  - **Number / Date / DateTime / DateOnly / TimeOnly / Decimal / Long:** `equal`, `notequal`, `greaterthan`, `greaterthanorequal`, `lessthan`, `lessthanorequal`, `isnull`, `isnotnull`
  - **Boolean:** `equal`, `notequal`
- `GridFilterSettings.Operators` overrides the default list globally per type.
- `FilterDialogOpeningEventArgs.FilterOperators` overrides the list for the current open operation only.
- `OnActionBegin(FilterBeforeOpen).FilterOperators` also allows per-open override (same as above).

### Value input rules
- When the selected operator is `isnull` or `isnotnull`, the value input is disabled (no value required).
- Foreign key columns: predicate `Field` = `GridColumn.ForeignKeyValue`; `SfAutoComplete` uses `GridColumn.DataManager` instead of the grid's DataManager.
- Complex fields (dot-notation, e.g., `"Address.City"`) are supported; the field string is passed as-is to the query generator.
- `FilterTemplate` fully replaces the operator dropdown and value input; the template receives a `PredicateModel<TValue>` as context.

### Error handling
- If `FilterDialogOpening` or `OnActionBegin(FilterBeforeOpen)` cancels the event, the dialog is not shown and state remains Idle.
- If no value is entered and the operator is not `isnull`/`isnotnull`, the "Filter" button press is a no-op (no predicate is added).
- Remote data source failures during reload propagate through the standard grid error event pipeline.

### ShouldRender optimization
- `FilterMenuRenderer` suppresses re-renders when the dialog is not open, reducing unnecessary Blazor diff cycles.

---

## Workflows

### 1. Opening the filter menu dialog
```
1. User clicks filter icon on column header (or Column Menu "Filter" item)
2. Filter<T>.FilterIconIsClicked = true; FilterIconColumn = column
3. FilterMenuRenderer.Rendered() fires
4. JS call: sfBlazor.Grid.filterPopupRender(gridID, columnUID, "menu", isColumnMenu)
   → returns [X, Y] pixel position
5. OpenFilterDialog() called:
   a. Raise FilterDialogOpening (FilterDialogOpeningEventArgs) → if Cancel → stop
   b. Raise OnActionBegin(RequestType = FilterBeforeOpen) → if Cancel → stop
   c. Look up existing GridFilterColumn by Column.Uid → pre-populate operator + value
   d. MenuDialog.ShowAsync() → dialog becomes visible
   e. Raise OnActionComplete(RequestType = FilterAfterOpen)
   f. Raise FilterDialogOpened (FilterDialogOpenedEventArgs)
```

### 2. Applying a filter
```
1. User selects operator from SfDropDownList
2. User enters value in type-specific input (or leaves disabled for isnull/isnotnull)
3. User clicks "Filter" button (e-flmenu-okbtn)
4. FilterBtnClick():
   a. Hide dialog (MenuDialog.HideAsync())
   b. Call Filter<T>.FilterByColumn(field, operator, value, matchCase, ignoreAccent, uid, actualValue)
5. Filter<T>.FilterByColumn():
   a. Raise OnActionBegin(RequestType = Filtering, FilteringEventArgs) → if Cancel → stop
   b. Add/update GridFilterColumn in GridFilterSettings.Columns
   c. Raise Filtering event
   d. DataGenerator.GenerateQuery() incorporates filter predicates
   e. Data reload triggered
   f. Raise OnActionComplete(RequestType = Filtering)
   g. Raise Filtered event
6. If IsColumnMenuFilter = true → HideColumnMenuPopup()
7. If EnablePersistence = true → serialize to localStorage["grid{ID}"]
```

### 3. Clearing a filter
```
1. User clicks "Clear" button (e-flmenu-cancelbtn)
2. ClearBtnClick():
   a. Hide dialog
   b. Call Filter<T>.RemoveFilterColumnByField(field, uid)
3. RemoveFilterColumnByField():
   a. Raise OnActionBegin(RequestType = ClearFiltering)
   b. Remove matching GridFilterColumn from GridFilterSettings.Columns
   c. Data reload triggered
   d. Raise OnActionComplete(RequestType = ClearFiltering)
4. Column header filter icon reverts to inactive state
5. If EnablePersistence = true → update localStorage
```

### 4. Adaptive (mobile) flow
```
1. Steps 1–4a of "Opening" apply
2. sfBlazor.Grid.customFilterDialog(gridID, columnUID) JS call → positions fullscreen dialog
3. Fullscreen SfDialog rendered with header close button
4. On close button → CloseHandler() → MenuAdaptiveDialog.HideAsync()
5. Apply/Clear buttons behave identically to desktop flow
```

### 5. Restoring persisted filter state on grid init
```
1. EnablePersistence = true → grid reads localStorage["grid{ID}"]
2. Deserializes GridFilterSettings.Columns → List<GridFilterColumn>
3. DataGenerator.GenerateQuery() applies predicates on first data load
4. Filter icons on applicable columns render in active state
```

---

## Architecture

### Component boundaries

```
SfGrid<T>  (SfGrid.Lifecycle.cs)
 ├─ Filter<T>  (Internal/Actions/Filter.cs)           ← business logic module
 ├─ FilterMenuRenderer<TContent>  (Internal/Renderer/) ← per-column dialog UI
 │    ├─ SfDialog  (MenuDialog / MenuAdaptiveDialog)
 │    ├─ SfDropDownList  (operator selector)
 │    └─ Type-specific value input
 │         ├─ SfAutoComplete        (String)
 │         ├─ SfNumericTextBox<T>   (Number types)
 │         ├─ SfDatePicker<T>       (Date / DateOnly)
 │         ├─ SfDateTimePicker<T>   (DateTime)
 │         ├─ SfTimePicker<T>       (TimeOnly)
 │         └─ SfDropDownList<bool?> (Boolean)
 ├─ GridFilterSettings  (GridFilterSettings.razor.cs)  ← settings component
 └─ GridFilterColumn    (GridFilterColumn.cs)          ← predicate model
```

### Internal modules and collaboration
- `Filter<T>` is instantiated in `SfGrid.Lifecycle.cs` as `Parent.FilterModule = new Filter<TValue>(this)`.
- `FilterMenuRenderer` is rendered once per filterable column inside the column header renderer.
- `Filter<T>` owns the predicate collection (`GridFilterSettings.Columns`) and drives data reload via `DataGenerator.GenerateQuery()`.
- JS interop bridge: `sfBlazor.Grid.filterPopupRender` / `sfBlazor.Grid.customFilterDialog` in `sf-grid.js`.

### Client / server responsibilities
- **Client (Blazor WASM / Server):** All dialog rendering, event orchestration, predicate model management.
- **Server / remote data:** Receives the generated `Query` with filter predicates; no awareness of the dialog.

---

## Data Flow

```
Filter Icon Click
      │
      ▼
Filter<T>.FilterIconIsClicked = true
      │
      ▼
FilterMenuRenderer.Rendered()
      │  JS: sfBlazor.Grid.filterPopupRender → [X,Y]
      ▼
OpenFilterDialog()
      │  FilterDialogOpening event (cancellable, operator list editable)
      │  OnActionBegin(FilterBeforeOpen) (cancellable)
      ▼
MenuDialog.ShowAsync()  →  Dialog visible
      │  OnActionComplete(FilterAfterOpen)
      │  FilterDialogOpened event
      ▼
User: select operator + enter value
      ▼
FilterBtnClick()
      │  MenuDialog.HideAsync()
      ▼
Filter<T>.FilterByColumn(field, op, value, ...)
      │  OnActionBegin(Filtering) / Filtering event (cancellable)
      │  GridFilterSettings.Columns updated
      ▼
DataGenerator.GenerateQuery()  →  includes filter Where clause
      │
      ▼
Data source reload  (in-memory: LINQ / remote: HTTP + QueryString)
      │
      ▼
Grid re-renders with filtered rows
      │  OnActionComplete(Filtering) / Filtered event
      ▼
Header filter icon → active state
```

- **Synchronous path:** In-memory data applies LINQ predicates immediately.
- **Async path:** Remote data issues a new HTTP request; loading indicator shown during reload.
- **Persistence side-effect:** After each filter/clear, `localStorage["grid{ID}"]` updated if `EnablePersistence = true`.

---

## Events & Integration Points

### Emitted events

| Event name | Args type | Payload highlights | Cancellable |
|---|---|---|---|
| `FilterDialogOpening` | `FilterDialogOpeningEventArgs` | `ColumnName`, `FilterOperators` (mutable `List<IFilterOperator>`) | ✅ |
| `FilterDialogOpened` | `FilterDialogOpenedEventArgs` | `ColumnName` | ❌ |
| `OnActionBegin` | `ActionEventArgs<T>` | `RequestType`: `FilterBeforeOpen`, `Filtering`, `ClearFiltering`; `FilterOperators` on `FilterBeforeOpen` | ✅ |
| `OnActionComplete` | `ActionEventArgs<T>` | `RequestType`: `FilterAfterOpen`, `Filtering`, `ClearFiltering` | ❌ |
| `Filtering` | `FilteringEventArgs` | `Field`, `Operator`, `Value`, `MatchCase` | ✅ |
| `Filtered` | `FilteredEventArgs` | Applied predicates snapshot | ❌ |

### Consumed events / commands
- Column header renderer raises filter icon click → consumed by `Filter<T>`.
- Column menu renderer raises "Filter" item click → consumed by `Filter<T>` with `IsColumnMenuFilter = true`.
- `FilterMenuRenderer` consumes `Filter<T>.FilterIconIsClicked` flag during `Rendered()` lifecycle.

### External services / APIs
- **JS interop:** `sfBlazor.Grid.filterPopupRender`, `sfBlazor.Grid.customFilterDialog` (in `sf-grid.js`)
- **SyncfusionService:** `IsDeviceMode` — determines whether adaptive UI is activated
- **localStorage:** Read/write for `EnablePersistence`
- **SfDataManager:** Used by `SfAutoComplete` for string column suggestions (supports foreign key DataManager)

---

## API Details

### Public methods on `Filter<T>` (exposed via `SfGrid`)

| Method | Signature | Description |
|---|---|---|
| `FilterByColumn` | `Task FilterByColumn(string field, string filterOperator, object value, string predicate = "and", bool matchCase = false, bool ignoreAccent = false, string uid = null, object actualValue = null)` | Apply a filter predicate for a column |
| `ClearFiltering` | `Task ClearFiltering(List<string> fields = null)` | Clear all filters or filters for specified fields |
| `RemoveFilterColumnByField` | `Task RemoveFilterColumnByField(string field, string uid)` | Remove a single column's filter predicate |
| `GetOperator` | `string GetOperator(string operatorName)` | Convert operator enum name → string key |
| `GetColumnOperator` | `string GetColumnOperator(GridColumn column)` | Resolve effective operator for a column |
| `GetFilterType` | `FilterType GetFilterType(GridColumn column)` | Resolve effective filter type for a column |

### Key `GridFilterSettings` properties (see Configuration section for full table)
- `Type`, `Columns`, `Operators`, `EnableCaseSensitivity`, `IgnoreAccent`

### Key `GridColumn` filter properties
- `AllowFiltering`, `FilterSettings`, `FilterEditorSettings`, `FilterTemplate`

### `FilterDialogOpeningEventArgs` notable members
- `FilterOperators` — `List<IFilterOperator>` — mutate to change operator list for this open
- `Cancel` — set `true` to prevent dialog opening

---

## Dependencies

### Internal modules
- `Filter<T>` (Internal/Actions/Filter.cs)
- `FilterMenuRenderer<TContent>` (Internal/Renderer/)
- `DataGenerator` — query building with filter predicates
- Column header renderer — renders filter icon and triggers click
- Column menu renderer — exposes "Filter" menu item when `ShowColumnMenu = true`
- `SfGrid.Lifecycle.cs` — module registration

### External libraries / services
- Syncfusion Blazor component library: `SfDialog`, `SfDropDownList`, `SfAutoComplete`, `SfNumericTextBox`, `SfDatePicker`, `SfDateTimePicker`, `SfTimePicker`
- `SfDataManager` — remote data adapter
- `SyncfusionService` — device detection (`IsDeviceMode`)
- Browser `localStorage` API (via JS interop)

### Feature flags / configuration toggles
- `SfGrid.AllowFiltering` — master switch
- `GridFilterSettings.Type = FilterType.Menu` — activates this feature
- `GridColumn.AllowFiltering` — per-column opt-out
- `SfGrid.EnableAdaptiveUI` + `AdaptiveUIMode` — activates mobile fullscreen dialog
- `SfGrid.EnablePersistence` — activates localStorage serialization
- `SfGrid.ShowColumnMenu` — activates column menu integration path

---

## Edge Cases

| Scenario | Behavior |
|---|---|
| `isnull` / `isnotnull` operator selected | Value input is disabled; `FilterByColumn` called with `null` value |
| No value entered (non-null operator) | "Filter" button is a no-op; no predicate added |
| Column with `FilterTemplate` | Operator dropdown and value input replaced entirely; template receives `PredicateModel<TValue>` context |
| Foreign key column | Predicate `Field` = `ForeignKeyValue`; `SfAutoComplete` uses column's own `DataManager` |
| Complex / dot-notation field (e.g., `"Address.City"`) | Field string passed as-is to query; no special parsing required |
| `EnablePersistence` restore on init | `GridFilterSettings.Columns` deserialized; data loaded pre-filtered; filter icons render active |
| Re-opening dialog with active filter | Existing `GridFilterColumn` found by `Column.Uid`; operator + value pre-populated |
| `FilterDialogOpening` cancelled | Dialog not shown; state returns to Idle; no events beyond `FilterDialogOpening` fire |
| `OnActionBegin(FilterBeforeOpen)` cancelled | Same as above |
| `Filtering` event cancelled | Predicate not applied; data not reloaded; `GridFilterSettings.Columns` not mutated |
| Large dataset with remote source | Async reload with loading indicator; throttling handled by data adapter |
| Two filter icons clicked rapidly | First dialog closes; second opens; only one dialog visible at a time |
| Adaptive mode close without applying | `CloseHandler` → `HideAsync()`; no predicate change; state returns to Idle |
| Column menu filter on already-filtered column | Dialog pre-populated with existing predicate; user can overwrite or clear |
| `ShouldRender` suppression | `FilterMenuRenderer` skips re-render while dialog is closed to avoid diff overhead |
| Grid with no data (empty state) | Filter icon still renders; dialog opens; predicate applied (returns empty result set) |
| Locale / `IgnoreAccent = true` | Diacritics stripped before string comparison in query predicate |
| `EnableCaseSensitivity = true` | String predicates emit case-sensitive LINQ / OData clauses |

---

## Non-Functional Requirements

### Performance
- `FilterMenuRenderer.ShouldRender` must suppress re-renders when dialog is closed.
- Dialog position computation via JS interop must complete within one rendering frame (~16 ms) to avoid visible jump.
- Remote data filter requests must not duplicate; debounce/cancel in-flight requests when a new filter is applied before the previous reload completes.

### Accessibility
- Dialog must be focusable and keyboard-navigable (Tab through operator dropdown → value input → Filter/Clear buttons).
- Filter icon must have `aria-label` and `role="button"`.
- Active filter state on column header must be communicated via `aria-pressed` or equivalent.
- Adaptive fullscreen dialog must trap focus while open.

### Security & privacy
- Filter values entered by users are used in query predicates only; no server-side SQL injection risk when using parameterized `SfDataManager` adapters.
- Persisted filter state in `localStorage` is unencrypted; sensitive column data should not be filtered with `EnablePersistence` if confidentiality is required.

### Telemetry / logging
- `OnActionBegin` / `OnActionComplete` with `RequestType` values provide hooks for application-level audit logging.
- No built-in telemetry emitted by the component.

### Localization
- Operator display strings, "Filter" / "Clear" button labels, and Boolean dropdown values ("True" / "False") are resource-string driven and must be localizable via the Syncfusion localization pipeline.
- `IgnoreAccent` must be applied consistently across in-memory and remote (OData) data adapters.
