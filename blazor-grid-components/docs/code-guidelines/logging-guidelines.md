# Logging Guidelines — Syncfusion Blazor DataGrid

> **Audience**: All developers contributing to `SfGrid<TValue>`
> **Prerequisite**: [`code-guidelines/error-handling.md`](./error-handling.md)
> **Last Updated**: March 10, 2026

---

## Overview

Logging in the DataGrid serves two distinct purposes:

1. **Developer diagnostics** — surfacing misconfiguration, deprecation warnings, and invalid API usage to the consuming developer's console at development time.
2. **Internal diagnostics** — tracing data flow and lifecycle events for debugging during development (never shipped to production consumers as noise).

The grid does **not** use a general-purpose logging framework (e.g., `Microsoft.Extensions.Logging`). It uses the Syncfusion-internal `SyncfusionLogger` / `GridLogger` utility from `Syncfusion.Blazor.Core`, which routes messages to the browser console via JS interop in development mode and is a no-op in release builds.

---

## 1. Logger Interface

```csharp
/// <summary>
/// Provides logging utilities for the DataGrid component.
/// </summary>
internal static class GridLogger
{
    /// <summary>
    /// Logs a developer-facing warning to the browser console.
    /// Only active when the host application runs in Development environment.
    /// </summary>
    /// <param name="gridId">The ID of the grid instance emitting the warning.</param>
    /// <param name="message">The warning message.</param>
    internal static void Warn(string gridId, string message) { }

    /// <summary>
    /// Logs a developer-facing error to the browser console.
    /// </summary>
    /// <param name="gridId">The ID of the grid instance emitting the error.</param>
    /// <param name="message">The error message.</param>
    internal static void Error(string gridId, string message) { }

    /// <summary>
    /// Logs an informational trace message. Only active in DEBUG builds.
    /// </summary>
    /// <param name="gridId">The ID of the grid instance.</param>
    /// <param name="message">The trace message.</param>
    [System.Diagnostics.Conditional("DEBUG")]
    internal static void Trace(string gridId, string message) { }
}
```

---

## 2. Log Levels

| Level | Method | When to Use | Shipped? |
|-------|--------|------------|---------|
| `TRACE` | `GridLogger.Trace(...)` | Lifecycle entry/exit, data flow checkpoints — debug only | ❌ No-op in Release |
| `WARN` | `GridLogger.Warn(...)` | Deprecated API usage, invalid configuration, fallback behavior activated | ✅ Development only |
| `ERROR` | `GridLogger.Error(...)` | Invalid API combination that will cause incorrect behavior | ✅ Development only |

> There is no `INFO` or `FATAL` level in the grid logger. `INFO` is too noisy for a component library. `FATAL` is handled by .NET exceptions, not logging.

---

## 3. When to Log

### 3.1 Log a WARN

| Scenario | Example Message |
|----------|----------------|
| Deprecated property used | `"SfGrid [ID]: 'FrozenColumns' is deprecated. Use 'FrozenLeftColumns' instead."` |
| Required peer property missing | `"SfGrid [ID]: 'AllowGrouping' is true but 'GroupSettings' is null. Default GroupSettings applied."` |
| Invalid column field reference | `"SfGrid [ID]: Column field 'NonExistentField' not found in data source. Filter ignored."` |
| Feature conflict | `"SfGrid [ID]: 'EnableVirtualization' and 'AllowGrouping' are both enabled. Grouping is disabled when virtualization is active."` |

```csharp
// ✅ CORRECT — warn on deprecated property access
if (FrozenColumns > 0)
{
    GridLogger.Warn(ID ?? string.Empty,
        "FrozenColumns is deprecated. Use FrozenLeftColumns to freeze columns from the left side.");
}
```

### 3.2 Log an ERROR

| Scenario | Example Message |
|----------|----------------|
| `SortColumnAsync` called on non-existent field | `"SfGrid [ID]: SortColumnAsync failed. Column 'OrderDate' does not exist."` |
| Edit attempted with no `EditSettings` | `"SfGrid [ID]: AddRecordAsync called but EditSettings is null. Configure <GridEditSettings> to enable editing."` |
| Export triggered with no data | `"SfGrid [ID]: ExportToExcelAsync triggered with empty CurrentViewData. Export aborted."` |

```csharp
// ✅ CORRECT — error on invalid method usage
public async Task SortColumnAsync(string columnName, SortDirection direction)
{
    var col = GridUtils.GetColumnByField(columnName, GridUtils.GetColumns(this));
    if (col == null)
    {
        GridLogger.Error(ID ?? string.Empty,
            $"SortColumnAsync failed. Column '{columnName}' does not exist in the grid.");
        return;
    }
    // ...
}
```

### 3.3 Log a TRACE (Debug Only)

Use `TRACE` sparingly for lifecycle and data flow diagnostics. These are stripped from Release builds via the `[Conditional("DEBUG")]` attribute.

```csharp
// ✅ CORRECT — trace data pipeline entry
[System.Diagnostics.Conditional("DEBUG")]
internal async Task DataProcess()
{
    GridLogger.Trace(Parent.ID ?? string.Empty, "DataProcess: query generation started.");
    var query = GenerateQuery();
    GridLogger.Trace(Parent.ID ?? string.Empty, $"DataProcess: query built — Page {query.Page}, Sort {query.SortedColumns?.Count ?? 0} columns.");
    // ...
}
```

---

## 4. Log Message Format

All log messages follow this structure:

```
SfGrid [{GridID}]: {Message}. {Context if applicable.}
```

| Element | Rule |
|---------|------|
| Prefix | Always `SfGrid [{ID}]:` — identifies the source component |
| Message | Sentence case, ends with a period |
| Context | Actionable: tell the developer **what to do** to fix it |
| No stack traces | Never log exception stack traces — .NET runtime handles that |
| No PII | Never log data values, row contents, or user input |

```csharp
// ❌ WRONG — no prefix, no context, no action guidance
GridLogger.Warn("AllowFiltering is false");

// ❌ WRONG — logs data value (potential PII)
GridLogger.Warn(ID, $"Filter value '{filterValue}' is invalid.");

// ✅ CORRECT — prefixed, actionable, no data
GridLogger.Warn(ID ?? string.Empty,
    "FilterByColumnAsync called but AllowFiltering is false. Set AllowFiltering=\"true\" on SfGrid to enable filtering.");
```

---

## 5. Where to Place Log Calls

### 5.1 Public Method Entry (WARN/ERROR only)

Log before the guard-return when the developer has misconfigured the API.

```csharp
public async Task AddRecordAsync(TValue data, int? index = null)
{
    if (EditSettings == null || !EditSettings.AllowAdding)
    {
        GridLogger.Warn(ID ?? string.Empty,
            "AddRecordAsync called but AllowAdding is false or EditSettings is null.");
        return;
    }
    // ...
}
```

### 5.2 Module Initialization (TRACE only)

Log module construction in debug builds to trace initialization order issues.

```csharp
// In SfGrid.Lifecycle.cs — OnInitializedAsync
GridLogger.Trace(ID ?? string.Empty, "OnInitializedAsync: all modules instantiated.");
```

### 5.3 Data Pipeline Checkpoints (TRACE only)

```csharp
// ✅ CORRECT — trace inside DataGenerator<T>
GridLogger.Trace(Parent.ID ?? string.Empty,
    $"GenerateQuery: filters={filterCount}, sorts={sortCount}, page={pageIndex}.");
```

---

## 6. What NOT to Log

| Do NOT log | Reason |
|-----------|--------|
| Data values (`row.CustomerID`, filter values) | Potential PII exposure |
| Every property change | Excessive noise in developer console |
| `StateHasChanged()` calls | Too frequent; use TRACE only if diagnosing render loops |
| Successful happy-path operations | No diagnostic value |
| Exception stack traces | .NET handles this; avoid double-logging |
| Internal render decisions | Too noisy for production consumers |

---

## 7. Logging in Tests

Unit tests must not assert on log output — log calls are side effects, not behavior. Test the functional outcome instead.

```csharp
// ❌ WRONG — asserting on log side effect
Assert.Contains("AllowAdding is false", capturedLogs);

// ✅ CORRECT — assert on actual behavior
await grid.AddRecordAsync(newRecord);
Assert.Equal(originalCount, grid.CurrentViewData.Count()); // record was not added
```

---

## 8. Remote Logging

The DataGrid does **not** send telemetry or logs to any remote endpoint. All logging is local to the browser console (via JS interop) or the .NET debug output. No logging integration with Application Insights, Sentry, or any third-party monitoring service is implemented at the component level. Host applications may capture exceptions via their own error handling pipeline.

---

## 9. Performance Considerations

- All `TRACE` calls are stripped at compile time via `[Conditional("DEBUG")]` — zero cost in Release.
- `WARN` and `ERROR` calls perform a JS interop invoke — never place them inside hot render loops or per-row operations.
- `GridLogger.Warn` and `GridLogger.Error` are no-ops when the application is not in Development environment — confirmed via `SyncfusionService.IsDevEnv`.

```csharp
// ❌ WRONG — WARN inside per-row render loop
foreach (var row in Rows)
{
    GridLogger.Warn(ID, "Rendering row...");  // ← catastrophic performance
}

// ✅ CORRECT — WARN only on configuration issues, outside render loops
if (!AllowFiltering && FilterSettings?.Columns?.Count > 0)
{
    GridLogger.Warn(ID ?? string.Empty,
        "FilterSettings.Columns contains values but AllowFiltering is false. Filters will not be applied.");
}
```

---

*For error handling patterns, see [`code-guidelines/error-handling.md`](./error-handling.md).*
*For coding standards, see [`code-guidelines/coding-standards.md`](./coding-standards.md).*
