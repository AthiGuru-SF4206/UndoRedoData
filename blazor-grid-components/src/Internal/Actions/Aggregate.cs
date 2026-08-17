using Syncfusion.Blazor.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    internal partial class ReactiveAggregate<T>
    {
        #region Aggregate Configuration

        internal void UpdateEmptyList(object data)
        {
            var aggregates = BuildAggregateList();
            var emptyCollection = new List<object> { data };
            var updatedAggregate = DataUtil.PerformAggregation(emptyCollection, aggregates);
            Parent.Aggregate = updatedAggregate;
            Parent.EventAggregator.Trigger("RenderAggregate", data);
        }

        #endregion

        #region Footer Aggregate Refresh

        internal async Task RefreshFooterAggregate()
        {
            if (Parent.Aggregates?.Count <= 0)
                return;

            var dataRows = await ResolveAggregateDataSource().ConfigureAwait(true);
            dataRows = await ApplyFilteringToAggregateData(dataRows).ConfigureAwait(true);
            var datalist = await MergeAggregateDataWithBatchChangesAsync(dataRows).ConfigureAwait(true);

            UpdateAggregateAndTriggerRefresh(datalist);
        }

        #endregion

        #region Data Source Resolution

        private async Task<IEnumerable<object>> ResolveAggregateDataSource()
        {
            bool isRemoteAggregate = Parent.DataManager != null? (Parent.DataManager!.DataAdaptor!.IsRemote() || Parent.DataManager.Adaptor == Adaptors.CustomAdaptor): false;
            IEnumerable<object> dataRows = isRemoteAggregate && Parent.CurrentViewData != null
                ? Parent.CurrentViewData
                : Parent.Query != null && !Parent.IsRenderedFromTreeGrid
                    ? await ResolveQueryDataSource().ConfigureAwait(true)
                    : Parent?.DataSource as IEnumerable<object> ?? Enumerable.Empty<object>();

            if (Parent?.GroupSettings?.Columns != null && isRemoteAggregate)
            {
                dataRows = ResolveGroupedRemoteData();
            }

            return dataRows;
        }

        private async Task<IEnumerable<object>> ResolveQueryDataSource()
        {
            if (Parent.DataManager != null && Parent.Query != null)
            {
                var queryResult = await Parent.DataManager!.ExecuteQuery<T>(Parent.Query).ConfigureAwait(true);
                return (IEnumerable<object>)queryResult!;
            }
            return Array.Empty<object>();
        }

        private IEnumerable<object> ResolveGroupedRemoteData()
        {
            var currentViewRecords = new List<T>();
            var dataRowObjects = Parent.Rows.Where(e => e.IsDataRow).ToList();
            foreach (var row in dataRowObjects)
            {
                currentViewRecords.Add((T)row.Data!);
            }
            return currentViewRecords.Cast<object>().AsEnumerable();
        }

        #endregion

        #region Filtering Operations

        private async Task<IEnumerable<object>> ApplyFilteringToAggregateData(IEnumerable<object> dataRows)
        {
            if (Parent == null || !Parent.AllowFiltering || Parent.FilterSettings?.Columns?.Count <= 0)
                return dataRows;

            bool isRemoteAggregate = Parent.DataManager != null ? (Parent.DataManager!.DataAdaptor!.IsRemote() || Parent.DataManager!.Adaptor == Adaptors.CustomAdaptor): false;

            if (Parent.GroupSettings?.Columns != null && !isRemoteAggregate)
            {
                return await ApplyFilteredQueryForGrouping().ConfigureAwait(true);
            }
            else if (!isRemoteAggregate)
            {
                return (IEnumerable<object>)await Parent.GetFilteredRecordsAsync().ConfigureAwait(true);
            }

            return dataRows;
        }

        private async Task<IEnumerable<object>> ApplyFilteredQueryForGrouping()
        {
            var query = new Query();
            this.Parent.DataModule?.FilterQuery(query, Parent.FilterSettings?.Columns);
            if (Parent.DataManager != null)
            {
                object result = await Parent.DataManager!.ExecuteQuery<T>(query).ConfigureAwait(true);
                return (IEnumerable<object>)(result is DataResult ? (IEnumerable<object>)((DataResult)result).Result! : result)!;
            }
            return Array.Empty<object>();
        }

        #endregion

        #region Batch Change Merging

        internal async Task<List<object>> MergeAggregateDataWithBatchChangesAsync(IEnumerable<object> dataRows)
        {
            var batchChanges = Parent.EditModule!.GetBatchChanges();
            var dataList = dataRows.ToList();
            var keyField = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            var key = keyField?.Count > 0 ? keyField[0]?.ToString() : null;

            if (!string.IsNullOrEmpty(key))
            {
                MergeBatchChangedRecords(batchChanges, dataList, key);
                MergeBatchDeletedRecords(batchChanges, dataList, key);
            }

            MergeBatchAddedRecords(batchChanges, dataList);

            return dataList;
        }

        private void MergeBatchChangedRecords(BatchChanges<T> batchChanges, List<object> dataList, string? key)
        {
            if (batchChanges?.ChangedRecords?.Count <= 0)
                return;
            if (batchChanges != null)
            {
                foreach (var rec in batchChanges.ChangedRecords)
                {
                    var index = dataList.FindIndex(e => Parent.PropHelper?.GetValue(e, key) != null
                        && Parent.PropHelper.GetValue(e, key).ToString() == Parent.PropHelper.GetValue(rec, key)?.ToString());

                    if (index >= 0 && index < dataList.Count)
                    {
                        dataList[index] = rec!;
                    }
                }
            }
        }

        private static void MergeBatchAddedRecords(BatchChanges<T> batchChanges, List<object> dataList)
        {
            if (batchChanges?.AddedRecords.Count <= 0)
                return;
            if (batchChanges != null)
            {
                foreach (var rec in batchChanges.AddedRecords)
                {
                    dataList.Add(rec!);
                }
            }
        }

        private void MergeBatchDeletedRecords(BatchChanges<T> batchChanges, List<object> dataList, string? key)
        {
            if (batchChanges?.DeletedRecords?.Count <= 0)
                return;
            if (batchChanges != null)
            {
                foreach (var rec in batchChanges.DeletedRecords)
                {
                    var temp = dataList.FirstOrDefault(e => Parent.PropHelper?.GetValue(e, key) != null
                        && Parent.PropHelper.GetValue(e, key).Equals(Parent.PropHelper.GetValue(rec, key)));
                    if (temp != null)
                    {
                        dataList.Remove(temp);
                    }
                }
            }
        }

        #endregion

            #region Batch Operations

        internal void HandleBatchCancel()
        {
            foreach (var item in OriginalCells)
            {
                var actualRowObject = Parent.Rows?.FirstOrDefault(_ => _.Uid?.Equals(item.Key, StringComparison.Ordinal) == true);
                if (actualRowObject != null)
                {
                    actualRowObject.Cells = item.Value;
                }
            }
        }

        #endregion

        #region Group Aggregates

        internal async Task UpdateGroupCaptionFooterAggregates()
        {
            if (Parent.Aggregates?.Count <= 0 || Parent.GroupSettings?.Columns == null)
                return;

            var currentViewRecords = ExtractCurrentViewRecords();
            var updatedData = await MergeAggregateDataWithBatchChangesAsync((IEnumerable<object>)currentViewRecords).ConfigureAwait(true);
            IEnumerable aggregateData = updatedData.AsEnumerable();

            var groupAggregate = BuildAggregateList();
            var groupByFormatter = BuildGroupFormatters();

            foreach (var group in Parent.GroupSettings.Columns)
            {
                aggregateData = DataUtil.Group<T>(aggregateData, group, groupAggregate, 0, groupByFormatter);
            }

            UpdateAggregateRowsWithGroupData(aggregateData);
        }

        private List<T> ExtractCurrentViewRecords()
        {
            var currentViewRecords = new List<T>();
            var dataRowObjects = Parent.Rows?.Where(e => e.IsDataRow).ToList();
            foreach (var row in dataRowObjects!)
            {
                currentViewRecords.Add((T)row.Data!);
            }
            return currentViewRecords;
        }

        private Dictionary<string, string> BuildGroupFormatters()
        {
            var groupByFormatter = new Dictionary<string, string>();
            if (Parent.GroupSettings != null && Parent.GroupSettings.Columns != null)
            {
                foreach (var group in Parent.GroupSettings!.Columns)
                {
                    GridColumn? groupColumn = GridUtils.GetColumnByField(group, Parent.Columns!);
                    if (groupColumn != null)
                    {
                        if (groupByFormatter.ContainsKey(group))
                        {
                            groupByFormatter[group] = groupColumn.Format!;
                        }
                        else
                        {
                            groupByFormatter.Add(group, groupColumn.Format!);
                        }
                    }
                }
            }
                return groupByFormatter;
        }

        private void UpdateAggregateRowsWithGroupData(IEnumerable aggregateData)
        {
            var agggregateRowObjects = new GroupModelGenerator<T>(Parent).GenerateRows(aggregateData);
            var updatedSummaryRows = agggregateRowObjects.Where(e => !e.IsDataRow).ToList();
            var originalSummaryRows = Parent.Rows?.Where(e => !e.IsDataRow).ToList();

            for (var j = 0; j < originalSummaryRows?.Count; j++)
            {
                var updateRowObjects = updatedSummaryRows.Where(e => string.Equals(e.GroupKey?.ToString(), originalSummaryRows[j].GroupKey?.ToString(), StringComparison.Ordinal)).ToList();
                if (updateRowObjects.Count == 0)
                {
                    ResetGroupAggregateValues(originalSummaryRows[j]);
                    continue;
                }

                UpdateGroupRowCells(originalSummaryRows, j, updateRowObjects);
            }
        }

        private void ResetGroupAggregateValues(Row<object> row)
        {
            foreach (var cell in row.Cells)
            {
                if (cell.AggregateValue != null)
                {
                    cell.AggregateValue = 0;
                }
            }
            Parent.EventAggregator.Trigger("RowStateChanged", row);
        }

        private void UpdateGroupRowCells(List<Row<object>>? originalSummaryRows, int index, List<Row<object>> updateRowObjects)
        {
            var originalRowObjects = originalSummaryRows!.Where(e => string.Equals(e.GroupKey?.ToString(), originalSummaryRows?[index].GroupKey?.ToString(), StringComparison.Ordinal)).ToList();

            for (var a = 0; a < originalRowObjects.Count; a++)
            {
                if (!OriginalCells.ContainsKey(originalRowObjects[a].Uid!))
                {
                    OriginalCells.Add(originalRowObjects[a].Uid!, originalRowObjects[a].Cells);
                }
                originalRowObjects[a].Cells = updateRowObjects[a].Cells;
                Parent.EventAggregator.Trigger("RowStateChanged", originalRowObjects[a]);
            }
        }

        #endregion

        #region Helper Methods

        private List<Aggregate> BuildAggregateList()
        {
            var aggregates = new List<Aggregate>();

            foreach (GridAggregate item in Parent.Aggregates!)
            {
                foreach (GridAggregateColumn aggregateColumn in item.Columns!)
                {
                    aggregates.Add(new Aggregate { Field = aggregateColumn.Field, Type = aggregateColumn.Type!.ToString() });
                }
            }

            return aggregates;
        }

        private void UpdateAggregateAndTriggerRefresh(List<object> datalist)
        {
            var aggregates = BuildAggregateList();
            var updatedAggregate = DataUtil.PerformAggregation(datalist, aggregates);

            if (Parent.IsRenderedFromTreeGrid)
            {
                var rowData = datalist.FirstOrDefault();
                var type = rowData?.GetType();
                var isDynamic = type?.GenericTypeArguments.FirstOrDefault()?.BaseType?.FullName?.Contains("DynamicObject", StringComparison.Ordinal) == true;
                Parent.Aggregate = isDynamic ? Parent.Aggregate : updatedAggregate;
            }
            else
            {
                Parent.Aggregate = updatedAggregate;
            }

            if (Parent?.Aggregates?.Where(e => e.Columns?.Any(_ => _.FooterTemplate != null) == true).Any() == true)
                Parent.EventAggregator.Trigger("RefreshFooterContent", datalist);
        }

        internal bool IsGroupCaptionTemplate()
        {
            if (Parent.Aggregates?.Count > 0)
            {
                var visibleColumns = GridUtils.GetColumns(Parent)
                                            .Where(column => column.Visible)
                                            .Select(col => col.Field)
                                            .ToHashSet();

                return Parent.Aggregates.Any(aggregate =>
                        aggregate.Columns!.Any(column =>
                        visibleColumns.Contains(column.Field!) &&
                        Parent.GroupSettings != null && Parent.GroupSettings.EnableLazyLoading &&
                        column.GroupCaptionTemplate != null));
            }
            return false;
        }

        internal void AppendAggregateFooterRows(int aggregateIndex, Row<object> Row, List<Row<object>> lazyRows)
        {
            if (Parent.Aggregates?.Count == 0 || Parent.AllowPaging || Parent.EnableVirtualization)
                return;

            var visibleColumns = GridUtils.GetColumns(Parent).Where(column => column.Visible).Select(col => col.Field).ToArray();
            
            Parent.Aggregates?.ForEach(aggregate =>
            {
                var isVisibleFooter = false;
                
                aggregate.Columns?.ForEach(column =>
                {
                    if (visibleColumns.Contains(column.Field) && Parent.GroupSettings != null && Parent.GroupSettings.EnableLazyLoading && ((column.GroupCaptionTemplate == null && column.FooterTemplate == null) || column.GroupFooterTemplate != null))
                    {
                        isVisibleFooter = true;
                    }
                });
                
                if (isVisibleFooter)
                {
                    var ExpandRow = new GroupModelGenerator<T>(Parent).GenerateFooterRow((Row.Data as Group<T>)!, Row.Indent + 1, aggregate, 0, 0, 0, Row.Uid!);
                    lazyRows.Insert(aggregateIndex, (Row<object>)ExpandRow);
                    Parent.TotalItemCount = Parent.TotalItemCount + 1;
                }
            });
            
            if (Parent.EnableInfiniteScrolling)
            {
                Row<object>? firstChildRow = lazyRows.FirstOrDefault(row => row.CssClass?.Contains("e-lazyload-last-down", StringComparison.Ordinal) == true);
                if (firstChildRow != null)
                {
                    firstChildRow.CssClass = "e-firstchildrow";
                }

                Row<object>? lastChildRow = lazyRows.LastOrDefault(x => x.ParentUid == Row.Uid);
                if (lastChildRow != null)
                {
                    lastChildRow.CssClass = "e-lazyload-last-down";
                }
            }
        }

        internal List<object> AddGroupFooter(List<object> uiData, Group<T> group)
        {
            var visibleColumns = GridUtils.GetColumns(Parent)
                                          .Where(column => column.Visible)
                                          .Select(col => col.Field)
                                          .ToArray();

            int indent = Parent.GroupSettings!.Columns!.IndexOf(group.Field);
            var currentRow = new GroupModelGenerator<T>(Parent).GenerateCaptionRow(group, indent);

            Parent.Aggregates?.ForEach(aggregate =>
            {
                bool isVisibleFooter = false;

                aggregate.Columns?.ForEach(column =>
                {
                    if (visibleColumns.Contains(column.Field) &&
                        Parent.GroupSettings.EnableLazyLoading &&
                        ((column.GroupCaptionTemplate == null && column.FooterTemplate == null) || column.GroupFooterTemplate != null))
                    {
                        isVisibleFooter = true;
                    }
                });

                if (isVisibleFooter)
                {
                    var footerRow = new GroupModelGenerator<T>(Parent).GenerateFooterRow(
                        group, currentRow.Indent + 1, aggregate, parentUid: null!);
                    uiData.Add(footerRow);
                }
            });

            return uiData;
        }

        internal void UpdateAggregateFromDataResult(DataResult? dataResult, bool isEmptyGrid)
        {
            Parent.Aggregate = isEmptyGrid ? null : (object?)dataResult?.Aggregates;
        }

        internal void UpdateAggregateFromEventArgs(DataReadyArgs<T> eventArgs)
        {
            if (eventArgs.Aggregates != null)
            {
                Parent.Aggregate = eventArgs.Aggregates;
            }
        }

        internal void RefreshAggregateAfterBatchCancel()
        {
            Parent.Aggregate = ((DataResult)Parent.Data!)?.Aggregates;
            Parent.EventAggregator.Trigger("RenderAggregate", null!);
        }

        #endregion
    }
}
