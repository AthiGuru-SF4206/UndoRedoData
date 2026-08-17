using System;
using System.Linq;
using System.Collections.Generic;
using Syncfusion.Blazor.Data;
using System.Globalization;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Generate rows based on the aggregates.
    /// </summary>
    /// <typeparam name="T">TValue of the grid.</typeparam>
    internal class SummaryModelGenerator<T>
    {
        public SfGrid<T> Parent { get; set; }

        public SummaryModelGenerator(SfGrid<T> parent)
        {
            Parent = parent;
        }

        public virtual List<Row<object>> GeneratorRows(IEnumerable<object> data, object Aggregate, int startIndex = 0)
        {
            List<Row<object>> rows = new List<Row<object>>();
            foreach (var item in Parent.Aggregates!)
            {
                rows.Add(GenerateRow(item, startIndex, Aggregate, Parent.Aggregates?.ElementAtOrDefault(startIndex)!));
                startIndex++;
            }
            return rows;
        }

        public virtual Row<object> GenerateRow(object item, int index, object Aggregate, GridAggregate AggregateColumn)
        {
            var row = new Row<object>()
            {
                Data = item,
                IsDataRow = false,
                IsExpand = false,
                RowType = "Summary",
                Uid = Parent.GetUid("grid-row"),
            };
            row.Cells = GenerateCells(row, Aggregate, AggregateColumn);
            return row;
        }

        public virtual List<Cell<object>> GenerateCells(Row<object> row, object Aggregate, GridAggregate AggregateColumn)
        {
            List<GridColumn> columns = Parent.RearrangeColumns(GridUtils.GetColumns(Parent));
            List<Cell<object>> cells = new List<Cell<object>>();
            bool isGroupingEnabled = Parent.AllowGrouping && Parent.GroupSettings != null && Parent.GroupSettings.Columns != null && Parent.GroupSettings.Columns.Length > 0;
            var visibleColumns = columns.Where(col => (col.Visible == true && (isGroupingEnabled && Parent.GroupSettings != null && Parent.GroupSettings.Columns?.Contains(col.Field) == true ? Parent.GroupSettings.ShowGroupedColumn : true))).ToArray();
            int visibleColumnsLen = visibleColumns.Length;
            if (isGroupingEnabled)
            {
                foreach (var col in Parent.GroupSettings!.Columns!)
                {
                    cells.Add(new Cell<object>() { CellType = CellType.Indent });
                }
            }

            if (((IGrid)Parent).GridTemplates?.DetailTemplate != null)
            {
                cells.Add(new Cell<object>() { CellType = CellType.DetailIndent });
            }

            if (Parent.AllowRowDragAndDrop)
            {
                cells.Add(new Cell<object>() { CellType = CellType.RowDrag });
            }

            List<Cell<object>> aggrCells = new List<Cell<object>>();
            if (Parent.Aggregates?.Count > 0)
            {
                var aggregates = Aggregate as IDictionary<string, object>;
                IDictionary<string, GridAggregateColumn> aggCols = new Dictionary<string, GridAggregateColumn>();
                Dictionary<string, GridAggregateColumn> footerCols = new Dictionary<string, GridAggregateColumn>();
                (AggregateColumn?.Columns ?? new List<GridAggregateColumn>()).ForEach(column =>
                {
                    if (column.FooterTemplate != null || (column.GroupCaptionTemplate == null && column.GroupFooterTemplate == null))
                    {
                        var disp = (column.ColumnName != null) ? column.ColumnName : column.Field;
                        footerCols.TryAdd(disp!, column);
                    }
                });
                for (int j = 0; j < visibleColumnsLen; j++)
                {
                    if (footerCols.Count > 0)
                    {
                        if (footerCols.TryGetValue(visibleColumns[j].Field, out GridAggregateColumn? val))
                        {
                            var type = $"{footerCols[visibleColumns[j].Field].Type}".ToLower(System.Globalization.CultureInfo.CurrentCulture);
                            var value = aggregates != null && aggregates.Any() && aggregates.ContainsKey($"{footerCols[visibleColumns[j].Field].Field}" + " - " + type) ? aggregates[$"{footerCols[visibleColumns[j].Field].Field}" + " - " + type] : 0;
                            if (value != null && footerCols[visibleColumns[j].Field].Format != null)
                            {
                                value = DataUtil.GetFormattedValue(value, footerCols[visibleColumns[j].Field].Format!);
                            }

                            aggrCells.Add(GenerateFooterSummary(visibleColumns[j], footerCols[visibleColumns[j].Field], null!, CellType.Summary, value!, (int)visibleColumns[j].Index));
                        }
                        else
                        {
                            var cell = GenerateCell(visibleColumns[j], null!, CellType.Summary, j);
                            cell.Visible = true;
                            aggrCells.Add(cell);
                        }
                    }
                }
            }

            if (aggrCells.Count > 0)
            {
                cells.AddRange(aggrCells);
            }

            return cells;
        }

        protected virtual Cell<object> GenerateCell(GridColumn column, string rowId, CellType cellType, int? oIndex = null, int? colSpan = null, IDictionary<string, IEnumerable<object>> ForeignKeyData = null!)
        {
            return new Cell<object>()
            {
                Visible = column?.Visible ?? true,
                IsDataCell = !string.IsNullOrEmpty(column?.Field) || column?.Template != null,
                IsTemplate = column?.Template != null,
                RowID = rowId,
                Column = column!,
                CellType = cellType,
                Index = oIndex,
                ColSpan = colSpan,
                Freeze = column!.Freeze,
                IsFrozen = column?.IsFrozen == true || Parent.FrozenColumns > oIndex,
                IsForeignKey = column?.IsForeignColumn() == true,
                ForeignKeyData = column?.IsForeignColumn() == true ? (ForeignKeyData?.TryGetValue(column.Field!, out IEnumerable<object>? val) == true ? val : null!) : null!
            };
        }

        protected Cell<object> GenerateFooterSummary(GridColumn gridColumn, GridAggregateColumn aggregateColumn, string rowId, CellType cellType, object aggregateValue, int? oIndex = null, int? colSpan = null)
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
                Freeze = gridColumn.Freeze,
                IsFrozen = gridColumn?.IsFrozen == true || Parent.FrozenColumns > oIndex,
                AggregateValue = aggregateValue,
                Column = gridColumn!
            };
        }
}
}
