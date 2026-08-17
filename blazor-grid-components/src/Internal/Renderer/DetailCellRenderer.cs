using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Partial class containing detail cell rendering logic for GridRow component.
    /// Handles detail cell state management, rendering, and user interactions.
    /// </summary>
    public partial class GridRowBase<TRow>
    {
        #region Detail Cell Rendering

        /// <summary>
        /// Main RenderFragment for rendering a detail cell TD element
        /// </summary>
        /// <param name="cellParameters">Parameters containing cell and row information</param>
        /// <returns>RenderFragment for the detail cell</returns>
        internal RenderFragment RenderDetailCell(GridCellParameters cellParameters) => builder =>
        {
            var sequence = 0;
            var row = cellParameters.Row;
            var cell = cellParameters.Cell;

            if (row == null || cell == null) return;

            builder.OpenElement(sequence++, "td");
            string? expandClass = cell.DetailRowExpand ? "e-detailrowexpand" : "e-detailrowcollapse";
            string? cellClassName = BaseClassName(cell, row);
            string? baseClass = cellClassName.Replace(" e-freezeleftborder", string.Empty, StringComparison.Ordinal).Trim();
            string? classNames = $"{expandClass} {baseClass}";

            if (Parent != null && Parent.FreezeModule!.GetFreezeLeftCount() > 0)
            {
                classNames = string.Concat(classNames, " e-leftfreeze");
            }

            builder.AddAttribute(sequence++, "class", classNames);

            // Add attributes
            builder.AddAttribute(sequence++, "aria-expanded", cell.DetailRowExpand ? "true" : "false");
            builder.AddAttribute(sequence++, "tabindex", 0);
            builder.AddAttribute(sequence++, "data-uid", cell.Uid);

            // Add style if frozen
            var styleText = Parent?.DetailRowModule != null ? Parent.DetailRowModule.GetDetailCellStyle(row, cell) : string.Empty;
            if (!string.IsNullOrEmpty(styleText))
            {
                builder.AddAttribute(sequence++, "data-sf-style", styleText);
            }

            // Add click handler
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async (args) =>
            {
                await (Parent?.FocusModule?.ClearFocus()!).ConfigureAwait(true);
                await Parent.FocusModule.Focus(row.Uid!, cell.Uid).ConfigureAwait(true);
                if (Parent?.DetailRowModule != null)
                    await Parent.DetailRowModule.DetailCellClickHandlerInternal(args, row, cell).ConfigureAwait(true);
            }));

            // Add keydown handler
            builder.AddAttribute(sequence++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this,
                async (e) => await (Parent?.FocusModule?.ProcessKeyDown(e, row, cell)!).ConfigureAwait(true)));
            builder.AddEventPreventDefaultAttribute(sequence++, "onkeydown", true);

            // Render expand/collapse icon
            var ariaLabel = cell.DetailRowExpand ? "Row Expand" : "Row Collapse";
            var iconClass = cell.DetailRowExpand
                ? "e-icons e-dtdiagonaldown e-icon-gdownarrow"
                : "e-icons e-dtdiagonalright e-icon-grightarrow";

            builder.OpenElement(sequence++, "a");
            builder.AddAttribute(sequence++, "aria-label", ariaLabel);
            builder.AddAttribute(sequence++, "class", iconClass);
            builder.CloseElement(); // </a>

            builder.CloseElement(); // </td>
        };
        #endregion
    }
}