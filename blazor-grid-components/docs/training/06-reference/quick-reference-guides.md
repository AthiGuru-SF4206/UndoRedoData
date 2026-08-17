# Quick Reference Guides — Syncfusion Blazor DataGrid

> **Audience**: All DataGrid developers — use this daily  
> **Module**: 06 — Reference  
> **Last Updated**: March 12, 2026

---

## 1. Module Quick Reference

| Module | Class | File | Feature |
|--------|-------|------|---------|
| Sort | `Sort<TValue>` | `Internal/Actions/Sort.cs` | Column sorting |
| Filter | `Filter<TValue>` | `Internal/Actions/Filter.cs` | Filter bar, Excel, Menu, Checkbox |
| Group | `Group<TValue>` | `Internal/Actions/Group.cs` | Column grouping |
| Edit | `Edit<TValue>` | `Internal/Actions/Edit.cs` | CRUD editing (Normal, Dialog, Batch) |
| Selection | `Selection<TValue>` | `Internal/Actions/Selection.cs` | Row, cell, checkbox selection |
| VirtualScroll | `VirtualScroll<TValue>` | `Internal/Actions/VirtualScroll.cs` | Row and column virtualization |
| InfiniteScroll | `InfiniteScroll<TValue>` | `Internal/Actions/InfiniteScroll.cs` | On-demand loading |
| FocusHandler | `FocusHandler<TValue>` | `Internal/Actions/FocusHandler.cs` | Keyboard focus management |
| Reorder | `Reorder<TValue>` | `Internal/Actions/Reorder.cs` | Column drag-and-drop reorder |
| RowReorder | `RowReorder<TValue>` | `Internal/Actions/RowReorder.cs` | Row drag-and-drop |
| ForeignKey | `ForeignKey<TValue>` | `Internal/Actions/ForeignKey.cs` | Foreign key column lookup |
| DetailRow | `DetailRow<TValue>` | `Internal/Actions/DetailRow.cs` | Expandable detail rows |
| ReactiveAggregate | `ReactiveAggregate<TValue>` | `Internal/Actions/ReactiveAggregate.cs` | Live aggregate calculation |
| MergeHandler | `MergeHandler<TValue>` | `Internal/Actions/MergeHandler.cs` | Auto cell spanning |

---

## 2. Key Public Files Reference

| File | Purpose |
|------|---------|
| `SfGrid.Properties.cs` | All `[Parameter]` properties — never modify without API review |
| `SfGrid.Methods.cs` | All public `async` API methods |
| `SfGrid.Lifecycle.cs` | `OnInitializedAsync`, `OnAfterRenderAsync`, `SetParametersAsync`, `Dispose` |
| `SfGrid.razor.cs` | Main component entry point |
| `Internal/SfGrid.razor` | Root render shell |
| `sf-grid.js` | All JS-side DOM operations |
| `Enumeration/GridsEnumerations.cs` | All public enums |
| `EventModels/Grids.cs` | All public event argument models |
| `Interfaces/IGrid.cs` | Public grid interface contract |
| `Internal/Base/GridJSInteropAdaptor.cs` | JS-interop bridge |
| `Internal/Actions/Data.cs` | `DataGenerator<TValue>` — data pipeline |

---

## 3. Naming Conventions Quick Reference

| Element | Convention | Example |
|---------|-----------|---------|
| Class | PascalCase | `DataGenerator<TValue>` |
| Interface | `I` + PascalCase | `IGrid`, `IActionModule` |
| Public method | PascalCase | `GetSortedColumnsAsync()` |
| Private method | PascalCase | `CalculateRowRange()` |
| Local variable | camelCase | `rowIndex`, `editableColumns` |
| Private field | `_` + camelCase | `_currentEditRowIndex` |
| Parameter | camelCase | `columnIndex`, `args` |
| Constant | UPPER_SNAKE_CASE | `MAX_PAGE_SIZE` |
| Event callback | Microsoft standard: Verb + Noun | `OnRowSelected`, `OnActionBegin` |
| `[Parameter]` property | PascalCase | `AllowSorting`, `EnableVirtualization` |
| Razor component file | PascalCase | `GridRowRenderer.razor` |
| C# source file | PascalCase | `Sort.cs`, `VirtualScroll.cs` |
| Documentation file | kebab-case | `system-architecture.md` |

---

## 4. EventAggregator Events Reference

| Event Name | Fired By | Listened By | When |
|-----------|----------|------------|------|
| `InitialLoad` | `SfGrid` lifecycle | `DataGenerator` | First render, trigger initial data fetch |
| `DataBound` | `DataGenerator` | `ReactiveAggregate`, `Selection`, `FocusHandler` | After data fetch completes |
| `InternalDataBound` | `DataGenerator` | Renderers | After data render completes |
| `ActionBegin` | Action modules | `SfGrid` (fires public event) | Before sort, filter, page action |
| `ActionComplete` | Action modules | `SfGrid` (fires public event) | After sort, filter, page action |
| `VirtualComponentUpdate` | `VirtualScroll` | `DataGenerator` | After row height detection, re-fetch with correct range |
| `EditBegin` | `Edit<T>` | `FocusHandler` | When edit mode starts on a row |
| `EditComplete` | `Edit<T>` | `Selection`, `FocusHandler` | When edit mode ends |
| `ColumnStateChange` | `Reorder`, `Resize` | Renderers | After column reorder or resize completes |

---

## 5. Regression Risk Reference

### High-Risk Feature Combinations

| Combination | Risk Level | What to Test |
|-------------|-----------|-------------|
| Virtualization + Editing | 🔴 High | Add-new row DOM stability on CRUD ops |
| Grouping + Selection | 🔴 High | Selection index calculation with group rows |
| Grouping + Editing | 🔴 High | Add-new row position, Tab navigation past groups |
| Frozen Columns + Virtualization | 🔴 High | Scroll sync between frozen and movable containers |
| Frozen Columns + Column Reorder | 🔴 High | Reorder boundaries at freeze zone |
| Infinite Scroll + Editing | 🔴 High | Cache eviction must not remove edited rows |
| Batch Edit + Aggregates | 🔴 High | Aggregates must recalculate on every cell change |

### Medium-Risk Feature Combinations

| Combination | Risk Level | What to Test |
|-------------|-----------|-------------|
| Paging + Grouping | 🟡 Medium | Group boundaries across page breaks |
| Sort + Filter | 🟡 Medium | Operation order: filter first, sort after |
| Export + Column Templates | 🟡 Medium | Templates must evaluate to plain text |
| Column Resize + Frozen Columns | 🟡 Medium | Movable content width recalculation |
| DetailRow + Selection | 🟡 Medium | Selection state isolation from detail content |
| FilterBar + ForeignKey Column | 🟡 Medium | Separate data source for filter dropdown |

---

## 6. PR Submission Checklist

Use this checklist before every PR submission:

### Code Quality
- [ ] Build succeeds: `0 Warning(s). 0 Error(s).`
- [ ] No `#pragma warning disable` suppressions added
- [ ] All new/modified `public` members have XML `/// <summary>` comments
- [ ] No `dynamic` or untyped `object` parameters where generics are possible
- [ ] No direct `StateHasChanged()` calls in action modules
- [ ] No direct `JSRuntime.InvokeAsync` calls outside `GridJSInteropAdaptor<T>`
- [ ] No new direct module-to-module method calls (use `EventAggregator`)

### Requirements
- [ ] `requirements/bugs/<id>/` or `requirements/features/<name>/` folder exists
- [ ] `fix-approach.md` or `feature-requirement.md` was approved by Scrum Master AI

### Testing
- [ ] All existing tests pass: `dotnet test`
- [ ] New test cases cover the acceptance criteria
- [ ] Manual verification completed for the specific scenario
- [ ] High-risk feature combinations tested manually

### PR Template
- [ ] Bug/feature description with Work Item link filled in
- [ ] Root cause section completed
- [ ] Solution description completed
- [ ] AI log details completed
- [ ] Code Studio usage section checked
- [ ] Impact assessment checked (Low / Medium / High)
- [ ] Breaking changes checked (Yes / No)
- [ ] Cross-platform verification completed
- [ ] API changes section checked

### Accessibility
- [ ] Keyboard navigation unaffected (if UI-visible change)
- [ ] ARIA attributes correct (if new DOM elements added)
- [ ] Screen reader behavior verified (if focus management changed)

---

## 7. Build and Test Commands

```bash
# Build (Debug, .NET 8)
dotnet build Syncfusion.Blazor/Grids/Syncfusion.Blazor.Grid.csproj \
  --configuration Debug --framework net8.0

# Build (Release, all targets)
dotnet build Syncfusion.Blazor/Grids/Syncfusion.Blazor.Grid.csproj \
  --configuration Release

# Run all grid tests
dotnet test --filter "Category=Grid" --configuration Debug

# Run tests for a specific module
dotnet test --filter "Category=Grid&Feature=Edit" --configuration Debug

# Run with verbose output
dotnet test --filter "Category=Grid" --logger "console;verbosity=detailed"
```

---

## 8. Git Quick Reference

```bash
# Create a feature branch
git checkout develop && git pull origin develop
git checkout -b feature/your-feature-name

# Create a bug fix branch
git checkout -b bugfix/1015142-tab-after-grouping

# Stage and commit (conventional commit format)
git add Internal/Actions/Edit.cs
git commit -m "fix(edit): resolve Tab key script error with grouping (#1015142)"

# Push and open PR
git push origin bugfix/1015142-tab-after-grouping
# Open PR in Azure DevOps targeting: develop branch
```

### Commit Message Format

```
<type>(<scope>): <short description> (#workItemId)

Types: feat | fix | refactor | test | docs | perf | style | chore
Scope: edit | sort | filter | group | selection | virtual | paging | export | all
```

---

## 9. Architecture Quick Reference — Layer Decision Table

| Change Type | Belongs In |
|-------------|-----------|
| New `[Parameter]` property | `SfGrid.Properties.cs` (requires API review) |
| New public method | `SfGrid.Methods.cs` (requires API review) |
| Feature business logic change | `Internal/Actions/<Module>.cs` |
| Rendering / DOM structure change | `Internal/Renderer/<Renderer>.razor` |
| Data fetch / query logic | `Internal/Actions/Data.cs` |
| DOM-dependent operation (scroll, focus, resize) | `sf-grid.js` + `GridJSInteropAdaptor.cs` |
| New enum value | `Enumeration/GridsEnumerations.cs` |
| New event argument property | `EventModels/Grids.cs` |
| Cross-module communication | `EventAggregator` event (no direct calls) |

---

## 10. Agent Request Quick Reference

| Task | Agent | Key Inputs |
|------|-------|-----------|
| Analyze a bug | Bug Fix AI | `root-cause.md` + code chunk |
| Implement a feature method | Code AI | `functional-spec.md` + code chunk |
| Write BUnit tests | Test AI | Acceptance criteria (Given-When-Then) |
| Update XML comments | Documentation AI | Modified method signatures |
| Review a code change | Code Review AI | Modified code excerpt |
| Check performance | Performance AI | Modified method + performance targets |
| Check accessibility | Accessibility AI | Modified Razor + ARIA context |
| Gate a PR | Scrum Master AI | Completed PR template |

---

## Navigation

**Previous**: [`../05-practical-examples/feature-implementation-walkthrough.md`](../05-practical-examples/feature-implementation-walkthrough.md)  
**Completion**: [`../DELIVERY-SUMMARY.md`](../DELIVERY-SUMMARY.md)  
**Back to Training Root**: [`../README.md`](../README.md)  
**Back to Docs Root**: [`../../README.md`](../../README.md)
