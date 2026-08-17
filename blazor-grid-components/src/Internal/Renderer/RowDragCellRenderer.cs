using Microsoft.AspNetCore.Components;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Partial class containing cell rendering logic for GridRow component.
    /// Handles cell state management, rendering, and user interactions.
    /// </summary>
    public partial class GridRowBase<TRow>
    {
        #region RowDrag Cell RenderFragment
        // Renders the RowDrag (drag handle) cell as a RenderFragment
        internal RenderFragment RenderRowDragCell(GridCellParameters p) => builder =>
        {
            var row = p.Row;
            var cell = p.Cell;
            if (row == null || cell == null) return;

            // Build base class list from static classes + any cell-specific classes
            // Then apply RowDrag specific adjustments used in the original component
            // Get or create cell state
            var cellClassName = BaseClassName(cell, row);
            var classNames = string.Join(" ", cellClassName).Replace(" e-freezeleftborder", string.Empty, System.StringComparison.Ordinal).Trim();

            // If we have frozen columns, append left-freeze class when needed
            if (Parent != null && Parent.FreezeModule!.GetFrozenCount() > 0 && Parent.FreezeModule!.GetFreezeLeftCount() > 0)
            {
                classNames = string.Concat(classNames, " e-leftfreeze");
            }

            // Compute the inline left offset only for frozen layout (same as original)
            // Avoid LINQ; O(n) walk until we find this cell
            string? styleText = null;
            if (Parent != null && Parent.FreezeModule!.GetFrozenCount() > 0)
            {
                int indent = 0;
                var cells = row.Cells;
                for (int i = 0; i < cells.Count; i++)
                {
                    if (ReferenceEquals(cells[i], cell))
                    {
                        if (i > 0) indent = 30 * i;
                        styleText = $"left: {indent}px";
                        break;
                    }
                }
                // Fallback if not found (shouldn't happen)
                styleText ??= "left: 0px";
            }

            // <td ...>
            builder.OpenElement(0, "td");
            builder.AddAttribute(1, "class", classNames);
            builder.AddAttribute(2, "tabindex", -1);
            builder.AddAttribute(3, "data-uid", cell.Uid);
            if (styleText != null)
                builder.AddAttribute(4, "data-sf-style", styleText);

            // <div class="e-icons e-rowcelldrag e-dtdiagonalright e-icon-rowdragicon"></div>
            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", "e-icons e-rowcelldrag e-dtdiagonalright e-icon-rowdragicon");
            builder.CloseElement();

            builder.CloseElement(); // </td>
        };
        #endregion
    }
}
