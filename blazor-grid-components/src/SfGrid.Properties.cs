using System;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using Syncfusion.Blazor.Internal;


namespace Syncfusion.Blazor.Grids
{
    public partial class SfGrid<TValue> : SfDataBoundComponent, IGrid, ISfCircularComponent
    {
        /// <summary>
        /// Gets or sets the unique ID of the grid element.
        /// </summary>
        /// <value>
        /// Accepts the string value.
        /// </value>
        [Parameter]
        public string? ID { get; set; }

        /// <summary>
        /// Defines the child content.
        /// </summary>
        /// <exclude/>
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Syncfusion.Blazor.Grids.GridAggregate"/> instances that control the rendering of aggregate rows displayed in the grid content.
        /// </summary>
        /// <value>
        /// A list of <see cref="Syncfusion.Blazor.Grids.GridAggregate"/> instances.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.Blazor.Grids.GridAggregates"/> class provides various properties to customize aggregate operations.
        /// You can use the <see cref="Syncfusion.Blazor.Grids.GridAggregate"/> class to configure specific aggregate operations for individual columns in the grid.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("aggregates")]
        public List<GridAggregate>? Aggregates { get; set; }

        private List<GridAggregate>? _aggregates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is allowed to export the grid to an Excel file.
        /// </summary>
        /// <value>
        /// <c>true</c>, User will export the grid to an Excel file.
        /// The default value is <c>false</c>
        /// </value>
        /// <remarks>
        /// To perform excel export, bind <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnToolbarClick"/> event and
        /// invoke <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.ExportToExcelAsync(ExcelExportProperties)"/> method
        /// in its toolbar item click handler.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowExcelExport")]
        public bool AllowExcelExport { get; set; }

        private bool _allowExcelExport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the filter bar for all columns in the Syncfusion Blazor DataGrid.
        /// </summary>
        /// <value>
        /// <c>true</c>, Filter bar will be displayed for all columns and allows the user to filter grid records with required criteria.
        /// The default value is <c>false</c>
        /// </value>
        /// <remarks>
	/// Filter bar is disabled for template and command column, which means the columns is not having Field property.
        /// Filter type can be changed from <c>Filterbar</c> using the <see cref="Syncfusion.Blazor.Grids.GridFilterSettings.Type"/> property.
        /// Filter can be disabled for a particular column by using the <see cref="Syncfusion.Blazor.Grids.GridColumn.AllowFiltering"/> property.
        /// See <see cref="Syncfusion.Blazor.Grids.FilterType"/> for more details.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowFiltering")]
        public bool AllowFiltering { get; set; }

        private bool _allowFiltering { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the user can dynamically group or ungroup columns.
        /// </summary>
        /// <value>
        /// <c>true</c>, The user can group columns by drag and drop columns from the column header to the group drop area.
        /// The default value is <c>false</c>
        /// </value>
        /// <remarks>
        /// To disable grouping for a particular column, set the <see cref="Syncfusion.Blazor.Grids.GridColumn.AllowGrouping"/> property to false for that column.
        /// To programmatically group or ungroup columns, use the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.GroupColumnAsync(string)"/> and <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.UngroupColumnAsync(string)"/> methods.
        /// Grouping can be further customized using the <see cref="Syncfusion.Blazor.Grids.GridGroupSettings"/> component.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowGrouping")]
        public bool AllowGrouping { get; set; }

        private bool _allowGrouping { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether to allow the user to sort multiple columns in the grid.
        /// </summary>
        /// <value>
        /// <c>true</c>, the user can do the multi sort by clicking on the column header while holding the Shift or Ctrl key.
        /// The default value is <c>true</c>.
        /// </value>
        /// <remarks>
        /// Note that <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowSorting"/> must be set to true in order to use this property.
        /// Sorting can be further configured using the <see cref="Syncfusion.Blazor.Grids.GridSortSettings"/> component.
        /// </remarks>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("allowMultiSorting")]
        public bool AllowMultiSorting { get; set; } = true;

        private bool _allowMultiSorting { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether paging is enabled for the <see cref="Syncfusion.Blazor.Grids.SfGrid{T}"/> component.
        /// </summary>
        /// <value>
        /// <c>true</c>, a pager is rendered at the footer of the grid. The pager can be used to handle page navigation in the grid.
        /// The default value is <c>false</c>
        /// </value>
        /// <remarks>
        /// Paging can be further configured using <see cref="Syncfusion.Blazor.Grids.GridPageSettings"/> component.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowPaging")]
        public bool AllowPaging { get; set; }

        private bool _allowPaging { get; set; }

        /// <summary>
        /// Gets or sets a value specifies whether the grid allow users to export grid to PDF document.
        /// </summary>
        /// <value>
        /// <c>true</c>, The user can export the grid to a PDF document. The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// To perform pdf export, bind the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnToolbarClick"/> event and
        /// In the toolbar item click handler, invoke the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.ExportToPdfAsync(PdfExportProperties)"/> method with required export properties.
        /// The export properties can be further customized using the <see cref="Syncfusion.Blazor.Grids.PdfExportProperties"/> class.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowPdfExport")]
        public bool AllowPdfExport { get; set; }

        private bool _allowPdfExport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether users can reorder the columns in the grid by dragging and dropping them.
        /// </summary>
        /// <value>
        /// <c>true</c>, if the grid columns can be reordered. The default value is <c>false</c>
        /// </value>
        /// <remarks>
        /// If Grid is rendered with stacked headers, reordering is allowed only at the same level as the column headers.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowReordering")]
        public bool AllowReordering { get; set; }

        private bool _allowReordering { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user is allowed to resize the columns of the Grid.
        /// </summary>
        /// <value>
        /// <c>true</c>, Grid columns can be resized by dragging the right edge of the column header.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// Resizing can be disabled for a particular column by setting the <see cref="Syncfusion.Blazor.Grids.GridColumn.AllowResizing"/> property to false.
        /// In RTL mode, Grid columns can be resized by clicking and dragging the left edge of the header cell.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowResizing")]
        public bool AllowResizing { get; set; }

        private bool _allowResizing { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to allow the user to drag and drop grid rows.
        /// </summary>
        /// <value>
        /// <c>true</c>, Users can drag and drop grid rows at another or within grid. The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// To drag and drop rows between grids or to another component, you should provide the ID of the target grid or component to the <see cref="Syncfusion.Blazor.Grids.GridRowDropSettings.TargetID"/> property.
        /// Selection feature must be enabled for row drag and drop within grids.
        /// Multiple rows can be selected by clicking and dragging inside the grid. For multiple row selection, the <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.Type"/> property must be set to multiple.
        /// For performing row drag and drop action on the data grid, any one of the columns should be defined as a primary key using the <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/>  property
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowRowDragAndDrop")]
        public bool AllowRowDragAndDrop { get; set; }

        private bool _allowRowDragAndDrop { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the Grid records can be selected by clicking on it.
        /// </summary>
        /// <value>
        /// <c>true</c>, The Grid records can be selected by clicking on it. The default value is <c>true</c>.
        /// </value>
        /// <remarks>
        /// Selection can be further configured using <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings"/> component.
        /// </remarks>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("allowSelection")]
        public bool AllowSelection { get; set; } = true;

        private bool _allowSelection { get; set; }

        /// <summary>
        /// Gets or sets a value that determines whether the grid records are allowed to sort while clicking on the column header.
        /// </summary>
        /// <value>
        /// <c>true</c>, Grid records can be sorted by clicking on the column header. The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// Columns in the DataGrid are sorted in ascending order when clicked. Clicking on an already sorted column will toggle the sort direction between ascending and descending.
        /// To disable sorting for a particular column, set the <see cref="Syncfusion.Blazor.Grids.GridColumn.AllowSorting"/> property to <c>false</c>.
        /// Sorting can be further configured using <see cref="Syncfusion.Blazor.Grids.GridSortSettings"/> component.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowSorting")]
        public bool AllowSorting { get; set; }

        private bool _allowSorting { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the text content in the column cells wraps to the next line when it exceeds the width of the column.
        /// </summary>
        /// <value>
        /// <c>true</c>, If the text content of the column cells will wrap to the next line when it exceeds the width of the column.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// When a column width is not specified, the text wrapping of columns will automatically adjust based on the width of the DataGrid.
        /// To further customize text wrapping for specific columns, use the <see cref="Syncfusion.Blazor.Grids.GridTextWrapSettings.WrapMode"/> property.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowTextWrap")]
        public bool AllowTextWrap { get; set; }

        private bool _allowTextWrap { get; set; }

        /// <summary>
        /// Gets or sets the clip mode to handle content overflow of the Grid cell.
        /// </summary>
        /// <value>
        /// One of the <see cref="ClipMode"/> enumeration that specifies the Clip Mode.
        /// The default value is <see cref="ClipMode.Ellipsis"/> which displays an ellipsis when the content overflows the cell area.
        /// </value>
        /// <remarks>
        /// The <c>ClipMode</c> property can be set to one of the following values:
	/// <list type="bullet">
        /// <item>
        /// <term>Clip</term>
        /// <description>Truncates the cell content when it overflows the cell area.</description>
        /// </item>
        /// <item>
        /// <term>Ellipsis</term>
        /// <description>Displays an ellipsis when the cell content overflows its area.</description>
        /// </item>
        /// <item>
        /// <term>EllipsisWithTooltip</term>
        /// <description>Displays an ellipsis when the cell content overflows its area and displays a tooltip while hovering on the cell.</description>
        /// </item>
        /// </list>
        /// </remarks>
        [Parameter]
        [DefaultValue(ClipMode.Ellipsis)]
        [JsonPropertyName("clipMode")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ClipMode ClipMode { get; set; } = ClipMode.Ellipsis;

        private ClipMode _clipMode { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.Blazor.Grids.GridColumnChooserSettings"/> instance that configures the behavior of the column chooser in the grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridColumnChooserSettings"/>.
        /// </value>
        /// <remarks> 
        /// The <see cref="Syncfusion.Blazor.Grids.GridColumnChooserSettings"/> class provides various properties to customize column chooser operations, 
        /// such as enabling or disabling the column chooser, specifying the column chooser button's text, and setting the position of the column chooser dialog.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("columnChooserSettings")]
        public GridColumnChooserSettings? ColumnChooserSettings { get; set; }

        private GridColumnChooserSettings? _columnChooserSettings { get; set; }

        /// <summary>
        /// Gets or sets instance of <see cref="Syncfusion.Blazor.Grids.GridKeySettings"/> which configures the cell movement keys in the grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridKeySettings"/>.
        /// </value>
        /// <remarks> 
        /// <see cref="Syncfusion.Blazor.Grids.GridKeySettings"/> class provides various properties to customize key operations. 
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("keySettings")]
        public GridKeySettings? KeySettings { get; set; }

        private GridKeySettings? _keySettings { get; set; }

        /// <summary>
        /// Gets or sets the column menu items that define both built-in and custom items.
        /// </summary>
        /// <value>
        /// The available built-in items are:
	/// <list type="bullet">
        /// <item>
        /// <term>AutoFitAll</term>
        /// <description>Auto fit the size of all columns.</description>
        /// </item>
        /// <item>
        /// <term>AutoFit</term>
        /// <description>Auto fit the current column.</description>
        /// </item>
        /// <item>
        /// <term>Group</term>
        /// <description>Group by current column.</description>
        /// </item>
	/// <item>
        /// <term>Ungroup</term>
        /// <description>Ungroup by current column.</description>
        /// </item>
	/// <item>
        /// <term>SortAscending</term>
        /// <description>Sort the current column in ascending order.</description>
        /// </item>
	/// <item>
        /// <term>SortDescending</term>
        /// <description>Sort the current column in descending order.</description>
        /// </item>
	/// <item>
        /// <term>Filter</term>
        /// <description>Filter options will show based on <c>FilterSettings</c> property like checkbox filter, excel filter, menu filter.</description>
        /// </item>
        /// </list>
        /// </value>
        /// <remarks>
        /// To disable column menu for a particular column by defining the <see cref="Syncfusion.Blazor.Grids.GridColumn.ShowColumnMenu"/> property as <c>false</c>.
        /// To customize the default menu items, define the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.ColumnMenuItems"/> property with the required items.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("columnMenuItems")]
        public object? ColumnMenuItems { get; set; }

        private object? _columnMenuItems { get; set; }

        /// <summary>
        /// Gets or sets the column query mode for retrieving data from the data source.
        /// </summary>
        /// <value>
        /// The default value is <see cref="ColumnQueryModeType.All"/>.
        /// </value>
        /// <remarks>
        /// The available options are:
        /// <list type="bullet">
        /// <item>
        /// <term>All</term>
        /// <description>Retrieves the entire data source.</description>
        /// </item>
        /// <item>
        /// <term>Schema</term>
        /// <description>Retrieves data for all the defined columns in the grid from the data source.</description>
        /// </item>
        /// <item>
        /// <term>ExcludeHidden</term>
        /// <description>Retrieves data only for visible columns of the grid from the data source.</description>
        /// </item>
        /// </list>
        /// </remarks>
        [Parameter]
        [DefaultValue(ColumnQueryModeType.All)]
        [JsonPropertyName("columnQueryMode")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ColumnQueryModeType ColumnQueryMode { get; set; } = ColumnQueryModeType.All;

        private ColumnQueryModeType _columnQueryMode { get; set; }

        /// <summary>
        /// Defines the schema of the data source for the grid and allows you to add, customize, and remove columns in the grid.
        /// </summary>
        /// <value>
        /// A list of <see cref="Syncfusion.Blazor.Grids.GridColumn"/> instances that define the columns in the grid.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.Blazor.Grids.GridColumn"/> class provides various properties to customize the grid columns, such as setting the column header text,
        /// specifying the data field to bind the column to, and formatting the cell values in the column. If the Columns declaration is empty or undefined,
        /// the columns are automatically generated from the data source.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("columns")]
        public List<GridColumn>? Columns { get; set; }

        private List<GridColumn>? _columns { get; set; }

        /// <summary>
        /// Gets or sets a collection of item to be displayed in the context menu when you right-click on a row or cell in the Grid.
        /// </summary>
        /// <value>
        /// The context menu items can be both built-in and custom. The available built-in items are:
        /// <list type="bullet">
        /// <item>
        /// <term>AutoFitAll</term>
        /// <description>Autofit the size of all columns width.</description>
        /// </item>
        /// <item>
        /// <term>AutoFit</term>
        /// <description>Autofit the current column width.</description>
        /// </item>
        /// <item>
        /// <term>Group</term>
        /// <description>Group by current column.</description>
        /// </item>
        /// <item>
        /// <term>Ungroup</term>
        /// <description>Ungroup by current column.</description>
        /// </item>
        /// <item>
        /// <term>Edit</term>
        /// <description>Edit the current record.</description>
        /// </item>
        /// <item>
        /// <term>Delete</term>
        /// <description>Delete the current record.</description>
        /// </item>
        /// <item>
        /// <term>Save</term>
        /// <description>Save the edited record.</description>
        /// </item>
        /// <item>
        /// <term>Cancel</term>
        /// <description>Cancel the edited state.</description>
        /// </item>
        /// <item>
        /// <term>Copy</term>
        /// <description>Copy the selected records.</description>
        /// </item>
        /// <item>
        /// <term>PdfExport</term>
        /// <description>Export the grid as PDF format.</description>
        /// </item>
        /// <item>
        /// <term>ExcelExport</term>
        /// <description>Export the grid as Excel format.</description>
        /// </item>
        /// <item>
        /// <term>CsvExport</term>
        /// <description>Export the grid as CSV format.</description>
        /// </item>
        /// <item>
        /// <term>SortAscending</term>
        /// <description>Sort the current column in ascending order.</description>
        /// </item>
        /// <item>
        /// <term>SortDescending</term>
        /// <description>Sort the current column in descending order.</description>
        /// </item>
        /// <item>
        /// <term>FirstPage</term>
        /// <description>Go to the first page.</description>
        /// </item>
        /// <item>
        /// <term>PrevPage</term>
        /// <description>Go to the previous page.</description>
        /// </item>
        /// <item>
        /// <term>LastPage</term>
        /// <description>Go to the last page.</description>
        /// </item>
        /// <item>
        /// <term>NextPage</term>
        /// <description>Go to the next page.</description>
        /// </item>
        /// </list>
        /// </value>
        /// <remarks>
        /// The context menu can also be disabled for specific columns using the <see cref="Syncfusion.Blazor.Grids.ContextMenuOpenEventArgs{TValue}.Cancel"/> property.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("contextMenuItems")]
        public object? ContextMenuItems { get; set; }

        private object? _contextMenuItems { get; set; }

        /// <summary>
        /// Gets or sets the current action details.
        /// </summary>
        /// <exclude/>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("currentAction")]
        public ActionArgs? CurrentAction { get; set; }

        private ActionArgs? _currentAction { get; set; }

        /// <summary>
        /// Gets or sets the data source for the grid rows.
        /// </summary>
        /// <remarks>
        /// Use this property to set the data source for the grid. This property expects an IEnumerable of TValue, where TValue represents the type of the data object.
        /// To consume data from a remote service or custom adaptor, use the <see cref="Syncfusion.Blazor.Data.SfDataManager"/> component.
        /// </remarks>
        [Parameter]
        [JsonIgnore]
        [JsonPropertyName("dataSource")]
        public IEnumerable<TValue>? DataSource { get; set; }

        private IEnumerable<TValue>? _dataSource { get; set; }

        /// <summary>
        /// Gets or sets the event that occurs when the data source changes.
        /// </summary>
        /// <remarks>
        /// Use this event to handle changes to the data source in the grid.This event is raised when the data source is updated or changed. 
        /// The event handler receives an IEnumerable of TValue, where TValue represents the type of the data object.
        /// </remarks>
        [Parameter]
        [JsonIgnore]
        public EventCallback<IEnumerable<TValue>> DataSourceChanged { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the Grid is rendered with a full-screen adaptive UI layout for some grid actions, such as filtering, sorting, and CRUD operations.
        /// </summary>
        /// <value>
        /// <c>true</c>, The grid is render adaptive dialogs such that they will fit the full screen to provide a better user experience on smaller screen devices.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// For rendering the adaptive UI layout in grid only for mobile devices then need to set the <see cref="AdaptiveUIMode"/> property value as "Mobile".
        /// Also to view the the rows vertically with headers positioned in the same row instead of at the top then need to set the <see cref="RowRenderingMode"/> property value as "Vertical".
	/// </remarks>
        [Parameter]
        [JsonIgnore]
        public bool EnableAdaptiveUI { get; set; }

        /// <summary>
        /// Gets or sets the Adaptive mode which used to render Grid component with adaptive UI layout in the specified mode.
        /// </summary>
        /// <value>
        /// One of the <see cref="AdaptiveMode"/> enumeration that specifies the Adaptive Mode. The default value is <see cref="AdaptiveMode.Both"/>.
        /// </value>
        /// <remarks>
        /// The <c>AdaptiveMode</c> property can be set to one of the following values:
	/// <list type="bullet">
        /// <item>
        /// <term>Both</term>
        /// <description>Renders adaptive layout for both mobile and desktop devices.</description>
        /// </item>
	/// <item>
        /// <term>Mobile</term>
        /// <description>Renders adaptive layout only for smaller devices.</description>
        /// </item>
	/// <item>
        /// <term>Desktop</term>
        /// <description>Renders adaptive layout only for desktop devices.</description>
        /// </item>
	/// </list>
	/// When set to <c>true</c> the Grid is rendered with a full-screen adaptive UI layout for some grid actions, such as filtering, sorting, and CRUD operations.
        /// </remarks>
        [Parameter]
        [DefaultValue(AdaptiveMode.Both)]
        [JsonPropertyName("adaptiveUIMode")]
        public AdaptiveMode AdaptiveUIMode { get; set; }

        /// <summary>
        /// Gets or sets the instance of <see cref="Syncfusion.Blazor.Grids.GridEditSettings"/> that configures the editing behavior of the grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridEditSettings"/>.
        /// </value>
        /// <remarks>
        ///  Customize the editing behavior of grid by <see cref="Syncfusion.Blazor.Grids.GridEditSettings"/> class provides various properties to configure editing operations, such as allowing or disallowing editing for specific columns, enabling inline or dialog editing, and setting validation rules for edited data.  
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("editSettings")]
        public GridEditSettings? EditSettings { get; set; }

        private GridEditSettings? _editSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid will render with alternative row styling for improved readability.
        /// </summary>
        /// <value>
        /// <c>false</c> the grid rows are rendered without any alternative row styling.
        /// The default value is <c>true</c>.
        /// </value>
        /// <remarks>
        /// When set to <c>true</c>, the grid will apply the <c>e-altrow</c> CSS class to alternative tr element of grid rows. This can be useful for styling alternating rows differently for improved readability.
        /// The alternative row styling is customized by simply overriding the <c>e-altrow</c> class in application end.
	/// </remarks>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("enableAltRow")]
        public bool EnableAltRow { get; set; } = true;

        private bool _enableAltRow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the AutoFill feature is enabled, which allows copying and pasting data from selected cells to other cells by dragging the AutoFill icon. 
        /// </summary>
        /// <value>
        /// <c>true</c>, The auto fill icon will be displayed on cell selection for copying and pasting the data to other cells while dragging the icon.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// To use the AutoFill feature, the <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.Mode"/> property must be set to "Cell",
        /// the <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.CellSelectionMode"/> property must be set to "Box", and <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> property must be set as "Batch".
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("enableAutoFill")]
        public bool EnableAutoFill { get; set; }

        private bool _enableAutoFill { get; set; }

        /// <summary>
        ///Gets or sets a value indicating whether the Grid will render with the columns which are visible within the view-port and load the subsequent columns on horizontal scrolling.
        /// </summary>
        /// <value>
        /// <c>true</c>, It helps to load large amount of columns in Grid by rendering only the columns that are visible within the view-port and loading subsequent columns on horizontal scrolling.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.Blazor.Grids.GridColumn.Width"/> is required for column virtualization. If <see cref="Syncfusion.Blazor.Grids.GridColumn.Width"/> is not defined for any of the column then by default 200px is considered for that column.
        /// The collapsed or expanded state will persist only for local dataSource while scrolling.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("enableColumnVirtualization")]
        public bool EnableColumnVirtualization { get; set; }

        private bool _enableColumnVirtualization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the masked row or placeholder until the data's are loaded in the grid during virtualization.
        /// </summary>
        /// <value>
        /// <c>true</c> the DataGrid will display a masked row when the data is not readily available to show in the grid.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// It is must to set <c>EnableVirtualization</c> or <c>EnableColumnVirtualization</c> property value as true to use this mask row feature.
	/// The Virtual mask row is supported for column virtualization too.
        /// </remarks>
        [Parameter]
        [JsonPropertyName("enableVirtualMaskRow")]
        public bool EnableVirtualMaskRow { get; set; }

        private bool _enableVirtualMaskRow { get; set; }

        /// <summary>
        /// Gets or sets the number of additional items to be render in the DOM before and after the visible items (based on <see cref="Syncfusion.Blazor.Grids.GridPageSettings.PageSize"/>) during virtual scrolling and initial rendering.
        /// </summary>
        /// <value>
        /// The number of additional items to pre-render before and after the visible items (based on <c>PageSize</c>).
        /// The default value is 0, indicating no overscan.
        /// </value>
        /// <remarks>
        /// Adjusting this property can enhance scrolling performance and optimize rendering, especially for large datasets in a virtualized grid.
        /// By pre-rendering a buffer of extra items that are not yet visible, the component minimizes the need for frequent re-rendering while scrolling.
        /// This optimization results in a smoother and more responsive user experience.
        /// If the <c>PageSize</c> is not explicitly provided, it will be calculated based on the viewport height to ensure an optimal user experience.
        /// </remarks>
        [Parameter]
        [JsonPropertyName("overscanCount")]
        public int OverscanCount { get; set; }

        private int _overscanCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the hovering effect is enabled while hover on the grid rows.
        /// </summary>
        /// <value>
        /// <c>false</c>, the rows are not highlighted while hover on the grid rows.
        /// The default value is <c>true</c>.
        /// </value>
        /// <remarks>
        /// When it is set to true, the CSS class "e-hover" will be applied to the row when it is hovered over. This can be used to style the row differently and provide visual feedback to the user.
        /// The row hovering style is customized by simply overriding the "e-hover" class in the application end.
        /// </remarks>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("enableHover")]
        public bool EnableHover { get; set; } = true;

        private bool _enableHover { get; set; }

        /// <summary>
        /// Gets or sets a value indicates whether enables or disables the persistence of component's state while page reloads.
        /// </summary>
        /// <value>
        /// <c>true</c>, The grid state such as column order, column width, sort information etc. is stored in the <c>window.localStorage</c> when the component is disposed.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// If the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.ID"/> property is set for the Grid, then the state will be persisted based on this ID. Otherwise, it will use a default ID.
        /// Users can also store grid state in a database instead of the browser's local storage by using the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.GetPersistDataAsync"/> method.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("enablePersistence")]
        public bool EnablePersistence { get; set; }

        private bool _enablePersistence { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the component in right to left (RTL) direction.
        /// </summary>
        /// <value>
        /// <c>true</c>, the component should rendered in RTL direction.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// When the <c>true</c> value is set, the component content will be aligned to the right of the page and the sorting, filtering, and paging icons will be rendered in the right-to-left (RTL) direction.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("enableRtl")]
        public bool EnableRtl { get; set; }

        private bool _enableRtl { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the grid header should remain fixed while scrolling the grid content vertically.
        /// </summary>
        /// <value>
        /// <c>true</c> to make the column headers sticky while scrolling the grid content vertically.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// It is suitable only for single headers.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("enableStickyHeader")]
        public bool EnableStickyHeader { get; set; }

        private bool _enableStickyHeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether virtualization is enabled in the DataGrid which means loads the data in on-demand basis while scrolling the grid vertically.
        /// </summary>
        /// <value>
        /// <c>true</c> to enable virtualization in the DataGrid. The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// When set to <c>true</c>, virtualization is enabled, and the DataGrid will load only the rows that are currently visible in the viewport, which can significantly improve the performance and responsiveness of the DataGrid when dealing with large datasets. 
        /// Subsequent rows will be loaded dynamically as the user scrolls vertically through the DataGrid. 
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("enableVirtualization")]
        public bool EnableVirtualization { get; set; }

        private bool _enableVirtualization { get; set; }

        /// <summary>
        /// Gets or sets instance of <see cref="Syncfusion.Blazor.Grids.GridFilterSettings"/> which configures the filtering behavior of grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridFilterSettings"/>.
        /// </value>
        /// <remarks> 
        /// <see cref="Syncfusion.Blazor.Grids.GridFilterSettings"/> class provides 
        /// various properties to customize filtering operations. 
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("filterSettings")]
        public GridFilterSettings? FilterSettings { get; set; }

        private GridFilterSettings? _filterSettings { get; set; }

        /// <summary>
        /// Gets or sets the number of columns to be frozen in the DataGrid.
        /// </summary>
        /// <value>
        /// The number of columns that need to be frozen in the DataGrid. The default value is 0.
        /// </value>
        /// <remarks>
        /// Use this property to freeze a specific number of columns in the DataGrid, so that they remain fixed on the left side of the grid when the user scrolls horizontally through the grid. Note that the frozen columns must be within the view port of the DataGrid in order to be visible.
        /// For example, to freeze the first two columns of the grid, you can set the <c>FrozenColumns</c> property to 2. 
        ///To freeze a specific column, set its <see cref="Syncfusion.Blazor.Grids.GridColumn.IsFrozen"/> property to <c>true</c> and also use the <see cref="Syncfusion.Blazor.Grids.FreezeDirection"/> property to set the direction of frozen columns.
        /// </remarks>
        [Parameter]
        [DefaultValue(0)]
        [JsonPropertyName("frozenColumns")]
        public int FrozenColumns { get; set; }

        private int _frozenColumns { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can move the frozen line by dragging and dropping it in the Syncfusion Blazor DataGrid columns.
        /// </summary>
        /// <value>
        /// <c>true</c>, User can adjust the freeze line. The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// When this property is enabled, the user can adjust the number of frozen columns by dragging and dropping the freeze bar between columns.
        /// If frozen columns are not specified, the frozen column separator will be displayed at the left and right edges of the Grid, and the user can dynamically adjust the number of frozen columns by dragging the separator.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowFreezeLineMoving")]
        public bool AllowFreezeLineMoving { get; set; }
        private bool _allowFreezeLineMoving { get; set; }

        /// <summary>
        /// Gets or sets the number of rows need to be frozen in the DataGrid.
        /// </summary>
        /// <value>
        /// The number of rows that needs to be frozen DataGrid. The default value is 0.
        /// </value>
        /// <remarks>
        /// The frozen rows will always be displayed at the top of the grid content, and will not move when the user scrolls vertically.
        /// Note that the frozen rows must be within the view port of the DataGrid.
        /// For example, to freeze the first two rows of the grid, you can set the <c>FrozenRows</c> property to 2.
        /// </remarks>
        [Parameter]
        [DefaultValue(0)]
        [JsonPropertyName("frozenRows")]
        public int FrozenRows { get; set; }

        private int _frozenRows { get; set; }

        /// <summary>
        /// Gets or sets the visibility of border lines of rows and columns in the grid. 
        /// </summary>
        /// <value>
        /// The default value is <see cref="GridLine.Default"/>.
        /// </value>
        /// <remarks>
        /// The available modes are,
	/// <list type="bullet">
        /// <item>
        /// <term>Both</term>
        /// <description>Displays both the horizontal and vertical grid lines.</description>
        /// </item>
	/// <item>
        /// <term>None</term>
        /// <description>No grid lines are displayed.</description>
        /// </item>
	/// <item>
        /// <term>Horizontal</term>
        /// <description>Displays the horizontal grid lines only.</description>
        /// </item>
	/// <item>
        /// <term>Vertical</term>
        /// <description>Displays the vertical grid lines only.</description>
        /// </item>
	/// <item>
        /// <term>Default</term>
        /// <description>Displays DataGrid lines based on the theme.</description>
        /// </item>
	/// </list>    
        /// </remarks>
        [Parameter]
        [DefaultValue(GridLine.Default)]
        [JsonPropertyName("gridLines")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GridLine GridLines { get; set; } = GridLine.Default;

        private GridLine _gridLines { get; set; }

        /// <summary>
        /// Gets or sets instance of <see cref="Syncfusion.Blazor.Grids.GridGroupSettings"/> which configures the grouping behavior of grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridGroupSettings"/>.
        /// </value>
        /// <remarks> 
        /// <see cref="Syncfusion.Blazor.Grids.GridGroupSettings"/> class provides various properties to customize grouping operations. 
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("groupSettings")]
        public GridGroupSettings? GroupSettings { get; set; }

        private GridGroupSettings? _groupSettings { get; set; }

        /// <summary>
        /// Gets or sets the scrollable height of the grid content.
        /// </summary>
        /// <value>
        /// The default value is "auto".
        /// </value>
        /// <remarks>
        /// You can assign the height using pixel and percentage values such as 100px, 100%, etc. 
        /// If the height is set to "auto", the grid height will be automatically adjusted based on the number of rows displayed.
        /// </remarks>
        [Parameter]
        [DefaultValue("auto")]
        [JsonPropertyName("height")]
        public string Height { get; set; } = "auto";

        private string? _height { get; set; }

        /// <summary>
        /// Gets or sets the hierarchy grid print mode, which defines how the grid and child grids are printed based on this specific mode.
        /// </summary>
        /// <value>
        /// The default value is <see cref="HierarchyGridPrintMode.Expanded"/>.
        /// </value>
        /// <remarks>
        /// The available modes are:
	/// <list type="bullet">
        /// <item>
        /// <term>Expanded</term>
        /// <description>Prints the master grid with expanded child grids.</description>
        /// </item>
	/// <item>
        /// <term>All</term>
        /// <description>Prints the master grid with all the child grids.</description>
        /// </item>
	/// <item>
        /// <term>None</term>
        /// <description>Prints the master grid alone without any child grids.</description>
        /// </item>
	/// </list>
        /// </remarks>
        [Parameter]
        [DefaultValue(HierarchyGridPrintMode.Expanded)]
        [JsonPropertyName("hierarchyPrintMode")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HierarchyGridPrintMode HierarchyPrintMode { get; set; } = HierarchyGridPrintMode.Expanded;

        private HierarchyGridPrintMode _hierarchyPrintMode { get; set; }

        /// <summary>
        /// Gets or sets instance of <see cref="Syncfusion.Blazor.Grids.GridPageSettings"/> which configures the pager behavior of grid.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Syncfusion.Blazor.Grids.GridPageSettings"/> class.
        /// </value>
        /// <remarks> 
        /// The <see cref="Syncfusion.Blazor.Grids.GridPageSettings"/> class provides various properties to customize paging operations, such as the number of pages, page size, and current page number. 
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("pageSettings")]
        public GridPageSettings? PageSettings { get; set; }

        private GridPageSettings? _pageSettings { get; set; }

        /// <summary>
        /// Gets or sets the hierarchy grid print mode, which defines how the grid and child grids are printed.
        /// </summary>
        /// <value>
        /// The default value is <see cref="HierarchyGridPrintMode.Expanded"/>.
        /// </value>
        /// <remarks>
        /// The available modes are:
        /// * Expanded: Prints the master grid with expanded child grids.
        /// * All: Prints the master grid with all the child grids.
        /// * None: Prints the master grid alone without any child grids.
        /// </remarks>
        [Parameter]
        [DefaultValue(PrintMode.AllPages)]
        [JsonPropertyName("printMode")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PrintMode PrintMode { get; set; } = PrintMode.AllPages;

        private PrintMode _printMode { get; set; }

        /// <summary>
        /// Gets or sets the external query that will be executed along with data processing.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Data.Query"/> class that represents the query parameters that will be sent to the server.
        /// The default value is null.
        /// </value>
        /// <remarks>
        /// This property can be used to add additional parameters to the data request by using the <see cref="Syncfusion.Blazor.Data.Query.AddParams(string, object)"/> method.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("query")]
        public Syncfusion.Blazor.Data.Query? Query { get; set; }

        private Syncfusion.Blazor.Data.Query? _query { get; set; }

        /// <summary>
        /// Gets or sets instance of <see cref="Syncfusion.Blazor.Grids.GridRowDropSettings"/> which configures the row drop behavior of grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridRowDropSettings"/>.
        /// </value>
        /// <remarks> 
        /// <see cref="Syncfusion.Blazor.Grids.GridRowDropSettings"/> class provides various properties to customize row drop operations. 
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("rowDropSettings")]
        public GridRowDropSettings? RowDropSettings { get; set; }

        private GridRowDropSettings? _rowDropSettings { get; set; }

        /// <summary>
        /// Gets or sets the height of grid rows.
        /// </summary>
        /// <value>
        /// The default value is differ based on the theme.
        /// </value>
        /// <remarks>
        /// The row height can be specified in pixels or as a percentage of the Grid's overall height. 
        /// For example, to set the row height to 50 pixels, you can set <c>RowHeight</c> property to 50. 
        /// To set the row height to 10% of the Grid's overall height, you can set <c>RowHeight</c> property to "10%". 
        /// </remarks>
        [Parameter]
        [DefaultValue(default(double))]
        [JsonPropertyName("rowHeight")]
        public double RowHeight { get; set; } = default;

        private double _rowHeight { get; set; }

        /// <summary>
        /// Gets or sets the row rendering mode in the grid.
        /// </summary>
        /// <value>
        /// The default value is <see cref="RowDirection.Horizontal"/>.
        /// </value>
        /// <remarks>
        /// The available modes are:
	/// <list type="bullet">
        /// <item>
        /// <term>Horizontal</term>
        /// <description>Displays the data rows in horizontal direction.</description>
        /// </item>
	/// <item>
        /// <term>Vertical</term>
        /// <description>Displays the data rows in vertical direction.</description>
        /// </item>
	/// </list>
        /// The <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.RowRenderingMode"/> property is rendered on the adaptive layout based on the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AdaptiveUIMode"/> property.
        /// Setting this property value as "Vertical" rows are rendered vertically with headers in the same row which suits better to view grid in small screens.
        /// In vertical row rendering mode, limited features are supported like filtering, sorting, dialog editing, selection, searching and row virtualization.
        /// </remarks>
        [Parameter]
        [JsonIgnore]
        public RowDirection RowRenderingMode { get; set; }

        /// <summary>
        /// Gets or sets instance of <see cref="Syncfusion.Blazor.Grids.GridSearchSettings"/> which configures the search behavior of grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridSearchSettings"/>.
        /// </value>
        /// <remarks> 
        /// <see cref="Syncfusion.Blazor.Grids.GridSearchSettings"/> class provides various properties to customize searching operations. 
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("searchSettings")]
        public GridSearchSettings? SearchSettings { get; set; }

        private GridSearchSettings? _searchSettings { get; set; }

        /// <summary>
        /// Gets or sets the index of the row that is selected initially while rendering the grid component.
        /// </summary>
        /// <value>
        /// The default value is <c>-1</c>. Initially, no rows are selected in the data grid. 
        /// </value>
        /// <remarks>
        /// This property allows you to select a row at initial rendering. It can also be used to programmatically select a row.
        /// You can select a row initially by setting the value of this property to the index of the row to be selected.
        /// </remarks>
        [Parameter]
        [DefaultValue(-1)]
        [JsonPropertyName("selectedRowIndex")]
        public int SelectedRowIndex { get; set; } = -1;

        private int _selectedRowIndex { get; set; }

        /// <summary>
        /// Gets or sets instance of <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings"/> which configures the selection behavior of the grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings"/>.
        /// </value>
        /// <remarks> 
        /// The <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings"/> class provides various properties to customize the selection operations of the grid, such as mode, type, persistSelection, and more.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("selectionSettings")]
        public GridSelectionSettings? SelectionSettings { get; set; }

        private GridSelectionSettings? _selectionSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable the column chooser feature to dynamically show or hide the grid columns.
        /// </summary>
        /// <value>
        /// <c>true</c>, allows users to show or hide columns dynamically by using the column chooser feature.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// You can hide the column names in column chooser by defining the <see cref="Syncfusion.Blazor.Grids.GridColumn.ShowInColumnChooser"/>  property as <c>false</c>.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("showColumnChooser")]
        public bool ShowColumnChooser { get; set; }

        private bool _showColumnChooser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable the column menu options in each columns.
        /// </summary>
        /// <value>
        /// <c>true</c>, then it will enable the column menu options in each columns.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// To disable column menu for a particular column by defining the <see cref="Syncfusion.Blazor.Grids.GridColumn.ShowColumnMenu"/> property as false.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("showColumnMenu")]
        public bool ShowColumnMenu { get; set; }

        private bool _showColumnMenu { get; set; }

        /// <summary>
        /// Gets or sets instance of <see cref="Syncfusion.Blazor.Grids.GridSortSettings"/> which configures the sorting behavior of grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridSortSettings"/>.
        /// </value>
        /// <remarks> 
        /// <see cref="Syncfusion.Blazor.Grids.GridSortSettings"/> class provides various properties to customize sort operations. 
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("sortSettings")]
        public GridSortSettings? SortSettings { get; set; }

        private GridSortSettings? _sortSettings { get; set; }

        /// <summary>
        /// Gets or sets instance of <see cref="Syncfusion.Blazor.Grids.GridTextWrapSettings"/> which configures the text wrap behavior of grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridTextWrapSettings"/>.
        /// </value>
        /// <remarks> 
        /// <see cref="Syncfusion.Blazor.Grids.GridTextWrapSettings"/> class provides various properties to customize text wrap operations. 
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("textWrapSettings")]
        public GridTextWrapSettings? TextWrapSettings { get; set; }

        private GridTextWrapSettings? _textWrapSettings { get; set; }

        /// <summary>
        /// Gets or sets the ToolBar items of the Grid.
        /// </summary>
        /// <value>
        /// The available built-in items are:
        /// <list type="bullet">
        /// <item>
        /// <term>Add</term>
        /// <description>Adds a new record.</description>
        /// </item>
	/// <item>
        /// <term>Add</term>
        /// <description>Adds a new record.</description>
        /// </item> 
	/// <item>
        /// <term>Edit</term>
        /// <description>Edits the selected record.</description>
        /// </item> 
	/// <item>
        /// <term>Update</term>
        /// <description>Updates the edited record.</description>
        /// </item> 
	/// <item>
        /// <term>Delete</term>
        /// <description>Deletes the selected record.</description>
        /// </item> 
        /// <item>
        /// <term>Cancel</term>
        /// <description>Cancels the edit state.</description>
        /// </item> 
	/// <item>
        /// <term>Search</term>
        /// <description>Searches the records by the given key.</description>
        /// </item> 
	/// <item>
        /// <term>Print</term>
        /// <description>Prints the datagrid.</description>
        /// </item> 
	/// <item>
        /// <term>ExcelExport</term>
        /// <description>Exports the datagrid to Excel file format.</description>
        /// </item> 
	/// <item>
        /// <term>PdfExport</term>
        /// <description>Exports the datagrid to PDF file format.</description>
        /// </item> 
	/// <item>
        /// <term>CsvExport</term>
        /// <description>Exports the datagrid to CSV file format.</description>
        /// </item>       
        /// </list>
        /// </value>
        /// <remarks>
        /// In some cases, you may want to use a custom toolbar instead of the default one. In such cases, you can use the
        /// <see cref="Syncfusion.Blazor.Grids.GridTemplates.ToolbarTemplate"/> property to provide a custom toolbar template
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("toolbar")]
        public object? Toolbar { get; set; }

        private object? _toolbar { get; set; }

        /// <summary>
        /// Gets or sets the width of the Grid.
        /// </summary>
        /// <value>
        /// The default value is "auto".
        /// </value>
        /// <remarks>
        /// The <see cref="Width"/> property can be assigned with pixel and percentage values such as 100px, 100% etc.
        /// When the total column width exceeds the specified value, a horizontal scrollbar will be displayed to allow the user to scroll through the data.
        /// </remarks>
        [Parameter]
        [DefaultValue("auto")]
        [JsonPropertyName("width")]
        public string Width { get; set; } = "auto";

        private string? _width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Grid loads the next set of data's on-demand, when the vertical scrollbar reaches the end of the scroller. This feature enables loading large datasets into the Grid without the need of traditional pagination.
        /// </summary>
        /// <value>
        /// Set <c>true</c> to enable infinite scroll by loading data when vertical scroll bar reaches the end of scrollbar.
        /// The default value is <c>false</c>.        
        /// </value>
        /// <remarks>
        /// During the initial rendering, the grid loads a block of data based on the <see cref="Syncfusion.Blazor.Grids.GridInfiniteScrollSettings.InitialBlocks"/> property.  The default value of <c>InitialBlocks</c> is 3.
        /// After that the buffering data's are loaded based on the page size or rows which are rendered within the provided height.
        /// Subsequently, as the user scrolls to the end of the grid, additional blocks of data will be loaded in on-demand.
        /// In the default Infinite Scrolling mode, a block of data accumulates every time the scrollbar reaches the end. However, in the cache mode, blocks of data are rendered based on the <c>MaximumBlocks</c> setting. If the number of blocks exceeds this limit during scrolling, the Grid removes rows from the DOM to accommodate the new block of data.
        /// <c>EnableCache</c> and <c>MaximumBlocks</c> can be configured through the properties of the <see cref="Syncfusion.Blazor.Grids.GridInfiniteScrollSettings"/> class.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("enableInfiniteScrolling")]
        public bool EnableInfiniteScrolling { get; set; }
        private bool _enableInfiniteScrolling { get; set; }

        /// <summary>
        /// Gets or sets an instance of <see cref="Syncfusion.Blazor.Grids.GridInfiniteScrollSettings"/> which configures the infinite scrolling behavior of the grid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.GridInfiniteScrollSettings"/>.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.Blazor.Grids.GridInfiniteScrollSettings"/> class provides various properties to customize the infinite scrolling operation of the grid. 
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("infiniteScrollSettings")]
        public GridInfiniteScrollSettings? InfiniteScrollSettings { get; set; }
        private GridInfiniteScrollSettings? _infiniteScrollSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the cell elements of hidden columns in the DOM.
        /// </summary>
        /// <value>
        /// <c>true</c> to render the hidden column elements in the DOM; <c>false</c> to prevent their rendering.  
        /// The default value is <c>true</c>.
        /// </value>
        /// <remarks>
        /// By default, hidden columns are rendered in the DOM and hidden using CSS.  
        /// Setting this property to <c>false</c> removes the corresponding elements from the DOM,  
        /// improving performance when handling a large number of hidden columns.
        /// </remarks>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("shouldRenderHiddenColumns")]
        public bool ShouldRenderHiddenColumns { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that determines whether the columns should automatically fit based on the given width.
        /// </summary>
        /// <value>
        /// <c>true</c> to enforce column width as defined in <see cref="GridColumn.Width"/>; otherwise, <c>false</c>.  
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// If the total width of the columns is less than the width of the Grid, white space will be displayed.
        /// If any column width is undefined, it will automatically adjust to fill the grid width, even if <c>AutoFit</c> is enabled.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("autoFit")]
        public bool AutoFit { get; set; } = false;
        private bool _autoFit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tooltips are enabled when hovering over grid cells and headers.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> that determines whether tooltips are shown on hover. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// When set to <see langword="true"/>, tooltips are displayed when hovering over grid headers and content cells. These tooltips show either the formatted value or the raw data of the cell.
        /// </para>
        /// <para>
        /// For templated columns or special display types such as <c>DisplayAsCheckbox</c>, the tooltip displays the cells bound value.
        /// </para>
        /// <para>
        /// Tooltips are not shown for non-data cell elements such as group captions, aggregate rows, or when the cell value is <c>null</c> or an empty string.
        /// </para>
        /// <para>
        /// To customize the tooltip content, use the <see cref="Syncfusion.Blazor.Grids.GridTemplates.TooltipTemplate"/> parameter within the <see cref="Syncfusion.Blazor.Grids.GridTemplates"/> component.
        /// </para>
        /// </remarks>
        /// <example>
        /// Demonstrates how to enable <see cref="ShowTooltip"/> to display tooltips on grid cells and headers.
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" ShowTooltip="true">
        /// <GridColumns>
        /// <GridColumn Field="CustomerID" HeaderText="Customer ID" Width="150"></GridColumn>
        /// </GridColumns>
        /// </SfGrid>
        ///
        /// @code {
        /// public class Order
        /// {
        /// public string CustomerID { get; set; }
        /// }
        /// }
        /// ]]></code>
        /// </example>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("showTooltip")]
        public bool ShowTooltip { get; set; }
        private bool _showTooltip { get; set; }

        /// <summary>
        /// Gets or sets the automatic cell spanning mode for the entire grid.
        /// </summary>
        /// <value>
        /// A <see cref="Syncfusion.Blazor.Grids.AutoSpanMode"/> enumeration that dictates how identical cell values are merged.
        /// The default value is <see cref="Syncfusion.Blazor.Grids.AutoSpanMode.None"/>. Supported options include
        /// <see cref="Syncfusion.Blazor.Grids.AutoSpanMode.None"/>, <see cref="Syncfusion.Blazor.Grids.AutoSpanMode.Row"/>,
        /// <see cref="Syncfusion.Blazor.Grids.AutoSpanMode.Column"/>, and <see cref="Syncfusion.Blazor.Grids.AutoSpanMode.HorizontalAndVertical"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// Serves as the master configuration for automatic cell spanning, enabling the grid to consolidate adjacent cells that share the same content.
        /// Each column can optionally override this behavior via <see cref="Syncfusion.Blazor.Grids.GridColumn.AutoSpan"/>;
        /// a <see langword="null"/> column value inherits the grid-level choice.
        /// </para>
        /// <para>
        /// The effective spanning mode for a column is the intersection of <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AutoSpan"/> 
        /// and <see cref="Syncfusion.Blazor.Grids.GridColumn.AutoSpan"/>. A column cannot enable
        /// <see cref="Syncfusion.Blazor.Grids.AutoSpanMode.Row"/> or <see cref="Syncfusion.Blazor.Grids.AutoSpanMode.Column"/> 
        /// if the grid-level setting disables that direction.
        /// </para>
        /// <para>
        /// Cell spanning always executes in two passes: horizontal (row) merging runs first, followed by vertical (column) merging.
        /// This order applies to data rows, grouped rows, and summary rows, ensuring that vertical merges respect any horizontal spans already created.
        /// </para>
        /// <para>
        /// This property controls automatic merging of identical values. Manual merging or unmerging of cells based on cell index can be performed
        /// using the appropriate methods and is not affected by this property's enum value, even if <see cref="Syncfusion.Blazor.Grids.AutoSpanMode.None"/> is set.
        /// </para>
        /// <para>
        /// Grid-level behaviors for each option are outlined below (assuming columns do not override the setting):
        /// </para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Grid.AutoSpanMode</term>
        ///     <description>Behavior</description>
        ///   </listheader>
        ///   <item>
        ///     <term><see cref="Syncfusion.Blazor.Grids.AutoSpanMode.None"/></term>
        ///     <description>Prevents all automatic merging. Every cell renders independently, which favors maximum performance and clarity for unique data.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Syncfusion.Blazor.Grids.AutoSpanMode.Row"/></term>
        ///     <description>Merges identical values across columns within the same row. Horizontal spanning occurs once per row; vertical merging never occurs.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Syncfusion.Blazor.Grids.AutoSpanMode.Column"/></term>
        ///     <description>Merges identical values down the column across adjacent rows. Vertical spanning occurs after the horizontal pass but horizontal merging is disabled.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Syncfusion.Blazor.Grids.AutoSpanMode.HorizontalAndVertical"/></term>
        ///     <description>Runs the horizontal pass first and the vertical pass second, producing combined spans where repetitive values exist in both directions.</description>
        ///   </item>
        /// </list>
        /// <para>
        /// Enabling <see cref="Syncfusion.Blazor.Grids.AutoSpanMode.Row"/> or <see cref="Syncfusion.Blazor.Grids.AutoSpanMode.HorizontalAndVertical"/> 
        /// increases rendering work because the grid scans cell content to form spans. For large datasets, consider virtualization
        /// (see <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.EnableVirtualization"/>) or apply spanning only on targeted columns.
        /// </para>
        /// <para>
        /// <strong>Note:</strong> When the grid is bound to a remote data service, cell merging is applied only to the data currently loaded in the viewport.
        /// Identical values that appear at the end of one page and the beginning of the next will not be merged because merging is limited to the visible data set.
        /// </para>
        /// </remarks>
        /// <example>
        /// Demonstrates enabling horizontal-first, vertical-second spanning at the grid level.
        /// <code><![CDATA[
        /// <SfGrid Data="@Orders" AutoSpan="AutoSpanMode.Row">
        ///     <GridColumns>
        ///         <GridColumn Field="@nameof(Order.OrderID)" HeaderText="Order ID" Width="120" TextAlign="Syncfusion.Blazor.Grids.TextAlign.Right" />
        ///         <GridColumn Field="@nameof(Order.CustomerName)" HeaderText="Customer Name" Width="150" />
        ///         <GridColumn Field="@nameof(Order.OrderDate)" HeaderText="Order Date" Format="d" TextAlign="Syncfusion.Blazor.Grids.TextAlign.Right" Width="130" />
        ///         <GridColumn Field="@nameof(Order.Freight)" HeaderText="Freight" Format="C2" TextAlign="Syncfusion.Blazor.Grids.TextAlign.Right" Width="120" />
        ///     </GridColumns>
        /// </SfGrid>
        ///
        /// @code {
        ///     public class Order
        ///     {
        ///         public int OrderID { get; set; }
        ///         public string CustomerName { get; set; }
        ///         public DateTime OrderDate { get; set; }
        ///         public double Freight { get; set; }
        ///     }
        ///
        ///     public List<Order> Orders { get; } = new()
        ///     {
        ///         new Order { OrderID = 10248, CustomerName = "Vinet",  OrderDate = new DateTime(1996, 7, 4), Freight = 32.38 },
        ///         new Order { OrderID = 10249, CustomerName = "Vinet",  OrderDate = new DateTime(1996, 7, 5), Freight = 11.61 },
        ///         new Order { OrderID = 10250, CustomerName = "Hanari", OrderDate = new DateTime(1996, 7, 8), Freight = 65.83 },
        ///         new Order { OrderID = 10251, CustomerName = "Hanari", OrderDate = new DateTime(1996, 7, 8), Freight = 40.42 }
        ///     };
        /// }
        /// ]]></code>
        /// </example>
        [Parameter]
        [DefaultValue(Syncfusion.Blazor.Grids.AutoSpanMode.None)]
        [JsonPropertyName("autoSpan")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AutoSpanMode AutoSpan { get; set; } = AutoSpanMode.None;

        private AutoSpanMode _autoSpan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to temporarily suppress automatic row and column spanning.
        /// </summary>
        [JsonIgnore]
        internal bool SuppressAutoSpanning { get; set; }
    }
}
