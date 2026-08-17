using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Internal;
using Syncfusion.ExcelExport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles virtual scrolling feature.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class VirtualScroll<T>
    {
        #region Private Fields
        private SfGrid<T> _parent { get; set; }

        private int _virtualTableWidth { get; set; }

        private Dictionary<int, int> _coffSets { get; set; } = new Dictionary<int, int>();

        private bool FrozenMidScroll { get; set; }

        //Mainly used for column virtualization cases updated when the axis is X
        private int TranslateY { get; set; }
        #endregion

        #region Internal Properties

        internal IDictionary<int, List<Row<object>>> GeneratedRows { get; set; } = new Dictionary<int, List<Row<object>>>();

        internal IDictionary<int, IEnumerable<object>> GeneratedData { get; set; } = new Dictionary<int, IEnumerable<object>>();
        internal IDictionary<int, GroupedDataItem> GroupGeneratedData { get; set; } = new Dictionary<int, GroupedDataItem>();

        internal IDictionary<int, IEnumerable<object>> CurrentViewDataLookup = new Dictionary<int, IEnumerable<object>>();

        internal IEnumerable<object>? QueriedCurrentViewData { get; set; }

        internal List<GridColumn> VirtualizedColumns { get; set; } = new List<GridColumn>();

        internal List<Row<object>> GeneratedGroupedRows { get; set; } = new List<Row<object>>();

        internal Dictionary<string, GroupedDataItem>? CurrentGroupedDataCaptionRowMap { get; set; }

        internal List<Row<object>> VisibleGroupRows { get; set; } = new List<Row<object>>();

        internal List<Row<object>>? FrozenCachedRowObject { get; set; }

        internal IEnumerable<object>? FrozenCachedData { get; set; }

        internal int RowStartIndex { get; set; }

        internal int RowEndIndex { get; set; }

        internal int RowQueryStartIndex{ get; set; }

        internal int RowQueryEndIndex { get; set; }

        internal int StartColumnIndex { get; set; }

        internal int EndColumnIndex { get; set; }

        internal int TranslateX { get; set; }

        internal int VirtualRowIndex { get; set; }

        internal int RHeight { get; set; }

        internal bool NeedClientAction { get; set; }

        internal bool IsColumnIdxChanged { get; set; }

        internal bool IsSelAllChangedByRowClick { get; set; }

        internal bool IsSelectAllWithFilter { get; set; }

        internal string? RequestType { get; set; }

        internal int NextRowToNavigate { get; set; }

        internal int SelectedCellNavigation { get; set; } = -1;

        internal bool FocusFromPager { get; set; }

        internal int SelectedRowNavigation { get; set; }
        
        internal bool IsHeaderNavigated { get; set; }

        internal int CurrentRowIndex { get; set; }

        internal int PreNavigatedIndex { get; set; }

        internal int CalculatedOverScan { get; set; }

        internal bool IsDataSourceChanged { get; set; }

        internal bool RefreshByMethod { get; set; }

        internal bool IsObservable { get; set; }

        internal ValueTuple<int?, int?> ShiftSelectionRowIndexes { get; set; } = (-1, -1);

        internal ValueTuple<int?, int?> ShiftSelectionCellIndexes { get; set; } = (-1, -1);

        internal double[] SelectRowsMethodIndexes { get; set; } = Array.Empty<double>();

        //For MaskRow
        internal int ScrollTop { get; set; }

        internal (int startIndex, int endIndex) virtualIndex { get; set; }

        internal List<ValidationResult> VirtualValidation { get; set; } = new List<ValidationResult>();

        internal List<GroupedDataItem>? CurrentGroupedData { get; set; }

        /// <summary>
        /// Indicates that an add-row action was performed or was initiated and then cancelled
        /// while virtualization is enabled and the new-row position is Bottom.
        /// </summary>
        internal bool HasAddOrCancelAction { get; set; }
        #endregion

        #region Constructor
        public VirtualScroll(SfGrid<T> parent) => _parent = parent;
        #endregion

        #region Cache Management

        /// <summary>
        /// Checks and resets the virtual scroll cache based on the request type.
        /// Responsibility: Manages cache invalidation for various grid operations.
        /// </summary>
        /// <param name="requestType">The type of request that triggered the cache check.</param>
        internal async Task CheckAndResetCache(string requestType)
        {
            if (!_parent.EnableVirtualization || string.IsNullOrEmpty(requestType))
            {
                return;
            }
            HashSet<string> clearCacheActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "sorting", "reorder", "searching", "filtering", "refresh", "save", "delete", "grouping", 
                "UnGrouping", "FreezeLineReorder", "RowDragAndDrop", "ClearFiltering"
            };
            if (clearCacheActions.Contains(requestType))
            {
                ClearCacheData();
                ResetSelectionModule(requestType);
                ResetIndexesForGrouping(requestType);
            }
            await HandleColumnStateChange(requestType).ConfigureAwait(true);
            await HandleDataSourceChange().ConfigureAwait(true);
        }

        /// <summary>
        /// Clears all cached data for virtual scrolling.
        /// </summary>
        private void ClearCacheData()
        {
            GeneratedData = new Dictionary<int, IEnumerable<object>>();
            GeneratedRows = new Dictionary<int, List<Row<object>>>();
            GeneratedGroupedRows = new List<Row<object>>();
            FrozenCachedData = null!;
            FrozenCachedRowObject = new List<Row<object>>();
            ShiftSelectionRowIndexes = (-1, -1);
            ShiftSelectionCellIndexes = (-1, -1);
        }

        /// <summary>
        /// Resets the selection module state based on request type.
        /// </summary>
        private void ResetSelectionModule(string requestType)
        {
            if(_parent.SelectionModule != null)
            {
                _parent.SelectionModule.ResetSelectionModule(requestType);
            }

            if (_parent.SelectionSettings != null && !_parent.SelectionSettings.PersistSelection && _parent.VirtualScrollModule != null)
            {
                IsSelAllChangedByRowClick = false;
            }
        }

        /// <summary>
        /// Resets row indexes when grouping or ungrouping.
        /// </summary>
        private void ResetIndexesForGrouping(string requestType)
        {
            if ((requestType.Equals("grouping", StringComparison.OrdinalIgnoreCase) ||
                 requestType.Equals("ungrouping", StringComparison.OrdinalIgnoreCase)) &&
                _parent.GroupSettings!.Columns?.Length > 0 && !IsObservable)
            {
                RowStartIndex = RowEndIndex = 0;
            }
        }

        /// <summary>
        /// Handles column state changes and refreshes column indexes if needed.
        /// </summary>
        private async Task HandleColumnStateChange(string requestType)
        {
            if (requestType.Equals("ColumnState", StringComparison.Ordinal) &&
                GeneratedGroupedRows.Count > 0 &&
                _parent.GroupSettings!.Columns?.Length > 0)
            {
                GeneratedGroupedRows = new List<Row<object>>();
            }

            if (_parent.IsRendered &&
                _parent.EnableColumnVirtualization &&
                requestType.Equals("ColumnState", StringComparison.Ordinal) &&
                _parent.ForceUpdate)
            {
                await _parent.InvokeMethod("sfBlazor.Grid.refreshColumnIndex",
                    new object[] { _parent.DataId, _parent.Columns! }).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handles data source changes and refreshes the grid.
        /// </summary>
        private async Task HandleDataSourceChange()
        {
            if (_parent.IsRendered && IsDataSourceChanged)
            {
                IsDataSourceChanged = false;
                await _parent.InvokeMethod("sfBlazor.Grid.refreshOnDataChange",
                    new object[] { _parent.DataId }).ConfigureAwait(true);
            }
        }

        internal void AddGeneratedRows(List<Row<object>> RecentlyGeneratedRows, int rowQueryStartIndex, int rowQueryEndIndex)
        {
            bool isRenderedFromTreeGrid = _parent.IsRenderedFromTreeGrid && _parent.FocusModule?.IsKeyPressedUpOrDown == true;
            int startIndex = _parent.IsRowReordered && _parent.OverscanCount == 0 ? RowStartIndex : isRenderedFromTreeGrid ? RowQueryStartIndex : rowQueryStartIndex;
            int endIndex = _parent.IsRowReordered && _parent.OverscanCount == 0 ? RowEndIndex : isRenderedFromTreeGrid ? RowQueryEndIndex : rowQueryEndIndex;
            int iteration = 0;
            if (startIndex != 0 && endIndex != 0 && _parent.SelectionModule != null)
            {
                _parent.SelectionModule.UpdateGeneratedRowsSelection(RecentlyGeneratedRows, IsSelAllChangedByRowClick, IsSelectAllWithFilter);
            }
            for (int i = startIndex; i < endIndex; i++)
            {
                List<Row<object>>? alreadyAdded;
                if (!GeneratedRows.TryGetValue(i, out alreadyAdded) && RecentlyGeneratedRows.Skip(iteration).Take(1).ToList().Count > 0)
                {
                    GeneratedRows.Add(i, RecentlyGeneratedRows.Skip(iteration).Take(1).ToList());
                }
                else
                {
                    if (GeneratedRows.ContainsKey(i))
                    {
                        List<Row<object>> tempList = new List<Row<object>>();
                        foreach (var row in RecentlyGeneratedRows)
                        {
                            if (row.Index == i)
                                tempList.Add(row);
                        }
                        if (tempList.Count > 0)
                            GeneratedRows[i] = tempList;
                    }
                }
                iteration = iteration + 1;
            }
            if (_parent.DataSource == null && _parent.EnableVirtualization && !_parent.CheckBoxState.Equals(CheckState.Check) 
                && _parent.SelectionModule != null)
            {
                _parent.SelectionModule.SetHeaderCheckState();
                _parent.EventAggregator.Trigger("HeaderStateChanged", null!);
            }
        }

        internal List<Row<object>> SetRows(int rowStartIndex, int rowEndIndex)
        {
            List<Row<object>> currentRows = new List<Row<object>>();
            var virtualIndexes = virtualIndex = VirtualIndexes(rowStartIndex, rowEndIndex);
            for (int i = virtualIndexes.startIndex; i < virtualIndexes.endIndex; i++)
            {
                if (!((_parent.TotalItemCount - 1) >= i))
                {
                    return currentRows;
                }
                List<Row<object>> addRow;
                if (GeneratedRows.TryGetValue(i, out addRow!) && addRow.Count > 0)
                {
                    _parent.EditModule!.HandleVirtualScrollEditState(addRow, currentRows, rowStartIndex, rowEndIndex, IsDataSourceChanged, i);
                    if (!addRow[0].Visible)
                    {
                        virtualIndexes.endIndex++;
                    }
                }
            }
            return currentRows;
        }
        #endregion

        #region Column Virtualization

        /// <summary>
        /// Gets the virtualized columns based on frozen columns configuration.
        /// Responsibility: Determines which columns should be rendered in the viewport.
        /// </summary>
        /// <returns>List of columns to be virtualized.</returns>
        internal List<GridColumn>? GetVirtualizedColumns()
        {
            if (!_parent.EnableColumnVirtualization)
            {
                return new List<GridColumn>();
            }
            if (_parent.FrozenColumns > 0 && _parent.FreezeModule != null)
            {
                return _parent.FreezeModule.GetFrozenColumns().Concat(GetMovableColumnsList(StartColumnIndex, EndColumnIndex)).ToList();
            }
            if (_parent.FreezeModule!.GetFrozenCount() > 0 && _parent.Columns != null)
            {
                IEnumerable<GridColumn> leftFreezedColumns = _parent.Columns.Where(col => col.IsFrozen && col.Freeze.Equals(FreezeDirection.Left));
                IEnumerable<GridColumn> rightFreezedColumns = _parent.Columns.Where(col => col.IsFrozen && col.Freeze.Equals(FreezeDirection.Right));
                IEnumerable<GridColumn> movableColumns = GetMovableColumnsList(StartColumnIndex, (EndColumnIndex - StartColumnIndex + 1));
                return (leftFreezedColumns)?.Concat(movableColumns)?.Concat(rightFreezedColumns!).ToList();
            }
            return VirtualizedColumns;
        }

        /// <summary>
        /// Gets the movable columns within the specified index range.
        /// </summary>
        /// <returns>List of movable columns.</returns>

        private IEnumerable<GridColumn> GetMovableColumnsList(int startColumnIndex, int endColumnIndex)
        {
            IEnumerable<GridColumn> movableColumns = _parent.FreezeModule!.GetMovableColumns().Skip(startColumnIndex).Take(endColumnIndex);
            return movableColumns;
        }

        /// <summary>
        /// Gets the list of virtualized columns
        /// </summary>
        /// <returns>List virtual columns.</returns>
        internal List<GridColumn> GetVirtualColumns()
        {
            return !_parent.EnableColumnVirtualization ? GridUtils.GetColumns(_parent) : VirtualizedColumns;
        }

        /// <summary>
        /// Refreshes column offsets and calculates column widths.
        /// </summary>
        /// <returns>List of column widths.</returns>
        private List<int> RefreshColOffsets()
        {
            List<GridColumn>? columns = _parent.Columns;
            if (columns == null || columns.Count == 0)
            {
                _coffSets = new Dictionary<int, int>();
                return new List<int>();
            }

            int totalColumnsLength = columns.Count;
            List<int> columnWidths = new List<int>(totalColumnsLength);
            _coffSets = new Dictionary<int, int>();
            for (int i = 0; i < totalColumnsLength; i++)
            {
                int temp = i == 0 ? 0 : Convert.ToInt32(_coffSets[i - 1]);
                int autoWidth;
                if (string.IsNullOrEmpty(columns[i].Width))
                {
                    autoWidth = 200;
                    columns[i].SetWidth(autoWidth.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    autoWidth = columns[i].Visible ? RemovePx(columns[i].Width) : 0;
                }

                columnWidths.Add(autoWidth);

                _coffSets.Add(i, autoWidth + temp);
            }
            return columnWidths;
        }

        /// <summary>
        /// Calculates column indexes for virtualization.
        /// </summary>
        /// <param name="columnWidth">List of column widths.</param>
        /// <returns>List of column indexes.</returns>
        private List<int> GetColumnIndexes(List<int> columnWidth)
        {
            List<int> columnIndexes = new List<int>();
            int gridWidth = RemovePx(_parent.Width);
            int calculatedWidth = gridWidth * 2;
            for (int i = 0; i < _coffSets.Count; i++)
            {
                if (_coffSets[i] <= calculatedWidth)
                {
                    columnIndexes.Add(i);
                }
            }
            if (columnIndexes.Count == 1 || columnIndexes.Count == 0)
            {
                var minWidth = columnWidth.Min();
                if (gridWidth > minWidth)
                {
                    StartColumnIndex = 0;
                    EndColumnIndex = (gridWidth / minWidth + 1);
                }
                else
                {
                    StartColumnIndex = 0;
                    EndColumnIndex = (columnWidth.First() / gridWidth) + 1;
                }
            }
            else
            {
                StartColumnIndex = columnIndexes[0];
                EndColumnIndex = columnIndexes[columnIndexes.Count - 1];
            }
            return columnIndexes;
        }

        #endregion

        #region Data Generation and Management

        internal async Task<bool> VirtualDataProcess(List<int> QueryStartIndexes, ActionArgs action, DataReadyArgs<T> eventArgs, string requestType = null!, Query query = null!)
        {
            _parent.VirtualScrollModule!.RequestType = action?.RequestType!;
            _parent.EventAggregator.Trigger("DataReady", eventArgs = new DataReadyArgs<T>() { Data = _parent.CurrentViewData, Grid = _parent, Query = query!, Count = _parent.TotalItemCount });
            _parent.CurrentViewData = _parent.IsEmptyGrid ? null! : eventArgs.Data;
            if (_parent.EnableVirtualization)
            {
                _parent.SelectionModule?.SetHeaderCheckState(requestType: requestType);
            }
            ((DataResult)_parent.Data!).Result = _parent.IsEmptyGrid ? null : eventArgs.Data;
            _parent.TotalItemCount = _parent.IsEmptyGrid ? 0 : eventArgs.Count;
            _parent.ReactiveAggregateModule?.UpdateAggregateFromEventArgs(eventArgs);
            // Set virtual scroll indexes before OnDataBound event to ensure GridVirtualContent receives correct parameters
            if (_parent.EnableVirtualization && !_parent.Reset)
            {
                SetIdxForVscroll(action!, QueryStartIndexes);
            }
            if ((_parent.GridEvents != null && _parent.GridEvents.OnDataBound.HasDelegate) || _parent.IsRenderedFromTreeGrid)
            {
                List<T> _data = null!;
                if (_parent.CurrentViewData is Group<T> group && group.Records != null)
                {
                    _data = group.Records.OfType<T>().ToList<T>();
                }
                else
                {
                    _data = _parent.CurrentViewData?.OfType<T>().ToList<T>()!;
                }

                var bArgs = new BeforeDataBoundArgs<T>()
                {
                    Cancel = false,
                    Count = _parent.TotalItemCount,
                    Result = _data,
                    Parent = _parent
                };
                if (_parent.IsRenderedFromTreeGrid)
                    await _parent.EventAggregator.NotifyAsync("BeforDataBound", bArgs).ConfigureAwait(true);
                else
                    await (_parent.GridEvents?.OnDataBound.InvokeAsync(bArgs))!.ConfigureAwait(true)!;
                if (bArgs.Cancel)
                    return true;
            }

            _parent._requireDataBoundInvoke = true;

            return false;
        }

        /// <summary>
        /// Sets generated data for the specified query range.
        /// Responsibility: Populates data cache with queried data and group information.
        /// </summary>
        /// <param name="queryStartIndex">Start index of the query range.</param>
        /// <param name="queryEndIndex">End index of the query range.</param>
        /// <param name="recentlyGeneratedData">The data generated from the query.</param>
        internal void SetGeneratedData(int queryStartIndex, int queryEndIndex, IEnumerable<object> recentlyGeneratedData)
        {
            GroupGeneratedData = new Dictionary<int, GroupedDataItem>();

            if (queryStartIndex >= queryEndIndex || recentlyGeneratedData == null)
            {
                return;
            }

            PopulateGroupGeneratedData(queryStartIndex, queryEndIndex);
            PopulateGeneratedData(queryStartIndex, recentlyGeneratedData);
        }

        /// <summary>
        /// Populates grouped data for the specified range.
        /// </summary>
        private void PopulateGroupGeneratedData(int queryStartIndex, int queryEndIndex)
        {
            if (CurrentGroupedData == null || CurrentGroupedData.Count == 0)
            {
                return;
            }

            var visibleRows = CurrentGroupedData.Where(item => item.Visible);
            if (!_parent.GroupSettings!.EnableLazyLoading && _parent.AllowGrouping && _parent.GroupSettings.Columns?.Length > 0 && _parent.OverscanCount > 0)
            {
                (int startIndex, int endIndex) = virtualIndex = VirtualIndexes(queryStartIndex, queryEndIndex);
                queryStartIndex = startIndex;
                queryEndIndex = endIndex;
            }
            var rangeRows = visibleRows.Skip(queryStartIndex).Take(queryEndIndex - queryStartIndex);
            int groupDataCount = 0;

            foreach (var row in rangeRows)
            {
                GroupGeneratedData[groupDataCount] = new GroupedDataItem
                {
                    Index = row.Index,
                    RowIndex = row.RowIndex,
                    Indent = row.Indent,
                    Visible = row.Visible,
                    Uid = row.Uid,
                    ParentUid = row.ParentUid,
                    IsExpand = row.IsExpand,
                    Item = row.Item,
                    IsCaptionRow = row.IsCaptionRow,
                    IsSelected = row.IsSelected,
                    IsFooterRow = row.IsFooterRow
                };
                groupDataCount++;
            }
        }

        /// <summary>
        /// Populates the generated data dictionary with query results.
        /// </summary>
        private void PopulateGeneratedData(int queryStartIndex, IEnumerable<object> recentlyGeneratedData)
        {
            if (recentlyGeneratedData == null)
            {
                return;
            }
            int loopIteration = queryStartIndex;
            foreach (var data in recentlyGeneratedData)
            {
                // Skip if this index already has cached data (preserve existing entries)
                if (!GeneratedData.ContainsKey(loopIteration))
                {
                    // Wrap single item in list for consistency with IDictionary<int, IEnumerable<object>>
                    GeneratedData[loopIteration] = new List<object>(1) { data };
                    loopIteration++;
                }
            }
        }

        /// <summary>
        /// Sets the current view data based on virtual index range.
        /// </summary>
        /// <param name="virtualStartIndex">Start index of the virtual range.</param>
        /// <param name="virtualEndIndex">End index of the virtual range.</param>
        internal void SetCurrentViewData(int virtualStartIndex, int virtualEndIndex)
        {
            // Before: Multiple resizes (0→4→8→16→32→64)
            //List<object> CurrentData = new List<object>();
            // After: Exact capacity, zero resizes
            int expectedCapacity = virtualEndIndex - virtualStartIndex;
            List<object> CurrentData = new List<object>(expectedCapacity);
            for (var index = virtualStartIndex; index < virtualEndIndex; index++)
            {
                if (GeneratedData.TryGetValue(index, out IEnumerable<object>? cachedData) && cachedData != null)
                {
                    // Use LINQ's FirstOrDefault for efficient single-item extraction
                    object? firstItem = cachedData.FirstOrDefault();
                    if (firstItem != null)
                    {
                        CurrentData.Add(firstItem);
                    }
                }
            }

            _parent.CurrentViewData = (IEnumerable<object>)CurrentData;
        }

        #endregion

        #region Query and Index Calculation

        /// <summary>
        /// Generates query and sets query indexes for virtual scrolling.
        /// Responsibility: Orchestrates data fetching based on virtual scroll position.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="foreignKeyHandle">Whether foreign key handling is enabled.</param>
        /// <param name="isForeignKeyAction">Whether this is a foreign key action.</param>
        /// <param name="VirtualStartIndex">Virtual start index.</param>
        /// <param name="VirtualEndIndex">Virtual end index.</param>
        /// <param name="saveAction">Whether this is a save action.</param>
        /// <returns>List containing query start and end indexes.</returns>
        internal async Task<List<int>> GenerateQueryAndSetQueryIndex(Query query, bool foreignKeyHandle, bool isForeignKeyAction, int VirtualStartIndex, int VirtualEndIndex, bool saveAction = false)
        {
            bool isStartIndexSet = false;
            int VStartIndex = VirtualStartIndex;
            GridPageSettings? gridPageSettings = _parent.PageSettings;
            int gridPageSize = gridPageSettings!.PageSize;
            int StartIndexToSort = (gridPageSettings.CurrentPage - 1) * gridPageSize;

            // Below calculation done to do actions like sorting
            int VEndIndex = VirtualEndIndex != 0 ? VirtualEndIndex : (StartIndexToSort + gridPageSize);
            var (startIndex, endIndex) = CalculateQueryIndexes(VStartIndex, VEndIndex, saveAction);

            var queryResult = QueryIndexes(startIndex, endIndex, isStartIndexSet);
            int QStartIndex = queryResult.QStartIndex;
            int QEndIndex = queryResult.QEndIndex;
            isStartIndexSet = queryResult.isStartIndexSet;

            if (isStartIndexSet)
            {
                DataReadyArgs<T> eventArgs;
                _parent.EventAggregator.Trigger("SetVirtualScrollIndex", eventArgs = new DataReadyArgs<T>()
                {
                    StartIndex = QStartIndex,
                    EndIndex = QEndIndex,
                    VStartIndex = VStartIndex,
                    VEndIndex = VEndIndex
                });
                await _parent.GenerateAndExecuteQuery(query, isForeignKeyAction, (int)eventArgs.StartIndex, (int)eventArgs.EndIndex).ConfigureAwait(true);
                QueriedCurrentViewData = (IEnumerable<object>)(((DataResult)_parent.Data!)?.Result)!;
                QStartIndex = (int)eventArgs.StartIndex;
                QEndIndex = (int)eventArgs.EndIndex;
                _parent.EventAggregator.Trigger("DataReady", eventArgs = new DataReadyArgs<T>()
                {
                    Data = QueriedCurrentViewData,
                    Grid = _parent,
                    Query = query,
                    StartIndex = (int)QStartIndex,
                    EndIndex = (int)QEndIndex,
                    VStartIndex = VStartIndex,
                    VEndIndex = VEndIndex
                });
                QueriedCurrentViewData = eventArgs.Data;
                QStartIndex = (int)eventArgs.StartIndex;
                QEndIndex = (int)eventArgs.EndIndex;
                VStartIndex = (int)eventArgs.VStartIndex;
                VEndIndex = (int)eventArgs.VEndIndex;
                SetGeneratedData(QStartIndex, QEndIndex, QueriedCurrentViewData);
            }

            List<int> QueryStartIndexes = new List<int>() { QStartIndex, QEndIndex };
            var currentIndex = _parent.OverscanCount > 0 ? CurrentIndexes(VStartIndex, VEndIndex) : (startIndex: VStartIndex, endIndex: VEndIndex);
            SetCurrentViewData(currentIndex.startIndex, currentIndex.endIndex);
            return QueryStartIndexes;
        }

        /// <summary>
        /// Calculates query indexes based on local or remote data source.
        /// </summary>
        private (int startIndex, int endIndex) CalculateQueryIndexes(int vStartIndex, int vEndIndex, bool saveAction)
        {
            int startIndex = vStartIndex;
            int endIndex = vEndIndex;

            if (IsLocal())
            {
                (startIndex, endIndex) = CalculateLocalDataIndexes(vStartIndex, vEndIndex, saveAction);
            }
            else if (_parent.OverscanCount > 0)
            {
                var virtualIndex = CurrentIndexes(vStartIndex, vEndIndex);
                startIndex = virtualIndex.startIndex;
                endIndex = virtualIndex.endIndex;
            }

            return (startIndex, endIndex);
        }

        /// <summary>
        /// Calculates indexes for local data source with overscan.
        /// </summary>
        private (int startIndex, int endIndex) CalculateLocalDataIndexes(int vStartIndex, int vEndIndex, bool saveAction)
        {
            int gridPageSize = _parent.PageSettings!.PageSize;
            int gridOverScanCount = _parent.OverscanCount;
            bool isRowReordered = _parent.IsRowReordered;
            int size = gridPageSize > gridOverScanCount ? gridPageSize : gridOverScanCount;

            int startIndex = vStartIndex > size && !isRowReordered ? vStartIndex - size : vStartIndex;
            int endIndex = !isRowReordered ? vEndIndex + size : vEndIndex;

            if (_parent.TotalItemCount > 0 && gridOverScanCount > 0)
            {
                (startIndex, endIndex) = AdjustIndexesForOverscan(vStartIndex, vEndIndex, startIndex, endIndex, saveAction);
            }

            return (startIndex, endIndex);
        }

        /// <summary>
        /// Adjusts indexes to account for overscan buffer.
        /// </summary>
        private (int startIndex, int endIndex) AdjustIndexesForOverscan(int vStartIndex, int vEndIndex, int startIndex, int endIndex, bool saveAction)
        {
            int gridOverScanCount = _parent.OverscanCount;
            int gridTotalItemCount = _parent.TotalItemCount;
            int gridPageSize = _parent.PageSettings!.PageSize;
            // Adjust for end of data
            if (vEndIndex == gridTotalItemCount && !_parent.EnableVirtualMaskRow)
            {
                int overscanPageSize = (gridOverScanCount * 2) + gridPageSize;
                endIndex = endIndex > gridTotalItemCount ? gridTotalItemCount : endIndex;
                startIndex = (endIndex - startIndex) > 0 && (endIndex - startIndex) < overscanPageSize
                    ? endIndex - overscanPageSize
                    : startIndex;
                startIndex = Math.Max(startIndex, 0);
            }

            // Adjust for start of data
            if (startIndex < gridOverScanCount)
            {
                endIndex = ((endIndex + gridOverScanCount) > gridTotalItemCount) && startIndex > 0
                    ? gridTotalItemCount
                    : endIndex + gridOverScanCount;
            }

            // Adjust for near end of data
            if (startIndex >= gridTotalItemCount - (gridPageSize + gridOverScanCount))
            {
                startIndex = startIndex - gridOverScanCount >= 0 ? startIndex - gridOverScanCount : 0;
            }

            // Handle save action
            if (saveAction)
            {
                (startIndex, endIndex) = HandleSaveActionIndexes(startIndex, endIndex);
            }
            if (_parent.IsRowReordered)
            {
                int indexDifference = endIndex - startIndex;
                int overScanBufferCount = _parent.PageSettings.PageSize + (_parent.OverscanCount * 2);
                if (indexDifference >= 0 && (indexDifference < overScanBufferCount))
                {
                    var virtualIndex = CurrentIndexes(vStartIndex, vEndIndex);
                    startIndex = virtualIndex.startIndex;
                    endIndex = virtualIndex.endIndex;
                }
            }

            return (startIndex, endIndex);
        }

        /// <summary>
        /// Handles index calculation for save actions.
        /// </summary>
        private (int startIndex, int endIndex) HandleSaveActionIndexes(int startIndex, int endIndex)
        {
            if (IsBottomAddForm((RowEndIndex - 1)))
            {
                int totalItemCount = _parent.TotalItemCount + 1;
                endIndex = totalItemCount;
                int gridOverScanPageSize = _parent.PageSettings!.PageSize + (_parent.OverscanCount * 2);
                gridOverScanPageSize = totalItemCount < gridOverScanPageSize ? totalItemCount : gridOverScanPageSize;
                startIndex = endIndex - gridOverScanPageSize;
            }
            else
            {
                endIndex = virtualIndex.endIndex;
                startIndex = virtualIndex.startIndex;
            }

            return (startIndex, endIndex);
        }

        /// <summary>
        /// Queries indexes to determine which data needs to be fetched.
        /// </summary>
        /// <param name="VirtualStartIndex">Virtual start index.</param>
        /// <param name="VirtualEndIndex">Virtual end index.</param>
        /// <param name="isStartIndexSet">Whether start index is already set.</param>
        /// <returns>Tuple containing query start, end indexes and status flags.</returns>
        internal (int QStartIndex, int QEndIndex, bool isStartIndexSet, bool virtualRefresh) QueryIndexes(int VirtualStartIndex, int VirtualEndIndex, bool isStartIndexSet = false)
        {
            var QStartIndex = 0;
            var QEndIndex = 0;
            for (var i = VirtualStartIndex; i < VirtualEndIndex; i++)
            {
                if (!GeneratedData.TryGetValue(i, out IEnumerable<object>? alreadyAdded))
                {
                    if (!isStartIndexSet)
                    {
                        isStartIndexSet = true;
                        QStartIndex = i;
                    }
                    else
                    {
                        QEndIndex = (i == VirtualEndIndex) ? i : i + 1;
                    }
                }
            }
            return (QStartIndex, QEndIndex, isStartIndexSet, (QStartIndex == 0 && QEndIndex == 0));
        }

        /// <summary>
        /// Calculates current indexes with overscan applied.
        /// </summary>
        /// <param name="startIndex">Start index.</param>
        /// <param name="endIndex">End index.</param>
        /// <returns>Adjusted start and end indexes.</returns>
        internal (int startIndex, int endIndex) CurrentIndexes(int startIndex, int endIndex)
        {
            int gridOverScanCount = _parent.OverscanCount;
            int gridTotalItemCount = _parent.TotalItemCount;
            startIndex = startIndex > gridOverScanCount ? startIndex - gridOverScanCount : startIndex;
            endIndex = startIndex > gridOverScanCount ? endIndex + gridOverScanCount : endIndex + (gridOverScanCount * 2);
            if (gridTotalItemCount > 0 && endIndex > gridTotalItemCount && startIndex > 0)
            {
                endIndex = gridTotalItemCount;
                startIndex = Math.Max(endIndex - (_parent.PageSettings!.PageSize + (gridOverScanCount * 2)), 0);
            }
            return (startIndex, endIndex);
        }

        /// <summary>
        /// Calculates virtual indexes for rendering with overscan support.
        /// </summary>
        /// <param name="startIndex">Start index.</param>
        /// <param name="endIndex">End index.</param>
        /// <returns>Calculated virtual indexes.</returns>
        internal (int startIndex, int endIndex) VirtualIndexes(int startIndex, int endIndex)
        {
            int gridOverScanCount = _parent.OverscanCount;
            int gridPageSize = _parent.PageSettings!.PageSize;
            int gridTotalItemCount = _parent.TotalItemCount;
            int _endIndex = endIndex != 0 ? endIndex : gridPageSize;
            var totalItemCount = _parent.AllowGrouping && !_parent.GroupSettings!.EnableLazyLoading 
                && _parent.GroupSettings.Columns?.Length > 0 ? VisibleGroupRows.Count : gridTotalItemCount;
            if (gridOverScanCount > 0)
            {
                if ((endIndex != 0 && endIndex < virtualIndex.endIndex) && startIndex > virtualIndex.startIndex)
                {
                    return virtualIndex;
                }
                int differenceOfOverscancount = totalItemCount - endIndex;
                int actualEndIndex = _endIndex;
                if (endIndex <= totalItemCount)
                {
                    actualEndIndex += differenceOfOverscancount;
                }
                else if (endIndex > totalItemCount)
                {
                    actualEndIndex = totalItemCount;
                }

                _endIndex = _endIndex + gridOverScanCount > totalItemCount ? actualEndIndex : _endIndex + gridOverScanCount;
                if (startIndex == 0 || startIndex < gridOverScanCount)
                {
                    _endIndex += startIndex == 0 ? startIndex + gridOverScanCount : startIndex + (startIndex - gridOverScanCount) + gridOverScanCount;
                }
                int _startIndex = startIndex >= gridOverScanCount ? startIndex - gridOverScanCount : startIndex - gridOverScanCount <= 0 ? 0 : startIndex;
                if (_endIndex != gridPageSize + (gridOverScanCount * 2))
                {
                    if (_startIndex == 0)
                    {
                        _endIndex = gridPageSize + (gridOverScanCount * 2);
                    }
                    else if (_parent.AllowGrouping && _parent.GroupSettings!.Columns?.Length > 0 && !_parent.GroupSettings.EnableLazyLoading)
                    {
                        _endIndex = endIndex + gridOverScanCount;
                    }
                }
                if (_parent.VisibleGroupedDataCount > 0 && _endIndex > _parent.VisibleGroupedDataCount && _startIndex > 0)
                {
                    _endIndex = _parent.VisibleGroupedDataCount;
                    _startIndex = _endIndex - (gridPageSize + (gridOverScanCount * 2));
                }
                if (_parent.IsRenderedFromTreeGrid && _startIndex > gridOverScanCount && endIndex < gridTotalItemCount)
                {
                    int dataCount = _parent.DataSource!.Count();
                    int currentViewCount = _parent.CurrentViewData!.Count();
                    _endIndex = _endIndex >= dataCount && !_parent.IsSingleRootData ? dataCount : _endIndex - _startIndex > currentViewCount && !_parent.IsSingleRootData ? _startIndex + currentViewCount : _endIndex;
                    _startIndex = (currentViewCount < gridPageSize && _parent.IsSingleRootData) ? 
                        0 : currentViewCount == gridTotalItemCount && _endIndex > gridTotalItemCount
                        && ScrollTop > 0 && !_parent.IsSingleRootData ? 
                        (gridTotalItemCount - (gridPageSize + (2 * gridOverScanCount))) : _startIndex;
                    return (_startIndex, _endIndex);
                }
                if (_startIndex >= gridTotalItemCount - (gridPageSize + (2 * gridOverScanCount)))
                {
                    _startIndex = (_parent.CurrentViewData?.Count() < gridPageSize && _parent.IsRenderedFromTreeGrid) ? 0 : _endIndex - (gridPageSize + (gridOverScanCount * 2));
                }
                return (_startIndex, _endIndex);
            }
            else
            {
                return (startIndex, _endIndex);
            }
        }

        /// <summary>
        /// Calculates the translation Y value for virtual mask row.
        /// </summary>
        /// <param name="scrollTop">Current scroll top position.</param>
        /// <param name="startIndex">Start index.</param>
        /// <param name="endIndex">End index.</param>
        /// <returns>Calculated translation Y value.</returns>
        internal int GetVirtualMaskTranslateY(int scrollTop, int startIndex, int endIndex)
        {
            CalculatedOverScan = 0;
            if (_parent.OverscanCount > 0)
            {
                int? rowStartIndex = 0;

                if (_parent.Rows?.Count > 0)
                {
                    Row<object>? firstRow = _parent.Rows.FirstOrDefault();
                    rowStartIndex = _parent.AllowGrouping && !_parent.GroupSettings!.EnableLazyLoading 
                        && _parent.GroupSettings.Columns?.Length > 0 ? firstRow?.GroupIndex : firstRow?.Index;
                }
                CalculatedOverScan = (int)(startIndex - (rowStartIndex ?? 0) > 0 ? startIndex - (rowStartIndex ?? 0) : 0);
            }
            
            return (scrollTop - ((_parent.PageSettings!.PageSize + CalculatedOverScan) * RHeight));
        }

        /// <summary>
        /// Sets indexes for virtual scrolling based on action arguments.
        /// </summary>
        /// <param name="action">Action arguments containing virtual index information.</param>
        /// <param name="QueryStartIndexes">List of query start indexes to update.</param>
        internal void SetIdxForVscroll(ActionArgs action, List<int> QueryStartIndexes)
        {
            int gridPageSize = _parent.PageSettings!.PageSize;
            int gridOverScanCount = _parent.OverscanCount;
            int gridTotalItemCount = _parent.TotalItemCount;
            if (QueryStartIndexes?.Count > 0)
            {
                DataReadyArgs<T> args;
                _parent.EventAggregator.Trigger("SetVirtualScrollRowIndex", args = new DataReadyArgs<T>()
                {
                    StartIndex = QueryStartIndexes[0],
                    EndIndex = QueryStartIndexes[1],
                    VStartIndex = (int)action.VirtualStartIndex,
                    VEndIndex = (int)action.VirtualEndIndex
                });

                if (_parent.IsRenderedFromTreeGrid && _parent.IsAdd)
                {
                    action.VirtualEndIndex = (int)args.VEndIndex;
                }
                
                QueryStartIndexes[0] = (int)args.StartIndex;
                QueryStartIndexes[1] = (int)args.EndIndex;
            }

            RowStartIndex = (int)action.VirtualStartIndex;
            RowEndIndex = (int)action.VirtualEndIndex;

            int pageSize = gridOverScanCount > 0 ? gridPageSize * 2 : gridPageSize;
            var endIndex = IsLocal() && gridPageSize > gridOverScanCount ? action.VirtualEndIndex + pageSize 
                : action.VirtualEndIndex + (gridOverScanCount * 2);

            RowQueryStartIndex = QueryStartIndexes?.Count > 0 ? QueryStartIndexes[0] : (int)action.VirtualStartIndex;

            RowQueryEndIndex = QueryStartIndexes?.Count > 0 && QueryStartIndexes[1] > 0 
                ? QueryStartIndexes[1] : endIndex > gridTotalItemCount && RowStartIndex != 0 
                && gridOverScanCount > 0 ? gridTotalItemCount : endIndex;
            if (_parent.EnableColumnVirtualization && action.EndColumnIndex != 0)
            {
                StartColumnIndex = (int)action.StartColumnIndex;
                EndColumnIndex = (int)action.EndColumnIndex;
            }
        }
        #endregion

        #region Group Row Management

        /// <summary>
        /// Sets current view group rows based on virtual indexes.
        /// Responsibility: Manages visible grouped rows and edit mode handling.
        /// </summary>
        /// <param name="virtualStartIndex">Virtual start index.</param>
        /// <param name="virtualEndIndex">Virtual end index.</param>
        internal void SetCurrentViewGroupRows(int virtualStartIndex = 0, int virtualEndIndex = 0)
        {
            virtualEndIndex = virtualEndIndex != 0 ? virtualEndIndex : _parent.PageSettings!.PageSize;
            if (_parent.GroupSettings != null && !_parent.GroupSettings.EnableLazyLoading)
            {
                int visibleIndex = 0;
                VisibleGroupRows.Clear();
                foreach (var row in GeneratedGroupedRows)
                {
                    if (row.Visible)
                    {
                        row.GroupIndex = visibleIndex++;
                        VisibleGroupRows.Add(row);
                    }
                }
            }
            else
            {
                VisibleGroupRows = GeneratedGroupedRows.Where(Row => Row.Visible).ToList();
            }
            
            if (VisibleGroupRows.Count > 0)
            {
                if (virtualEndIndex > VisibleGroupRows.Count)
                {
                    RowEndIndex = virtualEndIndex = VisibleGroupRows.Count;
                    var sIndex = virtualEndIndex - _parent.PageSettings!.PageSize;
                    RowStartIndex = virtualStartIndex = sIndex > 0 ? sIndex : 0;
                }
                else
                {
                    RowStartIndex = virtualStartIndex;
                    RowEndIndex = (virtualEndIndex < _parent.PageSettings!.PageSize ) ? _parent.PageSettings.PageSize : virtualEndIndex;
                }
            }
            if (!_parent.GroupSettings!.EnableLazyLoading && _parent.AllowGrouping && _parent.GroupSettings.Columns?.Length > 0)
            {
                (int startIndex, int endIndex) = virtualIndex = _parent.OverscanCount > 0 ? VirtualIndexes(RowStartIndex, RowEndIndex) : (RowStartIndex, RowEndIndex);
                virtualStartIndex = startIndex;
                virtualEndIndex = endIndex;
            }
            _parent.Rows = VisibleGroupRows.Skip(virtualStartIndex).Take(virtualEndIndex - virtualStartIndex).ToList();
            if (_parent.EditSettings!.Mode.Equals(EditMode.Normal))
            {
                var editedRow = _parent.Rows?.Where(_ => _.IsEdit)?.ToList();
                var result = _parent.Rows?.Where(_ => _.Index == _parent.EditModule!.EditedRow?.Index)?.ToList();
                bool isEditAdd = _parent.IsEdit && _parent.EditModule!.IsAdd;
                if (!_parent.IsEdit && editedRow?.Count > 0)
                {
                    editedRow[0].IsEdit = false;
                }
                else if (_parent.IsEdit && result?.Count > 0)
                {
                    result[0].IsEdit = true;
                }
                if (isEditAdd && !IsDataSourceChanged) 
                {
                    _parent.EditModule!.EditedRow!.IsEdit = true;
                    if (_parent.EditSettings.NewRowPosition == NewRowPosition.Top
                    && RowStartIndex == 0 )
                    {
                        _parent.EditModule.EditedRow.IsAddedTop = true;
                        _parent.Rows?.Insert(0, _parent.EditModule.EditedRow);
                    }
                    else if (_parent.EditSettings.NewRowPosition == NewRowPosition.Bottom
                        && (VisibleGroupRows.Count == RowEndIndex || (_parent.GroupSettings.EnableLazyLoading && RowEndIndex >= 0)))
                    {
                        _parent.EditModule.EditedRow.IsAddedBottom = true;
                        _parent.Rows?.Add(_parent.EditModule.EditedRow);
                    }
                }  
            }
        }

        internal void SetCurrentGroupedData()
        {
            var currentViewData = (IEnumerable<object>?)((DataResult)_parent.Data!).Result ?? Enumerable.Empty<object>();
            var flattenedData = new List<GroupedDataItem>();
            int rowIndex = 0;
            int index = 0;
            bool isExpand = _parent.GroupSettings?.ExpandAllGroups ?? false;
            if(currentViewData != null)
            {
                foreach (var item in currentViewData)
                {
                    if (item is Group<T> group)
                    {
                        var newUid = _parent.GetUid("grid-row");
                        flattenedData.Add(new GroupedDataItem
                        {
                            Index = null,
                            RowIndex = rowIndex++,
                            Indent = group.Level - 1,
                            Visible = true,
                            Uid = newUid,
                            ParentUid = null!,
                            IsExpand = isExpand,
                            Item = group,
                            IsCaptionRow = true
                        });
                        AddGroupChildren(flattenedData, group, ref rowIndex, ref index, newUid);
                    }
                }
            }
            CurrentGroupedData = flattenedData;
            CurrentGroupedDataCaptionRowMap = CurrentGroupedData.Where(x => x.Uid != null && x.IsCaptionRow).ToDictionary(x => x.Uid!, x => x);
        }

        private void AddGroupChildren(List<GroupedDataItem> list, Group<T> group, ref int rowIndex, ref int index, string parentUid)
        {
            int indent = group.Level;
            bool isExpanded = _parent.GroupSettings!.ExpandAllGroups;

            foreach (var child in group.Items ?? Enumerable.Empty<T>())
            {
                var childUid = _parent.GetUid("grid-row");

                if (child is Group<T> childGroup)
                {
                    list.Add(new GroupedDataItem
                    {
                        Index = null,
                        RowIndex = rowIndex++,
                        Indent = childGroup.Level - 1,
                        Visible = isExpanded,
                        Uid = childUid,
                        ParentUid = parentUid,
                        IsExpand = isExpanded,
                        Item = childGroup,
                        IsCaptionRow = true
                    });

                    AddGroupChildren(list, childGroup, ref rowIndex, ref index, childUid);
                }
                else
                {
                    list.Add(new GroupedDataItem
                    {
                        Index = index++,
                        RowIndex = rowIndex++,
                        Indent = indent,
                        Visible = isExpanded,
                        Uid = childUid,
                        ParentUid = parentUid,
                        IsExpand = isExpanded,
                        Item = child,
                        IsCaptionRow = false
                    });
                }
            }
            if (group?.Aggregates is IDictionary<string, object> agg)
            {
                if (agg.Count > 0)
                {
                    list.Add(new GroupedDataItem
                    {
                        Index = index++,
                        RowIndex = rowIndex++,
                        Indent = indent,
                        Visible = isExpanded,
                        Uid = _parent.GetUid("grid-row"),
                        ParentUid = parentUid,
                        IsExpand = isExpanded,
                        Item = group.Aggregates,
                        IsCaptionRow = false,
                        IsFooterRow = true
                    });
                }
            }
        }

        internal void VirtualExpandCollapse(Dictionary<int, int> expandedRowIndexes = null!)
        {
            expandedRowIndexes = new Dictionary<int, int>();
            int count = 0;
            var totalItemsCount = 0;
            //Calculation for totalExpanded items
            foreach (Group<T> row in _parent.CurrentViewData!)
            {
                if (!expandedRowIndexes.ContainsKey(count))
                {
                    var totalExpandedCount = 0;
                    IEnumerable<object> dataRows = (IEnumerable<object>)(row.Items ?? Enumerable.Empty<object>());
                    if(dataRows != null)
                    totalExpandedCount += dataRows.Count();
                    totalItemsCount += totalExpandedCount + 1;
                    if (dataRows != null && dataRows is Group<T>)
                    {
                        foreach (Group<T> val in dataRows)
                        {
                            GetExpandedCount(val, ref totalExpandedCount, ref totalItemsCount);
                        }
                    }
                    expandedRowIndexes.Add(count, totalItemsCount);
                }
                ++count;
            }
        }

        internal void SetReorderCurrentData(int startIndex, IEnumerable<object> data)
        {
            CurrentViewDataLookup.Clear();
            int currentIndex = startIndex;
            foreach (var item in data)
            {
                if (!CurrentViewDataLookup.ContainsKey(currentIndex))
                {
                    CurrentViewDataLookup.Add(currentIndex, new List<object> { item });
                    currentIndex++;
                }
            }
        }

        /// <summary>
        /// Calculates and sets GroupIndex on generated grouped rows for proper virtual mask row translation.
        /// </summary>
        internal void SetGroupIndexOnRows(List<Row<object>> rows, int startIndex = 0)
        {
            if (rows == null || rows.Count == 0)
            {
                return;
            }

            // Set GroupIndex on generated rows based on actual visible position (startIndex)
            if (_parent.AllowGrouping && !_parent.GroupSettings!.EnableLazyLoading && _parent.GroupSettings.Columns?.Length > 0)
            {
                for (int idx = 0; idx < rows.Count; idx++)
                {
                    rows[idx].GroupIndex = startIndex + idx;
                }
            }
        }

        private static void GetExpandedCount(Group<T> value, ref int totalItems, ref int totalItemsCount)
        {
            IEnumerable<object> dataValues = (IEnumerable<object>)(value.Items ?? Enumerable.Empty<object>());
            if (value.Items is Group<T> && dataValues != null)
            {
                foreach (Group<T> childValue in dataValues)
                {
                    GetExpandedCount(childValue, ref totalItems, ref totalItemsCount);
                }
            }
            else
            {
                if (dataValues != null)
                {
                    totalItems += dataValues.Count();
                    totalItemsCount += dataValues.Count();
                }
                 
            }
        }

        internal async void RefreshVirtualContent(int index, List<Row<object>> Rows)
        {
            int visibleCount = _parent.EnableVirtualization ? Rows.Where(vRow => vRow.Visible).Count() : 0;
            int exactTopIndex = await GetExactTopIndex(index, Rows.Count, visibleCount).ConfigureAwait(true);
            _parent.EventAggregator.Trigger("VirtualComponentUpdate", null!);
        }

        internal async Task<int> GetExactTopIndex(int index, int virtualRowsCount, int visibleCount)
        {
            int exactTopIndex = _parent.VirtualScrollModule?.RowStartIndex ?? 0;
            if (index >= virtualRowsCount - (_parent.PageSettings!.PageSize / 4))
            {
                int ClientHeight = 0;
                if (_parent.Height.Contains('%', StringComparison.Ordinal))
                {
                    ClientHeight = await _parent.InvokeMethod<int>("sfBlazor.Grid.clientHeight", false, new object[] { _parent.DataId }).ConfigureAwait(true);
                }
                exactTopIndex = _parent.Height.Contains('%', StringComparison.Ordinal) ? visibleCount - (ClientHeight / RHeight) : visibleCount - (VirtualScroll<T>.RemovePx(_parent.Height) / RHeight);
            }
            return exactTopIndex;
        }

        /// <summary>
        /// Processes grouped data during virtual scrolling operations with lazy loading support.
        /// </summary>
        /// <param name="action">Action arguments containing virtual scroll indexes.</param>
        /// <param name="scrollTop">Current vertical scroll position in pixels.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ProcessGroupedVirtualScrollAsync(ActionArgs action, int scrollTop)
        {
            action.VirtualEndIndex = (int)(action.VirtualEndIndex > 0 ? action.VirtualEndIndex : _parent.PageSettings!.PageSize);
            QueriedCurrentViewData = _parent.CurrentViewData;
            if (_parent.GroupSettings != null && _parent.GroupSettings.EnableLazyLoading)
            {
                // Update view by updating the _parent.Rows object here
                var originalStartIndex = (int)action.VirtualStartIndex;
                var originalEndIndex = (int)action.VirtualEndIndex;
                var dataStartIndex = originalStartIndex;
                var dataEndIndex = originalEndIndex;
                List<object> uiData = new List<object>();
                List<Row<object>> lazyRows = new List<Row<object>>();
                Dictionary<int, int> tempDicitonary = new Dictionary<int, int>();
                RowStartIndex = originalStartIndex;
                RowEndIndex = originalEndIndex;
                uiData = _parent.GroupModule?.GetUiData(_parent.CurrentViewData!)!;
                List<object> currentUiData = uiData.Skip(originalStartIndex).Take(_parent.PageSettings!.PageSize).ToList();

                lazyRows = _parent.GroupModule?.GenerateLazyRowsobject(currentUiData, RowStartIndex)!;
                _parent.Rows = lazyRows;

                if (_parent.EditSettings != null && _parent.EditSettings.Mode == EditMode.Normal)
                {
                    var EditedRow = _parent.Rows.Where(_ => _.IsEdit).ToList();
                    var Result = _parent.Rows.Where(_ => _.Index == _parent.EditModule!.EditedRow?.Index).ToList();
                    if (!_parent.IsEdit && EditedRow.Count != 0)
                    {
                        EditedRow[0].IsEdit = false;
                    }
                    else if (_parent.IsEdit && Result.Count != 0)
                    {
                        Result[0].IsEdit = true;
                    }
                    if (_parent.IsEdit && _parent.EditModule!.IsAdd && _parent.EditModule.EditedRow != null && _parent.EditSettings.NewRowPosition == NewRowPosition.Top && RowStartIndex == 0 && !IsDataSourceChanged)
                    {
                        _parent.EditModule.EditedRow.IsEdit = true;
                        _parent.EditModule.EditedRow.IsAddedTop = true;
                        _parent.Rows.Insert(0, _parent.EditModule.EditedRow);
                    }
                    else if (_parent.IsEdit && _parent.EditModule!.IsAdd && _parent.EditSettings.NewRowPosition == NewRowPosition.Bottom && (_parent.TotalItemCount == RowEndIndex || _parent.GroupSettings.EnableLazyLoading && RowEndIndex >= 0) && !IsDataSourceChanged)
                    {
                        _parent.EditModule.EditedRow!.IsEdit = true;
                        _parent.EditModule.EditedRow.IsAddedBottom = true;
                        _parent.Rows.Add(_parent.EditModule.EditedRow);
                    }
                }
                if (!string.IsNullOrEmpty(action.RequestType))
                {
                    RequestType = action.RequestType;
                }
            }
            if (_parent.AllowGrouping && !_parent.GroupSettings!.EnableLazyLoading)
            {
                SetCurrentViewGroupRows((int)action.VirtualStartIndex, (int)action.VirtualEndIndex);
                ScrollTop = _parent.OverscanCount > 0 ? scrollTop : ScrollTop;
            }
            if (_parent.GroupModule != null)
                _parent.GroupModule.GroupVirtualRefresh = true;
            _parent.EventAggregator.Trigger("VirtualHeaderComponentUpdate", null!);
            _parent.EventAggregator.Trigger("VirtualComponentUpdate", new
            {
                StartIndex = (int)action.VirtualStartIndex,
                EndIndex = (int)action.VirtualEndIndex,
                NextRowToNavigate = NextRowToNavigate
            });
            await _parent.InvokeSuccessAsync(action).ConfigureAwait(true);
        }

        /// <summary>
        /// Processes grouped data during virtual scrolling operations with lazy loading support.
        /// </summary>
        /// <param name="action">Action arguments containing virtual scroll indexes.</param>
        /// <param name="scrollTop"></param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task GroupedScrollDataRefresh(ActionArgs action, int scrollTop = 0)
        {
            ScrollTop = _parent.OverscanCount > 0 ? scrollTop : _parent.VirtualScrollModule!.ScrollTop;
            SetGeneratedData((int)action.VirtualStartIndex, (int)action.VirtualEndIndex, _parent.CurrentViewData!);
            _parent.ForceUpdate = true;
            _parent.EventAggregator.Trigger("VirtualComponentUpdate", new
            {
                StartIndex = (int)action.VirtualStartIndex,
                EndIndex = (int)action.VirtualEndIndex,
                NextRowToNavigate = NextRowToNavigate
            });
        }

        internal void ProcessGroupedData(GridVirtualContentParameters virtualContentParameters)
        {
            if (_parent.IsRowReordered && virtualContentParameters != null && virtualContentParameters.Data != _parent.CurrentViewData)
            {
               GeneratedGroupedRows = new GroupModelGenerator<T>(_parent).GenerateRows(_parent.CurrentViewData!);
            }
            else
            {
               GeneratedGroupedRows = _parent.Rows.Count > 0 && _parent.IsColumnHideOrShow && _parent.GroupSettings != null && _parent.GroupSettings.EnableLazyLoading ? 
                    _parent.Rows : new GroupModelGenerator<T>(_parent).GenerateRows(virtualContentParameters?.Data!);
            }
            if (_parent.IsColumnHideOrShow && _parent.GroupSettings != null && _parent.GroupSettings.EnableLazyLoading)
            {
                UpdateColumnVisibility();
            }
            var visibleGroupRows = GeneratedGroupedRows?.Where(row => row.Visible).ToList();
            if(visibleGroupRows == null)
            {
                return;
            }
            var visibleGroupRowsCount = visibleGroupRows.Count;
            var gridPageSize = _parent.PageSettings!.PageSize;
            if (visibleGroupRowsCount > 0)
            {
                if (virtualContentParameters != null && virtualContentParameters.RowEndIndex >= visibleGroupRowsCount)
                {
                    virtualContentParameters.RowEndIndex = visibleGroupRowsCount;
                    var sIndex = virtualContentParameters.RowEndIndex - gridPageSize;
                    virtualContentParameters.RowStartIndex = sIndex > 0 ? sIndex : 0;
                }

                else
                {
                   virtualContentParameters!.RowEndIndex = virtualContentParameters.RowStartIndex + gridPageSize;
                   RowEndIndex = RowStartIndex + gridPageSize;
                }
            }
        }

        internal void AdjustVirtualIndexesForReordering(GridVirtualContentParameters virtualContentParameters)
        {
            int visibleGroupRowsCount = Grouping<T>.GetVisibleGroupeddataCountInternal(CurrentGroupedData!);
            int gridPageSize = _parent.PageSettings!.PageSize;
            int virtualStartIndex = virtualContentParameters!.RowStartIndex;
            int virtualEndIndex = virtualContentParameters.RowStartIndex + gridPageSize;
            virtualEndIndex = virtualEndIndex != 0 ? virtualEndIndex : gridPageSize;
            if (visibleGroupRowsCount > 0)
            {
                if (virtualEndIndex > visibleGroupRowsCount)
                {
                    RowEndIndex = virtualEndIndex = visibleGroupRowsCount;
                    var sIndex = virtualEndIndex - gridPageSize;
                    RowStartIndex = virtualStartIndex = sIndex > 0 ? sIndex : 0;
                }
                else
                {
                    RowStartIndex = virtualStartIndex;
                    RowEndIndex = (virtualEndIndex < gridPageSize) ? gridPageSize : virtualEndIndex;
                }
            }
            if (!_parent.GroupSettings!.EnableLazyLoading && _parent.AllowGrouping && _parent.GroupSettings.Columns?.Length > 0)
            {
                (int startIndex, int endIndex) = virtualIndex = _parent.OverscanCount > 0 ? VirtualIndexes(RowStartIndex, RowEndIndex) : (RowStartIndex, RowEndIndex);
                virtualStartIndex = startIndex;
                virtualEndIndex = endIndex;
            }
        }

        #endregion

        #region Page and Load Management

        /// <summary>
        /// Handles virtual load event for initialization.
        /// Responsibility: Ensures proper page size and column virtualization setup.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        internal void VirtualLoadListener(object args)
        {
            var arg = args;
            if (_parent.EnableVirtualization)
            {
                EnsurePageSize();
            }
            if (_parent.EnableColumnVirtualization)
            {
                if (_parent.Width?.Contains('%', StringComparison.Ordinal) == true || _parent.Width == "auto")
                {
                    NeedClientAction = true;
                    List<int> columnWidths = RefreshColOffsets();
                }
                else
                {
                    List<int> columnWidths = RefreshColOffsets();
                    GetColumnIndexes(columnWidths);
                }
            }
        }


        internal void HideAtMediaListener(object args)
        {
            var arg = args;
            if (!_parent.EnableColumnVirtualization && GridUtils.GetColumns(_parent).Where(Col => !string.IsNullOrEmpty(Col.HideAtMedia)).ToList().Count > 0 && _parent.VirtualScrollModule != null)
            {
                // NeedClientAction should only be used for Virtual Scroll and HideAtMedia features
                NeedClientAction = true;
            }
        }

        /// <summary>
        /// Ensures the page size is properly calculated based on grid height and row height.
        /// </summary>
        private void EnsurePageSize()
        {
            const int defaultPageSize = 12;
            if (_parent.PageSettings != null && _parent.PageSettings.PageSize == defaultPageSize && (_parent.RowHeight == 0 || (_parent.RowHeight != 0 && (_parent.Height?.Contains('%', StringComparison.Ordinal) == true || _parent.Height == "auto"))))
            {
                NeedClientAction = true;
            }
            else if (_parent.PageSettings != null && _parent.PageSettings.PageSize == defaultPageSize)
            {
                int rowHeight = (int)_parent.RowHeight;
                int gridHeight = RemovePx(_parent.Height!);
                int height = Convert.ToInt32(gridHeight / rowHeight) * 2;
#pragma warning disable BL0005
                _parent.PageSettings.PageSize = _parent.PageSettings.PageSize < height ? height : _parent.PageSettings.PageSize;
#pragma warning restore BL0005
            }
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Removes "px" suffix from width or height string and converts to integer.
        /// </summary>
        /// <param name="WidthOrHeight">Width or height string with or without "px".</param>
        /// <returns>Integer value of the dimension.</returns>
        internal static int RemovePx(string WidthOrHeight)
        {
            if (WidthOrHeight.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                return 0; 
            }
            else if (WidthOrHeight.Contains("px", StringComparison.Ordinal))
            {
                return (int)Math.Ceiling(Convert.ToDouble(WidthOrHeight.Substring(0, WidthOrHeight.IndexOf("px", StringComparison.Ordinal)), CultureInfo.CurrentCulture));
            }
            else
            {
                return (int)Math.Ceiling(Convert.ToDouble(WidthOrHeight, CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Calculates total width of visible columns.
        /// </summary>
        /// <param name="cols">List of grid columns.</param>
        /// <returns>Total width of visible columns.</returns>
        internal static int GetColumnsWidth(List<GridColumn> cols)
        {
            int Width = 0;
            foreach (GridColumn col in cols)
            {
                if (col?.Visible == true && !string.IsNullOrEmpty(col?.Width))
                {
                    Width = Width + RemovePx(col.Width);
                }
            }

            return Width;
        }

        /// <summary>
        /// Determines if the data source is local or remote.
        /// </summary>
        /// <returns>True if data source is local, false otherwise.</returns>
        internal bool IsLocal()
        {
            if (_parent.DataManager != null && _parent.DataManager.DataAdaptor != null)
            {
                return !(_parent.DataManager.DataAdaptor.IsRemote() || _parent.DataManager.Adaptor == Adaptors.CustomAdaptor);
            }
            return false;
        }

        /// <summary>
        /// Sets the virtual table width.
        /// </summary>
        /// <param name="Width">Width value to set.</param>
        internal void SetVTableWidth(int Width)
        {
            _virtualTableWidth = Width;
        }

        /// <summary>
        /// Gets the virtual table width.
        /// </summary>
        /// <returns>Virtual table width.</returns>
        internal int GetVTableWidth()
        {
            return _virtualTableWidth;
        }

        /// <summary>
        /// Checks whether the add-new row form should appear at the bottom of the virtual grid.
        /// </summary>
        /// <param name="rowEndIndex">The index of the last visible row. Defaults to 0.</param>
        /// <returns>
        /// <c>true</c> if the grid is in add mode, the new row position is bottom, the edit mode is normal,
        /// and the end index equals to the total item count; otherwise, <c>false</c>.
        /// </returns>

        internal bool IsBottomAddForm(int rowEndIndex = 0)
        {
            int gridTotalItemCount = _parent.TotalItemCount;
            int virtualRowEndIndex = rowEndIndex > gridTotalItemCount ? gridTotalItemCount : rowEndIndex;
            return _parent.EditModule!.IsAdd && _parent.EditSettings!.NewRowPosition == NewRowPosition.Bottom 
                && _parent.EditSettings.Mode == EditMode.Normal && virtualRowEndIndex == gridTotalItemCount;
        }


        /// <summary>
        /// Updates the visibility of grid columns based on the grouping settings of the parent component.
        /// </summary>
        
        internal void UpdateColumnVisibility()
        {
            List<GridColumn> gridColumns = GridUtils.GetColumns(_parent);
            for (int j = 0; j < gridColumns?.Count; j++)
            {
                if (_parent.GroupSettings != null && _parent.GroupSettings.Columns?.Contains(gridColumns[j].Field) == true)
                {
#pragma warning disable BL0005
                    gridColumns[j].Visible = _parent.GroupSettings.ShowGroupedColumn ? (gridColumns[j].IsHiddenByGrouping ? true : gridColumns[j].Visible) : _parent.GroupSettings.ShowGroupedColumn;
                    gridColumns[j].SetVisibility(gridColumns[j].Visible);
#pragma warning restore BL0005
                }
            }
        }
        #endregion

        #region Keyboard Interaction and Focus Management

        /// <summary>
        /// Handles keyboard navigation to the next row when fast scrolling with arrow keys.
        /// </summary>
        internal async Task<int> HandleNextRowNavigationAsync(int nextRowToNavigate, 
            KeyboardEventArgs? lastKeyCombination, int lastNavigatedCellIdx)
        {
            if (nextRowToNavigate <= 0)
            {
                return 0;
            }

            bool IsRowNavigatedFastly = false;
            Row<object> row;
            var parentRows = _parent.Rows;
            var targetRow = parentRows.FirstOrDefault(_ => _.Index == NextRowToNavigate);

            if (targetRow != null)
            {
                row = targetRow;
            }
            else
            {
                IsRowNavigatedFastly = true;
                row = lastKeyCombination?.Code == "ArrowDown" ? parentRows[parentRows.Count - 1]  // Last row in viewport
                   : parentRows[1]; // Second row (skip first)
            }
            Cell<object> cell = row.Cells[lastNavigatedCellIdx];
            PreNavigatedIndex = NextRowToNavigate = 0;
            await InvokeProcessKeyDown(lastKeyCombination!, row, cell).ConfigureAwait(true);
            if (IsRowNavigatedFastly)
            {
                PreNavigatedIndex = 0;
            }
            return NextRowToNavigate;
        }

        /// <summary>
        /// Handles focus management when navigating from pager in column virtualization mode.
        /// </summary>
        internal async Task HandlePagerFocusAsync()
        {
            Row<object> lastRow = _parent.Rows.LastOrDefault()!;
            if (lastRow == null)
            {
                return;
            }

            int lastCellIndex = _parent.Columns!.Count - 1;
            Cell<object> lastCell = _parent.Rows![(int)lastRow.Index!].Cells[lastCellIndex];

            await (_parent.FocusModule?.Focus(
                lastRow.Uid!,
                lastCell.Uid!,
                cellColIndex: lastCell.Index + 1 ?? -1
            ))!.ConfigureAwait(true);

            FocusFromPager = false;
        }

        /// <summary>
        /// Handles horizontal cell navigation using arrow keys (left/right) and Tab key.
        /// </summary>
        /// <param name="lastKeyCombination">The keyboard event arguments containing key information.</param>
        /// <param name="lastNavigatedCellIdx">The index of the last navigated cell.</param>
        /// <param name="selectedCellNavigationIndex">The index used for cell navigation calculation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task HandleHorizontalCellNavigationAsync(
            KeyboardEventArgs? lastKeyCombination,
            int lastNavigatedCellIdx,
            int selectedCellNavigationIndex)
        {
            // Guard clauses for invalid input
            if (lastKeyCombination == null)
            {
                return;
            }

            var focusModule = _parent.FocusModule;
            if (focusModule == null)
            {
                return;
            }

            var columns = _parent.Columns;
            var rows = _parent.Rows;

            // Validate collections
            if (columns == null || rows == null || rows.Count == 0)
            {
                return;
            }

            // Cache frequently accessed values
            int columnCount = columns.Count;
            bool isColumnVirtualizationEnabled = _parent.EnableColumnVirtualization;

            bool hasNoFrozenColumns = isColumnVirtualizationEnabled && _parent.FreezeModule!.GetFrozenCount() == 0;
            bool hasOnlyFrozenRight = _parent.FreezeModule!.GetFreezeRightCount() > 0 && _parent.FreezeModule!.GetFreezeLeftCount() == 0;
            bool hasFrozenLeftWithScroll = StartColumnIndex != 0 && (_parent.FreezeModule!.GetFreezeLeftCount() > 0 || _parent.FrozenColumns > 0);
            
            bool isVirtualNavigation = hasNoFrozenColumns
                || (hasOnlyFrozenRight)
                || (hasFrozenLeftWithScroll) || FrozenMidScroll;

            string lastKeyCode = lastKeyCombination.Code;

            // Handle right arrow or forward tab
            if (lastKeyCode == "ArrowRight" || (lastKeyCode == "Tab" && !lastKeyCombination.ShiftKey))
            {
                await ProcessRightNavigationAsync(lastKeyCombination, lastNavigatedCellIdx,
                    selectedCellNavigationIndex, isVirtualNavigation, columnCount).ConfigureAwait(true);
            }

            // Handle left arrow or backward tab
            else if (lastKeyCode == "ArrowLeft" || lastKeyCode == "Tab" && lastKeyCombination.ShiftKey)
            {
                await ProcessLeftNavigationAsync(lastKeyCombination, lastNavigatedCellIdx,
                    selectedCellNavigationIndex, isVirtualNavigation, columnCount).ConfigureAwait(true);
            }
            FrozenMidScroll = false;
            SelectedCellNavigation = -1;
        }

        internal async Task HandleColumnVirtualKeyBoard()
        {
            int selectedCellNavigation = SelectedCellNavigation;
            KeyboardEventArgs? lastKeyCombination = _parent.FocusModule!.LastKeyCombination;
            int lastNavigatedCellIdx = _parent.FocusModule.LastNavigatedCellIdx;
            if (IsHeaderNavigated && selectedCellNavigation != -1 && lastKeyCombination != null)
            {
                string lastKeyCode = lastKeyCombination.Code;
                bool tabKey = (lastKeyCode == "Tab" && !lastKeyCombination.ShiftKey);
                bool ShiftTabKey = (lastKeyCode == "Tab" && lastKeyCombination.ShiftKey);
                int columnsCount = (int)_parent.Columns?.Count! - 1;
                if (lastKeyCode == "ArrowRight" || tabKey)
                {
                    if (lastNavigatedCellIdx != columnsCount)
                    {
                        bool cellNavigationEqualToColumnCount = selectedCellNavigation == columnsCount;
                        if (!cellNavigationEqualToColumnCount || (cellNavigationEqualToColumnCount && lastNavigatedCellIdx == columnsCount - 1 && _parent.FreezeModule!.GetFrozenCount() == 0))
                        {
                            SelectedCellNavigation = selectedCellNavigation - 1;
                        }
                    }
                }
                else if (lastKeyCode == "ArrowLeft" || ShiftTabKey)
                {
                    if (lastNavigatedCellIdx != 0 && !FrozenMidScroll)
                    {
                        SelectedCellNavigation = selectedCellNavigation + 1;
                    }

                }
                int rowIndex = 0;
                //int headerCellsCount = _parent.HeaderRows[rowIndex].Cells.Count;
                List<GridColumn> columns = _parent.RearrangeColumns(_parent.Columns);
                GridColumn currentColumn = SelectedCellNavigation != -1 ? columns[SelectedCellNavigation] : columns[0];
                Cell<object>? currentCell = _parent.HeaderRows[0].Cells.Where(x => x.Column != null && x.Column.Field == currentColumn.Field).FirstOrDefault();
                bool cellIndexEqualToColumnCount = SelectedCellNavigation == columnsCount;
                if (cellIndexEqualToColumnCount && _parent.FreezeModule!.GetFreezeRightCount() == 0 && currentCell == null
                    && _parent.HeaderRows[0].Cells?.LastOrDefault()?.IsFocused == false)
                {
                    _parent.HeaderRows[0].Cells.LastOrDefault()!.IsFocused = true;
                }
                Row<object> currentRow = _parent.HeaderRows[rowIndex];
                if ((_parent.Rows?.Count > 0 && _parent.FocusModule.SelectedRowIndex == 0 || _parent.FocusModule.SelectedRowIndex == null) && lastNavigatedCellIdx == 0 && cellIndexEqualToColumnCount)
                {
                    currentRow = _parent.Rows![0];
                    currentCell = currentRow?.Cells?.FirstOrDefault(_ => _.Visible);
                }
                await InvokeProcessKeyDown(lastKeyCombination, currentRow!, currentCell!, isHeader: true)!.ConfigureAwait(true);
                SelectedCellNavigation = -1;
            }
        }

        /// <summary>
        /// Processes right or forward tab navigation.
        /// </summary>
        private async Task ProcessRightNavigationAsync(KeyboardEventArgs? lastKeyCombination, int lastNavigatedCellIdx,
            int selectedCellNavigationIndex, bool isVirtualNavigation, int columnsCount)
        {
            bool isColumnVirtualizationEnabled = _parent.EnableColumnVirtualization;
            List<Row<object>>? parentRows = _parent.Rows;
            SelectedCellNavigation = lastNavigatedCellIdx != (columnsCount - 1) ? selectedCellNavigationIndex - 1 : selectedCellNavigationIndex;
            
            if ((!isColumnVirtualizationEnabled || isVirtualNavigation) && SelectedCellNavigation != -1 && SelectedRowNavigation != -1)
            {
                if (SelectedCellNavigation < columnsCount && parentRows?.Count > 0)
                {
                    int rowIndex = (int)parentRows?.FindIndex(e => e.Index == SelectedRowNavigation)!;
                    Cell<object> cell = parentRows[rowIndex].Cells[SelectedCellNavigation];
                    await InvokeProcessKeyDown(lastKeyCombination!, parentRows[rowIndex], cell)!.ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Processes left or backward tab navigation.
        /// </summary>
        private async Task ProcessLeftNavigationAsync(KeyboardEventArgs? lastKeyCombination, int lastNavigatedCellIdx,
            int selectedCellNavigationIndex, bool isVirtualNavigation, int columnsCount)
        {
            bool isColumnVirtualizationEnabled = _parent.EnableColumnVirtualization;
            List<Row<object>>? parentRows = _parent.Rows;
            if (SelectedRowNavigation != -1)
            {
                SelectedCellNavigation = lastNavigatedCellIdx != 0 && !FrozenMidScroll ? selectedCellNavigationIndex + 1 : selectedCellNavigationIndex;
                if (!isColumnVirtualizationEnabled || isVirtualNavigation)
                {
                    if (lastNavigatedCellIdx == 0 && SelectedRowNavigation != _parent.FocusModule?.SelectedRowIndex)
                    {
                        SelectedRowNavigation = (int)_parent.FocusModule!.SelectedRowIndex!;
                        SelectedCellNavigation = 0;
                    }
                    if (SelectedCellNavigation < columnsCount && parentRows?.Count > 0)
                    {
                        int rowIndex = (int)parentRows?.FindIndex(e => e.Index == SelectedRowNavigation)!;
                        Cell<object> cell = parentRows[rowIndex].Cells[SelectedCellNavigation];
                        await InvokeProcessKeyDown(lastKeyCombination!, parentRows[rowIndex], cell)!.ConfigureAwait(true);
                    }
                }
            }
        }

        private async Task InvokeProcessKeyDown(KeyboardEventArgs keyboardArgs, Row<object> row, Cell<object> cell, bool isHeader = false)
        {
            _parent.FocusModule?.ProcessKeyDown(keyboardArgs, row, cell, isHeader);
        }
        #endregion

        #region All Post-render Operations
        /// <summary>
        /// Handles all post-render operations including virtual scrolling updates, navigation, validation, and client synchronization.
        /// </summary>
        internal async Task HandlePostRenderOperationsAsync(int rowEndIndex) 
        {
            bool isClientInitialized = _parent.IsClientInitialized;

            if (_parent.IsRendered && isClientInitialized && _parent.EnableVirtualMaskRow && _parent.EnableColumnVirtualization)
            {
                await ClientTransformUpdate(TranslateX, TranslateY, _parent.OverscanCount > 0).ConfigureAwait(true);
            }

            if ((_parent.EnableVirtualization || (_parent.EnableInfiniteScrolling 
                && _parent.GroupSettings != null &&  _parent.GroupSettings.EnableLazyLoading)) 
                && _parent.GroupModule != null && _parent.GroupModule.IsGroupExpandAndCollapse 
                && _parent.Columns != null && _parent.Columns.Where(_ => _.AutoFit).Any())
            {
                await _parent.InvokeMethod("sfBlazor.Grid.autoFit", new object[] { _parent.DataId }).ConfigureAwait(true);
                _parent.GroupModule.IsGroupExpandAndCollapse = false;
            }

            if (VirtualValidation.Count > 0 && _parent.EditModule != null)
            {
                var errorResult = _parent.EditModule.ErrorResult;
                _parent.EditModule!.ErrorResult = new List<ValidationResult>();
                _parent.EventAggregator?.Trigger("ShowValidationMessage", null!);
                _parent.EditModule.ErrorResult = errorResult!;
                await _parent.EditModule.InvokeValidation(VirtualValidation)!.ConfigureAwait(true);
                VirtualValidation = new List<ValidationResult>();
            }

            if (_parent.IsRenderedFromTreeGrid && _parent.FocusModule?.IsKeyPressedUpOrDown == true
               && _parent._requireDataBoundInvoke && isClientInitialized)
            {
                _parent._requireDataBoundInvoke = false;
                await _parent.EventAggregator!.NotifyAsync("DataBoundMock", null!).ConfigureAwait(true);
            }

            if (HasAddOrCancelAction && IsBottomAddForm(rowEndIndex))
            {
                HasAddOrCancelAction = false;
                await _parent.InvokeMethod("sfBlazor.Grid.clientTransformUpdate", new object[] { _parent.DataId, null!, null!, false, true }).ConfigureAwait(true);
            }
        }
        #endregion

        #region JsInterop Handled

        /// <summary>
        /// Handles vertical (Y-axis) virtual scrolling operations.
        /// </summary>
        internal async Task HandleVerticalScrollAsync(ActionArgs action, int scrollTop,
            int selectedRowIndex,
            bool isScrollIntoView,
            int focusColumnIndex,
            bool isPreventFocusScroll)
        {
            _parent.EventAggregator.Trigger("VirtualScroll", action!);
            if (_parent.AllowGrouping && _parent.EnableVirtualization && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0 
                && _parent.VirtualScrollModule?.GeneratedGroupedRows?.Count > 0 && action != null)
            {
                await ProcessGroupedVirtualScrollAsync(action, scrollTop).ConfigureAwait(true);
            }
            else
            {
                await ProcessScrollDataAsync(action!, scrollTop, isPreventFocusScroll).ConfigureAwait(true);
            }

            if (isScrollIntoView && selectedRowIndex >= 0 && _parent.EditSettings != null && !_parent.EditSettings.ShowAddNewRow && !_parent.IsAdd)
            {
                ScrollTop = _parent.OverscanCount > 0 ? scrollTop : ScrollTop;
                if (_parent.SelectionModule != null)
                    await _parent.SelectionModule.SelectRow(selectedRowIndex, isSelectionMethodInvoked: true, isScrollIntoView: isScrollIntoView, focusColumnIndex: focusColumnIndex).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Processes virtual scroll data for non-grouped records with cache-aware handling.
        /// </summary>
        private async Task ProcessScrollDataAsync(ActionArgs action, int scrollTop, bool isPreventFocusScroll)
        {
            var isNoFocusInVirtualRows = isPreventFocusScroll && _parent.OverscanCount == 0 && _parent.EnableVirtualization && _parent.EnableVirtualMaskRow && !_parent.EnableColumnVirtualization;
            var currentIndex = VirtualIndexes(action!.VirtualStartIndex, action.VirtualEndIndex);

            HasAddOrCancelAction = IsBottomAddForm(currentIndex.endIndex) ? true : HasAddOrCancelAction;
            
            if (_parent.AllowGrouping && _parent.GroupSettings != null &&  _parent.GroupSettings.Columns != null && _parent.GroupSettings.Columns.Length > 0)
            {
                await GroupedScrollDataRefresh(action, scrollTop).ConfigureAwait(true);
            }
            else if (QueryIndexes(currentIndex.startIndex, currentIndex.endIndex).virtualRefresh)
            {
                if (virtualIndex != currentIndex)
                {
                    HasAddOrCancelAction = IsBottomAddForm(currentIndex.endIndex) ? true : HasAddOrCancelAction;

                    CurrentRowIndex = isNoFocusInVirtualRows ? action.VirtualStartIndex : CurrentRowIndex;
                    
                    ScrollTop = _parent.OverscanCount > 0 ? scrollTop : ScrollTop;
                    _parent.ForceUpdate = true;
                    SetCurrentViewData(currentIndex.startIndex, currentIndex.endIndex);
                    _parent.EventAggregator.Trigger("VirtualComponentUpdate", new
                    {
                        StartIndex = (int)action.VirtualStartIndex,
                        EndIndex = (int)action.VirtualEndIndex,
                        NextRowToNavigate = NextRowToNavigate
                    });
                    if (_parent.EnableVirtualMaskRow)
                    {
                        var translateY = GetVirtualMaskTranslateY(scrollTop, (int)action.VirtualStartIndex, (int)action.VirtualEndIndex);
                        await ClientTransformUpdate(TranslateX, translateY, true).ConfigureAwait(true);
                    }
                }
                else if (currentIndex.endIndex == _parent.TotalItemCount && _parent.EnableVirtualMaskRow)
                {
                    if (isNoFocusInVirtualRows)
                    {
                        CurrentRowIndex = action.VirtualStartIndex;
                    }
                    ScrollTop = _parent.OverscanCount > 0 ? scrollTop : ScrollTop;
                    RowStartIndex = action.VirtualStartIndex;
                    RowEndIndex = action.VirtualEndIndex;
                    var translateY = GetVirtualMaskTranslateY(scrollTop, (int)action.VirtualStartIndex, (int)action.VirtualEndIndex);
                    await _parent.InvokeMethod("sfBlazor.Grid.clientTransformUpdate", new object[] { _parent.DataId, TranslateX, translateY, true, false }).ConfigureAwait(true);
                }
            }
            else
            {
                ScrollTop = _parent.OverscanCount > 0 ? scrollTop : ScrollTop;
                HasAddOrCancelAction = IsBottomAddForm(currentIndex.endIndex) ? true : HasAddOrCancelAction;
                
                await _parent.ShowSpinnerAsync().ConfigureAwait(true);
                await _parent.DataProcess(action).ConfigureAwait(true);
                await _parent.HideSpinnerAsync().ConfigureAwait(true);

                if (_parent.IsRenderedFromTreeGrid && _parent.FocusModule?.IsKeyPressedUpOrDown == true && _parent.SelectedRowIndex > -1)
                {
                    _parent.ForceUpdate = true;
                    _parent.EventAggregator.Trigger("VirtualComponentUpdate", new
                    {
                        StartIndex = (int)action.VirtualStartIndex,
                        EndIndex = (int)action.VirtualEndIndex,
                        NextRowToNavigate = NextRowToNavigate
                    });
                    _parent.ForceUpdate = false;

                }
            }
        }
        private async Task ClientTransformUpdate(int translateX, int translateY, bool overScanCount)
        {
            await _parent.InvokeMethod("sfBlazor.Grid.clientTransformUpdate", new object[]
                { _parent.DataId, translateX, translateY, overScanCount, false }).ConfigureAwait(true);

            if (_parent.IsRenderedFromTreeGrid)
            {
                var args = new object[] { _parent.DataId, _parent!.PageSettings!.PageSize, _parent.OverscanCount };
                await _parent.InvokeMethod("sfBlazor.TreeGrid.maskRowUpdate", args).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handles vertical (X-axis) virtual scrolling operations.
        /// </summary>
        internal async Task HandleHorizontalScrollAsync(ActionArgs action, bool frozenMidScroll = false, bool focusFromPager = false)
        {
            if (action?.IsScrollByFocus == true || frozenMidScroll)
            {
                IsHeaderNavigated = (action != null && action.IsHeaderNavigated);
                SelectedRowNavigation = (int)action!.SelectedRowNavigation;
                SelectedCellNavigation = (int)action.SelectedCellNavigation;
                FrozenMidScroll = frozenMidScroll;
            }
            FocusFromPager = focusFromPager;
            SetVTableWidth((int)action?.VTableWidth!);
            TranslateX = (int)action.TranslateX;
            TranslateY = (int)action.TranslateY;
            StartColumnIndex = (int)action.StartColumnIndex;
            EndColumnIndex = (int)action.EndColumnIndex;
            _parent.EventAggregator.Trigger("VirtualHeaderComponentUpdate", null!);
            _parent.EventAggregator.Trigger("VirtualComponentUpdate", new { Axis = action.Axis });
            await _parent.InvokeMethod("sfBlazor.Grid.updateVirtualColumns", new object[] { _parent.DataId, GetVirtualizedColumns()!, null! }).ConfigureAwait(true);
        }
        #endregion

        #region Virtualization CRUD actions
        /// <summary>
        /// Scrolls the grid to the edited row position and stores validation results.
        /// </summary>
        /// <param name="validateFields">Validation results for the edited row.</param>
        /// <returns>A task representing the asynchronous scroll operation.</returns>
        internal async Task ScrollToEditedRowAsync(List<ValidationResult> validateFields)
        {
            VirtualValidation = validateFields;
            int groupedColumnsLength = _parent.GroupSettings!.Columns != null ? _parent.GroupSettings!.Columns!.Length : 0;
            bool normalEditModeAddEdit = _parent.IsEdit && _parent.EditModule!.IsAdd && _parent.EditSettings != null && _parent.EditSettings.Mode.Equals(EditMode.Normal);
            if (normalEditModeAddEdit && _parent.EditSettings != null && _parent.EditSettings.NewRowPosition == NewRowPosition.Top)
            {
                await _parent.ScrollIntoViewAsync(-1, 0, -1).ConfigureAwait(true);
            }
            else if (normalEditModeAddEdit && _parent.EditSettings!.NewRowPosition == NewRowPosition.Bottom)
            {
                if (_parent.AllowGrouping && groupedColumnsLength > 0)
                {
                    await _parent.InvokeMethod("sfBlazor.Grid.scrollIntoView", new object[] { _parent.DataId, -1, VisibleGroupRows.Count, -1, true }).ConfigureAwait(true);
                }
                else
                {
                    await _parent.ScrollIntoViewAsync(-1, _parent.TotalItemCount, -1).ConfigureAwait(true);
                }
            }
            else if (_parent.GroupSettings.Columns == null || groupedColumnsLength == 0)
            {
                await _parent.ScrollIntoViewAsync(-1, (int)_parent.EditModule?.EditedRow?.Index!, -1).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handles row insertion for add mode operations based on the configured new row position.
        /// </summary>
        internal void HandleAddModeRowInsertion(int virtualRowStartIndex, int virtualRowEndIndex)
        {
            if(_parent.IsAdd && _parent.IsEdit && _parent.EditModule!.IsAdd && _parent.EditSettings != null &&  _parent.EditSettings.Mode.Equals(EditMode.Normal))
            {
                var editedRow = _parent.EditModule.EditedRow;
                if (editedRow == null) return;
                bool isNotEditedRow = !IsDataSourceChanged && !editedRow.IsEdit;
                if (_parent.EditSettings.NewRowPosition == NewRowPosition.Top && virtualRowStartIndex == 0 && isNotEditedRow)
                {
                    editedRow.IsEdit = true;
                    editedRow.IsAddedTop = true;
                    _parent.Rows.Insert(0, editedRow);
                }
                else if (_parent.EditSettings.NewRowPosition == NewRowPosition.Bottom && isNotEditedRow)
                {
                    if (virtualRowEndIndex == _parent.TotalItemCount || VisibleGroupRows?.Count >= 0)
                    {
                        editedRow.IsEdit = true;
                        editedRow.IsAddedBottom = true;
                        _parent.Rows.Add(editedRow);
                    }
                }
            }
            
        }
        #endregion

        #region Selection Helpers

        /// <summary>
        /// Gets the range of virtual selected rows based on start and end indexes.
        /// </summary>
        /// <param name="startIndex">The starting row index for the selection range.</param>
        /// <param name="endIndex">The ending row index for the selection range.</param>
        /// <param name="startCellIndex">Optional starting cell index. Defaults to -1.</param>
        /// <param name="endCellIndex">Optional ending cell index. Defaults to -1.</param>
        /// <returns>A list of rows within the specified index range.</returns>
        internal List<Row<object>> GetRangeOfVirtualSelectedRows(int? startIndex, int? endIndex, int? startCellIndex = -1, int? endCellIndex = -1)
        {
            List<Row<object>> range = new List<Row<object>>();
            ShiftSelectionRowIndexes = (startIndex, endIndex);
            ShiftSelectionCellIndexes = (startCellIndex, endCellIndex);
            // Extract values for performance
            int startValue = startIndex ?? -1;
            int endValue = endIndex ?? -1;
            if (_parent.GroupSettings != null && _parent.GroupSettings.Columns?.ToList().Count > 0)
            {
                range = _parent.Rows?.Where(row => row.IsDataRow && row.Index >= startValue && row.Index <= endValue).ToList()!;
            }
            else
            {
                var startIdx = startIndex ?? -1;
                for (int i = startIdx; i <= endIndex; i++)
                {
                    if (GeneratedRows.TryGetValue(i, out List<Row<object>>? rowList) && rowList != null && rowList.Count > 0)
                    {
                        range.Add(rowList[0]);
                    }
                }
            }
            return range;
        }

        #endregion
    }
}
