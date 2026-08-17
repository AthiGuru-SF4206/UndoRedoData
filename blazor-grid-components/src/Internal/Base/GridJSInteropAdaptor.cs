using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Syncfusion.Blazor.Grids;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Internal;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using Syncfusion.Blazor.Data;

namespace Syncfusion.Blazor.Grids.Internal
{
    internal class GridJSInteropAdaptor<T> : ComponentBase, IDisposable
    {
        #region Initialization & Lifecycle
        public void Init() => _dotnetRef = Create();

        private DotNetObjectReference<GridJSInteropAdaptor<T>>? _dotnetRef { get; set; }

        public DotNetObjectReference<GridJSInteropAdaptor<T>> Create() => DotNetObjectReference.Create<GridJSInteropAdaptor<T>>(this);

        public DotNetObjectReference<GridJSInteropAdaptor<T>> GetRef() => _dotnetRef ?? Create();

        public void Dispose() => _dotnetRef?.Dispose();

        internal SfGrid<T> Parent { get; set; }

        //This property is used for ResizeStarted method json serialize options.
        private JsonSerializerOptions _resizeJsonSettings = new JsonSerializerOptions() {
	    PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
        };

        //This property is used for GridKeyDown and EndEdit method json serialization options.
        private JsonSerializerOptions _jsonSettings = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };


        public GridJSInteropAdaptor(SfGrid<T> parent)
        {
            Parent = parent;
        }

        #endregion

        #region Column Management & Configuration
        [JSInvokable]
        public async Task SetMediaColumnVisibility(object args)
        {
            var ColumnArgs = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!);
            List<GridColumn> columns = new List<GridColumn>();
            Parent.IsColumnHeaderChange = true;
            foreach (var uid in ColumnArgs?.MediaColVisibility!.Keys!)
            {
                var column = GridUtils.grabColumnByUidOrField(uid, Parent);
                if (column != null)
                {
                    columns.Add(column);
                }
            }

            foreach (var col in columns)
            {
                if (ColumnArgs.MediaColVisibility!.TryGetValue(col.Uid, out bool value))
                {
                    col.SetVisibility(value);
                }
            }

            if (ColumnArgs.InvokedByMedia)
            {
                Parent.ForceUpdate = true;
                await Parent.CallStateHasChangedAsync().ConfigureAwait(true);
            }
            else
            {
                await Parent.DataProcess().ConfigureAwait(true);
            }

            Parent.IsColumnHeaderChange = false;
        }

        [JSInvokable]
        public async ValueTask ColumnReordered(object? args)
        {
            await Parent.ReorderModule!.ColumnReordered(args).ConfigureAwait(true);
        }

        [JSInvokable]
        public void SetColumnIndexes(int StartColumnIndex, int EndColumnIndex)
        {
            if (Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.StartColumnIndex = StartColumnIndex;
                Parent.VirtualScrollModule.EndColumnIndex = EndColumnIndex;
            }
        }

        [JSInvokable]
        public async Task SetPageSizeAndCIndex(object args)
        {
            ActionArgs? action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!);
            await Parent.PageSettings!.UpdateProperties("PageSize", (int)action!.PageSize).ConfigureAwait(true);
            if (Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.StartColumnIndex = (int)action?.StartColumnIndex!;
                Parent.VirtualScrollModule.EndColumnIndex = (int)action?.EndColumnIndex!;
                Parent.VirtualScrollModule.SetVTableWidth((int)action?.VTableWidth!);
            }
            if (GridUtils.GetColumns(Parent)?.Where(Col => !string.IsNullOrEmpty(Col.HideAtMedia)).ToList().Count == 0 && !Parent.EnablePersistence)
            {
                await Parent.DataProcess().ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task SetMinWidth(IDictionary<string, string> minWidthValues)
        {
            Parent.MinWidth = minWidthValues;
            Parent.EventAggregator.Trigger("ColumnWidthStateChange", null!);
            await Parent.FreezeModule!.InvokeClientFrozenHeight().ConfigureAwait(true);
        }

        [JSInvokable]
        public void CalculateOffSetWidth(int gridWidth)
        {
            if (Parent.GroupModule != null)
            {
                Parent.GroupModule.GridOffsetWidth = gridWidth;
            }

        }
        #endregion

        #region Column Resizing
        [JSInvokable]
        public void ColumnWidthChanged(object args, bool isResizing = false)
        {
            var settings = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
            };
            ActionArgs action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!, settings)!;
            ColumnResizeStop allowStopEvent = JsonSerializer.Deserialize<ColumnResizeStop>(args?.ToString()!, settings);

            if (!string.IsNullOrEmpty(action?.ColumnUid))
            {
                ColumnWidthChanger(action, allowStopEvent, args!, isResizing);
            }
        }

        private async void ColumnWidthChanger(ActionArgs action, ColumnResizeStop columnResizeStop, object args, bool isResizing = false)
        {
            List<GridColumn> columns = GridUtils.GetColumns(Parent);
            GridColumn? column = columns.FirstOrDefault(x => x.Uid == action.ColumnUid);
            column ??= Parent.Columns?.FirstOrDefault(x => x.Uid == action.ColumnUid); // Stacked header resize
            column?.SetWidth(action.Width!.ToString(CultureInfo.InvariantCulture));
            if (columnResizeStop.AllowStopEvent)
            {
                if (Parent.GridEvents?.ResizeStopped.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                {
                    var e = new ResizeArgs() { Column = column!, Parent = Parent };

                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                        await Parent.EventAggregator.NotifyAsync("ResizeStopped", e).ConfigureAwait(true);
                    else
                        await Parent.GridEvents!.ResizeStopped.InvokeAsync(e).ConfigureAwait(true);
                    if (e.Cancel)
                        return;
                }

            }
            if (columnResizeStop.AllowStopEvent && Parent.AllowResizing && Parent.EnablePersistence)
            {
                foreach (var col in Parent.Columns!)
                {
                    col.TableWidth = columnResizeStop.TableWidth;
                    col.LeftFrozenTableWidth = columnResizeStop.LeftFrozenTableWidth;
                    col.RightFrozenTableWidth = columnResizeStop.RightFrozenTableWidth;
                }
                await Parent.SetLocalStorage().ConfigureAwait(true);
            }
            Parent.ForceUpdate = true;
            Parent.EventAggregator.Trigger("ColumnWidthStateChange", args);
            if (Parent.FreezeModule!.GetFrozenCount() > 0)
            {
                if (columnResizeStop.AllowStopEvent)
                {
                    if (((IGrid)Parent).GridTemplates?.DetailTemplate != null)
                    {
                        Parent.SoftRefresh = true;
                    }
                    else
                    {
                        if (Parent?.EditSettings?.Mode == EditMode.Batch && Parent?.Rows?.Any(row => row?.Cells?.Any(cell => cell.IsDirty) == true) == true)
                        {
                            Parent.SoftRefresh = true;
                        }
                        Parent?.EventAggregator.Trigger("ContentStateChanged", null!);
                    }
                }
                else if (isResizing)
                {
                    await Parent.InvokeMethod("sfBlazor.Grid.updateResizeCursor", new object[] { Parent.DataId, false }).ConfigureAwait(true);
                }
            }
            Parent!.ForceUpdate = false;
        }

        [JSInvokable]
        public async ValueTask ResizeStarted(object args)
        {
            ActionArgs action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!)!;
            ColumnResizeStop allowStopEvent = JsonSerializer.Deserialize<ColumnResizeStop>(args?.ToString()!, _resizeJsonSettings);
            if (Parent.FreezeModule!.GetFrozenCount() > 0)
            {
                if (Parent.AllowRowDragAndDrop && Parent.RowReorderModule != null)
                {
                    Parent.RowReorderModule.RowReorderIndentWidth = string.Empty;
                }
                if (Parent.AllowGrouping || ((IGrid)Parent).GridTemplates?.DetailTemplate != null)
                {
                    Parent.GroupModule!.IndentWidth = string.Empty;
                }
            }

            if (allowStopEvent.ColumnList?.Count > 0)
            {
                for (var i = 0; i < allowStopEvent.ColumnList.Count; i++)
                {
                    ColumnWidthChanger(allowStopEvent.ColumnList[i], allowStopEvent, args!);
                }
            }

            if (!string.IsNullOrEmpty(action?.ColumnUid))
            {
                List<GridColumn> columns = GridUtils.GetColumns(Parent);
                GridColumn? column = columns.FirstOrDefault(x => x.Uid == action.ColumnUid);
                if (Parent.GridEvents?.OnResizeStart.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                {
                    var e = new ResizeArgs() { Column = column!, Parent = Parent };

                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                        await Parent.EventAggregator.NotifyAsync("ResizeStart", e).ConfigureAwait(true);
                    else
                        await (Parent.GridEvents?.OnResizeStart.InvokeAsync(e))!.ConfigureAwait(true)!;

                    if (e.Cancel)
                    {
                        await Parent.InvokeMethod("sfBlazor.Grid.preventResizeAction", new object[] { Parent.DataId, true }).ConfigureAwait(true);
                        return;
                    }
                }
                await Parent.InvokeMethod("sfBlazor.Grid.preventResizeAction", new object[] { Parent.DataId, false }).ConfigureAwait(true);
            }
        }

        private struct ColumnResizeStop
        {
            public List<ActionArgs> ColumnList { get; set; }
            public bool AllowStopEvent { get; set; }
            public string TableWidth { get; set; }
            public string LeftFrozenTableWidth { get; set; }
            public string RightFrozenTableWidth { get; set; }
        }
        #endregion

        #region Frozen Columns & Rows
        [JSInvokable]
        public async Task InvokeFreezeLineMoving(object args)
        {
            await Parent.FreezeModule!.InvokeFreezeLineMoving(args).ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task InvokeFreezeLineMoved(object args)
        {
            await Parent.FreezeModule!.InvokeFreezeLineMoved(args).ConfigureAwait(true);

        }

        [JSInvokable]
        public void PreventExtraFrozenCellRendering()
        {
            if (Parent.GroupModule != null)
                Parent.GroupModule.DisableExtraFrozenTd = true;
            Parent.ForceUpdate = true;
            Parent.EventAggregator.Trigger("ContentStateChanged", null!);
            Parent.ForceUpdate = false;
        }
        #endregion

        #region Grouping Operations
        [JSInvokable]
        public async ValueTask GroupColumn(string Field, string Action)
        {
            if (Action == "Group")
            {
                await Parent.GroupModule!.GroupColumn(Field).ConfigureAwait(true);
            }
            else
            {
                await Parent.GroupModule!.UnGroupColumn(Field).ConfigureAwait(true);
            }
        }
        #endregion

        #region Filtering
        [JSInvokable]
        public async Task FilterPopupClose()
        {
            await Parent.FilterModule!.FilterPopupClose().ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task CloseEnhancedOperatorDropdown()
        {
            await Parent.FilterModule!.CloseEnhancedOperatorDropdown().ConfigureAwait(true);
        }

        [JSInvokable]
        public async void FilterMouseOverHandler(string uid, bool showDialog)
        {
            await Parent.FilterModule!.FilterMouseOverHandler(uid, showDialog).ConfigureAwait(true);
        }
        [JSInvokable]
        public void PreventColumnMenuClose(bool IsPrevent)
        {
            Parent.IsColumnMenuFilter = IsPrevent;
        }

        #endregion

        #region Editing & Cell Operations

        [JSInvokable]
        public async void UpdateChanges()
        {
            await Parent.EditModule!.SaveCell().ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task UpdateCell(double rowIndex, string fieldName, string value)
        {
            await Parent.EditModule!.UpdateCopyCell(rowIndex, fieldName, value).ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task UpdateAutofillCell(double rowIndex, string fieldName, string columnName, double valueIndex, string value)
        {
            var Row = Parent.Rows?.FirstOrDefault(_ => _.Index == valueIndex);
            if (Parent.GridEvents?.BeforeAutoFill.HasDelegate == true)
            {
                var args = new BeforeAutoFillEventArgs
                {
                    Cancel = false,
                };
                await Parent.GridEvents.BeforeAutoFill.InvokeAsync(args).ConfigureAwait(true);
                if (args.Cancel)
                {
                    return;
                }
            }
            if (Parent.EditModule != null && (Row?.Data?.GetType() == typeof(System.Dynamic.ExpandoObject) || (Row?.Data is System.Dynamic.DynamicObject)))
            {
                await Parent.EditModule.UpdateCopyCell(rowIndex, fieldName, value, columnName, valueIndex).ConfigureAwait(true);
            }
            else
            {
                await Parent.EditModule!.UpdateAutofillCell(rowIndex, fieldName, columnName, valueIndex).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task UpdateAutofillPositions(object positions, string updateFunction)
        {
            if (Parent.SelectionModule != null)
            {
                await Parent.SelectionModule.HandleAutofillPositionUpdate(positions, updateFunction).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task EndEdit(object? e = null)
        {
            if (Parent.IsRenderedFromGantt) return;
            string? KeyCombination = null;
            if (e != null)
            {
                KeyboardEventArgs? action = JsonSerializer.Deserialize<KeyboardEventArgs>(e.ToString()!, _jsonSettings);
                Parent.EditModule!.KeyCode = action?.Code!;
                Parent.EditModule.IsShiftKey = action?.ShiftKey ?? false;
                KeyCombination = action?.GetKeyCombination();
            }
            if (KeyCombination == "Enter")
            {
                Parent.FocusModule!.SelectedCellIndex = await Parent.EditModule!.GetSelectedCellIndex().ConfigureAwait(true);
            }
            await Parent.EditModule!.EndEdit(null!, false, KeyCombination!).ConfigureAwait(true);
            Parent.EditModule.KeyCode = null!;
            Parent.EditModule.IsShiftKey = false;
        }


        [JSInvokable]
        public void ShowValidationPopup(IDictionary<string, string> position, string arrowPosition)
        {
            foreach (var pos in position)
            {
                Parent.EditModule!.position?.AddOrUpdateItem(pos.Key, pos.Value);
            }

            Parent.EditModule!.ArrowPosition = arrowPosition;
            Parent.EventAggregator.Trigger("ShowValidationMessage", null!);
        }

        [JSInvokable]
        public void RemoveValidationPopup()
        {
            Parent.EditModule!.ClearValidationErrors();
        }

        #endregion

        #region Selection & Focus

        [JSInvokable]
        public void GridFocus(object args)
        {
            args = null!;
            if (Parent.FocusModule != null)
                Parent.FocusModule.ProcessGridFocus(null!);
        }

        [JSInvokable]
        public async Task SelectCellByRow(int rowIndex, double cellIndex)
        {
            Row<object>? row = Parent.Rows?.FirstOrDefault(_ => _.Index == rowIndex);
            if (Parent.SelectionModule != null)
                await Parent.SelectionModule.SelectAutofillCell(row!, cellIndex).ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task ClearSelection()
        {
            if (Parent.SelectionModule != null)
            {
                await Parent.SelectionModule.ClearCellSelection(autofillSelect: true).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task SelectRow(int index, bool isScrollIntoView = false, int focusColumnIndex = -1)
        {
            if (Parent.SelectionModule != null)
            {
                if (isScrollIntoView)
                {
                    await Parent.SelectionModule.SelectRow(index, isSelectionMethodInvoked: true, isScrollIntoView: isScrollIntoView, focusColumnIndex: focusColumnIndex).ConfigureAwait(true);
                }
                else
                {
                    await Parent.SelectionModule.SelectRow(index).ConfigureAwait(true);
                }
            }
        }

        #endregion

        #region Selection - Drag Operations

        [JSInvokable]
        public async Task DragSelection(int StartIndex, int EndIndex, bool ClearAll, string TargetId = null!, int StartCellIndex = 0, int EndCellIndex = 0)
        {
            await Parent.SelectionModule!.DragSelection(StartIndex, EndIndex, ClearAll, TargetId, StartCellIndex, EndCellIndex).ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task DragSelectionStarted(int RowIndex, int CellIndex)
        {
            if (Parent.SelectionModule != null)
            {
                await Parent.SelectionModule.HandleDragSelectionStarting(RowIndex, CellIndex).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task DragCellSelection(int StartIndex, int StartCellIndex, int RowIndex, int CellIndex, bool ClearAll, string TargetId = null!)
        {
            await Parent.SelectionModule!.DragCellSelection(StartIndex, StartCellIndex, RowIndex, CellIndex, ClearAll, TargetId).ConfigureAwait(true);
        }

        #endregion

        #region Row Reordering & Drag-Drop

        [JSInvokable]
        public async Task ReorderRows(int fromIndex, int toIndex, string action, bool isDragWithinGrid, string targetClass, string targetId, object? targetDimension = null, DotNetObjectReference<GridJSInteropAdaptor<T>>? destInstance = null, bool outsideGrid = false, bool differentTValue = false, string? fromUid = null, string? toUid = null, bool withinBothGrid = false, double clientX = 0, double clientY = 0)
        {
            if (Parent.RowReorderModule != null)
            {
                Parent.RowReorderModule.IsReorderByInteraction = true;
                await Parent.RowReorderModule.ReorderRows(fromIndex, toIndex, action, isDragWithinGrid, targetClass, targetId, targetDimension, destInstance, outsideGrid, differentTValue, fromUid, toUid, withinBothGrid: withinBothGrid, clientX: clientX, clientY: clientY).ConfigureAwait(true);
                Parent.RowReorderModule.IsReorderByInteraction = false;
            }
        }

        [JSInvokable]
        public async Task RowDragStartEvent(int fromIndex, string Uid)
        {
            if (Parent.RowReorderModule != null)
            {
                await Parent.RowReorderModule.RowDragStartEvent(fromIndex, Uid).ConfigureAwait(true);
            }
        }


        [JSInvokable]
        public async Task DisableShowAddForm(string requestType, bool isTargetAddForm, DotNetObjectReference<GridJSInteropAdaptor<T>> destInstance = null!)
        {
            if (Parent.RowReorderModule != null)
            {
                Parent.RowReorderModule.HandleAddFormStateOnRowDrag(requestType, isTargetAddForm, destInstance);
            }
        }
        #endregion

        #region UI State Management
        [JSInvokable]
        public void LastRowBorder(bool IsRequired)
        {
            if (Parent.RequireLastRowBorder != IsRequired)
            {
                Parent.RequireLastRowBorder = IsRequired;
                Parent.EventAggregator.Trigger("ContentStateChanged", null!);
            }
        }

        [JSInvokable]
        public void MaximumVisibleRows(int VisibleRows)
        {
            Parent.MaxVisibleRowsCount = VisibleRows;
        }
        #endregion

        #region KeyDown and MouseDown Events
        [JSInvokable]
        public void MouseDownHandler(string Target, string CellUid, string rowUid, int? cellColIndex)
        {
            var row = new Row<object>();
            var cell = new Cell<object>();
            using var column = new GridColumn();
            if ((!string.IsNullOrEmpty(CellUid) || (rowUid != null && cellColIndex != null)) && Target == "Content")
            {
                row = Parent.Rows?.FirstOrDefault(r => r.Uid == rowUid);
                cell = row?.Cells?.FirstOrDefault(c => c.Index + 1 == cellColIndex);
            }

            if (!string.IsNullOrEmpty(CellUid) && Target == "Header")
            {
                row = Parent.HeaderRows.Concat(Parent.FrozenHeaderRows)
                    .Concat(Parent.FrozenRightHeaderRows)
                    .FirstOrDefault(r => r.Cells?.Any(c => c.Uid == CellUid) == true);
                cell = row?.Cells?.FirstOrDefault(c => c.Uid == CellUid);
            }

            if(Parent.EditSettings != null && Parent.EditSettings.Validator != null && Target == "Edit" && Parent.EditSettings.Mode == EditMode.Normal && !(Parent.EditSettings.ShowAddNewRow))
            {
                Parent.PreventEndEdit = true;
            }

            var arguments = new GridMouseDown() { Column = cell?.Column!, Row = row!, Cell = cell!, Target = Target };
            Parent.EventAggregator.Trigger("MouseDown", arguments);
        }

        [JSInvokable]
        public async ValueTask GridKeyDown(object args, bool value, bool isPagerFocused, bool isToolbarFocused, int? cellIndex, int? rowIndex = null, int? templateCellIndex = null, bool focusColumnTemplate = false, bool isMultiSelectPopUpOpened = false)
        {
            KeyboardEventArgs? action = JsonSerializer.Deserialize<KeyboardEventArgs>(args?.ToString()!, _jsonSettings);
            if (Parent.FocusModule != null)
            {
                Parent.FocusModule.isMultiSelectPopUpOpened = isMultiSelectPopUpOpened;
                await Parent.FocusModule.ProcessGridKeyDown(action!, null!, isPagerFocused, isToolbarFocused, cellIndex, rowIndex, templateCellIndex, focusColumnTemplate).ConfigureAwait(true);
            }
        }
        #endregion

        #region Virtualization & Infinite Scroll
        [JSInvokable]
        public async Task VirtualRefresh(object args, int scrollTop = 0, int selectedRowIndex = -1, bool isScrollIntoView = false, int focusColumnIndex = -1, bool frozenMidScroll = false, bool focusFromPager = false, bool isPreventFocusScroll = false)
        {
            ActionArgs? action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!);
            if (Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.NextRowToNavigate = (int)action!.NextRowToNavigate;
                Parent.VirtualScrollModule.ScrollTop = Parent.OverscanCount == 0 ? scrollTop : Parent.VirtualScrollModule.ScrollTop;
                Parent.VirtualScrollModule.RequestType = action?.RequestType!;
            }
            if (action?.RequestType == "virtualscroll" && action?.Axis != "X")
            {
                await Parent.VirtualScrollModule!.HandleVerticalScrollAsync(action!, scrollTop, selectedRowIndex, isScrollIntoView, focusColumnIndex,
                    isPreventFocusScroll).ConfigureAwait(true);
            }
            if (action?.RequestType == "virtualscroll" && action?.Axis == "X" && Parent.VirtualScrollModule != null)
            {
                await Parent.VirtualScrollModule.HandleHorizontalScrollAsync(action, frozenMidScroll, focusFromPager).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task LoadInfiniteData(object args, bool isBottom = false, bool isTop = false, bool isLazyLoadChild = false, string middleRowUid = null!, string lastRowUid = null!, int middleRowIndex = 0, bool keyInteractionScroll = false)
        {
            ActionArgs? action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!);
            if (!isLazyLoadChild && Parent.InfiniteScrollModule != null)
            {
                Parent.InfiniteScrollModule.KeyInteractionScroll = keyInteractionScroll;
                await Parent.InfiniteScrollModule.GenerateInfiniteScrollDatas(action!, isBottom, isTop).ConfigureAwait(true);
                Parent.InfiniteScrollModule.KeyInteractionScroll = false;
            }
            else if (isLazyLoadChild && Parent.InfiniteScrollModule != null)
            {
                Parent.EventAggregator.Trigger("ContentStateChanged", true);
                await Parent.InvokeMethod("sfBlazor.Grid.updateClonedMaskTranslates", new object[] { Parent.DataId }).ConfigureAwait(true);
                await Task.Yield();
                await Parent.InfiniteScrollModule.LoadLazyLoadChildData(action!, middleRowUid, lastRowUid).ConfigureAwait(true);
            }
        }
        #endregion

        #region Clipboard Operations
        [JSInvokable]
        public async Task InvokeCopyPasteAction(object args, string name)
        {        
            BeforeCopyPasteEventArgs? copy = JsonSerializer.Deserialize<BeforeCopyPasteEventArgs>(args.ToString()!);
            if (Parent.GridEvents?.BeforeCopyPaste.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                BeforeCopyPasteEventArgs eventArgs = new BeforeCopyPasteEventArgs()
                {
                    Action = name,
                    Cancel = false,
                    ClipboardText = copy?.ClipboardText!
                };
                if(Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("BeforeCopyPaste", eventArgs).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.BeforeCopyPaste.InvokeAsync(eventArgs))!.ConfigureAwait(true)!;
                await Parent.InvokeMethod("sfBlazor.Grid.preventCopyToClipBoard", new object[] { Parent.DataId, eventArgs.Cancel, eventArgs.ClipboardText, eventArgs.Action }).ConfigureAwait(true);
            }
            else
            {
                await Parent.InvokeMethod("sfBlazor.Grid.preventCopyToClipBoard", new object[] { Parent.DataId, false, copy?.ClipboardText!, name }).ConfigureAwait(true);
            }


        }

        [JSInvokable]
        public async Task InvokePasteAction(object args, int rowIndex, int columnIndex, string columnField)
        {
            BeforeCellPasteEventArgs<T>? paste = JsonSerializer.Deserialize<BeforeCellPasteEventArgs<T>>(args.ToString()!);
            Row<object>? row = Parent.Rows.Find(x => x.Index == rowIndex);
            if (Parent.GridEvents?.BeforeCellPaste.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                BeforeCellPasteEventArgs<T> eventArgs = new BeforeCellPasteEventArgs<T>()
                {
                    Cancel = false,
                    CellValue = paste?.CellValue!,
                    RowIndex = rowIndex,
                    ColumnName = columnField,
                    ColumnIndex = columnIndex,
                    Data = (T)row!.Data!
                };
                if(Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("BeforeCellPaste", eventArgs).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.BeforeCellPaste.InvokeAsync(eventArgs))!.ConfigureAwait(true)!;
                await Parent.InvokeMethod("sfBlazor.Grid.preventPasteAction", new object[] { Parent.DataId, eventArgs.RowIndex, eventArgs.ColumnName, eventArgs.CellValue, eventArgs.ColumnIndex, eventArgs.Cancel }).ConfigureAwait(true);
            }
            else
            {
                await Parent.InvokeMethod("sfBlazor.Grid.preventPasteAction", new object[] { Parent.DataId, rowIndex, columnField, paste?.CellValue!, columnIndex, false }).ConfigureAwait(true);
            }

        }
        #endregion

    }
}