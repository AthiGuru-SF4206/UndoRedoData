using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Blazor.Data;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Generates rows based on the grouped data.
    /// </summary>
    /// <typeparam name="T">TValue of the grid.</typeparam>
    internal class GroupModelGenerator<T> : RowModelGenerator<T>
    {
        private List<Row<object>>? Rows { get; set; }

        private int Index { get; set; }
        private bool _isStatePersistence;

        internal record GroupedDataParams(
            object Data,
            string? Uid,
            string ParentUid,
            int? Index,
            int RowIndex,
            int Indent,
            bool IsExpand,
            bool IsVisible,
            bool IsSelected,
            bool IsFooterRow
        );

        public GroupModelGenerator(SfGrid<T> parent)
            : base(parent)
        {
            Parent = parent;
            _isStatePersistence = Parent.GroupSettings!.PersistGroupState && Parent.GroupStates.Count != 0 && !Parent.GroupSettings.EnableLazyLoading;
        }
        public List<Row<object>> GroupGeneratorRows(IEnumerable<GroupedDataParams>? groupData)
        {
            var rows = new List<Row<object>>();
            var currentGroupedDataCaptionRowMap = Parent.VirtualScrollModule?.CurrentGroupedDataCaptionRowMap;
            Parent.VirtualScrollModule!.UpdateColumnVisibility();
            if (groupData != null)
            {
                foreach (var item in groupData)
                {
                    if (Parent.GroupSettings!.Columns != null && Parent.GroupSettings.Columns.Length > 0)
                    {
                        if (item.Data is Group<T> groupedData)
                        {
                            rows.Add(new GroupModelGenerator<T>(Parent).GenerateCaptionRow(
                                groupedData,
                                item.Indent,
                                rowsIndex: item.RowIndex,
                                parentUid: item.ParentUid,
                                uid: item.Uid!,
                                isExpand: item.IsExpand,
                                isVisible: item.IsVisible
                            ));
                        }
                        else if (item.IsFooterRow && !string.IsNullOrEmpty(item.ParentUid) &&
                                 currentGroupedDataCaptionRowMap != null &&
                                 currentGroupedDataCaptionRowMap.TryGetValue(item.ParentUid, out var captionItem) &&
                                 captionItem.Item is Group<T> parentGroup)
                        {
                            CheckAndGenerateFooterRows(parentGroup, rows, item.Indent, item.RowIndex, item.ParentUid, captionItem.IsExpand);
                        }
                        else
                        {
                            rows.Add(new GroupModelGenerator<T>(Parent).GenerateRow(
                                item.Data,
                                index: (int)item.Index!,
                                indent: item.Indent,
                                rowsIndex: item.RowIndex,
                                parentUid: item.ParentUid,
                                isVisible: item.IsVisible,
                                isSelected: item.IsSelected,
                                uid: item.Uid!
                            ));
                        }
                    }
                }
            }

            return rows;
        }

        public void CheckAndGenerateFooterRows(Group<T> parentGroup, List<Row<object>> rows, int indent, int rowIndex, string parentUid, bool visible = false)
        {
            var visibleColumns = GridUtils.GetColumns(Parent).Where(col => col.Visible).Select(col => col.Field).ToArray();
            if(visibleColumns == null || visibleColumns.Length == 0)
                return;
            Parent.Aggregates?.ForEach(aggregate =>
            {
                var isVisibleFooter = false;
                aggregate.Columns?.ForEach(column =>
                {
                    if (visibleColumns.Contains(column.Field) && ((column.GroupCaptionTemplate == null && column.FooterTemplate == null) || column.GroupFooterTemplate != null))
                    {
                        isVisibleFooter = true;
                    }
                });
                if (isVisibleFooter)
                {
                    var footerRow = new GroupModelGenerator<T>(Parent).GenerateFooterRow(
                        parentGroup,
                        indent,
                        aggregate,
                        parentId: 0,
                        childId: 0,
                        rowsIndex: rowIndex,
                        parentUid: parentUid
                    );
                    footerRow.Visible = visible;
                    rows.Add(footerRow);
                }
            });
        }

        public List<Row<object>> GenerateInfiniteGroupedRows(IEnumerable data)
        {
            List<GridColumn> gridColumns = GridUtils.GetColumns(Parent);
            for (int j = 0; j < gridColumns?.Count; j++)
            {
                if (Parent.GroupSettings!.Columns?.Contains(gridColumns[j].Field) == true)
                {
#pragma warning disable BL0005
                    gridColumns[j].Visible = Parent.GroupSettings.ShowGroupedColumn ? (gridColumns[j].IsHiddenByGrouping ? true : gridColumns[j].Visible) : Parent.GroupSettings.ShowGroupedColumn;
                    gridColumns[j].SetVisibility(gridColumns[j].Visible);
#pragma warning restore BL0005
                }
            }

            Rows = new List<Row<object>>();
            List<Row<object>> infiniteGroupedRows = new List<Row<object>>();
            infiniteGroupedRows = Parent.Rows;
            Index = 0;
            int lastRowIndex = 0;
            int firstRowIndex = 0;
            if (Parent.InfiniteScrollModule != null &&(Parent.InfiniteScrollModule.IsDownScroll || Parent.InfiniteScrollModule.IsUpScroll))
            {
                lastRowIndex = (int)(Parent.Rows?.LastOrDefault(x => x.Index != null)?.Index ?? 0) + 1;
                firstRowIndex = (int)(Parent.Rows?.FirstOrDefault(x => x.Index != null)?.Index ?? 0) - Parent.PageSettings!.PageSize;
            }
            
            Index = Parent.InfiniteScrollModule!.IsDownScroll ? lastRowIndex : firstRowIndex;
            int i = 0;
            if (data != null && (data is Group<T> || data is List<object>))
            {
                foreach (Group<T> obj in data)
                {
                    GetGroupedRecords(0, obj, obj.Level, i, 0, Rows.Count);
                    i += 1;
                }
            }
            if (Parent.InfiniteScrollModule.IsDownScroll)
            {
                infiniteGroupedRows.AddRange(Rows);
            }
            else if (Parent.InfiniteScrollModule.IsUpScroll)
            {
                infiniteGroupedRows.InsertRange(0, Rows);
            }
            else
            {
                infiniteGroupedRows = Rows;
            }
            if (Parent.InfiniteScrollSettings!.EnableCache)
            {
                infiniteGroupedRows = Parent.InfiniteScrollModule.RefreshInfiniteCacheRows(infiniteGroupedRows)!;
            }
            if (!Parent.IsExpanded && !Parent.GroupSettings!.EnableLazyLoading && !Parent.EnableVirtualization)
            {
                EnsureRowVisibility();
            }

            if (Parent.GroupSettings!.EnableLazyLoading && Parent.GroupModule != null)
            {
                Parent.GroupModule.LazyRows = Rows;
                if (Parent.AllowPaging)
                {
                    Parent.TotalItemCount = Rows.Count;
                    return (List<Row<object>>)Rows.ToList().Skip((Parent.PageSettings!.CurrentPage - 1) * Parent.PageSettings.PageSize).Take(Parent.PageSettings.PageSize).ToList();
                }
                else if (Parent.EnableInfiniteScrolling)
                {
                    Parent.TotalItemCount = Rows.Count;
                    var loadSize = Parent.PageSettings!.PageSize * Parent.InfiniteScrollSettings.InitialBlocks;

                    Parent.InfiniteScrollModule.CaptionRowsList = Rows;
                    List<Row<object>> infiniteRows = new List<Row<object>>();
                    if (Parent.GroupModule.IsLazyExpandAll || Parent.InfiniteScrollModule.RequestType == "GroupExpandCollapseAll")
                    {
                        infiniteRows = Rows;
                    }
                    else
                    {
                        if (Parent.InfiniteScrollModule.RequestType == "Save" || Parent.InfiniteScrollModule.RequestType == "Delete")
                        {
                            infiniteRows = Rows.Skip(Parent.PageSettings.CurrentPage - 1).Take(Parent.Rows?.Count ?? 0).ToList();
                        }
                        else
                        {
                            infiniteRows = Rows.Skip(Parent.PageSettings.CurrentPage - 1).Take(loadSize).ToList();
                        }
                    }

                    Parent.GroupModule.LazyRows = infiniteRows;
                    Parent.MergeModule!.GroupingProcess(infiniteRows);

                    return infiniteRows;
                }
                else
                {
                    Parent.MergeModule!.GroupingProcess(Rows);
                    return Rows;
                }
            }
            else
            {
                Parent.MergeModule!.GroupingProcess(infiniteGroupedRows);
                return infiniteGroupedRows;
            }
        }

        public List<Row<object>> GenerateRows(IEnumerable data)
        {
            List<GridColumn> gridColumns = GridUtils.GetColumns(Parent);
            for (int j = 0; j < gridColumns?.Count; j++)
            {
                if (Parent.GroupSettings!.Columns?.Contains(gridColumns[j].Field) == true)
                {
#pragma warning disable BL0005
                    gridColumns[j].Visible = Parent.GroupSettings.ShowGroupedColumn ? (gridColumns[j].IsHiddenByGrouping ? true : gridColumns[j].Visible) : Parent.GroupSettings.ShowGroupedColumn;
                    gridColumns[j].SetVisibility(gridColumns[j].Visible);
#pragma warning restore BL0005
                }
            }

            Rows = new List<Row<object>>();
            Index = 0;
            int i = 0;
            if (data != null && (data is Group<T> || (data is List<object> && (data as List<object>)?.FirstOrDefault() is Group<T>)))
            {
                foreach (Group<T> obj in data)
                {
                    GetGroupedRecords(0, obj, obj.Level, i, 0, Rows.Count);
                    i += 1;
                }
            }

            if (!Parent.IsExpanded && !Parent.GroupSettings!.EnableLazyLoading && !Parent.EnableVirtualization)
            {
                EnsureRowVisibility();
            }

            if (Parent.GroupSettings!.EnableLazyLoading && Parent.GroupModule != null)
            {
                Parent.GroupModule.LazyRows = Rows;
                if (Parent.AllowPaging)
                {
                    Parent.TotalItemCount = Rows.Count;
                    var pagedRows = (List<Row<object>>)Rows.ToList().Skip((Parent.PageSettings!.CurrentPage - 1) * Parent.PageSettings.PageSize).Take(Parent.PageSettings.PageSize).ToList();
                    Parent.MergeModule!.GroupingProcess(pagedRows);

                    return pagedRows;
                }
                else
                {
                    Parent.MergeModule!.GroupingProcess(Rows);

                    return Rows;
                }
            }
            else
            {
                Parent.MergeModule!.GroupingProcess(Rows);

                return Rows;
            }
        }

        public void GetGroupedRecords(int index, object data, object raw = null!, int parentId = 0, int childId = 0, int tIndex = 0, string parentUid = null!)
        {
            childId = 0;
            tIndex = 0;
            int level = (int)raw;
            var itemsProperty = data?.GetType().GetProperty("Items");
            if (itemsProperty == null || itemsProperty.GetValue(data) == null)
            {
                var groupGuidProperty = data?.GetType().GetProperty("GroupGuid");
                if (groupGuidProperty == null || groupGuidProperty.GetValue(data) == null)
                {
                    var newRows = GenerateDataRows((data as IEnumerable<object>)!, index, parentId, Rows!.Count, parentUid);
                    Rows.AddRange(newRows);
                }
                else
                {
                    Group<T>? groupedData = data as Group<T>;
                    foreach (Group<T> obj in (data as IEnumerable<Group<T>>)!)
                    {
                        GetGroupedRecords(index, obj, groupedData?.Level!, parentId, index, Rows!.Count, parentUid);
                    }
                }
            }
            else
            {
                Group<T>? groupedData = data as Group<T>;
                Row<object> captionRow = GenerateCaptionRow(groupedData!, index, 0, 0, 0, parentUid);
                Rows!.Add(captionRow);
                if (groupedData?.Items != null && groupedData.Items.GetEnumerator().MoveNext())
                {
                    GetGroupedRecords(index + 1, groupedData?.Items!, groupedData?.Level!, parentId, index + 1, Rows.Count, captionRow.Uid!);
                }
                if (Parent.Aggregates?.Count > 0 && Parent.GroupModule != null && (Parent.GroupModule.IsLazyExpandAll || !Parent.GroupSettings!.EnableLazyLoading))
                {
                    CheckAndGenerateFooterRows((data as Group<T>)!, Rows, level, 0, captionRow.Uid!, captionRow.IsExpand);
                }
            }
        }

        public Row<object> GenerateCaptionRow(Group<T> data, int indent, int parentId = 0, int childId = 0, int rowsIndex = 0, string parentUid = null!, string uid = null!, bool? isExpand = null, bool? isVisible= null)
        {
            var row = new Row<object>()
            {
                RowType = "GroupCaption",
                Data = data,
                IsDataRow = false,
                IsExpand = Parent.GroupModule!.IsLazyExpandAll ? true : (Parent.EnableVirtualization && !Parent.GroupSettings!.EnableLazyLoading ? Parent.IsExpanded : (data.Items as IEnumerable<object>)!.Any()),
                ParentId = parentId,
                ChildId = childId,
                rowsIndex = rowsIndex,
                IsCaptionRow = true,
                GroupKey = data.Key,
                Indent = indent,
                Uid = uid ?? Parent.GetUid("grid-row"),
                ParentUid = parentUid,
                ForeignKeyData = new Dictionary<string, IEnumerable<object>>()
            };
            var field = data.Field;

            if (_isStatePersistence)
            {
                string groupKey = (data.Key?.ToString() ?? string.Empty) + Grouping<T>.GetUniqueGroupKey(Rows!, parentUid);
                row.IsExpand = Parent.GroupStates.GetValueOrDefault(groupKey, row.IsExpand);
            }
            if (isVisible != null)
            {
                row.Visible = (bool)isVisible;
            }

            else if (row.ParentUid != null && parentUid != null && IsValidChildRow(parentUid, row.ParentUid) && Parent.GroupStates.Count == 0)// parentUid is caption row Uid
            {
                row.Visible = row.IsExpand;
            }
            else
            {
                row.Visible = _isStatePersistence ? GetRowVisibility(parentUid!, row.Visible) : row.Visible;
            }
            if(field != null)
            {
                row.Cells = GetCaptionRowCells(field, indent, data);
            }
            if (isExpand != null)
            {
                row.IsExpand = (bool)isExpand;
            }
            return row;
        }

        public List<Cell<object>> GetCaptionRowCells(string field, int indent, object data, bool isCaption = true, GridAggregate aggregate = null!)
        {
            List<GridColumn> gridColumns = GridUtils.GetColumns(Parent);
            GridColumn? column = gridColumns?.Where(col => col.Field == field).FirstOrDefault();
            int groupedLen = Parent.GroupSettings!.Columns!.Length;
            var agg = aggregate;
            List<Cell<object>> cells = new List<Cell<object>>();
            using var col = new GridColumn();
            var indentClass = new string[groupedLen];
            if (!isCaption)
            {
                indentClass = Parent.GroupSettings.Columns?.Select((col, index) =>
                {
                    return index <= indent - 1 ? string.Empty : "e-indentcelltop";
                }).ToArray();
                indent = Parent.GroupSettings.Columns?.Length ?? 0;
            }

            for (int i = 0; i < indent; i++)
            {
                using var gridColumn = new GridColumn();
                if (!string.IsNullOrEmpty(indentClass?[i]))
                {
#pragma warning disable BL0005
                    gridColumn.CustomAttributes = new Dictionary<string, object> { { "class", indentClass[i] } };
#pragma warning restore BL0005
                }
                cells.Add(GenerateCell(gridColumn, null!, CellType.Indent));
            }

            if (!isCaption && IsRowDraggable())
            {
                cells.Add(GenerateCell(col, null!, CellType.Indent));
            }

            if (!isCaption && ((IGrid)Parent).GridTemplates?.DetailTemplate != null)
            {
                var cell = GenerateCell(col, null!, CellType.DetailIndent);
                cells.Add(cell);
            }

            if (isCaption)
            {
                var cell = GenerateCell(col, null!, CellType.Expand);
                cell.Visible = true;
                cells.Add(cell);
            }
            List<Cell<object>> aggrCells = new List<Cell<object>>();
            (indent, aggrCells) = GetCaptionRowCell(field, indent, data, isCaption, aggregate);
            var cols = !Parent.EnableColumnVirtualization ? new List<GridColumn> { column! } : GridUtils.GetColumns(Parent);
            var _this = this;
            cols?.ForEach(col =>
            {
                indent = indent + (IsRowDraggable() ? 1 : 0);
                if (isCaption)
                {
                    var cell = _this.GenerateCell(new GridColumn(), null!, CellType.GroupCaption, indent);
                    cell.Visible = true;
                    cells.Add(cell);
                }
            });

            if (aggrCells?.Count > 0)
            {
                cells.AddRange(aggrCells);
            }

            return cells;
        }

        private ValueTuple<int, List<Cell<object>>> GetCaptionRowCell(string field, int indent, object data, bool isCaption = true, GridAggregate aggregate = null!)
        {
            List<GridColumn> gridColumns = GridUtils.GetColumns(Parent);
            GridColumn? column = gridColumns?.Where(col => col.Field == field).FirstOrDefault();

            List<string> originalGroupedColumns = new List<string>();
            foreach (var gcol in Parent.GroupSettings!.Columns!)
            {
                if (gridColumns?.Where(e => e.Field == gcol).Any() == true)
                {
                    originalGroupedColumns.Add(gcol);
                }
            }
            int groupedLen = originalGroupedColumns.Count;
            var visibleColumns = gridColumns?.Where(col => col.Visible == true).ToArray();
            int visibleColumnsLen = visibleColumns?.Length ?? 0;
            indent = visibleColumnsLen + groupedLen + ((((IGrid)Parent).GridTemplates?.DetailTemplate != null) ? 1 : 0) - indent + (visibleColumnsLen > 0 ? -1 : 0);
            List<Cell<object>> aggrCells = new List<Cell<object>>();
            if (Parent.Aggregates?.Count > 0)
            {
                var aggregates = (data as Group<T>)?.Aggregates as IDictionary<string, object>;
                Dictionary<string, GridAggregateColumn> aggCols = new Dictionary<string, GridAggregateColumn>();
                Dictionary<string, GridAggregateColumn> footerCols = new Dictionary<string, GridAggregateColumn>();
                bool isGroupCaptionTemplateNonNull = Parent.Aggregates.Any(aggregate => aggregate.Columns!.Any(column => column.GroupCaptionTemplate != null));

                if (isCaption && isGroupCaptionTemplateNonNull)
                {
                    int summaryCellsLen = visibleColumnsLen - 1;
                    indent -= summaryCellsLen;
                    Parent.Aggregates?.ForEach(aggregate => aggregate.Columns?.ForEach(column =>
                    {
                        if (column.GroupCaptionTemplate != null || (column.GroupFooterTemplate == null && column.FooterTemplate == null))
                        {
                            aggCols.Add(column.ColumnName! ?? column.Field!, column);
                        }
                    }));
                    for (int j = 1; j <= summaryCellsLen; j++)
                    {
                        if (aggCols.TryGetValue(visibleColumns?[j].Field!, out GridAggregateColumn? val))
                        {
                            var value = aggregates?[$"{aggCols[visibleColumns![j].Field].Field} - {aggCols[visibleColumns[j].Field].Type}"];
                            if (aggCols[visibleColumns![j]!.Field].Format != null && value != null )
                            {
                                value = DataUtil.GetFormattedValue(value, aggCols[visibleColumns[j].Field].Format!);
                            }

                            aggrCells.Add(GenerateCaptionSummary(visibleColumns[j], aggCols[visibleColumns[j].Field], null!, CellType.CaptionSummary, value!));
                        }
                        else
                        {
                            var cell = GenerateCell(visibleColumns![j], null!, CellType.Summary);
                            cell.Visible = true;
                            aggrCells.Add(cell);
                        }
                    }
                }
                else if (!isCaption)
                {
                    aggregate?.Columns?.ForEach(column =>
                    {
                        if (column.GroupFooterTemplate != null || (column.GroupCaptionTemplate == null && column.FooterTemplate == null))
                        {
                            footerCols.Add(column.Field!, column);
                        }
                    });
                    for (int j = 0; j < visibleColumnsLen; j++)
                    {
                        if (footerCols.Count > 0)
                        {
                            if (footerCols.TryGetValue(visibleColumns?[j].Field!, out GridAggregateColumn? val))
                            {
                                var value = aggregates?[$"{footerCols[visibleColumns![j].Field].Field} - {footerCols[visibleColumns[j].Field].Type}"];
                                if (footerCols[visibleColumns![j].Field].Format != null && value != null)
                                {
                                    value = DataUtil.GetFormattedValue(value, footerCols[visibleColumns[j].Field].Format!);
                                }

                                aggrCells.Add(GenerateFooterSummary(visibleColumns[j], footerCols[visibleColumns[j].Field], null!, CellType.CaptionSummary, value!));
                            }
                            else
                            {
                                var cell = GenerateCell(visibleColumns![j], null!, CellType.Summary);
                                cell.Visible = true;
                                aggrCells.Add(cell);
                            }
                        }
                    }
                }
            }
            return (indent, aggrCells);
        }

        public IEnumerable<Row<object>> GenerateDataRows(IEnumerable<object> data, int indent, int childId = 0, int tIndex = 0, string parentUid = null!)
        {
            List<Row<object>> rows = new List<Row<object>>();
            var dataList = data as IList ?? data.ToList() ?? new List<object>();
            int len = dataList.Count;
            for (int i = 0; i < len; i++, tIndex++)
            {
                rows.Add(GenerateRow(dataList[i]!, Index, i == 0 ? null! : "e-firstchildrow", indent, childId, tIndex, parentUid));
                Index++;
            }

            return rows;
        }

        public Row<object> GenerateRow(object data, int index, string cssClass = null!, int indent = 0, int pid = 0, int rowsIndex = 0, string parentUid = null!,bool ? isVisible = null , bool ? isSelected = null,string uid = null!)
        {
            var firstRow = Parent.Rows?.FirstOrDefault(x => x.Data == data);
            var Currentrow = (Parent.EditSettings!.ShowAddNewRow == false) ? firstRow : null;
            var row = new Row<object>()
            {
                Uid = uid == null ? Parent.GetUid("grid-row"): uid,
                Data = data,
                Index = index,
                Indent = indent,
                rowsIndex = rowsIndex,
                IsDataRow = true,
                ParentId = pid,
                CssClass = cssClass,
                IsTemplate = ((IGrid)Parent).GridTemplates?.RowTemplate != null,
                IsAltRow = Parent.EnableAltRow && index % 2 != 0,
                IsSelected = isSelected ?? false,
                ForeignKeyData = new Dictionary<string, IEnumerable<object>>(),
                ParentUid = parentUid,
                IsEdit = Currentrow?.IsEdit ?? false,
                IsDirty = Currentrow?.IsDirty ?? false,
                EditedData = (Parent.IsEdit || (Currentrow?.IsDirty == true)) ? Currentrow?.EditedData! : null!,
            };
            Parent.ForeignKeyModule!.RefreshForeignKeyRow(row, data);
			List<Cell<object>>? cells = firstRow?.Cells;
            if (isVisible != null)
            {
                row.Visible = (bool)isVisible;
            }
            else if(IsValidChildRow(parentUid, row.ParentUid) && Parent.GroupStates.Count == 0 && Parent.GroupModule!= null && !Parent.GroupModule.IsLazyExpandAll) //parentUid is caption row uid
            {
                row.Visible = row.IsExpand;
            }
            else
            {
                row.Visible = _isStatePersistence ? GetRowVisibility(parentUid, row.Visible) : row.Visible;
            }
            row.Cells = GenerateCells(row,cells!);
            row.IsSelected = EnsureSelectionState(row);
            return row;
        }

        protected static Cell<object> GenerateCaptionSummary(GridColumn gridColumn, GridAggregateColumn aggregateColumn, string rowId, CellType cellType, object aggregateValue, int? colSpan = null, int? oIndex = null)
        {
            return new Cell<object>()
            {
                IsDataCell = false,
                IsTemplate = true,
                RowID = rowId,
                AggregateColumn = aggregateColumn,
                CellType = cellType,
                Index = oIndex,
                ColSpan = colSpan,
                AggregateValue = aggregateValue,
                Column = gridColumn,
                Visible = true
            };
        }

        protected static Cell<object> GenerateFooterSummary(GridColumn gridColumn, GridAggregateColumn aggregateColumn, string rowId, CellType cellType, object aggregateValue, int? colSpan = null, int? oIndex = null)
        {
            return new Cell<object>()
            {
                IsDataCell = false,
                IsTemplate = true,
                RowID = rowId,
                AggregateColumn = aggregateColumn,
                CellType = cellType,
                Index = oIndex,
                ColSpan = colSpan,
                AggregateValue = aggregateValue,
                Column = gridColumn,
                Visible = true
            };
        }

        public Row<object> GenerateFooterRow(Group<T> data, int indent, GridAggregate aggregate = null!, int parentId = 0, int childId = 0, int rowsIndex = 0, string parentUid = null!)
        {
            var row = new Row<object>()
            {
                Data = data,
                IsDataRow = false,
                IsExpand = false,
                ParentId = parentId,
                ChildId = childId,
                rowsIndex = rowsIndex,
                IsCaptionRow = false,
                GroupKey = data?.Key!,
                Indent = indent,
                RowType = "Summary",
                Uid = Parent.GetUid("grid-row"),
                ForeignKeyData = new Dictionary<string, IEnumerable<object>>(),
                ParentUid = parentUid
            };
            var field = data?.Field;
            row.Cells = GetCaptionRowCells(field!, indent, data!, false, aggregate);
            if (IsValidChildRow(parentUid, row.ParentUid) && Parent.GroupStates.Count == 0) //parentUid is caption row uid
            {
                row.Visible = row.IsExpand;
            }
            else
            {
                row.Visible = _isStatePersistence ? GetRowVisibility(parentUid, row.Visible) : row.Visible;
            }
            return row;
        }
        public override List<Cell<object>> GenerateCells(Row<object> row, List<Cell<object>> cell = null!)
        {
            List<Cell<object>> CellValue = cell;
            int? IndentCellCount = Parent.Rows?.FirstOrDefault(x => x.IsDataRow == true)?.Cells.Count(x => x.Index == null);
            List<GridColumn> gridParentColumns = GridUtils.GetColumns(Parent);
            List<GridColumn> columns = Parent.IsFixedColumnPresent() ? Parent.RearrangeColumns(gridParentColumns)  : gridParentColumns;
            List<Cell<object>> cells = new List<Cell<object>>();
            using var col = new GridColumn();
            for (var j = 0; j < row.Indent; j++)
            {
                cells.Add(GenerateIndentCell(CellValue));
            }

            if (((IGrid)Parent).GridTemplates?.DetailTemplate != null)
            {
                cells.Add(GenerateCell(col, null!, CellType.Detail,cell: CellValue, indentVal: IndentCellCount));
            }

            if (IsRowDraggable())
            {
                cells.Add(GenerateCell(col, null!, CellType.RowDrag,cell: CellValue, indentVal: IndentCellCount));
            }

            for (var i = 0; i < columns?.Count; i++)
            {
                var cellType = columns[i].Commands != null ? CellType.CommandColumn : CellType.Data;
                cells.Add(GenerateCell(columns[i], row?.Uid!, cellType, null, i, row?.ForeignKeyData!,cell: CellValue, indentVal: IndentCellCount));
            }

            return cells;
        }

        public Cell<object> GenerateIndentCell(List<Cell<object>> cell = null!)
        {
            List<Cell<object>> CellValue = cell;
            int? IndentCellCount = Parent.Rows?.FirstOrDefault(x => x.IsDataRow == true)?.Cells.Count(x => x.Index == null);
            using var col = new GridColumn();
            return GenerateCell(col, null!, CellType.Indent, cell: CellValue, indentVal: IndentCellCount);
        }

        private bool IsRowDraggable() => Parent.AllowRowDragAndDrop;

        public void EnsureRowVisibility()
        {
            for (int i = 0; i < Rows?.Count; i++)
            {
                Row<object> row = Rows[i];
                if (!string.Equals(row?.RowType, "GroupCaption", StringComparison.Ordinal))
                {
                    continue;
                }

                for (int j = i + 1; j < Rows.Count; j++)
                {
                    Row<object> childRow = Rows[j];
                    if (string.Equals(row?.Uid, childRow?.ParentUid, StringComparison.Ordinal))
                    {
                        Rows[j].Visible = (row != null && row.IsExpand);
                    }
                }
            }
        }

        private bool IsValidChildRow(string parentUid, string childRowParentUid)
        {
            if (childRowParentUid == null || parentUid == null)
                return false;
            return string.Equals(parentUid, childRowParentUid, StringComparison.Ordinal)
                   && Parent.EnableVirtualization && !Parent.IsExpanded;
        }

        private bool GetRowVisibility(string parentUid, bool existingVisible)
        {
            if (parentUid == null || Rows == null)
                return existingVisible;

            var parentRow = Rows.FirstOrDefault(row => row.Uid == parentUid);
            return parentRow is null ? existingVisible : parentRow.Visible && parentRow.IsExpand;
        }
    }
}
