using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Syncfusion.Blazor.Data;
using System.Globalization;
using System.Collections;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles infinite scrolling feature.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class InfiniteScroll<T>
    {
        // Private Properties
        private SfGrid<T> _parent { get; set; }

        public InfiniteScroll(SfGrid<T> parent) => _parent = parent;

        private List<object> _infiniteCurrentViewData { get; set; } = new List<object>();

        /// <summary>
        /// Gets or sets the list of caption rows.
        /// </summary>
        internal List<Row<object>> CaptionRowsList { get; set; } = new List<Row<object>>();

        /// <summary>
        /// Gets or sets the list of generated infinite grouped rows.
        /// </summary>
        internal List<Row<object>> GeneratedInfiniteGroupedRows { get; set; } = new List<Row<object>>();

        /// <summary>
        /// Gets or sets a value indicating whether the infinite scroll should force a refresh.
        /// </summary>
        internal bool InfiniteForceRefresh { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the scroll is upward.
        /// </summary>
        internal bool IsUpScroll { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the scroll is in the downward direction.
        /// </summary>
        internal bool IsDownScroll { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it is the initial render of the infinite scroll.
        /// </summary>
        internal bool IsInfiniteInitialRender { get; set; } = true;

        /// <summary>
        /// Gets or sets the type of the request for infinite scrolling.
        /// </summary>
        internal string RequestType { get; set; } = "InfiniteScrolling";

        /// <summary>
        /// Gets or sets the size of the lazy load block.
        /// </summary>
        internal int LazyLoadBlockSize { get; set; }

        /// <summary>
        /// Gets or sets the number of items to load in each lazy load page.
        /// </summary>
        internal int LazyLoadPageSize { get; set; }

        internal int CurrentRowIndex { get; set; }

        internal int PreRowIndex { get; set; }

        internal bool RenderMaskTable { get; set; }

        internal IDictionary<int, Row<object>> InfiniteGeneratedRows = new Dictionary<int, Row<object>>();

        internal bool KeyInteractionScroll { get; set; }

        /// <summary>
        /// Configures the query for infinite scrolling based on the current page settings and infinite scroll settings.
        /// </summary>
        /// <param name="query">The query object to be configured for infinite scrolling.</param>
        public void IntialInfinitePageQuery(Query query)
        {
            int currentPage = _parent.PageSettings!.CurrentPage;
            int infiniteInitialBlocks = _parent.InfiniteScrollSettings!.InitialBlocks;
            int infiniteMaximumBlocks = _parent.InfiniteScrollSettings.MaximumBlocks;
            int gridPageSize = _parent.PageSettings.PageSize;
            int initialBlockPageSize = gridPageSize * infiniteInitialBlocks;
            int maximumBlockPageSize = gridPageSize * infiniteMaximumBlocks;
            int parentRowsCount = _parent.Rows?.Count ?? 0;
            if (_parent.InfiniteScrollSettings.EnableCache && infiniteInitialBlocks > infiniteMaximumBlocks)
            {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
                _parent.InfiniteScrollSettings.InitialBlocks = infiniteInitialBlocks = infiniteMaximumBlocks;
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
            }
            if (IsInfiniteInitialRender)
            {
                _ = query.Page(currentPage, initialBlockPageSize);
                IsInfiniteInitialRender = false;
            }
            else if (parentRowsCount > 0)
            {
                if (new HashSet<string> { "Save", "Delete", "Add", "RowDragAndDrop", "PDFExport", "ExcelExport", "CsvExport" }.Contains(RequestType))
                {
                    if (_parent.InfiniteScrollSettings.EnableCache)
                    {
                        query.Queries.Skip = (int)(_parent.Rows?.FirstOrDefault()?.Index ?? 0);
                        query.Queries.Take = parentRowsCount < maximumBlockPageSize && parentRowsCount <= initialBlockPageSize ? initialBlockPageSize : maximumBlockPageSize;
                    }
                    else
                    {
                        currentPage = 1;
                        _ = query.Page(currentPage, parentRowsCount);
                    }
                }
                else if (RequestType == "InfiniteScrolling")
                {
                    _ = query.Page(currentPage, gridPageSize);
                    int totalItemCount = _parent.TotalItemCount;
                    if ((maximumBlockPageSize + gridPageSize) > totalItemCount)
                    {
                        int itemsToLoad = totalItemCount - maximumBlockPageSize;
                        query.Queries.Take = itemsToLoad > 0 ? itemsToLoad : totalItemCount - query.Queries.Skip;
                    }
                    if (IsDownScroll && currentPage < infiniteMaximumBlocks)
                    {
                        InfiniteGeneratedRows?.Clear();
                        _infiniteCurrentViewData.Clear();
                        _parent.Rows = new List<Row<object>>();
                        query.Queries.Take = maximumBlockPageSize;
                    }
                }
            }
        }
        /// <summary>
        /// Sets the current grouped rows based on the specified caption row.
        /// </summary>
        /// <param name="CaptionRow">The caption row to set the current grouped rows.</param>
        /// <returns>The list of current grouped rows.</returns>
        internal List<Row<object>>? SetCurrentGroupedRows(Row<object> CaptionRow = null!)
        {
            bool enableCache = _parent.InfiniteScrollSettings!.EnableCache;
            int gridPageSize = _parent.PageSettings!.PageSize;
            int initialBlocks = _parent.InfiniteScrollSettings.InitialBlocks;
            int maximumBlocks = _parent.InfiniteScrollSettings.MaximumBlocks;
            bool lazyLoadGrouping = _parent.GroupSettings!.EnableLazyLoading;
            int currentPage = _parent.PageSettings.CurrentPage;

            if (lazyLoadGrouping)
            {
                SetLazyLoadPageSize();
            }

            int upStart = 0;
            List<Row<object>> newRows = new List<Row<object>>();
            List<Row<object>> infiniteGroupedRows = new List<Row<object>>();
            if (enableCache && initialBlocks > maximumBlocks)
            {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
                _parent.InfiniteScrollSettings.InitialBlocks = initialBlocks = maximumBlocks;
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
            }
            int loadSize = gridPageSize * initialBlocks;
            int skip = (currentPage - 1) * gridPageSize;
            int take = IsInfiniteInitialRender ? loadSize : gridPageSize;
            infiniteGroupedRows = _parent.Rows;

            newRows = GeneratedInfiniteGroupedRows.Where(x => x.Visible).ToList();
            if (IsDownScroll)
            {
                if (enableCache)
                {
                    var lastRow = infiniteGroupedRows.LastOrDefault();
                    int lastIndex = (lastRow != null) ? newRows.LastIndexOf(lastRow) : -1;
                    skip = (lastIndex != -1) ? lastIndex + 1 : 0;
                }

                List<Row<object>> rowsToAdd = newRows.Skip(skip).Take(take).ToList();
                infiniteGroupedRows.AddRange(rowsToAdd);
            }
            else if (IsUpScroll)
            {
                if (currentPage >= 0)
                {
                    if (enableCache)
                    {
                        upStart = newRows.FindIndex(x => x == infiniteGroupedRows.FirstOrDefault());
                        skip = (upStart - gridPageSize) <= 0 ? 0 : upStart - gridPageSize;
                    }
                    newRows = newRows.Skip(skip).Take(take).ToList();
                    infiniteGroupedRows.InsertRange(0, newRows);
                }
            }
            else
            {
                int insertIndex = 0;
                int skipChild = 0;
                List<Row<object>> correspondingChild = new List<Row<object>>();
                List<Row<object>> childRows = new List<Row<object>>();
                if (CaptionRow != null && CaptionRow.IsExpand)
                {
                    correspondingChild = newRows.Where(x => x.ParentUid == CaptionRow.Uid).ToList();
                    insertIndex = _parent.Rows.IndexOf(CaptionRow);
                    skipChild = newRows.IndexOf(CaptionRow);
                    Group<T>? groupData = CaptionRow.Data as Group<T>;                  
                    int takeCount = groupData?.Items?.AsQueryable().Count() ?? 0;
                    if (groupData?.Items is Group<T>)
                    {
                        takeCount = InfiniteScroll<T>.FetchGroupedChildCount(groupData.Items);
                    }
                    if (_parent.Aggregates?.Count > 0)
                    {
                        takeCount += InfiniteScroll<T>.FetchChildAggregateCount(CaptionRow, newRows);
                    }
                    childRows = newRows.Skip(skipChild + 1).Take(takeCount).ToList();
                }

                if (lazyLoadGrouping && _parent.GroupModule != null &&(_parent.GroupModule.IsLazyExpandAll || RequestType == "GroupExpandCollapseAll"))
                {
                    _parent.Rows = newRows;
                }
                else
                {
                    if (RequestType == "Save" || RequestType == "Delete")
                    {
                        int takeCount = _parent.Rows.Count;
                        if (lazyLoadGrouping)
                        {
                            takeCount = _parent.Rows.Where(x => x.IsCaptionRow).Count();
                            skip = currentPage - 1;
                        }
                        skip = enableCache ? Math.Max(currentPage - maximumBlocks, 0) * gridPageSize : 0;
                        _parent.Rows = newRows.Skip(skip).Take(takeCount).ToList();
                    }
                    if (RequestType == "GroupExpandCollapse" && !lazyLoadGrouping && correspondingChild.Count > 0)
                    {
                        int takeCount = ((_parent.Rows.Count + gridPageSize) < loadSize) || (newRows.LastOrDefault() != _parent.Rows?.LastOrDefault()) ? loadSize : ((_parent.Rows?.Count ?? 0) + gridPageSize);
                        _parent.Rows?.InsertRange(insertIndex + 1, childRows);
                        if (enableCache)
                        {
                            skip = 0;
                            _parent.Rows = _parent.Rows?.Skip(skip).Take(takeCount).ToList()!;
                        }
                    }
                    else if (IsInfiniteInitialRender)
                    {
                        _parent.Rows = newRows.Skip(skip).Take(loadSize).ToList();
                    }
                    else if (InfiniteForceRefresh && (RequestType == "GroupExpandCollapse" || RequestType == "GroupExpandCollapseAll"))
                    {
                        int takeCount = enableCache ? _parent.Rows.Count : (currentPage * gridPageSize);
                        if (RequestType == "GroupExpandCollapseAll")
                        {
                            takeCount = loadSize;
                        }
                        else
                        {
                            skip = 0;
                            if (enableCache)
                            {
                                skip = Math.Max(newRows.IndexOf(_parent.Rows?.FirstOrDefault()!), 0);
                                List<Row<object>>? visibleRows = _parent.Rows?.Where(x => x.Visible).ToList();
                                if (visibleRows != null && visibleRows.LastOrDefault() == newRows.LastOrDefault())
                                {
                                    skip = newRows.Count - takeCount;
                                }
                            }
                        }
                        _parent.Rows = newRows.Skip(skip).Take(takeCount).ToList();
                    }
                }
            }
            if (enableCache && (IsDownScroll || IsUpScroll))
            {
                infiniteGroupedRows = RefreshInfiniteCacheRows(infiniteGroupedRows)!;
                _parent.Rows = infiniteGroupedRows;
            }
            return _parent.Rows;
        }

        /// <summary>
        /// Generates data for infinite scrolling in the grid based on the specified action and scroll direction.
        /// </summary>
        /// <param name="action">The action arguments containing the request type and other details.</param>
        /// <param name="isBottom">Indicates whether the scroll direction is towards the bottom.</param>
        /// <param name="isTop">Indicates whether the scroll direction is towards the top.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal async Task GenerateInfiniteScrollDatas(ActionArgs action = null!, bool isBottom = false, bool isTop = false)
        {
            RequestType = action?.RequestType!;
            IsDownScroll = isBottom;
            IsUpScroll = isTop;
            IsInfiniteInitialRender = false;
            string[] groupSettingsColumns = _parent.GroupSettings?.Columns!;
            int gridPageSize = _parent.PageSettings!.PageSize;

            //Normal Grouping and Normal grid infinite scrolling handling in if condition
            if (!_parent.GroupSettings!.EnableLazyLoading || (groupSettingsColumns == null || groupSettingsColumns.Length == 0))
            {
                int currentPageUpdate = _parent.PageSettings.CurrentPage;
                if (_parent.EnableInfiniteScrolling && action?.RequestType == "InfiniteScrolling" && IsDownScroll)
                {
                    int lastRowIndex = 0;
                    if (_parent.AllowGrouping && groupSettingsColumns != null && groupSettingsColumns.Length > 0)
                    {
                        List<Row<object>> visibleRows = GeneratedInfiniteGroupedRows.Where(x => x.Visible).ToList();
                        Row<object>? lastRow = _parent.Rows?.LastOrDefault();
                        lastRowIndex = visibleRows.FindIndex(x => x == lastRow) - 1;
                        if (visibleRows.LastOrDefault() == lastRow)
                        {
                            lastRowIndex = _parent.TotalItemCount;
                        }
                    }
                    else
                    {
                        lastRowIndex = _parent.Rows?.LastOrDefault(x => x.Index != null)?.Index ?? 0;
                        lastRowIndex = lastRowIndex == 0 ? 0 : lastRowIndex + 1;
                    }
                    currentPageUpdate = (int)(Math.Ceiling((double)lastRowIndex / gridPageSize) + 1);
                    currentPageUpdate = currentPageUpdate < _parent.InfiniteScrollSettings!.MaximumBlocks ? 1 : currentPageUpdate;
                }
                else if (_parent.EnableInfiniteScrolling && action?.RequestType == "InfiniteScrolling" && IsUpScroll)
                {
                    if (_parent.AllowGrouping && groupSettingsColumns != null && groupSettingsColumns.Length > 0)
                    {
                        Row<object>? firstRow = _parent.Rows?.FirstOrDefault();
                        if (firstRow != null)
                        {
                            List<Row<object>> visibleRows = GeneratedInfiniteGroupedRows.Where(x => x.Visible).ToList();
                            int rowIndex = visibleRows.FindIndex(x => x == firstRow);
                            currentPageUpdate = (int)(Math.Ceiling((double)rowIndex / gridPageSize) - 1);
                            //While performing up scroll, request made to server is prevented using the current page so when value is negative it correct to set 0
                            currentPageUpdate = currentPageUpdate <= 0 ? 0 : currentPageUpdate;
                        }
                        else
                        {
                            currentPageUpdate = 1; // Default to page 1 if no rows are found
                        }
                    }
                    else
                    {
                        Row<object>? row = _parent.Rows?.ElementAtOrDefault(gridPageSize - 1);
                        int? rowIndex = row?.Index;
                        currentPageUpdate = (int)(Math.Ceiling((double)(rowIndex ?? 0) / gridPageSize) - 1);
                        currentPageUpdate = currentPageUpdate <= 0 ? 1 : currentPageUpdate;
                    }
                }
                await _parent.PageSettings.UpdateProperties("CurrentPage", currentPageUpdate).ConfigureAwait(true);

                if (_parent.AllowGrouping && groupSettingsColumns != null && groupSettingsColumns.Length > 0)
                {
                    _ = SetCurrentGroupedRows();
                    await _parent.CallStateHasChangedAsync().ConfigureAwait(true);
                    await _parent.InvokeSuccessAsync(action).ConfigureAwait(true);
                }
                else
                {
                    await _parent.DataProcess(action).ConfigureAwait(true);
                    _parent.CurrentViewData = _infiniteCurrentViewData;
                }
            }
            else
            {
                List<object> uiData = new List<object>();
                List<Row<object>> lazyRows = new List<Row<object>>();
                int startIndex = 0;
                int insertIndex = 0;
                int currentPageUpdate = _parent.PageSettings.CurrentPage;

                foreach (object data in _parent.CurrentViewData!)
                {
                    uiData.Add(data);
                    IEnumerable? items = (data as Group<T>)?.Items;
                    IEnumerable<object>? dataItems = (IEnumerable<object>)items!;
                    foreach (object childData in dataItems!)
                    {
                        uiData.Add(childData);
                        if (childData is Group<T>)
                        {
                            _parent.GroupModule?.AddUiData((Group<T>)childData, ref uiData);
                        }
                    }
                }

                if (IsDownScroll)
                {
                    insertIndex = _parent.Rows.Count;
                    List<Row<object>> expandedCaptionRow = _parent.Rows.Where(x => x.IsExpand).ToList();
                    int notRenderedChildCount = 0;
                    int childDataCount = 0;
                    for (int i = expandedCaptionRow.Count; i > 0; i--)
                    {
                        List<Row<object>> childRows = _parent.Rows.Where(x => x.ParentUid == expandedCaptionRow[i - 1].Uid).ToList();
                        childDataCount = ((expandedCaptionRow[i - 1].Data as Group<T>)!.Items as IEnumerable<object>)!.Count();
                        if (childDataCount > gridPageSize)
                        {
                            notRenderedChildCount += childDataCount - childRows.Count;
                        }
                    }
                    int lastRowIndex = startIndex = (_parent.Rows.Count) + notRenderedChildCount;
                    currentPageUpdate = (int)(Math.Ceiling((double)lastRowIndex / gridPageSize) + 1);
                }

                await _parent.PageSettings.UpdateProperties("CurrentPage", currentPageUpdate).ConfigureAwait(true);

                List<object> currentUiData = uiData.Skip(startIndex).Take(gridPageSize).ToList();
                lazyRows = _parent.GroupModule?.GenerateLazyRowsobject(currentUiData)!;
                if (lazyRows.Count > 0 && _parent.GroupModule != null)
                {
                    _parent.Rows?.InsertRange(insertIndex, lazyRows);
                    _parent.GroupModule.LazyRows = _parent.Rows!;
                    _parent.EventAggregator.Trigger("ContentStateChanged", null!);
                    await _parent.InvokeSuccessAsync(action).ConfigureAwait(true);
                }
            }
            IsDownScroll = false;
            IsUpScroll = false;
        }

        internal async Task LoadLazyLoadChildData(ActionArgs action = null!, string middleRowUid = null!, string lastRowUid = null!)
        {
            RequestType = action?.RequestType!;
            List<object> uiData = new List<object>();
            List<Row<object>> lazyRows = new List<Row<object>>();
            string childUid = middleRowUid ?? lastRowUid;
            Row<object>? childRow = _parent.Rows?.FirstOrDefault(x => x.Uid == childUid);
            if (childRow == null)
            {
                return;
            }
            Row<object>? parentCaptionRow = _parent.Rows?.FirstOrDefault(x => x.Uid == childRow.ParentUid);
            if (parentCaptionRow == null)
            {
                return;
            }
            List<Row<object>>? renderedChildRows = _parent.Rows?.Where(x => x.ParentUid == parentCaptionRow.Uid).ToList();
            IQueryable? childDataItems = (parentCaptionRow.Data as Group<T>)?.Items?.AsQueryable();
            if (childDataItems == null)
            {
                return;
            }

            IEnumerable<object> childrenItems = childDataItems.Cast<object>();
            if (renderedChildRows?.Count < childDataItems.Count())
            {
                int insertIndex = _parent.Rows!.FindLastIndex(x => x.ParentUid == parentCaptionRow.Uid && x.RowType != "Summary");

                int middleRowIndex = middleRowUid == null
                    ? _parent.Rows.FindIndex(x => x.Uid == lastRowUid) - LazyLoadBlockSize + 1
                    : _parent.Rows.FindIndex(x => x.Uid == middleRowUid);

                if (middleRowIndex < 0 || middleRowIndex >= (_parent.Rows.Count))
                {
                    return;
                }
                string? removeChildUid = middleRowUid ?? _parent.Rows?[middleRowIndex].Uid;
                Row<object>? removeChildRow = _parent.Rows?.FirstOrDefault(x => x.Uid == removeChildUid);
                if (removeChildRow != null)
                {
                    removeChildRow.CssClass = "e-firstchildrow";
                }

                Row<object>? lastChildRow = _parent.Rows?.Where(x => x.ParentUid == parentCaptionRow.Uid).LastOrDefault();
                if (lastChildRow != null)
                {
                    lastChildRow.CssClass = "e-firstchildrow";
                }

                List<object> currentChild = childrenItems.Skip(renderedChildRows.Count).Take(_parent.InfiniteScrollModule?.LazyLoadPageSize ?? 0).ToList();
                lazyRows = _parent.GroupModule!.GenerateLazyRowsobject(currentChild, index: renderedChildRows.Count, parentUid: parentCaptionRow.Uid!);

                if ((lazyRows.Count + renderedChildRows.Count) < childDataItems.Count())
                {
                    if (LazyLoadBlockSize < lazyRows.Count)
                    {
                        lazyRows[LazyLoadBlockSize].CssClass += " e-lazyload-middle-down";
                    }

                    if (_parent.InfiniteScrollModule != null && _parent.InfiniteScrollModule.LazyLoadPageSize - 1 < lazyRows.Count)
                    {
                        lazyRows[_parent.InfiniteScrollModule.LazyLoadPageSize - 1].CssClass += " e-lazyload-last-down";
                    }
                }

                _parent.Rows?.InsertRange(insertIndex + 1, lazyRows);
                _parent.GroupModule.LazyRows = _parent.Rows!;
                _parent.SoftRefresh = true;
                RenderMaskTable = false;
                _parent.EventAggregator.Trigger("ContentStateChanged", null!);
                await _parent.InvokeSuccessAsync(action).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Resets the infinite scroll properties based on the specified action.
        /// </summary>
        /// <param name="Action">The action that triggers the reset. Possible values are:
        /// "Filtering", "ClearFiltering", "Sorting", "ClearSorting", "Searching", "Refresh", 
        /// "Reorder", "GroupExpandCollapse", "Grouping", "UnGrouping", "Add", "Save", "Delete", 
        /// and "RowDragAndDrop".</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal async Task ResetInfiniteProperties(string Action = null!)
        {
            int currentPage = _parent.PageSettings!.CurrentPage;
            bool enableCache = _parent.InfiniteScrollSettings!.EnableCache;
            switch (Action)
            {
                case "Filtering":
                case "ClearFiltering":
                case "Sorting":
                case "ClearSorting":
                case "Searching":
                case "Refresh":
                case "Reorder":
                case "Grouping":
                case "UnGrouping":
                    IsInfiniteInitialRender = true;
                    _infiniteCurrentViewData.Clear();
                    InfiniteGeneratedRows?.Clear();
                    currentPage = 1;
                    break;

                case "GroupExpandCollapse":
                    InfiniteGeneratedRows?.Clear();
                    break;
                case "GroupExpandCollapseAll":
                    if (_parent.GroupSettings != null && !_parent.GroupSettings.EnableLazyLoading) 
                    {
                        InfiniteForceRefresh = true;
                    }
                    currentPage = 1;
                    InfiniteGeneratedRows?.Clear();
                    break;

                case "Add":
                case "Save":
                case "Delete":
                case "RowDragAndDrop":
                    _infiniteCurrentViewData.Clear();
                    if (enableCache &&
                        ((Action == "Save" && !_parent.IsEdit && _parent.EditModule!.IsAdd) || Action == "Add"))
                    {
                        currentPage = 1;
                        InfiniteGeneratedRows?.Clear();
                    }
                    if (Action == "Delete")
                    {
                        InfiniteGeneratedRows?.Clear();
                    }
                    break;
            }
            await _parent.PageSettings.UpdateProperties("CurrentPage", currentPage).ConfigureAwait(true);
        }

        /// <summary>
        /// Refreshes the current view data for infinite scrolling based on the provided data.
        /// </summary>
        /// <param name="data">The data to be added to the current view.</param>
        /// <remarks>
        /// This method handles the addition of new data to the current view for infinite scrolling.
        /// It takes into account the page size, initial blocks, cache settings, and the direction of scrolling (up or down).
        /// If caching is enabled, it also manages the removal of excess items to maintain the maximum block size.
        /// </remarks>
        internal void RefreshInfiniteCurrentViewData(IEnumerable<object> data)
        {
            int pageSize = _parent.PageSettings!.PageSize;
            int initialBlocks = _parent.InfiniteScrollSettings!.InitialBlocks;
            bool enableCache = _parent.InfiniteScrollSettings.EnableCache;
            int maxBlocks = _parent.InfiniteScrollSettings.MaximumBlocks;
            int maxBlock = maxBlocks * pageSize;

            if (initialBlocks > 1 && data.Count() == (initialBlocks * pageSize) && _parent.GroupSettings != null &&  _parent.GroupSettings.Columns == null)
            {
                _infiniteCurrentViewData.AddRange(data);
            }
            else
            {
                if (enableCache && IsUpScroll)
                {
                    _infiniteCurrentViewData.InsertRange(0, data);
                }
                else
                {
                    _infiniteCurrentViewData.AddRange(data);
                }
            }
            if (enableCache && (_infiniteCurrentViewData.Count > maxBlock))
            {
                if (IsDownScroll)
                {
                    int itemsToRemove = Math.Min(GetMinimumValueCount(), _infiniteCurrentViewData.Count - pageSize);
                    _infiniteCurrentViewData.RemoveRange(0, itemsToRemove);
                }
                else if (IsUpScroll)
                {
                    if (_infiniteCurrentViewData.Count > maxBlock)
                    {
                        int itemsToRemove = Math.Min(GetMinimumValueCount(), _infiniteCurrentViewData.Count - maxBlock);
                        _infiniteCurrentViewData.RemoveRange(_infiniteCurrentViewData.Count - itemsToRemove, itemsToRemove);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the top rows from the provided list of rows if the total count exceeds the maximum allowed blocks.
        /// </summary>
        /// <param name="rows">The list of rows to be processed.</param>
        /// <param name="maxIndex">The maximum index up to which rows can be removed.</param>
        /// <returns>The modified list of rows after removing the top rows if necessary.</returns>
        internal List<Row<object>>? RemoveTopRows(List<Row<object>> rows, int maxIndex)
        {
            int maxBlock = _parent.InfiniteScrollSettings!.MaximumBlocks * _parent.PageSettings!.PageSize;
            if (rows?.Count > maxBlock)
            {
                int itemsToRemove = Math.Min(GetMinimumValueCount(), rows.Count - maxBlock);
                rows?.RemoveRange(0, itemsToRemove);
            }
            return rows;
        }

        /// <summary>
        /// Removes a specified number of rows from the bottom of the list if the total number of rows exceeds the maximum allowed blocks.
        /// </summary>
        /// <param name="rows">The list of rows to be modified.</param>
        /// <param name="maxIndex">The maximum index up to which rows can be removed.</param>
        /// <returns>The modified list of rows after removing the bottom rows.</returns>
        internal List<Row<object>>? RemoveBottomRows(List<Row<object>> rows, int maxIndex)
        {
            int gridPageSize = _parent.PageSettings!.PageSize;
            int maxBlock = _parent.InfiniteScrollSettings!.MaximumBlocks * gridPageSize;

            if (rows?.Count > maxBlock)
            {
                int itemsToRemove = Math.Min(GetMinimumValueCount(), maxIndex);
                int startIndex = maxIndex - itemsToRemove;

                if (startIndex >= 0)
                {
                    rows?.RemoveRange(startIndex, itemsToRemove);
                }
            }

            return rows;
        }

        /// <summary>
        /// Refreshes the infinite cache rows based on the scroll direction.
        /// </summary>
        /// <param name="infiniteRows">The list of infinite rows to be refreshed.</param>
        /// <returns>The refreshed list of infinite rows.</returns>
        /// <remarks>
        /// If the scroll direction is down, the top rows are removed based on the page size.
        /// If the scroll direction is up, the bottom rows are removed.
        /// </remarks>
        internal List<Row<object>>? RefreshInfiniteCacheRows(List<Row<object>> infiniteRows)
        {
            if (IsDownScroll)
            {
                infiniteRows = RemoveTopRows(infiniteRows, _parent.PageSettings!.PageSize)!;
            }
            if (IsUpScroll)
            {
                infiniteRows = RemoveBottomRows(infiniteRows, infiniteRows.Count)!;
            }
            return infiniteRows;
        }

        /// <summary>
        /// Sets the lazy load page size based on the provided grid height.
        /// </summary>
        /// <param name="gridHeight">
        /// The height of the grid as a string. If not provided, the height from the parent grid is used.
        /// </param>
        /// <remarks>
        /// This method calculates the block size by dividing the grid height by the row height and subtracting one.
        /// The lazy load page size is set to three times the block size if it is not already set and the block size is greater than zero.
        /// The lazy load block size is set to half of the lazy load page size, rounded up.
        /// </remarks>
        internal void SetLazyLoadPageSize(string gridHeight = "")
        {
            double height = GridUtils.GetDoubleParsedWidth(string.IsNullOrEmpty(gridHeight) ? _parent.Height : gridHeight);
            double blockSize = Math.Floor(height / _parent.RowHeight) - 1;
            LazyLoadPageSize = LazyLoadPageSize <= 0 && blockSize > 0 ? (int)blockSize * 3 : LazyLoadPageSize;
            LazyLoadBlockSize = (int)Math.Ceiling(LazyLoadPageSize / 2.0);
        }

        /// <summary>
        /// Recursively counts the number of child items in a grouped collection.
        /// </summary>
        /// <param name="groupItems">The grouped collection of items to count.</param>
        /// <returns>The total number of child items in the grouped collection.</returns>
        private static int FetchGroupedChildCount(object groupItems)
        {
            int count = 0;
            foreach (object item in (groupItems as IEnumerable<object>)!)
            {
                count++;
                if (item is Group<T> nestedGroup && nestedGroup.Items != null)
                {
                    count += InfiniteScroll<T>.FetchGroupedChildCount(nestedGroup.Items);
                }
            }            
            return count;
        }

        /// <summary>
        /// Fetches the child aggregate count for a given caption row and list of new rows.
        /// </summary>
        /// <param name="captionRow">The caption row.</param>
        /// <param name="newRows">The list of new rows.</param>
        /// <returns>The child aggregate count.</returns>
        private static int FetchChildAggregateCount(Row<object> captionRow, List<Row<object>> newRows)
        {
            int count = 0;
            if (captionRow != null)
            {
                List<Row<object>> childCaptionRows = newRows.Where(x => x.ParentUid == captionRow.Uid).ToList();
                if (childCaptionRows.Count > 0)
                {
                    count = 1 + childCaptionRows.Sum(childCaptionRow => InfiniteScroll<T>.FetchChildAggregateCount(childCaptionRow, newRows));
                }
            }
            return count;
        }

        /// <summary>
        /// Lazy loading expand and collapse row is calculated based on the data and row.
        /// </summary>
        /// <param name="data">The data to be expanded or collapsed.</param>
        /// <param name="Row">The row to expand or collapse.</param>
        /// <param name="index">The index of the row.</param>
        /// <returns>A tuple containing the expanded row and the index.</returns>
        internal (IQueryable<Row<object>> ExpandRow, int? index) LazyLoadExpandCollapse(object data = null!, Row<object> Row = null!, int? index = 0)
        {
            IQueryable<Row<object>> ExpandRow = null!;
            IEnumerable<object>? dataValues = data as IEnumerable<object>;

            if (dataValues == null)
            {
                // Handle the case where data is not a valid IEnumerable<object>
                return (ExpandRow, index);
            }

            _parent.TotalItemCount += dataValues.Count();
            dataValues = dataValues.Skip(0).Take(LazyLoadPageSize).ToList();

            if (data is Group<T> || data is List<Group<T>>)
            {
                foreach (Group<T> obj in dataValues.Cast<Group<T>>())
                {
                    index++;
                    Row<object> expandedRow = new GroupModelGenerator<T>(_parent).GenerateCaptionRow(obj, Row?.Indent + 1 ?? 0, 0, 0, 0, Row?.Uid!);

                    if ((data as IEnumerable<object>)?.Count() > LazyLoadPageSize)
                    {
                        if (dataValues.ElementAtOrDefault(LazyLoadBlockSize) == obj)
                        {
                            expandedRow.CssClass = "e-lazyload-middle-down";
                        }
                        if (dataValues.ElementAtOrDefault(LazyLoadPageSize - 1) == obj)
                        {
                            expandedRow.CssClass = "e-lazyload-last-down";
                        }
                    }

                    _parent.GroupModule?.LazyRows?.Insert((int)index!, expandedRow);
                }
            }
            else
            {
                ExpandRow = new GroupModelGenerator<T>(_parent).GenerateDataRows(dataValues, Row.Indent + 1, Row.ParentId, _parent.Rows.Count, Row.Uid!).AsQueryable();

                if ((data as IEnumerable<object>)?.Count() > LazyLoadPageSize && ExpandRow?.Any() == true)
                {
                    Row<object>? middleRow = ExpandRow.ElementAtOrDefault(LazyLoadBlockSize);
                    if (middleRow != null)
                    {
                        middleRow.CssClass += " e-lazyload-middle-down";
                    }

                    Row<object>? lastRow = ExpandRow.ElementAtOrDefault(LazyLoadPageSize - 1);
                    if (lastRow != null)
                    {
                        lastRow.CssClass += " e-lazyload-last-down";
                    }
                }
            }
            return (ExpandRow, index)!;
        }

        internal List<Row<object>> CacheGeneratedRows(List<Row<object>> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                return new List<Row<object>>();
            }
            int iteration = 0;
            int index = (int)rows[0].Index!;
            var cachedRows = new List<Row<object>>();
            InfiniteGeneratedRows = InfiniteGeneratedRows ?? new Dictionary<int, Row<object>>();

            foreach (var row in rows)
            {
                if (iteration == rows.Count)
                {
                    break;
                }
                if (!InfiniteGeneratedRows.TryGetValue(index, out var alreadyAdded))
                {
                    InfiniteGeneratedRows.Add(index, row);
                    cachedRows.Add(row);
                }
                else if (InfiniteGeneratedRows.TryGetValue(index, out var infiniteRow))
                {
                    cachedRows.Add(infiniteRow);
                }
                
                iteration++;
                index++;
            }
            return cachedRows;
        }
        internal int GetMinimumValueCount()
        {
            int gridPageSize = _parent.PageSettings!.PageSize;
            int maxBlock = _parent.InfiniteScrollSettings!.MaximumBlocks * gridPageSize;
            int totalItemcount = _parent.TotalItemCount;
            int minmumValue = (maxBlock + gridPageSize) > totalItemcount ? totalItemcount - maxBlock : gridPageSize;
            
            return minmumValue;
        }
    }
}

