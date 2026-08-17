# Design Document: Enhanced FilterBar with Operator Dropdowns

**Date**: May 15, 2026  
**Feature**: Enhanced FilterBar Support with Operator Dropdowns and Integrated UI Controls  
**Component**: `FilterInput.razor` (integrated) + supporting infrastructure  
**Status**: ✅ DESIGN COMPLETE (Architecture Updated - Integrated into FilterInput.razor)

---

## Architecture Overview

### Component Hierarchy

```
FilterBarRenderer.razor
  └─ FilterInput.razor (unified)
      ├─ [IF ShowFilterOperator==true]
      │   ├─ Type-Specific Input Control
      │   │   ├─ SfAutoComplete (String)
      │   │   ├─ SfNumericTextBox (Number)
      │   │   ├─ SfDatePicker (Date)
      │   │   ├─ SfDateTimePicker (DateTime)
      │   │   ├─ SfTimePicker (TimeOnly)
      │   │   ├─ Tri-State Selector (Boolean)
      │   │   └─ SfDropDownList (Enum/FK)
      │   │
      │   └─ Operator Selector
      │       ├─ Dropdown Arrow Icon (trigger)
      │       └─ SfDropDownList (operator popup)
      │
      └─ [ELSE ShowFilterOperator==false]
          └─ Standard text-based filter input (backward compatible)
```

### Data Flow

```
User clicks operator dropdown arrow
    ↓
[Operator Dropdown opens] → SfDropDownList popup shows available operators
    ↓
User selects operator (e.g., "GreaterThan")
    ↓
[OnChange event fires]
    ↓
[BeforeOperatorChange event]
  ├─ Validation: Can user perform this operator change?
  └─ If Cancel=true → Abort; revert operator
    ↓
[Operator confirmed]
    ↓
[Value input component updates]
  └─ Old component type → New component type (if needed)
    ↓
[OnOperatorChanged event]
  ├─ Logging: Track operator change for audit
  └─ Side effects: Update dependent filters, etc.
    ↓
[User enters value]
    ↓
[Validation on blur]
  ├─ Type coercion (string → int, etc.)
  ├─ Range validation (min/max)
  └─ Format validation (date format, etc.)
    ↓
[Press Enter OR Immediate mode timeout]
    ↓
[FilterByColumn] → Apply filter
    ↓
[OnActionComplete event] → Grid updates
```

---

## Component Design: `FilterInput.razor` (Enhanced Mode)

### Overview

The `FilterInput.razor` component now supports both legacy and enhanced filter bar modes:
- **Legacy Mode** (ShowFilterOperator=false): Text-only input for backward compatibility
- **Enhanced Mode** (ShowFilterOperator=true): Operator dropdown + type-specific input controls

### Template Structure

#### HTML Layout

```html
<td class="e-filterdiv e-enhancement-filterbar">
    <div class="e-enhanced-filter-input">
        
        <!-- Type-Specific Input Control (e.g., SfDatePicker, SfNumericTextBox) -->
        <SfComponentPerType>
            <!-- Changes based on column type -->
        </SfComponentPerType>
        
        <!-- Operator Dropdown Trigger Icon -->
        <div class="e-icons e-filter" 
             @onmousedown="OnIconMouseDown"
             @onclick="OpenOperatorDropdown"
             @onclick:stopPropagation="true"
             @onkeydown="OnIconKeyDown"
             role="button"
             tabindex="0"
             aria-label="@GetOperatorIconAriaLabel()"
             title="Filter Operator">
        </div>
        
        <!-- Clear Filter Icon Button -->
        <span class="@GetClearIconClass()"
              @onclick="@((MouseEventArgs e) => HasActiveFilter() ? OnClearIconClick(e, FilterInputParameters.Column!) : Task.CompletedTask)"
              @onkeydown="@((KeyboardEventArgs args) => OnClearIconKeyDown(args, FilterInputParameters.Column!))"
              role="button"
              tabindex="@(HasActiveFilter() ? "0" : "-1")"
              aria-label="@($"Clear filter for {FilterInputParameters.Column?.Field}")"
              aria-disabled="@(!HasActiveFilter())"
              title="Clear Filter">
        </span>
        
        <!-- Operator Dropdown (inline popup) -->
        <SfDropDownList @ref="OperatorDropDown"
                        @key="@($"{FilterInputParameters.Column?.Uid}_operator")"
                        TValue="string"
                        TItem="OperatorItem"
                        DataSource="@AvailableOperators"
                        @bind-Value="@CurrentOperator"
                        ID="@($"{FilterInputParameters.Column?.Field}_operator")"
                        CssClass="e-enhanced-operator-dropdown"
                        PopupHeight="300px"
                        PopupWidth="160px">
            <DropDownListFieldSettings Text="Text" Value="Value"></DropDownListFieldSettings>
            <DropDownListEvents TValue="string" TItem="OperatorItem" 
                               ValueChange="@OnOperatorChanged" 
                               Closed="@OnOperatorDropdownClosed">
            </DropDownListEvents>
        </SfDropDownList>
        
    </div>
</td>
```

**UI Components Layout** (Enhanced Mode):
```
┌─────────────────────────────────────────────┐
│ [Type-Input]  [Filter Icon]  [Clear X]  [▼] │
│              (Operator)                      │
│                        ┌──────────────────┐  │
│                        │ Contains       ✓ │  │
│                        │ StartsWith       │  │
│                        │ EndsWith         │  │
│                        │ Equal            │  │
│                        └──────────────────┘  │
└─────────────────────────────────────────────┘
```

### Code-Behind Logic

```csharp
@code {
    // Input Parameters
    [Parameter] public FilterInputParameters FilterInputParameters { get; set; }
    [CascadingParameter] public SfGrid<dynamic> Parent { get; set; }
    
    // Type-Specific Input References
    private SfTextBox StringInput;
    private SfNumericTextBox<int?> NumericValueAsInt;
    private SfNumericTextBox<long?> NumericValueAsLong;
    private SfNumericTextBox<double?> NumericValueAsDouble;
    private SfNumericTextBox<decimal?> NumericValueAsDecimal;
    private SfDatePicker<DateTime?> DatePickerComponent;
    private SfDateTimePicker<DateTime?> DateTimePickerComponent;
    private SfTimePicker<TimeOnly?> TimePickerComponent;
    private SfDropDownList<string, OperatorItem> OperatorDropDown;
    private SfDropDownList<bool?, BooleanOption> BooleanDropDown;
    
    // State
    private string CurrentOperator;
    private string PreviousOperator;
    private object FilterValue;
    private List<OperatorItem> AvailableOperators = new();
    private List<BooleanOption> BooleanOptions;
    
    // Lifecycle
    protected override async Task OnInitializedAsync()
    {
        // Initialize available operators based on column type
        AvailableOperators = GetOperatorsForColumnType(FilterInputParameters.Column.Type);
        CurrentOperator = FilterInputParameters.Predicate ?? GetDefaultOperator();
        PreviousOperator = CurrentOperator;
        FilterValue = FilterInputParameters.FilterValue;
        
        // Boolean options for tri-state
        BooleanOptions = new()
        {
            new BooleanOption { Text = "True", Value = true },
            new BooleanOption { Text = "False", Value = false },
            new BooleanOption { Text = "(Empty)", Value = null }
        };
    }
    
    // Operator Selection
    private async Task OpenOperatorDropdown()
    {
        if (OperatorDropDown != null)
            await OperatorDropDown.ShowPopupAsync();
    }
    
    private void OnIconMouseDown(MouseEventArgs args)
    {
        // Track if dropdown was open before mouse down (for toggle behavior)
        _wasOpenOnMouseDown = IsOperatorDropdownOpen;
    }
    
    private async Task OnIconKeyDown(KeyboardEventArgs args)
    {
        // Allow keyboard access (Enter/Space to open, Escape to close)
        if (args?.Key == "Escape" && IsOperatorDropdownOpen && OperatorDropDown != null)
        {
            await OperatorDropDown.HidePopupAsync().ConfigureAwait(true);
        }
    }
    
    // Clear Filter Button
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
    
    private string GetOperatorIconAriaLabel()
    {
        // Accessibility: Announce filter status in label
        if (HasActiveFilter())
        {
            return $"Filter operator for {FilterInputParameters?.Column?.Field}, currently filtered";
        }
        else
        {
            return $"Filter operator for {FilterInputParameters?.Column?.Field}";
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
        
        await Parent!.InvokeMethod("sfBlazor.Grid.searchClear", new object[]
        {
            Parent.DataId, $"{column?.Field}_filterBarcell"
        });
        
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
    
    private void UpdateFilterDisplayState()
    {
        // Update clear icon visibility based on filter state
        FilterInputParameters!.Column!.FilterClearIcon = 
            HasActiveFilter() ? "e-clear-icon" : string.Empty;
    }
    
    // Operator Selection
    private async Task OnOperatorChanged(ChangeEventArgs<string, OperatorItem> e)
    {
        var newOperator = e.Value;
        
        // Fire BeforeOperatorChange event
        var beforeArgs = new BeforeOperatorChangeEventArgs
        {
            Column = FilterInputParameters.Column,
            CurrentOperator = CurrentOperator,
            NewOperator = newOperator,
            Cancel = false
        };
        
        if (Parent?.OnBeforeOperatorChange != null)
            await Parent.OnBeforeOperatorChange.InvokeAsync(beforeArgs);
        
        if (beforeArgs.Cancel)
        {
            // Revert operator selection
            CurrentOperator = PreviousOperator;
            await OperatorDropDown.SetValueAsync(PreviousOperator);
            return;
        }
        
        // Update operator state
        PreviousOperator = CurrentOperator;
        CurrentOperator = newOperator;
        
        // Clear value input if operator doesn't need one
        if (IsOperatorWithoutValue(newOperator))
            FilterValue = null;
        
        // Fire OnOperatorChanged event
        var afterArgs = new OperatorChangedEventArgs
        {
            Column = FilterInputParameters.Column,
            PreviousOperator = PreviousOperator,
            NewOperator = newOperator,
            CurrentValue = FilterValue?.ToString(),
            ChangedAt = DateTime.Now
        };
        
        if (Parent?.OnOperatorChanged != null)
            await Parent.OnOperatorChanged.InvokeAsync(afterArgs);
        
        StateHasChanged();
    }
    
    // Type-Specific Input Handlers
    private async Task OnStringInputChanged(ChangeEventArgs e)
    {
        FilterValue = e.Value;
        if (Parent?.FilterSettings?.Mode == FilterBarMode.Immediate)
            await ApplyFilter();
    }
    
    private async Task OnNumericIntInput(ChangeEventArgs e)
    {
        FilterValue = e.Value;
        if (Parent?.FilterSettings?.Mode == FilterBarMode.Immediate)
            await ApplyFilter();
    }
    
    private async Task OnDateInput(ChangeEventArgs e)
    {
        FilterValue = e.Value;
        if (Parent?.FilterSettings?.Mode == FilterBarMode.Immediate)
            await ApplyFilter();
    }
    
    private async Task OnBooleanValueChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<bool?, BooleanOption> e)
    {
        FilterValue = e.Value;
        await ApplyFilter();  // Boolean filters apply immediately
    }
    
    // Enter Key Handler (OnEnter mode)
    private async Task KeyDownHandler(KeyboardEventArgs args, GridColumn column)
    {
        if (args.Key == "Enter")
        {
            await ApplyFilter();
        }
        else if (args.Key == "Escape")
        {
            FilterValue = null;
            StateHasChanged();
        }
    }
    
    // Icon Keyboard Handler (Operator Dropdown Trigger)
    private async Task OnIconKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Enter" || args.Key == " ")
        {
            args.PreventDefault();
            await OpenOperatorDropdown();
        }
        else if (args.Key == "Escape")
        {
            // Close dropdown (if open)
            if (OperatorDropDown != null)
                await OperatorDropDown.HidePopupAsync();
        }
    }
    
    // Apply Filter
    private async Task ApplyFilter()
    {
        try
        {
            // Validate operator & value
            if (!ValidateOperatorAndValue(CurrentOperator, FilterValue))
            {
                ShowValidationError();
                return;
            }
            
            // Type coercion & formatting
            var typedValue = GetTypedValue(FilterValue, FilterInputParameters.Column.Type);
            
            // Apply filter via grid
            if (Parent != null)
            {
                await Parent.FilterByColumn(
                    FilterInputParameters.FieldName,
                    typedValue,
                    CurrentOperator
                );
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }
    
    // Validation Helpers
    private bool ValidateOperatorAndValue(string op, object value)
    {
        // IsEmpty, IsNull, IsNotEmpty, IsNotNull don't need values
        if (new[] { "IsEmpty", "IsNull", "IsNotEmpty", "IsNotNull" }.Contains(op))
            return true;
        
        // Other operators require a value
        return value != null && !string.IsNullOrEmpty(value.ToString());
    }
    
    private object GetTypedValue(object value, ColumnType type)
    {
        return type switch
        {
            ColumnType.Number => Convert.ToDouble(value),
            ColumnType.Date => Convert.ToDateTime(value),
            ColumnType.Boolean => Convert.ToBoolean(value),
            _ => value
        };
    }
    
    // Operator & Type Helpers
    private List<OperatorItem> GetOperatorsForColumnType(ColumnType type) => type switch
    {
        ColumnType.String => new()
        {
            new() { Text = "Contains", Value = "Contains" },
            new() { Text = "Does not contain", Value = "DoesNotContain" },
            new() { Text = "Starts with", Value = "StartsWith" },
            new() { Text = "Ends with", Value = "EndsWith" },
            new() { Text = "Equal", Value = "Equal" },
            new() { Text = "Not equal", Value = "NotEqual" },
            new() { Text = "Is empty", Value = "IsEmpty" },
            new() { Text = "Is not empty", Value = "IsNotEmpty" }
        },
        ColumnType.Integer or ColumnType.Double or ColumnType.Long or ColumnType.Decimal => new()
        {
            new() { Text = "Equal", Value = "Equal" },
            new() { Text = "Not equal", Value = "NotEqual" },
            new() { Text = "Greater than", Value = "GreaterThan" },
            new() { Text = "Greater than or equal", Value = "GreaterThanOrEqual" },
            new() { Text = "Less than", Value = "LessThan" },
            new() { Text = "Less than or equal", Value = "LessThanOrEqual" },
            new() { Text = "Between", Value = "Between" },
            new() { Text = "Not between", Value = "NotBetween" },
            new() { Text = "Is null", Value = "IsNull" },
            new() { Text = "Is not null", Value = "IsNotNull" }
        },
        ColumnType.Date or ColumnType.DateTime => new()
        {
            new() { Text = "Equal", Value = "Equal" },
            new() { Text = "Not equal", Value = "NotEqual" },
            new() { Text = "After", Value = "GreaterThan" },
            new() { Text = "On or after", Value = "GreaterThanOrEqual" },
            new() { Text = "Before", Value = "LessThan" },
            new() { Text = "On or before", Value = "LessThanOrEqual" },
            new() { Text = "Between", Value = "Between" },
            new() { Text = "Is null", Value = "IsNull" },
            new() { Text = "Is not null", Value = "IsNotNull" }
        },
        ColumnType.Boolean => new()
        {
            new() { Text = "Equal", Value = "Equal" },
            new() { Text = "Not equal", Value = "NotEqual" },
            new() { Text = "Is null", Value = "IsNull" },
            new() { Text = "Is not null", Value = "IsNotNull" }
        },
        _ => new()
    };
    
    private string GetDefaultOperator() => FilterInputParameters.Column.Type switch
    {
        ColumnType.String => "Contains",
        ColumnType.Integer or ColumnType.Double => "Equal",
        ColumnType.Date or ColumnType.DateTime => "Equal",
        ColumnType.Boolean => "Equal",
        _ => "Equal"
    };
    
    private bool IsOperatorWithoutValue(string op) =>
        new[] { "IsEmpty", "IsNull", "IsNotEmpty", "IsNotNull" }.Contains(op);
    
    // UI State Helpers
    private bool GetInputEnabledState(string op, ColumnType? type) =>
        !IsOperatorWithoutValue(op);
    
    private string GetPlaceholder() =>
        FilterInputParameters.Column?.FilterInputPlaceholder ?? 
        $"Filter {FilterInputParameters.Column?.Field}";
    
    // Cleanup
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        // Cleanup event subscriptions & resources
        if (OperatorDropDown != null)
            await OperatorDropDown.DisposeAsync();
    }
}

// Helper Classes
public class OperatorItem
{
    public string Text { get; set; }
    public string Value { get; set; }
}

public class BooleanOption
{
    public string Text { get; set; }
    public bool? Value { get; set; }
}
```

---

## Integration Points

### 1. FilterBarRenderer.razor (Conditional Rendering - No Change Needed)

The FilterBarRenderer already renders `FilterInput.razor`, which now handles both modes internally:

```csharp
// FilterBarRenderer.razor - existing code, no change required
<FilterInput TContent="TContent" 
            FilterInputParameters="@FilterInputParameters" />

// FilterInput.razor internally uses:
if (Parent?.FilterSettings?.ShowFilterOperator == true)
{
    // Render enhanced mode: operator dropdown + type-specific controls
}
else
{
    // Render legacy mode: text-only input
}
```

### 2. GridFilterSettings.razor (New Parameters)

```csharp
/// <summary>Enables operator dropdown selector + type-specific inputs in FilterBar.</summary>
[Parameter]
public bool ShowFilterOperator { get; set; } = false;

/// <summary>CSS width for operator dropdown selector.</summary>
[Parameter]
public string OperatorDropdownWidth { get; set; } = "auto";
```

### 3. GridColumn.cs (New Properties)

```csharp
/// <summary>Per-column operator override (restricts available operators).</summary>
public List<string> CustomOperators { get; set; }

/// <summary>Hide operator dropdown for this column.</summary>
public bool ShowOperatorDropdown { get; set; } = true;

/// <summary>Placeholder text for filter value input.</summary>
public string FilterInputPlaceholder { get; set; }
```

### 4. GridEvents.cs (New Event Callbacks)

```csharp
/// <summary>Fires before operator changes; can be cancelled.</summary>
public EventCallback<BeforeOperatorChangeEventArgs> OnBeforeOperatorChange { get; set; }

/// <summary>Fires after operator successfully changes.</summary>
public EventCallback<OperatorChangedEventArgs> OnOperatorChanged { get; set; }
```

### 5. SfGrid.Properties.cs (New Public Methods)

```csharp
public List<string> GetAvailableOperators(string columnField)
{
    // Return operators for column, respecting CustomOperators override
}

public async Task ChangeFilterOperatorAsync(string columnField, string newOperator)
{
    // Change operator programmatically; fires events
}

public string GetCurrentFilterOperator(string columnField)
{
    // Get currently selected operator
}
```

---

## Type-to-Component Mapping

| Column Type | Input Component | Operators | Example |
|---|---|---|---|
| **String** | SfAutoComplete | Contains, StartsWith, Equal, ... (8+) | Product name search |
| **Int, Long, Decimal, Double** | SfNumericTextBox | =, >, >=, <, <=, Between, ... (10) | Price > 100 |
| **Date, DateOnly** | SfDatePicker | =, After, Before, Between, ... (9) | OrderDate >= 2025-01-01 |
| **DateTime** | SfDateTimePicker | =, After, Before, Between, ... (9) | CreatedAt > 2025-01-01 10:00 |
| **TimeOnly** | SfTimePicker | =, After, Before, Between, ... (8) | StartTime >= 09:00 |
| **Boolean** | Tri-state SfDropDownList | Equal, NotEqual, IsNull, ... (4) | Status = true |
| **Enum** | SfDropDownList | Equal, NotEqual, IsEmpty, ... (4) | Priority = High |
| **ForeignKey** | Async SfDropDownList | Equal, NotEqual, IsEmpty, ... (4) | DepartmentId = Sales |

---

## CSS Architecture

### Style Classes

```css
/* Container */
.e-filterdiv.e-enhancement-filterbar {
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 4px 8px;
}

/* Enhanced Input Container */
.e-enhanced-filter-input {
    display: flex;
    align-items: center;
    gap: 8px;
    flex: 1;
}

/* Type-Specific Input */
.e-enhanced-filter-input .e-control {
    flex: 1;
    min-width: 100px;
}

/* Operator Dropdown Icon Trigger */
.e-icons.e-filter {
    cursor: pointer;
    font-size: 16px;
    color: #666;
    transition: color 0.2s;
    padding: 4px 8px;
}

.e-icons.e-filter:hover {
    color: #333;
}

/* Operator Dropdown Styles */
.e-enhanced-operator-dropdown {
    min-width: 160px;
}

.e-enhanced-operator-dropdown .e-popup {
    max-height: 300px;
    border-radius: 4px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}
```

---

## Accessibility Implementation

### ARIA Attributes

```html
<!-- Operator Dropdown Trigger -->
<div class="e-icons e-filter"
     role="button"
     tabindex="0"
     aria-label="Filter operator for {ColumnName}"
     aria-expanded="false"
     aria-controls="{DropdownMenuId}">
</div>

<!-- Value Input -->
<SfDatePicker aria-label="Filter value for {ColumnName} using {SelectedOperator}"
              aria-required="true"
              aria-invalid="@(HasError ? "true" : "false")"
              aria-describedby="@(HasError ? $"{FieldId}_error" : null)" />

<!-- Error Message -->
@if (HasError)
{
    <div id="@($"{FieldId}_error")" 
         role="alert" 
         class="e-error-message">
        @ErrorMessage
    </div>
}
```

### Keyboard Navigation

| Key | Action |
|-----|--------|
| **Tab** | Move focus to next element (operator dropdown → value input) |
| **Shift+Tab** | Move focus to previous element |
| **Enter** on operator icon | Open operator dropdown menu |
| **Arrow Down/Up** in dropdown | Navigate operator options |
| **Enter** on operator option | Select operator |
| **Escape** in dropdown | Close dropdown |
| **Enter** in value input | Apply filter (OnEnter mode) |
| **Escape** in value input | Clear value |

---

## Error Handling

### Validation Errors

| Scenario | Error Message | UI Feedback |
|----------|---|---|
| Invalid numeric value | "Expected numeric value, got 'abc'" | Red border on input; error text below |
| Invalid date format | "Invalid date format. Expected {format}" | Red border on input; error text below |
| Value outside range | "Value must be between {min} and {max}" | Red border + tooltip |
| Missing required value | "Value required for '{Operator}' operator" | Red border; tooltip appears |
| Invalid enum/FK value | "Value not found in available options" | Red border; error message |

### Exception Handling

```csharp
try
{
    // Validate & apply filter
    await ApplyFilter();
}
catch (ArgumentException ex)
{
    // Type coercion failure
    ShowError($"Invalid value: {ex.Message}");
}
catch (Exception ex)
{
    // Unexpected error
    Logger.LogError(ex, "Filter application failed");
    ShowError("An error occurred. Please try again.");
}
```

---

## Performance Optimization

### Memoization

- Operator lists cached per column type (no dynamic generation per row)
- Boolean options list cached as singleton

### Lazy Loading

- Type-specific components only instantiated when visible
- Operator dropdown DataSource not pre-populated (loaded on demand)

### Virtual Rendering

- FilterInput component (enhanced mode) only renders for visible columns in column virtualization mode
- Off-screen columns' state maintained but not rendered

---

## Summary

| Aspect | Details |
|--------|---------|
| **Component** | `FilterInput.razor` (enhanced mode) + supporting code |
| **Layout** | Horizontal: [Type-specific input] [Operator dropdown icon] [Operator menu] |
| **Type-Specific Controls** | SfDatePicker, SfNumericTextBox, SfAutoComplete, SfDropDownList, SfTimePicker |
| **Operators** | 4-10 per column type; respect CustomOperators override |
| **Events** | BeforeOperatorChange (cancellable), OnOperatorChanged (post-change) |
| **Validation** | Type coercion, range, format, operator-value matching |
| **Accessibility** | WCAG 2.1 AA, keyboard navigation, screen reader support |
| **Performance** | <50ms overhead, operator list cached, lazy-loaded components |
| **Risk Level** | LOW — component reuse from Editing feature, proven patterns |

---

**Status**: ✅ DESIGN COMPLETE  
**Next Phase**: IMPLEMENTATION  
**Created**: May 15, 2026  

