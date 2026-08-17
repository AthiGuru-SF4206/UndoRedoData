using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles sort action.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class Sort<T>
    {
        #region Private Fields

        private SfGrid<T> Parent { get; set; }

        #endregion

        #region Public Properties

        public List<string> SortedColumns = new List<string>();

        public List<GridSortColumn> LastSortedCols = new List<GridSortColumn>();

        public Nullable<bool> MultiSort = true;

        #endregion

        #region Constructor

        public Sort(SfGrid<T> parent) => Parent = parent;

        #endregion

        #region Event Handling

        internal async Task SortClickHandler(GridColumn Column, string Class, MouseEventArgs args)
        {
            if (Column != null && Column.Type != ColumnType.CheckBox && Parent.AllowSorting)
            {
                await InitiateSort(Column, Class, args).ConfigureAwait(true);
            }
        }

        internal async Task HandleSortKeyboardAction(string key, string column, SortDirection sortDirection)
        {
            if (key?.Equals("Enter", StringComparison.Ordinal) == true && Parent != null && Parent.AllowSorting)
            {
                await SortColumn(column, sortDirection, true).ConfigureAwait(true);
            }
        }

        internal async Task InitiateSort(GridColumn Column, string Class, MouseEventArgs e)
        {
            var Direction = !Class?.Contains("e-ascending", StringComparison.Ordinal) == true ? SortDirection.Ascending : SortDirection.Descending;
            var GCols = Parent.AllowGrouping? Parent.GroupSettings!.Columns?.ToList() ?? new List<string>() : new List<string>();
            if (e.ShiftKey || (Parent.SortSettings!.AllowUnsort && Class?.Contains("e-descending", StringComparison.Ordinal) == true) && 
                (!GCols.Contains(Column?.Field!)))
            {
                await RemoveSortColumn(Column?.Field!, isCtrlKeyPressed: e.CtrlKey || ((Parent.IsMacDevice ?? false) && e.MetaKey) || (Parent.SyncfusionService.IsDeviceMode && Parent.AllowMultiSorting)).ConfigureAwait(true);
            }
            else
            {
                await SortColumn(Column?.Field!, Direction, e?.CtrlKey == true || ((Parent.IsMacDevice ?? false) && (e != null && e.MetaKey)) || (Parent.SyncfusionService.IsDeviceMode && Parent.AllowMultiSorting)).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handles sort action by either removing the sort column or updating the sort model.
        /// </summary>
        /// <param name="columnName">The field name of the column to sort.</param>
        /// <param name="direction">The sort direction to apply.</param>
        internal void HandleSortAction(string columnName, SortDirection direction)
        {
            if (direction == SortDirection.None)
            {
                Parent.SortSettings?.Columns?.Remove(Parent.SortSettings.Columns.Where(col => col.Field == columnName).FirstOrDefault()!);
            }
            else
            {
                UpdateModel(columnName, direction);
            }
        }

        #endregion

        #region Core Sorting Operations

        internal async Task SortColumn(string ColumnName, SortDirection Direction, bool? IsMultiSort = false, bool multipleCols = false, bool invokedByMethod = false)
        {
            Parent.IsColumnHeaderChange = true;
            var Column = GridUtils.GetColumnByField(ColumnName, GridUtils.GetColumns(Parent));
            if (Column == null || !Parent.AllowSorting || !Column.AllowSorting) // TODO: Handle iscontextMenuOpen()
            {
                return;
            }

            IsMultiSort ??= false;
            if (!Parent.AllowMultiSorting)
            {
                IsMultiSort = Parent.AllowMultiSorting;
            }

            MultiSort = IsMultiSort;

            NotifyCollectionChangedAction action = new NotifyCollectionChangedAction();
            var sortedColumns = Parent.SortSettings!.Columns;
            if ((sortedColumns == null || sortedColumns.Count == 0) && !invokedByMethod)
            {
                action = Direction.Equals(SortDirection.Ascending) ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Replace;
            }
            else if (invokedByMethod && Direction == SortDirection.None)
            {
                action = NotifyCollectionChangedAction.Remove;
            }
            else if (invokedByMethod)
            {
                action = (sortedColumns == null || sortedColumns.Count == 0) ? NotifyCollectionChangedAction.Add : sortedColumns.Where(e => e.Field == ColumnName).Any() ? NotifyCollectionChangedAction.Replace : NotifyCollectionChangedAction.Add;
            }
            else if (sortedColumns != null && sortedColumns.Count != 0)
            {
                action = sortedColumns.Where(e => e.Field == ColumnName).Any() ? NotifyCollectionChangedAction.Replace : NotifyCollectionChangedAction.Add;
            }
            UpdateModel(ColumnName, Direction);
            if (multipleCols)
            {
                return;
            }

            await Parent.SelectionModule!.ClearSelectionOnSort().ConfigureAwait(true);

            await Parent.ModelChanged(new ActionEventArgs<T>()
            {
                RequestType = Action.Sorting,
                ColumnName = ColumnName,
                Direction = Direction,
                Cancel = false
            }, eventArgs: new SortingEventArgs() { Action = action, ColumnName = ColumnName, Direction = Direction, Cancel = false, IsCtrlKeyPressed = IsMultiSort ?? false }, requestType: "Sorting").ConfigureAwait(true);
            Parent.IsColumnHeaderChange = false;
        }

        public async Task RemoveSortColumn(string Field, bool multipleCols = false, bool isCtrlKeyPressed = false)
        {
            Parent.IsColumnHeaderChange = true;
            LastSortedCols = Parent.SortSettings!.Columns?.Clone()!;
            var colToRemove = Parent.SortSettings.Columns?.Where(col => col.Field == Field).FirstOrDefault();
            if (colToRemove != null)
            {
                Parent.SortSettings.Columns?.Remove(colToRemove);
            }
            if (multipleCols)
            {
                return;
            }

            await Parent.SelectionModule!.ClearSelectionOnSort().ConfigureAwait(true);

            await Parent.ModelChanged(new ActionEventArgs<T>() { RequestType = Action.Sorting, ColumnName = Field, Direction = SortDirection.None }, eventArgs: new SortingEventArgs() { Action = NotifyCollectionChangedAction.Remove, IsCtrlKeyPressed = isCtrlKeyPressed, ColumnName = Field, Direction = SortDirection.None, Cancel = false }, requestType: "Sorting").ConfigureAwait(true);
            Parent.IsColumnHeaderChange = false;
        }

        /// <summary>
        /// Processes multiple columns sorting by applying sort or remove sort based on direction.
        /// </summary>
        /// <param name="columns">List of columns to be sorted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task ProcessColumnsSorting(List<SortColumn> columns)
        {
            if (columns != null)
            {
                foreach (var col in columns)
                {
                    if (col.Direction == SortDirection.None)
                    {
                        await RemoveSortColumn(col.Field!, multipleCols: true).ConfigureAwait(true);
                    }
                    else
                    {
                        await SortColumn(col.Field!, col.Direction, true, true).ConfigureAwait(true);
                    }
                }
            }
        }

        internal void UpdateModel(string columnName, SortDirection direction)
        {
            using var SortedColumn = new GridSortColumn()
            {
#pragma warning disable BL0005
                Field = columnName,
                Direction = direction
#pragma warning restore BL0005
            };
            var GCols = Parent.AllowGrouping ? Parent.GroupSettings!.Columns?.ToList() : new List<string>();
            var SortedCols = new List<GridSortColumn>();
            LastSortedCols = Parent.SortSettings!.Columns?.Clone()!;
            if (MultiSort != true)
            {
                if (GCols == null)
                {
                    SortedCols = new List<GridSortColumn> { SortedColumn };
                }
                else
                {
                    var flag = false;
                    for (var i = 0; i < GCols.Count; i++)
                    {
                        if (columnName == GCols[i])
                        {
                            flag = true;
                            SortedCols.Add(SortedColumn);
                        }
                        else
                        {
                            var SCol = Parent.SortSettings.Columns?.Find(col => col.Field == GCols[i]);
                            if (SCol != null)
                            {
                                SortedCols.Add(SCol);
                            }
                        }
                    }

                    if (!flag)
                    {
                        SortedCols.Add(SortedColumn);
                    }
                }
            }
            else
            {
                SortedCols = Parent.SortSettings.Columns ?? new List<GridSortColumn> { };
                var RemoveCol = Parent.SortSettings.Columns?.Where(col => col.Field == columnName).FirstOrDefault();
                if (RemoveCol != null)
                {
                    SortedCols.Remove(RemoveCol);
                }
                SortedCols.Add(SortedColumn);
            }

            if (SortedColumns.IndexOf(columnName) == -1 && GCols != null && GCols.IndexOf(columnName) > -1)
            {
                SortedColumns.Add(columnName);
            }

            Parent.SortSettings.UpdateColumns("Columns", SortedCols);
        }

        #endregion

        #region Group Sorting

        internal void GroupAddSortingQuery(string ColName)
        {
            var sortedColumns = Parent.SortSettings!.Columns ?? new List<GridSortColumn>();
            if (!sortedColumns.Any(col => col.Field == ColName))
            {
                sortedColumns.Add(new GridSortColumn()
                {
#pragma warning disable BL0005
                    Field = ColName,
                    Direction = SortDirection.Ascending,
                    IsFromGroup = true
#pragma warning restore BL0005
                });
                Parent.SortSettings.UpdateColumns("Columns", sortedColumns);
            }
            else if (!Parent.AllowSorting)
            {
#pragma warning disable BL0005
                Parent.SortSettings.Columns!.Find(col => col.Field == ColName)!.Direction = SortDirection.Ascending;
#pragma warning restore BL0005
            }
        }

        public void SortQuery(Query query, Row<object> Row)
        {
            List<GridSortColumn> columns = Parent.SortSettings!.Columns?.ToList() ?? new List<GridSortColumn>();
            List<GridSortColumn> groupCols = new List<GridSortColumn>();
            var groupedColumns = Parent.GroupSettings!.Columns?.ToList();
            var level = Parent.GroupSettings.Columns!.IndexOf((Row.Data as Group<T>)?.Field) + 1;
            if (groupedColumns?.Count > 0 && Parent.GroupSettings.Columns?.Length != level)
            {
                for (var i = level; i < Parent.GroupSettings.Columns?.Length; i++)
                {
                    if (!columns.Any(col => col.Field == groupedColumns[i]))
                    {
#pragma warning disable BL0005
                        groupCols.Add(new GridSortColumn() { Field = groupedColumns[i] });
#pragma warning restore BL0005
                    }
                    else
                    {
                        groupCols = columns.Where(col => col.Field == groupedColumns[i]).ToList();
                    }
                }
            }
            var count = groupCols.Count;
            if ((Parent.AllowSorting || Parent.AllowGrouping) && count != 0)
            {
                List<GridSortColumn> cols = new List<GridSortColumn>();
                for (var i = count - 1; i > -1; i--)
                {
                    var field = groupCols[i].Field;
                    var dir = groupCols[i].Direction;
                    if (groupedColumns?.Contains(field) == true)
                    {
#pragma warning disable BL0005
                        cols.Add(new GridSortColumn() { Field = field, Direction = dir });
#pragma warning restore BL0005
                    }
                    else
                    {
                        query.Sort(field, dir.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture));
                    }
                }
                cols.ForEach(sorts => query.Sort(sorts.Field, sorts.Direction.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture)));
            }
        }

        internal void InitialGroupSort(object args)
        {
            args = null!;
            if (Parent.AllowGrouping)
            {
                Parent.EnsureFeaturesCompatibility();
#pragma warning disable BL0005
                Parent.SortSettings!.Columns ??= new List<GridSortColumn>();
#pragma warning restore BL0005
                var GCols = Parent.GroupSettings!.Columns?.ToList();
                if (GCols?.Count > 0)
                {
                    for (var i = 0; i < GCols.Count; i++)
                    {
                        var Column = GridUtils.GetColumnByField(GCols[i], GridUtils.GetColumns(Parent));
                        if (Column == null) continue;
                        var columnsVisibility = Column.directParamKeys.Contains("Visible");
                        if (Column.Visible && !Parent.GroupSettings.ShowGroupedColumn && !columnsVisibility)
                        {
                            Column.IsHiddenByGrouping = true;
                        }
                        Column.SetVisibility(Parent.GroupSettings.ShowGroupedColumn);
                        if (!Parent.SortSettings.Columns.Any(Col => Col.Field == GCols[i]))
                        {
                            Parent.SortSettings.Columns.Add(new GridSortColumn()
                            {
#pragma warning disable BL0005
                                Field = GCols[i],
                                Direction = SortDirection.Ascending,
                                IsFromGroup = true
                            });
#pragma warning restore BL0005
                        }
                        else if (!Parent.SortSettings.Columns.Any(Col => Col.IsFromGroup))
                        {
                            Parent.AddSortColumn(GCols[i]);
                        }
                    }
                }
            }
        }

        #endregion

        #region Clear Sorting Operations

        /// <summary>
        /// Clears all sorted columns except grouped columns.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task ClearSortAsync()
        {
            var sortedCols = Parent.SortSettings!.Columns?.Clone();
            var gCols = Parent.GroupSettings!.Columns?.ToList() ?? new List<string>();
            for (var i = 0; i < sortedCols?.Count; i++)
            {
                if (!gCols.Any(field => field == sortedCols[i].Field))
                {
                    await RemoveSortColumn(sortedCols[i].Field, true).ConfigureAwait(true);
                }
            }
            await Parent.ModelChanged(new ActionEventArgs<T>() { RequestType = Action.Sorting }, eventArgs: new SortingEventArgs() { Action = NotifyCollectionChangedAction.Reset, Direction = SortDirection.None }, requestType: "Sorting").ConfigureAwait(true); 
        }

        /// <summary>
        /// Clears specified sorted columns by field names, except grouped columns.
        /// </summary>
        /// <param name="fieldNames">List of field names to clear from sorting.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task ClearSortAsync(List<string> fieldNames)
        {
            var gCols = Parent.GroupSettings!.Columns?.ToList() ?? new List<string>();
            if (fieldNames != null)
            {
                foreach (var field in fieldNames)
                {
                    if (!gCols.Any(fieldName => fieldName == field))
                    {
                        await RemoveSortColumn(field, true).ConfigureAwait(true);
                    }
                }
            }

            await Parent.ModelChanged(new ActionEventArgs<T>() { RequestType = Action.Sorting }).ConfigureAwait(true);
        }
        
        #endregion


        #region UI Helper Methods
        /// <summary>
        /// Gets the sort direction for a specific column.
        /// </summary>
        /// <param name="fieldName">The field name of the column.</param>
        /// <returns>The sort direction of the column.</returns>
        internal SortDirection GetColumnSortDirection(string fieldName)
        {
            var sortCol = Parent.SortSettings?.Columns?.FirstOrDefault(col => col.Field == fieldName);
            return sortCol?.Direction ?? SortDirection.None;
        }

        /// <summary>
        /// Gets the CSS class for the sort icon based on the column's sort direction.
        /// </summary>
        /// <param name="fieldName">The field name of the column.</param>
        /// <returns>The CSS class string for the sort icon.</returns>
        internal string GetSortIconClass(string fieldName)
        {
            var direction = GetColumnSortDirection(fieldName);
            return direction switch
            {
                SortDirection.Ascending => "e-ascending e-icon-ascending",
                SortDirection.Descending => "e-descending e-icon-descending",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Gets the ARIA label for the sort direction of a specific column.
        /// </summary>
        /// <param name="fieldName">The field name of the column.</param>
        /// <returns>The ARIA label string for accessibility.</returns>
        internal string GetSortAriaLabel(string fieldName)
        {
            var direction = GetColumnSortDirection(fieldName);
            return direction switch
            {
                SortDirection.Ascending => "ascending",
                SortDirection.Descending => "descending",
                _ => "none"
            };
        }

        /// <summary>
        /// Handles the sorting event by updating the sort state based on the event arguments.
        /// </summary>
        /// <param name="sortingEvent">The sorting event arguments containing sort information.</param>
        internal void HandleSortingEvent(SortingEventArgs sortingEvent)
        {
            if (sortingEvent.Direction == SortDirection.None && Parent.SortSettings != null)
            {
                var colToRemove = Parent.SortSettings.Columns?.FirstOrDefault(col => col.Field == sortingEvent.ColumnName);
                if (colToRemove != null)
                {
                    Parent.SortSettings.Columns?.Remove(colToRemove);
                }
            }
            else
            {
                if (sortingEvent.SortedColumns != null)
                {
                    foreach (var col in sortingEvent.SortedColumns.Where(col => col.Direction != SortDirection.None))
                    {
                        UpdateModel(col.Field!, col.Direction);
                    }
                }
                else if (sortingEvent.ColumnName != null)
                {
                    UpdateModel(sortingEvent.ColumnName, sortingEvent.Direction);
                }
            }
        }

        /// <summary>
        /// Gets the sort direction string for a specific field by searching through sorted columns.
        /// </summary>
        /// <param name="fieldName">The field name of the column to search for.</param>
        /// <returns>The direction string (e.g., "Ascending", "Descending", "None") if found; otherwise "None".</returns>
        internal string GetSortDirectionStringByField(string fieldName)
        {
            var sortedColumns = Parent?.SortSettings?.Columns;
            if (sortedColumns != null && sortedColumns.Count > 0)
            {
                foreach (var col in sortedColumns)
                {
                    if (col?.Field?.Equals(fieldName, StringComparison.Ordinal) == true)
                    {
                        return col?.Direction.ToString() ?? "None";
                    }
                }
            }
            return "None";
        }

        /// <summary>
        /// Gets the next sort direction in the cycle: Ascending -> Descending -> None -> Ascending.
        /// </summary>
        /// <param name="currentDirection">The current sort direction string.</param>
        /// <returns>The next sort direction string in the cycle.</returns>
        internal static string GetNextSortDirection(string currentDirection)
        {
            return currentDirection switch
            {
                "Ascending" => "Descending",
                "Descending" => "None",
                _ => "Ascending"
            };
        }

        /// <summary>
        /// Determines if a sort menu item should be disabled based on the current sort state and column.
        /// </summary>
        /// <param name="targetColumn">The target column to evaluate for sorting.</param>
        /// <param name="sortDirectionItem">The sort direction menu item (e.g., "SortAscending", "SortDescending").</param>
        /// <returns>True if the menu item should be disabled; otherwise false.</returns>
        internal bool IsSortMenuItemDisabled(GridColumn? targetColumn, string sortDirectionItem)
        {
            string? columnField = targetColumn?.Field;
            if ((!Parent!.AllowSorting) || (targetColumn != null && columnField == null))
            {
                return true;
            }

            var sortColumns = Parent.SortSettings?.Columns;
            if (sortColumns == null || sortColumns.Count == 0)
            {
                return false;
            }
            var targetDirection = sortDirectionItem.Replace("sort", "", StringComparison.OrdinalIgnoreCase);
            foreach (var col in sortColumns)
            {
                if (col.Field == columnField && string.Equals(col.Direction.ToString(), targetDirection, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        internal int GetSortColumnIndex(string? fieldName)
        {
            if (Parent?.SortSettings?.Columns == null || string.IsNullOrEmpty(fieldName))
            {
                return -1;
            }
            return Parent.SortSettings.Columns.FindIndex(col => col.Field == fieldName);
        }
        #endregion
    }
}