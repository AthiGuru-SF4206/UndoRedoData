# Third-Party Libraries — Syncfusion Blazor DataGrid

> **Audience**: Developers managing dependencies, DevOps, and release engineers
> **Prerequisite**: [`tech-stack/tech-stack.md`](./tech-stack.md)
> **Last Updated**: March 11, 2026

---

## Overview

All dependencies of `Syncfusion.Blazor.Grid` are first-party Syncfusion packages. There are **no external third-party NuGet dependencies** — the component relies exclusively on sibling Syncfusion Blazor packages and .NET framework BCL.

This is a deliberate architectural constraint ensuring:
- Predictable versioning (all packages share the same version number)
- No transitive CVE exposure from external libraries
- Consistent API patterns across the Syncfusion suite

---

## 1. Direct NuGet Dependencies

All packages below are versioned in lockstep. The current release version is `32.1.19`.

| Package | Version | Purpose |
|---------|---------|---------|
| `Syncfusion.Blazor.Core` | `32.1.19` | Base component infrastructure: `SfDataBoundComponent`, `SfBaseComponent`, interop base, `IJSRuntime` wrapper |
| `Syncfusion.Blazor.Data` | `32.1.19` | `SfDataManager`, `Query`, all adaptors (Blazor, WebApi, OData, URL, GraphQL) |
| `Syncfusion.Blazor.Buttons` | `32.1.19` | Checkbox cells, command column buttons, toolbar buttons |
| `Syncfusion.Blazor.Calendars` | `32.1.19` | Date editor for date/datetime column types |
| `Syncfusion.Blazor.DropDowns` | `32.1.19` | DropDown editor for enum columns; Excel filter dropdowns; column chooser |
| `Syncfusion.Blazor.Inputs` | `32.1.19` | Text, numeric, and masked input editors for inline/dialog edit |
| `Syncfusion.Blazor.Navigations` | `32.1.19` | Toolbar component, context menu, tab component in adaptive mode |
| `Syncfusion.Blazor.Popups` | `32.1.19` | Dialog (dialog edit mode, validation dialog), tooltip |
| `Syncfusion.Blazor.Spinner` | `32.1.19` | Loading indicator overlay (`Preloader.razor`) |
| `Syncfusion.PdfExport.Net.Core` | `32.1.19` | PDF export engine (server-side PDF generation) |
| `Syncfusion.ExcelExport.Net.Core` | `32.1.19` | Excel/XLSX export engine (server-side workbook generation) |

> ⚠️ **Rule**: Never add an external third-party NuGet package. All new functionality must be sourced from Syncfusion packages or the .NET BCL.

---

## 2. Syncfusion.Blazor.Core — Key Contracts

This package provides the base infrastructure that `SfGrid<TValue>` builds on. The following classes are critical integration points:

| Class / Interface | Role in Grid |
|-------------------|-------------|
| `SfDataBoundComponent` | Base class of `SfGrid<TValue>`; provides `PropertyChanges`, `UpdateProperty<T>`, `DataManager`, `IsRendered` |
| `SfBaseComponent` | Base of child components (toolbar, dialogs); provides lifecycle coordination |
| `ISfCircularComponent` | Interface implemented by `SfGrid<TValue>` for child-parent circular reference prevention |
| `SfScriptModules` | Enum identifying which JS module to load; grid sets `SfScriptModules.SfGrid` |
| `EventAggregator` | Internal pub-sub messaging bus for cross-module events |
| `SyncfusionLocalizer` | Localization string provider; used for all UI labels in the grid |

---

## 3. Syncfusion.Blazor.Data — Key Contracts

| Class | Role in Grid |
|-------|-------------|
| `SfDataManager` | Unified data access; bound to `SfGrid.DataSource` or child `<SfDataManager>` tag |
| `Query` | Composable query object built by `DataGenerator<T>` |
| `BlazorAdaptor` | Default adaptor for local `IEnumerable<TValue>` |
| `WebApiAdaptor` | REST API adaptor with built-in paging/sort/filter parameter serialization |
| `ODataV4Adaptor` | OData-compliant query serialization |
| `DataResult` | Return type from remote adaptors: `{ result: T[], count: int }` |
| `DataManagerRequest` | Deserializable server-side request model for `CustomAdaptor` |

---

## 4. Editor Component Dependencies

The grid's edit cell renderers embed these Syncfusion input components directly into edit rows:

| Edit Type | Component | Package |
|-----------|-----------|---------|
| Text column | `SfTextBox` | `Syncfusion.Blazor.Inputs` |
| Numeric column | `SfNumericTextBox<T>` | `Syncfusion.Blazor.Inputs` |
| Date/DateTime column | `SfDatePicker<T>` / `SfDateTimePicker<T>` | `Syncfusion.Blazor.Calendars` |
| Boolean column | `SfCheckBox<T>` | `Syncfusion.Blazor.Buttons` |
| Enum / FK column | `SfDropDownList<T, TVal>` | `Syncfusion.Blazor.DropDowns` |
| Masked input | `SfMaskedTextBox` | `Syncfusion.Blazor.Inputs` |

---

## 5. Export Dependencies

| Package | Type | Usage |
|---------|------|-------|
| `Syncfusion.ExcelExport.Net.Core` | Server-side only | `ExcelExport.Export(columns, data, excelExportProperties)` |
| `Syncfusion.PdfExport.Net.Core` | Server-side only | `PdfExport.Export(columns, data, pdfExportProperties)` |

> ⚠️ **Export limitation**: Export APIs execute on the .NET server/WASM thread. In Blazor Server, large exports should be triggered via streaming to avoid SignalR message size limits. In WASM, memory limits apply.

---

## 6. JavaScript Assets

The grid's JS interop file (`sf-grid.js`) ships as a static web asset in `Syncfusion.Blazor.Grid` and is loaded automatically by the Blazor static file pipeline via `_content/`. There is no manual `<script>` tag required.

| Asset | Namespace / Path | Purpose |
|-------|-----------------|---------|
| Grid JS module | `window.sfBlazor.Grid` — `_content/Syncfusion.Blazor.Grid/sf-grid.js` | DOM measurements, scroll, resize, pointer events, keyboard, virtual offsets |
| Grid CSS | Referenced via `Syncfusion.Blazor.Themes` NuGet or CDN | Visual styling only — no behavioral dependency |

---

## 7. Development Dependencies

These packages appear only in test projects and are **not** shipped in the NuGet package:

| Package | Version | Role |
|---------|---------|------|
| `bunit` | latest stable | Blazor unit testing framework |
| `Microsoft.NET.Test.Sdk` | latest stable | .NET test host |
| `xunit` / `NUnit` | latest stable | Test runner |
| `Microsoft.Playwright` | latest stable | E2E browser automation |
| `coverlet.collector` | latest stable | Code coverage collection |

---

## 8. Version Compatibility Matrix

| Syncfusion Version | .NET 8 | .NET 9 | .NET 10 | Blazor Server | Blazor WASM |
|---------------------|--------|--------|---------|---------------|-------------|
| `32.1.x` (current)  | ✅      | ✅      | ✅       | ✅             | ✅           |
| `27.x`              | ✅      | ❌      | ❌       | ✅             | ✅           |
| `26.x`              | ❌      | ❌      | ❌       | ✅             | ✅           |

> All packages within a release (e.g., `32.1.19`) **must** share the same version number. Mixed versions across Syncfusion packages are unsupported and will cause runtime assembly resolution failures.

---

## 9. Polyfills and Shims

The grid has **no polyfill dependencies**. All supported browsers (Chrome, Edge, Firefox, Safari — current and previous major versions) natively support the Web APIs used:

- `IntersectionObserver` — used for lazy-loading detection
- `ResizeObserver` — used for column/container resize detection
- `CustomEvent` — used for DOM-level grid events
- `localStorage` — used for state persistence

Blazor WASM itself handles .NET runtime shims internally via the Mono/CoreCLR WASM runtime.

---

## 10. Optional / Conditional Packages

These packages are referenced at the application level (not in the Grid NuGet itself) when optional features are used:

| Feature | Required Package | Notes |
|---------|-----------------|-------|
| PDF Export (server-side streaming) | `Syncfusion.PdfExport.Net.Core` | Already a direct dependency |
| Excel Export | `Syncfusion.ExcelExport.Net.Core` | Already a direct dependency |
| Custom Adaptor (EF Core) | `Microsoft.EntityFrameworkCore` | User project; not a Grid dependency |
| Localization | `Syncfusion.Blazor.Core` locale files | Bundled with Core; no additional package |
| Themes | `Syncfusion.Blazor.Themes` | Optional NuGet or CDN; CSS only |

---

*For build environment setup, see [`tech-stack/environment-setup.md`](./environment-setup.md).*
*For language and framework details, see [`tech-stack/tech-stack.md`](./tech-stack.md).*
