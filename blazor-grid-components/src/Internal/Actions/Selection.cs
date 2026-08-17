using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Internal;
using Syncfusion.Blazor.Data;
using System.Globalization;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles selection feature of grid.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class Selection<T>
    {
        #region Private Fields - Row and Cell State

        private Row<object> _lastSelectedRow { get; set; }

        private Cell<object> _lastSelectedCell { get; set; }

        private object _lastSelectedCellValue { get; set; }

        private bool _hasCheckBoxColumn { get; set; }

        private bool _isRowDeselectCancelled;

        private bool _isInteracted;

        private bool _isHeaderClicked;

        private string? _primaryKey;

        private Dictionary<object, object> _persistedData { get; set; } = new Dictionary<object, object>();

        private Dictionary<object, object> _filteredOrSearchedData { get; set; } = new Dictionary<object, object>();

        #endregion

        #region Internal Properties - Selection State

        internal bool InvokedFromClient { get; set; }

        internal bool IsBatchModeDoubleClick { get; set; }

        internal bool IsHeaderCheckboxChecked { get; set; }

        internal bool IsInitialSelectionCompleted { get; set; } = true;

        /// <summary>
        /// Gets or sets the starting index of the range selection.
        /// </summary>
        internal int? RangeStartIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the ending index of the range selection.
        /// </summary>
        internal int? RangeEndIndex { get; set; } = -1;

        internal string IsSelectFilteredField { get; set; } = string.Empty;

        internal string IsSelectSearchKey { get; set; } = string.Empty;

        internal Dictionary<object, object> DeSelectedPersistData { get; set; } = new Dictionary<object, object>();

        internal Dictionary<object, object> PersistedData
        {
            get { return _persistedData; }
            set { _persistedData = value; }
        }

        /// <summary>
        /// Gets the cloned selected row records.
        /// </summary>
        /// <remarks>
        /// This property represents a list of cloned records that correspond to the selected rows. Each record is of type TValue.
        /// </remarks>
        internal List<T> ClonedSelectedRowRecords { get; set; }

        #endregion

        #region Public Properties - Parent Component and Metadata

        public SfGrid<T> Parent;

        internal string PrimaryKey
        {
            get
            {
                if (string.IsNullOrEmpty(_primaryKey))
                {
                    return _primaryKey = GridUtils.GetColumns(Parent).Find(_ => _.IsPrimaryKey)?.Field!;
                }

                return _primaryKey;
            }
        }

        internal static readonly string[] sourceArray = new string[] { "CtrlA", "Delete", "ShiftTab" };

        #endregion

        #region Constructor

        public Selection(SfGrid<T> parent)
        {
            Parent = parent;
            parent.EventAggregator.Add("CellFocused", CellFocused);

            _lastSelectedRow = null!;
            _lastSelectedCell = null!;
            _lastSelectedCellValue = null!;
            ClonedSelectedRowRecords = new List<T>();
        }

        #endregion

        #region Initialization Methods

        internal async Task InitializeRowSelection(bool firstRender)
        {
            if (Parent.SelectionModule != null && Parent.SelectedRowIndex > -1 && (Parent.CurrentViewData == null || !Parent.CurrentViewData.Any()) && firstRender)
            {
                IsInitialSelectionCompleted = false;
            }

            if (Parent.SelectedRowIndex != -1 && Parent.CurrentViewData != null && Parent.CurrentViewData.Any() && (firstRender || Parent._rowIndexPropertyChanged || (Parent.SelectionModule != null && !IsInitialSelectionCompleted)))
            {
                Parent._rowIndexPropertyChanged = false;
                if (Parent.SelectionModule != null)
                {
                    IsInitialSelectionCompleted = true;
                }
                if (Parent.SelectedRowIndexes?.IndexOf(Parent.SelectedRowIndex) == -1 && Parent.SelectedRowIndex >= 0)
                {
                    await (Parent.SelectionModule?.SelectRow(Parent.SelectedRowIndex)!).ConfigureAwait(true);
                }
            }
        }

        #endregion

        #region Row Selection Methods

        public async Task SelectRows(object rowIndexes)
        {
            if (!Parent.AllowSelection || Parent.SelectionSettings!.Mode.Equals(SelectionMode.Cell))
            {
                return;
            }

            double[] indexes = GridUtils.ToDoubleArray(rowIndexes);
            await ClearRowSelection().ConfigureAwait(true);
            List<Row<object>>? _dataRows = GetRowsObject()?.Where(_ => _.Visible && _.IsDataRow && !_.RowType.Equals("DetailRow", StringComparison.Ordinal)).ToList();
            int _rowCount = _dataRows?.Count ?? 0;
            bool isSelectionMethodInvoked = false;
            int length = indexes.Length - 1;
            if (Parent.EnableVirtualization && Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.SelectRowsMethodIndexes = indexes;
            }
            foreach (var index in indexes)
            {
                Row<object>? _row;
                if (Parent.EnableVirtualization)
                {
                    _row = _dataRows?.Where(row => row.Index == (int)index).FirstOrDefault();
                    if (index < 0 || _row == null)
                    {
                        continue;
                    }
                }
                else
                {
                    if (!(index > -1 && index < _rowCount))
                    {
                        continue;
                    }
                    _row = _dataRows?[(int)index];
                }
                isSelectionMethodInvoked = index == indexes[length];
                await SelectByRow(_row!, null!, isSelectionMethodInvoked).ConfigureAwait(true);
                isSelectionMethodInvoked = false;
            }
        }

        public async Task SelectRow(double index, Nullable<bool> isToggle = null, bool isSelectionMethodInvoked = false, bool isScrollIntoView = false, int focusColumnIndex = -1)
        {
            if (!Parent.AllowSelection || (Parent.SelectionSettings!.Mode.Equals(SelectionMode.Cell) && Parent.EditSettings != null && Parent.EditSettings.Mode != EditMode.Batch))
            {
                return;
            }

            List<Row<object>> _dataRows = Parent.Rows.Where(_ => _.IsDataRow).ToList();
            List<Row<object>>? _selectedRow = GetRowsObject()?.Where(_ => _.IsSelected).ToList();
            List<Row<object>>? _selectedIndex = _selectedRow?.Where(_ => _.IsDataRow && _.Index == (int)index).ToList();
            if (Parent.EditModule!.ClearSelection)
            {
                await ClearRowSelection().ConfigureAwait(true);
                Parent.SelectedRowIndexes.Clear();
                _persistedData.Clear();
            }
            Row<object> rowToSelect = null!;
            bool hasItem = _dataRows.Any(_ =>
            {
                bool isPresent = _.Index == (int)index;

                if (isPresent)
                {
                    rowToSelect = _;
                }

                return isPresent;
            });
            if (Parent.SelectionSettings.Mode.Equals(SelectionMode.Cell) && Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Batch)
            {
                if (IsBatchModeDoubleClick && (Parent.IsEdit || Parent.IsAdd))
                {
                    Parent.EventAggregator.Trigger("RowStateChanged", rowToSelect);
                }
                return;
            }
            hasItem = isToggle == true ? _selectedIndex!.Count == 0 : hasItem;

            if (IsCheckBoxPersistSelection() && isSelectionMethodInvoked && isToggle != null && isToggle == true && rowToSelect?.IsSelected == true)
            {
                await ClearSelectionByRow(rowToSelect).ConfigureAwait(true);
            }

            if (hasItem)
            {
                await SelectByRow(rowToSelect!, null!, isSelectionMethodInvoked, isScrollIntoView, focusColumnIndex: focusColumnIndex).ConfigureAwait(true);
            }
        }

        public async Task SelectRowsByRange(double startIndex, double? endIndex, bool isSelectionMethodInvoked = false)
        {
            if (!Parent.AllowSelection || IsCellMode())
            {
                return;
            }

            if (!endIndex.HasValue)
            {
                endIndex = Parent.CurrentViewData!.Count() - 1;
            }

            if (!InvokedFromClient)
            {
                await ClearRowSelection().ConfigureAwait(true);
            }

            await SelectRangeOfRows(null!, ((int?)startIndex, (int?)endIndex), null!, isSelectionMethodInvoked).ConfigureAwait(true);
        }

        internal async Task SelectByRow(Row<object> rowToSelect, MouseAndKeyArgs evt = null!, bool isSelectionMethodInvoked = false, bool isScrollIntoView = false, int focusColumnIndex = -1)
        {
            if (rowToSelect != null && !rowToSelect.IsSelected && !rowToSelect.RowType.Equals("DetailRow", StringComparison.Ordinal))
            {
                if (Parent.VirtualScrollModule!.CurrentGroupedData != null && Parent.VirtualScrollModule!.CurrentGroupedData.Count > 0)
                {
                    var dataCount = Parent.VirtualScrollModule!.CurrentGroupedData.Count;
                    for (int i = 0; i < dataCount; i++)
                    {

                        if (Parent.VirtualScrollModule!.CurrentGroupedData[i].Uid == rowToSelect.Uid)
                        {

                            Parent.VirtualScrollModule!.CurrentGroupedData[i].IsSelected = true;
                            break;
                        }
                    }
                }

                var rowIndex = Convert.ToInt32(rowToSelect.Index, CultureInfo.InvariantCulture);
                List<int> rowIndexes = new List<int> { rowIndex };
                List<T> SelectingDatas = new List<T> { (T)rowToSelect.Data! };
                if (Parent.GridEvents?.RowSelecting.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                {
                    var arg = new RowSelectingEventArgs<T>()
                    {
                        Cancel = false,
                        Event = evt?.Click!,
                        Data = (T)rowToSelect.Data!,
                        Datas = SelectingDatas,
                        RowIndexes = rowIndexes,
                        IsCtrlPressed = evt?.CtrlKey ?? false,
                        IsShiftPressed = evt?.ShiftKey ?? false,
                        RowIndex = rowIndex,
                        IsInteracted = _isInteracted,
                        IsHeaderCheckboxClicked = _isHeaderClicked,
                        ForeignKeyData = rowToSelect.ForeignKeyData,
                        Parent = Parent
                    };

                    if (evt != null && evt.Type == "keydown" && evt.IsKeyEvent)
                    {
                        Parent.PreventRender();
                    }
                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                        await Parent.EventAggregator.NotifyAsync("RowSelecting", arg).ConfigureAwait(true);
                    else
                        await (Parent.GridEvents?.RowSelecting.InvokeAsync(arg))!.ConfigureAwait(true)!;

                    if (arg.Cancel)
                    {
                        rowToSelect.IsRowSelectionCancelled = true;
                        return;
                    }
                    rowIndex = arg.RowIndex;
                }

                rowToSelect.IsSelected = true;
                if (!IsBothMode())
                {
                    rowToSelect.Cells.ForEach(cell => cell.IsSelected = true);
                }

                Parent.SelectedRowIndexes.Add(rowIndex);
                if (IsCheckBoxPersistSelection())
                {
                    SetDeSelectPersistData(IsRemove: true, data: rowToSelect.Data!);
                }
                SetHeaderCheckState(rowToSelect);
                Parent.SoftRefresh = true;
                if (isScrollIntoView && Parent.FocusModule != null)
                {
                    Parent.FocusModule.SelectedRowIndex = (int)rowIndex;
                    Parent.FocusModule.SelectedCellIndex = focusColumnIndex != -1 ? focusColumnIndex : 0;
                }
                if (!Parent.IsEdit && Parent.FocusModule?.SelectedCellIndex != null && isSelectionMethodInvoked && Parent.FocusModule != null)
                {
                    await Parent.FocusModule.HandleRowSelectionFocus(rowToSelect, isScrollIntoView, isSelectionMethodInvoked, Parent.IsCellClicked, Parent.IsAdd, Parent.EditModule!.IsCancelAction).ConfigureAwait(true);
                }
                if (evt?.Click != null && Parent.FocusModule != null)
                {
                    Parent.FocusModule.SelectedRowIndex = (int)rowIndex;
                    if (Parent.EnableColumnVirtualization && Parent.VirtualScrollModule != null)
                    {
                        Parent.VirtualScrollModule.SelectedRowNavigation = (int)rowIndex;
                    }
                }

                Parent.EventAggregator.Trigger("RowStateChanged", rowToSelect!);
#pragma warning disable BL0005
                Parent.SelectedRowIndex = rowIndex;
#pragma warning restore BL0005
                List<T> SelectedDatas = UpdateSelectedRecordDatas();

                if (Parent.GridEvents?.RowSelected.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                {
                    var selectedArgs = new RowSelectEventArgs<T>()
                    {
                        Event = evt?.Click!,
                        Data = (T)rowToSelect!.Data!,
                        Datas = SelectedDatas,
                        RowIndexes = rowIndexes,
                        IsCtrlPressed = evt?.CtrlKey ?? false,
                        IsShiftPressed = evt?.ShiftKey ?? false,
                        IsVerticalArrowPressed = evt != null && evt.IsKeyEvent && evt.IsVerticalArrowPressed,
                        PreviousRowIndex = _lastSelectedRow?.Index != null ? Convert.ToInt32(_lastSelectedRow.Index, CultureInfo.InvariantCulture) : -1,
                        RowIndex = Convert.ToInt32(rowToSelect.Index, CultureInfo.InvariantCulture),
                        IsInteracted = _isInteracted,
                        IsHeaderCheckboxClicked = _isHeaderClicked,
                        ForeignKeyData = rowToSelect.ForeignKeyData,
                        Parent = Parent
                    };

                    if (evt != null && evt.Type == "keydown" && evt.IsKeyEvent)
                    {
                        Parent.PreventRender();
                    }
                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                        await Parent.EventAggregator.NotifyAsync("RowSelected", selectedArgs).ConfigureAwait(true);
                    else
                        await (Parent.GridEvents?.RowSelected.InvokeAsync(selectedArgs)!).ConfigureAwait(true);
                }
                if (evt == null || (!evt.ShiftKey && !evt.IsKeyEvent) || (evt.ShiftKey == false && evt.IsKeyEvent))
                {
                    _lastSelectedRow = rowToSelect!;
                }
                bool isMobileDevice = Parent.SyncfusionService.IsDeviceMode;
                bool isAdaptiveMobileMode = Parent.AdaptiveUIMode == AdaptiveMode.Mobile;
                bool isAdaptiveDesktopMode = Parent.AdaptiveUIMode == AdaptiveMode.Desktop;
                bool isVerticalUIMode = Parent.RowRenderingMode == RowDirection.Vertical && ((isAdaptiveMobileMode && isMobileDevice) || (isAdaptiveDesktopMode && !isMobileDevice));
                bool isHorizontalMode = Parent.RowRenderingMode == RowDirection.Horizontal && ((isAdaptiveMobileMode && isMobileDevice) || (isAdaptiveDesktopMode && !isMobileDevice));
                if (Parent.Toolbar != null && Parent.EnableAdaptiveUI && (isVerticalUIMode || isHorizontalMode || Parent.AdaptiveUIMode == AdaptiveMode.Both) && Parent.IsDeleteAction)
                {
                    Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
                }
            }
        }

        #endregion

        #region Row Range Selection Methods

        private List<T> UpdateSelectedRecordDatas()
        {
            List<T> SelectedDatas = new List<T>();
            var SelectedRecord = Parent.Rows?.Where(row => row.IsSelected && row.IsDataRow).ToList();
            if (SelectedRecord != null)
            {
                foreach (var record in SelectedRecord)
                {
                    SelectedDatas.Add((T)record.Data!);
                }
            }

            return SelectedDatas;
        }

        public async Task SelectRangeOfRows(Row<object> endRow, ValueTuple<int?, int?> indexes, MouseAndKeyArgs? e = null, bool isSelectionMethodInvoked = false)
        {
            bool CtrlShiftSelection = e != null && e.CtrlKey && e.ShiftKey;
            int? startIndex = indexes.Item1;
            int? endIndex = indexes.Item2;
            bool? isAtTop = null;
            if (CtrlShiftSelection && _lastSelectedRow != null && !_lastSelectedRow.IsSelected)
            {
                return;
            }

            if (startIndex.HasValue)
            {
                isAtTop = startIndex.Value > endIndex!.Value;
                int? tmpStart = startIndex;
                int? tmpEnd = endIndex;
                startIndex = isAtTop.Value ? tmpEnd : tmpStart;
                endIndex = isAtTop.Value ? tmpStart : tmpEnd;
            }

            if (!startIndex.HasValue && _lastSelectedRow == null)
            {
                await SelectByRow(endRow, e!, isSelectionMethodInvoked).ConfigureAwait(true);
                return;
            }

            isAtTop = isAtTop ?? (_lastSelectedRow?.Index > endRow?.Index);
            startIndex = startIndex ?? (isAtTop.Value ? endRow?.Index : _lastSelectedRow?.Index);
            endIndex = endIndex ?? (isAtTop.Value ? _lastSelectedRow?.Index : endRow?.Index);
            List<Row<object>>? _range = GetRangeofSelectionRows(startIndex, endIndex);
            List<Row<object>> selectedRecords = GetUpdatedSelectedRecords();

            if (isAtTop.Value)
            {
                _range?.Reverse();
            }
            if (_range != null)
            {
                foreach (var record in selectedRecords)
                {
                    if (!_range.Any(row => row.Index == record.Index))
                    {
                        await ClearSelectionByRow(record, e!).ConfigureAwait(true);
                    }
                }
            }
            foreach (var _row in _range!)
            {
                await SelectByRow(_row, e!, isSelectionMethodInvoked).ConfigureAwait(true);
            }
            selectedRecords = GetUpdatedSelectedRecords();
            if (!CtrlShiftSelection && selectedRecords != null)
            {
                if(Parent.VirtualScrollModule!.CurrentGroupedData != null && Parent.VirtualScrollModule!.CurrentGroupedData.Count > 0 && _range.Count != selectedRecords.Count)
                {

                    var data = Parent.VirtualScrollModule!.CurrentGroupedData;
                    var count = data.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var item = data[i];
                        item.IsSelected = false;
                    }

                    var uidSet = _range
                        .Where(r => r.Uid != null)
                        .Select(r => r.Uid)
                        .ToHashSet(StringComparer.Ordinal);

                    for (int i = 0; i < count; i++)
                    {
                        var item = data[i];
                        if (item.Uid != null && uidSet.Contains(item.Uid))
                        {
                            item.IsSelected = true;
                        }
                    }


                }
            }
        }

        private List<Row<object>> GetUpdatedSelectedRecords()
        {
            List<Row<object>> selectedRecords = new List<Row<object>>();

            if (Parent.EnableVirtualization && Parent.VirtualScrollModule != null)
            {
                if (Parent.GroupSettings!.Columns?.ToList().Count > 0)
                {
                    selectedRecords = Parent.Rows?.Where(row => row.IsDataRow && row.IsSelected).ToList()!;
                }
                else
                {
                    foreach (var item in Parent.VirtualScrollModule.GeneratedRows)
                    {
                        Row<object>? row = Parent.VirtualScrollModule.GeneratedRows[item.Key]?.FirstOrDefault();
                        if (row?.IsSelected == true)
                        {
                            selectedRecords.Add(row);
                        }
                    }
                }
            }
            else
            {
                selectedRecords = Parent.Rows?.Where(row => row.IsSelected).ToList()!;
            }

            return selectedRecords;
        }

        private List<Row<object>>? GetRangeofSelectionRows(int? startIndex, int? endIndex, int? startCellIndex = -1, int? endCellIndex = -1)
        {
            List<Row<object>> range = new List<Row<object>>();
            if (Parent.EnableVirtualization && Parent.VirtualScrollModule != null)
            {
                range = Parent.VirtualScrollModule.GetRangeOfVirtualSelectedRows(startIndex, endIndex, startCellIndex, endCellIndex);
            }
            else
            {
                range = Parent.Rows?.Where(row => row.IsDataRow && row.Index >= startIndex!.Value && row.Index <= endIndex!.Value).ToList()!;
            }
            return range;
        }

        #endregion

        #region Click and Keyboard Event Handlers

        public async Task ClickHandler(MouseEventArgs e, ValueTuple<Row<object>, Cell<object>, bool> target)
        {
            MouseAndKeyArgs mk = new MouseAndKeyArgs()
            {
                AltKey = e.AltKey,
                CtrlKey = e.CtrlKey || ((Parent.IsMacDevice ?? false) && e.MetaKey),
                ShiftKey = e.ShiftKey,
                Type = e.Type,
                IsRowStateChanged = true,
                Click = e,
            };
            if (!_hasCheckBoxColumn && !mk.CtrlKey && Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.ShiftSelectionRowIndexes = (-1, -1);
                Parent.VirtualScrollModule.ShiftSelectionCellIndexes = (-1, -1);
            }

            if (IsRowMode())
            {
                await RowSelectionClickHandler(mk, target).ConfigureAwait(true);
                if (Parent.Toolbar != null && Parent.EnableAdaptiveUI && (Parent.EditSettings != null && Parent.EditSettings.AllowDeleting || Parent.EditSettings != null && Parent.EditSettings.AllowEditing))
                {
                    Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
                }
            }

            if (!IsCellMode() && !IsRowMode())
            {
                await RowCellSelectionClickHandler(mk, target).ConfigureAwait(true);
            }

            if (IsCellMode())
            {
                await CellSelectionClickHandler(mk, target).ConfigureAwait(true);
            }
        }

        public void CellFocused(object arg) => KeyHandler(arg).GetAwaiter();

        public async Task KeyHandler(object focus)
        {
            CellFocused? f = (focus as CellFocused);
            var _dRows = Parent.Rows?.Where(_ => _.IsDataRow);
            GridColumn? _col = f?.Cell?.Column;
            if (!Parent.AllowSelection || ((f != null) && (!f.IsKeyEvent)) || ((_dRows != null) && (!_dRows.Any())) || Parent.EditModule!.EditNextCell)
            {
                return;
            }

            if (f != null && f.IsHeader && f.IsJump)
            {
                if (!_col?.Type.Equals(ColumnType.CheckBox) == true && !(f.KeyCombination == "ArrowUp"))
                {
                    await ClearSelection().ConfigureAwait(true);
                    return;
                }
            }

            if ((IsSingle() && !_hasCheckBoxColumn) && (f?.KeyCombination == "ShiftDown" || f?.KeyCombination == "ShiftUp"))
            {
                return;
            }

            KeyboardEventArgs e = f!.KeyArgs!;
            string combination = f.KeyCombination!;
            bool _isProcessed = false;

            if (string.IsNullOrEmpty(f.Action)
                && sourceArray.Contains(combination))
            {
                _isProcessed = true;
            }

            ValueTuple<Row<object>?, Cell<object>?, bool> target = (f.Row, f.Cell, false);
            MouseAndKeyArgs mk = new MouseAndKeyArgs()
            {
                AltKey = e.AltKey,
                CtrlKey = e.CtrlKey,
                ShiftKey = e.ShiftKey && !e.IsShiftEnter() && !e.IsShiftTab(),
                Type = e.Type,
                IsKeyEvent = f.IsKeyEvent,
                IsRowStateChanged = f.IsRowChanged,
                IsVerticalArrowPressed = !(e.CtrlKey || e.ShiftKey) && !string.IsNullOrEmpty(e.Key) && (e.Key == "ArrowUp" || e.Key == "ArrowDown")
            };

            // Space key in checkbox column toggle row selection
            if (e.IsSpace() && _col?.Type.Equals(ColumnType.CheckBox) == true)
            {
                mk.IsRowStateChanged = true;
            }

            await KeyPressed(_isProcessed, combination, _dRows!, f).ConfigureAwait(true);

            if (_isProcessed || f.Cell?.CellType.Equals(CellType.Indent) == true)
            {
                return;
            }

            List<string> keyCombination = new List<String> { "Escape", "PageUp", "PageDown", "AltPageUp", "AltPageDown", "CtrlAltPageUp", "CtrlAltPageDown" };
            if (!keyCombination.Contains(combination) && !f.IsHeader && f.Row?.Index != null && f.Row.IsDataRow)
            {
                if (IsRowMode())
                {
                    await RowSelectionClickHandler(mk, target!).ConfigureAwait(true);
                }

                if (!IsCellMode() && !IsRowMode())
                {
                    await RowCellSelectionClickHandler(mk, target!).ConfigureAwait(true);
                }

                if (IsCellMode())
                {
                    await CellSelectionClickHandler(mk, target!).ConfigureAwait(true);
                }
            }
            else
            {     
                if (IsGroupOrSummaryCell(f.Cell)&& IsGroupOrSummaryRow(f.Row)&& _lastSelectedRow?.IsDataRow == true&& _lastSelectedRow?.IsSelected == true && !keyCombination.Contains(combination))
                {
                    await Parent.ClearSelectionAsync().ConfigureAwait(true);
                }
                _lastSelectedRow = Parent.Rows?[(int)Parent.FocusModule?.SelectedRowIndex!]!;
            }
        }
        private static bool IsGroupOrSummaryCell(Cell<object>? cell)
        {
            return cell?.CellType switch
            {
                CellType.Expand or CellType.GroupCaption or CellType.Summary or CellType.CaptionSummary => true,
                _ => false
            };
        }
        private static bool IsGroupOrSummaryRow(Row<object>? row)
        {
            return row?.RowType switch
            {
                "GroupCaption" or "Summary" => true,
                _ => false
            };
        }
        private async Task KeyPressed(bool _isProcessed, string combination, IEnumerable<Row<object>> _dRows, CellFocused f)
        {
            switch (combination)
            {
                case "Escape":
                    if (Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection && Parent.EditModule!.ClearSelection)
                    {
                        Parent.EditModule.ClearSelection = false;
                        return;
                    }

                    _isProcessed = true;
                    await ClearSelection().ConfigureAwait(true);
                    break;
                case "CtrlA":
                    if (Parent.SelectionSettings != null && Parent.SelectionSettings.Type == SelectionType.Single)
                    {
                        return;
                    }

                    _isProcessed = true;

                    if (IsRowMode() || IsBothMode())
                    {
                        await SelectRowsByRange(0, _dRows?.Count() - 1).ConfigureAwait(true);
                    }
                    else
                    {
                        await SelectCellsByRange((0, 0), (_dRows.AsQueryable().Count() - 1, (int)_dRows.AsQueryable().Last().Cells.Last().Index!)).ConfigureAwait(true);
                    }

                    break;
                case "CtrlHome":
                case "CtrlEnd":
                    if (!f.IsHeader && !Parent.SelectionSettings!.PersistSelection)
                    {
                        await ClearSelection().ConfigureAwait(true);
                    }
                    break;
            }
        }

        #endregion

        #region Row Selection Event Handlers

        public async Task RowSelectionClickHandler(MouseAndKeyArgs e, ValueTuple<Row<object>?, Cell<object>?, bool> target)
        {
            Row<object> row = target.Item1!;
            Cell<object> cell = target.Item2!;
            bool IsCheckBox = target.Item3;
            GridSelectionSettings? _settings = Parent.SelectionSettings;
            _isInteracted = true;
            if (_settings != null && _settings.CheckboxOnly && IsCheckBox == false)
            {
                return;
            }

            if (_settings != null && _settings.CheckboxOnly && !IsSingle())
            {
                if (row?.IsSelected == true && CanToggle())
                {
                    await ClearSelectionByRow(row, e).ConfigureAwait(true);
                }
                else
                {
                    await SelectByRow(row!, e).ConfigureAwait(true);
                }
            }
            else if (_settings != null && (_settings.EnableSimpleMultiRowSelection && !IsSingle()) || HasCheckBoxColumn(IsCheckBox, e))
            {
                if (e?.ShiftKey == true)
                {
                    await SelectRangeOfRows(row!, (null!, null!), e).ConfigureAwait(true);
                }
                else if (row?.IsSelected == true && CanToggle() && e?.IsRowStateChanged == true)
                {
                    await ClearSelectionByRow(row, e).ConfigureAwait(true);
                }
                else if (_settings != null && _settings.CheckboxMode == CheckboxSelectionType.ResetOnRowClick && row?.IsSelected == true && !CanToggle() && e?.IsRowStateChanged == true)
                {
                    await ClearSelectionByRow(row, e).ConfigureAwait(true);
                }
                else
                {
                    await SelectByRow(row!, e!).ConfigureAwait(true);
                }
            }
            else if (!IsSingle() && (e.CtrlKey || e.ShiftKey))
            {
                if (e.ShiftKey)
                {
                    await SelectRangeOfRows(row!, (null!, null!), e).ConfigureAwait(true);
                }
                else if (row?.IsSelected == true && CanToggle() && e?.IsRowStateChanged == true)
                {
                    await ClearSelectionByRow(row, e).ConfigureAwait(true);
                }
                else
                {
                    await SelectByRow(row!, e!).ConfigureAwait(true);
                }
            }
            else
            {
                await ValidateRowSelectionClick(row!, e).ConfigureAwait(true);
            }

            _isInteracted = false;
        }

        private async Task ValidateRowSelectionClick(Row<object> row, MouseAndKeyArgs e)
        {
            if ((row?.IsSelected == true && CanToggle()) || row?.IsSelected != true)
            {
                if (row?.IsSelected == true && CanToggle() && e?.IsRowStateChanged == true)
                {
                    if (Parent.EditModule!.IsPersistSelection() && Parent.IsEdit)
                    {
                        return;
                    }
                    await ClearRowSelection(evt: e).ConfigureAwait(true);
                }
                else if (row?.IsSelected != true)
                {
                    await ClearRowSelection(evt: e!).ConfigureAwait(true);
                    await SelectByRow(row!, e!).ConfigureAwait(true);
                }
            }
            else
            {
                if (CanToggle() && e?.IsRowStateChanged == true)
                {
                    await ClearRowSelection(row, e).ConfigureAwait(true);
                }

                if (row?.IsSelected == true && e != null && e.IsRowStateChanged && !CanToggle() && Parent.SelectionSettings != null && Parent.SelectionSettings.CheckboxMode == CheckboxSelectionType.ResetOnRowClick)
                {
                    await ClearRowSelection(evt: e).ConfigureAwait(true);
                }

                await SelectByRow(row!, e!).ConfigureAwait(true);
            }
        }

        #endregion

        #region Header Checkbox Event Handler

        public async Task HeaderClickHandler(MouseEventArgs e, CheckState state)
        {
            _isInteracted = true;
            _isHeaderClicked = true;
            List<GridFilterColumn>? gridFilteredColumns = Parent.FilteredColumns;
            // Check if filtering or searching is active
            bool hasFilter = gridFilteredColumns?.Count > 0;
            bool hasSearch = !string.IsNullOrEmpty(Parent.SearchSettings?.Key);
            // Ensure early exit if selection persistence or checkbox column is not enabled.
            if (Parent.SelectionSettings != null && !Parent.SelectionSettings.PersistSelection && !HasCheckBoxColumn())
            {
                return;
            }
            if (IsCheckBoxPersistSelection())
            {
                if ((hasFilter || hasSearch) && !IsHeaderCheckboxChecked)
                {
                    IsHeaderCheckboxChecked = true;
                    IsSelectFilteredField = hasFilter ? gridFilteredColumns!.LastOrDefault()!.Field : string.Empty;
                    IsSelectSearchKey = hasSearch ? Parent.SearchSettings?.Key! : string.Empty;
                }
                else if (!hasFilter && !hasSearch)
                {
                    IsHeaderCheckboxChecked = state == CheckState.UnCheck;
                }
            }
            List<Row<object>>? _dataRows = Parent.Rows?.Where(_ => _.IsDataRow && !_.RowType.Equals("DetailRow", StringComparison.Ordinal)).ToList();
            if (_dataRows != null && _dataRows.Count == 0)
            {
                return;
            }

            List<T> RowDatas = await Parent.GetCurrentViewRecordsAsync().ConfigureAwait(true);
            List<int>? RowIndexs = _dataRows?.Select(_ => (int)_.Index!).ToList();
            Row<object>? _row = _dataRows?[0];
            Row<object>? _lastrow = _dataRows?[_dataRows.Count - 1];
            if (state.Equals(CheckState.UnCheck))
            {
                if (Parent.GridEvents?.RowSelecting.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                {
                    var arg = new RowSelectingEventArgs<T>()
                    {
                        Cancel = false,
                        Data = (T)_row?.Data!,
                        Datas = RowDatas,
                        RowIndexes = RowIndexs!,
                        Event = e,
                        IsCtrlPressed = e?.CtrlKey ?? false,
                        IsShiftPressed = e?.ShiftKey ?? false,
                        RowIndex = Convert.ToInt32(_row?.Index, CultureInfo.InvariantCulture),
                        IsInteracted = _isInteracted,
                        IsHeaderCheckboxClicked = _isHeaderClicked,
                        ForeignKeyData = _row?.ForeignKeyData!,
                        Parent = Parent
                    };
                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                        await Parent.EventAggregator.NotifyAsync("RowSelecting", arg).ConfigureAwait(true);
                    else
                        await (Parent.GridEvents?.RowSelecting.InvokeAsync(arg)!).ConfigureAwait(true);
                    if (arg.Cancel)
                    {
                        _isHeaderClicked = false;
                        return;
                    }
                }

            }
            else if (state.Equals(CheckState.Check) || state.Equals(CheckState.Intermediate))
            {
                if (Parent.GridEvents?.RowDeselecting.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                {
                    RowDatas = Parent.SelectedRecords;
                    RowIndexs = Parent.SelectionModule?.GetRowsObject()!.Where(_ => _.IsSelected && _.Index.HasValue).Select(x => (int)x.Index!).ToList<int>()!;
                    var selectingArgs = new RowDeselectEventArgs<T>()
                    {
                        Cancel = false,
                        Event = e,
                        Data = (T)_lastrow?.Data!,
                        Datas = RowDatas,
                        RowIndexes = RowIndexs,
                        RowIndex = Convert.ToInt32(_lastrow?.Index, CultureInfo.InvariantCulture),
                        IsInteracted = _isInteracted,
                        IsHeaderCheckboxClicked = _isHeaderClicked,
                        ForeignKeyData = _lastrow?.ForeignKeyData!,
                        Parent = Parent
                    };
                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                        await Parent.EventAggregator.NotifyAsync("RowDeSelecting", selectingArgs).ConfigureAwait(true);
                    else
                        await (Parent.GridEvents?.RowDeselecting.InvokeAsync(selectingArgs)!).ConfigureAwait(true);

                    if (selectingArgs.Cancel)
                    {
                        _isHeaderClicked = false;
                        return;
                    }
                }

            }

            Parent.SelectedRowIndexes = new List<int>();
            if (state.Equals(CheckState.UnCheck))
            {
                //Check state to intermediate when persist selection is false in virtual grid
                Parent.CheckBoxState = Parent.DataSource == null && Parent.EnableVirtualization && GetTotalCount() != Parent.TotalItemCount && Parent.SelectionSettings != null && !Parent.SelectionSettings.PersistSelection ? CheckState.Intermediate : CheckState.Check;
                Parent.SelectedRowIndexes = _dataRows?.Select(_ => (int)_.Index!).ToList()!;
                var filteredDataList = _filteredOrSearchedData;
                if (IsHeaderCheckboxChecked && (gridFilteredColumns?.Count > 0 || Parent.SearchSettings?.Key?.Length > 0) && DeSelectedPersistData.Count > 0)
                {
                    foreach (var item in filteredDataList)
                    {
                        SetDeSelectPersistData(IsRemove: true, data: item.Value);
                    }
                }
            }
            else if (state.Equals(CheckState.Intermediate))
            {
                ResetPersistSelection();
                if (Parent.VirtualScrollModule != null)
                {
                    Parent.VirtualScrollModule.ShiftSelectionRowIndexes = (-1, -1);
                    Parent.VirtualScrollModule.ShiftSelectionCellIndexes = (-1, -1);
                    Parent.VirtualScrollModule.SelectRowsMethodIndexes = Array.Empty<double>();
                }

            }
            else
            {
                ResetPersistSelection();
            }
            bool isGroupedVirtualization = Parent.EnableVirtualization && Parent.VirtualScrollModule!.CurrentGroupedData?.Count > 0;
            if (Parent.CheckBoxState.Equals(CheckState.Check) || (Parent.DataSource == null && Parent.EnableVirtualization && Parent.VirtualScrollModule != null && Parent.CheckBoxState.Equals(CheckState.Intermediate)))
            {
                // To update GeneratingRows when SelectAll is changed to intermediate by RowClick
                Parent.VirtualScrollModule!.IsSelAllChangedByRowClick = true;
                if ((Parent.FilterSettings != null && Parent.FilterSettings.Columns?.Count > 0) || !string.IsNullOrEmpty(Parent.SearchSettings?.Key))
                {
                    Parent.VirtualScrollModule!.IsSelectAllWithFilter = true;
                }
                if (isGroupedVirtualization)
                {
                    foreach(var item in Parent.VirtualScrollModule!.CurrentGroupedData!)
                    {
                        item.IsSelected = true;
                    }
                }
            }
            else if (Parent.CheckBoxState.Equals(CheckState.UnCheck) && Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.IsSelAllChangedByRowClick = false;
                Parent.VirtualScrollModule.IsSelectAllWithFilter = false;
                if (isGroupedVirtualization)
                {
                    foreach (var item in Parent.VirtualScrollModule!.CurrentGroupedData!)
                    {
                        item.IsSelected = false;
                    }
                }
            }

            SetPersistData(state: Parent.CheckBoxState, filterValue: Parent.CurrentFilteredRecords?.Count() ?? 0);
            UpdateCBoxSelection(Parent.CheckBoxState);
            Parent.SoftRefresh = true;
            Parent._shouldRender = IsCheckBoxPersistSelection() && Parent.IsEdit ? true : Parent._shouldRender;
            if (Parent.EnableVirtualization)
            {
                Parent.EventAggregator.Trigger("VirtualComponentUpdate", null!);
            }
            else
            {
                Parent.EventAggregator.Trigger("ContentStateChanged", null!);
            }

            bool isDirectCheckBoxSelected = Parent.CheckBoxState.Equals(CheckState.Check);

            bool isIntermediateState = _isHeaderClicked && Parent.SelectedRecords.Count > 0 
                && Parent.CheckBoxState.Equals(CheckState.Intermediate);

            bool hasValidSelectionSettings = Parent.SelectionSettings != null && !Parent.SelectionSettings.PersistSelection;

            bool isRemoteAdaptor = (Parent.DataManager?.DataAdaptor?.IsRemote() ?? false) || Parent.DataManager?.Adaptor == Adaptors.CustomAdaptor;
            
            bool isRowSelected = isDirectCheckBoxSelected 
                || (isIntermediateState && Parent.EnableVirtualization && hasValidSelectionSettings && isRemoteAdaptor);
            
            if (isRowSelected)        
            {
                if (Parent.GridEvents?.RowSelected.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                {
                    RowDatas = Parent.SelectedRecords;
                    var selectedArgs = new RowSelectEventArgs<T>()
                    {
                        Event = e!,
                        Data = (T)_row?.Data!,
                        Datas = RowDatas,
                        RowIndexes = RowIndexs!,
                        PreviousRowIndex = 0,
                        RowIndex = Convert.ToInt32(_row?.Index, CultureInfo.InvariantCulture),
                        IsInteracted = _isInteracted,
                        IsHeaderCheckboxClicked = _isHeaderClicked,
                        ForeignKeyData = _row?.ForeignKeyData!,
                        Parent = Parent
                    };
                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                        await Parent.EventAggregator.NotifyAsync("RowSelected", selectedArgs).ConfigureAwait(true);
                    else
                        await (Parent.GridEvents?.RowSelected.InvokeAsync(selectedArgs)!).ConfigureAwait(true);
                }
            }
            else if (Parent.CheckBoxState.Equals(CheckState.UnCheck))
            {
                if (Parent.GridEvents?.RowDeselected.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                {
                    RowDatas = Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection && Parent.DataSource != null ? GetCurrentViewData()?.ToList()! : RowDatas;
                    var selectedArgs = new RowDeselectEventArgs<T>()
                    {
                        Cancel = false,
                        Event = e!,
                        Data = (T)_lastrow?.Data!,
                        Datas = RowDatas,
                        RowIndexes = RowIndexs!,
                        RowIndex = Convert.ToInt32(_lastrow?.Index, CultureInfo.InvariantCulture),
                        IsInteracted = _isInteracted,
                        IsHeaderCheckboxClicked = _isHeaderClicked,
                        ForeignKeyData = _lastrow?.ForeignKeyData!,
                        Parent = Parent
                    };
                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                        await Parent.EventAggregator.NotifyAsync("RowDeSelected", selectedArgs).ConfigureAwait(true);
                    else
                        await (Parent.GridEvents?.RowDeselected.InvokeAsync(selectedArgs)!).ConfigureAwait(true);
                }
            }

            _isInteracted = false;
            _isHeaderClicked = false;
        }

        #endregion

        #region Persist Collection Management Methods

        /// <summary>
        /// Updates the persisted selection collection based on the current filter and search settings.
        /// </summary>
        private void UpdatePersistCollection()
        {
            bool hasFilterOrSearch = Parent.FilteredColumns?.Count > 0 || !string.IsNullOrEmpty(Parent.SearchSettings?.Key);
            if (Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection && IsHeaderCheckboxChecked && hasFilterOrSearch)
            {
                var FilteredData = _filteredOrSearchedData;
                foreach (var rowData in FilteredData)
                {
                    SetDeSelectPersistData(isAdd: true, data: rowData.Value);
                    object? key = Parent.PropHelper?.GetObject(PrimaryKey, rowData.Value);
                    if (key != null)
                    {
                        _persistedData?.Remove(key);
                    }
                }
            }
            else
            {
                DeSelectedPersistData.Clear();
            }
        }

        #endregion

        #region Clear Selection Methods

        public async Task ClearSelection()
        {
            if (!IsRowMode() && !IsBothMode())
            {
                await ClearCellSelection().ConfigureAwait(true);
            }
            else if (IsBothMode())
            {
                await ClearCellSelection().ConfigureAwait(true);
                await ClearRowSelection().ConfigureAwait(true);
            }
            else
            {
                await ClearRowSelection().ConfigureAwait(true);
                if (Parent.Toolbar != null && Parent.EnableAdaptiveUI && (Parent.EditSettings != null && Parent.EditSettings.AllowEditing || Parent.EditSettings != null && Parent.EditSettings.AllowDeleting))
                {
                    Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
                }
            }
        }

        public async Task ClearRowSelection(Row<object>? except = null, MouseAndKeyArgs? evt = null)
        {
            if (Parent.AllowGrouping && Parent.GroupSettings != null && Parent.GroupSettings.Columns?.ToList().Count > 0 && Parent.VirtualScrollModule!.CurrentGroupedData != null && Parent.VirtualScrollModule!.CurrentGroupedData.Count > 0 && Parent.EnableVirtualization)
            {
                foreach (var group in Parent.VirtualScrollModule!.CurrentGroupedData)
                {
                    if (!group.IsCaptionRow)
                    {
                        group.IsSelected = false;

                    }
                }
            }
            List<Row<object>> _dataRows = GetRowsObject()!.Where(_ => _.IsSelected || (_.IsDetailRow && _.IsDirty && _.IsExpand)).ToList();
            Row<object> _lastRow = null!;
            List<int> rowIndexes = new List<int>();
            List<T> rowDatas = new List<T>();
            var selectedRowDatas = Parent.SelectedRecords;
            var selectedRowIndexs = Parent?.SelectionModule?.GetRowsObject()?.Where(_ => _.IsSelected && _.Index.HasValue).Select(x => (int)x.Index!).ToList<int>();
            foreach (Row<object> _row in _dataRows)
            {
                rowIndexes.Add(Convert.ToInt32(_row.Index, CultureInfo.InvariantCulture));
                rowDatas.Add((T)_row.Data!);
                List<int> currentRowIndex = new List<int> { Convert.ToInt32(_row.Index, CultureInfo.InvariantCulture) };
                List<T> currentRowData = new List<T> { (T)_row.Data! };
                if (_row.Equals(except))
                {
                    continue;
                }

                if (_row != null && Parent != null && Parent.SelectionSettings != null && Parent.SelectionSettings.AllowDragSelection && Parent.SelectionSettings.PersistSelection)
                {
                    object? key = Parent.PropHelper?.GetObject(PrimaryKey, _row.Data!);
                    if (key != null)
                    {
                        _persistedData.Remove(key);
                    }
                }

                if (Parent?.GridEvents?.RowDeselecting.HasDelegate == true || Parent!.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                {
                    var selectingArgs = new RowDeselectEventArgs<T>()
                    {
                        Cancel = false,
                        Event = evt?.Click!,
                        Data = (T)_row!.Data!,
                        Datas = currentRowData,
                        RowIndexes = currentRowIndex,
                        RowIndex = Convert.ToInt32(_row.Index, CultureInfo.InvariantCulture),
                        IsCtrlPressed = evt?.CtrlKey ?? false,
                        IsShiftPressed = evt?.ShiftKey ?? false,
                        IsInteracted = _isInteracted,
                        IsHeaderCheckboxClicked = _isHeaderClicked,
                        ForeignKeyData = _row.ForeignKeyData,
                        Parent = Parent
                    };

                    if (evt != null && evt.Type == "keydown" && evt.IsKeyEvent)
                    {
                        Parent.PreventRender();
                    }
                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                        await Parent.EventAggregator.NotifyAsync("RowDeSelecting", selectingArgs).ConfigureAwait(true);
                    else
                        await Parent.GridEvents!.RowDeselecting.InvokeAsync(selectingArgs).ConfigureAwait(true);
                    if (selectingArgs.Cancel)
                        return;
                }

                if (except == null)
                {
                    Parent.CheckBoxState = CheckState.UnCheck;
                    if (Parent.SelectionSettings != null && !Parent.SelectionSettings.AllowDragSelection && !Parent.SelectionSettings.PersistSelection)
                    {
                        _persistedData.Clear();
                    }
                }
                if (_row != null)
                {
                    _row.IsSelected = false;
                    if (!IsBothMode()) { _row.Cells?.ForEach(_ => _.IsSelected = false); }
                }
                if (Parent.SelectedRowIndexes.Count != 0 && _row != null && _row.Index != null)
                {
                    Parent.SelectedRowIndexes.Remove((int)_row.Index);
                }

                if (_row != null && _row.Index != null)
                {
                    int deleteIndex = Array.IndexOf(Parent.VirtualScrollModule!.SelectRowsMethodIndexes, (int)_row.Index);
                    if (deleteIndex != -1)
                    {
                        Parent.VirtualScrollModule.SelectRowsMethodIndexes = Parent.VirtualScrollModule.SelectRowsMethodIndexes.Where((val, idx) => idx != deleteIndex).ToArray();
                    }
                }

                _lastRow = _row!;
                SetHeaderCheckState(_row!);
                Parent.SoftRefresh = true;
                currentRowIndex.Clear();
                currentRowData.Clear();
#pragma warning disable BL0005
                Parent.SelectedRowIndex = -1;
#pragma warning restore BL0005
                Parent.EventAggregator.Trigger("RowStateChanged", _row!);
            }
            if (Parent != null && Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.ShiftSelectionRowIndexes = (-1, -1);
            }
            if (Parent != null && Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection && Parent.SelectionModule != null && Parent.SelectionModule.PersistedData != null)
            {
                Parent.CheckBoxState = CheckState.UnCheck;
                Parent.SelectionModule.PersistedData.Clear();
                IsHeaderCheckboxChecked = false;
            }

            if (_lastRow != null)
            {
                if (Parent?.GridEvents?.RowDeselected.HasDelegate == true || (Parent != null && Parent.IsRenderedFromTreeGrid) || (Parent != null && Parent.IsRenderedFromPivotTable) || (Parent != null && Parent.IsRenderedFromFileManager))
                {
                    var selectedArgs = new RowDeselectEventArgs<T>()
                    {
                        Cancel = false,
                        Event = evt?.Click!,
                        Data = (T)_lastRow.Data!,
                        Datas = selectedRowDatas,
                        RowIndexes = selectedRowIndexs!,
                        RowIndex = Convert.ToInt32(_lastRow.Index, CultureInfo.InvariantCulture),
                        IsCtrlPressed = evt?.CtrlKey ?? false,
                        IsShiftPressed = evt?.ShiftKey ?? false,
                        IsInteracted = _isInteracted,
                        IsHeaderCheckboxClicked = _isHeaderClicked,
                        ForeignKeyData = _lastRow.ForeignKeyData,
                        Parent = Parent
                    };
                    if (evt != null && evt.Type == "keydown" && evt.IsKeyEvent)
                    {
                        Parent.PreventRender();
                    }
                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                        await Parent.EventAggregator.NotifyAsync("RowDeSelected", selectedArgs).ConfigureAwait(true);
                    else
                        await (Parent.GridEvents?.RowDeselected.InvokeAsync(selectedArgs))!.ConfigureAwait(true)!;
                }
            }
            _lastSelectedRow = null!;
        }

        #endregion

        #region Individual Row Deselection Methods

        public async Task ClearSelectionByRow(Row<object> selectedRow = null!, MouseAndKeyArgs? evt = null)
        {
            _isRowDeselectCancelled = false;
            Row<object> _row = selectedRow;
            if (_row?.IsSelected != true)
            {
                return;
            }
            if (Parent.VirtualScrollModule!.CurrentGroupedData != null && Parent.VirtualScrollModule!.CurrentGroupedData.Count > 0)
            {
                var dataCount = Parent.VirtualScrollModule!.CurrentGroupedData.Count;
                for (int i = 0; i < dataCount; i++)
                {
                    if (Parent.VirtualScrollModule!.CurrentGroupedData[i].Uid == selectedRow.Uid)
                    {
                        Parent.VirtualScrollModule!.CurrentGroupedData[i].IsSelected = false;
                        break;
                    }
                }
            }
            List<int> rowIndexes = new List<int> { Convert.ToInt32(selectedRow?.Index, CultureInfo.InvariantCulture) };
            List<T> rowDatas = new List<T> { (T)selectedRow?.Data! };
            if (Parent.GridEvents?.RowDeselecting.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
            {
                var selectingArgs = new RowDeselectEventArgs<T>()
                {
                    Cancel = false,
                    Event = evt?.Click!,
                    Data = (T)_row.Data!,
                    Datas = rowDatas,
                    RowIndexes = rowIndexes,
                    RowIndex = Convert.ToInt32(_row.Index, CultureInfo.InvariantCulture),
                    IsCtrlPressed = evt?.CtrlKey ?? false,
                    IsShiftPressed = evt?.ShiftKey ?? false,
                    IsInteracted = _isInteracted,
                    IsHeaderCheckboxClicked = _isHeaderClicked,
                    ForeignKeyData = _row.ForeignKeyData,
                    Parent = Parent
                };
                if (evt != null && evt.Type == "keydown" && evt.IsKeyEvent)
                {
                    Parent.PreventRender();
                }
                if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                    await Parent.EventAggregator.NotifyAsync("RowDeSelecting", selectingArgs).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.RowDeselecting.InvokeAsync(selectingArgs))!.ConfigureAwait(true)!;
                if (selectingArgs.Cancel)
                {
                    _isRowDeselectCancelled = true;
                    return;
                }
            }
            _row.IsSelected = false;
            if (!IsBothMode()) { _row.Cells?.ForEach(_ => _.IsSelected = false); }
            Parent.SelectedRowIndexes?.Remove((int)_row.Index!);
            if (IsCheckBoxPersistSelection())
            {
                SetDeSelectPersistData(isAdd: true, data: _row.Data!);
            }

#pragma warning disable BL0005
            Parent.SelectedRowIndex = -1;
#pragma warning restore BL0005
            if (_row.Index != null && Parent.VirtualScrollModule != null)
            {
                int deleteIndex = Array.IndexOf(Parent.VirtualScrollModule.SelectRowsMethodIndexes, (int)_row.Index);
                if (deleteIndex != -1)
                {
                    Parent.VirtualScrollModule.SelectRowsMethodIndexes = Parent.VirtualScrollModule.SelectRowsMethodIndexes.Where((val, idx) => idx != deleteIndex).ToArray();
                }
            }
            if (Parent.SelectionSettings != null && Parent.SelectionSettings.CellSelectionMode != CellSelectionMode.Box)
            {
                SetHeaderCheckState(_row);
            }
            Parent.SoftRefresh = true;

            Parent.EventAggregator.Trigger("RowStateChanged", _row);

            if (Parent.GridEvents?.RowDeselected.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
            {
                var selectedArgs = new RowDeselectEventArgs<T>()
                {
                    Cancel = false,
                    Event = evt?.Click!,
                    Data = (T)_row.Data!,
                    Datas = rowDatas,
                    RowIndexes = rowIndexes,
                    RowIndex = Convert.ToInt32(_row.Index, CultureInfo.InvariantCulture),
                    IsCtrlPressed = evt?.CtrlKey ?? false,
                    IsShiftPressed = evt?.ShiftKey ?? false,
                    IsInteracted = _isInteracted,
                    IsHeaderCheckboxClicked = _isHeaderClicked,
                    ForeignKeyData = _row.ForeignKeyData,
                    Parent = Parent
                };

                if (evt != null && evt.Type == "keydown" && evt.IsKeyEvent)
                {
                    Parent.PreventRender();
                }
                if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable || Parent.IsRenderedFromFileManager)
                    await Parent.EventAggregator.NotifyAsync("RowDeSelected", selectedArgs).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.RowDeselected.InvokeAsync(selectedArgs))!.ConfigureAwait(true)!;
            }
        }

        #endregion

        #region Header Check State Methods

        public void SetHeaderCheckState(Row<object> row = null!, string requestType = null!)
        {
            if (Parent.DataSource == null)
            {
                SetPersistData(targetRow: row, state: Parent.CheckBoxState, requestType: requestType);
            }
            else
            {
                CheckState? checkState = requestType == "Searching" ? Parent.CheckBoxState : null;
                string? currentRequestType = requestType == "Searching" ? requestType : null;
                SetPersistData(targetRow: row, state: checkState, requestType: currentRequestType);
            }

            int selectedCount = Parent.SelectedRecords.Count;
            int totalCount = GetTotalCount();
            List<T>? filteredRecords = Parent.CurrentFilteredRecords?.ToList();
            int filterSelectedCount = 0;
            if (filteredRecords != null)
            {
                int count = 0;
                foreach (T item in Parent.SelectedRecords)
                {
                    if (filteredRecords.Any(e => e!.Equals(item)))
                    {
                        ++count;
                    }
                }
                filterSelectedCount = count;
                if (Parent.IsRenderedFromTreeGrid && Parent.SelectedRecords != null && selectedCount > 0 && filteredRecords.Count > 0)
                {
                    filterSelectedCount = selectedCount;
                }
            }
            int gridTotalItemCount = Parent.TotalItemCount;
            List<GridFilterColumn>? filteredColumns = Parent.FilteredColumns;
            string? searchKey = Parent.SearchSettings?.Key;
            var filteredDataList = _filteredOrSearchedData;

            bool persistSelection = Parent.SelectionSettings!.PersistSelection;
            bool filterDataExist = filteredDataList != null && filteredDataList.Count > 0;
            bool allFilteredDataIsDeselected = filterDataExist && filteredDataList!.All(filterdata => IsDataInDeselectedCollection(filterdata.Value));
            bool allFilteredDataIsPersisted = filterDataExist && filteredDataList!.All(fiterdata => IsDataInPersistedCollection(fiterdata.Value)) && filteredDataList?.Count == totalCount;
            int deselectedPersistDataCount = DeSelectedPersistData.Count;
            bool nonPersistedCount = !persistSelection && HasCheckBoxColumn() && Parent.Rows?.Count > 0 && Parent.Rows?.Count == selectedCount;
            //Local data all items are selected
            bool isLocalAllItemsSelected = Parent.DataSource != null && ((deselectedPersistDataCount == 0 && IsHeaderCheckboxChecked) || totalCount == selectedCount
                || (nonPersistedCount));
            //Remote data normal grid selection
            bool isRemoteAllItemsSelected = Parent.DataSource == null && ((deselectedPersistDataCount == 0 && IsHeaderCheckboxChecked)
                || (selectedCount == gridTotalItemCount)
                || (nonPersistedCount));
            //Remote data filtered value all selected
            bool filterGridNoDeselectedRecord = IsRemoteDataPersistSelection() && deselectedPersistDataCount == 0 && totalCount > 0
                && ((totalCount == selectedCount && allFilteredDataIsPersisted) || allFilteredDataIsPersisted || (IsHeaderCheckboxChecked && (filteredColumns?.Count > 0 || searchKey?.Length > 0)));
            //Any of the filter data is persisted in collection
            bool isAnyOfFilterDataPersisted = filterDataExist && filteredDataList != null && filteredDataList.Any(filterdata => IsDataInPersistedCollection(filterdata.Value));
            //Filter grid is selected in persist list
            bool isFilterSomeRecordsSelected = (filteredRecords != null && isAnyOfFilterDataPersisted && ((deselectedPersistDataCount > 0 && !allFilteredDataIsDeselected)
                || deselectedPersistDataCount == 0));
            //Some records are selected in remote data persist selection for normal and filter grid
            bool isRemoteSomeRecordsSelected = IsRemoteDataPersistSelection() &&
                (filteredRecords == null && (deselectedPersistDataCount > 0 || selectedCount != gridTotalItemCount))
                || isFilterSomeRecordsSelected;

            bool batchModeAllRowsDeleted = HasCheckBoxColumn() && Parent.EditSettings!.Mode == EditMode.Batch && Parent.Rows?.All(x => x.Action == EditAction.Deleted) == true;

            if ((totalCount != 0 && !batchModeAllRowsDeleted && ((isLocalAllItemsSelected) || (isRemoteAllItemsSelected)) && filteredRecords == null && searchKey?.Length == 0)
                || (totalCount != 0 && ((filteredRecords != null || searchKey?.Length > 0) && (((totalCount != selectedCount && !allFilteredDataIsDeselected
                || (persistSelection && totalCount == selectedCount)) && totalCount == filterSelectedCount)
                || filterGridNoDeselectedRecord))))
            {
                int gridPageSize = Parent.PageSettings!.PageSize;
                int? virtualSelectionRowIndexesItem1 = Parent.VirtualScrollModule?.ShiftSelectionRowIndexes.Item1;
                int? virtualSelectionRowIndexesItem2 = Parent.VirtualScrollModule?.ShiftSelectionRowIndexes.Item2;
                if (!Parent.EnableVirtualization || (Parent.EnableVirtualization && (virtualSelectionRowIndexesItem1 == -1
                    || ((virtualSelectionRowIndexesItem1 == 0 && Parent.VirtualScrollModule?.RowEndIndex > gridTotalItemCount - gridPageSize)
                    || (virtualSelectionRowIndexesItem2 == gridTotalItemCount - 1 && Parent.VirtualScrollModule?.RowStartIndex < gridPageSize)
                    || (virtualSelectionRowIndexesItem1 < gridPageSize && virtualSelectionRowIndexesItem2 > gridTotalItemCount - gridPageSize))
                    )))
                {
                    Parent.CheckBoxState = CheckState.Check;
                }
            }

            else if (selectedCount > 0 && !batchModeAllRowsDeleted && ((Parent.DataSource != null && (filteredRecords == null || isFilterSomeRecordsSelected || !persistSelection || (persistSelection && Parent.IsRenderedFromTreeGrid)))
                || isRemoteSomeRecordsSelected
                || (Parent.DataSource == null && (!persistSelection || (!HasCheckBoxColumn() && persistSelection)))))
            {
                Parent.CheckBoxState = CheckState.Intermediate;
            }
            else if ((selectedCount == 0 && (Parent.DataSource != null || (Parent.DataSource == null && !persistSelection && HasCheckBoxColumn()))) ||
                 IsRemoteDataPersistSelection() && ((selectedCount == 0 && deselectedPersistDataCount == gridTotalItemCount) || (selectedCount == 0 && !IsHeaderCheckboxChecked)))
            {
                Parent.CheckBoxState = CheckState.UnCheck;
                _persistedData?.Clear();
            }
            else if ((filteredRecords != null && filterSelectedCount == 0 && !IsHeaderCheckboxChecked)
                || (filteredDataList != null && filteredDataList.Count > 0
                && (IsHeaderCheckboxChecked && allFilteredDataIsDeselected && filteredDataList.Count == totalCount)) || Parent.TotalItemCount == 0)
            {
                Parent.CheckBoxState = CheckState.UnCheck;
            }

            SetPersistData(state: Parent.CheckBoxState, filterValue: filterSelectedCount);
        }

        #endregion

        #region Persist Data Storage Methods

        internal void ResetHeaderCheckboxOnSearchingAndRefresh(string requestType)
        {
            if (requestType == "Searching" && IsCheckBoxPersistSelection() && DeSelectedPersistData.Count == 0
                && PersistedData.Count > 0 && IsHeaderCheckboxChecked && Parent.SearchSettings?.Key.Length == 0
                && !string.IsNullOrEmpty(IsSelectSearchKey))
            {
                IsHeaderCheckboxChecked = false;
                IsSelectSearchKey = string.Empty;
            }
            if (requestType == "Refresh" && IsHeaderCheckboxChecked)
            {
                IsHeaderCheckboxChecked = false;
            }
        }

        public void SetPersistData(Row<object>? targetRow = null, CheckState? state = null, int filterValue = 0, string? requestType = null)
        {
            var _helper = Parent.PropHelper;
            if (Parent.SelectionSettings != null && !Parent.SelectionSettings.PersistSelection)
            {
                _persistedData?.Clear();
                return;
            }

            if (state != null && targetRow == null)
            {
                if ((state.Equals(CheckState.Check) && IsHeaderCheckboxChecked) || ((!_isInteracted || _isHeaderClicked) && (IsHeaderCheckboxChecked) && Parent.DataSource == null && Parent.CheckBoxState == CheckState.Intermediate))
                {
                    var _list = GetCurrentViewData();
                    if (_list != null)
                    {
                        foreach (var item in _list)
                        {
                            object? key = _helper?.GetObject(PrimaryKey, item);
                            if (IsRemoteDataPersistSelection() && IsDataInDeselectedCollection(item!) && Parent.CheckBoxState == CheckState.Check)
                            {
                                SetDeSelectPersistData(IsRemove: true, data: item!);
                            }
                            if (Parent.DataSource != null || (Parent.DataSource == null && !IsDataInDeselectedCollection((object)item!)))
                            {
                                _persistedData?.AddOrUpdateItem(key!, item!);
                            }
                        }
                    }
                }
                bool clearSearching = requestType != null && requestType.Equals("Searching", StringComparison.Ordinal) && Parent.SearchSettings?.Key?.Length == 0;
                bool clearFiltering = requestType != null && requestType.Equals("ClearFiltering", StringComparison.Ordinal);
                bool hasFilter = Parent.FilteredColumns?.Count > 0;
                bool hasSearch = Parent.SearchSettings?.Key?.Length > 0;

                bool isUncheckStateWithNoFilters = state.Equals(CheckState.UnCheck) &&
                                       (Parent.FilteredColumns == null || Parent.FilteredColumns.Count == 0) && Parent.SearchSettings?.Key?.Length == 0;

                bool isNoFilteredRecords = (Parent.CurrentFilteredRecords == null &&
                                            !(clearFiltering || clearSearching)) || filterValue != 0;

                bool isValidDataSource = HasCheckBoxColumn() && !IsHeaderCheckboxChecked && (Parent.DataSource != null || Parent.DataSource == null);

                bool shouldClearForRemotePersist = IsCheckBoxPersistSelection() && Parent.TotalItemCount == 0 && Parent.SelectedRecords.Count > 0 &&
                                                   _persistedData?.Count > 0;

                if ((isUncheckStateWithNoFilters && isNoFilteredRecords && isValidDataSource) || (shouldClearForRemotePersist && !hasFilter && !hasSearch)
                    || (Parent.PropertyChanges.Count > 0 && Parent.PropertyChanges.ContainsKey(nameof(Parent.DataSource)) && Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection))
                {
                    _persistedData?.Clear();
                }
            }

            if (targetRow != null)
            {
                object? key = _helper?.GetObject(PrimaryKey, targetRow.Data!);
                if (key != null)
                {
                    if (targetRow.IsSelected)
                    {
                        _persistedData?.AddOrUpdateItem(key, targetRow.Data!);
                    }
                    else
                    {
                        _persistedData?.Remove(key);
                    }
                }
            }
        }

        internal void SetDeSelectPersistData(bool isAdd = false, bool IsRemove = false, object data = null!)
        {
            if (Parent.SelectionSettings != null && !Parent.SelectionSettings.PersistSelection)
            {
                return;
            }
            object? key = Parent.PropHelper?.GetObject(PrimaryKey, data);
            if (key != null)
            {
                if (isAdd)
                {
                    DeSelectedPersistData.AddOrUpdateItem(key, data);
                }
                if (IsRemove)
                {
                    DeSelectedPersistData.Remove(key);
                }
            }
        }

        #endregion

        #region Selection Mode Validation Methods

        public bool IsSingle() => Parent.SelectionSettings!.Type.Equals(SelectionType.Single);

        public bool IsRowMode() => Parent.SelectionSettings != null && Parent.SelectionSettings.Mode.Equals(SelectionMode.Row);

        public bool IsCellMode() => Parent.SelectionSettings != null && Parent.SelectionSettings.Mode.Equals(SelectionMode.Cell);

        internal bool IsBothMode() => Parent.SelectionSettings != null && Parent.SelectionSettings.Mode.Equals(SelectionMode.Both);

        #endregion

        #region Selection Mode Configuration Methods

        public bool CanToggle() => Parent.SelectionSettings != null && Parent.SelectionSettings.EnableToggle;

        public bool IsCellFlow() => Parent.SelectionSettings != null && Parent.SelectionSettings.CellSelectionMode.Equals(CellSelectionMode.Flow);

        public bool IsCellBox() => Parent.SelectionSettings != null && Parent.SelectionSettings.CellSelectionMode.Equals(CellSelectionMode.Box);

        private bool IsResetOnRowClick() => Parent.SelectionSettings != null && Parent.SelectionSettings.CheckboxMode.Equals(CheckboxSelectionType.ResetOnRowClick);

        #endregion

        #region CheckBox Column Detection Methods

        public bool HasCheckBoxColumn(bool IsFromCheckBox = false, MouseAndKeyArgs? e = null)
        {
            bool returnValue = false;

            returnValue = _hasCheckBoxColumn = GridUtils.GetColumns(Parent)
                    ?.Find(_ => _.Type.Equals(ColumnType.CheckBox)) != null;

            if (_hasCheckBoxColumn && !IsFromCheckBox
                && Parent.SelectionSettings != null && Parent.SelectionSettings.CheckboxMode == CheckboxSelectionType.ResetOnRowClick
                && (e != null && !e.CtrlKey && !e.ShiftKey))
            {
                // If row is clicked without special keys then act like single selection.
                returnValue = false;
            }

            return returnValue;
        }

        #endregion

        #region Cell Selection Core Methods

        public async Task SelectCell(ValueTuple<int, int> cellIndex, bool isSelectionMethodInvoked = false)
        {
            if (!Parent.AllowSelection)
            {
                return;
            }

            if (IsRowMode())
            {
                return;
            }

            List<Row<object>>? _dataRows = Parent.Rows?.Where(_ => _.IsDataRow).ToList();
            await ClearCellSelection().ConfigureAwait(true);
            Row<object> rowToSelect = null!;
            bool hasItem = _dataRows?.Any(_ =>
            {
                bool isPresent = _.Index == (int)cellIndex.Item1;
                if (isPresent)
                {
                    rowToSelect = _;
                }

                return isPresent;
            }) ?? false;

            if (hasItem)
            {
                await SelectCellByRow(rowToSelect, cellIndex.Item2, null!, isSelectionMethodInvoked).ConfigureAwait(true);
            }
        }

        public async Task SelectCells(ValueTuple<int, int>[] rowCellIndexes, bool isSelectionMethodInvoked = false)
        {
            if (!Parent.AllowSelection || IsRowMode())
            {
                return;
            }

            ValueTuple<int, int>[] indexes = rowCellIndexes;
            await ClearCellSelection().ConfigureAwait(true);
            List<Row<object>>? _dataRows = Parent.Rows?.Where(_ => _.IsDataRow && !_.RowType.Equals("DetailRow", StringComparison.Ordinal)).ToList();
            foreach (ValueTuple<int, int> index in indexes)
            {
                Row<object>? _row = _dataRows?[(int)index.Item1];
                await SelectCellByRow(_row!, index.Item2, null!, isSelectionMethodInvoked).ConfigureAwait(true);
            }
        }

        public async Task SelectCellsByRange(ValueTuple<int, int> startIndex, ValueTuple<int, int> endIndex, bool isSelectionMethodInvoked = false)
        {
            if (!Parent.AllowSelection || IsRowMode())
            {
                return;
            }

            if (!InvokedFromClient)
            {
                await ClearCellSelection().ConfigureAwait(true);
            }
            await SelectRangeOfCells((null!, null!), null!, startIndex, endIndex, isSelectionMethodInvoked).ConfigureAwait(true);

            if (InvokedFromClient && Parent.FocusModule != null && !Parent.IsEdit)
            {
                var lastRow = Parent.Rows?.FirstOrDefault(r => r.IsDataRow && r.Index == endIndex.Item1);
                var lastCell = lastRow?.Cells?[endIndex.Item2];
                if (lastCell != null)
                {
                    Parent.FocusModule.ClearCurrent();
                    Parent.FocusModule.SetCurrent(lastRow!, lastCell, outline: true);
                    await Parent.FocusModule.Focus(lastRow!.Uid!, lastCell.Uid, cellColIndex: lastCell.Index!.Value + 1).ConfigureAwait(true);
                }
            }
        }

        public async Task SelectAutofillCell(Row<object> rowToSelect, double cellIndex)
        {
            rowToSelect.IsSelected = true;
            rowToSelect.Cells?.ForEach(_ =>
            {
                if (_.Index == cellIndex)
                {
                    _.IsSelected = true;
                    _lastSelectedCell = _;
                }
            });
            _lastSelectedRow = rowToSelect;
            Parent.SoftRefresh = true;
            Parent.EventAggregator.Trigger("RowStateChanged", rowToSelect);
            await Task.CompletedTask.ConfigureAwait(true);
        }

        public async Task SelectCellByRow(Row<object> rowToSelect, int cellIndex, MouseAndKeyArgs? e = null, bool isSelectionMethodInvoked = false)
        {
            if (rowToSelect?.Cells?.Where(e => e.Index == cellIndex).FirstOrDefault()?.IsSelected == true)
            {
                if (IsBothMode())
                {
                    _lastSelectedRow = rowToSelect;
                }
                return;
            }

            object currrentCellValue = string.IsNullOrEmpty(rowToSelect?.Cells?[cellIndex]?.Column?.Field) ? null! : DataUtil.GetObject(rowToSelect?.Cells?[cellIndex]?.Column?.Field ?? "", rowToSelect?.Data ?? new object());
            if (Parent.GridEvents?.CellSelecting.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
            {
                var arg = new CellSelectingEventArgs<T>()
                {
                    Cancel = false,
                    Event = e?.Click!,
                    Data = (T)rowToSelect?.Data!,
                    IsCtrlPressed = e?.CtrlKey ?? false,
                    IsShiftPressed = e?.ShiftKey ?? false,
                    CellIndex = cellIndex,
                    RowIndex = (int)rowToSelect?.Index!,
                    CurrentValue = currrentCellValue,
                    PreviousValue = _lastSelectedCellValue,
                    Parent = Parent
                };
                if (e != null && e.Type == "keydown" && e.IsKeyEvent)
                {
                    Parent.PreventRender();
                }
                if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                    await Parent.EventAggregator.NotifyAsync("CellSelecting", arg).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.CellSelecting.InvokeAsync(arg))!.ConfigureAwait(true);
                if (arg.Cancel)
                    return;
            }

            rowToSelect!.IsSelected = true;
            rowToSelect?.Cells?.ForEach(_ =>
            {
                if (_.Index == cellIndex)
                {
                    _.IsSelected = true;
                    _lastSelectedCell = e != null && !e.ShiftKey ? _ : _lastSelectedCell;
                }
                else if ((Parent.SelectionSettings != null && Parent.SelectionSettings.Type != SelectionType.Multiple && e == null) || (e != null && !e.CtrlKey && !e.ShiftKey))
                {
                    _.IsSelected = false;
                }
            });
            if (e == null || !e.ShiftKey)
            {
                _lastSelectedRow = rowToSelect!;
            }
            Parent.SoftRefresh = true;

            if (!Parent.IsEdit && isSelectionMethodInvoked && Parent.FocusModule != null && !InvokedFromClient)
            {
                Parent.FocusModule.ClearCurrent();
                Parent.FocusModule.SetCurrent(rowToSelect!, rowToSelect?.Cells?[(int)cellIndex]!);
                var cellToSelect = rowToSelect?.Cells?[(int)cellIndex];
                await Parent.FocusModule.Focus(rowToSelect?.Uid!, cellToSelect?.Uid!, cellColIndex: cellToSelect?.Index! + 1 ?? -1).ConfigureAwait(true);
            }

            Parent.EventAggregator.Trigger("RowStateChanged", rowToSelect!);

            if (Parent.GridEvents?.CellSelected.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
            {
                var selectedArgs = new CellSelectEventArgs<T>()
                {
                    Event = e?.Click!,
                    Data = (T)rowToSelect?.Data!,
                    CellIndex = cellIndex,
                    RowIndex = (int)rowToSelect?.Index!,
                    IsCtrlPressed = e?.CtrlKey ?? false,
                    IsShiftPressed = e?.ShiftKey ?? false,
                    CurrentValue = currrentCellValue,
                    PreviousValue = _lastSelectedCellValue,
                    Parent = Parent
                };
                if (e != null && e.Type == "keydown" && e.IsKeyEvent)
                {
                    Parent.PreventRender();
                }
                if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                    await Parent.EventAggregator.NotifyAsync("CellSelected", selectedArgs).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.CellSelected.InvokeAsync(selectedArgs))!.ConfigureAwait(true)!;
            }
            _lastSelectedCellValue = currrentCellValue;

            if (Parent.EnableAutoFill && Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Batch)
            {
                await Task.Delay(1).ConfigureAwait(true);
                await AutofillBox((double)rowToSelect!.Index!, cellIndex).ConfigureAwait(true);
                await AutofillBorder((double)rowToSelect.Index, cellIndex).ConfigureAwait(true);
            }
        }

        #endregion

        #region Autofill Box and Border Methods

        public async Task AutofillBox(double? rowIndex = null, double? cellIndex = null, object? positions = null)
        {
            AutofillPosition styles = null!;
            if (rowIndex != null && cellIndex != null)
            {
                styles = await Parent.InvokeMethod<AutofillPosition>("sfBlazor.Grid.updateAutofillPosition", false, new object[] { Parent.DataId, (double)cellIndex, (double)rowIndex }).ConfigureAwait(true);
            }
            else
            {
                styles = JsonSerializer.Deserialize<AutofillPosition>(positions?.ToString()!)!;
            }

            if (styles != null)
            {
                Autofill<object> autofill = new Autofill<object>()
                {
                    AutofillRight = styles.Right!,
                    AutofillTop = styles.Top!,
                    AutofillLeft = styles.Left!,
                    IsSelected = true,
                    ISBorderSelected = false,
                    ISBoxSelected = true,
                    AutofillDisplay = "none",
                    AutofillBoxDisplay = Parent.IsEdit ? "none" : "",
                };
                Parent.EventAggregator.Trigger("ContentAutofillStateChanged", autofill);
            }
        }

        public async Task AutofillBorder(double? rowIndex = null, double? cellIndex = null, object? positions = null)
        {
            AutofillPosition border = null!;
            var boxDisplay = string.Empty;
            if (rowIndex != null && cellIndex != null)
            {
                border = await Parent.InvokeMethod<AutofillPosition>("sfBlazor.Grid.createBorder", false, new object[] { Parent.DataId, (double)rowIndex, (double)cellIndex }).ConfigureAwait(true);
            }
            else
            {
                border = JsonSerializer.Deserialize<AutofillPosition>(positions?.ToString()!)!;
                boxDisplay = "none";
            }

            if (border != null)
            {
                Autofill<object> autofill = new Autofill<object>()
                {
                    BorderRight = border.Right!,
                    BorderTop = border.Top!,
                    BorderLeft = border.Left!,
                    BorderHeight = border.Height!,
                    BorderWidth = border.Width!,
                    BordersWidth = border.BorderWidth!,
                    IsSelected = true,
                    ISBorderSelected = true,
                    ISBoxSelected = false,
                    AutofillBoxDisplay = boxDisplay,
                    AutofillDisplay = "none",
                };
                Parent.EventAggregator.Trigger("ContentAutofillStateChanged", autofill);
            }
        }

        public void UpdateAutofillPosition(object positions)
        {
            BorderAutofill styles = JsonSerializer.Deserialize<BorderAutofill>(positions?.ToString()!)!;
            if (styles != null)
            {
                Autofill<object> autofillBox = new Autofill<object>()
                {
                    AutofillDisplay = string.Empty,
                    BorderLeftAutofillLeft = styles.BorderLeftAutofillLeft!,
                    BorderLeftAutofillTop = styles.BorderLeftAutofillTop!,
                    BorderLeftAutofillHeight = styles.BorderLeftAutofillHeight!,
                    BorderLeftAutofillRight = styles.BorderLeftAutofillRight!,
                    BorderRightAutofillLeft = styles.BorderRightAutofillLeft!,
                    BorderRightAutofillHeight = styles.BorderRightAutofillHeight!,
                    BorderRightAutofillRight = styles.BorderRightAutofillRight!,
                    BorderRightAutofillTop = styles.BorderRightAutofillTop!,
                    BorderTopAutofillLeft = styles.BorderTopAutofillLeft!,
                    BorderTopAutofillTop = styles.BorderTopAutofillTop!,
                    BorderTopAutofillWidth = styles.BorderTopAutofillWidth!,
                    BorderBottomAutofillLeft = styles.BorderBottomAutofillLeft!,
                    BorderBottomAutofillTop = styles.BorderBottomAutofillTop!,
                    BorderBottomAutofillWidth = styles.BorderBottomAutofillWidth!,
                    BorderBottomAutofillRight = styles.BorderBottomAutofillRight!,
                    BorderTopAutofillRight = styles.BorderTopAutofillRight!,
                    IsSelected = true,
                    IsBorderPositionSelected = true,
                };
                Parent.EventAggregator.Trigger("ContentAutofillStateChanged", autofillBox);
            }
        }

        #endregion

        #region Autofill Position Models

        private class AutofillPosition
        {
            public string? Left { get; set; }

            public string? Top { get; set; }

            public string? Right { get; set; }

            public string? Height { get; set; }

            public string? Width { get; set; }

            public string? BorderWidth { get; set; }
        }

        #endregion

        #region Autofill Border Models

        private class BorderAutofill
        {
            public string? BorderLeftAutofillLeft { get; set; }

            public string? BorderLeftAutofillTop { get; set; }

            public string? BorderLeftAutofillRight { get; set; }

            public string? BorderLeftAutofillHeight { get; set; }

            public string? BorderRightAutofillLeft { get; set; }

            public string? BorderRightAutofillTop { get; set; }

            public string? BorderRightAutofillRight { get; set; }

            public string? BorderRightAutofillHeight { get; set; }

            public string? BorderTopAutofillLeft { get; set; }

            public string? BorderTopAutofillTop { get; set; }

            public string? BorderTopAutofillRight { get; set; }

            public string? BorderTopAutofillWidth { get; set; }

            public string? BorderBottomAutofillLeft { get; set; }

            public string? BorderBottomAutofillTop { get; set; }

            public string? BorderBottomAutofillRight { get; set; }

            public string? BorderBottomAutofillWidth { get; set; }
        }

        #endregion

        #region Cell Range Selection and Iteration Methods

        public async Task SelectRangeOfCells(
            ValueTuple<Row<object>,
            Cell<object>> target, MouseAndKeyArgs e = null!,
            ValueTuple<int, int>? rangeStart = null!, ValueTuple<int, int>? rangeEnd = null!, bool isSelectionMethodInvoked = false)
        {
            Row<object> _endRow = target.Item1;
            Cell<object> _endCell = target.Item2;
            int? startIndex = null;
            int? startCellIndex = null;
            int? endIndex = null;
            int? endCellIndex = null;
            bool isAtTop = false;
            bool tmp = false;
            Row<object> startRow = null!;
            Row<object> endRow = null!;
            if (_lastSelectedRow != null && _lastSelectedCell != null && _endRow != null)
            {
                isAtTop = _lastSelectedRow.Index < _endRow.Index;
                tmp = _lastSelectedCell.Index <= _endCell?.Index;
                startIndex = isAtTop ? _lastSelectedRow.Index : _endRow.Index;
                endIndex = isAtTop ? _endRow.Index : _lastSelectedRow.Index;
                if (IsCellBox())
                {
                    startCellIndex = tmp ? _lastSelectedCell.Index : _endCell?.Index;
                    endCellIndex = tmp ? _endCell?.Index : _lastSelectedCell.Index;
                }

                if (IsCellFlow())
                {
                    if (_lastSelectedRow.Equals(_endRow))
                    {
                        startCellIndex = tmp ? _lastSelectedCell.Index : _endCell?.Index;
                        endCellIndex = tmp ? _endCell?.Index : _lastSelectedCell.Index;
                    }
                    else
                    {
                        startCellIndex = isAtTop ? _lastSelectedCell.Index : _endCell?.Index;
                        endCellIndex = isAtTop ? _endCell?.Index : _lastSelectedCell.Index;
                    }
                }

                startRow = isAtTop ? _lastSelectedRow : _endRow;
                endRow = isAtTop ? _endRow : _lastSelectedRow;
            }

            if (rangeStart != null && rangeEnd != null)
            {
                List<Row<object>>? _dataRows = Parent.Rows?.Where(_ => _.IsDataRow).ToList();
                isAtTop = rangeStart.Value.Item1 <= rangeEnd.Value.Item1;
                tmp = rangeStart.Value.Item2 <= rangeEnd.Value.Item2;
                startIndex = isAtTop ? rangeStart.Value.Item1 : rangeEnd.Value.Item1;
                endIndex = isAtTop ? rangeEnd.Value.Item1 : rangeStart.Value.Item1;
                startCellIndex = tmp ? rangeStart.Value.Item2 : rangeEnd.Value.Item2;
                endCellIndex = tmp ? rangeEnd.Value.Item2 : rangeStart.Value.Item2;
                startRow = _dataRows?[startIndex.Value]!;
                endRow = _dataRows?[endIndex.Value]!;
                e = new MouseAndKeyArgs() { ShiftKey = true };
            }
            if (startIndex == null || endIndex == null)
            {
                return;
            }

            List<Row<object>>? _range = GetRangeofSelectionRows(startIndex, endIndex, startCellIndex, endCellIndex);

            if (isAtTop)
            {
                _range?.Reverse();
            }
            await IterateRange(_range!, startRow, endRow, isAtTop, tmp, startCellIndex, endCellIndex, e, isSelectionMethodInvoked).ConfigureAwait(true);
        }

        private async Task IterateRange(List<Row<object>> _range, Row<object> startRow, Row<object> endRow, bool isAtTop, bool tmp, int? startCellIndex, int? endCellIndex, MouseAndKeyArgs e, bool isSelectionMethodInvoked = false)
        {
            _range?.ForEach(async _row =>
            {
                bool isStartRow = _row?.Equals(startRow) == true;
                bool isEndRow = _row?.Equals(endRow) == true;
                List<Cell<object>>? _cells = _row?.Cells?.Select(x => x).ToList();
                if ((IsCellFlow() && (isAtTop || (isStartRow && isEndRow && tmp))) || (IsCellBox() && tmp))
                {
                    _cells?.Reverse();
                }

                await IterateCells(_cells!, _row!, e, isStartRow, isEndRow, startCellIndex, endCellIndex, isSelectionMethodInvoked).ConfigureAwait(true);
            });

        }

        private async Task IterateCells(List<Cell<object>> _cells, Row<object> _row, MouseAndKeyArgs e, bool isStartRow, bool isEndRow, int? startCellIndex, int? endCellIndex, bool isSelectionMethodInvoked = false)
        {
            _cells?.ForEach(async _cell =>
            {
                if (IsCellFlow())
                {
                    if (isStartRow && isEndRow)
                    {
                        if (_cell?.Index >= startCellIndex && _cell?.Index <= endCellIndex)
                        {
                            await SelectCellByRow(_row, (int)_cell.Index, e, isSelectionMethodInvoked).ConfigureAwait(true);
                        }
                    }
                    else if (isStartRow)
                    {
                        if (_cell?.Index >= startCellIndex)
                        {
                            await SelectCellByRow(_row, (int)_cell.Index, e, isSelectionMethodInvoked).ConfigureAwait(true);
                        }
                    }
                    else if (isEndRow)
                    {
                        if (_cell?.Index <= endCellIndex)
                        {
                            await SelectCellByRow(_row, (int)_cell.Index, e, isSelectionMethodInvoked).ConfigureAwait(true);
                        }
                    }
                    else if (!isStartRow && !isEndRow)
                    {
                        if ((_cell?.IsDataCell == true || _cell?.Column?.Type.Equals(ColumnType.CheckBox) == true) && _row?.Cells?.Where(x => x.IsSelected).Count() != _row?.Cells?.Count)
                        {
                            await SelectCellByRow(_row!, (int)_cell.Index!, e).ConfigureAwait(true);
                        }
                    }
                }

                if (IsCellBox())
                {
                    if (_cell?.Index >= startCellIndex && _cell?.Index <= endCellIndex)
                    {
                        await SelectCellByRow(_row!, (int)_cell.Index, e, isSelectionMethodInvoked).ConfigureAwait(true);
                    }
                }
            });

        }

        #endregion

        #region Cell Deselection Methods

        public async Task ClearCellSelection(Row<object>? expect = null, int? expectCellIndex = null, bool autofillSelect = false, MouseAndKeyArgs? evt = null, bool IsClearRowSelectionNeeded = false)
        {
            if (!Parent.AllowSelection || IsRowMode() && (Parent.PropertyChanges?.Count == 0 || (!Parent.PropertyChanges?.ContainsKey("CellSelectionModeChanged") == true && !Parent.PropertyChanges?.ContainsKey("BothSelectionModeChanged") == true)))
            {
                return;
            }

            List<Row<object>>? _dataRows = IsBothMode() ? GetRowsObject()?.Where(row => row.IsSelected).ToList() : GetRowsObject()?.Where(row => row.Cells?.Any(cell => cell.IsSelected) == true).ToList();
            List<CellType> cellType = new List<CellType>() { CellType.Indent, CellType.Detail, CellType.RowDrag };
            bool _isCellDeselectionCanceled = false;
            foreach (Row<object> _row in _dataRows!)
            {
                foreach (Cell<object> _cell in _row?.Cells!)
                {
                    if (_cell.IsSelected && (!(evt != null && evt.ShiftKey && _row == _lastSelectedRow && _lastSelectedCell == _cell) || (evt.ShiftKey && evt.CtrlKey) || (evt.ShiftKey && evt.Type == "click" && Parent.SelectionSettings != null && Parent.SelectionSettings.Type == SelectionType.Single)))
                    {
                        if (cellType.Any(type => type == _cell.CellType))
                        {
                            continue;
                        }

                        int? cellIndex = _cell.Index;
                        if (_row != null && _row.Equals(expect) && cellIndex == expectCellIndex)
                        {
                            continue;
                        }

                        if (Parent.GridEvents?.CellDeselecting.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                        {
                            var arg = new CellDeselectEventArgs<T>()
                            {
                                Cancel = false,
                                Event = evt?.Click!,
                                Data = (T)_row!.Data!,
                                CellIndex = (int)cellIndex!,
                                RowIndex = (int)_row.Index!,
                                IsCtrlPressed = evt?.CtrlKey ?? false,
                                IsShiftPressed = evt?.ShiftKey ?? false,
                                Parent = Parent
                            };
                            if (evt != null && evt.Type == "keydown" && evt.IsKeyEvent)
                            {
                                Parent.PreventRender();
                            }
                            if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                                await Parent.EventAggregator.NotifyAsync("CellDeselecting", arg).ConfigureAwait(true);
                            else
                                await (Parent.GridEvents?.CellDeselecting.InvokeAsync(arg))!.ConfigureAwait(true)!;
                            if (arg.Cancel)
                            {
                                if (IsBothMode())
                                {
                                    _isCellDeselectionCanceled = true;
                                    break;
                                }
                                return;
                            }
                        }

                        _cell.IsSelected = false;

                        if (Parent.GridEvents?.CellDeselected.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                        {
                            var arg1 = new CellDeselectEventArgs<T>()
                            {
                                Cancel = false,
                                Event = evt?.Click!,
                                Data = (T)_row!.Data!,
                                CellIndex = (int)cellIndex!,
                                RowIndex = (int)_row.Index!,
                                IsCtrlPressed = evt?.CtrlKey ?? false,
                                IsShiftPressed = evt?.ShiftKey ?? false,
                                Parent = Parent
                            };
                            if (evt != null && evt.Type == "keydown" && evt.IsKeyEvent)
                            {
                                Parent.PreventRender();
                            }
                            if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                                await Parent.EventAggregator.NotifyAsync("CellDeselected", arg1).ConfigureAwait(true);
                            else
                                await (Parent.GridEvents?.CellDeselected.InvokeAsync(arg1))!.ConfigureAwait(true)!;
                        }
                    }
                }
                if (Parent.PropertyChanges?.ContainsKey("CellSelectionModeChanged") == true)
                {
                    _lastSelectedRow = null!;
                }
                if (Parent.VirtualScrollModule != null)
                {
                    Parent.VirtualScrollModule.ShiftSelectionRowIndexes = (-1, -1);
                    Parent.VirtualScrollModule.ShiftSelectionCellIndexes = (-1, -1);
                }
                if (!IsClearRowSelectionNeeded)
                {
                    if (_row?.Cells?.Find(_ => _.IsSelected) == null || (_isCellDeselectionCanceled && IsBothMode()))
                    {
                        if (IsBothMode() && expect?.Uid != _row?.Uid && (Parent.PropertyChanges?.Count == 0 || (!Parent.PropertyChanges?.ContainsKey("CellSelectionModeChanged") == true && !Parent.PropertyChanges?.ContainsKey("BothSelectionModeChanged") == true)))
                        {
                            await ClearSelectionByRow(_row!, evt!).ConfigureAwait(true);
                            if (_isRowDeselectCancelled && IsBothMode())
                            {
                                return;
                            }
                        }
                        if ((IsBothMode() && expect?.Uid != _row?.Uid) || IsCellMode())
                        {
                            _row!.IsSelected = false;
                        }

                        var key = Parent.PropHelper?.GetObject(PrimaryKey, _row?.Data!);
                        if (key != null && !(IsBothMode() && Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection && expect?.Uid == _row?.Uid))
                        {
                            PersistedData?.Remove(key);
                        }
                    }
                }

                Parent.SoftRefresh = true;
                Parent.EventAggregator.Trigger("RowStateChanged", _row!);
            }

            if (Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Batch && !autofillSelect)
            {
                AutofillChanges();
            }
            if (IsBothMode() && _dataRows?.Count == 0 && expect != null && evt != null && evt.Type == "click")
            {
                if (Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection && Parent.SelectionModule != null && Parent.SelectionModule.PersistedData != null && (IsResetOnRowClick() || IsSingle()))
                {
                    Parent.CheckBoxState = CheckState.UnCheck;
                    Parent.SelectionModule.PersistedData?.Clear();
                }
            }
        }

        public void AutofillChanges()
        {
            if (Parent.EnableAutoFill)
            {
                Autofill<object> autofillBox = new Autofill<object>()
                {
                    IsSelected = true,
                    AutofillBoxDisplay = "none",
                    AutofillBorderDisplay = "none",
                    AutofillDisplay = "none"
                };
                Parent.EventAggregator.Trigger("ContentAutofillStateChanged", autofillBox);
            }
        }

        public async Task ClearSelectionByCell(Row<object> _row, Cell<object> _cell, MouseAndKeyArgs? evt = null, bool rowDeselect = false, bool preventSelectedProperty = false)
        {
            int? cellIndex = _cell?.Index;
            if (Parent.GridEvents?.CellDeselecting.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
            {
                var arg = new CellDeselectEventArgs<T>()
                {
                    Cancel = false,
                    Event = evt?.Click!,
                    Data = (T)_row.Data!,
                    CellIndex = (int)cellIndex!,
                    IsCtrlPressed = evt?.CtrlKey ?? false,
                    IsShiftPressed = evt?.ShiftKey ?? false,
                    Parent = Parent
                };
                if (evt != null && evt.Type == "keydown" && evt.IsKeyEvent)
                {
                    Parent.PreventRender();
                }
                if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                    await Parent.EventAggregator.NotifyAsync("CellDeselecting", arg).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.CellDeselecting.InvokeAsync(arg))!.ConfigureAwait(true)!;
                if (arg.Cancel)
                    return;
            }
            if (_cell != null)
            {
                _cell.IsSelected = false;
            }
            if (!preventSelectedProperty)
            {
                _lastSelectedRow = _row;
                _lastSelectedCell = _cell!;
            }

            if (Parent.GridEvents?.CellDeselected.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
            {
                var arg1 = new CellDeselectEventArgs<T>()
                {
                    Cancel = false,
                    Event = evt?.Click!,
                    Data = (T)_row.Data!,
                    CellIndex = (int)cellIndex!,
                    RowIndex = (int)_row.Index!,
                    IsCtrlPressed = evt?.CtrlKey ?? false,
                    IsShiftPressed = evt?.ShiftKey ?? false,
                    Parent = Parent
                };
                if (evt != null && evt.Type == "keydown" && evt.IsKeyEvent)
                {
                    Parent.PreventRender();
                }
                if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                    await Parent.EventAggregator.NotifyAsync("CellDeselected", arg1).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.CellDeselected.InvokeAsync(arg1))!.ConfigureAwait(true)!;
            }

            if (_row?.Cells?.Find(_ => _.IsSelected) == null || rowDeselect)
            {
                await ClearSelectionByRow(_row!, evt!).ConfigureAwait(true);
                if (_row != null)
                {
                    _row.IsSelected = false;
                    _row.Cells?.ForEach(x => x.IsSelected = false);
                }
            }

            Parent.SoftRefresh = true;

            Parent.EventAggregator.Trigger("RowStateChanged", _row!);
        }

        #endregion

        #region Cell Selection Event Handlers

        public async Task CellSelectionClickHandler(MouseAndKeyArgs e, ValueTuple<Row<object>, Cell<object>, bool> target)
        {
            Row<object> row = target.Item1;
            Cell<object> cell = target.Item2;
            bool IsCheckBox = target.Item3;
            bool CtrlShiftSelection = false;
            CtrlShiftSelection = e != null && e.CtrlKey && e.ShiftKey;
            GridSelectionSettings _settings = Parent.SelectionSettings!;
            _isInteracted = true;
            Parent.FocusModule!.LastKeyCombination = e != null && !e.IsKeyEvent && e.Type?.Equals("click", StringComparison.Ordinal) == true ? null! : Parent.FocusModule.LastKeyCombination!;

            if (!IsSingle() && (e!.CtrlKey || e.ShiftKey) && _lastSelectedRow != null && _lastSelectedCell != null)
            {
                if (e.ShiftKey)
                {
                    bool preventSelection = false;
                    if (!CtrlShiftSelection)
                    {
                        if (Parent.FocusModule!.LastKeyCombination != null && !Parent.FocusModule.LastKeyCombination.Code?.Equals("KeyC", StringComparison.Ordinal) == true && IsCellFlow())
                        {
                            preventSelection = await ClearRedundantCellSelections(row, cell, e).ConfigureAwait(true);
                        }
                        else if (Parent.FocusModule.LastKeyCombination != null && !Parent.FocusModule.LastKeyCombination.Code?.Equals("KeyC", StringComparison.Ordinal) == true && IsCellBox())
                        {
                            preventSelection = await ClearRedundantCellsInBoxMode(row, cell, e).ConfigureAwait(true);
                        }
                        else
                        {
                            await ClearCellSelection(evt: e).ConfigureAwait(true);
                        }
                    }

                    if (!preventSelection)
                    {
                        await SelectRangeOfCells((row, cell), e).ConfigureAwait(true);
                    }
                }
                else if (cell?.IsSelected == true && CanToggle())
                {
                    await ClearSelectionByCell(row, cell, e).ConfigureAwait(true);
                }
                else
                {
                    await SelectCellByRow(row, (int)cell!.Index!, e).ConfigureAwait(true);
                }
            }
            else
            {
                if ((cell?.IsSelected == true && CanToggle()) || !cell?.IsSelected == true)
                {
                    if (cell?.IsSelected == true && CanToggle())
                    {
                        await ClearCellSelection(evt: e!).ConfigureAwait(true);
                    }
                    else if (!cell?.IsSelected == true)
                    {
                        await ClearCellSelection(evt: e!).ConfigureAwait(true);
                        await SelectCellByRow(row, (int)cell?.Index!, e!).ConfigureAwait(true);
                    }
                }
                else
                {
                    if (CanToggle())
                    {
                        await ClearSelectionByCell(row, cell!, e!).ConfigureAwait(true);
                    }
                    else
                    {
                        await ClearCellSelection(row, (int)cell?.Index!, evt: e!).ConfigureAwait(true);
                    }
                }
            }

            _isInteracted = false;
        }

        public async Task RowCellSelectionClickHandler(MouseAndKeyArgs e, ValueTuple<Row<object>, Cell<object>, bool> target)
        {
            Row<object> row = target.Item1;
            Cell<object> cell = target.Item2;
            bool IsCheckBox = target.Item3;
            bool CtrlShiftSelection = e?.CtrlKey == true && e?.ShiftKey == true;
            bool IsClickEvent = e?.Type == "click";
            GridSelectionSettings _settings = Parent.SelectionSettings!;
            if (Parent.FocusModule != null)
                Parent.FocusModule.LastKeyCombination = !e?.IsKeyEvent == true && e?.Type?.Equals("click", StringComparison.Ordinal) == true ? null! : Parent.FocusModule.LastKeyCombination;
            _isInteracted = true;
            if ((!IsSingle() && e != null && (e.CtrlKey || e.ShiftKey)) || HasCheckBoxColumn(IsCheckBox, e!))
            {
                if (e?.ShiftKey == true)
                {
                    if (!CtrlShiftSelection)
                    {
                        if (Parent.FocusModule!.LastKeyCombination != null)
                        {
                            bool preventSelection = await ClearRedundantCellSelections(row, cell, e).ConfigureAwait(true);
                        }
                    }
                    await SelectRangeOfRows(row, (null, null), e).ConfigureAwait(true);
                    await SelectRangeOfCells((row, cell), e).ConfigureAwait(true);
                }
                else if ((cell?.IsSelected == true && CanToggle()) || IsCheckBox && row?.IsSelected == true)
                {
                    await ClearCellSelection(row, (int)cell?.Index!, evt: e!, IsClearRowSelectionNeeded: true).ConfigureAwait(true);
                    await ClearSelectionByCell(row, cell, e!, IsCheckBox && row?.IsSelected == true).ConfigureAwait(true);
                }
                else
                {
                    if (e != null && e.CtrlKey != true)
                    {
                        await ClearCellSelection(row!, (int)cell?.Index!, evt: e!, IsClearRowSelectionNeeded: true).ConfigureAwait(true);
                    }
                    await SelectByRow(row!, e!).ConfigureAwait(true);
                    await SelectCellByRow(row!, (int)cell?.Index!, e!).ConfigureAwait(true);
                }
            }
            else
            {
                if ((cell?.IsSelected == true && CanToggle()) || cell?.IsSelected != true)
                {
                    if (cell?.IsSelected == true && CanToggle())
                    {
                        if (IsClickEvent)
                        {
                            await ClearCellSelection(evt: e!).ConfigureAwait(true);
                        }
                        else
                        {
                            await ClearCellSelection(row, (int)cell.Index!, evt: e!, IsClearRowSelectionNeeded: true).ConfigureAwait(true);
                            await SelectCellByRow(row, (int)cell.Index, e!).ConfigureAwait(true);
                        }
                    }
                    else if (row?.IsSelected == true && Parent.SelectedRecords?.Count > 1 && !IsClickEvent)
                    {
                        await ClearCellSelection(row, (int)cell?.Index!, evt: e!, IsClearRowSelectionNeeded: true).ConfigureAwait(true);
                        await SelectCellByRow(row, (int)cell.Index, e!).ConfigureAwait(true);
                    }
                    else if (cell?.IsSelected != true)
                    {
                        await ClearCellSelection(row!, (int)(cell?.Index ?? 0), evt: e!).ConfigureAwait(true);
                        await SelectByRow(row!, e!).ConfigureAwait(true);
                        await SelectCellByRow(row!, (int)(cell?.Index ?? 0), e!).ConfigureAwait(true);
                    }
                }
                else
                {
                    if ((cell?.IsSelected == true && IsClickEvent) || (!IsClickEvent && Parent.SelectedRecords?.Count == 1))
                    {
                        await ClearCellSelection(row, (int)cell?.Index!, evt: e!).ConfigureAwait(true);
                    }
                    else if (row?.IsSelected == true && Parent.SelectedRecords?.Count > 1 && !IsClickEvent)
                    {
                        await ClearCellSelection(row, (int)cell?.Index!, evt: e!, IsClearRowSelectionNeeded: true).ConfigureAwait(true);
                    }

                    await SelectByRow(row!, e!).ConfigureAwait(true);
                    await SelectCellByRow(row!, (int)cell?.Index!, e!).ConfigureAwait(true);
                }
            }

            _isInteracted = false;
        }

        #endregion

        #region Row Object Retrieval Methods

        internal List<Row<object>>? GetRowsObject()
        {
            List<Row<object>> _rows = new List<Row<object>>();
            if (Parent.EnableVirtualization)
            {
                if (Parent.GroupSettings != null && Parent.GroupSettings.Columns?.ToList().Count > 0)
                {
                    return Parent.GroupSettings.EnableLazyLoading ? Parent.Rows : Parent.Rows?.Where(_ => _.IsDataRow).ToList();
                }
                else
                {
                    foreach (List<Row<object>> row in Parent.VirtualScrollModule!.GeneratedRows.Values)
                    {
                        _rows.Add(row[0]);
                    }

                    return _rows;
                }
            }
            else
            {
                return Parent.Rows;
            }
        }

        private async Task ClearCellIndexesInBoxMode(int cellIndex)
        {
            var selectedCellIndexes = await Parent.GetSelectedRowCellIndexesAsync().ConfigureAwait(true);
            var valueTuples = selectedCellIndexes.Where(tuple => tuple.Item2 == cellIndex).ToList();
            foreach (var tuple in valueTuples)
            {
                var rowToBeDeselected = Parent.Rows?.Where(e => e.IsDataRow).ToList()?.FirstOrDefault(e => e.Index == tuple.Item1);
                await ClearSelectionByCell(rowToBeDeselected!, rowToBeDeselected?.Cells?.FirstOrDefault(e => e.Index == tuple.Item2)!, preventSelectedProperty: true).ConfigureAwait(true);
            }
        }

        private async Task<bool> HandleCellSelectionForSingleRow(string keyCombination, MouseAndKeyArgs e, int firstCellIndex, int lastCellIndex)
        {
            var selectedCellIndexes = await Parent.GetSelectedRowCellIndexesAsync().ConfigureAwait(true);
            bool preventSelection = false;
            if (selectedCellIndexes != null && selectedCellIndexes.Count > 1)
            {
                var isLeftToRight = _lastSelectedCell?.Index != lastCellIndex ? _lastSelectedRow?.Cells?.FirstOrDefault(e => e.Index == _lastSelectedCell?.Index + 1)?.IsSelected == true : false;
                var isRightToLeft = _lastSelectedCell?.Index != firstCellIndex ? _lastSelectedRow?.Cells?.FirstOrDefault(e => e.Index == _lastSelectedCell?.Index - 1)?.IsSelected == true : false;

                if ((isLeftToRight && keyCombination.Equals("ShiftLeft", StringComparison.Ordinal)) || (isRightToLeft && keyCombination.Equals("ShiftRight", StringComparison.Ordinal)))
                {
                    await ClearSelectionByCell(_lastSelectedRow!, Parent.FocusModule!.KeyInvokedCell!, preventSelectedProperty: true).ConfigureAwait(true);
                    return true;
                }
            }
            else
            {
                await ClearCellSelection(evt: e).ConfigureAwait(true);
            }
            return preventSelection;
        }

        private async Task<bool> ClearRedundantCellsInBoxMode(Row<object> row, Cell<object> cell, MouseAndKeyArgs e)
        {
            if (_lastSelectedRow == null || _lastSelectedCell == null || Parent?.FocusModule?.KeyInvokedRow == null!)
            {
                return false;
            }

            bool preventSelection = false;
            var keyCombination = Parent.FocusModule.LastKeyCombination?.GetKeyCombination();

            bool isShiftUp = keyCombination != null && keyCombination!.Equals("ShiftUp", StringComparison.Ordinal);
            bool isShiftDown = keyCombination != null && keyCombination.Equals("ShiftDown", StringComparison.Ordinal);
            bool isShiftLeft = keyCombination != null && keyCombination.Equals("ShiftLeft", StringComparison.Ordinal);
            bool isShiftRight = keyCombination != null && keyCombination.Equals("ShiftRight", StringComparison.Ordinal);

            bool selectedRowIsLast = Parent.Rows?.LastOrDefault()?.Index == _lastSelectedRow.Index;
            bool selectedRowIsFirst = Parent.Rows?.FirstOrDefault()?.Index == _lastSelectedRow.Index;
            bool isSelectionUpwards = !selectedRowIsFirst && Parent.Rows?[(int)_lastSelectedRow.Index! - 1]?.Cells?.Any(e => e.IsSelected) == true;
            bool isSelectionDownwards = !selectedRowIsLast && Parent.Rows?[(int)_lastSelectedRow.Index! + 1]?.Cells?.Any(e => e.IsSelected) == true;
            var firstCellIndex = Parent.Rows?.FirstOrDefault(e => e.IsDataRow)?.Cells?.FirstOrDefault(e => e.Visible)?.Index;
            var lastCellIndex = Parent.Rows?.FirstOrDefault(e => e.IsDataRow)?.Cells?.LastOrDefault(e => e.Visible)?.Index;

            if (Parent.FocusModule.KeyInvokedCell != null && (isShiftRight && Parent.FocusModule.KeyInvokedCell.Index == lastCellIndex) || (isShiftLeft && Parent.FocusModule.KeyInvokedCell != null && Parent.FocusModule.KeyInvokedCell.Index == firstCellIndex))
            {
                return true;
            }

            if (Parent.FocusModule.KeyInvokedRow == _lastSelectedRow)
            {
                preventSelection = await HandleCellSelectionForSingleRow(keyCombination!, e, (int)firstCellIndex!, (int)lastCellIndex!).ConfigureAwait(true);
            }
            else if ((isShiftUp && isSelectionDownwards) || (isShiftDown && isSelectionUpwards))
            {
                preventSelection = true;
                ClearCellSelectionByRow(Parent.FocusModule.KeyInvokedRow);
            }
            else if ((isShiftLeft && isSelectionDownwards) || (isShiftRight && isSelectionUpwards))
            {
                if (isShiftLeft ? cell?.Index < _lastSelectedCell?.Index : cell?.Index > _lastSelectedCell?.Index)
                {
                    return false;
                }

                preventSelection = true;
                await ClearCellIndexesInBoxMode((int)Parent.FocusModule.KeyInvokedCell?.Index!).ConfigureAwait(true);
            }
            else if ((isShiftLeft && isSelectionUpwards) || (isShiftRight && isSelectionDownwards))
            {
                if (cell?.IsSelected == true)
                {
                    preventSelection = true;
                    await ClearCellIndexesInBoxMode((int)Parent.FocusModule.KeyInvokedCell?.Index!).ConfigureAwait(true);
                }
            }

            return preventSelection;
        }

        private async Task<bool> ClearRedundantCellSelections(Row<object> row, Cell<object> cell, MouseAndKeyArgs e)
        {
            if (_lastSelectedRow == null)
            {
                return false;
            }
            bool preventSelection = false;
            bool isCellSelectionOnMultipleRows = false;
            var selectedCellIndexes = await Parent.GetSelectedRowCellIndexesAsync().ConfigureAwait(true);
            var keyCombination = Parent?.FocusModule?.LastKeyCombination?.GetKeyCombination();
            bool isShiftDown = (keyCombination != null && keyCombination.Equals("ShiftDown", StringComparison.Ordinal));
            bool isShiftUp = (keyCombination != null && keyCombination.Equals("ShiftUp", StringComparison.Ordinal));
            bool isShiftLeft = (keyCombination != null && keyCombination.Equals("ShiftLeft", StringComparison.Ordinal));
            bool isShiftRight = (keyCombination != null && keyCombination.Equals("ShiftRight", StringComparison.Ordinal));
            bool selectedRowIsLast = Parent?.Rows?.LastOrDefault()?.Index == _lastSelectedRow.Index;
            bool selectedRowIsFirst = Parent?.Rows?.FirstOrDefault()?.Index == _lastSelectedRow.Index;
            int? nextRowIndex = isShiftDown ? (selectedRowIsLast ? null : _lastSelectedRow.Index + 1) : isShiftUp ? (selectedRowIsFirst ? null : _lastSelectedRow.Index - 1) : null;
            var nextRow = nextRowIndex != null ? Parent?.Rows?.FirstOrDefault(e => e.Index == nextRowIndex) : null;
            bool isSelectionUpwards = !selectedRowIsFirst && Parent?.Rows?[(int)_lastSelectedRow.Index! - 1]?.Cells?.Any(e => e.IsSelected) == true;
            bool isSelectionDownwards = !selectedRowIsLast && Parent?.Rows?[(int)_lastSelectedRow.Index! + 1]?.Cells?.Any(e => e.IsSelected) == true;

            if (!(selectedRowIsFirst || selectedRowIsLast))
            {
                isCellSelectionOnMultipleRows = Parent?.Rows?.FirstOrDefault(e => e.Index == _lastSelectedRow?.Index - 1)?.IsSelected == true || Parent?.Rows?.FirstOrDefault(e => e.Index == _lastSelectedRow?.Index + 1)?.IsSelected == true;
            }
            else
            {
                int? rowIndex = _lastSelectedRow?.Index + (selectedRowIsFirst ? +1 : -1);
                isCellSelectionOnMultipleRows = Parent?.Rows?.Count > 1 ? Parent.Rows?.FirstOrDefault(e => e.Index == rowIndex)?.IsSelected == true : false;
            }

            if (row == _lastSelectedRow)
            {
                var firstCell = row?.Cells?.FirstOrDefault(e => e.Visible);
                var lastCell = row?.Cells?.LastOrDefault(e => e.Visible);
                if (isShiftDown || isShiftUp || !(isShiftLeft && cell?.Index == firstCell?.Index && firstCell?.IsSelected == true) || !(isShiftRight && cell?.Index == lastCell?.Index && lastCell?.IsSelected == true))
                {
                    await ClearCellSelection(evt: e).ConfigureAwait(true);
                }
            }
            else if (isShiftDown && !(nextRow?.Cells?.Any(e => e.IsSelected) == true))
            {
                if (!(row?.Index > _lastSelectedRow?.Index))
                {
                    preventSelection = true;

                    row?.Cells?.ForEach(async _cell =>
                    {
                        if (_cell?.Index < cell?.Index)
                        {
                            await ClearSelectionByCell(row, _cell, preventSelectedProperty: true).ConfigureAwait(true);
                        }
                    });
                    var rowToBeDeselected = Parent?.Rows?.Where(e => e.Index == row!.Index - 1).FirstOrDefault();
                    ClearCellSelectionByRow(rowToBeDeselected!);
                }
            }
            else if (isShiftUp && !(nextRow?.Cells?.Any(e => e.IsSelected) == true))
            {
                if (!(row?.Index < _lastSelectedRow?.Index))
                {
                    preventSelection = true;

                    row?.Cells?.ForEach(async _cell =>
                    {
                        if (_cell?.Index > cell?.Index)
                        {
                            await ClearSelectionByCell(row, _cell, preventSelectedProperty: true).ConfigureAwait(true);
                        }
                    });
                    var rowToBeDeselected = Parent?.Rows?.Where(e => e.IsDataRow)?.ToList()?.FirstOrDefault(e => e.Index == row?.Index + 1);
                    ClearCellSelectionByRow(rowToBeDeselected!);
                }
            }
            else if (isCellSelectionOnMultipleRows && isShiftLeft && isSelectionDownwards)
            {
                await ClearSelectionByCell(row, row?.Cells?.Where(e => e.Visible)?.ToList()?.FirstOrDefault(e => e.Index == cell?.Index + 1)!, preventSelectedProperty: true).ConfigureAwait(true);
            }
            else if (isCellSelectionOnMultipleRows && isShiftRight && isSelectionUpwards)
            {
                await ClearSelectionByCell(row, row?.Cells?.Where(e => e.Visible)?.ToList()?.FirstOrDefault(e => e.Index == cell?.Index - 1)!, preventSelectedProperty: true).ConfigureAwait(true);
            }

            return preventSelection;
        }

        private void ClearCellSelectionByRow(Row<object> row)
        {
            row?.Cells?.ForEach(async _cell =>
            {
                if (_cell?.IsSelected == true)
                    await ClearSelectionByCell(row, _cell, preventSelectedProperty: true).ConfigureAwait(true);
            });
        }

        #endregion

        #region Selection Count and State Helper Methods

        private int GetTotalCount()
        {
            int count = Parent.TotalItemCount;
            if (Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection && Parent.DataSource != null)
            {
                if (Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Batch)
                {
                    List<Row<object>>? AddedRow = Parent.Rows?.Where(Row => Row.Action == EditAction.Added).ToList();
                    if (AddedRow?.Count > 0)
                    {
                        count = count + AddedRow.Count;
                    }
                }

                return (Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Batch) ? count : Parent.TotalItemCount;
            }
            else
            {
                if (Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Batch)
                {
                    List<Row<object>>? AddedRow = Parent.Rows?.Where(Row => Row.Action == EditAction.Added).ToList();
                    if (AddedRow?.Count > 0)
                    {
                        count = count + AddedRow.Count;
                    }
                }

                if (Parent.EnableVirtualization && Parent.VirtualScrollModule != null)
                {
                    if (Parent.GroupSettings != null && Parent.GroupSettings.Columns?.ToList().Count > 0)
                    {
                        if (Parent.GroupSettings.EnableLazyLoading)
                        {
                            return Parent.TotalItemCount;
                        }
                        else
                        {
                            return Parent.VirtualScrollModule.VisibleGroupRows.Count;
                        }
                    }
                    else
                    {
                        return Parent.VirtualScrollModule.GeneratedRows.Count;
                    }
                }
                else if (Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Batch)
                {
                    return count;
                }
                else
                {
                    return Parent.TotalItemCount;
                }
            }
        }

        private void UpdateCBoxSelection(CheckState state)
        {
            IEnumerable<Row<object>> Rows = GetRowsObject()?.Where(_ => _.IsDataRow && _.RowType?.Equals("Data", StringComparison.Ordinal) == true) ?? Enumerable.Empty<Row<object>>();
            foreach (Row<object> Row in Rows)
            {
                if ((state.Equals(CheckState.Check) || (Parent.DataSource == null && Parent.EnableVirtualization && Parent.CheckBoxState == CheckState.Intermediate)) && Row != null && Row.State != "UnSelected")
                {
                    Row.IsSelected = true;
                }

                if (state.Equals(CheckState.UnCheck) && Row != null)
                {
                    Row.IsSelected = false;
                    Row.Cells?.ForEach(x => x.IsSelected = false);
                    object? key = Parent.PropHelper?.GetObject(PrimaryKey, Row?.Data!);
                    if (key != null)
                    {
                        _persistedData?.Remove(key);
                    }
                }
            }
        }

        #endregion

        #region Selection State Management Methods

        internal List<T> GetSelectedRecords()
        {
            List<T>? _records = null;
            if (Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection)
            {
                _records = PersistedData.Values.Select(x => (T)x).ToList();
            }
            else
            {
                _records = GetRowsObject()?.Where(_ => _.IsSelected)
                    .Select(x => (T)x.Data!).ToList<T>();
                Parent.SelectedRowIndexes = GetRowsObject()!
                    .Where(_ => _.IsSelected && _.Index.HasValue)
                    .Select(x => x.Index!.Value).ToList()
                    ?? new List<int>();
            }

            return _records!;
        }

        internal void UpdateSelectionAfterDataProcess()
        {
            if (Parent.EnableVirtualization && HasCheckBoxColumn() && Parent.SelectionSettings!.PersistSelection && Parent.VirtualScrollModule != null && Parent.VirtualScrollModule.RowEndIndex == Parent.TotalItemCount && IsHeaderCheckboxChecked)
            {
                SetPersistData(state: Parent.CheckBoxState);
            }
            Parent.SelectedRowIndexes = GetRowsObject()?.Where(_ => _.IsSelected && _.Index.HasValue).Select(x => (int)x.Index!).ToList<int>()!;
            if (Parent.SelectionSettings!.PersistSelection && Parent.DataSource == null && Parent.EnableInfiniteScrolling && !(Parent.CheckBoxState == CheckState.UnCheck))
            {
                SetPersistData(state: Parent.CheckBoxState);
            }
        }

        internal void RefreshSelectionOnPaging()
        {
            if (Parent.SelectionSettings != null && !Parent.SelectionSettings.PersistSelection)
            {
                Parent.CheckBoxState = CheckState.UnCheck;
            }

            _lastSelectedRow = null!;
            _lastSelectedCell = null!;
        }

        internal Row<object>? SelectedRow()
        {
            if (Parent.Rows?.Count > 0 && _lastSelectedRow != null)
            {
                foreach (var row in Parent.Rows?.Where(_ => _.IsDataRow)!)
                {
                    if (row?.Index.HasValue == true && row.Index == _lastSelectedRow?.Index)
                    {
                        _lastSelectedRow = row;
                    }
                }
            }
            return _lastSelectedRow;
        }

        #endregion

        #region Persist Selection State Query Methods

        /// <summary>
        /// Determines if remote data persist selection is enabled based on data source and adaptor settings.
        /// </summary>
        internal bool IsRemoteDataPersistSelection()
        {
            return Parent.DataSource == null && IsCheckBoxPersistSelection() &&
                (Parent.DataManager!.DataAdaptor!.IsRemote() || Parent.DataManager.Adaptor == Adaptors.CustomAdaptor);
        }

        /// <summary>
        /// Determines if the persist selection is enabled and a checkbox column exists.
        /// </summary>
        internal bool IsCheckBoxPersistSelection()
        {
            return Parent.SelectionSettings!.PersistSelection && HasCheckBoxColumn();
        }

        /// <summary>
        /// Resets the persisted selection state and updates the selection UI components.
        /// </summary>
        private void ResetPersistSelection()
        {
            if (Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection)
            {
                UpdatePersistCollection();
            }
            Parent.CheckBoxState = CheckState.UnCheck;
#pragma warning disable BL0005
            Parent.SelectedRowIndex = -1;
#pragma warning restore BL0005
        }

        /// <summary>
        /// Determines whether the given data is present in the deselected persistent data collection.
        /// </summary>
        internal bool IsDataInDeselectedCollection(object data)
        {
            return DoesDataExistInDictionary(data, DeSelectedPersistData);
        }

        /// <summary>
        /// Determines whether the specified data is part of the persisted data collection.
        /// </summary>
        internal bool IsDataInPersistedCollection(object data)
        {
            return DoesDataExistInDictionary(data, PersistedData);
        }


        /// <summary>
        /// Checks if the specified data is represented in the given dictionary by its primary key.
        /// </summary>
        private bool DoesDataExistInDictionary(object data, Dictionary<object, object> dictionary)
        {
            if (data == null)
            {
                return false; // Return early if data is null
            }

            object? key = Parent.PropHelper?.GetObject(Parent.SelectionModule?.PrimaryKey!, data);
            // Check if the key exists in the dictionary
            return key != null && dictionary.ContainsKey(key);
        }

        #endregion

        #region Filter and Search Data Methods

        /// <summary>
        /// Updates the current selection with filtered or searched data based on the request type.
        /// </summary>
        /// <param name="requestType">
        /// Specifies the type of request to process. 
        /// If "ClearSearch" or "ClearFiltering", clears the existing selection data.
        /// Otherwise, updates the selection with the current data.
        /// </param>
        internal void GetCurrentFilterData(string requestType = "")
        {
            if (Parent.SelectionSettings != null && !Parent.SelectionSettings.PersistSelection)
            {
                return;
            }
            var filteredColumns = Parent.FilteredColumns;
            if ((requestType == "ClearSearch" || (requestType == "ClearFiltering" && filteredColumns?.Count == 0) || requestType == "Delete"))
            {
                _filteredOrSearchedData.Clear();
                if (Parent.Query?.Queries?.Where?.Count > 0)
                {
                    GetCurrentFilterData("Filtering");
                }
                return;
            }
            if (_filteredOrSearchedData.Count > 0 && (requestType == "Filtering" || requestType == "Searching"
                || (Parent.AllowFiltering && filteredColumns?.Count > 0 && requestType == "ClearFiltering")))
            {
                _filteredOrSearchedData.Clear();
            }
            if (!string.IsNullOrEmpty(requestType))
            {
                var dataList = GetCurrentViewData();
                if (dataList != null)
                {
                    foreach (var item in dataList)
                    {
                        object? key = Parent.PropHelper?.GetObject(PrimaryKey, item);
                        if (key != null && !DoesDataExistInDictionary(item!, _filteredOrSearchedData))
                        {
                            _filteredOrSearchedData.AddOrUpdateItem(key, item!);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the current dataset based on filtering, searching, and selection settings.
        /// </summary>
        private IEnumerable<T>? GetCurrentViewData()
        {
            IEnumerable<T> _list = null!;
            var filteredColumns = Parent.FilterSettings!.Columns;
            var filteringOrSearchExisit = (Parent.AllowFiltering && filteredColumns?.Count > 0)
                    || (Parent.SearchSettings?.Key?.Length > 0)
                    || (Parent?.Query?.Queries?.Where != null && !Parent.IsRenderedFromTreeGrid);
            if (Parent?.DataSource != null)
            {
                if (filteringOrSearchExisit)
                {
                    _list = Parent.CurrentFilteredRecords ?? Parent.DataSource;
                }
                else if (Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection && Parent.SelectedRecords?.Count > 0)
                {
                    _list = Parent.SelectedRecords;
                }
                else
                {
                    _list = Parent.DataSource;
                }
            }
            else
            {
                if (Parent != null && Parent.CurrentFilteredRecords?.Count() > 0 && filteringOrSearchExisit)
                {
                    _list = Parent.CurrentFilteredRecords;
                }
                else
                {
                    _list = GetRowsObject()?.Where(_ => _.IsDataRow)?.Select(x => (T)x.Data!)!;
                }
            }
            return _list;
        }

        #endregion

        #region Action Event Handlers

        internal async Task HandleDeleteActionSelection(bool isDeleteAction, ActionEventArgs<T> actionArgs)
        {
            if (Parent.SelectionSettings!.PersistSelection && PersistedData != null)
            {
                if (isDeleteAction && actionArgs.Data != null)
                {
                    var primaryKey = (await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true)).FirstOrDefault();
                    var primaryKeyValue = primaryKey != null ? Parent.PropHelper!.GetValue(actionArgs.Data, primaryKey) : null;
                    if (primaryKeyValue != null)
                    {
                        PersistedData.Remove(primaryKeyValue);
                    }
                }
                else
                {
                    PersistedData.Clear();
                }
            }
            else
            {
                var selectedRow = Parent.Rows.Where(e => e.IsSelected == true).ToList();
                selectedRow.ForEach(x => x.IsSelected = false);
            }
        }

        internal void HandleCheckBoxPersistSelection(Action requestType, bool allowFiltering, int totalItemCount, List<GridFilterColumn> filteredColumns)
        {
            if (IsCheckBoxPersistSelection() && totalItemCount > 0)
            {
                var currentRequestType = requestType switch
                {
                    Action.Filtering when allowFiltering => "Filtering",
                    Action.Searching when Parent.SearchSettings?.Key?.Length > 0 => "Searching",
                    Action.ClearFiltering when filteredColumns?.Count > 0 && allowFiltering => "ClearFiltering",
                    _ => (Parent.Query?.Queries?.Where?.Count > 0 || (allowFiltering && Parent.FilterSettings?.Columns?.Count > 0)) ? "Filtering" : ""
                };

                GetCurrentFilterData(requestType: currentRequestType);
            }
            SetHeaderCheckState(requestType: requestType.ToString()!);
        }

        internal void UpdatePersistSelectionState(string requestType)
        {
            if (Parent.SelectionSettings!.PersistSelection && (Parent.CheckBoxState.Equals(CheckState.Check)
                || ((requestType == "Paging" || requestType == "Sorting" || requestType == "Filtering" || requestType == "ClearFiltering"
                || ((requestType == "Searching" || requestType == "Save" || requestType == "Delete") && IsRemoteDataPersistSelection())) && Parent.CheckBoxState.Equals(CheckState.Intermediate))))
            {
                if (IsCheckBoxPersistSelection() && (IsHeaderCheckboxChecked && requestType == "Delete") ||
                    (Parent.TotalItemCount == 0 && ((Parent.AllowFiltering && Parent.FilteredColumns?.Count == 0) || Parent.SearchSettings?.Key?.Length == 0)))
                {
                    GetCurrentFilterData(requestType: "Delete");
                }

                SetHeaderCheckState();
                Parent.EventAggregator.Trigger("HeaderStateChanged", null!);
            }
            if (Parent.SelectionSettings != null && Parent.SelectionSettings.PersistSelection && (requestType == "Filtering" || requestType == "ClearFiltering" || requestType == "Searching"
                || (requestType == "Paging" && (Parent.SearchSettings?.Key?.Length > 0 || Parent.FilteredColumns?.Count > 0))))
            {
                var currentRequestType = requestType == "Searching" && Parent.SearchSettings?.Key?.Length == 0
                     ? "ClearSearch"
                     : requestType;
                GetCurrentFilterData(requestType: currentRequestType);
            }
        }


        internal async Task ClearSelectionOnSort()
        {
            if (!Parent.SelectionSettings!.PersistSelection && Parent.SelectionModule != null)
            {
                var selectedRowIndexesCount = Parent.SelectedRowIndexes?.Count ?? 0;
                var selectedRowCellIndexes = await Parent.GetSelectedRowCellIndexesAsync().ConfigureAwait(true);
                if (selectedRowIndexesCount != 0 || selectedRowCellIndexes.Count > 0)
                {
                    await Parent.SelectionModule!.ClearSelection().ConfigureAwait(true);
                }
            }
        }

        internal async Task SelectRowAndCell(int? editRowIndex, int rowIndex, Cell<object> cell, bool isCellMode, bool isBothMode)
        {
            await Parent.SelectRowAsync(editRowIndex > -1 ? (int)editRowIndex : rowIndex).ConfigureAwait(true);
            var rowIndexToFind = editRowIndex > -1 ? editRowIndex : rowIndex;
            var selectedRowObject = Parent.Rows!.Find(_ => _.Index == rowIndexToFind);
            if (cell != null && !cell.IsSelected && (isCellMode || isBothMode))
            {
                await SelectCellByRow(selectedRowObject!, (int)cell.Index!).ConfigureAwait(true);
            }
        }

        internal void UpdateCheckBoxStateOnAdd(bool isAdd, EditAction editAction)
        {
            if ((Parent.SelectionModule != null && Parent.SelectionModule.IsCheckBoxPersistSelection()) && !Parent.SelectionModule.IsHeaderCheckboxChecked && Parent.SelectionModule.PersistedData.Count == Parent.TotalItemCount && isAdd && editAction == EditAction.Added)
            {
                Parent.CheckBoxState = Parent.CheckBoxState == CheckState.Check ? CheckState.Intermediate : Parent.CheckBoxState;
            }
            if (Parent.TotalItemCount == 0 && (Parent.SelectionModule != null && Parent.SelectionModule.HasCheckBoxColumn()) && isAdd && editAction == EditAction.Added && Parent.SelectionModule.IsHeaderCheckboxChecked)
            {
                Parent.CheckBoxState = Parent.CheckBoxState == CheckState.UnCheck ? CheckState.Check : Parent.CheckBoxState;
            }
        }
        #endregion

        #region Module Reset and Virtual Row Update Methods

        internal void ResetSelectionModule(string requestType)
        {
            RangeStartIndex = -1;
            RangeEndIndex = -1;

            if (IsCheckBoxPersistSelection() && PersistedData.Count > 0 && requestType == "Delete")
            {
                PersistedData.Clear();
            }
        }

        internal void UpdateGeneratedRowsSelection(List<Row<object>> recentlyGeneratedRows, bool isSelAllChangedByRowClick, bool isSelectAllWithFilter)
        {
            if (HasCheckBoxColumn() && isSelAllChangedByRowClick && !isSelectAllWithFilter)
            {
                foreach (Row<object> row in recentlyGeneratedRows)
                {
                    bool generatedDataNotInDeselected = DeSelectedPersistData.Count > 0 && IsDataInDeselectedCollection(row.Data!);
                    if (Parent.CheckBoxState.Equals(CheckState.Intermediate) && !generatedDataNotInDeselected)
                    {
                        row.IsSelected = true;
                    }
                }
            }
        }

        internal async Task HandleAutofillPositionUpdate(object positions, string updateFunction)
        {
            if (updateFunction == "UpdateAutofillPosition")
            {
                UpdateAutofillPosition(positions);
            }
            else if (updateFunction == "UpdateAutofillBorder")
            {
                await AutofillBorder(null, null, positions).ConfigureAwait(true);
            }
            else if (updateFunction == "UpdateAutofillBox")
            {
                await AutofillBox(null, null, positions).ConfigureAwait(true);
            }
        }

        #endregion

        #region Context Menu Selection Processing Methods

        internal async Task HandleContextMenuSelection(Row<object> rowObject, int cellIndex, GridColumn targetColumn)
        {
            if (!(Parent!.SelectionSettings!.Type == SelectionType.Multiple && rowObject.IsSelected))
            {
                if (Parent.SelectionModule != null && Parent.SelectionModule.IsBothMode())
                {
                    await Parent.SelectionModule.ClearSelection().ConfigureAwait(true);
                    await Parent.SelectionModule.RowCellSelectionClickHandler(null!, (rowObject, rowObject.Cells[cellIndex], targetColumn?.Type == ColumnType.CheckBox)).ConfigureAwait(true);
                }
                else if (!rowObject.IsSelected && Parent.SelectionModule != null)
                {
                    await Parent.SelectionModule.ClearRowSelection().ConfigureAwait(true);
                    await Parent.SelectionModule.SelectRow((int)rowObject.Index!).ConfigureAwait(true);
                }
            }
        }

        #endregion

        #region Drag Selection Event Handlers

        internal async Task HandleDragSelectionCompleted(int startIndex, int endIndex, string targetId, int startCellIndex, int endCellIndex)
        {
            Parent.DragStopIndex = startIndex < endIndex ? endIndex : startIndex;
            Parent.HasDragSelectionCompleted = true;
            if (Parent.GridEvents?.RowDragSelectionCompleting.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                var args = new RowDragSelectedEventArgs<T>
                {
                    TargetGridID = targetId,
                    RowStartIndex = startIndex,
                    RowEndIndex = endIndex,
                    CellStartIndex = startCellIndex,
                    CellEndIndex = endCellIndex,
                    Parent = Parent
                };
                if (Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("RowDragSelectionCompleting", args).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.RowDragSelectionCompleting.InvokeAsync(args))!.ConfigureAwait(true)!;
            }
            if (Parent.GridEvents?.RowDragSelectionCompleted.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                Parent.DragSelectionEventArgs = new RowDragSelectedEventArgs<T>
                {
                    TargetGridID = targetId,
                    RowStartIndex = startIndex,
                    RowEndIndex = endIndex,
                    CellStartIndex = startCellIndex,
                    CellEndIndex = endCellIndex,
                    Parent = Parent
                };
            }
        }

        internal async Task HandleDragSelectionStarting(int rowIndex, int cellIndex)
        {
            if (Parent.GridEvents?.RowDragSelectionStarting.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                var args = new RowDragSelectionEventArgs<T>
                {
                    RowStartIndex = rowIndex,
                    CellStartIndex = cellIndex,
                    Parent = Parent
                };
                if (Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("RowDragSelectionStarting", args).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.RowDragSelectionStarting.InvokeAsync(args))!.ConfigureAwait(true);
            }
        }

        internal async Task HandleDragCellSelectionCompleting(int startIndex, int startCellIndex, int rowIndex, int cellIndex, string targetId)
        {
            Parent.DragStopIndex = startIndex < rowIndex ? rowIndex : startIndex;
            Parent.HasDragSelectionCompleted = true;
            if (Parent.GridEvents?.RowDragSelectionCompleting.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                var args = new RowDragSelectedEventArgs<T>
                {
                    TargetGridID = targetId,
                    RowStartIndex = startIndex,
                    RowEndIndex = rowIndex,
                    CellStartIndex = startCellIndex,
                    CellEndIndex = cellIndex,
                    Parent = Parent
                };
                if (Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("RowDragSelectionCompleting", args).ConfigureAwait(true);
                else
                    await Parent.GridEvents!.RowDragSelectionCompleting.InvokeAsync(args).ConfigureAwait(true);
            }
            if (Parent.GridEvents?.RowDragSelectionCompleted.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                Parent.DragSelectionEventArgs = new RowDragSelectedEventArgs<T>
                {
                    TargetGridID = targetId,
                    RowStartIndex = startIndex,
                    RowEndIndex = rowIndex,
                    CellStartIndex = startCellIndex,
                    CellEndIndex = cellIndex,
                    Parent = Parent
                };
            }
        }
        #endregion

        #region Drag Selection Js Interop
        internal async Task DragSelection(int StartIndex, int EndIndex, bool ClearAll, string TargetId = null!, int StartCellIndex = 0, int EndCellIndex = 0)
        {
            if (!ClearAll)
            {
                await HandleDragSelectionCompleted(StartIndex, EndIndex, TargetId, StartCellIndex, EndCellIndex).ConfigureAwait(true);
            }
            InvokedFromClient = !ClearAll;
            await SelectRowsByRange(StartIndex, EndIndex, false).ConfigureAwait(true);
            InvokedFromClient = false;
        }

        internal async Task DragCellSelection(int StartIndex, int StartCellIndex, int RowIndex, int CellIndex, bool ClearAll, string TargetId = null!)
        {
            if (!ClearAll)
            {
                await HandleDragCellSelectionCompleting(StartIndex, StartCellIndex, RowIndex, CellIndex, TargetId).ConfigureAwait(true);
            }
            InvokedFromClient = !ClearAll;
            await Parent.SelectCellsByRangeAsync((StartIndex, StartCellIndex), (RowIndex, CellIndex)).ConfigureAwait(true);
            InvokedFromClient = false;
        }
        #endregion
    }
}