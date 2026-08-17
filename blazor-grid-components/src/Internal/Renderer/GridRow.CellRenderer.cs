using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Internal;
using Syncfusion.ExcelExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Partial class containing cell rendering logic for GridRow component.
    /// Handles cell state management, rendering, and user interactions.
    /// </summary>
    public partial class GridRowBase<TRow>
    {
        #region Cell Properties
        /// <summary>
        /// Cache for storing cell state information to optimize re-renders
        /// </summary>
        private Dictionary<string, CellStateInfo> _cellCache { get; set; } = new Dictionary<string, CellStateInfo>();
        internal Cell<object>? Cell { get; set; }
        internal Row<object>? Row { get; set; }

        /// <summary>
        /// Gets or sets the cell parameters containing cell and row information for rendering.
        /// </summary>
        [Parameter]
        public GridCellParameters? CellParameters { get; set; }
        /// <summary>
        /// Internal class to store cell state information
        /// </summary>
        internal class CellStateInfo
        {
            public Cell<object>? PreviousCell { get; set; }
            public CellDOM? CellDom { get; set; }
            public List<string> ClassList { get; set; } = new List<string>();
            public List<string> StyleList { get; set; } = new List<string>();
            public IDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
            public string StyleText { get; set; } = string.Empty;
            public object? CellValue { get; set; }
            public bool CheckBoxChecked { get; set; }
            public string FocusClass { get; set; } = string.Empty;
            public bool IsRecordClickEvent { get; set; }
        }

        #endregion

        #region Main Cell Rendering and RenderFragment
        /// <summary>
        /// Main RenderFragment for rendering a cell TD element
        /// </summary>
        /// <param name="cellParameters">Parameters containing cell and row information</param>
        /// <returns>RenderFragment for the cell</returns>
        internal RenderFragment RenderCell(GridCellParameters cellParameters) => builder =>
        {
            if (Parent == null || cellParameters?.Cell == null || cellParameters?.Row == null)
                return;

            var cell = Cell = cellParameters.Cell;
            var row = Row = cellParameters.Row;

            if (cell == null || row == null) return;

            // Skip rendering for spanned cells
            if (cell.IsSpanned || cell.IsRowSpanned)
                return;

            // Get or create cell state
            string? cacheKey = null!;
            bool shouldCache = Parent.GridEvents?.QueryCellInfo.HasDelegate == true;
            if (Parent.IsRenderedFromTreeGrid)
            {
                var response = new Dictionary<string, bool>();
                Parent.EventAggregator?.Trigger("CheckQueryCellInfoCache", response);
                shouldCache = response.ContainsKey("ShouldCache") && response["ShouldCache"];
                if (response["ShouldAddDataItem"] && cell.Column?.Field?.Contains("DataItem", StringComparison.Ordinal) == false)
                {
                    Parent?.PropHelper?.SetValue(cell.Column, nameof(cell.Column.Field), $"DataItem.{cell.Column.Field}");
                }
            }
            if (Parent != null &&(shouldCache || Parent.IsRenderedFromPivotTable))
            {
                cacheKey = $"{row.Uid}_{cell.Uid}";
            }
            var cellState = GetOrUpdateCellState(cacheKey, cell, row);

            // Check if we should render this cell
            if (Parent != null && Parent.ShouldRenderColumn(cell.Visible, cell?.Column?.Field!))
            {
                string? classNames = string.Join(" ", cellState.ClassList);
                string? txtClass = GridUtils.GetAlignmentClass(cell?.Column!);
                if (cell != null && cell.IsEdit)
                {
                    // Batch Edit Cell
                    RenderBatchEditCell(builder, cell, row, classNames, txtClass, cellState);
                }
                else if (cell != null && cell.IsTemplate)
                {
                    // Template Cell
                    RenderTemplateCell(builder, cell, row, cellParameters!, classNames, txtClass, cellState);
                }
                else
                {
                    // Normal Data Cell
                    RenderDataCell(builder, cell!, row, cellParameters!, classNames, txtClass, cellState);
                }
            }
        };

        /// <summary>
        /// Renders a batch edit cell
        /// </summary>
        private void RenderBatchEditCell(RenderTreeBuilder builder, Cell<object> cell, Row<object> row,
            string classNames, string txtClass, CellStateInfo cellState)
        {
            builder.OpenElement(0, "td");
            builder.AddAttribute(1, "class", $"{classNames} {txtClass}");
            builder.AddAttribute(2, "tabindex", cell?.TabIndex);
            builder.AddAttribute(3, "role", "gridcell");
            builder.AddMultipleAttributes(4, GridUtils.GetAttributeValues(cellState.Attributes, cellState.StyleText));
            builder.AddAttribute(5, "data-uid", cell?.Uid);
            builder.AddAttribute(6, "e-mappinguid", cell?.Column?.Uid);
            builder.AddAttribute(7, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(
                this, async (e) => await KeyDownHandlerInternal(e, row, cell!).ConfigureAwait(true)));
            builder.AddEventStopPropagationAttribute(8, "onkeydown", true);
            builder.AddAttribute(9, "onfocusout", EventCallback.Factory.Create(this, () =>
            {
                cellState.FocusClass = string.Empty;
            }));

            builder.OpenComponent(10, typeof(BatchEdit<>).MakeGenericType(typeof(TRow)));
            builder.AddAttribute(11, "CellParameters", new GridCellParameters()
            {
                Row = row,
                Cell = cell
            });
            builder.CloseComponent();

            builder.CloseElement();
        }

        /// <summary>
        /// Renders a template cell
        /// </summary>
        private void RenderTemplateCell(RenderTreeBuilder builder, Cell<object> cell, Row<object> row,
            GridCellParameters cellParameters, string classNames, string txtClass, CellStateInfo cellState)
        {
            builder.OpenElement(0, "td");
            builder.AddAttribute(1, "class", $"{classNames} {cellState.FocusClass} {txtClass}");
            builder.AddAttribute(2, "role", "gridcell");
            builder.AddAttribute(3, "tabindex", EnsureTabIndexInternal(row, cell));
            builder.AddAttribute(4, "data-uid", cell?.Uid);
            builder.AddMultipleAttributes(5, GridUtils.GetAttributeValues(cellState.Attributes, cellState.StyleText));
            builder.AddAttribute(6, "onclick", EventCallback.Factory.Create<MouseEventArgs>(
                this, async (e) =>
                {
                    if(Parent?.EditModule != null && await Parent.EditModule.InvokeSingleClickHandler(row,cell!).ConfigureAwait(true))
                    {
                        return;
                    }
                    await CellClickHandlerInternal(e, row, cell!, false).ConfigureAwait(true);
                    if (Parent != null && !Parent.AllowSelection && Parent.FocusModule != null)
                    {
                        await Parent.FocusModule.Refresh(row, cell!, isCtrlOrShiftKeyPressed: e.CtrlKey || e.ShiftKey).ConfigureAwait(true);
                    }
                }));
            builder.AddAttribute(7, "ondblclick", EventCallback.Factory.Create(
                this, async () => await (Parent!.EditModule!.DblClickHandler(row, cell!).ConfigureAwait(true))));
            builder.AddAttribute(8, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(
                this, async (e) => await Parent!.FocusModule!.ProcessKeyDown(e, row, cell!).ConfigureAwait(true)));
            builder.AddEventPreventDefaultAttribute(9, "onkeydown", !IsLastCellInternal(row, cell!));

            builder.OpenElement(10, "div");
            builder.AddEventStopPropagationAttribute(11, "onkeydown", true);

            if (cell != null && cell.IsDirty)
            {
                builder.AddContent(12, cell.Column?.Template!(row?.EditedData!));
            }
            else
            {
                builder.AddContent(13, cell?.Column?.Template!(row?.Data!));
            }

            builder.CloseElement();

            // Render frozen cursor
            RenderFrozenLineCursorTemplate(builder, 14, cell!, cellParameters);

            builder.CloseElement();
        }

        /// <summary>
        /// Renders a normal data cell
        /// </summary>
        private void RenderDataCell(RenderTreeBuilder builder, Cell<object> cell, Row<object> row,
            GridCellParameters cellParameters, string classNames, string txtClass, CellStateInfo cellState)
        {
            var isChecked = false;

            builder.OpenElement(0, "td");
            builder.AddAttribute(1, "class", $"{classNames} {cellState.FocusClass} {txtClass}");
            builder.AddAttribute(2, "tabindex", EnsureTabIndexInternal(row, cell));
            builder.AddAttribute(3, "role", "gridcell");
            builder.AddMultipleAttributes(4, GridUtils.GetAttributeValues(cellState.Attributes, cellState.StyleText));
            builder.AddAttribute(5, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this,
                async (e) => await ClickHandlerInternal(e, row, cell).ConfigureAwait(true)));
            builder.AddAttribute(6, "ondblclick", EventCallback.Factory.Create<MouseEventArgs>(this,
                async (e) => await CellDoubleClickHandlerInternal(e, row, cell).ConfigureAwait(true)));
            builder.AddAttribute(7, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(
                this, async (e) => await KeyDownHandlerInternal(e, row, cell).ConfigureAwait(true)));
            builder.AddEventPreventDefaultAttribute(8, "onkeydown", !IsLastCellInternal(row, cell));

            // Render cell content
            if (cell?.Column?.Type == ColumnType.CheckBox)
            {
                RenderCheckBoxInternal(builder, 9, cell, row, cellState.CellValue!, ref isChecked, false);
            }
            else if (cell?.Column?.DisplayAsCheckBox != true)
            {
                GridRowBase<TRow>.RenderCellContentInternal(builder, 10, cell!, cellState.CellValue!);
            }
            else
            {
                var checkBoxChecked = cellState.CheckBoxChecked;
                RenderCheckBoxInternal(builder, 11, cell, row, cellState.CellValue!, ref checkBoxChecked, true);
            }

            // Render frozen cursor
            RenderFrozenLineCursorDataCell(builder, 12, cell!, cellParameters);

            builder.CloseElement();
        }

        /// <summary>
        /// Renders a checkbox control
        /// </summary>
        private void RenderCheckBoxInternal(RenderTreeBuilder builder, int sequence, Cell<object> cell,
            Row<object> row, object cellValue, ref bool isChecked, bool isDisabled)
        {
            var aria = new Dictionary<string, string>()
            {
                { "aria-label", Parent!.Localizer?.GetText(GridLocaleKeys.SelectRowARIA) ?? string.Empty }
            };

            string? strVal = cellValue?.ToString();
            if (!string.IsNullOrEmpty(Cell?.Column?.Field) && strVal == Convert.ToString(true) || strVal == Convert.ToString(1, System.Globalization.CultureInfo.InvariantCulture))
            {
                isChecked = true;
            }

            object checkClass = null!;
            cell?.Column?.CustomAttributes?.TryGetValue("checkboxclass", out checkClass!);
            checkClass = checkClass ?? string.Empty;

            builder.OpenComponent(sequence, typeof(CheckBoxRenderer));
            builder.AddAttribute(sequence + 1, "WrapperClass", checkClass.ToString() ?? string.Empty);

            if (!isDisabled)
            {
                // Create a synchronous Action<MouseEventArgs> wrapper that fires async work
                Action<MouseEventArgs> clickAction = (MouseEventArgs e) =>
                {
                    // Fire and forget the async operation
                    _ = CellClickHandlerInternal(e, row, cell!, true);
                };

                builder.AddAttribute(sequence + 2, "OnClick", clickAction);
            }

            builder.AddAttribute(sequence + 3, "Checked", isChecked);
            builder.AddAttribute(sequence + 4, "AriaLabel", aria);

            if (isDisabled)
            {
                builder.AddAttribute(sequence + 5, "IsDisabled", true);
                builder.AddAttribute(sequence + 6, "RequireInput", false);
            }

            builder.CloseComponent();
        }

        /// <summary>
        /// Renders cell text content with proper HTML encoding
        /// </summary>
        private static void RenderCellContentInternal(RenderTreeBuilder builder, int sequence, Cell<object> cell, object cellValue)
        {
            if (cell?.Column?.DisableHtmlEncode == true)
            {
                if (string.IsNullOrEmpty(cellValue?.ToString()))
                {
                    builder.AddMarkupContent(sequence, "&nbsp;");
                }
                else
                {
                    builder.AddContent(sequence + 1, cellValue);
                }
            }
            else
            {
                if (cell?.Column?.EnableSanitization == false)
                {
                    builder.AddContent(sequence + 2, (MarkupString)($"{cellValue}"));
                }
                else
                {
                    builder.AddContent(sequence + 2, GridUtils.GetRawContent(SanitizeHtmlHelper.Sanitize($"{cellValue}")));
                }
            }
        }

        #endregion

        #region Cell State Management Methods

        /// <summary>
        /// Gets or updates cell state from cache, handles change detection
        /// </summary>
        /// <param name="cacheKey">Unique cache key for the cell (format: "{rowUid}_{cellUid}")</param>
        /// <param name="cell">The cell object to process</param>
        /// <param name="row">The row containing the cell</param>
        /// <returns>The cached or newly created cell state information</returns>
        internal CellStateInfo GetOrUpdateCellState(string cacheKey = null!, Cell<object> cell = null!, Row<object> row = null!)
        {
            CellStateInfo cellState;
            bool needsUpdate = false;

            if (cacheKey != null && _cellCache.TryGetValue(cacheKey, out cellState!))
            {
                // Check if update is needed
                if (cell != cellState.PreviousCell ||
                    (cellState.CellDom != null && cellState.CellDom.HasChanges) ||
                    Parent!.SoftRefresh ||
                    row?.HasChanges == true ||
                    cell.Changes)
                {
                    needsUpdate = true;
                }
            }
            else
            {
                cellState = new CellStateInfo();
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                // Reset change flags
                if (cellState.CellDom != null && cellState.CellDom.HasChanges)
                {
                    cellState.CellDom.HasChanges = false;
                }

                // Reset Row.HasChanges if this is the last cell
                if (row?.HasChanges == true && row?.Cells?.Count != 0 &&
                    (row?.Cells?.LastOrDefault()?.Uid?.Equals(cell.Uid, StringComparison.Ordinal) == true))
                {
                    row.HasChanges = false;
                }
                var currentKey = $"{row!.Uid}_{cell.Uid}";
                if (Parent!.GridEvents?.OnRecordClick.HasDelegate == true && _cellCache.Count > 0
                    && _cellCache.TryGetValue(currentKey, out var cachedState))
                {
                    cellState = cachedState;
                }
                // Update cell state if cell changed or cell.Changes is true
                if (cell != cellState.PreviousCell || cell.Changes)
                {
                    cellState = UpdateCellState(cell, row, cellState);
                    if ((Parent.GridEvents?.QueryCellInfo.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable) && cell.CellType == CellType.Data)
                    {
                        InvokeQueryCellInfoIfNeeded(cell, row, cellState);
                    }
                    cell.Changes = false;
                }

                cellState.PreviousCell = cell;
                cellState.StyleText = string.Empty;
                BuildCellAttributesComplete(cell, row, cellState);
                ApplyClassAndStylesComplete(cell, cellState);
            }
            if (cacheKey != null)
            {
                _cellCache[cacheKey] = cellState;
            }

            return cellState;
        }

        /// <summary>
        /// Invokes QueryCellInfo event if it has delegates
        /// </summary>
        private void InvokeQueryCellInfoIfNeeded(Cell<object> cell, Row<object> row, CellStateInfo cellState)
        {
            if (cell.CellType == CellType.Data)
            {
                if (Parent!.GridEvents?.QueryCellInfo.HasDelegate == true ||
                    Parent.IsRenderedFromTreeGrid ||
                    Parent.IsRenderedFromPivotTable)
                {
                    Parent.IsFreezeLineMoved = false;
                    var args = new QueryCellInfoEventArgs<TRow>()
                    {
                        Column = cell.Column,
                        Data = (TRow)row?.Data!,
                        Cell = cellState.CellDom!,
                        Parent = Parent
                    };

                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                    {
                        Parent.EventAggregator.NotifyAsync("QueryCellInfo", args).GetAwaiter().GetResult();
                    }
                    else
                    {
                        Parent.GridEvents!.QueryCellInfo.InvokeAsync(args).GetAwaiter().GetResult();
                    }

                    // Apply changes from QueryCellInfo
                    cellState.ClassList = cellState.CellDom!.ClassList ?? cellState.ClassList;
                    cellState.StyleList = cellState.CellDom.Styles ?? cellState.StyleList;
                    cellState.Attributes = cellState.CellDom.AttributeList ?? cellState.Attributes;
                }
            }
        }

        #endregion

        #region Cell Attribute Building

        /// <summary>
        /// Builds complete cell attributes including aria, span, visibility, etc.
        /// </summary>
        private void BuildCellAttributesComplete(Cell<object> cell, Row<object> row, CellStateInfo cellState)
        {
            // Build basic cell attributes
            BuildCellAttributeInternal(cell, row, cellState);

            // Adaptive UI
            if (Parent != null && Parent.EnableAdaptiveUI && Parent.RowRenderingMode.Equals(RowDirection.Vertical))
            {
                cellState.Attributes.AddOrUpdateItem("data-cell", cell?.Column?.HeaderText ?? cell?.Column?.Field!);
            }

            if (cell?.Column?.DisplayAsCheckBox == true && !string.IsNullOrEmpty(cellState.CellValue?.ToString()))
            {
                string value = cellState.CellValue.ToString()!;
                cellState.CheckBoxChecked = bool.Parse(value);
            }

            if (cell?.Column?.Type == ColumnType.CheckBox)
            {
                cellState.ClassList.AddOrSkip("e-gridchkbox");
                cellState.ClassList.AddOrSkip("e-rowcell");
                cellState.Attributes.AddOrUpdateItem("aria-label", "checkbox");
            }

            ApplyClipMode(cell!, cellState);

            GridRowBase<TRow>.ApplyCustomAttributes(cell!, cellState);

            if (cell != null && cell.AttributeList.Any())
            {
                foreach (var attribute in cell.AttributeList)
                {
                    cellState.Attributes.AddOrUpdateItem(attribute.Key, attribute.Value);
                }
            }
        }

        /// <summary>
        /// Applies clip mode classes to cell
        /// </summary>
        private void ApplyClipMode(Cell<object> cell, CellStateInfo cellState)
        {
            bool isEllipsisWithTooltip = (cell?.Column != null && cell?.Column.ClipMode.Equals(ClipMode.EllipsisWithTooltip) == true) ||
                (Parent!.ClipMode.Equals(ClipMode.EllipsisWithTooltip) == true && cell?.CellType != CellType.RowDrag &&
                 cell?.CellType != CellType.Expand && cell?.CellType != CellType.Detail);
            bool isNotCheckBoxColumn = cell?.Column != null && cell?.Column.Type.Equals(ColumnType.CheckBox) != true;

            if ((cell?.Column != null && cell?.Column.ClipMode.Equals(ClipMode.Clip) == true) ||
                Parent!.ClipMode.Equals(ClipMode.Clip) == true)
            {
                cellState.ClassList.AddOrSkip("e-gridclip");
            }
            else if (isEllipsisWithTooltip && isNotCheckBoxColumn)
            {
                cellState.ClassList.AddOrSkip("e-ellipsistooltip");
            }
        }

        /// <summary>
        /// Applies custom attributes from column configuration
        /// </summary>
        private static void ApplyCustomAttributes(Cell<object> cell, CellStateInfo cellState)
        {
            if (cell?.Column?.CustomAttributes != null && (cell?.Column.CustomAttributes.Any() ?? false))
            {
                Dictionary<string, object> customAttrType = new Dictionary<string, object>((
                    cell?.Column?.CustomAttributes as Dictionary<string, object>)!);

                if (customAttrType.TryGetValue("class", out var cls))
                {
                    cellState.ClassList.AddOrSkip(cls?.ToString()!);
                    customAttrType.Remove("class");
                }

                cellState.StyleText = GridUtils.GetStyleAsStringFromObject(customAttrType);
                customAttrType.Remove("style");

                foreach (var item in customAttrType)
                {
                    cellState.Attributes.AddOrUpdateItem(item.Key, item.Value);
                }
            }
        }

        /// <summary>
        /// Builds internal cell attributes (aria, spans, visibility, selection)
        /// </summary>
        private void BuildCellAttributeInternal(Cell<object> cell, Row<object> row, CellStateInfo cellState)
        {
            if (cell?.Index != null)
            {
                cellState.Attributes.AddOrUpdateItem("aria-colindex", cell.Index + 1);
            }
            GridRowBase<TRow>.AddSpanAttributesInternal(cell!, cellState);
            ApplyLastRowBorder(cell!, row, cellState);
            ApplyVisibility(cell!, row, cellState);
            GridRowBase<TRow>.ApplyEditAndDirtyStates(cell!, row, cellState);
            if (cell?.ClassList?.Count != 0)
            {
                cellState.ClassList.AddRange(cell!.ClassList);
            }

            if (cell?.StyleList?.Count != 0)
            {
                cellState.StyleList.AddRange(cell!.StyleList);
            }

            if (cell?.Changes == true)
            {
                cell.Changes = false;
                cellState.CellValue = GridUtils.GetCellValue(cell, row);
            }

            if (cell?.IsTemplate == true)
            {
                cellState.ClassList.AddOrSkip("e-templatecell");
            }

            if (cell?.EnableFrozenLineCursor == true && cell?.EnableRightFrozenLineCursor == true &&
                Parent!.EnableRightDefaultCursor)
            {
                cellState.ClassList.AddOrSkip("e-freezeline");
            }
            else
            {
                cellState.ClassList.Remove("e-freezeline");
            }

            AttributeSelectionInternal(cell!, row, cellState);
        }

        /// <summary>
        /// Applies last row border styling
        /// </summary>
        private void ApplyLastRowBorder(Cell<object> cell, Row<object> row, CellStateInfo cellState)
        {
            // Last row border
            if (Parent != null && Parent.EnableVirtualization)
            {
                bool isLastRowBorderRequired = Parent.Rows?.LastOrDefault() == row &&
                    Parent.Rows?.Count < Parent.PageSettings!.PageSize;
                Parent.RequireLastRowBorder = isLastRowBorderRequired;
            }
            else if (Parent!.EditSettings != null && Parent.EditSettings.ShowAddNewRow && Parent.EditSettings.NewRowPosition == NewRowPosition.Bottom)
            {
                Parent.RequireLastRowBorder = false;
            }

            if (!Parent.RequireLastRowBorder && !Parent.AllowPaging && !Parent.EnableVirtualization &&
                Parent.Rows?.LastOrDefault() == row && row?.IsLastRow == true)
            {
                Parent.RequireLastRowBorder = true;
            }

            // RowSpan logic for last row
            var anchorIndex = row?.Index ?? -1;
            var spanDepth = cell?.RowSpan ?? 1;

            if (row?.IsLastRow == true && Parent.RequireLastRowBorder)
            {
                cellState.ClassList.AddOrSkip("e-lastrowcell");
            }
            else if (cell!.RowSpan > 1 && anchorIndex != -1)
            {
                var lastVisibleIndex = GetLastVisibleDataRowIndexInternal();
                if (lastVisibleIndex != -1 && (anchorIndex + spanDepth - 1) >= lastVisibleIndex)
                {
                    cellState.ClassList.AddOrSkip("e-lastrowcell");
                }
            }
            else
            {
                cellState.ClassList.Remove("e-lastrowcell");
            }
        }

        /// <summary>
        /// Applies visibility and grouping-related classes
        /// </summary>
        private void ApplyVisibility(Cell<object> cell, Row<object> row, CellStateInfo cellState)
        {
            if (cell != null && cell.CellType != CellType.Summary && cell.Column != null)
            {
                bool isGroupedColumn =  Parent!.AllowGrouping  && Parent.GroupSettings!.Columns != null
                    && Parent.GroupSettings.Columns.Any(col =>
                        !string.IsNullOrEmpty(col) && col == cell.Column.Field);

                cell.Visible =
                    (cell.Column.IsHiddenByGrouping && Parent.GroupSettings!.ShowGroupedColumn)
                        ? Parent.GroupSettings.ShowGroupedColumn
                        : isGroupedColumn
                            ? ((cell.Visible && Parent.GroupSettings!.ShowGroupedColumn)
                                ? Parent.GroupSettings!.ShowGroupedColumn
                                : (cell.Column.IsHiddenByGrouping ? cell.Column.Visible : cell.Visible))
                            : cell.Column.Visible;
            }

            if (Parent != null && Parent.AllowGrouping && !Parent.GroupSettings!.ShowGroupedColumn && cell!.CellType == CellType.Indent &&
                row.Cells.Any(c => c.CellType == CellType.Data) && !row.Cells.Any(c => c.CellType == CellType.Data && c.Visible))
            {
                cellState.ClassList.AddOrSkip("e-hide-padding");
            }

            if (cell?.Visible == false)
            {
                cellState.ClassList.AddOrSkip("e-hide");
            }
            else
            {
                cellState.ClassList.Remove("e-hide");
            }
        }

        /// <summary>
        /// Applies edit and dirty state classes
        /// </summary>
        private static void ApplyEditAndDirtyStates(Cell<object> cell, Row<object> row, CellStateInfo cellState)
        {
            if (cell?.IsEdit == true)
            {
                cellState.ClassList.AddOrSkip("e-editedbatchcell");
            }
            else
            {
                cellState.ClassList.Remove("e-editedbatchcell");
            }

            if (cell?.IsDirty == true)
            {
                cellState.CellValue = GridUtils.GetCellValue(cell, row);
                cellState.ClassList.AddOrSkip("e-updatedtd");
            }
            else
            {
                cellState.ClassList.Remove("e-updatedtd");
            }
        }

        /// <summary>
        /// Adds colspan and rowspan attributes if applicable
        /// </summary>
        private static void AddSpanAttributesInternal(Cell<object> cell, CellStateInfo cellState)
        {
            if (cell == null || cell.IsSpanned || cell.IsRowSpanned)
            {
                return;
            }

            if (cell.ColSpan.HasValue && cell.ColSpan > 1 && cell.IsDataCell)
            {
                cellState.Attributes.AddOrUpdateItem("colspan", cell.ColSpan.Value);
                cellState.Attributes.AddOrUpdateItem("aria-colspan", cell.ColSpan.Value);
            }
            else
            {
                cellState.Attributes.Remove("colspan");
                cellState.Attributes.Remove("aria-colspan");
            }

            if (cell.RowSpan.HasValue && cell.RowSpan > 1 && cell.IsDataCell)
            {
                cellState.Attributes.AddOrUpdateItem("rowspan", cell.RowSpan.Value);
                cellState.Attributes.AddOrUpdateItem("aria-rowspan", cell.RowSpan.Value);
            }
            else
            {
                cellState.Attributes.Remove("rowspan");
                cellState.Attributes.Remove("aria-rowspan");
            }
        }

        /// <summary>
        /// Gets the index of the last visible data row
        /// </summary>
        private int GetLastVisibleDataRowIndexInternal()
        {
            if (Parent?.Rows == null)
            {
                return -1;
            }

            var lastVisible = Parent.Rows
                .Where(r => r.IsDataRow && r.Visible)
                .LastOrDefault();

            return lastVisible?.Index ?? -1;
        }

        /// <summary>
        /// Handles cell selection state and aria attributes
        /// </summary>
        private void AttributeSelectionInternal(Cell<object> cell, Row<object> row, CellStateInfo cellState)
        {
            var isRowMode = Parent!.SelectionSettings!.Mode.Equals(SelectionMode.Row);
            var isCellMode = Parent.SelectionSettings.Mode.Equals(SelectionMode.Cell);
            var isBoth = !isRowMode && !isCellMode;

            if (row?.IsDataRow == true && isRowMode && Parent.CheckBoxState.Equals(CheckState.Check) == true &&
                row?.State != "UnSelected" && !(row?.RowType?.Equals("DetailRow", StringComparison.Ordinal) == true))
            {
                row!.IsSelected = true;
            }

            if (isRowMode && (Parent.CheckBoxState.Equals(CheckState.UnCheck) == true))
            {
                if (row?.Index != null && Parent.VirtualScrollModule != null && Parent.VirtualScrollModule!.CurrentGroupedData == null &&
                    Array.IndexOf(Parent.VirtualScrollModule.SelectRowsMethodIndexes, (int)row.Index) == -1)
                {
                    row.IsSelected = false;
                }
            }

            bool isSelectable = GridRowBase<TRow>.IsSelectableInternal(cell);

            if (GridRowBase<TRow>.ShouldSelectCell(cell, row!, isRowMode, isCellMode, isBoth) || isBoth && (Cell?.IsSelected == true))
            {
                if (isRowMode || isBoth)
                {
                    cellState.ClassList.AddOrSkip("e-selectionbackground");
                    cellState.ClassList.AddOrSkip("e-active");
                }

                if (isCellMode || (isBoth && (cell?.IsSelected == true)))
                {
                    cellState.ClassList.AddOrSkip("e-cellselectionbackground");
                }
                else
                {
                    cellState.ClassList.Remove("e-cellselectionbackground");
                }

                if (Parent.AllowRowDragAndDrop)
                {
                    cellState.ClassList.AddOrSkip("e-disableuserselect");
                }

                cellState.Attributes.AddOrUpdateItem("aria-selected", "true");

                if (cell?.Column?.Type == ColumnType.CheckBox)
                {
                    cellState.CellValue = "1";
                }
            }
            else
            {
                cellState.ClassList.Remove("e-cellselectionbackground");
                cellState.ClassList.Remove("e-selectionbackground");
                cellState.ClassList.Remove("e-active");
                cellState.Attributes.Remove("aria-selected");

                if (cell?.Column?.Type == ColumnType.CheckBox)
                {
                    cellState.CellValue = "0";
                }
            }
        }

        /// <summary>
        /// Determines if a cell is selectable
        /// </summary>
        private static bool IsSelectableInternal(Cell<object> cell)
            => cell.CellType.Equals(CellType.Data)
            || cell.CellType.Equals(CellType.Detail)
            || cell.CellType.Equals(CellType.RowDrag)
            || cell.CellType.Equals(CellType.CommandColumn);

        #endregion

        #region Style and Class Application

        /// <summary>
        /// Applies final classes and styles to cell state
        /// </summary>
        internal void ApplyClassAndStylesComplete(Cell<object> cell, CellStateInfo cellState)
        {
            // Join class names
            string classNames = string.Join(" ", cellState.ClassList.ToArray());

            // Get style from attributes
            string setAttributeStyleString = GridUtils.GetStyleAsStringFromObject(cellState.Attributes);
            cellState.StyleText = GridUtils.EnsureUniqueStyles(cellState.StyleText, setAttributeStyleString);

            // Get style from style list
            string styleListText = string.Join(";", cellState.StyleList.ToArray());
            cellState.StyleText = GridUtils.EnsureUniqueStyles(cellState.StyleText, styleListText);

            if (Parent != null && Parent.FreezeModule!.GetFrozenCount() > 0)
            {
                ApplyFrozenColumnStyles(cell, ref classNames, cellState);
            }

            cellState.ClassList = classNames.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        /// <summary>
        /// Applies frozen column specific styles and classes
        /// </summary>
        private void ApplyFrozenColumnStyles(Cell<object> cell, ref string classNames, CellStateInfo cellState)
        {
            var frozenClasses = Parent!.FreezeModule!.ApplyFrozenColumnsClass(cell?.Column!);
            classNames = string.Concat(classNames, frozenClasses);

            if ((cell != null && cell.Index + cell.ColSpan == Parent.FreezeModule!.GetFrozenCount() - Parent.FreezeModule!.GetFreezeRightCount()) &&
                cell.ColSpan > 1 && cell.IsSpanned != true && cell.Column!.Freeze == FreezeDirection.Left)
            {
                classNames = string.Concat(classNames, " ", "e-freezeleftborder");
            }

            var visibleColumns = Parent.Columns?.Where(c => c.Visible)?.ToList();
            if (visibleColumns != null && visibleColumns.Count - (cell!.Index - 1) == Parent.FreezeModule!.GetFreezeRightCount() &&
                cell.ColSpan > 1 && cell.Column != null && cell.Column.Freeze == FreezeDirection.Right)
            {
                classNames = string.Concat(classNames, " ", "e-freezerightborder");
            }

            string frozenStyle = Parent.FreezeModule!.ApplyFrozenColumnsStyles(cell?.Column!)?.ToLower(System.Globalization.CultureInfo.CurrentCulture)!;
            cellState.StyleText = GridUtils.EnsureUniqueStyles(cellState.StyleText, frozenStyle);
        }

        #endregion

        #region Cell Rendering Methods

        /// <summary>
        /// Gets base class names for a cell
        /// </summary>
        internal List<string> GetCellClassNames(Cell<object> cell, Row<object> row, CellStateInfo cellState, bool IsRenderFromTreeGrid = false)
        {
            var baseClasses = GridUtils.CellStaticClasses[cell.CellType].Clone<string>();
            //if (cell?.IsEdit == true && Parent.EditSettings.Mode.Equals(EditMode.Batch) &&
            //    !(Parent.EditSettings.NewRowPosition == NewRowPosition.Bottom))
            //{
            //    return new List<string>();
            //}

            if (Parent != null && Parent.IsRenderedFromTreeGrid && cellState?.ClassList != null && baseClasses != null)
            {
                return cellState.ClassList.Union(baseClasses).ToList();
            }

            return baseClasses ?? new List<string>();
        }

        /// <summary>
        /// Gets style list for a cell (placeholder for future styles)
        /// </summary>
        private static List<string> GetCellStyleList(Cell<object> cell, Row<object> row)
        {
            return new List<string>();
        }

        /// <summary>
        /// Gets base attributes for a cell (placeholder for future attributes)
        /// </summary>
        private static Dictionary<string, object> GetCellAttributes(Cell<object> cell, Row<object> row)
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Ensures correct tab index for cell based on position and state
        /// </summary>
        private int EnsureTabIndexInternal(Row<object> row, Cell<object> cell)
        {
            var firstRow = Parent!.Rows?.FirstOrDefault();
            var lastRow = Parent.Rows?.LastOrDefault();
            bool firstRowFirstVisibleCell = row == firstRow && cell == firstRow?.Cells?.Where(e => e.Visible)?.FirstOrDefault();
            bool lastRowLastVisibleCell = row == lastRow && cell == lastRow?.Cells?.Where(e => e.Visible)?.LastOrDefault();
            bool isFirstOrLastCell = firstRowFirstVisibleCell || lastRowLastVisibleCell;

            if (isFirstOrLastCell && !(Parent.IsEdit || Parent.IsAdd) && !Parent.FocusModule!.ChangeLastCellTabIndex)
            {
                return 0;
            }
            if (Parent != null && Parent.EditSettings!.ShowAddNewRow && !Parent.IsEdit && Parent.IsAdd && isFirstOrLastCell)
            {
                return 0;
            }
            return cell.TabIndex;
        }

        /// <summary>
        /// Determines if the given cell is the last cell in the row
        /// </summary>
        internal bool IsLastCellInternal(Row<object> row, Cell<object> cell)
        {
            Cell<object>? _veryLastCell = row?.Cells?.Where(_ => _.Visible)?.DefaultIfEmpty()?.Last();
            if (_veryLastCell != null)
            {
                return ((Parent != null && Parent.FocusModule!.IsChildFocused && (_veryLastCell.IsTemplate || _veryLastCell.CellType.Equals(CellType.CommandColumn)))
                            || CheckLastDataCellInternal(row!) || (row?.RowType == "Summary" && cell?.AggregateColumn?.GroupFooterTemplate != null && row == Parent!.Rows?.LastOrDefault()))
                            && _veryLastCell.Equals(cell) == true && !(cell?.EditDisabled == true);
            }
            return false;
        }

        /// <summary>
        /// Checks if the row is the last data cell considering grouping
        /// </summary>
        private bool CheckLastDataCellInternal(Row<object> row)
        {
            if (Parent != null && Parent.AllowGrouping && Parent.GroupSettings != null && Parent.GroupSettings.Columns?.Length > 0)
            {
                if (row?.Index == null && row?.IsExpand != true && row == Parent.Rows?.Where(e => e.IsCaptionRow && e.Visible)?.LastOrDefault())
                {
                    return true;
                }
                return row?.Index == Parent.Rows?.Where(e => e.IsDataRow)?.ToList()?.Count - 1;
            }
            else
            {
                return ((row?.Index ?? 0) == Parent!.Rows?.Count - 1);
            }
        }

        #endregion

        #region Frozen Line Cursor Rendering

        /// <summary>
        /// Renders frozen line cursor for template cells
        /// </summary>
        private void RenderFrozenLineCursorTemplate(RenderTreeBuilder builder, int sequence,
            Cell<object> cell, GridCellParameters cellParameters)
        {
            if (cell != null && cell.EnableFrozenLineCursor)
            {
                string cursorClass = cellParameters.IsFrozenRight ?
                    "e-frozen-cursor e-frozen-right-cursor" : "e-frozen-cursor e-frozen-left-cursor";

                if (cell.EnableFixedLeftFrozenLineCursor)
                {
                    cursorClass = "e-frozen-cursor e-frozen-left-cursor";
                }
                else if (cell.EnableFixedRightFrozenLineCursor)
                {
                    cursorClass = "e-frozen-cursor e-frozen-right-cursor";
                }

                if ((cellParameters.IsFrozen.HasValue && cellParameters.IsFrozen == true) ||
                    (cell.EnableFixedLeftFrozenLineCursor || cell.EnableFixedRightFrozenLineCursor))
                {
                    builder.OpenElement(sequence, "div");
                    builder.AddAttribute(sequence + 1, "class", cursorClass);
                    builder.CloseElement();
                }
                else if (Parent!.FreezeModule!.GetFrozenCount() == 0 ||
                         (cellParameters.IsFrozen.HasValue && cellParameters.IsFrozen == false))
                {
                    if (cell.EnableLeftFrozenLineCursor)
                    {
                        cursorClass = "e-frozen-cursor e-frozen-right-cursor e-frozen-default-cursor";
                        if (cell.EnableFrozenResizeCursor)
                        {
                            cursorClass = string.Concat(cursorClass, " e-frozen-resize-cursor");
                        }
                        builder.OpenElement(sequence + 2, "div");
                        builder.AddAttribute(sequence + 3, "class", cursorClass);
                        builder.CloseElement();
                    }
                    else if (cell.EnableRightFrozenLineCursor && Parent.EnableRightDefaultCursor)
                    {
                        cursorClass = "e-frozen-cursor e-frozen-left-cursor e-frozen-default-cursor";
                        if (cell.EnableFrozenResizeCursor)
                        {
                            cursorClass = string.Concat(cursorClass, " e-frozen-resize-cursor");
                        }
                        builder.OpenElement(sequence + 4, "div");
                        builder.AddAttribute(sequence + 5, "class", cursorClass);
                        builder.CloseElement();
                    }
                }
            }
        }

        /// <summary>
        /// Renders frozen line cursor for data cells
        /// </summary>
        private void RenderFrozenLineCursorDataCell(RenderTreeBuilder builder, int sequence,
            Cell<object> cell, GridCellParameters cellParameters)
        {
            if (cell != null && cell.EnableFrozenLineCursor)
            {
                bool IsFrozenRight = cellParameters?.Row?.Cells?.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Right) && x == cell).Any() == true;
                string cursorClass = IsFrozenRight ? "e-frozen-cursor e-frozen-right-cursor" : "e-frozen-cursor e-frozen-left-cursor";
                bool isFrozenRightColumns = cellParameters?.Row?.Cells?.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Right)).Any() == true;
                bool isFrozenLeftColumns = cellParameters?.Row?.Cells?.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Left)).Any() == true;
                bool isFrozenFixedColumns = cellParameters?.Row?.Cells?.Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Fixed)).Any() == true;

                if (Parent != null && Parent.FreezeModule!.GetFrozenCount() == 0)
                {
                    RenderFrozenCursorForMovableColumns(builder, sequence, cell, cursorClass);
                }
                else if (!IsFrozenRight && isFrozenRightColumns && !isFrozenLeftColumns &&
                         !cell.EnableFixedLeftFrozenLineCursor && !cell.EnableFixedRightFrozenLineCursor)
                {
                    if (cell.EnableLeftFrozenLineCursor)
                    {
                        cursorClass = "e-frozen-cursor e-frozen-right-cursor e-frozen-default-cursor";
                        if (cell.EnableFrozenResizeCursor)
                        {
                            cursorClass = string.Concat(cursorClass, " e-frozen-resize-cursor");
                        }
                        builder.OpenElement(sequence, "div");
                        builder.AddAttribute(sequence + 1, "class", cursorClass);
                        builder.CloseElement();
                    }
                }
                else if (!isFrozenRightColumns && cell.EnableFrozenLineCursor &&
                         cell.EnableRightFrozenLineCursor && Parent != null && Parent.EnableRightDefaultCursor)
                {
                    cursorClass = "e-frozen-cursor e-frozen-left-cursor e-frozen-default-cursor";
                    if (cell.EnableFrozenResizeCursor)
                    {
                        cursorClass = string.Concat(cursorClass, " e-frozen-resize-cursor");
                    }
                    builder.OpenElement(sequence + 2, "div");
                    builder.AddAttribute(sequence + 3, "class", cursorClass);
                    builder.CloseElement();
                }
                else if (isFrozenFixedColumns && cell.EnableFrozenLineCursor && cell.EnableFixedLeftFrozenLineCursor)
                {
                    GridRowBase<TRow>.RenderFixedFrozenCursor(builder, sequence, cell);
                }
                else if (isFrozenFixedColumns && cell.EnableFrozenLineCursor && cell.EnableFixedRightFrozenLineCursor)
                {
                    cursorClass = "e-frozen-cursor e-frozen-fixedright-cursor";
                    if (cell.EnableFrozenResizeCursor)
                    {
                        cursorClass = string.Concat(cursorClass, " e-frozen-resize-cursor");
                    }
                    builder.OpenElement(sequence + 4, "div");
                    builder.AddAttribute(sequence + 5, "class", cursorClass);
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement(sequence + 5, "div");
                    builder.AddAttribute(sequence + 6, "class", cursorClass);
                    builder.CloseElement();
                }
            }
        }

        /// <summary>
        /// Renders frozen cursor for movable columns (no frozen columns)
        /// </summary>
        private void RenderFrozenCursorForMovableColumns(RenderTreeBuilder builder, int sequence, Cell<object> cell, string cursorClass)
        {
            if (cell.EnableLeftFrozenLineCursor)
            {
                cursorClass = "e-frozen-cursor e-frozen-right-cursor e-frozen-default-cursor";
                if (cell.EnableFrozenResizeCursor)
                {
                    cursorClass = string.Concat(cursorClass, " e-frozen-resize-cursor");
                }
                builder.OpenElement(sequence, "div");
                builder.AddAttribute(sequence + 1, "class", cursorClass);
                builder.CloseElement();
            }
            else if (cell.EnableRightFrozenLineCursor && Parent != null && Parent.EnableRightDefaultCursor)
            {
                cursorClass = "e-frozen-cursor e-frozen-left-cursor e-frozen-default-cursor";
                if (cell.EnableFrozenResizeCursor)
                {
                    cursorClass = string.Concat(cursorClass, " e-frozen-resize-cursor");
                }
                builder.OpenElement(sequence + 2, "div");
                builder.AddAttribute(sequence + 3, "class", cursorClass);
                builder.CloseElement();
            }
        }

        /// <summary>
        /// Renders fixed frozen cursor with left and optional right cursor
        /// </summary>
        private static void RenderFixedFrozenCursor(RenderTreeBuilder builder, int sequence, Cell<object> cell)
        {
            string cursorClass = "e-frozen-cursor e-frozen-fixedleft-cursor";
            if (cell.EnableDefaultFrozenLine)
            {
                cursorClass = string.Concat(cursorClass, " e-frozen-default-cursor");
            }
            if (cell.EnableFrozenResizeCursor)
            {
                cursorClass = string.Concat(cursorClass, " e-frozen-resize-cursor");
            }
            builder.OpenElement(sequence, "div");
            builder.AddAttribute(sequence + 1, "class", cursorClass);
            builder.CloseElement();

            if (cell.EnableFixedRightFrozenLineCursor)
            {
                cursorClass = "e-frozen-cursor e-frozen-fixedright-cursor";
                builder.OpenElement(sequence + 2, "div");
                builder.AddAttribute(sequence + 3, "class", cursorClass);
                builder.CloseElement();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles cell click events, triggers selection and other cell-level actions
        /// </summary>
        internal async Task CellClickHandlerInternal(MouseEventArgs e, Row<object> row, Cell<object> cell, bool IsCheckBox)
        {
            if (Parent == null || cell == null || row == null)
                return;

            if (Parent.VirtualScrollModule != null && Parent.InfiniteScrollModule != null)
            {
                Parent.VirtualScrollModule.CurrentRowIndex = Parent.InfiniteScrollModule.CurrentRowIndex = row.Index ?? -1;
                Parent.VirtualScrollModule.PreNavigatedIndex = Parent.InfiniteScrollModule.PreRowIndex = 0;
            }

            // Trigger RecordClick event
            if (!IsCheckBox && (Parent.GridEvents?.OnRecordClick.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromFileManager))
            {
                string cacheKey = $"{row.Uid}_{cell.Uid}";
                var cellState = _cellCache.TryGetValue(cacheKey, out CellStateInfo? value) ? value : new CellStateInfo();
                cellState = UpdateCellState(cell, row, cellState);
                cellState.PreviousCell = cell;
                var cellDom = new CellDOM(cellState.ClassList, cellState.StyleList, cellState.Attributes);
                var args = new RecordClickEventArgs<TRow>()
                {
                    CellIndex = (cell.Index ?? -1),
                    RowData = (TRow)row.Data!,
                    RowIndex = (row.Index ?? -1),
                    Column = cell.Column,
                    Parent = Parent,
                    CurrentCell = cellDom,
                    CellValue = GridUtils.GetCellValue(cell, row)
                };

                if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromFileManager)
                {
                    await Parent.EventAggregator.NotifyAsync("RecordClick", args).ConfigureAwait(true);
                }
                else
                {
                    await Parent.GridEvents!.OnRecordClick.InvokeAsync(args).ConfigureAwait(true);

                    _cellCache[cacheKey] = cellState;
                    _cellCache[cacheKey].IsRecordClickEvent = true;

                }
            }

            // Handle edit mode
            if(Parent.EditModule != null){
                bool isHandled = await Parent.EditModule!.HandleCellClickInEditMode(row, cell).ConfigureAwait(true);
                if(isHandled)
                {
                    return;
                }
            }
            // Handle focus
            bool isCheckColumn = cell.Column?.Type == ColumnType.CheckBox;
            if (Parent.FocusModule != null)
            {
                await Parent.FocusModule.CellClickHandler((row, cell), e).ConfigureAwait(true);
            }

            // Handle column virtualization navigation
            if (Parent.EnableColumnVirtualization && !Parent.IsRenderedFromTreeGrid)
            {
                List<GridColumn> columns = Parent.FreezeModule!.GetFrozenCount() > 0 ? Parent.RearrangeColumns(Parent.Columns!) : Parent.Columns!;
                List<GridColumn> visibleColumns = columns!.Where(column => column.Visible).ToList();
                if (Parent.VirtualScrollModule != null)
                {
                    Parent.VirtualScrollModule.SelectedCellNavigation = visibleColumns != null && visibleColumns.Count > 0 ? visibleColumns.IndexOf(cell.Column!) : 0;
                }
            }

            // Handle selection
            if (Parent.AllowSelection && e != null)
            {
                if (isCheckColumn && IsCheckBox && Parent.SelectionModule != null)
                {
                    await Parent.SelectionModule.ClickHandler(e, (row, cell, IsCheckBox)).ConfigureAwait(true);
                }
                else if (!isCheckColumn && IsCheckBox == false && Parent.SelectionModule != null)
                {
                    await Parent.SelectionModule.ClickHandler(e, (row, cell, IsCheckBox)).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Handles keyboard navigation in cells
        /// </summary>
        internal async Task KeyDownHandlerInternal(KeyboardEventArgs e, Row<object> row, Cell<object> cell)
        {
            if (!Parent!.IsRenderedFromTreeGrid || (Parent.IsRenderedFromTreeGrid && !Parent.EnableVirtualization))
            {
                Parent.EventAggregator.Trigger("OnKeyDown", e);
            }

            if (!Parent.EnableVirtualization && !Parent.EnableInfiniteScrolling && Parent.FocusModule != null)
            {
                await Parent.FocusModule.ProcessKeyDown(e, row, cell).ConfigureAwait(true);
            }
            else if (Parent.EnableInfiniteScrolling)
            {
                await HandleInfiniteScrollKeyDown(e, cell).ConfigureAwait(true);
            }
            else
            {
                await HandleVirtualScrollKeyDown(e, cell).ConfigureAwait(true);
            }

            if (Parent != null && Parent.IsRenderedFromTreeGrid && Parent.EnableVirtualization)
            {
                Parent.EventAggregator.Trigger("OnKeyDown", e);
            }
        }

        /// <summary>
        /// Handles keyboard navigation for infinite scrolling
        /// </summary>
        private async Task HandleInfiniteScrollKeyDown(KeyboardEventArgs e, Cell<object> cell)
        {
            if (Parent != null && Parent.InfiniteScrollModule != null &&
                (Parent.InfiniteScrollModule.CurrentRowIndex != Parent.InfiniteScrollModule.PreRowIndex) ||
                Parent?.InfiniteScrollModule?.PreRowIndex == 0)
            {
                List<Row<object>>? AvailableRow = Parent.Rows?.Where(r => r.Index == Parent.InfiniteScrollModule.CurrentRowIndex)?.ToList();
                if (AvailableRow?.Count > 0)
                {
                    Row<object> CurrentRow = AvailableRow[0];
                    Cell<object> CurrentCell = CurrentRow?.Cells?.Where(c => c.Index == cell?.Index)?.ToList()?[0]!;
                    await Parent.FocusModule!.ProcessKeyDown(e, CurrentRow!, CurrentCell).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Handles keyboard navigation for virtual scrolling
        /// </summary>
        private async Task HandleVirtualScrollKeyDown(KeyboardEventArgs e, Cell<object> cell)
        {
            if (Parent != null && Parent.VirtualScrollModule?.CurrentRowIndex != Parent.VirtualScrollModule?.PreNavigatedIndex ||
                Parent!.VirtualScrollModule?.PreNavigatedIndex == 0)
            {
                List<Row<object>> AvailableRow = Parent.Rows.Where(r => r.Index == Parent.VirtualScrollModule!.CurrentRowIndex).ToList();
                if (AvailableRow?.Count > 0)
                {
                    Row<object> CurrentRow = AvailableRow[0];
                    Cell<object> CurrentCell = CurrentRow?.Cells?.Where(c => c.Index == cell?.Index)?.ToList()?[0]!;
                    if (Parent.VirtualScrollModule != null)
                    {
                        Parent.VirtualScrollModule.PreNavigatedIndex = (e.Code == "ArrowDown" || e.Code == "ArrowUp") ?
                        Parent.VirtualScrollModule.CurrentRowIndex : Parent.VirtualScrollModule.PreNavigatedIndex;
                    }
                    if ((e.Code == "ArrowDown" && Parent.VirtualScrollModule?.CurrentRowIndex == (Parent.TotalItemCount - 1) &&
                        Parent.VirtualScrollModule.CurrentRowIndex == Parent.VirtualScrollModule.PreNavigatedIndex) ||
                        (e.Code != "ArrowUp" && e.Code != "ArrowDown"))
                    {
                        Parent.VirtualScrollModule!.PreNavigatedIndex = 0;
                    }
                    if (e.Code == "ArrowUp" && Parent.VirtualScrollModule != null && Parent.VirtualScrollModule.RowStartIndex != 0 &&
                        Parent.VirtualScrollModule.CurrentRowIndex == Parent.VirtualScrollModule.RowStartIndex)
                    {
                        Parent.VirtualScrollModule.PreNavigatedIndex = 0;
                        return;
                    }
                    if (Parent.FocusModule != null)
                    {
                        await Parent.FocusModule.ProcessKeyDown(e, CurrentRow!, CurrentCell).ConfigureAwait(true);
                    }
                }
            }
            else if (Parent.IsRenderedFromTreeGrid && Parent.EnableVirtualization && e.CtrlKey == true &&
                     (e.Code == "ArrowDown" || e.Code == "ArrowUp") && Parent.FocusModule != null)
            {
                await Parent.FocusModule.ProcessKeyDown(e, Parent.Rows?.FirstOrDefault()!, cell).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handles command button click events
        /// </summary>
        internal async Task CommandClickHandler(GridCommandColumn column, Row<object> row, Cell<object> cell)
        {
            if (column == null || row == null) return;

            if (Parent!.GridEvents?.OnRecordClick.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromFileManager)
            {
                var recordClickArgs = new RecordClickEventArgs<TRow>()
                {
                    CellIndex = (cell?.Index ?? -1),
                    RowData = (TRow)row.Data!,
                    RowIndex = (row.Index ?? -1),
                    Column = cell?.Column!,
                    Parent = Parent
                };
                if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromFileManager)
                    await Parent.EventAggregator.NotifyAsync("RecordClick", recordClickArgs).ConfigureAwait(true);
                else if (Parent.GridEvents != null)
                    await Parent.GridEvents.OnRecordClick.InvokeAsync(recordClickArgs).ConfigureAwait(true);
            }

            bool isRowModified = Parent.IsDataModified(row, Parent.EditModule!.CloneData!, column);
            if (Parent.GridEvents?.CommandClicked.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                var args = new CommandClickEventArgs<TRow>()
                {
                    Cancel = false,
                    RowData = (TRow)row.Data!,
                    EditedData = (isRowModified) ? (TRow)Parent.EditModule.CloneData! : default(TRow)!,
                    CommandColumn = new CommandModel()
                    {
                        ButtonOption = column.ButtonOption,
                        Title = column.Title,
                        Type = column.Type,
                        ID = column.ID,
                        Uid = column.Uid
                    },
                    Parent = Parent
                };
                if (Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("CommandClick", args).ConfigureAwait(true);
                else
                    await Parent.GridEvents!.CommandClicked.InvokeAsync(args).ConfigureAwait(true);
                if (args.Cancel)
                    return;
            }

            switch (column.Type)
            {
                case CommandButtonType.Edit:
                    if (!Parent.EditSettings!.Mode.Equals(EditMode.Batch))
                    {
                        await Parent.EditModule.StartEdit(row).ConfigureAwait(true);
                    }
                    break;
                case CommandButtonType.Delete:
                    await Parent.SelectRowAsync(row.Index ?? -1).ConfigureAwait(true);
                    await Parent.DeleteRecordAsync().ConfigureAwait(true);
                    break;
                case CommandButtonType.Save:
                    await Parent.EndEditAsync().ConfigureAwait(true);
                    break;
                case CommandButtonType.Cancel:
                    await Parent.CloseEditAsync().ConfigureAwait(true);
                    break;
            }
        }

        internal string BaseClassName(Cell<object> cell, Row<object>row)
        {
            string classNames = string.Empty;
            string? cacheKey = null!;
            if (Parent != null && Parent.GridEvents?.QueryCellInfo.HasDelegate == true)
            {
                cacheKey = $"{row.Uid}_{cell.Uid}";
            }
            var cellStateInfo = new CellStateInfo();
            var cellState = GetOrUpdateCellState(cacheKey, cell, row);
            classNames = string.Join(" ", cellState.ClassList.ToArray());
            return classNames;
        }
        #endregion

        #region Helper Methods

        private async Task CellDoubleClickHandlerInternal(MouseEventArgs e, Row<object> row, Cell<object> cell)
        {
            await Parent!.EditModule!.DblClickHandler(row, cell).ConfigureAwait(true);
        }

        private async Task ClickHandlerInternal(MouseEventArgs e, Row<object> row, Cell<object> cell)
        {
            if (await Parent!.EditModule!.InvokeSingleClickHandler(row, cell).ConfigureAwait(true))
            {
                return;
            }
            await CellClickHandlerInternal(e, row, cell, false).ConfigureAwait(true);
            if (Parent!.AllowSelection && Parent.Rows?.Find(x => x.Uid == row?.Uid) != null &&
                (Parent.EditSettings?.Mode != EditMode.Batch ||
                 (Parent.EditSettings?.Mode == EditMode.Batch && (e.ShiftKey || e.CtrlKey) && e.Type == "click")))
            {
                await (Parent.FocusModule?.Refresh(row, cell, isCtrlOrShiftKeyPressed: e.CtrlKey || e.ShiftKey)!).ConfigureAwait(true);
            }
        }

        private static bool ShouldSelectCell(Cell<object> cell, Row<object> row, bool isRowMode, bool isCellMode, bool isBoth)
        {
            if (!IsSelectableInternal(cell))
                return false;

            if (isRowMode && row?.IsSelected == true)
                return true;

            if (isCellMode && cell?.IsSelected == true)
                return true;

            if (isBoth)
            {
                bool rowSelectedNotCancelled = row?.IsSelected == true && row?.IsRowSelectionCancelled == false;
                bool cellSelectedWhenCancelled = row?.IsRowSelectionCancelled == true && cell?.IsSelected == true;
                return rowSelectedNotCancelled || cellSelectedWhenCancelled;
            }

            return false;
        }

        private CellStateInfo UpdateCellState(Cell<object> cell, Row<object> row, CellStateInfo cellState)
        {
            cellState.CellValue = GridUtils.GetCellValue(cell, row);
            cellState.ClassList = GetCellClassNames(cell, row, cellState);
            cellState.StyleList = GridRowBase<TRow>.GetCellStyleList(cell, row);
            cellState.Attributes = GridRowBase<TRow>.GetCellAttributes(cell, row);
            cellState.CellDom = new CellDOM(
                new List<string>(cellState.ClassList),
                new List<string>(cellState.StyleList),
                new Dictionary<string, object>(cellState.Attributes)
            );
            return cellState;
        }

        #endregion
    }
}