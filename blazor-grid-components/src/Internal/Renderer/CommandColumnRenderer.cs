using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Buttons;
using Syncfusion.ExcelExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Partial class containing command column cell rendering logic for GridRow component.
    /// Handles rendering of command buttons (Edit, Delete, Custom) in command columns.
    /// </summary>
    public partial class GridRowBase<TRow>
    {
        #region Command Column Cell Renderfragment

        /// <summary>
        /// Main RenderFragment for rendering a command column cell TD element
        /// </summary>
        /// <param name="cellParameters">Parameters containing cell and row information</param>
        /// <returns>RenderFragment for the command column cell</returns>
        internal RenderFragment RenderCommandColumnCell(GridCellParameters cellParameters) => builder =>
        {
            var sequence = 0;
            var row = cellParameters.Row;
            var cell = cellParameters.Cell;

            if (row == null || cell == null || cell.Column?.Commands == null) return;

            // Track focus state
            string focusClass = string.Empty;
            string? txtAlign = GridRowBase<TRow>.GetCommandColumnTextAlign(cell);
            string? cellClassName = BaseClassName(cell, row);
            string? classNames = $"e-rowcell {cellClassName} {focusClass} {txtAlign}";

            var attributes = GetCommandColumnAttributes(cell, row);

            builder.OpenElement(sequence++, "td");
            builder.AddAttribute(sequence++, "class", classNames);
            builder.AddAttribute(sequence++, "role", "gridcell");

            // Add style
            string cacheKey = $"{row.Uid}_{cell.Uid}";
            var cellState = GetOrUpdateCellState(cacheKey, cell, row);
            if (!string.IsNullOrEmpty(cellState.StyleText))
                builder.AddAttribute(sequence++, "data-sf-style", cellState.StyleText);

            builder.AddAttribute(sequence++, "data-uid", cell.Uid);
            builder.AddAttribute(sequence++, "tabindex", cell.TabIndex);
            builder.AddMultipleAttributes(sequence++, attributes);

            // Click handler
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async (e) =>
            {
                await CellClickHandlerInternal(e, row, cell, false).ConfigureAwait(true);
                if (Parent?.AllowSelection == true)
                {
                    await (Parent.FocusModule?.Refresh(row, cell))!.ConfigureAwait(true);
                }
            }));

            // Focus handler
            builder.AddAttribute(sequence++, "onfocus", EventCallback.Factory.Create(this, () =>
            {
                focusClass = "e-focus e-focused";
            }));

            // Blur handler
            builder.AddAttribute(sequence++, "onblur", EventCallback.Factory.Create(this, async () =>
            {
                focusClass = string.Empty;
                await (Parent?.FocusModule?.ClearFocus(row, cell))!.ConfigureAwait(true);
            }));

            // Keydown handler
            builder.AddAttribute(sequence++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this,
                async (e) => await (Parent?.FocusModule?.ProcessKeyDown(e, row, cell))!.ConfigureAwait(true)));
            builder.AddEventPreventDefaultAttribute(sequence++, "onkeydown", !IsLastCellInternal(row, cell));

            // Render unboundcelldiv with command buttons
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "e-unboundcelldiv");
            builder.AddEventStopPropagationAttribute(sequence++, "onclick", true);

            // Render each command button
            foreach (var command in cell.Column.Commands)
            {
                if (command.Type != CommandButtonType.Save && command.Type != CommandButtonType.Cancel)
                {
                    RenderCommandButton(builder, ref sequence, row, cell, command);
                }
            }

            builder.CloseElement(); // </div>

            builder.CloseElement(); // </td>
        };
        #endregion

        #region Helper Methods
        /// <summary>
        /// Renders a single command button
        /// </summary>
        private void RenderCommandButton(RenderTreeBuilder builder, ref int seq,
            Row<object> row, Cell<object> cell, GridCommandColumn command)
        {
            var buttonOptions = Parent?.SetButtonOptions(command);
            var buttonCssClass = $"{command.ButtonOption?.CssClass} e-edit-delete e-{command.Type}button";

            builder.OpenComponent(seq++, typeof(SfButton));
            builder.AddAttribute(seq++, "Type", "button");
            builder.AddMultipleAttributes(seq++, buttonOptions);

            if (command.ButtonOption?.Disabled == true)
                builder.AddAttribute(seq++, "Disabled", true);

            if (!string.IsNullOrEmpty(command.ButtonOption?.IconCss))
                builder.AddAttribute(seq++, "IconCss", command.ButtonOption.IconCss);

            builder.AddAttribute(seq++, "CssClass", buttonCssClass);
            builder.AddAttribute(seq++, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(
                this,
                async (e) => await CommandClickHandler(command, row, cell).ConfigureAwait(true)
            ));

            builder.CloseComponent();
        }

        /// <summary>
        /// Gets text alignment class for command column
        /// </summary>
        private static string GetCommandColumnTextAlign(Cell<object> cell)
        {
            if (cell.Column?.directParamKeys?.Contains("TextAlign") == true &&
                cell.Column.TextAlign != TextAlign.None)
            {
                return GridUtils.GetAlignmentClass(cell.Column) + " ";
            }
            return "e-rightalign ";
        }

        /// <summary>
        /// Gets attributes for command column cell (including adaptive UI data-cell)
        /// </summary>
        private Dictionary<string, object> GetCommandColumnAttributes(Cell<object> cell, Row<object> row)
        {
            var cellState = GetOrUpdateCellState(null!, cell, row);
            var attributes = new Dictionary<string, object>(cellState.Attributes);

            if (Parent!.EnableAdaptiveUI && Parent.RowRenderingMode.Equals(RowDirection.Vertical))
            {
                // Use GetDataCell() method from base class if available, otherwise use column header
                var dataCellValue = cell.Column?.HeaderText ?? cell.Column?.Field ?? "Command";
                attributes["data-cell"] = dataCellValue;
            }

            return attributes;
        }
        #endregion
    }
}