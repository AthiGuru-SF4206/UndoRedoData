# Performance Guidelines — Syncfusion Blazor DataGrid

> **Audience**: Architects, Senior Developers, Performance Engineers, AI Agents  
> **Prerequisite**: [`architecture/system-architecture.md`](../architecture/system-architecture.md)  
> **Related**: [`performance/optimization-techniques.md`](./optimization-techniques.md) · [`performance/benchmarks.md`](./benchmarks.md)  
> **Last Updated**: March 12, 2026

---

## Overview

Performance is a first-class concern in the Syncfusion Blazor DataGrid. The grid must remain responsive and efficient across all supported data sizes, Blazor hosting models (Server and WebAssembly), and feature combinations. This document defines the **mandatory performance targets**, **profiling methodology**, **memory management rules**, and **optimization techniques** every developer and AI agent must follow.

Failure to meet any target defined here blocks merge to `main`.

---

## 1. Performance Targets

### 1.1 Rendering Targets

| Scenario | Data Size | Target | Notes |
|----------|-----------|--------|-------|
| Initial render (standard) | 500 rows × 10 columns | **< 200 ms** | No virtualization |
| Initial render (virtual) | 100,000 rows × 10 columns | **< 110 ms** | Virtualization enabled |
| Initial render (WASM) | 500 rows × 10 columns | **< 350 ms** | AOT-compiled |
| Re-render on parameter change | Any | **< 80 ms** | Only changed modules must re-render |
| Edit row open (Normal/Dialog) | N/A | **< 50 ms** | Single row DOM mutation |
| Add new row (ShowAddNewRow) | N/A | **< 40 ms** | Persistent row prepend |
| Column show/hide | Any | **< 60 ms** | Width recalculation + CSS update |

### 1.2 Data Operation Targets

| Operation | Data Size | Target | Notes |
|-----------|-----------|--------|-------|
| Sort (client-side) | 10,000 rows | **< 50 ms** | Single column, in-memory |
| Sort (client-side, multi-column) | 10,000 rows | **< 80 ms** | Up to 3 sort columns |
| Filter (client-side) | 10,000 rows | **< 50 ms** | Single column predicate |
| Group (client-side) | 10,000 rows | **< 100 ms** | Single level |
| Page navigation | 10,000 rows | **< 30 ms** | Client-side paging |
| Edit save/update | 10,000 rows | **< 100 ms** | Includes re-render |
| Aggregate computation | 10,000 rows | **< 40 ms** | All aggregate types |
| Search | 10,000 rows | **< 60 ms** | All searchable columns |
| Row selection (single) | Any | **< 20 ms** | Single row CSS state update |
| Row selection (range) | 10,000 rows | **< 60 ms** | Shift+Click range |

### 1.3 Scroll & Virtualization Targets

| Scenario | Target | Notes |
|----------|--------|-------|
| Scroll frame rate | **≥ 60 FPS** | Virtual and infinite scroll |
| Virtual scroll row render (per scroll event) | **< 30 ms** | Rows entering viewport |
| Infinite scroll page append | **< 80 ms** | Next-page rows appended |
| Horizontal scroll (frozen + virtual) | **≥ 60 FPS** | Synchronized header/content scroll |
| Row height detection (virtual) | **< 20 ms** | First-render JS measurement |

### 1.4 Memory Targets

| Scenario | Target |
|----------|--------|
| WASM bundle size (trimmed + AOT) | **< 5 MB** (grid module contribution) |
| Memory per 10,000 rows (standard) | **< 80 MB** heap increase |
| Memory per 100,000 rows (virtual) | **< 40 MB** heap increase (only viewport rows in DOM) |
| Memory after repeated sort/filter cycles | **Zero growth** after 50 operations |
| Memory after column hide/show cycles | **Zero growth** after 20 cycles |
| DotNetObjectReference leaks | **Zero** after component dispose |
| JS event listener leaks | **Zero** after component dispose |

### 1.5 Blazor Server Circuit Targets

| Metric | Target |
|--------|--------|
| Circuit memory per user | Bounded; no unbounded accumulation |
| SignalR batch size per interaction | **< 10 KB** |
| Round-trips per user action | **≤ 2** (1 action + 1 render diff) |
| Render diff size (per page navigation) | **< 5 KB** binary diff |

---

## 2. Performance Architecture Rules

### Rule P-01 — Module-Scoped Rendering

Every action module (`Sort<T>`, `Filter<T>`, `Group<T>`, etc.) must call `StateHasChanged()` **only on the components it owns**. Calling `StateHasChanged()` on `SfGrid<TValue>` directly triggers a full tree re-render and is **prohibited** unless the entire grid layout has changed (e.g., column structure change, theme change).

```csharp
// ❌ WRONG — triggers full grid re-render
await Grid.StateHasChanged();

// ✅ CORRECT — re-renders only the content zone
await Grid.GridContent.StateHasChanged();

// ✅ CORRECT — re-renders only header (e.g., after sort indicator update)
await Grid.GridHeader.StateHasChanged();
```

### Rule P-02 — DataManager Query Optimization

`DataGenerator<T>.GenerateQuery()` must compose one `Query` object and execute it once per user action. Multiple sequential `ExecuteQuery()` calls for a single user action are prohibited.

```csharp
// ❌ WRONG — two round-trips for one sort+filter action
await DataManager.ExecuteQuery(sortQuery);
await DataManager.ExecuteQuery(filterQuery);

// ✅ CORRECT — compose all predicates into one query
var query = new Query()
    .AddParams(SortParams)
    .Where(FilterPredicates)
    .Page(PageIndex, PageSize);
await DataManager.ExecuteQuery(query);
```

### Rule P-03 — PropertyInfoHelper Caching

All `TValue` property access via reflection **must** go through `PropertyInfoHelper<TValue>`. Direct `typeof(TValue).GetProperty(name)` calls inside render loops are prohibited — they bypass the cache and add measurable overhead.

```csharp
// ❌ WRONG — uncached reflection on every row render
var value = typeof(TValue).GetProperty(column.Field)?.GetValue(rowData);

// ✅ CORRECT — cached via PropertyInfoHelper
var value = PropertyInfoHelper<TValue>.GetValue(rowData, column.Field);
```

### Rule P-04 — JS Interop Batching

Multiple JS-interop calls targeting the same user interaction **must** be batched into a single `InvokeMethod` call using a structured payload. Sequential `InvokeMethodAsync` calls within the same synchronous method are prohibited.

```csharp
// ❌ WRONG — three round-trips to JS
await JSAdaptor.InvokeMethod("setScrollLeft", scrollLeft);
await JSAdaptor.InvokeMethod("setScrollTop", scrollTop);
await JSAdaptor.InvokeMethod("updateFocus", focusArgs);

// ✅ CORRECT — one round-trip
await JSAdaptor.InvokeMethod("updateScrollAndFocus", new {
    ScrollLeft = scrollLeft,
    ScrollTop = scrollTop,
    Focus = focusArgs
});
```

### Rule P-05 — Virtualization DOM Budget

When `EnableVirtualization` or `EnableColumnVirtualization` is enabled, the number of DOM rows at any time must not exceed:

```
Max DOM rows = (ViewportHeight / MinRowHeight) × 3
```

The multiplier of `3` accounts for buffer rows above and below the viewport. Any code that appends rows beyond this budget introduces layout thrashing and violates the virtualization contract.

### Rule P-06 — Aggregate Deferred Computation

Reactive aggregates (`ReactiveAggregate<T>`) must defer computation using `Task.Delay(0)` (yield) when triggered during a batch edit cycle to avoid blocking the UI thread during multi-cell updates.

```csharp
// ✅ CORRECT — yield before heavy aggregate recompute in batch edit
await Task.Delay(0);
await RecomputeAggregates(currentViewData);
```

### Rule P-07 — ObservableCollection Coalescing

When `DataSource` is `ObservableCollection<TValue>`, the `CollectionChanged` handler must **coalesce** rapid successive events (e.g., bulk inserts) using a cancellation token pattern rather than triggering a re-render per event.

```csharp
// ✅ CORRECT — coalesce rapid ObservableCollection changes
private CancellationTokenSource? _observableCts;

private async void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
{
    _observableCts?.Cancel();
    _observableCts = new CancellationTokenSource();
    var token = _observableCts.Token;
    await Task.Delay(50, token); // coalesce window
    if (!token.IsCancellationRequested)
    {
        await RefreshDataAsync();
    }
}
```

### Rule P-08 — Frozen Column Layout Recalculation

Frozen column width recalculation must only be triggered when column structure changes (add, remove, resize, reorder). It must not be triggered on every scroll event or every `OnParametersSetAsync` cycle.

---

## 3. Memory Management Rules

### Rule M-01 — Module Disposal

Every action module that registers event listeners or holds references must implement `IDisposable` and release all resources in `Dispose()`:

```csharp
public void Dispose()
{
    EventAggregator.Unsubscribe("InitialLoad", HandleInitialLoad);
    EventAggregator.Unsubscribe("InternalDataBound", HandleDataBound);
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();
}
```

### Rule M-02 — DotNetObjectReference Lifetime

Every `DotNetObjectReference` created in a component or module **must** be tracked and disposed in `DisposeAsync()` or `Dispose()`. Orphaned `DotNetObjectReference` instances prevent garbage collection of the entire component tree.

```csharp
// ✅ CORRECT — tracked and disposed
private DotNetObjectReference<GridJSInteropAdaptor<TValue>>? _dotNetRef;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        _dotNetRef = DotNetObjectReference.Create(_jsAdaptor);
        await JSRuntime.InvokeVoidAsync("sfBlazor.Grid.initialize", _dotNetRef);
    }
}

public async ValueTask DisposeAsync()
{
    _dotNetRef?.Dispose();
}
```

### Rule M-03 — CurrentViewData Replacement

`Grid.CurrentViewData` must be **replaced** (new collection reference) after each data operation — never mutated in place. This enables Blazor's change detection to correctly diff new vs. old state without retaining stale references.

### Rule M-04 — No Static Caches for Instance Data

Module-level caches (e.g., filtered row sets, group keys) must be **instance fields**, never static dictionaries keyed by grid instance. Static caches survive component dispose and cause memory leaks when many grids are rendered/destroyed.

```csharp
// ❌ WRONG — static cache leaks across component lifetimes
private static readonly Dictionary<string, List<object>> _groupCache = new();

// ✅ CORRECT — instance field, released on Dispose
private Dictionary<string, List<object>> _groupCache = new();
```

### Rule M-05 — Large Dataset Lifecycle

When `DataSource` contains more than 50,000 rows and virtualization is disabled, the developer must receive a `[Obsolete]`-style console warning via `GridLogger` recommending `EnableVirtualization`. The grid must not silently degrade.

---

## 4. Rendering Optimization Techniques

### 4.1 ShouldRender Guard

All child renderer components that receive stable inputs (no parameter change) must override `ShouldRender()` to return `false` when their parameters are unchanged:

```csharp
/// <summary>
/// Prevents unnecessary re-renders when cell data has not changed.
/// </summary>
protected override bool ShouldRender()
{
    return _isParameterChanged;
}

public override async Task SetParametersAsync(ParameterView parameters)
{
    _isParameterChanged = parameters.DidParameterChange(nameof(RowData), RowData)
                       || parameters.DidParameterChange(nameof(Column), Column);
    await base.SetParametersAsync(parameters);
}
```

### 4.2 Template Rendering Isolation

Custom `Template` and `HeaderTemplate` razor fragments must be wrapped in isolated child components to prevent the outer grid from re-rendering when template content changes. Templates rendered directly in parent Razor markup cause cascading re-renders.

### 4.3 CascadingParameter Scope Reduction

`CascadingValue` should wrap only the component subtree that requires the value — not the entire `SfGrid.razor` output. Each broad `CascadingValue` increases Blazor's parameter propagation cost linearly with the number of descendant components.

```razor
@* ❌ WRONG — wraps entire grid causing cascading propagation *@
<CascadingValue Value="this" IsFixed="false">
    @* All 40+ child components *@
</CascadingValue>

@* ✅ CORRECT — scope to only the consumers *@
<GridContent Grid="this">
    <CascadingValue Value="_editState" IsFixed="true">
        <NormalEdit />
        <BatchEdit />
    </CascadingValue>
</GridContent>
```

Use `IsFixed="true"` whenever the cascaded value does not change after initial render.

### 4.4 Virtualization Buffer Tuning

The default virtual scroll buffer is 3× the visible row count. For datasets with frequent random access patterns (keyboard navigation with large jumps), increase the buffer to 5× but **only when `EnableVirtualization` is true** and row height is fixed.

### 4.5 Frozen Column CSS-Only Updates

Frozen column scroll synchronization must be implemented as a **CSS transform** update via JS, not as a Blazor `StateHasChanged()` trigger. Scroll is a high-frequency event (up to 60 times/second) and must never enter the .NET render pipeline.

```javascript
// ✅ CORRECT — CSS transform only, no .NET round-trip
function syncFrozenScroll(contentEl, frozenEl, scrollLeft) {
    frozenEl.style.transform = `translateX(-${scrollLeft}px)`;
}
```

---

## 5. Data Operation Optimization

### 5.1 Predicate Compilation Caching

Filter predicates built from `GridFilterColumn` definitions must be **compiled once** and cached per unique filter state fingerprint. Recompiling `Expression<Func<TValue, bool>>` on every render cycle adds significant CPU overhead for large datasets.

```csharp
// ✅ CORRECT — cache compiled predicate by filter state hash
private Func<TValue, bool>? _compiledPredicate;
private int _lastFilterHash;

private Func<TValue, bool> GetCompiledPredicate(IList<GridFilterColumn> columns)
{
    var hash = ComputeFilterHash(columns);
    if (_compiledPredicate == null || _lastFilterHash != hash)
    {
        _compiledPredicate = BuildPredicate(columns).Compile();
        _lastFilterHash = hash;
    }
    return _compiledPredicate;
}
```

### 5.2 Sort Stability

All client-side sort operations must use a **stable sort algorithm**. `List<T>.Sort()` is not stable in all .NET versions. Use `OrderBy` / `ThenBy` LINQ operators which guarantee stability, or implement a merge sort.

```csharp
// ❌ WRONG — List<T>.Sort() is not stable in all .NET targets
data.Sort((a, b) => Comparer.Compare(GetField(a), GetField(b)));

// ✅ CORRECT — stable via LINQ OrderBy
var sorted = data.OrderBy(row => GetField(row, primaryCol))
                 .ThenBy(row => GetField(row, secondaryCol))
                 .ToList();
```

### 5.3 Group Aggregation Incremental Updates

When a single row changes in batch edit, only the affected group's aggregate must be recomputed — not all groups. Modules must track which group key is affected by an edit and pass it to `ReactiveAggregate<T>.RefreshGroup(groupKey)`.

### 5.4 Paging with Server-Side DataManager

When `SfDataManager` targets a remote adaptor, the `Query` must include `Take` and `Skip` parameters set from `GridPageSettings`. Full dataset fetch followed by client-side slicing is prohibited for server-side data sources.

---

## 6. JS Interop Performance Rules

### 6.1 Module Import Once

The `sf-grid.js` module must be imported **exactly once** per grid instance via `IJSRuntime.InvokeAsync("import", ...)` during `firstRender`. Re-importing on subsequent renders wastes memory and breaks the module's internal state.

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // ✅ Import once, store reference
        _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Syncfusion.Blazor/sf-grid.js");
        await _jsAdaptor.Init(_jsModule);
    }
}
```

### 6.2 Measurement Calls on Demand

DOM measurement calls (viewport height, column widths, scroll offsets) must be made **on demand** — only when the layout changes. They must not be called in `OnAfterRenderAsync` on every render cycle unless `firstRender` is true or a structural layout change has occurred.

### 6.3 Pointer Event Delegation

All pointer/mouse event listeners in `sf-grid.js` must use **event delegation** from the grid root element — not per-row or per-cell listeners. Per-row listeners with 10,000+ rows add significant GC pressure and slow scroll performance.

```javascript
// ❌ WRONG — per-row listener
rows.forEach(row => row.addEventListener('pointerdown', handler));

// ✅ CORRECT — delegated from grid root
gridRoot.addEventListener('pointerdown', (e) => {
    const row = e.target.closest('.e-row');
    if (row) handler(e, row);
});
```

### 6.4 Scroll Throttling

The JS scroll event handler must throttle `.NET` callbacks using `requestAnimationFrame`. Raw scroll events fire at browser rate (up to 200/sec) and must never trigger `DotNetObjectReference.InvokeMethod` at that frequency.

```javascript
// ✅ CORRECT — throttled via rAF
let rafPending = false;
gridRoot.addEventListener('scroll', () => {
    if (!rafPending) {
        rafPending = true;
        requestAnimationFrame(() => {
            dotNetRef.invokeMethodAsync('OnScrolled', getScrollState());
            rafPending = false;
        });
    }
});
```

---

## 7. Blazor Server-Specific Rules

### Rule BS-01 — Minimize Render Diff Size

Every `StateHasChanged()` call on Blazor Server sends a binary render diff over SignalR. Developers must minimize the diff size by:
- Using `ShouldRender()` guards on all child components
- Updating only the affected rendering zone (content, header, footer)
- Avoiding full-grid re-renders for operations that affect a single row

### Rule BS-02 — No Synchronous Blocking in Event Handlers

All grid event handlers (`OnCellEdit`, `OnActionBegin`, `OnSortingComplete`, etc.) must be `async`. Synchronous blocking in event handlers on Blazor Server blocks the circuit thread and degrades all users sharing the server process.

```csharp
// ❌ WRONG — blocks circuit thread
private void OnSortingComplete(SortEventArgs args)
{
    Thread.Sleep(100); // Never do this
}

// ✅ CORRECT — async, non-blocking
private async Task OnSortingComplete(SortEventArgs args)
{
    await Task.Delay(0); // yield if needed
    // lightweight handler only
}
```

### Rule BS-03 — SignalR Payload Budgeting

When custom column templates or detail row templates render large HTML trees, the developer must be guided (via documentation and `GridLogger` warnings) to enable virtualization or lazy-load template content to keep per-interaction SignalR payloads under 10 KB.

---

## 8. WebAssembly-Specific Rules

### Rule WA-01 — AOT Compilation Compatibility

All grid C# code must compile cleanly under .NET AOT. Generic types, reflection calls, and `DynamicMethod` patterns that break AOT trimming must be replaced with source-generated alternatives.

### Rule WA-02 — Bundle Size Control

The grid module must not add unnecessary package references. Each new NuGet dependency must be evaluated for its contribution to WASM download size. The grid module's contribution to the trimmed WASM bundle must remain **< 5 MB**.

### Rule WA-03 — Thread-Safe DataManager

All `SfDataManager.ExecuteQuery()` calls in WASM run on the main browser thread. Developers must not create blocking `Task.Wait()` or `.Result` chains that freeze the UI thread during data fetch.

---

## 9. Performance Regression Detection

### 9.1 Regression Gate Criteria

A performance regression is defined as any change that causes:

| Metric | Regression Threshold |
|--------|---------------------|
| Initial render time | > 10% increase |
| Sort/filter operation time | > 15% increase |
| Virtual scroll frame rate | Drop below 55 FPS |
| Memory growth per 50 operations | Any measurable net growth |
| WASM bundle size | > 100 KB increase |

Any PR that introduces a regression above these thresholds is **blocked from merge** until the regression is resolved or explicitly approved by the Architect AI with documented justification.

### 9.2 Performance Sensitive Files

The following files are marked **performance-regression-sensitive**. Any modification must include before/after benchmark data in the PR:

| File | Sensitive Area |
|------|---------------|
| `Internal/Actions/Data.cs` | Query composition, data fetch, view data assignment |
| `Internal/Actions/VirtualScroll.cs` | Row buffer calculation, viewport update, DOM budget |
| `Internal/Actions/InfiniteScroll.cs` | Page append, debounce timing |
| `Internal/Actions/Sort.cs` | Comparator logic, stable sort |
| `Internal/Actions/Filter.cs` | Predicate compilation, filter application |
| `Internal/Actions/Group.cs` | Group key extraction, aggregate trigger |
| `Internal/Actions/Selection.cs` | Range selection, DOM class update |
| `Internal/Renderer/GridRow.razor` | Per-row render cycle, `ShouldRender` guard |
| `Internal/Renderer/CellRender.razor` | Per-cell render cycle, template isolation |
| `sf-grid.js` | Scroll handler, event delegation, DOM measurement |
| `SfGrid.Lifecycle.cs` | `OnParametersSetAsync` change detection cost |

### 9.3 Profiling Methodology

**Blazor Server profiling:**
1. Use the browser DevTools Performance tab to capture a timeline of a sort or filter action.
2. Identify `.NET` → JS → `.NET` round-trip costs in the flame chart.
3. Use `dotnet-trace` to collect a server-side trace during load testing.
4. Verify SignalR payload size in the Network tab (WS frames).

**Blazor WASM profiling:**
1. Enable WASM profiler: `--profiling` flag in `dotnet.js` initialization.
2. Use `dotnet-counters` to monitor GC pressure.
3. Use browser DevTools Memory tab to capture heap snapshots before/after 50 sort operations.
4. Compare heap snapshots to identify retained objects.

---

## 10. Performance Checklist for Code Review

Every PR touching any performance-sensitive file must satisfy the following checklist before approval:

### Rendering Checklist
- [ ] `ShouldRender()` overridden in all modified child components
- [ ] `StateHasChanged()` called only on the affected rendering zone
- [ ] No new `CascadingValue` wrappers added around the full grid tree
- [ ] Template fragments isolated in child components
- [ ] `IsFixed="true"` used on all immutable `CascadingValue` instances

### Data Operation Checklist
- [ ] All data operations produce a single `DataManager.ExecuteQuery()` call
- [ ] Filter predicates compiled and cached
- [ ] Sort uses stable algorithm
- [ ] Group aggregate computation is incremental where possible
- [ ] `ObservableCollection` changes are coalesced

### JS Interop Checklist
- [ ] `sf-grid.js` imported once on `firstRender`
- [ ] JS callbacks throttled via `requestAnimationFrame`
- [ ] Event listeners use delegation from grid root
- [ ] DOM measurements called on-demand only
- [ ] All listeners removed in the `dispose()` JS function

### Memory Checklist
- [ ] No `DotNetObjectReference` leaks (verified via dispose path)
- [ ] No static caches holding instance data
- [ ] All action modules implement `IDisposable` with full cleanup
- [ ] `CurrentViewData` replaced, not mutated in place

### Blazor Server Checklist
- [ ] All event handlers are `async`
- [ ] No `Thread.Sleep` or `.Result` blocking
- [ ] SignalR diff size verified < 10 KB for common interactions

### WASM Checklist
- [ ] No AOT-incompatible patterns introduced
- [ ] Bundle size contribution measured and within budget
- [ ] No blocking `.Result` / `.Wait()` on async data operations

---

## 11. Related Documents

| Document | Purpose |
|----------|---------|
| [`performance/benchmarks.md`](./benchmarks.md) | Concrete benchmark results across dataset sizes and browsers |
| [`performance/optimization-techniques.md`](./optimization-techniques.md) | 10+ advanced optimization patterns with implementation details |
| [`architecture/data-flow.md`](../architecture/data-flow.md) | Full data pipeline — prerequisite for query optimization |
| [`architecture/system-architecture.md`](../architecture/system-architecture.md) | Module map — prerequisite for understanding rendering zones |
| [`architecture/dependency-map.md`](../architecture/dependency-map.md) | Module coupling — prerequisite for isolating render scope |
| [`code-guidelines/coding-standards.md`](../code-guidelines/coding-standards.md) | Code quality rules that intersect with performance |

---

*Maintained by the Architect AI. All target revisions require Architect AI approval.*  
*Performance regressions unresolved after 3 business days escalate to the Scrum Master AI.*
