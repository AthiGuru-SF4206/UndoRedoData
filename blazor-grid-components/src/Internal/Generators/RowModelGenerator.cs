using System.Linq;
using System.Collections.Generic;
using Syncfusion.Blazor.Data;
using System.Dynamic;
using System;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Generate rows based on the data.
    /// </summary>
    /// <typeparam name="T">TValue of the grid.</typeparam>
    internal class RowModelGenerator<T>
    {
        public SfGrid<T> Parent { get; set; }

        public RowModelGenerator(SfGrid<T> parent)
        {
            Parent = parent;
        }

        public virtual List<Row<object>> GeneratorRows(IEnumerable<object> data, int startIndex = 0)
        {
            List<Row<object>> rows = new List<Row<object>>();
            if (data != null)
            {
                int visibleColumnCount = GridUtils.GetColumns(Parent).FindAll(_ => _.Visible).Count;
                foreach (var item in data)
                {
                    rows.Add(GenerateRow(item, startIndex));
                    if (Parent.DetailRowModule != null)
                    {
                        Parent.DetailRowModule.GenerateDetailRows(rows, data, startIndex, visibleColumnCount,item);
                    }
                    startIndex++;
                }
            }
            Parent.MergeModule!.Process(rows);
            return rows;
        }

        public virtual List<Row<object>> GenerateInfiniteRows(IEnumerable<object> data, int? startIndex = 0)
        {
            int? rowIndex = 0;
            bool enableCache = Parent.InfiniteScrollSettings!.EnableCache;
            string? requestType = Parent.InfiniteScrollModule?.RequestType;
            List<Row<object>> rows = new List<Row<object>>();

            if (Parent.InfiniteScrollModule != null && Parent.InfiniteScrollModule.IsDownScroll)
            {
                Row<object>? lastRowInGrid = Parent.Rows?.LastOrDefault(x => Parent.EnableColumnVirtualization || x.IsLastRow);
                rowIndex = lastRowInGrid?.Index + 1 ?? 0;
            }
            else if (Parent.InfiniteScrollModule != null && Parent.InfiniteScrollModule.IsUpScroll)
            {
                rowIndex = Math.Max(0, (int)Parent.Rows.First().Index! - Parent.PageSettings!.PageSize);
            }
            rowIndex ??= 0;
            rowIndex = (requestType == "Delete" || requestType == "Save") ? Parent.Rows?.Count > 0 && !Parent.EditModule!.IsAdd ? Parent.Rows.First().Index : 0 : rowIndex;
            if (data != null && rowIndex >= 0)
            {
                foreach (object item in data)
                {
                    rows.Add(GenerateRow(item, (int)rowIndex));
                    rowIndex++;
                }
            }
            List<Row<object>>? cachedRows = enableCache ? Parent.InfiniteScrollModule?.CacheGeneratedRows(rows) : rows;
            if (Parent.InfiniteScrollModule != null && Parent.InfiniteScrollModule.IsDownScroll)
            {
                Parent.Rows?.AddRange(cachedRows!);
            }
            else if (Parent.InfiniteScrollModule != null && Parent.InfiniteScrollModule.IsUpScroll) 
            {
                Parent.Rows?.InsertRange(0, cachedRows!);
            }
            List<Row<object>> infiniteRows = Parent.Rows?.Count > 0 && requestType == "InfiniteScrolling" ? Parent.Rows : rows;
            if (enableCache && Parent.InfiniteScrollModule != null && !Parent.InfiniteScrollModule.IsInfiniteInitialRender)
            {
                infiniteRows = Parent.InfiniteScrollModule.RefreshInfiniteCacheRows(infiniteRows)!;
            }
            Row<object>? lastRow = infiniteRows?.LastOrDefault();
            foreach (Row<object> row in infiniteRows!)
            {
                row.IsLastRow = row.IsLastRow && row.Index != lastRow?.Index ? false : row.Index == lastRow?.Index;
            }
            Parent.MergeModule!.Process(infiniteRows);

            return infiniteRows;
        }

        public virtual Row<object> GenerateRow(object item, int index, bool isAdd = false)
        {
            if (item is ExpandoObject && isAdd)
            {
                dynamic dynamicObject = new ExpandoObject();
                var outerDynamicObject = dynamicObject as IDictionary<string, object>;

                foreach (var column in Parent.Columns!)
                {
                    if (column.Field != null)
                    {
                        var splits = column.Field.Split(".");
                        if (splits.Length > 1)
                        {
                            dynamic innerComplexReference = new ExpandoObject();
                            var innerPropertyLevels = innerComplexReference as IDictionary<string, object>;

                            for (int i = 0; i < splits.Length; i++)
                            {
                                if (i == 0)
                                {
                                    outerDynamicObject![splits[i]] = innerComplexReference;
                                    continue;
                                }
                                if (i == splits.Length - 1)
                                {
                                    innerPropertyLevels![splits[i]] = null!;
                                    break;
                                }
                                innerComplexReference = new ExpandoObject();
                                innerPropertyLevels![splits[i]] = innerComplexReference;
                                innerPropertyLevels = innerPropertyLevels[splits[i]] as IDictionary<string, object>;
                            }
                        }
                        else
                        {
                            outerDynamicObject![column.Field] = null!;
                        }
                    }
                }
                item = dynamicObject;
            }
            if (Parent.VirtualScrollModule != null)
                Parent.VirtualScrollModule.VirtualRowIndex = Parent.EditSettings!.NewRowPosition == NewRowPosition.Top && isAdd ? 0 : Parent.VirtualScrollModule.VirtualRowIndex;
            var Currentrow = (Parent.EditSettings!.ShowAddNewRow == false) ? Parent.Rows?.FirstOrDefault(x => x.Data == item) : null;
            var row = new Row<object>()
            {
                Uid = Parent.GetUid("grid-row"),
                Index = Parent.EnableVirtualization ? ((Parent.FrozenRows != 0 && index < Parent.FrozenRows ? index : Parent.VirtualScrollModule!.VirtualRowIndex)) : index,
                IsEdit = Currentrow?.IsEdit ?? false,
                IsDirty = Currentrow?.IsDirty ?? false,
                Data = item,
                EditedData = (Parent.IsEdit || (Currentrow?.IsDirty == true)) ? Currentrow?.EditedData! : null!,
                IsDataRow = true,
                IsTemplate = ((IGrid)Parent).GridTemplates?.RowTemplate != null,
                IsAltRow = Parent.EnableAltRow ? (Parent.EnableVirtualization ? Parent.VirtualScrollModule!.VirtualRowIndex % 2 == 0 : index % 2 == 0) : false,
                ForeignKeyData = new Dictionary<string, IEnumerable<object>>()
            };
            Parent.ForeignKeyModule!.RefreshForeignKeyRow(row, item);
            row.IsSelected = EnsureSelectionState(row);
            List<Cell<object>>? cells = Parent.Rows?.FirstOrDefault(x => x.Data == item)?.Cells;
            row.Cells = GenerateCells(row, cells!);
            if (Parent.EnableVirtualization && Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.VirtualRowIndex++;
            }

            return row;
        }

        public virtual List<Cell<object>> GenerateCells(Row<object> row, List<Cell<object>>cell = null!)
        {
            List<Cell<object>> cellsval = cell;
            int? IndentCellCount = cellsval?.Count(x => x.Index == null);
            List<GridColumn> columns = Parent.FreezeModule!.GetFrozenCount() > 0 || Parent.IsFixedColumnPresent() ? Parent.RearrangeColumns(GridUtils.GetColumns(Parent)) : GridUtils.GetColumns(Parent);
            List<Cell<object>> cells = new List<Cell<object>>();
            using var col = new GridColumn();
            if (((IGrid)Parent).GridTemplates?.DetailTemplate != null)
            {
                cells.Add(GenerateCell(col, null!, CellType.Detail,cell: cellsval!, indentVal: IndentCellCount));
            }

            if (Parent.AllowRowDragAndDrop)
            {
                cells.Add(GenerateCell(col, null!, CellType.RowDrag, cell:cellsval!, indentVal: IndentCellCount));
            }

            if ((!Parent.FrozenName.Equals(FreezeTable.None)) && Parent.FrozenColumnModel?.Count != 0)
            {
                for (var i = 0; i < Parent.FrozenColumnModel?.Count; i++)
                {
                    if (Parent.FrozenColumnModel[i].Commands == null)
                    {
                        cells.Add(GenerateCell(Parent.FrozenColumnModel[i], row?.Uid!, CellType.Data, null, i, row?.ForeignKeyData!, row!, cell: cellsval!, indentVal: IndentCellCount));

                    }
                    else
                    {
                        cells.Add(GenerateCell(Parent.FrozenColumnModel[i], row?.Uid!, CellType.CommandColumn, null, i, null!, row!, cell: cellsval!, indentVal: IndentCellCount));
                    }
                }
            }
            else
            {
                for (var i = 0; i < columns?.Count; i++)
                {
                    if (columns[i].Commands == null)
                    {
                        cells.Add(GenerateCell(columns[i], row?.Uid!, CellType.Data, null, i, row?.ForeignKeyData!, row!, cell: cellsval!, indentVal: IndentCellCount));
                    }
                    else
                    {
                        cells.Add(GenerateCell(columns[i], row?.Uid!, CellType.CommandColumn, null, i, null!,row!, cell: cellsval!, indentVal: IndentCellCount));
                    }
                }

                if (columns?.Count == 0 && cells.Count == 0 && Parent.PivotColumns?.Count > 0)
                {
                    for (var i = 0; i < Parent.PivotColumns.Count; i++)
                    {
                        cells.Add(GenerateCell(Parent.PivotColumns[i], row?.Uid!, CellType.CommandColumn, null, i));
                    }
                }
            }

            return cells;
        }

        protected virtual Cell<object> GenerateCell(GridColumn column, string rowId, CellType cellType, int? colSpan = null, int? oIndex = null, IDictionary<string, IEnumerable<object>> ForeignKeyData = null!, Row<object> row = null!, List<Cell<object>> cell = null!, int? indentVal = 0)
        {
            bool isValidIndex = cell != null && oIndex.HasValue && ((oIndex + indentVal) < cell.Count);
            bool isForeignKey = column?.IsForeignColumn() ?? false;
            return new Cell<object>()
            {
                Visible = column?.Visible ?? true,
                IsDataCell = !string.IsNullOrEmpty(column?.Field) || column?.Template != null,
                IsEdit = isValidIndex ? cell![oIndex!.Value + indentVal!.Value].IsEdit : false,
                IsDirty = isValidIndex ? cell![oIndex!.Value + indentVal!.Value].IsDirty : false,
                IsTemplate = column?.Template != null,
                RowID = rowId,
                Column = column!,
                CellType = cellType,
                Index = oIndex,
                ColSpan = colSpan,
                IsFrozen = column?.IsFrozen == true || Parent.FrozenColumns > oIndex,
                Freeze = (column!.Freeze),
                IsForeignKey = isForeignKey,
                IsSelected = EnsureCellIsSelected(row, oIndex),
                ForeignKeyData = isForeignKey ? (ForeignKeyData?.TryGetValue(column.Uid, out IEnumerable<object>? value) == true ? value : null!) : null!
            };
        }

        public bool EnsureSelectionState(Row<object> row)
        {
            GridSelectionSettings? _settings = Parent.SelectionSettings;
            string? pKey = Parent.SelectionModule?.PrimaryKey;
            var _helper = Parent.PropHelper;
            if (_settings != null && _settings.PersistSelection)
            {
                object key = _helper?.GetObject(pKey!, row?.Data!)!;
                if (key != null && Parent.SelectionModule != null && Parent.SelectionModule.PersistedData?.ContainsKey(key) == true)
                {
                    return true;
                }
            }
            if(row != null && Parent.EnableVirtualization && Parent.VirtualScrollModule!.CurrentGroupedData != null && Parent.VirtualScrollModule!.CurrentGroupedData.Count > 0)
            {
                return row.IsSelected;
            }
            if (Parent.EnableVirtualization && Parent.VirtualScrollModule != null) {
                ValueTuple<int?, int?> shiftSelectionRowIndexes = Parent.VirtualScrollModule.ShiftSelectionRowIndexes;
                string virtualRequestType = Parent.VirtualScrollModule.RequestType!;
                int? rowIndex = row?.Index;
                if ((((rowIndex >= shiftSelectionRowIndexes.Item1 && rowIndex <= shiftSelectionRowIndexes.Item2)
                    || (rowIndex != null && (Array.IndexOf(Parent.VirtualScrollModule.SelectRowsMethodIndexes, (int)rowIndex) != -1 || Parent.SelectedRowIndex == (int)rowIndex)))
                    && (virtualRequestType != null && virtualRequestType.Equals("virtualscroll", System.StringComparison.Ordinal)))
                    || (Parent.CheckBoxState.Equals(CheckState.Check))) {
                    return true;
                }
            }

            return false;
        }

        private bool EnsureCellIsSelected(Row<object> row, int? cellIndex)
        {
            if (Parent.EnableVirtualization && row?.Index >= Parent.VirtualScrollModule?.ShiftSelectionRowIndexes.Item1 && row?.Index <= Parent.VirtualScrollModule.ShiftSelectionRowIndexes.Item2
                && Parent.VirtualScrollModule.RequestType != null && Parent.VirtualScrollModule.RequestType.Equals("virtualscroll", System.StringComparison.Ordinal)
                && (Parent.SelectionModule != null && Parent.SelectionModule.IsCellMode()))
            {
                if (Parent.SelectionModule.IsCellFlow())
                {
                    return true;
                }
                else if (Parent.SelectionModule.IsCellBox() && cellIndex >= Parent.VirtualScrollModule.ShiftSelectionCellIndexes.Item1
                    && cellIndex <= Parent.VirtualScrollModule.ShiftSelectionCellIndexes.Item2)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
