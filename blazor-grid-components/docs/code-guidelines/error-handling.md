# Error Handling — Syncfusion Blazor DataGrid

> **Audience**: All developers contributing to `SfGrid<TValue>`
> **Prerequisite**: [`code-guidelines/coding-standards.md`](./coding-standards.md)
> **Last Updated**: March 10, 2026

---

## Overview

The DataGrid is a long-lived, interactive component. Poor error handling causes silent data corruption, broken UI state, or uncatchable exceptions that crash the Blazor circuit (Server) or the WASM application. These patterns are mandatory for all code paths.

---

## 1. Guard Clauses — Input Validation

### 1.1 Parameter Guards on Public Methods

All public methods on `SfGrid<TValue>` must validate inputs at the top of the method body, before any state mutation. Use early-return guards rather than deep nesting.

```csharp
// ❌ WRONG — state mutated before guard; nested validation
public async Task SortColumnAsync(string columnName, SortDirection direction)
{
    Parent.IsColumnHeaderChange = true;
    if (columnName != null)
    {
        var col = GetColumn(columnName);
        if (col != null && col.AllowSorting) { /* ... */ }
    }
}

// ✅ CORRECT — guard first, flat structure
public async Task SortColumnAsync(string columnName, SortDirection direction)
{
    if (string.IsNullOrEmpty(columnName))
    {
        return;
    }

    var col = GridUtils.GetColumnByField(columnName, GridUtils.GetColumns(this));
    if (col == null || !AllowSorting || !col.AllowSorting)
    {
        return;
    }

    IsColumnHeaderChange = true;
    await SortModule!.SortColumn(columnName, direction).ConfigureAwait(true);
}
```

### 1.2 Null Guards on Module Access

Internal modules may be `null` during partial initialization (e.g., when `IsLoaded` is false). Always null-check before calling a module method.

```csharp
// ❌ WRONG — NullReferenceException if EditModule not yet initialized
await EditModule.AddRecord(data, index).ConfigureAwait(true);

// ✅ CORRECT — null guard
if (EditModule == null) { return; }
await EditModule.AddRecord(data, index).ConfigureAwait(true);
```

### 1.3 Edit Permission Guards

CRUD methods must check the corresponding `EditSettings` flag before proceeding. This prevents silent no-op operations that confuse callers.

```csharp
// ✅ CORRECT — from SfGrid.Methods.cs pattern
public async Task AddRecordAsync(TValue data, int? index = null)
{
    if (EditSettings != null && !EditSettings.AllowAdding)
    {
        return;
    }

    await EditModule!.AddRecord(data!, index).ConfigureAwait(true);
}

public async Task DeleteRecordAsync(string fieldName, TValue data)
{
    if (EditSettings != null && !EditSettings.AllowDeleting)
    {
        return;
    }

    await EditModule!.DeleteRecord(fieldName, data).ConfigureAwait(true);
}
```

---

## 2. Null and Reference Checks

### 2.1 Null-Conditional Operator

Use `?.` for single access chains where null is a valid non-error state.

```csharp
// ✅ CORRECT — null is expected, not an error
var groupCols = Parent.GroupSettings?.Columns?.ToList() ?? new List<string>();
var sortCols = Parent.SortSettings?.Columns;
```

### 2.2 Null-Coalescing Defaults

Provide safe defaults with `??` rather than branching.

```csharp
// ❌ WRONG — branching for a simple default
List<string> cols;
if (Parent.GroupSettings?.Columns == null)
    cols = new List<string>();
else
    cols = Parent.GroupSettings.Columns.ToList();

// ✅ CORRECT — null-coalescing default
var cols = Parent.GroupSettings?.Columns?.ToList() ?? new List<string>();
```

### 2.3 Null-Forgiving Operator (`!`)

Use `!` only when you can prove the value is non-null by lifecycle contract. Add a comment justifying the assertion.

```csharp
// ✅ CORRECT — EditModule guaranteed non-null after OnInitializedAsync
await EditModule!.AddRecord(data!, index).ConfigureAwait(true);

// ❌ WRONG — no justification; hides a real potential null
var result = _columns!.FirstOrDefault(c => c.Field == field);
```

---

## 3. Try-Catch Usage

### 3.1 Scope Rule

**Do not use try-catch inside internal module methods.** Exceptions from internal logic should propagate to the public API boundary where they can be caught with full context and surfaced to the developer.

```csharp
// ❌ WRONG — silently swallows exceptions inside a module
internal async Task SaveRecord()
{
    try
    {
        await Parent.DataModule!.UpdateRecord(record).ConfigureAwait(true);
    }
    catch { }   // ← hides bugs
}
```

### 3.2 Catch Specific Exceptions at the Boundary

Catch only at the public method boundary, and only the specific exception types you can meaningfully handle.

```csharp
// ✅ CORRECT — specific exception at the public boundary
public async Task ExportToExcelAsync(ExcelExportProperties? properties = null)
{
    try
    {
        await ExcelExportModule!.Export(FlatColumns, CurrentViewData, properties)
            .ConfigureAwait(true);
    }
    catch (InvalidOperationException ex)
    {
        // Export requires at least one visible column — surface to developer
        throw new InvalidOperationException(
            $"SfGrid [{ID}]: Excel export failed. Ensure at least one column is visible.", ex);
    }
}
```

### 3.3 Never Swallow Exceptions

A bare `catch { }` or `catch (Exception) { }` with no re-throw is prohibited in all production code paths.

```csharp
// ❌ WRONG
try { await SomeOperation(); }
catch { }

// ❌ WRONG — catches and discards
catch (Exception) { return; }

// ✅ CORRECT — log and rethrow, or handle specifically
catch (OperationCanceledException)
{
    // Cancellation is expected during component disposal — do not rethrow
    return;
}
```

---

## 4. ActionArgs Cancellation Pattern

All grid actions that support cancellation must check `args.Cancel` after raising `OnActionBegin`. Failing to check `Cancel` bypasses developer-configured validation.

```csharp
// ✅ CORRECT — full cancellable pipeline
internal async Task InitiateDelete(TValue rowData)
{
    var args = new ActionEventArgs<T>
    {
        RequestType = "delete",
        Data = rowData,
        Cancel = false
    };

    await Parent.GridEvents!.OnActionBegin.InvokeAsync(args).ConfigureAwait(true);

    if (args.Cancel)
    {
        return;   // ← developer cancelled the operation — stop here
    }

    await PerformDelete(rowData).ConfigureAwait(true);

    await Parent.GridEvents!.OnActionComplete.InvokeAsync(args).ConfigureAwait(true);
}
```

---

## 5. Observable Collection Error Handling

When wiring `INotifyCollectionChanged` events, wrap the handler body in a try-catch that logs and disconnects a faulty handler, preventing a broken data source from crashing the grid circuit.

```csharp
// ✅ CORRECT
private void OnObservableCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
{
    try
    {
        InvokeAsync(async () =>
        {
            await RefreshDataAsync().ConfigureAwait(true);
        });
    }
    catch (ObjectDisposedException)
    {
        // Component disposed before the event fired — safe to ignore
    }
}
```

---

## 6. JS Interop Error Handling

JS interop calls can throw `JSException` when the JS module is not yet loaded or the component has been disposed. All `InvokeMethod` calls must account for disposal state.

```csharp
// ✅ CORRECT
internal async Task InvokeMethod(string methodName, params object[] args)
{
    if (_isDisposed)
    {
        return;
    }

    try
    {
        await JSRuntime.InvokeVoidAsync(methodName, args).ConfigureAwait(true);
    }
    catch (JSDisconnectedException)
    {
        // Circuit disconnected — component is being torn down, safe to ignore
    }
    catch (TaskCanceledException)
    {
        // Navigation or disposal cancelled the task — safe to ignore
    }
}
```

---

## 7. DataManager Error Handling

Remote data failures (network errors, 500 responses) surface as exceptions from `SfDataManager.ExecuteQuery`. Catch them at the `DataProcess` level and raise `OnDataSourceChanged` with an empty result rather than leaving the grid in an indeterminate state.

```csharp
// ✅ CORRECT pattern in DataGenerator<T>
internal async Task<DataResult> FetchData(Query query)
{
    try
    {
        return await Parent.DataManager.ExecuteQuery<T>(query).ConfigureAwait(true);
    }
    catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
    {
        // Surface to developer via OnActionFailure if wired, then return empty
        await Parent.GridEvents?.OnActionFailure
            .InvokeAsync(new FailureEventArgs { Error = ex })
            .ConfigureAwait(true);

        return new DataResult { Result = Array.Empty<T>(), Count = 0 };
    }
}
```

---

## 8. Disposal State Guards

After `DisposeAsync()` is called, any pending async callbacks (JS interop, observable events) may still fire. Guard against post-disposal calls.

```csharp
// ✅ CORRECT
private bool _isDisposed;

public async ValueTask DisposeAsync()
{
    _isDisposed = true;
    await _jsAdaptor.DisposeAsync().ConfigureAwait(true);
    GC.SuppressFinalize(this);
}

private async Task RefreshDataAsync()
{
    if (_isDisposed) { return; }
    // ... safe to proceed
}
```

---

## 9. Error Recovery Strategies

| Failure Scenario | Recovery Strategy |
|-----------------|------------------|
| Data source returns null | Treat as empty: `CurrentViewData = Array.Empty<TValue>()` |
| `OnActionBegin` cancels operation | Return early; restore pre-action UI state |
| JS interop disconnected | Swallow `JSDisconnectedException`; set `_isDisposed = true` |
| Edit form validation failure | Surface via `ValidationDialog`; do not save |
| Remote data fetch failure | Raise `OnActionFailure`; render empty grid body |
| Observable collection changed during render | Defer update via `InvokeAsync` to next render cycle |
| Column field not found | Return null; log warning via `GridLogger`; do not throw |

---

## 10. Error Boundary in Blazor

The grid does not implement `ErrorBoundary` internally — it is the host application's responsibility to wrap `<SfGrid>` with an `<ErrorBoundary>` component in critical scenarios. Internal exceptions that escape the component should be treated as bugs — file a bug report per the requirements workflow.

---

*For coding rules, see [`code-guidelines/coding-standards.md`](./coding-standards.md).*
*For logging patterns, see [`code-guidelines/logging-guidelines.md`](./logging-guidelines.md).*
