# Syncfusion Blazor DataGrid — Feature Catalog (Extracted)

> Consolidated feature list synthesized from `overview/product-overview.md` and `architecture/system-architecture.md`.
> Last updated: 2026-03-16

---

## Core Data Features

- Data Binding: Local `IEnumerable<T>`, `ObservableCollection<T>`, and `SfDataManager` adaptors (REST, OData, GraphQL, custom).
- Query shaping: server-side `Query` generation (select, filter, search, sort, group, page, aggregate).
- Paging: built-in pager with configurable page size, templates, and integration with virtualization.
- Sorting: single & multi-column sorting, per-column disable, custom comparers.
- Filtering: filter bar, Excel/menu/checkbox filters, per-column control, foreign-key column filters.
- Grouping: drag-and-drop grouping, lazy group loading, expandable group rows, persisted group state.

## Editing & CRUD

- Edit modes: Inline, Dialog, Batch, AddNewRow position control.
- CRUD operations: Add/Edit/Delete/Save/Cancel with validation (data annotations & custom rules).
- Command column: per-row action buttons and custom command templates.
- Custom editors: cell editor templates and editor components.

## Selection & Interaction

- Selection modes: Row, Cell, Both; single/multiple selection; programmatic APIs (`SelectRow`, `SelectCell`, etc.).
- Checkbox selection with header checkbox and selection persistence across pages.
- Row drag & drop: intra-grid and cross-grid row dragging with configurable targets.
- Context menu: right-click menu with built-in and custom items.

## Performance & Large-Data Scenarios

- Virtual Scrolling: row & column virtualization, overscan buffer, integration with frozen columns and grouping.
- Infinite Scrolling: on-demand data loading with cache-block control.
- High-volume support: optimized rendering to handle 100K+ rows with minimal DOM.

## Columns & Layout

- Auto-generated columns from model reflection and manual column definitions.
- Template columns: header, cell, edit, filter templates.
- Stacked headers, column reorder, resize, column chooser, column menu (sort/filter/group/autofit).
- Freeze columns/rows with direction control and movable freeze line.
- Auto column fit and clip modes (`Clip`, `Ellipsis`, `EllipsisWithTooltip`).

## Aggregation & Reporting

- Aggregates in footer, group footer, and group caption rows (Sum, Avg, Count, Min, Max, TrueCount, FalseCount, Custom).
- Reactive aggregates that update on data changes.
- Export: Excel, PDF, CSV (configurable and supports hierarchy export).

## Toolbar, Search & Utilities

- Toolbar: built-in items (Add, Edit, Delete, Update, Cancel, Search, Print, ExcelExport, PdfExport, CsvExport, ColumnChooser) and custom templates.
- Global search across searchable columns with configurable operators.
- Tooltip, sticky header, auto cell spanning (AutoSpan), and localized/RTL support.

## Accessibility & Adaptive UI

- WCAG 2.0 AA compliance, full keyboard navigation, ARIA attributes, screen reader support.
- Adaptive UI for mobile (vertical row mode, adaptive dialogs) and responsive layouts.

## State, Persistence & Integration

- State persistence for column order, sort, filter, and group state (localStorage and optional DB persistence API).
- JS interop bridge (`GridJSInteropAdaptor`) for scroll measurement, layout, focus, and other DOM duties via `sfBlazor.Grid.*`.
- Event aggregator and lifecycle hooks for cross-module communication and action pipeline (`OnActionBegin` / `OnActionComplete`).

## Module-based Architecture (Implementation Details)

- Module injection: feature modules instantiated at initialization (Data, Sort, Filter, Grouping, Edit, Selection, VirtualScroll, InfiniteScroll, DetailRow, ForeignKey, ReactiveAggregate, MergeHandler, Reorder, RowReorder, FocusHandler).
- Presentation separation: layered architecture (Infrastructure, Data, Business/Action, Presentation) supporting maintainability and testability.
- Rendering strategy: server-driven Blazor render tree with JS handling of scroll/layout events; minimal DOM patches from Blazor.

---

## Notes and Sources

- Source files used: `docs/overview/product-overview.md`, `docs/architecture/system-architecture.md`.
- No explicit `design.md` change notes were found in the repository; design decisions are typically captured under `openspec/changes/<name>/design.md` when present.

---

*If you want this file moved to `docs/features.md` or merged back into `overview/product-overview.md`, I can update accordingly.*
