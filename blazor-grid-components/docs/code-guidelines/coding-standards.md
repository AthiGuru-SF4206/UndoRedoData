# Coding Standards — Syncfusion Blazor DataGrid

> **Audience**: All developers contributing to `SfGrid<TValue>`
> **Prerequisite**: [`tech-stack/tech-stack.md`](../tech-stack/tech-stack.md)
> **Applies to**: All `.cs`, `.razor`, `.razor.cs` files under `Syncfusion.Blazor/Grids/`
> **Last Updated**: March 10, 2026

---

## 1. Strong Typing Rules

### 1.1 Nullable Reference Types

Nullable reference types are **enabled project-wide** (`<Nullable>enable</Nullable>`). Every reference type must carry an explicit nullability annotation.

```csharp
// ❌ WRONG — reference type without nullability annotation
public string ID { get; set; }
public List<GridColumn> Columns { get; set; }

// ✅ CORRECT — explicit nullability declared
public string? ID { get; set; }
public List<GridColumn>? Columns { get; set; }
```

**Use `= null!` only** when the lifecycle guarantees initialization before first use, and add a comment explaining the guarantee:

```csharp
// ✅ CORRECT — initialized in OnInitializedAsync before any usage
private SfGrid<T> Parent { get; set; } = null!;
```

### 1.2 No `dynamic` in Public API

`dynamic` is permitted only in internal rendering logic (e.g., `DynamicInfo.cs`). It must never appear in public method signatures or public properties.

```csharp
// ❌ WRONG — dynamic in public method
public async Task UpdateRecordAsync(dynamic data) { }

// ✅ CORRECT — generic type parameter
public async Task UpdateRecordAsync(TValue data) { }
```

### 1.3 No `var` for Non-Obvious Types

Use `var` only when the right-hand side clearly reveals the type. Avoid `var` for primitives in logic-dense code.

```csharp
// ❌ WRONG — type is not obvious from right side
var result = DataModule.GetData();
var count = GetRowCount();

// ✅ CORRECT — type is self-evident or explicit
var columns = new List<GridColumn>();            // clearly List<GridColumn>
int count = GetRowCount();                       // primitive, explicit
DataResult result = DataModule.GetData();        // opaque return type, explicit
```

### 1.4 `Nullable<T>` vs `T?`

For value types, prefer the `T?` shorthand over `Nullable<T>` in code bodies. In XML documentation, always write the full type name.

```csharp
// ❌ WRONG
public Nullable<int> PageIndex { get; set; }

// ✅ CORRECT
public int? PageIndex { get; set; }
```

---

## 2. Class vs Interface Usage

### 2.1 Public API Contracts → Interfaces

All public component contracts must be expressed as interfaces. `IGrid` is the canonical example — it mirrors every public property on `SfGrid<TValue>`, enabling test mocking and circular-reference prevention.

```csharp
// ✅ CORRECT — component implements the interface
public partial class SfGrid<TValue> : SfDataBoundComponent, IGrid, ISfCircularComponent
```

### 2.2 Internal Modules → Classes (Not Interfaces)

Internal action modules (`Sort<T>`, `Edit<T>`, etc.) are concrete classes. They do not need interfaces because they are never substituted or mocked at the module level — only the parent grid is mocked via `IGrid`.

```csharp
// ✅ CORRECT — internal module is a plain class
internal class Sort<T>
{
    private SfGrid<T> Parent { get; set; }
    public Sort(SfGrid<T> parent) => Parent = parent;
}
```

### 2.3 Event Argument Models → Classes (Not Records)

Event argument models in `EventModels/Grids.cs` are mutable classes, not records, because Blazor event callbacks need mutable cancellation flags.

```csharp
// ❌ WRONG — record cannot have Cancel mutated through event pipeline
public record ActionEventArgs<T>(string RequestType, bool Cancel);

// ✅ CORRECT — mutable class with settable Cancel
public class ActionEventArgs<T>
{
    public string? RequestType { get; set; }
    public bool Cancel { get; set; }
}
```

---

## 3. Async / Await Patterns

### 3.1 Always Use `ConfigureAwait(true)`

All `await` calls within Blazor component code **must** use `.ConfigureAwait(true)` to ensure continuation on the Blazor synchronization context. Omitting this risks `StateHasChanged()` being called from a thread pool thread, causing rendering exceptions.

```csharp
// ❌ WRONG — missing ConfigureAwait
await EditModule.AddRecord(data, index);

// ✅ CORRECT — continuation on Blazor context
await EditModule.AddRecord(data, index).ConfigureAwait(true);
```

### 3.2 Lifecycle Methods Must Be `async Task`

All overridden lifecycle methods must be `async Task`, not `async void`. `async void` exceptions are unhandled and crash the application silently.

```csharp
// ❌ WRONG
protected override async void OnInitializedAsync() { }

// ✅ CORRECT
protected override async Task OnInitializedAsync() { }
```

### 3.3 `Task.Yield()` for Deferred UI Updates

Use `await Task.Yield()` before raising `DataBound` or `Created` events to allow the Blazor rendering pipeline to flush DOM updates before firing user callbacks.

```csharp
// ✅ CORRECT — yield before external event to let render complete
await Task.Yield();
await GridEvents.DataBound.InvokeAsync(EventArgs.Empty).ConfigureAwait(true);
```

### 3.4 Avoid Fire-and-Forget

Never call an async method without awaiting it inside a Blazor component lifecycle.

```csharp
// ❌ WRONG — exception will be swallowed
_ = EditModule.SaveRecord();

// ✅ CORRECT
await EditModule.SaveRecord().ConfigureAwait(true);
```

---

## 4. Parameter Declaration Pattern

Every `[Parameter]` on `SfGrid<TValue>` follows a strict four-part pattern:

```csharp
/// <summary>
/// Gets or sets a value indicating whether sorting is enabled.
/// </summary>
/// <value>
/// <c>true</c> if sorting is enabled; otherwise, <c>false</c>. The default value is <c>false</c>.
/// </value>
/// <remarks>
/// To disable sorting for a specific column, set
/// <see cref="Syncfusion.Blazor.Grids.GridColumn.AllowSorting"/> to <c>false</c>.
/// </remarks>
[Parameter]
[DefaultValue(false)]
[JsonPropertyName("allowSorting")]
public bool AllowSorting { get; set; }

private bool _allowSorting { get; set; }   // backing field for change detection
```

**Rules:**
- Every `[Parameter]` must have a corresponding private `_camelCase` backing field
- Every `[Parameter]` must carry `[DefaultValue(...)]` with the actual default
- Every `[Parameter]` must carry `[JsonPropertyName("camelCaseName")]`
- Every `[Parameter]` must have a complete XML doc block (see Section 8)

---

## 5. Property Change Detection Pattern

The `OnParametersSetAsync` method detects changes via `PropertyChanges` (inherited from `SfDataBoundComponent`). When adding a new parameter, register it with `UpdateProperty`:

```csharp
// ✅ CORRECT — inside OnHybridParametersSet (called by OnParametersSetAsync)
await UpdateProperty(nameof(AllowSorting), _allowSorting, AllowSorting).ConfigureAwait(true);
_allowSorting = AllowSorting;
```

Never manually compare parameter values outside of this pattern — it breaks the centralized change tracking.

---

## 6. Module Access Pattern (Service Locator)

Internal modules access sibling modules through the `Parent` grid reference. Always null-check module access since modules can be conditionally null in partial grid scenarios.

```csharp
// ❌ WRONG — no null check
var sortedCols = Parent.SortModule.SortedColumns;

// ✅ CORRECT — null-conditional access
var sortedCols = Parent.SortModule?.SortedColumns;
```

Never store a reference to a sibling module in a local field — always read via `Parent`:

```csharp
// ❌ WRONG — stale reference if module is replaced
private Sort<T> _sortRef = Parent.SortModule!;

// ✅ CORRECT — always resolve through Parent at call time
await Parent.SortModule?.SortColumn(field, direction).ConfigureAwait(true);
```

---

## 7. LINQ Usage

### 7.1 Prefer Method Syntax

Use LINQ method syntax over query syntax for consistency with the existing codebase.

```csharp
// ❌ WRONG — query syntax
var result = from col in columns where col.AllowSorting select col;

// ✅ CORRECT — method syntax
var result = columns.Where(col => col.AllowSorting);
```

### 7.2 Avoid Multiple Enumeration

Never enumerate an `IEnumerable<T>` more than once without materializing it first.

```csharp
// ❌ WRONG — double enumeration
if (columns.Any() && columns.Count() > 1) { }

// ✅ CORRECT — materialize once
var cols = columns.ToList();
if (cols.Count > 1) { }
```

### 7.3 `FirstOrDefault` vs `First`

Use `FirstOrDefault` and null-check the result. Never use `First` in production code paths — it throws on empty sequences.

```csharp
// ❌ WRONG
var col = columns.First(c => c.Field == fieldName);

// ✅ CORRECT
var col = columns.FirstOrDefault(c => c.Field == fieldName);
if (col == null) { return; }
```

---

## 8. XML Documentation Standards

All public and `internal` API members **must** be documented. The documentation is compiled into the NuGet package XML file consumed by IntelliSense.

### 8.1 `[Parameter]` Properties

```csharp
/// <summary>
/// Gets or sets [what it controls — one sentence].
/// </summary>
/// <value>
/// [Describe accepted value type and default].
/// </value>
/// <remarks>
/// [Cross-references to related properties or methods using <see cref="..."/>].
/// </remarks>
[Parameter]
public bool AllowPaging { get; set; }
```

### 8.2 Public Methods

```csharp
/// <summary>
/// [Verb phrase describing what the method does — one sentence].
/// </summary>
/// <param name="columnName">
/// [Description of what this parameter represents and valid values].
/// </param>
/// <param name="direction">
/// [Description with enum value references].
/// </param>
/// <returns>
/// A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.
/// </returns>
/// <remarks>
/// [Pre-conditions, side effects, and related APIs].
/// </remarks>
/// <example>
/// <code><![CDATA[
/// await grid.SortColumnAsync("OrderID", SortDirection.Ascending);
/// ]]></code>
/// </example>
public async Task SortColumnAsync(string columnName, SortDirection direction) { }
```

### 8.3 Enumerations

```csharp
/// <summary>
/// Defines the [what the enum controls].
/// <list type="bullet">
/// <item><term>ValueA</term><description>What ValueA means.</description></item>
/// <item><term>ValueB</term><description>What ValueB means.</description></item>
/// </list>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MyEnum
{
    /// <summary>Description of ValueA.</summary>
    [EnumMember(Value = "ValueA")]
    ValueA,
}
```

### 8.4 Internal Classes and Methods

```csharp
/// <summary>
/// Handles [module responsibility — one sentence].
/// </summary>
/// <typeparam name="T">The grid model type (<c>TValue</c>).</typeparam>
internal class Sort<T> { }
```

### 8.5 Forbidden Comment Patterns

```csharp
// ❌ WRONG — restates the code, adds no value
// Loop through columns
foreach (var col in columns) { }

// ❌ WRONG — commented-out dead code
// var oldResult = GetOldData();

// ❌ WRONG — TODO without task reference
// TODO: Fix this later

// ✅ CORRECT — explains non-obvious intent or links to task
// Query comparison is forced here to avoid Hierarchy Grid re-render on reference equality.
// See: https://dev.azure.com/EssentialStudio/Ej2-Web/_workitems/edit/XXXXXX
```

---

## 9. Razor Component Standards

### 9.1 Code-Behind Pattern

Components with significant logic must use a code-behind file, not inline `@code { }` blocks.

```
GridHeader.razor       ← markup only
GridHeader.razor.cs    ← logic, parameters, lifecycle
```

### 9.2 Parameter Binding

```razor
// ❌ WRONG — string formatting for boolean attributes
<GridRow Visible="@("true")">

// ✅ CORRECT — strongly typed binding
<GridRow Visible="@isRowVisible">
```

### 9.3 Null Propagation in Razor

Always null-check before rendering child content that depends on a nullable parameter.

```razor
@* ❌ WRONG — NullReferenceException if Columns is null *@
@foreach (var col in Grid.Columns) { }

@* ✅ CORRECT — null guard *@
@if (Grid.Columns != null)
{
    @foreach (var col in Grid.Columns) { }
}
```

### 9.4 `@key` Directive

Use `@key` for all list-rendering loops to prevent DOM identity loss during reconciliation.

```razor
@* ❌ WRONG — Blazor may reuse wrong DOM nodes during diff *@
@foreach (var row in Rows) { <GridRow Row="@row" /> }

@* ✅ CORRECT *@
@foreach (var row in Rows) { <GridRow @key="@row.UID" Row="@row" /> }
```

---

## 10. IDisposable / Cleanup

All components and modules that register event handlers, JS interop callbacks, or `ObservableCollection` listeners **must** implement `IAsyncDisposable` and deregister in `DisposeAsync`.

```csharp
// ✅ CORRECT pattern
public async ValueTask DisposeAsync()
{
    await _jsAdaptor.DisposeAsync().ConfigureAwait(true);
    UpdateObservableEvents(nameof(DataSource), DataSource, remove: true);
    GC.SuppressFinalize(this);
}
```

Never unregister events in `Dispose()` when the component is used in Blazor — always `DisposeAsync()`.

---

## 11. String Comparison

Always pass `StringComparison` explicitly. Default comparison behavior differs across platforms.

```csharp
// ❌ WRONG — culture-sensitive comparison
if (column.Field == fieldName) { }
if (className.Contains("e-ascending")) { }

// ✅ CORRECT — ordinal, case-sensitive
if (string.Equals(column.Field, fieldName, StringComparison.Ordinal)) { }
if (className.Contains("e-ascending", StringComparison.Ordinal)) { }
```

---

## 12. Magic Numbers and Constants

Avoid inline numeric/string literals in logic. Use named private constants or well-named variables.

```csharp
// ❌ WRONG
if (rows.Count > 50) { EnableVirtualMode(); }
string cls = "e-grid e-responsive";

// ✅ CORRECT
private const int VIRTUAL_ROW_THRESHOLD = 50;
private const string BASE_CSS_CLASS = "e-grid e-responsive";

if (rows.Count > VIRTUAL_ROW_THRESHOLD) { EnableVirtualMode(); }
string cls = BASE_CSS_CLASS;
```

---

## 13. Error Guard Pattern

Guard clauses must appear at the top of methods, before any state mutation.

```csharp
// ❌ WRONG — guard after state mutation
public async Task SortColumnAsync(string field, SortDirection direction)
{
    Parent.IsColumnHeaderChange = true;  // ← state mutated before guard
    var col = GridUtils.GetColumnByField(field, columns);
    if (col == null) { return; }
}

// ✅ CORRECT — guard first, mutate after
public async Task SortColumnAsync(string field, SortDirection direction)
{
    var col = GridUtils.GetColumnByField(field, columns);
    if (col == null || !Parent.AllowSorting || !col.AllowSorting)
    {
        return;
    }

    Parent.IsColumnHeaderChange = true;
}
```

---

## 14. Action Event Pipeline

All grid user-facing operations must raise `OnActionBegin` before execution and `OnActionComplete` after, and must respect the `Cancel` flag.

```csharp
// ✅ CORRECT — full pipeline
var args = new ActionEventArgs<T> { RequestType = "sorting", Cancel = false };
await Parent.GridEvents!.OnActionBegin.InvokeAsync(args).ConfigureAwait(true);
if (args.Cancel) { return; }

// ... perform operation ...

await Parent.GridEvents!.OnActionComplete.InvokeAsync(args).ConfigureAwait(true);
```

---

## 15. Performance-Sensitive Code Rules

- **Do not call `StateHasChanged()` inside loops** — batch changes and call once.
- **Do not allocate inside hot render paths** — avoid `new List<T>()` inside `BuildRenderTree` overrides.
- **Do not use reflection in per-row render paths** — use cached `PropertyInfo` from `PropertyInfoHelper<T>`.
- **Do not read `DateTime.Now` or `Guid.NewGuid()` in render methods** — cache them before the render cycle.

---

*For naming rules, see [`code-guidelines/naming-conventions.md`](./naming-conventions.md).*
*For error handling patterns, see [`code-guidelines/error-handling.md`](./error-handling.md).*
*For logging rules, see [`code-guidelines/logging-guidelines.md`](./logging-guidelines.md).*
