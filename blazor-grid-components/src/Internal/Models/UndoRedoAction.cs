// Copyright Syncfusion. All rights reserved.
// Use of this source code is governed by a license file that can be found in the LICENSE file.

using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Defines the type of action that can be recorded for undo/redo operations.
    /// </summary>
    public enum UndoRedoActionType
    {
        /// <summary>
        /// Single cell value change.
        /// </summary>
        CellEdit,

        /// <summary>
        /// New row added to the grid.
        /// </summary>
        RowAdd,

        /// <summary>
        /// Row deleted from the grid.
        /// </summary>
        RowDelete,

        /// <summary>
        /// Multi-cell paste operation (atomic).
        /// </summary>
        Paste,

        /// <summary>
        /// Fill-handle pattern operation (atomic).
        /// </summary>
        AutoFill
    }

    /// <summary>
    /// Represents a single cell change within an undo/redo action.
    /// </summary>
    /// <typeparam name="T">The type of data in the row.</typeparam>
    public class CellChange<T>
    {
        /// <summary>
        /// Gets or sets the index of the row containing the changed cell.
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the column containing the changed cell.
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets the field name of the column.
        /// </summary>
        public string? FieldName { get; set; }

        /// <summary>
        /// Gets or sets the old value of the cell before the change.
        /// </summary>
        public object? OldValue { get; set; }

        /// <summary>
        /// Gets or sets the new value of the cell after the change.
        /// </summary>
        public object? NewValue { get; set; }

        /// <summary>
        /// Gets or sets the column definition of the changed cell.
        /// </summary>
        public GridColumn? Column { get; set; }
    }

    /// <summary>
    /// Represents a single undo/redo action that can be recorded in the history.
    /// </summary>
    /// <typeparam name="T">The type of data in the grid rows.</typeparam>
    public class UndoRedoAction<T>
    {
        /// <summary>
        /// Gets or sets the type of action being performed (CellEdit, RowAdd, etc.).
        /// </summary>
        public UndoRedoActionType ActionType { get; set; }

        /// <summary>
        /// Gets or sets the sequence number for debugging and tracking purposes.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the cell change details for CellEdit actions.
        /// </summary>
        public CellChange<T>? CellChange { get; set; }

        /// <summary>
        /// Gets or sets the row data for RowAdd or RowDelete actions.
        /// </summary>
        public T? RowData { get; set; }

        /// <summary>
        /// Gets or sets the index of the row affected by the action.
        /// </summary>
        public int? RowIndex { get; set; }

        /// <summary>
        /// Gets or sets the position where a row was added (Top or Bottom).
        /// </summary>
        public NewRowPosition? RowPosition { get; set; }

        /// <summary>
        /// Gets or sets the collection of previous cell values for multi-cell actions like Paste or AutoFill.
        /// </summary>
        public List<CellChange<T>>? PreviousValues { get; set; }

        /// <summary>
        /// Gets or sets the collection of previous row data for multi-row actions.
        /// </summary>
        public List<T>? PreviousRows { get; set; }
    }
}
