# Tasks: Enhanced FilterBar Implementation

**Date**: May 15, 2026  
**Feature**: Enhanced FilterBar with Operator Dropdowns & Type-Aware Input Controls  
**Total Tasks**: 16 across 4 phases  
**Estimated Effort**: 350-400 LOC | 8-9 weeks  
**Status**: ✅ TASK BREAKDOWN COMPLETE

---

## Phase 1: Foundation (Model & Configuration)

### Duration: 3-4 days | Effort: 6 tasks | LOC: 74

**Objective**: Add model classes, parameters, and event infrastructure required by all subsequent phases.

---

### Task 1.1: Add `ShowFilterOperator` Parameter to GridFilterSettings

**File**: `src/GridFilterSettings.razor.cs`  
**Type**: Add parameter + XML documentation  
**Estimated LOC**: 12

**Requirements**:
- Add `bool ShowFilterOperator { get; set; }` property
- Default value: `false` (opt-in; backward compatible)
- Add 4-part XML documentation (Summary, Value, Remarks, Example)
- Document interaction with `Type` property (must be `FilterBar`, not `FilterMenu`)

**Acceptance Criteria**:
- [ ] Property added to GridFilterSettings class
- [ ] Default = false (backward compatible)
- [ ] XML documentation 4-part format
- [ ] No compiler warnings
- [ ] Property accessible from Razor templates (`@Grid.FilterSettings.ShowFilterOperator`)

**Example**:
```csharp
/// <summary>
/// Enables operator dropdown selector and type-specific input controls
/// in the FilterBar header row when <c>true</c>.
/// </summary>
/// <value>
/// <c>true</c> to render FilterInput component in enhanced mode with operator dropdowns;
/// <c>false</c> to use FilterInput in legacy mode (text-only).
/// Default is <c>false</c>.
/// </value>
/// <remarks>
/// <para>
/// When set to <c>true</c>, FilterBar rendering changes from simple text input
/// to inline operator selector dropdown + type-aware input control (SfDatePicker,
/// SfNumericTextBox, SfDropDownList, etc.) based on column data type.
/// </para>
/// <para>
/// Requires <see cref="GridFilterSettings.Type"/> to be
/// <see cref="FilterBarType.FilterBar"/>. Has no effect if <see cref="GridFilterSettings.Type"/>
/// is <see cref="FilterBarType.FilterMenu"/>.
/// </para>
/// </remarks>
[Parameter]
public bool ShowFilterOperator { get; set; } = false;
```

---

### Task 1.2: Add `OperatorDropdownWidth` Parameter to GridFilterSettings

**File**: `src/GridFilterSettings.razor.cs`  
**Type**: Add parameter + XML documentation  
**Estimated LOC**: 10

**Requirements**:
- Add `string OperatorDropdownWidth { get; set; }` property
- Default: `"auto"` (CSS sizing)
- Add 4-part XML documentation
- Document CSS value examples (e.g., "80px", "10%", "auto")

**Acceptance Criteria**:
- [ ] Property added to GridFilterSettings
- [ ] Default = "auto"
- [ ] XML documentation with CSS examples
- [ ] No compiler warnings
- [ ] Property used in FilterInput component rendering (enhanced mode)

**Example**:
```csharp
/// <summary>
/// Gets or sets the CSS width for the operator dropdown selector in ShowFilterOperator.
/// </summary>
/// <value>
/// A CSS width value (e.g., "80px", "10%", "auto").
/// Default is <c>"auto"</c>.
/// </value>
[Parameter]
public string OperatorDropdownWidth { get; set; } = "auto";
```

---

### Task 1.3: Add New Properties to GridColumn

**File**: `src/GridColumn.cs`  
**Type**: Add 3 properties + XML documentation  
**Estimated LOC**: 20

**Requirements**:
- Add `CustomOperators` (List<string>): Per-column operator override
- Add `ShowOperatorDropdown` (bool): Show/hide operator dropdown for column
- Add `FilterInputPlaceholder` (string): Placeholder text for filter input
- Add 4-part XML documentation for each
- Document business use case (e.g., "restrict operators for sensitive columns")

**Acceptance Criteria**:
- [ ] All 3 properties added
- [ ] Defaults: CustomOperators=null, ShowOperatorDropdown=true, FilterInputPlaceholder=null
- [ ] XML documentation for each (4-part format)
- [ ] No compiler warnings
- [ ] Properties accessible from Razor templates

**Example**:
```csharp
/// <summary>
/// Gets or sets a list of operator names to restrict available operators
/// for this column in ShowFilterOperator.
/// </summary>
/// <value>
/// A list of operator names (e.g., ["Contains", "Equal", "StartsWith"]),
/// or <c>null</c> to use default operators for column type.
/// </value>
/// <remarks>
/// <para>
/// When set, only operators in this list appear in the operator dropdown
/// for this column. Other column type operators are excluded.
/// </para>
/// <para>
/// Example: To restrict a String column to "Contains" and "Equal" only,
/// set <c>CustomOperators = new() { "Contains", "Equal" }</c>.
/// </para>
/// </remarks>
[Parameter]
public List<string> CustomOperators { get; set; }

/// <summary>Gets or sets whether to show the operator dropdown for this column.</summary>
/// <value><c>true</c> (default) to show operator dropdown; <c>false</c> to hide.</value>
[Parameter]
public bool ShowOperatorDropdown { get; set; } = true;

/// <summary>Gets or sets placeholder text for the filter value input.</summary>
/// <value>Placeholder text (e.g., "Enter search term"), or <c>null</c> to use default.</value>
[Parameter]
public string FilterInputPlaceholder { get; set; }
```

---

### Task 1.4: Add New Fields to GridFilterColumn

**File**: `src/GridFilterColumn.cs`  
**Type**: Add 2 fields (internal state)  
**Estimated LOC**: 4

**Requirements**:
- Add `SelectedOperator` (string): Tracks currently selected operator
- Add `PreviousOperator` (string): Tracks operator before last change
- Both are internal tracking fields; don't expose publicly
- Used for change detection + event firing

**Acceptance Criteria**:
- [ ] Fields added as private/internal
- [ ] Proper initialization in constructor or property getter
- [ ] No compiler warnings

**Example**:
```csharp
public class GridFilterColumn
{
    // Existing fields...
    
    /// <summary>Tracks the currently selected operator in ShowFilterOperator.</summary>
    public string SelectedOperator { get; set; }
    
    /// <summary>Tracks the previous operator before last change (for change detection).</summary>
    public string PreviousOperator { get; set; }
}
```

---

### Task 1.5: Add Event Args Classes

**File**: `src/EventModels/Grids.cs`  
**Type**: Add 2 new classes + XML documentation  
**Estimated LOC**: 22

**Requirements**:
- Add `BeforeOperatorChangeEventArgs` class (cancellable)
- Add `OperatorChangedEventArgs` class (post-change)
- 4-part XML documentation for class + each property
- Follow existing grid event arg patterns (Syncfusion conventions)

**Acceptance Criteria**:
- [ ] Both classes added
- [ ] All properties documented (4-part format)
- [ ] BeforeOperatorChangeEventArgs has `Cancel` property
- [ ] OperatorChangedEventArgs has `ChangedAt` timestamp
- [ ] No compiler warnings

**Example**:
```csharp
/// <summary>
/// Provides data for the BeforeOperatorChange event, allowing validation
/// or business logic gates before an operator change is applied.
/// </summary>
public class BeforeOperatorChangeEventArgs
{
    /// <summary>Gets the column being filtered.</summary>
    public GridColumn Column { get; set; }

    /// <summary>Gets the current (previous) selected operator.</summary>
    public string CurrentOperator { get; set; }

    /// <summary>Gets the operator the user selected from the dropdown.</summary>
    public string NewOperator { get; set; }

    /// <summary>Gets or sets a value indicating whether to cancel this operator change.</summary>
    /// <remarks>Set to <c>true</c> to prevent the operator change.</remarks>
    public bool Cancel { get; set; }
}

/// <summary>
/// Provides data for the OnOperatorChanged event, fired after a filter operator
/// has been successfully changed in the ShowFilterOperator.
/// </summary>
public class OperatorChangedEventArgs
{
    /// <summary>Gets the column being filtered.</summary>
    public GridColumn Column { get; set; }

    /// <summary>Gets the previous operator.</summary>
    public string PreviousOperator { get; set; }

    /// <summary>Gets the newly selected operator.</summary>
    public string NewOperator { get; set; }

    /// <summary>Gets the current filter value in the input field.</summary>
    public string CurrentValue { get; set; }

    /// <summary>Gets the timestamp when the operator change occurred.</summary>
    public DateTime ChangedAt { get; set; }
}
```

---

### Task 1.6: Add Event Callbacks to GridEvents

**File**: `src/GridEvents.cs`  
**Type**: Add 2 event callbacks + XML documentation  
**Estimated LOC**: 8

**Requirements**:
- Add `EventCallback<BeforeOperatorChangeEventArgs> OnBeforeOperatorChange`
- Add `EventCallback<OperatorChangedEventArgs> OnOperatorChanged`
- 4-part XML documentation for each
- Follow existing grid event patterns

**Acceptance Criteria**:
- [ ] Both callbacks added as [Parameter]
- [ ] XML documentation (4-part format)
- [ ] No compiler warnings
- [ ] Callbacks accessible from Razor templates

**Example**:
```csharp
/// <summary>
/// Gets or sets an event callback raised before a filter operator changes
/// in the ShowFilterOperator. Can be cancelled.
/// </summary>
[Parameter]
public EventCallback<BeforeOperatorChangeEventArgs> OnBeforeOperatorChange { get; set; }

/// <summary>
/// Gets or sets an event callback raised after a filter operator has been
/// successfully changed in the ShowFilterOperator.
/// </summary>
[Parameter]
public EventCallback<OperatorChangedEventArgs> OnOperatorChanged { get; set; }
```

---

## Phase 2: Core Logic (Filtering Engine Extensions)

### Duration: 10-14 days | Effort: 5 tasks | LOC: 112

**Objective**: Implement operator discovery, validation, and change logic. Reuse existing Filter.cs logic.

---

### Task 2.1: Implement `GetAvailableOperators()` Method

**File**: `src/SfGrid.Properties.cs`  
**Type**: Add public method + XML documentation  
**Estimated LOC**: 25

**Requirements**:
- Signature: `public List<string> GetAvailableOperators(string columnField)`
- Returns operator names based on column type
- Respects `GridColumn.CustomOperators` override if set
- Handles null column gracefully

**Acceptance Criteria**:
- [ ] Method added to SfGrid<TValue>
- [ ] Returns correct operators per type (String, Number, Date, Boolean, Enum, FK)
- [ ] CustomOperators override respected
- [ ] Null column returns empty list (safe)
- [ ] XML documentation (4-part format)
- [ ] Unit tests pass (6+ test cases)
- [ ] No compiler warnings

**Example Use Case**:
```csharp
var opsForPrice = Grid.GetAvailableOperators("Price");
// Returns: ["=", "≠", ">", ">=", "<", "<=", "Between", "IsNull", "IsNotNull"]

// With CustomOperators override:
// If column has CustomOperators = ["Equal", "GreaterThan"]
var restricted = Grid.GetAvailableOperators("Price");
// Returns: ["Equal", "GreaterThan"] (only overridden operators)
```

---

### Task 2.2: Implement `GetCurrentFilterOperator()` Method

**File**: `src/SfGrid.Properties.cs`  
**Type**: Add public method + XML documentation  
**Estimated LOC**: 20

**Requirements**:
- Signature: `public string GetCurrentFilterOperator(string columnField)`
- Returns currently selected operator for column (from GridFilterColumn.SelectedOperator)
- Falls back to column type default if not explicitly set
- Handles null column gracefully

**Acceptance Criteria**:
- [ ] Method added to SfGrid<TValue>
- [ ] Returns current operator or type default
- [ ] Null column returns null/empty string safely
- [ ] XML documentation (4-part format)
- [ ] Unit tests pass (5+ test cases)
- [ ] No compiler warnings

**Example Use Case**:
```csharp
var currentOp = Grid.GetCurrentFilterOperator("Price");
// Returns: "GreaterThan" (if that's the current selection)

// Or default if not set:
var defaultOp = Grid.GetCurrentFilterOperator("UnfilteredColumn");
// Returns: "Contains" (for String), "Equal" (for Number), etc.
```

---

### Task 2.3: Implement `ChangeFilterOperatorAsync()` Method

**File**: `src/SfGrid.Properties.cs`  
**Type**: Add public async method + XML documentation  
**Estimated LOC**: 28

**Requirements**:
- Signature: `public async Task ChangeFilterOperatorAsync(string columnField, string newOperator)`
- Fires BeforeOperatorChange event (with cancellation support)
- Updates GridFilterColumn.SelectedOperator
- Fires OnOperatorChanged event after successful change
- Validates operator against available operators (log warning if invalid)
- Triggers filter reapplication if necessary

**Acceptance Criteria**:
- [ ] Method added to SfGrid<TValue> as async
- [ ] BeforeOperatorChange fires + cancellation respected
- [ ] GridFilterColumn.SelectedOperator updated
- [ ] OnOperatorChanged fires after change
- [ ] Invalid operator logged as warning (doesn't crash)
- [ ] XML documentation (4-part format)
- [ ] Unit tests pass (8+ test cases)
- [ ] No compiler warnings
- [ ] ConfigureAwait(true) used on all awaits

**Example Use Case**:
```csharp
// Programmatically change operator
await Grid.ChangeFilterOperatorAsync("Price", "GreaterThan");
// → BeforeOperatorChange fires
// → Operator changes (if not cancelled)
// → OnOperatorChanged fires
// → Filter reapplies
```

---

### Task 2.4: Implement `ValidateOperatorAndValue()` Helper

**File**: `src/Internal/Actions/Filter.cs`  
**Type**: Add static helper method + XML documentation  
**Estimated LOC**: 20

**Requirements**:
- Validates operator + value combination is legal
- Checks: Does operator require a value? Is value type-valid?
- Handles special cases (IsEmpty, IsNull, Between, etc.)
- Returns validation result + error message

**Acceptance Criteria**:
- [ ] Helper added to Filter class (static method)
- [ ] Validates all operator types
- [ ] IsEmpty/IsNull operators don't require value
- [ ] Between requires start AND end values
- [ ] Error messages actionable for users
- [ ] XML documentation (4-part format)
- [ ] Unit tests pass (12+ test cases)
- [ ] No compiler warnings

**Example**:
```csharp
public static (bool IsValid, string ErrorMessage) ValidateOperatorAndValue(
    string @operator, 
    object value, 
    ColumnType columnType)
{
    if (new[] { "IsEmpty", "IsNull", "IsNotEmpty", "IsNotNull" }.Contains(@operator))
        return (true, null);  // These operators don't need values
    
    if (value == null || string.IsNullOrEmpty(value.ToString()))
        return (false, $"Value required for '{@operator}' operator");
    
    // Type-specific validation...
    return (true, null);
}
```

---

### Task 2.5: Update FilterByColumn Documentation

**File**: `src/Internal/Actions/Filter.cs`  
**Type**: Update XML documentation + remarks  
**Estimated LOC**: 19

**Requirements**:
- Document operator parameter (now explicitly user-selectable via ShowFilterOperator)
- Update remarks to mention operator dropdown UI
- Add examples showing operator usage
- Cross-reference new public API methods

**Acceptance Criteria**:
- [ ] FilterByColumn method documentation enhanced
- [ ] Operator parameter clearly documented
- [ ] Examples show ShowFilterOperator operator selections
- [ ] Cross-references added to GetAvailableOperators, etc.
- [ ] No new code logic added (documentation only)
- [ ] No compiler warnings

---

## Phase 3: UI Rendering (FilterInput Component Enhancement)

### Duration: 12-15 days | Effort: 3 tasks | LOC: 94

**Objective**: Enhance existing `FilterInput.razor` component to support enhanced filterbar mode + integrate operator dropdown.

---

### Task 3.1: Enhance FilterInput.razor Template with Conditional Rendering

**File**: `src/Internal/Renderer/Filter/FilterInput.razor` (UPDATE)  
**Type**: Add conditional rendering logic to existing component  
**Estimated LOC**: 50

**Requirements**:
- Add conditional rendering block: `@if (Parent?.FilterSettings?.ShowFilterOperator == true)`
- In enhanced mode: render operator dropdown icon + type-specific input component
- In legacy mode: render existing text-only input (backward compatible)
- Support all column types: String, Number, Date, DateTime, TimeOnly, Boolean, Enum, FK
- Handle IsEmpty/IsNull operators (hide value input)
- Accessibility: aria-labels, aria-required, aria-invalid
- Key handlers: Enter (apply), Escape (clear), Tab (focus)

**Acceptance Criteria**:
- [ ] Conditional logic added to FilterInput.razor
- [ ] All column types rendered correctly via @switch
- [ ] Operator dropdown icon visible + functional in enhanced mode
- [ ] Type-specific inputs render: SfDatePicker, SfNumericTextBox, SfAutoComplete, etc.
- [ ] Legacy mode still works (backward compatible)
- [ ] IsEmpty/IsNull operators hide value input
- [ ] Accessibility: All aria-labels present
- [ ] No compiler errors
- [ ] Component renders in browser without runtime errors (integration test)

**Key Sections**:
- Template header with conditional class: `e-filterdiv` or `e-filterdiv e-enhancement-filterbar`
- Type-specific input component (@switch)
- Clear filter icon button (positioned between input and operator dropdown)
- Operator dropdown trigger icon
- Operator SfDropDownList popup
- Accessibility attributes
- CSS classes applied correctly

**Clear Filter Icon Button Details**:
- HTML: `<span class="@GetClearIconClass()" @onclick="OnClearIconClick" ... />`
- CSS classes:
  - `e-icons e-filter-clear` (base)
  - `e-filter-clear-active` (when filter exists)
  - `e-filter-clear-disabled` (when no filter)
- Attributes:
  - `role="button"` (semantic HTML)
  - `aria-label="Clear filter for {ColumnField}"` (accessibility)
  - `aria-disabled="@(!HasActiveFilter())"` (state announcement)
  - `tabindex="@(HasActiveFilter() ? "0" : "-1")"` (keyboard nav)
  - `title="Clear Filter"` (tooltip with localization support)
- Event handlers:
  - `@onclick="OnClearIconClick"` (mouse click)
  - `@onkeydown="OnClearIconKeyDown"` (keyboard: Enter/Space)
- Conditional visibility: Only rendered when enhanced mode active

---

### Task 3.2: Enhance FilterInput.razor.cs Code-Behind with Enhanced Mode Logic

**File**: `src/Internal/Renderer/Filter/FilterInput.razor.cs` (UPDATE)  
**Type**: Add enhanced mode event handlers and state management to existing code-behind  
**Estimated LOC**: 38

**Requirements**:
- Add enhanced mode event handlers: OnStringInputChanged, OnNumericInput, OnDateInput, OnBooleanChanged, OnOperatorChanged
- Add key handlers: KeyDownHandler (Enter/Escape), OnIconKeyDown, OnClearIconKeyDown
- Add clear button handlers: OnClearIconClick, HasActiveFilter, GetClearIconClass
- Add validation: ValidateOperatorAndValue, GetTypedValue
- Add operator logic: GetOperatorsForColumnType, GetDefaultOperator, IsOperatorWithoutValue
- Add type detection: IsNumericInteger, IsNumericLong, IsNumericDecimal, IsNumericDouble
- Add Immediate mode support: Debounce timer or direct apply
- Preserve existing legacy mode code paths
- Error handling: Try-catch with user-friendly messages
- Cleanup: IAsyncDisposable implementation (if needed)

**Acceptance Criteria**:
- [ ] Enhanced mode event handlers implemented + functional
- [ ] Clear button handlers implemented: OnClearIconClick, HasActiveFilter, GetClearIconClass
- [ ] Validation logic working (type coercion, range, format)
- [ ] Operator detection per column type working
- [ ] Error handling in place (no unhandled exceptions)
- [ ] Legacy mode still works (backward compatible)
- [ ] XML documentation for all new public/internal methods (4-part format)
- [ ] No compiler warnings
- [ ] All parameters properly decorated ([Parameter], [CascadingParameter])
- [ ] Unit tests pass (20+ test cases covering both modes)

**Key Methods to Add**:
- `GetOperatorsForColumnType()` — Return operator list per type
- `GetDefaultOperator()` — Default operator per column type
- `ValidateOperatorAndValue()` — Type + value validation
- `GetTypedValue()` — Type coercion (string → typed value)
- `OnOperatorChanged()` — Handle operator selection + fire events
- `KeyDownHandler()` — Handle Enter/Escape keys
- `OnClearIconClick(MouseEventArgs, GridColumn)` — Clear filter on button click
- `OnClearIconKeyDown(KeyboardEventArgs, GridColumn)` — Keyboard access to clear button
- `HasActiveFilter()` — Determine if clear button enabled
- `GetClearIconClass()` — Return dynamic CSS class
- `ApplyFilter()` — Trigger filter application (enhanced mode)
- `LoadOperators()` — Initialize operator list from column type

**Clear Button Implementation**:

```csharp
private bool HasActiveFilter()
{
    // Determine if clear button should be enabled
    if (Filter<TContent>.IsNullOrEmptyOperator(CurrentOperator))
        return true;  // IsEmpty/IsNull operators show clear button
    
    bool hasFilterValue = FilterValue != null;
    bool hasFilteredValue = !string.IsNullOrEmpty(FilteredValue);
    
    return hasFilterValue || hasFilteredValue;
}

private string GetClearIconClass()
{
    // Dynamic CSS class for clear icon state
    if (HasActiveFilter())
    {
        return "e-icons e-filter-clear e-filter-clear-active";
    }
    else
    {
        return "e-icons e-filter-clear e-filter-clear-disabled";
    }
}

private async Task OnClearIconClick(MouseEventArgs e, GridColumn column)
{
    // Clear filter when user clicks clear icon
    if (column == null) return;
    
    FilteredValue = string.Empty;
    FilterValue = null;
    StringInputValue = string.Empty;
    CurrentOperator = Filter<TContent>.GetDefaultOperator(column.Type);
    column.FilterClearIcon = string.Empty;
    StopTimer();
    
    if (Parent?.FilterModule != null)
    {
        Parent.FilterModule.FilteredValue = string.Empty;
        await Parent.FilterModule.RemoveFilterColumnByField(column?.Field!, column?.Uid!);
    }
    
    await InvokeAsync(StateHasChanged).ConfigureAwait(true);
}

private async Task OnClearIconKeyDown(KeyboardEventArgs args, GridColumn column)
{
    // Allow keyboard access to clear button (Enter/Space)
    if (column == null) return;
    
    if (args?.Key == "Enter" || args?.Key == " ")
    {
        await OnClearIconClick(new MouseEventArgs(), column);
    }
}
```

---

### Task 3.3: Update FilterBarRenderer.razor Conditional Logic (if applicable)

**File**: `src/Internal/Renderer/Filter/FilterBarRenderer.razor`  
**Type**: No changes required  
**Estimated LOC**: 0

**Rationale**:
- FilterBarRenderer already renders `FilterInput.razor` component
- `FilterInput.razor` now handles both legacy and enhanced modes internally via `ShowFilterOperator` setting
- No conditional rendering changes needed in FilterBarRenderer
- Backward compatibility maintained automatically

**Acceptance Criteria**:
- [ ] No changes made to FilterBarRenderer
- [ ] FilterInput.razor renders with correct mode based on ShowFilterOperator
- [ ] Backward compatibility confirmed (existing behavior unchanged when flag not set)
- [ ] Integration test: FilterBar displays correct UI based on setting

---

## Phase 4: Testing & Validation

### Duration: 18-21 days | Effort: Parallel | Tests: 79+

**Objective**: Write + execute comprehensive tests (unit, component, integration, E2E).

---

### Task 4.1: Unit Tests (String + Number Operators)

**File**: `tests/FilterBar/ShowFilterOperator.StringNumber.Tests.cs`  
**Estimated Tests**: 25-30

**Test Coverage**:
- [x] GetAvailableOperators (String, Number columns)
- [x] GetCurrentFilterOperator (initial, after change)
- [x] ChangeFilterOperatorAsync (success, validation failure)
- [x] ValidateOperatorAndValue (all operator types)
- [x] GetTypedValue (type coercion)

---

### Task 4.2: Component Tests (Type-Specific Inputs)

**File**: `tests/FilterBar/FilterInput.EnhancedMode.Component.Tests.cs`  
**Estimated Tests**: 25-30

**Test Coverage**:
- [x] FilterInput renders correctly per column type (enhanced mode)
- [x] SfDatePicker renders for Date columns
- [x] SfNumericTextBox renders for Number columns
- [x] SfAutoComplete renders for String columns
- [x] Operator dropdown appears + selectable
- [x] IsEmpty/IsNull hides value input

---

### Task 4.3: Integration Tests (Cross-Feature)

**File**: `tests/FilterBar/ShowFilterOperator.Integration.Tests.cs`  
**Estimated Tests**: 15-20

**Test Coverage**:
- [x] Virtualization + operator change
- [x] Grouping + filter applied
- [x] Paging + filter updates pager
- [x] Selection + filter behavior
- [x] Sorting + filter composition

---

### Task 4.4: Accessibility Tests

**File**: `tests/FilterBar/ShowFilterOperator.Accessibility.Tests.cs`  
**Estimated Tests**: 10-15

**Test Coverage**:
- [x] WCAG 2.1 AA compliance (axe-core scan)
- [x] Keyboard navigation (Tab, Shift+Tab, Enter, Escape)
- [x] Screen reader announcements (ARIA labels, live regions)
- [x] Focus management

---

### Task 4.5: Cross-Browser Tests

**Manual / E2E**: Chrome, Firefox, Edge, Safari (latest 2 versions)

---

## Phase 5: Polish & Accessibility

### Duration: 9-11 days | Effort: 2 tasks | LOC: ~26

---

### Task 5.1: Wire Operator Events in Lifecycle

**File**: `src/SfGrid.Lifecycle.cs`  
**Type**: Modify lifecycle initialization  
**Estimated LOC**: 8

**Requirements**:
- Subscribe to OnBeforeOperatorChange event (if developer provided)
- Subscribe to OnOperatorChanged event (if developer provided)
- Wire in appropriate lifecycle phase (OnAfterRenderAsync or SetParametersAsync)
- Cleanup subscriptions in disposal phase

**Acceptance Criteria**:
- [ ] Event subscriptions wired in Lifecycle
- [ ] Events fire at correct moments (before/after operator change)
- [ ] No double-subscription
- [ ] Cleanup on component disposal
- [ ] No compiler warnings

---

### Task 5.2: Implement Component Cleanup (DisposeAsync)

**File**: `src/Internal/Renderer/Filter/FilterInput.razor.cs`  
**Type**: Implement IAsyncDisposable cleanup for enhanced mode  
**Estimated LOC**: 18

**Requirements**:
- Implement `async ValueTask IAsyncDisposable.DisposeAsync()` (if not already present)
- Dispose/cleanup all SfDropDownList + input component references in enhanced mode
- Unsubscribe from any internal timers (Immediate mode debounce)
- Clear event subscriptions
- Prevent memory leaks

**Acceptance Criteria**:
- [ ] DisposeAsync implemented correctly
- [ ] All SfComponent references disposed
- [ ] Timers cancelled + cleaned up
- [ ] No memory leaks detected (profiler testing)
- [ ] No compiler warnings

---

## Summary

### Execution Schedule

```
Week 1-3:   Phase 1 (Foundation) — Complete model, parameters, events
Week 3-5:   Phase 2 (Core Logic) — Implement operator discovery + validation
Week 5-9:   Phase 3 (UI Rendering) — Enhance FilterInput component with operator dropdown + type-specific controls
Week 4-9:   Phase 4 (Testing) — Write + execute 79+ tests (parallel)
Week 8-9:   Phase 5 (Polish) — Cleanup, events, accessibility final checks

TOTAL: 8-9 weeks (with parallel testing)
```

### Task Dependencies

```
Phase 1 ← [Gate: All Phase 1 complete]
  ↓
Phase 2 ← [Depends on: Phase 1 foundation]
  ↓
Phase 3 ← [Depends on: Phase 1 + Phase 2]
  ↓
Phase 4 (Parallel with Phases 2-3)
  ↓
Phase 5 ← [Final polish after Phase 3]
```

### Success Criteria

✅ All 16 tasks complete  
✅ 79+ tests GREEN  
✅ Zero compiler warnings  
✅ WCAG 2.1 AA compliant  
✅ Cross-browser validated  
✅ Performance <50ms overhead  
✅ Backward compatibility confirmed  

---

**Status**: ✅ TASK BREAKDOWN COMPLETE  
**Next**: START IMPLEMENTATION (Phase 1, Task 1.1)  
**Created**: May 15, 2026  

