using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Internal;
using Syncfusion.ExcelExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Partial class containing cell rendering logic for GridRow component.
    /// Handles expand cell state management, rendering, and user interactions.
    /// </summary>
    public partial class GridRowBase<TRow>
    {
        #region Expand cell state Management
        internal RenderFragment RenderIndentCell(GridCellParameters cellParameters) => builder =>
        {
            var sequence = 0;
            var row = cellParameters.Row!;
            var cell = cellParameters.Cell!;

            var cellClassName = BaseClassName(cell, row);
            var isDetailIndent = cellClassName.Contains("e-detailindentcell", StringComparison.Ordinal);

            if (isDetailIndent)
            {
                // Render TH for detail indent
                builder.OpenElement(sequence++, "th");
                builder.AddAttribute(sequence++, "class", cellClassName);
                builder.AddAttribute(sequence++, "data-sf-style", StyleText);
                builder.AddAttribute(sequence++, "tabindex", cell.TabIndex);
                builder.AddAttribute(sequence++, "data-uid", cell.Uid);
                builder.AddAttribute(sequence++, "scope", "col");
                builder.CloseElement();
            }
            else
            {
                // Render TD for regular indent
                builder.OpenElement(sequence++, "td");
                builder.AddAttribute(sequence++, "class", GetIndentClass(row, cell));
                builder.AddAttribute(sequence++, "data-sf-style", GridRowBase<TRow>.GetIndentStyle(row, cell));
                builder.AddAttribute(sequence++, "tabindex", cell.TabIndex);
                builder.AddAttribute(sequence++, "data-uid", cell.Uid);
                builder.CloseElement();
            }
        };
        #endregion

        #region Helper Methods

        private static string GetIndentStyle(Row<object> row, Cell<object> cell)
        {
            var styleText = string.Empty;
            var indent = 0;

            for (int i = 0; i < row.Cells.Count; i++)
            {
                if (row.Cells[i] == cell)
                {
                    if (i > 0)
                    {
                        indent = 30 * i;
                    }
                    styleText = $"Left: {indent}px";
                    break;
                }
            }

            if (string.IsNullOrEmpty(styleText))
            {
                styleText = "Left: 0px";
            }

            return styleText;
        }

        private string GetIndentClass(Row<object> row, Cell<object> cell)
        {
            //var cellClassName = string.Join(" ", GetCellClassNames(cell, row).ToArray());
            var cellClassName = BaseClassName(cell, row);
            string classNames = cellClassName.Replace(" e-freezeleftborder", string.Empty, StringComparison.Ordinal).Trim();

            if (Parent!.FreezeModule!.GetFreezeLeftCount() > 0)
            {
                return string.Concat(classNames, " e-leftfreeze");
            }

            return classNames;
        }
        #endregion
    }
}
