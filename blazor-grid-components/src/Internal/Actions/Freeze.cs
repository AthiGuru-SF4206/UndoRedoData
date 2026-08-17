using Syncfusion.ExcelExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles freeze-related operations for the grid.
    /// Responsibility: Manages frozen column state, freeze direction tracking, and column categorization (left/right/movable).
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class Freeze<T>
    {
        #region Private Properties

        /// <summary>
        /// Reference to the parent SfGrid component.
        /// </summary>
        private SfGrid<T> _parent { get; set; }

        private List<GridColumn> LeftRightFrozenColumns { get; set; } = new List<GridColumn>();
        private List<GridColumn> MovableColumns { get; set; } = new List<GridColumn>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Freeze class.
        /// </summary>
        /// <param name="parent">The parent grid component.</param>
        public Freeze(SfGrid<T> parent) => _parent = parent;

        #endregion

        #region Frozen State Organization

        /// <summary>
        /// Sets the frozen table state based on the freeze directions and frozen column counts.
        /// Determines whether the grid has frozen columns on the left, right, both, or none.
        /// Also organizes columns into left-frozen, movable, and right-frozen categories.
        /// </summary>
        internal void SetFrozenCount()
        {
            if ((GetFreezeLeftCount() != 0 || _parent.FrozenColumns != 0) && GetFreezeRightColumnsCount() == 0)
            {
                _parent.FrozenName = FreezeTable.Left;
            }
            else if ((GetFreezeLeftCount() == 0 && _parent.FrozenColumns == 0) && GetFreezeRightColumnsCount() != 0)
            {
                _parent.FrozenName = FreezeTable.Right;
            }
            else if ((GetFreezeLeftCount() != 0 || _parent.FrozenColumns != 0) && GetFreezeRightColumnsCount() != 0)
            {
                _parent.FrozenName = FreezeTable.LeftRight;
            }
            else
            {
                _parent.FrozenName = FreezeTable.None;
            }

            if (GetFreezeLeftCount() != 0 || GetFreezeRightColumnsCount() != 0 || _parent.IsFreezeLineMoved)
            {
                var leftCols = new List<GridColumn>();
                var rightCols = new List<GridColumn>();
                var movableCols = new List<GridColumn>();
                SetFrozenColumnModel(_parent.Columns!, ref leftCols, ref rightCols, ref movableCols);
            }
        }

        /// <summary>
        /// Organizes columns into left-frozen, right-frozen, and movable categories.
        /// Recursively processes nested/stacked columns.
        /// </summary>
        /// <param name="cols">The list of columns to categorize.</param>
        /// <param name="leftCols">Output: List of left-frozen columns.</param>
        /// <param name="rightCols">Output: List of right-frozen columns.</param>
        /// <param name="movableCols">Output: List of movable columns.</param>
        private void SetFrozenColumnModel(List<GridColumn> cols, ref List<GridColumn> leftCols, ref List<GridColumn> rightCols, ref List<GridColumn> movableCols)
        {
            for (var i = 0; i < cols.Count; i++)
            {
                if ((cols[i].Columns) != null)
                {
                    SetFrozenColumnModel(cols[i].Columns!, ref leftCols, ref rightCols, ref movableCols);
                }
                else
                {
                    if ((cols[i].Freeze.Equals(FreezeDirection.Left) && cols[i].IsFrozen) || cols[i].Index < _parent.FrozenColumns)
                    {
                        leftCols.Add(cols[i]);
                    }
                    else if (cols[i].Freeze.Equals(FreezeDirection.Right) && cols[i].IsFrozen)
                    {
                        rightCols.Add(cols[i]);
                    }
                    else
                    {
                        movableCols.Add(cols[i]);
                    }
                }
            }
            _parent.FrozenColumnModel = leftCols.Concat(movableCols).Concat(rightCols).ToList();
        }

        #endregion

        #region Frozen Column Count Queries

        /// <summary>
        /// Calculates the total count of frozen columns (FrozenColumns property + freeze direction columns).
        /// </summary>
        /// <returns>The total number of frozen columns.</returns>
        internal int GetFrozenCount() =>
            (int)_parent.FrozenColumns + GridUtils.GetColumns(_parent).Where(_ => _.IsFrozen && (_.Freeze.Equals(FreezeDirection.Left) || _.Freeze.Equals(FreezeDirection.Right))).ToList().Count;

        /// <summary>
        /// Calculates the count of columns with freeze direction set to Right.
        /// </summary>
        /// <returns>The number of right-frozen columns that are visible.</returns>
        internal int GetFreezeRightCount() =>
            (int)GridUtils.GetColumns(_parent).Where(_ => _.IsFrozen && _.Freeze.Equals(FreezeDirection.Right) && _.Visible).Count();

        /// <summary>
        /// Calculates the count of columns with freeze direction set to Left.
        /// Applies column virtualization filtering if enabled.
        /// </summary>
        /// <returns>The number of left-frozen columns.</returns>
        internal int GetFreezeLeftCount()
        {
            List<GridColumn> columns = GridUtils.GetColumns(_parent);
            if (_parent.EnableColumnVirtualization)
            {
                columns = columns.Where(_ => _.Visible).ToList();
            }
            return columns.Where(_ => _.IsFrozen && _.Freeze.Equals(FreezeDirection.Left)).Count();
        }

        /// <summary>
        /// Calculates the count of visible frozen columns from the _parent.FrozenColumns property.
        /// </summary>
        /// <returns>The number of visible frozen columns.</returns>
        internal int GetVisibleFrozenColumnsCount() =>
            (GridUtils.GetColumns(_parent).Skip(0).Take((int)_parent.FrozenColumns)).Where(_ => _.Visible).Count();

        /// <summary>
        /// Calculates the count of columns with freeze direction set to Right (includes hidden columns).
        /// </summary>
        /// <returns>The number of right-frozen columns.</returns>
        internal int GetFreezeRightColumnsCount() =>
            GridUtils.GetColumns(_parent).Where(_ => _.IsFrozen && _.Freeze.Equals(FreezeDirection.Right)).Count();

        /// <summary>
        /// Calculates the count of columns with freeze direction set to Fixed.
        /// </summary>
        /// <returns>The number of fixed-freeze columns that are visible.</returns>
        internal int GetFreezeFixedCount() =>
            (int)GridUtils.GetColumns(_parent).Where(_ => _.IsFrozen && _.Freeze.Equals(FreezeDirection.Fixed) && _.Visible).Count();

        #endregion

        #region Frozen Column Collection Management

        /// <summary>
        /// Sets the collection of left and right frozen columns.
        /// </summary>
        /// <param name="frozenColumns">The list of frozen columns to store.</param>
        internal void SetFrozenColumns(List<GridColumn> frozenColumns)
        {
            LeftRightFrozenColumns = frozenColumns;
        }

        /// <summary>
        /// Gets the collection of left and right frozen columns.
        /// </summary>
        /// <returns>The list of stored frozen columns.</returns>
        internal List<GridColumn> GetFrozenColumns()
        {
            return LeftRightFrozenColumns;
        }

        /// <summary>
        /// Gets the collection of right-frozen columns that are visible.
        /// </summary>
        /// <returns>The list of right-frozen columns.</returns>
        internal List<GridColumn> GetFrozenRightFreezeColumns()
        {
            return GridUtils.GetColumns(_parent).Where(_ => _.IsFrozen && _.Visible && _.Freeze.Equals(FreezeDirection.Right)).ToList();
        }

        /// <summary>
        /// Gets the collection of right-frozen columns from the given list of columns
        /// </summary>
        /// <returns>The list of right-frozen columns.</returns>
        internal static List<GridColumn> GetFrozenRightColumns(List<GridColumn>? columns)
        {
            return columns!.Where(_ => _.IsFrozen && _.Freeze.Equals(FreezeDirection.Right)).ToList();
        }

        /// <summary>
        /// Gets the collection of left-frozen columns from the given list of columns
        /// </summary>
        /// <returns>The list of left-frozen columns.</returns>
        internal static List<GridColumn> GetFrozenLeftColumns(List<GridColumn>? columns)
        {
            return columns!.Where(_ => _.IsFrozen && _.Freeze.Equals(FreezeDirection.Left)).ToList();
        }

        /// <summary>
        /// Sets the collection of movable (non-frozen) columns.
        /// </summary>
        /// <param name="movableColumns">The list of movable columns to store.</param>
        internal void SetMovableColumns(List<GridColumn> movableColumns)
        {
            MovableColumns = movableColumns;
        }

        /// <summary>
        /// Gets the collection of movable (non-frozen) columns.
        /// </summary>
        /// <returns>The list of stored movable columns.</returns>
        internal List<GridColumn> GetMovableColumns()
        {
            return MovableColumns;
        }

        #endregion

        #region ClientOptions

        internal FreezeLineMovingClientOptions GetFreezeLineClientOptions() => new FreezeLineMovingClientOptions()
        {
            columns = _parent.Columns,
            actualFrozenColumns = _parent.FrozenColumns,
            frozenRightCount = GetFreezeRightCount(),
            frozenLeftCount = GetFreezeLeftCount(),
            frozenLeftColumnsCount = GetFreezeLeftCount(),
            frozenColumns = GetFrozenCount(),
            isColumnReordered = (_parent.ReorderModule != null && _parent.ReorderModule.IsColumnReordered),
        };

        #endregion

        #region Stacked Frozen Column Management

        internal GridColumn SetStackedFrozenAndMovableColumns(GridColumn column, string label = "", bool isLocked = false)
        {
            string jsonString = JsonSerializer.Serialize(column);
            GridColumn? StackedColumns = JsonSerializer.Deserialize<GridColumn>(jsonString);
            var ColumnsRemoveCount = StackedColumns?.Columns?.Count;

            for (int j = 0; j < column.Columns?.Count; j++)
            {
                var innerColumn = column.Columns[j];
                if (innerColumn.Columns == null)
                {
                    if (!string.IsNullOrEmpty(label) && innerColumn.FrozenMovableLabel != null && innerColumn.FrozenMovableLabel.Contains(label, StringComparison.CurrentCulture))
                    {
                        StackedColumns?.Columns?.Add(innerColumn);
                        if (label == "FrozenLeft" && _parent.frozenColumnCount != 0)
                        {
                            _parent.frozenColumnCount--;
                        }
                    }
                    else if (innerColumn.FixedColumn && isLocked)
                    {
                        StackedColumns?.Columns?.Add(innerColumn);
                    }
                    else if (!innerColumn.FixedColumn && !isLocked && GetFrozenCount() == 0)
                    {
                        StackedColumns?.Columns?.Add(innerColumn);
                    }
                }
                else
                {
                    var col = SetStackedFrozenAndMovableColumns(innerColumn, label, isLocked: isLocked);
                    if (col.Columns?.Count != 0)
                    {
                        StackedColumns?.Columns?.Add(col);
                    }
                }
            }
            if (ColumnsRemoveCount != 0)
            {
                StackedColumns?.Columns?.RemoveRange(0, (int)ColumnsRemoveCount!);
            }
            return StackedColumns!;
        }

        internal int GetStackedFrozenColumns(List<GridColumn> ColumnList, int FrozenColumnsCount)
        {
            var Count = 0;
            for (int i = 0; i < FrozenColumnsCount; i++)
            {
                if (_parent.Columns != null && ColumnList.Where(x => x.Index == i && x.Columns != null && x.Columns.Count > 0).Any())
                {
                    foreach (var col in _parent.Columns[i].Columns!)
                    {
                        if (col.Columns != null && col.Columns.Count > 0)
                        {
                            var num = GetStackedFrozenColumns(col.Columns, col.Columns.Count);
                            i += num - 1;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    i--;
                    Count++;
                }
                else
                {
                    Count++;
                }
            }

            return Count;

        }

        internal static void SetFrozenMovableLabel(List<GridColumn>? parentColumn, int frozenColumnNum)
        {
            for (int i = 0; i < parentColumn?.Count; i++)
            {
                if (frozenColumnNum > 0)
                {
                    parentColumn[i].FrozenMovableLabel = frozenColumnNum == 1 ? "FrozenLeftLast" : "FrozenLeft";
                    frozenColumnNum--;
                }
                else if (parentColumn[i].IsFrozen && parentColumn[i].Freeze == FreezeDirection.Left)
                {
                    parentColumn[i].FrozenMovableLabel = parentColumn.Skip(i + 1).ToList().Where(x => x.Freeze == FreezeDirection.Left && x.IsFrozen).Any() ? "FrozenLeft" : "FrozenLeftLast";
                }
                else if (parentColumn[i].IsFrozen && parentColumn[i].Freeze == FreezeDirection.Right)
                {
                    parentColumn[i].FrozenMovableLabel = parentColumn.Take(i).ToList().Where(x => x.Freeze == FreezeDirection.Right && x.IsFrozen).Any() ? "FrozenRight" : "FrozenRightFirst";
                }
                else
                {
                    parentColumn[i].FrozenMovableLabel = parentColumn[i].IsFrozen && parentColumn[i].Freeze == FreezeDirection.Fixed ? "MovableFixed" : "Movable";
                }
            }
        }

        private static bool StackedFrozenColContains(GridColumn StackedColumn)
        {
            if (StackedColumn.Columns != null && StackedColumn.Columns.Count > 0)
            {
                foreach (var Col in StackedColumn.Columns)
                {
                    if (Col.Columns == null && Col.FrozenMovableLabel != null && Col.FrozenMovableLabel.Contains("Movable", StringComparison.CurrentCulture))
                    {
                        return false;
                    }
                    else if (Col.Columns == null && Col.FrozenMovableLabel != null && Col.FrozenMovableLabel.Contains("FrozenLeft", StringComparison.CurrentCulture))
                    {
                        return true;
                    }
                    else if (Col.Columns != null && Col.Columns.Count > 0)
                    {
                        return StackedFrozenColContains(Col);
                    }
                }
            }
            return true;
        }

        #endregion

        #region Frozen Column Styling

        internal string ApplyFrozenColumnsClass(GridColumn Column)
        {
            var classNames = "";
            var groupedColumnCount = _parent.GroupSettings?.Columns?.Length ?? 0;
            var GroupedColumn = _parent.GroupSettings?.Columns?.ToList();

            // Refactored frozen column primary check
            bool isColumnFrozen = Column?.IsFrozen == true;
            bool hasStackedHeaderWithoutFrozen = _parent.FrozenColumns == 0 && !Column?.IsFrozen == true && Column?.Columns != null && Column?.Columns.Count > 0;
            bool hasChildFrozenColumns = hasStackedHeaderWithoutFrozen && Column != null && IsFrozenColumn(Column);
            bool shouldProcessFrozen = isColumnFrozen || hasChildFrozenColumns;

            if (shouldProcessFrozen)
            {
                if (_parent.IsStackedHeader && Column != null)
                {
                    if (IsLastColumn(Column))
                    {
                        classNames = string.Concat(classNames, " e-leftfreeze e-freezeleftborder");
                    }
                    else if (Column.FrozenMovableLabel != null && Column.FrozenMovableLabel.Contains("FrozenLeftLast", StringComparison.CurrentCulture))
                    {
                        classNames = string.Concat(classNames, " e-leftfreeze e-freezeleftborder");
                    }
                    else if ((Column.FrozenMovableLabel == null && IsLeftFreezeColumn(Column)) || Column.FrozenMovableLabel == "FrozenLeft")
                    {
                        classNames = string.Concat(classNames, " e-leftfreeze");
                    }
                    else if (IsFirstColumn(Column))
                    {
                        classNames = string.Concat(classNames, " e-rightfreeze e-freezerightborder");
                    }
                    else if (Column.FrozenMovableLabel != null && Column.FrozenMovableLabel.Contains("FrozenRight", StringComparison.CurrentCulture))
                    {
                        classNames = string.Concat(classNames, " e-rightfreeze");
                    }
                    else if ((Column.FrozenMovableLabel == null && IsRightFreezeColumn(Column)) || Column.FrozenMovableLabel == "FrozenRight")
                    {
                        classNames = string.Concat(classNames, " e-rightfreeze");
                    }
                }
                else
                {
                    var FrozenLeftColumns = _parent.Columns?.Where(x => x.Freeze == FreezeDirection.Left && x.IsFrozen && x.Visible).ToList();
                    var FrozenRightColumns = _parent.Columns?.Where(x => x.Freeze == FreezeDirection.Right && x.IsFrozen && x.Visible).ToList();
                    // Refactored left freeze border condition
                    bool isLastFrozenLeftColumn = Column?.Index == FrozenLeftColumns?.LastOrDefault()?.Index;
                    bool hasGroupedColumns = _parent.GroupSettings?.Columns != null && _parent.GroupSettings.Columns.Length > 0;

                    if (Column != null && Column.Freeze.Equals(FreezeDirection.Left) && Column.IsFrozen && FrozenLeftColumns != null && FrozenLeftColumns.Count > 0 && !isLastFrozenLeftColumn)
                    {
                        classNames = string.Concat(classNames, " e-leftfreeze");
                        if (hasGroupedColumns)
                        {
                            for (int i = 0; i < FrozenLeftColumns.Count; i++)
                            {
                                foreach (var col in GroupedColumn!)
                                {
                                    if (col != FrozenLeftColumns.LastOrDefault()?.Field && Column.Equals(FrozenLeftColumns[FrozenLeftColumns.Count - 1]))
                                    {
                                        classNames = string.Concat(classNames, " e-freezeleftborder");
                                    }
                                    else if (col == FrozenLeftColumns.LastOrDefault()?.Field &&_parent.GroupSettings!=null && _parent.GroupSettings.ShowGroupedColumn && Column.Equals(FrozenLeftColumns[FrozenLeftColumns.Count - 1]))
                                    {
                                        classNames = string.Concat(classNames, " e-freezeleftborder");
                                    }
                                }
                            }
                        }
                    }
                    else if (Column != null && Column.Freeze.Equals(FreezeDirection.Left) && Column.IsFrozen && FrozenLeftColumns != null && FrozenLeftColumns.Count > 0 && Column.Index == FrozenLeftColumns.LastOrDefault()?.Index)
                    {
                        classNames = string.Concat(classNames, " e-leftfreeze e-freezeleftborder");
                    }
                    // Refactored right freeze border condition
                    bool isFirstRightFrozenColumn = (!_parent.EnableColumnVirtualization && FrozenRightColumns?.Count > 0 && Column != null && (Column.Index == FrozenRightColumns[0].Index)) || (_parent.EnableColumnVirtualization && Column?.Index == _parent.VirtualScrollModule?.VirtualizedColumns.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Right)).FirstOrDefault()?.Index);
                    bool hasNoGroupedColumns = groupedColumnCount == 0;
                    bool shouldApplyRightFreezeBorder = Column!= null && Column.Freeze.Equals(FreezeDirection.Right) && Column.IsFrozen && Column.Visible && FrozenRightColumns != null && FrozenRightColumns.Count > 0 && isFirstRightFrozenColumn && hasNoGroupedColumns;

                    if (shouldApplyRightFreezeBorder)
                    {
                        classNames = string.Concat(classNames, " e-rightfreeze e-freezerightborder");
                    }
                    else if (Column!= null && Column.Freeze.Equals(FreezeDirection.Right) && Column.IsFrozen)
                    {
                        classNames = string.Concat(classNames, " e-rightfreeze");
                        if (_parent.GroupSettings!.Columns != null && _parent.GroupSettings.Columns.Length > 0)
                        {
                            for (int i = 0; i < FrozenRightColumns?.Count; i++)
                            {
                                foreach (var col in GroupedColumn!)
                                {
                                    if (col != FrozenRightColumns[i].Field && Column.Equals(FrozenRightColumns.FirstOrDefault()) && !_parent.GroupSettings.ShowGroupedColumn)
                                    {
                                        classNames = string.Concat(classNames, " e-freezerightborder");
                                    }
                                    else if (col.Equals(FrozenRightColumns.FirstOrDefault()!.Field, StringComparison.Ordinal) && Column.Index.Equals(FrozenRightColumns.FirstOrDefault()?.Index))
                                    {
                                        classNames = string.Concat(classNames, " e-freezerightborder");
                                    }
                                }
                            }
                        }
                    }
                    // Refactored fixed freeze border conditions
                    bool isFixedFrozen = Column!= null && Column.Freeze.Equals(FreezeDirection.Fixed) && Column.IsFrozen;

                    if (isFixedFrozen && Column != null)
                    {
                        var ArrangedColumns = _parent.RearrangeColumns(_parent.Columns!.Where(x => x.Visible).ToList());
                        var frozenColumnsFixed = _parent.Columns!.Where(x => x.Freeze == FreezeDirection.Fixed && x.IsFrozen && x.Visible).ToList();
                        classNames = string.Concat(classNames, " e-fixedfreeze");
                        var CurrentIndex = Column.Index;

                        // Refactored isolated fixed column border logic
                        bool isIsolatedFixedColumn = frozenColumnsFixed.Count > 0 && CurrentIndex != 0 && CurrentIndex != ArrangedColumns.Count - 1 && !ArrangedColumns[CurrentIndex - 1].IsFrozen && !ArrangedColumns[CurrentIndex + 1].IsFrozen && ArrangedColumns[CurrentIndex + 1].Freeze != FreezeDirection.Fixed && ArrangedColumns[CurrentIndex - 1].Freeze != FreezeDirection.Fixed;

                        if (isIsolatedFixedColumn)
                        {
                            classNames = string.Concat(classNames, " e-freezeleftborder e-freezerightborder");
                        }
                        else if (frozenColumnsFixed.Count > 0)
                        {
                            // Refactored left fixed freeze condition
                            bool hasLeftUnfrozenColumn = CurrentIndex > 0 && !ArrangedColumns[CurrentIndex - 1].IsFrozen;
                            bool hasLeftFrozenButDifferentFreeze = CurrentIndex > 0 && ArrangedColumns[CurrentIndex - 1].IsFrozen && ArrangedColumns[CurrentIndex - 1].Freeze != FreezeDirection.Fixed;
                            bool shouldApplyLeftFreezeBorder = hasLeftUnfrozenColumn || hasLeftFrozenButDifferentFreeze;

                            if (shouldApplyLeftFreezeBorder)
                            {
                                classNames = string.Concat(classNames, " e-freezeleftborder");
                            }

                            // Refactored right fixed freeze condition
                            bool hasRightUnfrozenColumn = CurrentIndex < ArrangedColumns.Count - 1 && !ArrangedColumns[CurrentIndex + 1].IsFrozen;
                            bool hasRightFrozenButDifferentFreeze = CurrentIndex < ArrangedColumns.Count - 1 && ArrangedColumns[CurrentIndex + 1].IsFrozen && ArrangedColumns[CurrentIndex + 1].Freeze != FreezeDirection.Fixed;

                            if (hasRightUnfrozenColumn || hasRightFrozenButDifferentFreeze)
                            {
                                classNames = string.Concat(classNames, " e-freezerightborder");
                            }
                        }
                    }
                }
            }
            else if (!_parent.IsStackedHeader && GetFrozenCount() > 0 && Column?.Index < GetFrozenCount() && _parent.Columns?.Where(x => x.Freeze == FreezeDirection.Right && x.IsFrozen).ToList().Count == 0 && GetFreezeLeftCount() == 0 && GetFreezeFixedCount() == 0)
            {
                // Refactored last frozen column check
                bool isLastFrozenColumn = Column.Index == GetFrozenCount() - 1;

                if (isLastFrozenColumn)
                {
                    classNames = string.Concat(classNames, " e-leftfreeze e-freezeleftborder");
                }
                else
                {
                    classNames = string.Concat(classNames, " e-leftfreeze");
                    if (Column == _parent.Columns.Skip(0).Take((int)_parent.FrozenColumns).Where(_ => _.Visible).ToList().LastOrDefault())
                    {
                        classNames = string.Concat(classNames, " e-freezeleftborder");
                    }

                }
            }
            else if (_parent.IsStackedHeader && _parent.FrozenColumns > 0 && StackedFrozenColContains(Column!))
            {
                // Refactored stacked header frozen column label checks
                bool isLastStackedColumn = IsLastColumn(Column!);
                bool hasFrozenLeftLastLabel = Column != null && Column.FrozenMovableLabel != null && Column.FrozenMovableLabel.Contains("FrozenLeftLast", StringComparison.CurrentCulture);
                bool isLeftFreezeColumn = (Column != null && Column.FrozenMovableLabel == null && IsLeftFreezeColumn(Column)) || Column!.FrozenMovableLabel == "FrozenLeft";

                if (isLastStackedColumn || hasFrozenLeftLastLabel)
                {
                    classNames = string.Concat(classNames, " e-leftfreeze e-freezeleftborder");
                }
                else if (isLeftFreezeColumn)
                {
                    classNames = string.Concat(classNames, " e-leftfreeze");
                }
            }
            else
            {
                classNames = string.Concat(classNames, " e-unfreeze");
            }

            return classNames;
        }

        internal string ApplyFrozenColumnsStyles(GridColumn Column)
        {
            string text = "";
            var groupedColumnCount = _parent.GroupSettings?.Columns?.Length ?? 0;
            var GroupedColumn = _parent.GroupSettings?.Columns?.ToList();
            if (_parent.IsStackedHeader && _parent.FrozenColumns == 0 && (IsLeftFreezeColumn(Column) || IsRightFreezeColumn(Column)))
            {
                int LeftRightWidth = 0;
                if (IsLeftFreezeColumn(Column) && Column.Visible)
                {
                    List<GridColumn> FreezeLeftColumn = new List<GridColumn>();
                    FreezeLeftColumn = _parent.Columns!.Where(x => IsLeftFreezeColumn(x) && x.Visible).ToList();
                    if (IsFrozenColumn(Column))
                    {
                        LeftRightWidth = _parent.SetStyleWidth(FreezeLeftColumn, Column, FreezeDirection.Left);
                    }
                    text = " left:" + LeftRightWidth + "px";
                }
                else if (IsRightFreezeColumn(Column) && Column?.Visible == true)
                {
                    List<GridColumn> FreezeRightColumn = new List<GridColumn>();
                    FreezeRightColumn = _parent.Columns!.Where(x => IsRightFreezeColumn(x) && x.Visible).ToList();
                    if (IsFrozenColumn(Column))
                    {
                        LeftRightWidth = _parent.SetStyleWidth(FreezeRightColumn, Column, FreezeDirection.Right);
                    }
                    text = " right:" + LeftRightWidth + "px";
                }

            }
            else if (Column?.IsFrozen == true)
            {
                double LeftRightWidth = 0;
                if (Column.Freeze.Equals(FreezeDirection.Left) && Column.IsFrozen)
                {
                    var frozenColumnsLeft = _parent.Columns!.Where(x => x.Freeze == FreezeDirection.Left && x.IsFrozen).ToList();
                    for (int i = 0; i < Column.Index; i++)
                    {
                        if (GroupedColumn != null && GroupedColumn.Any(x => x == frozenColumnsLeft[i].Field) && _parent.GroupSettings != null && !_parent.GroupSettings.ShowGroupedColumn)
                            continue;
                        // Refactored frozen left column index check
                        bool isValidFrozenIndex = Column.Index < frozenColumnsLeft.Count;
                        bool isFrozenVisible = isValidFrozenIndex && frozenColumnsLeft[i].Visible;
                        if (isFrozenVisible)
                        {
                            LeftRightWidth += GridUtils.GetDoubleParsedWidth(frozenColumnsLeft[i].Width);
                        }
                    }
                    for (int i = 0; i < groupedColumnCount; i++)
                    {
                        LeftRightWidth += 30;
                    }
                    if (_parent.AllowRowDragAndDrop)
                    {
                        LeftRightWidth += 30;
                    }
                    if (((IGrid)_parent).GridTemplates?.DetailTemplate != null)
                    {
                        LeftRightWidth += 30;
                    }
#pragma warning disable BL0005
                    Column.TranslateLeftRightValue = LeftRightWidth;
#pragma warning restore BL0005
                    if (_parent.EnableColumnVirtualization && _parent.VirtualScrollModule != null && _parent.VirtualScrollModule.TranslateX != 0)
                    {
                        LeftRightWidth = LeftRightWidth - _parent.VirtualScrollModule.TranslateX;
                    }
                    text = _parent.EnableRtl ? " right:" + LeftRightWidth + "px" : " left:" + LeftRightWidth + "px";
                }
                else if (Column.Freeze.Equals(FreezeDirection.Right) && Column?.IsFrozen == true)
                {
                    var frozenColumnsRight = _parent.Columns?.Where(x => x.Freeze == FreezeDirection.Right && x.IsFrozen).ToList();
                    for (int i = 0; i < frozenColumnsRight!.Count; i++)
                    {
                        if (Column.Index < frozenColumnsRight[i].Index)
                        {
                            if (GroupedColumn != null && GroupedColumn.Any(x => x == frozenColumnsRight[i].Field))
                                continue;
                            if (frozenColumnsRight[i].Visible)
                            {
                                LeftRightWidth += GridUtils.GetDoubleParsedWidth(frozenColumnsRight[i].Width);
                            }
                        }
                    }
#pragma warning disable BL0005
                    Column.TranslateLeftRightValue = LeftRightWidth;
#pragma warning restore BL0005
                    if (_parent.EnableColumnVirtualization && _parent.VirtualScrollModule != null && _parent.VirtualScrollModule.TranslateX != 0)
                    {
                        LeftRightWidth = LeftRightWidth + _parent.VirtualScrollModule.TranslateX;
                    }
                    text = " right:" + LeftRightWidth + "px";

                }
                else if (_parent.Columns != null && Column != null && Column.Freeze.Equals(FreezeDirection.Fixed) && Column.IsFrozen)
                {
                    double LeftWidth = 0;
                    double RightWidth = 0;
                    var frozenColumnsFixed = _parent.EnableColumnVirtualization ? _parent.Columns.Where(x => x.Freeze == FreezeDirection.Fixed && x.IsFrozen && x.Index >= _parent.VirtualScrollModule?.StartColumnIndex && x.Index <= _parent.VirtualScrollModule.EndColumnIndex).ToList() : _parent.Columns.Where(x => x.Freeze == FreezeDirection.Fixed && x.IsFrozen).ToList();
                    var frozenColumnsLeft = _parent.Columns.Where(x => x.Freeze == FreezeDirection.Left && x.IsFrozen).ToList();
                    var frozenColumnsRight = _parent.Columns.Where(x => x.Freeze == FreezeDirection.Right && x.IsFrozen && x.Visible).ToList();
                    double FixedLeftWidth = 0;
                    double FixedRightWidth = 0;
                    foreach (var frozenLeftColumn in frozenColumnsLeft)
                    {
                        if (GroupedColumn != null && _parent.GroupSettings != null && GroupedColumn.Any(x => x == frozenLeftColumn.Field) && !_parent.GroupSettings.ShowGroupedColumn)
                            continue;
                        if (frozenLeftColumn.Visible)
                        {
                            LeftWidth = LeftWidth + GridUtils.GetDoubleParsedWidth(frozenLeftColumn.Width);
                        }
                    }
                    foreach (var frozenRightColumn in frozenColumnsRight)
                    {
                        if (frozenRightColumn.Visible)
                        {
                            RightWidth = RightWidth + GridUtils.GetDoubleParsedWidth(frozenRightColumn.Width);
                        }
                    }
                    if (frozenColumnsLeft.Count == 0 && frozenColumnsRight.Count == 0 && frozenColumnsFixed.Count == 0)
                    {
                        var FrozenColumns = _parent.Columns.Where(x => x.Index < GetFrozenCount()).ToList();
                        for (int i = 0; i < FrozenColumns.Count; i++)
                        {
                            if (FrozenColumns[i].Visible)
                            {
                                LeftWidth += GridUtils.GetDoubleParsedWidth(FrozenColumns[i].Width);
                            }
                        }
                    }
                    if (frozenColumnsFixed.Count > 1)
                    {
                        var leftColumns = frozenColumnsFixed.Where(x => x.Index < Column.Index).ToList();
                        var rightColumns = frozenColumnsFixed.Where(x => x.Index > Column.Index).ToList();
                        for (int i = 0; i < rightColumns.Count; i++)
                        {
                            if (rightColumns[i].Visible)
                            {
                                FixedRightWidth = FixedRightWidth + GridUtils.GetDoubleParsedWidth(rightColumns[i].Width);
                            }
                        }
                        for (int i = 0; i < leftColumns.Count; i++)
                        {
                            if (GroupedColumn != null && GroupedColumn.Any(x => x == leftColumns[i].Field) && !_parent.GroupSettings!.ShowGroupedColumn)
                                continue;
                            if (leftColumns[i].Visible)
                            {
                                FixedLeftWidth = FixedLeftWidth + GridUtils.GetDoubleParsedWidth(leftColumns[i].Width);
                            }
                        }
                    }
                    for (int i = 0; i < groupedColumnCount; i++)
                    {
                        LeftWidth += 30;
                    }

                    LeftWidth += FixedLeftWidth;
                    RightWidth += FixedRightWidth;

                    if (_parent.EnableColumnVirtualization && _parent.VirtualScrollModule != null && _parent.VirtualScrollModule.TranslateX != 0)
                    {
                        LeftWidth = LeftWidth - _parent.VirtualScrollModule.TranslateX;
                        RightWidth = RightWidth + _parent.VirtualScrollModule.TranslateX;
                    }

                    text = " left:" + LeftWidth + "px; " + " right:" + RightWidth + "px";
                }
            }
            else if (!_parent.IsStackedHeader && GetFrozenCount() > 0 && Column?.Index < GetFrozenCount() && GetFreezeRightCount() == 0 && GetFreezeLeftCount() == 0 && GetFreezeFixedCount() == 0)
            {
                double LeftWidth = 0;
                var FrozenColumns = _parent.Columns?.Take(GetFrozenCount()).ToList();
                for (int i = 0; i < Column.Index; i++)
                {
                    if (GroupedColumn != null && _parent.GroupSettings != null && FrozenColumns != null && GroupedColumn.Any(x => x == FrozenColumns[i].Field) && !_parent.GroupSettings.ShowGroupedColumn)
                        continue;
                    if (FrozenColumns != null && FrozenColumns[i].Visible)
                    {
                        LeftWidth += GridUtils.GetDoubleParsedWidth(FrozenColumns[i].Width);
                    }
                }
                for (int i = 0; i < groupedColumnCount; i++)
                {
                    LeftWidth += 30;
                }
                if (_parent.AllowRowDragAndDrop)
                {
                    LeftWidth += 30;
                }
                if (((IGrid)_parent).GridTemplates?.DetailTemplate != null)
                {
                    LeftWidth += 30;
                }
#pragma warning disable BL0005
                Column.TranslateLeftRightValue = LeftWidth;
#pragma warning restore BL0005
                if (_parent.EnableColumnVirtualization && _parent.VirtualScrollModule != null && _parent.VirtualScrollModule.TranslateX != 0)
                {
                    LeftWidth = LeftWidth - _parent.VirtualScrollModule.TranslateX;
                }
                text = " left:" + LeftWidth + "px";
            }
            else if (_parent.IsStackedHeader && _parent.FrozenColumns > 0 && StackedFrozenColContains(Column!))
            {
                var LeftWidth = 0;
                var FrozenColumnsCount = _parent.FrozenColumns;
                FrozenColumnsCount = GetStackedFrozenColumns(_parent.Columns!, _parent.FrozenColumns);
                var FrozenLeftColumns = _parent.Columns!.Take(FrozenColumnsCount).ToList();
                FrozenLeftColumns = FrozenLeftColumns.Where(x => x.Visible).ToList();
                if (IsFrozenColumn(Column!))
                {
                    LeftWidth = _parent.SetStyleWidth(FrozenLeftColumns, Column!);
                    text = " left:" + LeftWidth + "px";
                }
            }
            return text;
        }

        #endregion

        #region Internal Helper
        internal void SetEnableFrozenLineCursorByCells(List<Cell<object>> Cells, string firstOrLastCell = "")
        {
            if (_parent!.AllowFreezeLineMoving && Cells.Count != 0)
            {
                foreach (Cell<object> cell in Cells)
                {
                    cell.EnableFrozenLineCursor = false;
                }
                var firstVisibleCell = Cells?.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Right))?.FirstOrDefault();
                var lastVisibleCell = Cells?.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Left))?.LastOrDefault();
                var fixedFirstVisibleCell = Cells?.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Fixed))?.FirstOrDefault();
                var fixedLastVisibleCell = Cells?.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Fixed))?.LastOrDefault();
                var FrozenFixedCells = Cells?.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Fixed))?.ToList();
                var movableFirstVisibleCell = Cells?.Where(x => !x.IsFrozen)?.FirstOrDefault();
                var movableLastVisibleCell = Cells?.Where(x => !x.IsFrozen)?.LastOrDefault();
                if (firstVisibleCell != null)
                {
                    firstVisibleCell.EnableFrozenLineCursor = true;
                }
                if (lastVisibleCell != null)
                {
                    lastVisibleCell.EnableFrozenLineCursor = true;
                }
                if (fixedLastVisibleCell != null && fixedFirstVisibleCell != null)
                {
                    for (int i = 0; i < FrozenFixedCells?.Count; i++)
                    {
                        var CurrentCell = FrozenFixedCells[i];
                        var PreviousCell = CurrentCell.Column!.Index == 0 ? Cells?[CurrentCell.Column.Index] : Cells?[CurrentCell.Column.Index - 1];
                        var NextCell = CurrentCell.Column.Index == Cells?.Count - 1 ? Cells?[CurrentCell.Column.Index] : Cells?[CurrentCell.Column.Index + 1];
                        if ((CurrentCell.Column.Index == 0 || !PreviousCell!.Column!.IsFrozen || (PreviousCell.Column.IsFrozen && !PreviousCell.Column.Freeze.Equals(FreezeDirection.Fixed))) && (!NextCell!.Column!.IsFrozen || (NextCell.Column.IsFrozen && !NextCell.Column.Freeze.Equals(FreezeDirection.Fixed))))
                        {
                            CurrentCell.EnableFrozenLineCursor = true;
                            CurrentCell.EnableFixedLeftFrozenLineCursor = true;
                            CurrentCell.EnableFixedRightFrozenLineCursor = true;
                        }
                        if ((CurrentCell.Column.Index == 0 || !PreviousCell!.Column!.IsFrozen || (PreviousCell.Column.IsFrozen && !PreviousCell.Column.Freeze.Equals(FreezeDirection.Fixed))) && NextCell!.Column!.IsFrozen && NextCell.Column.Freeze.Equals(FreezeDirection.Fixed))
                        {
                            CurrentCell.EnableFrozenLineCursor = true;
                            CurrentCell.EnableFixedLeftFrozenLineCursor = true;
                        }
                        if (PreviousCell!.Column!.IsFrozen && PreviousCell.Column.Freeze.Equals(FreezeDirection.Fixed) && (!NextCell!.Column!.IsFrozen || (NextCell.Column.IsFrozen && !NextCell.Column.Freeze.Equals(FreezeDirection.Fixed))))
                        {
                            CurrentCell.EnableFrozenLineCursor = true;
                            CurrentCell.EnableFixedRightFrozenLineCursor = true;
                        }
                        if (CurrentCell.Column.Index == 0 || CurrentCell.Column.Index == Cells!.Count - 1)
                        {
                            CurrentCell.EnableDefaultFrozenLine = true;
                        }
                    }
                }
                if (movableFirstVisibleCell != null && firstVisibleCell == null && lastVisibleCell == null)
                {
                    movableFirstVisibleCell.EnableFrozenLineCursor = true;
                    movableFirstVisibleCell.EnableLeftFrozenLineCursor = true;
                }
                if (movableFirstVisibleCell != null && firstVisibleCell != null && lastVisibleCell == null)
                {
                    movableFirstVisibleCell.EnableFrozenLineCursor = true;
                    movableFirstVisibleCell.EnableLeftFrozenLineCursor = true;
                }
                if (movableLastVisibleCell != null && firstVisibleCell == null && _parent.EnableRightDefaultCursor)
                {
                    movableLastVisibleCell.EnableFrozenLineCursor = true;
                    movableLastVisibleCell.EnableRightFrozenLineCursor = true;
                }
            }
        }

        internal void SetEnableFrozenLineCursorByCellsExceptFirstAndLast(List<Cell<object>> Cells)
        {
            if (_parent.AllowFreezeLineMoving && Cells.Count != 0)
            {
                var visibleCells = Cells?.Where(_ => _.Visible);
                foreach (Cell<object> cell in Cells!)
                {
                    cell.EnableFrozenLineCursor = cell.EnableLeftFrozenLineCursor = cell.EnableRightFrozenLineCursor = cell.EnableFrozenResizeCursor = false;
                }
                var firstVisibleCell = visibleCells?.FirstOrDefault();
                if (firstVisibleCell != null && GetFreezeLeftCount() == 0 && _parent.FrozenColumns == 0 && _parent.VirtualScrollModule!.StartColumnIndex == 0)
                {
                    firstVisibleCell.EnableFrozenLineCursor = true;
                    firstVisibleCell.EnableLeftFrozenLineCursor = true;
                    if (_parent.AllowResizing && firstVisibleCell.Column?.AllowResizing == true)
                    {
                        firstVisibleCell.EnableFrozenResizeCursor = true;
                    }
                }
                var lastVisibleCell = visibleCells?.LastOrDefault();
                if (lastVisibleCell != null && GetFreezeRightCount() == 0 && _parent.VirtualScrollModule!.EndColumnIndex == GridUtils.GetColumns(_parent).Count - 1)
                {
                    lastVisibleCell.EnableFrozenLineCursor = true;
                    lastVisibleCell.EnableRightFrozenLineCursor = true;
                    if (_parent.AllowResizing && lastVisibleCell.Column?.AllowResizing == true)
                    {
                        lastVisibleCell.EnableFrozenResizeCursor = true;
                    }
                }
            }
        }


        internal static void EnableFrozenLineCursorVirtualHeader(List<GridColumn> frozenColumn, List<GridColumn> movableColumn, List<GridColumn> frozenRightColumn)
        {
            if (frozenColumn.Count > 0)
            {
                var column = frozenColumn.Where(_ => _.Visible).LastOrDefault();
                if (column != null)
                {
                    column.EnableFrozenLineCursor = true;
                }
                else
                {
                    SetEnableFrozeLineCursorMovableColumn(movableColumn, true);
                }
            }
            else
            {
                SetEnableFrozeLineCursorMovableColumn(movableColumn, true);
            }
            if (frozenRightColumn.Count > 0)
            {
                var column = frozenRightColumn.Where(_ => _.Visible).FirstOrDefault();
                if (column != null)
                {
                    column.EnableFrozenLineCursor = true;
                }
                else
                {
                    SetEnableFrozeLineCursorMovableColumn(movableColumn, false);
                }
            }
            else
            {
                SetEnableFrozeLineCursorMovableColumn(movableColumn, false);
            }
        }

        private static void SetEnableFrozeLineCursorMovableColumn(List<GridColumn> movableColumn, bool isFirst = true)
        {
            var column = isFirst ? movableColumn.Where(_ => _.Visible).FirstOrDefault() : movableColumn.Where(_ => _.Visible).LastOrDefault();
            if (column != null)
            {
                column.EnableFrozenLineCursor = true;
                column.EnableLeftFrozenLineCursor = true;
            }
        }

        internal void EnableFrozenLineCursor(List<GridColumn>? columns)
        {
            if (_parent.AllowFreezeLineMoving && _parent.FrozenColumns == 0 && columns != null && columns.Count != 0)
            {
                if (_parent.IsFreezeLineMoved)
                {
                    foreach (var col in columns)
                    {
                        col.EnableLeftFrozenLineCursor = col.EnableRightFrozenLineCursor = col.EnableFrozenLineCursor = false;
                    }
                }
                var FrozenLeftColumns = _parent.FrozenColumns > 0 ? columns.Take(_parent.FrozenColumns)?.ToList() : columns.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Left))?.ToList();
                var FrozenRightColumns = GetFrozenRightColumns(columns);
                var FrozenFixedColumns = columns?.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Fixed))?.ToList();
                var MovableColumns = columns?.Where(x => !x.IsFrozen || (x.IsFrozen && x.Freeze == FreezeDirection.Fixed))?.ToList();
                if (FrozenLeftColumns != null && FrozenLeftColumns.Count > 0)
                {
                    var column = FrozenLeftColumns.Where(_ => _.Visible)?.LastOrDefault();
                    if (column != null)
                    {
                        column.EnableFrozenLineCursor = true;
                    }
                    else
                    {
                        column = MovableColumns?.Where(_ => _.Visible)?.FirstOrDefault();
                        if (column != null)
                        {
                            column.EnableFrozenLineCursor = true;
                            column.EnableLeftFrozenLineCursor = true;
                        }
                    }
                }
                else
                {
                    var column = MovableColumns?.Where(_ => _.Visible)?.FirstOrDefault();
                    if (column != null)
                    {
                        column.EnableFrozenLineCursor = true;
                        column.EnableLeftFrozenLineCursor = true;
                    }
                }
                if (FrozenRightColumns != null && FrozenRightColumns.Count > 0)
                {
                    var column = FrozenRightColumns.Where(_ => _.Visible)?.FirstOrDefault();
                    if (column != null)
                    {
                        column.EnableFrozenLineCursor = true;
                    }
                    else
                    {
                        column = MovableColumns?.Where(_ => _.Visible)?.LastOrDefault();
                        if (column != null)
                        {
                            column.EnableFrozenLineCursor = true;
                            column.EnableRightFrozenLineCursor = true;
                        }
                    }
                }
                else
                {
                    var column = MovableColumns?.Where(_ => _.Visible)?.LastOrDefault();
                    if (column != null)
                    {
                        column.EnableFrozenLineCursor = true;
                        column.EnableRightFrozenLineCursor = true;
                    }
                }
                if (FrozenFixedColumns != null && FrozenFixedColumns.Count > 0)
                {
                    for (int i = 0; i < FrozenFixedColumns.Count; i++)
                    {
                        var CurrentColumn = FrozenFixedColumns[i];
                        var PreviousColumn = CurrentColumn?.Index == 0 ? columns![CurrentColumn.Index] : columns![CurrentColumn!.Index - 1];
                        var NextColumn = CurrentColumn?.Index == columns?.Count - 1 ? columns![CurrentColumn!.Index] : columns![CurrentColumn!.Index + 1];
                        if ((CurrentColumn != null && CurrentColumn.Index == 0 && NextColumn?.IsFrozen != true) || (PreviousColumn?.IsFrozen != true && NextColumn?.IsFrozen != true))
                        {
                            CurrentColumn!.EnableFrozenLineCursor = true;
                            CurrentColumn.EnableFixedLeftFreezeLineCursor = true;
                            CurrentColumn.EnableFixedRightFreezeLineCursor = true;
                        }
                        if ((PreviousColumn?.IsFrozen != true || (CurrentColumn != null && CurrentColumn.Index == 0)) && NextColumn?.IsFrozen == true && NextColumn.Freeze.Equals(FreezeDirection.Fixed))
                        {
                            CurrentColumn!.EnableFrozenLineCursor = true;
                            CurrentColumn.EnableFixedLeftFreezeLineCursor = true;
                        }
                        if (PreviousColumn?.IsFrozen == true && PreviousColumn.Freeze.Equals(FreezeDirection.Fixed) && NextColumn?.IsFrozen != true)
                        {
                            CurrentColumn!.EnableFrozenLineCursor = true;
                            CurrentColumn.EnableFixedRightFreezeLineCursor = true;
                        }
                    }
                }
            }
            else if (_parent.AllowFreezeLineMoving && _parent.FrozenColumns != 0 && columns != null && columns.Count != 0)
            {
                var FrozenLeftColumns = columns.Take(_parent.FrozenColumns)?.ToList();
                if (FrozenLeftColumns != null && FrozenLeftColumns.Count > 0)
                {
                    var column = FrozenLeftColumns.Where(_ => _.Visible)?.LastOrDefault();
                    if (column != null)
                    {
                        column.EnableFrozenLineCursor = true;
                        column.EnableLeftFrozenLineCursor = true;
                    }
                }
            }
        }

        internal async Task InvokeFreezeLineMoving(object args)
        {
            ActionArgs? action = JsonSerializer.Deserialize<ActionArgs>(args.ToString()!);
            if (_parent.GridEvents?.FreezeLineMoving.HasDelegate == true || _parent.IsRenderedFromTreeGrid)
            {
                FreezeLineMovingEventArgs eventArgs = new FreezeLineMovingEventArgs() { StartIndex = action!.FromIndex, Cancel = false, FrozenColumns = _parent.Columns!.Where(col => col.IsFrozen).ToList() };
                if (_parent.IsRenderedFromTreeGrid)
                    await _parent.EventAggregator.NotifyAsync("FreezeLineMoving", eventArgs).ConfigureAwait(true);
                else
                    await _parent.GridEvents!.FreezeLineMoving.InvokeAsync(eventArgs).ConfigureAwait(true);
                await _parent.InvokeMethod("sfBlazor.Grid.preventFreezeLineMoving", new object[] { _parent.DataId, eventArgs.Cancel }).ConfigureAwait(true);
            }
            else
            {
                await _parent.InvokeMethod("sfBlazor.Grid.preventFreezeLineMoving", new object[] { _parent.DataId, false }).ConfigureAwait(true);
            }
        }
        internal async Task InvokeFreezeLineMoved(object args)
        {
            ActionArgs? action = JsonSerializer.Deserialize<ActionArgs>(args.ToString()!);
            FreezeDirection freezeDirection = action?.FreezeDirection == "Left" ? FreezeDirection.Left : action?.FreezeDirection == "Fixed" ? FreezeDirection.Fixed : FreezeDirection.Right;
            List<GridColumn> newFrozenColumns = new List<GridColumn>();
            _parent.IsColumnHeaderChange = true;
            if (action?.FrozenColumnsUidCollection?.Length > 0)
            {
                for (int i = 0; i < action.FrozenColumnsUidCollection.Length; i++)
                {
                    var column = GridUtils.grabColumnByUidOrField(action.FrozenColumnsUidCollection[i], _parent);
                    if (column != null)
                    {
                        newFrozenColumns.Add(column);
                    }
                }
            }
            if (action?.FrozenColumnsUidCollection?.Length > 0)
            {
                if (_parent.SelectedRecords?.Count > 0 && _parent.SelectionSettings != null && !_parent.SelectionSettings.PersistSelection && _parent.SelectionModule != null)
                {
                    await _parent.SelectionModule.ClearRowSelection().ConfigureAwait(true);
                }
                if (_parent.FrozenColumns > 0)
                {
                    if (action.IsFrozen)
                    {
                        action.FrozenColumnsCount = _parent.FrozenColumns + action.FrozenColumnsUidCollection.Length;
                    }
                    else
                    {
                        action.FrozenColumnsCount = _parent.FrozenColumns - action.FrozenColumnsUidCollection.Length;
                    }
                    await _parent.UpdateChildProperty(nameof(_parent.FrozenColumns), action.FrozenColumnsCount).ConfigureAwait(true);
                }
                else
                {
                    foreach (GridColumn col in newFrozenColumns)
                    {
                        col.SetIsFrozen(action.IsFrozen);
                        col.SetFreeze(freezeDirection);
                    }
                }
                _parent.ForceUpdate = true;
                _parent.IsFreezeLineMoved = true;
                if (_parent.EnableVirtualization && _parent.VirtualScrollModule != null)
                {
                    await _parent.VirtualScrollModule.CheckAndResetCache("FreezeLineReorder").ConfigureAwait(true);
                    if (_parent.EnableColumnVirtualization)
                    {
                        _parent.VirtualScrollModule.EndColumnIndex = _parent.VirtualScrollModule.EndColumnIndex - _parent.VirtualScrollModule.StartColumnIndex;
                        _parent.VirtualScrollModule.StartColumnIndex = 0;
                    }
                }
                var batchChanges = await _parent.GetBatchChangesAsync().ConfigureAwait(true);
                if (batchChanges.ChangedRecords.Count > 0 && _parent.IsFreezeLineMoved)
                {
                    await _parent.EditModule!.BatchSave().ConfigureAwait(true);
                    await _parent.EditModule.EndEdit().ConfigureAwait(true);
                }
                if (_parent.IsFreezeLineMoved && _parent.IsEdit)
                {
                    await _parent.EditModule!.EndEdit().ConfigureAwait(true);
                }
                await _parent.CallStateHasChangedAsync().ConfigureAwait(true);
                _parent.IsColumnHeaderChange = false;
                if (action?.hasGridStructureChanges == true)
                {
                    await _parent.ClientRefresh().ConfigureAwait(true);
                }
                else
                {
                    await _parent.InvokeMethod("sfBlazor.Grid.freezeLineMovedActions", new object[] { _parent.DataId, GetFreezeLineClientOptions() }).ConfigureAwait(true);
                }
                _parent.IsFreezeLineMoved = false;
            }
            if (_parent.GridEvents?.FreezeLineMoved.HasDelegate == true || _parent.IsRenderedFromTreeGrid)
            {
                FreezeLineMovedEventArgs eventArgs = new FreezeLineMovedEventArgs()
                {
                    StartIndex = action!.FromIndex,
                    EndIndex = action.ToIndex,
                    Direction = (action?.FreezeLineMovingDirection == "Left") ? FreezeDirection.Left : FreezeDirection.Right,
                    FrozenColumns = _parent.Columns!.Where(col => col.IsFrozen).ToList()
                };
                if (_parent.IsRenderedFromTreeGrid)
                    await _parent.EventAggregator.NotifyAsync("FreezeLineMoved", eventArgs).ConfigureAwait(true);
                else
                    await (_parent.GridEvents?.FreezeLineMoved.InvokeAsync(eventArgs))!.ConfigureAwait(true)!;
            }
        }

        internal async Task InvokeClientFrozenHeight()
        {
            if (GetFrozenCount() > 0 && (_parent.Width == "100%" || (_parent.AllowTextWrap && _parent.IsStackedHeader)))
            {
                await _parent.InvokeMethod("sfBlazor.Grid.frozenHeight", new object[] { _parent.DataId, _parent.GetClientOption(), null! }).ConfigureAwait(true);
            }
        }
        #endregion

        #region Column State Detection Utilites

        internal static bool IsFrozenColumnPresent(List<GridColumn> columns)
        {
            foreach (var column in columns)
            {
                var subColumns = column.Columns;
                if (subColumns == null || subColumns.Count == 0)
                {
                    if (column.IsFrozen || column.FixedColumn)
                    {
                        return true;
                    }
                }
                else if (IsFrozenColumnPresent(subColumns))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsLeftFreezeColumn(GridColumn Column)
        {
            var Flag = false;
            if ((Column?.FrozenMovableLabel ?? "").Contains("FrozenLeft", StringComparison.CurrentCulture))
            {
                Flag = true;
            }
            else if (Column?.Columns != null && Column?.Columns.Count > 0)
            {
                for (int i = 0; i < Column?.Columns.Count; i++)
                {
                    if (Column?.Columns.Count > 0)
                    {
                        Flag = IsLeftFreezeColumn(Column?.Columns[i]!);
                        if (Flag)
                            break;
                    }
                }
            }
            return Flag;
        }

        internal static bool IsRightFreezeColumn(GridColumn Column)
        {
            var Flag = false;
            if ((Column?.FrozenMovableLabel ?? "").Contains("FrozenRight", StringComparison.CurrentCulture))
            {
                Flag = true;
            }
            else if (Column?.Columns != null && Column?.Columns.Count > 0)
            {
                for (int i = 0; i < Column?.Columns.Count; i++)
                {
                    if (Column?.Columns.Count > 0)
                    {
                        Flag = IsRightFreezeColumn(Column?.Columns[i]!);
                        if (Flag)
                            break;
                    }
                }
            }
            return Flag;
        }

        internal static bool IsLastColumn(GridColumn Column)
        {
            var Flag = false;
            if (Column.FrozenMovableLabel != null && Column.FrozenMovableLabel.Contains("FrozenLeftLast", StringComparison.CurrentCulture))
            {
                Flag = true;
            }
            else if (Column.FrozenMovableLabel == null && Column.Columns != null && Column.Columns.Count > 0)
            {
                for (int i = 0; i < Column.Columns.Count; i++)
                {
                    if (Column.Columns.Count > 0)
                    {
                        Flag = IsLastColumn(Column.Columns[i]);
                    }
                }
            }
            return Flag;
        }

        internal static bool IsFirstColumn(GridColumn Column)
        {
            var Flag = false;
            if (Column.FrozenMovableLabel != null && Column.FrozenMovableLabel.Contains("FrozenRightFirst", StringComparison.CurrentCulture))
            {
                Flag = true;
            }
            else if (Column.FrozenMovableLabel == null && Column.Columns != null && Column.Columns.Count > 0)
            {
                for (int i = 0; i < Column.Columns.Count; i++)
                {
                    if (Column.Columns.Count > 0)
                    {
                        Flag = IsFirstColumn(Column.Columns[i]);
                        if (Flag)
                            break;
                    }
                }
            }
            return Flag;
        }

        internal static bool IsFrozenColumn(GridColumn column)
        {
            var Frozen = false;
            if (column.FrozenMovableLabel != null && !column.FrozenMovableLabel.Contains("Movable", StringComparison.CurrentCulture))
            {
                Frozen = true;
            }
            else if (column.FrozenMovableLabel == null && column.Columns != null)
            {
                for (int i = 0; i < column.Columns.Count; i++)
                {
                    Frozen = IsFrozenColumn(column.Columns[i]);
                }
            }
            return Frozen;
        }

        #endregion
    }
}
