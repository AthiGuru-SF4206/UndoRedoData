using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Syncfusion.Blazor.Internal;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids.Internal;
using System.Linq;
using System.Runtime.CompilerServices;



namespace Syncfusion.Blazor.Grids
{
    public partial class SfGrid<TValue> : SfDataBoundComponent, IGrid, ISfCircularComponent
    {
        private bool _isObservableWired { get; set; }

        internal bool _rowIndexPropertyChanged { get; set; }


        /// <summary>
        /// Initializes the grid component, sets up hybrid initialization, and configures all required modules.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await OnHybridInitialized().ConfigureAwait(true);
            DetailRowModule = new DetailRow<TValue>(this);
            ReorderModule = new Reorder<TValue>(this);
            DataModule = new DataGenerator<TValue>(this);
            ReactiveAggregateModule = new ReactiveAggregate<TValue>(this);
            ForeignKeyModule = new ForeignKey<TValue>(this);
            SearchModule = new Searching<TValue>(this);
            VirtualScrollModule = new VirtualScroll<TValue>(this);
            InfiniteScrollModule = new InfiniteScroll<TValue>(this);
            SortModule = new Sort<TValue>(this);
            GroupModule = new Grouping<TValue>(this);
            FilterModule = new Filter<TValue>(this);
            SelectionModule = new Selection<TValue>(this);
            EditModule = new Edit<TValue>(this);
            PageModule = new Paging<TValue>(this);
            FreezeModule = new Freeze<TValue>(this);
            FocusModule = new FocusHandler<TValue>(this);
            RowReorderModule = new RowReorder<TValue>(this);
            _jsAdaptor = new GridJSInteropAdaptor<TValue>(this);
            PropHelper = new PropertyInfoHelper<TValue>();
            ScriptModules = SfScriptModules.SfGrid;
			MergeModule = new MergeHandler<TValue>(this);
            
            // Initialize UndoRedo manager with parent reference
            if (UndoRedoManager == null)
            {
                UndoRedoManager = new UndoRedoManager<TValue>(this);
            }
            if (string.IsNullOrEmpty(ColumnMenuClass))
            {
                ColumnMenuClass = $"e-hide-menu e-{ID}-column-menu e-grid-column-menu e-grid-menu";
            }
            if(this.Columns != null)
            {
                HasColumnChanges = true;
            }
            _isLoaded = true;
            // Telemetry event for grid initialization
            GridTelemetryHelper.LogTelemetry(false, "");
        }

        /// <summary>
        /// Handles unmatched attributes and caches them when the table class is not applied.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            if (UnMatchedAttributes != null)
            {
                if (!TableClass)
                {
                    foreach (var item in UnMatchedAttributes)
                    {
                        if (item.Key.Equals("style", StringComparison.OrdinalIgnoreCase) || item.Key.Equals("data-sf-style", StringComparison.OrdinalIgnoreCase))
                        {
                          
                            if (!_cachedAttributes.ContainsKey("data-sf-style"))
                            {
                                _cachedAttributes["data-sf-style"] = item.Value?.ToString() ?? "";
                            }
                            else
                            {
                                _cachedAttributes["data-sf-style"] += item.Value?.ToString();
                            }
                        }
                        else
                        {
                            if(item.Value != null)
                            {
                                _cachedAttributes.AddOrUpdateItem(item.Key, item.Value);
                            }
                        }
                    }
                }
            }
	    var query = _query ?? new Query();
            if (!object.ReferenceEquals(DataSource, _dataSource))
            {
                UpdateObservableEvents(nameof(DataSource), _dataSource, true);
                UpdateObservableEvents(nameof(DataSource), DataSource);
            }

            await OnHybridParametersSet().ConfigureAwait(true);

            // For query, forced comparison is made to avoid Hierarchy grid issue.
            if (PropertyChanges.ContainsKey("Query"))
            {
                if (Query.IsEqual(query, Query ?? new Query()))
                {
                    PropertyChanges.Remove("Query");
                }
            }

            //Clear persist data on data source change
            if (PropertyChanges.ContainsKey(nameof(DataSource)))
            {
                Rows?.ForEach(_ => 
                    { 
                        _.IsSelected = false; 
                        _.Cells?.ForEach(c => c.IsSelected = false);
                    });
                CheckBoxState = CheckState.UnCheck;
                if(SelectionModule != null)
                {
                    SelectionModule.IsHeaderCheckboxChecked = false;
                    SelectionModule.SetPersistData(state: CheckBoxState);
                }
                _rowIndexPropertyChanged = false;
            }

            if (DataSource != null && !_isObservableWired)
            {
                UpdateObservableEvents(nameof(DataSource), DataSource, true);
                UpdateObservableEvents(nameof(DataSource), DataSource);
                _isObservableWired = true;
            }
            if (PropertyChanges.ContainsKey("AllowRowDragAndDrop") && RowReorderModule != null)
            {
                RowReorderModule.RowReorderIndentWidth = string.Empty;
            }

            if (PropertyChanges.ContainsKey("AllowRowDragAndDrop") && RowReorderModule != null)
            {
                RowReorderModule.RowReorderIndentWidth = string.Empty;
            }

            // Handle server side property change or external property change
            if (PropertyChanges.Count > 0)
            {
                bool isNeedClientFrozenHeight = false;
                if(VirtualScrollModule != null)
                {
                    VirtualScrollModule.IsDataSourceChanged = IsRendered && PropertyChanges.ContainsKey("DataSource");
                }
                bool headerRef = PropertyChanges.ContainsKey("AllowGrouping")
                    || PropertyChanges.ContainsKey("GroupSettings")
                    || PropertyChanges.ContainsKey("AllowSorting")
                    || PropertyChanges.ContainsKey("SortSettings")
                    || PropertyChanges.ContainsKey("AllowRowDragAndDrop")
                    || PropertyChanges.ContainsKey("Columns")
                    || PropertyChanges.ContainsKey("AllowFiltering")
                    || PropertyChanges.ContainsKey("ShowColumnMenu");
                if(PropertyChanges.ContainsKey("FilterSettings") && PropertyChanges.ContainsKey("ShowFilterBarOperator"))
                {
                    EventAggregator.Trigger("FilterBarComponentUpdate", null!);
                }
                RefreshColumnHeader = headerRef;
                var keys = PropertyChanges.Keys.Select(x => x).ToList();
                IsColumnPropertyChanged = PropertyChanges.ContainsKey(nameof(Columns));
                if (PropertyChanges.ContainsKey("Columns") || PropertyChanges.ContainsKey(nameof(DataSource)))
                {
                    MinWidth?.Clear();
                }
                if ((PropertyChanges.ContainsKey("Width") || PropertyChanges.ContainsKey("Height")) && ((int)FrozenColumns) > 0)
                {

                    RefreshColumnHeader = true;
                    RefreshFrozenHeader = true;                   
                    isNeedClientFrozenHeight = true;
                }
                if (PropertyChanges.ContainsKey("EnableVirtualization") || PropertyChanges.ContainsKey("FrozenColumns") || (EnableVirtualization && PropertyChanges.ContainsKey("RowHeight")))
                {
                    if (PropertyChanges.ContainsKey("EnableVirtualization") && PropertyChanges.GetValueOrDefault("EnableVirtualization")!.ToString()!.Equals("False", StringComparison.Ordinal))
                    {
                        await InvokeMethod("sfBlazor.Grid.virtualDisconnect", new object[] { DataId, GetClientOption() }).ConfigureAwait(true);
                    }
                    _isRerendered = true;
                    RefreshFrozenHeader = true;
                }
                if (PropertyChanges.ContainsKey("ColumnWidth"))
                {
                    _isColumnWidthChanged = true;
                }
                if (PropertyChanges.ContainsKey("GroupSettings") && GroupSettings?.Columns?.Length > 0 && !string.IsNullOrEmpty(GroupModule?.IndentWidth) && !PropertyChanges.ContainsKey("PersistGroupState"))
                {
                    GroupModule!.IndentWidth = "";
                }
                if (PropertyChanges.ContainsKey("ColumnClipMode")) {
                    _isColumnClipModeChanged = true;
                }
                if (PropertyChanges.ContainsKey("SelectedRowIndex") && this.AddOrDeleteArgs?.RequestType != Action.Delete)
                {
                    _rowIndexPropertyChanged = true;
                }
                if (SelectedRowIndexes?.Count > 0 && PropertyChanges.ContainsKey("RowSelectionModeChanged"))
                {
                    await ClearRowSelectionAsync().ConfigureAwait(true);
                }
                else if (PropertyChanges.ContainsKey("CellSelectionModeChanged"))
                {
                    await ClearCellSelectionAsync().ConfigureAwait(true);
                }
                else if(PropertyChanges.ContainsKey("BothSelectionModeChanged"))
                {
                    await ClearCellSelectionAsync().ConfigureAwait(true);
                    await ClearRowSelectionAsync().ConfigureAwait(true);
                }
                if (PropertyChanges.ContainsKey("PersistGroupState"))
                {
                    GroupStates.Clear();
                }
                PropertyChanges.Clear();
                if (!Reset && (keys.Any(p => GridUtils.IsRefreshable(p) || string.Equals(p, nameof(AutoSpan), StringComparison.Ordinal)) || isNeedClientFrozenHeight))
                {
                    await ModelChanged(new ActionEventArgs<TValue>() { RequestType = Action.Refresh }).ConfigureAwait(true);
                }
            }
            EnsurePagerDropdown();
        }


        /// <summary>
        /// Invoked after the component has rendered. Handles grid model refresh and triggers the model change event if required.
        /// </summary>
        /// <param name="firstRender">Indicates whether this is the first render of the component.</param>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (isGridModelRefresh)
            {
                isGridModelRefresh = false;
                await InvokeAsync(async () => await (ModelChanged(new ActionEventArgs<TValue>() { RequestType = Action.Refresh })).ConfigureAwait(true)).ConfigureAwait(true);               
            }
            if (_requireDataBoundInvoke && IsClientInitialized)
            {
                _requireDataBoundInvoke  = false;
                await Task.Yield(); //To get the proper grid instance for client - side actions

                if (GridEvents?.DataBound.HasDelegate == true)
                    await GridEvents.DataBound.InvokeAsync(null).ConfigureAwait(true);
                else if(IsRenderedFromTreeGrid)
                    await (EventAggregator?.NotifyAsync("DataBoundMock", null!)!).ConfigureAwait(true);
                else if(IsRenderedFromPivotTable)
                    EventAggregator?.Trigger("PivotDataBound", null!);
            }
            
            if (AddOrDeleteArgs != null)
            {
                var addDeleteArgs = AddOrDeleteArgs;
                IsDeleteAction = addDeleteArgs.Action == "Delete";
                AddOrDeleteArgs = null;
                if (IsRenderedFromTreeGrid)
                    await Task.Run(() => { }).ConfigureAwait(true);
                await (EditModule?.EditComplete(addDeleteArgs)!).ConfigureAwait(true);    
            }

            HasColumnChanges = false;
            IsColumnHideOrShow = false;
            _shouldRender = true;
            if(ReorderModule != null)
            ReorderModule.IsColumnReordered = false;
            EditModule!.ClearSelection = this.SelectionSettings != null && !this.SelectionSettings.PersistSelection;
            EnsureFeaturesCompatibility();
            if (SoftRefresh)
            {
                SoftRefresh = false;
            }

            SetColumnValueType();
            if (_setOnce && firstRender)
            {
                _setOnce = false;
                _originalProp = SerializeModel(this);
            }

            if (SelectionModule != null)
            {
                await SelectionModule.InitializeRowSelection(firstRender).ConfigureAwait(true);
            }

            if (!IsClientInitialized && Columns != null && GroupSettings != null && GroupSettings.Columns?.Length > 0 && !GroupSettings.ShowGroupedColumn)
            {
                List<GridColumn> templateColumns = Columns.Where(x => (x.Template != null || x.HeaderTemplate != null || x.FilterItemTemplate != null || x.FilterTemplate != null || x.EditTemplate != null) && x.Visible).ToList();
                if(templateColumns.Count > 0)
                {
                    foreach (var column in templateColumns)
                    {
                        if (GroupSettings.Columns?.Contains(column.Field) == true)
                        {
                            var visible = GroupSettings.ShowGroupedColumn ? (column.IsHiddenByGrouping ? true : column.Visible) :
                                GroupSettings.ShowGroupedColumn;
                            column.SetVisibility(visible);
                        }
                    }
                } 
            }
            await base.OnAfterRenderAsync(firstRender).ConfigureAwait(true);
        }

        internal override async Task OnAfterScriptRendered()
        {
            _jsAdaptor?.Init();
            _hasSpinner = true;
            if (!IsDataLoaded)
            {
                EventAggregator?.Trigger("InitialLoad", this);
                if (GridEvents?.OnLoad.HasDelegate == true)
                    await GridEvents.OnLoad.InvokeAsync(null).ConfigureAwait(true);

                await (EventAggregator?.NotifyAsync("OnLoadMock", null!)!).ConfigureAwait(true);
            }

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
                if (IsMacDevice == null)
                {
                    IsMacDevice = initializeResults.IsMacDevice;
                }
                if (!string.IsNullOrEmpty(initializeResults.IndentWidth))
                {
                    await RefreshIndentWidth(initializeResults.IndentWidth, (bool)initializeResults.IsRowDragCell!).ConfigureAwait(true);
                }
                if ((EnableVirtualization || EnableColumnVirtualization)
                && initializeResults.RowHeight != null && VirtualScrollModule != null)
                {
                    VirtualScrollModule.RHeight = (int)initializeResults.RowHeight;
                    EventAggregator.Trigger("VirtualComponentUpdate", null!);
                }
            }

            Reset = IsSetPersistDataCalled;
            if (EnablePersistence && !IsDataLoaded)
            {
                var LocalStorage = await InvokeMethod<string>("window.localStorage.getItem", false, new object[] { $"grid{ID}" }).ConfigureAwait(true);
                await PersistProperties(LocalStorage).ConfigureAwait(true);
            }

            if (!IsDataLoaded)
            {
                IsDataLoaded = true;
                if (VirtualScrollModule != null &&(!VirtualScrollModule.NeedClientAction || (VirtualScrollModule.NeedClientAction && EnablePersistence)))
                {
                    await DataProcess().ConfigureAwait(true);
                    var foreignkeyColumns = Columns != null ? ForeignKey<TValue>.GetForeignKeyColumnsAsync(Columns) : null;
                    if (EnablePersistence && (_isPersistAutoFit || _isColumnResized))
                    {
                        await InvokeMethod("sfBlazor.Grid.autoFitColumns", new object[] { DataId, Columns!, _targetColumns, _isPersistAutoFit, _isColumnResized }).ConfigureAwait(true);
                    }
                    else if (Columns != null && Columns.Any(col => col.AutoFit && col.Visible) && (foreignkeyColumns?.Count > 0 || (Aggregates?.Count > 0)))
                    {
                        var fields = Columns.Where(col => col.AutoFit && col.Visible).Select(col => col.Field).ToArray();
                        await AutoFitColumnsAsync(fields).ConfigureAwait(true);
                    }
                }
            }

            SetColumnValueType();
            EventAggregator?.Trigger("InternalDataBound", null!);

            if (GridEvents?.Created.HasDelegate == true)
                await GridEvents.Created.InvokeAsync(null).ConfigureAwait(true);
            else if(IsRenderedFromTreeGrid || IsRenderedFromFileManager)
                await (EventAggregator?.NotifyAsync("CreatedMock", null!)!).ConfigureAwait(true);


            if (_requireDataBoundInvoke  && !IsClientInitialized)
            {
                _requireDataBoundInvoke  = false;
                
                if (GridEvents?.DataBound.HasDelegate == true)
                    await GridEvents.DataBound.InvokeAsync(null).ConfigureAwait(true);
                else if(IsRenderedFromTreeGrid && EventAggregator != null)
                    await EventAggregator.NotifyAsync("DataBoundMock", null!).ConfigureAwait(true);
                else if(IsRenderedFromPivotTable)
                EventAggregator?.Trigger("PivotDataBound", null!);
            }

            IsClientInitialized = true;
            if(RefreshPivotRowHeight)
            {
                await InvokeMethod("sfBlazor.Grid.refreshPivotRowHeight", new object[] { DataId }).ConfigureAwait(true);
            }
            if (SyncfusionService.IsDeviceMode)
            {
                if (FreezeModule!.GetFrozenCount() > 0 && AllowTextWrap)
                {
                    await InvokeMethod("sfBlazor.Grid.refreshPivotRowHeight", new object[] { DataId }).ConfigureAwait(true);
                }
            }
            if (Height == "100%" && PageSettings != null && PageSettings.PageSizes != null)
            {
                await InvokeMethod("sfBlazor.Grid.refreshGridPageSize", new object[] { DataId }).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Initializes hybrid grid settings and synchronizes internal state with component parameters.
        /// </summary>
        protected async Task OnHybridInitialized()
        {

            await base.OnInitializedAsync().ConfigureAwait(true);
            _aggregates = Aggregates;
            _allowExcelExport = AllowExcelExport;
            _allowFiltering = AllowFiltering;
            _allowGrouping = AllowGrouping;
            _allowMultiSorting = AllowMultiSorting;
            _allowPaging = AllowPaging;
            _allowPdfExport = AllowPdfExport;
            _allowReordering = AllowReordering;
            _allowResizing = AllowResizing;
            _allowRowDragAndDrop = AllowRowDragAndDrop;
            _allowSelection = AllowSelection;
            _allowSorting = AllowSorting;
            _allowTextWrap = AllowTextWrap;
            _overscanCount = OverscanCount;

            _clipMode = ClipMode;
            UpdateChildProperties(nameof(ColumnChooserSettings), ColumnChooserSettings!);
            _columnMenuItems = ColumnMenuItems;
            _columnQueryMode = ColumnQueryMode;
            _columns = Columns;
            _contextMenuItems = ContextMenuItems;
            _currentAction = CurrentAction;
            _dataSource = DataSource;

            UpdateChildProperties(nameof(EditSettings), EditSettings!);
            _enableAltRow = EnableAltRow;
            _enableAutoFill = EnableAutoFill;
            _enableColumnVirtualization = EnableColumnVirtualization;
            _enableHover = EnableHover;
            _enablePersistence = EnablePersistence;
            _enableRtl = EnableRtl;
            _enableVirtualization = EnableVirtualization;
            _enableVirtualMaskRow = EnableVirtualMaskRow;
            UpdateChildProperties(nameof(FilterSettings), FilterSettings!);
            _enableInfiniteScrolling = EnableInfiniteScrolling;
            UpdateChildProperties(nameof(InfiniteScrollSettings), InfiniteScrollSettings!);
            _frozenColumns = FrozenColumns;
            _frozenRows = FrozenRows;
            _gridLines = GridLines;
            UpdateChildProperties(nameof(GroupSettings), GroupSettings!);
            _height = Height;
            _hierarchyPrintMode = HierarchyPrintMode;
            UpdateChildProperties(nameof(PageSettings), PageSettings!);
            _printMode = PrintMode;
            _query = Query;
            UpdateChildProperties(nameof(RowDropSettings), RowDropSettings!);
            _rowHeight = RowHeight;
            UpdateChildProperties(nameof(SearchSettings), SearchSettings!);
            _selectedRowIndex = SelectedRowIndex;
            UpdateChildProperties(nameof(SelectionSettings), SelectionSettings!);
            _showColumnChooser = ShowColumnChooser;
            _showColumnMenu = ShowColumnMenu;
            UpdateChildProperties(nameof(SortSettings), SortSettings!);
            UpdateChildProperties(nameof(TextWrapSettings), TextWrapSettings!);
            UpdateChildProperties(nameof(KeySettings), KeySettings!);
            _toolbar = Toolbar;
            _width = Width;
            _autoFit = AutoFit;
            _showTooltip = ShowTooltip;
			_allowFreezeLineMoving = AllowFreezeLineMoving;
            _enableStickyHeader = EnableStickyHeader;
            _autoSpan = AutoSpan;

                if (Columns != null)
            {
                IsAutoGeneratedColumns = true;
            }

            if (string.IsNullOrEmpty(ID))
            {
                ID = $"sfgrid{System.IO.Path.GetRandomFileName().Replace(".", string.Empty, StringComparison.Ordinal)}";
            }
        }

        /// <summary>
        /// Handles parameter updates for hybrid grid mode and synchronizes the data source when changes occur.
        /// </summary>
        protected async Task OnHybridParametersSet()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);
            if ((_dataSource == null && DataSource != null) ||
                (_dataSource != null && DataSource == null) ||
                (_dataSource != null && DataSource != null && !_dataSource.Equals(DataSource)) ||
                (_dataSource != null && DataSource != null &&
                ((!_dataSource.Any() && DataSource.Any()) || (_dataSource.Any() && !DataSource.Any())))) // Handles delayed data assigning.
            {
                PropertyChanges.TryAdd(nameof(DataSource), DataSource);
                DataSource = _dataSource = DataSource!;
            }

            Aggregates = _aggregates = await UpdateProperty(nameof(Aggregates), Aggregates, _aggregates!).ConfigureAwait(true);
            AllowExcelExport = _allowExcelExport = await UpdateProperty(nameof(AllowExcelExport), AllowExcelExport, _allowExcelExport).ConfigureAwait(true);
            AllowFiltering = _allowFiltering = await UpdateProperty(nameof(AllowFiltering), AllowFiltering, _allowFiltering).ConfigureAwait(true);
            AllowGrouping = _allowGrouping = await UpdateProperty(nameof(AllowGrouping), AllowGrouping, _allowGrouping).ConfigureAwait(true) ;
            AllowMultiSorting = _allowMultiSorting = await UpdateProperty(nameof(AllowMultiSorting), AllowMultiSorting, _allowMultiSorting).ConfigureAwait(true);
            AllowPaging = _allowPaging = await UpdateProperty(nameof(AllowPaging), AllowPaging, _allowPaging).ConfigureAwait(true);
            AllowPdfExport = _allowPdfExport = await UpdateProperty(nameof(AllowPdfExport), AllowPdfExport, _allowPdfExport).ConfigureAwait(true);
            AllowReordering = _allowReordering = await UpdateProperty(nameof(AllowReordering), AllowReordering, _allowReordering).ConfigureAwait(true);
            AllowResizing = _allowResizing = await UpdateProperty(nameof(AllowResizing), AllowResizing, _allowResizing).ConfigureAwait(true);
            AllowRowDragAndDrop = _allowRowDragAndDrop = await UpdateProperty(nameof(AllowRowDragAndDrop), AllowRowDragAndDrop, _allowRowDragAndDrop).ConfigureAwait(true);
            AllowSelection = _allowSelection = await UpdateProperty(nameof(AllowSelection), AllowSelection, _allowSelection).ConfigureAwait(true);
            AllowSorting = _allowSorting = await UpdateProperty(nameof(AllowSorting), AllowSorting, _allowSorting).ConfigureAwait(true);
            AllowTextWrap = _allowTextWrap = await UpdateProperty(nameof(AllowTextWrap), AllowTextWrap, _allowTextWrap).ConfigureAwait(true);
            ClipMode = _clipMode = await UpdateProperty(nameof(ClipMode), ClipMode, _clipMode).ConfigureAwait(true);
            ColumnChooserSettings = _columnChooserSettings = await UpdateProperty(nameof(ColumnChooserSettings), ColumnChooserSettings, _columnChooserSettings).ConfigureAwait(true);
            ColumnMenuItems = _columnMenuItems = await UpdateProperty(nameof(ColumnMenuItems), ColumnMenuItems, _columnMenuItems!).ConfigureAwait(true);
            ColumnQueryMode = _columnQueryMode = await UpdateProperty(nameof(ColumnQueryMode), ColumnQueryMode, _columnQueryMode).ConfigureAwait(true);
            if (PivotColumns != null && PivotColumns.Count > 0 && PivotColumns.Count != Columns!.Count && IsRendered)
            {
                _columns = Columns;
            }
            else
            {
                Columns = _columns = await UpdateProperty(nameof(Columns), Columns, _columns!).ConfigureAwait(true);
            }
            ContextMenuItems = _contextMenuItems = await UpdateProperty(nameof(ContextMenuItems), ContextMenuItems, _contextMenuItems!).ConfigureAwait(true);
            CurrentAction = _currentAction = await UpdateProperty(nameof(CurrentAction), CurrentAction, _currentAction!).ConfigureAwait(true);

            DataSource = (_dataSource = await UpdateProperty(nameof(DataSource), DataSource, _dataSource, DataSourceChanged).ConfigureAwait(true))!;
            EditSettings = _editSettings = await UpdateProperty(nameof(EditSettings), EditSettings, _editSettings!).ConfigureAwait(true);
            EnableAltRow = _enableAltRow = await UpdateProperty(nameof(EnableAltRow), EnableAltRow, _enableAltRow).ConfigureAwait(true);
            EnableAutoFill = _enableAutoFill = await UpdateProperty(nameof(EnableAutoFill), EnableAutoFill, _enableAutoFill).ConfigureAwait(true);
            EnableColumnVirtualization = _enableColumnVirtualization = await UpdateProperty(nameof(EnableColumnVirtualization), EnableColumnVirtualization, _enableColumnVirtualization).ConfigureAwait(true);
            EnableHover = _enableHover = await UpdateProperty(nameof(EnableHover), EnableHover, _enableHover).ConfigureAwait(true);
            EnablePersistence = _enablePersistence = await UpdateProperty(nameof(EnablePersistence), EnablePersistence, _enablePersistence).ConfigureAwait(true);
            EnableRtl = _enableRtl = await UpdateProperty(nameof(EnableRtl), EnableRtl, _enableRtl).ConfigureAwait(true) || SyncfusionService.options.EnableRtl;
            EnableVirtualization = _enableVirtualization = await UpdateProperty(nameof(EnableVirtualization), EnableVirtualization, _enableVirtualization).ConfigureAwait(true);
            EnableVirtualMaskRow = _enableVirtualMaskRow = await UpdateProperty(nameof(EnableVirtualMaskRow), EnableVirtualMaskRow, _enableVirtualMaskRow).ConfigureAwait(true);
            OverscanCount = _overscanCount = await UpdateProperty(nameof(OverscanCount), OverscanCount, _overscanCount).ConfigureAwait(true);
            FilterSettings = _filterSettings = await UpdateProperty(nameof(FilterSettings), FilterSettings, _filterSettings!).ConfigureAwait(true);
            FrozenColumns = _frozenColumns = await UpdateProperty(nameof(FrozenColumns), FrozenColumns, _frozenColumns).ConfigureAwait(true);
            FrozenRows = _frozenRows = await UpdateProperty(nameof(FrozenRows), FrozenRows, _frozenRows).ConfigureAwait(true);
            GridLines = _gridLines = await UpdateProperty(nameof(GridLines), GridLines, _gridLines).ConfigureAwait(true);
            GroupSettings = _groupSettings = await UpdateProperty(nameof(GroupSettings), GroupSettings, _groupSettings!).ConfigureAwait(true);
            Height = (_height = await UpdateProperty(nameof(Height), Height, _height).ConfigureAwait(true))!;
            HierarchyPrintMode = _hierarchyPrintMode = await UpdateProperty(nameof(HierarchyPrintMode), HierarchyPrintMode, _hierarchyPrintMode).ConfigureAwait(true);
            PageSettings = _pageSettings = await UpdateProperty(nameof(PageSettings), PageSettings, _pageSettings!).ConfigureAwait(true);
            PrintMode = _printMode = await UpdateProperty(nameof(PrintMode), PrintMode, _printMode).ConfigureAwait(true);
            Query = _query = await UpdateProperty(nameof(Query), Query, _query!).ConfigureAwait(true);
            RowDropSettings = _rowDropSettings = await UpdateProperty(nameof(RowDropSettings), RowDropSettings, _rowDropSettings!).ConfigureAwait(true);
            RowHeight = _rowHeight = await UpdateProperty(nameof(RowHeight), RowHeight, _rowHeight).ConfigureAwait(true);
            SearchSettings = _searchSettings = await UpdateProperty(nameof(SearchSettings), SearchSettings, _searchSettings!).ConfigureAwait(true);
            ShowTooltip = _showTooltip = await UpdateProperty(nameof(ShowTooltip), ShowTooltip, _showTooltip).ConfigureAwait(true);
            if (!SfBaseUtils.Equals(SelectedRowIndex, _selectedRowIndex))
            {
                SelectedRowIndex = _selectedRowIndex = SelectedRowIndex;
                SfBaseUtils.UpdateDictionary("SelectedRowIndex", SelectedRowIndex, PropertyChanges);
            }
            SelectionSettings = _selectionSettings = await UpdateProperty(nameof(SelectionSettings), SelectionSettings, _selectionSettings!).ConfigureAwait(true);
            ShowColumnChooser = _showColumnChooser = await UpdateProperty(nameof(ShowColumnChooser), ShowColumnChooser, _showColumnChooser).ConfigureAwait(true);
            ShowColumnMenu = _showColumnMenu = await UpdateProperty(nameof(ShowColumnMenu), ShowColumnMenu, _showColumnMenu).ConfigureAwait(true);
            SortSettings = _sortSettings = await UpdateProperty(nameof(SortSettings), SortSettings, _sortSettings!).ConfigureAwait(true);
            InfiniteScrollSettings = _infiniteScrollSettings = await UpdateProperty(nameof(InfiniteScrollSettings), InfiniteScrollSettings, _infiniteScrollSettings!).ConfigureAwait(true);
            TextWrapSettings = _textWrapSettings = await UpdateProperty(nameof(TextWrapSettings), TextWrapSettings, _textWrapSettings!).ConfigureAwait(true);
            Toolbar = _toolbar = await UpdateProperty(nameof(Toolbar), Toolbar, _toolbar!).ConfigureAwait(true);

            Width = _width = await UpdateProperty(nameof(Width), Width, _width!).ConfigureAwait(true);
            AutoSpan = _autoSpan = await UpdateProperty(nameof(AutoSpan), AutoSpan, _autoSpan).ConfigureAwait(true);
            AutoFit = _autoFit = await UpdateProperty(nameof(AutoFit), AutoFit, _autoFit).ConfigureAwait(true);
            KeySettings = _keySettings = await UpdateProperty(nameof(KeySettings), KeySettings, _keySettings!).ConfigureAwait(true);
			AllowFreezeLineMoving = _allowFreezeLineMoving = await UpdateProperty(nameof(AllowFreezeLineMoving), AllowFreezeLineMoving, _allowFreezeLineMoving).ConfigureAwait(true);
            EnableStickyHeader = _enableStickyHeader = await UpdateProperty(nameof(EnableStickyHeader), EnableStickyHeader, _enableStickyHeader).ConfigureAwait(true);
            SetDataManager<TValue>(DataSource);
            if (DataManager != null && DataManager.DataAdaptor!= null && DataManager.Adaptor.Equals(Adaptors.BlazorAdaptor) && DataSource == null && !DataManager.DataAdaptor.IsRemote() && DataManager.Json == null) 
            {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
                DataManager.Json = new List<object>();
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
            }

            // Enable UndoRedoManager if configured in EditSettings
            if (EditSettings != null && 
                EditSettings.EnableUndoRedo && 
                EditSettings.Mode == EditMode.Batch && 
                UndoRedoManager != null && 
                !UndoRedoManager.IsEnabled)
            {
                UndoRedoManager.Enable(EditSettings.UndoRedoLimit);
            }
        }
    }
}
