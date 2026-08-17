# Naming Conventions — Syncfusion Blazor DataGrid

> **Audience**: All developers contributing to `SfGrid<TValue>`
> **Prerequisite**: [`code-guidelines/coding-standards.md`](./coding-standards.md)
> **Last Updated**: March 10, 2026

---

## Overview

Consistent naming is enforced across all source files. These conventions are derived from the existing codebase patterns and must be followed without exception. Deviations will be rejected during code review.

---

## 1. File Naming

| Type | Convention | Examples |
|------|-----------|---------|
| C# class files | `PascalCase.cs` | `GridColumn.cs`, `GridEditSettings.cs` |
| Razor component | `PascalCase.razor` | `GridHeader.razor`, `GridRow.razor` |
| Razor code-behind | `PascalCase.razor.cs` | `GridHeader.razor.cs`, `GridRow.razor.cs` |
| Partial class files | `ClassName.Purpose.cs` | `SfGrid.Lifecycle.cs`, `SfGrid.Methods.cs`, `SfGrid.Properties.cs` |
| Enum files | `PascalCase.cs` | `GridsEnumerations.cs` |
| Interface files | `IPascalCase.cs` | `IGrid.cs` |
| Internal action modules | `PascalCase.cs` | `Sort.cs`, `Edit.cs`, `Filter.cs` |
| Documentation files | `kebab-case.md` | `coding-standards.md`, `data-flow.md` |

---

## 2. Type Names

### 2.1 Classes

Use `PascalCase`. Internal classes use no prefix or suffix.

```csharp
// ✅ CORRECT
public partial class SfGrid<TValue> { }
internal class DataGenerator<T> { }
internal class GridJSInteropAdaptor<T> { }
public class GridColumn { }
public class ActionEventArgs<T> { }
```

### 2.2 Interfaces

Use `PascalCase` with an `I` prefix.

```csharp
// ✅ CORRECT
public interface IGrid { }
public interface ISfCircularComponent { }
```

### 2.3 Generic Type Parameters

| Position | Convention | Example |
|----------|-----------|---------|
| Grid model type (public API) | `TValue` | `SfGrid<TValue>` |
| Grid model type (internal) | `T` | `Sort<T>`, `Edit<T>` |
| Other generics | `TPurpose` | `TKey`, `TResult` |

```csharp
// ✅ CORRECT — public-facing uses TValue
public partial class SfGrid<TValue> { }

// ✅ CORRECT — internal module uses T
internal class Sort<T> { }
```

### 2.4 Enumerations

Use `PascalCase` for both the enum type name and every member value.

```csharp
// ✅ CORRECT
public enum SortDirection { Ascending, Descending, None }
public enum EditMode { Normal, Dialog, Batch }
public enum FilterType { FilterBar, Menu, Excel, CheckBox }
```

---

## 3. Variable and Field Naming

### 3.1 Public Properties

`PascalCase` — no underscores, no prefixes.

```csharp
// ✅ CORRECT
public bool AllowSorting { get; set; }
public List<GridColumn>? Columns { get; set; }
public GridEditSettings? EditSettings { get; set; }
```

### 3.2 Private Backing Fields (Parameters)

`_camelCase` — single underscore prefix, camelCase suffix. Mirrors the parameter name.

```csharp
// ✅ CORRECT — backing field for AllowSorting [Parameter]
private bool _allowSorting { get; set; }

// ✅ CORRECT — backing field for Columns [Parameter]
private List<GridColumn>? _columns { get; set; }
```

### 3.3 Private Instance Fields (Non-Parameter)

`_camelCase` — single underscore prefix.

```csharp
// ✅ CORRECT
private bool _isLoaded { get; set; }
private bool _isObservableWired { get; set; }
private GridJSInteropAdaptor<TValue>? _jsAdaptor { get; set; }
```

### 3.4 Internal Properties on Modules

`PascalCase` — no prefix for `internal` properties that are accessed across modules.

```csharp
// ✅ CORRECT — accessed as Parent.SelectionModule.IsAdd
internal bool IsAdd { get; set; }
internal bool KeyPressed { get; set; }
internal EditContext? EditContext { get; set; }
```

### 3.5 Local Variables

`camelCase` — no prefix.

```csharp
// ✅ CORRECT
var sortedColumns = Parent.SortSettings!.Columns;
int rowIndex = GetRowIndex(data);
bool isMultiSort = e.CtrlKey || e.MetaKey;
```

### 3.6 Constants

`UPPER_SNAKE_CASE` for private constants. `PascalCase` for public constants.

```csharp
// ✅ CORRECT — private constant
private const int VIRTUAL_ROW_THRESHOLD = 50;
private const string BASE_CSS_CLASS = "e-grid";

// ✅ CORRECT — public constant
public const string DefaultDateFormat = "MM/dd/yyyy";
```

---

## 4. Method Naming

### 4.1 Public Async Methods

Use `PascalCase` with `Async` suffix. All public async methods on `SfGrid<TValue>` end with `Async`.

```csharp
// ✅ CORRECT
public async Task SortColumnAsync(string columnName, SortDirection direction) { }
public async Task FilterByColumnAsync(string fieldName, string filterOperator, object value) { }
public async Task SelectRowAsync(int index, bool? isToggle = null) { }
public async Task AddRecordAsync(TValue data, int? index = null) { }
public async Task ExportToExcelAsync(ExcelExportProperties? excelExportProperties = null) { }
```

### 4.2 Internal Async Methods

Use `PascalCase`. The `Async` suffix is optional for internal methods but encouraged for clarity.

```csharp
// ✅ CORRECT
internal async Task SortColumn(string columnName, SortDirection direction) { }
internal async Task InitiateSort(GridColumn column, string cssClass, MouseEventArgs args) { }
internal async Task SaveRecord() { }
```

### 4.3 Event Handler Methods

Use `On` + noun + event verb pattern.

```csharp
// ✅ CORRECT
internal async Task OnSortHeaderClick(GridColumn column, MouseEventArgs args) { }
internal void OnDataSourceChanged() { }
internal async Task OnRowEditBegin(ActionEventArgs<T> args) { }
```

### 4.4 Private Helper Methods

Use `PascalCase`. Name clearly describes what is returned or done.

```csharp
// ✅ CORRECT
private List<GridColumn> GetFlatColumnList() { }
private bool IsColumnRefreshRequired(string propertyName) { }
private string BuildCssClass(GridColumn column) { }
```

---

## 5. Event Naming

### 5.1 `EventCallback` Parameters on `GridEvents<TValue>`

Use `On` + `PascalCase` noun + action suffix pattern, consistent with existing events.

```csharp
// ✅ CORRECT — matches existing conventions
public EventCallback<ActionEventArgs<TValue>> OnActionBegin { get; set; }
public EventCallback<ActionEventArgs<TValue>> OnActionComplete { get; set; }
public EventCallback<RowSelectEventArgs<TValue>> RowSelected { get; set; }
public EventCallback<RowDeselectEventArgs<TValue>> RowDeselected { get; set; }
public EventCallback<RecordDoubleClickEventArgs<TValue>> RecordDoubleClick { get; set; }
```

> **Rule**: Events that fire *before* an action use `On[Action]` (e.g., `OnActionBegin`). Events that fire *after* use the noun-only form (e.g., `RowSelected`, `DataBound`).

---

## 6. CSS Class Naming

All dynamically generated CSS class strings follow the Syncfusion `e-` prefix convention.

```csharp
// ✅ CORRECT
private const string ASCENDING_CSS  = "e-ascending";
private const string DESCENDING_CSS = "e-descending";
private const string SORTED_CSS     = "e-sorted";

// ✅ CORRECT — composite class strings
ColumnMenuClass = $"e-hide-menu e-{ID}-column-menu e-grid-column-menu e-grid-menu";
```

Never use plain string literals for CSS classes in logic — always use named constants or well-named variables.

---

## 7. JSON Serialization Names

All `[Parameter]` properties must carry `[JsonPropertyName("camelCaseName")]` matching the EJ2 JavaScript property name.

```csharp
// ✅ CORRECT
[Parameter]
[JsonPropertyName("allowSorting")]
public bool AllowSorting { get; set; }

[Parameter]
[JsonPropertyName("allowMultiSorting")]
public bool AllowMultiSorting { get; set; }
```

Enum members must carry `[EnumMember(Value = "PascalCaseString")]` for JSON round-tripping.

```csharp
// ✅ CORRECT
[EnumMember(Value = "Ascending")]
Ascending,

[EnumMember(Value = "Descending")]
Descending,
```

---

## 8. Razor Component Naming

### 8.1 Component Names

`PascalCase`. All grid child components use the `Grid` prefix or the feature area prefix.

```
GridHeader.razor
GridRow.razor
GridHeaderCell.razor
GridVirtualContent.razor
GridAddNewRow.razor
```

### 8.2 Razor Parameters

`PascalCase` — identical to C# property naming.

```razor
@* ✅ CORRECT *@
<GridRow Row="@currentRow" IsSelected="@isRowSelected" />
```

### 8.3 Razor References

`@ref` variables use `camelCase` with the component type name as suffix.

```razor
@* ✅ CORRECT *@
<SfGrid @ref="orderGrid" TValue="Order" />
<SfDialog @ref="editDialog" />

@code {
    private SfGrid<Order>? orderGrid;
    private SfDialog? editDialog;
}
```

---

## 9. Namespace Naming

| Area | Namespace |
|------|-----------|
| Public API (components, settings) | `Syncfusion.Blazor.Grids` |
| Internal modules and renderers | `Syncfusion.Blazor.Grids.Internal` |
| Event models | `Syncfusion.Blazor.Grids` |
| Enumerations | `Syncfusion.Blazor.Grids` |
| Annotations | `Syncfusion.Blazor.Grids.Internal` |

Never place public-facing types in the `Internal` namespace. Never place internal implementation types in the root namespace.

---

## 10. Quick Reference Table

| Symbol Type | Convention | Example |
|-------------|-----------|---------|
| Public class | `PascalCase` | `GridColumn`, `SfGrid<TValue>` |
| Internal class | `PascalCase` | `Sort<T>`, `DataGenerator<T>` |
| Interface | `IPascalCase` | `IGrid`, `ISfCircularComponent` |
| Enum type | `PascalCase` | `SortDirection`, `EditMode` |
| Enum member | `PascalCase` | `Ascending`, `Dialog` |
| Public property | `PascalCase` | `AllowSorting`, `EditSettings` |
| Private backing field | `_camelCase` | `_allowSorting`, `_columns` |
| Internal module property | `PascalCase` | `IsAdd`, `EditRowIndex` |
| Local variable | `camelCase` | `sortedColumns`, `rowIndex` |
| Private constant | `UPPER_SNAKE_CASE` | `VIRTUAL_ROW_THRESHOLD` |
| Public constant | `PascalCase` | `DefaultDateFormat` |
| Public async method | `PascalCaseAsync` | `SortColumnAsync` |
| Internal method | `PascalCase` | `InitiateSort`, `SaveRecord` |
| Event handler | `On[Noun][Verb]` | `OnSortHeaderClick` |
| EventCallback | `On[Action]` / noun | `OnActionBegin`, `RowSelected` |
| Generic (public) | `TValue` | `SfGrid<TValue>` |
| Generic (internal) | `T` | `Sort<T>` |
| CSS class constant | `UPPER_SNAKE_CASE` | `ASCENDING_CSS` |
| `@ref` variable | `camelCase` | `orderGrid`, `editDialog` |
| C# file | `PascalCase.cs` | `GridColumn.cs` |
| Razor file | `PascalCase.razor` | `GridHeader.razor` |
| Doc file | `kebab-case.md` | `coding-standards.md` |

---

*For coding quality rules, see [`code-guidelines/coding-standards.md`](./coding-standards.md).*
*For error handling patterns, see [`code-guidelines/error-handling.md`](./error-handling.md).*
