using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles detail row interactions.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    /// <exclude/>
    internal class DetailRow<T>
    {
        #region Private Properties
        private SfGrid<T> Parent { get; set; }

        #endregion

        #region Constructors
        public DetailRow(SfGrid<T> parent)
        {
            Parent = parent;
            parent.EventAggregator.Add("BeforeCellFocus", OnDetailCellFocused);
        }
        #endregion

        #region Detail Row Expansion and Collapse

        internal async Task ExpandOrCollapseAll(bool ExpandAll)
        {
            Action RequestType = Action.ExpandAllComplete;
            if (ExpandAll)
            {
                for (var i = 0; i < Parent.Rows?.Count; i++)
                {
                    if (!Parent.Rows[i].IsExpand && Parent.Rows[i].IsDataRow)
                    {
                        await DetailClick(Parent.Rows[i].Index, Parent.Rows[i].Uid!, true).ConfigureAwait(true);
                    }
                    else if (Parent.Rows[i].IsCaptionRow)
                    {
                        Parent.Rows[i].IsExpand = true;
                    }
                }
            }
            else
            {
                RequestType = Action.CollapseAllComplete;
                for (var i = 0; i < Parent.Rows?.Count; i++)
                {
                    if (Parent.Rows[i].IsExpand && Parent.Rows[i].IsDataRow)
                    {
                        await DetailClick(Parent.Rows[i].Index, Parent.Rows[i].Uid!, true).ConfigureAwait(true);
                    }
                }
            }

            Parent.SoftRefresh = true;
            await Parent.CallStateHasChangedAsync().ConfigureAwait(true);
            if (Parent.GridEvents?.OnActionComplete.HasDelegate == true)
            {
                await Parent.GridEvents.OnActionComplete.InvokeAsync(new ActionEventArgs<T>() { RequestType = RequestType, Parent = Parent }).ConfigureAwait(true);
            }
            else if(Parent.IsRenderedFromTreeGrid)
                await Parent.EventAggregator.NotifyAsync("ActionComplete", new ActionEventArgs<T>() { RequestType = RequestType, Parent = Parent }).ConfigureAwait(true);
        }

        #endregion

        #region Detail Row Click Handling

        internal async Task DetailClick(int? index, string DetailUid = null!, bool ExpandCollapseAll = false)
        {
            Row<object>? clickedRow = Parent.Rows?.Find(_ => _.Uid == DetailUid);
            int detailRowIndex = Parent.Rows!.IndexOf(clickedRow!) + 1;
            var uid = Parent.GetUid("grid-row");
            var Data = new object();
            Data = Parent.Rows?.Find(x => x.Uid == DetailUid)?.Data;
            if (DetailUid != null && !clickedRow?.IsExpand == true)
            {
                if (Parent.GridEvents?.DetailsExpanding.HasDelegate == true)
                {
                    var DetailsExpandingArgs = new DetailsExpandingEventArgs<T>()
                    {
                        Cancel = false,
                        Data = (T)Data!,
                        RowIndex = detailRowIndex,
                        Parent = Parent
                    };
                    await Parent.GridEvents.DetailsExpanding.InvokeAsync(DetailsExpandingArgs).ConfigureAwait(true);
                    if (DetailsExpandingArgs.Cancel)
                    {
                        return;
                    }
                }
                Row<object> row = new Row<object>()
                {
                    RowType = "DetailRow",
                    IsDataRow = true,
                    IsExpand = true,
                    Uid = uid,
                    Index = null!,
                    Data = Data!,
                    IsDetailRow = true,
                    Indent = Parent.AllowGrouping && Parent.GroupSettings?.Columns != null ? Parent.GroupSettings.Columns.Length : 0
                };
                row.Cells = GenerateDetailCellsForRow(row, GridUtils.GetColumns(Parent)?.FindAll(_ => _.Visible).Count); // TODO: visible columns
                DetailRowChanges(true, index, Parent.Rows!);
                Parent.Rows?.Insert(detailRowIndex, row);
                if (Parent.GridEvents?.DetailDataBound.HasDelegate == true)
                {
                    await Parent.GridEvents.DetailDataBound.InvokeAsync(new DetailDataBoundEventArgs<T>() { Data = (T)Data!, Parent = Parent }).ConfigureAwait(true);
                }
            }
            else
            {
                if (Parent.GridEvents?.DetailsCollapsing.HasDelegate == true)
                {
                    var DetailsCollapsingArgs = new DetailsCollapsingEventArgs<T>()
                    {
                        Cancel = false,
                        Data = (T)Data!,
                        RowIndex = detailRowIndex,
                        Parent = Parent
                    };
                    await Parent.GridEvents.DetailsCollapsing.InvokeAsync(DetailsCollapsingArgs).ConfigureAwait(true);
                    if (DetailsCollapsingArgs.Cancel)
                    {
                        return;
                    }
                }
                Parent.Rows?.RemoveAt(detailRowIndex);
                DetailRowChanges(false, index, Parent.Rows!);
            }
            if (!ExpandCollapseAll)
            {
                if (Parent.EnableVirtualization)
                {
                    Parent.EventAggregator.Trigger("VirtualComponentUpdate", null!);
                }
                else
                {
                    Parent.EventAggregator.Trigger("ContentStateChanged", null!);
                }
            }
            if (clickedRow?.IsExpand == true)
            {
                if (Parent.GridEvents?.DetailsExpanded.HasDelegate == true)
                {
                    var DetailsExpandedArgs = new DetailsExpandedEventArgs<T>()
                    {
                        Data = (T)Data!,
                        RowIndex = detailRowIndex,
                        Parent = Parent
                    };
                    await Parent.GridEvents.DetailsExpanded.InvokeAsync(DetailsExpandedArgs).ConfigureAwait(true);
                }
            }

            else if (!clickedRow?.IsExpand == true)
            {
                if (Parent.GridEvents?.DetailsCollapsed.HasDelegate == true)
                {
                    var DetailsCollapsedArgs = new DetailsCollapsedEventArgs<T>()
                    {
                        Data = (T)Data!,
                        RowIndex = detailRowIndex,
                        Parent = Parent
                    };
                    await Parent.GridEvents.DetailsCollapsed.InvokeAsync(DetailsCollapsedArgs).ConfigureAwait(true);
                }
            }
        }

        #endregion

        #region Detail Row State Management

        private static void DetailRowChanges(bool Value, int? Index, List<Row<object>> Rows)
        {
            foreach (var Row in Rows)
            {
                if (Row.Index == Index && Row.Index != -1)
                {
                    Row.IsExpand = Value;
                }

                foreach (var Cell in Row.Cells)
                {
                    if (Cell.CellType == CellType.Detail && Row.Index == Index)
                    {
                        Cell.DetailRowExpand = Value;
                    }
                }
            }
        }

        #endregion

        #region Detail Cell Generation

        private List<Cell<object>> GenerateDetailCellsForRow(Row<object> row, int? colsSpan)
        {
            //List<GridColumn> columns = GridUtils.GetColumns(Parent);
            List<Cell<object>> cells = new List<Cell<object>>();
            for (var j = 0; j < row.Indent; j++)
            {
                cells.Add(new Cell<object>() { CellType = CellType.Indent, Visible = true });
            }

            if (((IGrid)Parent).GridTemplates?.DetailTemplate != null)
            {
                cells.Add(new Cell<object>() { CellType = CellType.DetailIndent, Visible = true });
            }

            if (Parent.AllowRowDragAndDrop)
            {
                colsSpan = colsSpan + 1;
            }

            cells.Add(new Cell<object>() { IsDataCell = true, Visible = true, ColSpan = colsSpan });
            return cells;
        }

        #endregion

        #region Keyboard Event Handling

        private void OnDetailCellFocused(object args) => KeyHandler(args).GetAwaiter();

        private async Task KeyHandler(object args)
        {
            BeforeCellFocus? focus = args as BeforeCellFocus;

            if (((IGrid)Parent).GridTemplates?.DetailTemplate == null || focus!.Cell == null || focus!.Row == null)
            {
                return;
            }

            CellType _type = focus!.Cell.CellType;
            string keyAction = focus.KeyCombination!;
            Row<object> row = focus.Row!;
            switch (keyAction)
            {
                case "Enter":
                    if (_type.Equals(CellType.Detail))
                    {
                        focus.Cancel = true;
                        await DetailClick((int?)row.Index, row.Uid!).ConfigureAwait(true);
                    }

                    break;
                case "CtrlDown":
                    if (!Parent.IsRenderedFromTreeGrid)
                    {
                        await ExpandOrCollapseAll(true).ConfigureAwait(true);
                    }
                    break;
                case "CtrlUp":
                    if (!Parent.IsRenderedFromTreeGrid)
                    {
                        await ExpandOrCollapseAll(false).ConfigureAwait(true);
                    }
                    break;
                case "AltUp":
                    if (row.IsExpand)
                    {
                        await DetailClick((int?)row.Index, row.Uid!).ConfigureAwait(true);
                    }

                    break;
                case "AltDown":
                    if (!row.IsExpand)
                    {
                        await DetailClick((int?)row.Index, row.Uid!).ConfigureAwait(true);
                    }

                    break;
            }
        }

        #endregion

        #region Batch Delete Operations

        /// <summary>
        /// Includes expanded detail rows in the rows list when deleting records in batch mode.
        /// This ensures that detail rows are also deleted when their parent rows are selected for deletion.
        /// </summary>
        /// <param name="rowsToDelete">The list of rows to be deleted (modified in place)</param>
        internal void IncludeDetailRowsInBatchDelete(List<Row<object>>? rowsToDelete)
        {
            if (rowsToDelete == null || Parent.Rows == null)
            {
                return;
            }

            bool hasDetailRows = Parent.Rows.Where(_ => _.IsDetailRow).Any();
            if (!hasDetailRows)
            {
                return;
            }

            var expandRows = rowsToDelete.ToList();
            int counts = expandRows.Count;
            for (int i = 0; i < counts; i++)
            {
                Row<object>? selectedRow = Parent.Rows.Find(_ => (_.Uid == expandRows[i].Uid && _.IsExpand && _.IsSelected));
                int selectedRowIndex = expandRows.IndexOf(selectedRow!) + 1;
                int detailRowIndex = selectedRow != null ? Parent.Rows.IndexOf(selectedRow) + 1 : -1;
                Row<object>? detailedRow = detailRowIndex >= 0 ? Parent.Rows[detailRowIndex] : null;
                if (detailRowIndex >= 0 && Parent.Rows[detailRowIndex].RowType.Equals("DetailRow", StringComparison.Ordinal) == true)
                {
                    rowsToDelete.Insert(selectedRowIndex, detailedRow!);
                }
            }
        }

        #endregion

        #region Focus Navigation

        /// <summary>
        /// Handles focus navigation for detail template elements when Tab or Shift+Tab keys are pressed.
        /// Returns true if detail template focus was handled, false otherwise.
        /// </summary>
        /// <param name="row">The current row</param>
        /// <param name="cell">The current cell</param>
        /// <param name="e">The keyboard event arguments</param>
        /// <param name="previouslyFocusedCell">The previously focused cell to determine if it's a detail cell</param>
        /// <returns>True if detail template focus navigation was handled; otherwise false</returns>
        internal async Task<bool> HandleDetailTemplateFocusNavigationAsync(Row<object> row, Cell<object> cell, KeyboardEventArgs e, Cell<object>? previouslyFocusedCell)
        {
            bool isdetailTemplateFocused = row?.IsDetailRow == true && cell?.IsFocused == true && !Parent.IsRenderedFromTreeGrid;
            bool isDetailTemplateCell = previouslyFocusedCell?.CellType.Equals(CellType.Detail) == true;
            
            if (((IGrid)Parent).GridTemplates?.DetailTemplate != null && isdetailTemplateFocused && row?.IsDetailRow == true && (e.IsTab() || e.IsShiftTab()))
            {
                await Parent.InvokeMethod("sfBlazor.Grid.focusDetailTemplateElements", new object[] { Parent.DataId, e.GetKeyCombination(), isDetailTemplateCell }).ConfigureAwait(true);
                return true;
            }
            
            return false;
        }

        #endregion

        #region Detail Cell Styling

        /// <summary>
        /// Gets inline style for detail cell to support frozen layout positioning.
        /// Calculates left offset based on cell position when columns are frozen.
        /// </summary>
        /// <param name="row">The row containing the cell</param>
        /// <param name="cell">The detail cell to calculate style for</param>
        /// <returns>CSS inline style string for left positioning; empty string if no frozen columns</returns>
        internal string GetDetailCellStyle(Row<object> row, Cell<object> cell)
        {
            if (Parent!.FreezeModule!.GetFrozenCount() == 0)
                return string.Empty;

            int indent = 0;
            var cells = row.Cells;

            for (int i = 0; i < cells.Count; i++)
            {
                if (ReferenceEquals(cells[i], cell))
                {
                    if (i > 0) indent = 30 * i;
                    return $"left: {indent}px";
                }
            }

            return "left: 0px";
        }

        #endregion

        #region Detail Cell Click Handling

        /// <summary>
        /// Handles detail cell click events including focus management, record click events, and detail row expansion.
        /// </summary>
        /// <param name="e">The mouse event arguments</param>
        /// <param name="row">The row containing the clicked detail cell</param>
        /// <param name="cell">The detail cell that was clicked</param>
        internal async Task DetailCellClickHandlerInternal(MouseEventArgs e, Row<object> row, Cell<object> cell)
        {
            var focusModule = Parent?.FocusModule;
            var isRenderedFromTreeGrid = Parent?.IsRenderedFromTreeGrid == true;
            var isRenderedFromFileManager = Parent?.IsRenderedFromFileManager == true;
            focusModule?.ClearCurrent();
            focusModule?.SetCurrent(row, cell, true);

            if (Parent!.GridEvents?.OnRecordClick.HasDelegate == true ||
                isRenderedFromTreeGrid || isRenderedFromFileManager)
            {
                var args = new RecordClickEventArgs<T>()
                {
                    CellIndex = cell.Index ?? -1,
                    RowIndex = row.Index ?? -1,
                    RowData = (T)row.Data!,
                    Column = cell.Column
                };

                if (isRenderedFromTreeGrid || isRenderedFromFileManager)
                    await Parent.EventAggregator.NotifyAsync("RecordClick", args).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.OnRecordClick.InvokeAsync(args)!).ConfigureAwait(true);
            }

            await DetailClick(row?.Index, row?.Uid!).ConfigureAwait(true);

            if (!Parent.AllowSelection && focusModule != null)
            {
                await focusModule.Refresh(row!, cell, isCtrlOrShiftKeyPressed: e.CtrlKey || e.ShiftKey).ConfigureAwait(true);
            }
        }

        #endregion

        #region Detail Row Generation

        /// <summary>
        /// Generates and inserts detail rows for TreeGrid when detail template is enabled.
        /// For each data item in TreeGrid rendering, creates an expanded detail row automatically.
        /// </summary>
        /// <param name="rows">The rows list to which detail rows will be added (modified in place)</param>
        /// <param name="data">The data items to generate detail rows for</param>
        /// <param name="startIndex">The starting index for row generation</param>
        /// <param name="visibleColumnCount">The count of visible columns for cell generation</param>
        /// <param name="item">The data item for the detail row</param>
        internal void GenerateDetailRows(List<Row<object>> rows, IEnumerable<object> data, int startIndex, int visibleColumnCount,object item)
        {
            if (!Parent.IsRenderedFromTreeGrid || data == null)
            {
                return;
            }

            bool hasDetailTemplate = ((IGrid)Parent).GridTemplates?.DetailTemplate != null;
            if (!hasDetailTemplate)
            {
                return;
            }

            int currentStartIndex = startIndex;
            var uid = Parent.GetUid("grid-row");
            var detailRow = new Row<object>
            {
                RowType = "DetailRow",
                IsDataRow = true,
                IsExpand = true,
                Uid = uid,
                Index = null,
                Data = item,
                IsDetailRow = true,
                Indent = 0,
            };

            detailRow.Cells = GenerateDetailCellsForRow(detailRow, visibleColumnCount);
            rows.Add(detailRow);
            rows[currentStartIndex].IsExpand = true;
        }
        #endregion
    }
}
