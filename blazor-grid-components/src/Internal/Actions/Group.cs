using System;
using System.Collections.Generic;
using Syncfusion.Blazor.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles grouping action.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class Grouping<T>
    {
        #region Private Fields

        private SfGrid<T> Parent { get; set; }

        private Dictionary<int, int> _expandedRowIndexes = new Dictionary<int, int>();

        #endregion

        #region Internal Properties

        internal bool GroupVirtualRefresh { get; set; }

        internal bool IsLazyExpandAll { get; set; }

        internal bool IsLazyExpand { get; set; }

        internal bool IsGroupExpandAndCollapse { get; set; }

        internal List<Row<Object>> LazyRows { get; set; } = new List<Row<Object>>();

        internal int GridOffsetWidth { get; set; }

        internal bool DisableExtraFrozenTd { get; set; }

        internal bool ChildDataExist { get; set; }

        #endregion

        #region Public Properties

        public string IndentWidth { get; set; } = string.Empty;

        #endregion

        #region Constructor

        public Grouping(SfGrid<T> parent)
        {
            Parent = parent;
            parent.EventAggregator.Add("BeforeCellFocus", CellFocused);
        }

        #endregion

        #region Core Grouping Operations

        /// <summary>
        /// Groups the grid by a specified column.
        /// Adds the column to the group settings and triggers a model update with necessary UI changes.
        /// </summary>
        /// <param name="ColumnName">The field name of the column to group by.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GroupColumn(string ColumnName)
        {
            Parent.IsColumnHeaderChange = true;
            // TODO: Reordering in Groupdrop
            var Column = GridUtils.GetColumnByField(ColumnName, GridUtils.GetColumns(Parent));
            IndentWidth = string.Empty;
            if(Parent.RowReorderModule != null)
            Parent.RowReorderModule.RowReorderIndentWidth = string.Empty;
            var GCols = Parent.GroupSettings!.Columns?.ToList();
            if (!Parent.AllowGrouping || !Column!.AllowGrouping || (GCols != null && GCols.IndexOf(ColumnName) > -1) || Column.FixedColumn)
            {
                return;
            }

            var columnsVisibility = Column?.directParamKeys.Contains("Visible");
            if (Column != null && Column.Visible && !Parent.GroupSettings.ShowGroupedColumn && columnsVisibility == false)
            {
                Column.IsHiddenByGrouping = true;
            }
            Column?.SetVisibility(Column.Visible ? Parent.GroupSettings.ShowGroupedColumn : Column.Visible);
            Parent.GroupStates.Clear();
            await UpdateModel(ColumnName).ConfigureAwait(true);
        }

        /// <summary>
        /// Removes a column from the grouping.
        /// Removes the column from group settings, clears its hidden state, and triggers a model update.
        /// </summary>
        /// <param name="ColumnName">The field name of the column to ungroup.</param>
        /// <param name="clearGroupingInvoked">Indicates if this was invoked from a clear grouping action (affects event type).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UnGroupColumn(string ColumnName, bool clearGroupingInvoked = false)
        {
            // TODO: Reordering in Groupdrop
            var Column = GridUtils.GetColumnByField(ColumnName, GridUtils.GetColumns(Parent));
            var temporaryIndentWidth = IndentWidth;
            IndentWidth = string.Empty;
            if(Parent.RowReorderModule != null)
            Parent.RowReorderModule.RowReorderIndentWidth = string.Empty;
            var GCols = Parent.GroupSettings!.Columns?.ToList();
            Parent.GroupStates.Clear();
            if (Column != null && Parent.AllowGrouping && GCols != null && GCols.IndexOf(ColumnName) > -1)
            {
                Column.IsHiddenByGrouping = false;
                Column.SetVisibility(true);
                GCols.Remove(ColumnName);
                await Parent.GroupSettings.UpdateProperties("Columns", GCols.ToArray()).ConfigureAwait(true);
                if (Parent.SortModule != null && Parent.SortModule.SortedColumns?.IndexOf(ColumnName) == -1)
                {
                    var RemoveCol = Parent.SortSettings!.Columns?.Where(col => col.Field == ColumnName).FirstOrDefault();
                    Parent.SortSettings.Columns?.Remove(RemoveCol!);
                }

                Parent.SortModule?.SortedColumns?.Remove(ColumnName);
                Parent.RefreshColumnHeader = true;
                var tempForceUpdate = Parent.ForceUpdate;
                Parent.ForceUpdate = Parent.SelectedRowIndex >= 0 ? true : Parent.ForceUpdate;
                await Parent.ModelChanged(new ActionEventArgs<T>() { ColumnName = ColumnName, RequestType = Action.UnGrouping }, eventArgs: new GroupingEventArgs() { ColumnName = ColumnName, Action = clearGroupingInvoked ? NotifyCollectionChangedAction.Reset : NotifyCollectionChangedAction.Remove }, requestType:"UnGrouping", temporaryIndentWidth : temporaryIndentWidth).ConfigureAwait(true);
                Parent.ForceUpdate = tempForceUpdate;
            }
        }

        /// <summary>
        /// Updates the group model by adding a column to the group settings and triggering a sort query.
        /// </summary>
        /// <param name="ColumnName">The field name of the column to add to grouping.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UpdateModel(string ColumnName)
        {
            var GCols = Parent.GroupSettings!.Columns?.ToList() ?? new List<string>();
            if (Parent.SortSettings != null && Parent.SortSettings.Columns != null && Parent.SortSettings.Columns.Any(col => col.Field == ColumnName) && Parent.SortModule != null)
            {
                Parent.SortModule.SortedColumns?.Add(ColumnName);
            }

            if (GCols.IndexOf(ColumnName) == -1)
            {
                GCols.Add(ColumnName);
            }

            Parent.SortModule?.GroupAddSortingQuery(ColumnName);
            await Parent.GroupSettings.UpdateProperties("Columns", GCols.ToArray()).ConfigureAwait(true);
            Parent.RefreshColumnHeader = true;
            await Parent.ModelChanged(new ActionEventArgs<T>() { ColumnName = ColumnName, RequestType = Action.Grouping }, eventArgs: new GroupingEventArgs() { ColumnName = ColumnName, Action = NotifyCollectionChangedAction.Add }, requestType:"Grouping").ConfigureAwait(true);
        }

        #endregion

        #region UI Helper Methods

        /// <summary>
        /// Checks if there are group caption aggregate templates available for display.
        /// </summary>
        /// <returns>True if group caption templates exist, false otherwise.</returns>
        internal bool IsGroupCaptionTemplate()
        {
            return Parent.GetReactiveAggregateModule().IsGroupCaptionTemplate();
        }
        
        internal async void LazyPageSetting()
        {
                await Parent.PageSettings!.UpdateProperties("CurrentPage", 1).ConfigureAwait(true);
        }
        private void CollapseGroupAndChildren(string targetUid, bool isExpanding)
        {
            int targetIndex = Parent.VirtualScrollModule!.CurrentGroupedData!.FindIndex(x => x.Uid == targetUid);
            if (targetIndex == -1) return;

            var targetItem = Parent.VirtualScrollModule!.CurrentGroupedData[targetIndex];
            targetItem.IsExpand = isExpanding;
            Parent.VirtualScrollModule!.CurrentGroupedData[targetIndex] = targetItem;

            if (Parent.GroupSettings!=null && Parent.GroupSettings.PersistGroupState && targetItem.Item is Group<T> itemGroup && targetItem.ParentUid != null)
            {
                string groupKey = (itemGroup.Key?.ToString() ?? string.Empty) + Grouping<T>.GetUniqueGroupKey(new List<Row<object>>(), targetItem.ParentUid, Parent.VirtualScrollModule!.CurrentGroupedData);
                Parent.GroupStates[groupKey] = isExpanding;
            }

            var expandLookup = Parent.VirtualScrollModule.CurrentGroupedDataCaptionRowMap;
            var ancestorExpanded = new Dictionary<int, bool>
            {
                [targetItem.Indent] = isExpanding
            };
            var dataCount = Parent.VirtualScrollModule!.CurrentGroupedData.Count;
            for (int i = targetIndex + 1; i < dataCount; i++)
            {
                var currentItem = Parent.VirtualScrollModule!.CurrentGroupedData[i];

                if (currentItem.Indent <= targetItem.Indent)
                    break;

                bool newVisible;
                bool newIsExpand = currentItem.IsExpand;

                if (!isExpanding)
                {
                    newVisible = false;
                }
                else
                {
                    bool parentExpanded = expandLookup !=null ? expandLookup[currentItem.ParentUid!].IsExpand : false;
                    bool allAncestorsExpanded = ancestorExpanded.ContainsKey(currentItem.Indent - 1) &&
                                                ancestorExpanded[currentItem.Indent - 1];
                    newVisible = parentExpanded && allAncestorsExpanded;
                }

                ancestorExpanded[currentItem.Indent] = newVisible && newIsExpand;

                currentItem.Visible = newVisible;
                currentItem.IsExpand = newIsExpand;

                Parent.VirtualScrollModule!.CurrentGroupedData[i] = currentItem;
            }
        }

        #endregion

        #region Expand and Collapse - Helper Methods

        /// <summary>
        /// Handles collapse operations for non-virtual row layouts.
        /// </summary>
        private static void CollapseRowsNonVirtual(Row<object> Row, List<Row<object>> Rows)
        {
            Row<object> closestParent = null!;
            for (var i = Rows.FindIndex(row => row == Row) + 1; i < Rows.Count; i++)
            {
                Row<object> childRow = Rows[i];
                if (Rows[i].ParentUid == null)
                {
                    if (childRow.IsDetailRow)
                    {
                        childRow.Visible = Rows[i - 1].Visible;
                    }
                }

                if (!string.Equals(childRow.ParentUid, closestParent?.Uid, StringComparison.Ordinal))
                {
                    closestParent = Rows.Where(r => string.Equals(r.Uid, childRow.ParentUid, StringComparison.Ordinal)).FirstOrDefault()!;
                }

                if (string.Equals(Row.Uid, childRow.ParentUid, StringComparison.Ordinal))
                {
                    childRow.Visible = !Row.IsExpand;
                }
                else if (closestParent != null && string.Equals(closestParent.Uid, childRow.ParentUid, StringComparison.Ordinal))
                {
                    if (closestParent.IsExpand && closestParent.Visible == false)
                    {
                        childRow.Visible = false;
                    }
                    else if (closestParent.IsExpand && closestParent.Visible)
                    {
                        childRow.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles collapse operations for virtual row layouts with CurrentGroupedData.
        /// </summary>
        private static void CollapseRowsVirtual(Row<object> Row, List<Row<object>> Rows)
        {
            var rows = Grouping<T>.CurrentExpandedRows(Rows, Row);
            foreach (var childrow in rows)
            {
                childrow.Visible = !Row.IsExpand;
            }
        }

        /// <summary>
        /// Handles expand/collapse dispatch for infinite scroll scenarios.
        /// </summary>
        private async Task HandleInfiniteScrollExpandCollapse(Row<object> Row)
        {
            var infiniteScrollModule = Parent.InfiniteScrollModule;
            infiniteScrollModule!.InfiniteForceRefresh = true;
            await infiniteScrollModule.ResetInfiniteProperties(infiniteScrollModule.RequestType).ConfigureAwait(true);
            infiniteScrollModule.SetCurrentGroupedRows(Row);
            await Parent.InvokeMethod("sfBlazor.Grid.updateOptions", new object[] { Parent.DataId, Parent.GetClientOption() }).ConfigureAwait(true);
        }

        /// <summary>
        /// Triggers UI refresh after expand/collapse operation completes.
        /// </summary>
        private async Task TriggerExpandCollapseRefresh(bool isEdit)
        {
            if (isEdit)
            {
                Parent.EventAggregator.Trigger("ContentStateChanged", null!);
            }
            else
            {
                if (Parent.EnableVirtualization)
                {
                    Parent.ForceUpdate = true;
                    Parent.EventAggregator.Trigger("VirtualComponentUpdate", null!);
                }
                else
                {
                    Parent.EventAggregator.Trigger("ContentStateChanged", null!);
                }
            }
        }

        #endregion

        #region Expand and Collapse

        /// <summary>
        /// Toggles the expansion/collapse state of a group row and updates the UI accordingly.
        /// Handles both lazy loading and non-lazy loading scenarios, with support for virtual scrolling and infinite scrolling.
        /// </summary>
        /// <param name="Row">The group caption row to toggle.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task ExpandCollapse(Row<object> Row)
        {
            bool isCurrentGroupedDataNotEmpty = Parent.VirtualScrollModule!.CurrentGroupedData != null && Parent.VirtualScrollModule!.CurrentGroupedData.Count > 0;
            if (isCurrentGroupedDataNotEmpty)
            {
                CollapseGroupAndChildren(Row.Uid!, !Row.IsExpand);
                
                if(Parent.GroupSettings != null && Parent.VirtualScrollModule.CurrentGroupedData != null)
                {
                    Parent.VisibleGroupedDataCount = Grouping<T>.GetVisibleGroupeddataCountInternal(Parent.VirtualScrollModule!.CurrentGroupedData, Parent.GroupStates, Parent.GroupSettings.PersistGroupState, Parent.VirtualScrollModule.CurrentGroupedDataCaptionRowMap);
                }
            }
            var isEdit = Parent.EditSettings!.Mode.Equals(EditMode.Normal) && Parent.IsEdit;
            if (isEdit)
            {
                await Parent.EditModule!.CloseEdit().ConfigureAwait(true);
            }
            List<Row<object>> Rows = Parent.VirtualScrollModule?.GeneratedGroupedRows?.Count > 0 ? Parent.GroupSettings != null &&  Parent.GroupSettings.EnableLazyLoading && Parent.EnableVirtualization ? Parent.Rows : Parent.VirtualScrollModule.GeneratedGroupedRows
                                    : Parent.Rows;
            if (Parent.EnableInfiniteScrolling && Parent.GroupSettings != null && !Parent.GroupSettings.EnableLazyLoading && Parent.InfiniteScrollModule != null)
            {
                Rows = Parent.InfiniteScrollModule.GeneratedInfiniteGroupedRows;
                Parent.InfiniteScrollModule.RequestType = "GroupExpandCollapse";
            }
            if(Parent.GroupSettings != null && Parent.GroupSettings.PersistGroupState && !isCurrentGroupedDataNotEmpty)
            {
                var key = (Row.GroupKey!.ToString() ?? "") + Grouping<T>.GetUniqueGroupKey(Rows, Row.ParentUid ?? "");
                Parent.GroupStates[key] = !Parent.GroupStates.GetValueOrDefault(key, Row.IsExpand);
            }
            IsGroupExpandAndCollapse = true;
            if (Parent.GroupSettings != null && Parent.GroupSettings.EnableLazyLoading)
            {
                await LazyExpandCollapse(Row, Rows).ConfigureAwait(true);
            }
            else
            {
                int VisibleCount = Parent.EnableVirtualization ? Rows?.Where(vRow => vRow.Visible).ToList().Count ?? 0 : 0;
                var index = Rows!.FindIndex(row => row == Row) + 1;
                
                // Collapse/expand row visibility based on layout type
                if (!Parent.EnableVirtualization)
                {
                    CollapseRowsNonVirtual(Row, Rows);
                }
                else if (Parent.EnableVirtualization && Parent.VirtualScrollModule!.CurrentGroupedData != null && Parent.VirtualScrollModule!.CurrentGroupedData.Count > 0)
                {
                    CollapseRowsVirtual(Row, Rows);
                }

                Row.IsExpand = !Row.IsExpand;
                var virtualScrollModule = Parent.VirtualScrollModule;
                // Update virtual scroll state
                if (Parent.EnableVirtualization && Parent.VirtualScrollModule!.CurrentGroupedData != null && Parent.VirtualScrollModule!.CurrentGroupedData.Count > 0)
                {
                    virtualScrollModule?.SetGeneratedData((int)virtualScrollModule.RowStartIndex, (int)virtualScrollModule.RowEndIndex, virtualScrollModule.QueriedCurrentViewData!);
                }
                if (Parent.EnableVirtualization && virtualScrollModule != null && Parent.VirtualScrollModule!.CurrentGroupedData != null && Parent.VirtualScrollModule!.CurrentGroupedData.Count == 0)
                {
                    int virtualRowsCount = virtualScrollModule.GeneratedGroupedRows.Count;
                    int exactTopIndex = await virtualScrollModule.GetExactTopIndex(index, virtualRowsCount, VisibleCount).ConfigureAwait(true);
                    virtualScrollModule.SetCurrentViewGroupRows(exactTopIndex, exactTopIndex + Parent.PageSettings!.PageSize);
                }
                
                // Handle infinite scroll expand/collapse
                if (Parent.EnableInfiniteScrolling && Parent.InfiniteScrollModule != null)
                {
                    await HandleInfiniteScrollExpandCollapse(Row).ConfigureAwait(true);
                }
                
                // Trigger UI refresh
                await TriggerExpandCollapseRefresh(isEdit).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handles expand/collapse operations for lazy-loaded grouped data.
        /// Fetches child data from the server when expanding and manages collapse operations.
        /// </summary>
        /// <param name="Row">The group caption row being expanded or collapsed.</param>
        /// <param name="Rows">The list of rendered rows in the current view.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LazyExpandCollapse(Row<Object> Row, List<Row<Object>> Rows)
        {
            Row.IsExpand = !Row.IsExpand;
            var index = Rows?.FindIndex(row => row == Row) ?? 0;
            if (Row.IsExpand)
            {
                if (Parent.AllowPaging && !(Rows?.Any(_ => _.IsDataRow) == true))
                {
                    IsLazyExpand = true;
                }
                var query = new Query();
                var dataModule = Parent.DataModule;
                dataModule?.FilterQuery(query);
                dataModule?.SearchQuery(query);
                dataModule?.AggregateQuery(query);
                dataModule?.SortQuery(query);
                BuildLazyExpandFilterQuery(query, index, Rows!, Row);
                BuildLazyExpandGroupQuery(query, Row);
                query.Queries.LazyLoad = true;
                var data = Parent.DataManager != null ? await Parent.DataManager!.ExecuteQuery<T>(query).ConfigureAwait(true) : null;
                if(data != null)
                {
                    (Row.Data as Group<T>)!.Items = (System.Collections.IEnumerable)data;
                }
                
                if (Parent.EnableVirtualization || Parent.AllowPaging)
                {
                    Parent.VirtualScrollModule?.VirtualExpandCollapse(_expandedRowIndexes);
                }
                if (Parent.EnableInfiniteScrolling && Parent.InfiniteScrollModule != null && Parent.InfiniteScrollModule.LazyLoadPageSize == 0 && Parent.Height.Equals("100%", StringComparison.OrdinalIgnoreCase))
                {
                    int gridHeight = await Parent.InvokeMethod<int>("sfBlazor.Grid.lazyLoadGridHeight", false, Parent.DataId).ConfigureAwait(true);
                    Parent.InfiniteScrollModule.SetLazyLoadPageSize(gridHeight.ToString(CultureInfo.InvariantCulture));
                }
                if(data != null)
                LazyExpand(data, index, Row, _expandedRowIndexes);
            }
            else
            {
                (Row.Data as Group<T>)!.Items = new List<T>();
                List<Row<object>> CollapseRow = new List<Row<object>>();
                if (Parent.EnableVirtualization || Parent.AllowPaging)
                {
                    Parent.VirtualScrollModule?.VirtualExpandCollapse(_expandedRowIndexes);
                }
                else
                {
                    CollapseRow = LazyRows.Where(chunk => chunk.ParentUid == Row?.Uid).ToList();
                }
                LazyCollapse(CollapseRow);
            }
            if (Parent.EnableVirtualization && Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.RefreshVirtualContent(index, Parent.Rows);
                Parent.VirtualScrollModule.RequestType = "GroupExpandCollapse";
                await Parent.InvokeMethod("sfBlazor.Grid.lazyGroupExpand", new object[] { Parent.DataId, Parent.GetClientOption() }).ConfigureAwait(true);
            }
            else
            {
                if (Parent.EnableInfiniteScrolling)
                {
                    await Parent.InvokeMethod("sfBlazor.Grid.lazyGroupExpand", new object[] { Parent.DataId, Parent.GetClientOption() }).ConfigureAwait(true);
                }
                RefreshContentAndPager();
            }
        }

        #endregion

        #region Lazy Load Row Management

        internal void AddUiData(Group<T> value, ref List<object> rows)
        {
            IEnumerable<object> dataValues = (IEnumerable<object>)(value.Items ?? Enumerable.Empty<object>());
            bool isExpanded = false;
            foreach (var item in dataValues ?? Enumerable.Empty<object>())
            {
                isExpanded = true;
                if (item is Group<T> group)
                {
                    rows.Add(item);
                    AddUiData(group, ref rows);
                }
                else
                {
                    rows.Add(item);
                }
            }
            if(isExpanded)
            {
                rows = AddGroupFooter(rows,value);
            }
        }

        #region LazyExpand Helper Methods

        /// <summary>
        /// Determines if data represents grouped objects rather than raw data.
        /// </summary>
        private static bool IsGroupedDataType(object data)
        {
            return data is Group<T> || data is List<Group<T>>;
        }

        /// <summary>
        /// Expands grouped data in infinite scroll mode.
        /// </summary>
        private int LazyExpandInfiniteGroupAsync(object data, int pagerIndex, int index, Row<object> Row)
        {
            int i = pagerIndex + index;
            (IQueryable<Row<object>>, int?) lazyLoadGroupedRows = Parent.InfiniteScrollModule!.LazyLoadExpandCollapse(data, Row, i);
            
            if (lazyLoadGroupedRows.Item2 != null)
            {
                return (int)lazyLoadGroupedRows.Item2 + 1;
            }
            
            return 0;
        }

        /// <summary>
        /// Expands grouped data in non-paged, non-infinite scroll mode.
        /// </summary>
        private int LazyExpandNonPagedGroupAsync(object data, int pagerIndex, int index, Row<object> Row)
        {
            var i = pagerIndex + index;
            var dataValues = data as IEnumerable<object>;
            int groupCount = 0;
            
            foreach (Group<T> obj in dataValues!)
            {
                i++;
                var ExpandRow = new GroupModelGenerator<T>(Parent).GenerateCaptionRow((Group<T>)obj, Row.Indent + 1, 0, 0, 0, Row.Uid!);
                LazyRows.Insert(i, (Row<object>)ExpandRow);
                groupCount++;
            }
            
            Parent.TotalItemCount = Parent.TotalItemCount + groupCount;
            return i + 1;
        }

        /// <summary>
        /// Expands raw data in infinite scroll mode.
        /// </summary>
        private int LazyExpandInfiniteDataAsync(object data, int pagerIndex, int index, Row<object> Row)
        {
            (IQueryable<Row<object>>, int?) lazyLoadDataRows = Parent.InfiniteScrollModule!.LazyLoadExpandCollapse(data, Row);
            
            if (lazyLoadDataRows.Item1 != null)
            {
                LazyRows.InsertRange(pagerIndex + index + 1, lazyLoadDataRows.Item1);
                return pagerIndex + index + 1 + lazyLoadDataRows.Item1.Count();
            }
            
            return 0;
        }

        /// <summary>
        /// Expands raw data in paged mode.
        /// </summary>
        private void LazyExpandPagedAsync(object data, Row<object> Row, Dictionary<int, int> expandedRowIndexes)
        {
            var CurrentPageData = (IEnumerable<object>)data;
            var rowIndex = Parent.Rows?.FindIndex(row => row == Row) + 1;
            var startIndex = (Parent.PageSettings!.PageSize) * (Parent.PageSettings.CurrentPage - 1);
            var expandedData = (IEnumerable<object>)data;
            
            if (rowIndex > Parent.PageSettings.PageSize)
            {
                rowIndex = rowIndex - (Parent.PageSettings.PageSize * Parent.PageSettings.CurrentPage - 1);
            }
            
            var pageSize = (int)(Parent.PageSettings.PageSize - rowIndex)!;
            CurrentPageData = expandedData.Take(pageSize);
            
            GenerateRows(startIndex);
        }

        /// <summary>
        /// Expands raw data in non-paged, non-infinite scroll mode.
        /// </summary>
        private int LazyExpandNonPagedDataAsync(object data, int pagerIndex, int index, Row<object> Row)
        {
            var ExpandRow = new GroupModelGenerator<T>(Parent).GenerateDataRows((IEnumerable<object>)data, Row.Indent + 1, Row.ParentId, Parent.Rows.Count, Row.Uid!);
            LazyRows.InsertRange(pagerIndex + index + 1, ExpandRow);
            
            int rowCount = ExpandRow.Count();
            Parent.TotalItemCount = Parent.TotalItemCount + rowCount;
            
            return pagerIndex + index + 1 + rowCount;
        }

        /// <summary>
        /// Expands data in virtual scroll mode.
        /// </summary>
        private void LazyExpandVirtualAsync()
        {
            var originalStartIndex = Parent.VirtualScrollModule!.RowStartIndex;
            GenerateRows(originalStartIndex);
        }

        /// <summary>
        /// Inserts aggregate footer rows after expansion.
        /// </summary>
        private void AppendAggregateFooterRows(int aggregateIndex, Row<object> Row)
        {
            Parent.GetReactiveAggregateModule().AppendAggregateFooterRows(aggregateIndex, Row, LazyRows);
        }

        #endregion

        private void LazyExpand(object data, int index, Row<object> Row, Dictionary<int, int> expandedRowIndexes = null!)
        {
            var aggregateIndex = 0;
            var pagerIndex = 0;
            if (Parent.PageModule != null)
            {
                pagerIndex = Parent.PageModule.CalculatePagerIndex();
            }

            if (!Parent.EnableVirtualization)
            {
                if (IsGroupedDataType(data) && Parent.EnableInfiniteScrolling && Parent.InfiniteScrollModule != null)
                {
                    aggregateIndex = LazyExpandInfiniteGroupAsync(data, pagerIndex, index, Row);
                }
                else if (IsGroupedDataType(data) && !Parent.AllowPaging)
                {
                    aggregateIndex = LazyExpandNonPagedGroupAsync(data, pagerIndex, index, Row);
                }
                else
                {
                    // Raw data paths
                    if (Parent.EnableInfiniteScrolling && Parent.InfiniteScrollModule != null)
                    {
                        aggregateIndex = LazyExpandInfiniteDataAsync(data, pagerIndex, index, Row);
                    }
                    else if (Parent.AllowPaging)
                    {
                        LazyExpandPagedAsync(data, Row, expandedRowIndexes);
                    }
                    else
                    {
                        aggregateIndex = LazyExpandNonPagedDataAsync(data, pagerIndex, index, Row);
                    }
                }
            }
            else if (Parent.EnableVirtualization && Parent.VirtualScrollModule != null)
            {
                LazyExpandVirtualAsync();
            }

            // Insert aggregate footers if applicable
            AppendAggregateFooterRows(aggregateIndex, Row);
        }
        internal int ProcessGroupedData(DataResult dataResult, string primaryKeyColumnName, object value)
        {
            int index = 0;
            bool FindInGroup(Group<T> group)
            {
                var items = (Parent.GroupSettings != null && Parent.GroupSettings.EnableLazyLoading ? group.GroupedData : group.Items)?.Cast<object>()
                            ?? Enumerable.Empty<object>();
                foreach (var item in items)
                {
                    switch (item)
                    {
                        case Group<T> nestedGroup when FindInGroup(nestedGroup):
                            return true;
                        case T row when !GridUtils.CompareValues(Parent.PropHelper?.GetObject(primaryKeyColumnName, row), value):
                            return true;
                        case T:
                            index++;
                            break;
                    }
                }
                return false;
            }
            if(dataResult.Result != null)
            {
                foreach (var groupObj in dataResult.Result)
                {
                    if (groupObj is Group<T> group && FindInGroup(group))
                        return index;
                }
            }
           
            return -1;
        }
        internal int CalculatePageIndex(IEnumerable<object> currentViewData, int targetIndex, int pageSize)
        {
            var uiData = GetUiData(currentViewData);
            int captionCount = 0;
            int dataIndex = 0;
            foreach (var item in uiData)
            {
                if (item.GetType().IsGenericType &&
                    item.GetType().GetGenericTypeDefinition() == typeof(Group<>))
                {
                    captionCount++;
                }
                else
                {
                    if (dataIndex == targetIndex)
                        break;
                    dataIndex++;
                }
            }
            return dataIndex == targetIndex ? (targetIndex + captionCount) / pageSize + 1 : Parent.PageSettings!.CurrentPage;
        }

        /// <summary>
        /// Flattens grouped data into a flat UI-renderable list that includes both group captions and data rows.
        /// Recursively processes nested groups and appends aggregate footer rows where applicable.
        /// </summary>
        /// <param name="currentViewData">The grouped data structure to flatten.</param>
        /// <returns>A flat list of UI objects (groups and data rows) ready for rendering.</returns>
        internal List<object> GetUiData(IEnumerable<object> currentViewData)
        {
            var uiData = new List<object>();
            foreach (var childData in currentViewData)
            {
                bool isExpanded = false;
                uiData.Add(childData);
                var items = (childData as Group<T>)?.Items ?? Enumerable.Empty<object>();
                foreach (var uiItem in items)
                {
                    isExpanded = true;
                    ChildDataExist = true;
                    uiData.Add(uiItem);
                    if (uiItem is Group<T> subgroup)
                    {
                        AddUiData(subgroup, ref uiData);
                    }
                }
                if (isExpanded)
                {
                   uiData =  AddGroupFooter(uiData, (childData as Group<T>)!);
                    
                }
            }

            return uiData;
        }
        private List<object> AddGroupFooter(List<object> uiData, Group<T> group)
        {
            return Parent.GetReactiveAggregateModule().AddGroupFooter(uiData, group);
        }

        private void GenerateRows(int startIndex)
        {
            List<object> uiData = GetUiData(Parent.CurrentViewData!);
            List<object> expandedRows = uiData.Skip(startIndex).Take(Parent.PageSettings!.PageSize).ToList();
            var lazyRows = GenerateLazyRowsobject(expandedRows, startIndex);
            Parent.Rows = (List<Row<object>>)lazyRows;
            Parent.TotalItemCount = uiData.Count;
        }

        /// <summary>
        /// Converts a flat UI data collection into Row objects for rendering.
        /// Handles conversion of both Group caption rows and data rows, with proper parent-child relationships.
        /// </summary>
        /// <param name="uiCollection">The flat collection of UI data objects (groups and data items).</param>
        /// <param name="index">The starting index for row numbering.</param>
        /// <param name="parentUid">The parent UID for establishing parent-child relationships.</param>
        /// <returns>A list of Row objects ready for grid rendering.</returns>
        internal List<Row<object>> GenerateLazyRowsobject(List<object> uiCollection, int index = 0, string parentUid = null!)
        {
            List<Row<object>> rowObjects = new List<Row<object>>();
            foreach (var item in uiCollection)
            {
                if(item is Row<object>)
                {
                    rowObjects.Add((item as Row<object>)!);
                }
                else if (item is Group<T>)
                {
                    Group<T> captionItem = (Group<T>)item;
                    string captionParentUid = null!;
                    string Uid = null!;
                    if (Parent.EnableInfiniteScrolling)
                    {
                        object groupKey = (captionItem as Group<T>)?.Key!;
                        Row<object> infiniteCaptionRow = Parent.InfiniteScrollModule!.CaptionRowsList?.FirstOrDefault(x => x.GroupKey == groupKey)!;
                        Uid = infiniteCaptionRow?.Uid!;
                        captionParentUid = infiniteCaptionRow != null ? null! : parentUid!;
                    }
                    int indent = Parent.GroupSettings!.Columns!.IndexOf(captionItem.Field);
                    Row<object> captionRow = new GroupModelGenerator<T>(Parent).GenerateCaptionRow(captionItem, indent, parentUid: captionParentUid, uid: Uid);
                    rowObjects.Add(captionRow);
                }
                else
                {
                    Row<object> dataRow = new GroupModelGenerator<T>(Parent).GenerateRow(item, index, cssClass: index == 0 ? null! : "e-firstchildrow", indent: Parent.GroupSettings!.Columns!.Length, parentUid: parentUid);
                    rowObjects.Add(dataRow);
                    index++;
                }
            }
            return rowObjects;
        }

        #region LazyCollapse Helper Methods

        /// <summary>
        /// Handles collapse operations for virtual scroll or paged layouts.
        /// </summary>
        private void LazyCollapseVirtualOrPagedAsync()
        {
            var originalStartIndex = 0;
            var originalEndIndex = 0;
            var virtualScrollModule = Parent.VirtualScrollModule;
            if (Parent.EnableVirtualization && virtualScrollModule != null)
            {
                originalStartIndex = virtualScrollModule.RowStartIndex;
                originalEndIndex = virtualScrollModule.RowQueryEndIndex;
            }
            
            if (Parent.AllowPaging)
            {
                originalStartIndex = (Parent.PageSettings!.CurrentPage - 1) * Parent.PageSettings.PageSize;
                originalEndIndex = Parent.PageSettings.PageSize;
            }

            List<object> uiData = new List<object>();
            ChildDataExist = false;
            uiData = GetUiData(Parent.CurrentViewData!);

            if (Parent.EnableVirtualization && virtualScrollModule != null && virtualScrollModule.RowEndIndex >= virtualScrollModule.VisibleGroupRows.Count && !ChildDataExist)
            {
                originalEndIndex = virtualScrollModule.VisibleGroupRows.Count;
                var sIndex = originalEndIndex - Parent.PageSettings!.PageSize;
                originalStartIndex = sIndex < 0 ? 0 : sIndex;
            }
            
            List<object> collapsedRows = uiData.Skip(originalStartIndex).Take(originalEndIndex).ToList();
            Parent.TotalItemCount = uiData.Count; 
            Parent.Rows = Parent.GroupModule?.GenerateLazyRowsobject(collapsedRows, virtualScrollModule!.RowStartIndex)!;
        }

        /// <summary>
        /// Handles collapse operations for non-paged, non-virtual layouts.
        /// </summary>
        private void LazyCollapseNonPagedAsync(List<Row<object>> Rows)
        {
            for (var i = 0; i < Rows?.Count; i++)
            {
                var childRow = Rows?[i];
                var childRowCount = (childRow?.Data as Group<T>)?.Items?.Cast<object>().Count();
                
                if (childRowCount > 0)
                {
                    var Child = LazyRows?.Where(chunk => chunk.ParentUid == childRow?.Uid).ToList();
                    LazyCollapse(Child!);
                }
                
                LazyRows?.Remove(childRow!);
            }
        }

        #endregion

        private void LazyCollapse(List<Row<object>> Rows = null!)
        {
            if (Parent.EnableVirtualization || Parent.AllowPaging)
            {
                LazyCollapseVirtualOrPagedAsync();
            }
            else
            {
                LazyCollapseNonPagedAsync(Rows);
            }
        }

        #endregion

        #region Lazy Expand Query Builders

        /// <summary>
        /// Builds WHERE filter predicates for lazy-loaded group expansion based on parent group values.
        /// Constructs OR conditions for parent group field values to fetch only the relevant child groups.
        /// Used exclusively during lazy expand to fetch child rows matching parent group key.
        /// </summary>
        /// <param name="query">The Query object to add WHERE filters to.</param>
        /// <param name="index">The index of the row being expanded.</param>
        /// <param name="Rows">The list of rendered rows.</param>
        /// <param name="Row">The group caption row being expanded.</param>
        private void BuildLazyExpandFilterQuery(Query query, int index, List<Row<object>> Rows, Row<Object> Row)
        {
            var fields = new List<string>();
            List<WhereFilter> listpredicates = new List<WhereFilter>();
            List<WhereFilter> Orpredicate = new List<WhereFilter>();
            List<List<WhereFilter>> Andpredicate = new List<List<WhereFilter>>();
            if (Parent.EnableVirtualization && Parent.GroupSettings != null && Parent.GroupSettings.EnableLazyLoading)
            {
                Group<T>? currentData = (Row?.Data as Group<T>);
                var GroupedColumns = Parent.GroupSettings.Columns;
                var indent = Row?.Indent ?? 0;
                var currentGroupedData = currentData?.GroupedData?.Cast<object>().Count();
                for (var l = indent; l <= indent; l--)
                {
                    if (l < 0)
                    {
                        break;
                    }
                    var filterValues = new List<object>();
                    var customData = (currentData?.GroupedData as List<T>)![0];
                    var filterValue = Parent.PropHelper?.GetObject(GroupedColumns?[l]!, customData);
                    if (filterValues.IndexOf(filterValue!) == -1)
                    {
                        Orpredicate.Add(new WhereFilter() { Condition = "or", Field = GroupedColumns?[l], value = filterValue, Operator = "equal" });
                        filterValues.Add(filterValue!);
                    }
                    Andpredicate.Add(Orpredicate);
                    Orpredicate = new List<WhereFilter>();
                    fields.Add(currentData?.Field!);
                }
                indent--;
            }
            else
            {
                for (var i = index; i >= 0; i--)
                {
                    Row<object>? childRow = Rows?[i];
                    if ((childRow?.IsCaptionRow == true && fields.IndexOf(((childRow).Data as Group<T>)?.Field!) == -1) && ((childRow.Indent < Rows?[index].Indent) || i == index))
                    {
                        Group<T>? customDatas = childRow.Data as Group<T>;
                        var gColumn = GridUtils.GetColumns(Parent)?.FirstOrDefault(_ => _.Field == customDatas?.Field);
                        var filterValues = new List<object>();
                        var currentGroupedData = customDatas?.GroupedData?.Cast<object>().Count();
                        for (var j = 0; j < currentGroupedData; j++)
                        {
                            var customData = (customDatas?.GroupedData as List<T>)![j];
                            var filterValue = Parent.PropHelper?.GetObject(customDatas?.Field!, customData);
                            if (filterValues.IndexOf(filterValue!) == -1)
                            {
                                Orpredicate.Add(new WhereFilter() { Condition = "or", Field = customDatas?.Field, value = filterValue, Operator = "equal" });
                                filterValues.Add(filterValue!);
                            }
                        }
                        Andpredicate.Add(Orpredicate);
                        Orpredicate = new List<WhereFilter>();
                        fields.Add(customDatas?.Field!);
                        if (childRow.Indent == 0)
                        {
                            break;
                        }
                    }
                }
            }
            for (var k = 0; k < Andpredicate.Count; k++)
            {
                listpredicates.Add(WhereFilter.Or(Andpredicate[k]));
            }
            query.Where(WhereFilter.And(listpredicates));
        }

        /// <summary>
        /// Builds GROUP BY clause for lazy-loaded group expansion to group remaining ungrouped columns.
        /// Applied when expanding a group to fetch the next level of grouped data.
        /// Used exclusively during lazy expand to determine which columns to group by at current level.
        /// </summary>
        /// <param name="query">The Query object to add GROUP BY clause to.</param>
        /// <param name="Row">The group caption row being expanded, used to determine the grouping level.</param>
        private void BuildLazyExpandGroupQuery(Query query, Row<object> Row)
        {
            var level = Parent.GroupSettings!.Columns!.IndexOf((Row.Data as Group<T>)?.Field) + 1;
            if (Parent.GroupSettings.Columns?.Length > 0 && Parent.GroupSettings.Columns.Length != level)
            {
                var gCols = new List<string>();
                var groupFormatter = new Dictionary<string, string>();
                for (var i = level; i < Parent.GroupSettings.Columns?.Length; i++)
                {
                    var gColumn = GridUtils.GetColumns(Parent)?.FirstOrDefault(_ => _.Field == Parent.GroupSettings.Columns?[i]);
                    if (gColumn != null)
                    {
                        if (gColumn.EnableGroupByFormat)
                        {
                            groupFormatter.Add(gColumn.Field, gColumn.Format!);
                        }
                        gCols.Add(Parent.GroupSettings.Columns?[i]!);
                    }
                }
                if (gCols.Count != 0)
                {
                    query.Group(gCols, groupFormatter);
                }
            }
        }

        #endregion

        #region Pager and State Helpers

        /// <summary>
        /// Refreshes the pager and content state after lazy loading expand/collapse operations.
        /// Updates the pager total items count and triggers content refresh in the grid.
        /// </summary>
        private async void RefreshContentAndPager()
        {
            if (Parent.AllowPaging && Parent.PagerRef != null)
            {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
                Parent.PagerRef.TotalItemsCount = Parent.TotalItemCount;
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
                if (Parent.PageModule != null)
                {
                    await Parent.PageModule.RefreshPagerAsync().ConfigureAwait(true);
                }
            }
            else
            {
                Parent.Rows = LazyRows?.ToList()!;
            }
            Parent.EventAggregator.Trigger("ContentStateChanged", null!);
        }

        #endregion

        #region Event Handling

        private void CellFocused(object args) => KeyHandler(args).GetAwaiter();

        private async Task KeyHandler(object args)
        {
            BeforeCellFocus focus = (args as BeforeCellFocus)!;

            int count = Parent.GroupSettings!.Columns?.Length ?? 0;
            if (!Parent.AllowGrouping || count == 0 || focus?.Cell == null)
            {
                return;
            }

            CellType _type = focus.Cell.CellType;
            string keyAction = focus.KeyCombination!;
            switch (keyAction)
            {
                case "Enter":
                    if (_type.Equals(CellType.Expand) || _type.Equals(CellType.GroupCaption) ||
                        _type.Equals(CellType.CaptionSummary) || _type.Equals(CellType.GroupCaptionEmpty))
                    {
                        focus.Cancel = true;
                        await ExpandCollapse(focus?.Row!).ConfigureAwait(true);
                    }
                    break;
                case "CtrlDown":
                    await Parent.ExpandAllGroupAsync().ConfigureAwait(true);
                    break;
                case "CtrlUp":
                    await Parent.CollapseAllGroupAsync().ConfigureAwait(true);
                    break;
                case "AltUp":
                case "AltDown":
                    Row<object> row = focus?.Row!;
                    if (row?.RowType != null && row.RowType.Equals("GroupCaption", StringComparison.Ordinal))
                    {
                        if ((keyAction.Equals("AltUp", StringComparison.Ordinal) && row.IsExpand) || (keyAction.Equals("AltDown", StringComparison.Ordinal) && !row.IsExpand))
                        {
                            await ExpandCollapse(row).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        int indx = Parent.Rows.IndexOf(row!);
                        while (indx >= 0)
                        {
                            Row<object>? _r = Parent.Rows?[indx];
                            if (_r != null && _r.RowType != null && _r.RowType.Equals("GroupCaption", StringComparison.Ordinal) && ((keyAction.Equals("AltUp", StringComparison.Ordinal) && _r.IsExpand) || (keyAction.Equals("AltDown", StringComparison.Ordinal) && !_r.IsExpand)))
                            {
                                await ExpandCollapse(_r).ConfigureAwait(true);
                                break;
                            }
                            indx--;
                        }
                    }
                    break;
                case "CtrlSpace":
                    // TODO header grouping/ungrouping
                    break;
            }
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Retrieves a list of child rows for a given parent caption row, including recursively nested children if expanded.
        /// </summary>
        /// <param name="rows">The list of all rendered rows.</param>
        /// <param name="expandOrCollapseRow">The parent group caption row.</param>
        /// <returns>A list of all child rows (direct and nested) under the parent row.</returns>
        private static List<Row<object>> CurrentExpandedRows(List<Row<object>> rows, Row<object> expandOrCollapseRow)
        {
            List<Row<object>> expandedRows = new List<Row<object>>();
            if (rows == null || expandOrCollapseRow == null)
            {
                return expandedRows;
            }
            List<Row<object>> rowsToProcess = rows.Where(row => row.ParentUid == expandOrCollapseRow.Uid).ToList();
            expandedRows.AddRange(rowsToProcess);

            foreach (var row in rowsToProcess)
            {
                if (!string.Equals(row.RowType, "GroupCaption", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (row.IsExpand)
                {
                    List<Row<object>> nestedChildRows = rows.Where(nestedRow => nestedRow.ParentUid == row.Uid).ToList();
                    expandedRows.AddRange(nestedChildRows);

                    if (nestedChildRows.Count > 0 &&
                        nestedChildRows[0].RowType == "GroupCaption" &&
                        nestedChildRows[0].IsExpand)
                    {
                        expandedRows.AddRange(Grouping<T>.CurrentExpandedRows(rows, nestedChildRows[0]));
                    }
                }
            }

            return expandedRows;
        }

        #endregion

        #region Expand All / Collapse All Operations

        /// <summary>
        /// Expands all grouped rows in the Grid.
        /// Sets IsExpand to true for all caption rows and updates visibility accordingly.
        /// Handles lazy loading, infinite scrolling, and virtual scrolling scenarios.
        /// </summary>
        internal async Task ExpandAllGroupsAsync()
        {
            if (!Parent.AllowGrouping)
            {
                return;
            }
            if (Parent.EnableInfiniteScrolling)
            {
                Parent.ForceUpdate = true;
                await Parent.InfiniteScrollModule!.ResetInfiniteProperties("GroupExpandCollapseAll").ConfigureAwait(true);
                Parent.InfiniteScrollModule.RequestType = "GroupExpandCollapseAll";
            }
            if (Parent.GroupSettings!.EnableLazyLoading)
            {
                IsLazyExpandAll = true;
                await Parent.DataProcess().ConfigureAwait(true);
                IsLazyExpandAll = false;
                if (Parent.AllowPaging && Parent.PagerRef != null && Parent.PagerRef.TotalItemsCount != Parent.TotalItemCount)
                {
#pragma warning disable BL0005
                    Parent.PagerRef.TotalItemsCount = Parent.TotalItemCount;
#pragma warning restore BL0005
                    await Parent.PagerRef.RefreshAsync().ConfigureAwait(true);
                }
            }
            else
            {
                if (Parent.VirtualScrollModule!.CurrentGroupedData?.Count > 0)
                {
                    var dataCount = Parent.VirtualScrollModule!.CurrentGroupedData.Count;
                    for (int i = 0; i < dataCount; i++)
                    {
                        var data = Parent.VirtualScrollModule!.CurrentGroupedData[i];

                        if (data.IsCaptionRow)
                        {
                            data.IsExpand = true;
                            if (Parent.GroupSettings != null && Parent.GroupSettings.PersistGroupState && data.Item is Group<T> itemGroup && data.ParentUid != null)
                            {
                                string groupKey = (itemGroup.Key?.ToString() ?? string.Empty) + Grouping<T>.GetUniqueGroupKey(new List<Row<object>>(), data.ParentUid, Parent.VirtualScrollModule!.CurrentGroupedData);
                                Parent.GroupStates[groupKey] = true;
                            }
                        }
                        data.Visible = true;
                    }
                    if (Parent.GroupSettings != null)
                    Parent.VisibleGroupedDataCount = Grouping<T>.GetVisibleGroupeddataCountInternal(Parent.VirtualScrollModule!.CurrentGroupedData, Parent.GroupStates, Parent.GroupSettings.PersistGroupState, Parent.VirtualScrollModule.CurrentGroupedDataCaptionRowMap);

                    Parent.VirtualScrollModule!.SetGeneratedData((int)Parent.VirtualScrollModule.RowStartIndex, (int)Parent.VirtualScrollModule.RowEndIndex, Parent.VirtualScrollModule.QueriedCurrentViewData!);
                }
                else
                {
                    List<Row<object>> gridRows = Parent.VirtualScrollModule?.GeneratedGroupedRows?.Count > 0 ? Parent.VirtualScrollModule.GeneratedGroupedRows : Parent.Rows;
                    if (Parent.EnableInfiniteScrolling && Parent.InfiniteScrollModule != null)
                    {
                        gridRows = Parent.InfiniteScrollModule.GeneratedInfiniteGroupedRows;
                    }
                    foreach (var row in gridRows)
                    {
                        row.Visible = true;
                        if (!row.IsExpand)
                        {
                            row.IsExpand = row.IsDataRow ? false : true;
                            if (Parent.GroupSettings.PersistGroupState && !row.IsDataRow)
                            {
                                var key = (row.GroupKey!.ToString() ?? "") + Grouping<T>.GetUniqueGroupKey(gridRows, row.ParentUid ?? "");
                                Parent.GroupStates[key] = true;
                            }
                        }
                    }
                }
                if (Parent.EnableVirtualization)
                {
                    Parent.ForceUpdate = true;
                }
                await Parent.CallStateHasChangedAsync().ConfigureAwait(true);
                if (Parent.EnableInfiniteScrolling)
                {
                    await Parent.InvokeMethod("sfBlazor.Grid.resetExpandCollapseAllScroll", new object[] { Parent.DataId, Parent.InfiniteScrollModule?.RequestType! }).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Collapses all grouped rows in the Grid.
        /// Sets IsExpand to false for all caption rows and hides their child rows.
        /// Handles lazy loading, infinite scrolling, and virtual scrolling scenarios.
        /// </summary>
        internal async Task CollapseAllGroupsAsync()
        {
            if (!Parent.AllowGrouping)
            {
                return;
            }
            if(Parent.EnableInfiniteScrolling)
            {
                Parent.ForceUpdate = true;
                Parent.InfiniteScrollModule!.RequestType = "GroupExpandCollapseAll";
                await Parent.InfiniteScrollModule.ResetInfiniteProperties("GroupExpandCollapseAll").ConfigureAwait(true);
            }
            if (Parent.GroupSettings != null && Parent.GroupSettings.EnableLazyLoading)
            {
                if (Parent.EnableVirtualization && Parent.VirtualScrollModule != null)
                {
                    Parent.VirtualScrollModule.GeneratedGroupedRows = new List<Row<object>>();
                }
                else if (Parent.AllowPaging && Parent.PageSettings != null)
                {
                    await Parent.PageSettings.UpdateProperties("CurrentPage", 1).ConfigureAwait(true);
                }
                await Parent.DataProcess().ConfigureAwait(true);
            }
            else
            {
                if (Parent.VirtualScrollModule!.CurrentGroupedData != null && Parent.VirtualScrollModule!.CurrentGroupedData.Count > 0)
                {
                    var dataCount = Parent.VirtualScrollModule!.CurrentGroupedData.Count;
                    for (int i = 0; i < dataCount; i++)
                    {
                        var data = Parent.VirtualScrollModule!.CurrentGroupedData[i];
                        bool isTopLevel = data.Indent == 0;

                        data.Visible = isTopLevel;
                        data.IsExpand = false;
                        if (data.IsCaptionRow && Parent.GroupSettings != null && Parent.GroupSettings.PersistGroupState && data.Item is Group<T> itemGroup && data.ParentUid != null)
                        {
                            string groupKey = (itemGroup.Key?.ToString() ?? string.Empty) + Grouping<T>.GetUniqueGroupKey(new List<Row<object>>(), data.ParentUid, Parent.VirtualScrollModule!.CurrentGroupedData);
                            Parent.GroupStates[groupKey] = false;
                        }

                        Parent.VirtualScrollModule!.CurrentGroupedData[i] = data;
                    }
                    if(Parent.GroupSettings != null)
                    {
                        Parent.VisibleGroupedDataCount = Grouping<T>.GetVisibleGroupeddataCountInternal(Parent.VirtualScrollModule!.CurrentGroupedData!, Parent.GroupStates, Parent.GroupSettings.PersistGroupState, Parent.VirtualScrollModule.CurrentGroupedDataCaptionRowMap);
                    }
                    
                    Parent.VirtualScrollModule!.SetGeneratedData(
                        (int)Parent.VirtualScrollModule.RowStartIndex,
                        (int)Parent.VirtualScrollModule.RowEndIndex,
                        Parent.VirtualScrollModule.QueriedCurrentViewData!
                    );
                }
                else
                {
                    List<Row<object>> gridRows = Parent.VirtualScrollModule?.GeneratedGroupedRows?.Count > 0 ? Parent.VirtualScrollModule.GeneratedGroupedRows : Parent.Rows;
                    if (Parent.EnableInfiniteScrolling && Parent.InfiniteScrollModule != null)
                    {
                        gridRows = Parent.InfiniteScrollModule.GeneratedInfiniteGroupedRows;
                    }
                    foreach (var row in gridRows)
                    {
                        row.Visible = row.ParentUid == null ? true : false;
                        if (row.IsExpand)
                        {
                            row.IsExpand = false;
                            if (Parent.GroupSettings != null && Parent.GroupSettings.PersistGroupState && !row.IsDataRow)
                            {
                                var key = (row.GroupKey!.ToString() ?? "") + Grouping<T>.GetUniqueGroupKey(gridRows, row.ParentUid ?? "");
                                Parent.GroupStates[key] = false;
                            }
                        }
                    }
                }
                if(Parent.EnableVirtualization)
                {
                    Parent.ForceUpdate = true;
                }
                await Parent.CallStateHasChangedAsync().ConfigureAwait(true);
                if (Parent.EnableInfiniteScrolling)
                {
                    await Parent.InvokeMethod("sfBlazor.Grid.resetExpandCollapseAllScroll", new object[] { Parent.DataId, Parent.InfiniteScrollModule?.RequestType!}).ConfigureAwait(true);
                }
            }
        }

        #endregion

        #region Static Utility Methods for Group Operations

        /// <summary>
        /// Internal static helper for calculating unique group keys.
        /// Used for group state persistence and group identification.
        /// </summary>
        internal static string GetUniqueGroupKey(List<Row<object>> rows, string parentUid, IEnumerable<GroupedDataItem> currentGroupedDataItems = null!)
        {
            if (string.IsNullOrEmpty(parentUid))
            {
                return string.Empty;
            }
            if(currentGroupedDataItems != null)
            {
                var parentDataItem = currentGroupedDataItems.FirstOrDefault(dataItem => dataItem.Uid == parentUid);
                if (parentDataItem != null && parentDataItem.Item is Group<T> parentDataGroupItem && currentGroupedDataItems != null)
                {
                    return parentDataGroupItem.Key?.ToString() + GetUniqueGroupKey(new List<Row<object>>(), parentDataItem.ParentUid!, currentGroupedDataItems);
                }
            }
            var parentRow = rows?.FirstOrDefault(row => row.Uid == parentUid);
            return parentRow is null ? string.Empty : parentRow.GroupKey?.ToString() + GetUniqueGroupKey(rows!, parentRow.ParentUid!);
        }

        /// <summary>
        /// Internal static helper for calculating visible grouped data count.
        /// Applies group state persistence and calculates total visible rows including captions.
        /// </summary>
        internal static int GetVisibleGroupeddataCountInternal(IEnumerable<GroupedDataItem> groupedData, Dictionary<string, bool> groupStates = null!, bool isPersistGroupState = false, Dictionary<string, GroupedDataItem>? parentLookup = null)
        {
            if (groupedData == null)
                return 0;
            int count = 0;
            bool isGroupStatePersist = isPersistGroupState && groupStates != null;
            foreach (var item in groupedData)
            {
                if(isGroupStatePersist)
                {
                    if (item.Item is Group<T> itemGroup && item.ParentUid != null)
                    {
                        string groupKey = (itemGroup.Key?.ToString() ?? string.Empty) + GetUniqueGroupKey(new List<Row<object>>(), item.ParentUid, groupedData);
                        if (groupStates!=null && groupStates.TryGetValue(groupKey, out var isExpand))
                        {
                            item.IsExpand = isExpand;
                        }
                    }
                    if (parentLookup != null && !string.IsNullOrEmpty(item.ParentUid))
                    {
                        if (parentLookup.TryGetValue(item.ParentUid, out var parent))
                        {
                            item.Visible = parent.IsExpand && parent.Visible;
                        }
                    }
                }
                if (item.Visible)
                    count++;
            }
            return count;
        }

        #endregion
    }
}
