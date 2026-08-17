# Syncfusion Blazor DataGrid — Product Overview

> **Component**: `SfGrid<TValue>`  
> **Version**: 32.x  
> **Framework**: Blazor Server | Blazor WebAssembly | Blazor Hybrid  
> **Namespace**: `Syncfusion.Blazor.Grids`  
> **Last Updated**: March 11, 2026

---

## What is the Syncfusion Blazor DataGrid?

The **Syncfusion Blazor DataGrid** (`SfGrid<TValue>`) is a high-performance, feature-rich tabular data component built natively for Blazor applications. It enables developers to display, manipulate, and analyze large volumes of structured data through a rich, interactive user interface.

The component is implemented as a strongly-typed generic class `SfGrid<TValue>`, where `TValue` represents the model type of each data row. It integrates deeply with Blazor's component model using Razor components, `[Parameter]` binding, `EventCallback` delegates, and the `SfDataManager` data abstraction layer.

The DataGrid is not just a table renderer — it is a complete data management UI that covers the full lifecycle of data: **fetching → displaying → filtering → sorting → editing → exporting**.

---

## Key Features

### 1. Data Binding
- Supports `IEnumerable<TValue>` for local in-memory data
- Supports `ObservableCollection<TValue>` with reactive updates
- Integrates with `SfDataManager` for remote REST, OData, GraphQL, and custom adaptors
- Supports custom `Query` objects for server-side data shaping
- Column query mode control: `All`, `Schema`, `ExcludeHidden`

### 2. Paging
- Built-in pager component with configurable page size and navigation
- Supports `AllowPaging = true` with `GridPageSettings` for page size, page count, and template customization
- Works seamlessly with virtual scrolling and infinite scrolling

### 3. Sorting
- Single-column and multi-column sorting (Shift+Click or Ctrl+Click)
- Configurable via `GridSortSettings` and `GridSortColumn`
- Per-column sort disable via `GridColumn.AllowSorting = false`
- Custom sort comparer support

### 4. Filtering
- Filter bar (inline), Excel-style filter, Menu filter, and Checkbox filter
- Configurable via `GridFilterSettings`
- Per-column filter disable via `GridColumn.AllowFiltering = false`
- Foreign key column filter support

### 5. Grouping
- Drag-and-drop column grouping with visual group drop area
- Configurable via `GridGroupSettings`
- Lazy loading support for grouped data
- Expandable/collapsible group rows with aggregate display
- Persist group state across navigation

### 6. Editing
- Four edit modes: Inline (`Normal`), `Dialog`, `Batch`, and `Normal` with `ShowAddNewRow`
- CRUD operations: Add, Edit, Delete, Save, Cancel
- `ShowAddNewRow` — persistent empty row at the top for immediate record entry
- Command column support for per-row action buttons
- Custom editor cell templates via `Editors/`
- Validation via data annotations and custom rules
- New row position control via `GridEditSettings.NewRowPosition`

### 7. Selection
- Row, Cell, and Both selection modes
- Single and Multiple selection types
- Checkbox selection with header checkbox
- Persist selection across pages
- AutoFill support for batch editing (Box cell selection)
- Programmatic selection via `SelectRow()`, `SelectRows()`, `SelectCell()`, `SelectCells()`

### 8. Virtual Scrolling
- Row virtualization: renders only visible rows in the DOM
- Column virtualization: renders only visible columns horizontally
- Virtual mask rows for loading state feedback
- Overscan count for buffer pre-rendering
- Integrated with frozen columns and grouping

### 9. Infinite Scrolling
- Loads data on-demand as the user scrolls to the bottom
- Configurable initial blocks and maximum cache blocks
- Works with local and remote data sources

### 10. Freeze Columns and Rows
- Freeze columns to left, right, or both sides
- Freeze rows at the top of the grid content
- Movable freeze line via `AllowFreezeLineMoving`
- Column-level freeze via `GridColumn.IsFrozen` and `FreezeDirection`

### 11. Column Features
- Auto-generated columns from model reflection
- Template columns (header, cell, edit, filter)
- Foreign key columns with sub-data source
- Stacked headers (multi-level column headers)
- Column reorder (drag-and-drop)
- Column resize
- Column chooser (show/hide columns)
- Column menu with sort, filter, group, autofit actions
- Clip mode: `Clip`, `Ellipsis`, `EllipsisWithTooltip`
- Auto column fit and manual auto-fit

### 12. Aggregates
- Footer, group footer, and group caption aggregate rows
- Built-in types: Sum, Average, Count, Min, Max, TrueCount, FalseCount, Custom
- Reactive aggregates that update on data change

### 13. Export
- Excel export (`ExportToExcelAsync`)
- PDF export (`ExportToPdfAsync`)
- CSV export (`ExportToCsvAsync`)
- Configurable export properties for both formats
- Hierarchy grid export support

### 14. Toolbar
- Built-in toolbar items: Add, Edit, Delete, Update, Cancel, Search, Print, ExcelExport, PdfExport, CsvExport, ColumnChooser
- Custom toolbar items support
- Custom toolbar template via `GridTemplates.ToolbarTemplate`

### 15. Search
- Global search across all searchable columns
- Configurable via `GridSearchSettings`
- Supports case-insensitive, contains, startsWith, endsWith operators

### 16. Row Drag and Drop
- Drag rows within the same grid
- Drag rows to another grid or component
- Configurable target via `GridRowDropSettings.TargetID`

### 17. Detail Row
- Expandable detail rows for master-detail layouts
- Template-driven detail content
- Hierarchy grid support with parent-child relationship

### 18. Context Menu
- Right-click context menu with built-in and custom items
- Items include: Edit, Delete, Copy, Sort, Group, Export, Pager navigation

### 19. Adaptive UI
- Full-screen responsive layout for mobile/tablet
- Vertical row rendering mode
- Configurable via `EnableAdaptiveUI` and `AdaptiveUIMode`

### 20. Accessibility and Keyboard Navigation
- WCAG 2.0 AA compliant
- Full keyboard navigation via arrow keys, Tab, Enter, Escape
- Screen reader support with ARIA attributes
- Customizable key bindings via `GridKeySettings`

### 21. Localization and RTL
- Right-to-left layout via `EnableRtl`
- Locale string customization for all UI labels
- Compatible with Syncfusion global locale service

### 22. State Persistence
- Persists column order, sort, filter, group state in `localStorage`
- Enable via `EnablePersistence`
- Supports database-driven persistence via `GetPersistDataAsync()`

### 23. Auto Cell Spanning
- Automatic horizontal (row), vertical (column), or combined spanning
- Configurable via `AutoSpan` property using `AutoSpanMode` enum
- Per-column override via `GridColumn.AutoSpan`

### 24. Tooltip
- Cell and header tooltip support
- Enable via `ShowTooltip`
- Custom tooltip template via `GridTemplates.TooltipTemplate`

### 25. Sticky Header
- Fixed column headers during vertical scroll
- Enable via `EnableStickyHeader`

---

## Ideal Use Cases

| Scenario | Why SfGrid |
|----------|-----------|
| Enterprise data tables | Handles 100K+ rows via virtualization |
| Financial dashboards | Aggregates, sorting, export to Excel/PDF |
| Inventory management | CRUD editing, batch updates, validation |
| Admin portals | Column chooser, toolbar, search, filters |
| Reporting screens | Grouping, aggregates, PDF/Excel export |
| Mobile-responsive apps | Adaptive UI, vertical row mode |
| Master-detail UIs | Detail row, hierarchy grid |
| Real-time data feeds | ObservableCollection reactive updates |

---

## Framework Support

| Framework | Support Level |
|-----------|--------------|
| Blazor Server | ✅ Full support |
| Blazor WebAssembly | ✅ Full support |
| Blazor Hybrid (MAUI) | ✅ Full support |
| .NET 6 | ✅ Supported |
| .NET 7 | ✅ Supported |
| .NET 8 | ✅ Supported (LTS) |
| .NET 9 | ✅ Supported |
| .NET 10 | ✅ Supported |

---

## Quick Start Example

```razor
<SfGrid DataSource="@Orders"
        AllowSorting="true"
        AllowFiltering="true"
        AllowPaging="true">
    <GridPageSettings PageSize="10" />
    <GridColumns>
        <GridColumn Field="@nameof(Order.OrderID)"
                    HeaderText="Order ID"
                    IsPrimaryKey="true"
                    Width="120" />
        <GridColumn Field="@nameof(Order.CustomerName)"
                    HeaderText="Customer"
                    Width="150" />
        <GridColumn Field="@nameof(Order.Freight)"
                    HeaderText="Freight"
                    Format="C2"
                    Width="120" />
        <GridColumn Field="@nameof(Order.OrderDate)"
                    HeaderText="Order Date"
                    Format="d"
                    Width="130" />
    </GridColumns>
</SfGrid>

@code {
    public List<Order> Orders { get; set; } = new();

    protected override void OnInitialized()
    {
        Orders = Enumerable.Range(1, 100).Select(i => new Order
        {
            OrderID    = 10000 + i,
            CustomerName = $"Customer {i}",
            Freight    = Math.Round(i * 0.75, 2),
            OrderDate  = DateTime.Now.AddDays(-i)
        }).ToList();
    }

    public class Order
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public double Freight { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
```

---

## Official Resources

| Resource | Link |
|----------|------|
| Live Demo | https://blazor.syncfusion.com/demos/datagrid/overview |
| API Reference | https://help.syncfusion.com/cr/blazor/Syncfusion.Blazor.Grids.SfGrid-1.html |
| Getting Started | https://blazor.syncfusion.com/documentation/datagrid/getting-started |
| NuGet Package | `Syncfusion.Blazor.Grid` |

---

## Quick Benefits Summary

- ✅ **Zero-config start** — works with just `DataSource` and auto-generated columns
- ✅ **Strongly typed** — full IntelliSense on column fields and model properties
- ✅ **Performance first** — row/column virtualization handles millions of records
- ✅ **Full CRUD** — built-in editing with validation, no external form needed
- ✅ **Export ready** — Excel, PDF, CSV out of the box
- ✅ **Accessible** — WCAG 2.0 AA keyboard navigation and screen reader support
- ✅ **Blazor native** — JS interop is scoped to DOM-dependent operations only (scroll, focus, resize, drag); all data and render logic is pure C#
- ✅ **Composable** — every sub-feature is a Razor child component
- ✅ **Unified JS bridge** — a single `sfBlazor.Grid.*` JS module handles all client-side DOM operations via a generic dispatcher pattern

---

*See [`architecture/system-architecture.md`](../architecture/system-architecture.md) for the internal design.*  
*See [`overview/glossary.md`](./glossary.md) for terminology definitions.*
