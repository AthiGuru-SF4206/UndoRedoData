using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Partial class containing caption summary cell rendering logic for GridRow component.
    /// Handles aggregate cells in group captions and summary rows.
    /// </summary>
    public partial class GridRowBase<TRow>
    {
        #region Caption Summary Cell Rendering

        /// <summary>
        /// Main RenderFragment for rendering a caption/summary aggregate cell TD element
        /// </summary>
        /// <param name="cellParameters">Parameters containing cell and row information</param>
        /// <returns>RenderFragment for the caption/summary cell</returns>
        internal RenderFragment RenderCaptionSummaryCell(GridCellParameters cellParameters) => builder =>
        {
            var sequence = 0;
            var row = cellParameters.Row;
            var cell = cellParameters.Cell;

            if (row == null || cell == null) return;

            // Build aggregate context
            var aggregateContext = BuildAggregateContext(row, cell);

            // Determine cell type and render accordingly
            var txtClass = GridUtils.GetAlignmentClass(cell.Column!);
            var cellClassName = BaseClassName(cell, row);
            var classNames = $"{cellClassName} {txtClass}";
            var ariaLabel = BuildAriaLabel(cell, aggregateContext);

            if (row.RowType == "Summary" && cell.AggregateColumn?.GroupFooterTemplate != null)
            {
                RenderGroupFooterTemplateCell(builder, ref sequence, row, cell, classNames, ariaLabel, aggregateContext);
            }
            else if (cell.AggregateColumn?.GroupCaptionTemplate != null)
            {
                RenderGroupCaptionTemplateCell(builder, ref sequence, row, cell, classNames, ariaLabel, aggregateContext);
            }
            else
            {
                RenderSimpleAggregateCell(builder, ref sequence, row, cell, classNames, ariaLabel);
            }
        };



        /// <summary>
        /// Renders group footer template cell (supports focus)
        /// </summary>
        private void RenderGroupFooterTemplateCell(RenderTreeBuilder builder, ref int seq,
            Row<object> row, Cell<object> cell, string classNames, string ariaLabel,
            AggregateTemplateContext context)
        {
           
            builder.OpenElement(seq++, "td");
            builder.AddAttribute(seq++, "class", $"{classNames} {(Parent?.FocusModule != null ? Parent.FocusModule.GetFocusClass(cell) : string.Empty)}");
            builder.AddAttribute(seq++, "role", "gridcell");
            builder.AddAttribute(seq++, "tabindex", EnsureTabIndexForSummary(row, cell));
            if (cell.AggregateColumn?.Type is AggregateType type && type != AggregateType.Custom) { builder.AddAttribute(seq++, "aria-label", ariaLabel); }
            builder.AddAttribute(seq++, "data-uid", cell.Uid);

            if (!string.IsNullOrEmpty(StyleText))
                builder.AddAttribute(seq++, "data-sf-style", StyleText);

            // Click handler
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async (e) =>
            {
                if (Parent?.FocusModule != null)
                {
                    await Parent.FocusModule.CellClickHandler((row, cell), e).ConfigureAwait(true);
                    await Parent.FocusModule.Refresh(row, cell, isCtrlOrShiftKeyPressed: e.CtrlKey || e.ShiftKey).ConfigureAwait(true);
                }
            }));

            // Focus handlers
            builder.AddAttribute(seq++, "onfocus", EventCallback.Factory.Create(this, () =>
            {
                if (Parent?.FocusModule != null)
                {
                    Parent.FocusModule.SetFocusedCell(cell.Uid, true);
                }
            }));

            builder.AddAttribute(seq++, "onblur", EventCallback.Factory.Create(this, async () =>
            {
                if (Parent?.FocusModule != null)
                {
                    Parent.FocusModule.SetFocusedCell(cell.Uid, false);
                    await Parent.FocusModule.ClearFocus(row, cell).ConfigureAwait(true);
                }
            }));

            // Keydown handler
            builder.AddAttribute(seq++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this,
                async (e) =>
                {
                    if (Parent?.FocusModule != null)
                    {
                        await Parent.FocusModule.ProcessKeyDown(e, row, cell).ConfigureAwait(true);
                    }
                }));
            builder.AddEventPreventDefaultAttribute(seq++, "onkeydown", !IsLastCellForSummary(row, cell));

            // Render template content
            builder.AddContent(seq++, cell.AggregateColumn?.GroupFooterTemplate!(context));

            builder.CloseElement(); // </td>
        }

        /// <summary>
        /// Renders group caption template cell
        /// </summary>
        private void RenderGroupCaptionTemplateCell(RenderTreeBuilder builder, ref int seq,
            Row<object> row, Cell<object> cell, string classNames, string ariaLabel,
            AggregateTemplateContext context)
        {
            builder.OpenElement(seq++, "td");
            builder.AddAttribute(seq++, "class", classNames);
            builder.AddAttribute(seq++, "role", "gridcell");
            builder.AddAttribute(seq++, "tabindex", cell.TabIndex);
            if (cell.AggregateColumn?.Type is AggregateType type && type != AggregateType.Custom) { builder.AddAttribute(seq++, "aria-label", ariaLabel); }
            builder.AddAttribute(seq++, "data-uid", cell.Uid);

            if (!string.IsNullOrEmpty(StyleText))
                builder.AddAttribute(seq++, "data-sf-style", StyleText);

            // Click handler
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async (e) =>
            {
                if (Parent?.FocusModule != null)
                {
                    await Parent.FocusModule.CellClickHandler((row, cell), e).ConfigureAwait(true);
                    await Parent.FocusModule.Refresh(row, cell, isCtrlOrShiftKeyPressed: e.CtrlKey || e.ShiftKey).ConfigureAwait(true);
                }
            }));

            // Blur handler
            builder.AddAttribute(seq++, "onblur", EventCallback.Factory.Create(this, async () =>
            {
                if (Parent?.FocusModule != null)
                {
                    await Parent.FocusModule.ClearFocus(row, cell).ConfigureAwait(true);
                }
            }));

            // Keydown handler
            builder.AddAttribute(seq++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this,
                async (e) =>
                {
                    if (Parent?.FocusModule != null)
                    {
                        await Parent.FocusModule.ProcessKeyDown(e, row, cell).ConfigureAwait(true);
                    }
                }));
            builder.AddEventPreventDefaultAttribute(seq++, "onkeydown", !IsLastCellForSummary(row, cell));

            // Render template content
            builder.AddContent(seq++, cell.AggregateColumn?.GroupCaptionTemplate!(context));

            builder.CloseElement(); // </td>
        }

        /// <summary>
        /// Renders simple aggregate value cell (no template)
        /// </summary>
        private void RenderSimpleAggregateCell(RenderTreeBuilder builder, ref int seq,
            Row<object> row, Cell<object> cell, string classNames, string ariaLabel)
        {
            builder.OpenElement(seq++, "td");
            builder.AddAttribute(seq++, "class", classNames);
            builder.AddAttribute(seq++, "role", "gridcell");
            builder.AddAttribute(seq++, "tabindex", cell.TabIndex);
            builder.AddAttribute(seq++, "aria-label", ariaLabel);
            builder.AddAttribute(seq++, "data-uid", cell.Uid);

            if (!string.IsNullOrEmpty(StyleText))
                builder.AddAttribute(seq++, "data-sf-style", StyleText);

            // Click handler
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async (e) =>
            {
                if (Parent?.FocusModule != null)
                {
                    await Parent.FocusModule.CellClickHandler((row, cell), e).ConfigureAwait(true);
                    await Parent.FocusModule.Refresh(row, cell, isCtrlOrShiftKeyPressed: e.CtrlKey || e.ShiftKey).ConfigureAwait(true);
                }
            }));

            // Blur handler
            builder.AddAttribute(seq++, "onblur", EventCallback.Factory.Create(this, async () =>
            {
                if (Parent?.FocusModule != null)
                {
                    await Parent.FocusModule.ClearFocus(row, cell).ConfigureAwait(true);
                }
            }));

            // Keydown handler
            builder.AddAttribute(seq++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this,
                async (e) =>
                {
                    if (Parent?.FocusModule != null)
                    {
                        await Parent.FocusModule.ProcessKeyDown(e, row, cell).ConfigureAwait(true);
                    }
                }));
            builder.AddEventPreventDefaultAttribute(seq++, "onkeydown", !IsLastCellForSummary(row, cell));

            // Render aggregate value
            builder.AddContent(seq++, cell.AggregateValue?.ToString() ?? string.Empty);

            builder.CloseElement(); // </td>
        }
        #endregion

        #region Helper Methods for Caption Summary Cell Rendering

        /// <summary>
        /// Builds the aggregate template context from row and cell data
        /// </summary>
        private AggregateTemplateContext BuildAggregateContext(Row<object> row, Cell<object> cell)
        {
            var context = new AggregateTemplateContext();

            // Extract grouped data
            
            var groupedData = (row?.Data as Group<TRow>);
            context.Key = groupedData?.Key?.ToString();
            context.Field = groupedData?.Field?.ToString();

            // Get column and handle foreign key
            if (!string.IsNullOrEmpty(context.Field))
            {
                var col = GridUtils.GetColumnByField(context.Field, GridUtils.GetColumns(Parent!));
                if (col != null)
                {
                    context.HeaderText = col.HeaderText;

                    if (col.IsForeignColumn() && col.ColumnData is IEnumerable columnData)
                    {
                        var fData = columnData.Cast<object>().ToList();
                        var value = fData?.Find(a =>
                        {
                            var fieldValue = a?.GetType()?.GetProperty(context.Field)?.GetValue(a)?.ToString();
                            return fieldValue == context.Key;
                        });

                        if (value != null && !string.IsNullOrEmpty(col.ForeignKeyValue))
                        {
                            context.ForeignKey = value.GetType()?.GetProperty(col.ForeignKeyValue)?.GetValue(value)?.ToString()!;
                        }
                    }
                }
            }

            // Set aggregate value based on type
            if (cell.AggregateColumn != null && cell.AggregateValue != null)
            {
                var aggValue = cell.AggregateValue.ToString();
                switch (cell.AggregateColumn.Type)
                {
                    case AggregateType.Sum:
                        context.Sum = aggValue!;
                        break;
                    case AggregateType.Average:
                        context.Average = aggValue!;
                        break;
                    case AggregateType.Max:
                        context.Max = aggValue!;
                        break;
                    case AggregateType.Min:
                        context.Min = aggValue!;
                        break;
                    case AggregateType.Count:
                        context.Count = aggValue!;
                        break;
                    case AggregateType.TrueCount:
                        context.TrueCount = aggValue!;
                        break;
                    case AggregateType.FalseCount:
                        context.FalseCount = aggValue!;
                        break;
                    case AggregateType.Custom:
                        context.Custom = aggValue!;
                        break;
                }
            }

            return context;
        }

        /// <summary>
        /// Builds aria-label for accessibility
        /// </summary>
        private string BuildAriaLabel(Cell<object> cell, AggregateTemplateContext context)
        {
            var parts = new List<string>
            {
                cell.AggregateValue?.ToString() ?? string.Empty,
                Parent?.Localizer?.GetText(GridLocaleKeys.TemplateColumnARIA) ?? string.Empty,
                Parent?.Localizer?.GetText(GridLocaleKeys.ColumnHeaderARIA) ?? string.Empty,
                cell.Column?.HeaderText ?? string.Empty
            };

            return string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        
        /// <summary>
        /// Ensures correct tab index for summary cells
        /// </summary>
        private int EnsureTabIndexForSummary(Row<object> row, Cell<object> cell)
        {
            var firstRow = Parent?.Rows?.FirstOrDefault();
            var lastRow = Parent?.Rows?.LastOrDefault();
            bool firstRowFirstVisibleCell = row == firstRow && cell == firstRow?.Cells?.Where(e => e.Visible)?.FirstOrDefault();
            bool lastRowLastVisibleCell = row == lastRow && cell == lastRow?.Cells?.Where(e => e.Visible)?.LastOrDefault();
            bool isFirstOrLastCell = firstRowFirstVisibleCell || lastRowLastVisibleCell;

            if (isFirstOrLastCell && !(Parent!.IsEdit || Parent.IsAdd) && !Parent.FocusModule!.ChangeLastCellTabIndex)
            {
                return 0;
            }
            if (Parent!.EditSettings!.ShowAddNewRow && !Parent.IsEdit && Parent.IsAdd && isFirstOrLastCell)
            {
                return 0;
            }
            return cell.TabIndex;
        }

        /// <summary>
        /// Checks if this is the last cell in summary row
        /// </summary>
        private bool IsLastCellForSummary(Row<object> row, Cell<object> cell)
        {
            Cell<object> lastCell = row?.Cells?.Where(_ => _.Visible)?.DefaultIfEmpty()?.LastOrDefault()!;
            if (lastCell == null) return false;

            return (row?.RowType == "Summary" &&
                    cell?.AggregateColumn?.GroupFooterTemplate != null &&
                    row == Parent?.Rows?.LastOrDefault() &&
                    lastCell.Equals(cell) &&
                    !(cell.EditDisabled == true));
        }

        #endregion
    }
}