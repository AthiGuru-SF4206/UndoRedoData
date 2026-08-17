# Tech Stack — Syncfusion Blazor DataGrid

> **Audience**: All developers working on `SfGrid<TValue>`
> **Prerequisite**: [`overview/product-overview.md`](../overview/product-overview.md)
> **Last Updated**: March 11, 2026

---

## Overview

The Syncfusion Blazor DataGrid is a multi-target .NET class library built with Microsoft Blazor. It ships as a NuGet package (`Syncfusion.Blazor.Grid`) targeting three active .NET frameworks simultaneously and supports both Blazor hosting models (Server and WebAssembly).

---

## 1. Languages

### C# (Primary Language)

| Target Framework | C# Language Version |
|------------------|---------------------|
| `net8.0`         | C# 12               |
| `net9.0`         | C# 13               |
| `net10.0`        | C# 14               |

**Key language features in use:**

- **Nullable reference types** — `<Nullable>enable</Nullable>` is enforced project-wide. All reference types must carry explicit nullability annotations (`string?`, `List<T>?`).
- **Generic type parameters** — Core component is `SfGrid<TValue>` where `TValue` is the data model type.
- **Partial classes** — `SfGrid<TValue>` is split across four `.cs` files and one `.razor.cs` file for maintainability.
- **Async/Await** — All public methods and lifecycle hooks are `async Task`. `ConfigureAwait(true)` is used for Blazor context continuations.
- **Records and init-only setters** — Used for immutable event argument models in `EventModels/Grids.cs`.
- **Pattern matching** — Used extensively in rendering path switches (`switch(EditMode)`, type-checking in cell renderers).
- **Expression trees** — Used in `Annotation/` for compile-time property access and validation attribute reading.

### Razor (Component Markup)

All UI components use `.razor` files combining C# code blocks with HTML markup. File naming follows `PascalCase.razor` convention (e.g., `GridHeader.razor`, `GridRow.razor`).

- Razor syntax version: tied to the target framework SDK
- Inline `@code { }` blocks are used only in non-split components
- Complex components use the code-behind pattern: `Component.razor` + `Component.razor.cs`

### JavaScript (JS Interop Layer)

A companion JavaScript file (`sf-grid.js`, bundled into the package as a static web asset) exposes the `window.sfBlazor.Grid` namespace and handles:

- DOM measurements (row heights, viewport size, column widths, scroll position)
- Scroll event capture and throttling
- Keyboard event coordination
- Column resize drag handling
- Pointer event tracking (drag, reorder)
- Virtual scroll offset computation
- `localStorage` reads for state persistence

JS code is never responsible for data operations — it only measures, observes, and reports DOM state to the .NET layer.

---

## 2. Frameworks

### Microsoft Blazor

| Hosting Model | Supported | Notes |
|---------------|-----------|-------|
| Blazor Server  | ✅ Yes     | Full feature set; data stays on server |
| Blazor WebAssembly (WASM) | ✅ Yes | Full feature set; data loads in browser |
| Blazor Hybrid (MAUI) | ✅ Yes | Via WebView; standard Blazor APIs |
| Blazor SSR (Static) | ❌ No | Interactive rendering required |

**Blazor-specific patterns used:**

- `[Parameter]` / `[CascadingParameter]` for component data binding
- `EventCallback<T>` for component event propagation
- `IJSRuntime` for JavaScript interop
- `DotNetObjectReference<T>` for JS-to-.NET callbacks
- `IDisposable` / `IAsyncDisposable` for cleanup on component unmount
- `StateHasChanged()` for manual render triggers
- `RenderFragment` / `RenderFragment<TValue>` for template columns

### .NET Base Class Libraries

| BCL Area | Usage |
|----------|-------|
| `System.Reflection` | `PropertyInfo` caching in `PropertyInfoHelper<T>` |
| `System.Linq` | LINQ queries on `IEnumerable<TValue>` in local adaptor |
| `System.Collections.ObjectModel.ObservableCollection<T>` | Two-way data binding with auto-refresh |
| `System.ComponentModel.DataAnnotations` | Validation attribute reading for edit forms |
| `System.Text.Json` | Serialization of query parameters for remote adaptors |
| `System.Threading.Tasks` | Async data operations throughout |

---

## 3. Build Tools

### MSBuild / .NET SDK

The project uses the `Microsoft.NET.Sdk.Razor` SDK:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <SignAssembly>true</SignAssembly>
    <AssemblyOriginatorKeyFile>..\sf.snk</AssemblyOriginatorKeyFile>
    <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
    <Nullable>enable</Nullable>
    <LangVersion Condition="'$(TargetFramework)'=='net8.0'">12</LangVersion>
    <LangVersion Condition="'$(TargetFramework)'=='net9.0'">13</LangVersion>
    <LangVersion Condition="'$(TargetFramework)'=='net10.0'">14</LangVersion>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <PackageId>Syncfusion.Blazor.Grid</PackageId>
    <AssemblyName>Syncfusion.Blazor.Grids</AssemblyName>
    <RootNamespace>Syncfusion.Blazor.Grids</RootNamespace>
    <Version>32.1.19</Version>
  </PropertyGroup>
</Project>
```

**Build commands:**

| Command | Purpose |
|---------|---------|
| `dotnet build` | Standard development build |
| `dotnet build -c Release` | Release build with optimization |
| `dotnet pack -c Release` | Produce NuGet package |
| `dotnet test` | Run unit tests |

### Assembly Signing

The assembly is strong-named via `<SignAssembly>true</SignAssembly>` using a shared key file (`sf.snk`). Do not modify or replace this file — all packages in the Syncfusion.Blazor suite must be signed with the same key.

### Documentation XML Generation

`<GenerateDocumentationFile>true</GenerateDocumentationFile>` ensures all XML doc comments in `.cs` files are emitted into the distributable NuGet package. All public API members **must** have `<summary>`, `<param>`, and `<returns>` tags.

---

## 4. Testing Frameworks

### Unit Testing — bUnit

[bUnit](https://bunit.dev/) is the primary framework for component-level unit tests.

- Tests Razor component rendering and lifecycle
- Validates parameter binding and event callbacks
- Used for editor, renderer, and event model tests
- Test project: `Syncfusion.Blazor.Grid.Tests/`

### Integration & E2E Testing — Playwright

[Playwright](https://playwright.dev/) is used for end-to-end browser testing:

- Cross-browser: Chromium, Firefox, WebKit
- Tests user interactions (sort, filter, edit, virtual scroll)
- Each PR fix must reference a Playwright PR with reproduction coverage

### Test Identification Convention

```
UnitTest:  SfGrid_[Feature]_[Scenario]_[ExpectedBehavior]
E2E:       Grid_[Feature]_[Action]_[Verification]
```

---

## 5. Data Layer

### SfDataManager

`Syncfusion.Blazor.Data.SfDataManager` is the unified data abstraction across all Syncfusion components. For the Grid:

- Accepts `DataSource` as `IEnumerable<TValue>` or `SfDataManager` configuration
- Executes `Query` objects built by `DataGenerator<T>`
- Returns typed or untyped result sets to `SfGrid<TValue>`

### DataManager Adaptors

| Adaptor | Package | Use Case |
|---------|---------|----------|
| `BlazorAdaptor` | `Syncfusion.Blazor.Data` | Local in-memory `IEnumerable<T>` |
| `WebApiAdaptor` | `Syncfusion.Blazor.Data` | ASP.NET Core REST endpoints |
| `ODataV4Adaptor` | `Syncfusion.Blazor.Data` | OData v4 compliant APIs |
| `UrlAdaptor` | `Syncfusion.Blazor.Data` | Generic HTTP endpoints |
| `CustomAdaptor` | User-defined | Developer-controlled fetch logic |
| `GraphQLAdaptor` | `Syncfusion.Blazor.Data` | GraphQL query endpoints |

### Query Object

`Syncfusion.Blazor.Data.Query` is a composable query builder:

```csharp
new Query()
    .Where(filterPredicate)
    .SortBy(field, direction)
    .Group(groupField)
    .Page(pageIndex, pageSize)
    .Select(columnFields)
    .Aggregate(aggregateType, field)
```

---

## 6. Framework Wrappers

The Grid is the Blazor wrapper for the EJ2 TypeScript/JavaScript Grid. The relationship:

| Layer | Technology | Role |
|-------|-----------|------|
| EJ2 Grid (TypeScript) | `@syncfusion/ej2-grids` | Canonical logic reference |
| Blazor Grid | `Syncfusion.Blazor.Grid` | .NET-native reimplementation |
| JS Interop | `sf-grid.js` (`window.sfBlazor.Grid`) | DOM-only bridge |

**Important**: The Blazor Grid is **not** a wrapper around the EJ2 JS component. It is an independent .NET reimplementation that shares only the JS DOM-handling layer for scroll, resize, and keyboard events. All data logic, rendering, and state management are native .NET/Blazor.

---

## 7. Performance Tooling

| Tool | Purpose |
|------|---------|
| .NET Memory Profiler (dotMemory / VS Diagnostic Tools) | Detect memory leaks in module lifecycle |
| Blazor WASM DevTools | Profile WASM rendering and IL execution |
| Browser DevTools (Performance tab) | Measure FPS, layout thrashing, paint costs |
| `BenchmarkDotNet` | Micro-benchmark data query and aggregation methods |
| Playwright Performance Metrics | Page load and interaction timing in E2E |

---

## 8. Packaging

| Property | Value |
|----------|-------|
| **NuGet Package ID** | `Syncfusion.Blazor.Grid` |
| **Assembly Name** | `Syncfusion.Blazor.Grids` |
| **Root Namespace** | `Syncfusion.Blazor.Grids` |
| **Current Version** | `32.1.19` |
| **License** | Syncfusion Commercial / Community (see `LICENSE.txt`) |
| **Static Assets** | Bundled via `.razor` component resource imports |

---

*For dependency details, see [`tech-stack/third-party-libraries.md`](./third-party-libraries.md).*
*For environment setup, see [`tech-stack/environment-setup.md`](./environment-setup.md).*
