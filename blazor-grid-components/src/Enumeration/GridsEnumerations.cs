using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Defines the cell content's overflow mode. The available modes are.
    /// <list type="bullet">
    /// <item>
    /// <term>Clip</term>
    /// <description>Truncates the cell content when it overflows its area.</description>
    /// </item>
    /// <item>
    /// <term>Ellipsis</term>
    /// <description>Displays ellipsis when the cell content overflows its area.</description>
    /// </item>
    /// <item>
    /// <term>EllipsisWithTooltip</term>
    /// <description>Displays ellipsis when the cell content overflows its area also it will display tooltip while hover on ellipsis applied cell.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ClipMode
    {
        /// <summary>
        ///  Truncates the cell content when it overflows its area.
        /// </summary>
        [EnumMember(Value = "Clip")]
        Clip,

        /// <summary>
        ///  Displays ellipsis when the cell content overflows its area.
        /// </summary>
        [EnumMember(Value = "Ellipsis")]
        Ellipsis,

        /// <summary>
        /// Displays ellipsis when the cell content overflows its area
        /// </summary>
        [EnumMember(Value = "EllipsisWithTooltip")]
        EllipsisWithTooltip,
    }

    /// <summary>
    /// <c>ColumnQueryMode</c> provides options to retrive data from the datasource.
    /// <list type="bullet">
    /// <item>
    /// <term>All</term>
    /// <description>It retrives whole data source</description>
    /// </item>
    /// <item>
    /// <term>Schema</term>
    /// <description>Retrives data for all the defined columns in grid from the data source.</description>
    /// </item>
    /// <item>
    /// <term>ExcludeHidden</term>
    /// <description>Retrives data only for visible columns of grid from the data source.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ColumnQueryModeType
    {
        /// <summary>
        /// Specifies that all columns should be queried from data source.
        /// </summary>
        [EnumMember(Value = "All")]
        All,

        /// <summary>
        /// Specifies that only columns specified in the <c>GridColumns</c> component
        /// should be queried from data source.
        /// </summary>
        [EnumMember(Value = "Schema")]
        Schema,

        /// <summary>
        /// Specifies that exclude hidden columns specified in the <c>GridColumns</c> should be queried from data source.
        /// </summary>
        [EnumMember(Value = "ExcludeHidden")]
        ExcludeHidden,
    }

    /// <summary>
    /// <c>AdaptiveMode</c> Defines the mode of AdaptiveUI layout. The available Adaptive modes are:
    /// <list type="bullet">
    /// <item>
    /// <term>Both</term>
    /// <description> Default.Render the Adaptive Layout for both mobile and desktop.</description>
    /// </item>
    /// <item>
    /// <term>Mobile</term>
    /// <description>Render the Adaptive Layouts only on the smaller devices.</description>
    /// </item>
    /// <item>
    /// <term>Desktop</term>
    /// <description>Render the Adaptive Layouts only on the desktop.</description>
    /// </item>
    /// </list>
    /// </summary>

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AdaptiveMode
    {
        /// <summary>
        /// Default. Render the Adaptive Layout for both mobile and desktop.
        /// </summary>
        [EnumMember(Value = "Both")]
        Both,

        /// <summary>
        /// Render the Adaptive Layouts only on the smaller devices. 
        /// </summary>
        [EnumMember(Value = "Mobile")]
        Mobile,

        /// <summary>
        /// Render the Adaptive Layouts only on the Desktop. 
        /// </summary>
        [EnumMember(Value = "Desktop")]
        Desktop,

    }

    /// <summary>
    /// Defines modes of GridLine, They are.
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
    /// <description>Displays grid lines based on the theme.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GridLine
    {
        /// <summary>
        /// Displays both the horizontal and vertical grid lines.
        /// </summary>
        [EnumMember(Value = "Both")]
        Both,

        /// <summary>
        /// No grid lines are displayed.
        /// </summary>
        [EnumMember(Value = "None")]
        None,

        /// <summary>
        /// Displays the horizontal grid lines only.
        /// </summary>
        [EnumMember(Value = "Horizontal")]
        Horizontal,

        /// <summary>
        /// Displays the vertical grid lines only
        /// </summary>
        [EnumMember(Value = "Vertical")]
        Vertical,

        /// <summary>
        /// Displays grid lines based on the theme.
        /// </summary>
        [EnumMember(Value = "Default")]
        Default,
    }

    /// <summary>
    /// Hierarchy Grid Print modes are.
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
    /// <description>Prints the master grid alone.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HierarchyGridPrintMode
    {
        /// <summary>
        /// Prints the master grid with expanded child grids.
        /// </summary>
        [EnumMember(Value = "Expanded")]
        Expanded,

        /// <summary>
        /// Prints the master grid with all the child grids.
        /// </summary>
        [EnumMember(Value = "All")]
        All,

        /// <summary>
        /// Prints the master grid alone.
        /// </summary>
        [EnumMember(Value = "None")]
        None,
    }

    /// <summary>
    /// Print mode options are.
    /// <list type="bullet">
    /// <item>
    /// <term>AllPages</term>
    /// <description>Print all pages records of the Grid.</description>
    /// </item>
    /// <item>
    /// <term>CurrentPage</term>
    /// <description>Print current page records of the Grid.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PrintMode
    {
        /// <summary>
        /// Print all pages records of the Grid
        /// </summary>
        [EnumMember(Value = "AllPages")]
        AllPages,

        /// <summary>
        /// Print current page records of the Grid
        /// </summary>
        [EnumMember(Value = "CurrentPage")]
        CurrentPage
    }

    /// <summary>
    /// Defines alignments of text.
    /// <list type="bullet">
    /// <item>
    /// <term>Left</term>
    /// <description>Default. Text is left aligned.</description>
    /// </item>
    /// <item>
    /// <term>Right</term>
    /// <description>Text is right aligned.</description>
    /// </item>
    /// <item>
    /// <term>Center</term>
    /// <description>Text is centered.</description>
    /// </item>
    /// <item>
    /// <term>Justify</term>
    /// <description>Text is justified.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TextAlign
    {
	    /// <summary>
	    /// Specifies that no specific text alignment is applied.
	    /// </summary>
	    /// <remarks>
	    /// If no specific alignment is provided or if the value is set to <c>None</c>, the text will align to the Left by default.
	    /// If a different <c>TextAlign</c> value is given, it will override this default alignment.
	    /// </remarks>
	    [EnumMember(Value = "None")]
	    None,
  
        /// <summary>
        /// Default. Text is left aligned.
        /// </summary>
        [EnumMember(Value = "Left")]
        Left,

        /// <summary>
        /// Text is right aligned.
        /// </summary>
        [EnumMember(Value = "Right")]
        Right,

        /// <summary>
        /// Text is centered.
        /// </summary>
        [EnumMember(Value = "Center")]
        Center,

        /// <summary>
        /// Text is justified.
        /// </summary>
        [EnumMember(Value = "Justify")]
        Justify
    }

    /// <summary>
    /// Defines direction  of freeze column.
    /// <list type="bullet">
    /// <item>
    /// <term>None</term>
    /// <description>Column will not freeze.</description>
    /// </item>
    /// <item>
    /// <term>Left</term>
    /// <description>Freeze the column at left side.</description>
    /// </item>
    /// <item>
    /// <term>Right</term>
    /// <description>Freeze the column at right side.</description>
    /// </item>
    /// <item>
    /// <term>Fixed</term>
    /// <description>Freeze the column at current position.</description>
    /// </item>
    /// </list>
    /// </summary>

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FreezeDirection
    {
        
        /// <summary>
        /// Freeze the column at left side.
        /// </summary>
        [EnumMember(Value ="Left")]
        Left,
        /// <summary>
        /// Freeze the column at right side.
        /// </summary>
        [EnumMember(Value ="Right")]
        Right,
        /// <summary>
        /// Freeze the column at current position.
        /// </summary>
        [EnumMember(Value = "Fixed")]
        Fixed
    }


    /// <summary>
    /// Defines data row rendering direction of the grid that helps to view the grid in a compact way which is suitable for small screen.
    /// <list type="bullet">    
    /// <item>
    /// <term>Horizontal</term>
    /// <description>Display the data rows in Horizontal direction.</description>
    /// </item>
    /// <item>
    /// <term>Vertical</term>
    /// <description>Display the data rows in Vertical direction..</description>
    /// </item>    
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RowDirection
    {
        /// <summary>
        /// Default. Display the data rows in Horizontal direction.
        /// </summary>
        [EnumMember(Value = "Horizontal")]
        Horizontal,
        /// <summary>
        /// Display data rows in Vertical direction.
        /// </summary>
        [EnumMember(Value = "Vertical")]
        Vertical
    }

    /// <summary>
    /// Defines direction  of freeze table.
    /// <list type="bullet">
    /// <item>
    /// <term>None</term>
    /// <description>Column will not freeze.</description>
    /// </item>
    /// <item>
    /// <term>Left</term>
    /// <description>Freeze the column at left side.</description>
    /// </item>
    /// <item>
    /// <term>Right</term>
    /// <description>Freeze the column at right side.</description>
    /// </item>
    /// <item>
    /// <term>Left-Right</term>
    /// <description>Freeze the column at left and right side.</description>
    /// </item>
    /// </list>
    /// </summary>
    internal enum FreezeTable
    {
        /// <summary>
        /// I does not freeze the column.
        /// </summary>
        [EnumMember(Value ="None")]
        None,
        /// <summary>
        /// Freeze the column at left side.
        /// </summary>
        [EnumMember(Value ="Left")]
        Left,
        /// <summary>
        /// Freeze the column at right side.
        /// </summary>
        [EnumMember(Value ="Right")]
        Right,
        /// <summary>
        /// Freeze the column at left and right side.
        /// </summary>
        [EnumMember(Value ="Left-Right")]
        LeftRight
    }
    /// <summary>
    /// Defines the Command Buttons type.
    /// <list type="bullet">
    /// <item>
    /// <term>None</term>
    /// <description>Default. A command button with no default action. Use this for custom command actions.</description>
    /// </item>
    /// <item>
    /// <term>Edit</term>
    /// <description>A edit command button that edit current record.</description>
    /// </item>
    /// <item>
    /// <term>Delete</term>
    /// <description>A delete command button that delete current record.</description>
    /// </item>
    /// <item>
    /// <term>Save</term>
    /// <description>A save command button that saves the current edited record.</description>
    /// </item>
    /// <item>
    /// <term>Cancel</term>
    /// <description>A cancel command button that cancels the edit state.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CommandButtonType
    {
        /// <summary>
        /// Default. A command button with no default action. Use this for custom command actions.
        /// </summary>
        [EnumMember(Value = "None")]
        None,

        /// <summary>
        /// A edit command button that edit current record.
        /// </summary>
        [EnumMember(Value = "Edit")]
        Edit,

        /// <summary>
        /// A delete command button that delete current record.
        /// </summary>
        [EnumMember(Value = "Delete")]
        Delete,

        /// <summary>
        /// A save command button that saves the current edited record.
        /// </summary>
        [EnumMember(Value = "Save")]
        Save,

        /// <summary>
        /// A cancel command button that cancels the edit state.
        /// </summary>
        [EnumMember(Value = "Cancel")]
        Cancel,
    }

    /// <summary>
    /// Specified the Filter bar mode.
    /// <list type="bullet">
    /// <item>
    /// <term>OnEnter</term>
    /// <description>Initiate filter operation after Enter key is pressed.</description>
    /// </item>
    /// <item>
    /// <term>Immediate</term>
    /// <description>Initiate filter operation after certain time interval. By default time interval is 1500ms.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FilterBarMode
    {
        /// <summary>
        /// Initiate filter operation after Enter key is pressed.
        /// </summary>
        [EnumMember(Value = "OnEnter")]
        OnEnter,

        /// <summary>
        /// Initiate filter operation after certain time interval. By default time interval is 1500ms.
        /// </summary>
        [EnumMember(Value = "Immediate")]
        Immediate,
    }

    /// <summary>
    /// Defines types of Filter.
    /// <list type="bullet">
    /// <item>
    /// <term>FilterBar</term>
    /// <description>Default. Specifies the filter type as filter bar.</description>
    /// </item>
    /// <item>
    /// <term>Menu</term>
    /// <description>Specifies the filter type as menu.</description>
    /// </item>
    /// <item>
    /// <term>CheckBox</term>
    /// <description>Specifies the filter type as check box.</description>
    /// </item>
    /// <item>
    /// <term>Excel</term>
    /// <description>Specifies the filter type as excel.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FilterType
    {
        /// <summary>
        /// Default. Specifies the filter type as filter bar.
        /// </summary>
        [EnumMember(Value = "FilterBar")]
        FilterBar,

        /// <summary>
        /// Specifies the filter type as excel.
        /// </summary>
        [EnumMember(Value = "Excel")]
        Excel,

        /// <summary>
        /// Specifies the filter type as menu.
        /// </summary>
        [EnumMember(Value = "Menu")]
        Menu,

        /// <summary>
        /// Specifies the filter type as check box.
        /// </summary>
        [EnumMember(Value = "CheckBox")]
        CheckBox,
    }

    /// <summary>
    /// Defines the sort direction.
    /// <list type="bullet">
    /// <item>
    /// <term>Ascending</term>
    /// <description>Default. Sorts records in ascending order.</description>
    /// </item>
    /// <item>
    /// <term>Descending</term>
    /// <description>Sorts records in descending order.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SortDirection
    {
        /// <summary>
        /// Default. Sorts records in ascending order.
        /// </summary>
        [EnumMember(Value = "Ascending")]
        Ascending,

        /// <summary>
        /// Sorts records in descending order.
        /// </summary>
        [EnumMember(Value = "Descending")]
        Descending,

        /// <summary>
        /// Records are not sorted.
        /// </summary>
        [EnumMember(Value = "None")]
        None,
    }

    /// <summary>
    /// Defines modes of editing.
    /// <list type="bullet">
    /// <item>
    /// <term>Normal</term>
    /// <description>Default. Editing is done in an inline form. Edit form is rendered inline as one of the table rows.</description>
    /// </item>
    /// <item>
    /// <term>Dialog</term>
    /// <description>Editing is done in a Dialog/Pop component.</description>
    /// </item>
    /// <item>
    /// <term>Batch</term>
    /// <description>Enables cell editing. Multiple cells can be edited, added or deleted and saved.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EditMode
    {
        /// <summary>
        /// Default. Editing is done in an inline form. Edit form is rendered inline as one of the table rows.
        /// </summary>
        [EnumMember(Value = "Normal")]
        Normal,

        /// <summary>
        /// Editing is done in a Dialog/Pop component.
        /// </summary>
        [EnumMember(Value = "Dialog")]
        Dialog,

        /// <summary>
        /// Enables cell editing. Multiple cells can be edited, added or deleted and saved.
        /// </summary>
        [EnumMember(Value = "Batch")]
        Batch,
    }

    /// <summary>
    /// Defines add new row position.
    /// <list type="bullet">
    /// <item>
    /// <term>Top</term>
    /// <description>Default. Add form is placed at the first row of the grid.</description>
    /// </item>
    /// <item>
    /// <term>Bottom</term>
    /// <description>Add form is placed at the last row of the grid</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NewRowPosition
    {
        /// <summary>
        /// Default. Add form is placed at the first row of the grid.
        /// </summary>
        [EnumMember(Value = "Top")]
        Top,

        /// <summary>
        /// Add form is placed at the last row of the grid
        /// </summary>
        [EnumMember(Value = "Bottom")]
        Bottom,
    }

    /// <summary>
    /// Defines mode of cell selection.
    /// <list type="bullet">
    /// <item>
    /// <term>Flow</term>
    /// <description>Default. Selects the range of cells between start index and end index that also includes the other cells of the selected rows..</description>
    /// </item>
    /// <item>
    /// <term>Box</term>
    /// <description>Selects the range of cells within the start and end column indexes that includes in between cells of rows within the range</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CellSelectionMode
    {
        /// <summary>
        /// Default. All the cells between start and end cell will be selected.
        /// </summary>
        [EnumMember(Value = "Flow")]
        Flow,

        /// <summary>
        /// Range of cells that match the index of start and end cell will be selected
        /// </summary>
        [EnumMember(Value = "Box")]
        Box,

        /// <summary>
        /// Same as <c>Box</c>, but shows border during selection.
        /// </summary>
        [EnumMember(Value = "BoxWithBorder")]
        BoxWithBorder
    }

    /// <summary>
    /// Defines type of checkbox selection.
    /// This helps to reset selection when <c>CheckboxOnly</c> property is enabled.
    /// <list type="bullet">
    /// <item>
    /// <term>Default</term>
    /// <description>Default. In this mode, user can select multiple rows by clicking rows one by one.</description>
    /// </item>
    /// <item>
    /// <term>ResetOnRowClick</term>
    /// <description>In ResetOnRowClick mode, on clicking a row it will reset previously selected row and also multiple
    ///  rows can be selected by using CTRL or SHIFT key.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CheckboxSelectionType
    {
        /// <summary>
        /// Default. Clicking row will not clear selection selection of the row.
        /// </summary>
        [EnumMember(Value = "Default")]
        Default,

        /// <summary>
        /// Clicking row will reset the row selection.
        /// </summary>
        [EnumMember(Value = "ResetOnRowClick")]
        ResetOnRowClick,
    }

    /// <summary>
    /// Defines modes of Selection.
    /// <list type="bullet">
    /// <item>
    /// <term>Row</term>
    /// <description>Default. Row selection is enabled</description>
    /// </item>
    /// <item>
    /// <term>Cell</term>
    /// <description>Cell selection is enabled.</description>
    /// </item>
    /// <item>
    /// <term>Both</term>
    /// <description>Both Row and Cell selection is enabled.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SelectionMode
    {
        /// <summary>
        /// Cell selection is enabled.
        /// </summary>
        [EnumMember(Value = "Cell")]
        Cell,

        /// <summary>
        /// Default. Row selection is enabled.
        /// </summary>
        [EnumMember(Value = "Row")]
        Row,

        /// <summary>
        /// Both row and cell selection is enabled.
        /// </summary>
        [EnumMember(Value = "Both")]
        Both,
    }

    /// <summary>
    /// Defines types of Selection.
    /// <list type="bullet">
    /// <item>
    /// <term>Single</term>
    /// <description>Default. Allows user to select a row or cell.</description>
    /// </item>
    /// <item>
    /// <term>Multiple</term>
    /// <description>Allows user to select a multiple rows or cells.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SelectionType
    {
        /// <summary>
        /// Default. Allows user to select a row or cell.
        /// </summary>
        [EnumMember(Value = "Single")]
        Single,

        /// <summary>
        /// Allows user to select a multiple rows or cells.
        /// </summary>
        [EnumMember(Value = "Multiple")]
        Multiple,
    }

    /// <summary>
    /// Defines the wrap mode.
    /// <list type="bullet">
    /// <item>
    /// <term>Both</term>
    /// <description>Default. Wraps both header and content.</description>
    /// </item>
    /// <item>
    /// <term>Header</term>
    /// <description>Wraps header alone.</description>
    /// </item>
    /// <item>
    /// <term>Content</term>
    /// <description>Wraps content alone.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WrapMode
    {
        /// <summary>
        /// Default. Wraps both header and content.
        /// </summary>
        [EnumMember(Value = "Both")]
        Both,

        /// <summary>
        /// Wraps header alone.
        /// </summary>
        [EnumMember(Value = "Header")]
        Header,

        /// <summary>
        /// Wraps content alone.
        /// </summary>
        [EnumMember(Value = "Content")]
        Content,
    }

    /// <summary>
    /// Defines Actions of the Grid.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Action
    {
        /// <summary>
        /// Specifies paging action.
        /// </summary>
        [EnumMember(Value = "paging")]
        Paging,

        /// <summary>
        /// Specifies grid refresh.
        /// </summary>
        [EnumMember(Value = "refresh")]
        Refresh,

        /// <summary>
        /// Specifies sorting action.
        /// </summary>
        [EnumMember(Value = "sorting")]
        Sorting,

        /// <summary>
        /// Specifies filtering action.
        /// </summary>
        [EnumMember(Value = "filtering")]
        Filtering,

        /// <summary>
        /// Specifies clear filtering action.
        /// </summary>
        [EnumMember(Value = "clearfiltering")]
        ClearFiltering,

        /// <summary>
        /// Specifies searching action.
        /// </summary>
        [EnumMember(Value = "searching")]
        Searching,

        /// <summary>
        /// Specifies row drag and drop action.
        /// </summary>
        RowDragAndDrop,

        /// <summary>
        /// Specifies reorder action.
        /// </summary>
        [EnumMember(Value = "reorder")]
        Reorder,

        /// <summary>
        /// Specifies grouping action.
        /// </summary>
        [EnumMember(Value = "grouping")]
        Grouping,

        /// <summary>
        /// Specifies ungrouping action.
        /// </summary>
        UnGrouping,

        /// <summary>
        /// Specifies batch save action.
        /// </summary>
        BatchSave,

        /// <summary>
        /// Specifies virtual scrolling.
        /// </summary>
        VirtualScroll,

        /// <summary>
        /// Specifies print action.
        /// </summary>
        [EnumMember(Value = "print")]
        Print,

        /// <summary>
        /// Specifies edit begin action.
        /// </summary>
        [EnumMember(Value = "beginEdit")]
        BeginEdit,

        /// <summary>
        /// Specifies before edit begin action.
        /// </summary>
        [EnumMember(Value = "beforeBeginEdit")]
        BeforeBeginEdit,

        /// <summary>
        /// Specifies save action.
        /// </summary>
        [EnumMember(Value = "save")]
        Save,

        /// <summary>
        /// Specifies delete action.
        /// </summary>
        [EnumMember(Value = "delete")]
        Delete,

        /// <summary>
        /// Specifies cancel action.
        /// </summary>
        [EnumMember(Value = "cancel")]
        Cancel,

        /// <summary>
        /// Specifies add action.
        /// </summary>
        [EnumMember(Value = "add")]
        Add,

        /// <summary>
        /// Specifies filter pop opening.
        /// </summary>
        [EnumMember(Value = "filterBeforeOpen")]
        FilterBeforeOpen,

        /// <summary>
        /// Specifies filter choice request action is initiated in checkbox and excel filter.
        /// </summary>
        [EnumMember(Value = "filterChoiceRequest")]
        FilterChoiceRequest,

        /// <summary>
        /// Specifies filter pop is opened.
        /// </summary>
        [EnumMember(Value = "filterAfterOpen")]
        FilterAfterOpen,

        /// <summary>
        /// Specifies search action in checkbox and excel filter search textbox.
        /// </summary>
        [EnumMember(Value = "filterSearchBegin")]
        FilterSearchBegin,

        /// <summary>
        /// Specifies column visibility changed.
        /// </summary>
        ColumnState,

        /// <summary>
        /// Specifies expand all action.
        /// </summary>
        [EnumMember(Value = "expandAllComplete")]
        ExpandAllComplete,

        /// <summary>
        /// Specifies collapse all action.
        /// </summary>
        [EnumMember(Value = "collapseAllComplete")]
        CollapseAllComplete,

        /// <summary>
        /// Specifies column chooser is opening.
        /// </summary>
        [EnumMember(Value = "beforeOpenColumnChooser")]
        BeforeOpenColumnChooser,
    }

    /// <summary>
    /// Defines the aggregate types.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AggregateType
    {
        /// <summary>
        /// Performs sum aggregation.
        /// </summary>
        [EnumMember(Value = "Sum")]
        Sum,

        /// <summary>
        /// Performs average aggregation.
        /// </summary>
        [EnumMember(Value = "Average")]
        Average,

        /// <summary>
        /// Performs max aggregation.
        /// </summary>
        [EnumMember(Value = "Max")]
        Max,

        /// <summary>
        /// Performs min aggregation.
        /// </summary>
        [EnumMember(Value = "Min")]
        Min,

        /// <summary>
        /// Performs count aggregation.
        /// </summary>
        [EnumMember(Value = "Count")]
        Count,

        /// <summary>
        /// Performs true count aggregation.
        /// </summary>
        [EnumMember(Value = "TrueCount")]
        TrueCount,

        /// <summary>
        /// Performs false count aggregation.
        /// </summary>
        [EnumMember(Value = "FalseCount")]
        FalseCount,

        /// <summary>
        /// Performs custom aggregation.
        /// </summary>
        [EnumMember(Value = "Custom")]
        Custom,
    }

    /// <summary>
    /// Specifies the template type used for rendering aggregate cells in a grid during exporting. 
    /// This property allows differentiation between cells in the group caption, grid footer, or group footer.
    /// <list type="bullet">
    /// <item>
    /// <term>GroupCaption</term>
    /// <description>Indicates that the aggregate cell is part of the group caption.</description>
    /// </item>
    /// <item>
    /// <term>Footer</term>
    /// <description>Indicates that the aggregate cell is part of the grid footer.</description>
    /// </item>
    /// <item>
    /// <term>GroupFooter</term>
    /// <description>Indicates that the aggregate cell is part of the group footer.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AggregateTemplateType
    {
        /// <summary>
        /// Indicates that the aggregate cell is part of the group caption.
        /// </summary>
        [EnumMember(Value = "GroupCaption")]
        GroupCaption,

        /// <summary>
        /// Indicates that the aggregate cell is part of the grid footer.
        /// </summary>
        [EnumMember(Value = "Footer")]
        Footer,

        /// <summary>
        /// Indicates that the aggregate cell is part of the group footer.
        /// </summary>
        [EnumMember(Value = "GroupFooter")]
        GroupFooter,
    }

    /// <summary>
    /// Defines border line style.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BorderLineStyle
    {
        /// <summary>
        /// Border line is thin.
        /// </summary>
        [EnumMember(Value = "Thin")]
        Thin,

        /// <summary>
        /// Border line is thick.
        /// </summary>
        [EnumMember(Value = "Thick")]
        Thick,
    }

    /// <summary>
    /// Defines the header checkbox state.
    /// </summary>
    /// <exclude/>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CheckState
    {
        /// <summary>
        /// Default.
        /// </summary>
        [EnumMember(Value = "None")]
        None,

        /// <summary>
        /// Header is checked
        /// </summary>
        [EnumMember(Value = "Check")]
        Check,

        /// <summary>
        /// Header is unchecked.
        /// </summary>
        UnCheck,

        /// <summary>
        /// Header is in intermediate.
        /// </summary>
        [EnumMember(Value = "Intermediate")]
        Intermediate
    }

    /// <summary>
    /// Defines the EditActions.
    /// </summary>
    /// <exclude/>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EditAction
    {
        /// <summary>
        /// Default
        /// </summary>
        [EnumMember(Value = "None")]
        None,

        /// <summary>
        /// Holds the edited records.
        /// </summary>
        [EnumMember(Value = "Edited")]
        Edited,

        /// <summary>
        /// Holds the deleted records.
        /// </summary>
        [EnumMember(Value = "Deleted")]
        Deleted,

        /// <summary>
        /// Holds the Added records.
        /// </summary>
        [EnumMember(Value = "Added")]
        Added
    }

    /// <summary>
    /// Represents the types of actions that can occur when updating rows in the grid.
    /// </summary>
    /// <exclude/>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SaveActionType
    {
        /// <summary>
        /// An existing row in the grid has been edited.
        /// </summary>
        [EnumMember(Value = "Edited")]
        Edited,

        /// <summary>
        /// A new row has been added to the grid.
        /// </summary>
        [EnumMember(Value = "Added")]
        Added
    }

    /// <summary>
    /// Specifies the column type of the <see cref="Syncfusion.Blazor.Grids.GridColumn"/>, denoting the type of data it displays. 
    /// <list type="bullet">
    /// <item>
    /// <term>String</term>
    /// <description>A string type column.</description>
    /// </item>
    /// <item>
    /// <term>Number</term>
    /// <description>A number type column. Primitive types such as int, int?, floar, double, decimal etc. are consider
    /// as number type column.</description>
    /// </item>
    /// <item>
    /// <term>Integer</term>
    /// <description>A integer type column.</description>
    /// </item>
    /// <item>
    /// <term>Double</term>
    /// <description>A double type column.</description>
    /// </item>
    /// <item>
    /// <term>Long</term>
    /// <description>A Long type column.</description>
    /// </item>
    /// <item>
    /// <term>Decimal</term>
    /// <description>A Decimal type column.</description>
    /// </item>
    /// <item>
    /// <term>Boolean</term>
    /// <description>A boolean type column.</description>
    /// </item>
    /// <item>
    /// <term>Date</term>
    /// <description>A date type column. Primitive types such as datetime and datetimeoffset are consider
    /// as date type column.</description>
    /// </item>
    /// <item>
    /// <term>DateTime</term>
    /// <description>A datetime type column. Primitive types such as datetime and datetimeoffset are consider
    /// as date type column.</description>
    /// </item>
    /// <item>
    /// <term>CheckBox</term>
    /// <description>Enables checkbox column for selection purpose. No data operation is assosiated with this column.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ColumnType
    {
        /// <summary>
        /// No column type is specified.
        /// </summary>
        [EnumMember(Value = "none")]
        None,

        /// <summary>
        /// A string type column.
        /// </summary>
        [EnumMember(Value = "string")]
        String,

        /// <summary>
        /// An integer type column. Displays integer values.
        /// </summary>
        /// <value>
        /// The value representing the "integer" type column.
        /// </value>
        [EnumMember(Value = "integer")]
        Integer,
        
        /// <summary>
        /// A double type column. Displays double values.
        /// </summary>
        /// <value>
        /// The value representing the "double" type column.
        /// </value>
        [EnumMember(Value = "double")]
        Double,

        /// <summary>
        /// A long type column. Displays long integer values.
        /// </summary>
        /// <value>
        /// The value representing the "long" type column.
        /// </value>
        [EnumMember(Value = "long")]
        Long,

        /// <summary>
        /// A decimal type column. Displays decimal values.
        /// </summary>
        /// <value>
        /// The value representing the "decimal" type column.
        /// </value>
        [EnumMember(Value = "decimal")]
        Decimal,

        /// <summary>
        /// A boolean type column.
        /// </summary>
        [EnumMember(Value = "boolean")]
        Boolean,

        /// <summary>
        /// A date type column. Primitive types such as datetime and datetimeoffset are consider
        /// as date type column.
        /// </summary>
        [EnumMember(Value = "date")]
        Date,

        /// <summary>
        /// A datetime type column. Primitive types such as datetime and datetimeoffset are consider
        /// as date type column.
        /// </summary>
        [EnumMember(Value = "dateTime")]
        DateTime,

        /// <summary>
        /// Enables checkbox column for selection purpose. No data operation is assosiated with this column.
        /// </summary>
        [EnumMember(Value = "checkBox")]
        CheckBox,

        /// <summary>
        /// Specifies that the grid column is used to display and edit values of the <c>System.DateOnly</c> struct. 
        /// </summary>
        [EnumMember(Value = "dateOnly")]
        DateOnly,

        /// <summary>
        /// Specifies that the grid column is used to display and edit values of the <c>System.TimeOnly</c> struct. 
        /// </summary>
        [EnumMember(Value = "timeOnly")]
        TimeOnly
    }

    /// <summary>
    /// Defines the content type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContentType
    {
        /// <summary>
        /// Content type is image.
        /// </summary>
        [EnumMember(Value = "Image")]
        Image,

        /// <summary>
        /// Content type is line.
        /// </summary>
        [EnumMember(Value = "Line")]
        Line,

        /// <summary>
        /// Content type is page number.
        /// </summary>
        [EnumMember(Value = "PageNumber")]
        PageNumber,

        /// <summary>
        /// Content type is text.
        /// </summary>
        [EnumMember(Value = "Text")]
        Text,
    }

    /// <summary>
    /// Specifies the edit type of the <see cref="Syncfusion.Blazor.Grids.GridColumn"/>. It is used to render the specified editor component in the grid edit form to edit the corresponding cell value.
    /// <list type="bullet">
    /// <item>
    /// <term>DefaultEdit</term>
    /// <description>Default. Text box is used for editing.</description>
    /// </item>
    /// <item>
    /// <term>DropDownEdit</term>
    /// <description>DropDownList is used for editing.</description>
    /// </item>
    /// <item>
    /// <term>BooleanEdit</term>
    /// <description>Checkbox is used for editing.</description>
    /// </item>
    /// <item>
    /// <term>DatePickerEdit</term>
    /// <description>Date picker is used for editing.</description>
    /// </item>
    /// <item>
    /// <term>DateTimePickerEdit</term>
    /// <description>Datetime picker is used for editing.</description>
    /// </item>
    /// <item>
    /// <term>NumericEdit</term>
    /// <description>Numeric textbox is used for editing.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EditType
    {
        /// <summary>
        /// Default. Text box is used for editing.
        /// </summary>
        [EnumMember(Value = "defaultEdit")]
        DefaultEdit,

        /// <summary>
        /// DropDownList is used for editing.
        /// </summary>
        [EnumMember(Value = "dropDownEdit")]
        DropDownEdit,

        /// <summary>
        /// Checkbox is used for editing.
        /// </summary>
        [EnumMember(Value = "booleanEdit")]
        BooleanEdit,

        /// <summary>
        /// Date picker is used for editing.
        /// </summary>
        [EnumMember(Value = "datePickerEdit")]
        DatePickerEdit,

        /// <summary>
        /// Date time picker is used for editing.
        /// </summary>
        [EnumMember(Value = "dateTimePickerEdit")]
        DateTimePickerEdit,

        /// <summary>
        /// Numeric textbox is used for editing.
        /// </summary>
        [EnumMember(Value = "numericEdit")]
        NumericEdit,

        /// <summary>
        /// <see cref="Syncfusion.Blazor.Calendars.SfTimePicker{TValue}"/> component will rendered to edit the corresponding cell value.
        /// </summary>
        [EnumMember(Value = "timePickerEdit")]
        TimePickerEdit
    }

    /// <summary>
    /// Defines Excel horizontal alignment.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExcelHorizontalAlign
    {
        /// <summary>
        /// Horizontal align is left
        /// </summary>
        [EnumMember(Value = "Left")]
        Left,

        /// <summary>
        /// Horizontal align is right
        /// </summary>
        [EnumMember(Value = "Right")]
        Right,

        /// <summary>
        /// Horizontal align is center
        /// </summary>
        [EnumMember(Value = "Center")]
        Center,

        /// <summary>
        /// Horizontal align is fill
        /// </summary>
        [EnumMember(Value = "Fill")]
        Fill,
    }

    /// <summary>
    /// Defines Excel vertical alignment.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExcelVerticalAlign
    {
        /// <summary>
        /// Vertical align is bottom
        /// </summary>
        [EnumMember(Value = "Bottom")]
        Bottom,
		
        /// <summary>
        /// Vertical align is top
        /// </summary>
        [EnumMember(Value = "Top")]
        Top,

        /// <summary>
        /// Vertical align is center
        /// </summary>
        [EnumMember(Value = "Center")]
        Center,

        /// <summary>
        /// Vertical align is justify
        /// </summary>
        [EnumMember(Value = "Justify")]
        Justify,
    }

    /// <summary>
    /// Defines Export Type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExportType
    {
        /// <summary>
        /// Exports all page of the grid.
        /// </summary>
        [EnumMember(Value = "AllPages")]
        AllPages,

        /// <summary>
        /// Exports only the current page records of the grid.
        /// </summary>
        [EnumMember(Value = "CurrentPage")]
        CurrentPage,
    }

    /// <summary>
    /// Defines the mode for exporting the detail rows to the PDF file format.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PdfDetailRowMode
    {
        /// <summary>
        /// Default. Exports the detail row in an expanded state.
        /// </summary>
        /// <value>"Expand"</value>
        [DefaultValue(Expand)]
        [EnumMember(Value = "Expand")]
        Expand,

        /// <summary>
        /// Exports only the parent rows, excluding detail rows.
        /// </summary>
        /// <remarks>
        /// This option excludes the exporting of detailed rows, exporting only the parent rows.
        /// </remarks>
        /// <value>"None"</value>
        [EnumMember(Value = "None")]
        None,
    }

    /// <summary>
    /// Defines the mode for exporting the detail rows to Excel file format
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExcelDetailRowMode
    {
        /// <summary>
        /// Default. Exports the detail row in an expanded state.
        /// </summary>
        /// <value>"Expand"</value>
        [DefaultValue(Expand)]
        [EnumMember(Value = "Expand")]
        Expand,

        /// <summary>
        /// Exports the detail rows in a collapsed state.
        /// </summary>   
        /// <value>"Collapse"</value>
        [EnumMember(Value = "Collapse")]
        Collapse,

        /// <summary>
        /// Exports only the parent row, excluding detail rows.
        /// </summary>
        /// <remarks>
        /// This option excludes the export of detailed rows, exporting only the parent rows.
        /// </remarks>
        /// <value>"None"</value>
        [EnumMember(Value = "None")]
        None,
    }

    /// <summary>
    /// Defines the PDF page orientation.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PageOrientation
    {
        /// <summary>
        /// Pdf page is oriented in portrait.
        /// </summary>
        [EnumMember(Value = "Portrait")]
        Portrait,

        /// <summary>
        /// Pdf page is oriented in landscape.
        /// </summary>
        [EnumMember(Value = "Landscape")]
        Landscape
    }

    /// <summary>
    /// Defines the PDF dash style.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PdfDashStyle
    {
        /// <summary>
        /// Specifies the PDF dash style as solid.
        /// </summary>
        [EnumMember(Value = "Solid")]
        Solid,

        /// <summary>
        /// Specifies the PDF dash style as dash.
        /// </summary>
        [EnumMember(Value = "Dash")]
        Dash,

        /// <summary>
        /// Specifies the PDF dash style as dot.
        /// </summary>
        [EnumMember(Value = "Dot")]
        Dot,

        /// <summary>
        /// Specifies the PDF dash style as dashed dot.
        /// </summary>
        [EnumMember(Value = "DashDot")]
        DashDot,

        /// <summary>
        /// Specifies the PDF dash style as dashed dot dot.
        /// </summary>
        [EnumMember(Value = "DashDotDot")]
        DashDotDot,
    }

    /// <summary>
    /// Defines PDF horizontal alignment.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PdfHorizontalAlign
    {
        /// <summary>
        /// Horizontal align is left
        /// </summary>
        [EnumMember(Value = "Left")]
        Left,

        /// <summary>
        /// Horizontal align is right
        /// </summary>
        [EnumMember(Value = "Right")]
        Right,

        /// <summary>
        /// Horizontal align is center
        /// </summary>
        [EnumMember(Value = "Center")]
        Center,

        /// <summary>
        /// Horizontal align is justify.
        /// </summary>
        [EnumMember(Value = "Justify")]
        Justify,
    }

    /// <summary>
    /// Defines the pdf page number type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PdfPageNumberType
    {
        /// <summary>
        /// Defines the pdf page number type as lower latin.
        /// </summary>
        [EnumMember(Value = "LowerLatin")]
        LowerLatin,

        /// <summary>
        /// Defines the pdf page number type as lower roman.
        /// </summary>
        [EnumMember(Value = "LowerRoman")]
        LowerRoman,

        /// <summary>
        /// Defines the pdf page number type as upper latin.
        /// </summary>
        [EnumMember(Value = "UpperLatin")]
        UpperLatin,

        /// <summary>
        /// Defines the pdf page number type as upper roman.
        /// </summary>
        [EnumMember(Value = "UpperRoman")]
        UpperRoman,

        /// <summary>
        /// Defines the pdf page number type as numeric.
        /// </summary>
        [EnumMember(Value = "Numeric")]
        Numeric,

        /// <summary>
        /// Defines the pdf page number type as arabic.
        /// </summary>
        [EnumMember(Value = "Arabic")]
        Arabic,
    }

    /// <summary>
    /// Defined the PDF page size.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PdfPageSize
    {
        /// <summary>
        /// Defined the PDF page size as letter.
        /// </summary>
        [EnumMember(Value = "Letter")]
        Letter,

        /// <summary>
        /// Defined the PDF page size as note.
        /// </summary>
        [EnumMember(Value = "Note")]
        Note,

        /// <summary>
        /// Defined the PDF page size as legal.
        /// </summary>
        [EnumMember(Value = "Legal")]
        Legal,

        /// <summary>
        /// Defined the PDF page size as A0.
        /// </summary>
        [EnumMember(Value = "A0")]
        A0,

        /// <summary>
        /// Defined the PDF page size as A1.
        /// </summary>
        [EnumMember(Value = "A1")]
        A1,

        /// <summary>
        /// Defined the PDF page size as A2.
        /// </summary>
        [EnumMember(Value = "A2")]
        A2,

        /// <summary>
        /// Defined the PDF page size as A3.
        /// </summary>
        [EnumMember(Value = "A3")]
        A3,

        /// <summary>
        /// Defined the PDF page size as A4.
        /// </summary>
        [EnumMember(Value = "A4")]
        A4,

        /// <summary>
        /// Defined the PDF page size as A5.
        /// </summary>
        [EnumMember(Value = "A5")]
        A5,

        /// <summary>
        /// Defined the PDF page size as A6.
        /// </summary>
        [EnumMember(Value = "A6")]
        A6,

        /// <summary>
        /// Defined the PDF page size as A7.
        /// </summary>
        [EnumMember(Value = "A7")]
        A7,

        /// <summary>
        /// Defined the PDF page size as A8.
        /// </summary>
        [EnumMember(Value = "A8")]
        A8,

        /// <summary>
        /// Defined the PDF page size as A9.
        /// </summary>
        [EnumMember(Value = "A9")]
        A9,

        /// <summary>
        /// Defined the PDF page size as B0.
        /// </summary>
        [EnumMember(Value = "B0")]
        B0,

        /// <summary>
        /// Defined the PDF page size as B1.
        /// </summary>
        [EnumMember(Value = "B1")]
        B1,

        /// <summary>
        /// Defined the PDF page size as B2.
        /// </summary>
        [EnumMember(Value = "B2")]
        B2,

        /// <summary>
        /// Defined the PDF page size as B3.
        /// </summary>
        [EnumMember(Value = "B3")]
        B3,

        /// <summary>
        /// Defined the PDF page size as B4.
        /// </summary>
        [EnumMember(Value = "B4")]
        B4,

        /// <summary>
        /// Defined the PDF page size as B5.
        /// </summary>
        [EnumMember(Value = "B5")]
        B5,

        /// <summary>
        /// Defined the PDF page size as Archa.
        /// </summary>
        [EnumMember(Value = "Archa")]
        Archa,

        /// <summary>
        /// Defined the PDF page size as Archb.
        /// </summary>
        [EnumMember(Value = "Archb")]
        Archb,

        /// <summary>
        /// Defined the PDF page size as Archc.
        /// </summary>
        [EnumMember(Value = "Archc")]
        Archc,

        /// <summary>
        /// Defined the PDF page size as Archd.
        /// </summary>
        [EnumMember(Value = "Archd")]
        Archd,

        /// <summary>
        /// Defined the PDF page size as Arche.
        /// </summary>
        [EnumMember(Value = "Arche")]
        Arche,

        /// <summary>
        /// Defined the PDF page size as Flsa.
        /// </summary>
        [EnumMember(Value = "Flsa")]
        Flsa,

        /// <summary>
        /// Defined the PDF page size as HalfLetter.
        /// </summary>
        [EnumMember(Value = "HalfLetter")]
        HalfLetter,

        /// <summary>
        /// Defined the PDF page size as Letter11*17.
        /// </summary>
        [EnumMember(Value = "Letter11x17")]
        Letter11x17,

        /// <summary>
        /// Defined the PDF page size as Ledger.
        /// </summary>
        [EnumMember(Value = "Ledger")]
        Ledger,
    }

    /// <summary>
    /// Defines PDF vertical alignment.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PdfVerticalAlign
    {
        /// <summary>
        /// Defines PDF vertical alignment as Top.
        /// </summary>
        [EnumMember(Value = "Top")]
        Top,

        /// <summary>
        /// Defines PDF vertical alignment as Bottom.
        /// </summary>
        [EnumMember(Value = "Bottom")]
        Bottom,

        /// <summary>
        /// Defines PDF vertical alignment as Middle.
        /// </summary>
        [EnumMember(Value = "Middle")]
        Middle,
    }

    /// <summary>
    /// Exports types used by Grid.
    /// </summary>
    /// <exclude/>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ValueType
    {
        /// <summary>
        /// Defines the value type as number.
        /// </summary>
        [EnumMember(Value = "number")]
        Number,
        /// <summary>
        /// Defines the value type as string.
        /// </summary>
        [EnumMember(Value = "string")]
        String,
        /// <summary>
        /// Defines the value type as date.
        /// </summary>
        [EnumMember(Value = "Date")]
        Date,
        /// <summary>
        /// Defines the value type as boolean.
        /// </summary>
        [EnumMember(Value = "boolean")]
        Boolean,
    }

    /// <summary>
    /// Defines the scroll direction.
    /// </summary>
    /// <exclude/>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ScrollDirection
    {
        /// <summary>
        /// Scrolls the content upward.
        /// </summary>
        [EnumMember(Value = "up")]
        Up,
        /// <summary>
        /// Scrolls the content downward.
        /// </summary>
        [EnumMember(Value = "down")]
        Down,
        /// <summary>
        /// Scrolls the content to the right.
        /// </summary>
        [EnumMember(Value = "right")]
        Right,
        /// <summary>
        /// Scrolls the content to the left.
        /// </summary>
        [EnumMember(Value = "left")]
        Left,
    }

    /// <summary>
    /// Defines the hierarchy export mode for the pdf and excel.
    /// </summary>
    /// <exclude/>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HierarchyExportMode
    {
        /// <summary>
        /// Exports only the parent-level records, excluding all child hierarchy data.
        /// </summary>
        [EnumMember(Value = "none")]
        None,
        /// <summary>
        /// Exports all hierarchy levels.
        /// </summary>
        [EnumMember(Value = "all")]
        All,
        /// <summary>
        /// Exports only expanded hierarchy levels.
        /// </summary>
        [EnumMember(Value = "expanded")]
        Expanded
    }

    /// <summary>
    /// Defines the target element for the context menu.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContextMenuTarget
    {
        /// <summary>
        /// Default.
        /// </summary>
        [EnumMember(Value = "None")]
        None,

        /// <summary>
        /// Header is clicked.
        /// </summary>
        [EnumMember(Value = "Header")]
        Header,

        /// <summary>
        /// Content is clicked.
        /// </summary>
        [EnumMember(Value = "Content")]
        Content,

        /// <summary>
        /// Pager is clicked.
        /// </summary>
        [EnumMember(Value = "Pager")]
        Pager,

        /// <summary>
        /// Edit is clicked.
        /// </summary>
        [EnumMember(Value = "EditForm")]
        EditForm
    }
    /// <summary>
    /// Specifies automatic cell spanning modes for the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}"/> component.
    /// </summary>
    /// <remarks>
    /// <para>Use <see cref="AutoSpanMode"/> to control horizontal and vertical merging of identical cell values. The default value is <see cref="None"/>.</para>
    /// <list type="bullet">
    /// <item><description><see cref="None"/> - Keeps every cell isolated; no merging is attempted.</description></item>
    /// <item><description><see cref="Row"/> - Merges neighboring cells across columns when they share the same value, and it always runs before <see cref="Column"/> when both are enabled.</description></item>
    /// <item><description><see cref="Column"/> - Merges stacked cells down the column when they share the same value, and it always runs after <see cref="Row"/> to respect existing horizontal spans.</description></item>
    /// <item><description><see cref="HorizontalAndVertical"/> - Runs the <see cref="Row"/> pass first, then the <see cref="Column"/> pass, giving a full two-direction merge sequence.</description></item>
    /// </list>
    /// </remarks>
    public enum AutoSpanMode
    {
        /// <summary>
        /// Disables automatic cell spanning for the grid and its columns.
        /// </summary>
        /// <remarks>
        /// <para>Use <see cref="None"/> when duplicate values must remain visible in individual cells.</para>
        /// </remarks>
        None,

        /// <summary>
        /// Enables horizontal merging for adjacent cells with identical content within the same row.
        /// </summary>
        /// <remarks>
        /// <para>This pass evaluates first when combined with <see cref="Column"/>, ensuring side-by-side duplicates become a single wide cell before any vertical merge takes place.</para>
        /// </remarks>
        Row,

        /// <summary>
        /// Enables vertical merging for adjacent cells with identical content within the same column.
        /// </summary>
        /// <remarks>
        /// <para>This pass runs after <see cref="Row"/> so that it respects any horizontal spans already produced, creating taller cells only where the horizontal pass left aligned values.</para>
        /// </remarks>
        Column,

        /// <summary>
        /// Enables both horizontal and vertical automatic cell spanning.
        /// </summary>
        /// <remarks>
        /// <para>Executes the <see cref="Row"/> pass first and then the <see cref="Column"/> pass, giving a full two-direction merge sequence.</para>
        /// </remarks>
        HorizontalAndVertical
    }
}
