using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;
using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Partial class containing summary cell rendering logic for GridRow component.
    /// Handles rendering of summary row cells (empty cells in summary rows).
    /// </summary>
    public partial class GridRowBase<TRow>
    {
        #region Summary Cell Rendering

        /// <summary>
        /// Main RenderFragment for rendering a summary cell TD element (empty cell in summary row)
        /// </summary>
        /// <param name="cellParameters">Parameters containing cell and row information</param>
        /// <returns>RenderFragment for the summary cell</returns>
        internal RenderFragment RenderSummaryCell(GridCellParameters cellParameters) => builder =>
        {
            var sequence = 0;
            var row = cellParameters.Row;
            var cell = cellParameters.Cell;

            if (row == null || cell == null) return;

            // Get alignment class
            var txtClass = GridUtils.GetAlignmentClass(cell.Column!);
            var cellClassName = BaseClassName(cell, row);
            var classNames = $"{cellClassName} {txtClass}";

            // Build aria label
            var ariaLabel = string.Empty;
            if (Parent != null && Parent.Localizer != null && cell.Column != null)
            {
                var columnHeader = Parent.Localizer.GetText(GridLocaleKeys.ColumnHeaderARIA);
                ariaLabel = $"{columnHeader} {cell.Column.HeaderText}";
            }

            builder.OpenElement(sequence++, "td");
            builder.AddAttribute(sequence++, "class", classNames);
            builder.AddAttribute(sequence++, "aria-label", ariaLabel);

            if (!string.IsNullOrEmpty(StyleText))
                builder.AddAttribute(sequence++, "data-sf-style", StyleText);

            builder.AddAttribute(sequence++, "tabindex", cell.TabIndex);
            builder.AddAttribute(sequence++, "data-uid", cell.Uid);

            // Click handler
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async (e) =>
            {
                await Parent!.FocusModule!.CellClickHandler((row, cell), e).ConfigureAwait(true);
                await Parent.FocusModule.Refresh(row, cell, isCtrlOrShiftKeyPressed: e.CtrlKey || e.ShiftKey).ConfigureAwait(true);
            }));

            // Blur handler
            builder.AddAttribute(sequence++, "onblur", EventCallback.Factory.Create(this,
                async () => await Parent!.FocusModule!.ClearFocus(row, cell).ConfigureAwait(true)));

            // Keydown handler
            builder.AddAttribute(sequence++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this,
                async (e) => await Parent!.FocusModule!.ProcessKeyDown(e, row, cell).ConfigureAwait(true)));
            builder.AddEventPreventDefaultAttribute(sequence++, "onkeydown", !IsLastCellInternal(row, cell));

            // Empty cell - no content

            builder.CloseElement(); // </td>
        };
        #endregion
    }
}