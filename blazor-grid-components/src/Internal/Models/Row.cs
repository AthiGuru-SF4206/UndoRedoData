using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Defines the row of the grid.
    /// </summary>
    /// <typeparam name="T">TValue of the grid.</typeparam>
    /// <exclude/>
    public class Row<T>
    {
        /// <summary>
        /// Gets or set unique identifier.
        /// </summary>
        public string? Uid { get; set; }

        /// <summary>
        /// Gets the data of the row.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Gets the EditedData of the row.
        /// </summary>
        public T? EditedData { get; set; }

        /// <summary>
        /// Specifies that row is detail row.
        /// </summary>
        public bool IsDetailRow { get; set; }
        private bool _isSelect { get; set; }

        /// <summary>
        /// Specifies that row is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelect; set
            {
                _isSelect = value;
                HasChanges = true;
            }
        }

        /// <summary>
        /// Specifies that row is alternate.
        /// </summary>
        public bool IsAltRow { get; set; }

        /// <summary>
        /// Specifies that row is a data row.
        /// </summary>
        public bool IsDataRow { get; set; }

        /// <summary>
        /// Specifies that row is template row.
        /// </summary>
        public bool IsTemplate { get; set; }

        /// <summary>
        /// Specifies that row is last row.
        /// </summary>
        public bool IsLastRow { get; set; }

        /// <summary>
        /// Specifies that row is visible.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Specifies that row is expanded.
        /// </summary>
        public bool IsExpand { get; set; }

        /// <summary>
        /// Gets cells of the row.
        /// </summary>
        public List<Cell<T>> Cells { get; set; } = new List<Cell<T>>();

        /// <summary>
        /// Gets the row index.
        /// </summary>
        public int? Index { get; set; }

        /// <summary>
        /// Gets the row indent.
        /// </summary>
        public int Indent { get; set; }

        /// <summary>
        /// Gets the foreign key data.
        /// </summary>
        public IDictionary<string, IEnumerable<object>>? ForeignKeyData { get; set; }

        /// <summary>
        /// Specifies the parent row id.
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Specifies the child row id.
        /// </summary>
        public int ChildId { get; set; }

        /// <summary>
        /// Specifies the row index.
        /// </summary>
        public int rowsIndex { get; set; }

        /// <summary>
        /// Specifies the group summary.
        /// </summary>
        public int GroupSummary { get; set; }

        /// <summary>
        /// Specifies that row is caption row.
        /// </summary>
        public bool IsCaptionRow { get; set; }

        /// <summary>
        /// Gets the css class.
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Gets the row type.
        /// </summary>
        public string RowType { get; set; } = "Data";

        /// <summary>
        /// Gets the parent row uid.
        /// </summary>
        public string? ParentUid { get; set; }

        /// <summary>
        /// Gets the row selected state.
        /// </summary>
        public string State { get; set; } = "None";

        /// <summary>
        /// Gets the row is in edit state.
        /// </summary>
        public bool IsEdit { get; set; }

        /// <summary>
        /// Indicates whether the row has unsaved changes.
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Indicates whether the row was added at the top of the grid.
        /// </summary>
        public bool IsAddedTop { get; set; }

        /// <summary>
        /// Indicates whether the row was added at the bottom of the grid.
        /// </summary>
        public bool IsAddedBottom { get; set; }

        /// <summary>
        /// Gets or sets the current edit action performed on the row.
        /// </summary>
        public EditAction Action { get; set; } = EditAction.None;

        /// <summary>
        /// Indicates whether the row has any changes.
        /// </summary>
        public bool HasChanges { get; set; }

        /// <summary>
        /// Indicates whether the row has changes in its data.
        /// </summary>
        public bool HasDataChanges { get; set; }

        /// <summary>
        /// Gets or sets the group key associated with the row.
        /// </summary>
        public object? GroupKey { get; set; }

        internal bool IsRowSelectionCancelled { get; set; }

        internal int GroupIndex { get; set; }
    }
}
