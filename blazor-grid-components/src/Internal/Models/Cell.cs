using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Defines the cell of the grid.
    /// </summary>
    /// <typeparam name="T">TValue of the grid.</typeparam>
    /// <exclude/>
    public class Cell<T>
    {
        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        public string Uid { get; set; }

        /// <summary>
        /// Gets the cell type.
        /// </summary>
        public CellType CellType { get; set; }

        /// <summary>
        /// Gets the visible state.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets the cell template.
        /// </summary>
        public bool IsTemplate { get; set; }

        /// <summary>
        /// Specifies that cell is data cell.
        /// </summary>
        public bool IsDataCell { get; set; }

        /// <summary>
        /// Specifies that cell is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Specifies that cell is detail row expand.
        /// </summary>
        public bool DetailRowExpand { get; set; }

        /// <summary>
        /// Specifies that cell is frozen.
        /// </summary>
        public bool IsFrozen { get; set; }
        
        /// <summary>
        /// Defines which side the column need to freeze.
        /// </summary>
        public FreezeDirection Freeze { get; set; }

        /// <summary>
        /// Gets the column associated with cell.
        /// </summary>
        public GridColumn? Column { get; set; }

        /// <summary>
        /// Gets the row id.
        /// </summary>
        public string? RowID { get; set; }

        /// <summary>
        /// Gets the cell index.
        /// </summary>
        public int? Index { get; set; }

        /// <summary>
        /// Gets the cell index,.
        /// </summary>
        public int? ColIndex { get; set; }

        /// <summary>
        /// Gets the class name.
        /// </summary>
        public string? ClassName { get; set; }

        /// <summary>
        /// Gets the cell attributes.
        /// </summary>
        public object? Attributes { get; set; }

        /// <summary>
        /// Specifies that cell is foreign key column.
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// Gets the foreign key data.
        /// </summary>
        public object? ForeignKeyData { get; set; }

        /// <summary>
        /// Get the col span.
        /// </summary>
        public int? ColSpan { get; set; } = 1;

        /// <summary>
        /// Gets the row span.
        /// </summary>
        public int? RowSpan { get; set; } = 1;

        /// <summary>
        /// Gets the aggregate column.
        /// </summary>
        public GridAggregateColumn? AggregateColumn { get; set; }

        /// <summary>
        /// Gets the aggregate value.
        /// </summary>
        public object? AggregateValue { get; set; }

        /// <summary>
        /// Specifies that cell is stacked.
        /// </summary>
        public bool IsStacked { get; set; }

        /// <summary>
        /// Specifies that cell is focused.
        /// </summary>
        public bool IsFocused { get; set; }

        /// <summary>
        /// Specifies that show outline.
        /// </summary>
        public bool ShowFocusLine { get; set; }

        /// <summary>
        /// Specifies the cell tabindex.
        /// </summary>
        public int TabIndex { get; set; } = -1;
        /// <summary>
        /// Gets or sets a value indicating whether this cell is covered by another cell's span.
        /// When true, this cell should not be rendered.
        /// </summary>
        internal bool IsSpanned { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this cell participates in row spanning.
        /// </summary>
        internal bool IsRowSpanned { get; set; }


        /// <summary>
        /// Initializes a new instance of the Cell class and assigns a unique identifier.
        /// </summary>
        public Cell() => Uid = GridUtils.GetUid("gridcell");


        /// <summary>
        /// Indicates whether the cell is currently in edit mode.
        /// </summary>
        public bool IsEdit { get; set; }


        /// <summary>
        /// Indicates whether the cell has unsaved changes.
        /// </summary>
        public bool IsDirty { get; set; }


        /// <summary>
        /// Indicates whether the cell has any changes applied.
        /// </summary>
        public bool Changes { get; set; }


        /// <summary>
        /// Indicates whether editing is disabled for the cell.
        /// </summary>
        public bool EditDisabled { get; set; }
		
		internal bool EnableFrozenLineCursor { get; set; }

        internal bool EnableLeftFrozenLineCursor { get; set; }

        internal bool EnableRightFrozenLineCursor { get; set; }

        internal bool EnableFixedLeftFrozenLineCursor { get; set; }

        internal bool EnableFixedRightFrozenLineCursor { get; set; }

        internal bool EnableDefaultFrozenLine { get; set; }

        internal bool EnableFrozenResizeCursor { get; set; }


        internal List<string> ClassList { get; set; } = new List<string>();

        internal List<string> StyleList { get; set; } = new List<string>();

        internal IDictionary<string, object> AttributeList { get; set; } = new Dictionary<string, object>();

    }

    /// <summary>
    /// Cell rendering context.
    /// </summary>
    /// <typeparam name="T">TValue of the grid.</typeparam>
    internal class CellContext<T>
    {
        public Row<T>? Row { get; set; }

        public Cell<T>? Cell { get; set; }
    }
}
