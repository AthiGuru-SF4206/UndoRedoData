using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms;

#region Syncfusion
using Syncfusion.Blazor.Navigations;
using Syncfusion.PdfExport;
using Syncfusion.ExcelExport;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Grids.Internal;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Internal;
using System.Collections.Specialized;

#endregion

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Defines predicate model class for generating filter criteria.
    /// </summary>
    public class PredicateModel<T>
    {
        /// <summary>
        /// Defines the field name of the filter column.
        /// </summary>        
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// If IgnoreAccent is set to true, then filter ignores the diacritic characters or accents while filtering.
        /// </summary>
        public bool IgnoreAccent { get; set; }

        /// <summary>
        /// If match case set to true, then filter records with exact match or else
        /// filter records with case insensitive(uppercase and lowercase letters treated as same).
        /// </summary>
        public bool MatchCase { get; set; }

        /// <summary>
        /// Defines the operator to filter records.
        /// <seealso cref="Syncfusion.Blazor.Operator"/>
        /// </summary>
        public Syncfusion.Blazor.Operator Operator { get; set; } = Syncfusion.Blazor.Operator.None;

        /// <summary>
        /// Defines the relationship between one filter query and another by using AND or OR predicate.
        /// </summary>
        public string? Predicate { get; set; }

        /// <summary>
        /// Defines the UID of filter column.
        /// </summary>
        public string? Uid { get; internal set; }

        /// <summary>
        /// Defines the value used to filter records.
        /// </summary>
        public T? Value { get; set; }

        /// <summary>
        /// Defines the Collection/Original value used to filter records.
        /// </summary>
        public object? ActualValue { get; set; }
    }

    /// <summary>
    /// Defines predicate model class for generating filter criteria.
    /// </summary>
    public class PredicateModel : PredicateModel<object>
    {
    }

    /// <summary>
    /// This model used for Tree grid internal event support for Excel export row data bound.
    /// </summary>
    internal class ExportRowDataBound<T>
    {
        /// <summary>
        /// Defines the grid Row data while processing the records.
        /// </summary>
        public T? RowData { get; set; }

        /// <summary>
        /// Grouping Outline level.
        /// </summary>
        public int OutlineLevel { get; set; }

        /// <summary>
        /// export sheet row.
        /// </summary>
        public Row? SheetRow { get; set; }

        /// <summary>
        /// Excel Grouping row property.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Collapse/Expand the Grouping row.
        /// </summary>
        public bool IsCollapsed { get; set; }
    }

    /// <summary>
    /// Defines the filter param class which provides column level filter operator and read method to get data
    /// from Filter bar template.
    /// </summary>
    public class FilterSettings
    {
        /// <summary>
        /// Change the default filter operator for a column.
        /// </summary>
        public Operator? Operator { get; set; }

        /// <summary>
        /// Change the filter type for particular column.
        /// </summary>
        public FilterType? Type { get; set; }

        /// <summary>
        /// Gets or sets whether to show the filter bar operator dropdown for this column.
        /// </summary>
        /// <value>
        /// <c>true</c> to show the dropdown; <c>false</c> to hide it; <c>null</c> to inherit from parent <see cref="Syncfusion.Blazor.Grids.FilterSettings.ShowFilterBarOperator"/>. Default is <c>null</c>.
        /// </value>
        /// <remarks>
        /// When the parent <see cref="Syncfusion.Blazor.Grids.FilterSettings.ShowFilterBarOperator"/> is <c>false</c>, this setting is ignored and no dropdown is shown.
        /// When the parent is <c>true</c>, this property controls the column-level behavior.
        /// </remarks>
        public bool? ShowFilterBarOperator { get; set; }
    }

    /// <summary>
    /// Defines the column validation rules.
    /// </summary>
    public class ValidationRules : ValidationRuleBase
    {
        internal static ValidationRules ToRules(ValidationRuleBase a)
        {
            return new ValidationRules()
            {
                Email = a.Email,
                Max = a.Max,
                Min = a.Min,
                MaxLength = a.MaxLength,
                MinLength = a.MinLength,
                Number = a.Number,
                Range = a.Range,
                RangeLength = a.RangeLength,
                RegexPattern = a.RegexPattern,
                Required = a.Required,
                Messages = a.Messages
            };
        }
    }

    /// <summary>
    /// Internal Action args.
    /// </summary>
    /// <exclude/>
    public class ActionArgs
    {
        /// <summary>
        /// Gets or sets the axis of the action.
        /// </summary>
        [JsonPropertyName("axis")]
        public string? Axis { get; set; }

        /// <summary>
        /// Gets or sets the ending column index.
        /// </summary>
        [JsonPropertyName("endColumnIndex")]
        public int EndColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets the UIDs of the starting columns.
        /// </summary>
        [JsonPropertyName("fromColumnUid")]
        public string[]? FromColumnUid { get; set; }

        /// <summary>
        /// Gets or sets the original index of the column.
        /// </summary>
        [JsonPropertyName("fromIndex")]
        public int FromIndex { get; set; }

        /// <summary>
        /// Gets or sets whether multiple columns are being reordered in a single operation.
        /// </summary>
        [JsonPropertyName("isMultipleReorder")]
        public bool IsMultipleReorder { get; set; }

        /// <summary>
        /// Gets or sets the row height.
        /// </summary>
        [JsonPropertyName("rHeight")]
        public double RHeight { get; set; }

        /// <summary>
        /// Gets or sets the internal request type identifier (for example, "reorder", "resize", "virtualscroll", etc.).
        /// </summary>
        [DefaultValue(null)]
        [JsonPropertyName("requestType")]
        public string? RequestType { get; set; }

        /// <summary>
        /// Gets or sets the start column index.
        /// </summary>
        [JsonPropertyName("startColumnIndex")]
        public int StartColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets the UID of the destination column.
        /// </summary>
        [JsonPropertyName("toColumnUid")]
        public string? ToColumnUid { get; set; }

        /// <summary>
        /// Gets or sets the destination index after the action completes.
        /// </summary>
        [JsonPropertyName("toIndex")]
        public int ToIndex { get; set; }

        /// <summary>
        /// Gets or sets the horizontal translation value.
        /// </summary>
        [JsonPropertyName("translateX")]
        public double TranslateX { get; set; }

        /// <summary>
        /// Gets or sets the vertical translation value.
        /// </summary>
        [JsonPropertyName("translateY")]
        public double TranslateY { get; set; }

        /// <summary>
        /// Gets or sets the internal action type.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the virtual table width.
        /// </summary>
        [JsonPropertyName("vTableWidth")]
        public double VTableWidth { get; set; }

        /// <summary>
        /// Gets or sets the virtual end index.
        /// </summary>
        [JsonPropertyName("virtualEndIndex")]
        public int VirtualEndIndex { get; set; }

        /// <summary>
        /// Gets or sets the virtual start index.
        /// </summary>
        [JsonPropertyName("virtualStartIndex")]
        public int VirtualStartIndex { get; set; }

        /// <summary>
        /// Gets or sets the next row index to navigate.
        /// </summary>
        [JsonPropertyName("nextRowToNavigate")]
        public int NextRowToNavigate { get; set; }

        /// <summary>
        /// Gets or sets the selected cell navigation index.
        /// </summary>
        [JsonPropertyName("selectedCellNavigation")]
        public int SelectedCellNavigation { get; set; }

        /// <summary>
        /// Gets or sets the selected row navigation index.
        /// </summary>
        [JsonPropertyName("selectedRowNavigation")]
        public double SelectedRowNavigation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether scrolling is triggered by focus.
        /// </summary>
        [JsonPropertyName("isScrollByFocus")]
        public bool IsScrollByFocus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the header is navigated.
        /// </summary>
        [JsonPropertyName("isHeaderNavigated")]
        public bool IsHeaderNavigated { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the width value.
        /// </summary>
        [JsonPropertyName("width")]
        public string? Width { get; set; }

        /// <summary>
        /// Gets or sets the media column visibility settings.
        /// </summary>
        [JsonPropertyName("mediaColVisibility")]
        public Dictionary<string, bool>? MediaColVisibility { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether media invokes the action.
        /// </summary>
        [JsonPropertyName("invokedByMedia")]
        public bool InvokedByMedia { get; set; }

        /// <summary>
        /// Gets or sets the column UID.
        /// </summary>
        [JsonPropertyName("columnUid")]
        public string? ColumnUid { get; set; }

        /// <summary>
        /// Gets or sets the collection of frozen column UIDs.
        /// </summary>
        [JsonPropertyName("frozenColumnsUidCollection")]
        public string[]? FrozenColumnsUidCollection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is frozen.
        /// </summary>
        [JsonPropertyName("isFrozen")]
        public bool IsFrozen { get; set; }

        /// <summary>
        /// Gets or sets the freeze direction.
        /// </summary>
        [JsonPropertyName("freezeDirection")]
        public string? FreezeDirection { get; set; }

        /// <summary>
        /// Gets or sets the freeze line moving direction.
        /// </summary>
        [JsonPropertyName("freezeLineMovingDirection")]
        public string? FreezeLineMovingDirection { get; set; }

        /// <summary>
        /// Gets or sets the count of frozen columns.
        /// </summary>
        [JsonPropertyName("frozenColumnsCount")]
        public int FrozenColumnsCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid structure has changes.
        /// </summary>
        [JsonPropertyName("hasGridStructureChanges")]
        public bool hasGridStructureChanges { get; set; }

        /// <summary>
        /// Gets or sets the indentation width used for hierarchical/tree layouts (CSS length).
        /// </summary>
        [JsonPropertyName("indentWidth")]
        public string? IndentWidth { get; set; }

        /// <summary>
        /// Indicates whether the current cell is the row-drag handle cell.
        /// </summary>
        [JsonPropertyName("isRowDragCell")]
        public bool? IsRowDragCell { get; set; }

        /// <summary>
        /// Indicates whether the client device is macOS (used for platform-specific behavior).
        /// </summary>
        [JsonPropertyName("isMacDevice")]
        public bool? IsMacDevice { get; set; }

        /// <summary>
        /// Gets or sets the row height to apply/measure for the current action (in pixels).
        /// </summary>
        [JsonPropertyName("rowHeight")]
        public int? RowHeight { get; set; }
    }

/// <summary>
/// Class that defines template context detail of EmptyRecordTemplate.
/// <seealso cref="Syncfusion.Blazor.Grids.GridTemplates.EmptyRecordTemplate"/>
/// </summary>
public class EmptyRecordTemplateContext
    {
        /// <summary>
        /// Specifies whether data is loaded in the grid.
        /// </summary>
        public bool IsDataLoaded { get; set; }
    }

    /// <summary>
    /// Provides contextual information for customizing tooltip content in the Blazor DataGrid via the <see cref="Syncfusion.Blazor.Grids.GridTemplates.TooltipTemplate"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This context is passed to the <see cref="Syncfusion.Blazor.Grids.GridTemplates.TooltipTemplate"/> to enable dynamic rendering of tooltip content based on the hovered cell.
    /// </para>
    /// <para>
    /// The context includes details such as the cell value, row and column indices, associated data object, and column metadata.
    /// </para>
    /// <para>
    /// When hovering over a content cell, the <see cref="RowIndex"/> will be a non-negative value and <see cref="Data"/> will contain the corresponding data object. 
    /// When hovering over a header cell, <see cref="RowIndex"/> will be <c>-1</c> and <see cref="Data"/> will be <see langword="null"/>.
    /// </para>
    /// <para>
    /// These properties can be used to apply conditional styling or logic within the tooltip template, allowing differentiation between header and content cells.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// <SfGrid DataSource="@Orders" ShowTooltip="true">
    /// <GridTemplates>
    /// <TooltipTemplate>
    /// @{
    ///    var tooltip = context as TooltipTemplateContext;
    ///    @if (tooltip.RowIndex == -1)
    /// {
    ///    @tooltip.Value 
    /// }
    /// else {
    ///    @tooltip.Column.Field
    /// }
    /// }
    /// </TooltipTemplate>
    /// </GridTemplates>
    /// <GridColumns>
    /// <GridColumn Field=@nameof(Order.OrderID) HeaderText="Order ID"></GridColumn>
    /// </GridColumns>
    /// </SfGrid>
    /// ]]>
    /// </code>
    /// </example>

    public class TooltipTemplateContext
    {
        /// <summary>
        /// Gets the value of the currently hovered cell.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the value displayed in the cell. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// This value is useful for displaying raw or formatted data in the tooltip. For templated columns or special display types like <c>DisplayAsCheckbox</c>, it reflects the underlying data value.
        /// </para>
        /// <para>
        /// Applicable for both header and content cells. When hovering over a header cell, this property returns the column's <c>HeaderText</c>. For content cells, it returns the corresponding cell value.
        /// </para>
        /// </remarks>
        public string? Value { get; internal set; }

        /// <summary>
        /// Gets the column index of the currently hovered column.
        /// </summary>
        /// <value>
        /// An integer representing the column index of the hovered cell.
        /// </value>
        public int ColumnIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the currently hovered row.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> representing the row index. Returns <c>-1</c> for header cells.
        /// </value>
        /// <remarks>
        /// <para>
        /// When hovering over a content cell, this property returns the corresponding row index within the grid.
        /// </para>
        /// <para>
        /// For header cells, it always returns <c>-1</c>. This distinction is useful for customizing tooltip styles or behavior based on whether the hovered element is a header or a content cell.
        /// </para>
        /// </remarks>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets the data object of the currently hovered row.
        /// </summary>
        /// <value>
        /// The data associated with the hovered row. Returns <c>null</c> for header cells.
        /// </value>
        /// <remarks>
        /// This object provides access to the full row data, enabling dynamic tooltip content based on multiple fields or business logic.
        /// When hovering over a content cell, this property returns the corresponding row object. 
        /// For header cells, it always returns <c>null</c>. This distinction is useful for customizing tooltip styles or behavior 
        /// based on whether the hovered element is a header or a content cell.    
        /// </remarks>
        public object? Data { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn"/> associated with the currently hovered cell.
        /// </summary>
        /// <value>
        /// A <see cref="GridColumn"/> object containing metadata such as field name, header text, and formatting options.
        /// </value>
        /// <remarks>
        /// This property provides access to column metadata, enabling dynamic tooltip content based on column-specific information.
        /// </remarks>
        public GridColumn? Column { get; internal set; }
    }

    /// <summary>
    /// Class that defines template context detail of FooterTemplate, GroupFooterTemplate and GroupCaptionTemplate.
    /// <seealso cref="Syncfusion.Blazor.Grids.GridAggregateColumn.FooterTemplate"/>
    /// <seealso cref="Syncfusion.Blazor.Grids.GridAggregateColumn.GroupCaptionTemplate"/>
    /// <seealso cref="Syncfusion.Blazor.Grids.GridAggregateColumn.GroupFooterTemplate"/>
    /// </summary>
    public class AggregateTemplateContext
    {
        /// <summary>
        /// Gets average aggregate value.
        /// </summary>
        public string? Average { get; internal set; }

        /// <summary>
        /// Gets count aggregate value.
        /// </summary>
        public string? Count { get; internal set; }

        /// <summary>
        /// Gets custom aggregate value.
        /// </summary>
        public string? Custom { get; internal set; }

        /// <summary>
        /// Specifies false count aggregate value.
        /// </summary>
        public string? FalseCount { get; internal set; }

        /// <summary>
        /// Gets the current group field name.
        /// </summary>
        public string? Field { get; internal set; }

        /// <summary>
        /// Gets corresponding grouped foreign key value.
        /// </summary>
        public string? ForeignKey { get; internal set; }

        /// <summary>
        /// Gets header text of the grouped column.
        /// </summary>
        public string? HeaderText { get; internal set; }

        /// <summary>
        /// Gets grouped data key value.
        /// </summary>
        public string? Key { get; internal set; }

        /// <summary>
        /// Gets maximum aggregate value.
        /// </summary>
        public string? Max { get; internal set; }

        /// <summary>
        /// Gets minimum aggregate value.
        /// </summary>

        public string? Min { get; internal set; }

        /// <summary>
        /// Gets sum aggregate value.
        /// </summary>
        public string? Sum { get; internal set; }

        /// <summary>
        /// Gets true count aggregate value.
        /// </summary>
        public string? TrueCount { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnBatchAdd"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class BeforeBatchAddArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to prevent the rendering of the batch add form in the grid.
        /// </summary>
        /// <value>
        /// If set to <c>true</c>, the batch add form will not be rendered in the grid. The default value is <c>false</c>
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the value for a newly added row.
        /// </summary>
        /// <value>
        /// The default value is <c>null</c>.
        /// </value>
        /// <remarks>
        /// If a custom default value is provided, it will be displayed in the batch add form when a new row is added.
        /// </remarks>
        public T? DefaultData { get; set; }

        /// <summary>
        /// Gets the primary key value of the grid for the columns that have <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/> property set to true.
        /// </summary>
        /// <value>
        /// A string array that represents the primary key value of the grid.
        /// </value>
        public string[]? PrimaryKey { get; internal set; }

        /// <summary>
        /// Gets or sets the index of the row to add a new row in the grid.
        /// </summary>
        /// <value>
        /// An integer representing the index of the newly added row. The default value is 0.
        /// </value>
        /// <remarks>
        /// If the index property is set, a batch add form will be generated in the grid based on the specified index.
        /// </remarks>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the data related to the editing process, such as flags indicating which fields have been modified and the current set of validation messages.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Microsoft.AspNetCore.Components.Forms.EditContext"/> class that provides data about the editing process.
        /// </value>
        public EditContext? EditContext { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnBatchDelete"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class BeforeBatchDeleteArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the before batch delete action.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then the before batch delete action will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the primary key value of the grid for the columns that have <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/> property set to true.
        /// </summary>
        /// <value>
        /// A string? array that represents the primary key value of the grid.
        /// </value>
        public string[]? PrimaryKey { get; internal set; }

        /// <summary>
        /// Gets the data of the selected row to perform batch delete action.
        /// </summary>
        /// <value>
        /// The data of the selected row. The default value is null.
        /// </value>
        public T? RowData { get; internal set; }

        /// <summary>
        /// Gets the row index of the selected record to perform batch delete action.
        /// </summary>
        /// <value>
        /// The row index of the selected record.
        /// </value>
        /// <remarks>
        /// When multiple rows are selected and the delete action is performed, the row index property will contain the index of the last selected row that was deleted.
        /// </remarks>
        public int RowIndex { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnBatchSave"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class BeforeBatchSaveArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the collection of <see cref="Syncfusion.Blazor.Grids.BatchChanges{T}"/> records when save action is performed in the grid.
        /// </summary>
        /// <value>
        /// The collection of <c>Added</c>, <c>Deleted</c>, and <c>Changed</c> records.
        /// </value>
        public BatchChanges<T>? BatchChanges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the batch save action.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then the batch save action will be cancelled.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.PageChanging"/> event.
    /// </summary>
    public class GridPageChangingEventArgs : PageChangingEventArgs
    {
        /// <summary>
        /// Gets the number of items displaying on the single page of the pager.
        /// </summary>
        /// <value>
        /// The number of items shown on a single page.
        /// </value>
        public int CurrentPageSize { get; internal set; }

        /// <summary>
        /// Gets the total number of pages calculated using <see cref="SfPager.TotalItemsCount"/> and <see cref="SfPager.PageSize"/>.
        /// </summary>
        /// <value>
        /// Total number of pages.
        /// </value>
        public int TotalPages { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.PageChanged"/> event.
    /// </summary>
    public class GridPageChangedEventArgs : PageChangedEventArgs
    {
        /// <summary>
        /// Gets the number of items displaying on the single page of the pager.
        /// </summary>
        /// <value>
        /// The number of items shown on a single page.
        /// </value>
        public int CurrentPageSize { get; internal set; }

        /// <summary>
        /// Gets the total number of pages calculated using <see cref="SfPager.TotalItemsCount"/> and <see cref="SfPager.PageSize"/>.
        /// </summary>
        /// <value>
        /// Total number of pages.
        /// </value>
        public int TotalPages { get; internal set; }
    }



    /// <summary>
    /// Defines the event argument of  batch cancel action.
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnBatchCancel"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class BeforeBatchCancelArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the collection of <see cref="Syncfusion.Blazor.Grids.BatchChanges{T}"/> records when cancel action is performed in the grid.
        /// </summary>
        /// <value>
        /// The collection of <c>Added</c>, <c>Deleted</c>, and <c>Changed</c> records.
        /// </value>
        public BatchChanges<T>? BatchChanges { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the batch cancel action.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then the batch cancel action will be cancelled.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnDataBound"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class BeforeDataBoundArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the data binding process before it occurs in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then the data will not be bound in the grid.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the total number of data items that are bound to the grid.
        /// </summary>
        /// <value>
        /// The total number of data items that are bound to the grid.
        /// </value>
        public int Count { get; internal set; }

        /// <summary>
        /// Gets the list of <c>CurrentViewData</c> of the grid.
        /// </summary>
        /// <value>
        /// The list of current view data.
        /// </value>
        /// <remarks>
        /// If the <c>AllowPaging</c> property is set to <c>true</c>, the view will display data based on the <see cref="Syncfusion.Blazor.Grids.GridPageSettings.PageSize"/> property. If <c>AllowPaging</c> is set to <c>false</c>, then all the items in the grid will be displayed.
        /// </remarks>
        public List<T>? Result { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.BeforeCopyPaste"/> event.
    /// </summary>
    public class BeforeCopyPasteEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the current action whether it is Copy or Paste.
        /// </summary>
        /// <value>
        /// When copy action is performed then the value will be <c>Copy</c> and when paste action is performed then the value will be <c>Paste</c>. 
        /// </value>
        /// <remarks>
        /// If the <b>Copy</b> action is prevented using <c>Cancel</c> argument, then the corresponding <b>Paste</b> events doesn't trigger, since Paste events will be triggered based on clipboard text.  
        /// </remarks>
        public string? Action { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the entire copy, paste action in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then the <c>Copy</c> and <c>Paste</c> action will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the copied content of the selected cells or rows.
        /// </summary>  
        /// <value>
        /// A string that represents the copied content of the selected cells or rows. The default value is <c>null</c>.
        /// </value>
        [JsonPropertyName("clipboardText")]
        public string? ClipboardText { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.BeforeCellPaste"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    public class BeforeCellPasteEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the paste action of the cell in grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then cell paste action will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the value being pasted in the cell. You can modify the value using <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.BeforeCellPaste"/> event.
        /// </summary>    
        /// <value>
        /// The string value being pasted in the cell.  The default value is <c>null</c>.
        /// </value>
        [JsonPropertyName("cellValue")]
        public string? CellValue { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> name associated with the cell being pasted.
        /// </summary>
        /// <value>
        /// The name of the field associated with the cell being pasted.
        /// </value>
        public string? ColumnName { get; internal set; }

        /// <summary>
        /// Gets the column index of the cell associated with a paste action.
        /// </summary>    
        /// <value>
        /// An integer value that represents the column index of the cell associated with the paste action.
        /// </value>  
        public int ColumnIndex { get; internal set; }

        /// <summary>
        /// Gets the selected row data of the cell associated with a paste action.
        /// </summary>
        /// <value>
        /// The data of the selected row.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets the row index of the cell associated with a paste action.
        /// </summary>
        /// <value>
        /// The index of the row associated with the paste action.
        /// </value>
        public int RowIndex { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnBeginEdit"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    public class BeginEditArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the edit action in grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then edit action will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the list of <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/> <b>true</b> field names of the column.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> names of the column with <c>IsPrimaryKey</c> set to <b>true</b>.
        /// </value>
        public string[]? PrimaryKey { get; internal set; }

        /// <summary>
        /// Gets the list of primary key values where <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/> is <b>true</b>.
        /// </summary>
        /// <value>
        /// The list of primary key values associated with the column where <c>IsPrimaryKey</c> is <b>true</b>.
        /// </value>
        public string[]? PrimaryKeyValue { get; internal set; }

        /// <summary>
        /// Gets the data of the row that is currently selected for editing.
        /// </summary>
        /// <value>
        /// The data of the currently selected row for editing.
        /// </value>
        public T? RowData { get; set; }

        /// <summary>
        /// Gets or sets the index of the row that is currently selected for editing.
        /// </summary>
        /// <value>
        /// The row index of the currently selected row for editing.
        /// </value>
        public int RowIndex { get; internal set; }
    }

    /// <summary>
    /// Class that defines the cell border details.
    /// </summary>
    public class Border
    {
        /// <summary>
        /// Defines the color of border.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Defines the line style of border.
        /// </summary>
        public LineStyle LineStyle { get; set; }
    }

    /// <summary>
    /// Class that defines template context detail of CaptionTemplate.
    /// <seealso cref="Syncfusion.Blazor.Grids.GridGroupSettings.CaptionTemplate"/>
    /// </summary>
    public class CaptionTemplateContext
    {
        /// <summary>
        /// Gets the group GUID.
        /// </summary>
        public string? GroupGuid { get; internal set; }

        /// <summary>
        /// Gets count value which specified the number of records in the group.
        /// </summary>
        public int Count { get; internal set; }

        /// <summary>
        /// Gets the current group field name.
        /// </summary>
        public string? Field { get; internal set; }

        /// <summary>
        /// Gets the current foreign key value name.
        /// </summary> 
        public string? ForeignKeyValue { get; internal set; }

        /// <summary>
        /// Gets corresponding grouped foreign key value.
        /// </summary>
        public string? ForeignKey { get; internal set; }

        /// <summary>
        /// Gets header text of the grouped column.
        /// </summary>
        public string? HeaderText { get; internal set; }

        /// <summary>
        /// Gets grouped data key value.
        /// </summary>
        public string? Key { get; internal set; }

        /// <summary>
        /// Gets depth or level in which the group caption is present.
        /// </summary>
        public int Level { get; internal set; }

        /// <summary>
        /// Retrieves the aggregate data context for a grouped column.
        /// </summary>
        /// <value>
        /// An instance of <see cref="AggregateTemplateContext"/> containing the aggregate values for the grouped column. 
        /// This object typically includes calculated aggregates such as sum, average, and other values.
        /// </value>
        /// <remarks>
        /// The <see cref="GroupAggregates"/> property provides access to aggregate information, including field names and corresponding aggregate values, which are useful when columns are grouped.
        /// It supports the retrieval of aggregate values for display within the group caption template. Note that these values are available only when the aggregated columns are positioned first.
        /// The GroupAggregates property contains values only if the first column has an associated group aggregate.If columns are dynamically hidden, shown, or reordered, and the new first column has a group aggregate, the GroupAggregates context will update to reflect this change. 
        /// It's important to note that GroupAggregates holds values only when the first column contains an aggregate value.
        /// </remarks>
        public AggregateTemplateContext? GroupAggregates { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CellDeselecting"/> event, and <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CellDeselected"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class CellDeselectEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the <see cref="MouseEventArgs"/> of the currently deselected/deselecting cell.
        /// </summary>
        public MouseEventArgs? Event { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the deselection of the cell.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then the deselection of the cell will be cancelled.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CellDeselected"/> event should not be cancelled since it is triggered after the completion of a selection.
        /// </remarks>
        public bool Cancel { get; set; }
        /// <summary>
        /// Gets the cell index for the currently deselected/deselecting cell.
        /// </summary>
        /// <value>
        /// The index of the cell that is currently being deselected or has been deselected.
        /// </value>
        public int CellIndex { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the CTRL key is currently pressed or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the CTRL key is pressed otherwise, <c>false</c>.
        /// The default value is <c>false</c>.
        /// </value>
        public bool IsCtrlPressed { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the SHIFT key is currently pressed or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the SHIFT key is pressed otherwise, <c>false</c>.
        /// The default value is <c>false</c>.
        /// </value>
        public bool IsShiftPressed { get; internal set; }

        /// <summary>
        /// Gets the index of the row for the currently deselecting or deselected cell.
        /// </summary>
        /// <value>
        /// An integer representing the row index of the current deselecting or deselected cell.
        /// </value>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets the row data associated with the currently deselecting or deselected cell in a grid.
        /// </summary>
        /// <value>
        /// An object of type <typeparamref name="T"/> representing the data associated with the currently deselecting or deselected row in a grid.
        /// </value>
        public T? Data { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnCellEdit"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class CellEditArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the edit action of the cell
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then the edit action of the cell will be cancelled.
        /// </value>
        public bool Cancel { get; set; }


        /// <summary>
        /// Gets the name of the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> of the currently selected cell for editing.
        /// </summary>
        /// <value>
        /// A string representing the field name of the currently selected cell for editing.
        /// </value>
        public string? ColumnName { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn"/> associated with the currently selected cell for editing.
        /// </summary>
        /// <value>
        /// A <c>GridColumn</c> object representing the column associated with the currently selected cell for editing.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the foreign key column data of the data grid.
        /// </summary>
        /// <value>
        /// A dictionary that represents the foreign key column data. Each key represents a foreign key column name and the value represents the associated data as an enumerable object.
        /// </value>
        public IDictionary<string, IEnumerable<object>>? ForeignKeyData { get; internal set; }

        /// <summary>
        /// Gets the boolean property value indicating whether the edited cell is associated with a foreign key column.
        /// </summary>
        /// <value>
        /// The value <c>true</c> if the edited cell is associated with foreign key column, otherwise it is <c>false</c>.
        /// </value>
        public bool IsForeignKey { get; internal set; }

        /// <summary>
        /// Gets the list of <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/> <b>true</b> field names of the column.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> names of the column with <c>IsPrimaryKey</c> set to <b>true</b>.
        /// </value>
        public string[]? PrimaryKey { get; internal set; }

        /// <summary>
        /// Gets the original data associated with the currently selected cell for editing.
        /// </summary>
        /// <value>
        /// An object of type <typeparamref name="T"/> representing the data associated with the currently selected cell for editing.
        /// </value>
        public T? RowData { get; internal set; }

        /// <summary>
        /// Gets the changed data associated with the currently selected cell for editing.
        /// </summary>
        /// <value>
        /// An object of type <typeparamref name="T"/> representing the changed data associated with the currently selected cell for editing.
        /// </value>
        /// <remarks>
        /// When editing a cell, you can change the cell value either in the edit form or programmatically in the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnCellSave"/> event handler. 
        /// The new value is updated in the underlying data source and displayed in the edited cell and in the edit form (if open).
        /// </remarks>
        public T? Data { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.ValidationRules"/> associated with the currently selected cell for editing.
        /// </summary>
        public ValidationRules? ValidationRules { get; internal set; }

        /// <summary>
        /// Gets the cell value associated with the currently selected cell for editing.
        /// </summary>        
        public string? Value { get; set; }

        /// <summary>
        /// Gets the data related to the editing process, such as flags indicating which fields have been modified and the current set of validation messages.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Microsoft.AspNetCore.Components.Forms.EditContext"/> class that provides data about the editing process.
        /// </value>
        public EditContext? EditContext { get; set; }
    }

    /// <summary> 
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnCellSave"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class CellSaveArgs<T> : CellSavedArgs<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the save action of the cell.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then the save action of the cell will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

    }   

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CellSaved"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class CellSavedArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> name of the currently edited cell.
        /// </summary>
        /// <value>
        /// A string value that represents the field name of the currently edited cell.
        /// </value>
        public string? ColumnName { get; internal set; }

        /// <summary>
        /// Gets the corresponding column associated with the edited cell.
        /// </summary>
        /// <value>
        /// A <see cref="Syncfusion.Blazor.Grids.GridColumn"/> associated with the edited cell.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the boolean property value indicating whether the edited cell is associated with a foreign key column.
        /// </summary>
        /// <value>
        /// The value <c>true</c> if the edited cell is associated with foreign key column, otherwise it is <c>false</c>.
        /// </value>
        public bool IsForeignKey { get; internal set; }

        /// <summary>
        /// Gets or sets the previously edited data of the currently edited cell.
        /// </summary>
        /// <value>
        /// The previous value of the currently edited cell.
        /// </value>
        public object? PreviousValue { get; internal set; }

        /// <summary>
        /// Gets the original data associated with the currently edited cell for saving.
        /// </summary>
        /// <value>
        /// An object of type <typeparamref name="T"/> representing the original data associated with the currently edited cell for saving.
        /// </value>
        public T? RowData { get; internal set; }

        /// <summary>
        /// Gets the changed data associated with the currently edited cell.
        /// </summary>
        /// <value>
        /// An object of type <typeparamref name="T"/> representing the changed data associated with the currently edited cell.
        /// </value>
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets the currently edited cell value.
        /// </summary>
        /// /// <value>
        /// The value of the currently edited cell. If the cell is empty or has no value, then the property returns null.
        /// </value>
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets the CellDOM object associated with the edited cell.
        /// </summary>
        /// <value>
        /// The CellDOM object that represents the edited cell.
        /// </value>
        /// <remarks>
        /// The CellDOM properties include:
        /// <list type="bullet">
        /// <item><description>
        /// <b>HasChanges:</b> A boolean property that indicates whether the Cell object associated with the edited cell has changed.
        /// </description></item>
        /// <item><description>
        /// <b>ClassList:</b> A property that contains the class list of the edited cell.
        /// </description></item>
        /// <item><description>
        /// <b>Styles:</b> A property that contains the styles of the edited cell.
        /// </description></item>
        /// <item><description>
        /// <b>AttributeList:</b> A property that contains the attribute list of the edited cell.
        /// </description></item>
        /// <item><description>
        /// <b>AddClass:</b> A method to add class names to the class list for the current edited cell.
        /// </description></item>
        /// <item><description>
        /// <b>AddStyle:</b> A method to add styles for the current edited cell.
        /// </description></item>
        /// <item><description>
        /// <b>SetAttribute:</b> A method to set an attribute for the current edited cell.
        /// </description></item>
        /// </list>
        /// </remarks>
        public CellDOM? CellInfo { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CellSelected"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class CellSelectEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the <see cref="MouseEventArgs"/> of the currently selected cell.
        /// </summary>
        public MouseEventArgs? Event { get; internal set; }

        /// <summary>
        /// Gets the cell index for the currently selected cell.
        /// </summary>
        /// <value>
        /// The index of the cell that is currently selected.
        /// </value>
        public int CellIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the row for the currently selected cell.
        /// </summary>
        /// <value>
        /// An integer representing the row index of the currently selected cell.
        /// </value>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the CTRL key is currently pressed or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the CTRL key is pressed otherwise, <c>false</c>.
        /// The default value is <c>false</c>.
        /// </value>
        public bool IsCtrlPressed { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the SHIFT key is currently pressed or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the SHIFT key is pressed otherwise, <c>false</c>.
        /// The default value is <c>false</c>.
        /// </value>
        public bool IsShiftPressed { get; internal set; }

        /// <summary>
        /// Gets the row data associated with the currently selected cell.
        /// </summary>
        /// <value>
        /// The row data associated with the currently selected cell.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets the previously selected cell index.
        /// </summary>
        /// <value>
        /// An integer value representing the previously selected cell index.
        /// </value>
        public int PreviousCellIndex { get; internal set; }

	    /// <summary>
        /// Gets the value of the selected cell.
        /// </summary>
        /// <value>
        /// Returns the value of the selected cell.
        /// </value>
        /// <remarks>
        /// If the <c>Field</c> property is not set for a GridColumn, such as a template column or checkbox column, then the corresponding cell value will be returned as null when selected.
        /// </remarks>
        public object? CurrentValue { get; internal set; }


        /// <summary>
        /// Gets the value of the previously selected cell.
        /// </summary>
        /// <value>
        /// Returns the previously selected cell value.
        /// </value>
        /// <remarks>
        /// If the <c>Field</c> property is not set for a GridColumn, such as a template column or checkbox column, then the corresponding cell value will be returned as null when selected.
        /// </remarks>
        public object? PreviousValue { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CellSelecting"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class CellSelectingEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the <see cref="MouseEventArgs"/> of the currently selecting cell.
        /// </summary>
        public MouseEventArgs? Event { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the selection of the cell.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If the Cancel property is set to true, then the selection of the cell will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the index of the currently selecting cell.
        /// </summary>
        /// <value>
        /// The index of the currently selecting cell.
        /// </value>
        public int CellIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the row associated with the currently selecting cell.
        /// </summary>
        /// <value>
        /// The index of the row associated with the currently selecting cell.
        /// </value>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets the row data associated with the currently selecting cell.
        /// </summary>
        /// <value>
        /// The row data associated with the currently selecting cell.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the CTRL key is currently pressed.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If thr CTRL key is pressed then the value is <c>true</c>.
        /// </value>
        public bool IsCtrlPressed { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the SHIFT key is currently pressed.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If SHIFT key is pressed then the value is <c>true</c>.
        /// </value>
        public bool IsShiftPressed { get; internal set; }

        /// <summary>
        /// Gets the previously selected cell index.
        /// </summary>
        /// <value>
        /// An integer value representing the previously selected cell index.
        /// </value>
        public int PreviousCellIndex { get; internal set; }

        /// <summary>
        /// Gets the value of the cell which is going to be selected.
        /// </summary>
        /// <value>
        /// Returns the value of the cell which is going to be selected.
        /// </value>
        /// <remarks>
        ///  If the <c>Field</c> property is not set for a GridColumn, such as a template column or checkbox column, then the corresponding cell value will be returned as null while selecting.
        /// </remarks>
        public object? CurrentValue { get; internal set; }

        /// <summary>
        /// Gets the value of the previously selected cell.
        /// </summary>
        /// <value>
        /// Returns the previously selected cell value.
        /// </value>
        /// <remarks>
        /// If the <c>Field</c> property is not set for a GridColumn, such as a template column or checkbox column, then the corresponding cell value will be returned as null while selecting.
        /// </remarks>
        public object? PreviousValue { get; internal set; }
    }

    /// <summary>
    /// Defines members of the column chooser template context.
    /// </summary>
    public class ColumnChooserTemplateContext
    {
        /// <summary>
        /// Gets or sets the columns list. If there is any search criteria applied then the columns which matches the search criteria will be provided.
        /// </summary>
        public List<GridColumn>? Columns { get; set; }
    }

    /// <summary>
    /// Defines members of the ColumnChooser FooterTemplate context.
    /// </summary>
    public class ColumnChooserFooterTemplateContext
    {
        /// <summary>
        /// Gets the list of columns from the data source.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of <see cref="GridColumn"/> objects representing the columns in the data source.
        /// </value>
        /// <remarks>
        /// This property returns a list of columns used by the data source. If no search criteria have been applied, the list will include all columns in the data source. If search criteria have been applied, only columns that match the criteria will be returned.
        /// </remarks>
        public List<GridColumn>? Columns { get; internal set; }

        /// <summary>
        /// Gets a function that cancels the column chooser operation and closes the dialog.
        /// </summary>
        /// <value>
        /// A <see cref="Func{TResult}"/> that returns a <see cref="Task"/> representing the async operation.
        /// </value>
        public Func<Task>? CancelAsync { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.BeforeOpenColumnChooser"/> event.
    /// </summary>
    public class ColumnChooserEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the column chooser popup open.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If the Cancel property is set to true, then the column chooser popup open will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the list of <see cref="Syncfusion.Blazor.Grids.GridColumn"/> that is being displayed in the column chooser pop up.
        /// </summary>
        /// /// <value>
        /// The list of columns that is being displayed in the column chooser pop up. The default value is null.
        /// </value>
        public List<GridColumn>? Columns { get; internal set; }

        /// <summary>
        /// Gets the instance of the column chooser dialog.
        /// </summary>
        /// <value>
        /// A <see cref="SfDialog"/> representing the instance of the column chooser dialog.
        /// </value>
        public SfDialog? DialogInstance { get; internal set; }

        /// <summary>
        /// Gets or sets the search operator for the column chooser search request.
        /// </summary>
        /// <value>
        /// The string value representing the search operator. By default, the value is <see cref="Syncfusion.Blazor.Operator.StartsWith"/>.
        /// </value>
        public string? SearchOperator { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ColumnMenuItemClicked"/> event.
    /// </summary>
    public class ColumnMenuClickEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn"/> associated with the column menu pop up that is currently opened.
        /// </summary>
        /// <value>
        /// The current grid column associated with the column menu pop up that is currently opened.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the current <see cref="Microsoft.AspNetCore.Components.ElementReference"/> target.
        /// </summary>
        public ElementReference Element { get; internal set; }

        /// <summary>
        /// Gets the <see cref="System.EventArgs"/> details associated with the column menu pop up that is currently opened.
        /// </summary>
        /// <value>
        /// This property specifies the name of the event args details associated with the column menu pop up that is currently opened. The default value is <c>columnMenuItemclick</c>.
        /// </value>
        public System.EventArgs? Event { get; internal set; }

        /// <summary>
        /// Gets or sets the currently clicked <see cref="Navigations.MenuItemModel"/>.
        /// </summary>
        /// <value>
        /// The currently clicked menu item represented by the <see cref="Navigations.MenuItemModel"/> type.
        /// </value>
        public Navigations.MenuItemModel? Item { get; set; }
    }

    /// <summary>
    /// Class that defines column menu item model.
    /// </summary>
    public class ColumnMenuItemModel
    {

        /// <summary>
        /// Gets or sets a value indicating whether this menu item is hidden.
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// Defines class/multiple classes separated by a space for the menu Item that is used to include an icon.
        /// Menu Item can include font icon and sprite image.
        /// </summary>
        public string? IconCss { get; set; }

        /// <summary>
        /// Gets or sets the ID of the menu items.
        /// </summary>
        /// <value>
        /// The ID of the menu items as a string. The default value is an empty string.
        /// </value>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Specifies the sub menu items that is the array of MenuItem model.
        /// </summary>
        public List<MenuItem>? Items { get; set; }

        /// <summary>
        /// Specifies separator between the menu items. Separator are either horizontal or vertical lines used to group menu items.
        /// </summary>
        public bool Separator { get; set; }

        /// <summary>
        /// Specifies text for menu item.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Specifies URL for menu item that creates the anchor link to navigate to the URL provided.
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnColumnMenuOpen"/> event.
    /// </summary>
    public class ColumnMenuOpenEventArgs : GridEventBaseArgs
    {
        /// <summary> 
        /// Gets or sets a value indicating whether to prevent the column menu in the grid. 
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the <c>Cancel</c> property is set to <c>true</c>, then the column menu will not be shown.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn"/> where the column menu is currently open in the grid.
        /// </summary>
        /// <value>
        /// The grid column instance where the column menu is currently open.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Syncfusion.Blazor.Navigations.MenuItem"/> that are displayed in the column menu.
        /// </summary>
        /// <value>
        /// The list of menu items that are displayed in the column menu.
        /// </value>
        public List<MenuItem>? Items { get; set; }

        /// <summary>
        /// Gets or sets the left position of the column menu relative to the document or container.
        /// </summary>
        /// <value>
        /// The left position of the column menu relative to the document or container.
        /// </value>
        public double Left { get; set; }

        /// <summary>
        /// Gets the parent <see cref="Syncfusion.Blazor.Navigations.MenuItem"/> of the currently clicked sub menu item.
        /// </summary>
        /// <value>
        /// The parent menu item of the currently clicked sub menu item.
        /// </value>
        /// <remarks>
        /// If the currently clicked menu item is a sub menu item, then the <c>ParentItem</c> property will return the parent menu item, otherwise the value will be null.
        /// </remarks>
        public Navigations.MenuItem? ParentItem { get; internal set; }

        /// <summary>
        /// Gets or sets the top position of the column menu relative to the document or container.
        /// </summary>
        /// <value>
        /// The top position of the column menu relative to the document or container.
        /// </value>
        public double Top { get; set; }

        /// <summary>
        /// Gets the column menu index indicating the level of the menu item within the menu hierarchy.
        /// </summary>
        /// <value>
        /// An integer value that represents the column menu index of the menu item.
        /// </value>
        /// <remarks>
        /// The <c>ColumnMenuIndex</c> property indicates the level of submenu items. The navigation index for parent menu items starts from 0.
        /// This index for top-level menu items starts from 0, and it increases as the menu item becomes a sub-menu item.
        /// </remarks>
        public int ColumnMenuIndex { get; internal set; }
    }

    /// <summary>
    /// Defines options for command buttons.
    /// </summary>
    public class CommandButtonOptions
    {
        /// <summary>
        /// Defines the text content of the Button element.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Defines class/multiple classes separated by a space in the Button element.
        /// The Button types, styles, and size can be defined.
        /// </summary>
        public string CssClass { get; set; } = string.Empty;

        /// <summary>
        /// Specifies a value that indicates whether the Button is disabled or not.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Enable or disable rendering component in right to left direction.
        /// </summary>
        public bool EnableRtl { get; set; }

        /// <summary>
        /// Defines class/multiple classes separated by a space for the Button that is used to include an icon.
        /// Buttons can also include font icon and sprite image.
        /// </summary>
        public string IconCss { get; set; } = string.Empty;

        /// <summary>
        /// Positions the icon before/after the text content in the Button.
        /// The possible values are:
        ///  Left: The icon will be positioned to the left of the text content.
        ///  Right: The icon will be positioned to the right of the text content.
        /// </summary>
        public IconPosition IconPosition { get; set; }

        /// <summary>
        /// Allows the appearance of the Button to be enhanced and visually appealing when set to true.
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Makes the Button toggle, when set to true. When you click it, the state changes from normal to active.
        /// </summary>
        public bool IsToggle { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CommandClicked"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class CommandClickEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the CUD("Create", "Update", and "Delet") actions in grid when command column is clicked.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If the Cancel property is set to true, then the CUD actions will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the current <see cref="CommandModel"/> of the command column.
        /// </summary>
        /// <value>
        /// The <see cref="CommandModel"/> representing the current command column.
        /// </value>
        public CommandModel? CommandColumn { get; internal set; }

        /// <summary>
        /// Gets the row data of the current command column.
        /// </summary>
        /// <value>
        /// The row data of type T? associated with the current command column.
        /// </value>
        public T? RowData { get; internal set; }

        /// <summary>
        /// Gets the modified row data associated with the row being updated using a command column.
        /// </summary>
        /// <value>
        /// An object of type T? representing the modified data of the row being updated using a command column.
        /// </value>
        /// <remarks>
        /// This property returns null when the Command Column operation is either edited, deleted, or canceled.
        /// If the user modifies the data but decides to cancel the operation, the modified data will remain accessible through this property.
        /// </remarks>
        public T? EditedData { get; set; }        
    }

    /// <summary>
    /// Define options for command buttons.
    /// </summary>
    public class CommandModel
    {
        /// <summary>
        /// Define the button model.
        /// </summary>
        public CommandButtonOptions? ButtonOption { get; set; }

        /// <summary>
        /// Define the command Button tooltip.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Define the command Button type.
        /// </summary>
        public CommandButtonType Type { get; set; } = CommandButtonType.None;

        /// <summary>
        /// Define the command button ID.
        /// </summary>
        public string? ID { get; set; }

        /// <summary>
        ///  Defines the command button Uid.
        /// </summary>
        public string? Uid { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ContextMenuItemClicked"/> event.
    /// </summary>
    public class ContextMenuClickEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the current <see cref="Syncfusion.Blazor.Grids.GridColumn"/> where the context menu is opened in the grid.
        /// </summary>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the instance of the <see cref="SfContextMenu{TItem}"/> component used to display the context menu.
        /// </summary>
        /// <value>
        /// The <see cref="SfContextMenu{TItem}"/> instance used to display the context menu.
        /// </value>
        public ElementReference Element { get; internal set; }

        /// <summary>
        /// Gets the details about either a <see cref="Microsoft.AspNetCore.Components.Web.MouseEventArgs"/> or <see cref="Microsoft.AspNetCore.Components.Web.KeyboardEventArgs"/> event.
        /// </summary>
        /// <value>
        /// The event object. It can be either a mouse event or keyboard event.
        /// </value>
        public object? Event { get; internal set; }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.Blazor.Navigations.MenuItemModel"/> instance representing the details of the currently clicked context menu item.
        /// </summary>
        /// <value>
        /// The menu item model instance representing the currently clicked context menu item.
        /// </value>
        public MenuItemModel? Item { get; set; }

        /// <summary>
        /// Gets the Row information where the context menu is opened in grid. 
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description><b>Cell:</b> Cell Dom elements of the current cell.</description></item>
        /// <item><description><b>CellIndex:</b> The cell index value of the current cell.</description></item>
        /// <item><description><b>Column:</b> The current target column.</description></item>
        /// <item><description><b>Row:</b> The current traget Row details.</description></item>
        /// <item><description><b>RowData:</b> The current target row data.</description></item>
        /// <item><description><b>RowIndex:</b>The current target row index.</description></item>
        /// </list>
        /// </remarks>
        public RowInfo<T>? RowInfo { get; internal set; }

    }

    /// <summary>
    /// Defines the context menu item model.
    /// </summary>
    public class ContextMenuItemModel : ColumnMenuItemModel
    {
        /// <summary>
        /// Define the target to show the menu item.
        /// </summary>
        public string? Target { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ContextMenuOpen"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class ContextMenuOpenEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the instance of the <see cref="SfContextMenu{TItem}"/> component used to display the context menu.
        /// </summary>
        /// <value>
        /// The <see cref="SfContextMenu{TItem}"/> instance used to display the context menu.
        /// </value>
        public SfContextMenu<Navigations.MenuItem>? ContextMenu { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether to prevent the context menu from rendering in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the <c>Cancel</c> property is set to <c>true</c>, then the context menu will not be rendered.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn"/> where the context menu is currently open in the grid.
        /// </summary>
        /// <value>
        /// The grid column instance where the context menu is currently open.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Microsoft.AspNetCore.Components.ElementReference"/> of the current target element where the context menu is opened.
        /// </summary>
        public ElementReference Element { get; internal set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Syncfusion.Blazor.Navigations.MenuItemModel"/> that are displayed in the context menu.
        /// </summary>
        /// <value>
        /// The list of menu items that are displayed in the context menu.
        /// </value>
        public List<MenuItemModel>? Items { get; set; }

        /// <summary>
        /// Gets the left position of the context menu relative to the document or container.
        /// </summary>
        /// <value>
        /// The left position of the context menu relative to the document or container.
        /// </value>
        public double Left { get; set; }

        /// <summary>
        /// Gets or sets the parent <see cref="Syncfusion.Blazor.Navigations.MenuItemModel"/> of the currently clicked sub menu item.
        /// </summary>
        /// <value>
        /// The parent menu item of the currently clicked sub menu item. For items with no parent items, the value will be null.
        /// </value>
        public MenuItemModel? ParentItem { get; set; }

        /// <summary>
        /// Gets the <see cref="ContextMenuTarget"/> when clicking to open context menu.
        /// </summary>
        public ContextMenuTarget Target { get; internal set; }

        /// <summary>
        /// Gets information about the row that was right-clicked to open the context menu, including the row index, cell index, and row data.
        /// </summary>
        /// <value>
        /// Information about the row that was right-clicked to open the context menu, including the row index, cell index, and row data.
        /// </value>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description><b>Cell:</b> Cell Dom elements of the current cell.</description></item>
        /// <item><description><b>CellIndex:</b> The cell index value of the current cell.</description></item>
        /// <item><description><b>Column:</b> The current target column.</description></item>
        /// <item><description><b>Row:</b> The current traget Row details.</description></item>
        /// <item><description><b>RowData:</b> The current target row data.</description></item>
        /// <item><description><b>RowIndex:</b>The current target row index.</description></item>
        /// </list>
        /// </remarks>
        public RowInfo<T>? RowInfo { get; internal set; }

        /// <summary>
        /// Gets the top position of the context menu.
        /// </summary>
        /// <value>
        /// The top position of the context menu.
        /// </value>
        public double Top { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.DetailDataBound"/> event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DetailDataBoundEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the data of the currently selected row.
        /// </summary>
        /// <value>
        /// The data of the currently selected row as a type parameter of the class.
        /// </value>
        public T? Data { get; internal set; }

    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.DetailsExpanding"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    public class DetailsExpandingEventArgs<T> : DetailsExpandedEventArgs<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the expanding action.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If the Cancel property is set to <c>true</c>, then the expanding action. will be cancelled.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.DetailsExpanded"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    public class DetailsExpandedEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the expanded row data.
        /// </summary>
        /// <value>
        /// The expanded row data as a type parameter of the class.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets the row index of the expanded row.
        /// </summary>
        /// <value>
        /// The row index of the expanded row as an integer.
        /// </value>
        public int RowIndex { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.DetailsCollapsing"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    public class DetailsCollapsingEventArgs<T> : DetailsCollapsedEventArgs<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the collapsing action.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If the Cancel property is set to <c>true</c>, then the collapsing action. will be cancelled.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.DetailsCollapsed"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    public class DetailsCollapsedEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the collapsed row data.
        /// </summary>
        /// <value>
        /// The collapsed row data as a type parameter of the class.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets the index of the collapsed row.
        /// </summary>
        /// <value>
        /// The index of the collapsed row as an integer.
        /// </value>
        public int RowIndex { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.BeforeAutoFill"/> event.
    /// </summary>
    public class BeforeAutoFillEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the autofill action.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If the Cancel property is set to <c>true</c>, then the autofill action. will be cancelled.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.BeforeAutoFillCell"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    public class BeforeAutoFillCellEventArgs<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the autofill action for a particular cell. You can cancel and handle auto filling. 
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If the Cancel property is set to <c>true</c>, then the autofill action in a particular cell. will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>    
        /// Gets the row index of the cell associated with autofill action.
        /// </summary>
        /// <value>
        /// An integer representing the row index associated with the autofill action.
        /// </value>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets the column index of the cell associated with autofill action.
        /// </summary>
        /// <value>
        /// An integer representing the column index associated with the autofill action.
        /// </value>
        public int ColumnIndex { get; internal set; }

        /// <summary>
        /// Gets the column field name of the cell associated with autofill action.
        /// </summary>
        /// <value>
        /// A string representing the column field name associated with the autofill action.
        /// </value>
        public string? ColumnName { get; internal set; }

        /// <summary>
        /// Gets or sets the value getting filled in the cell. You can change value using <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.BeforeAutoFillCell"/> event.
        /// </summary>
        /// <value>
        /// Returns the cell value based on column value type. The default value is null. 
        /// </value>    
        public object? Value { get; set; }

        /// <summary>
        /// Gets the row data of the cell associated with autofill action.
        /// </summary>
        /// <value>
        /// Row data associated with autofill action.
        /// </value>
        public T? Data { get; internal set; }
    }

    /// <summary>
    /// Defines the cell of exported excel.
    /// </summary>
    public class ExcelCell
    {
        /// <summary>
        /// Defines the column span for the cell.
        /// </summary>
        public int ColSpan { get; set; }

        /// <summary>
        /// Defines the hyperlink of the cell.
        /// </summary>
        public Hyperlink? Hyperlink { get; set; }

        /// <summary>
        /// Defines the index for the cell.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Defines the row span for the cell.
        /// </summary>
        public int RowSpan { get; set; }

        /// <summary>
        /// Defines the style of the cell.
        /// </summary>
        public ExcelStyle? Style { get; set; }

        /// <summary>
        /// Defines the value of the cell.
        /// </summary>
        public object? Value { get; set; }
    }

    /// <summary>
    /// Defines the options for customizing the excel document during export.
    /// </summary>
    public class ExcelExportProperties
    {
        /// <summary>
        /// Defines the columns which are to be customized for Export alone.
        /// </summary>
        public List<GridColumn>? Columns { get; set; }

        /// <summary>
        /// Defines the data source dynamically before exporting.
        /// </summary>
        public IEnumerable<object>? DataSource { get; set; }

        /// <summary>
        /// Indicates to export current page or all page.
        /// </summary>
        public ExportType ExportType { get; set; }

        /// <summary>
        /// Gets or sets the mode for exporting detail rows to excel file format.
        /// </summary>
        /// <remarks>
        /// This property determines how detail rows are exported in Excel exporting.
        /// - When set to "Expand", detail rows are exported in their expanded state.
        /// - When set to “Collapse” details rows are exported in their collapsed state.
        /// - When set to "None", only parent rows are exported.
        /// The default mode is <c>Expand</c>.
        /// </remarks>
        public ExcelDetailRowMode ExcelDetailRowMode { get; set; }


        /// <summary>
        /// Defines the file name for the exported file.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Defines the footer content for exported document.
        /// </summary>
        public ExcelFooter? Footer { get; set; }

        /// <summary>
        /// Defines the header content for exported document.
        /// </summary>
        public ExcelHeader? Header { get; set; }

        /// <summary>
        /// Defines the hierarchy export mode for the pdf grid.
        /// </summary>
        public HierarchyExportMode HierarchyExportMode { get; set; }

        /// <summary>
        /// Indicates whether to show the hidden columns in exported excel.
        /// </summary>
        public bool IncludeHiddenColumn { get; set; }

        /// <summary>
        /// Indicates whether to show/hide the command columns in exported excel.
        /// </summary>
        public bool IncludeCommandColumn { get; set; }

        /// <summary>
        /// Indicates whether to show/hide the Template columns in exported excel.
        /// </summary>
        public bool IncludeTemplateColumn { get; set; }

        /// <summary>
        /// Defines the theme for exported data.
        /// </summary>
        public ExcelTheme? Theme { get; set; }

        /// <summary>
        /// Defines the additional workbook sheets for export.
        /// </summary>
        public Workbook? Workbook { get; set; }

        /// <summary>
        /// Defines the Grid sheet index. Based on index Grid sheet will append.
        /// </summary>
        public int GridSheetIndex { get; set; }

        /// <summary>
        /// Enable/disable the property to export the Grid column header row.
        /// </summary>
        public bool IncludeHeaderRow { get; set; } = true;

        /// <summary>
        /// Gets or sets the character encoding used for CSV export.
        /// </summary>
        /// <remarks>
        /// Specifies how characters are encoded when exporting data to CSV format.
        /// Use this property when exporting the Data Grid that contains non-ASCII characters,
        /// such as currency symbols (£, €, ¥), accented characters (é, ñ, ü),
        /// or right-to-left text (e.g., Arabic).
        /// <para>
        /// <strong>Note:</strong> This property is applicable only for CSV export operations. 
        /// For Excel export operations (.xlsx, .xls), this property is ignored as Excel handles     character encoding internally.
        /// </para>
        /// </remarks>
        /// <value>
        /// An <see cref="System.Text.Encoding"/> instance that defines the character encoding to use during CSV export.
        /// </value>
        public Encoding? Encoding { get; set; }
    }

    /// <summary>
    /// Defines the excel footer option class.
    /// </summary>
    public class ExcelFooter
    {
        /// <summary>
        /// Defines the number of rows between the grid data and footer.
        /// </summary>
        public int FooterRows { get; set; }

        /// <summary>
        /// Defines the rows in footer content.
        /// </summary>
        public List<ExcelRow>? Rows { get; set; }
    }

    /// <summary>
    /// Defines the excel header options.
    /// </summary>
    public class ExcelHeader
    {
        /// <summary>
        /// Defines the number of rows between the header and grid data.
        /// </summary>
        public int HeaderRows { get; set; }

        /// <summary>
        /// Defines the rows in header content.
        /// </summary>
        public List<ExcelRow>? Rows { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ExcelGroupCaptionTemplateInfo"/> event.
    /// </summary>
    public class ExcelCaptionTemplateArgs
    {
        /// <summary>
        /// Gets the <see cref="Syncfusion.ExcelExport.Cell"/> details of the grid cell.
        /// </summary>
        /// <value>
        /// The details of the grid cell as an instance of <see cref="Syncfusion.ExcelExport.Cell"/>.
        /// </value>
        public Cell? Cell { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn"/> object that represents the current cell's grouped column.
        /// </summary>
        /// <value>
        /// A gridcolumn object that represents the current cell's grouped column.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.ExcelExport.CellStyle"/> of the current cell.
        /// </summary>
        public CellStyle? Style { get; set; }

        /// <summary>
        /// Gets the value of the current grouped cell.
        /// </summary>
        /// <value>
        /// An object that represents the value of the current grouped cell.
        /// </value>
        public object? Value { get; internal set; }

        /// <summary>
        /// Gets the count of the child data items of the grouped record.
        /// </summary>
        /// /// <value>
        /// An integer value that represents the count of the child data items of the grouped record.
        /// </value>
        public int Count { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> name that is currently grouped.
        /// </summary>
        /// <value>
        /// A string that represents the name of the field that is currently grouped.
        /// </value>
        public string? Field { get; internal set; }

        /// <summary>
        /// Gets the foreign key value of the current grouped record.
        /// </summary>
        /// <value>
        /// A string that represents the foreign key value of the current grouped record.
        /// If the foreign key column is not grouped, the value of this property is null.
        /// </value>
        public string? ForeignKeyValue { get; internal set; }

        /// <summary>
        /// Gets the grouped key value of the current foreign key column.
        /// </summary>
        /// <value>
        /// A string that represents the grouped key value of the current foreign key column record.
        /// If the foreign key column is not grouped, the value of this property is null.
        /// </value>
        public string? ForeignKey { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn.HeaderText"/> grouped column.
        /// </summary>
        /// <value>
        /// A string that represents the header text of the current grouped column.
        /// </value>
        public string? HeaderText { get; internal set; }

        /// <summary>
        /// Gets the key value of the current grouped record.
        /// </summary>
        /// <value>
        /// A string that represents the key value of the current grouped record.
        /// </value>
        public string? Key { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ExcelAggregateTemplateInfo"/> event.
    /// </summary>
    public class ExcelAggregateEventArgs
    {
        /// <summary>
        /// Gets the <see cref="Syncfusion.ExcelExport.Cell"/> data.
        /// </summary>
        /// <value>
        /// A cell object that represents the cell data.
        /// </value>
        public Cell? Cell { get; set; }

        /// <summary>
        /// Gets the aggregate column of the current cell.
        /// </summary>
        /// <value>
        /// A <see cref="Syncfusion.Blazor.Grids.GridAggregateColumn"/> object that represents the aggregate column of the current cell.
        /// </value>
        public GridAggregateColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the cell style of the current aggregate cell.
        /// </summary>
        /// <value>
        /// A <see cref="Syncfusion.ExcelExport.CellStyle"/> object that represents the style of the current aggregate cell.
        /// </value>
        public CellStyle? Style { get; set; }

        /// <summary>
        /// Gets the value of the current aggregate cell.
        /// </summary>
        /// <value>
        /// An object that represents the value of the current aggregate cell.
        /// </value>
        public object? Value { get; set; }

        /// <summary>
        /// Gets the key value of the current grouped record, allowing customization of aggregate values for group caption and group footer template cells when exporting data to Excel.
        /// </summary>
        /// <value>
        /// A string that represents the key value of the current grouped record. By default, it is an empty string.
        /// </value>
        /// <remarks>
        /// The <see cref="GroupKey"/> contains a value only when rendering group caption and group footer template cells in Excel. Otherwise, it retains its default value, which is an empty string.
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the <see cref="GroupKey"/> to calculate a custom aggregate in the ExcelAggregateTemplateInfo event.
        /// <SfGrid>
        /// <GridEvents TValue="Orders" ExcelAggregateTemplateInfo="ExcelAggregateTemplateInfoHandler"> </GridEvents>
        /// ........
        /// </SfGrid>
        /// @code {
        /// public void ExcelAggregateTemplateInfoHandler(Syncfusion.Blazor.Grids.ExcelAggregateEventArgsargs)
        /// {
        /// args.Cell.Value = Orders
        ///     .Where(o => o.CustomerID == args.GroupKey)
        ///     .Sum(o => o.Freight);
        /// }
        /// }
        /// </example>
        public string? GroupKey { get; internal set; }

        /// <summary>
        /// Gets the type of aggregate cell being rendered during Excel export, enabling customization based on the specific aggregate type.
        /// </summary>
        /// <value>
        /// The aggregate template type, as defined in <see cref="AggregateTemplateType"/>.
        /// </value>
        /// <remarks>
        /// This event applies to all aggregate cells, including <see cref="AggregateTemplateType.GroupCaption"/>, 
        /// <see cref="AggregateTemplateType.GroupFooter"/>, and <see cref="AggregateTemplateType.Footer"/>. 
        /// This property enables differentiation of these types for customized rendering.
        /// </remarks>
        /// <example>
        /// The following example shows how to use the <see cref="AggregateType"/> property 
        /// to apply custom logic to specific aggregate types in the <c>ExcelAggregateTemplateInfo</c> event.
        /// <code>
        /// <SfGrid>
        ///     <GridEvents TValue="Orders" ExcelAggregateTemplateInfo="ExcelAggregateTemplateInfoHandler"></GridEvents>
        ///     ...
        /// </SfGrid>
        /// @code {
        ///     public void ExcelAggregateTemplateInfoHandler(Syncfusion.Blazor.Grids.ExcelAggregateEventArgs args)
        ///     {
        ///         if (args.AggregateType == AggregateTemplateType.Footer)
        ///         {
        ///             // Custom logic for footer template
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public AggregateTemplateType AggregateType { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ExcelQueryCellInfoEvent"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    public class ExcelQueryCellInfoEventArgs<T> : ExcelHeaderQueryCellInfoEventArgs
    {
        /// <summary>
        /// Gets the row data associated with the cell.
        /// </summary>
        /// <value>
        /// The data object associated with the current cell.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets the column span of the current cell.
        /// </summary>
        /// <value>
        /// An integer value that represents the number of columns spanned by the current cell.
        /// </value>
        public int ColSpan { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ExcelHeaderQueryCellInfoEvent"/> event.
    /// </summary>
    public class ExcelHeaderQueryCellInfoEventArgs
    {
        /// <summary>
        /// Gets or sets the cell details of the grid.
        /// </summary>
        /// <value>
        /// The cell object that represents the cell details, which contains the following properties:
        /// <list type="bullet">
        /// <item><description><b>CellStyle</b>: The <see cref="Syncfusion.ExcelExport.CellStyle"/> of the cell.</description></item>
        /// <item><description><b>Value</b>: The value of the cell.</description></item>
        /// <item><description><b>ColSpan</b>: The number of columns that the cell spans.</description></item>
        /// <item><description><b>Index</b>: The index of the cell.</description></item>
        /// </list>
        /// </value>
        public Cell? Cell { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn"/> object that represents the column of the current cell.
        /// </summary>
        /// <value>
        /// The grid column that represents the column of the current cell.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.ExcelExport.CellStyle"/> object that represents the style of the current cell.
        /// </summary>
        /// <value>
        /// The cell styles of the current cell.
        /// </value>
        public CellStyle? Style { get; set; }

        /// <summary>
        /// Gets the column span of the current cell.
        /// </summary>
        /// <value>
        /// An integer value that represents the number of columns spanned by the current cell.
        /// </value>
        public int Colspan { get; set; }

        /// <summary>
        /// Gets or sets the value of the current cell.
        /// </summary>
        /// <value>
        /// An object that represents the value of the current cell.
        /// </value>
        public object? Value { get; set; }

        /// <summary>
        /// Gets the row index of the current cell in the Excel Grid column.
        /// </summary>
        /// <value>
        /// An integer value that represents the row index of the current cell.
        /// </value>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets the column index of the current cell in the Excel Grid column.
        /// </summary>
        /// <value>
        /// An integer value that represents the column index of the current cell.
        /// </value>
        public int ColumnIndex { get; internal set; }
    }

    /// <summary>
    /// Provides event data for configuring the PDF detail template generated for a grid row during export.
    /// </summary>
    /// <typeparam name="T">The type of the data item bound to the parent row.</typeparam>
    /// <remarks>
    /// Use this event argument to access the parent row context and to specify the content and layout
    /// of the corresponding detail row in the exported PDF.
    /// </remarks>
    public class PdfDetailTemplateEventArgs<T>
    {
        /// <summary>
        /// Gets details about the parent row.
        /// </summary>
        /// <value>
        /// This property contains information about the parent row index, data, and its corresponding columns.
        /// </value>
        /// <remarks>
        /// Utilizing parent row details, you can customize the detail row.
        /// </remarks>
        public ParentRowInfo<T>? ParentRow { get; internal set; }

        /// <summary>
        /// Gets or sets the value for the detail template.
        /// </summary>
        /// <value>
        /// This property contains information about detail content, such as images, text, hyperlinks, or grids.
        /// </value>
        public PdfDetailTemplateRowSettings? RowInfo { get; set; }
    }

    /// <summary>
    /// Provides configuration for rendering a detail template row in PDF export.
    /// Includes column layout, optional header and content rows, and image support.
    /// Inherits hyperlink and text settings from <see cref="DetailTemplateSettings"/>.
    /// </summary>
    public class PdfDetailTemplateRowSettings : DetailTemplateSettings
    {
        /// <summary>
        /// Gets or sets the total number of columns in the detail rows.
        /// </summary>
        /// <value>
        /// The default value could be null.
        /// </value>
        /// <remarks>
        /// If the column count property is not specified, it will be determined based on the <c>Headers</c> and <c>Rows</c> of the first row's cell count. If the column count is less than the cell count, it will be considered as the row's cell count instead of the column count.
        /// </remarks>
        public int? ColumnCount { get; set; }


        /// <summary>
        /// Gets or sets the image details of the current cell.
        /// </summary>
        /// <value>
        /// An <see cref="Syncfusion.PdfExport.PdfImage "/> object represents image details such as ImageStream, width, and height etc.., You can customize and export image using this <c>Image</c> property.
        /// </value>
        public PdfImage? Image { get; set; }

        /// <summary>
        /// Gets or sets the PdfGrid header content of the detail row which includes stacked headers too.
        /// </summary>
        /// <value>
        /// A collection of <see cref=" Syncfusion.Blazor.Grids.PdfDetailTemplateRow"/> objects representing header content.
        /// </value>
        /// <remarks>
        /// This property is utilized to render the hierarchical grid structure, which includes headers.
        /// It can render multi-level stacked headers if the detail grid has them; otherwise, there is no need to define value for this property.
        /// </remarks>        
        public List<PdfDetailTemplateRow>? Headers { get; set; }

        /// <summary>
        /// Gets or sets the collection of PdfGrid content rows for the corresponding detail row.
        /// </summary>
        /// <value>
        /// A list of <c>PdfDetailTemplateRow</c> objects representing content for the detail row.
        /// </value>
        /// <remarks>
        /// This property is utilized to render the hierarchical grid structure; otherwise, there is no need to define value for this property.
        /// </remarks>   
        public List<PdfDetailTemplateRow>? Rows { get; set; }
    }

    /// <summary>
    /// Represents a header or content row used in the PDF detail template export.
    /// Contains a collection of cells and optional nested child row information for hierarchical layouts.
    /// Inherits row index metadata from <see cref="DetailTemplateRow"/>.
    /// </summary>
    public class PdfDetailTemplateRow : DetailTemplateRow
    {
        /// <summary>
        /// Represents a list of cells within the header or content rows of a detail row in the PDF document.
        /// </summary>
        /// <value>
        /// A list of <see cref="Syncfusion.Blazor.Grids.PdfDetailTemplateCell"/> objects representing cells in the header or content rows of detail rows in the PDF document.
        /// </value>
        /// <remarks>
        /// These cells can contain various types of values, including text, images, hyperlinks, etc.
        /// </remarks>
        public List<PdfDetailTemplateCell>? Cells { get; set; }

        /// <summary>
        /// Gets or sets the nested level information of detail row, when parent row has nested level hierarchical structure.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.Blazor.Grids.PdfDetailTemplateRowSettings"/> containing nested level values.    
        /// </value>
        /// <remarks>
        /// Use this property if a complex level hierarchy structure needs to be rendered; otherwise, it is not needed.
        /// </remarks>
        public PdfDetailTemplateRowSettings? ChildRowInfo { get; set; }
    }


    /// <summary>
    /// Represents a cell within a PDF detail template row.
    /// Supports styling and images, and inherits hyperlink, value, index, and column span settings from <see cref="DetailTemplateCell"/>.
    /// </summary>
    public class PdfDetailTemplateCell : DetailTemplateCell
    {
        /// <summary>
        /// Gets or sets the style of the PDF cell.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.Blazor.Grids.PdfThemeStyle"/> object associated with the theme style for the current cell. Use the <c>Style</c> property to customize the cell's appearance.
        /// </value>
        /// <remarks>
        /// By default, it applies the parent grid header style for <c>Headers</c> and content styles for <c>Rows</c>.
        /// </remarks>
        public PdfThemeStyle? Style { get; set; }

        /// <summary>
        /// Gets or sets the image details of the current cell.
        /// </summary>
        /// <value>
        /// An <see cref="Syncfusion.PdfExport.PdfImage "/> object represents image details such as ImageStream, width, and height etc.., You can customize and export image using this <c>Image</c> property.
        /// </value>
        public PdfImage? Image { get; set; }

    }

    /// <summary>
    /// Provides contextual information about the parent row when exporting detail templates to PDF.
    /// Includes the parent row data item, its row index, and the parent grid's columns.
    /// </summary>
    /// <typeparam name="T">Type of the parent row data item.</typeparam>
    public class ParentRowInfo<T>
    {
        /// <summary>
        /// Gets the row data associated with the parent row.
        /// </summary>
        /// <value>
        /// A <typeparamref name="T"/> instance representing the row data associated with the parent row. This data facilitates the establishment of a parent-child relationship.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets the row index of the parent row.
        /// </summary>
        /// <value>
        /// An index representing the row index of the parent row.
        /// </value>
        public int Index { get; internal set; }

        /// <summary>
        /// Gets the columns of the parent grid.
        /// </summary>
        /// <value>
        /// A list of <see cref="Syncfusion.Blazor.Grids.GridColumn"/> objects representing the columns of the parent grid.
        /// </value>
        public List<GridColumn>? Columns { get; internal set; }
    }

    /// <summary>
    /// Represents the base settings for a detail template cell used during export.
    /// </summary>
    public class DetailTemplateSettings
    {
        /// <summary>
        /// Gets or sets the hyperlink associated with the detail cell.
        /// </summary>
        /// <value>
        /// A <see cref=" Syncfusion.Blazor.Grids.Hyperlink "/> object representing the cell's hyperlink. During exporting the corresponding cell value is rendered with hyperlink.
        /// </value>
        public Hyperlink? Hyperlink { get; set; }

        /// <summary>
        /// Gets or sets the text content of the detail cell.
        /// </summary>
        /// <value>
        /// The text representing the content of the cell. Here you can specify the value you want to display on the detail cell while exporting.
        /// </value>
        public string? Text { get; set; }
    }

    /// <summary>
    /// Represents a row within a detail template during export.
    /// </summary>
    public class DetailTemplateRow
    {
        /// <summary>
        /// Gets or sets the index of the header or content row.
        /// </summary>
        /// <value>
        /// The default value is null. Represents the index of the row that will be exported in the detail row.
        /// </value>
        public int? Index { get; set; }
    }

    /// <summary>
    /// Describes a single cell within a detail template row for export.
    /// </summary>
    public class DetailTemplateCell
    {

        /// <summary>
        /// Gets or sets the hyperlink associated with the current cell.
        /// </summary>
        /// <value>
        /// A <see cref=" Syncfusion.Blazor.Grids.Hyperlink "/> object representing the cell's hyperlink. During exporting the corresponding cell value is rendered with hyperlink.
        /// </value>
        public Hyperlink? Hyperlink { get; set; }

        /// <summary>
        /// Gets or sets the text content of the current cell.
        /// </summary>
        /// <value>
        /// Represents content of the cell. You can specify values such as string, boolean, date, or any desired content to be displayed in the current cell.
        /// </value>
        public object? CellValue { get; set; }

        /// <summary>
        /// Gets or sets the index for the cell.
        /// </summary>
        /// <value>
        /// Represents the index of the current cell, determining its location in the row.        
        /// </value>
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets the column span for the cell.
        /// </summary>
        /// <value>
        /// The column span representing the number of columns spanned by the cell.
        /// </value>
        public int? ColumnSpan { get; set; }

        /// <summary>
        /// Gets or sets the row span for the cell.
        /// </summary>
        /// <value>
        /// The row span representing the number of rows spanned by the cell.
        /// </value>
        public int? RowSpan { get; set; }
    }

    /// <summary>
    /// Provides data for configuring the detail template content during Excel export.
    /// </summary>
    public class ExcelDetailTemplateEventArgs<T>
    {
        /// <summary>
        /// Gets details about the parent row.
        /// </summary>
        /// <value>
        /// This property contains information about the row index, data, and its corresponding columns
        /// </value>
        /// <remarks>
        /// Utilizing parent row details, you can customize the detail row.
        /// </remarks>
        public ParentRowInfo<T>? ParentRow { get; internal set; }


        /// <summary>
        /// Gets or sets the values for the detail template.
        /// </summary>
        /// <value>
        /// This property contains information about detail content, such as images, text, hyperlinks, or grids.
        /// </value>
        public ExcelDetailTemplateRowSettings? RowInfo { get; set; }
    }

    /// <summary>
    /// Defines settings used to compose a single Excel detail row during export, including optional image content,
    /// header rows (with stacked headers), and data rows for hierarchical layouts.
    /// </summary>
    public class ExcelDetailTemplateRowSettings : DetailTemplateSettings
    {

        /// <summary>
        /// Gets or sets the image details of the detail cell.
        /// </summary>
        /// <value>
        /// An <see cref="Syncfusion.ExcelExport.Image"/> object represents the image details such as image string, width, and height. You can customize and export image in the corresponding cell.
        /// </value>
        public Image? Image { get; set; }


        /// <summary>
        /// Gets or sets the Excel header content of the detail row which includes stacked headers too.
        /// </summary>
        /// <value>
        /// A collection of <see cref=" Syncfusion.Blazor.Grids.ExcelDetailTemplateRow"/> objects representing the header content.
        /// </value>
        /// <remarks>
        /// This property is utilized to render the hierarchical grid structure, which includes headers.
        /// It can render multi-level stacked headers  if the detail grid has them; otherwise, there is no need to define value for this property.
        /// </remarks>               
        public List<ExcelDetailTemplateRow>? Headers { get; set; }


        /// <summary>
        /// Gets or sets the collection of Excel content rows for the corresponding detail row.
        /// </summary>
        /// <value>
        /// A list of <see cref=" Syncfusion.Blazor.Grids.ExcelDetailTemplateRow"/> objects representing the content for the detail row.
        /// </value>
        /// <remarks>
        /// This property is utilized to render the hierarchical grid structure; otherwise, there is no need to define value for this property.
        /// </remarks>
        public List<ExcelDetailTemplateRow>? Rows { get; set; }

    }

    /// <summary>
    /// Represents a header or content row within the Excel detail template. 
    /// A row is made up of one or more cells, and can optionally participate in Excel grouping or contain nested child row information.
    /// </summary>
    public class ExcelDetailTemplateRow : DetailTemplateRow
    {
        /// <summary>
        /// Represents a list of cells within the header or content rows of a detail row in the Excel document.
        ///</summary>
        /// <value>
        /// A list of <see cref=" Syncfusion.Blazor.Grids.ExcelDetailTemplateCell"/> objects representing cells in the header or content rows of detail rows in the Excel document.
        /// </value>
        /// <remarks>
        /// These cells can contain various types of values, including text, images, hyperlinks, etc.
        /// </remarks>
        public List<ExcelDetailTemplateCell>? Cells { get; set; }

        /// <summary>
        /// Gets or sets the group of rows to expand and collapse.
        /// </summary>
        /// <value>
        /// A grouping representing groups of detail rows to be expanded or collapsed. You can customize the grouping options for content rows of detail rows in the Excel document.
        /// </value>
        public Grouping? Grouping { get; set; }

        /// <summary>
        /// Gets or sets the nested level information of detail row, when parent row has nested level hierarchical structure.
        /// </summary>
        /// <value>
        /// An instance of <see cref=" Syncfusion.Blazor.Grids.ExcelDetailTemplateRowSettings"/> containing nested level values.        
        /// </value>
        /// <remarks>
        /// Use this property if a complex level hierarchy structure needs to be rendered; otherwise, it is not needed.
        /// </remarks>
        public ExcelDetailTemplateRowSettings? ChildRowInfo { get; set; }

    }

    /// <summary>
    /// Represents an individual cell in an Excel detail template row. 
    /// Supports styling and rich content such as text, images, and hyperlinks when exporting hierarchical detail rows to Excel.
    /// </summary>
    public class ExcelDetailTemplateCell : DetailTemplateCell
    {
        /// <summary>
        /// Gets or sets the style of the Excel row cell.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.Blazor.Grids.ExcelStyle "/> object associated with the theme style for the current cell. Use the <c>Style</c> property to customize the cell's appearance.
        /// </value>
        /// <remarks>
        /// By default, it applies the header style for <c>Headers</c> and content styles for <c>Rows</c>.
        /// </remarks>
        public ExcelStyle? Style { get; set; }

        /// <summary>
        /// Gets or sets the image details of the detail cell.
        /// </summary>
        /// <value>
        /// An <see cref="Syncfusion.ExcelExport.Image"/> object represents the image details such as image string, width, and height. You can customize and export image in the corresponding cell.
        /// </value>
        public Image? Image { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.PdfGroupCaptionTemplateInfo"/> event.
    /// </summary>
    public class PdfCaptionTemplateArgs
    {
        /// <summary>
        /// Gets the <see cref="Syncfusion.PdfExport.PdfGridCell"/> object that represents the current cell.
        /// </summary>
        /// <value>
        /// A <c>PdfGridCell</c> object that represents the current cell.
        /// </value>
        public PdfGridCell? Cell { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn"/> object that represents the current cell's grouped column.
        /// </summary>
        /// <value>
        /// A gridcolumn object that represents the current cell's grouped column.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.PdfExport.PdfGridCellStyle"/> object that represents the style of the current cell.
        /// </summary>
        /// <value>
        /// A <c>PdfGridCellStyle</c> object that represents the style of the current cell.
        /// </value>
        public PdfGridCellStyle? Style { get; set; }

        /// <summary>
        /// Gets the value of the current grouped cell.
        /// </summary>
        /// <value>
        /// An object that represents the value of the current grouped cell.
        /// </value>
        public object? Value { get; set; }

        /// <summary>
        /// Gets the count of the child data items of the grouped record.
        /// </summary>
        /// /// <value>
        /// An integer value that represents the count of the child data items of the grouped record.
        /// </value>
        public int Count { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> name that is currently grouped.
        /// </summary>
        /// <value>
        /// A string that represents the name of the field that is currently grouped.
        /// </value>
        public string? Field { get; internal set; }

        /// <summary>
        /// Gets the foreign key value of the current grouped record.
        /// </summary>
        /// <value>
        /// A string that represents the foreign key value of the current grouped record.
        /// If the foreign key column is not grouped, the value of this property is null.
        /// </value>
        public string? ForeignKeyValue { get; internal set; }

        /// <summary>
        /// Gets the grouped key value of the current foreign key column.
        /// </summary>
        /// <value>
        /// A string that represents the grouped key value of the current foreign key column record.
        /// If the foreign key column is not grouped, the value of this property is null.
        /// </value>
        public string? ForeignKey { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn.HeaderText"/> grouped column.
        /// </summary>
        /// <value>
        /// A string that represents the header text of the current grouped column.
        /// </value>
        public string? HeaderText { get; internal set; }

        /// <summary>
        /// Gets the key value of the current grouped record.
        /// </summary>
        /// <value>
        /// A string that represents the key value of the current grouped record.
        /// </value>
        public string? Key { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.PdfAggregateTemplateInfo"/> event.
    /// </summary>
    public class PdfAggregateEventArgs
    {
        /// <summary>
        /// Gets the <see cref="Syncfusion.PdfExport.PdfGridCell"/> object that represents the current cell.
        /// </summary>
        /// <value>
        /// A <c>PdfGridCell</c> object that represents the current cell.
        /// </value>
        public PdfGridCell? Cell { get; set; }

        /// <summary>
        /// Gets the aggregate column of the current cell.
        /// </summary>
        /// <value>
        /// A <see cref="Syncfusion.Blazor.Grids.GridAggregateColumn"/> object that represents the aggregate column of the current cell.
        /// </value>
        public GridAggregateColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.PdfExport.PdfGridCellStyle"/> object that represents the style of the current cell.
        /// </summary>
        /// <value>
        /// A <c>PdfGridCellStyle</c> object that represents the style of the current cell.
        /// </value>
        public PdfGridCellStyle? Style { get; set; }

        /// <summary>
        /// Gets the value of the current aggregate cell.
        /// </summary>
        /// <value>
        /// An object that represents the value of the current aggregate cell.
        /// </value>
        public object? Value { get; set; }

        /// <summary>
        /// Gets the key value of the current grouped record, allowing customization of aggregate values for group caption and group footer template cells when exporting data to Pdf.
        /// </summary>
        /// <value>
        /// A string that represents the key value of the current grouped record. By default, it is an empty string.
        /// </value>
        /// <remarks>
        /// The <see cref="GroupKey"/> contains a value only when rendering group caption and group footer template cells in Pdf. Otherwise, it retains its default value, which is an empty string.
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the <see cref="GroupKey"/> to calculate a custom aggregate in the PdfAggregateTemplateInfo event.
        /// <SfGrid>
        /// <GridEvents TValue="Orders" PdfAggregateTemplateInfo="PdfAggregateTemplateInfoHandler"> </GridEvents>
        /// ........
        /// </SfGrid>
        /// @code {
        /// public void PdfAggregateTemplateInfoHandler(Syncfusion.Blazor.Grids.PdfAggregateEventArgsargs)
        /// {
        /// args.Cell.Value = Orders
        ///     .Where(o => o.CustomerID == args.GroupKey)
        ///     .Sum(o => o.Freight);
        /// }
        /// }
        /// </example>
        public string? GroupKey { get; internal set; }

        /// <summary>
        /// Gets the type of aggregate cell being rendered during Pdf export, enabling customization based on the specific aggregate type.
        /// </summary>
        /// <value>
        /// The aggregate template type, as defined in <see cref="AggregateTemplateType"/>.
        /// </value>
        /// <remarks>
        /// This event applies to all aggregate cells, including <see cref="AggregateTemplateType.GroupCaption"/>, 
        /// <see cref="AggregateTemplateType.GroupFooter"/>, and <see cref="AggregateTemplateType.Footer"/>. 
        /// This property enables differentiation of these types for customized rendering.
        /// </remarks>
        /// <example>
        /// The following example shows how to use the <see cref="AggregateType"/> property 
        /// to apply custom logic to specific aggregate types in the <c>PdfAggregateTemplateInfo</c> event.
        /// <code>
        /// <SfGrid>
        ///     <GridEvents TValue="Orders" PdfAggregateTemplateInfo="PdfAggregateTemplateInfoHandler"></GridEvents>
        ///     ...
        /// </SfGrid>
        /// @code {
        ///     public void PdfAggregateTemplateInfoHandler(Syncfusion.Blazor.Grids.PdfAggregateEventArgs args)
        ///     {
        ///         if (args.AggregateType == AggregateTemplateType.Footer)
        ///         {
        ///             // Custom logic for footer template
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public AggregateTemplateType AggregateType { get; internal set; }
    }

    /// <summary>
    /// Defines excel export row of grid.
    /// </summary>
    public class ExcelRow
    {
        /// <summary>
        /// Defines the cells in a row.
        /// </summary>
        public List<ExcelCell>? Cells { get; set; }

        // public object? Grouping { get; set; }

        /// <summary>
        /// Defines the index for cells.
        /// </summary>
        public int Index { get; set; }
    }

    /// <summary>
    /// Defines option for styling excel cell/row.
    /// </summary>
    public class ExcelStyle
    {
        /// <summary>
        /// Defines the background color for cell style.
        /// </summary>
        public string? BackColor { get; set; }

        /// <summary>
        /// Defines the bold style for fonts.
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Defines the borders for cell style.
        /// </summary>
        public Border? Borders { get; set; }

        /// <summary>
        /// Defines the color of font.
        /// </summary>
        public string? FontColor { get; set; }

        /// <summary>
        /// Defines the name of font.
        /// </summary>
        public string? FontName { get; set; }

        /// <summary>
        /// Defines the size of font.
        /// </summary>
        public double FontSize { get; set; }

        /// <summary>
        /// Defines the horizontal alignment for cell style.
        /// </summary>
        public ExcelHorizontalAlign HAlign { get; set; }

        /// <summary>
        /// Defines the indent for cell style.
        /// </summary>
        public int Indent { get; set; }

        /// <summary>
        /// Defines the italic style for fonts.
        /// </summary>
        public bool Italic { get; set; }

        /// <summary>
        /// Defines the format of the cell.
        /// </summary>
        public string? NumberFormat { get; set; }

        /// <summary>
        /// Defines the type of the cell.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Defines the underline style for fonts.
        /// </summary>
        public bool Underline { get; set; }

        /// <summary>
        /// Defines the vertical alignment for cell style.
        /// </summary>
        public ExcelVerticalAlign VAlign { get; set; }

        /// <summary>
        /// Defines the wrapText for cell style.
        /// </summary>
        public bool WrapText { get; set; }
    }

    /// <summary>
    /// Defines options for customizing theme during excel export.
    /// </summary>
    public class ExcelTheme
    {
        /// <summary>
        /// Defines the theme style of caption content.
        /// </summary>
        public ExcelStyle? Caption { get; set; }

        /// <summary>
        /// Defines the style of header content.
        /// </summary>
        public ExcelStyle? Header { get; set; }

        /// <summary>
        /// Defines the theme style of record content.
        /// </summary>
        public ExcelStyle? Record { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnActionFailure"/> event.
    /// </summary>
    public class FailureEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the error information associated with an action.
        /// </summary>
        /// <value>
        /// An Exception object that provides details about the error that occurred during an action.
        /// </value>
        /// <remarks>
        /// The Error property is typically used in error-handling scenarios to retrieve information about an error that occurred during the execution of an operation or task.
        /// When an exception occurs, the .NET runtime automatically creates an Exception object to encapsulate information about the error, such as the error message, 
        /// the type of exception, and the stack trace. 
        /// You can use the Error property to retrieve this Exception object and access its properties and methods to obtain more detailed information about the error.
        /// </remarks>
        public Exception? Error { get; internal set; }
    }

    /// <summary>
    /// Defines the filter UI option that can be used to get filter menu details.
    /// </summary>
    public class FilterUI
    {
        /// <summary>
        /// Defines the field.
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Defines the first operator for excel filter.
        /// </summary>
        public string? FirstOperator { get; set; }

        /// <summary>
        /// Defines the Operator.
        /// </summary>
        public string? Operator { get; set; }

        /// <summary>
        /// Defines the second Operator for excel filter.
        /// </summary>
        public string? SecondOperator { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.HeaderCellInfo"/> event.
    /// </summary>
    public class HeaderCellInfoEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets the CellDOM object associated with the header cell.
        /// </summary>
        /// <value>
        /// The CellDOM object that represents the header cell.
        /// </value>
        /// <remarks>
        /// The CellDom properties include:
        /// <list type="bullet">
        /// <item><description>
        /// <b>HasChanges:</b> A boolean property that indicates whether the Cell object associated with the header cell has changed.
        /// </description></item>
        /// <item><description>
        /// <b>ClassList:</b> A property that contains the class list of the header cell.
        /// </description></item>
        /// <item><description>
        /// <b>Styles:</b> A property that contains the styles of the header cell.
        /// </description></item>
        /// <item><description>
        /// <b>AttributeList:</b> A property that contains the attribute list of the header cell.
        /// </description></item>
        /// <item><description>
        /// <b>AddClass:</b> A method to add class names to the class list for the current header cell.
        /// </description></item>
        /// <item><description>
        /// <b>AddStyle:</b> A method to add styles for the current header cell.
        /// </description></item>
        /// <item><description>
        /// <b>SetAttribute:</b> A method to set an attribute for the current header cell.
        /// </description></item>
        /// </list>
        /// </remarks>
        public CellDOM? Cell { get; internal set; }

        /// <summary>
        /// Gets the corresponding column associated with the header cell.
        /// </summary>
        /// <value>
        /// A <see cref="GridColumn"/> associated with the header cell.
        /// </value>
        public GridColumn? Column { get; internal set; }
    }

    /// <summary>
    /// Defines hyper link options for exporting.
    /// </summary>
    public class Hyperlink
    {
        /// <summary>
        /// Defines the display text for hyperlink.
        /// </summary>
        public string? DisplayText { get; set; }

        /// <summary>
        /// Defines the Url for hyperlink.
        /// </summary>
        public string? Target { get; set; }
    }

    /// <summary>
    /// Defines pdf cell border options.
    /// </summary>
    public class PdfBorder
    {
        /// <summary>
        /// Defines the border color.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Defines the border dash style.
        /// </summary>
        public PdfDashStyle DashStyle { get; set; }

        /// <summary>
        /// Defines the line style of border.
        /// </summary>
        public BorderLineStyle LineStyle { get; set; }

        /// <summary>
        /// Defines the border width.
        /// </summary>
        public double Width { get; set; }
    }

    /// <summary>  
    /// Represents class that provides style configuration of text content in the exported Pdf. 
    /// </summary>  
    public class PdfTextStyle 
    {
        /// <summary> 
        /// Gets or sets the font of the text content in the exported PDF. 
        /// </summary> 
        /// <value> 
        /// An instance of <see cref="PdfGridFont"/>. 
        /// </value> 
        /// <remarks> 
        /// Represents the font configuration of the text content in the exported PDF using an <see cref="PdfGridFont"/> instance. 
        /// The <see cref="PdfGridFont"/> class allows customization of font styles, sizes, colors, and other properties for text rendering in the PDF. 
        /// Use this property to apply specific font settings to the text content when exporting to PDF. 
        /// </remarks> 
        public PdfGridFont? Font { get; set; }

        /// <summary> 
        /// Gets or sets the background color for text in the exported PDF. 
        /// </summary> 
        /// <value> 
        /// A string representing the background color of the text in the PDF export, specified by color name, RGB code, or hexadecimal code. 
        /// </value> 
        /// <remarks> 
        /// This property determines the background color of the text in the PDF export. 
        /// Use color names (e.g., "Red"), RGB codes (e.g., "255,0,0"), or hexadecimal codes (e.g., "#FF0000") to specify the background color.
        /// </remarks> 
        public string? FillBackgroundColor { get; set; }

        /// <summary> 
        /// Gets or sets the text formatting and alignment in the exported PDF, specifically for vertical and horizontal alignment. 
        /// </summary> 
        /// <value> 
        /// An instance of <see cref="PdfStringFormat"/>. 
        /// </value> 
        /// <remarks>  
        /// Represents the text formatting and alignment settings for the exported PDF using an <see cref="PdfStringFormat"/> instance.  
        /// The <see cref="PdfStringFormat"/> class allows customization of vertical and horizontal text alignment, character spacing, line spacing, text direction, and more. 
        /// Use this property to apply specific alignment settings to text content when exporting to Pdf.  
        /// </remarks> 
        public PdfStringFormat? StringFormat { get; set; }

    }

    /// <summary>  
    /// Represents a class that provides style configuration for elements in a PDF document.  
    /// </summary> 
    public class PdfElementStyle : PdfTextStyle 
    {
        /// <summary> 
        /// Gets or sets the border style configuration of the header or footer in the exported PDF. 
        /// </summary> 
        /// <value> 
        /// An instance of <see cref="PdfBorder"/>. 
        /// </value> 
        public PdfBorder? Border { get; set; }

        /// <summary> 
        /// Gets or sets the image to be included in the exported PDF. 
        /// </summary> 
        /// <value> 
        /// An instance of <see cref="PdfImage"/>. 
        /// </value> 
        public PdfImage? Image { get; set; }

        /// <summary> 
        /// Gets or sets the position of the image drawn in the exported PDF. 
        /// </summary> 
        /// <value> 
        /// An instance of <see cref="PdfPosition"/> representing the horizontal (X) and vertical (Y) alignment of the image within the exported Pdf.
        /// The coordinates are defined as follows: 
        /// <c>X</c> and <c>Y</c> specify the starting coordinates for the image draw in exported Pdf. 
        /// </value> 
        public PdfPosition? ImagePosition { get; set; }

        /// <summary> 
        /// Gets or sets the cell padding of the content in the exported PDF. 
        /// </summary> 
        /// <value> 
        /// An instance of <see cref="PdfPaddings"/>. 
        /// </value> 
        public PdfPaddings? Padding { get; set; }

    }

    /// <summary>
    /// Defines pdf cell style options.
    /// </summary>
    public class PdfContentStyle : PdfElementStyle
    {
        /// <summary>
        /// Defines the dash style.
        /// </summary>
        public PdfDashStyle DashStyle { get; set; }

        /// <summary>
        /// Defines the font size.
        /// </summary>
        public double FontSize { get; set; }

        /// <summary>
        /// Defines the horizontal alignment.
        /// </summary>
        public PdfHorizontalAlign? HAlign { get; set; }

        /// <summary>
        /// Defines the pen color.
        /// </summary>
        public string? PenColor { get; set; }

        /// <summary>
        /// Defines the pen size.
        /// </summary>
        public double PenSize { get; set; }

        /// <summary>
        /// Defines the text brush color.
        /// </summary>
        public string? TextBrushColor { get; set; }

        /// <summary>
        /// Defines the text pen color.
        /// </summary>
        public string? TextPenColor { get; set; }

        /// <summary>
        /// Defines the vertical alignment.
        /// </summary>
        public PdfVerticalAlign VAlign { get; set; }
    }

    /// <summary> 
    /// Represents the PDF export customization options for the PDF document. 
    /// </summary> 
    public class PdfExportPropertiesBase 
    {
        /// <summary>  
        /// Gets or sets the file name of the exported Pdf.  
        /// </summary>  
        /// <value>  
        /// A string representing the file name of the exported Pdf. The default file name is <c>Export.pdf</c> 
        /// </value>  
        public string? FileName { get; set; } 

        /// <summary> 
        /// Gets or sets the instance of <see cref="PdfFooter"/> which has footer configuration for the Pdf export. 
        /// </summary> 
        /// <value> 
        /// An instance of <see cref="PdfFooter"/>. 
        /// </value> 
        /// <remarks> 
        /// <see cref="PdfFooter"/> class provides the footer configuration, like footer height, position and other configuration 
        /// </remarks> 
        public PdfFooter? Footer { get; set; } 

        /// <summary> 
        /// Gets or sets the header configuration for the Pdf export. 
        /// </summary> 
        /// <value> 
        /// An instance of <see cref="PdfHeader"/>. 
        /// </value> 
        /// <remarks> 
        /// <see cref="PdfHeader"/> class provides the header configuration, like header height, position and other configuration 
        /// </remarks> 
        public PdfHeader? Header { get; set; }

        /// <summary> 
        /// Gets or sets whether to show the hidden columns in exported Pdf. 
        /// </summary> 
        /// <value> 
           /// <c>true</c> to include hidden columns in the Pdf export; otherwise, <c>false</c>. 
           /// </value> 
          /// <remarks> 
           /// Setting this property to <c>true</c> will include hidden columns in the generated Pdf document. 
           /// </remarks> 
        public bool IncludeHiddenColumn { get; set; }

        /// <summary> 
        /// Gets or sets the page orientation of the exported Pdf. 
        /// </summary> 
        /// <value> 
        /// One of the <see cref=" PageOrientation"/> enumeration that represents the page orientation of the exported Pdf.  
        /// The default mode is <b>PageOrientation.Portrait</b> 
        /// </value> 
        /// <remarks> 
        /// The page orientation determines whether the pages in the Pdf will be in portrait or landscape mode. 
        /// </remarks> 
        public PageOrientation PageOrientation { get; set; }

        /// <summary> 
        /// Gets or sets the page size of the exported Pdf. 
        /// </summary> 
        /// <value> 
        /// One of the <see cref="PdfPageSize"/> enumeration that represents the page size of the exported Pdf. 
        /// The default page size is <b>PdfPageSize.Letter</b> 
        /// </value> 
        /// <remarks> 
        /// The page size specifies the dimensions of the pages in the Pdf document. 
        /// </remarks> 
        public PdfPageSize PageSize { get; set; }

        /// <summary> 
        /// Gets or sets whether to show the template columns in exported Pdf. 
        /// </summary> 
        /// <value> 
        /// <c>true</c> to include template columns in the Pdf export; otherwise, <c>false</c>. 
        /// </value> 
        /// <remarks> 
        /// Setting this property to <c>true</c> will include template columns in the generated Pdf document. 
        /// </remarks> 
        public bool IncludeTemplateColumn { get; set; }

    }

    /// <summary>
    /// Defines pdf export customization options.
    /// </summary>
    public class PdfExportProperties : PdfExportPropertiesBase
    {
        /// <summary>
        /// Defines the overflow of columns for the pdf grid.
        /// </summary>
        public bool AllowHorizontalOverflow { get; set; }

        /// <summary>
        /// Defines the columns which are to be customized for Export alone.
        /// </summary>
        public List<GridColumn>? Columns { get; set; }

        /// <summary>
        /// Defines the data source dynamically before exporting.
        /// </summary>
        public IEnumerable<object>? DataSource { get; set; }

        /// <summary>
        /// Indicates to export current page or all page.
        /// </summary>
        public ExportType ExportType { get; set; }

        /// <summary>
        /// Gets or sets the mode for exporting detail rows to the PDF file format.
        /// </summary>
        /// <remarks>
        /// This property determines how detail rows are exported in PDF format:
        /// - When set to "Expand", detail rows are exported in their expanded state.
        /// - When set to "None", only parent rows are exported.
        /// The default mode is <c>Expand</c>.
        /// </remarks>
        public PdfDetailRowMode PdfDetailRowMode { get; set; }

        /// <summary>
        /// Enable/disable the PDF header repeats every page.
        /// </summary>
        public bool IsRepeatHeader { get; set; }

        /// <summary>
        /// Indicates whether to show/hide the command columns in exported excel.
        /// </summary>
        public bool IncludeCommandColumn { get; set; }

        /// <summary>
        /// Enable/Disable the PDF style. If increasing the performance while using large records then disable this property.
        /// </summary>
        public bool IsThemeEnabled { get; set; } = true;

        /// <summary>
        /// Defines the hierarchy export mode for the pdf grid.
        /// </summary>
        public HierarchyExportMode HierarchyExportMode { get; set; }

        /// <summary>
        /// Defines the theme for exported data.
        /// </summary>
        public PdfTheme? Theme { get; set; }

        /// <summary>
        /// Defines the Grid's Column width to Pdf Column width. This can be also enables AllowHorizontalOverflow API internally.
        /// </summary>
        public bool DisableAutoFitWidth { get; set; }

        /// <summary>
        /// Enable/disable the property to export the Grid column header row.
        /// </summary>
        public bool IncludeHeaderRow { get; set; } = true;

        /// <summary>
        /// Gets or sets the value which is going to apply/customize the PDF Grid cell graphics. This event is raised when laying out a cell on a page.
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" Toolbar="@(new List<string>() { "PdfExport" })" AllowPdfExport="true" >
        ///  <GridEvents OnToolbarClick="ToolbarClickHandler" TValue="Order"></GridEvents>
        ///   ........
        /// </SfGrid>
        /// @code{
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs args)
        ///  {
        ///    if (args.Item.Id == "Grid_pdfexport")  //Id is combination of Grid's ID and itemname
        ///    {
        ///        PdfExportProperties ExportProperties = new PdfExportProperties();
        ///        ExportProperties.BeginCellLayout = new PdfGridBeginCellLayoutEventHandler(BeginCellEvent);
        ///        await this.DefaultGrid.PdfExport(ExportProperties);
        ///    }
        ///  } 
        ///  private void BeginCellEvent(object sender, PdfGridBeginCellLayoutEventArgs args)
        ///  {
        ///    ........
        ///  }
        /// ]]>
        /// </code>
        /// </example>
        /// </summary>
        public PdfGridBeginCellLayoutEventHandler? BeginCellLayout { get; set; }

        /// <summary>
        /// Gets or sets the value which is going to apply/customize the PDF Grid cell graphics. This event is raised when you have finished laying out a page.
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" Toolbar="@(new List<string>() { "PdfExport" })" AllowPdfExport="true" >
        ///  <GridEvents OnToolbarClick="ToolbarClickHandler" TValue="Order"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs args)
        ///  {
        ///    if (args.Item.Id == "Grid_pdfexport")  //Id is combination of Grid's ID and itemname
        ///    {
        ///        PdfExportProperties ExportProperties = new PdfExportProperties();
        ///        ExportProperties.EndCellLayout = new PdfGridEndCellLayoutEventHandler(EndCellEvent);
        ///        await this.DefaultGrid.PdfExport(ExportProperties);
        ///    }
        ///  } 
        ///  private void EndCellEvent(object sender, PdfGridEndCellLayoutEventArgs args)
        ///  {
        ///    ........
        ///  }
        /// ]]>
        /// </code>
        /// </example>
        /// </summary>
        public PdfGridEndCellLayoutEventHandler? EndCellLayout { get; set; }
    }

    /// <summary>
    /// Defines pdf footer options.
    /// </summary>
    public class PdfFooter
    {
        /// <summary>
        /// Defines the footer contents.
        /// </summary>
        public List<PdfHeaderFooterContent>? Contents { get; set; }

        /// <summary>
        /// Defines the footer content distance from bottom.
        /// </summary>
        public double FromBottom { get; set; }

        /// <summary>
        /// Defines the height of footer content.
        /// </summary>
        public double Height { get; set; }

        /// <summary> 
        /// Gets or sets the distance from the left side of the page to the footer in the exported PDF. 
        /// </summary> 
        /// <value> 
        /// A float value representing the distance of the footer content from the left side of the page. 
        /// </value> 
        /// <remarks> 
        /// This property specifies the horizontal position of the footer content relative to the left side of the page. 
        /// </remarks> 
        public double Left { get; set; }
    }

    /// <summary>
    /// Defines pdf grid font options.
    /// </summary>
    public class PdfGridFont
    {
        /// <summary>
        /// Defines the font family of font content. Value can be either PdfStandardFont or TrueTypeFont.
        /// </summary>
        public object? FontFamily { get; set; }

        /// <summary>
        /// Defines the fontSize of font content.
        /// </summary>
        public double FontSize { get; set; }

        /// <summary>
        /// Defines the fontStyle of font content.
        /// </summary>
        public PdfFontStyle? FontStyle { get; set; }

        /// <summary>
        /// Defines the trueTypeFont is enabled or not for font content.
        /// </summary>
        public bool IsTrueType { get; set; }

        /// <summary> 
        /// Gets or sets the color of the text in the exported PDF. 
        /// </summary> 
        /// <value> 
        /// A string representing the color name, RGB code, or hexadecimal code for the PDF export. 
        /// </value> 
        /// <remarks> 
        /// This property specifies the color used for text in the exported PDF document. 
        /// It accepts color values in various formats, such as color names (e.g., "Red"), RGB codes (e.g., "255,0,0"), or hexadecimal codes (e.g., "#FF0000"). 
        /// </remarks> 
        public string? TextColor { get; set; }

        /// <summary> 
        /// Gets or sets the highlight color of the text in the exported PDF. 
        /// </summary> 
        /// <value> 
        /// A string representing the color name, RGB code, or hexadecimal code for the PDF export. 
        /// </value> 
        /// <remarks> 
        /// This property specifies the color used to highlight text in the exported PDF document. 
        /// It accepts color values in various formats, such as color names (e.g., "Red"), RGB codes (e.g., "255,0,0"), or hexadecimal codes (e.g., "#FF0000"). 
        /// </remarks> 
        public string? TextHighlightColor { get; set; }
    }

    /// <summary>
    /// Defines pdf header options.
    /// </summary>
    public class PdfHeader
    {
        /// <summary>
        /// Defines the header contents.
        /// </summary>
        public List<PdfHeaderFooterContent>? Contents { get; set; }

        /// <summary>
        /// Defines the header content distance from top.
        /// </summary>
        public double FromTop { get; set; }

        /// <summary>
        /// Defines the height of header content.
        /// </summary>
        public double Height { get; set; }

        /// <summary> 
        /// Gets or sets the distance from the left side of the page to the header in the exported PDF. 
        /// </summary> 
        /// <value> 
        /// A float value representing the distance of the header content from the left side of the page. 
        /// </value> 
        /// <remarks> 
        /// This property specifies the horizontal position of the header content relative to the left side of the page. 
        /// </remarks> 
        public double Left { get; set; }
    }

    /// <summary>
    /// Represents class that provides Pdf header or footer customization options for the Pdf document.
    /// </summary>
    public class PdfHeaderFooterElement 
    {
        /// <summary> 
        /// Gets or sets the points for the lines drawn within the header or footer in the exported Pdf.  
        /// </summary>  
        /// <value>  
        /// An instance of <see cref="PdfPoints"/> representing the coordinates (X1, Y1, X2, Y2) of the lines drawn within the header or footer.  
        /// The coordinates are defined as follows: 
        /// <c>X1</c> and <c>Y1</c> specify the starting coordinates for the line. 
        /// <c>X2</c> and <c>Y2</c> specify the ending coordinates for the line. 
        /// </value>  
        /// <remarks>  
        /// <see cref="PdfPoints"/> class instance specifies the precise location of the lines within the header or footer.  
        /// The lines drawn in the header, coordinates should be greater than the <see cref="PdfHeader.FromTop"/> and <see cref="PdfHeader.Left"/> property values and within the height and width range of the header.  
        /// For the footer, the coordinates should be greater than the <see cref="PdfFooter.FromBottom"/> and <see cref="PdfFooter.Left"/> property values and within the height and width range of the footer.  
        /// Lines are only drawn when <c>ContentType</c> is set to <see cref="ContentType.Line"/>. 
        /// </remarks> 
        public PdfPoints? Points { get; set; }

        /// <summary>  
        /// Gets or sets the position of the content to be drawn within the header or footer in the exported Pdf.  
        /// </summary>  
        /// <value>  
        /// An instance of <see cref="PdfPosition"/> representing the horizontal (X) and vertical (Y) alignment of the content within the header or footer.  
        /// </value>  
        /// <remarks>  
        /// <see cref="PdfPosition"/> class represents the alignment of the content within the header or footer, specifying the position relative to the header or footer.  
        /// The <c>X</c> and <c>Y</c> coordinates should be greater than the <see cref="PdfHeader.FromTop"/> and <see cref="PdfHeader.Left"/> property values and within the height and width range of the header.  
        /// For the footer, they should be greater than the <see cref="PdfFooter.FromBottom"/> and <see cref="PdfFooter.Left"/> property values and within the height and width range of the footer. 
        /// </remarks>  
        public PdfPosition? Position { get; set; }

        /// <summary> 
        /// Gets or sets the size of the header or footer content in the exported Pdf. 
        /// </summary> 
        /// <value> 
        /// An instance of <see cref="PdfSize"/>. 
        /// </value> 
        /// <remarks> 
        /// <see cref="PdfSize"/> class represents the size that specifies the dimensions of the content within the header or footer. 
        /// </remarks> 
        public PdfSize? Size { get; set; }

        /// <summary> 
        /// Gets or sets the type of the header or footer content in the exported Pdf. 
        /// </summary> 
        /// <value> 
        /// One of the <see cref="ContentType"/> enumeration represents the type of the header or footer content in the exported Pdf. 
        /// The default content type is <b>Syncfusion.Blazor.Grids.ContentType.Image</b> 
        /// </value> 
        /// <remarks>  
        /// The ContentType specifies the nature of the header or footer content. It can be one of the following: 
        /// <list type="bullet"> 
        /// <item> 
        /// <description>ContentType.Image: The content is an image.</description> 
        /// </item> 
        /// <item> 
        /// <description>ContentType.Line: The content is a line.</description> 
        /// </item> 
        /// <item> 
        /// <description>ContentType.PageNumber: The content is a page number.</description> 
        /// </item> 
        /// <item> 
        /// <description>ContentType.Text: The content is text.</description> 
        /// </item> 
        /// </list> 
        /// </remarks> 
        public ContentType Type { get; set; }

        /// <summary> 
        /// Gets or sets the value of the header or footer content in the exported Pdf. 
        /// </summary> 
        /// <value> 
        /// An object representing the value of the header or footer content. 
        /// </value> 
        /// <remarks> 
        /// The value specifies the actual header or footer content to be displayed, such as text or image data. 
        /// </remarks> 
        public object? Value { get; set; }

        /// <summary>  
        /// Gets or sets the style of the header or footer content in the exported PDF. 
        /// </summary>
        /// <value>  
        /// An instance of <see cref="PdfElementStyle"/>.  
        /// </value>  
        /// <remarks>  
        /// The <see cref="PdfElementStyle"/> class represents the style that specifies the appearance of text in the header or footer content, such as font color and formatting. 
        /// </remarks>  
        public PdfElementStyle? ElementStyle { get; set; }

    }
    /// <summary>
    /// Defines pdf header footer options.
    /// </summary>
    public class PdfHeaderFooterContent : PdfHeaderFooterElement
    {
        /// <summary>
        /// Defines the font for the content. Value can be either PdfStandardFont or TrueTypeFont.
        /// </summary>
        public object? Font { get; set; }

        /// <summary>
        /// Defines the format for customizing page number.
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// Defines the page number type.
        /// </summary>
        public PdfPageNumberType PageNumberType { get; set; }

        /// <summary>
        /// Defines the base64 string for image content type.
        /// </summary>
        public string? Src { get; set; }

        /// <summary>
        /// Defines the style of content.
        /// </summary>
        public PdfContentStyle? Style { get; set; }

        /// <summary>
        /// Defines the trueTypeFont is enabled or not for font content.
        /// </summary>
        public bool IsTrueType { get; set; }

        /// <summary>
        /// Gets or sets the tion of the PDF header and footer content. It allows changing the text direction.
        /// </summary>
        /// <value>One of the values in the <see cref="PdfTextDirection"/> enumeration that specifies the text direction. The default value is <see cref="PdfTextDirection.None"/>.</value>
        /// <remarks>
        /// The <c>PdfTextDirection</c> property can be set to one of the following values:
        /// <list type="bullet">
        /// <item><description>None: Content is displayed without any shaping or formatting based on languages. For example, the content <c>Welcome to سينكفيوجن products</c> will be rendered as <c> Welcome to نجويفكنيس products</c></description></item>
        /// <item><description>LeftToRight: Content is shaping based on language and the reading order of content is from left to right. For example, the content <c>Welcome to سينكفيوجن products</c> will be rendered as <c> Welcome to سينكفيوجن products </c></description></item>
        /// <item><description>RightToLeft: Content is shaping based on language and the reading order of content is from right to left. For example, the content <c>Welcome to سينكفيوجن products</c> will be rendered as <c> products سينكفيوجن Welcome to</c></description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" Toolbar="@(new List<string>() { "PdfExport" })" AllowPdfExport="true">
        /// <GridEvents OnToolbarClick="ToolbarClickHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        ///@code{
        /// SfGrid<BusinessObject> Grid;
        /// public List<PdfHeaderFooterContent>? HeaderContent = new List<PdfHeaderFooterContent>
        /// {
        ///      new PdfHeaderFooterContent() { TextDirection = Syncfusion.PdfExport.PdfTextDirection.RightToLeft, Type = ContentType.Text, Value = "Welcome to سينكفيوجن products", Position = new PdfPosition() { X = 300, Y = 50 }},
        /// }
        /// public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs args)
        /// {
        /// if (args.Item.Text == "PDF Export")
        /// {
        ///          PdfExportProperties ExportProperties = new PdfExportProperties();
        ///          PdfHeader Header = new PdfHeader()
        ///          {
        ///                   Contents = HeaderContent,
        ///          };
        ///          ExportProperties.Header = Header;
        ///          await this.Grid.PdfExport(ExportProperties);
        /// }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public PdfTextDirection TextDirection { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.PdfHeaderQueryCellInfoEvent"/> event.
    /// </summary>
    public class PdfHeaderQueryCellInfoEventArgs
    {
        /// <summary>
        /// Gets the <see cref="Syncfusion.PdfExport.PdfGridCell"/> object that represents the current cell.
        /// </summary>
        /// <value>
        /// A <c>PdfGridCell</c> object that represents the current cell.
        /// </value>
        public PdfGridCell? Cell { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn"/> that represents the column of the current cell.
        /// </summary>
        /// <value>
        /// The grid column that represents the column of the current cell.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.PdfExport.PdfGridColumn"/> details of the current cell.
        /// </summary>
        /// <value>
        /// A  <c>PdfGridColumn</c> that represents the column details of the current cell.
        /// </value>
        public PdfGridColumn? PdfGridColumn { get; internal set; }

        /// <summary>
        /// Gets the row index of the current cell in the Pdf Grid column.
        /// </summary>
        /// <value>
        /// An integer value that represents the row index of the current cell.
        /// </value>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets the column index of the current cell in the Pdf Grid column.
        /// </summary>
        /// <value>
        /// An integer value that represents the column index of the current cell.
        /// </value>
        public int ColumnIndex { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.PdfExport.PdfGridCellStyle"/> object that represents the style of the current cell.
        /// </summary>
        /// <value>
        /// A <c>PdfGridCellStyle</c> object that represents the style of the current cell.
        /// </value>
        public PdfGridCellStyle? Style { get; set; }
    }

    /// <summary>
    /// Defines pdf points.
    /// </summary>
    public class PdfPoints
    {
        /// <summary>
        /// Defines the x1 position.
        /// </summary>
        public double X1 { get; set; }

        /// <summary>
        /// Defines the x2 position.
        /// </summary>
        public double X2 { get; set; }

        /// <summary>
        /// Defines the y1 position.
        /// </summary>
        public double Y1 { get; set; }

        /// <summary>
        /// Defines the y2 position.
        /// </summary>
        public double Y2 { get; set; }
    }

    /// <summary>
    /// Defines pdf position.
    /// </summary>
    public class PdfPosition
    {
        /// <summary>
        /// Defines the x position.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Defines the y position.
        /// </summary>
        public double Y { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.PdfQueryCellInfoEvent"/> event.
    /// </summary>
    public class PdfQueryCellInfoEventArgs<T> : PdfHeaderQueryCellInfoEventArgs
    {
        /// <summary>
        /// Gets the column span of the current cell.
        /// </summary>
        /// <value>
        /// An integer value that represents the number of columns spanned by the current cell.
        /// </value>
        public int ColSpan { get; set; }

        /// <summary>
        /// Gets the row data associated with the current cell in the PDF grid column.
        /// </summary>
        /// <value>
        /// A <typeparamref name="T"/> that represents the row data associated with the current cell in the PDF grid column.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the current cell.
        /// </summary>
        /// <value>
        /// An object that represents the value of the current cell.
        /// </value>
        public object? Value { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.PdfExport.PdfGridRow"/> object that represents the row in the PDF grid column.
        /// </summary>
        /// <value>
        /// A <c>PdfGridRow</c> that represents the row in the PDF grid column.
        /// </value>
        public PdfGridRow? Row { get; set; }
    }

    /// <summary>
    /// Defines pdf size.
    /// </summary>
    public class PdfSize
    {
        /// <summary>
        /// Defines the height.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Defines the width.
        /// </summary>
        public double Width { get; set; }
    }

    /// <summary>
    /// Defines pdf export theme.
    /// </summary>
    public class PdfTheme
    {
        /// <summary>
        /// Defines the theme style of caption content.
        /// </summary>
        public PdfThemeStyle? Caption { get; set; }

        /// <summary>
        /// Defines the style of header content.
        /// </summary>
        public PdfThemeStyle? Header { get; set; }

        /// <summary>
        /// Defines the theme style of record content.
        /// </summary>
        public PdfThemeStyle? Record { get; set; }
    }

    /// <summary>
    /// Defines pdf export theme.
    /// </summary>
    public class PdfThemeStyle
    {
        /// <summary>
        /// Defines the bold of theme style.
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Defines the borders of theme style.
        /// </summary>
        public PdfBorder? Border { get; set; }

        /// <summary>
        /// Defines the font of the theme.
        /// </summary>
        public PdfGridFont? Font { get; set; }

        /// <summary>
        /// Defines the font color of theme style.
        /// </summary>
        public string? FontColor { get; set; }

        /// <summary>
        /// Defines the font name of theme style.
        /// </summary>
        public string? FontName { get; set; }

        /// <summary>
        /// Defines the font size of theme style.
        /// </summary>
        public double FontSize { get; set; }

        /// <summary>
        /// Defines the italic of theme style.
        /// </summary>
        public bool Italic { get; set; }

        /// <summary>
        /// Defines the strikeout of theme style.
        /// </summary>
        public bool Strikeout { get; set; }

        /// <summary>
        /// Defines the underline of theme style.
        /// </summary>
        public bool Underline { get; set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.QueryCellInfo"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    public class QueryCellInfoEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets the CellDOM object associated with the grid content cell.
        /// </summary>
        /// <value>
        /// The CellDOM object that represents the grid content cell.
        /// </value>
        /// <remarks>
        /// The CellDom properties include:
        /// <list type="bullet">
        /// <item><description>
        /// <b>HasChanges:</b> A boolean property that indicates whether the Cell object associated with the grid content cell has changed.
        /// </description></item>
        /// <item><description>
        /// <b>ClassList:</b> A property that contains the class list of the grid content cell.
        /// </description></item>
        /// <item><description>
        /// <b>Styles:</b> A property that contains the styles of the grid content cell.
        /// </description></item>
        /// <item><description>
        /// <b>AttributeList:</b> A property that contains the attribute list of the grid content cell.
        /// </description></item>
        /// <item><description>
        /// <b>AddClass:</b> A method to add class names to the class list for the current grid content cell.
        /// </description></item>
        /// <item><description>
        /// <b>AddStyle:</b> A method to add styles for the current grid content cell.
        /// </description></item>
        /// <item><description>
        /// <b>SetAttribute:</b> A method to set an attribute for the current grid content cell.
        /// </description></item>
        /// </list>
        /// </remarks>
        public CellDOM? Cell { get; internal set; }

        /// <summary>
        /// Gets the corresponding <see cref="Syncfusion.Blazor.Grids.GridColumn"/> associated with the content of the current cell in the grid.
        /// </summary>
        /// <value>
        /// A <see cref="GridColumn"/> object that represents the corresponding column associated with the content of the current cell in the grid.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the row data associated with the content of the current cell in the grid.
        /// </summary>
        /// <value>
        /// A <typeparamref name="T"/> object that represents the row data associated with the content of the current cell in the grid.
        /// </value>
        public T? Data { get; internal set; }
        /// <summary>
        /// Gets the foreign key row data associated with the grid column.
        /// </summary>
        /// <value>
        /// An <see cref="IDictionary{TKey,TValue}"/> object that represents the foreign key row data associated with the grid column.
        /// </value>
        public IDictionary<string, IEnumerable<object>>? ForeignKeyData { get; internal set; }

    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRecordClick"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RecordClickEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the index of the clicked cell in the grid column.
        /// </summary>
        /// <value>
        /// An integer value that represents the index of the clicked cell in the grid column.
        /// </value>
        public int CellIndex { get; internal set; }

        /// <summary>
        /// Gets the grid column of the clicked cell.
        /// </summary>
        /// <value>
        /// A <see cref="Syncfusion.Blazor.Grids.GridColumn"/> that represents the grid column of the clicked cell.
        /// </value>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the foreign key row data associated with the column.
        /// </summary>
        /// <value>
        /// A dictionary of foreign key row data associated with the column. The keys of the dictionary represent the names of the tables, and the values represent the associated rows as a collection of objects.
        /// </value>
        public IDictionary<string, IEnumerable<object>>? ForeignKeyData { get; internal set; }

        /// <summary>
        /// Gets the row data of the clicked row.
        /// </summary>
        /// <value>
        /// The row data of the clicked row as an object of type T.
        /// </value>
        public T? RowData { get; internal set; }

        /// <summary>
        /// Gets the index of the clicked row.
        /// </summary>
        /// <value>
        /// The index of the clicked row as an integer.
        /// </value>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets or sets the DOM object associated with the clicked grid cell.
        /// </summary>
        /// <value>
        /// The DOM object representing the clicked grid cell.
        /// </value>
        /// <remarks>
        /// The <see cref="CellDOM"/> properties include:
        /// <list type="bullet">
        /// <item><description>
        /// <b>HasChanges:</b> Indicates whether the cell object associated with the clicked grid cell has been modified.
        /// </description></item>
        /// <item><description>
        /// <b>ClassList:</b> Contains the list of classes applied to the clicked grid cell.
        /// </description></item>
        /// <item><description>
        /// <b>Styles:</b> Contains the inline styles of the clicked grid cell.
        /// </description></item>
        /// <item><description>
        /// <b>AttributeList:</b> Contains the attributes of the clicked grid cell.
        /// </description></item>
        /// <item><description>
        /// <b>AddClass:</b> Adds one or more class names to the class list of the clicked grid cell.
        /// </description></item>
        /// <item><description>
        /// <b>AddStyle:</b> Adds one or more inline styles to the clicked grid cell.
        /// </description></item>
        /// <item><description>
        /// <b>SetAttribute:</b> Sets a specific attribute for the clicked grid cell.
        /// </description></item>
        /// </list>
        /// </remarks>
        public CellDOM? CurrentCell { get; internal set; }

        /// <summary>
        /// Gets or sets the value of the clicked cell.
        /// </summary>
        /// <value>
        /// The data value of the clicked cell. This can be of any type, such as <c>string</c>, <c>bool</c>, <c>DateTime</c>, or any other type that represents the cell's data.
        /// </value>
        /// <remarks>
        /// Use this property to access the underlying data value of the clicked cell.
        /// If the grid uses a template cell, this property returns the corresponding data value instead of the rendered template content.  
        /// </remarks>
        public object? CellValue { get; internal set; }

    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRecordDoubleClick"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RecordDoubleClickEventArgs<T> : RecordClickEventArgs<T>
    {
        
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnResizeStart"/> event.
    /// </summary>
    public class ResizeArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the resize action.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then the resize action will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the details of the resizing column.
        /// </summary>
        /// <value>
        /// A <see cref="Syncfusion.Blazor.Grids.GridColumn"/> object that represents the details of the resizing column.
        /// </value>
        public GridColumn? Column { get; internal set; }
    }

    /// <summary>
    /// Provides information about an <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDataBound"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowDataBoundEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the current row data.
        /// </summary>
        /// <value>
        /// An object of type T? that represents the current row data.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets or sets the CellDOM object associated with the grid content row.
        /// </summary>
        /// <value>
        /// The CellDOM object that represents the grid content row.
        /// </value>
        /// <remarks>
        /// The CellDom properties include:
        /// <list type="bullet">
        /// <item><description>
        /// <b>HasChanges:</b> A boolean property that indicates whether the Cell object associated with the grid content row has changed.
        /// </description></item>
        /// <item><description>
        /// <b>ClassList:</b> A property that contains the class list of the grid content row.
        /// </description></item>
        /// <item><description>
        /// <b>Styles:</b> A property that contains the styles of the grid content row.
        /// </description></item>
        /// <item><description>
        /// <b>AttributeList:</b> A property that contains the attribute list of the grid content row.
        /// </description></item>
        /// <item><description>
        /// <b>AddClass:</b> A method to add class names to the class list for the current grid content row.
        /// </description></item>
        /// <item><description>
        /// <b>AddStyle:</b> A method to add styles for the current grid content row.
        /// </description></item>
        /// <item><description>
        /// <b>SetAttribute:</b> A method to set an attribute for the current grid content row.
        /// </description></item>
        /// </list>
        /// </remarks>
        public CellDOM? Row { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDeselecting"/> event.
    /// Also, provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDeselected"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowDeselectEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the <see cref="MouseEventArgs"/> of the currently deselected/deselecting row.
        /// </summary>
        public MouseEventArgs? Event { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the deselection of the row.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, then the deselection of the row will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the row data associated with the currently deselecting or deselected row in a grid.
        /// </summary>
        /// <value>
        /// An object of type <typeparamref name="T"/> representing the data associated with the currently deselecting or deselected row in a grid.
        /// </value>  
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets the collection of row data while perform the deselecting action.
        /// </summary>
        /// <value>
        /// A collection of row data which is associate with clear the multiple selection.
        /// </value>
        /// <remarks>
        /// When binding remote data, unselect-all action using checkbox returns only the data of rows in the current view.
        /// When binding list data, if <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.PersistSelection"/> is disabled then unselect-all action using checkbox returns only the data of rows in the current view.
        /// </remarks>
        public List<T>? Datas { get; internal set; }

        /// <summary>
        /// Gets the foreignkey row data associated with the currently deselecting or deselected row in a grid.
        /// </summary>
        /// <value>
        /// An object representing the foreignkey row data associated with the currently deselecting or deselected row in a grid.
        /// </value>
        public IDictionary<string, IEnumerable<object>>? ForeignKeyData { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the event was triggered by user interaction or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the event was triggered by user interaction; otherwise, <c>false</c>.
        /// </value>
        public bool IsInteracted { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the header checkbox was clicked or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the header checkbox was clicked; otherwise, <c>false</c>.
        /// </value>
        public bool IsHeaderCheckboxClicked { get; internal set; }

        /// <summary>
        /// Gets the row index associated with the deselecting action.
        /// </summary>
        /// <value>
        /// The row index that is associated with the deselecting action.
        /// </value>
        /// <remarks>
        /// If multiple rows are selected and an attempt is made to clear the selection, 
        /// this property will return the index of the last row that was deselected.
        /// </remarks> 
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets the collection of row indexes associated with the deselecting action.
        /// </summary>
        /// <value>
        /// A collection of row indexes that are associated with the deselecting action.
        /// </value>
        /// <remarks>
        /// This property returns only the indexes of the rows that are currently visible in the view, 
        /// even if the user attempts to unselect all rows using a checkbox selection.
        /// </remarks>    
        public List<int>? RowIndexes { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the CTRL key is currently pressed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the CTRL key is currently pressed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCtrlPressed { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the SHIFT key is currently pressed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the SHIFT key is currently pressed; otherwise, <c>false</c>.
        /// </value>
        public bool IsShiftPressed { get; internal set; }
    }

    /// <summary>
    /// Defines the dimension of selected target.
    /// </summary>
    public class Dimension
    {
        /// <summary>
        /// Defines the left position of the target.
        /// </summary>
        [JsonPropertyName("left")]
        public double Left { get; set; }

        /// <summary>
        /// Defines the right position of the target.
        /// </summary>
        [JsonPropertyName("right")]
        public double Right { get; set; }

        /// <summary>
        /// Defines the top position of the target.
        /// </summary>
        [JsonPropertyName("top")]
        public double Top { get; set; }
        /// <summary>
        /// Defines the bottom position of the target.
        /// </summary>
        [JsonPropertyName("bottom")]
        public double Bottom { get; set; }

        /// <summary>
        /// Defines the width position of the target.
        /// </summary>
        [JsonPropertyName("width")]
        public double Width { get; set; }

        /// <summary>
        /// Defines the height position of the target.
        /// </summary>
        [JsonPropertyName("height")]
        public double Height { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDragStarting"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowDragStartingEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the collection of row data that is going to be dragged.
        /// </summary>
        /// <value>
        /// A list of the row data associated with the drag start action.
        /// </value>
        public List<T>? Data { get; internal set; }

        /// <summary>
        /// Gets the row index of the row associated with the drag start action.
        /// </summary>
        /// <value>
        /// The index of the row that is being dragged from the grid.
        /// </value>
        public int FromIndex { get; internal set; } 
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDropping"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component</typeparam>
    public class RowDroppingEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the row drop action.
        /// </summary>
        /// <value>
        /// <b>true</b>, if the drop action is cancelled; otherwise, <b>false</b>. The default value is <b>false</b>.
        /// </value>       
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the collection of row data associated with a dropped action in the grid.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of row data associated with the dropped action.
        /// </value>
        public List<T>? Data { get; internal set; }


        /// <summary>
        /// Gets the current action, either "Add" or "Delete", when dragging and dropping rows between two grids.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> value that represents the current action. Possible values are "Add" if the collection of row data was added to the destination grid, and "Delete" if the collection of row data was removed from the source grid.
        /// </value>
        /// <remarks>
        /// The value of the <c>Action</c> property will be null when performing drag and drop operations within the same grid. 
        /// This property is set and updated when the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDropping"/> event is triggered.
        /// </remarks>
        public string? Action { get; internal set; }

        /// <summary>
        /// Gets the row index of the row associated with the drag start action.
        /// </summary>
        /// <value>
        /// An integer value representing the index of the row associated with the drag start action.
        /// </value>
        public int FromIndex { get; internal set; }

        /// <summary>
        /// Gets the row index of the row associated with the drop action.
        /// </summary>
        /// <value>
        /// An integer value representing the index of the row associated with the drop action.
        /// </value>
        public int DropIndex { get; internal set; }

        /// <summary>
        /// Gets the target element's ID and its XPath.
        /// </summary>
        /// <value>
        /// <c>ID</c>, returns ID of the target element. If there is no ID for the target element then the value for the ID will be empty string(ID= "")
        /// <c>XPath</c>, XPath of the target element.
        /// </value>
        public DOM? Target { get; internal set; }

        /// <summary>
        /// Gets the dimensions of the target element.
        /// </summary>
        /// <remarks>
        /// The <see cref="TargetDimension"/> contains the following dimensions:
        /// <list type="bullet">
        /// <item><term>Left</term><description>The left position of the target element.</description></item>
        /// <item><term>Right</term><description>The right position of the target element.</description></item>
        /// <item><term>Top</term><description>The top position of the target element.</description></item>
        /// <item><term>Bottom</term><description>The bottom position of the target element.</description></item>
        /// <item><term>Height</term><description>The height of the target element.</description></item>
        /// <item><term>Width</term><description>The width of the target element.</description></item>
        /// </list>
        /// </remarks>
        public Dimension? TargetDimension { get; internal set; }

        /// <summary>
        /// Gets the X-coordinate of the mouse pointer in the browser's client area at the time the event was triggered.
        /// </summary>
        /// <value>
        /// The X-coordinate of the mouse pointer.
        /// </value>
        public double ClientX { get; internal set; }

        /// <summary>
        /// Gets the Y-coordinate of the mouse pointer in the browser's client area at the time the event was triggered.
        /// </summary>
        /// <value>
        /// The Y-coordinate of the mouse pointer.
        /// </value>
        public double ClientY { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDropped"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component</typeparam>
    public class RowDroppedEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the current action performed during drag and drop between two grids.
        /// </summary>
        /// <value>
        /// The current action can be one of the following:
        /// <list type="bullet">
        /// <item><description><c>Add</c> - The row data is added to the destination grid.</description></item>
        /// <item><description><c>Delete</c> - The row data is removed from the source grid.</description></item>
        /// </list>
        /// </value>
        /// <remarks>
        /// The value of the <see cref="Action"/> property will be <c>null</c> when the drag and drop operation occurs within the same grid and the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDropping"/> event is triggered.
        /// </remarks>
        public string? Action { get; internal set; }

        /// <summary>
        /// Gets the X-coordinate of the mouse pointer in the browser's client area at the time the event was triggered.
        /// </summary>
        /// <value>
        /// The X-coordinate of the mouse pointer.
        /// </value>
        public double ClientX { get; internal set; }

        /// <summary>
        /// Gets the Y-coordinate of the mouse pointer in the browser's client area at the time the event was triggered.
        /// </summary>
        /// <value>
        /// The Y-coordinate of the mouse pointer.
        /// </value>
        public double ClientY { get; internal set; }

        /// <summary>
        /// Gets the collection of row data that is going to be dragged.
        /// </summary>
        /// <value>
        /// A list of the row data associated with the drag start action.
        /// </value>
        public List<T>? Data { get; internal set; }

        /// <summary>
        /// Gets the row index of the row associated with the drag start action.
        /// </summary>
        /// <value>
        /// The index of the row that is being dragged from the grid.
        /// </value>
        public int FromIndex { get; internal set; }

        /// <summary>
        /// Gets the row index of the row that is associated with the drop action.
        /// </summary>
        /// <value>
        /// The index of the row that is associated with the drop action.
        /// </value>
        public int DropIndex { get; internal set; }

        /// <summary>
        /// Gets the target element's ID and its XPath.
        /// </summary>
        /// <value>
        /// <c>ID</c>, returns ID of the target element. If there is no ID for the target element then the value for the ID will be empty string(ID= "")
        /// <c>XPath</c>, XPath of the target element.
        /// </value>
        public DOM? Target { get; internal set; }

        /// <summary>
        /// Gets the dimensions of the target element.
        /// </summary>
        /// <remarks>
        /// The dimensions include the following properties:
        /// <list type="bullet">
        ///     <item><term>Left</term><description>The left position of the target element.</description></item>
        ///     <item><term>Right</term><description>The right position of the target element.</description></item>
        ///     <item><term>Top</term><description>The top position of the target element.</description></item>
        ///     <item><term>Bottom</term><description>The bottom position of the target element.</description></item>
        ///     <item><term>Height</term><description>The height of the target element.</description></item>
        ///     <item><term>Width</term><description>The width of the target element.</description></item>
        /// </list>
        /// </remarks>
        public Dimension? TargetDimension { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDragSelectionStarting"/> event,
    /// which occurs when drag based selection of rows or cells is initiated in the Grid.
    /// </summary>
    /// <typeparam name="T">The type of the data bound to the Data Grid.</typeparam>
    public class RowDragSelectionEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the index of the row where the drag selection begins.
        /// </summary>
        public int RowStartIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the cell where the drag selection begins.
        /// </summary>
        public int CellStartIndex { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the drag selection operation.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// If the <c>Cancel</c> property is set to <c>true</c>, the drag selection action will be canceled.
        /// </remarks>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="GridEvents{TValue}.RowDragSelectionCompleting"/> and 
    /// <see cref="GridEvents{TValue}.RowDragSelectionCompleted"/> events, which occur when drag based selection ends in the Grid.
    /// </summary>
    /// <typeparam name="T">The type of the data bound to the Data Grid.</typeparam>
    public class RowDragSelectedEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Provides the ID of the target Grid if the drop target is a Grid; otherwise, returns <c>null</c>.
        /// </summary>
        /// <value>
        /// The target Grid ID if applicable; otherwise, <c>null</c>.
        /// </value>
        public string? TargetGridID { get; internal set; }

        /// <summary>
        /// Gets the index of the row where the drag selection started.
        /// </summary>
        public int RowStartIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the row where the drag selection ended.
        /// </summary>
        public int RowEndIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the cell where the drag selection started.
        /// </summary>
        public int CellStartIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the cell where the drag selection ended.
        /// </summary>
        public int CellEndIndex { get; internal set; }

    }

    /// <summary>
    /// Defines the row info such as data, row index and cell index.
    /// </summary>
    public class RowInfo<T>
    {
        /// <summary>
        /// returns particular cell index.
        /// </summary>
        public int CellIndex { get; internal set; }

        /// <summary>
        /// return particular column information.
        /// </summary>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// returns particular row data.
        /// </summary>
        public T? RowData { get; internal set; }

        /// <summary>
        /// returns particular rowIndex.
        /// </summary>
        public int RowIndex { get; internal set; }


    }

    /// <summary>
    /// Defines the context details of the FilterItemTemplate of checkbox and excel filter.
    /// </summary>
    /// <seealso cref="Syncfusion.Blazor.Grids.GridColumn.FilterItemTemplate"/>
    public class FilterItemTemplateContext
    {
        /// <summary>
        /// Gets the current cell value.
        /// </summary>
        public object? Value { get; internal set; }

        /// <summary>
        /// Gets the current column.
        /// </summary>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the current record.
        /// </summary>
        public object? Record { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowSelected"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowSelectEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the <see cref="MouseEventArgs"/> of the currently selected row.
        /// </summary>
        public MouseEventArgs? Event { get; internal set; }

        /// <summary>
        /// Gets the row data of the first selected row, when multiple rows are selected.
        /// </summary>
        /// <value>
        /// The row data of the first selected row.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets the collection of row data that is currently selected.
        /// </summary>
        /// <value>
        /// A collection of selected row data.
        /// </value>
        /// <remarks>
        /// When binding remote data, the select all action using checkbox selection only returns the data of rows in the current view.
        /// When binding list data, if <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.PersistSelection"/> is disabled, the select all action using checkbox selection only returns the data of rows in the current view.
        /// </remarks>
        public List<T>? Datas { get; internal set; }

        /// <summary>
        /// Gets the foreign key row data associated with this column.
        /// </summary>
        /// <value>
        /// A dictionary containing the foreign key column name and a collection of the associated row data for the column.
        /// </value>
        public IDictionary<string, IEnumerable<object>>? ForeignKeyData { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the event was triggered by user interaction or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the event was triggered by user interaction; otherwise, <c>false</c>.
        /// </value>
        public bool IsInteracted { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the header checkbox was clicked or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the header checkbox was clicked; otherwise, <c>false</c>.
        /// </value>
        public bool IsHeaderCheckboxClicked { get; internal set; }

        /// <summary>
        /// Gets the index of the previously selected row.
        /// </summary>
        /// <value>
        /// An integer value representing the index of the previously selected row.
        /// </value>
        public int PreviousRowIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the selected row.
        /// </summary>
        /// <value>
        /// An integer value representing the index of the selected row.
        /// </value>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets the collection of selected row indexes.
        /// </summary>
        /// <value>
        /// A collection of selected row indexes.
        /// Returns only the indexes of the rows in current view, even though user select all rows using checkbox selection.
        /// </value>
        public List<int>? RowIndexes { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the CTRL key is currently pressed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the CTRL key is currently pressed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCtrlPressed { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the SHIFT key is currently pressed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the SHIFT key is currently pressed; otherwise, <c>false</c>.
        /// </value>
        public bool IsShiftPressed { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the Up or Down arrow key is currently pressed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the Up or Down arrow key is currently pressed; otherwise, <c>false</c>.
        /// </value>
        internal bool IsVerticalArrowPressed { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowSelecting"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowSelectingEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the <see cref="MouseEventArgs"/> of the currently selected row.
        /// </summary>
        public MouseEventArgs? Event { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the selection action.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If the Cancel property is set to <c>true</c>, the row selection action will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the data of the row that is going to be selected.
        /// </summary>
        /// <value>
        /// The data of the row associated with the selection action.
        /// </value>
        /// <remarks>
        /// This property returns the data of the row that is going to be selected in a grid. 
        /// </remarks>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets the collection of row data going to be selected.
        /// </summary>
        /// <value>
        /// A collection of row data going to be selected.
        /// </value>
        /// <remarks>
        /// When binding remote data, select all action using checkbox selection returns only the data of rows in the current view.
        /// When binding list data, if <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.PersistSelection"/> is disabled then select all action using checkbox returns only the data of rows in the current view.
        /// </remarks>
        public List<T>? Datas { get; internal set; }

        /// <summary>
        /// Gets the foreign key row data associated with a grid foreign key column.
        /// </summary>
        /// <value>
        /// A dictionary containing the foreign key row data.
        /// </value>
        public IDictionary<string, IEnumerable<object>>? ForeignKeyData { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the CTRL key is currently pressed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the CTRL key is currently pressed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCtrlPressed { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the event was triggered by user interaction or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the event was triggered by user interaction; otherwise, <c>false</c>.
        /// </value>
        public bool IsInteracted { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the SHIFT key is currently pressed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the SHIFT key is currently pressed; otherwise, <c>false</c>.
        /// </value>
        public bool IsShiftPressed { get; internal set; }

        /// <summary>
        /// Gets a boolean value indicating whether the header checkbox was clicked or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if the header checkbox was clicked; otherwise, <c>false</c>.
        /// </value>
        public bool IsHeaderCheckboxClicked { get; internal set; }

        /// <summary>
        /// Gets the index of the previously selected row.
        /// </summary>
        /// <value>
        /// The index of the previously selected row.
        /// </value>
        public int PreviousRowIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the row that is going to be selected.
        /// </summary>
        /// <value>
        /// The index of the row that is going to be selected.
        /// </value>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets the collection of row indexes going to be selected.
        /// </summary>
        /// <value>
        /// A collection of row indexes going to be selected.
        /// Returns only the indexes of the rows in current view, even though user selects all the rows using checkbox selection.
        /// </value>
        public List<int>? RowIndexes { get; internal set; }
    }

    /// <summary>
    /// Provides information about <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.FreezeLineMoving"/> event callback.
    /// </summary>
    public class FreezeLineMovingEventArgs : FreezeLineMovedEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the freeze line moving action.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If the Cancel property is set to <c>true</c>, the freeze line moving action will be cancelled.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.FreezeLineMoved"/>  event callback.
    /// </summary>
    public class FreezeLineMovedEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the index of the starting column while dragging the frozen line.
        /// </summary>
        /// <value>
        /// The index of the starting column while dragging the frozen line.
        /// </value>
        public int StartIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the ending column while dropping the frozen line.
        /// </summary>
        /// <value>
        /// The index of the ending column while dropping the frozen line.
        /// </value>
        public int EndIndex { get; internal set; }

        /// <summary>
        /// Gets the frozen columns in the grid.
        /// </summary>
        /// <value>
        /// A list of frozen columns in the grid.
        /// </value>
        public List<GridColumn>? FrozenColumns { get; internal set; }

        /// <summary>
        /// Gets the direction of the column freeze.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.Blazor.Grids.FreezeDirection"/> of the columns. Possible values include:
        /// <list type="bullet">
        ///     <item><term>None:</term><description>Column will not freeze.</description></item>
        ///     <item><term>Left:</term><description>Freeze the column at the left side.</description></item>
        ///     <item><term>Right:</term><description>Freeze the column at the right side.</description></item>
        /// </list>
        /// </value>
        public FreezeDirection Direction { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.Sorting"/> event.
    /// </summary>
    public class SortingEventArgs : SortedEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the sorting action.
        /// </summary>
        /// <value>
        /// The default value is false.
        /// </value>
        /// <remarks>
        /// The <c>Cancel</c> property is used to control the sorting action of the grid. If the <c>Cancel</c> property is set to true, then the sorting action will be Canceled.
        /// </remarks>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets a value indicating whether the CTRL key is currently pressed for multi-sorting.
        /// </summary>
        /// <value>
        /// <c>true</c> if the CTRL key is pressed for multi-sorting; otherwise, <c>false</c>.
        /// The default value is <c>false</c>.
        /// </value>
        public bool IsCtrlKeyPressed { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.Sorted"/> event.
    /// </summary>
    public class SortedEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets a value indicating the sorting action.
        /// </summary>
        /// <value>
        /// <c>Add</c> sorts the grid data based on the specified column and direction.
        /// <c>Remove</c> Removes sorting from the specified column.
        /// <c>Replace</c> when the sort column direction changes from Ascending to Descending or vice versa for the same column.
        /// <c>Reset</c> Clears sorting from all columns in the grid using <see cref="SfGrid{TValue}.ClearSortingAsync()"/> Method.
        /// </value>
        public NotifyCollectionChangedAction Action { get; internal set; }

        /// <summary>
        /// Gets the field name of the column which is associated with sorting.
        /// </summary>
        /// <value>
        /// The string value that represents the field name of column which is associated with sorting.
        /// </value>
        /// <remarks>
        /// This property returns the field name of the column currently associated with sorting,
        /// even in cases of multi-sorting.
        /// </remarks>
        public string? ColumnName { get; internal set; }

        /// <summary>
        /// Gets or sets the direction of the sorted column.
        /// </summary>
        /// <remarks>
        /// The available sort directions are:
        /// <c>SortDirection.None</c>: Default, no sorting is applied or when sorting is removed.
        /// <c>SortDirection.Ascending</c>: Sorts records in ascending order.
        /// <c>SortDirection.Descending</c>: Sorts records in descending order.
        /// </remarks>
        public SortDirection Direction { get; set; }

        /// <summary>
        /// Gets the list of sorted columns field name and it's sort direction.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.Blazor.Grids.SortColumn"/> objects. By default, it is null.
        /// </value>
        /// <remarks>
        /// The list of sorted columns field name and it's sort direction will be available when columns are sorted using <see cref="SfGrid{TValue}.SortColumnsAsync(List{SortColumn}, bool)"/> method.
        /// </remarks>
        public List<SortColumn>? SortedColumns { get; internal set; }
    }


    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.Grouping"/> event.
    /// </summary>
    public class GroupingEventArgs : GroupedEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the grouping or un-grouping action.
        /// </summary>
        /// <value>
        /// The default value is false.
        /// </value>
        /// <remarks>
        /// The <c>Cancel</c> property is used to control the grouping action. If the <c>Cancel</c> property is set to true, then the grouping or un-grouping action will be canceled.
        /// </remarks>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.Grouped"/> event.
    /// </summary>
    public class GroupedEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets a value indicating the grouping action.
        /// </summary>
        /// <value>
        /// <c>Add</c> to group the specified column.
        /// <c>Remove</c> to remove grouping from the specified column.
        /// <c>Reset</c> to clear grouping from all columns in the grid using the <see cref="SfGrid{TValue}.ClearGroupingAsync()"/> method.
        /// </value>
        public NotifyCollectionChangedAction Action { get; internal set; }

        /// <summary>
        /// Gets the field name of the column which is associated with grouping or un-grouping.
        /// </summary>
        /// <value>
        /// A string value that represents the field name of the column which is associated with grouping or un-grouping.
        /// </value>
        public string? ColumnName { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.Searching"/> event.
    /// </summary>
    public class SearchingEventArgs : SearchedEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the search action in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, 
        /// then the search action will be canceled.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.Searched"/> event.
    /// </summary>
    public class SearchedEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets the value to search.
        /// </summary>
        /// <value>
        /// A string representing the value to search. Default value is an empty string.
        /// </value>
        public string SearchText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowCreating"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowCreatingEventArgs<T> : RowCreatedEventArgs<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the adding new record action in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, 
        /// then the adding new record action will be canceled.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowCreated"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowCreatedEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets the data of the new row associated with the adding action.
        /// </summary>
        /// <value>
        /// A <typeparamref name="T"/> object representing the data of the grid.
        /// </value>
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets the index of the row associated with the adding action.
        /// </summary>
        /// <value>
        /// An integer representing the index of the newly added row. The default value is 0.
        /// </value>
        /// <remarks>
        /// If the index property is set, then the add form will be generated in the grid based on the specified index.
        /// </remarks>
        public int Index { get; set; }

        /// <summary>
        /// Gets the current <see cref="EditContext"/> instance.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="EditContext"/> class that represents the current edit context. By default, the value is null.
        /// </value>
        public EditContext? EditContext { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowUpdating"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowUpdatingEventArgs<T> : RowUpdatedEventArgs<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the saving action in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, 
        /// then the saving action will be canceled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets a Boolean value indicating whether the Shift key was pressed from the first edited cell to save the edited record in <see cref="Syncfusion.Blazor.Grids.EditMode.Normal"/> edit mode.  
        /// </summary>
        /// <value>
        /// <b>true</b> if the SHIFT key is currently pressed; otherwise, <c>false</c>        
        /// </value>
        public bool IsShiftKeyPressed { get; internal set; }

        /// <summary>
        /// Gets the string that identifies the physical key being pressed, while saving the edited record using Enter or Tab keys in <see cref="Syncfusion.Blazor.Grids.EditMode.Normal"/> edit mode.
        /// </summary>
        /// <value>
        /// <c>Enter</c>: The Enter key is used to save the edited record.
        /// <c>Tab</c>: The Tab key is pressed from the last edited cell, or the combination of <c>Shift+Tab</c> keys is pressed from the first edited cell to save the edited record.
        /// By default, the value is set to null.
        /// </value>
        /// <remarks>
        /// The value of this property is assigned while performing the save operation using Enter or Tab keys or a combination of the Shift+Tab keyboard keys.
        /// </remarks>
        public string? KeyCode { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowUpdated"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowUpdatedEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets the data of the row associated with updating in the grid.
        /// </summary>
        /// <value>
        /// A <typeparamref name="T"/> object representing the data of the grid.
        /// </value>
        public T? Data { get; set; }

        /// <summary>
        /// Gets the row index of the row associated with updating.
        /// </summary>
        /// <value>
        /// The row index of the updating row. By default, the value is 0.
        /// </value>
        /// <remarks>
        /// The value of this property can be used to identify which row in a collection or data source is being saved.
        /// </remarks>
        public int Index { get; internal set; }

        /// <summary>
        /// Gets the previous data of the row.
        /// </summary>
        /// <value>
        /// An object of type T? that contains the previous data of the row. By default, the value is null.
        /// </value>
        public T? PreviousData { get; internal set; }

        /// <summary>
        /// Gets the list of the primary key values.
        /// </summary>
        /// <value>
        /// A string array that contains the list of primary key values.
        /// </value>
        public string[]? PrimaryKeys { get; internal set; }

        /// <summary>
        /// Gets the primary key value of the <see cref="Syncfusion.Blazor.Grids.GridColumn"/>.
        /// </summary>
        /// <value>
        /// An object that defines the primary key value of the column when <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/> is true, otherwise null.
        /// </value>
        public object? PrimaryKeyValue { get; internal set; }

        /// <summary>
        /// Gets the type of update action: whether a new row was added or an existing row was edited.
        /// </summary>
        /// <value>
        /// The action performed on the row. The possible values are:
        /// <list type="bullet">
        /// <item>
        /// <term>Added</term>
        /// <description>A new row is added to the grid.</description>
        /// </item>
        /// <item>
        /// <term>Edited</term>
        /// <description>An existing row is updated in the grid.</description>
        /// </item>
        /// </list>    
        /// </value>
        /// <remarks>
        /// This property is used when the Grid edit mode is set to 
        /// <see cref="Syncfusion.Blazor.Grids.EditMode.Normal"/> or 
        /// <see cref="Syncfusion.Blazor.Grids.EditMode.Dialog"/>.
        /// </remarks>
        public SaveActionType Action { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDeleting"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowDeletingEventArgs<T> : RowDeletedEventArgs<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the delete action in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, 
        /// then the delete action will be canceled.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDeleted"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowDeletedEventArgs<T> : GridEventBaseArgs
    {

        /// <summary> 
        /// Gets the collection of row data intended for deletion. 
        /// </summary> 
        /// <value> 
        /// A collection of row data to be deleted. 
        /// <typeparamref name="T"/> object representing the data of the grid. 
        /// </value> 
        /// <remarks> 
        /// This collection holds the row data that is marked for deletion, whether it's a single row or multiple rows. 
        /// Additionally, it holds the deleted row data when the delete operation is performed using the <see cref="SfGrid{TValue}.DeleteRecordAsync(string, TValue)"/> method. 
        /// </remarks> 
        public List<T>? Datas { get; internal set; }

        /// <summary>
        /// Gets the list of the primary key values.
        /// </summary>
        /// <value>
        /// A string array that contains the list of primary key values.
        /// </value>
        public string[]? PrimaryKeys { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}. EditCanceling "/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class EditCancelingEventArgs<T>: EditCanceledEventArgs<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the editing or adding new record actions in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, 
        /// then the cancel action will be Canceled.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.EditCanceled"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class EditCanceledEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the data of the grid which is associated with canceling action.
        /// </summary>
        /// <value>
        /// A <typeparamref name="T"/> object representing the data of the grid.
        /// </value>
        public T? Data { get; internal set; }

        /// <summary>
        /// Gets the previous data of the row.
        /// </summary>
        /// <value>
        /// An object of type T? that contains the previous data of the row. By default, the value is null.
        /// </value>
        public T? PreviousData { get; internal set; }

        /// <summary>
        /// Gets the list of the primary key values.
        /// </summary>
        /// <value>
        /// A string array that contains the list of primary key values.
        /// </value>
        public string[]? PrimaryKeys { get; internal set; }

        /// <summary>
        /// Gets the row index of the row associated with canceling action.
        /// </summary>
        /// <value>
        /// The row index of the canceling row. By default, the value is 0.
        /// </value>
        /// <remarks>
        /// The value of this property can be used to identify which row in a collection or data source is being canceled.
        /// </remarks>
        public int Index { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRowEditStart "/> event.
    /// </summary>
    public class OnRowEditStartEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the editing action in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, 
        /// then the edit action will be Canceled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets whether to clone data object during editing. Set the property when the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRowEditStart "/> event is triggered.
        /// </summary>
        /// <value>
        /// A Boolean value that indicates whether the data object should be cloned or not when editing begins.       
        /// <c>true</c>: A clone of the data object will not be created, and the original data object is used for editing.        
        /// <c>false</c>: A clone of the data object will be created and used for editing instead of the original data object.        
        /// The default value is false.
        /// </value>
        /// <remarks>
        /// If <c>PreventDataClone</c> is set to <c>true</c>, the edited data will be saved even if the user discards the changes using the Cancel button in the toolbar or dialog editing cancel button.
        /// </remarks>
        public bool PreventDataClone { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowEditing"/> event.
    /// </summary>
    public class RowEditingEventArgs<T> : RowEditedEventArgs<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the edit action in the grid. 
        /// </summary> 
        /// <value> 
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>,  
        /// then the edit action will be canceled. 
        /// </value> 
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowEdited"/> event.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class RowEditedEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the list of the primary key values.
        /// </summary>
        /// <value>
        /// A string array that contains the list of primary key values.
        /// </value>
        public string[]? PrimaryKeys { get; internal set; }

        /// <summary>
        /// Gets the primary key value of the <see cref="Syncfusion.Blazor.Grids.GridColumn"/>.
        /// </summary>
        /// <value>
        /// An object that defines the primary key value of the column when <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/> is true, otherwise null.
        /// </value>
        public object? PrimaryKeyValue { get; internal set; }

        /// <summary>
        /// Gets or sets the data of the row associated with editing. 
        /// </summary>
        /// <value>
        /// The data of the row associated with editing.
        /// </value>
        /// <remarks>
        /// By default, this property is cloned, which means the original data will be reverted even if the user discards
        /// the edited data using the Cancel button in the toolbar or dialog editing cancel button.
        /// To prevent this cloning, you can set the <c>PreventDataClone</c> argument of the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRowEditStart"/>
        /// event to true. This event is triggered before the current event.
        /// </remarks>
        public T? Data { get; set; }

        /// <summary>
        /// Gets the row index of the row associated with editing.
        /// </summary>
        /// <value>
        /// The row index of the editing row. By default, the value is 0.
        /// </value>
        /// <remarks>
        /// The value of this property can be used to identify which row in a collection or a data source is being edited.
        /// </remarks>
        public int Index { get; internal set; }

        /// <summary>
        /// Gets the current <see cref="EditContext"/> instance.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="EditContext"/> class that represents the current edit context. By default, the value is null.
        /// </value>
        public EditContext? EditContext { get; internal set; }

        /// <summary>
        /// Gets the foreign key column data of the data grid.
        /// </summary>
        /// <value>
        /// A dictionary that represents the foreign key column data. Each key represents a foreign key column name and the value represents the associated data as an enumerable object.
        /// </value>
        public IDictionary<string, IEnumerable<object>>? ForeignKeyData { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.Filtering"/> event.
    /// </summary>
    public class FilteringEventArgs : FilteredEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the filtering or clear filtering action in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, 
        /// then the filtering action or clear filtering action will be canceled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets whether to prevent the grid column’s default filter query during the API call.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// By default, when a filter is applied to a grid column, the grid sends a default filter request with the column name and filter value to the server. 
        /// In some cases, the default filter request may be too long and exceed URL length limitations, resulting in a long URI exception. This property provides an option to generate a custom filter query for a specific grid column and override the default filter request. 
        /// To utilize this property, set it to true within this event, and then override the <c>ProcessCustomFilterQuery</c> method in the adapter.   
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// @implements IDisposable
        /// <SfGrid AllowFiltering="true">
        /// <GridEvents TValue="Book" CheckboxFilterSearch="CheckboxFilterSearchHandler" Filtering="FilteringHandler"/>
        /// <GridForeignColumn @nameof(Book.CustomerId)>
        /// <SfDataManager @ref="DataManagerRef" Url="http://localhost:64956/odata/customers" Adaptor="Adaptors.ODataV4Adaptor"></SfDataManager>
        /// </GridForeignColumn>
        /// </SfGrid>
        /// @code{
        /// SfGrid<Order> Grid;
        /// public SfDataManager DataManagerRef { get; set; }
        /// public static Query CustomQuery = new Query();
        /// protected override void OnAfterRender(bool firstRender)
        /// {
        ///    if (firstRender)
        ///    {
        ///        DataManagerRef.DataAdaptor = new TestOData(DataManagerRef);
        ///    }
        ///    base.OnAfterRender(firstRender);
        /// }
        /// void IDisposable.Dispose()
        /// {        
        ///    CustomQuery = null;
        /// }
        /// public class TestOData : ODataV4Adaptor
        /// {
        ///    public TestOData(DataManager dm) : base(dm)
        ///    {
        ///    }
        ///    public override Query ProcessCustomFilterQuery(Query query)
        ///    {
        ///        return CustomQuery;
        ///    }
        /// }
        /// private void CheckboxFilterSearchHandler(CheckboxFilterSearchEventArgs args)
        /// {
        ///    if (args.SearchText != string.Empty)
        ///    {
        ///      args.SearchText = string.Empty;
        ///      args.CheckboxListData = new List<Book>() { new Book() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), CustomerId1 = Guid.NewGuid(), Active = false, CreditLimit = 20 } };
        ///    }
        ///  }
        /// private void FilteringHandler(Syncfusion.Blazor.Grids.FilteringEventArgs<Book> args)
        /// {
        ///   if (args.RequestType.Equals(Syncfusion.Blazor.Grids.Action.ClearFiltering))
        ///   {
        ///     CustomQuery = new Query();
        ///   }
        ///   if (args.RequestType == Syncfusion.Blazor.Grids.Action.Filtering)
        ///   {
        ///     if (String.Equals(args.ColumnName, null, StringComparison.OrdinalIgnoreCase) && String.Equals(args.FilterPredicate?.Field, "Name", StringComparison.OrdinalIgnoreCase))
        ///     {
        ///        CustomQuery = new Query();
        ///     }
        ///     if (String.Equals(args.ColumnName, nameof(Book.CustomerId), StringComparison.OrdinalIgnoreCase))
        ///     {
        ///       args.PreventFilterQuery = true;
        ///       List<WhereFilter> AndPredicate = new List<WhereFilter>();
        ///       if (args.FilterPredicates != null)
        ///       {
        ///         foreach (var col in args.FilterPredicates)
        ///         {
        ///           AndPredicate.Add(new WhereFilter() { Field = "Customer/Name", Operator = col.Operator.ToString().ToLower(), value = col.Value, Condition = col.Predicate });
        ///         }
        ///         if (AndPredicate[0].Condition == "and")
        ///         {
        ///           CustomQuery = new Query().Where(new WhereFilter() { Condition = "and", IsComplex = true, predicates = AndPredicate });
        ///         }
        ///         else
        ///         {
        ///           CustomQuery = new Query().Where(new WhereFilter() { Condition = "or", IsComplex = true, predicates = AndPredicate });
        ///         }
        ///        }
        ///        else if (args.FilterPredicates == null)
        ///        {
        ///           CustomQuery = new Query().Where("Customer/Name", args.CurrentFilterObject.Operator.ToString().ToLower(), args.CurrentFilterObject.Value, true, true);
        ///        }
        ///    }
        ///  }
        /// }
        /// ]]>
        /// </code>        
        /// </example> 
        public bool PreventFilterQuery { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.Filtered"/> event.
    /// </summary>
    public class FilteredEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the list of <see cref="PredicateModel{T}"/> objects containing filter predicate model details currently associated with filtering.
        /// </summary>
        /// <value>
        /// A list of <see cref="PredicateModel{T}"/> objects representing filter predicate model details currently associated with filtering.
        /// </value>
        /// <remarks>
        /// This property holds a collection of filter predicate values for a column currently undergoing filtering.
        /// - For filter types such as <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> or <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/>,
        ///   multiple predicate details are included from filtering using checkboxes and custom filters.
        /// - For filter types like <see cref="Syncfusion.Blazor.Grids.FilterType.Menu"/> or <see cref="Syncfusion.Blazor.Grids.FilterType.FilterBar"/>,
        ///   only a single filter predicate detail is present.
        /// In essence, this property contains the current filter predicate details when a single or multiple values are filtered for a column.
        /// If the filter for a column is removed, the <see cref="FilterPredicates"/> property becomes null.
        /// To clear or reset filtering from all columns in the grid, use the <see cref="SfGrid{TValue}.ClearFilteringAsync()"/> method,
        /// which results in both the <see cref="FilterPredicates"/> and <c>ColumnName</c> properties becoming null.
        /// </remarks>
        public List<PredicateModel<object>>? FilterPredicates { get; internal set; }
        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> name of the column that is currently associated with filtering action.
        /// </summary>
        /// <value>
        /// The field name of the column that is currently associated with filtering, otherwise the value is <c>null</c>.
        /// </value>
        public string? ColumnName { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.FilterDialogOpening"/> event.
    /// </summary>
    public class FilterDialogOpeningEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the filter dialog opening action in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, 
        /// then the filter dialog does not opened in the grid.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> name of the column which is associated with filtering.
        /// </summary>
        /// <value>
        /// The field name of the column which is associated with filtering. 
        /// </value>
        public string? ColumnName { get; internal set; }

        /// <summary>
        /// Gets or sets the custom data source for <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/> and <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> filter.
        /// </summary>
        /// <value>
        /// An IEnumerable collection of objects that serve as the custom data source for the Checkbox and Excel filter types in grid, By default the value is <c>null</c>
        /// </value>
        public IEnumerable<object>? CheckboxListData { get; set; }

        /// <summary>
        /// Gets or sets the number of items to be displayed in the filter popup for <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/> and <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> filters.
        /// </summary>
        /// <value>
        /// The number of items to be displayed in the filter popup. The default value is 0.
        /// </value>
        /// <remarks>
        /// If this property value is greater than 0, the filter popup will display the specified number of items. Otherwise, 1000 records will be displayed in the filter popup.
        /// </remarks>
        public int FilterChoiceCount { get; set; }

        /// <summary>
        /// Gets or sets the custom filter operators for <see cref="Syncfusion.Blazor.Grids.FilterType.Menu"/> filter .
        /// </summary>
        /// <value>
        /// A list of <see cref="IFilterOperator"/> that represent the custom filter operators. By default, the value is null.
        /// </value>   
        /// /// <example>
        /// <code><![CDATA[
        /// <SfGrid TValue="Order" AllowFiltering="true" AllowPaging="true" DataSource="@Orders">
        ///    <GridEvents FilterDialogOpening="FilterDialogOpeningHandler" TValue="Order"></GridEvents>
        ///   <GridFilterSettings Type="Syncfusion.Blazor.Grids.FilterType.Menu"></GridFilterSettings>
        ///    . . .
        ///</SfGrid>
        /// @code {
        ///    private SfGrid<Order> Grid;
        ///    public List<Order> Orders { get; set; }
        ///    public async Task FilterDialogOpeningHandler(FilterDialogOpeningEventArgs args)
        ///    {
        ///       if (args.ColumnName == "OrderDate")//Specify Field name
        ///       {
        ///            args.FilterOperators = CustomerIDOperator;
        ///        }
        ///    }
        ///    public class Operators: IFilterOperator
        ///    {
        ///        public Syncfusion.Blazor.Operator Value { get; set; }
        ///        public string? Text { get; set; }
        ///    }
        ///    List<IFilterOperator> CustomerIDOperator = new List<IFilterOperator> {
        ///    new Operators() { Text = "Equal", Value = Syncfusion.Blazor.Operator.Equal },
        ///    new Operators() { Text = "Contains", Value = Syncfusion.Blazor.Operator.Contains },
        ///    new Operators() { Text = "Greater/Equal(Between)", Value = Syncfusion.Blazor.Operator.GreaterThanOrEqual}
        ///    };                
        ///}
        ///]]>
        /// </code>
        /// </example>
        public List<IFilterOperator>? FilterOperators { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.FilterDialogOpened"/> event.
    /// </summary>
    public class FilterDialogOpenedEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> name of the column which is associated with filtering.
        /// </summary>
        /// <value>
        /// The field name of the column which is associated with filtering. 
        /// </value>
        public string? ColumnName { get; internal set; }

        /// <summary>
        /// Gets the custom data source for <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/> and <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> filter.
        /// </summary>
        /// <value>
        /// An IEnumerable collection of objects that serve as the custom data source for the Checkbox and Excel filter types in grid, By default the value is <c>null</c>
        /// </value>
        public IEnumerable<object>? CheckboxListData { get; internal set; }

        /// <summary>
        /// Gets the number of items to be displayed in the filter popup for <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/> and <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> filters.
        /// </summary>
        /// <value>
        /// The number of items to be displayed in the filter popup. The default value is 0.
        /// </value>
        /// <remarks>
        /// If this property value is greater than 0, the filter popup will display the specified number of items. Otherwise, 1000 records will be displayed in the filter popup.
        /// </remarks>
        public int FilterChoiceCount { get; internal set; }
    }


    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CheckboxFilterSearching"/> event.
    /// </summary>
    public class CheckboxFilterSearchingEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets or sets the custom data source for <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/> and <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> filter.
        /// </summary>
        /// <value>
        /// An IEnumerable collection of objects that serve as the custom data source for the Checkbox and Excel filter types in grid, by default the value is <c>null</c>
        /// </value>
        public IEnumerable<object>? CheckboxListData { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> name associated with the current column for filtering.
        /// </summary>
        /// <value>
        /// The field name associated with the current column for filtering. 
        /// for remaining actions the value will be <c>null</c>.
        /// </value>
        public string? ColumnName { get; internal set; }

        /// <summary>
        /// Gets or sets the string value to search in the search bar.
        /// </summary>
        /// <value>
        /// A string representing the value to search. Default value is an empty string.
        /// </value>
        public string SearchText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ColumnReordering"/> event.
    /// </summary>    
    public class ColumnReorderingEventArgs : ColumnReorderedEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel columns reordering action in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the Cancel property is set to <c>true</c>, 
        /// then the columns reordering action will be Canceled in the grid.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ColumnReordered"/> event.
    /// </summary>
    public class ColumnReorderedEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the list of the column associated with column reordering.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.Blazor.Grids.GridColumn"/> associated with column reordering.        
        /// </value>
        public List<GridColumn>? ReorderingColumns { get; internal set; }

        /// <summary>
        /// Gets the destination column for placing the reordered columns during the column reorder action in the grid.
        /// </summary>
        /// <value>
        /// The destination <see cref="Syncfusion.Blazor.Grids.GridColumn"/> where the reordered columns will be positioned in the grid after the reorder action.        
        /// </value>
        /// <remarks>
        /// When dragging a column towards the left direction, the reordered columns will be placed before the destination column.
        /// Conversely, when dragging a column towards the right direction, the reordered columns will be placed after the destination column.
        /// </remarks>        
        public GridColumn? ToColumn { get; internal set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ColumnVisibilityChanging"/> event.
    /// </summary>
    public class ColumnVisibilityChangingEventArgs : ColumnVisibilityChangedEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel a column visibility change in the grid.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>. If the <c>Cancel</c> property is set to <c>true</c>, 
        /// the column visibility change will be Canceled in the grid.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.ColumnVisibilityChanged"/> event.
    /// </summary>
    public class ColumnVisibilityChangedEventArgs : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the list of visible columns in the column chooser.
        /// </summary>
        /// <value>
        /// A list of <see cref="GridColumn"/> that represents the visible columns in the grid, By default the value is <c>null</c>.
        /// </value>
        public List<GridColumn>? VisibleColumns { get; internal set; }

        /// <summary>
        /// Gets the list of hidden <see cref="Syncfusion.Blazor.Grids.GridColumn"/> which is selected in column chooser.
        /// </summary>
        /// <value>
        /// A list of columns that represents the hidden columns using the column chooser.
        /// </value>
        public List<GridColumn>? HiddenColumns { get; internal set; }
    }


    /// <summary>
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnActionBegin"/> event when grid action start's.
    /// Also, provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnActionComplete"/> event when grid action completed.
    /// Provides information about the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnActionBegin"/> event when a grid action begins and the
    /// <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnActionComplete"/> event when a grid action is completed.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    public class ActionEventArgs<T> : GridEventBaseArgs
    {
        /// <summary>
        /// Gets the CUD (Create, Update, Delete) actions that can be performed when the edit mode is set to <see cref="Syncfusion.Blazor.Grids.EditMode.Normal"/> or <see cref="Syncfusion.Blazor.Grids.EditMode.Dialog"/>.
        /// </summary>
        /// <value>
        /// <para>
        /// The possible values for this property are:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <term>Add</term> <description>Indicates that a new record is being added.</description></item>
        /// <item>
        /// <term>Edit</term> <description>Indicates that an existing record is being edited and saved.</description></item>
        /// <item>
        /// <term>Delete</term> <description>Indicates that an existing record is being deleted.</description></item>
        /// </list>
        /// <para>
        /// The default value of this property is null.
        /// </para>
        /// </value>
        public string? Action { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the current action.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>, If the Cancel property is set to <c>true</c>, the current action will be cancelled.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets whether to clone data object during editing. Set the property when the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnActionBegin"/> event is triggered with <c>RequestType</c> as <c>BeforeBeginEdit</c>.
        /// </summary>
        /// <value>
        /// A boolean value that indicates whether the data object should be cloned or not when editing begins. The possible values for this property are:
        /// <list type="bullet">
        /// <item>
        /// <term>true</term><description>A clone of the data object will not be created and the original data object is used for editing.</description></item>
        /// <item>
        /// <term>false</term><description>A clone of the data object will be created and used for editing instead of the original data object.</description></item>
        /// </list>
        /// The default value is false.
        /// </value>
        /// <remarks>
        /// If <c>PreventDataClone</c> is set to <c>true</c>, the edited data will be saved even if the user discards the changes using the Cancel button in the toolbar.
        /// </remarks>
        public bool PreventDataClone { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> name associated with the current column based on the current actions like <c>Grouping</c>, <c>Sorting</c>, and <c>Filtering</c>.
        /// </summary>
        /// <value>
        /// The field name associated with the current column based on the current actions like <c>Grouping</c>, <c>Sorting</c>, and <c>Filtering</c>,
        /// for remaining actions the value will be <c>null</c>.
        /// </value>
        public string? ColumnName { get; set; }

        /// <summary>
        /// Gets the list of columns to be moved while the <see cref="Syncfusion.Blazor.Grids.Action.Reorder"/> action is performed.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.Blazor.Grids.GridColumn"/> to be moved in the grid when the columns are reordered.
        /// If the column is not reordered, then the value of this property will be null.
        /// </value>
        public List<GridColumn>? FromColumns { get; set; }

        /// <summary>
        /// Gets the destination columns to place the reordered columns while the <see cref="Syncfusion.Blazor.Grids.Action.Reorder"/> action is performed in the grid.
        /// </summary>
        /// <value>
        /// The destination <see cref="Syncfusion.Blazor.Grids.GridColumn"/> to place the reordered columns in the grid when the columns are reordered.
        /// If the column is not reordered, then the value of this property will be null.
        /// </value>
        public GridColumn? ToColumn { get; set; }

        /// <summary>
        /// Gets the <see cref="List{T}"/> of <see cref="PredicateModel{T}"/> of the filtered columns.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of <see cref="PredicateModel{T}"/> objects representing the filtered columns.
        /// When the filter type is <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> or <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/>, this property will contain the filtered columns. Otherwise, the value of this property will be null.
        /// </value>
        public List<PredicateModel<object>>? Columns { get; set; }

        /// <summary>
        /// Gets the <see cref="PredicateModel{T}"/> that is currently filtered.
        /// </summary>
        /// <value>
        /// When <c>AllowFiltering</c> is true then the value is <see cref="PredicateModel{T}"/> representing the current filter object, otherwise the value is <c>null</c>.
        /// </value>
        public PredicateModel<object>? CurrentFilterObject { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> name of the column that is currently being filtered.
        /// </summary>
        /// <value>
        /// The field name of the column that is currently being filtered, when <c>Filtering</c> action is perfomed, otherwise the value is <c>null</c>.
        /// </value>
        public string? CurrentFilteringColumn { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        /// <value>
        /// The current page number, The default value is 0.
        /// </value>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the data of the grid for the current actions like grouping, filtering, sorting etc...
        /// </summary>
        /// <value>
        /// A <typeparamref name="T"/> object representing the data of the grid.
        /// </value>
        public T? Data { get; set; }

        /// <summary>
        /// Gets the direction of the sorted column.
        /// </summary>
        /// <remarks>
        /// The available sort directions are:
        /// <list type="bullet">
        /// <item><description>SortDirection.Ascending: Default, sorts records in ascending order. </description></item>
        /// <item><description>SortDirection.Desending: Sorts records in descending order. </description></item>
        /// </list>
        /// </remarks>
        public SortDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the Excel search operator.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.Blazor.Operator"/> enum value that represents the selected search operator.
        /// </value>
        /// <remarks>
        /// Use this property to get or set the operator that will be used to filter data based on Excel-style search criteria. The available operators are specified by the <see cref="Syncfusion.Blazor.Operator"/> enum. 
        /// The default value is <see cref="Syncfusion.Blazor.Operator.None"/>.
        /// The available operators are:
        /// <list type="bullet">
        /// <item><description>None: No operator is selected. For example, if we set <see cref="Operator.None"/></description></item>
        /// <item><description>Contains: Checks whether the value contains the specified value. For example, if we set <see cref="Operator.Contains"/> and the search term is "an", then the item would contains "an", columns to be filtered.</description></item>
        /// <item><description>StartsWith: Checks whether the value begins with the specified value. For example, if we set <see cref="Operator.StartsWith"/> and the search term is "an", then the item would startswith "an", columns to be filtered.</description></item>
        /// <item><description>EndsWith: Checks whether the value ends with the specified value. For example, if we set <see cref="Operator.EndsWith"/> and the search term is "an", then the item would endswith "an", columns to be filtered.</description></item>
        /// <item><description>Equal: Checks whether the value is equal to the specified value. For example, if we set <see cref="Operator.Equal"/> and the search term is "an", then the item would equal to "an", columns to be filtered.</description></item>
        /// <item><description>NotEqual: Checks for values not equal to the specified value. For example, if we set <see cref="Operator.NotEqual"/> and the search term is "an", then the item would notequal to "an", columns to be filtered.</description></item>
        /// <item><description>GreaterThan: Checks whether the value is greater than the specified value. For example, if we set <see cref="Operator.GreaterThan"/> and the search term is "10", then the item would greaterthan to "10", columns to be filtered.</description></item>
        /// <item><description>GreaterThanOrEqual: Checks whether a value is greater than or equal to the specified value. For example, if we set <see cref="Operator.GreaterThanOrEqual"/> and the search term is "10", then the item would greaterthan or equal to "10", columns to be filtered.</description></item>
        /// <item><description>LesserThan: Checks whether the value is less than the specified value. For example, if we set <see cref="Operator.LessThan"/> and the search term is "10", then the item would lessthan to "10", columns to be filtered.</description></item>
        /// <item><description>LesserThanOrEqual: Checks whether the value is less than or equal to the specified value. For example, if we set <see cref="Operator.LessThanOrEqual"/> and the search term is "10", then the item would lessthan or equal to "10", columns to be filtered.</description></item>
        /// </list>
        /// </remarks>
        public Syncfusion.Blazor.Operator ExcelSearchOperator { get; set; } = Syncfusion.Blazor.Operator.None;

        /// <summary>
        /// Gets or sets the number of data to take while filtering.
        /// </summary>
        /// <value>
        /// The number of data to take while filtering, By default the value is 0.
        /// </value>
        public int FilterChoiceCount { get; set; }

        /// <summary>
        /// Gets the foreign key column data.
        /// </summary>
        /// <value>
        /// A dictionary that represents the foreign key column data. Each key represents a foreign key column name and the value represents the associated data as an enumerable object.
        /// </value>
        public IDictionary<string, IEnumerable<object>>? ForeignKeyData { get; internal set; }

        /// <summary>
        /// Gets or sets the custom filter operators.
        /// </summary>
        /// <value>
        /// A list of objects that represent the custom filter operators. By default, the value is null.
        /// </value>
        public List<object>? FilterOperators { get; set; }

        /// <summary>
        /// Gets the list of hidden <see cref="Syncfusion.Blazor.Grids.GridColumn"/> which is selected in column chooser.
        /// </summary>
        /// <value>
        /// A list of columns that represents the hidden columns using the column chooser.
        /// </value>
        public List<GridColumn>? HiddenColumns { get; set; }

        /// <summary>
        /// Gets or sets the index of the row to be added.
        /// </summary>
        /// <value>
        /// The index at which the row will be added to the grid. By default, the value is 0.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        /// Gets the previous data of the row.
        /// </summary>
        /// <value>
        /// An object of type T? that contains the previous data of the row. By default, the value is null.
        /// </value>
        public T? PreviousData { get; set; }

        /// <summary>
        /// Gets or sets the previous page number. The default value is 0.
        /// </summary>
        /// <value>
        /// The page number of the previous page.
        /// </value>
        public int PreviousPage { get; set; }

        /// <summary>
        /// Gets the primary key value of the <see cref="Syncfusion.Blazor.Grids.GridColumn"/>.
        /// </summary>
        /// <value>
        /// An object that defines the primary key value of the column when <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/> is true, otherwise null.
        /// </value>
        public object? PrimaryKeyValue { get; set; }

        /// <summary>
        /// Gets or sets the list of the primary key values.
        /// </summary>
        /// <value>
        /// A string array that contains the list of primary key values.
        /// </value>
        public string[]? PrimaryKeys { get; set; }

        /// <summary>
        ///  Gets the current <see cref="Syncfusion.Blazor.Grids.Action"/> in the grid like sorting, filtering, grouping, and etc.
        /// </summary>
        /// <value>The current action being performed in the grid.</value>
        /// <remarks>
        /// The request type assigned to the grid depends on the actions performed. The available request types include:
        /// <list type="bullet">
        /// <item><description>Add: When adding a new record in the normal or dialog edit mode enabled DataGrid.</description></item>
        /// <item><description>BeforeBeginEdit: Before the current record becomes editable state in the normal or dialog edit mode enabled DataGrid (occurs only in <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnActionBegin"/> event).</description></item>
        /// <item><description>BeginEdit: After the current record becomes editable state in the normal or dialog edit mode enabled DataGrid.</description></item>
        /// <item><description>Save: When saving a record in the normal or dialog edit mode enabled DataGrid.</description></item>
        /// <item><description>Delete: When deleting a record in the normal or dialog edit mode enabled DataGrid.</description></item>
        /// <item><description>Cancel: When canceling an edit operation in the normal or dialog edit mode enabled DataGrid.</description></item>
        /// <item><description>Filtering: When filtering data in the DataGrid.</description></item>
        /// <item><description>FilterBeforeOpen: Before opening the filter dialog for <see cref="Syncfusion.Blazor.Grids.FilterType.Menu"/>, <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> and <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/> filter types (occurs only in <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnActionBegin"/> event.).</description></item>
        /// <item><description>FilterChoiceRequest: While fetching data to render the filtering checkboxes in <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> or <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/> filter type.</description></item>
        /// <item><description>FilterAfterOpen: After a filter dialog is opened (occurs only in When a filter dialog is opened. It will assigned only in <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnActionComplete"/> event).</description></item>
        /// <item><description>Sorting: When sorting data in the DataGrid.</description></item>
        /// <item><description>Grouping: When grouping data in the DataGrid.</description></item>
        /// <item><description>UnGrouping: When ungrouping a column in the DataGrid.</description></item>
        /// <item><description>Paging: When navigating pages in the DataGrid.</description></item>
        /// <item><description>Reorder: When reordering a column in the DataGrid.</description></item>
        /// <item><description>RowDragAndDrop: When dragging and dropping rows in the DataGrid.</description></item>
        /// </list>
        /// </remarks>

        public Action RequestType { get; internal set; }

        /// <summary>
        /// Gets the string that identifies the physical key being pressed, while saving the edited record using Enter or Tab keys in <see cref="Syncfusion.Blazor.Grids.EditMode.Normal"/> edit mode.
        /// </summary>
        /// <value>        
        /// <c>Enter</c>, if the Enter key is used to save the edited record.
        /// <c>Tab</c>, if the Tab key is pressed from the last edited cell or the <c>'Shift-Tab'</c>, key is pressed from the first edited cell.
        /// By default, the value is set to null.
        /// </value> 
        /// <remarks>
        /// The value of this property is assigned while performing the save operation using Enter or Tab keyboard keys. In this case, <see cref="Syncfusion.Blazor.Grids.ActionEventArgs{T}.RequestType" /> will be 'Save', and the <see cref="Syncfusion.Blazor.Grids.ActionEventArgs{T}.Action" /> will be 'Edit'. If no save operation has been performed, the value of this property will be null.
        /// </remarks>
        public string? Code { get; set; }

        /// <summary>
        /// Gets the boolean value that identifies whether the `Shift-Tab` key was pressed to save the edited record in <see cref="Syncfusion.Blazor.Grids.EditMode.Normal"/> edit mode.
        /// </summary>
        /// <value>
        /// <b>true</b> if the `Shift-Tab` key is pressed from the first edited cell, otherwise <b>false</b>.
        /// </value>
        /// <remarks>
        /// Corresponding value has been assigned while performing the save operation using `Shift-Tab` keyboard keys, also <see cref="Syncfusion.Blazor.Grids.ActionEventArgs{T}.RequestType" /> will be 'Save' and the <see cref="Syncfusion.Blazor.Grids.ActionEventArgs{T}.Action" /> will be 'Edit'. Otherwise value is null.
        /// </remarks>
        public bool IsShiftKeyPressed { get; set; }

        /// <summary>
        /// Gets the data of the row.
        /// </summary>
        /// <value>
        /// The data of the row.
        /// </value>
        public T? RowData { get; set; }

        /// <summary>
        /// Gets the edited rowIndex.
        /// </summary>
        /// <value>
        /// The edited row index. By default, the value is 0.
        /// </value>
        /// <remarks>
        /// The value of this property can be used to identify which row in a collection or a data source has been edited.
        /// </remarks>
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets or sets the string value to search.
        /// </summary>
        /// <value>
        /// Accepts the string value.
        /// </value>
        public string? SearchString { get; set; }

        /// <summary>
        /// Gets or Sets whether to prevent the grid column’s default filter query during the API call.
        /// </summary>
        /// <value>
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// By default, when a filter is applied to a grid column, the grid sends a default filter request with the column name and filter value to the server. 
        /// In some cases, the default filter request may be too long and exceed URL length limitations, resulting in a long URI exception. This property provides an option to generate a custom filter query for a specific grid column and override the default filter request. 
        /// To use this property, set it to true in the <c>OnActionBegin</c> event with <c>RequestType</c> as <c>Filtering</c>, and override the <c>ProcessCustomFilterQuery</c> method in the adaptor. 
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// @implements IDisposable
        /// <SfGrid AllowFiltering="true">
        /// <GridEvents TValue="Book" OnActionBegin="OnActionBegin"/>
        /// <GridForeignColumn @nameof(Book.CustomerId)>
        /// <SfDataManager @ref="DataManagerRef" Url="http://localhost:64956/odata/customers" Adaptor="Adaptors.ODataV4Adaptor"></SfDataManager>
        /// </GridForeignColumn>
        /// </SfGrid>
        /// @code{
        /// SfGrid<Order> Grid;
        /// public SfDataManager DataManagerRef { get; set; }
        /// public static Query CustomQuery = new Query();
        /// protected override void OnAfterRender(bool firstRender)
        /// {
        ///    if (firstRender)
        ///    {
        ///        DataManagerRef.DataAdaptor = new TestOData(DataManagerRef);
        ///    }
        ///    base.OnAfterRender(firstRender);
        /// }
        /// void IDisposable.Dispose()
        /// {        
        ///    CustomQuery = null;
        /// }
        /// public class TestOData : ODataV4Adaptor
        /// {
        ///    public TestOData(DataManager dm) : base(dm)
        ///    {
        ///    }
        ///    public override Query ProcessCustomFilterQuery(Query query)
        ///    {
        ///        return CustomQuery;
        ///    }
        /// }
        /// private void OnActionBegin(Syncfusion.Blazor.Grids.ActionEventArgs<Book> args)
        /// {
        ///    if(args.RequestType == Syncfusion.Blazor.Grids.Action.FilterSearchBegin)
        ///    {
        ///       if(args.SearchString != string.Empty)
        ///       {
        ///           args.SearchString = string.Empty;
        ///           args.CheckboxListData = new List<Book>() { new Book() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), CustomerId1 = Guid.NewGuid(), Active = false, CreditLimit = 20 }};
        ///        }
        ///     }
        ///     if (args.RequestType.Equals(Syncfusion.Blazor.Grids.Action.ClearFiltering))
        ///     {
        ///        CustomQuery = new Query();
        ///     }
        ///     if (args.RequestType == Syncfusion.Blazor.Grids.Action.Filtering)
        ///     {
        ///        if (String.Equals(args.CurrentFilteringColumn, null, StringComparison.OrdinalIgnoreCase) && String.Equals(args.CurrentFilterObject?.Field, "Name", StringComparison.OrdinalIgnoreCase))
        ///        {
        ///            CustomQuery = new Query();
        ///        }
        ///        if (String.Equals(args.CurrentFilteringColumn, nameof(Book.CustomerId), StringComparison.OrdinalIgnoreCase))
        ///        {
        ///        args.PreventFilterQuery = true;
        ///        List<WhereFilter> AndPredicate = new List<WhereFilter>();
        ///        if (args.Columns != null)
        ///        {
        ///            foreach (var col in args.Columns)
        ///            {
        ///                AndPredicate.Add(new WhereFilter() { Field = "Customer/Name", Operator = col.Operator.ToString().ToLower(), value = col.Value, Condition = col.Predicate });
        ///            }
        ///            if (AndPredicate[0].Condition == "and")
        ///            {
        ///                CustomQuery = new Query().Where(new WhereFilter() { Condition = "and", IsComplex = true, predicates = AndPredicate });
        ///            }
        ///            else
        ///            {
        ///                CustomQuery = new Query().Where(new WhereFilter() { Condition = "or", IsComplex = true, predicates = AndPredicate });
        ///            }
        ///        }
        ///        else if (args.Columns == null)
        ///        {
        ///            CustomQuery = new Query().Where("Customer/Name", args.CurrentFilterObject.Operator.ToString().ToLower(), args.CurrentFilterObject.Value, true, true);
        ///        }
        ///        }
        ///    }
        /// }
        /// }
        /// ]]>
        /// </code>
        /// </example> 
        public bool PreventFilterQuery { get; set; }

        /// <summary>
        /// Gets the index of the currently selected row.
        /// </summary>
        /// <value>
        /// The index of the currently selected row. By default, the value is 0.
        /// </value>
        public int SelectedRow { get; set; }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        /// <value>
        /// A string value that represents the type of the event.
        /// </value>
        public string? Type { get; set; }

        /// <summary>
        /// Gets the list of visible columns in the column chooser.
        /// </summary>
        /// <value>
        /// A list of <see cref="GridColumn"/> that represents the visible columns in the grid, By default the value is <c>null</c>.
        /// </value>
        public List<GridColumn>? VisibleColumns { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="EditContext"/> instance.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="EditContext"/> class that represents the current edit context. By default, the value is null.
        /// </value>
        public EditContext? EditContext { get; set; }

        /// <summary>
        /// Gets or sets the custom data source for <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/> and <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> filter.
        /// </summary>
        /// <value>
        /// An IEnumerable collection of objects that serve as the custom data source for the CheckBox and Excel filter types in grid, By default the value is <c>null</c>
        /// </value>
        public IEnumerable<object>? CheckboxListData { get; set; }
    }

    /// <summary>
    /// Interface for editor customization.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Used as a marker for DI")]
    public interface IEditorSettings
    {
    }

    /// <summary>
    /// Defines edit params for in-build numerictextbox.
    /// </summary>
    public class NumericEditCellParams : IEditorSettings
    {
        /// <summary>
        /// Specifies the params of the numerictextbox editor.
        /// </summary>
        public NumericTextBoxModel<object>? Params { get; set; }

    }

    /// <summary>
    /// Defines edit params for in-built checkbox.
    /// </summary>
    public class BooleanEditCellParams : IEditorSettings
    {
        /// <summary>
        /// Specifies the params of the checkbox.
        /// </summary>
        public CheckBoxModel<bool>? Params { get; set; }
    }

    /// <summary>
    /// Defines edit params for in-built dropdownlist.
    /// </summary>
    public class DropDownEditCellParams : IEditorSettings
    {
        /// <summary>
        /// Specifies the params of the dropdownlist.
        /// </summary>
        public DropDownListModel<object, object>? Params { get; set; }
    }

    /// <summary>
    /// Defines edit params for in-built datepicker.
    /// </summary>
    public class DateEditCellParams : IEditorSettings
    {
        /// <summary>
        /// Specifies the params of the datepicker.
        /// </summary>
        public DatePickerModel? Params { get; set; }
    }
	
     /// <summary>
    /// Provides edit params for customizing the DateTimePicker component during cell editing.
    /// </summary>
    /// <typeparam name="T">
    /// The data type associated with the DateTimePicker model.
    /// The type parameter should be <see cref="DateTime"/>.
    /// </typeparam>
    /// <remarks>
    /// This class allows for the customization of the in-built DateTimePicker used when editing cells in a grid.
    /// This configuration applies specifically to columns with an edit type set to <c>DateTimePicker</c> as <see cref="GridColumn.EditType"/>.
	/// Applies the specified format when editing the cell.
    /// <list type="bullet">
    /// <item><description><c>CssClass</c> - Specifies custom CSS classes to apply to the component. </description></item> 
    /// <item><description><c>EnableRtl</c> - Enables right-to-left text direction. </description></item>
    /// <item><description><c>Placeholder</c> - Representing the placeholder text displayed in the input field</description></item>
    /// <item><description><c>TimeFormat</c> - Sets the format for displaying the time. </description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to configure the <c>DateTimeEditCellParams</c> class:
    /// <code>
    /// var dateTimeEditParams = new DateTimeEditCellParams
    /// {
    ///     Params = new DateTimePickerModel
    ///     {        
    ///         TimeFormat = "HH:mm",
    ///         
    ///     }
    /// };
    /// 
    /// </code>
    /// </example>

    public class DateTimeEditCellParams<T> : IEditorSettings
    {
        /// <summary>
        /// Gets or sets the parameters used to configure the DateTimePicker during cell editing.
        /// </summary>
        /// <value>
        /// An instance of <see cref="DateTimePickerModel{T}"/> that defines the configuration settings for the DateTimePicker.
        /// </value>
        /// <remarks>
        /// Use this property to set up various aspects of the DateTimePicker, such as date and time format, that will be applied when editing a cell set to <c>DateTimePicker</c> as <see cref="GridColumn.EditType"/>
        /// </remarks>
        public DateTimePickerModel<T>? Params { get; set; }
     }


    /// <summary>
    ///  A class which holds edit setting to customize <see cref="Syncfusion.Blazor.Calendars.SfTimePicker{TValue}"/> component while editing in grid. 
    /// </summary>
    public class TimeEditCellParams : IEditorSettings
    {
        /// <summary>
        /// Gets or sets the edit setting to customize in-built <see cref="Syncfusion.Blazor.Calendars.SfTimePicker{TValue}"/> component while editing in grid.
        /// </summary>
        /// <value>
        /// A <see cref="TimePickerModel{TValue}"/> object that specifies the edit parameters for the time picker.
        /// </value>
        /// <remarks>
        /// This property allows to customize the time picker used in the edit form of the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}"/> component.
        /// </remarks>
        public TimePickerModel<object>? Params { get; set; }
    }

    /// <summary>
    /// Defines edit params for in-built textbox.
    /// </summary>
    public class StringEditCellParams : IEditorSettings
    {
        /// <summary>
        /// Specifies the params of the textbox.
        /// </summary>
        public TextBoxModel? Params { get; set; }
    }

    /// <summary>
    /// Defines model for Dialog.
    /// </summary>
    public class DialogSettings
    {
        /// <summary>
        /// Specifies the height of the Dialog.
        /// </summary>
        public string? Height { get; set; }

        /// <summary>
        /// Specifies the width of the Dialog.
        /// </summary>
        public string? Width { get; set; }

        /// <summary>
        /// Specifies the minheight of the Dialog.
        /// </summary>
        public string? MinHeight { get; set; }

        /// <summary>
        /// Specifies the value whether the dialog component can be dragged by the end-user.
        /// The dialog allows a user to drag by selecting the header and dragging it for re-positioning the dialog.
        /// </summary>
        public bool? AllowDragging { get; set; }

        /// <summary>
        /// Specifies the value that represents whether the close icon can be shown in the dialog’s title section.
        /// </summary>
        public bool? ShowCloseIcon { get; set; }

        /// <summary>
        /// Specifies the Boolean value whether the dialog can be closed on pressing the escape (ESC) key
        /// that is used to control the dialog's closing behavior.
        /// </summary>
        public bool? CloseOnEscape { get; set; }

        /// <summary>
        /// Specifies the value whether the dialog component can be resized by the end-user.
        /// If the EnableResize is true, the dialog component creates a grip to resize it in a diagonal direction.
        /// </summary>
        public bool? EnableResize { get; set; }

        /// <summary>
        /// Specifies the CSS class name that can be appended with the root element of the dialog.
        /// One or more custom CSS classes can be added to a dialog.
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Specifies the target element in which the dialog should be displayed.
        /// The default value is null, which refers to the `Document.body` element.
        /// </summary>
        public string? Target { get; set; }

        /// <summary>
        /// Specifies the offset left value to position the dialog.
        /// </summary>
        public string? XValue { get; set; }

        /// <summary>
        /// Specifies the offset top value to position the dialog.
        /// </summary>
        public string? YValue { get; set; }

        /// <summary>
        /// Specifies the delay in milliseconds to start the animation.
        /// </summary>
        public double AnimationDelay { get; set; }

        /// <summary>
        /// Specifies the duration in milliseconds that the animation takes to open or close the dialog.
        /// </summary>
        public double AnimationDuration { get; set; } = 400;

        /// <summary>
        /// Gets or sets the z-index value that determines the stack order of the edit dialog displayed by the Grid when using dialog editing mode.
        /// </summary>
        /// <value>
        /// Specifies an integer z-index value. The default is <c>1001</c>. Setting a higher value ensures the add or edit dialog appears above other UI elements with lower z-index values.
        /// </value>
        /// <remarks>
        /// <para>This property is only applicable when the Grid's <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/>
        /// is set to <see cref="Syncfusion.Blazor.Grids.EditMode.Dialog"/>.</para>
        /// <para>A higher <c>ZIndex</c> can help avoid dialog overlap issues with other layered components or third-party UI elements.</para>
        /// <para><b>Accessibility:</b> Ensure that modifying the visual stack order does not block important interactive elements or compromise keyboard navigation for users relying on assistive technologies.</para>
        /// <para>Changing this property at runtime will dynamically affect the dialog's rendered stacking order.</para>
        /// </remarks>
        public int ZIndex { get; set; } = 1001;

        /// <summary>
        /// Specifies the animation name that should be applied on while opening and closing the dialog.
        /// If the user sets Fade animation, the dialog will open with the `FadeIn` effect and close with the `FadeOut` effect.
        /// The following are the list of animation effects available to configure to the dialog:
        /// 1. Fade
        /// 2. FadeZoom
        /// 3. FlipLeftDown
        /// 4. FlipLeftUp
        /// 5. FlipRightDown
        /// 6. FlipRightUp
        /// 7. FlipXDown
        /// 8. FlipXUp
        /// 9. FlipYLeft
        /// 10. FlipYRight
        /// 11. SlideBottom
        /// 12. SlideLeft
        /// 13. SlideRight
        /// 14. SlideTop
        /// 15. Zoom
        /// 16. None.
        /// </summary>
        public DialogEffect? AnimationEffect { get; set; }
    }

    /// <summary>
    /// Defines the batch changes.
    /// </summary>
    public class BatchChanges<T>
    {
        /// <summary>
        /// Specifies the collection that contains changed records.
        /// </summary>
        public List<T> ChangedRecords { get; set; } = new List<T>();

        /// <summary>
        /// Specifies the collection that contains deleted records.
        /// </summary>
        public List<T> DeletedRecords { get; set; } = new List<T>();

        /// <summary>
        /// Specifies the collection that contains added records.
        /// </summary>
        public List<T> AddedRecords { get; set; } = new List<T>();
    }

    /// <summary>
    /// Class that defines argument of the Autofill operation.
    /// </summary>
    /// <typeparam name="T">TValue of the grid component.</typeparam>
    internal class Autofill<T>
    {
        /// <summary>
        /// Defines the Row selection.
        /// </summary>
        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; }

        /// <summary>
        /// Defines the AutofillBox selection.
        /// </summary>
        [JsonPropertyName("isBoxSelected")]
        public bool ISBoxSelected { get; set; }

        /// <summary>
        /// Defines the AutofillBorder selection.
        /// </summary>
        [JsonPropertyName("isBorderSelected")]
        public bool ISBorderSelected { get; set; }

        /// <summary>
        /// Defines the AutofillBorder position selection.
        /// </summary>
        [JsonPropertyName("isBorderPosSelected")]
        public bool IsBorderPositionSelected { get; set; }

        /// <summary>
        /// Defines the styles of Border's Right alignment.
        /// </summary>
        [JsonPropertyName("borderRight")]
        public string? BorderRight { get; set; }

        /// <summary>
        /// Defines the styles of Border's border-Width alignment.
        /// </summary>
        [JsonPropertyName("border-Width")]
        public string? BordersWidth { get; set; }

        /// <summary>
        /// Defines the styles of border's Height alignment.
        /// </summary>
        [JsonPropertyName("borderHeight")]
        public string? BorderHeight { get; set; }

        /// <summary>
        /// Defines the styles of border's Top alignment.
        /// </summary>
        [JsonPropertyName("borderTop")]
        public string? BorderTop { get; set; }

        /// <summary>
        /// Defines the styles border's  width alignment.
        /// </summary>
        [JsonPropertyName("borderWidth")]
        public string? BorderWidth { get; set; }

        /// <summary>
        /// Defines the styles border's left alignment..
        /// </summary>
        [JsonPropertyName("borderLeft")]
        public string? BorderLeft { get; set; }

        /// <summary>
        /// Defines the styles Box display.
        /// </summary>
        [JsonPropertyName("autofillBoxDisplay")]
        public string? AutofillBoxDisplay { get; set; }

        /// <summary>
        /// Defines the styles border display.
        /// </summary>
        [JsonPropertyName("autofillBorderDisplay")]
        public string? AutofillBorderDisplay { get; set; }

        /// <summary>
        /// Defines the styles of border position display.
        /// </summary>
        [JsonPropertyName("autofillDisplay")]
        public string? AutofillDisplay { get; set; }

        /// <summary>
        /// Defines the styles of Autofill's Right alignment.
        /// </summary>
        [JsonPropertyName("autofillRight")]
        public string? AutofillRight { get; set; }

        /// <summary>
        /// Defines the styles of Autofill's left alignment.
        /// </summary>
        [JsonPropertyName("autofillLeft")]
        public string? AutofillLeft { get; set; }

        /// <summary>
        /// Defines the styles of Autofill's Top alignment.
        /// </summary>
        [JsonPropertyName("autofillTop")]
        public string? AutofillTop { get; set; }

        /// <summary>
        /// Defines the styles of border's Left Left alignment.
        /// </summary>
        [JsonPropertyName("borderLeftAutofillLeft")]
        public string? BorderLeftAutofillLeft { get; set; }

        /// <summary>
        /// Defines the styles of border's Left Top alignment.
        /// </summary>
        [JsonPropertyName("borderLeftAutofillTop")]
        public string? BorderLeftAutofillTop { get; set; }

        /// <summary>
        /// Defines the styles of border's Left Right alignment.
        /// </summary>
        [JsonPropertyName("borderLeftAutofillRight")]
        public string? BorderLeftAutofillRight { get; set; }

        /// <summary>
        /// Defines the styles of border's Left Height alignment.
        /// </summary>
        [JsonPropertyName("borderLeftAutofillHeight")]
        public string? BorderLeftAutofillHeight { get; set; }

        /// <summary>
        /// Defines the styles of border's Left Width alignment.
        /// </summary>
        [JsonPropertyName("borderLeftAutofillWidth")]
        public string? BorderLeftAutofillWidth { get; set; }

        /// <summary>
        /// Defines the styles of border's Right Left alignment.
        /// </summary>
        [JsonPropertyName("borderRightAutofillLeft")]
        public string? BorderRightAutofillLeft { get; set; }

        /// <summary>
        /// Defines the styles of border's Right Top alignment.
        /// </summary>
        [JsonPropertyName("borderRightAutofillTop")]
        public string? BorderRightAutofillTop { get; set; }

        /// <summary>
        /// Defines the styles of border's Right Right alignment.
        /// </summary>
        [JsonPropertyName("borderRightAutofillRight")]
        public string? BorderRightAutofillRight { get; set; }

        /// <summary>
        /// Defines the styles of border's Right Height alignment.
        /// </summary>
        [JsonPropertyName("borderRightAutofillHeight")]
        public string? BorderRightAutofillHeight { get; set; }

        /// <summary>
        /// Defines the styles of border's Right Width alignment.
        /// </summary>
        [JsonPropertyName("borderRightAutofillWidth")]
        public string? BorderRightAutofillWidth { get; set; }

        /// <summary>
        /// Defines the styles of border's Top Left alignment.
        /// </summary>
        [JsonPropertyName("borderTopAutofillLeft")]
        public string? BorderTopAutofillLeft { get; set; }

        /// <summary>
        /// Defines the styles of border's Top Top alignment.
        /// </summary>
        [JsonPropertyName("borderTopAutofillTop")]
        public string? BorderTopAutofillTop { get; set; }

        /// <summary>
        /// Defines the styles of border's Top Right alignment.
        /// </summary>
        [JsonPropertyName("borderTopAutofillRight")]
        public string? BorderTopAutofillRight { get; set; }

        /// <summary>
        /// Defines the styles of border's Top Height alignment.
        /// </summary>
        [JsonPropertyName("borderTopAutofillHeight")]
        public string? BorderTopAutofillHeight { get; set; }

        /// <summary>
        /// Defines the styles of border's Top Width alignment.
        /// </summary>
        [JsonPropertyName("borderTopAutofillWidth")]
        public string? BorderTopAutofillWidth { get; set; }

        /// <summary>
        /// Defines the styles of border's Bottom Left alignment.
        /// </summary>
        [JsonPropertyName("borderBottomAutofillLeft")]
        public string? BorderBottomAutofillLeft { get; set; }

        /// <summary>
        /// Defines the styles of border's Bottom Top alignment.
        /// </summary>
        [JsonPropertyName("borderBottomAutofillTop")]
        public string? BorderBottomAutofillTop { get; set; }

        /// <summary>
        /// Defines the styles of border's Bottom Right alignment.
        /// </summary>
        [JsonPropertyName("borderBottomAutofillRight")]
        public string? BorderBottomAutofillRight { get; set; }

        /// <summary>
        /// Defines the styles of border's Bottom Height alignment.
        /// </summary>
        [JsonPropertyName("borderBottomAutofillHeight")]
        public string? BorderBottomAutofillHeight { get; set; }

        /// <summary>
        /// Defines the styles of border's Bottom Width alignment.
        /// </summary>
        [JsonPropertyName("borderBottomAutofillWidth")]
        public string? BorderBottomAutofillWidth { get; set; }
    }

    /// <summary>
    /// Base type for all Grid event argument classes. Provides shared infrastructure used by the Grid during event
    /// callbacks, including a reference to the owning grid and a switch to suppress re-rendering.
    /// </summary>
    public class GridEventBaseArgs
    {
        internal IGrid? Parent;
        private bool _preventRender;

        /// <summary>
        /// Setting true will override the ShouldRender method of grid to return false.
        /// </summary>
        public bool PreventRender
        {
            get
            {
                return _preventRender;
            }

            set
            {
                _preventRender = value;
                Parent?.PreventRender(_preventRender);
            }
        }
    }

    /// <summary>
    /// Defines members of validator template context object.
    /// </summary>
    public class ValidatorTemplateContext
    {
        /// <summary>
        /// Defines the current edited data.
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Holds EditContext instance of the EditForm used by grid.
        /// </summary>
        public EditContext? EditContext { get; set; }

        /// <summary>
        /// Holds delegate instance to show hide validation message on the given editor.
        /// </summary>
        /// <remarks>
        /// Accepts argument such as Field name(string), valid state(bool) and message(string) to show/hide
        /// popup validation message on the editor.
        /// </remarks>
        public Action<string, bool, string>? ShowValidationMessage { get; set; }
    }

    /// <summary>
    /// Defines the Sort Column.
    /// </summary>
    public struct SortColumn : IEquatable<SortColumn>
    {
        /// <summary>
        /// Specifies the field of the column to be sorted.
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Specifies the sort direction.
        /// </summary>
        public SortDirection Direction { get; set; }

        /// <summary>
        /// Compares the specified instance and the current instance of RemoteOptions
        ///     for value equality.
        /// </summary>
        /// <param name="obj">The instance to compare.</param>
        /// <returns>true.</returns>
        public override bool Equals(object? obj)
        {
            return true;
        }
        /// <summary>
        /// Compares the specified instance and the current instance of RemoteOptions
        ///     for value equality.
        /// </summary>
        /// <param name="other">The instance to compare.</param>
        /// <returns>true.</returns>
        public bool Equals(SortColumn other) => true;

        /// <summary>
        /// Returns the hash code.
        /// </summary>
        /// <returns>int.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Handles equal
        /// </summary>
        /// <param name="point1">argument one</param>
        /// <param name="point2">argument two</param>
        /// <returns>bool</returns>
        public static bool operator ==(SortColumn point1, SortColumn point2)
        {
            return point1.Equals(point2);
        }
        /// <summary>
        /// Handles unequal
        /// </summary>
        /// <param name="point1">argument one</param>
        /// <param name="point2">argument two</param>
        /// <returns>bool</returns>
        public static bool operator !=(SortColumn point1, SortColumn point2)
        {
            return !point1.Equals(point2);
        }
    }
    /// <summary>
    /// Interface for defining filter parameters used to customize filter components in both the Filter Menu dialog and Excel-like filter dialog of the Grid.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Used as a marker for DI")]
    public interface IFilterSettings
    {
    }


    /// <summary>
    /// Provides filter settings to customize the <see cref="Syncfusion.Blazor.Inputs.SfNumericTextBox{TValue}"/>
    /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
    /// </summary>
    public class NumericFilterParams : IFilterSettings
    {
        /// <summary>
        /// Gets or sets the configuration settings for the built-in <see cref="Syncfusion.Blazor.Inputs.SfNumericTextBox{TValue}"/>
        /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
        /// </summary>
        /// <value>
        /// A <see cref="NumericTextBoxModel{TValue}"/> object defining customization options like placeholder, decimals, clear button, CSS class for styling, read-only mode, and RTL support.
        /// </value>
        /// <remarks>
        /// <para>This property applies only when the <see cref="Syncfusion.Blazor.Grids.ColumnType"/> is one of the following: <see cref="Syncfusion.Blazor.Grids.ColumnType.Integer"/>, <see cref="Syncfusion.Blazor.Grids.ColumnType.Double"/>, <see cref="Syncfusion.Blazor.Grids.ColumnType.Decimal"/>, or <see cref="Syncfusion.Blazor.Grids.ColumnType.Long"/>.</para>
        /// </remarks>
        public NumericTextBoxModel<object>? NumericTextBoxParams { get; set; }
    }




    /// <summary>
    /// Provides filter settings to customize the <see cref="Syncfusion.Blazor.Calendars.SfDatePicker{TValue}"/>
    /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
    /// </summary>
    public class DateFilterParams : IFilterSettings
    {
        /// <summary>
        /// Gets or sets the filter settings to customize the built-in <see cref="Syncfusion.Blazor.Calendars.SfDatePicker{TValue}"/>
        /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
        /// </summary>
        /// <value>
        /// A <see cref="DatePickerModel"/> object defining customization options such as format, min/max dates, placeholders, and styling.
        /// </value>
        /// <remarks>
        /// <para>This property applies when the <see cref="Syncfusion.Blazor.Grids.ColumnType"/> is either <see cref="Syncfusion.Blazor.Grids.ColumnType.Date"/> or <see cref="Syncfusion.Blazor.Grids.ColumnType.DateOnly"/>.</para>
        /// </remarks>
        public DatePickerModel? DatePickerParams { get; set; }
    }




    /// <summary>
    /// Provides filter settings to customize the <see cref="Syncfusion.Blazor.Calendars.SfTimePicker{TValue}"/>
    /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
    /// </summary>
    public class TimeFilterParams : IFilterSettings
    {
        /// <summary>
        /// Gets or sets the filter settings to customize the built-in <see cref="Syncfusion.Blazor.Calendars.SfTimePicker{TValue}"/>
        /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
        /// </summary>
        /// <value>
        /// A <see cref="TimePickerModel{TValue}"/> object defining customization options such as time format,
        /// step intervals, min/max bounds, clear button visibility, read-only state, RTL layout support, component enable state,
        /// CSS class for styling, and additional HTML attributes.
        /// </value>
        /// <remarks>
        /// <para>This property applies when the <see cref="Syncfusion.Blazor.Grids.ColumnType"/> is <see cref="Syncfusion.Blazor.Grids.ColumnType.TimeOnly"/>.</para>
        /// </remarks>
        public TimePickerModel<object>? TimePickerParams { get; set; }
    }



    /// <summary>
    /// Provides filter settings to customize the <see cref="Syncfusion.Blazor.DropDowns.SfDropDownList{TValue, TItem}"/>
    /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
    /// </summary>
    public class DropDownFilterParams : IFilterSettings
    {
        /// <summary>
        /// Gets or sets the filter settings to customize the built-in <see cref="Syncfusion.Blazor.DropDowns.SfDropDownList{TValue, TItem}"/>
        /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
        /// </summary>
        /// <value>
        /// A <see cref="DropDownListModel{TValue, TItem}"/> object defining customization options such as placeholder, debounce delay, and styling.
        /// </value>
        /// <remarks>
        /// <para>This property applies when the <see cref="Syncfusion.Blazor.Grids.ColumnType"/> is <see cref="Syncfusion.Blazor.Grids.ColumnType.Boolean"/>.</para>
        /// </remarks>
        public DropDownListModel<object, object>? DropDownListParams { get; set; }
    }



    /// <summary>
    /// Provides filter settings to customize the <see cref="Syncfusion.Blazor.Calendars.SfDateTimePicker{TValue}"/>
    /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
    /// </summary>
    public class DateTimeFilterParams : IFilterSettings
    {
        /// <summary>
        /// Gets or sets the filter settings to customize the built-in <see cref="Syncfusion.Blazor.Calendars.SfDateTimePicker{TValue}"/>
        /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
        /// </summary>
        /// <value>
        /// A <see cref="DateTimePickerModel{TValue}"/> object that defines customization options for the date-time picker filter component,
        /// such as placeholder text, styling via CSS class, clear button visibility, and read-only mode.
        /// </value>
        /// <remarks>
        /// <para>This property is applicable only when the <see cref="Syncfusion.Blazor.Grids.ColumnType"/> is <see cref="Syncfusion.Blazor.Grids.ColumnType.DateTime"/>.</para>
        /// </remarks>
        public DateTimePickerModel<object>? DateTimePickerParams { get; set; }
    }




    /// <summary>
    /// Provides filter settings to customize the <see cref="Syncfusion.Blazor.DropDowns.SfAutoComplete{TValue, TItem}"/>
    /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
    /// </summary>
    public class AutoCompleteFilterParams : IFilterSettings
    {
        /// <summary>
        /// Gets or sets the configuration settings for the built-in <see cref="Syncfusion.Blazor.DropDowns.SfAutoComplete{TValue, TItem}"/>
        /// component used in the filter menu and the Excel-like filter dialogs of the Grid.
        /// </summary>
        /// <value>
        /// An <see cref="AutoCompleteModel"/> object defining customization options such as minimum input length, clear and popup buttons, debounce delay, suggestion count, auto-fill, highlight behavior, and more.
        /// </value>
        /// <remarks>
        /// <para>This property is applicable only when the <see cref="Syncfusion.Blazor.Grids.ColumnType"/> is <see cref="Syncfusion.Blazor.Grids.ColumnType.String"/>.</para>
        /// </remarks>
        public AutoCompleteModel? AutoCompleteParams { get; set; }
    }
    /// <summary>
    /// Represents a merge definition for a rectangular region of cells in <see cref="SfGrid{TValue}"/>.
    /// The region is anchored at the specified top-left cell and extends by the provided row and column spans within the current view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Indices are zero-based and refer to the current view only (for example, current page, virtual block, or group segment).
    /// The anchor cell is the top-left cell of the merged region; all covered cells are suppressed from rendering and focus.
    /// </para>
    /// <para>
    /// Constraints:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="RowIndex"/> and <see cref="ColumnIndex"/> must point to a visible data row and a visible leaf data column.</description></item>
    ///   <item><description><see cref="RowSpan"/> and <see cref="ColumnSpan"/> must be greater than or equal to <c>1</c>.</description></item>
    ///   <item><description>Merges are computed per view and do not cross view boundaries or include hidden rows/columns.</description></item>
    /// </list>
    /// <para>
    /// Usage:
    /// Create one or more <see cref="MergeCellInfo"/> instances and pass them to <c>SfGrid&lt;TValue&gt;.MergeCells(...)</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following example merges a 2x3 region starting at row 1, column 1 in the current view,
    /// and demonstrates batching multiple merge requests.
    /// <code><![CDATA[
    /// // Single merge
    /// grid.MergeCells(new MergeCellInfo
    /// {
    ///     RowIndex = 1,
    ///     ColumnIndex = 1,
    ///     RowSpan = 2,
    ///     ColumnSpan = 3
    /// });
    ///
    /// // Batch merge
    /// grid.MergeCells(new[]
    /// {
    ///     new MergeCellInfo { RowIndex = 0, ColumnIndex = 0, RowSpan = 2, ColumnSpan = 1 },
    ///     new MergeCellInfo { RowIndex = 5, ColumnIndex = 2, RowSpan = 1, ColumnSpan = 2 }
    /// });
    /// ]]></code>
    /// </example>
    public sealed class MergeCellInfo
    {
        /// <summary>
        /// Gets or sets the zero-based row index of the anchor cell within the current view (data rows only).
        /// </summary>
        /// <value>
        /// An <see cref="int"/> representing the data row index in the current view.
        /// The default value is <c>0</c>.
        /// </value>
        /// <remarks>
        /// Must be greater than or equal to <c>0</c> and within the bounds of the current view.
        /// </remarks>
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets or sets the zero-based column index of the anchor cell among visible leaf data columns.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> representing the visible leaf column index in the current view.
        /// The default value is <c>0</c>.
        /// </value>
        /// <remarks>
        /// Must reference a visible leaf data column; non-leaf or hidden columns are not valid merge anchors.
        /// </remarks>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets the number of rows to include in the merge starting at <see cref="RowIndex"/>.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> indicating the vertical span of the merged region.
        /// The default value is <c>1</c>.
        /// </value>
        /// <remarks>
        /// Must be greater than or equal to <c>1</c>. Values larger than the remaining rows in the current view are truncated to fit.
        /// </remarks>
        public int RowSpan { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of columns to include in the merge starting at <see cref="ColumnIndex"/>.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> indicating the horizontal span of the merged region.
        /// The default value is <c>1</c>.
        /// </value>
        /// <remarks>
        /// Must be greater than or equal to <c>1</c>. Values larger than the remaining visible leaf columns in the current view are truncated to fit.
        /// </remarks>
        public int ColumnSpan { get; set; } = 1;
    }

    /// <summary>
    /// Represents an unmerge request that targets a merged region in <see cref="SfGrid{TValue}"/> by its anchor cell.
    /// The anchor cell is the top-left cell of the merged region within the current view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Indices are zero-based and refer to the current view only (for example, current page, virtual block, or group segment).
    /// The coordinates must identify the anchor of an existing merged region; non-anchor coordinates result in a no-op.
    /// </para>
    /// <para>
    /// Typical usage: pass one or more instances to <c>SfGrid&lt;TValue&gt;.UnmergeCells(...)</c> to remove merged regions.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following example removes a merged region anchored at row 2, column 1.
    /// <code><![CDATA[
    /// grid.UnmergeCells(new UnmergeCellInfo
    /// {
    ///     RowIndex = 2,
    ///     ColumnIndex = 1
    /// });
    /// ]]></code>
    /// </example>
    public sealed class UnmergeCellInfo
    {
        /// <summary>
        /// Gets or sets the zero-based row index of the anchor cell in the current view (data rows only).
        /// </summary>
        /// <value>
        /// An <see cref="int"/> representing the data row index in the current view.
        /// The default value is <c>0</c>.
        /// </value>
        /// <remarks>
        /// Must be greater than or equal to <c>0</c> and within the bounds of the current view.
        /// </remarks>
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets or sets the zero-based column index of the anchor cell among visible leaf data columns.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> representing the visible leaf column index in the current view.
        /// The default value is <c>0</c>.
        /// </value>
        /// <remarks>
        /// Must reference a visible leaf data column; non-leaf or hidden columns are not valid anchors.
        /// </remarks>
        public int ColumnIndex { get; set; }
    }
}
