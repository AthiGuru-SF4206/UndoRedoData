using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Inputs;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Linq;

namespace Syncfusion.Blazor.Grids.Internal
{
    internal class GridClientOptions
    {
        public bool enableRtl { get; set; }

        public bool isWebAssembly { get; set; }

        public bool enableAutoFill { get; set; }

        public object? height { get; set; }

        public object? width { get; set; }

        public bool allowTextWrap { get; set; }

        public string? wrapMode { get; set; }

        public bool allowRowDragAndDrop { get; set; }

        public bool allowDragSelection { get; set; }

        public bool hasDropTarget { get; set; }

        public bool allowResizing { get; set; }

        public int frozenRows { get; set; }

        public int frozenColumns { get; set; }

        public int aggregatesCount { get; set; }

        public bool enableVirtualization { get; set; }

        public bool enableColumnVirtualization { get; set; }

        public bool enableVirtualMaskRow { get; set; }

        public bool allowReordering { get; set; }

        public bool allowGrouping { get; set; }

        public bool showDropArea { get; set; }

        public bool groupReordering { get; set; }

        public int groupCount { get; set; }

        public int filterCount { get; set; }

        public int currentPage { get; set; }

        public int pageSize { get; set; }

        public int rowHeight { get; set; }

        public string? url { get; set; }

        public bool offline { get; set; }

        public bool showGroupedColumn { get; set; }

        public int totalItemCount { get; set; }

        public bool needClientAction { get; set; }

        public string? requestType { get; set; }

        public bool enablePersistence { get; set; }
        
        public bool enableAdaptiveUI { get; set; }

        public int visibleGroupedRowsCount { get; set; }

        public int totalGroupedRowsCount { get; set; }

        public List<GridColumn>? columns { get; set; }

        public List<GridColumn>? virtualizedColumns { get; set; }

        public string? newRowPosition { get; set; }
        
        public bool showAddNewRow { get; set; }


        public string? editMode { get; set; }

        public int frozenCols { get; set; }

        public bool allowPaging { get; set; }

        public bool isAdd { get; set; }

        public bool isEdit { get; set; }

        public bool allowEditing { get; set; }

        public string? selectionMode { get; set; }

        public string? cellSelectionMode { get; set; }

        public object? rowCellIndexes { get; set; }

        public bool isPrerendered { get; set; }

        public string? clipMode { get; set; }

        public string? rowDropTarget { get; set; }

        public string? selectionType { get; set; }

        public bool hasDetailTemplate { get; set; }

        public bool hasTemplateInEditSettings { get; set; }

        public bool showColumnMenu { get; set; }

        public string[]? initGroupingField { get; set; }

        public bool isColumnResized { get; set; }

        public string? frozenName { get; set; }
        
        public int frozenRightCount { get; set; }

        public int frozenRightColumnsCount { get; set; }


        public int frozenLeftCount { get; set; }

        public int frozenLeftColumnsCount { get; set; }


        public bool isPreventScrollEvent { get; set; }
		
        public bool allowFreezeLineMoving { get; set; }

        public bool enableStickyHeader { get; set; }

        public bool enableInfiniteScrolling { get; set; }

        public int infiniteMaxBlocks { get; set; }

        public bool infiniteCacheMode { get; set; }

        public int infiniteInitialBlock { get; set; }

        public bool isFreezeLineMoved { get; set; }

        public int actualFrozenColumns { get; set; }

        public bool isColumnReordered { get; set; }

        public bool enableLazyLoading { get; set; }

        public bool isColumnWidthChanged { get; set; }

        public bool isClipboardEventBinded { get; set; }

        public int overscanCount { get; set; }

        public int customizedOverScan { get; set; }

        public bool isRenderedFromTreeGrid { get; set; }

        public bool isRenderedFromGantt { get; set; }

        public string? TValue { get; set; }

        public bool isColumnClipModeChanged { get; set; }

        public bool showColumnChooser { get; set; }

        public bool autoFit { get; set; }

        public bool isFixedColumnPresent { get; set; }

        public string? rowRenderingMode { get; set; }

        public bool allowEmptyAreaDrop { get; set; }

		public bool emptyCellTemplate { get; set; }
    }

    internal class FreezeLineMovingClientOptions
    {
        public List<GridColumn>? columns { get; set; }
        public int frozenColumns { get; set; }
        public int frozenRightCount { get; set; }
        public int frozenLeftCount { get; set; }
        public int frozenLeftColumnsCount { get; set; }
        public int actualFrozenColumns { get; set; }
        public bool isColumnReordered { get; set; }
    }

    /// <summary>
    /// Represents the base parameters for grid components, including frozen state configuration.
    /// </summary>
    public class GridBaseParameters
    {
        internal bool? IsFrozen { get; set; }

        internal bool IsFrozenRight { get; set; }
    }

    /// <summary>
    /// Represents the parameters for virtual scrolling in the grid, including start and end row indexes.
    /// </summary>
    public class GridVirtualBaseParameters
    {
        internal int RowStartIndex { get; set; }

        internal int RowEndIndex { get; set; }

        internal int StartColumnIndex { get; set; }

        internal int EndColumnIndex { get; set; }

        internal int TranslateX { get; set; }

        internal int VirtualTableWidth { get; set; }

        internal IEnumerable<object>? Data { get; set; }
    }

    /// <summary>
    /// Represents the parameters for the grid header, including column configuration.
    /// </summary>
    public class GridHeaderParameters : GridBaseParameters
    {
        internal List<GridColumn>? Columns { get; set; }

        internal List<Row<object>> RowCollection { get; set; } = new List<Row<object>>();

        internal IEnumerable<object>? Data { get; set; }

        internal int ARIAColumnIndex { get; set; }

        internal int FrozenLeftColumnCount { get; set; }

        internal bool IsFromClient { get; set; }

        internal bool IsFrozenRowMovable { get; set; }

        internal string? Id { get; set; }
    }

    /// <summary>
    /// Represents the parameters for the grid content section, including row details.
    /// </summary>
    public class GridContentParameters : GridBaseParameters
    {
        internal List<Row<object>>? Rows { get; set; }

        internal IEnumerable<object>? Data { get; set; }

        internal bool IsFrozenVirtual { get; set; }

        internal string? Id { get; set; }

        internal bool? IsHeader { get; set; }

    }

    /// <summary>
    /// Represents the parameters for a grid row, including row details and state information.
    /// </summary>
    public class GridRowParameters : GridBaseParameters
    {
        internal Row<object>? Row { get; set; }

        internal bool IsLastRow { get; set; }
    }

    /// <summary>
    /// Represents the parameters for the grid header, including column and row collection details.
    /// </summary>
    public class GridHeaderCellParameters : GridBaseParameters
    {
        internal List<Cell<object>>? Cells { get; set; }

        internal List<GridColumn>? Columns { get; set; }

        internal Row<object>? Row { get; set; }

        internal int RowIteration { get; set; }

        internal int Iteration { get; set; }

        internal int FrozenLeftColumnCount { get; set; }
    }

    /// <summary>
    /// Represents the parameters for a grid cell, including row and cell details.
    /// </summary>
    public class GridCellParameters : GridBaseParameters
    {
        internal Row<object>? Row { get; set; }

        internal Cell<object>? Cell { get; set; }

        internal bool IsMaskedCell { get; set; }
    }

    /// <summary>
    /// Represents the parameters for the virtualized grid header, including column details.
    /// </summary>
    public class GridVirtualHeaderParameters : GridVirtualBaseParameters
    {
		internal List<GridColumn>? Columns { get; set; }
    }

    /// <summary>
    /// Represents the parameters for the virtualized grid content.
    /// </summary>
    public class GridVirtualContentParameters : GridVirtualBaseParameters
    {
        internal IEnumerable<object>? QueriedData { get; set; }

        internal int RowQueryStartIndex { get; set; }

        internal int RowQueryEndIndex { get; set; }

        internal int NextRowToNavigate { get; set; }
    }

    /// <summary>
    /// Represents the parameters for a filter input in the grid.
    /// </summary>
    public class FilterInputParameters : GridBaseParameters
    {
        internal string? FieldName { get; set; }

        internal object? ActualValue { get; set; }

        internal bool IsCheckbox { get; set; }

        internal string? CellType { get; set; }

        internal GridColumn? Column { get; set; }

        internal object? FilterValue { get; set; }

        internal string? Predicate { get; set; }

        internal bool IgnoreAccent { get; set; }

        internal bool MatchCase { get; set; }
    }

    /// <summary>
    /// Represents the parameters for the filter bar in the grid.
    /// </summary>
    public class FilterBarParameters : GridBaseParameters
    {
        internal List<Row<object>>? Rows { get; set; }
    }

    /// <summary>
    /// Represents the parameters for the foreignkey column in the grid.
    /// </summary>
    public class GridForeignKeyColumnParameters
    {
        internal bool IsColumnUidRequired { get; set; }

        internal string? DuplicateForeignkeyField { get; set; }
    }

    /// <summary>
    /// Represents the arguments containing data and related information when the grid data is ready.
    /// </summary>
    public class DataReadyArgs<T>
    {

        /// <summary>
        /// Gets or sets the collection of data items.
        /// </summary>
        public IEnumerable<object>? Data { get; set; }

        /// <summary>
        /// Gets or sets the grid instance associated with the data.
        /// </summary>
        public SfGrid<T>? Grid { get; set; }

        /// <summary>
        /// Gets or sets the query used to retrieve the data.
        /// </summary>
        public Query? Query { get; set; }

        /// <summary>
        /// Gets or sets the aggregate values for the data.
        /// </summary>
        public IDictionary<string, object>? Aggregates { get; set; }

        /// <summary>
        /// Gets or sets the total count of records.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the starting index of the data.
        /// </summary>
        public int? StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the ending index of the data.
        /// </summary>
        public int? EndIndex { get; set; }

        /// <summary>
        /// Gets or sets the virtual starting index of the data.
        /// </summary>
        public int? VStartIndex { get; set; }

        /// <summary>
        /// Gets or sets the virtual ending index of the data.
        /// </summary>
        public int? VEndIndex { get; set; }
    }

    internal class VirtualHeightScroll
    {
        public int Height { get; set; }
    }

    internal interface IRenderer
    {
        IEnumerable<object> Data { get; set; }

        public List<Row<object>> Rows { get; set; }
    }

    internal class RowTemplateData
    {
        public object? Data { get; set; }
    }

    internal class EditTemplateData
    {
        public object? Data { get; set; }

        public object? Column { get; set; }
    }

    internal class GridToolbarEditItems
    {
        public List<string>? EnableItems { get; set; }

        public List<string>? DisableItems { get; set; }
    }

    internal interface IColumnChooser 
    {
        public bool Intermediate { get; set; }
        public bool SelectAllCheckbox { get; set; }
        public void CheckBoxClickHandler(MouseEventArgs args, GridColumn column);
        public void SelectAllClickHandler();
    }

    internal class GridLocaleKeys
    {
        public static string Prefix => "Grid_";

        public static string EmptyRecord => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.EmptyRecord);

        public static string True => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.True);

        public static string False => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.False);

        public static string InvalidFilterMessage => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.InvalidFilterMessage);

        public static string GroupDropArea => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.GroupDropArea);

        public static string UnGroup => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.UnGroup);

        public static string GroupDisable => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.GroupDisable);

        public static string FilterbarTitle => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterbarTitle);

        public static string EmptyDataSourceError => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.EmptyDataSourceError);

        public static string Add => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Add);

        public static string Back => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Back);

        public static string Edit => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Edit);

        public static string Cancel => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Cancel);

        public static string Update => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Update);

        public static string Delete => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Delete);

        public static string Print => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Print);

        public static string Pdfexport => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Pdfexport);

        public static string Excelexport => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Excelexport);

        public static string Wordexport => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Wordexport);

        public static string Csvexport => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Csvexport);

        public static string Search => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Search);

        public static string Columnchooser => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Columnchooser);

        public static string Save => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Save);

        public static string Item => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Item);

        public static string Items => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Items);

        public static string EditOperationAlert => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.EditOperationAlert);

        public static string DeleteOperationAlert => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.DeleteOperationAlert);

        public static string SaveButton => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.SaveButton);

        public static string OKButton => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.OKButton);

        public static string CancelButton => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.CancelButton);

        public static string EditFormTitle => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.EditFormTitle);

        public static string AddFormTitle => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.AddFormTitle);

        public static string BatchSaveConfirm => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.BatchSaveConfirm);

        public static string BatchSaveLostChanges => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.BatchSaveLostChanges);

        public static string ConfirmDelete => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.ConfirmDelete);

        public static string CancelEdit => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.CancelEdit);

        public static string ChooseColumns => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.ChooseColumns);

        public static string SearchColumns => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.SearchColumns);

        public static string Matchs => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Matchs);

        public static string FilterButton => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterButton);

        public static string ClearButton => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.ClearButton);

        public static string Like => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Like);

        public static string IsNull => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.IsNull);

        public static string IsNotNull => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.IsNotNull);

        public static string IsEmpty => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.IsEmpty);

        public static string IsNotEmpty => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.IsNotEmpty);

        public static string StartsWith => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.StartsWith);

        public static string DoesNotStartWith => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.DoesNotStartWith);

        public static string EndsWith => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.EndsWith);

        public static string DoesNotEndWith => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.DoesNotEndWith);

        public static string Contains => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Contains);

        public static string DoesNotContain => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.DoesNotContain);

        public static string Equal => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Equal);

        public static string NotEqual => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.NotEqual);

        public static string LessThan => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.LessThan);

        public static string LessThanOrEqual => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.LessThanOrEqual);

        public static string GreaterThan => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.GreaterThan);

        public static string GreaterThanOrEqual => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.GreaterThanOrEqual);

        public static string ChooseDate => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.ChooseDate);
		
		public static string ChooseTime => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.ChooseTime);

        public static string Copy => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Copy);

        public static string EnterValue => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.EnterValue);

        public static string Group => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Group);

        public static string Ungroup => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Ungroup);

        public static string AutoFitAll => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.AutoFitAll);

        public static string AutoFit => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.AutoFit);

        public static string Export => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Export);

        public static string FirstPage => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FirstPage);

        public static string LastPage => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.LastPage);

        public static string PreviousPage => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.PreviousPage);

        public static string NextPage => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.NextPage);

        public static string SortAscending => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.SortAscending);

        public static string SortDescending => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.SortDescending);

        public static string SortedAscending => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.SortedAscending);

        public static string SortedDescending => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.SortedDescending);

        public static string EditRecord => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.EditRecord);

        public static string DeleteRecord => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.DeleteRecord);

        public static string FilterMenu => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterMenu);

        public static string CurrentPageInfo => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.CurrentPageInfo);

        public static string SelectAll => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.SelectAll);

        public static string AddCurrentSelection => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.AddCurrentSelection);

        public static string Blanks => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Blanks);

        public static string FilterTrue => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterTrue);

        public static string FilterFalse => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterFalse);

        public static string NoResult => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.NoResult);

        public static string ClearFilter => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.ClearFilter);

        public static string NumberFilter => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.NumberFilter);

        public static string TextFilter => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.TextFilter);

        public static string DateFilter => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.DateFilter);
		
		public static string TimeFilter => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.TimeFilter);

        public static string DateTimeFilter => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.DateTimeFilter);

        public static string MatchCase => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.MatchCase);

        public static string Between => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Between);

        public static string CustomFilter => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.CustomFilter);

        public static string CustomFilterPlaceHolder => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.CustomFilterPlaceHolder);

        public static string CustomFilterDatePlaceHolder => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.CustomFilterDatePlaceHolder);

        public static string AND => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.AND);

        public static string OR => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.OR);

        public static string ShowRowsWhere => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.ShowRowsWhere);

        public static string RowSelectionCheckBoxARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.RowSelectionCheckBoxARIA);

        public static string HeaderSelectionCheckBoxARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.HeaderSelectionCheckBoxARIA);

        public static string FilterCheckBoxARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterCheckBoxARIA);

        public static string ColumnHeaderARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.ColumnHeaderARIA);

        public static string FilterMenuIconARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterMenuIconARIA);

        public static string ColumnMenuIconARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.ColumnMenuIconARIA);

        public static string UnGroupButtonARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.UnGroupButtonARIA);

        public static string GroupButtonARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.GroupButtonARIA);

        public static string FilterDescription => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterDescription);

        public static string SortDescription => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.SortDescription);

        public static string ColumnMenuDescription => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.ColumnMenuDescription);
        
        public static string GroupDescription => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.GroupDescription);
        
        public static string GroupCaption => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.GroupCaption);
        
        public static string TemplateColumnARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.TemplateColumnARIA);
        
        public static string GroupedSortIcon => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.GroupedSortIcon);
        
        public static string EmptyColumnHeaderUndefinedARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.EmptyColumnHeaderUndefinedARIA);
        
        public static string Close => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Close);

        public static string FilterOperator => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterOperator);
        
        public static string FilterValue => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterValue);

        public static string FilterBar => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.FilterBar);

        public static string SelectRowARIA => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.SelectRowARIA);

        public static string Undo => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Undo);

        public static string Redo => GridLocaleKeys.Prefix + nameof(GridLocaleKeys.Redo);

    }

    internal class MouseAndKeyArgs
    {
        public bool CtrlKey { get; set; }

        public bool ShiftKey { get; set; }

        public bool AltKey { get; set; }

        public string? Type { get; set; }

        public bool IsKeyEvent { get; set; }

        public bool IsRowStateChanged { get; set; }

        public MouseEventArgs? Click { get; set; }

        internal bool IsVerticalArrowPressed { get; set; }
    }

    /// <summary>
    /// Represents the model for an autocomplete filter in the grid, including column, data source, and filter configuration.
    /// </summary>
    public class FilterAutoCompleteModel
    {

        /// <summary>
        /// Gets or sets the grid column associated with the autocomplete filter.
        /// </summary>
        public GridColumn? Column { get; set; }

        /// <summary>
        /// Gets or sets the DataManager used for fetching filter data.
        /// </summary>
        public DataManager? DataManager { get; set; }

        /// <summary>
        /// Gets or sets the value used for filtering.
        /// </summary>
        public string? FilterValue { get; set; }

        /// <summary>
        /// Gets or sets the placeholder text displayed in the filter input.
        /// </summary>
        public string? PlaceHolder { get; set; }

        /// <summary>
        /// Gets or sets the JSON data source for the filter.
        /// </summary>
        public IEnumerable<object>? Json { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the filter input.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the operator used for filtering.
        /// </summary>
        public string? FilterOperator { get; set; }
    }

    /// <summary>
    /// Represents the model for a dropdown editor in the grid, including column details and configuration options.
    /// </summary>
    public class EditorDropDownModel
    {
        /// <summary>
        /// Gets or sets the grid column associated with the dropdown editor.
        /// </summary>
        public GridColumn? Column { get; set; }

        /// <summary>
        /// Indicates whether the dropdown editor is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the selected value of the dropdown editor.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets the data source for the dropdown items.
        /// </summary>
        public IEnumerable<object>? DropData { get; set; }

        /// <summary>
        /// Gets or sets the placeholder text displayed in the dropdown editor.
        /// </summary>
        public string? PlaceHolder { get; set; }

        /// <summary>
        /// Indicates whether the dropdown uses a DataManager for data operations.
        /// </summary>
        public bool isDataManager { get; set; }

        /// <summary>
        /// Gets or sets the float label type for the dropdown editor.
        /// </summary>
        public FloatLabelType FloatLabelType { get; set; }

        /// <summary>
        /// Gets or sets additional HTML attributes.
        /// </summary>
        public IDictionary<string, object>? Attributes { get; set; }

        /// <summary>
        /// Gets or sets additional attributes for configuring the DataManager.
        /// </summary>
        public IDictionary<string, object>? DataManagerAttributes { get; set; }

        /// <summary>
        /// Gets or sets the value expression used for binding the dropdown value.
        /// </summary>
        public object? ValueExpression { get; set; }
    }
    /// <summary>
    /// Represents a grouped data item in the grid, including its position, hierarchy, and state.
    /// </summary>
    internal class GroupedDataItem
    {
        /// <summary>
        /// Gets or sets the index of the grouped item.
        /// </summary>
        internal int? Index { get; set; }

        /// <summary>
        /// Gets or sets the row index of the grouped item.
        /// </summary>
        internal int RowIndex { get; set; }

        /// <summary>
        /// Gets or sets the indent level for hierarchical grouping.
        /// </summary>
        internal int Indent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grouped item is visible.
        /// </summary>
        internal bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the grouped item.
        /// </summary>
        internal string? Uid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the parent grouped item.
        /// </summary>
        internal string? ParentUid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grouped item is expanded.
        /// </summary>
        internal bool IsExpand { get; set; }

        /// <summary>
        /// Gets or sets the actual data item associated with this group.
        /// </summary>
        internal object? Item { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the row is a caption row.
        /// </summary>
        internal bool IsCaptionRow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grouped item is selected.
        /// </summary>
        internal bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item represents a group footer (summary) row.
        /// </summary>
        internal bool IsFooterRow { get; set; }
    }

    /// <summary>
    /// Provides extension methods for the AutoSpanMode enumeration.
    /// Used by both MergeHandler and GridColumn to determine and compute cell spanning modes.
    /// </summary>
    internal static class AutoSpanningExtensions
    {
        /// <summary>
        /// Determines if the specified AutoSpanMode supports row spanning.
        /// </summary>
        /// <param name="mode">The AutoSpanMode to check.</param>
        /// <returns>True if the mode supports row spanning; otherwise, false.</returns>
        internal static bool HasRow(this AutoSpanMode mode)
            => mode == AutoSpanMode.Column || mode == AutoSpanMode.HorizontalAndVertical;

        /// <summary>
        /// Determines if the specified AutoSpanMode supports column spanning.
        /// </summary>
        /// <param name="mode">The AutoSpanMode to check.</param>
        /// <returns>True if the mode supports column spanning; otherwise, false.</returns>
        internal static bool HasColumn(this AutoSpanMode mode)
            => mode == AutoSpanMode.Row || mode == AutoSpanMode.HorizontalAndVertical;

        /// <summary>
        /// Computes the effective spanning mode by intersecting grid-level and column-level AutoSpan settings.
        /// </summary>
        /// <param name="grid">The grid-level AutoSpanMode.</param>
        /// <param name="column">The column-level AutoSpanMode.</param>
        /// <returns>The resulting AutoSpanMode after intersection of both parameters.</returns>
        internal static AutoSpanMode Intersect(AutoSpanMode grid, AutoSpanMode column)
        {
            bool row = grid.HasRow() && column.HasRow();
            bool col = grid.HasColumn() && column.HasColumn();
            if (row && col) return AutoSpanMode.HorizontalAndVertical;
            if (row) return AutoSpanMode.Column;
            if (col) return AutoSpanMode.Row;
            return AutoSpanMode.None;
        }
    }
}
