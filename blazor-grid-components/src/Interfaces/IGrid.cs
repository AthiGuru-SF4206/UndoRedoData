using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Interface for SfGrid component.
    /// </summary>
    /// <exclude/>
    public interface IGrid
    {

        /// <summary>
        /// Gets or sets the collection of aggregate configurations for the grid.
        /// </summary>
        public List<GridAggregate>? Aggregates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether excel export functionality is enabled for the grid.
        /// </summary>
        public bool AllowExcelExport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled in the grid.
        /// </summary>
        public bool AllowFiltering { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether grouping of rows is enabled.
        /// </summary>
        public bool AllowGrouping { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multi-column sorting is allowed.
        /// </summary>
        public bool AllowMultiSorting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether paging is enabled in the grid.
        /// </summary>
        public bool AllowPaging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether PDF export functionality is enabled.
        /// </summary>
        public bool AllowPdfExport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether users can reorder columns by dragging.
        /// </summary>
        public bool AllowReordering { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether users can resize columns.
        /// </summary>
        public bool AllowResizing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether row drag-and-drop is enabled.
        /// </summary>
        public bool AllowRowDragAndDrop { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether row selection is enabled.
        /// </summary>
        public bool AllowSelection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sorting is enabled in the grid.
        /// </summary>
        public bool AllowSorting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether text wrapping is enabled in grid cells.
        /// </summary>
        public bool AllowTextWrap { get; set; }

        /// <summary>
        /// Gets or sets the overflow behavior for cell content when text wrapping is disabled.
        /// </summary>
        public ClipMode ClipMode { get; set; }

        /// <summary>
        /// Gets or sets the column chooser settings.
        /// </summary>
        public GridColumnChooserSettings? ColumnChooserSettings { get; set; }

        /// <summary>
        /// Gets or sets the column menu items.
        /// </summary>
        public object? ColumnMenuItems { get; set; }

        /// <summary>
        /// Gets or sets the query mode for columns.
        /// </summary>
        public ColumnQueryModeType ColumnQueryMode { get; set; }

        /// <summary>
        /// Gets or sets the collection of columns to be displayed in the grid.
        /// </summary>
        public List<GridColumn>? Columns { get; set; }

        /// <summary>
        /// Gets or sets the context menu items.
        /// </summary>
        public object? ContextMenuItems { get; set; }

        /// <summary>
        /// Gets or sets the current action details of the grid.
        /// </summary>
        public ActionArgs? CurrentAction { get; set; }

        /// <summary>
        /// Gets or sets the edit settings for the grid.
        /// </summary>
        public GridEditSettings? EditSettings { get; set; }

        /// <summary>
        /// Gets or sets the key settings for grid interactions.
        /// </summary>
        public GridKeySettings? KeySettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether alternate row styling (striped rows) is enabled.
        /// </summary>
        public bool EnableAltRow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether autofill (fill handle) functionality is enabled.
        /// </summary>
        public bool EnableAutoFill { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether column virtualization is enabled.
        /// </summary>
        public bool EnableColumnVirtualization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether hover effect is enabled on grid rows.
        /// </summary>
        public bool EnableHover { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether grid state persistence is enabled
        /// </summary>
        public bool EnablePersistence { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether right-to-left (RTL) rendering support is enabled.
        /// </summary>
        public bool EnableRtl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether row virtualization is enabled.
        /// </summary>
        public bool EnableVirtualization { get; set; }

        /// <summary>
        /// Gets or sets the filter settings for the grid.
        /// </summary>
        public GridFilterSettings? FilterSettings { get; set; }

        /// <summary>
        /// Gets or sets the infinite scroll settings.
        /// </summary>
        public GridInfiniteScrollSettings? InfiniteScrollSettings { get; set; }

        /// <summary>
        /// Gets or sets the number of frozen columns.
        /// </summary>
        public int FrozenColumns { get; set; }

        /// <summary>
        /// Gets or sets the number of frozen rows.
        /// </summary>
        public int FrozenRows { get; set; }

        /// <summary>
        /// Specifies the grid line style.
        /// </summary>
        public GridLine GridLines { get; set; }

        /// <summary>
        /// Gets or sets the group settings for the grid.
        /// </summary>
        public GridGroupSettings? GroupSettings { get; set; }

        /// <summary>
        /// Gets or sets the height of the grid.
        /// </summary>
        public string Height { get; set; }

        /// <summary>
        /// Specifies the hierarchy print mode.
        /// </summary>
        public HierarchyGridPrintMode HierarchyPrintMode { get; set; }

        /// <summary>
        /// Gets or sets the page settings for the grid.
        /// </summary>
        public GridPageSettings? PageSettings { get; set; }

        /// <summary>
        /// Specifies the print mode for the grid.
        /// </summary>
        public PrintMode PrintMode { get; set; }

        /// <summary>
        /// Gets or sets the query used for data operations.
        /// </summary>
        public Syncfusion.Blazor.Data.Query? Query { get; set; }

        /// <summary>
        /// Gets or sets the row drop settings.
        /// </summary>
        public GridRowDropSettings? RowDropSettings { get; set; }

        /// <summary>
        /// Gets or sets the height of each row.
        /// </summary>
        public double RowHeight { get; set; }

        /// <summary>
        /// Gets or sets the search settings for the grid.
        /// </summary>
        public GridSearchSettings? SearchSettings { get; set; }

        /// <summary>
        /// Gets or sets the index of the selected row.
        /// </summary>
        public int SelectedRowIndex { get; set; }

        /// <summary>
        /// Gets or sets the selection settings for the grid.
        /// </summary>
        public GridSelectionSettings? SelectionSettings { get; set; }

        /// <summary>
        /// Shows or hides the column chooser.
        /// </summary>
        public bool ShowColumnChooser { get; set; }

        /// <summary>
        /// Shows or hides the column menu.
        /// </summary>
        public bool ShowColumnMenu { get; set; }

        /// <summary>
        /// Gets or sets the sort settings for the grid.
        /// </summary>
        public GridSortSettings? SortSettings { get; set; }

        /// <summary>
        /// Gets or sets the text wrap settings.
        /// </summary>
        public GridTextWrapSettings? TextWrapSettings { get; set; }

        /// <summary>
        /// Gets or sets the toolbar items.
        /// </summary>
        public object? Toolbar { get; set; }

        /// <summary>
        /// Gets or sets the width of the grid.
        /// </summary>
        public string Width { get; set; }

        /// <summary>
        /// Gets or sets the grid templates.
        /// </summary>
        GridTemplates? GridTemplates { get; set; }

        /// <summary>
        /// Indicates whether column changes have occurred.
        /// </summary>
        public bool HasColumnChanges { get; set; }

        /// <summary>
        /// Indicates whether aggregate changes have occurred.
        /// </summary>
        public bool HasAggregateChanges { get; set; }

        /// <summary>
        /// Indicates whether sort column changes have occurred.
        /// </summary>
        public bool HasSortColumnChanges { get; set; }

        /// <summary>
        /// Indicates whether filter column changes have occurred.
        /// </summary>
        public bool HasFilterColumnChanges { get; set; }

        /// <summary>
        /// Gets or sets the current column index.
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Updates child properties of the grid based on the specified key and value.
        /// </summary>
        public void UpdateChildProperties(string key, object value);

        /// <summary>
        /// Gets or sets the automatic cell spanning mode for the entire grid.
        /// </summary>
        public AutoSpanMode AutoSpan { get; set; }
        
        /// <summary>
        /// Triggers a state change in the grid.
        /// </summary>
        public void CallStateHasChanged();

        /// <summary>
        /// Asynchronously triggers a state change in the grid.
        /// </summary>
        public Task CallStateHasChangedAsync();

        /// <summary>
        /// Retrieves the CSS class for the grid.
        /// </summary>
        public string GetClass();

        /// <summary>
        /// Adds a column to the sort collection.
        /// </summary>
        public void AddSortColumn(string colName);

        /// <summary>
        /// Annotates the specified column with additional metadata.
        /// </summary>
        public void AnnotateColumn(GridColumn column);

        /// <summary>
        /// Handles property change operations asynchronously.
        /// </summary>
        public Task PropertyChanged();

        /// <summary>
        /// Prevents or allows rendering of the grid.
        /// </summary>
        public void PreventRender(bool preventRender = true);
        /// <exclude/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Notify(string name, object args);

        /// <summary>
        /// Updates foreign key data in the grid.
        /// </summary>
        public Task UpdateForeignData();

        /// <summary>
        /// Sets the value of a specified field.
        /// </summary>
        public void SetValue<T>(T value, string field);
    }

    /// <summary>
    /// Interface for Grid column.
    /// </summary>
    public interface IGridColumn
    {

        /// <summary>
        /// Retrieves the foreign key data associated with the grid column.
        /// </summary>
        public object GetForeignData();
    }

    /// <summary>
    /// Represents a custom filter operator that can be used in the Menu filter.
    /// </summary>
    public interface IFilterOperator
    {
        /// <summary>
        /// Gets or sets the display text of the custom filter operator.
        /// </summary>
        /// <value>The display text of the custom filter operator.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the operator value that represents the custom filter operator.
        /// </summary>
        /// <value>The operator value of the custom filter operator.</value>
        public string Value { get; set; }
    }
}