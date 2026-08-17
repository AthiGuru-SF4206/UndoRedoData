# 00 — Start Here

> **Audience**: Every developer new to the Syncfusion Blazor DataGrid codebase  
> **Time Required**: 10 minutes  
> **Last Updated**: March 12, 2026

---

## Welcome to the Syncfusion Blazor DataGrid Team

You are working on one of the most feature-rich components in the Syncfusion Blazor suite — the **DataGrid** (`SfGrid<TValue>`). This component powers enterprise data tables, financial dashboards, admin portals, and mobile-responsive UIs across thousands of customer applications.

This document is your entry point. Read it completely before opening any source file.

---

## What You Are Working On

| Item | Value |
|------|-------|
| **Component** | `SfGrid<TValue>` |
| **Namespace** | `Syncfusion.Blazor.Grids` |
| **Source Folder** | `Syncfusion.Blazor/Grids/` |
| **Framework** | Blazor Server, Blazor WebAssembly, Blazor Hybrid |
| **Language** | C# 12, Razor, JavaScript (scoped JS-interop only) |
| **Key Features** | Sorting, Filtering, Grouping, Editing, Virtualization, Export, Selection, Accessibility |
| **Action Modules** | 14 injected modules in `Internal/Actions/` |
| **Renderer Components** | 30+ Razor renderers in `Internal/Renderer/` |

---

## The #1 Rule

> **Never break existing behavior.**

Every change you make — no matter how small — can affect thousands of customer applications. Before touching any code:

1. Understand what the code does today
2. Understand which features share that code path
3. Write or verify a regression test
4. Get a review from the Code Review AI or Scrum Master

If you are unsure about the impact of a change, **stop and ask**. Do not guess.

---

## Quick Start in 5 Steps

### Step 1 — Set up your environment
Follow [`01-getting-started/project-setup-guide.md`](./01-getting-started/project-setup-guide.md) to install prerequisites, clone the repo, and build the component.

### Step 2 — Understand the architecture
Read [`01-getting-started/architecture-overview.md`](./01-getting-started/architecture-overview.md) for a developer-friendly summary of the 4-layer architecture, module injection, and JS-interop bridge.

### Step 3 — Read the product overview
Read [`../overview/product-overview.md`](../overview/product-overview.md) to understand what features exist, what the public API looks like, and what customers use the grid for.

### Step 4 — Review coding standards
Read [`../code-guidelines/coding-standards.md`](../code-guidelines/coding-standards.md) before writing a single line of code. All PRs are rejected if they violate these standards.

### Step 5 — Follow the development workflow
Read [`../dev-process/development-workflow.md`](../dev-process/development-workflow.md) to understand the 7-phase lifecycle: Requirements → Architecture → Unit Test Cases → Development → Testing → Review → Merge → Release.

---

## Key Files at a Glance

| File | What It Is |
|------|-----------|
| `SfGrid.razor.cs` | Main component class — parameters, lifecycle hooks |
| `SfGrid.Properties.cs` | All public `[Parameter]` properties |
| `SfGrid.Methods.cs` | All public async API methods |
| `SfGrid.Lifecycle.cs` | `OnInitializedAsync`, `OnAfterRenderAsync`, `Dispose` |
| `Internal/SfGrid.razor` | Root render shell — orchestrates all child renderers |
| `Internal/Actions/` | 14 action modules (Sort, Filter, Group, Edit, ...) |
| `Internal/Renderer/` | 30+ Razor renderers (rows, cells, headers, pager, ...) |
| `Internal/Base/` | Shared services: DataGenerator, GridJSInteropAdaptor, Utils |
| `sf-grid.js` | JavaScript-side: scroll, focus, drag, resize, keyboard |
| `Enumeration/GridsEnumerations.cs` | All public enums |
| `EventModels/Grids.cs` | All public event argument models |
| `Interfaces/IGrid.cs` | Public grid interface |

---

## Terminology You Must Know

Before reading any source file, ensure you understand these terms:

| Term | Definition |
|------|-----------|
| `TValue` | The generic type parameter representing one data row's model class |
| `GridColumn` | A column definition component with field, header, template, and format settings |
| `DataGenerator<T>` | The internal service that fetches, sorts, filters, groups, and pages data |
| `GridJSInteropAdaptor<T>` | The single JS-interop bridge — all DOM operations go through here |
| `EventAggregator` | Internal pub-sub bus for cross-module communication |
| `PropertyChanges` | Dictionary tracking which `[Parameter]` values changed in the current render cycle |
| `Action Module` | A scoped injectable class in `Internal/Actions/` responsible for one feature |
| `Renderer` | A Razor component in `Internal/Renderer/` responsible for rendering one UI area |
| `VirtualContent` | The rendering mode where only visible rows are in the DOM |
| `FrozenColumn` | A column pinned to the left or right side of the grid |

See [`../overview/glossary.md`](../overview/glossary.md) for the full 65+ term glossary.

---

## What NOT to Do

❌ Do not modify `SfGrid.Properties.cs` without an API review task  
❌ Do not add `[Parameter]` properties with a `bool` default of `true` without consulting the Architect  
❌ Do not use `StateHasChanged()` directly — use the grid's internal render scheduling  
❌ Do not add JS calls outside of `GridJSInteropAdaptor<T>`  
❌ Do not use `dynamic` or `object` types where a generic or interface type is possible  
❌ Do not add dependencies between action modules — use `EventAggregator` instead  
❌ Do not commit code with analyzer warnings  
❌ Do not skip XML documentation comments on any `public` member  

---

## Navigation to Next Module

You are ready to continue. Go to:

**→ [`01-getting-started/architecture-overview.md`](./01-getting-started/architecture-overview.md)**

---

*If you have questions not covered in this training, contact the Architect AI or Scrum Master AI via the agent request templates in [`../ai-agents/usage-guidelines.md`](../ai-agents/usage-guidelines.md).*
