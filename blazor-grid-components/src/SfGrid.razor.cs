using System;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Collections.ObjectModel;



#region Syncfusion
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Internal;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Spinner;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Grids.Internal;
using System.Runtime.Remoting;
#if SyncfusionLicense
using Syncfusion.Licensing;
#endif
#endregion

[assembly: InternalsVisibleTo("Syncfuison.Blazor.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001002382fcb1069523ce72d849497a557a445c151eaf4007aa79adef551a8204ca7f728e5378607d85695b16f129ec35bf4af15dcf6d3581deb8bb0debb239c33e7f1271a37c7f60f1044ae417730f5082abee5f9ec568a8a4cef04074394755706376e982dc6f9d15430faaad385ae8f00a77ef1c97517f1a1517004ce78028b9ce")]
[assembly: InternalsVisibleTo("Syncfusion.Blazor.PivotView, PublicKey=00240000048000009400000006020000002400005253413100040000010001002382fcb1069523ce72d849497a557a445c151eaf4007aa79adef551a8204ca7f728e5378607d85695b16f129ec35bf4af15dcf6d3581deb8bb0debb239c33e7f1271a37c7f60f1044ae417730f5082abee5f9ec568a8a4cef04074394755706376e982dc6f9d15430faaad385ae8f00a77ef1c97517f1a1517004ce78028b9ce")]
[assembly: InternalsVisibleTo("Syncfusion.Blazor.Gantt, PublicKey=00240000048000009400000006020000002400005253413100040000010001002382fcb1069523ce72d849497a557a445c151eaf4007aa79adef551a8204ca7f728e5378607d85695b16f129ec35bf4af15dcf6d3581deb8bb0debb239c33e7f1271a37c7f60f1044ae417730f5082abee5f9ec568a8a4cef04074394755706376e982dc6f9d15430faaad385ae8f00a77ef1c97517f1a1517004ce78028b9ce")]
[assembly: InternalsVisibleTo("Syncfusion.Blazor.MultiColumnComboBox, PublicKey=00240000048000009400000006020000002400005253413100040000010001002382fcb1069523ce72d849497a557a445c151eaf4007aa79adef551a8204ca7f728e5378607d85695b16f129ec35bf4af15dcf6d3581deb8bb0debb239c33e7f1271a37c7f60f1044ae417730f5082abee5f9ec568a8a4cef04074394755706376e982dc6f9d15430faaad385ae8f00a77ef1c97517f1a1517004ce78028b9ce")]
[assembly: InternalsVisibleTo("Syncfusion.Blazor.TreeGrid, PublicKey=00240000048000009400000006020000002400005253413100040000010001002382fcb1069523ce72d849497a557a445c151eaf4007aa79adef551a8204ca7f728e5378607d85695b16f129ec35bf4af15dcf6d3581deb8bb0debb239c33e7f1271a37c7f60f1044ae417730f5082abee5f9ec568a8a4cef04074394755706376e982dc6f9d15430faaad385ae8f00a77ef1c97517f1a1517004ce78028b9ce")]
[assembly: InternalsVisibleTo("Syncfusion.Blazor.FileManager, PublicKey=00240000048000009400000006020000002400005253413100040000010001002382fcb1069523ce72d849497a557a445c151eaf4007aa79adef551a8204ca7f728e5378607d85695b16f129ec35bf4af15dcf6d3581deb8bb0debb239c33e7f1271a37c7f60f1044ae417730f5082abee5f9ec568a8a4cef04074394755706376e982dc6f9d15430faaad385ae8f00a77ef1c97517f1a1517004ce78028b9ce")]

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Blazor Grid component displays tabular data and it has in-built support for various data binding, editing,
    /// sorting and filtering.
    /// </summary>
    /// <typeparam name="TValue">A type which provides schema for the grid component.
    /// </typeparam>
    /// <remarks><c>TValue</c> is inferred from value of <c>DataSource</c> property if it is bounded with IEnumerable.
    /// If data is consumed using <c>SfDataManager</c> then TValue must be assigned explicitly.</remarks>
    /// <seealso cref="Syncfusion.Blazor.Data.SfDataManager"/>
    public partial class SfGrid<TValue> : SfDataBoundComponent, IGrid, ISfCircularComponent
    {
        #region Private Properties
#if SyncfusionLicense

        /// <summary>
        /// Checks if a parent component already provides LicenseContext (e.g., Gantt, TreeGrid)
        /// If yes, Grid should not create its own LicenseContextProvider to avoid shadowing parent's context
        /// </summary>
        [CascadingParameter]
        private LicenseContext? ExistingLicenseContext { get; set; }

        private LicenseContext gridLicenseContext => ExistingLicenseContext == null ? new LicenseContext
        {
            PrimaryPlatform = new Platform[] { Platform.GridSDK, Platform.ChartSDK, Platform.FileManagerSDK }
        } : ExistingLicenseContext;
        
#else
        private LicenseContext gridLicenseContext { get; set; } = null!;
#endif
        private bool _isShowAll { get; set; }

        private List<int>? _pagerDropdownData { get; set; }
        private bool _hasSpinner { get; set; }

        private Dictionary<string, object> _mediaColumnsUid { get; set; } = new Dictionary<string, object>();

        private bool _isColumnWidthChanged { get; set; }

        private bool _isColumnClipModeChanged { get; set; }

        private string? _originalProp;

        private bool _isRerendered { get; set; }

        private bool _updateVirtualPageSize { get; set; }

        private bool _isPersistAutoFit { get; set; }

        private List<string> _targetColumns { get; set; } = new List<string>();

        private bool _isLoaded { get; set; }

        private int _sequence { get; set; }

        private bool _isColumnResized { get; set; }

        private bool _setOnce { get; set; } = true;

        private ElementReference _element { get; set; }

        private Dictionary<string, object> _cachedAttributes { get; set; } = new Dictionary<string, object>();

        private List<GridColumn> FrozenColumn { get; set; } = new List<GridColumn>();
        private List<GridColumn> MovableColumn { get; set; } = new List<GridColumn>();
        private List<GridColumn> FrozenRightColumn { get; set; } = new List<GridColumn>();

        //This property is use for SerializeModel method jsonSerializeOptions
        private static readonly JsonSerializerOptions _serializeModelJsonSettings = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        #endregion

        #region Internal Properties

        internal bool _requireDataBoundInvoke;

        internal GridJSInteropAdaptor<TValue>? _jsAdaptor { get; set; }


        /// <summary>
        /// Indicates whether the current grid action was triggered by a toolbar click.
        /// </summary>
        /// <remarks>
        /// Set to <c>true</c> when a toolbar item is clicked. Used during CRUD operations to set
        /// which row or cell needs to be selected. Reset to <c>false</c> after processing.
        /// </remarks>

        internal bool IsToolbarInteraction { get; set; }
        internal bool IsCellClicked { get; set; }

        internal string DataId = "SfGrid-" + Guid.NewGuid().ToString();

        [Inject]
        internal ISyncfusionStringLocalizer? Localizer { get; set; }

        internal MergeHandler<TValue>? MergeModule { get; private set; }

        internal IEnumerable<TValue>? CurrentFilteredRecords { get; set; }

        internal object? Aggregate { get; set; }

        internal bool IsPrinting { get; set; }

        internal bool IsEmptyGrid { get; set; }

        internal bool RenderColumnChooser { get; set; }

        internal bool ShowChooser { get; set; }

        internal GridColumn? ColumnMenuColumn { get; set; }

        internal SfDialog? ChooserDialogInstance { get; set; }

        internal SfContextMenu<MenuItem>? ColumnMenuInstance { get; set; }

        internal SfTooltip? TooltipInstance { get; set; }
        internal string ColumnMenuClass { get; set; } = string.Empty;

        internal bool RequireLastRowBorder { get; set; }

        internal bool ShowHideEvent { get; set; } = true;

        internal bool IsColumnMenuFilter { get; set; }

        internal bool IsRowReordered { get; set; }

        internal bool IsColumnHeaderChange { get; set; }

        internal bool IsColumnHideOrShow { get; set; }

        internal bool IsRenderedFromTreeGrid { get; set; }

        internal bool IsRenderedFromFileManager { get; set; }

        internal bool IsSingleRootData { get; set; }

        internal bool IsRenderedFromGantt { get; set; }

        /// <summary>
        /// When set as true then localstorage will not be set. This helps avoiding local storage setting after
        /// foreign key rendering.
        /// </summary>
        internal bool SkipLocalStorageSet;

        internal bool Reset { get; set; }

        internal bool IsSetPersistDataCalled { get; set; }

        internal bool IsAdd { get; set; }

        /// <summary>
        ///  Specifies whether gridcolumn is autogenerated or not.
        /// </summary>
        internal bool IsAutoGeneratedColumns { get; set; }

        internal CellComponentService CellService { get; set; } = new CellComponentService();

        internal EventAggregator EventAggregator { get; set; } = new EventAggregator();

        internal IRenderer? Content { get; set; }

        internal DetailRow<TValue>? DetailRowModule { get; set; }

        internal bool IsPreventScrollEvent { get; set; }

        internal Sort<TValue>? SortModule { get; set; }

        internal Grouping<TValue>? GroupModule { get; set; }

        internal Filter<TValue>? FilterModule { get; set; }

        internal Reorder<TValue>? ReorderModule { get; set; }

        internal DataGenerator<TValue>? DataModule { get; set; }

        internal ReactiveAggregate<TValue>? ReactiveAggregateModule { get; set; }

        internal ReactiveAggregate<TValue> GetReactiveAggregateModule()
        {
            return ReactiveAggregateModule ??= new ReactiveAggregate<TValue>(this);
        }

        internal FreezeTable FrozenName { get; set; } = FreezeTable.None;

        internal bool IsColumnFrozen { get; set; }

        internal bool IsColumnFreeze { get; set; }

        internal bool IsFreezeLineMoved { get; set; }

        internal List<GridColumn> FrozenColumnModel { get; set; } = new List<GridColumn>();

        internal IDictionary<string, string> MinWidth { get; set; } = new Dictionary<string, string>();

        internal ForeignKey<TValue>? ForeignKeyModule { get; set; }

        internal Searching<TValue>? SearchModule { get; set; }

        internal VirtualScroll<TValue>? VirtualScrollModule { get; set; }

        internal InfiniteScroll<TValue>? InfiniteScrollModule { get; set; }

        internal RowReorder<TValue>? RowReorderModule { get; set; }
        internal Selection<TValue>? SelectionModule { get; set; }

        internal FocusHandler<TValue>? FocusModule { get; set; }

        internal PropertyInfoHelper<TValue>? PropHelper { get; set; }

        internal SfPager? PagerRef { get; set; }

        internal SfSpinner? SpinnerRef { get; set; }

        internal bool HideGridSpinner { get; set; }

        internal object? Data { get; set; }

        internal List<Row<object>> HeaderRows { get; set; } = new List<Row<object>>();

        internal List<Row<object>> FrozenHeaderRows { get; set; } = new List<Row<object>>();

        internal List<Row<object>> FrozenRightHeaderRows { get; set; } = new List<Row<object>>();

        internal List<Row<object>> Rows { get; set; } = new List<Row<object>>();

        internal CheckState CheckBoxState = CheckState.UnCheck;

        internal List<GridColumn>? PivotColumns { get; set; }

        internal bool RefreshPivotRowHeight { get; set; }

        internal bool EnablePivotSelection { get; set; }

        internal bool IsPivotColumnsModified { get; set; }

        internal bool IsRenderedFromPivotTable { get; set; }
        /// <summary>
        /// FilterSettings.Columns is set to FilteredColumns property before removing the columns from FilterSettings.Columns for preventing the default query generation.
        /// After processing the filter request, the FilteredColumns is set back to the FilterSettings.Columns to update the filter icons etc..
        /// </summary>
        internal List<GridFilterColumn>? FilteredColumns { get; set; }

        internal string SearchClearIcon { get; set; } = "e-clear-icon";

        internal bool TableClass { get; set; }

        internal bool EnableRightDefaultCursor { get; set; }

        /// <summary>
        /// If IsCollectionChanged is set to true then it will allow observableCollection data to make change in GridContent.
        /// </summary>
        internal bool IsCollectionChanged { get; set; }

        /// <summary>
        /// Specifies whether the Grid is frozen only by using freeze direction .
        /// </summary>
        internal bool HasFreezeDirection { get; set; }

        /// <summary>
        /// Specifies whether the frozen Grid is persisted.
        /// </summary>
        internal bool IsPersist { get; set; }
        /// <summary>
        /// Specifies whether the Grid header is stacked.
        /// </summary>
        internal bool IsStackedHeader { get; set; }

        /// <summary>
        /// Specifies refresh without data process and new row generation.
        /// </summary>
        internal bool IsFirstEventRender { get; set; } = true;

        internal bool PreventStateChange;

        internal bool IsDataLoaded { get; set; }

        internal bool _shouldRender = true;

        internal bool IsColumnPropertyChanged { get; set; }

        internal bool RefreshFrozenHeader { get; set; }

        internal Edit<TValue>? EditModule { get; set; }

        internal Paging<TValue>? PageModule { get; set; }

        internal Freeze<TValue>? FreezeModule { get; set; }

        /// <summary>
        /// Internal manager for Undo/Redo operations. 
        /// Accessed via public API methods and properties.
        /// </summary>
        internal UndoRedoManager<TValue>? UndoRedoManager { get; set; }

        internal bool IsClientInitialized { get; set; }

        internal bool IsResizing { get; set; }

        internal bool? IsMacDevice { get; set; }

        internal bool isGridModelRefresh { get; set; }

        internal int MaxVisibleRowsCount { get; set; }

        internal bool PreventEndEdit { get; set; }

        internal int DragStopIndex { get; set; }

        internal bool HasDragSelectionCompleted { get; set; }

        internal RowDragSelectedEventArgs<TValue>? DragSelectionEventArgs { get; set; }

        internal string GetUid(string prefix)
        {
            return $"{prefix}{_sequence++}";
        }

        internal ActionEventArgs<TValue>? AddOrDeleteArgs { get; set; }

        internal bool IsDeleteAction { get; set; }

        internal bool IsAutoFitEnabled { get; set; }
        internal object? FocusEditableCellArgs;
        internal Dictionary<string, bool> GroupStates { get; set; } = new Dictionary<string, bool>();

        internal int VisibleGroupedDataCount { get; set; }

        internal int frozenColumnCount;

        #endregion


        # region Public Properties

        /// <summary>
        /// Gets or sets the unmatched attributes for the Grid component.
        /// </summary>
        /// <remarks>
        /// The <see cref="UnMatchedAttributes"/> property can be used to specify custom attributes, styles, and classes
        /// for the Grid component that are not explicitly defined as properties in the component.
        /// </remarks>
        [Parameter(CaptureUnmatchedValues = true)]
        public IDictionary<string, object>? UnMatchedAttributes { get; set; }

        /// <summary>
        /// Gets or sets the current data details displayed in the grid.
        /// </summary>
        /// <remarks>
        /// This property returns an IEnumerable object that represents the current data displayed in the grid. You can use this property to access or modify the data in the grid.
        /// </remarks>
        public IEnumerable<object>? CurrentViewData { get; internal set; }

        /// <summary>
        /// Gets or sets the total number of records in the Grid's data source.
        /// </summary>
        public int TotalItemCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid is currently in edit mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if a row is being edited in the grid.The default value is <c>false</c>.
        /// </value>
        public bool IsEdit { get; set; }

        /// <summary>
        /// Gets or sets the grid events that are triggered on various actions in the grid.
        /// </summary>
        /// <remarks>
        /// The events can be used to customize the grid's behavior, perform custom actions on data, and handle user interactions with the grid.
        /// </remarks>
        public GridEvents<TValue>? GridEvents { get; set; }

        /// <summary>
        /// Gets the selected records of the grid.
        /// </summary>
        /// <remarks>
        /// If the selection persistence feature is enabled through the <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.PersistSelection"/> property,
        /// this property returns the selected records across all pages. Otherwise, it only returns the selected records in the current page.
        /// <seealso cref="Syncfusion.Blazor.Grids.GridSelectionSettings.PersistSelection"/>
        /// </remarks>
        public List<TValue> SelectedRecords
        {
            get
            {
                return SelectionModule?.GetSelectedRecords() ?? new List<TValue>();
            }
        }

        /// <summary>
        /// Gets the indexes of the currently selected rows in the Grid.
        /// </summary>
        /// <remarks>
        /// When one or more rows are selected in the Grid, the corresponding row index values are added to this list. 
        /// You can use this property to programmatically access the index values of the currently selected rows in the Grid.
        /// </remarks>
        public List<int> SelectedRowIndexes { get; internal set; } = new List<int>();

        /// <summary>
        /// Gets or sets a value indicating whether to force the immediate re-rendering of the grid component.
        /// </summary>
        /// <value>
        /// <c>true</c> to force the grid component to re-render immediately. The default value is <c>false</c>.
        /// </value>
        public bool ForceUpdate { get; set; }

        /// <summary>
        /// Specifies refresh without data process and new row generation.
        /// </summary>
        /// <exclude/>
        public bool SoftRefresh { get; set; }

        /// <summary>
        /// Specifies refresh column header.
        /// </summary>
        /// <exclude/>
        public bool RefreshColumnHeader { get; set; }

        /// <summary>
        /// Specifies new column added.
        /// </summary>
        /// <exclude/>
        public bool HasColumnChanges { get; set; }

        /// <summary>
        /// Specifies new aggregate column added.
        /// </summary>
        /// <exclude/>
        public bool HasAggregateChanges { get; set; }

        /// <summary>
        /// Specifies new sort column added.
        /// </summary>
        /// <exclude/>
        public bool HasSortColumnChanges { get; set; }

        /// <summary>
        /// Specifies new filter column added.
        /// </summary>
        /// <exclude/>
        public bool HasFilterColumnChanges { get; set; }

        /// <summary>
        /// Specifies grid is following server rendered.
        /// </summary>
        /// <exclude/>
        [JsonPropertyName("isServerRendered")]
        public bool IsServerRendered { get; internal set; } = true;

        /// <summary>
        /// Specifies group expand/collapse state should persist.
        /// </summary>
        /// <exclude/>
        [JsonPropertyName("isExpanded")]
        public bool IsExpanded { get; set; } = true;

        /// <summary>
        /// Specifies new column added index.
        /// </summary>
        /// <exclude/>
        public int ColumnIndex { get; set; } = -1;

        #endregion

        #region Utility & Rendering Methods Class And Styles

        /// <summary>
        /// Returns the class name to be added to the container element.
        /// </summary>
        /// <remarks>
        /// Generates CSS class names based on grid configuration including device mode, hover state,
        /// resizing, RTL, row height, text wrapping, adaptive UI, and grid lines settings.
        /// </remarks>
        /// <returns>string.</returns>
        public string GetClass()
        {
            string classNames = "sf-grid e-grid e-control e-responsive e-default";


            if (SyncfusionService.IsDeviceMode)
            {
                classNames = $"{classNames} e-device";
            }

            if (EnableHover)
            {
                classNames = $"{classNames} e-gridhover";
            }

            if (AllowResizing)
            {
                classNames = $"{classNames} e-resize-lines";
            }

            if (EnableRtl || SyncfusionService.options.EnableRtl)
            {
                classNames = $"{classNames} e-rtl";
            }

            if (RowHeight != 0)
            {
                classNames = $"{classNames} e-grid-min-height";
            }

            if (RowHeight == 0)
            {
                classNames = $"{classNames} e-grid-height";
            }

            if (AllowTextWrap && TextWrapSettings?.WrapMode != null && TextWrapSettings.WrapMode.Equals(WrapMode.Both))
            {
                classNames = $"{classNames} e-wrap";
            }
            
            // Refactored Adaptive UI condition
            bool isAdaptiveUIVertical = EnableAdaptiveUI && RowRenderingMode.Equals(RowDirection.Vertical);
            bool isMobileAdaptiveMode = SyncfusionService.IsDeviceMode && AdaptiveUIMode.Equals(AdaptiveMode.Mobile);
            bool isBothAdaptiveMode = AdaptiveUIMode.Equals(AdaptiveMode.Both);
            bool shouldApplyMobileAdaptive = isAdaptiveUIVertical && (isBothAdaptiveMode || isMobileAdaptiveMode);
            bool isDesktopAdaptiveMode = EnableAdaptiveUI && AdaptiveUIMode.Equals(AdaptiveMode.Desktop) && !SyncfusionService.IsDeviceMode && RowRenderingMode.Equals(RowDirection.Vertical);
            
            if (shouldApplyMobileAdaptive || isDesktopAdaptiveMode)
            {
                classNames = $"{classNames} e-bigger e-row-responsive";
            }
            if (!GridLines.Equals(GridLine.Default))
            {
                if (GridLines.Equals(GridLine.Both))
                {
                    classNames = $"{classNames} e-bothlines";
                }
                else if (GridLines.Equals(GridLine.Horizontal))
                {
                    classNames = $"{classNames} e-horizontallines";
                }
                else if (GridLines.Equals(GridLine.Vertical))
                {
                    classNames = $"{classNames} e-verticallines";
                }
                else if (GridLines.Equals(GridLine.None))
                {
                    classNames = $"{classNames} e-hidelines";
                }
            }

            if (UnMatchedAttributes != null && UnMatchedAttributes.TryGetValue("class", out object? value))
            {
                if (UnMatchedAttributes["class"] != null && UnMatchedAttributes["class"].Equals("table table-striped"))
                {
                    TableClass = true;
                }
                else
                {
                    _cachedAttributes.AddOrUpdateItem("class", $"{UnMatchedAttributes["class"]} {classNames}");
                }
            }
            return classNames;
        }

        internal string GetContentClassName()
        {
            string className = "e-content";
            if (!string.Equals("auto", Height, StringComparison.Ordinal))
            {
                className = $"{className} e-yscroll";
            }

            return className;
        }

        private string GetStyle()
        {
            string styleText = string.Empty;
            if (Height.Contains('%', StringComparison.Ordinal))
            {
                styleText = $"height:{GridUtils.FormarUnit(Height)};{styleText}";
            }
            styleText = $"width:{GridUtils.FormarUnit(Width)};{styleText}";

            if (UnMatchedAttributes != null && UnMatchedAttributes.TryGetValue("style", out object? value))
            {
                _cachedAttributes.AddOrUpdateItem("data-sf-style", $"{value};{styleText}");
            }

            return styleText;
        }

        private string GetContentStyle()
        {
            string styleText = string.Empty;
            if (!string.Equals("auto", Height, StringComparison.Ordinal))
            {
                styleText = $"height:{GridUtils.FormarUnit(Height)}";
            }
            if (FreezeModule!.GetFrozenCount() != 0 && EnableColumnVirtualization)
            {
                styleText = string.Concat(styleText, $"; overflow: hidden auto;");
            }

            return styleText;
        }

        private string GetContentClass()
        {
            string classNames = "e-gridcontent";
            if (AllowTextWrap && TextWrapSettings != null && TextWrapSettings.WrapMode.Equals(WrapMode.Content))
            {
                classNames = $"{classNames} e-wrap";
            }
            if (EnableAdaptiveUI && !(Toolbar != null || ((IGrid)this).GridTemplates?.ToolbarTemplate != null))
            {
                classNames = $"{classNames} e-responsive-header";
            }

            return classNames;
        }

        internal string GetTableStyles()
        {
            string defaultStyle = "border-collapse: separate; border-spacing: .25px;";

            if (AutoFit)
            {
                List<GridColumn> columns = GetVisibleColumnsAsync().Result;

                if (columns.Count > 0)
                {
                    if (columns.Any(column => string.IsNullOrWhiteSpace(column.Width) || column.Width == "auto" || column.Width.Contains('%', StringComparison.Ordinal)))
                    {
                        return defaultStyle;
                    }
                    double totalWidth = 0;
                    int properColumnWidth = 0;
                    foreach (GridColumn column in columns)
                    {
                        properColumnWidth = GridUtils.ConvertPxToInt(column.Width);
                        if (properColumnWidth == -1)
                        {
                            return defaultStyle;
                        }
                        totalWidth += properColumnWidth;
                    }
                    if (AllowGrouping && GroupSettings != null && GroupSettings.Columns != null)
                    {
                        foreach (var groupcol in GroupSettings.Columns)
                        {
                            totalWidth += 30;
                        }
                    }
                    if (AllowRowDragAndDrop)
                    {
                        totalWidth += 30;
                    }
                    if (((IGrid)this).GridTemplates != null && ((IGrid)this).GridTemplates.DetailTemplate != null)
                    {
                        totalWidth += 30;
                    }
                    if (totalWidth > 0)
                    {
                        defaultStyle += $" width: {totalWidth}px;";
                    }
                }
            }
            return defaultStyle;
        }

        #endregion

        #region Pager & Data Configuration

        private void EnsurePagerDropdown()
        {

            bool pageSizes = (PageSettings!.PageSizes as bool?) != null ? true : false;
            if (pageSizes)
            {
                _pagerDropdownData = new List<int>() { 5, 10, 12, 20 };
                _isShowAll = true;
            }
            else if (PageSettings.PageSizes != null)
            {
                List<object>? data = (PageSettings?.PageSizes as IList)?.Cast<object>().ToList();
                _pagerDropdownData = new List<int>();
                if (data != null)
                {
                    foreach (var item in data!)
                    {
                        int value;
                        if (int.TryParse(item.ToString(), out value))
                        {
                            _pagerDropdownData.Add(value);
                        }
                    }
                    if (data.Contains("All"))
                    {
                        _isShowAll = true;
                    }
                }
            }

        }

        #endregion

        #region Component Rendering & Lifecycle

        /// <summary>
        /// Determines whether the grid component should re-render.
        /// </summary>
        /// <remarks>
        /// Controls render optimization by evaluating edit mode and virtualization state.
        /// For virtualized grids in Normal edit mode, forces re-render when editing is active.
        /// </remarks>
        /// <returns>
        /// True if the grid should render; otherwise, returns the previous render state.
        /// </returns>
        protected override bool ShouldRender()
        {
            var _tmp = _shouldRender;
            
            // Refactored virtualization and edit mode condition
            bool hasVirtualizationEnabled = EnableVirtualization || EnableInfiniteScrolling;
            bool isNormalEditMode = EditSettings!.Mode.Equals(EditMode.Normal);
            bool shouldForceRenderDuringEdit = hasVirtualizationEnabled && isNormalEditMode && IsEdit;
            
            if (shouldForceRenderDuringEdit)
            {
                _shouldRender = true;
                return true;
            }
            else if (!IsEdit)
            {
                _shouldRender = true;
            }

            return _tmp;
        }

        /// <summary>
        /// Prevents the grid render. This method will internally sets value to be returned from ShouldRender method.
        /// </summary>
        /// <param name="preventRender">Default value is true. Toggles the ShouldRender method value.</param>
        public void PreventRender(bool preventRender = true) => _shouldRender = !preventRender;

        /// <inheritdoc/>
        protected override async void OnObservableChange(string propertyName, object sender, bool isCollectionChanged = false, NotifyCollectionChangedEventArgs? e = null)
        {
            if (VirtualScrollModule != null)
            {
                VirtualScrollModule.IsObservable = true;
            }

            if (!isCollectionChanged)
            {
                PropertyChanges?.Remove("DataSource");
                var rowData = Rows?.Where(x => x.Data != null && x.Data.Equals(sender)).Select(x => x);
                var row = rowData != null && rowData.AsQueryable().Any() ? rowData.AsQueryable().First() : null;
                var groupAggregateRow = Rows?.Where(x => x.RowType == "GroupCaption").Select(x => x);
                // Refactored aggregate refresh condition
                bool hasAggregates = this.Aggregate != null;
                bool hasPropertyChanges = PropertyChanges != null;
                bool isNonRefreshableChange = !PropertyChanges!.Keys.Any(p => GridUtils.IsRefreshable(p));
                bool hasGroupAggregateRows = groupAggregateRow != null && groupAggregateRow.Any();
                bool shouldSkipAggregateRefresh = hasAggregates && hasPropertyChanges && isNonRefreshableChange && hasGroupAggregateRows;
                
                if (shouldSkipAggregateRefresh)
                {
                    PropertyChanges.Add("Aggregates", groupAggregateRow);
                }
                if (row != null)
                {
                    row.Cells?.ForEach(_ => _.Changes = true);
                    SoftRefresh = true;
                    row.HasDataChanges = true;
                    EventAggregator?.Trigger("RowStateChanged", row);
                    SoftRefresh = false;
                    row.HasDataChanges = false;
                }
                else
                {
                    return;
                }
            }
            else if (PropertyChanges?.Remove("DataSource") == true)
            {
                // Refactored virtual scroll boundary check
                bool isValidVirtualization = EnableVirtualization && VirtualScrollModule != null;
                bool isTotalItemCountGreater = TotalItemCount > PageSettings!.PageSize;
                bool isNewStartingIndexInRange = e?.NewStartingIndex >= VirtualScrollModule?.RowStartIndex && e?.NewStartingIndex <= VirtualScrollModule?.RowEndIndex;
                bool isOutOfVirtualRange = !isNewStartingIndexInRange;
                
                if (isValidVirtualization && isTotalItemCountGreater && isOutOfVirtualRange && VirtualScrollModule != null)
                {
                    bool isCurrentView = false;
                    if (TotalItemCount < DataSource?.Count())
                    {
                        if (Rows?.Last().Index == TotalItemCount - 1 )
                        {
                            VirtualScrollModule.RowEndIndex = TotalItemCount;
                            VirtualScrollModule.RowStartIndex = VirtualScrollModule.RowEndIndex - PageSettings.PageSize;
                            VirtualScrollModule.CheckAndResetCache("Refresh").GetAwaiter();
                        }
                        else
                        {
                            ForceUpdate = true;
                            TotalItemCount = DataSource.Count();
                            VirtualScrollModule.RowStartIndex = VirtualScrollModule.RowStartIndex;
                            VirtualScrollModule.RowEndIndex = VirtualScrollModule.RowStartIndex + PageSettings.PageSize;
                            VirtualScrollModule.CheckAndResetCache("Refresh").GetAwaiter();
                            InvokeMethod("sfBlazor.Grid.virtualHeight", new object[] { DataId, GetClientOption(), TotalItemCount }).GetAwaiter();
                        }
                        DataProcess(new ActionArgs() { VirtualStartIndex = VirtualScrollModule.RowStartIndex, VirtualEndIndex = VirtualScrollModule.RowEndIndex }).GetAwaiter();
                    }
                    else
                    {
                        if (e?.OldItems != null)
                        {
                            foreach (var item in e.OldItems)
                            {
                                var rowData = Rows?.Where(x => x.Data != null && x.Data.Equals(item)).Select(x => x);
                                isCurrentView = rowData?.Any() ?? false;
                            }
                        }
                        if (isCurrentView)
                        {
                            if (RowReorderModule != null && !RowReorderModule.IsReorderByInteraction)
                            {
                                VirtualScrollModule.RowStartIndex = VirtualScrollModule.RowStartIndex;
                                VirtualScrollModule.RowEndIndex = VirtualScrollModule.RowStartIndex + PageSettings.PageSize;
                                VirtualScrollModule.CheckAndResetCache("Refresh").GetAwaiter();
                                DataProcess(new ActionArgs() { VirtualStartIndex = VirtualScrollModule.RowStartIndex, VirtualEndIndex = VirtualScrollModule.RowEndIndex }).GetAwaiter();
                            }
                        }
                        else
                        {

                            TotalItemCount = DataSource?.Count() ?? 0;
                            VirtualScrollModule.RowStartIndex = VirtualScrollModule.RowStartIndex;
                            VirtualScrollModule.RowEndIndex = VirtualScrollModule.RowStartIndex + PageSettings.PageSize;
                            if (RowReorderModule != null && !RowReorderModule.IsReorderByInteraction)
                            {
                                VirtualScrollModule.CheckAndResetCache("Refresh").GetAwaiter();
                            }
                            InvokeMethod("sfBlazor.Grid.virtualHeight", new object[] { DataId, GetClientOption(), TotalItemCount }).GetAwaiter();
                        }
                    }
                }
                else
                {
                    // Refactored virtual scroll boundary condition
                    bool isLastRow = Rows?.LastOrDefault()?.Index == TotalItemCount - 1;
                    bool hasVirtualScrollReady = VirtualScrollModule != null && TotalItemCount == VirtualScrollModule.RowEndIndex;
                    bool isNotAtTop = VirtualScrollModule?.ScrollTop != 0;
                    bool shouldResetScroll = isLastRow && hasVirtualScrollReady && isNotAtTop;
                    
                    if (shouldResetScroll && VirtualScrollModule != null)
                    {
                        // Executed when adding new record at the bottom of the grid and also scrollbar rendered at the end.
                        VirtualScrollModule.RowStartIndex = VirtualScrollModule.RowStartIndex + 1;
                        VirtualScrollModule.RowEndIndex = VirtualScrollModule.RowStartIndex + PageSettings!.PageSize;
                    }
                    if (RowReorderModule != null && !RowReorderModule.IsReorderByInteraction)
                    {
                        ModelChanged(new ActionEventArgs<TValue>() { RequestType = Action.Refresh }).GetAwaiter();
                    }
                }
            }
            if (VirtualScrollModule != null)
            {
                VirtualScrollModule.IsObservable = false;
            }
        }

        internal void SetColumnValueType()
        {
            var data = Rows?.Find(_ => _.IsDataRow)?.Data;
            var (isTreeGridExpando, isTreeGridDynamic) = DetectTreeGridDataType(data);
            var type = ResolveDataTypeForColumns(data, isTreeGridExpando, isTreeGridDynamic);
            HandleStaticDynamicColumnInitialization(isTreeGridExpando, isTreeGridDynamic);
            InitializeColumnIndexIfNeeded();
            List<GridColumn> columns = GridUtils.GetColumns(this);
            ApplyColumnValueTypes(columns, data, type, isTreeGridExpando, isTreeGridDynamic);
            EventAggregator?.Trigger("SetColumnType", columns);
        }

        /// <summary>
        /// Detects if data type is TreeGrid ExpandoObject or DynamicObject.
        /// </summary>
        private (bool isTreeGridExpando, bool isTreeGridDynamic) DetectTreeGridDataType(object? data)
        {
            bool isTreeGridExpando = false;
            bool isTreeGridDynamic = false;
            if (IsRenderedFromTreeGrid && data != null)
            {
                var dataType = data.GetType();
                var args = dataType.IsGenericType ? dataType.GetGenericArguments() : Type.EmptyTypes;
                var rowType = args.Length > 0 ? args[0] : null;
                isTreeGridExpando = rowType == typeof(ExpandoObject);
                isTreeGridDynamic = rowType?.IsSubclassOf(typeof(DynamicObject)) ?? false;
            }
            return (isTreeGridExpando, isTreeGridDynamic);
        }

        /// <summary>
        /// Resolves the column type dictionary from data for type inference.
        /// </summary>
        private IDictionary<string, Type>? ResolveDataTypeForColumns(object? data, bool isTreeGridExpando, bool isTreeGridDynamic)
        {
            IDictionary<string, Type>? type = null;
            if (data is ExpandoObject)
            {
                type = DataUtil.GetColumnType(new List<object>() { data }, true);
            }
            else if (data is DynamicObject || isTreeGridDynamic)
            {
                type = EditModule!.GetDynamicColType();
            }
            else if (isTreeGridExpando)
            {
                type = ExtractTypeFromTreeGridExpando(data);
            }
            return type;
        }

        /// <summary>
        /// Extracts column type from TreeGrid ExpandoObject DataItem property.
        /// </summary>
        private static IDictionary<string, Type>? ExtractTypeFromTreeGridExpando(object? data)
        {
            IDictionary<string, Type>? type = null;
            var dataItemProp = data!.GetType().GetProperty("DataItem");
            if (dataItemProp != null)
            {
                var innerData = dataItemProp.GetValue(data);
                if (innerData is ExpandoObject expandoData)
                {
                    type = DataUtil.GetColumnType(new List<object>() { expandoData }, true);
                }
            }
            return type;
        }

        /// <summary>
        /// Initializes column value types for static/dynamic column handling.
        /// </summary>
        private void HandleStaticDynamicColumnInitialization(bool isTreeGridExpando, bool isTreeGridDynamic)
        {
            if ((typeof(TValue) == typeof(ExpandoObject) || typeof(TValue).IsSubclassOf(typeof(DynamicObject))) && Columns != null)
            {
                foreach (var column in Columns)
                {
                    if (!column.Type.Equals(ColumnType.None) && column.ValueType == null)
                    {
                        column.ValueType = SetDynamicColumnType(column);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes column indices on first render if parameters include Columns.
        /// </summary>
        private void InitializeColumnIndexIfNeeded()
        {
            if (!IsRendered && directParamKeys?.Contains(nameof(Columns)) == true)
            {
                List<GridColumn> ColumnsData = (IsStackedHeader ? Columns : GridUtils.GetColumns(this)) ?? new List<GridColumn>();
                SetColumnIndex(ColumnsData, true);
            }
        }

        /// <summary>
        /// Applies resolved value types to all columns in the grid.
        /// </summary>
        private void ApplyColumnValueTypes(List<GridColumn> columns, object? data, IDictionary<string, Type>? type, bool isTreeGridExpando, bool isTreeGridDynamic)
        {
            foreach (var column in columns)
            {
                ApplyColumnValueType(column, data, type, isTreeGridExpando, isTreeGridDynamic);
            }
        }

        /// <summary>
        /// Applies value type to individual column and sets edit type.
        /// </summary>
        private void ApplyColumnValueType(GridColumn column, object? data, IDictionary<string, Type>? type, bool isTreeGridExpando, bool isTreeGridDynamic)
        {
            Type? _ref = null;
            var Fields = column.Field.Split('.');
            var Complex = Fields.Length;
            string fieldName = NormalizeFieldNameForTreeGrid(column.Field, isTreeGridExpando, isTreeGridDynamic);
            // Refactored dynamic data type detection
            bool isExpandoObject = data is ExpandoObject;
            bool isDynamicObject = data is DynamicObject;
            bool isDynamicData = isExpandoObject || isDynamicObject || isTreeGridExpando || isTreeGridDynamic;
            
            if (isDynamicData)
            {
                ResolveColumnValueTypeDynamic(column, type, fieldName, Complex, ref _ref, data, isTreeGridExpando, isTreeGridDynamic);
            }
            else
            {
                ResolveColumnValueTypeStatic(column, ref _ref, data);
            }
            column.SetColumnEditType();
        }

        /// <summary>
        /// Normalizes field name by removing TreeGrid DataItem prefix if present.
        /// </summary>
        private static string NormalizeFieldNameForTreeGrid(string fieldName, bool isTreeGridExpando, bool isTreeGridDynamic)
        {
            if ((isTreeGridExpando || isTreeGridDynamic) && fieldName.StartsWith("DataItem.", StringComparison.Ordinal))
            {
                return fieldName.Substring(9); // Length of "DataItem."
            }
            return fieldName;
        }

        /// <summary>
        /// Resolves column value type for dynamic data sources.
        /// </summary>
        private void ResolveColumnValueTypeDynamic(GridColumn column, IDictionary<string, Type>? type, string fieldName, int Complex, ref Type? _ref, object? data, bool isTreeGridExpando, bool isTreeGridDynamic)
        {
            // Refactored type information check with field search
            Type? value = null;
            bool hasTypeInfo = type != null && column.ForeignKeyValue != null;
            bool hasFieldInType = hasTypeInfo && type != null && type.TryGetValue(column.Field, out value);
                
            if (hasFieldInType)
            {
                column.ActualType = value;
                column.ValueType = column.ValueType ?? EditModule!.GetColumnType(column, ref _ref!)!;
            }
            else if (type != null && type.TryGetValue(fieldName, out Type? _value) && (!(Complex > 1) || isTreeGridExpando || isTreeGridDynamic))
            {
                column.ValueType = _value;
                if (column.ValueType == typeof(object))
                {
                    column.ValueType = SetDynamicColumnType(column);
                }
            }
            else if (Complex > 1)
            {
                var valueType = EditModule!.GetColumnType(column, ref _ref!, null!, data!);
                column.ValueType = valueType != null ? valueType : column.ValueType;
                column.ActualType = _ref;
            }
        }

        /// <summary>
        /// Resolves column value type for static (POCO) data sources.
        /// </summary>
        private void ResolveColumnValueTypeStatic(GridColumn column, ref Type? _ref, object? data)
        {
            var valueType = EditModule!.GetColumnType(column, ref _ref!, null!, data!);
            column.ValueType = valueType != null ? valueType : column.ValueType;
            column.ActualType = _ref;
        }

        private static Type SetDynamicColumnType(GridColumn column)
        {
            if (column.Type == ColumnType.Integer)
            {
                return typeof(int?);
            }
            else if (column.Type == ColumnType.Double)
            {
                return typeof(double?);
            }
            else if (column.Type == ColumnType.Long)
            {
                return typeof(long?);
            }
            else if (column.Type == ColumnType.Decimal)
            {
                return typeof(decimal?);
            }
            else if (column.Type == ColumnType.String)
            {
                return typeof(string);
            }
            else if (column.Type == ColumnType.Boolean)
            {
                return typeof(bool?);
            }
            else if (column.Type == ColumnType.DateTime || column.Type == ColumnType.Date)
            {
                return typeof(DateTime?);
            }
            else if (column.Type == ColumnType.DateOnly)
            {
                return typeof(DateOnly?);
            }
            else if (column.Type == ColumnType.TimeOnly)
            {
                return typeof(TimeOnly?);
            }

            return typeof(object);
        }
        internal void SetColumnIndex(List<GridColumn> columns, bool changeOriginalIndex = false)
        {

            List<GridColumn> stackedcolumns = new List<GridColumn>();
            if (columns != null && columns.Count != 0)
            {
                foreach (var col in columns)
                {
                    int index = ++ColumnIndex;
                    col.SetIndex(index);
                    if (changeOriginalIndex)
                    {
                        col.OriginalIndex = index;
                    }
            // Refactored complex column children check
            bool hasComplexChildren = col.Columns != null && col.Columns.Count != 0;
            
            if (hasComplexChildren && col.Columns != null)
            {
                        foreach (var stackedcol in col.Columns)
                        {
                            stackedcolumns.Add(stackedcol);
                        }
                    }
                }
            }
            if (stackedcolumns.Count > 0)
            {
                SetColumnIndex(stackedcolumns, changeOriginalIndex);
            }
        }
        internal void EnsureFeaturesCompatibility()
        {
            // Refactored virtualization and grouping compatibility check
            bool hasVirtualizationWithoutExpandAll = EnableVirtualization && !GroupSettings!.ExpandAllGroups;
            bool hasLazyLoadingEnabled = GroupSettings!.EnableLazyLoading;
            bool shouldDisableExpand = (hasVirtualizationWithoutExpandAll || hasLazyLoadingEnabled) && AllowGrouping && GroupModule != null && !GroupModule.IsLazyExpandAll;
            
            if (shouldDisableExpand)
            {
                IsExpanded = false;
            }
            
            bool shouldEnableVirtualMaskRow = EnableVirtualization && GroupSettings.EnableLazyLoading;
            if (shouldEnableVirtualMaskRow)
            {
                EnableVirtualMaskRow = true;
            }
        }

        private static string SerializeModel(SfGrid<TValue> comp)
        {
            IDictionary<string, object> model = new Dictionary<string, object>()
            {
                { "columns", comp.Columns! }, { "filterSettings", comp.FilterSettings! },
                { "searchSettings", comp.SearchSettings! }, { "sortSettings", comp.SortSettings! },
                { "groupSettings", comp.GroupSettings! }, { "pageSettings", comp.PageSettings! },
                { "autoSpanning", comp.AutoSpan }
            };
            return JsonSerializer.Serialize(model, _serializeModelJsonSettings);
        }

        private static bool CheckColumnType(GridColumn persistColumn, GridColumn currentColumn)
        {
            // Refactored column command type comparison
            bool persistCommandsExist = persistColumn.Commands != null && persistColumn.Commands.Count > 0;
            bool currentCommandsExist = currentColumn.Commands != null && currentColumn.Commands.Count > 0;
            bool bothHaveCommands = persistCommandsExist && currentCommandsExist;
            bool bothAreCheckbox = persistColumn.Type == ColumnType.CheckBox && currentColumn.Type == ColumnType.CheckBox;
            bool isSameCommandType = bothHaveCommands || bothAreCheckbox;
            
            if (isSameCommandType)
            {
                return true;
            }
            return false;
        }

        internal void CollectionDisposeMethod(object data)
        {
            if (SfBaseUtils.IsObservableCollection(data))
            {
                ((INotifyCollectionChanged)data).CollectionChanged -= CollectionChangedMethod!;
            }
        }

        private void CollectionChangedMethod(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsCollectionChanged = true;
            CollectionDisposeMethod(CurrentViewData!);
        }

        internal bool IsFixedColumnPresent()
        {
            if (Columns?.Count > 0)
            {
                try
                {
                    return Columns.Any(column => column?.FixedColumn == true ||
                        column!.Columns?.Any(subColumn => subColumn?.FixedColumn == true) == true);
                }
                catch (Exception exception) when (HandleException(exception))
                {
                    return false;
                }
            }
            return false;
        }

        private List<GridColumn> RearrangeLockedColumns(List<GridColumn> gridColumns)
        {
            List<GridColumn> Columns = new List<GridColumn>();
            Columns = gridColumns;
            if (!IsStackedHeader)
            {
                List<GridColumn> lockColumns = Columns.Where(x => x.FixedColumn).ToList();
                List<GridColumn> normalColumns = Columns.Where(x => !x.FixedColumn).ToList();
                Columns = lockColumns.Concat(normalColumns).ToList();
                for (int i = 0; i < Columns.Count; i++)
                {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
                    Columns[i].Index = i;
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.

                }
            }
            else if (gridColumns != null && gridColumns.Count != 0 && IsStackedHeader)
            {
                //Lock columns with stacked header
                List<GridColumn> lockedColumns = new List<GridColumn>();
                List<GridColumn> unLockedColumns = new List<GridColumn>();
                List<GridColumn> Stackedcolumns = Columns;
                if (Freeze<TValue>.IsFrozenColumnPresent(Stackedcolumns))
                {
                    for (int i = 0; i < Stackedcolumns.Count; i++)
                    {
                        GridColumn column = Stackedcolumns[i];
                        if (column.Columns != null && Freeze<TValue>.IsFrozenColumnPresent(column.Columns))
                        {
                            GridColumn lockColumn = FreezeModule!.SetStackedFrozenAndMovableColumns(column, isLocked: true);
                            GridColumn normalColumn = FreezeModule!.SetStackedFrozenAndMovableColumns(column, isLocked: false);
                            if (lockColumn.Columns?.Count > 0)
                            {
                                lockedColumns.Add(lockColumn);
                            }
                            if (normalColumn.Columns?.Count > 0)
                            {
                                unLockedColumns.Add(normalColumn);
                            }
                        }
                        else if (column.FixedColumn)
                        {
                            lockedColumns.Add(column);
                        }
                        else
                        {
                            unLockedColumns.Add(column);
                        }
                    }
                    Columns = lockedColumns.Concat(unLockedColumns).ToList();
                }
            }
            return Columns;
        }

        internal List<GridColumn> RearrangeColumns(List<GridColumn> columns)
        {
            List<GridColumn> Columns = new List<GridColumn>();
            Columns = columns;
            bool lockColumnCheck = Columns != null && Columns.Where(x => x.FixedColumn).Any();
            if (Columns != null && lockColumnCheck && !IsStackedHeader)
            {
                Columns = RearrangeLockedColumns(Columns);
            }
            // Refactored visible columns stacked header check
            bool hasVisibleColumns = columns != null && columns.Count != 0;
            bool isStackedHeaderLayout = IsStackedHeader;
            bool shouldProcessVisibleStacked = hasVisibleColumns && isStackedHeaderLayout;
            
            if (shouldProcessVisibleStacked)
            {
                List<GridColumn>? Stackedcolumns = Columns;
                frozenColumnCount = (int)FrozenColumns;
                FrozenColumn = new List<GridColumn>();
                FrozenRightColumn = new List<GridColumn>();
                MovableColumn = new List<GridColumn>();
                var parentColumn = GetColumnsAsync().Result;
                var frozenColumnNum = FrozenColumns;
                
                // Refactored frozen column check for stacked headers
                bool hasFrozenCount = FreezeModule!.GetFrozenCount() > 0;
                bool hasFixedFrozenColumns = Columns?.Where(_ => _.IsFrozen && (_.Freeze.Equals(FreezeDirection.Fixed))).ToList().Count > 0;
                bool shouldProcessFrozenColumns = hasFrozenCount || hasFixedFrozenColumns;
                
                if (shouldProcessFrozenColumns)
                {
                    Freeze<TValue>.SetFrozenMovableLabel(parentColumn, frozenColumnNum);
                    
                    if (Freeze<TValue>.IsFrozenColumnPresent(Stackedcolumns!))
                    {
                        for (int i = 0; i < Stackedcolumns?.Count; i++)
                        {
                            GridColumn column = Stackedcolumns[i];
                            if (column.Columns != null && Freeze<TValue>.IsFrozenColumnPresent(column.Columns))
                            {
                                var FrozenColumnList = FreezeModule!.SetStackedFrozenAndMovableColumns(column, "FrozenLeft");
                                var MovableColumnList = FreezeModule!.SetStackedFrozenAndMovableColumns(column, "Movable");
                                var FrozenRightColumnList = FreezeModule!.SetStackedFrozenAndMovableColumns(column, "FrozenRight");
                                if (FrozenColumnList.Columns?.Count != 0)
                                {
                                    FrozenColumn.Add(FrozenColumnList);
                                }
                                if (MovableColumnList.Columns?.Count != 0)
                                {
                                    MovableColumn.Add(MovableColumnList);
                                }

                                if (FrozenRightColumnList.Columns?.Count != 0)
                                {
                                    FrozenRightColumn.Add(FrozenRightColumnList);
                                }
                            }
                            else if (column.Freeze.Equals(FreezeDirection.Right) && column.IsFrozen)
                            {
                                FrozenRightColumn.Add(column);
                            }
                            else if ((FrozenColumns >= i && parentColumn.Any(x => x.FrozenMovableLabel == "FrozenLeft") && column.IsFrozen) || (column.IsFrozen && column.Freeze.Equals(FreezeDirection.Left)))
                            {
                                FrozenColumn.Add(column);
                            }
                            else
                            {
                                MovableColumn.Add(column);
                            }
                        }
                        IsColumnFrozen = FrozenColumn.Count != 0 ? true : false;
                        Columns = FrozenColumn.Concat(MovableColumn).Concat(FrozenRightColumn).ToList();
                    }
                    else
                    {
                        for (int i = 0; i < Stackedcolumns!.Count; i++)
                        {
                            GridColumn column = Stackedcolumns[i];

                            if (column.Columns != null && (frozenColumnCount > 0 || Freeze<TValue>.IsFrozenColumnPresent(column.Columns)))
                            {
                                var FrozenColumnList = FreezeModule!.SetStackedFrozenAndMovableColumns(column, "FrozenLeft");
                                var MovableColumnList = FreezeModule!.SetStackedFrozenAndMovableColumns(column, "Movable");
                                if (FrozenColumnList.Columns?.Count != 0)
                                {
                                    FrozenColumn.Add(FrozenColumnList);
                                }
                                if (MovableColumnList.Columns?.Count != 0)
                                {
                                    MovableColumn.Add(MovableColumnList);
                                }
                            }
                            else if (((column.FrozenMovableLabel == "FrozenLeft" || column.FrozenMovableLabel == "FrozenLeftLast") && frozenColumnCount > 0) || column.Freeze.Equals(FreezeDirection.Left) && column.IsFrozen)
                            {
                                FrozenColumn.Add(column);
                                frozenColumnCount--;
                            }
                            else
                            {
                                MovableColumn.Add(column);
                            }
                        }
                        IsColumnFreeze = FrozenColumn.Count != 0 ? true : false;
                        Columns = FrozenColumn.Concat(MovableColumn).ToList();
                    }
                }
                else
                {
                    Columns = RearrangeLockedColumns(Columns!);
                }
            }
            else if (Columns != null && Columns.Count > 0 && !lockColumnCheck)
            {
                var frozenColumnsLeft = Columns.Where(x => x.Freeze == FreezeDirection.Left && x.IsFrozen).ToList();
                var MovableColumns = Columns.Where(x => !x.IsFrozen || (x.IsFrozen && x.Freeze == FreezeDirection.Fixed)).ToList();
                var frozenColumnsRight = Columns.Where(x => x.Freeze == FreezeDirection.Right && x.IsFrozen).ToList();
                // Refactored no frozen columns condition
                bool hasNoDefaultFrozen = FrozenColumns == 0;
                bool hasNoLeftFrozen = frozenColumnsLeft.Count == 0;
                bool hasNoRightFrozen = frozenColumnsRight.Count == 0;
                bool isNoFrozenColumns = hasNoDefaultFrozen && hasNoLeftFrozen && hasNoRightFrozen;
                
                if (isNoFrozenColumns)
                {
                    return columns!;
                }
                if (FrozenColumns > 0 && FreezeModule!.GetFreezeLeftCount() == 0 && FreezeModule!.GetFreezeRightColumnsCount() == 0)
                {
                    frozenColumnsLeft = Columns.Where(x => x.Index < FreezeModule!.GetFrozenCount()).ToList();
                    MovableColumns = Columns.Where(x => x.Index >= FreezeModule!.GetFrozenCount()).ToList();
                }
                Columns = frozenColumnsLeft.Concat(MovableColumns).Concat(frozenColumnsRight).ToList();
                for (int i = 0; i < Columns.Count; i++)
                {
#pragma warning disable BL0005
                    Columns[i].Index = i;
                }
            }
            return Columns!;
        }

        internal static List<GridColumn> SetStackedFixedandMovableColumns(GridColumn column)
        {
            string jsonString = JsonSerializer.Serialize(column);
            string LastLabel = "";
            List<GridColumn> MovableFixedColumn = new List<GridColumn>();
            GridColumn? StackedColumns = JsonSerializer.Deserialize<GridColumn>(jsonString);
            var ColumnsRemoveCount = column.Columns?.Count;
            if (ColumnsRemoveCount != 0)
            {
                StackedColumns?.Columns?.RemoveRange(0, (int)ColumnsRemoveCount!);
            }
            for (int j = 0; j < column.Columns?.Count; j++)
            {
                var innerColumn = column.Columns[j];
                // Refactored frozen label condition
                bool isLeafColumn = innerColumn.Columns == null;
                bool isFrozenNotEqual = innerColumn.FrozenMovableLabel != LastLabel;
                bool hasLastLabel = !string.IsNullOrEmpty(LastLabel);
                bool isNotLeftFreeze = !innerColumn.FrozenMovableLabel!.Contains("FrozenLeft", StringComparison.CurrentCulture);
                bool isNotRightFreeze = !innerColumn.FrozenMovableLabel.Contains("FrozenRight", StringComparison.CurrentCulture);
                bool shouldUpdateFrozenLabel = isLeafColumn && isFrozenNotEqual && hasLastLabel && isNotLeftFreeze && isNotRightFreeze;
                
                if (shouldUpdateFrozenLabel)
                {
                    string jsonString1 = JsonSerializer.Serialize(StackedColumns);
                    GridColumn? StackedColumns1 = JsonSerializer.Deserialize<GridColumn>(jsonString1);
                    MovableFixedColumn.Add(StackedColumns1!);
                    StackedColumns?.Columns?.Clear();
                }
                if (innerColumn.Columns == null && innerColumn.FrozenMovableLabel == "MovableFixed")
                {
                    StackedColumns?.Columns?.Add(innerColumn);
                    LastLabel = "MovableFixed";
                    if (j == ColumnsRemoveCount - 1)
                    {
                        string jsonString1 = JsonSerializer.Serialize(StackedColumns);
                        GridColumn? StackedColumns1 = JsonSerializer.Deserialize<GridColumn>(jsonString1);
                        MovableFixedColumn.Add(StackedColumns1!);
                        StackedColumns?.Columns?.Clear();
                    }
                }
                else if (innerColumn.Columns == null && innerColumn.FrozenMovableLabel == "Movable")
                {
                    StackedColumns?.Columns?.Add(innerColumn);
                    LastLabel = "Movable";
                    if (j == ColumnsRemoveCount - 1)
                    {
                        string jsonString1 = JsonSerializer.Serialize(StackedColumns);
                        GridColumn? StackedColumns1 = JsonSerializer.Deserialize<GridColumn>(jsonString1);
                        MovableFixedColumn.Add(StackedColumns1!);
                        StackedColumns?.Columns?.Clear();
                    }
                }
                else if (innerColumn.Columns != null)
                {
                    var col = SfGrid<TValue>.SetStackedFixedandMovableColumns(innerColumn);
                }

            }

            return MovableFixedColumn;
        }

        internal int SetStyleWidth(List<GridColumn> ColumnList, GridColumn CurrentColumn, FreezeDirection Direction = default)
        {
            var Width = 0;
            var WidthAdded = 0;
            if (Freeze<TValue>.IsRightFreezeColumn(CurrentColumn))
            {
                for (int i = ColumnList.Count - 1; i >= 0; i--)
                {
                    if (i == ColumnList.Count - 1 && ColumnList[i] == CurrentColumn)
                    {
                        Width += WidthAdded;
                        break;
                    }
                    else if (ColumnList[i].Field == CurrentColumn.Field && ColumnList[i].HeaderText == CurrentColumn.HeaderText)
                    {
                        Width += WidthAdded;
                        break;
                    }
                    else if (ColumnList[i].Columns != null && SfGrid<TValue>.ColumnContains(ColumnList[i].Columns!, CurrentColumn))
                    {
                        Width = SetStyleWidth(ColumnList[i].Columns!, CurrentColumn, Direction);
                        Width += WidthAdded;
                    }
                    else
                    {
                        if (ColumnList[i].Columns == null && ColumnList[i].IsFrozen && ColumnList[i].Freeze.Equals(Direction))
                        {
                            WidthAdded += GridUtils.GetParsedWidth(ColumnList[i].Width);
                        }
                        else if (ColumnList[i].Columns != null)
                        {
                            WidthAdded += GridUtils.GetStackedWidth(ColumnList[i], Direction, FrozenColumns);
                        }
                        else
                        {
                            WidthAdded += 0;
                        }
                    }

                }

            }
            else
            {
                foreach (var column in ColumnList)
                {
                    if (CurrentColumn.Index == 0 && column == CurrentColumn)
                    {
                        Width += WidthAdded;
                        break;
                    }
                    else if (column.Field == CurrentColumn.Field && column.HeaderText == CurrentColumn.HeaderText && column.Index == CurrentColumn.Index)
                    {
                        Width += WidthAdded;
                        break;
                    }
                    else if (column.Columns != null && SfGrid<TValue>.ColumnContains(column.Columns, CurrentColumn))
                    {
                        Width = SetStyleWidth(column.Columns, CurrentColumn, Direction);
                        Width += WidthAdded;
                    }
                    else
                    {
                        if (FrozenColumns > 0)
                        {
                            if (column.Columns == null)
                            {
                                WidthAdded += GridUtils.GetParsedWidth(column.Width);
                            }
                            else
                            {
                                WidthAdded += GridUtils.GetStackedWidth(column, frozenColumns: FrozenColumns);
                            }
                        }
                        else
                        {
                            if (column.Columns == null && column.IsFrozen && column.Visible && column.Freeze.Equals(Direction))
                            {
                                WidthAdded += GridUtils.GetParsedWidth(column.Width);
                            }
                            else if (column.Columns != null)
                            {
                                WidthAdded += GridUtils.GetStackedWidth(column, Direction, FrozenColumns);
                            }
                            else
                            {
                                WidthAdded += 0;
                            }
                        }
                    }
                }
            }
            return Width;
        }

        internal static bool ColumnContains(List<GridColumn> Columns, GridColumn Column)
        {
            var Contains = false;
            foreach (var column in Columns)
            {
                if (column.Field == Column.Field && column.HeaderText == Column.HeaderText && column.Index == Column.Index)
                {
                    Contains = true;
                    break;
                }
                else if (column.Columns != null)
                {
                    Contains = SfGrid<TValue>.ColumnContains(column.Columns, Column);
                    if (Contains == true)
                        return Contains;
                }
            }
            return Contains;
        }


        /// <exclude/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        async public Task PropertyChanged() => await OnParametersSetAsync().ConfigureAwait(true);

        /// <exclude/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Notify(string name, object args) => EventAggregator.Trigger(name, args);

        /// <exclude/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public async Task UpdateForeignData()
        {
            SkipLocalStorageSet = true;
            await DataProcess().ConfigureAwait(true);
        }

        internal async Task InvokeViewRefresh(List<GridColumn> columns = null!)
        {
            // Refactored complex refresh conditions
            bool hasAutoFitColumns = columns?.Any(x => x.AutoFit) == true;
            bool hasFrozenWithResizing = AllowResizing && ShowColumnChooser && ((columns?.Any(x => x.IsFrozen) == true) || FrozenColumns > 0);
            bool hasColumnVirtualization = EnableColumnVirtualization && EnableVirtualization;
            bool isNonAutoWidth = !EnableVirtualization && (Width != "auto" && Width != "100%");
            bool isAutoFitWithFlexWidth = IsAutoFitEnabled && (Width == "100%" || Width == "auto");
            
            bool shouldInvokeRefresh = hasAutoFitColumns || hasFrozenWithResizing || hasColumnVirtualization || isNonAutoWidth || isAutoFitWithFlexWidth;
            
            if (shouldInvokeRefresh)
            {
                if (EnableVirtualization)
                {
                    await InvokeMethod("sfBlazor.Grid.updateOptions", new object[] { DataId, GetClientOption() }).ConfigureAwait(true);
                }
                await InvokeMethod("sfBlazor.Grid.viewRefresh", new object[] { DataId, columns! }).ConfigureAwait(true);
            }
        }


        internal async Task InvokeSuccessAsync(object? arguments = null, string? requestType = null, bool isResetData = false)
        {
            string action = null!;
            if (FocusModule != null && FocusModule.IsByKey)
            {
                if (arguments is ActionEventArgs<TValue> arg)
                {
                    action = arg.RequestType.ToString() ?? requestType!;
                }
                else if (requestType == "Sorting" || requestType == "Save")
                {
                    action = requestType;
                }
                FocusModule.IsByKey = false;
            }
            await Task.Yield();
            if (_isRerendered)
            {
                await ClientRefresh().ConfigureAwait(true);
                _isRerendered = false;
            }
            if (IsRendered)
            {
                bool hasLazyExpandAll = EnableVirtualization && GroupModule != null && GroupModule.IsLazyExpandAll && GroupSettings?.Columns?.Length > 0;
                if (hasLazyExpandAll && GroupModule != null)
                {
                    TotalItemCount = GroupModule.LazyRows.Count;
                }
                bool isRowDragRefresh = requestType == "RowDragAndDrop" && GetClientOption().requestType == "Refresh" && VirtualScrollModule != null;
                if (isRowDragRefresh && VirtualScrollModule != null)
                {
                    VirtualScrollModule.RequestType = requestType;
                }
                bool shouldInvokeContentReady = !string.Equals(requestType, "Sorting", StringComparison.OrdinalIgnoreCase) || EnableColumnVirtualization || EnableVirtualization || EnableInfiniteScrolling;
                if (shouldInvokeContentReady)
                {
                    ActionArgs? contentReadyResults = await InvokeMethod<ActionArgs>("sfBlazor.Grid.contentReady", false, new object[] { DataId, GetClientOption(), action, isResetData }).ConfigureAwait(true);
                    if (!string.IsNullOrEmpty(contentReadyResults?.IndentWidth))
                    {
                        await RefreshIndentWidth(contentReadyResults.IndentWidth, (bool)contentReadyResults.IsRowDragCell!).ConfigureAwait(true);
                    }
                }

                bool hasEditValidationErrors = EnableVirtualization && IsEdit && !(EditModule!.IsAdd) && EditModule.ErrorResult.Count != 0;
                if (hasEditValidationErrors && EditModule != null)
                {
                    await EditModule.InvokeValidation(EditModule.ErrorResult).ConfigureAwait(true);
                }
            }
            _isColumnWidthChanged = false;
            _isColumnClipModeChanged = false;

            if (IsRendered)
            {
                await HideSpinnerAsync().ConfigureAwait(true);
            }

            if (IsRendered && EnablePersistence)
            {
                await SetLocalStorage().ConfigureAwait(true);
            }
        }

        internal async Task RefreshIndentWidth(string indentWidth = null!, bool isRowDragCell = false)
        {
            if (isRowDragCell && RowReorderModule != null)
            {
                RowReorderModule.RowReorderIndentWidth = indentWidth;
            }
            if (GroupModule != null)
                GroupModule.IndentWidth = indentWidth;
            EventAggregator.Trigger("ColumnWidthStateChange", null!);
        }

        internal async Task ClientRefresh()
        {
            await InvokeMethod("sfBlazor.Grid.destroy", DataId, _isRerendered).ContinueWith(t =>
            {
                if (GridEvents?.Destroyed.HasDelegate == true)
                    GridEvents.Destroyed.InvokeAsync(null).ConfigureAwait(false);
            }, TaskScheduler.Current).ConfigureAwait(true);

            ActionArgs? initializeResults = await InvokeMethod<ActionArgs>("sfBlazor.Grid.initialize", false, new object[]
            {
                DataId,
                _element,
                GetClientOption(),
                _jsAdaptor?.GetRef()!,
                FocusEditableCellArgs!
            }).ConfigureAwait(true);

            if (initializeResults != null)
            {
                if (!string.IsNullOrEmpty(initializeResults.IndentWidth))
                {
                    await RefreshIndentWidth(initializeResults.IndentWidth, (bool)initializeResults.IsRowDragCell!).ConfigureAwait(true);
                    if (IsMacDevice == null)
                    {
                        IsMacDevice = initializeResults.IsMacDevice;
                    }
                }
                if ((EnableVirtualization || EnableColumnVirtualization)
                    && initializeResults.RowHeight != null && VirtualScrollModule != null)
                {
                    VirtualScrollModule.RHeight = (int)initializeResults.RowHeight;
                    EventAggregator.Trigger("VirtualComponentUpdate", null!);
                }
            }
        }

        internal async Task InvokeFailureAsync(Exception exception)
        {
            await HideSpinnerAsync().ConfigureAwait(true);
            if (GridEvents?.OnActionFailure.HasDelegate == true)
                await GridEvents.OnActionFailure.InvokeAsync(new FailureEventArgs() { Error = exception, Parent = this }).ConfigureAwait(true);
            else if (IsRenderedFromTreeGrid)
                await EventAggregator.NotifyAsync("ActionFailure", new FailureEventArgs() { Error = exception, Parent = this }).ConfigureAwait(true);
        }

        internal GridClientOptions GetClientOption() => new GridClientOptions()
        {
            isWebAssembly = JSRuntime is IJSInProcessRuntime,
            height = Height,
            width = Width,
            aggregatesCount = Aggregates?.Count ?? 0,
            frozenRows = FrozenRows,
            frozenColumns = FreezeModule!.GetFrozenCount(),
            allowTextWrap = AllowTextWrap,
            wrapMode = TextWrapSettings?.WrapMode.ToString(),
            allowResizing = AllowResizing,
            enableVirtualization = EnableVirtualization,
            enableColumnVirtualization = EnableColumnVirtualization,
            enableVirtualMaskRow = EnableVirtualMaskRow,
            enableRtl = EnableRtl || SyncfusionService.options.EnableRtl,
            enableAutoFill = EnableAutoFill,
            allowReordering = AllowReordering,
            allowGrouping = AllowGrouping,
            groupReordering = GroupSettings!.AllowReordering,
            showDropArea = GroupSettings.ShowDropArea,
            groupCount = GroupSettings.Columns?.Length ?? 0,
            filterCount = FilterSettings?.Columns?.Count ?? 0,
            editMode = EditSettings?.Mode.ToString(),
            newRowPosition = EditSettings?.NewRowPosition.ToString(),
            showAddNewRow = EditSettings!.ShowAddNewRow,
            frozenCols = FreezeModule!.GetFrozenCount(),
            allowPaging = AllowPaging,
            currentPage = PageSettings!.CurrentPage,
            rowHeight = (int)RowHeight,
            pageSize = PageSettings.PageSize,
            showGroupedColumn = GroupSettings.ShowGroupedColumn,
            totalItemCount = TotalItemCount,
            needClientAction = (VirtualScrollModule != null && VirtualScrollModule.NeedClientAction),
            requestType = EnableInfiniteScrolling ? InfiniteScrollModule?.RequestType! : VirtualScrollModule?.RequestType!,
            visibleGroupedRowsCount = VisibleGroupedDataCount,
            enablePersistence = EnablePersistence,
            enableAdaptiveUI = EnableAdaptiveUI,
            offline = DataManager != null ? DataManager!.Offline : false,
            url = DataManager?.Url,
            columns = Columns,
            virtualizedColumns = VirtualScrollModule?.GetVirtualizedColumns()!,
            selectionMode = SelectionSettings?.Mode.ToString(),
            cellSelectionMode = SelectionSettings?.CellSelectionMode.ToString(),
            selectionType = SelectionSettings?.Type.ToString(),
            rowDropTarget = RowDropSettings?.TargetID,
            allowRowDragAndDrop = AllowRowDragAndDrop,
            allowDragSelection = SelectionSettings!.AllowDragSelection,
            isEdit = IsEdit,
            isAdd = EditModule!.IsAdd,
            allowEditing = EditSettings.AllowEditing,
            isPrerendered = IsDataLoaded,
            clipMode = ClipMode.ToString(),
            showColumnMenu = ShowColumnMenu,
            hasTemplateInEditSettings = EditSettings.Template != null ? true : false,
            hasDetailTemplate = ((IGrid)this).GridTemplates?.DetailTemplate != null ? true : false,
            initGroupingField = GroupSettings.Columns!,
            isColumnResized = _isColumnResized,
            frozenName = FrozenName.ToString(),
            frozenRightCount = FreezeModule!.GetFreezeRightCount(),
            frozenLeftCount = FreezeModule!.GetFreezeLeftCount(),
            frozenLeftColumnsCount = FreezeModule!.GetFreezeLeftCount(),
            frozenRightColumnsCount = FreezeModule!.GetFreezeRightColumnsCount(),
            allowFreezeLineMoving = AllowFreezeLineMoving,
            actualFrozenColumns = FrozenColumns,
            isFreezeLineMoved = IsFreezeLineMoved,
            isColumnReordered = (ReorderModule != null && ReorderModule.IsColumnReordered),
            isPreventScrollEvent = IsPreventScrollEvent,
            enableStickyHeader = EnableStickyHeader,
            enableInfiniteScrolling = EnableInfiniteScrolling,
            infiniteMaxBlocks = InfiniteScrollSettings!.MaximumBlocks,
            infiniteCacheMode = InfiniteScrollSettings.EnableCache,
            infiniteInitialBlock = InfiniteScrollSettings.InitialBlocks,
            enableLazyLoading = GroupSettings.EnableLazyLoading,
            isColumnWidthChanged = _isColumnWidthChanged,
            isClipboardEventBinded = GridEvents?.BeforeCopyPaste.HasDelegate ?? false,
            overscanCount = OverscanCount,
            customizedOverScan = VirtualScrollModule!.CalculatedOverScan,
            isRenderedFromTreeGrid = IsRenderedFromTreeGrid,
            TValue = typeof(TValue).Name,
            isColumnClipModeChanged = _isColumnClipModeChanged,
            showColumnChooser = ShowColumnChooser,
            autoFit = AutoFit,
            isFixedColumnPresent = IsFixedColumnPresent(),
            rowRenderingMode = RowRenderingMode.ToString(),
            allowEmptyAreaDrop = RowDropSettings!.AllowEmptyAreaDrop,
            isRenderedFromGantt = IsRenderedFromGantt,
            emptyCellTemplate = ((IGrid)this).GridTemplates?.EmptyRecordTemplate != null ? true : false,
        };

        private async Task PageValueChanged(Syncfusion.Blazor.Navigations.PageChangedEventArgs args)
        {
            _shouldRender = false;
            await PageSettings!.UpdateProperties("CurrentPage", (int)args.CurrentPage).ConfigureAwait(true);
            GridPageChangingEventArgs pageChangingEventArgs = new GridPageChangingEventArgs()
            {
                CurrentPage = args.CurrentPage,
                PreviousPage = args.PreviousPage,
                TotalPages = PagerRef!.TotalPages,
                CurrentPageSize = PagerRef.PageSize,
            };
            await ModelChanged(new ActionEventArgs<TValue>() { RequestType = Action.Paging, CurrentPage = args.CurrentPage, PreviousPage = args.PreviousPage }, eventArgs: pageChangingEventArgs, requestType: "Paging").ConfigureAwait(true);
            _shouldRender = true;
        }

        private async Task DropDownChanged(PageSizeChangedArgs args)
        {
            _shouldRender = false;
            await PageSettings!.UpdateProperties("PageSize", (int)args.CurrentPageSize).ConfigureAwait(true);
            await PageSettings.UpdateProperties("CurrentPage", (int)args.CurrentPage).ConfigureAwait(true);
            GridPageChangingEventArgs pageChangingEventArgs = new GridPageChangingEventArgs()
            {
                CurrentPage = args.CurrentPage,
                TotalPages = args.TotalPages,
                CurrentPageSize = args.CurrentPageSize,
            };
            await ModelChanged(new ActionEventArgs<TValue>() { RequestType = Action.Paging, CurrentPage = PageSettings.CurrentPage }, eventArgs: pageChangingEventArgs, requestType: "Paging").ConfigureAwait(true); // TODO previous page sent in client
            _shouldRender = true;
        }

        #endregion

        #region Data & State Management

        /// <summary>
        /// Orchestrates grid state changes and triggers appropriate event handlers for all actions.
        /// Manages action validation, event sequencing, and data pipeline coordination.
        /// </summary>
        /// <param name="args">The action event arguments containing request type and data context.</param>
        /// <param name="additionalArgs">Additional context arguments for the specific action.</param>
        /// <param name="suppressEvent">If true, suppresses firing of ActionBegin and other events.</param>
        /// <param name="isDeleteAction">Indicates whether the current action is a delete operation.</param>
        /// <param name="requestType">The string identifier of the request type (e.g., "Paging", "Sorting").</param>
        /// <param name="eventArgs">Additional event-specific arguments (paging, sorting, filtering, etc.).</param>
        /// <param name="isSavingTriggered">Indicates whether the save operation was externally triggered.</param>
        /// <param name="temporaryIndentWidth">Temporary indent width for group hierarchies during operations.</param>
        /// <param name="groupedKey">Dictionary containing grouped key information for group-aware operations.</param>
        internal async Task ModelChanged(ActionEventArgs<TValue>? args = null, object? additionalArgs = null, bool suppressEvent = false, bool isDeleteAction = false, string? requestType = null, object? eventArgs = null, bool isSavingTriggered = false, string? temporaryIndentWidth = null, Dictionary<object, object>? groupedKey = null)
        {
            var additionalArgument = additionalArgs;
            SelectionModule!.ClonedSelectedRowRecords = isDeleteAction && requestType == "Delete" && args!.Data != null ? new List<TValue>() { (TValue)args.Data } : SelectedRecords;
            bool cancel = false;
            if (args != null)
            {
                args.Parent = this;
            }

            if (!suppressEvent)
            {
                string? searchString = args?.SearchString;
                await InvokeActionBeginEvents(args!).ConfigureAwait(true);
                cancel = await InvokeRequestTypeSpecificEvents(requestType, eventArgs, searchString, isSavingTriggered).ConfigureAwait(true);
                await HandleSearchStringUpdateIfNeeded(args!, searchString, requestType).ConfigureAwait(true);
                await PreventFilterQuery(args!, eventArgs: eventArgs, requestType: requestType).ConfigureAwait(true);
            }

            if (args!.Cancel || cancel)
            {
                await CancelBegin(args, eventArgs: eventArgs!, requestType: requestType!, temporaryIndentWidth: temporaryIndentWidth!).ConfigureAwait(true);
                if (EditSettings != null && EditSettings.ShowAddNewRow)
                {
                    //Preventing Disable and Reset addform when records are updated with cancel args
                    if (requestType != "Save")
                    {
                        EventAggregator.Trigger("DisableOrEnableAddForm", null!);
                        EventAggregator.Trigger("ResetAddFormValues", "Cancel");
                    }
                }
                return;
            }
            else
            {
                // Refactored pager navigation validation
                bool hasActionBegin = GridEvents?.OnActionBegin.HasDelegate == true || IsRenderedFromTreeGrid;
                bool isPageChanged = PageSettings!.CurrentPage != args.CurrentPage;
                bool hasPagerRef = PagerRef != null;
                bool isValidPageNumber = hasPagerRef && args.CurrentPage <= PagerRef?.TotalPages && args.CurrentPage >= 1;
                bool shouldNavigatePage = hasActionBegin && isPageChanged && hasPagerRef && isValidPageNumber;
                
                if (shouldNavigatePage)
                {
                    await PageSettings.UpdateProperties("CurrentPage", args.CurrentPage).ConfigureAwait(true);
                }

                await HandleEditDialogCloseIfNeeded().ConfigureAwait(true);
                await ClearSelectionIfNotPersistent().ConfigureAwait(true);

                if (IsEdit)
                {
                    PreventRender(false);
                }
                if (await HandleBatchEditStateConflict(args, additionalArgument, requestType).ConfigureAwait(true))
                {
                    return;
                }

                await ProcessRequestAndRefreshIfNeeded(args, additionalArgument, requestType, isDeleteAction, groupedKey, eventArgs).ConfigureAwait(true);
            }
        }

        private async Task PreventFilterQuery(ActionEventArgs<TValue> args, object? eventArgs = null, string? requestType = null)
        {
            // Refactored filtering column check
            bool isFilteringAction = args.RequestType.Equals(Syncfusion.Blazor.Grids.Action.Filtering) || requestType == "Filtering";
            bool hasCurrentFilteringColumn = args.CurrentFilteringColumn != null && FilterSettings?.Columns != null;
            
            if (isFilteringAction && hasCurrentFilteringColumn)
            {
                FilteringEventArgs? filteringEventArgs = eventArgs as FilteringEventArgs;
                var fGridcolumn = await GetColumnByFieldAsync(args?.CurrentFilteringColumn ?? filteringEventArgs!.ColumnName!).ConfigureAwait(true);
                if (fGridcolumn != null  && FilterSettings?.Columns != null)
                {
                    fGridcolumn.PreventFilterQuery = args?.PreventFilterQuery ?? filteringEventArgs!.PreventFilterQuery;
                    foreach (var col in FilterSettings.Columns)
                    {
                        if (col.Field == fGridcolumn.Field || col.Field == fGridcolumn.ForeignKeyValue)
                        {
                            col.PreventFilterQuery = fGridcolumn.PreventFilterQuery;
                        }
                    }
                    FilteredColumns = FilterSettings.Columns.ToList();
                }
            }
            
            // Refactored refresh with filter template check
            bool isRefreshAction = (args != null && args.RequestType == Action.Refresh) || requestType == "Refresh";
            bool hasFilterTemplates = Columns != null && Columns.Where(f => f.FilterTemplate != null).Any();
            bool shouldUpdateFilteredColumns = isRefreshAction && hasFilterTemplates && FilterSettings?.Columns != null;
            
            if (shouldUpdateFilteredColumns)
            {
                FilteredColumns = FilterSettings?.Columns?.ToList();
            }
            
            var filteredCols = FilterSettings?.Columns?.Where(col => col.PreventFilterQuery).ToList();
            if (filteredCols != null)
            {
                foreach (var col in filteredCols)
                {
                    FilterSettings?.Columns?.Remove(col);
                }
            }
        }

        /// <summary>
        /// Invokes the OnActionBegin and ActionBegin events for the current action.
        /// </summary>
        private async Task InvokeActionBeginEvents(ActionEventArgs<TValue> args)
        {
            args.Parent = this;
            await SfBaseUtils.InvokeEvent<ActionEventArgs<TValue>>(GridEvents?.OnActionBegin!, args).ConfigureAwait(true);
            await EventAggregator.NotifyAsync("ActionBegin", args).ConfigureAwait(true);
        }

        /// <summary>
        /// Invokes request-type-specific events and returns cancellation status.
        /// </summary>
        private async Task<bool> InvokeRequestTypeSpecificEvents(string? requestType, object? eventArgs, string? searchString, bool isSavingTriggered = false)
        {
            bool cancel = false;

            switch (requestType)
            {
                case "Paging":
                    cancel = await InvokePagingEvent(eventArgs).ConfigureAwait(true);
                    break;
                case "Sorting":
                    cancel = await InvokeSortingEvent(eventArgs).ConfigureAwait(true);
                    break;
                case "Grouping":
                case "UnGrouping":
                    cancel = await InvokeGroupingEvent(eventArgs).ConfigureAwait(true);
                    break;
                case "Searching":
                    cancel = await InvokeSearchingEvent(eventArgs, searchString).ConfigureAwait(true);
                    break;
                case "Save":
                    cancel = await InvokeSaveEvent(eventArgs, isSavingTriggered).ConfigureAwait(true);
                    break;
                case "Delete":
                    cancel = await InvokeDeleteEvent(eventArgs).ConfigureAwait(true);
                    break;
                case "Reorder":
                    cancel = await InvokeReorderEvent(eventArgs).ConfigureAwait(true);
                    break;
                case "ColumnState":
                    cancel = await InvokeColumnStateEvent(eventArgs).ConfigureAwait(true);
                    break;
                case "Filtering":
                    cancel = await InvokeFilteringEvent(eventArgs).ConfigureAwait(true);
                    break;
                case "ClearFiltering":
                    cancel = await InvokeClearFilteringEvent(eventArgs).ConfigureAwait(true);
                    break;
            }

            return cancel;
        }

        /// <summary>
        /// Invokes the PageChanging event for paging operations.
        /// </summary>
        private async Task<bool> InvokePagingEvent(object? eventArgs)
        {
            GridPageChangingEventArgs? pagingEventArgs = eventArgs as GridPageChangingEventArgs;
            await EventAggregator.NotifyAsync("PageChanging", pagingEventArgs!).ConfigureAwait(true);
            if (GridEvents?.PageChanging.HasDelegate == true)
            {
                await GridEvents.PageChanging.InvokeAsync(pagingEventArgs).ConfigureAwait(true);
            }
            return (pagingEventArgs != null && pagingEventArgs.Cancel);
        }

        /// <summary>
        /// Invokes the Sorting event for sorting operations.
        /// </summary>
        private async Task<bool> InvokeSortingEvent(object? eventArgs)
        {
            SortingEventArgs? sortingEventArgs = eventArgs as SortingEventArgs;
            sortingEventArgs!.Parent = this;
            await EventAggregator.NotifyAsync("Sorting", sortingEventArgs).ConfigureAwait(true);
            if (GridEvents?.Sorting.HasDelegate == true)
            {
                await GridEvents.Sorting.InvokeAsync(sortingEventArgs).ConfigureAwait(true);
            }
            return sortingEventArgs.Cancel;
        }

        /// <summary>
        /// Invokes the Grouping event for grouping and ungrouping operations.
        /// </summary>
        private async Task<bool> InvokeGroupingEvent(object? eventArgs)
        {
            GroupingEventArgs? groupingEventArgs = eventArgs as GroupingEventArgs;
            groupingEventArgs!.Parent = this;
            if (GridEvents?.Grouping.HasDelegate == true)
            {
                await GridEvents.Grouping.InvokeAsync(groupingEventArgs).ConfigureAwait(true);
                return groupingEventArgs.Cancel;
            }
            return false;
        }

        /// <summary>
        /// Invokes the Searching event and updates search settings if needed.
        /// </summary>
        private async Task<bool> InvokeSearchingEvent(object? eventArgs, string? originalSearchString)
        {
            SearchingEventArgs? searchBeginEventArgs = eventArgs as SearchingEventArgs;
            searchBeginEventArgs!.Parent = this;
            await EventAggregator.NotifyAsync("Searching", searchBeginEventArgs).ConfigureAwait(true);
            if (GridEvents?.Searching.HasDelegate == true)
            {
                await GridEvents.Searching.InvokeAsync(searchBeginEventArgs).ConfigureAwait(true);
            }
            bool cancel = searchBeginEventArgs.Cancel;
            if (searchBeginEventArgs.SearchText != SearchSettings?.Key && !string.IsNullOrEmpty(SearchSettings?.Key) && !string.IsNullOrEmpty(searchBeginEventArgs.SearchText))
            {
                await SearchSettings.UpdateProperties("Key", searchBeginEventArgs.SearchText).ConfigureAwait(true);
            }
            return cancel;
        }

        /// <summary>
        /// Invokes the RowUpdating event for save operations.
        /// </summary>
        private async Task<bool> InvokeSaveEvent(object? eventArgs, bool isSavingTriggered)
        {
            RowUpdatingEventArgs<TValue>? savingEventArgs = eventArgs as RowUpdatingEventArgs<TValue>;
            savingEventArgs!.Parent = this;
            if (GridEvents?.RowUpdating.HasDelegate == true || IsRenderedFromTreeGrid)
            {
                if (!isSavingTriggered)
                {
                    if (IsRenderedFromTreeGrid)
                        await EventAggregator.NotifyAsync("RowUpdating", savingEventArgs).ConfigureAwait(true);
                    else
                        await (GridEvents?.RowUpdating.InvokeAsync(savingEventArgs))!.ConfigureAwait(true)!;
                }
                return savingEventArgs.Cancel;
            }
            return false;
        }

        /// <summary>
        /// Invokes the RowDeleting event for delete operations.
        /// </summary>
        private async Task<bool> InvokeDeleteEvent(object? eventArgs)
        {
            RowDeletingEventArgs<TValue>? deletingEventArgs = eventArgs as RowDeletingEventArgs<TValue>;
            deletingEventArgs!.Parent = this;
            if (GridEvents?.RowDeleting.HasDelegate == true)
            {
                await GridEvents.RowDeleting.InvokeAsync(deletingEventArgs).ConfigureAwait(true);
            }
            await EventAggregator.NotifyAsync("RowDeleting", deletingEventArgs).ConfigureAwait(true);
            return deletingEventArgs.Cancel;
        }

        /// <summary>
        /// Invokes the ColumnReordering event for column reorder operations.
        /// </summary>
        private async Task<bool> InvokeReorderEvent(object? eventArgs)
        {
            ColumnReorderingEventArgs? reorderingEventArgs = eventArgs as ColumnReorderingEventArgs;
            reorderingEventArgs!.Parent = this;
            await EventAggregator.NotifyAsync("ColumnReordering", reorderingEventArgs).ConfigureAwait(true);
            if (GridEvents?.ColumnReordering.HasDelegate == true)
            {
                await GridEvents.ColumnReordering.InvokeAsync(reorderingEventArgs).ConfigureAwait(true);
            }
            return reorderingEventArgs.Cancel;
        }

        /// <summary>
        /// Invokes the ColumnVisibilityChanging event for column state changes.
        /// </summary>
        private async Task<bool> InvokeColumnStateEvent(object? eventArgs)
        {
            ColumnVisibilityChangingEventArgs? columnChangingEventArgs = eventArgs as ColumnVisibilityChangingEventArgs;
            columnChangingEventArgs!.Parent = this;
            await EventAggregator.NotifyAsync("ColumnsVisibilityChanging", columnChangingEventArgs).ConfigureAwait(true);
            if (GridEvents?.ColumnVisibilityChanging.HasDelegate == true)
            {
                await GridEvents.ColumnVisibilityChanging.InvokeAsync(columnChangingEventArgs).ConfigureAwait(true);
            }
            return columnChangingEventArgs.Cancel;
        }

        /// <summary>
        /// Invokes the Filtering event for filtering operations.
        /// </summary>
        private async Task<bool> InvokeFilteringEvent(object? eventArgs)
        {
            FilteringEventArgs? filteringEventArgs = eventArgs as FilteringEventArgs;
            filteringEventArgs!.Parent = this;
            await EventAggregator.NotifyAsync("Filtering", filteringEventArgs).ConfigureAwait(true);
            if (GridEvents != null && GridEvents.Filtering.HasDelegate)
            {
                await GridEvents.Filtering.InvokeAsync(filteringEventArgs).ConfigureAwait(true);
            }
            return filteringEventArgs.Cancel;
        }

        /// <summary>
        /// Invokes the Filtering event for clear filtering operations.
        /// </summary>
        private async Task<bool> InvokeClearFilteringEvent(object? eventArgs)
        {
            FilteringEventArgs clearFilteringEventArgs = (eventArgs as FilteringEventArgs)!;
            clearFilteringEventArgs.Parent = this;
            await EventAggregator.NotifyAsync("Filtering", clearFilteringEventArgs).ConfigureAwait(true);
            if (GridEvents != null && GridEvents.Filtering.HasDelegate)
            {
                await GridEvents.Filtering.InvokeAsync(clearFilteringEventArgs).ConfigureAwait(true);
            }
            return clearFilteringEventArgs.Cancel;
        }

        /// <summary>
        /// Handles updating search settings if search string changed after event firing.
        /// </summary>
        private async Task HandleSearchStringUpdateIfNeeded(ActionEventArgs<TValue> args, string? originalSearchString, string? requestType)
        {
            // Refactored search string change detection
            bool isSearchRequest = args != null && args.RequestType.Equals(Action.Searching);
            bool hasSearchChanged = originalSearchString != args?.SearchString;
            bool hadPreviousSearch = !string.IsNullOrEmpty(originalSearchString);
            bool shouldResetSearch = isSearchRequest && hasSearchChanged && hadPreviousSearch;
            
            if (shouldResetSearch)
            {
                await SearchSettings!.UpdateProperties("Key", args?.SearchString!).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Closes the edit dialog if it's open during model changes.
        /// </summary>
        private async Task HandleEditDialogCloseIfNeeded()
        {
            if (EditModule!.EditDialogInstance != null && EditSettings?.Dialog != null && EditSettings.Dialog.AnimationEffect != null)
            {
                await EditModule.EditDialogInstance.HideAsync().ConfigureAwait(true);
                await Task.Delay(250).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Clears selection if persistence is disabled and selection exists.
        /// </summary>
        private async Task ClearSelectionIfNotPersistent()
        {
            if (!SelectionSettings!.PersistSelection && SelectedRecords.Count > 0)
            {
                // Refactored row reorder mode check
                bool isObservableCollection = DataSource?.GetType() == typeof(ObservableCollection<TValue>);
                bool isRowDragEnabled = AllowRowDragAndDrop;
                bool hasRowReorderModule = RowReorderModule != null;
                bool isUserReordering = RowReorderModule?.IsReorderByInteraction == true;
                bool shouldSkipReorderRefresh = isObservableCollection && isRowDragEnabled && hasRowReorderModule && isUserReordering;
                
                if (!shouldSkipReorderRefresh)
                {
                    await ClearSelectionAsync().ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Handles batch edit state conflicts and displays confirmation dialog if needed.
        /// </summary>
        private async Task<bool> HandleBatchEditStateConflict(ActionEventArgs<TValue> args, object? additionalArgument, string? requestType)
        {
            // Refactored batch edit state check
            bool hasBatchChanges = EditModule!.HasBatchChanges;
            bool isNonReorderRequest = requestType != "RowDragAndDrop";
            bool shouldCheckBatchConflict = hasBatchChanges && isNonReorderRequest;
            
            bool hasDeletedOrAddedDuringDrag = requestType == "RowDragAndDrop" && EditModule.HasBatchChanges && (Rows.Any(x => x.Action == EditAction.Deleted) || Rows.Any(x => x.Action == EditAction.Added));
            
            bool isReorderDuringBatchEdit = IsEdit && EditSettings?.Mode == EditMode.Batch && requestType == "Reorder";
            
            bool shouldShowConfirmDialog = shouldCheckBatchConflict || hasDeletedOrAddedDuringDrag || isReorderDuringBatchEdit;
            
            if (shouldShowConfirmDialog)
            {
                if (await EditModule.HandleBatchChangesWithConfirmDialog(args, additionalArgument, requestType).ConfigureAwait(true))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Processes the request and triggers data refresh if required.
        /// </summary>
        private async Task<bool> ProcessRequestAndRefreshIfNeeded(ActionEventArgs<TValue> args, object? additionalArgument, string? requestType, bool isDeleteAction, Dictionary<object, object>? groupedKey, object? eventArgs)
        {
            var RequestType = args?.RequestType.ToString() ?? requestType;
            var RequireRefresh = await Request(args!, additionalArgument!, eventArgs: eventArgs!, requestType: requestType!).ConfigureAwait(true);

            // Refactored virtual scroll cache check
            bool shouldResetVirtualCache = EnableVirtualization && VirtualScrollModule != null;
            
            if (shouldResetVirtualCache && VirtualScrollModule != null)
            {
                await VirtualScrollModule.CheckAndResetCache(RequestType!).ConfigureAwait(true);
            }
            
            if (RequireRefresh)
            {
                // Refactored drag-drop with virtualization check
                bool isRefreshDuringDragVirtual = args != null && args.RequestType.Equals(Action.Refresh) && DataSource != null && DataSource is INotifyPropertyChanged && AllowRowDragAndDrop && EnableVirtualization && VirtualScrollModule != null;
                
                if (isRefreshDuringDragVirtual && VirtualScrollModule != null)
                {
                    await VirtualScrollModule.CheckAndResetCache(RequestType!).ConfigureAwait(true);
                }
                
                await DataProcess(actionArgs: args, eventArgs: eventArgs, requestType: requestType, isDeleteAction: isDeleteAction, groupedKey: groupedKey).ConfigureAwait(true);
                
                if (SelectionModule != null)
                {
                    SelectionModule.UpdatePersistSelectionState(requestType!);
                }
            }

            if (ForceUpdate)
            {
                await CallStateHasChangedAsync().ConfigureAwait(true);
            }

            return RequireRefresh;
        }

        #endregion

        #region Request Processing

        /// <summary>
        /// Handles query data actions (Searching, Paging, Sorting, Grouping, Filtering, Refresh).
        /// </summary>
        private async Task<bool> HandleQueryDataActions(string? requestType, ActionEventArgs<TValue> args, object? eventArgs, SortingEventArgs? sortingEvent)
        {
            bool RequireRefresh = false;

            if (requestType == "Searching" || requestType == "Paging" || requestType == "Sorting" || requestType == "Grouping" || requestType == "UnGrouping" || requestType == "Filtering" || requestType == "ClearFiltering" || requestType == "Refresh")
            {
                IsEdit = false;
                EditModule!.ClearRules();
                RequireRefresh = true;
                if (requestType == "UnGrouping")
                {
                    args!.Direction = SortDirection.None;
                }
                if (eventArgs != null && eventArgs is SortingEventArgs sortingEventLocal && (GridEvents?.Sorting.HasDelegate == true || IsRenderedFromTreeGrid))
                {
                    SortModule?.HandleSortingEvent(sortingEventLocal);
                }
                else if (args != null && (GridEvents?.OnActionBegin.HasDelegate == true || IsRenderedFromTreeGrid) && args.ColumnName != null && AllowSorting && SortSettings != null)
                {
                    SortModule?.HandleSortAction(args.ColumnName, args.Direction);
                }
                if (EnableInfiniteScrolling && InfiniteScrollModule != null)
                {
                    InfiniteScrollModule.RequestType = requestType;
                    await InfiniteScrollModule.ResetInfiniteProperties(requestType).ConfigureAwait(true);
                }
                if (SelectionModule != null && (requestType == "Searching" || requestType == "Refresh"))
                {
                    SelectionModule.ResetHeaderCheckboxOnSearchingAndRefresh(requestType);
                }
                if (requestType == "Refresh" && EnableVirtualization && AllowGrouping && !(GroupSettings?.EnableLazyLoading == true))
                {
                    IsExpanded = GroupSettings!.ExpandAllGroups && !IsExpanded;
                }
            }

            return RequireRefresh;
        }

        /// <summary>
        /// Handles CRUD actions (Save, Delete) and coordinates data module updates.
        /// </summary>
        private async Task<bool> HandleCRUDActions(string? requestType, ActionEventArgs<TValue> args, object? eventArgs)
        {
            bool RequireRefresh = false;

            if (requestType == "Save" || requestType == "Delete")
            {
                if ((GridEvents?.OnActionBegin.HasDelegate == true || IsRenderedFromTreeGrid) || eventArgs is RowUpdatingEventArgs<TValue> || eventArgs is RowDeletingEventArgs<TValue>)
                {
                    RequireRefresh = await (DataModule?.GetData(args!, eventArgs: eventArgs, requestType: requestType switch
                    {
                        "Save" => "Save",
                        "Delete" => "Delete",
                        _ => null!
                    }))!.ConfigureAwait(true)!;
                    if (EnableInfiniteScrolling && InfiniteScrollModule != null)
                    {
                        InfiniteScrollModule.RequestType = requestType;
                        await InfiniteScrollModule.ResetInfiniteProperties(requestType).ConfigureAwait(true);
                    }
                    if (eventArgs is RowUpdatingEventArgs<TValue> savingArgs)
                    {
                        savingArgs.Cancel = !RequireRefresh;
                    }
                    else if (eventArgs is RowDeletingEventArgs<TValue> deletingArgs)
                    {
                        deletingArgs.Cancel = !RequireRefresh;
                    }
                    else
                    {
                        args!.Cancel = !RequireRefresh;
                    }
                }
            }

            return RequireRefresh;
        }

        /// <summary>
        /// Handles column reordering and related module state updates.
        /// </summary>
        private async Task<bool> HandleReorderAction(object? additionalArgs)
        {
            bool RequireRefresh = false;

            if (ReorderModule != null)
                await ReorderModule.PerformReorder((additionalArgs as ActionArgs)!).ConfigureAwait(true);
            await EditModule!.HandleBatchEditDuringReorder().ConfigureAwait(true);
            if (EnableVirtualization)
            {
                var startIndex = VirtualScrollModule!.VirtualIndexes(VirtualScrollModule.RowStartIndex, VirtualScrollModule.RowEndIndex).startIndex;
                VirtualScrollModule.SetReorderCurrentData(startIndex, CurrentViewData!);
            }
            if (EnableInfiniteScrolling && InfiniteScrollModule != null)
            {
                InfiniteScrollModule.RequestType = "Reorder";
                await InfiniteScrollModule.ResetInfiniteProperties("Reorder").ConfigureAwait(true);
                RequireRefresh = true;
            }
            if (EditSettings?.ShowAddNewRow == true)
            {
                EditModule!.ClearRules();
                IsEdit = false;
            }
            ForceUpdate = true;
            SoftRefresh = false;

            return RequireRefresh;
        }

        /// <summary>
        /// Handles row drag-and-drop action and infinite scroll updates.
        /// </summary>
        private async Task<bool> HandleRowDragDropAction()
        {
            bool RequireRefresh = true;
            ForceUpdate = true;
            if (EnableInfiniteScrolling && InfiniteScrollModule != null)
            {
                InfiniteScrollModule.RequestType = "RowDragAndDrop";
                await InfiniteScrollModule.ResetInfiniteProperties("RowDragAndDrop").ConfigureAwait(true);
            }

            return RequireRefresh;
        }

        /// <summary>
        /// Updates module states and UI after request processing.
        /// </summary>
        private async Task<bool> UpdateModulesAfterRequest(string? requestType, bool requireRefresh, object eventArgs = null!)
        {
            if (GroupModule != null && AllowGrouping && GroupSettings != null && GroupSettings.EnableLazyLoading && AllowPaging && (requestType == "Grouping" || requestType == "UnGrouping" || requestType == "Filtering" || requestType == "ClearFiltering" || requestType == "Searching" || requestType == "Save" || requestType == "Sorting" || requestType == "Delete"))
            {
                GroupModule.LazyPageSetting();
            }
            if (requestType == "Paging" && SelectionModule != null)
            {
                SelectionModule.RefreshSelectionOnPaging();
            }
            if (requestType == "Paging" && (GroupSettings?.EnableLazyLoading == true) && GroupSettings.Columns?.Length > 0)
            {
                requireRefresh = false;
                List<object> uiData = new List<object>();
                var startIndex = (PageSettings!.PageSize) * (PageSettings.CurrentPage - 1);
                uiData = GroupModule?.GetUiData(CurrentViewData!)!;
                var totalDataRow = uiData.Take(startIndex);
                int totalDataRowsCount = totalDataRow.Where(row => !(row is Row<object>) && !(row is Group<TValue>)).Count();
                List<object> currentUiData = uiData.Skip(startIndex).Take(PageSettings.PageSize).ToList();
                this.Rows = GroupModule?.GenerateLazyRowsobject(currentUiData, totalDataRowsCount)!;
                EventAggregator.Trigger("ContentStateChanged", null!);
                if (eventArgs is GridPageChangingEventArgs pagingEventArgs)
                {
                    GridPageChangedEventArgs pageEventArgs = new GridPageChangedEventArgs()
                    {
                        CurrentPage = pagingEventArgs.CurrentPage,
                        PreviousPage = pagingEventArgs.PreviousPage,
                        CurrentPageSize = pagingEventArgs.CurrentPageSize,
                        TotalPages = pagingEventArgs.TotalPages,
                    };
                    await EventAggregator.NotifyAsync("PageChanged", pageEventArgs).ConfigureAwait(true);
                    if (GridEvents?.PageChanged.HasDelegate == true)
                    {
                        await GridEvents.PageChanged.InvokeAsync(pageEventArgs).ConfigureAwait(true);
                    }
                }
            }
            if (EditSettings!.ShowAddNewRow && requestType != "Save" && requestType != null)
            {
                EventAggregator.Trigger("DisableOrEnableAddForm", null!);
                EventAggregator.Trigger("ResetAddFormValues", requestType);
            }
            if (requestType == "ColumnState")
            {
                if (IsEdit)
                {
                    await EditModule!.CloseEdit().ConfigureAwait(true);
                }
                if (FreezeModule!.GetFrozenCount() > 0 && AllowGrouping && GroupSettings?.Columns?.Length > 0 && GroupModule != null)
                {
                    GroupModule.IndentWidth = string.Empty;
                }
            }
            return requireRefresh;
        }

        /// <summary>
        /// Processes grid action requests and determines whether data refresh is required.
        /// Routes different action types to appropriate handlers and coordinates module responses.
        /// </summary>
        /// <param name="args">The action event arguments containing request type and data context.</param>
        /// <param name="additionalArgs">Additional context arguments specific to the action type.</param>
        /// <param name="eventArgs">Event-specific arguments (sorting, filtering, etc.).</param>
        /// <param name="requestType">The string identifier of the request type (e.g., "Paging", "Sorting", "Save").</param>
        /// <returns>
        /// True if data refresh is required; false if no refresh needed (e.g., for cancelled operations).
        /// </returns>
        internal async Task<bool> Request(ActionEventArgs<TValue> args, object additionalArgs = null!, object eventArgs = null!, string requestType = null!)
        {
            var requireRefresh = false;
            var RequestType = args?.RequestType.ToString() ?? requestType;
            SortingEventArgs? sortingEvent = eventArgs as SortingEventArgs;
            if(args != null)
            switch (RequestType)
            {
                case "Searching":
                case "Paging":
                case "Sorting":
                case "Grouping":
                case "UnGrouping":
                case "Filtering":
                case "ClearFiltering":
                case "Refresh":
                    requireRefresh = await HandleQueryDataActions(RequestType, args, eventArgs, sortingEvent).ConfigureAwait(true);
                    break;
                case "Save":
                case "Delete":
                    requireRefresh = await HandleCRUDActions(RequestType, args, eventArgs).ConfigureAwait(true);
                    break;
                case "Reorder":
                    requireRefresh = await HandleReorderAction(additionalArgs).ConfigureAwait(true);
                    break;
                case "RowDragAndDrop":
                    requireRefresh = await HandleRowDragDropAction().ConfigureAwait(true);
                    break;
            }

            requireRefresh = await UpdateModulesAfterRequest(RequestType, requireRefresh, eventArgs).ConfigureAwait(true);
            return requireRefresh;
        }

        #endregion

        #region Action Cancellation

        /// <summary>
        /// Handles cancellation logic when user-triggered events return Cancel = true.
        /// Reverts state changes such as sorting, grouping, or column visibility.
        /// </summary>
        /// <param name="args">The action event arguments containing request type and context.</param>
        /// <param name="eventArgs">Event-specific arguments (grouping, sorting, etc.).</param>
        /// <param name="requestType">The string identifier of the request type (e.g., "Grouping", "Sorting").</param>
        /// <param name="temporaryIndentWidth">Temporary indent width to restore if cancelling grouping.</param>
        internal async Task CancelBegin(ActionEventArgs<TValue> args, object eventArgs = null!, string requestType = null!, string temporaryIndentWidth = null!)
        {
            var actionType = args?.RequestType.ToString() ?? requestType;
            if(args != null)
            {
                switch (actionType)
                {
                    case "Grouping":
                        await HandleGroupingCancellation(args, eventArgs).ConfigureAwait(true);
                        break;
                    case "UnGrouping":
                        await HandleUnGroupingCancellation(args, eventArgs, temporaryIndentWidth).ConfigureAwait(true);
                        break;
                    case "Sorting":
                        await HandleSortingCancellation().ConfigureAwait(true);
                        break;
                    case "Paging":
                        await HandlePagingCancellation(args, eventArgs).ConfigureAwait(true);
                        break;
                }
            }
            

            await CallStateHasChangedAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Reverts grouping cancellation by removing column from group, updating visibility, and clearing sort.
        /// </summary>
        private async Task HandleGroupingCancellation(ActionEventArgs<TValue> args, object eventArgs)
        {
            var GCols = GroupSettings?.Columns?.ToList() ?? new List<string>();
            var columnName = args?.ColumnName;

            if (GridEvents?.Grouping.HasDelegate == true && eventArgs is GroupingEventArgs beforeGroupingEventArgs && beforeGroupingEventArgs.Cancel && beforeGroupingEventArgs.ColumnName != null)
                columnName = beforeGroupingEventArgs.ColumnName;

            var SCol = SortSettings?.Columns?.Find(col => col.Field == columnName);
            GCols.Remove(columnName!);
            SortSettings?.Columns?.Remove(SCol!);
            GridUtils.GetColumns(this).Find(_ => _.Field.Equals(columnName, StringComparison.Ordinal))?.SetVisibility(true);
            await GroupSettings!.UpdateProperties("Columns", GCols.ToArray()).ConfigureAwait(true);
            RefreshColumnHeader = true;
        }

        /// <summary>
        /// Reverts ungrouping cancellation by adding column back to group and updating state.
        /// </summary>
        private async Task HandleUnGroupingCancellation(ActionEventArgs<TValue> args, object eventArgs, string temporaryIndentWidth)
        {
            var GCols = GroupSettings?.Columns?.ToList() ?? new List<string>();
            var columnName = args?.ColumnName;

            if (GridEvents?.Grouping.HasDelegate == true && eventArgs is GroupingEventArgs beforeUnGroupingEventArgs && beforeUnGroupingEventArgs.Cancel && beforeUnGroupingEventArgs.ColumnName != null)
                columnName = beforeUnGroupingEventArgs.ColumnName;

            GCols.Add(columnName!);
            SortModule?.GroupAddSortingQuery(columnName!);
            await GroupSettings!.UpdateProperties("Columns", GCols.ToArray()).ConfigureAwait(true);
            RefreshColumnHeader = true;

            if (args != null && args.Cancel)
            {
                HasColumnChanges = true;
                GroupModule!.IndentWidth = temporaryIndentWidth;
            }
        }

        /// <summary>
        /// Reverts sorting cancellation by restoring previous sort columns.
        /// </summary>
        private async Task HandleSortingCancellation()
        {
            if (SortSettings != null)
                await SortSettings.UpdateProperties("Columns", SortModule!.LastSortedCols!).ConfigureAwait(true);
        }

        /// <summary>
        /// Reverts paging cancellation by restoring previous page.
        /// </summary>
        private async Task HandlePagingCancellation(ActionEventArgs<TValue> args, object eventArgs)
        {
            var previousPage = 0;

            if ((GridEvents?.PageChanging.HasDelegate == true || IsRenderedFromTreeGrid || IsRenderedFromFileManager) && eventArgs is GridPageChangingEventArgs pagingEventArgs)
                previousPage = pagingEventArgs.PreviousPage >= 1 ? (int)pagingEventArgs.PreviousPage : 0;
            else
                previousPage = (args != null && args.PreviousPage >= 1) ? (int)args.PreviousPage : 0;

            if (previousPage >= 1)
                await PageSettings!.UpdateProperties("CurrentPage", previousPage).ConfigureAwait(true);
        }

        #endregion

        #region Property Management

        /// <summary>
        /// Updates component properties dynamically and synchronizes dependent state.
        /// </summary>
        /// <param name="propertyName">The name of the property to update (e.g., "Columns", "SelectedRowIndex").</param>
        /// <param name="newValue">The new value to apply to the property.</param>
        /// <remarks>
        /// This method ensures property changes are properly reflected in the grid's internal state,
        /// triggering necessary refreshes and re-renders when required.
        /// </remarks>
        internal async Task UpdateProperties(string propertyName, object newValue)
        {
            switch (propertyName)
            {
                case "Columns":
                    var columns = DirectParameters.TryGetValue("Columns", out object? value) ? (object)value : Columns;
                    Columns = _columns = (List<GridColumn>)await UpdateProperty(nameof(Columns), (object)newValue, columns!).ConfigureAwait(true);
                    break;
                case "SelectedRowIndex":
                    SelectedRowIndex = _selectedRowIndex = await UpdateProperty(nameof(SelectedRowIndex), _selectedRowIndex, (int)newValue).ConfigureAwait(true);
                    break;
            }
        }

        internal static bool IsColumnsLoaded(List<GridColumn> columns)
        {
            var isLoaded = false;
            foreach (GridColumn column in columns ?? new List<GridColumn>())
            {
                if (column.HasChild && column.Commands != null)
                {
                    return true;
                }
                else if (column.HasChild)
                {
                    if (column.Columns == null)
                    {
                        isLoaded = false;
                        break;
                    }
                    else
                    {
                        isLoaded = SfGrid<TValue>.IsColumnsLoaded(column.Columns);
                        if (isLoaded == false)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    isLoaded = true;
                }
            }

            return isLoaded;
        }

        /// <summary>
        /// Add data annotation to given column.
        /// </summary>
        /// <param name="column">Grid column component.</param>
        /// <exclude/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void AnnotateColumn(GridColumn column)
            => GridAnnotation.MapAnnotation(column ?? null!, typeof(TValue));

        #endregion

        #region Render State Management

        /// <summary>
        /// Invokes a synchronous state change notification to trigger component re-rendering.
        /// Respects PreventStateChange flag to allow render optimization.
        /// </summary>
        /// <remarks>
        /// This method is called when synchronous UI updates are needed.
        /// The PreventStateChange flag can be used to suppress unnecessary renders for performance.
        /// </remarks>
        /// <exclude/>
        public void CallStateHasChanged()
        {
            if (PreventStateChange)
            {
                PreventStateChange = false;
                return;
            }

            StateHasChanged();
        }

        /// <summary>
        /// Invokes an asynchronous state change notification to trigger component re-rendering.
        /// Executes within the Blazor synchronization context.
        /// </summary>
        /// <remarks>
        /// Async version ensures state changes are processed within the proper Blazor context,
        /// preventing cross-thread issues and ensuring proper event sequencing.
        /// The PreventStateChange flag can be used to suppress unnecessary renders for performance.
        /// </remarks>
        /// <exclude/>
        public async Task CallStateHasChangedAsync()
        {
            if (PreventStateChange)
            {
                PreventStateChange = false;
                return;
            }

            await InvokeAsync(StateHasChanged).ConfigureAwait(true);
        }

        #endregion

        #region Column Management & Utilities

        /// <summary>
        /// Add sort column while grouping.
        /// </summary>
        /// <exclude/>
        public void AddSortColumn(string colName) => SortModule?.SortedColumns.Add(colName);

        /// <summary>
        /// Set value while editing using column field.
        /// </summary>
        /// <exclude/>
        public void SetValue<T>(T value, string field)
        {
            if (field != null)
            {
                EditModule!.SetValue<T>(value, field);
            }
        }

        /// <summary>
        /// Handles dispose component.
        /// </summary>
        /// <remarks>
        /// Destroyed event will be invoked if any. Set current state of grid in window.localStorage if EnablePersistence is set as true.
        /// </remarks>
        internal override void ComponentDispose()
        {
            CollectionDisposeMethod(CurrentViewData!);
            UpdateObservableEvents(nameof(DataSource), DataSource, true);
            _jsAdaptor?.Dispose();
            if (IsRendered)
            {
                if (EnablePersistence)
                {
                    SetLocalStorage().GetAwaiter();
                }

                InvokeMethod("sfBlazor.Grid.destroy", DataId, _isRerendered).ContinueWith(t =>
                {
                    if (GridEvents?.Destroyed.HasDelegate == true)
                        InvokeAsync(() => GridEvents.Destroyed.InvokeAsync(null)).ConfigureAwait(false);
                }, TaskScheduler.Current);
            }
        }

        /// <summary>
        /// Sets button options for command button
        /// </summary>
        internal Dictionary<string, object> SetButtonOptions(GridCommandColumn column)
        {
            Dictionary<string, object> ButtonOptions = new Dictionary<string, object>();
            if (column?.ButtonOption != null)
            {
                if (!string.IsNullOrEmpty(column?.ButtonOption.Content))
                {
                    ButtonOptions.Add("Content", column.ButtonOption.Content);
                }

                if (!column!.ButtonOption.IconPosition.Equals(Syncfusion.Blazor.Buttons.IconPosition.Left))
                {
                    ButtonOptions.Add("IconPosition", column?.ButtonOption.IconPosition!);
                }

                ButtonOptions.Add("Disabled", column?.ButtonOption.Disabled ?? false);
                ButtonOptions.Add("EnableRtl", column?.ButtonOption.EnableRtl ?? false);
                ButtonOptions.Add("IsPrimary", column?.ButtonOption.IsPrimary ?? false);
                ButtonOptions.Add("IsToggle", column?.ButtonOption.IsToggle ?? false);
            }

            string TitleAttr = string.Empty;
            if (column?.Title != null)
            {
                TitleAttr = column.Title;
            }
            else if (column?.Type.ToString() != "None")
            {
                var colType = Localizer?.GetText("Grid_" + column?.Type.ToString());
                TitleAttr = (colType == null) ? column?.Type.ToString()! : colType;
            }

            ButtonOptions.Add("title", TitleAttr!);
            return ButtonOptions;
        }
        internal bool IsDataModified(Row<object> Row, object CloneData, GridCommandColumn column)
        {
            bool isModifiedData = false;
            if (column?.Type == CommandButtonType.Delete || column?.Type == CommandButtonType.Edit || column?.Type == CommandButtonType.None)
            {
                return isModifiedData;
            }
            List<Cell<object>> dataCells = Row?.Cells?.Where(_e => _e.Visible && _e.IsDataCell)?.ToList()!;
            foreach (var cell in dataCells)
            {
                object originalValue = PropHelper?.GetObject(cell?.Column?.Field!, Row?.Data!)!;
                object clonedValue = EditModule?.CloneData != null ? PropHelper?.GetObject(cell?.Column?.Field!, CloneData)! : null!;
                isModifiedData = GridUtils.CompareValues<object>(originalValue, clonedValue);
                if (isModifiedData)
                {
                    return isModifiedData;
                }
            }

            return isModifiedData;
        }

        #endregion

        #region Exception Handling
        private bool HandleException(Exception exception)
        {
            _ = NotifyExceptionAsync(exception);
            return true;
        }

        private async Task NotifyExceptionAsync(Exception exception)
        {
            await InvokeFailureAsync(exception).ConfigureAwait(false);
        }
        #endregion
    }
}
