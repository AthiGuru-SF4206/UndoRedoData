using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles Reordering operation for column reorder scenarios (drag-drop, column menu, column chooser, APIs).
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class Reorder<T>
    {
        #region Fields & Properties

        private SfGrid<T> Parent;

        public Reorder(SfGrid<T> parent) => Parent = parent;

        /// <summary>
        /// Tracks whether columns were reordered in current operation.
        /// </summary>
        internal bool IsColumnReordered { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs the column reorder operation on the grid.
        /// Executes single or multiple column reordering based on ActionArgs, rebuilds indices, and updates freeze direction tracking.
        /// </summary>
        /// <param name="action">ActionArgs containing reorder source, destination, and operation type</param>
        public async Task PerformReorder(ActionArgs action)
        {
            // Execute reorder operation
            if (action.IsMultipleReorder)
            {
                ReorderColumns(action.FromColumnUid!, action.ToColumnUid!);
            }
            else
            {
                ReorderColumn((int)action.ToIndex, (int)action.FromIndex, action?.ToColumnUid!);
            }

            // Rebuild column indices unless persistence is enabled
            if (!Parent.EnablePersistence)
            {
                RebuildColumnIndices();
            }

            IsColumnReordered = true;

            // Update freeze direction tracking if using FreezeDirection (not FrozenColumns)
            UpdateFreezeDirectionIfNeeded();
        }

        /// <summary>
        /// Updates the HasFreezeDirection flag if FreezeDirection mode is in use.
        /// This distinguishes between traditional frozen columns and dynamic freeze direction mode.
        /// </summary>
        private void UpdateFreezeDirectionIfNeeded()
        {
            if (Parent.FrozenColumnModel != null 
                && Parent.FrozenColumns == 0 
                && Parent.FrozenRows == 0)
            {
                Parent.HasFreezeDirection = true;
            }
        }

        /// <summary>
        /// Rebuilds column indices after reorder, accounting for frozen column rearrangement.
        /// </summary>
        private void RebuildColumnIndices()
        {
            Parent.ColumnIndex = -1;

            // If using FreezeDirection (GetFrozenCount > 0) with no traditional frozen columns,
            // rearrange columns to group frozen sections before indexing
            if (Parent.FrozenColumns == 0 && Parent.FreezeModule!.GetFrozenCount() > 0)
            {
                Parent.SetColumnIndex(Parent.RearrangeColumns(Parent.Columns!), false);
            }
            else
            {
                Parent.SetColumnIndex(Parent.Columns!, false);
            }
        }

        /// <summary>
        /// Reorders a single column by index, with optional scope resolution for stacked columns.
        /// </summary>
        /// <param name="fromIndex">Source column index</param>
        /// <param name="toIndex">Destination column index</param>
        /// <param name="uid">Column UID for scope resolution (used for stacked headers)</param>
        public void ReorderColumn(int fromIndex, int toIndex, string? uid = null)
        {
            List<GridColumn> scopedColumns = (List<GridColumn>)Parent.Columns!;
            if (!string.IsNullOrEmpty(uid) && scopedColumns != null)
            {
                scopedColumns = Reorder<T>.GetScopeRecursively(uid, (List<GridColumn>)Parent.Columns!);
                ReorderFrozenColumns(scopedColumns);
            }

            Swap(scopedColumns!, fromIndex, toIndex);
        }

        #endregion

        #region Frozen Column Helpers

        /// <summary>
        /// Consolidates frozen columns to their designated positions (left/right).
        /// Ensures frozen columns stay within their freeze zones during reorder operations.
        /// </summary>
        /// <param name="parentColumns">Column collection to organize</param>
        internal static void ReorderFrozenColumns(List<GridColumn> parentColumns)
        {
            // Process frozen left columns - move to beginning
            AdvanceFrozenColumns(parentColumns, FreezeDirection.Left, isRightFreeze: false);

            // Process frozen right columns - move to end
            AdvanceFrozenColumns(parentColumns, FreezeDirection.Right, isRightFreeze: true);
        }

        /// <summary>
        /// Consolidates frozen columns to their designated positions (left/right).
        /// Moves all columns with matching freeze direction to start or end of collection.
        /// </summary>
        /// <param name="parentColumns">Column collection to organize</param>
        /// <param name="freezeDirection">Target freeze direction (Left/Right)</param>
        /// <param name="isRightFreeze">If true, moves to end; if false, moves to beginning</param>
        private static void AdvanceFrozenColumns(List<GridColumn> parentColumns, FreezeDirection freezeDirection, bool isRightFreeze)
        {
            List<int> frozenIndices = CollectFrozenIndices(parentColumns, freezeDirection);
            if (frozenIndices.Count == 0) return;

            // Consolidate frozen columns to their designated zone (left or right)
            MoveFrozenColumnsToZone(parentColumns, frozenIndices, isRightFreeze);
        }

        /// <summary>
        /// Moves frozen columns to their designated zone (left/right) by consolidating indices.
        /// </summary>
        /// <param name="parentColumns">Column collection to reorganize</param>
        /// <param name="frozenIndices">Indices of frozen columns to move</param>
        /// <param name="moveToRight">If true, moves to end; if false, moves to beginning</param>
        private static void MoveFrozenColumnsToZone(List<GridColumn> parentColumns, List<int> frozenIndices, bool moveToRight)
        {
            if (moveToRight)
            {
                // Right frozen: move from end toward beginning (reverse order)
                int targetPosition = parentColumns.Count - 1;
                for (int i = frozenIndices.Count - 1; i >= 0; i--)
                {
                    Swap(parentColumns, frozenIndices[i], targetPosition, frozenSwap: true);
                    targetPosition--;
                }
            }
            else
            {
                // Left frozen: move from beginning forward
                int targetPosition = 0;
                for (int i = 0; i < frozenIndices.Count; i++)
                {
                    Swap(parentColumns, frozenIndices[i], targetPosition, frozenSwap: true);
                    targetPosition++;
                }
            }
        }

        /// <summary>
        /// Collects indices of all columns with specified freeze direction.
        /// </summary>
        /// <param name="parentColumns">Column collection to search</param>
        /// <param name="freezeDirection">Freeze direction to match (Left/Right)</param>
        /// <returns>List of column indices matching freeze direction</returns>
        private static List<int> CollectFrozenIndices(List<GridColumn> parentColumns, FreezeDirection freezeDirection)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < parentColumns.Count; i++)
            {
                var col = parentColumns[i];
                if (col?.IsFrozen == true && col.Freeze == freezeDirection)
                {
                    indices.Add(i);
                }
            }
            return indices;
        }

        #endregion

        #region Public Methods (Column Reordering - Multi)

        /// <summary>
        /// Reorders multiple columns to a target position by their field names.
        /// </summary>
        /// <param name="fromNames">Source column field names (array)</param>
        /// <param name="toName">Destination column field name</param>
        public void ReorderColumns(string[] fromNames, string toName)
        {
            List<GridColumn> ToScopedColumns = Reorder<T>.GetScopeRecursively(toName, (List<GridColumn>)Parent.Columns!);
            int fromIndex, toIndex = ToScopedColumns.FindIndex(col => col.Uid == toName);
            for (int i = 0; i < fromNames.Length; i++)
            {
                var name = fromNames[i];
                List<GridColumn> FromScopedColumns = Reorder<T>.GetScopeRecursively(name, (List<GridColumn>)Parent.Columns!);
                if (FromScopedColumns == ToScopedColumns)
                {
                    fromIndex = FromScopedColumns.FindIndex(col => col?.Uid == name);
                    Swap(ToScopedColumns, fromIndex, toIndex);
                    var tmp = i + 1 > fromNames.Length - 1 ? -1 : i + 1;
                    if (tmp != -1 && ToScopedColumns.FindIndex(col => col?.Uid == fromNames[tmp]) >= toIndex)
                    {
                        toIndex++;
                    }
                }
            }
        }

        #endregion

        #region Internal methods such as JS Interop
        public async ValueTask ColumnReordered(object? args)
        {
            ActionArgs? action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!);

            if (action?.RequestType == "reorder")
            {
                if (!Parent.HideGridSpinner)
                {
                    await Parent.ShowSpinnerAsync().ConfigureAwait(true);
                    await Task.Delay(1).ConfigureAwait(true);
                }
                List<GridColumn> columns = GridUtils.GetColumns(Parent);
                if (!string.IsNullOrEmpty(action.ToColumnUid))
                {
                    columns = GetScopeRecursively(action.ToColumnUid, (List<GridColumn>)Parent.Columns!);
                }
                var ar = new ActionEventArgs<T>()
                {
                    RequestType = Action.Reorder,
                    Cancel = false,
                    FromColumns = action.IsMultipleReorder ? new List<GridColumn>() : new List<GridColumn>() { columns[(int)action.ToIndex] },
                    ToColumn = action.IsMultipleReorder ? null! : columns[(int)action.FromIndex]
                };
                var reorderEventArgs = new ColumnReorderingEventArgs()
                {
                    Cancel = false,
                    ReorderingColumns = action.IsMultipleReorder ? new List<GridColumn>() : new List<GridColumn>() { columns[(int)action.ToIndex] },
                    ToColumn = action.IsMultipleReorder ? null! : columns[(int)action.FromIndex]
                };
                if (action.IsMultipleReorder)
                {
                    action.FromColumnUid?.ToList().ForEach(uid =>
                    {
                        var column = columns.Find(x => x.Uid == uid);
                        if (column != null)
                        {
                            ar.FromColumns.Add(column);
                            reorderEventArgs.ReorderingColumns.Add(column);
                        }
                    });
                    ar.ToColumn = reorderEventArgs.ToColumn = columns.Find(_ => _.Uid == action.ToColumnUid)!;
                }

                Parent.SoftRefresh = false;
                Parent.IsColumnHeaderChange = true;
                await Parent.ModelChanged(ar, additionalArgs: action, eventArgs: reorderEventArgs, requestType: "Reorder").ConfigureAwait(true);
                Parent.IsColumnHeaderChange = false;
                if (ar.Cancel || reorderEventArgs.Cancel)
                {
                    await Parent.HideSpinnerAsync().ConfigureAwait(true);
                    return;
                }
                await SfBaseUtils.InvokeEvent<ActionEventArgs<T>>(Parent.GridEvents?.OnActionComplete, ar).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("ActionComplete", ar).ConfigureAwait(true);
                var reoderedArgs = new ColumnReorderedEventArgs()
                {
                    ReorderingColumns = reorderEventArgs.ReorderingColumns,
                    ToColumn = reorderEventArgs.ToColumn!
                };
                await SfBaseUtils.InvokeEvent<ColumnReorderedEventArgs>(Parent.GridEvents?.ColumnReordered, reoderedArgs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("ColumnReordered", reorderEventArgs).ConfigureAwait(true);
                await Parent.InvokeSuccessAsync(ar, requestType: "Reorder").ConfigureAwait(true);
            }
        }

        #endregion
        #region Static Utility Methods

        /// <summary>
        /// Swaps two columns in the collection.
        /// When not a frozen swap, transfers freeze properties from destination to source.
        /// </summary>
        /// <param name="scopedColumns">Column collection</param>
        /// <param name="fromIndex">Source index</param>
        /// <param name="toIndex">Destination index</param>
        /// <param name="frozenSwap">If true, preserves freeze properties; if false, copies destination freeze state</param>
        internal static void Swap(List<GridColumn> scopedColumns, int fromIndex, int toIndex, bool frozenSwap = false)
        {
            var columns = scopedColumns;
            var toTmp = columns[fromIndex];

            // When swapping non-frozen columns, adopt freeze properties of destination column
            if (!frozenSwap)
            {
                toTmp.SetFreeze(columns[toIndex].Freeze);
                toTmp.SetIsFrozen(columns[toIndex].IsFrozen);
            }

            columns.RemoveAt(fromIndex);
            columns.Insert(toIndex, toTmp);
        }

        /// <summary>
        /// Recursively finds the scope (parent collection) containing a column by UID.
        /// Handles nested/stacked columns by traversing the hierarchy.
        /// </summary>
        /// <param name="uid">Column UID to locate</param>
        /// <param name="parent">Root column collection to search</param>
        /// <returns>The scoped collection containing the column, or empty list if not found</returns>
        internal static List<GridColumn> GetScopeRecursively(string uid, List<GridColumn> parent)
        {
            if (parent.Any(c => c.Uid == uid))
            {
                return parent;
            }

            List<GridColumn>? result = null;
            parent.ForEach((col) =>
            {
                if (result == null && col?.Columns != null)
                {
                    result = Reorder<T>.GetScopeRecursively(uid, (List<GridColumn>)col.Columns);
                }
            });

            return result!;
        }

        #endregion
    }
}