using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids
{
    internal class MergeHandler<T>
    {
        private SfGrid<T> Parent { get; }
        private List<MergeCellInfo> ManuallyMergedCells { get;  set; }
        private List<UnmergeCellInfo> ExcludedFromAutomaticSpanning { get; set; }

        /// <summary>
        /// Initializes a new instance of the CellMergeManager class.
        /// </summary>
        /// <param name="parent">The parent SfGrid instance</param>
        internal MergeHandler(SfGrid<T> parent)
        {
            Parent = parent;
            ManuallyMergedCells = new List<MergeCellInfo>();
            ExcludedFromAutomaticSpanning = new List<UnmergeCellInfo>();
        }

        /// <summary>
        /// Merges a single cell with specified row and column span dimensions.
        /// </summary>
        /// <param name="info">The merge cell information containing row index, column index, and span dimensions</param>
        /// <returns>A task representing the asynchronous operation</returns>
        internal async Task MergeCellsAsync(MergeCellInfo info)
        {
            if (info != null)
            {
                ManuallyMergedCells ??= new List<MergeCellInfo>();
                ManuallyMergedCells.Add(info);
                await Parent.Refresh().ConfigureAwait(true);

            }
        }

        /// <summary>
        /// Merges multiple cells with their specified row and column span dimensions.
        /// </summary>
        /// <param name="infos">An enumerable of merge cell information objects</param>
        /// <returns>A task representing the asynchronous operation</returns>
        internal async Task MergeCellsAsync(IEnumerable<MergeCellInfo> infos)
        {
            if (infos != null)
            {
                ManuallyMergedCells ??= new List<MergeCellInfo>();
                ManuallyMergedCells.AddRange(infos);
                await Parent.Refresh().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Unmerges a single cell and marks it as excluded from automatic spanning.
        /// </summary>
        /// <param name="info">The unmerge cell information containing row index and column index</param>
        /// <returns>A task representing the asynchronous operation</returns>
        internal async Task UnmergeCellsAsync(UnmergeCellInfo info)
        {
            if (info is null) return;

            if (ManuallyMergedCells?.RemoveAll(x =>
                    x.RowIndex == info.RowIndex && x.ColumnIndex == info.ColumnIndex) > 0)
            {
                await Parent.Refresh().ConfigureAwait(true);
                return;
            }

            ExcludedFromAutomaticSpanning ??= new();
            var key = (info.RowIndex, info.ColumnIndex);
            if (ExcludedFromAutomaticSpanning.All(x => (x.RowIndex, x.ColumnIndex) != key))
            {
                ExcludedFromAutomaticSpanning.Add(info);
            }
            await Parent.Refresh().ConfigureAwait(true);
        }

        /// <summary>
        /// Unmerges multiple cells and marks them as excluded from automatic spanning.
        /// </summary>
        /// <param name="infos">An enumerable of unmerge cell information objects</param>
        /// <returns>A task representing the asynchronous operation</returns>
        internal async Task UnmergeCellsAsync(IEnumerable<UnmergeCellInfo> infos)
        {
            if (infos != null)
            {
                ManuallyMergedCells ??= new List<MergeCellInfo>();
                ExcludedFromAutomaticSpanning ??= new List<UnmergeCellInfo>();

                foreach (var info in infos)
                {
                    bool isManuallyMerged = ManuallyMergedCells.RemoveAll(x => x.RowIndex == info.RowIndex && x.ColumnIndex == info.ColumnIndex) > 0;
                    if (!isManuallyMerged)
                    {
                        if (!ExcludedFromAutomaticSpanning.Any(x => x.RowIndex == info.RowIndex && x.ColumnIndex == info.ColumnIndex))
                        {
                            ExcludedFromAutomaticSpanning.Add(info);
                        }
                    }
                }
                await Parent.Refresh().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Removes all manual merges and automatic spanning exclusions, resetting the grid to its unmerged state.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        internal async Task UnmergeAllAsync()
        {
            ManuallyMergedCells = null!;
            ExcludedFromAutomaticSpanning = null!;
            Parent.SuppressAutoSpanning = true;
            await Parent.Refresh().ConfigureAwait(true);
            Parent.SuppressAutoSpanning = false;
        }

        /// <summary>
        /// Main entry point for cell merging and spanning operations on regular rows.
        /// Applies manual merges first, then automatic spanning based on grid configuration.
        /// </summary>
        /// <param name="rows">The list of rows to process</param>
        internal void Process(List<Row<object>> rows)
        {
            if (Parent.IsRenderedFromTreeGrid)
            {
                foreach (Row<object> Row in rows)
                {
                    if (Row.IsDataRow)
                    {
                        Parent.EventAggregator.NotifyAsync("VisibilitySet", Row).ConfigureAwait(false);
                    }
                }
            }
            ProcessManualMerges(rows);

            if (!Parent.SuppressAutoSpanning)
            {
                // Column-span now has higher priority. Apply column spanning first, then row spanning.
                if (Parent.AutoSpan == AutoSpanMode.Row || Parent.AutoSpan == AutoSpanMode.HorizontalAndVertical)
                {
                    ProcessAutomaticColumnSpanning(rows);
                }
                if (Parent.AutoSpan == AutoSpanMode.Column || Parent.AutoSpan == AutoSpanMode.HorizontalAndVertical)
                {
                    ProcessAutomaticRowSpanning(rows);
                }
            }
        }

        /// <summary>
        /// Entry point for cell merging and spanning operations on grouped rows.
        /// Applies manual merges first, then automatic spanning with support for summary rows.
        /// </summary>
        /// <param name="rows">The list of grouped rows to process</param>
        internal void GroupingProcess(List<Row<object>> rows)
        {
            if (rows == null || rows.Count == 0)
                return;

            // Apply manual merges first so auto passes respect them
            ProcessManualMerges(rows);
            if (!Parent.SuppressAutoSpanning)
            {
                // Column-first automatic spanning for grouped pipeline
                if (Parent.AutoSpan == AutoSpanMode.Row || Parent.AutoSpan == AutoSpanMode.HorizontalAndVertical)
                {
                    ProcessAutomaticColumnSpanning(rows);
                }

                if (Parent.AutoSpan == AutoSpanMode.Column || Parent.AutoSpan == AutoSpanMode.HorizontalAndVertical)
                {
                    ProcessAutomaticRowSpanning(rows);
                }
            }
        }
        
        /// <summary>
        /// Processes manually merged cells and applies their span properties to the row structure.
        /// </summary>
        /// <param name="rows">The list of rows to apply manual merges to</param>
        private void ProcessManualMerges(List<Row<object>> rows)
        {
            if (ManuallyMergedCells == null || ManuallyMergedCells.Count == 0)
            {
                return;
            }
            foreach (var mergeInfo in ManuallyMergedCells)
            {
                if (mergeInfo.RowIndex < 0 || mergeInfo.RowIndex >= rows.Count)
                {
                    continue;
                }

                var anchorRow = rows[mergeInfo.RowIndex];
                if (anchorRow?.Cells == null || mergeInfo.ColumnIndex < 0 || mergeInfo.ColumnIndex >= anchorRow.Cells.Count)
                {
                    continue;
                }

                var anchorCell = anchorRow.Cells[mergeInfo.ColumnIndex];
                anchorCell.IsSpanned = false;
                bool anchorIsDataRow = anchorRow.IsDataRow;
                string anchorRowType = anchorRow.RowType;
                string anchorParentUid = anchorRow.ParentUid!;
                int requestedRowSpan = Math.Max(mergeInfo.RowSpan, 1);
                int actualRowSpan = 1;
                for (int r = 1; r < requestedRowSpan; r++)
                {
                    int rowIndex = mergeInfo.RowIndex + r;
                    if (rowIndex >= rows.Count) break;
                    var row = rows[rowIndex];
                    if (row?.Cells == null) break;

                    if (anchorIsDataRow)
                    {
                        if (!row.IsDataRow) break;
                    }
                    else
                    {
                        if (!string.Equals(row.RowType, anchorRowType, StringComparison.Ordinal)) break;
                        if (!string.Equals(row.ParentUid, anchorParentUid, StringComparison.Ordinal)) break;
                    }

                    actualRowSpan++;
                }
                int requestedColSpan = Math.Max(mergeInfo.ColumnSpan, 1);
                int actualColSpan = requestedColSpan;
                for (int rr = 0; rr < actualRowSpan; rr++)
                {
                    var row = rows[mergeInfo.RowIndex + rr];
                    int available = Math.Max(0, (row?.Cells?.Count ?? 0) - mergeInfo.ColumnIndex);
                    if (available <= 0)
                    {
                        actualColSpan = 1;
                        actualRowSpan = Math.Max(1, rr);
                        break;
                    }
                    actualColSpan = Math.Min(actualColSpan, Math.Max(1, Math.Min(requestedColSpan, available)));
                }
                anchorCell.RowSpan = actualRowSpan;
                anchorCell.ColSpan = actualColSpan;
                for (int i = mergeInfo.RowIndex; i < mergeInfo.RowIndex + actualRowSpan; i++)
                {
                    var spannedRow = rows[i];
                    if (spannedRow?.Cells == null) continue;
                    for (int j = mergeInfo.ColumnIndex; j < mergeInfo.ColumnIndex + actualColSpan; j++)
                    {
                        if (i == mergeInfo.RowIndex && j == mergeInfo.ColumnIndex)
                        {
                            continue;
                        }
                        if (j >= spannedRow.Cells.Count) break;
                        var spannedCell = spannedRow.Cells[j];
                        spannedCell.IsSpanned = true;
                        spannedCell.RowSpan = 1;
                        spannedCell.ColSpan = null;
                    }
                }
            }
        }
        /// <summary>
        /// Wrapper method that processes automatic row spanning with frozen row support.
        /// This method identifies cells with identical values in adjacent rows and applies row spanning.
        /// Row spanning runs only when grid AutoSpan is Row/HorizontalAndVertical and the column's effective AutoSpan allows Row.
        /// </summary>
        /// <param name="rows">The list of generated rows to process</param>
        internal void ProcessAutomaticRowSpanning(List<Row<object>> rows)
        {
            if (rows == null || rows.Count <= 1 || !(Parent.AutoSpan == AutoSpanMode.Column || Parent.AutoSpan == AutoSpanMode.HorizontalAndVertical))
                return;

            var rowSpanColumns = GridUtils.GetColumns(Parent)
                .Where(col => col.Visible && col.GetEffectiveAutoSpanning(Parent.AutoSpan).HasRow())
                .ToList();
            var visibleRows = rows.Where(r => r.IsDataRow && r.Cells != null && r.Visible).ToList();
            if (rowSpanColumns.Count == 0)
                return;

            // Partition columns into frozen and movable groups in a single pass
            var leftFrozen = new List<GridColumn>();
            var movable = new List<GridColumn>();
            var rightFrozen = new List<GridColumn>();

            foreach (var col in rowSpanColumns)
            {
                if (!col.IsFrozen)
                {
                    movable.Add(col);
                }
                else if (col.Freeze == FreezeDirection.Left)
                {
                    leftFrozen.Add(col);
                }
                else if (col.Freeze == FreezeDirection.Right)
                {
                    rightFrozen.Add(col);
                }
            }

            // Process in correct visual order: Left → Movable → Right
            var columnGroups = new[] { leftFrozen, movable, rightFrozen };
            bool hasFrozenRows = Parent.FrozenRows > 0;

            foreach (var cols in columnGroups)
            {
                if (cols.Count == 0)
                    continue;

                if (hasFrozenRows)
                {
                    ProcessAutomaticRowSpanningWithFrozenSupport(Parent.IsRenderedFromTreeGrid ? visibleRows : rows, cols);
                }
                else
                {
                    ProcessRowSpanningInRange(Parent.IsRenderedFromTreeGrid ? visibleRows : rows, cols, 0, Parent.IsRenderedFromTreeGrid ? visibleRows.Count : rows.Count);
                }
            }
        }

        /// <summary>
        /// Processes row spanning separately for frozen and non-frozen row regions.
        /// </summary>
        private void ProcessAutomaticRowSpanningWithFrozenSupport(List<Row<object>> rows, List<GridColumn> rowSpanColumns)
        {
            var frozenRowBoundary = Parent.FrozenRows;
            ProcessRowSpanningInRange(rows, rowSpanColumns, 0, frozenRowBoundary);
            ProcessRowSpanningInRange(rows, rowSpanColumns, frozenRowBoundary, rows.Count);
        }

        /// <summary>
        /// Processes row spanning for a specific range of rows and columns.
        /// Creates exclusion HashSet once for all columns in this range to avoid repeated O(n) linear searches.
        /// </summary>
        /// <param name="rows">The list of rows to process</param>
        /// <param name="rowSpanColumns">The columns to apply row spanning to</param>
        /// <param name="startIndex">The starting row index for processing</param>
        /// <param name="endIndex">The ending row index for processing</param>
        private void ProcessRowSpanningInRange(List<Row<object>> rows, List<GridColumn> rowSpanColumns, int startIndex, int endIndex)
        {
            if (startIndex >= endIndex || endIndex - startIndex <= 1)
                return;

            // Create exclusion HashSet once for all columns in this range
            // This avoids repeated O(n) linear searches via .Any() calls
            var exclusionSet = ExcludedFromAutomaticSpanning != null && ExcludedFromAutomaticSpanning.Count > 0
                ? new HashSet<(int, int)>(
                    ExcludedFromAutomaticSpanning.Select(x => (x.RowIndex, x.ColumnIndex)))
                : null;

            foreach (var column in rowSpanColumns)
            {
                ProcessColumnRowSpanningInRange(rows, column, startIndex, endIndex, exclusionSet!);
            }
        }

        /// <summary>
        /// Processes row spanning for a single column within a specified row range.
        /// Identifies consecutive rows with identical cell values and applies row spanning.
        /// Uses pre-computed HashSet for O(1) exclusion lookups instead of O(n) .Any() calls.
        /// </summary>
        /// <param name="rows">The list of rows to process</param>
        /// <param name="column">The column to apply row spanning to</param>
        /// <param name="startIndex">The starting row index for processing</param>
        /// <param name="endIndex">The ending row index for processing</param>
        /// <param name="exclusionSet">Pre-computed HashSet of excluded row/column pairs for O(1) lookup</param>
        private void ProcessColumnRowSpanningInRange(List<Row<object>> rows, GridColumn column, int startIndex, int endIndex, 
            HashSet<(int, int)> exclusionSet)
        {
            int i = startIndex;
            while (i < endIndex)
            {
                var currentRow = rows[i];
                if (!currentRow.IsDataRow || currentRow.Cells == null)
                {
                    i++;
                    continue;
                }

                var currentCell = MergeHandler<T>.GetCellByColumn(currentRow, column);

                if (currentCell == null || currentCell.IsRowSpanned || currentCell.IsSpanned)
                {
                    i++;
                    continue;
                }

                // O(1) HashSet lookup instead of O(n) .Any() call
                if (exclusionSet != null && exclusionSet.Contains(((int, int))(currentRow.Index!, currentCell.Index!)))
                {
                    i++;
                    continue;
                }

                var currentValue = GetCellValueForComparison(currentRow.Data!, column);
                int currentColSpan = currentCell.ColSpan.HasValue ? currentCell.ColSpan.Value : 1;
                int spanCount = 1;
                
                for (int j = i + 1; j < endIndex; j++)
                {
                    var nextRow = rows[j];
                    if (!nextRow.IsDataRow || nextRow.Cells == null)
                        break;

                    var nextCell = MergeHandler<T>.GetCellByColumn(nextRow, column);

                    if (nextCell == null || nextCell.IsRowSpanned || nextCell.IsSpanned)
                        break;

                    // O(1) HashSet lookup instead of O(n) .Any() call
                    if (exclusionSet != null && exclusionSet.Contains(((int, int))(nextRow.Index!, nextCell.Index!)))
                    {
                        break;
                    }

                    var nextValue = GetCellValueForComparison(nextRow.Data!, column);
                    if (!MergeHandler<T>.AreValuesEqual(currentValue, nextValue))
                        break;

                    int nextColSpan = nextCell.ColSpan.HasValue ? nextCell.ColSpan.Value : 1;
                    if (currentColSpan != nextColSpan)
                        break;

                    spanCount++;
                }

                if (spanCount > 1)
                {
                    currentCell.RowSpan = spanCount;
                    for (int k = i + 1; k < i + spanCount && k < endIndex; k++)
                    {
                        if (rows[k].IsDataRow && rows[k].Cells != null)
                        {
                            var spannedCell = MergeHandler<T>.GetCellByColumn(rows[k], column);
                            if (spannedCell != null)
                            {
                                spannedCell.IsRowSpanned = true;
                            }
                        }
                    }
                    i += spanCount;
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// Retrieves a cell from a row by its associated column definition.
        /// First attempts to match by column UID, then by field name.
        /// </summary>
        /// <param name="row">The row to search</param>
        /// <param name="column">The column definition to match</param>
        /// <returns>The cell matching the column, or null if not found</returns>
        private static Cell<object> GetCellByColumn(Row<object> row, GridColumn column)
        {
            if (row?.Cells == null)
                return null!;

            if (!string.IsNullOrEmpty(column.Uid))
            {
                var cellByUid = row.Cells.FirstOrDefault(c => c.Column?.Uid == column.Uid);
                if (cellByUid != null)
                    return cellByUid;
            }

            if (!string.IsNullOrEmpty(column.Field))
            {
                var cellByField = row.Cells.FirstOrDefault(c => c.Column?.Field == column.Field);
                if (cellByField != null)
                    return cellByField;
            }

            return null!;
        }
        /// <summary>
        /// Extracts the cell value from row data for comparison during spanning operations.
        /// Uses either the property helper or reflection to get the value.
        /// </summary>
        /// <param name="data">The row data object</param>
        /// <param name="column">The column definition</param>
        /// <returns>The cell value from the row data, or null if not found</returns>
        private object GetCellValueForComparison(object data, GridColumn column)
        {
            if (data == null || column == null || string.IsNullOrEmpty(column.Field))
                return null!;

            // Fast path using PropHelper if available
            if (Parent.PropHelper != null)
                return Parent.PropHelper.GetObject(column.Field, data);

            // Fallback to reflection
            var property = data.GetType().GetProperty(column.Field);
            return property?.CanRead == true ? property.GetValue(data)! : null!;
        }

        /// <summary>
        /// Compares two values for equality, handling nulls, strings, and numeric types appropriately.
        /// </summary>
        /// <param name="value1">The first value to compare</param>
        /// <param name="value2">The second value to compare</param>
        /// <returns>True if the values are equal; otherwise, false</returns>
        private static bool AreValuesEqual(object value1, object value2)
        {
            if (value1 == null && value2 == null)
                return true;
            if (value1 == null || value2 == null)
                return false;

            return value1.Equals(value2);
        }

        /// <summary>
        /// Compares row span values from two cells, treating null as 1.
        /// </summary>
        /// <param name="cell1">The first cell</param>
        /// <param name="cell2">The second cell</param>
        /// <returns>True if both cells have the same row span value; otherwise, false</returns>
        private static bool AreRowSpansEqual(Cell<object> cell1, Cell<object> cell2)
        {
            if (cell1 == null || cell2 == null)
                return false;

            int rowSpan1 = cell1.RowSpan.HasValue ? cell1.RowSpan.Value : 1;
            int rowSpan2 = cell2.RowSpan.HasValue ? cell2.RowSpan.Value : 1;
            return rowSpan1 == rowSpan2;
        }

        /// <summary>
        /// Processes automatic column spanning with frozen column support.
        /// Identifies cells with identical values in adjacent columns and applies column spanning.
        /// Divides processing into left-frozen, movable, and right-frozen column bands.
        /// </summary>
        /// <param name="rows">The list of rows to process for column spanning</param>
        internal void ProcessAutomaticColumnSpanning(List<Row<object>> rows)
        {
            if (rows == null || rows.Count == 0 || !(Parent.AutoSpan == AutoSpanMode.Row || Parent.AutoSpan == AutoSpanMode.HorizontalAndVertical))
                return;
            var visibleColumns = GridUtils.GetColumns(Parent).Where(c => c.Visible).ToList();
            List<GridColumn> freezeLeft;
            if (Parent.FrozenColumns > 0)
            {
                freezeLeft = visibleColumns.Take(Parent.FrozenColumns).ToList();
            }
            else
            {
                freezeLeft = visibleColumns.Where(c => c.IsFrozen && c.Freeze == FreezeDirection.Left).ToList();
            }

            var freezeRight = visibleColumns.Where(c => c.IsFrozen && c.Freeze == FreezeDirection.Right).ToList();
            var movableColumns = visibleColumns.Where(c => !freezeLeft.Contains(c) && !freezeRight.Contains(c)).ToList();

            foreach (var row in rows)
            {
                if (!row.IsDataRow || row.Cells == null)
                    continue;
                if (freezeLeft.Count > 0)
                {
                    ProcessRowColumnSpanningByColumns(row, freezeLeft, reverse: false);
                }
                if (movableColumns.Count > 0)
                {
                    ProcessRowColumnSpanningByColumns(row, movableColumns, reverse: false);
                }
                if (freezeRight.Count > 0)
                {
                    ProcessRowColumnSpanningByColumns(row, freezeRight, reverse: true);
                }
            }
        }

        /// <summary>
        /// Processes column spanning for a single row using column definitions.
        /// Iterates by column definitions instead of cell indices to handle grouped rows correctly.
        /// </summary>
        /// <param name="row">The row to process</param>
        /// <param name="columns">The columns to apply column spanning to</param>
        /// <param name="reverse">If true, processes columns in reverse order (for right-frozen columns)</param>
        private void ProcessRowColumnSpanningByColumns(Row<object> row, List<GridColumn> columns, bool reverse)
        {
            if (row?.Cells == null || columns == null || columns.Count <= 1)
                return;

            if (!reverse)
            {
                int i = 0;
                while (i < columns.Count - 1)
                {
                    var currentCell = MergeHandler<T>.GetCellByColumn(row, columns[i]);
                    if (currentCell == null || !currentCell.IsDataCell || !ShouldCellParticipateInColumnSpanning(row, currentCell))
                    {
                        i++;
                        continue;
                    }

                    if (ExcludedFromAutomaticSpanning != null &&
                        ExcludedFromAutomaticSpanning.Any(x => x.RowIndex == row.Index && x.ColumnIndex == currentCell.Index))
                    {
                        i++;
                        continue;
                    }

                    var currentValue = GetCellValueForComparison(row.Data!, currentCell.Column!);
                    int spanCount = 1;

                    for (int j = i + 1; j < columns.Count; j++)
                    {
                        var nextCell = MergeHandler<T>.GetCellByColumn(row, columns[j]);
                        if (nextCell == null || !nextCell.IsDataCell || !ShouldCellParticipateInColumnSpanning(row, nextCell))
                            break;

                        if (ExcludedFromAutomaticSpanning != null &&
                            ExcludedFromAutomaticSpanning.Any(x => x.RowIndex == row.Index && x.ColumnIndex == nextCell.Index))
                        {
                            break;
                        }

                        var nextValue = GetCellValueForComparison(row.Data!, nextCell.Column!);
                        if (!MergeHandler<T>.AreValuesEqual(currentValue, nextValue) || !MergeHandler<T>.AreRowSpansEqual(currentCell, nextCell!))
                            break;

                        spanCount++;
                    }

                    if (spanCount > 1)
                    {
                        currentCell.ColSpan = spanCount;
                        for (int k = i + 1; k < i + spanCount; k++)
                        {
                            var spanned = MergeHandler<T>.GetCellByColumn(row, columns[k]);
                            if (spanned != null)
                            {
                                spanned.IsSpanned = true;
                            }
                        }
                        i += spanCount;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            else
            {
                int i = columns.Count - 1;
                while (i > 0)
                {
                    var currentCell = MergeHandler<T>.GetCellByColumn(row, columns[i]);
                    if (currentCell == null || !currentCell.IsDataCell || !ShouldCellParticipateInColumnSpanning(row, currentCell))
                    {
                        i--;
                        continue;
                    }

                    if (ExcludedFromAutomaticSpanning != null &&
                        ExcludedFromAutomaticSpanning.Any(x => x.RowIndex == row.Index && x.ColumnIndex == currentCell.Index))
                    {
                        i--;
                        continue;
                    }

                    var currentValue = GetCellValueForComparison(row.Data!, currentCell.Column!);
                    int spanCount = 1;

                    for (int j = i - 1; j >= 0; j--)
                    {
                        var prevCell = MergeHandler<T>.GetCellByColumn(row, columns[j]);
                        if (prevCell == null || !prevCell.IsDataCell || !ShouldCellParticipateInColumnSpanning(row, prevCell))
                            break;

                        if (ExcludedFromAutomaticSpanning != null &&
                            ExcludedFromAutomaticSpanning.Any(x => x.RowIndex == row.Index && x.ColumnIndex == prevCell.Index))
                        {
                            break;
                        }

                        var prevValue = GetCellValueForComparison(row.Data!, prevCell.Column!);
                        if (!MergeHandler<T>.AreValuesEqual(currentValue, prevValue) || !MergeHandler<T>.AreRowSpansEqual(currentCell, prevCell))
                            break;

                        spanCount++;
                    }

                    if (spanCount > 1)
                    {
                        currentCell.ColSpan = spanCount;
                        for (int k = i - 1; k > i - spanCount; k--)
                        {
                            var spanned = MergeHandler<T>.GetCellByColumn(row, columns[k]);
                            if (spanned != null)
                            {
                                spanned.IsSpanned = true;
                            }
                        }
                        i -= spanCount;
                    }
                    else
                    {
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether a cell should participate in automatic column spanning based on its properties.
        /// </summary>
        /// <param name="row">The row containing the cell</param>
        /// <param name="cell">The cell to evaluate</param>
        /// <returns>True if the cell can participate in column spanning; otherwise, false</returns>
        private bool ShouldCellParticipateInColumnSpanning(Row<object> row, Cell<object> cell)
        {
            if (!row.IsDataRow ||cell?.Column == null ||!cell.Column.GetEffectiveAutoSpanning(Parent.AutoSpan).HasColumn() ||cell.IsSpanned == true ||
                cell.IsRowSpanned == true || !cell.Visible || cell.IsForeignKey || cell.Column.Freeze == FreezeDirection.Fixed || (string.IsNullOrEmpty(cell.Column.Field) && !cell.IsTemplate))
            {
                return false;
            }

            if (!cell.IsTemplate)
            {
                var cellValue = GetCellValueForComparison(row.Data!, cell.Column);
                if (cellValue == null || (cellValue is string str && string.IsNullOrEmpty(str)))
                    return false;
            }
            return true;
        }
    }
}
