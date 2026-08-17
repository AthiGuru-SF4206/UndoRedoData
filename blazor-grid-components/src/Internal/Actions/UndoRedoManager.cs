

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Manages undo and redo operations for grid edit actions.
    /// Maintains separate stacks for undo and redo with configurable size limits.
    /// Also handles applying undo/redo actions to the grid.
    /// </summary>
    /// <typeparam name="T">The type of data in the grid rows.</typeparam>
    public class UndoRedoManager<T>
    {
        private LinkedList<UndoRedoAction<T>> undoStack = new LinkedList<UndoRedoAction<T>>();
        private LinkedList<UndoRedoAction<T>> redoStack = new LinkedList<UndoRedoAction<T>>();
        private int maxStackSize = 20;
        private bool isEnabled = false;
        private int sequenceCounter = 0;
        private SfGrid<T>? Parent { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of actions to keep in the undo/redo stacks.
        /// Default value is 20. When the stack exceeds this limit, the oldest action is removed.
        /// </summary>
        public int MaxStackSize
        {
            get => maxStackSize;
            set => maxStackSize = Math.Max(1, value);
        }

        /// <summary>
        /// Gets a value indicating whether undo/redo is currently enabled.
        /// </summary>
        public bool IsEnabled => isEnabled;

        /// <summary>
        /// Gets the number of actions available in the undo stack.
        /// </summary>
        public int UndoCount => undoStack.Count;

        /// <summary>
        /// Gets the number of actions available in the redo stack.
        /// </summary>
        public int RedoCount => redoStack.Count;

        /// <summary>
        /// Gets a value indicating whether undo operation is available (undo stack not empty).
        /// </summary>
        public bool IsUndoAvailable => undoStack.Count > 0;

        /// <summary>
        /// Gets a value indicating whether redo operation is available (redo stack not empty).
        /// </summary>
        public bool IsRedoAvailable => redoStack.Count > 0;

        /// <summary>
        /// Initializes a new instance of the UndoRedoManager class.
        /// </summary>
        /// <param name="parent">The parent SfGrid instance for accessing grid operations (optional).</param>
        public UndoRedoManager(SfGrid<T>? parent = null)
        {
            Parent = parent;
        }

        #region Helper Methods for Recording Actions

        /// <summary>
        /// Validates if an undo/redo action should be recorded based on guard conditions.
        /// Centralizes all guard logic in one place for consistency and maintainability.
        /// </summary>
        /// <param name="expectedMode">The expected edit mode for validation. Defaults to Batch.</param>
        /// <returns>True if the action should be recorded, false otherwise.</returns>
        internal bool ShouldRecordUndoRedoAction(EditMode expectedMode = EditMode.Batch)
        {
            if (Parent?.EditSettings?.EnableUndoRedo != true)
                return false;

            if (expectedMode == EditMode.Batch && Parent.EditSettings?.Mode != EditMode.Batch)
                return false;

            if (isEnabled == false)
                return false;

            return true;
        }

        /// <summary>
        /// Records a cell edit action in the undo/redo stack.
        /// Encapsulates the logic for creating and recording cell edit actions.
        /// </summary>
        /// <param name="rowIndex">The index of the row containing the cell.</param>
        /// <param name="columnIndex">The index of the column being edited.</param>
        /// <param name="fieldName">The field name of the cell being edited.</param>
        /// <param name="oldValue">The previous value before editing.</param>
        /// <param name="newValue">The new value after editing.</param>
        /// <param name="column">The GridColumn object associated with the cell.</param>
        internal void RecordCellEditAction(int rowIndex, int columnIndex, string? fieldName, 
            object? oldValue, object? newValue, GridColumn? column)
        {
            if (!ShouldRecordUndoRedoAction(EditMode.Batch))
                return;

            var cellChange = new CellChange<T>
            {
                RowIndex = rowIndex,
                ColumnIndex = columnIndex,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                Column = column
            };

            var action = new UndoRedoAction<T>
            {
                ActionType = UndoRedoActionType.CellEdit,
                CellChange = cellChange
            };

            RecordAction(action);
            TriggerUndoRedoStackChanged();
        }

        /// <summary>
        /// Records a row addition action in the undo/redo stack.
        /// Encapsulates the logic for creating and recording row add actions.
        /// </summary>
        /// <param name="rowData">The data of the newly added row.</param>
        /// <param name="rowIndex">The index at which the row was added.</param>
        /// <param name="newRowPosition">The position where the new row should be added (Top/Bottom).</param>
        internal void RecordRowAddAction(T? rowData, int rowIndex, NewRowPosition newRowPosition)
        {
            if (!ShouldRecordUndoRedoAction())
                return;

            if (rowData == null)
                return;

            var action = new UndoRedoAction<T>
            {
                ActionType = UndoRedoActionType.RowAdd,
                RowData = rowData,
                RowIndex = rowIndex >= 0 ? rowIndex : -1,
                RowPosition = newRowPosition
            };

            RecordAction(action);
            TriggerUndoRedoStackChanged();
        }

        /// <summary>
        /// Records a row deletion action in the undo/redo stack.
        /// Encapsulates the logic for creating and recording row delete actions.
        /// </summary>
        /// <param name="rowData">The data of the deleted row (should contain edited data if available).</param>
        /// <param name="rowIndex">The index of the deleted row.</param>
        internal void RecordRowDeleteAction(T? rowData, int rowIndex)
        {
            if (!ShouldRecordUndoRedoAction())
                return;

            if (rowData == null)
                return;

            var action = new UndoRedoAction<T>
            {
                ActionType = UndoRedoActionType.RowDelete,
                RowData = rowData,
                RowIndex = rowIndex >= 0 ? rowIndex : -1
            };

            RecordAction(action);
            TriggerUndoRedoStackChanged();
        }

        /// <summary>
        /// Triggers the UndoRedoStackChanged event to notify subscribers of stack state changes.
        /// Centralizes event triggering logic for consistency.
        /// </summary>
        internal void TriggerUndoRedoStackChanged()
        {
            if (Parent?.EventAggregator != null)
            {
                Parent.EventAggregator.Trigger("UndoRedoStackChanged", null!);
            }
        }

        #endregion

        /// <summary>
        /// Records a new action to the undo stack.
        /// When a new action is recorded, the redo stack is cleared (standard undo/redo behavior).
        /// If the undo stack exceeds MaxStackSize, the oldest action is removed.
        /// </summary>
        /// <param name="action">The action to record.</param>
        public void RecordAction(UndoRedoAction<T> action)
        {
            if (!isEnabled || action == null)
            {
                return;
            }

            // Assign sequence number for debugging
            action.SequenceNumber = ++sequenceCounter;

            // Add action to the front of the undo stack
            undoStack.AddFirst(action);

           

            // Clear the redo stack when a new action is recorded
            // (standard undo/redo behavior - any new action invalidates redo history)
            if (redoStack.Count > 0)
            {
                ClearRedoStack();
              
            }

            // Enforce stack size limit by removing the oldest (last) action
            while (undoStack.Count > maxStackSize && undoStack.Last != null)
            {
                undoStack.RemoveLast();
              
            }
        }

        /// <summary>
        /// Performs an undo operation by reverting the most recent action.
        /// The undone action is moved to the redo stack for potential redo.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. Returns the undone action, or null if undo stack is empty.</returns>
        public async Task<UndoRedoAction<T>?> UndoAsync()
        {
            if (!isEnabled || undoStack.Count == 0)
            {
                return await Task.FromResult<UndoRedoAction<T>?>(null);
            }

            // Pop from undo stack
            var action = undoStack.First?.Value;
            if (action != null)
            {
                undoStack.RemoveFirst();

                // Move to redo stack
                redoStack.AddFirst(action);
                return await Task.FromResult(action);
            }

            return await Task.FromResult<UndoRedoAction<T>?>(null);
        }

        /// <summary>
        /// Performs a redo operation by reapplying the most recently undone action.
        /// The redone action is moved back to the undo stack.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. Returns the redone action, or null if redo stack is empty.</returns>
        public async Task<UndoRedoAction<T>?> RedoAsync()
        {
            if (!isEnabled || redoStack.Count == 0)
            {
               
                return await Task.FromResult<UndoRedoAction<T>?>(null);
            }

            // Pop from redo stack
            var action = redoStack.First?.Value;
            if (action != null)
            {
                redoStack.RemoveFirst();
                // Move to undo stack
                undoStack.AddFirst(action);
                return await Task.FromResult(action);
            }

            return await Task.FromResult<UndoRedoAction<T>?>(null);
        }

        /// <summary>
        /// Undoes all actions sequentially, moving them all from the undo stack to the redo stack.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UndoAllAsync()
        {
            if (!isEnabled)
            {
                return;
            }

            int count = 0;
            while (undoStack.Count > 0)
            {
                await UndoAsync();
                count++;
            }
        }

        /// <summary>
        /// Redoes all actions sequentially, moving them all from the redo stack to the undo stack.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RedoAllAsync()
        {
            if (!isEnabled)
            {
                return;
            }

            int count = 0;
            while (redoStack.Count > 0)
            {
                await RedoAsync();
                count++;
            }
        }

        /// <summary>
        /// Clears only the redo stack.
        /// This is typically called when batch editing is cancelled to discard redo history.
        /// </summary>
        public void ClearRedoStack()
        {
            redoStack.Clear();
           
        }

        /// <summary>
        /// Clears both the undo and redo stacks, resetting to a clean state.
        /// </summary>
        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            sequenceCounter = 0;
        }

        /// <summary>
        /// Enables undo/redo with an optional stack size limit.
        /// </summary>
        /// <param name="stackLimit">The maximum number of actions to keep in each stack. Defaults to 20.</param>
        public void Enable(int stackLimit = 20)
        {
            isEnabled = true;
            MaxStackSize = stackLimit;
            Clear();
          
        }

        /// <summary>
        /// Disables undo/redo and clears both stacks immediately.
        /// </summary>
        public void Disable()
        {
            isEnabled = false;
            Clear();
        }

        /// <summary>
        /// Updates the RowData of the most recent RowAdd action if it exists for the given row index.
        /// Used to keep newly added rows' undo data in sync as cells are edited.
        /// Implements EJ2 pattern: "If row already in undo stack, just update rowData"
        /// </summary>
        /// <param name="rowIndex">The index of the row to update</param>
        /// <param name="newRowData">The updated row data to store</param>
        /// <returns>True if an action was found and updated, false otherwise</returns>
        public bool UpdateLastRowAddAction(int rowIndex, T newRowData)
        {
            if (!isEnabled || undoStack.Count == 0)
            {
                return false;
            }

            // Search from the most recent (First) backwards for a RowAdd action with matching rowIndex
            var currentNode = undoStack.First;
            while (currentNode != null)
            {
                var action = currentNode.Value;
                if (action?.ActionType == UndoRedoActionType.RowAdd && action.RowIndex == rowIndex)
                {
                    // Found it! Update the rowData with current values
                    action.RowData = newRowData;
                   
                    return true;
                }

                currentNode = currentNode.Next;
            }

            return false;  // No matching RowAdd action found
        }

        #region Undo/Redo Action Application

        /// <summary>
        /// Applies an undo/redo action to the grid by restoring old or new values based on operation type.
        /// This method is the trigger point that translates UndoRedoAction objects into actual grid updates.
        /// </summary>
        /// <param name="action">The undo/redo action to apply</param>
        /// <param name="isRedoAction">True if this is a redo operation; false if undo. Controls which value (old vs new) is applied.</param>
        /// <returns>Task representing the asynchronous operation</returns>
        internal async Task ApplyUndoRedoAction(UndoRedoAction<T>? action, bool isRedoAction = false)
        {
            if (action == null || Parent == null)
            {
                return;
            }

            try
            {
                switch (action.ActionType)
                {
                    case UndoRedoActionType.CellEdit:
                        await ApplyCellEditUndo(action, isRedoAction).ConfigureAwait(true);
                        break;

                    case UndoRedoActionType.RowAdd:
                        await ApplyRowAddUndo(action, isRedoAction).ConfigureAwait(true);
                        break;

                    case UndoRedoActionType.RowDelete:
                        await ApplyRowDeleteUndo(action, isRedoAction).ConfigureAwait(true);
                        break;

                    case UndoRedoActionType.Paste:
                        await ApplyPasteUndo(action, isRedoAction).ConfigureAwait(true);
                        break;

                    case UndoRedoActionType.AutoFill:
                        await ApplyAutoFillUndo(action, isRedoAction).ConfigureAwait(true);
                        break;

                    default:
                        break;
                }

                // Refresh grid UI after applying changes
                Parent.SoftRefresh = true;
                Parent.PreventRender(false);
                Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
            }
            catch (Exception ex)
            {
                if (Parent.GridEvents?.OnActionFailure.HasDelegate == true)
                {
                    await Parent.GridEvents.OnActionFailure.InvokeAsync(new FailureEventArgs() { Error = ex, Parent = Parent }).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Applies a single cell edit undo/redo action by restoring the appropriate cell value.
        /// For undo: restores the old value.
        /// For redo: applies the new value (what was edited to).
        /// </summary>
        private async Task ApplyCellEditUndo(UndoRedoAction<T> action, bool isRedoAction)
        {
            if (action.CellChange == null || Parent == null)
            {
                return;
            }

            var change = action.CellChange;
            var rowIndex = change.RowIndex;
            var fieldName = change.FieldName;
            
            // CRITICAL FIX: Use the correct value based on operation type
            // Undo → restore to OldValue (previous state)
            // Redo → apply NewValue (edited state)
            var valueToApply = isRedoAction ? change.NewValue : change.OldValue;

            if (string.IsNullOrEmpty(fieldName) || valueToApply == null)
            {
                return;
            }

            // Update the cell with the appropriate value via EditModule
            // Pass isUndoRedoAction=true to ensure we clone from Row.Data (the original), not Row.EditedData
            await Parent.EditModule!.UpdateCell(rowIndex, fieldName, valueToApply, isUndoRedoAction: true).ConfigureAwait(true);

            var operationType = isRedoAction ? "redone" : "undone";
        }

        /// <summary>
        /// Applies a row add undo/redo action.
        /// For undo: removes the added row.
        /// For redo: re-adds the row.
        /// </summary>
        private async Task ApplyRowAddUndo(UndoRedoAction<T> action, bool isRedoAction)
        {
            if (action.RowIndex == null || action.RowIndex < 0 || Parent == null)
            {
                return;
            }

            var rowIndex = action.RowIndex.Value;

            if (isRedoAction)
            {
                // Redo: Re-add the row
                if (action.RowData != null)
                {
                    var newRow = new RowModelGenerator<T>(Parent).GenerateRow(action.RowData, rowIndex);
                    newRow.EditedData = action.RowData;
                    newRow.IsDirty = true;
                    newRow.Action = EditAction.Added;

                    // CRITICAL FIX: Mark all cells as dirty to show the edit indicator
                    newRow.Cells?.ForEach(_ => _.IsDirty = true);

                    var insertIndex = Math.Min(rowIndex, Parent.Rows?.Count ?? 0);
                    if (insertIndex >= 0 && insertIndex <= (Parent.Rows?.Count ?? 0))
                    {
                        Parent.Rows?.Insert(insertIndex, newRow);
                       
                    }
                }
            }
            else
            {
                // Undo: Remove the row
                var row = Parent.Rows?.Find(_ => _.Index == rowIndex);
                if (row != null)
                {
                    Parent.Rows?.Remove(row);
                    
                }
            }

            // CRITICAL: Refresh row indices and trigger UI update after modifying rows collection
            Parent.EditModule!.RefreshRowIndex();
            Parent.EditModule!.HasBatchChanges = true;
            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
            Parent.EventAggregator.Trigger("ContentStateChanged", null!);

            await Task.CompletedTask.ConfigureAwait(true);
        }

        /// <summary>
        /// Applies a row delete undo/redo action.
        /// For undo: restores the deleted row.
        /// For redo: deletes the row again.
        /// </summary>
        private async Task ApplyRowDeleteUndo(UndoRedoAction<T> action, bool isRedoAction)
        {
            if (action.RowIndex == null || action.RowData == null || Parent == null)
            {
                return;
            }

            var rowIndex = action.RowIndex.Value;
            var primaryKeyFields = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            var primaryKeyField = primaryKeyFields?.Count > 0 ? primaryKeyFields[0] : null;

            if (isRedoAction)
            {
                // 🔧 Redo: Delete the row again by finding it and setting EditAction to Deleted
                Row<object>? row = null;
                
                if (primaryKeyField != null && action.RowData != null)
                {
                    var redoRowPrimaryKeyValue = Parent.PropHelper?.GetObject(primaryKeyField, action.RowData);
                    
                    // Find row with matching primary key that is NOT already deleted
                    row = Parent.Rows?.FirstOrDefault(_ => 
                        _.Data != null && 
                        _.Action != EditAction.Deleted &&
                        !GridUtils.CompareValues<object>(
                            Parent.PropHelper?.GetObject(primaryKeyField, _.Data)!,
                            redoRowPrimaryKeyValue!
                        )
                    );
                }

                if (row != null)
                {
                    // Simply toggle back to Deleted state (like EJ2)
                    row.Action = EditAction.Deleted;
                    row.IsDirty = true;  // Keep dirty so renderer maintains e-hiddenrow CSS
                    
                    Parent.EventAggregator.Trigger("RowStateChanged", row);
                }
            }
            else
            {
                // 🔧 Undo: Restore the deleted row by toggling EditAction back to None
                Row<object>? deletedRow = null;
                
                if (primaryKeyField != null)
                {
                    var undoRowPrimaryKeyValue = Parent.PropHelper?.GetObject(primaryKeyField, action.RowData);

                    // Find the deleted row with matching primary key using explicit loop
                    deletedRow = null;
                    if (Parent.Rows != null)
                    {
                        foreach (var row in Parent.Rows)
                        {
                            if (row.Action == EditAction.Deleted && row.Data != null &&
                                !GridUtils.CompareValues<object>(
                                    Parent.PropHelper?.GetObject(primaryKeyField, row.Data)!,
                                    undoRowPrimaryKeyValue!
                                ))
                            {
                                deletedRow = row;
                                break;
                            }
                        }
                    }
                }

                if (deletedRow != null)
                {
                    // 🔧 FIXED: Restore the deleted row with all user edits preserved
                    // Instead of clearing EditedData, restore it from action.RowData
                    // This ensures undo delete shows the edited values, not original values
                    deletedRow.Action = EditAction.None;
                    
                    // Restore EditedData from action.RowData (contains all user edits)
                    deletedRow.EditedData = action.RowData;
                    
                    // 🔧 CORRECTED: Set IsDirty based on whether restored data differs from original data
                    // If user edited the row BEFORE deleting it, show the dirty indicator after undo
                    // If row was never edited before delete, don't show dirty indicator
                    var hasEdits = GridUtils.CompareValues(deletedRow.Data, action.RowData);
                    deletedRow.IsDirty = hasEdits;
                    
                    // 🔧 IMPORTANT: Only mark SPECIFIC cells as dirty that were actually edited
                    // Don't apply hasEdits to all cells - compare each cell's value individually
                    deletedRow.Cells?.ForEach(cell =>
                    {
                        if (cell.Column?.Field != null)
                        {
                            // Get original value for this specific cell from row.Data
                            var originalValue = Parent.PropHelper?.GetObject(cell.Column.Field, deletedRow.Data);
                            
                            // Get restored/edited value for this cell from action.RowData
                            var restoredValue = Parent.PropHelper?.GetObject(cell.Column.Field, action.RowData);
                            
                            // Only mark this specific cell dirty if THIS cell was edited
                            // CompareValues returns TRUE when values DIFFER, FALSE when SAME
                            // So no negation needed - just use the result directly
                            var cellHasEdit = GridUtils.CompareValues(originalValue, restoredValue);
                            cell.IsDirty = cellHasEdit;
                        }
                        else
                        {
                            cell.IsDirty = false;
                        }
                        
                        cell.EditDisabled = false;
                    });

                    Parent.EventAggregator.Trigger("RowStateChanged", deletedRow);
                }
                else
                {
                    // Fallback: Create new row if no deleted row found
                    var newRow = new RowModelGenerator<T>(Parent).GenerateRow(action.RowData, rowIndex);
                    newRow.EditedData = action.RowData;
                    
                    // 🔧 CORRECTED: Set IsDirty based on whether restored data differs from original data
                    // Same logic as primary case - if data differs from original, mark as dirty
                    var hasEdits = !GridUtils.CompareValues(newRow.Data, action.RowData);
                    newRow.IsDirty = hasEdits;

                    var insertIndex = Math.Min(rowIndex, Parent.Rows?.Count ?? 0);
                    if (insertIndex >= 0 && insertIndex <= (Parent.Rows?.Count ?? 0))
                    {
                        Parent.Rows?.Insert(insertIndex, newRow);
                        
                    }
                }
            }

            Parent.EditModule!.HasBatchChanges = true;
            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);

            await Task.CompletedTask.ConfigureAwait(true);
        }

        /// <summary>
        /// Applies a multi-cell paste undo/redo action.
        /// For undo: restores all previous values.
        /// For redo: applies all new values.
        /// </summary>
        private async Task ApplyPasteUndo(UndoRedoAction<T> action, bool isRedoAction)
        {
            if (action.PreviousValues == null || action.PreviousValues.Count == 0 || Parent == null)
            {
                return;
            }

            // Apply appropriate values (old for undo, new for redo)
            foreach (var change in action.PreviousValues)
            {
                if (string.IsNullOrEmpty(change.FieldName))
                {
                    continue;
                }

                var valueToApply = isRedoAction ? change.NewValue : change.OldValue;
                if (valueToApply != null)
                {
                    await Parent.EditModule!.UpdateCell(change.RowIndex, change.FieldName, valueToApply ,false).ConfigureAwait(true);
                }
            }

            Parent.EditModule!.HasBatchChanges = true;
            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);

            var operationType = isRedoAction ? "redone" : "undone";
        }

        /// <summary>
        /// Applies an auto-fill undo/redo action.
        /// For undo: restores all previous values.
        /// For redo: applies all new values.
        /// </summary>
        private async Task ApplyAutoFillUndo(UndoRedoAction<T> action, bool isRedoAction)
        {
            if (action.PreviousValues == null || action.PreviousValues.Count == 0 || Parent == null)
            {
                return;
            }

            // Apply appropriate values (old for undo, new for redo)
            foreach (var change in action.PreviousValues)
            {
                if (string.IsNullOrEmpty(change.FieldName))
                {
                    continue;
                }

                var valueToApply = isRedoAction ? change.NewValue : change.OldValue;
                if (valueToApply != null)
                {
                    await Parent.EditModule!.UpdateCell(change.RowIndex, change.FieldName, valueToApply ,false).ConfigureAwait(true);
                }
            }

            Parent.EditModule!.HasBatchChanges = true;
            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);

            var operationType = isRedoAction ? "redone" : "undone";
        }

        #endregion
    }
}
