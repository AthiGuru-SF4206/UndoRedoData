using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using Syncfusion.Blazor.Internal;
using Syncfusion.Blazor.Grids.Internal;
using System;
using System.Linq;
using Syncfusion.Blazor.Data;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid columm.
    /// </summary>
    public partial class GridColumn : SfDataBoundComponent, ISfCircularComponent
    {
        internal virtual RenderFragment<object>? GetAutoComplete() => null;

        internal virtual RenderFragment<object>? GetDropDown() => null;

        internal virtual void AutoCompleteDispose() { }

        internal bool HasChild { get; set; }
        private bool ShouldSerialize { get; set; }
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

        [CascadingParameter]
        internal GridColumns? Parent { get; set; }

        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// Defines the child content.
        /// </summary>
        /// <exclude/>
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// If AllowEditing set to false, then it disables editing of a particular column.
        /// By default all columns are editable.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("allowEditing")]
        public bool AllowEditing { get; set; } = true;

        private bool _allowEditing { get; set; }

        /// <summary>
        /// If AllowAdding set to false, then it disables add operation of a particular column.
        /// By default all columns are editable.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("allowAdding")]
        public bool AllowAdding { get; set; } = true;

        /// <summary>
        /// If AllowFiltering set to false, then it disables filtering option and filter bar element of a particular column.
        /// By default all columns are filterable.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("allowFiltering")]
        public bool AllowFiltering { get; set; } = true;

        private bool _allowFiltering { get; set; }

        /// <summary>
        /// If AllowGrouping set to false, then it disables grouping of a particular column.
        /// By default all columns are groupable.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("allowGrouping")]
        public bool AllowGrouping { get; set; } = true;

        private bool _allowGrouping { get; set; }

        /// <summary>
        /// If AllowReordering set to false, then it disables reorder of a particular column.
        /// By default all columns can be reorder.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("allowReordering")]
        public bool AllowReordering { get; set; } = true;

        private bool _allowReordering { get; set; }

        /// <summary>
        /// If AllowResizing set to false, it disables resize option of a particular column.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("allowResizing")]
        public bool AllowResizing { get; set; } = true;

        private bool _allowResizing { get; set; }

        /// <summary>
        /// If AllowSearching set to false, then it disables searching of a particular column.
        /// By default all columns are searchable.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("allowSearching")]
        public bool AllowSearching { get; set; } = true;

        private bool _allowSearching { get; set; }

        /// <summary>
        /// If AllowSorting set to false, then it disables sorting option of a particular column.
        /// By default all columns are sortable.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("allowSorting")]
        public bool AllowSorting { get; set; } = true;

        private bool _allowSorting { get; set; }

        /// <summary>
        /// If AutoFit set to true, then the particular column content width will be
        /// adjusted based on its content in the initial rendering itself.
        /// Setting this property as true is equivalent to calling AutoFitColumns method in the DataBound event.
        /// </summary>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("autoFit")]
        public bool AutoFit { get; set; }

        private bool _autoFit { get; set; }

        /// <summary>
        /// Defines the cell content's overflow mode. The available modes are.
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.ClipMode.Clip"></see></term>
        /// <description>Truncates the cell content when it overflows its area.</description>
        /// </item>
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.ClipMode.Ellipsis"></see></term>
        /// <description>Displays ellipsis when the cell content overflows its area.</description>
        /// </item>
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.ClipMode.EllipsisWithTooltip"></see></term>
        /// <description>Displays ellipsis when the cell content overflows its area also it will display tooltip while hover on ellipsis applied cell.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        [DefaultValue(ClipMode.Ellipsis)]
        [JsonPropertyName("clipMode")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ClipMode ClipMode { get; set; } = ClipMode.Ellipsis;

        private ClipMode _clipMode { get; set; }

        /// <summary>
        /// Used to render multiple header rows(stacked headers) on the Grid header.
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("columns")]
        public List<GridColumn>? Columns { get; set; }

        private List<GridColumn>? _columns { get; set; }

        /// <summary>
        /// Commands provides an option to display command buttons in every cell.
        /// Use <see cref="Syncfusion.Blazor.Grids.GridCommandColumn"/> component to declare command columns.
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("commands")]
        public List<GridCommandColumn>? Commands { get; set; }

        private List<GridCommandColumn>? _commands { get; set; }

        /// <summary>
        /// The CSS styles and attributes of the content cells of a particular column can be customized.
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("customAttributes")]
        public IDictionary<string, object>? CustomAttributes { get; set; }

        private IDictionary<string, object>? _customAttributes { get; set; }

        /// <summary>
        /// Defines default values for the component when adding a new record to the Grid.
        /// </summary>
        /// <remarks>If no default value is provided then the default value of the model property type will be
        /// used as initial value.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("defaultValue")]
        public object? DefaultValue { get; set; }

        private object? _defaultValue { get; set; }

        /// <summary>
        /// If DisableHtmlEncode is set to false, it disables the encodes the HTML of the header and content cells.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("disableHtmlEncode")]
        public bool DisableHtmlEncode { get; set; } = true;

        private bool _disableHtmlEncode { get; set; }

        /// <summary>
        /// Specifies whether HTML sanitization is applied to the cell content when rendering the column.
        /// </summary>
        /// <value>
        /// <c>true</c> to sanitize HTML content before rendering; otherwise, <c>false</c>.
        /// The default value is <c>true</c>.
        /// </value>
        /// <remarks>
        /// When enabled, HTML content is sanitized to ensure only safe and valid markup is rendered.
        /// When set to <c>false</c>, raw HTML is rendered without sanitization, which may introduce security risks if the content is not trusted.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Employees">
        ///     <GridColumns>
        ///         <GridColumn Field="@nameof(Employee.EmailID)" DisableHtmlEncode="false" EnableSanitization="false">
        ///         </GridColumn>
        ///     </GridColumns>
        /// </SfGrid>
        /// ]]></code>
        /// </example>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("enableSanitization")]
        public bool EnableSanitization { get; set; } = true;
        private bool _enableSanitization { get; set; }

        /// <summary>
        /// If DisplayAsCheckBox is set to true, it displays the column value as a check box instead of Boolean value.
        /// </summary>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("displayAsCheckBox")]
        public bool DisplayAsCheckBox { get; set; }

        private bool _displayAsCheckBox { get; set; }

        /// <summary>
        /// Defines the object to customize default cell editors. The following types can be used to customize default
        /// editors.
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.NumericEditCellParams"/></term>
        /// <description>Customizes the default numerictextbox editor.</description>
        /// </item>
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.DropDownEditCellParams"/></term>
        /// <description>Customizes the default dropdown editor.</description>
        /// </item>
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.DateEditCellParams"/></term>
        /// <description>Customizes the default datepicker editor.</description>
        /// </item>
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.BooleanEditCellParams"/></term>
        /// <description>Customizes the default checkbox editor.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("editorSettings")]
        [JsonIgnore]
        public IEditorSettings? EditorSettings { get; set; }

        /// <summary>
        /// Defines a configuration option used to customize the default filter component rendered in the filter menu dialog
        /// and the Excel-like filter dialog of the Grid. The following filter parameter types can be assigned:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.NumericFilterParams"/></term>
        /// <description>Customizes the default numeric textbox filter component.</description>
        /// </item>
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.DateFilterParams"/></term>
        /// <description>Customizes the default date picker filter component.</description>
        /// </item>
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.TimeFilterParams"/></term>
        /// <description>Customizes the default time picker filter component.</description>
        /// </item>
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.DateTimeFilterParams"/></term>
        /// <description>Customizes the default date-time picker filter component.</description>
        /// </item>
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.DropDownFilterParams"/></term>
        /// <description>Customizes the default drop-down list filter component.</description>
        /// </item>
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.AutoCompleteFilterParams"/></term>
        /// <description>Customizes the default autocomplete filter component.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("filterEditorSettings")]
        [JsonIgnore]
        public IFilterSettings? FilterEditorSettings { get; set; }

        /// <summary>
        /// Defines the cell edit template that used as editor for a particular column.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type TValue.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonIgnore]
        [JsonPropertyName("editTemplate")]
        public RenderFragment<object>? EditTemplate { get; set; }

        /// <summary>
        /// Gets or sets the type of editor component to be rendered in the edit form.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Blazor.Grids.EditType "/> enumeration that specifies the editor component to be rendered in the edit form.
        /// If <code>EditType </code> is not provided then the EditType will inferred from the property type of the <c>TValue</c>.
        /// If <c>TValue</c> is <c>ExpandoObject</c>/ <c>DynamicObject</c>, then the <c>EditType</c> will inferred from the first data row.
        /// If <code>EditType</code> is not mentioned then the below components will rendered for the corresponding mentioned column types.
        /// For <c>String</c> type column, <see cref="Syncfusion.Blazor.Inputs.SfTextBox"/> will rendered in the edit form.
        /// For <c>Date</c> and <c>DateTime</c> type columns, <see cref="Syncfusion.Blazor.Calendars.SfDatePicker{TValue}"/> will rendered in the edit form.
        /// For <c>DateOnly</c> type column, <see cref="Syncfusion.Blazor.Calendars.SfDatePicker{TValue}"/> will rendered in the edit form.
        /// For <c>TimeOnly</c> type column, <see cref="Syncfusion.Blazor.Calendars.SfTimePicker{TValue}"/> will rendered in the edit form.
        /// For <c>Boolean</c> type column, <see cref="Syncfusion.Blazor.Buttons.SfCheckBox{TValue}"/> will rendered in the edit form.
        /// For <c>Number</c> type column, <see cref="Syncfusion.Blazor.Inputs.SfNumericTextBox{TValue}"/> will rendered in the edit form.
        /// </value>
        /// <remarks>
        /// The <code>EditType</code> property can be set to one of the following values:
        /// <c>DefaultEdit</c>: <see cref="Syncfusion.Blazor.Inputs.SfTextBox"/> will rendered in the edit form
        /// <c>DropDownEdit</c>: <see cref="Syncfusion.Blazor.DropDowns.SfDropDownList{TValue, TItem}"/> will rendered in the edit form.
        /// <c>BooleanEdit</c>: <see cref="Syncfusion.Blazor.Buttons.SfCheckBox{TValue}"/> will rendered in the edit form.
        /// <c>DatePickerEdit</c>: <see cref="Syncfusion.Blazor.Calendars.SfDatePicker{TValue}"/> will rendered in the edit form.
        /// <c>DateTimePickerEdit</c>: <see cref="Syncfusion.Blazor.Calendars.SfDateTimePicker{TValue}"/> will rendered in the edit form.
        /// <c>NumericEdit</c>: <see cref="Syncfusion.Blazor.Inputs.SfNumericTextBox{TValue}"/> will rendered in the edit form.
        /// <c>TimePickerEdit</c>: <see cref="Syncfusion.Blazor.Calendars.SfTimePicker{TValue}"/> will rendered in the edit form.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///  <GridColumns>
        /// <GridColumn Field=@nameof(Order.Freight) EditType="EditType.NumericEdit" ></GridColumn>
        ///  </GridColumns>
        /// </SfGrid>
        /// ]]></code>
        /// </example>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("editType")]
        public EditType EditType { get; set; }

        private EditType _editType { get; set; }

        /// <summary>
        /// If EnableGroupByFormat set to true, then it groups the particular column by formatted values.
        /// By default columns are group by format.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("enableGroupByFormat")]
        public bool EnableGroupByFormat { get; set; } = true;

        private bool _enableGroupByFormat { get; set; }

        /// <summary>
        /// Defines the field name of column which is mapped with mapping name of DataSource.
        /// The bounded columns can be sort, filter and group etc.,
        /// If the Field name contains “dot”, then it is considered as complex binding.
        /// </summary>
        [Parameter]
        [DefaultValue("")]
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        private string? _field { get; set; }
	
	internal void SetField(string name) => _field = name;
        /// <summary>
        ///  Defines the filter options to customize filtering for the particular column.
        /// </summary>
        /// <exclude/>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("filterSettings")]
        public FilterSettings? FilterSettings { get; set; }

        private FilterSettings? _filterSettings { get; set; }

        /// <summary>
        /// Defines the filter template that used as filter UI for a particular column in FilterBar and Menu.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.PredicateModel"/>.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonIgnore]
        [JsonPropertyName("filterTemplate")]
        public RenderFragment<object>? FilterTemplate { get; set; }

        /// <summary>
        /// Defines the mapping column name of the foreign data source.
        /// If it is not defined then the Field will be considered as mapping column name.
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("foreignKeyField")]
        public string? ForeignKeyField { get; set; }

        private string? _foreignKeyField { get; set; }

        /// <summary>
        /// Defines the display column name from the foreign data source which will be obtained from comparing local and foreign data.
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("foreignKeyValue")]
        public string? ForeignKeyValue { get; set; }

        private string? _foreignKeyValue { get; set; }

        /// <summary>
        /// It is used to change display value with the given format and does not affect the original data.
        /// Gets the format from the user which can be standard or custom formats.
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("format")]
        public string? Format { get; set; }

        private string? _format { get; set; }

        /// <summary>
        /// Defines the column template which is used to add customized element in the column header.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.GridColumn"/>.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonIgnore]
        [JsonPropertyName("headerTemplate")]
        public RenderFragment<object>? HeaderTemplate { get; set; }

        /// <summary>
        /// Defines the header text of column which is used to display in column header.
        /// If HeaderText is not defined, then field name value will be assigned to header text.
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("headerText")]
        public string? HeaderText { get; set; }

        private string? _headerText { get; set; }

        /// <summary>
        /// Define the alignment of column header which is used to align the text of column header.
        /// </summary>
        [Parameter]
        [JsonPropertyName("headerTextAlign")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TextAlign HeaderTextAlign { get; set; }

        private TextAlign _headerTextAlign { get; set; }

        /// <summary>
        /// Column visibility can change based on its Media Queries.
        /// HideAtMedia accepts only valid Media Queries.
        /// </summary>
        [Parameter]
        [DefaultValue("")]
        [JsonPropertyName("hideAtMedia")]
        public string HideAtMedia { get; set; } = string.Empty;

        private string? _hideAtMedia { get; set; }

        /// <summary>
        /// Gets the unique identifier value of the column. It is used to get the object.
        /// </summary>
        [Parameter]
        [DefaultValue(default(double))]
        [JsonPropertyName("index")]
        public int Index { get; set; }

        private int _index { get; set; }

        /// <exclude />
        [JsonPropertyName("originalIndex")]
        public int OriginalIndex { get; set; }

        /// <exclude />
        [JsonPropertyName("isPersistAutoFit")]
        public bool IsPersistAutoFit { get; set; }

        /// <exclude />
        [JsonPropertyName("tableWidth")]
        public string? TableWidth { get; set; }

        /// <exclude />
        [JsonPropertyName("leftFrozenTableWidth")]
        public string? LeftFrozenTableWidth { get; set; }

        /// <exclude />
        [JsonPropertyName("rightFrozenTableWidth")]
        public string? RightFrozenTableWidth { get; set; }

        /// <exclude />
        /// <summary>
        /// You can use this property to update it's freeze right and left column width.
        /// </summary>
        [JsonPropertyName("translateLeftRightValue")]
        public double TranslateLeftRightValue { get; set; }


        /// <summary>
        /// You can use this property to freeze selected columns in grid.
        /// </summary>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("isFrozen")]
        public bool IsFrozen { get; set; }

        private bool _isFrozen { get; set; }

        /// <summary>
        /// If IsIdentity is set to true, then this column is considered as identity column.
        /// This column will be in disabled state in add form.
        /// </summary>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("isIdentity")]
        public bool IsIdentity { get; set; }

        private bool _isIdentity { get; set; }

        /// <summary>
        /// If IsPrimaryKey is set to true, considers this column as the primary key constraint.
        /// </summary>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("isPrimaryKey")]
        public bool IsPrimaryKey { get; set; }

        private bool _isPrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column should be fixed at the beginning of the Grid.
        /// A fixed column cannot be moved through reordering or grouping actions.
        /// </summary>
        /// <value>
        /// <c>true</c> if the column is fixed; otherwise, <c>false</c>. The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// When set to <c>true</c>, the column remains fixed at the start of the grid, in the order specified by the 
        /// <see cref="GridColumn"/> collection. Fixed columns are unaffected by reordering or grouping actions, 
        /// and multiple columns can be fixed, appearing in the order they are defined in the collection.
        /// The fixed column feature is not compatible with the frozen column feature.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("fixedColumn")]
        public bool FixedColumn { get; set; }

        private bool _fixedColumn { get; set; }

        /// <summary>
        /// Defines the maximum width of the column in pixel or percentage, which will restrict resizing beyond this pixel or percentage.
        /// </summary>
        [Parameter]
        [DefaultValue("")]
        [JsonPropertyName("maxWidth")]
        public string MaxWidth { get; set; } = string.Empty;

        private string? _maxWidth { get; set; }

        /// <summary>
        /// Defines the minimum width of the column in pixels or percentage.
        /// </summary>
        [Parameter]
        [DefaultValue("")]
        [JsonPropertyName("minWidth")]
        public string MinWidth { get; set; } = string.Empty;

        private string? _minWidth { get; set; }

        /// <summary>
        /// If ShowColumnMenu set to false, then it disable the column menu of a particular column.
        /// By default column menu will show for all columns.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("showColumnMenu")]
        public bool ShowColumnMenu { get; set; } = true;

        private bool _showColumnMenu { get; set; }

        /// <summary>
        /// If ShowInColumnChooser set to false, then hides the particular column in column chooser.
        /// By default all columns are displayed in column chooser.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("showInColumnChooser")]
        public bool ShowInColumnChooser { get; set; } = true;

        private bool _showInColumnChooser { get; set; }

        /// <summary>
        /// Gets or sets the custom sort comparer function to implement own sort logic for a particular column.
        /// For foreign key column with local data source a sort comparer will be assigned by default to sort it by text(ForeignKeyValue) instead of the underlying field value.
        /// </summary>
        /// <remarks>
        /// Custom sort comparer cannot be used with remote data and Entity framework data source.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("sortComparer")]
        [JsonIgnore]
        public IComparer<object>? SortComparer { get; set; }

        /// <summary>
        /// Defines the column template that renders customized element in each cell of the column.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <c>TValue</c>.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("template")]
        [JsonIgnore]
        public RenderFragment<object>? Template { get; set; }

        /// <summary>
        /// Defines the alignment of the column in both header and content cells.
        /// </summary>
        [Parameter]
        [DefaultValue(TextAlign.Left)]
        [JsonPropertyName("textAlign")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TextAlign TextAlign { get; set; } = TextAlign.Left;

        private TextAlign _textAlign { get; set; }

        /// <summary>
        /// Defines which side the column need to freeze.
        /// </summary>
        [Parameter]
        [DefaultValue(FreezeDirection.Left)]
        [JsonPropertyName("freeze")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FreezeDirection Freeze { get; set; } = FreezeDirection.Left;
        private FreezeDirection _freeze { get; set; }

        /// <summary>
        /// Gets or sets the column type of the <see cref="Syncfusion.Blazor.Grids.GridColumn"/>. 
        /// It determines how the data in the column will be displayed based on the specified type.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Blazor.Grids.ColumnType"/> enumerations that specify the column type. 
        /// If the column type is not explicitly defined, the <code>Type</code> will be inferred from the property type of <c>TValue</c>.
        /// If <c>TValue</c> is <c>ExpandoObject</c> or <c>DynamicObject</c>, the <code>Type</code> will be inferred from the first row of the grid data.
        /// </value>
        /// <remarks>
        /// The <code>Type</code> property can be set to one of the following values:
        /// <c>String</c>: The column will display the string values in the UI. For example “Alfki”, “UK”. 
        /// <c>Number</c>: The column will display the numeric values such as int, int?, float, double, decimal etc. For example 2, 2.5, 3.33 
        /// <c>Integer</c>: Displays integer values from <see cref="System.Int32"/> struct. Example: 5, 123
        /// <c>Double</c>: Displays double values from <see cref="System.Double"/> struct. Example: 3.33, 45.567
        /// <c>Long</c>: Displays long integer values from <see cref="System.Int64"/> struct. Example: 255486129307
        /// <c>Decimal</c>: Displays decimal values from <see cref="System.Decimal"/> struct. Example: 123.45M
        /// <c>Boolean</c>: The column will display the boolean values such as true or false.
        /// <c>Date</c> and <c>DateTime</c>: The column will display the datetime value from the <see cref="System.DateTime"/> and <see cref="System.DateTimeOffset "/> struct.
        /// <c>DateOnly</c>: The column will display the date value from <see cref="System.DateOnly"/> struct. For example 2/1/2023
        /// <c>TimeOnly</c>: The column will display the time value from <see cref="System.TimeOnly"/> struct. For example 11:15 AM
        /// <c>CheckBox</c>: Display the checkbox for selection purpose. No data operation is assosiated with this column.
        /// This <code>Type</code> property is mainly used for <c>ExpandoObject</c> and <c>DynamicObject</c>.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///  <GridColumns>
        /// <GridColumn Field=@nameof(Order.OrderDate) Type="ColumnType.DateOnly"></GridColumn>
        ///  </GridColumns>
        /// </SfGrid>
        ///@code{
        ///public class Order
        ///{
        ///     public DateOnly? OrderDate { get; set; }
        ///}
        ///}
        /// ]]></code>
        /// </example>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("type")]
        public ColumnType Type { get; set; }

        private ColumnType _type { get; set; }

        /// <summary>
        /// Gets the unique identifier value of the column. It is used to get the object.
        /// </summary>
        [Parameter]
        [DefaultValue("")]
        [JsonPropertyName("uid")]
        public string Uid { get; set; } = string.Empty;

        private string? _uid { get; set; }

        /// <summary>
        /// Defines rules to validate data before creating and updating. The validation rules can be set
        /// as instance of <see cref="Syncfusion.Blazor.Grids.ValidationRules"/>.
        /// </summary>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("validationRules")]
        public ValidationRules? ValidationRules { get; set; }

        private ValidationRules? _validationRules { get; set; }

        /// <summary>
        /// If Visible is set to false, hides the particular column. By default, all columns are displayed.
        /// </summary>
        [Parameter]
        [DefaultValue(true)]
        [JsonPropertyName("visible")]
        public bool Visible { get; set; } = true;

        private bool _visible { get; set; }



        /// <summary>
        /// Gets or sets the automatic cell spanning mode for this column.
        /// </summary>
        /// <value>
        /// A <see cref="Syncfusion.Blazor.Grids.AutoSpanMode"/> value that controls the column's spanning behavior. The default value is <see langword="null"/>, which causes the column to inherit the grid's <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AutoSpan"/>.
        /// Allowed values are None, Row, Column, and HorizontalAndVertical.
        /// </value>
        /// <remarks>
        /// <para>
        /// The effective behavior for this column is the intersection of the grid-level and column-level modes:
        /// Effective = Grid.AutoSpan ∩ Column.AutoSpan (or inherited).
        /// The column can only narrow what the grid allows; it cannot enable a span direction (Row or Column) that the grid disables.
        /// </para>
        /// <para>
        /// Column spanning is processed first, then row spanning. These rules apply uniformly to data rows, grouped rows, and summary rows.
        /// </para>
        /// <para>
        /// This property controls automatic merging of identical values. Manual merging or unmerging of cells based on cell index can be performed using the appropriate methods and is not affected by this property's enum value, even if None is set.
        /// </para>
        /// <para>
        /// Below are all possible combinations of Grid-level AutoSpan and Column-level AutoSpan, explaining the effective behavior for this specific column:
        /// </para>
        /// <list type="table">
        /// <listheader>
        /// <term>Grid Mode</term>
        /// <term>Column Mode</term>
        /// <description>Effective Behavior</description>
        /// </listheader>
        /// <item>
        /// <term>None</term>
        /// <term>None</term>
        /// <description>No spanning. Both grid and column explicitly disable spanning.</description>
        /// </item>
        /// <item>
        /// <term>None</term>
        /// <term>Row</term>
        /// <description>No spanning. Grid-level None overrides column-level Row; grid disables row spanning.</description>
        /// </item>
        /// <item>
        /// <term>None</term>
        /// <term>Column</term>
        /// <description>No spanning. Grid-level None overrides column-level Column; grid disables column spanning.</description>
        /// </item>
        /// <item>
        /// <term>None</term>
        /// <term>HorizontalAndVertical</term>
        /// <description>No spanning. Grid-level None overrides column-level HorizontalAndVertical; grid disables all spanning.</description>
        /// </item>
        /// <item>
        /// <term>Row</term>
        /// <term>None</term>
        /// <description>No spanning. Particular column explicitly disables spanning, overriding grid's Row.</description>
        /// </item>
        /// <item>
        /// <term>Row</term>
        /// <term>Row</term>
        /// <description>Row spanning only. Both grid and column enable row spanning.</description>
        /// </item>
        /// <item>
        /// <term>Row</term>
        /// <term>Column</term>
        /// <description>No spanning. Grid only allows row spanning. Column cannot enable column spanning, so column spanning does not happen, resulting in no column spanning at all for this column.</description>
        /// </item>
        /// <item>
        /// <term>Row</term>
        /// <term>HorizontalAndVertical</term>
        /// <description>Row spanning only. Grid only allows row spanning. Column cannot enable column spanning, even if HorizontalAndVertical is set.</description>
        /// </item>
        /// <item>
        /// <term>Column</term>
        /// <term>None</term>
        /// <description>No spanning. Particular column explicitly disables spanning, overriding grid's Column.</description>
        /// </item>
        /// <item>
        /// <term>Column</term>
        /// <term>Row</term>
        /// <description>No spanning. Grid only allows column spanning. Column cannot enable row spanning.</description>
        /// </item>
        /// <item>
        /// <term>Column</term>
        /// <term>Column</term>
        /// <description>Column spanning only. Both grid and column enable column spanning.</description>
        /// </item>
        /// <item>
        /// <term>Column</term>
        /// <term>HorizontalAndVertical</term>
        /// <description>Column spanning only. Grid only allows column spanning. Column cannot enable row spanning, even if HorizontalAndVertical is set.</description>
        /// </item>
        /// <item>
        /// <term>HorizontalAndVertical</term>
        /// <term>None</term>
        /// <description>No spanning. Particular column explicitly disables both vertical and horizontal merging, overriding grid's HorizontalAndVertical.</description>
        /// </item>
        /// <item>
        /// <term>HorizontalAndVertical</term>
        /// <term>Row</term>
        /// <description>Row spanning only. Grid allows both, but column narrows it to Row. Column spanning does not occur for this column.</description>
        /// </item>
        /// <item>
        /// <term>HorizontalAndVertical</term>
        /// <term>Column</term>
        /// <description>Column spanning only. Grid allows both, but column narrows it to Column. Row spanning does not occur for this column.</description>
        /// </item>
        /// <item>
        /// <term>HorizontalAndVertical</term>
        /// <term>HorizontalAndVertical</term>
        /// <description>Row and Column spanning. Both grid and column enable both row and column spanning.</description>
        /// </item>
        /// </list>
        /// <para>
        /// Enabling spanning at the column level, especially HorizontalAndVertical, may impact performance on large datasets due to the sequential processing of merges. Consider using None for columns with unique or frequently changing content to optimize rendering.
        /// </para>
        /// </remarks>
        /// <example>
        /// This example demonstrates narrowing automatic spanning to row-only for a specific column while the grid enables both directions.
        /// <code><![CDATA[
        /// @using Syncfusion.Blazor.Grids
        ///
        /// <SfGrid TValue="Order" AutoSpan="AutoSpanMode.HorizontalAndVertical" AllowPaging="true">
        ///     <GridEvents TValue="Order" DataBound="DataBoundHandler" Created="CreatedHandler"></GridEvents>
        ///     <GridColumns>
        ///         <GridColumn Field="@nameof(Order.OrderID)" HeaderText="Order ID" Width="120" AutoSpan="AutoSpanMode.None" />
        ///         <GridColumn Field="@nameof(Order.CustomerID)" HeaderText="Customer ID" Width="150" AutoSpan="AutoSpanMode.Row" />
        ///         <GridColumn Field="@nameof(Order.Freight)" HeaderText="Freight" Format="C2" Width="120" />
        ///     </GridColumns>
        /// </SfGrid>
        ///
        /// @code {
        ///     public class Order
        ///     {
        ///         public int OrderID { get; set; }
        ///         public string CustomerID { get; set; }
        ///         public double Freight { get; set; }
        ///     }
        ///
        ///     public partial class GridAutoSpanMode
        ///     {
        ///         public List<Order> Orders { get; set; }
        ///
        ///         protected override void OnInitialized()
        ///         {
        ///             Orders = Enumerable.Range(1, 9).Select(x => new Order()
        ///             {
        ///                 OrderID = x,
        ///                 CustomerID = (new string[] { "ALFKI", "ANATR", "ANTON", "CHOPS", "FRANK", "BERGS", "BLAUS", "BLONP", "BOLID" })[new Random().Next(9)],
        ///                 Freight = new Random().Next(1000, 10000) / 100.0
        ///             }).ToList();
        ///         }
        ///
        ///         // In the Razor component, the CustomerID column will only span rows (vertical merges for identical customers),
        ///         // while Freight inherits HorizontalAndVertical (potential horizontal and vertical merges).
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        [Parameter]
        [JsonPropertyName("autoSpan")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AutoSpanMode? AutoSpan { get; set; }

        internal AutoSpanMode GetEffectiveAutoSpanning(AutoSpanMode gridMode)
        {
            AutoSpanMode columnMode = AutoSpan.HasValue ? AutoSpan.Value : gridMode;
            return Syncfusion.Blazor.Grids.Internal.AutoSpanningExtensions.Intersect(gridMode, columnMode);
        }

        /// <summary>
        /// Defines the width of the column in pixels or percentage.
        /// </summary>
        [Parameter]
        [DefaultValue("")]
        [JsonPropertyName("width")]
        public string Width { get; set; } = string.Empty;

        private string? _width { get; set; }

        /// <summary>
        /// Defines the Checkbox Item template that renders customized element/value in each checkbox of the Filter column.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.FilterItemTemplateContext"/>.
        /// </remarks>
        [Parameter]
        [DefaultValue(null)]
        [JsonPropertyName("filterItemTemplate")]
        [JsonIgnore]
        public RenderFragment<object>? FilterItemTemplate { get; set; }


        /// Below codes added to ignore the specified properties while serialization by JSON.net.
        /// <exclude />
        public bool ShouldSerializeChildContent() => ShouldSerialize;

        /// <exclude />
        public bool ShouldSerializeTemplate() => ShouldSerialize;

        /// <exclude />
        public bool ShouldSerializeFilterTemplate() => ShouldSerialize;

        /// <exclude />
        public bool ShouldSerializeEditTemplate() => ShouldSerialize;

        /// <exclude />
        public bool ShouldSerializeHeaderTemplate() => ShouldSerialize;

        /// <exclude />
        public bool ShouldSerializeFilterItemTemplate() => ShouldSerialize;

        /// <summary>
        /// The value set to the PreventFilterQuery property in OnActionBegin event handler is maintained by using this property.
		/// This helps to prevent the default filter query generation for previously filtered columns during the multiple column filtering.
        /// </summary>
        internal bool PreventFilterQuery { get; set; }
		
		internal bool EnableFrozenLineCursor { get; set; }
        internal bool EnableLeftFrozenLineCursor { get; set; }
        internal bool EnableRightFrozenLineCursor { get; set; }

        internal bool EnableFixedLeftFreezeLineCursor { get; set; }
        internal bool EnableFixedRightFreezeLineCursor { get; set; }
        internal bool IsHiddenByGrouping { get; set; }
        internal object? ForeignKeySorting { get; set; }

        /// <summary>
        /// Updates child properties based on the specified key and value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void UpdateChildProperties(string key, object propertyValue)
        {
            if (key == nameof(Commands))
            {
                Commands = _commands = (List<GridCommandColumn>)propertyValue;
            }
            else if (key == nameof(Columns))
            {
                Columns = _columns = (List<GridColumn>)propertyValue;
            }

            DirectParameters.AddOrUpdateItem(key, propertyValue);
        }

        /// <summary>
        /// Initializes the component asynchronously.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            MainParent = (SfBaseComponent)BaseParent!;
            Parent?.UpdateChildProperty(this);
            _allowEditing = AllowEditing;
            _allowFiltering = AllowFiltering;
            _allowGrouping = AllowGrouping;
            _allowReordering = AllowReordering;
            _allowResizing = AllowResizing;
            _allowSearching = AllowSearching;
            _allowSorting = AllowSorting;
            _autoFit = AutoFit;
            _clipMode = ClipMode;
            _columns = Columns;
            _commands = Commands;
            _customAttributes = CustomAttributes;
            _defaultValue = DefaultValue;
            _disableHtmlEncode = DisableHtmlEncode;
            _enableSanitization = EnableSanitization;
            _displayAsCheckBox = DisplayAsCheckBox;
            _editType = EditType;
            _enableGroupByFormat = EnableGroupByFormat;
            _field = Field;
            _filterSettings = FilterSettings;
            _foreignKeyField = ForeignKeyField;
            _foreignKeyValue = ForeignKeyValue;
            _format = Format;
            _headerText = HeaderText;
            _headerTextAlign = HeaderTextAlign;
            _hideAtMedia = HideAtMedia;
            _index = Index;
            _isFrozen = IsFrozen;
            _freeze = Freeze;
            
            _isIdentity = IsIdentity;
            _isPrimaryKey = IsPrimaryKey;
            _fixedColumn = FixedColumn;
            _maxWidth = MaxWidth;
            _minWidth = MinWidth;
            _showColumnMenu = ShowColumnMenu;
            _showInColumnChooser = ShowInColumnChooser;
            _textAlign = TextAlign;
            _type = Type;
            _uid = Uid;
            _validationRules = ValidationRules;
            _visible = Visible;
            _width = Width;
            HasChild = ChildContent != null;
            BaseParent!.HasColumnChanges = true;
            await BaseParent.CallStateHasChangedAsync().ConfigureAwait(true);

            BaseParent.AnnotateColumn(this);
            HeaderText = _headerText = HeaderText ?? Field;
            if (string.IsNullOrEmpty(Field))
            {
                AllowFiltering = false;
                AllowGrouping = false;
                AllowSorting = false;
                if (Type is ColumnType.CheckBox)
                {
                    AllowReordering = false;
                }
            }

            Uid = string.IsNullOrEmpty(Uid) ? GetColumnUid("grid-column") : Uid;
            OriginalIndex = Index = ++BaseParent.ColumnIndex;
            if (BaseParent.EnableRtl || SyncfusionService.options.EnableRtl)
            {
                TextAlign = TextAlign.Right;
                HeaderTextAlign = TextAlign.Right;
            }
        }

        private string[] _changeableProps = new string[] { "Visible", "HeaderText", "DataSource", "ForeignDataSource","Width", "IsFrozen", "Freeze", "ClipMode", "FixedColumn", "AutoSpan" };

        /// <summary>
        /// Handles parameter updates and synchronizes property values when component parameters are set.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);
            AllowEditing = _allowEditing = await UpdateProperty(nameof(AllowEditing), AllowEditing, _allowEditing).ConfigureAwait(true);
            AllowFiltering = _allowFiltering = await UpdateProperty(nameof(AllowFiltering), AllowFiltering, _allowFiltering).ConfigureAwait(true);
            AllowGrouping = _allowGrouping = await UpdateProperty(nameof(AllowGrouping), AllowGrouping, _allowGrouping).ConfigureAwait(true);
            AllowReordering = _allowReordering = await UpdateProperty(nameof(AllowReordering), AllowReordering, _allowReordering).ConfigureAwait(true);
            AllowResizing = _allowResizing = await UpdateProperty(nameof(AllowResizing), AllowResizing, _allowResizing).ConfigureAwait(true);
            AllowSearching = _allowSearching = await UpdateProperty(nameof(AllowSearching), AllowSearching, _allowSearching).ConfigureAwait(true);
            AllowSorting = _allowSorting = await UpdateProperty(nameof(AllowSorting), AllowSorting, _allowSorting).ConfigureAwait(true);
            AutoFit = _autoFit = await UpdateProperty(nameof(AutoFit), AutoFit, _autoFit).ConfigureAwait(true);
            ClipMode = _clipMode = await UpdateProperty(nameof(ClipMode), ClipMode, _clipMode).ConfigureAwait(true);
            Columns = _columns = await UpdateProperty(nameof(Columns), Columns, _columns).ConfigureAwait(true);
            Commands = _commands = await UpdateProperty(nameof(Commands), Commands, _commands).ConfigureAwait(true);
            CustomAttributes = _customAttributes = await UpdateProperty(nameof(CustomAttributes), CustomAttributes, _customAttributes).ConfigureAwait(true);
            DefaultValue = _defaultValue = await UpdateProperty(nameof(DefaultValue), DefaultValue, _defaultValue).ConfigureAwait(true);
            DisableHtmlEncode = _disableHtmlEncode = await UpdateProperty(nameof(DisableHtmlEncode), DisableHtmlEncode, _disableHtmlEncode).ConfigureAwait(true);
            EnableSanitization = _enableSanitization = await UpdateProperty(nameof(EnableSanitization), EnableSanitization, _enableSanitization).ConfigureAwait(true);
            DisplayAsCheckBox = _displayAsCheckBox = await UpdateProperty(nameof(DisplayAsCheckBox), DisplayAsCheckBox, _displayAsCheckBox).ConfigureAwait(true);
            EditType = _editType = await UpdateProperty(nameof(EditType), EditType, _editType).ConfigureAwait(true);
            EnableGroupByFormat = _enableGroupByFormat = await UpdateProperty(nameof(EnableGroupByFormat), EnableGroupByFormat, _enableGroupByFormat).ConfigureAwait(true);
            Field = _field = await UpdateProperty(nameof(Field), Field, _field!).ConfigureAwait(true);
            FilterSettings = _filterSettings = await UpdateProperty(nameof(FilterSettings), FilterSettings, _filterSettings).ConfigureAwait(true);
            ForeignKeyField = _foreignKeyField = await UpdateProperty(nameof(ForeignKeyField), ForeignKeyField, _foreignKeyField).ConfigureAwait(true);
            ForeignKeyValue = _foreignKeyValue = await UpdateProperty(nameof(ForeignKeyValue), ForeignKeyValue, _foreignKeyValue).ConfigureAwait(true);
            Format = _format = await UpdateProperty(nameof(Format), Format, _format).ConfigureAwait(true);
            HeaderText = _headerText = await UpdateProperty(nameof(HeaderText), HeaderText, _headerText).ConfigureAwait(true);
            HeaderTextAlign = _headerTextAlign = await UpdateProperty(nameof(HeaderTextAlign), HeaderTextAlign, _headerTextAlign).ConfigureAwait(true);
            HideAtMedia = _hideAtMedia = await UpdateProperty(nameof(HideAtMedia), HideAtMedia, _hideAtMedia!).ConfigureAwait(true);
            Index = _index = await UpdateProperty(nameof(Index), Index, _index).ConfigureAwait(true);
            IsFrozen = _isFrozen = await UpdateProperty(nameof(IsFrozen), IsFrozen, _isFrozen).ConfigureAwait(true);
            Freeze = _freeze = await this.UpdateProperty(nameof(Freeze), Freeze, _freeze).ConfigureAwait(true);
            IsIdentity = _isIdentity = await UpdateProperty(nameof(IsIdentity), IsIdentity, _isIdentity).ConfigureAwait(true);
            IsPrimaryKey = _isPrimaryKey = await UpdateProperty(nameof(IsPrimaryKey), IsPrimaryKey, _isPrimaryKey).ConfigureAwait(true);
            FixedColumn = _fixedColumn = await UpdateProperty(nameof(FixedColumn), FixedColumn, _fixedColumn).ConfigureAwait(true);
            MaxWidth = _maxWidth = await UpdateProperty(nameof(MaxWidth), MaxWidth, _maxWidth!).ConfigureAwait(true);
            MinWidth = _minWidth = await UpdateProperty(nameof(MinWidth), MinWidth, _minWidth!).ConfigureAwait(true);
            ShowColumnMenu = _showColumnMenu = await UpdateProperty(nameof(ShowColumnMenu), ShowColumnMenu, _showColumnMenu).ConfigureAwait(true);
            ShowInColumnChooser = _showInColumnChooser = await UpdateProperty(nameof(ShowInColumnChooser), ShowInColumnChooser, _showInColumnChooser).ConfigureAwait(true);
            TextAlign = _textAlign = await UpdateProperty(nameof(TextAlign), TextAlign, _textAlign).ConfigureAwait(true);
            Type = _type = await UpdateProperty(nameof(Type), Type, _type).ConfigureAwait(true);
            Uid = _uid = await UpdateProperty(nameof(Uid), Uid, _uid!).ConfigureAwait(true);
            ValidationRules = _validationRules = await UpdateProperty(nameof(ValidationRules), ValidationRules, _validationRules).ConfigureAwait(true);
            Visible = _visible = await UpdateProperty(nameof(Visible), Visible, _visible).ConfigureAwait(true);
            Width = _width = await UpdateProperty(nameof(Width), Width, _width!).ConfigureAwait(true);

            if (PropertyChanges.Count > 0 && PropertyChanges.Keys.Any(prop => _changeableProps.IndexOf(prop) > -1))
            {
                if (PropertyChanges.ContainsKey("Width"))
                {
                    ((SfBaseComponent)BaseParent!).PropertyChanges.TryAdd("ColumnWidth", this.Width);
                }
                if (PropertyChanges.ContainsKey("IsFrozen") || PropertyChanges.ContainsKey("Freeze"))
                {
                    ((SfBaseComponent)BaseParent!).PropertyChanges.TryAdd("FrozenColumns", null!);
                }
                if (PropertyChanges.ContainsKey("ClipMode"))
                {
                    ((SfBaseComponent)BaseParent!).PropertyChanges.TryAdd("ColumnClipMode", this.ClipMode);
                }
                if (PropertyChanges.ContainsKey("FixedColumn"))
                {
                    ((SfBaseComponent)BaseParent!).PropertyChanges.TryAdd("FixedColumn", this.FixedColumn);
                }
                ((SfBaseComponent)BaseParent!).PropertyChanges.TryAdd(nameof(IGrid.Columns), this);
                PropertyChanges.Clear();
                ColumnState state;
                BaseParent.Notify("ColumnAdded", state = new ColumnState { Column = this, PreventRefresh = false });
                if (!state.PreventRefresh)
                {
                    await BaseParent.PropertyChanged().ConfigureAwait(true);
                }
            }
        }
        internal override void ComponentDispose()
        {
            /* Dynamic column add/remove is handled.
             * 1. Remove column from collection and call state changes.
             * 2. Set the HasColumnChanges to let know grid something changed.
             * 3. Call state change. Must use CallStateHasChangedAsync to avoid threading issue.
             */
            if (BaseParent != null)
            {
                BaseParent.HasColumnChanges = true;
            }
            Parent?.RemoveChildProperty(this);
            BaseParent?.CallStateHasChangedAsync();
        }
    }

    internal class ColumnState
    {
        public GridColumn? Column { get; set; }

        public bool PreventRefresh { get; set; }
    }
}
