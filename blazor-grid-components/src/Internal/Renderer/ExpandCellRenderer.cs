using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Linq;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Partial class containing cell rendering logic for GridRow component.
    /// Handles expand cell state management, rendering, and user interactions.
    /// </summary>
    public partial class GridRowBase<TRow>
    {
        #region Expand cell RenderFragment

        /// <summary>
        /// Main RenderFragment for rendering a expand cell TD element
        /// </summary>
        /// <param name="cellParameters">Parameters containing cell and row information</param>
        /// <returns>RenderFragment for the cell</returns>
        internal RenderFragment RenderExpandCell(GridCellParameters cellParameters) => builder =>
        {
            var sequence = 0;
            var row = cellParameters.Row;
            var cell = cellParameters.Cell;
            if (row == null || cell == null)
            {
                return;
            }
            builder.OpenElement(sequence++, "td");

            // Build class string
            var classNames = $"{(row!.IsExpand ? "e-recordplusexpand" : "e-recordpluscollapse")} {GetClass(row, cell)} ";
            builder.AddAttribute(sequence++, "class", classNames);

            // Add styles and attributes
            builder.AddAttribute(sequence++, "data-sf-style", GridRowBase<TRow>.ExpandGetStyle(row, cell));
            builder.AddAttribute(sequence++, "tabindex", ValidateTabIndex(row, cell));
            builder.AddAttribute(sequence++, "data-uid", cell.Uid);
            builder.AddAttribute(sequence++, "aria-expanded", row?.IsExpand.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture));

            // Add event handlers
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async (args) =>
            {
                await (Parent?.FocusModule?.CellClickHandler((row!, cell), args)!).ConfigureAwait(true);
                await (Parent?.GroupModule?.ExpandCollapse(row!)!).ConfigureAwait(true);
                await Parent.FocusModule.ClearFocus().ConfigureAwait(true);
                await Parent.FocusModule.Focus(row?.Uid!, cell?.Uid!).ConfigureAwait(true);
                if (!Parent.AllowSelection)
                {
                    await Parent.FocusModule.Refresh(row!, cell!, isCtrlOrShiftKeyPressed: args.CtrlKey || args.ShiftKey).ConfigureAwait(true);
                }
            }));

            builder.AddAttribute(sequence++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this,
                async (e) => await (Parent?.FocusModule?.ProcessKeyDown(e, row!, cell)!).ConfigureAwait(true)!));
            builder.AddEventPreventDefaultAttribute(sequence++, "onkeydown", true);

            // Render icon
            if (row?.IsExpand == true)
            {
                builder.OpenElement(sequence++, "a");
                builder.AddAttribute(sequence++, "class", "e-icons e-gdiagonaldown e-icon-gdownarrow");
                builder.AddAttribute(sequence++, "title", "expanded");
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(sequence++, "a");
                builder.AddAttribute(sequence++, "class", "e-icons e-gnextforward e-icon-grightarrow");
                builder.AddAttribute(sequence++, "title", "collapsed");
                builder.CloseElement();
            }

            builder.CloseElement(); // </td>
        };

        #endregion

        #region Helper Methods

        private static string ExpandGetStyle(Row<object> row, Cell<object> cell)
        {
            var styleText = string.Empty;
            var indent = 0;

            for (int i = 0; i < row?.Cells.Count; i++)
            {
                if (row?.Cells[i] == cell && i > 0)
                {
                    indent = 30 * i;
                    styleText = $"Left: {indent}px";
                    break;
                }
            }

            if (indent == 0)
            {
                styleText = "Left: 0px";
            }

            return styleText;
        }

        private string GetClass(Row<object> row, Cell<object> cell)
        {
            var cellClassName = BaseClassName(cell, row);

            cellClassName = cellClassName.Replace(cellClassName.Contains("e-recordplusexpand", StringComparison.Ordinal) 
                ? "e-recordplusexpand" : "e-recordpluscollapse", string.Empty, StringComparison.Ordinal);

            string classNames = cellClassName.Replace(" e-freezeleftborder", string.Empty, StringComparison.Ordinal).Trim();

            if (Parent != null && Parent.FreezeModule!.GetFreezeLeftCount() > 0)
            {
                return string.Concat(classNames, " e-leftfreeze");
            }

            return classNames;
        }

        private int ValidateTabIndex(Row<object> row, Cell<object> cell)
        {
            if (Parent?.Rows != null && Parent.Rows.Any(e => e.IsCaptionRow) &&
                Parent.Rows.FirstOrDefault(e => e.IsCaptionRow)?.Equals(row) == true &&
                row.Cells.FirstOrDefault()?.Equals(cell) == true)
            {
                return 0;
            }

            return cell.TabIndex;
        }
        #endregion
    }
}
