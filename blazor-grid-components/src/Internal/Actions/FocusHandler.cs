using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles focus and keyboard navigation in grid.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FocusHandler<T>
    {
        #region Private Properties

        private SfGrid<T> _parent;

        private bool _isHeader { get; set; }

        private bool _isJump { get; set; }

        private Cell<Object>? _previouslyFocusedCell { get; set; }

        private ValueTuple<string, string> _current = (string.Empty, string.Empty);

        private List<Row<object>> _currentRows
        {
            get { return _isHeader ? _parent.HeaderRows : _parent.Rows; }
        }

        internal bool isMultiSelectPopUpOpened { get; set; }

        private List<Row<object>> _frozenHeaderRows
        {
            get { return _parent.FrozenHeaderRows; }
        }

        private readonly Dictionary<string, bool> _focusedCells = new();

        #endregion

        #region Internal Properties

        internal bool IsByKey { get; set; }

        internal bool IsChildFocused { get; set; }

        internal int? SelectedCellIndex { get; set; }

        internal int? SelectedRowIndex { get; set; }

        internal Row<object>? KeyInvokedRow { get; set; }

        internal Cell<object>? KeyInvokedCell { get; set; }

        internal bool ChangeLastCellTabIndex { get; set; }

        internal object? AriaLabel { get; set; }

        internal bool IsKeyPressedUpOrDown { get; set; }

        internal KeyboardEventArgs? LastKeyCombination;

        internal int LastNavigatedCellIdx;

        internal bool IsSelectAllClicked { get; set; }

        internal string ClickedCheckBoxId { get; set; } = string.Empty;

        internal bool IsGridFocused { get; set; }

        internal List<Row<object>> HeaderRows
        {
            get { return _parent.HeaderRows; }
        }

        #endregion

        #region Constructor & Initialization

        public FocusHandler(SfGrid<T> parent)
        {
            _parent = parent;
            _parent.EventAggregator.Add("HeaderMouseDown", _ => _isHeader = true);
        }

        #endregion

        #region Helper Methods

        private bool IsGroupingFilterTemplate(Cell<object> cell) => 
            _parent.GroupSettings?.Columns?.Length > 0 && !cell.IsDataCell && _parent.GetVisibleColumnsAsync().Result.FirstOrDefault()?.FilterTemplate != null;

        private GridColumn? GetFirstVisibleColumn()
        {
            return _parent.GetVisibleColumnsAsync().Result.FirstOrDefault();
        }

        private GridColumn? GetLastVisibleColumn()
        {
            return _parent.GetVisibleColumnsAsync().Result.LastOrDefault();
        }

        internal Cell<object> GetLastVisibleCell()
        {
            return _currentRows.Last().Cells.Where(_ => _.Visible).Last();
        }

        #endregion

        #region Focused Cell Management

        internal string GetFocusClass(Cell<object> cell)
            => _focusedCells.TryGetValue(cell.Uid, out var focused) && focused
                ? "e-focus e-focused"
                : string.Empty;

        internal void SetFocusedCell(string cellUid, bool isFocused)
            => _focusedCells[cellUid] = isFocused;

        #endregion

        #region Cell Click and Focus Handling

        internal async Task CellClickHandler(ValueTuple<Row<object>, Cell<object>> target, MouseEventArgs e, bool isHeader = false)
        {
            var eventArgs = e;
            _isHeader = isHeader;
            IsChildFocused = false;
            Row<object> row = target.Item1;
            Cell<object> cell = target.Item2;
            ClearCurrent();
            SetCurrent(row, cell);
            _parent.SoftRefresh = true;
            if (!_parent.AllowSelection)
            {
                _parent.EventAggregator.Trigger("RowStateChanged", row);
                await Focus(row?.Uid!, cell?.Uid!, cellColIndex: cell?.Index + 1 ?? -1).ConfigureAwait(true);
            }
            if (_parent.ShowTooltip && _parent.TooltipInstance != null)
            {
                await (_parent.TooltipInstance?.CloseAsync())!.ConfigureAwait(true);
            }
        }

        internal async Task Refresh(Row<object> Row, Cell<object> Cell, bool isHeader = false, bool isCtrlOrShiftKeyPressed = false)
        {
            _isHeader = !isHeader ? isHeader : _isHeader;
            if (!_currentRows.Any(_ => _?.Uid == Row?.Uid))
            {
                return;
            }

            _parent.SoftRefresh = true;
            _parent.EventAggregator.Trigger("RowStateChanged", Row);
            if (isCtrlOrShiftKeyPressed)
            {
                await Focus(Row?.Uid ?? _current.Item1, Cell?.Uid ?? _current.Item2, cellColIndex: Cell?.Index + 1 ?? -1).ConfigureAwait(true);
            }
        }

        #endregion

        #region Current Cell and Focus State Management

        internal void ClearCurrent()
        {
            if (!string.IsNullOrEmpty(_current.Item1))
            {
                List<Row<object>> _allRows = _parent.HeaderRows.Concat(_parent.Rows).ToList();
                Row<object>? _row = _allRows.Find(_ => _?.Uid?.Equals(_current.Item1, StringComparison.Ordinal) == true);
                Cell<object>? _cell = _row?.Cells.Find(_ => _?.Uid == _current.Item2);
                if (_cell != null && _cell.IsFocused)
                {
                    _cell.TabIndex = -1;
                    _cell.IsEdit = false;
                    _cell.IsFocused = false;
                    _cell.ShowFocusLine = false;
                    _parent.SoftRefresh = true;
                    _parent.EventAggregator.Trigger("RowStateChanged", _row!);
                }
            }
        }

        internal void SetCurrent(Row<object> row, Cell<object> cell, bool outline = false)
        {
            if (!cell.IsFocused)
            {
                cell.TabIndex = 0;
                cell.IsFocused = true;
                cell.ShowFocusLine = outline;
                SelectedCellIndex = cell.Index;
                if (IsByKey && row.IsSelected)
                {
                    SelectedRowIndex = row.Index;
                }
                _current = (row?.Uid!, cell?.Uid!);
            }
        }

        internal async Task ClearFocus(Row<object>? _row = null, Cell<object>? _cell = null)
        {
            List<Row<object>>? _visibleFrozenheader = _frozenHeaderRows?.Where(_ => _.Visible != false).ToList();
            if (_parent.IsEdit && _parent.EditSettings?.Mode == EditMode.Normal)
            {
                return;
            }
            if (_cell != null && _cell.IsFocused)
            {
                _cell.TabIndex = -1;
                _cell.IsFocused = false;
                _cell.ShowFocusLine = false;
                if (_visibleFrozenheader?.Count == 0)
                {
                    ClearCurrent();
                }
                _current = (string.Empty, string.Empty);
                _parent.SoftRefresh = true;
                _parent.EventAggregator.Trigger("RowStateChanged", _row!);
                await Task.CompletedTask.ConfigureAwait(true);
            }
        }

        #endregion

        #region Keyboard Event Processing

        internal async Task ProcessKeyDown(KeyboardEventArgs e, Row<object> row, Cell<object> cell, bool isHeader = false)
        {
            IsByKey = true;
            _isHeader = isHeader;
            GridKeySettings? _settings = _parent?.KeySettings;
            LastKeyCombination = e;
            LastNavigatedCellIdx = (int)(row?.Cells?.Find(c => c == cell)?.Index ?? -1);
            if (_parent != null && _parent.EnableColumnVirtualization)
            {
                List<GridColumn> orderedColumns = _parent.RearrangeColumns(_parent.Columns!);
                LastNavigatedCellIdx = cell != null && !string.IsNullOrEmpty(cell.Column?.Field) ? orderedColumns.FindIndex(x => x.Field == cell?.Column.Field) 
                    : orderedColumns.Count - 1;
            }
            KeyInvokedRow = row!;
            KeyInvokedCell = cell!;
            var keyCombination = e.GetKeyCombination(isMacDevice: _parent!.IsMacDevice ?? false);
            bool isFilterBar = _parent.AllowFiltering && _parent.FilterSettings!.Type == FilterType.FilterBar;
            bool isAddForm = _parent.EditSettings!.ShowAddNewRow;
            bool isArrowUp = keyCombination != null && keyCombination.Equals("ArrowUp", StringComparison.Ordinal);
            bool isArrowDown = keyCombination != null && keyCombination.Equals("ArrowDown", StringComparison.Ordinal);
            bool isArrowLeft = keyCombination != null && keyCombination.Equals("ArrowLeft", StringComparison.Ordinal);
            bool isArrowRight = keyCombination != null && keyCombination.Equals("ArrowRight", StringComparison.Ordinal);
            bool isArrowKeys = isArrowUp || isArrowDown || isArrowLeft || isArrowRight;
            bool isTabKey = keyCombination != null && keyCombination.Equals("Tab", StringComparison.Ordinal);
            bool isShiftTabKey = keyCombination != null && keyCombination.Equals("ShiftTab", StringComparison.Ordinal);
            if (cell?.IsEdit == true && _parent.IsEdit && _parent.EditSettings.Mode == EditMode.Batch && keyCombination != null && (isArrowKeys || isMultiSelectPopUpOpened && keyCombination.Equals("Enter", StringComparison.Ordinal)))
            {
                isMultiSelectPopUpOpened = false;
                return;
            }
            if (row?.Index == null && _parent.HeaderRows?.Count > 0 && _parent.HeaderRows.Where(e => e == row).Any() && (row!.Cells?.Where(e => e.Visible).FirstOrDefault() == cell || row.Cells?.Where(e => e.Visible).LastOrDefault() == cell))
            {
                cell!.IsFocused = true;
            }
            if (cell?.Column?.AllowFiltering == true && isFilterBar && ((_parent.Rows?.Count > 0 && _parent.Rows.FirstOrDefault()!.Equals(row) && isArrowUp && !isAddForm) || (_isHeader && isArrowDown)))
            {
                int columnIndex = -1;
                bool filterTemplate = false;
                ClearCurrent();
                if (isArrowUp && _parent.SelectedRecords?.Count > 0 && !_parent.SelectionSettings!.PersistSelection && _parent.SelectionModule != null)
                {
                    await _parent.SelectionModule.ClearSelection().ConfigureAwait(true);
                }
                if (_parent.Columns?.Where(col => !col.Visible).Any() == true)
                {
                    var visibleColumns = await _parent.GetVisibleColumnsAsync().ConfigureAwait(true);
                    columnIndex = visibleColumns.FindIndex(col => col.Index == cell.Column.Index);
                    columnIndex = _parent.AllowGrouping && _parent.GroupSettings?.Columns != null && _parent.GroupSettings.Columns.Length > 0 && cell.Column.FilterTemplate != null ? _parent.GroupSettings.Columns.Length + columnIndex : columnIndex;
                }
                filterTemplate = ((cell?.IsDataCell == true || _isHeader) && cell?.Column?.FilterTemplate != null) || IsGroupingFilterTemplate(cell!);
                await _parent.InvokeMethod("sfBlazor.Grid.focusFilterBar", new object[] { _parent.DataId, keyCombination!, filterTemplate, columnIndex }).ConfigureAwait(true);
                return;
            }

            if (row?.Index == null && !isFilterBar && GetLastVisibleColumn() != null && cell?.Index == GetLastVisibleColumn()?.Index && _parent.Rows?.Count == 0 && _isHeader && keyCombination != null && keyCombination.Equals("Tab", StringComparison.Ordinal))
            {
                ClearCurrent();
                await _parent.InvokeMethod("sfBlazor.Grid.blurActiveElement", new object[] { _parent.DataId }).ConfigureAwait(true);
                return;
            }
            if (!string.IsNullOrEmpty(keyCombination) && keyCombination == "Insert" && _parent.SelectedRecords?.Count == 0)
            {
                return;
            }
            if (cell!.IsFocused && _parent.EditSettings.AllowEditing && _parent.EditSettings.Mode == EditMode.Batch && !string.IsNullOrEmpty(keyCombination) && keyCombination == "Enter")
            {
                cell.IsFocused = false;
            }

            bool isColumnFilter = _parent.Columns?.Where(e => e.AllowFiltering).Any() == true;
            bool isFilterTemplate = false;

            // to focus the filter bar if tab is pressed from the last header cell.
            if (isTabKey && isHeader && isColumnFilter && row?.Index == null && cell?.Index == row?.Cells?.Where(e => e.Visible).LastOrDefault()?.Index && isFilterBar)
            {
                ClearCurrent();
                isFilterTemplate = _parent.Columns?.Where(e => e.Visible).Any() == true && _parent.Columns?.Where(e => e.Visible).FirstOrDefault()?.FilterTemplate != null;
                await _parent.InvokeMethod("sfBlazor.Grid.focusFilterBar", new object[] { _parent.DataId, keyCombination!, isFilterTemplate, -1 }).ConfigureAwait(true);
                return;
            }

            // to focus the last filter bar cell when shiftTab is pressed from the first content cell.
            if (isShiftTabKey && _parent.Rows?.Count > 0 && isColumnFilter && _parent.Rows.First().Equals(row) && _parent.Rows.First().Cells?.Where(_ => _.Visible).FirstOrDefault()!.Equals(cell) == true && isFilterBar && !isAddForm)
            {
                ClearCurrent();
                isFilterTemplate = _parent.Columns?.Where(e => e.Visible).Any() == true && _parent.Columns.Where(e => e.Visible).LastOrDefault()?.FilterTemplate != null;
                if (cell?.IsEdit == true)
                {
                    await _parent.EditModule!.SaveCell().ConfigureAwait(true);
                }
                await _parent.InvokeMethod("sfBlazor.Grid.focusFilterBar", new object[] { _parent.DataId, keyCombination!, isFilterTemplate, -1 }).ConfigureAwait(true);
                return;
            }

            if(isAddForm)
            {
                // to focus on the first cell of the 'Add form' when tab is pressed from the last header cell.
                if (isTabKey && isHeader && row?.Index == null && cell?.Index == row?.Cells?.Where(e => e.Visible).LastOrDefault()?.Index && _parent.EditSettings.NewRowPosition == NewRowPosition.Top)
                {
                    FocusAddForm(keyCombination!);
                    return;
                }

                // to focus on the last cell of the 'Add form' when shiftTab is pressed from the first content cell.
                if (isShiftTabKey && _parent.Rows?.Count > 0 && _parent.Rows.First().Equals(row) && _parent.Rows.First().Cells?.Where(_ => _.Visible).FirstOrDefault()?.Equals(cell) == true && _parent.EditSettings.NewRowPosition == NewRowPosition.Top)
                {
                    FocusAddForm(keyCombination!);
                    return;
                }

                if ((_parent.Rows?.Count > 0 && _parent.Rows.FirstOrDefault()!.Equals(row) && isArrowUp) || (_isHeader && isArrowDown))
                {
                    FocusAddForm(keyCombination!);
                    return;
                }
            }

            if (row?.IsSelected == true && (isTabKey || isShiftTabKey))
            {
                SelectedRowIndex = row?.Index;
            }
            SelectedCellIndex = cell?.Index;

            if (string.IsNullOrEmpty(keyCombination) || (keyCombination.Equals("Space", StringComparison.Ordinal) && (_parent.IsEdit || (cell?.Column != null && cell.Column.Type.Equals(ColumnType.CheckBox) && !_parent.SelectionSettings!.CheckboxOnly))))
            {
                return;
            }

            string[] actions = _settings?.GetAction(keyCombination)?? Array.Empty<string>();
            bool isMoveUpOrDown = actions.Length > 0 && (actions[0].Equals("MoveUpCell", StringComparison.Ordinal) || actions[0].Equals("MoveDownCell", StringComparison.Ordinal));

            // Escape from template cell to parent cell
            bool isReturn = EscapeFromTempalteCell(keyCombination) || (_parent.AllowSorting == false && keyCombination == "Enter" && row?.Index == null && _parent.HeaderRows?.Count > 0 && _parent.HeaderRows.Where(x => x == row).Any()) || (keyCombination == "ShiftEnter" && row?.Index == 0);
            if (isReturn)
            {
                return;
            }

            if (await FocusChild(row!, cell!, e).ConfigureAwait(true))
            {
                return;
            }

            if (isHeader && _parent.HeaderRows!.FirstOrDefault()!.Cells?.Where(x => x.Index != null).FirstOrDefault() == cell && keyCombination == "Tab")
            {
                foreach (var item in _parent.HeaderRows![0].Cells)
                {
                    item.IsFocused = false;
                }
            }

            if (!_isHeader && _parent.Rows?.Count == 0 || !cell?.IsEdit == true && !cell?.EditDisabled == true &&
                ShouldExitGrid(actions!, row!, cell!, keyCombination))
            {
                ClearCurrent();
                return;
            }

            if (actions?.Length != 0)
            {
                
                await MoveFocusCell(actions!, row!, cell!, e, keyCombination).ConfigureAwait(true);
            }
            else
            {
                List<Row<object>>? _rows = _currentRows?.Where(x => x.Visible != false).ToList();
                List<Row<object>>? _dataRows = _rows?.Where(_ => _.IsDataRow).ToList();
                Row<object>? nextRow = row;
                Cell<object> nextCell = cell!;
                bool isCheckBoxCell = cell?.Column != null && cell.Column.Type.Equals(ColumnType.CheckBox);

                switch (keyCombination)
                {
                    case "CtrlHome":
                        nextRow = _rows?[0]!;
                        nextCell = _rows?[0]?.Cells?.Where(x => x.Visible && !x.CellType.Equals(CellType.RowDrag)).First()!;
                        break;
                    case "CtrlEnd":
                        nextRow = _rows?.Last()!;
                        nextCell = nextRow?.Cells?.Where(x => x.Visible).Last()!;
                        break;
                    case "Home":
                        nextCell = nextRow?.Cells?.Where(x => x.Visible && !x.CellType.Equals(CellType.Indent) && !x.CellType.Equals(CellType.RowDrag)).First()!;
                        break;
                    case "End":
                        nextCell = nextRow?.Cells?.Where(x => x.Visible).Last()!;
                        break;
                    case "Space":
                        if (isCheckBoxCell && !isHeader && !nextRow?.IsSelected == true)
                        {
                            await _parent.SelectionModule!.SelectByRow(nextRow!).ConfigureAwait(true);
                        }
                        else if (isCheckBoxCell && nextRow?.IsSelected == true && (_parent != null && _parent.SelectionModule != null && _parent.SelectionModule.CanToggle()) && !isHeader)
                        {
                            await _parent.SelectionModule.ClearSelectionByRow(nextRow).ConfigureAwait(true);
                        }
                        break;
                    case "AltDown":
                        if (_parent.ShowColumnMenu && cell?.Column?.ShowColumnMenu == true && isHeader)
                        {
                            _parent.ColumnMenuColumn = cell?.Column;
                            _parent.EventAggregator.Trigger("ColumnMenuUpdate", null!);
                            if (_parent.ColumnMenuInstance != null &&  _parent.ColumnMenuInstance.CssClass.Contains("e-hide-menu", StringComparison.Ordinal))
                            {
                                await _parent.ColumnMenuInstance.OpenAsync(1, 1).ConfigureAwait(true);
                            }
                        }
                        else if (_parent.AllowFiltering && cell?.Column?.AllowFiltering == true && _parent.FilterSettings!.Type != FilterType.FilterBar && isHeader && _parent.FilterModule != null)
                        {
                            _parent.FilterModule.FilterIconIsClicked = true;
                            _parent.FilterModule.FilterIconColumn = cell?.Column!;
                            _parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                        }

                        break;
                }

                // Other than navigation other keys will be handled here.
                await FocusCell(row: nextRow!, cell: nextCell, navigator: (0, 0), e: e, actions: actions, keyCombination: keyCombination).ConfigureAwait(true);
            }
            // Only allow scrollToFocusedCell for Shift+Tab and Left Arrow when frozen columns exist
            bool isShiftTab = keyCombination != null && keyCombination.Equals("ShiftTab", StringComparison.Ordinal);
            bool isLeftArrow = keyCombination != null && keyCombination.Equals("ArrowLeft", StringComparison.Ordinal);
            bool shouldScrollForLeftFrozen = (_parent!.FrozenColumns > 0 || _parent.FreezeModule!.GetFreezeLeftCount() > 0) && (isShiftTab || isLeftArrow);
            if (_parent != null && _parent.FreezeModule!.GetFrozenCount() > 0 && actions?.Length != 0 && !(isMoveUpOrDown && _parent.IsEdit && _parent.EditSettings.Mode == EditMode.Batch) 
                && (_parent.FreezeModule!.GetFreezeRightCount() > 0 || shouldScrollForLeftFrozen))
            {
                await _parent.InvokeMethod("sfBlazor.Grid.scrollToFocusedCell", _parent.DataId).ConfigureAwait(true);
            }
            _previouslyFocusedCell = cell!;
            KeyInvokedRow = null!;
            KeyInvokedCell = null!;
        }

        #endregion

        #region Focus Movement and Navigation

        private async void FocusAddForm(string keyCombination)
        {
            ClearCurrent();
            await _parent.InvokeMethod("sfBlazor.Grid.focusAddForm", _parent.DataId, keyCombination).ConfigureAwait(true);
        }
        private bool EscapeFromTempalteCell(string keyCombination)
        {
            bool isReturn = false;
            if (IsChildFocused)
            {
                if (keyCombination == "Escape")
                {
                    IsChildFocused = false;
                }
                else
                {
                    isReturn = true;
                }
            }
            return isReturn;
        }

        #endregion

        #region Focus Cell Movement

        private async Task MoveFocusCell(string[] actions, Row<object> row, Cell<object> cell, KeyboardEventArgs e, string keyCombination)
        {
            switch (actions?[0])
            {
                case "MoveLeftCell":
                    if (row?.Index == null && cell?.Index == GetFirstVisibleColumn()?.Index && (keyCombination.Equals("ArrowLeft", StringComparison.Ordinal)))
                    {
                        return;
                    }
                    await FocusCell(row: row!, cell: cell!, navigator: (0, -1), e: e, actions: actions, keyCombination: keyCombination).ConfigureAwait(true);
                    break;
                case "MoveRightCell":
                    if (row?.Index == null && cell?.Index == GetLastVisibleColumn()?.Index && (keyCombination.Equals("ArrowRight", StringComparison.Ordinal) || (_parent.AllowFiltering && _parent.FilterSettings?.Type == FilterType.FilterBar && _parent.Columns?.Where(e => e.AllowFiltering).Any() == true)))
                    {
                        return;
                    }
                    await FocusCell(row: row!, cell: cell!, navigator: (0, 1), e: e, actions: actions, keyCombination: keyCombination).ConfigureAwait(true);
                    break;
                case "MoveUpCell":
                    await FocusCell(row: row, cell: cell, navigator: (-1, 0), e: e, actions: actions, keyCombination: keyCombination).ConfigureAwait(true);
                    break;
                case "MoveDownCell":
                    await FocusCell(row: row, cell: cell, navigator: (1, 0), e: e, actions: actions, keyCombination: keyCombination).ConfigureAwait(true);
                    break;
            }
        }

        #endregion

        #region Grid Exit Validation

        // This ensures the key control can be provided to other elements in page.
        private bool ShouldExitGrid(string[] actions, Row<object> row, Cell<object> cell, string combination)
        {
            var _stackedHeader = HeaderRows?.Where(_ => _.Visible != false).ToList();
            if (actions?.Length != 0)
            {
                switch (combination)
                {
                    case "ShiftTab":
                        if (ShouldExit(row, _stackedHeader!))
                        {
                            _parent.InvokeMethod("sfBlazor.Grid.gridFocus", new object[] { _parent.DataId, true, false, combination }).ConfigureAwait(true);
                            return true;
                        }
                        break;
                    case "Tab":
                        if (_parent.FreezeModule!.GetFrozenCount() > 0 && _parent.FreezeModule!.GetFreezeRightCount() > 0)
                        {
                            var rightFrozenColumn = _parent.FreezeModule!.GetFrozenRightFreezeColumns();
                            var lastRightFrozenColumn = rightFrozenColumn?.Where(x => x.Visible).LastOrDefault();
                            if (cell?.Index == lastRightFrozenColumn?.Index 
                                || (!string.IsNullOrEmpty(cell?.Column?.Field) && cell?.Column?.Field == lastRightFrozenColumn?.Field && _parent.EnableColumnVirtualization))
                            {
                                return false;
                            }
                        }
                        if (row?.Index == null && (cell?.Index == GetLastVisibleColumn()?.Index || _stackedHeader?.Count > 1))
                        {
                            return false;
                        }
                        else
                        {
                            Row<object>? _veryLastRow = _currentRows?.Where(e => e.Visible).LastOrDefault();
                            Cell<object>? _veryLastCell = _veryLastRow?.Cells?.Where(_ => _.Visible).LastOrDefault();
                            return _veryLastCell?.Equals(cell) == true && !row?.IsDetailRow == true;
                        }
                }
            }

            return false;
        }

        private bool ShouldExit(Row<object> row, List<Row<object>> stackedHeader)
        {
            var _visibleFrozenheader = _frozenHeaderRows?.Where(_ => _.Visible != false).ToList();
            var focusedCellIndex = row?.Cells?.Where(_ => _.IsFocused).Any() == true ? row.Cells.Where(_ => _.IsFocused).FirstOrDefault()?.Index : null;
            var focusedCell = row?.Cells?.Where(_ => _.IsFocused).FirstOrDefault();
            var firstColumnIndex = _parent.FreezeModule!.GetFrozenCount() > 0 ? focusedCell?.Column?.Index : _parent.Columns?.Where(e => e.Visible).FirstOrDefault()?.Index;

            if (row?.Index == null && focusedCellIndex != null && firstColumnIndex != null && focusedCellIndex == firstColumnIndex && ((_visibleFrozenheader?.Count == 0 && stackedHeader?[0]?.Cells?.Where(e => e.Visible).FirstOrDefault()?.IsFocused == true) || (_visibleFrozenheader?.Count != 0 && (_visibleFrozenheader != null && _visibleFrozenheader[0].Cells?[0]?.IsFocused == true))))
            {
                return true;
            }
            if (row?.IsCaptionRow == true && !_parent.GroupSettings!.ShowGroupedColumn && _parent.Columns?.Count == _parent.Columns?.Where(e => e.Visible == false).Count() && _currentRows?.FirstOrDefault()?.Cells?.FirstOrDefault()?.IsFocused == true)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Grid Key Down Processing

        internal async Task ProcessGridKeyDown(KeyboardEventArgs e, BeforeCellFocus? bf = null, bool isPagerFocused = true, bool isToolbarFocused = false, int? cellIndex = null, int? rowIndex = null, int? templateCellIndex = null, bool focusColumnTemplate = false)
        {
            var keyCombination = e.GetKeyCombination(isMacDevice: _parent.IsMacDevice ?? false);
            var isTabKey = keyCombination != null && keyCombination.Equals("Tab", StringComparison.Ordinal);
            var isShiftTabKey = keyCombination != null && keyCombination.Equals("ShiftTab", StringComparison.Ordinal);
            var editedRow = _parent.EditModule!.EditedRow;
            var editedRowFirstCell = editedRow?.Cells?.Where(e => e.Visible && e.CellType != CellType.RowDrag).FirstOrDefault();
            var editedRowLastCell = editedRow?.Cells?.Where(e => e.Visible && e.CellType != CellType.RowDrag).LastOrDefault();
            var cellUId = cellIndex != null ? editedRow?.Cells[(int)cellIndex] : null;
            var isExcelOrCheckBoxFilter = _parent.AllowFiltering && ((_parent.FilterSettings != null && _parent.FilterSettings.Type != FilterType.FilterBar && _parent.FilterSettings.Type != FilterType.Menu) || (_parent .FilterModule != null &&  _parent.FilterModule.FilterIconColumn?.FilterSettings != null && !_parent.FilterModule.FilterIconColumn.FilterSettings.Type.Equals(FilterType.FilterBar) && !_parent.FilterModule.FilterIconColumn.FilterSettings.Type.Equals(FilterType.Menu)));
            ClickedCheckBoxId = isExcelOrCheckBoxFilter ? string.Empty : ClickedCheckBoxId;
            IsSelectAllClicked = isExcelOrCheckBoxFilter ? false : IsSelectAllClicked;

            // Handling the focus from the template column.		
            if (rowIndex != null && templateCellIndex != null && focusColumnTemplate && keyCombination != null)
            {
                var templateRow = _parent.Rows?.Where(e => e.Index == (int)rowIndex).FirstOrDefault();
                var templateCell = templateRow?.Cells[(int)templateCellIndex];
                if (templateRow == _parent.Rows?.Where(e => e.IsLastRow).FirstOrDefault() && templateCell == templateRow?.Cells?.Where(e => e.Visible).LastOrDefault() && keyCombination.Equals("Tab", StringComparison.Ordinal))
                {
                    return;
                }
                IsChildFocused = false;
                if (_parent.IsRenderedFromTreeGrid && e != null && _parent.IsEdit && keyCombination == "Escape"
                    && templateRow != null && templateCell != null)
                {
                    await ProcessKeyDown(e, templateRow, templateCell).ConfigureAwait(true);
                    await ProcessKeyCombination(keyCombination, 0).ConfigureAwait(true);
                }
                ClearCurrent();
                SetCurrent(templateRow!, templateCell!);
                await Task.Yield();
                await Focus(templateRow?.Uid!, templateCell?.Uid!, keyCombination: keyCombination, cellColIndex: templateCell?.Index + 1 ?? -1).ConfigureAwait(true);
                return;
            }

            // Handling the mouse click focusing when clicked while the row is in editable state.
            if (cellIndex != null && editedRow != null)
            {
                bool isShiftTabAndDrag = isShiftTabKey && _parent.AllowRowDragAndDrop;
                bool isTabAndDrag = isTabKey && _parent.AllowRowDragAndDrop;

                if (_parent.IsEdit || (isTabKey && editedRowLastCell?.Index == (int)cellIndex) || (isShiftTabKey && editedRowFirstCell?.Index == (int)cellIndex) || (isTabAndDrag && editedRowLastCell?.Uid == cellUId?.Uid) || (isShiftTabAndDrag && editedRowFirstCell?.Uid == cellUId?.Uid) && (isTabKey || isShiftTabKey))
                {
                    ClearCurrent();
                    SetCurrent(editedRow, editedRow.Cells[(int)cellIndex]);
                }
            }

            if (ChangeLastCellTabIndex)
            {
                ChangeLastCellTabIndex = false;
                _parent.EventAggregator.Trigger("RowStateChanged", _parent.Rows?.Last()!);
            }

            // Handling the focus when a record is in editable state in inline editing.
            if (!isToolbarFocused && _parent.IsEdit && _parent.EditSettings?.Mode == EditMode.Normal && _parent.EditSettings.AllowEditing && editedRow != null)
            {
                var currentCellObject = editedRow.Cells?.Where(e => e.IsFocused && e.CellType != CellType.RowDrag).FirstOrDefault();
                var currentCellIndex = !_parent.AllowRowDragAndDrop ? currentCellObject?.Index : editedRow?.Cells?.IndexOf(currentCellObject!);
                var lastCellIndex = !_parent.AllowRowDragAndDrop ? editedRow?.Cells?.Where(e => e.Visible && e.CellType != CellType.RowDrag && (e.Column != null && e.Column.AllowEditing)).LastOrDefault()?.Index : editedRow?.Cells?.IndexOf(editedRowLastCell!);
                var firstCellIndex = !_parent.AllowRowDragAndDrop ? editedRow?.Cells?.Where(e => e.Visible && e.CellType != CellType.RowDrag && (e.Column != null && e.Column.AllowEditing)).FirstOrDefault()?.Index : editedRow?.Cells?.IndexOf(editedRowFirstCell!);

                if (currentCellIndex != null && lastCellIndex != null && firstCellIndex != null && ((isTabKey && currentCellIndex != lastCellIndex) || (isShiftTabKey && currentCellIndex != firstCellIndex)))
                {
                    var focusCellIndex = GetNextCellIndex(isTabKey, (int)currentCellIndex, editedRow!);
                    // to change the cell index if the next column is an primary key column while editing.
                    if (editedRow?.Cells?[(int)focusCellIndex]?.Column?.IsPrimaryKey == true)
                    {
                        focusCellIndex = isTabKey ? focusCellIndex + 1 : focusCellIndex == 0 ? focusCellIndex : focusCellIndex - 1;
                    }
                    // handling the columns visiblity set to false.
                    while (editedRow?.Cells?[(int)focusCellIndex]?.Visible != true)
                    {
                        focusCellIndex = isTabKey ? focusCellIndex + 1 : focusCellIndex - 1;
                    }
                    ClearCurrent();
                    SetCurrent(editedRow, editedRow?.Cells[(int)focusCellIndex]!);
                }
            }
            bool hasCurrentRows = _currentRows != null && _currentRows.Count > 0 && _currentRows.FirstOrDefault()?.Index != null;
            if (string.IsNullOrEmpty(keyCombination) && _parent.SelectedRowIndexes?.Count > 0)
            {
                var rowIndexForTree = (int)_parent.SelectedRowIndexes[0];
                if (hasCurrentRows && _currentRows?[0].Index != 0 && rowIndexForTree >= _currentRows?[0].Index)
                {
                    rowIndexForTree -= (int)_currentRows[0].Index!;
                }
                var isTHeader = _isHeader;
                BeforeCellFocus tbcf = new BeforeCellFocus()
                {
                    KeyCombination = keyCombination!, 
                    Action = null!,
                    Cancel = false,
                    Cell = hasCurrentRows ? _currentRows?[rowIndexForTree]?.Cells[0]! : null!,
                    Row = hasCurrentRows ? _currentRows?[rowIndexForTree]! : null!,
                    KeyArgs = e,
                    IsKeyEvent = e != null,
                    IsHeader = isTHeader
                };
                _parent.EventAggregator.Trigger("HandleNullKey", tbcf);
            }
            if (string.IsNullOrEmpty(keyCombination) || IsChildFocused)
            {
                return;
            }

            DataReadyArgs<T> eventArgs = new DataReadyArgs<T>() { Count = _parent.TotalItemCount };
            _parent.EventAggregator.Trigger("GetPageCount", eventArgs);
            _parent.TotalItemCount = eventArgs.Count;
            int tPage = 0;
            if (_parent.PageModule != null)
            {
                tPage = _parent.PageModule.CalculateTotalPages();
            }

            //IsRendered from TreeGrid to prevent insert key from add new record on cell edit.
            if (_parent.IsRenderedFromTreeGrid && _parent.EditSettings?.Mode == EditMode.Batch && _parent.IsEdit && keyCombination == "Insert")
            {
                _parent.EventAggregator.Trigger("OnKeyDown", e!);
            }

            await ProcessKeyCombination(keyCombination, tPage, bf, isPagerFocused, e).ConfigureAwait(true);

            if (keyCombination.Equals("AltW", StringComparison.Ordinal) && _parent.Rows != null && _parent.Rows.Count > 0 && string.IsNullOrEmpty(_parent.EditModule.AlertMessage))
            {
                var firstRow = _parent.Rows?.Where(_ => _.Visible).FirstOrDefault();
                var firstCell = firstRow?.Cells?.Where(_ => _.Visible && _.CellType != CellType.RowDrag).FirstOrDefault();
                await ProcessKeyDown(e!, firstRow!, firstCell!).ConfigureAwait(true);
            }
            if (((_parent.GridEvents?.RowDataBound.HasDelegate == true) || _parent.IsRenderedFromTreeGrid) && !_parent.IsEdit && _parent.EditSettings!.Mode.Equals(EditMode.Batch) && _parent.Rows!.Any(_ => _.Cells.Any(_ => _.IsDirty)) && (_parent.FocusModule != null && _parent.FocusModule.SelectedRowIndex.HasValue))
            {
                var selectedRow = _currentRows?.Find(_ => _.Index == _parent.FocusModule.SelectedRowIndex);
                var selectedCell = _parent.FocusModule.SelectedCellIndex.HasValue ? selectedRow?.Cells[(int)_parent.FocusModule.SelectedCellIndex] : selectedRow?.Cells[0];

                await ProcessKeyDown(e!, selectedRow!, selectedCell!).ConfigureAwait(true);
            }
        }

        // The below method returns the next editable cell index based on Tab or ShiftTab key.
        /// <summary>
        /// Gets the next editable cell index based on Tab or ShiftTab key.
        /// </summary>
        /// <param name="isTabKey">Specifies whether tab key is invoked or not.</param>
        /// <param name="currentCellIndex">Specifies the current editable cell index, where the key is invoked.</param>
        /// <param name="editedRow">Specifies the edited row object.</param>
        /// <returns><see cref="System.Threading.Tasks.Task{Integer}"/>.</returns>
        /// <exclude/>
        private static int GetNextCellIndex(bool isTabKey, int currentCellIndex, Row<object> editedRow)
        {
            if (isTabKey)
            {
                for (int i = currentCellIndex + 1; i <= editedRow.Cells.Count; i++)
                {
                    var cell = editedRow.Cells[i];
                    if (cell.Column != null && cell.Column.AllowEditing && cell.Visible)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for(int i = currentCellIndex - 1; i >= 0; i--)
                {
                    var cell = editedRow.Cells[i];
                    if (cell.Column != null && cell.Column.AllowEditing && cell.Visible && cell.CellType != CellType.RowDrag)
                    {
                        return i;
                    }
                }
            }
            return currentCellIndex;
        }

        #endregion

        #region Key Combination Processing

        private async Task ProcessKeyCombination(string keyCombination, int tPage, BeforeCellFocus? bf = null, bool isPagerFocused = true, KeyboardEventArgs? e = null)
        {
            // Handle Undo/Redo keyboard shortcuts (Ctrl+Z for undo, Ctrl+Y or Ctrl+Shift+Z for redo)
            if (keyCombination?.Equals("CtrlZ", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Check guards: EnableUndoRedo enabled, Batch mode, grid focused
                if (_parent.EditSettings?.EnableUndoRedo == true &&
                    _parent.EditSettings?.Mode == EditMode.Batch &&
                   
                    _parent.UndoRedoManager != null)
                {
                    // Capture the undone action and apply it to the grid
                    var undoneAction = await _parent.UndoRedoManager.UndoAsync().ConfigureAwait(true);
                    if (undoneAction != null)
                    {
                        // Trigger point: Apply the undo action to update grid UI (isRedoAction = false)
                        await _parent.UndoRedoManager.ApplyUndoRedoAction(undoneAction, isRedoAction: false).ConfigureAwait(true);
                    }
                    return;
                }
            }
            else if (keyCombination?.Equals("CtrlY", StringComparison.OrdinalIgnoreCase) == true ||
                     keyCombination?.Equals("CtrlShiftZ", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Check guards: EnableUndoRedo enabled, Batch mode, grid focused
                if (_parent.EditSettings?.EnableUndoRedo == true &&
                    _parent.EditSettings?.Mode == EditMode.Batch &&
                  
                    _parent.UndoRedoManager != null)
                {
                    // Capture the redone action and apply it to the grid
                    var redoneAction = await _parent.UndoRedoManager.RedoAsync().ConfigureAwait(true);
                    if (redoneAction != null)
                    {
                        // Trigger point: Apply the redo action to update grid UI (isRedoAction = true)
                        await _parent.UndoRedoManager.ApplyUndoRedoAction(redoneAction, isRedoAction: true).ConfigureAwait(true);
                    }
                    return;
                }
            }

            switch (keyCombination)
            {
                case "Insert":
                    if ((_parent.EditModule!.AlertMessage is "EditAlert" or "DeleteAlert") || (_parent.IsEdit && _parent.EditSettings?.Mode == EditMode.Batch))
                    {
                        return;
                    }
                    await _parent.AddRecordAsync().ConfigureAwait(true);
                    break;
                case "F2":
                    if(e != null && !e.AltKey && !e.CtrlKey && !e.ShiftKey)
                    {
                        await _parent.StartEditAsync().ConfigureAwait(true);
                    }
                    break;
                case "Delete":
                    await _parent.DeleteRecordAsync().ConfigureAwait(true);
                    break;
                case "Escape":
                    // Close all pop-ups here.
                    if (_parent.IsEdit || (_parent.EditSettings != null && _parent.EditSettings.ShowAddNewRow && !_parent.IsEdit && _parent.IsAdd))
                    {
                        ClearCurrent();
                        IsByKey = true;
                        await _parent.EditModule!.CloseEdit(true).ConfigureAwait(true);
                        IsByKey = false;
                    }

                    if (_parent.ShowColumnChooser && _parent.ChooserDialogInstance != null)
                    {
                        await _parent.ChooserDialogInstance.HideAsync().ConfigureAwait(true);
                    }

                    break;
                case "PageUp":
                case "PageDown":
                case "CtrlAltPageDown":
                case "CtrlAltPageUp":
                case "AltPageUp":
                case "AltPageDown":
                    if (!isPagerFocused && e != null && !e.ShiftKey && !_parent.IsEdit)
                    {
                        await ProcessPageKeyCombinations(keyCombination, tPage, bf).ConfigureAwait(true);
                    }

                    if (_parent.ShowTooltip && _parent.TooltipInstance != null)
                    {
                        await (_parent.TooltipInstance?.CloseAsync())!.ConfigureAwait(true);
                    }
                    break;
                case "CtrlP":
                case "MetaP":
                    if (bf != null)
                    {
                        bf.Cancel = true;
                    }

                    await _parent.PrintAsync().ConfigureAwait(true);
                    break;
            }
        }

        #endregion

        #region Page Key Combination Processing

        private async Task ProcessPageKeyCombinations(string keyCombination, int tPage, BeforeCellFocus? bf = null)
        {
            switch (keyCombination)
            {
                case "PageUp":
                    if (bf != null)
                    {
                        bf.Cancel = true;
                    }

                    if (_parent.Rows?.Count > 0)
                    {
                        int pNo = _parent.PageSettings!.CurrentPage - 1;
                        if (pNo > 0)
                        {
                            await _parent.GoToPageAsync(pNo).ConfigureAwait(true);
                        }
                    }

                    break;
                case "PageDown":
                    if (bf != null)
                    {
                        bf.Cancel = true;
                    }

                    if (_parent.Rows?.Count > 0)
                    {
                        int pNo = _parent.PageSettings!.CurrentPage + 1;
                        if (pNo <= tPage)
                        {
                            await _parent.GoToPageAsync(pNo).ConfigureAwait(true);
                        }
                    }
                    break;
                case "CtrlAltPageDown":
                    if (bf != null)
                    {
                        bf.Cancel = true;
                    }

                    if (_parent.Rows?.Count > 0)
                    {
                        await _parent.GoToPageAsync(tPage).ConfigureAwait(true);
                    }

                    break;
                case "CtrlAltPageUp":
                    if (bf != null)
                    {
                        bf.Cancel = true;
                    }

                    if (_parent.Rows?.Count > 0)
                    {
                        await _parent.GoToPageAsync(1).ConfigureAwait(true);
                    }
                    break;
                case "AltPageUp":
                    if (_parent.Rows?.Count > 0)
                    {
                        if (_parent.PageModule != null)
                        {
                            await _parent.PageModule.EllipsisButtonClickHandler("PreviousPage").ConfigureAwait(true);
                        }
                    }

                    break;
                case "AltPageDown":
                    if (_parent.Rows?.Count > 0 && _parent.PagerRef != null)
                    {
                        if (_parent.PageModule != null)
                        {
                            await _parent.PageModule.EllipsisButtonClickHandler("NextPage").ConfigureAwait(true);
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Grid Focus Processing

        //Need to check use case of this method
        internal void ProcessGridFocus(FocusEventArgs e)
        {
            List<Row<object>>? _visibleParentRows = _parent.Rows?.Where(_ => _.Visible).ToList();
            List<Cell<object>>? _frozenCells = null;
            for (int i = 0; i < _visibleParentRows?.Count; i++)
            {
                var totalCells = _visibleParentRows[i].Cells.Count;
                for (int j = 0; j < totalCells; j++)
                {
                    if (_visibleParentRows[i]?.Cells[j]?.IsFrozen == true)
                    {
                        _frozenCells = _visibleParentRows[i]?.Cells?.Where(_ => _.IsFrozen).ToList();
                    }
                }
            }
            if (string.IsNullOrEmpty(_current.Item1) && (_parent.HeaderRows?.Count > 0 || _parent.Rows?.Count > 0))
            {
                if (_frozenCells != null && _frozenCells.Count != 0)
                {
                    _frozenCells[0].IsFocused = true;
                }
                IsChildFocused = false;
                _parent.SoftRefresh = true;
            }
        }

        #endregion

        #region Primary Focus Management

        internal async Task Focus(string rowUid, string cellUid, string? action = null, string? keyCombination = null, string? headerUid = null, int cellColIndex = -1, bool isSelectionMethodInvoked = false,
            bool isLastBatchEditCell = false)
        {
            if (_parent.IsRendered)
            {
                if (!string.IsNullOrEmpty(action) && action.Equals("SaveCell", StringComparison.Ordinal))
                {
                    await _parent.InvokeMethod("sfBlazor.Grid.gridFocus", new object[] { _parent.DataId, false, isLastBatchEditCell }).ConfigureAwait(true);
                }
                await _parent.InvokeMethod("sfBlazor.Grid.focus", new object[] { _parent.DataId, rowUid, cellUid, action!, keyCombination!, headerUid!, cellColIndex, isSelectionMethodInvoked, isLastBatchEditCell }).ConfigureAwait(true);
            }
        }

        private async Task<bool> FocusFirstChild(string rowUid, string cellUid, int cellColIndex = -1)
        {
            if (_parent.IsRendered)
            {
                bool val = await _parent.InvokeMethod<bool>("sfBlazor.Grid.focusChild", false, new object[] { _parent.DataId, rowUid, cellUid, cellColIndex }).ConfigureAwait(true);
                return val;
            }
            else
            {
                return false;
            }
        }

        /// Checks if the pressed key is the F2 key without any modifier keys (Shift, Ctrl, Alt).
        private static bool IsF2KeyPressed(KeyboardEventArgs e)
        {
            return e?.Key == "F2" && !(e.ShiftKey || e.CtrlKey || e.AltKey);
        }

        #endregion

        #region Cell Focus and Navigation

        private async Task FocusCell(Row<object> row, Cell<object> cell,
    string keyCombination, string[] actions, KeyboardEventArgs e,
    ValueTuple<int, int> navigator)
        {
            var isheader = _isHeader;
            ValueTuple<Row<object>, Cell<object>> tuple = (null!, null!);
            string? action = null;
            if (actions?.Length != 0)
            {
                action = actions?[0];
                if (_parent.DetailRowModule != null && await _parent.DetailRowModule.HandleDetailTemplateFocusNavigationAsync(row, cell, e, _previouslyFocusedCell).ConfigureAwait(true))
                {
                    ClearCurrent();
                    return;
                }
                else
                {
                    bool isEnterKeyFromDetailCell = keyCombination.Equals("Enter", StringComparison.Ordinal) && cell?.CellType == CellType.Detail;
                    tuple = isEnterKeyFromDetailCell ? (row!, cell!) : GetNextRowCell(row!, cell!, e, navigator, action!);
                }

                if (_parent.AllowPaging && tuple.Item1 == null && tuple.Item2 == null)
                {
                    if (_parent.EditSettings != null && _parent.EditSettings.Mode == EditMode.Batch && _parent.EditModule!.IsLastRow && keyCombination == "Enter")
                    {
                        await _parent.EditModule.SaveCell().ConfigureAwait(true);
                    }
                    ClearCurrent();
                    return;
                }
            }
            else
            {
                tuple = (row, cell);
            }
            object cellValue = _parent.PropHelper?.GetObject(tuple.Item2?.Column?.Field!, tuple.Item1?.IsDirty == true ? tuple.Item1?.EditedData! : tuple.Item1?.Data!)!;
            if (e.Key == "Tab" || e.Key == "ShiftTab" || e.Key == "ArrowRight" || e.Key == "ArrowLeft")
            {
                AriaLabel = cellValue + " " + _parent.Localizer!.GetText(GridLocaleKeys.ColumnHeaderARIA) + " " + tuple.Item2?.Column?.Field;
            }
            if (e.Key == "ArrowDown" || e.Key == "ArrowUp")
            {
                IsKeyPressedUpOrDown = true;
                AriaLabel = cellValue;
            }

            BeforeCellFocus bcf = new BeforeCellFocus()
            {
                KeyCombination = keyCombination,
                Action = action!,
                Cancel = false,
                Cell = cell!,
                Row = row!,
                KeyArgs = e,
                IsKeyEvent = e != null,
                IsHeader = isheader,
                Tuple = tuple
            };

            _parent.EventAggregator.Trigger("BeforeCellFocus", bcf);
             if (bcf.Cancel == true || (_parent.EditSettings != null && _parent.EditSettings.Mode.Equals(EditMode.Batch) && IsF2KeyPressed(e!) && !cell?.EditDisabled == true))
            {
                return;
            }
            if (_parent.IsRenderedFromTreeGrid)
            {
                tuple = bcf.Tuple;
            }
            if (tuple.Item2?.CellType != CellType.RowDrag)
            {
                bool isSameRow;
                if (keyCombination.Equals("AltW", StringComparison.Ordinal))
                {
                    ClearCurrent();
                    var firstRow = _parent.Rows?.Where(_ => _.Visible).First();
                    var firstCell = firstRow?.Cells?.Where(_ => _.Visible && _.CellType != CellType.RowDrag).First();
                    tuple = (firstRow, firstCell)!;
                    isSameRow = _current.Item1 == null ? false : _current.Item1.Equals(tuple.Item1?.Uid, StringComparison.Ordinal);
                    SetCurrent(tuple.Item1!, tuple.Item2!, true);
                }
                else
                {
                    isSameRow = _current.Item1 == null ? false : _current.Item1.Equals(tuple.Item1?.Uid, StringComparison.Ordinal);
                    ClearCurrent();
                    SetCurrent(tuple.Item1!, tuple.Item2!, true);
                }

                CellFocused cf = new CellFocused()
                {
                    KeyCombination = keyCombination,
                    Action = action!,
                    Cancel = false,
                    Cell = tuple.Item2!,
                    Row = tuple.Item1!,
                    KeyArgs = e!,
                    IsKeyEvent = e != null,
                    IsRowChanged = !isSameRow,
                    IsHeader = _isHeader,
                    IsJump = _isJump
                };
                if (!keyCombination.Equals("F2", StringComparison.Ordinal))
                {
                    _parent.EventAggregator.Trigger("CellFocused", cf);
                }
                _isJump = false;
                if (cf.Cancel == true)
                {
                    return;
                }

                if ((action != null && (action == "MoveUpCell") || (action == "MoveDownCell") || (action == "MoveRightCell") || (action == "MoveLeftCell")) || keyCombination == "CtrlHome" || keyCombination == "CtrlEnd" || keyCombination == "AltW")
                {
                    _parent.VirtualScrollModule!.CurrentRowIndex = _parent.InfiniteScrollModule!.CurrentRowIndex = tuple.Item1?.Index != null ? (int)tuple.Item1.Index : 0;
                }

                _parent.SoftRefresh = true;
                _parent.EventAggregator.Trigger("RowStateChanged", cf.Row);
                if (!cf.PreventDOMFocus || (cf.PreventDOMFocus && !(cf.Cell?.Column?.AllowEditing == true && cf.Cell?.Column?.AllowAdding == true)))
                {
                    bool isCtrlUpOrDown = keyCombination.Equals("CtrlUp", StringComparison.Ordinal) || keyCombination.Equals("CtrlDown", StringComparison.Ordinal);

                    if (_parent.AllowGrouping && tuple.Item1?.GroupKey != null && tuple.Item1?.Index == null && isCtrlUpOrDown)
                    {
                        bool isFocused = await _parent.InvokeMethod<bool>("sfBlazor.Grid.focusNextFrame", false, new object[] { _parent.DataId }).ConfigureAwait(true);
                    }
                    await Focus(tuple.Item1?.Uid!, tuple.Item2?.Uid!, action, keyCombination, cellColIndex: tuple.Item2?.Index + 1 ?? -1).ConfigureAwait(true);
                }
            }
        }

        #endregion

        #region Row and Cell Navigation

        private ValueTuple<Row<object>, Cell<object>> GetNextRowCell(Row<object> row, Cell<object> cell,
    KeyboardEventArgs e, ValueTuple<int, int> navigator, string? action = null)
        {
            var keyCombination = e.GetKeyCombination(isMacDevice: _parent!.IsMacDevice ?? false);
            bool isArrowKeys = keyCombination != null && (keyCombination.Equals("ArrowUp", StringComparison.Ordinal) || keyCombination.Equals("ArrowDown", StringComparison.Ordinal) || keyCombination.Equals("ArrowLeft", StringComparison.Ordinal) || keyCombination.Equals("ArrowRight", StringComparison.Ordinal));
            SetCurrentArea(row, cell, e, navigator, action);
            List<Row<object>>? _visiblerows = _currentRows?.Where(_ => _.Visible != false && !(_.CssClass != null && _.CssClass.Contains("e-hiddenrow", StringComparison.Ordinal))).ToList();
            List<Row<object>>? _visibleheaderrows = HeaderRows?.Where(_ => _.Visible != false).ToList();
            List<Row<object>>? _visibleFrozenheader = _frozenHeaderRows?.Where(_ => _.Visible != false).ToList();
            var _frozenHeaderCount = _visibleFrozenheader?.Count - 1;
            var currentfrozenIndex = 0;
            var _headerRowsCount = (_visibleheaderrows?.Count - 1) ?? 0;
            var currentHeaderIndex = 0;
            int newCIndex = 0;
            int newRIndex = 0;
            List<GridColumn> orderedColumns = _parent.FreezeModule!.GetFrozenCount() > 0 ? _parent.RearrangeColumns(_parent.Columns!) : _parent.Columns!;
            for (int i = 0; i <= _frozenHeaderCount; i++)
            {
                if (_visibleFrozenheader != null && _visibleFrozenheader[i]?.Cells?.Count == row?.Cells?.Count)
                {
                    currentfrozenIndex = i;
                }
            }

            for (int i = 0; i <= _headerRowsCount; i++)
            {
                if (_visibleheaderrows?[i]?.Cells?.Count == row?.Cells?.Count)
                {
                    currentHeaderIndex = i;
                }
            }

            //To focus the first group caption cell when up arrow key is pressed from the first caption cell while grouping all columns.
            bool allColumnsAreInvisible = _parent.Columns?.Count == _parent.Columns?.Where(e => e.Visible == false).Count();
            if (allColumnsAreInvisible && action == "MoveUpCell" && _parent.Rows?.Count > 0 && _parent.Rows.First().Equals(row))
            {
                return (row, cell);
            }

            // To focus the first content cell when tab key is pressed from the last header cell.
            bool columnVirtualLastCellFocus = false;
            if (_parent.EnableColumnVirtualization && orderedColumns?.Count > 0)
            {
                int selectedCellNavigation = _parent.VirtualScrollModule!.SelectedCellNavigation;
                if (row?.Cells?.Where(e => e.Visible).Last()?.IsFocused == true && !string.IsNullOrEmpty(cell?.Column!.Field))
                {
                    columnVirtualLastCellFocus = cell?.Column!.Field == orderedColumns.LastOrDefault()?.Field;
                }
                if (selectedCellNavigation > -1 && selectedCellNavigation == _parent.Columns?.Count - 1)
                {
                    columnVirtualLastCellFocus = orderedColumns[selectedCellNavigation] == orderedColumns.LastOrDefault();
                }
            }
            bool normalGridLastCellFocus = !_parent.EnableColumnVirtualization && row?.Cells?.Where(e => e.Visible).Last()?.IsFocused == true;
            if (row?.Index == null && _parent.Rows?.Count > 0 && !row?.IsCaptionRow == true && (normalGridLastCellFocus || columnVirtualLastCellFocus) && action?.Equals("MoveRightCell", StringComparison.Ordinal) == true && _visibleheaderrows?[_headerRowsCount]?.Cells?.Where(e => e.Visible).Last().IsFocused == true)
            {
                row = _parent.Rows?.First()!;
                cell = row?.Cells?.Where(_ => _.Visible && _.CellType != CellType.RowDrag).First()!;
                return (row!, cell);
            }

            // To focus the last header cell when shift tab is pressed from the first content cell.
            if (_parent.Rows?.Count > 0 && !(_parent.AllowFiltering && _parent.FilterSettings != null && _parent.FilterSettings.Type.Equals(FilterType.FilterBar)) && _parent.Rows?.First().Equals(row) == true && row?.Cells?.Where(e => e.Visible && e.CellType != CellType.RowDrag).First().Equals(cell) == true && action?.Equals("MoveLeftCell", StringComparison.Ordinal) == true && !e.Key.Equals("ArrowLeft", StringComparison.Ordinal) && e.GetKeyCombination() == "ShiftTab")
            {
                row = HeaderRows?.Last()?.Cells != null ? HeaderRows.Last() : row;
                cell = HeaderRows?.Last()?.Cells?.Where(e => e.Visible).Last()!;
                return (row, cell);
            }

            // Handling the row and cell index when shift tab is pressed, from the first cell of the caption row.
            if (_visiblerows != null && row?.Index == null && row?.IsCaptionRow == true && e.IsShiftTab() && row?.Cells?.Where(e => e.Visible).FirstOrDefault()?.Equals(cell) == true && !allColumnsAreInvisible)
            {
                var currentRowIndex = _visiblerows.IndexOf(row);
                if (!_visiblerows.FirstOrDefault()?.Equals(_visiblerows?[currentRowIndex]) == true)
                {
                    var newRow = _visiblerows?[currentRowIndex - 1];
                    var newCell = newRow?.Cells?.Where(e => e.Visible).LastOrDefault();
                    return (newRow!, newCell!);
                }
            }

            if (_visibleFrozenheader?.Count != 0 && (_visibleheaderrows != null && _visibleheaderrows[0]?.Cells?.Where(e => e.Visible).First().IsFocused == true) && action?.Equals("MoveLeftCell", StringComparison.Ordinal) == true)
            {
                row = _visibleFrozenheader?.Last()?.Cells != null ? _visibleFrozenheader!.Last() : row!;
                cell = _visibleFrozenheader?.Last()?.Cells?.Where(e => e.Visible).Last()!;
                return (row, cell);
            }

            if (currentHeaderIndex > 0 && _visibleheaderrows?[currentHeaderIndex]?.Equals(row) == true && row?.Cells?.Where(e => e.Visible).First().IsFocused == true && action?.Equals("MoveLeftCell", StringComparison.Ordinal) == true)
            {
                row = _visibleheaderrows?[currentHeaderIndex - 1]!;
                cell = _visibleheaderrows?[currentHeaderIndex - 1]?.Cells?.Where(e => e.Visible).Last()!;
                return (row, cell);
            }

            if (_visibleFrozenheader?.Count != 0 && _visibleFrozenheader?[currentfrozenIndex]?.Equals(row) == true && !row?.Cells?.Where(e => e.Visible).Last().IsFocused == true && action?.Equals("MoveRightCell", StringComparison.Ordinal) == true && _visibleFrozenheader[0]?.Cells?.Count != 1)
            {
                newCIndex = (row?.Cells?.IndexOf(cell!) ?? 0) + navigator.Item2;
                newRIndex = _visibleFrozenheader.IndexOf(row!) + navigator.Item1;
            }
            else if (_visibleFrozenheader?.Count != 0 && _visibleFrozenheader?[currentfrozenIndex]?.Equals(row) == true && (row?.Cells?.Where(e => e.Visible).Last().IsFocused == true || _visibleFrozenheader[0]?.Cells?.Count == 1) && action?.Equals("MoveRightCell", StringComparison.Ordinal) == true)
            {
                row = _visibleheaderrows?.FirstOrDefault()!;
                cell = _visibleheaderrows?[0]?.Cells?.Where(_ => _.Visible).First()!;
                return (row, cell);
            }
            else
            {
                if (_parent.EnableColumnVirtualization && row?.Index == null && _parent.Rows?.Count > 0 && _parent.Columns?.Count > 0 && cell != null && !isArrowKeys)
                {
                    List<GridColumn>? visibleColumns = orderedColumns?.Where(x => x.Visible).ToList();
                    newCIndex = visibleColumns!.IndexOf(cell.Column!) + navigator.Item2;
                }
                else
                {
                    newCIndex = (row?.Cells?.IndexOf(cell!) ?? 0) + navigator.Item2;
                }

                newRIndex = (_visiblerows?.IndexOf(row!) ?? 0) + navigator.Item1;
            }

            if (_visiblerows?.Count <= newRIndex)
            {
                if (_parent.AllowPaging)
                {
                    if(_parent.AllowGrouping && row?.IsCaptionRow == true && !row?.IsExpand == true || (_parent.IsRenderedFromTreeGrid && row != null && row.IsLastRow))
                    {
                        newRIndex = _visiblerows.Count - 1;
                    }
                    else
                    {
                        return (null!, null!);
                    }
                }
                else
                {
                    newRIndex = _visiblerows.Count - 1;
                }
            }

            if (newRIndex < 0)
            {
                if (action == "MoveUpCell" && _visiblerows?.Count - 1 > 0)
                {
                    newRIndex = _visiblerows?.Count - 1 ?? 0;
                }
                else
                {
                    newRIndex = 0;
                }
            }

            var nRow = _visiblerows?[newRIndex];
            var isRowDataCellVisible = nRow!.Cells.Where(x => x.IsDataCell).Select(e => e.Visible).LastOrDefault();
            if ((action == "MoveDownCell" || action == "MoveLeftCell" || action == "MoveUpCell") && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0 && !isRowDataCellVisible && !nRow.IsCaptionRow && !nRow.IsExpand && nRow.RowType == "Data" && allColumnsAreInvisible)
            {
                nRow = (action == "MoveUpCell" || _visiblerows?.Count - 1 <= newRIndex) ? _visiblerows?[newRIndex - 1] : _visiblerows?[newRIndex + 1];
            }
            if (_visibleFrozenheader?.Count != 0 && (_visibleFrozenheader != null && _visibleFrozenheader[currentfrozenIndex]?.Equals(row) == true) && row?.Cells?.Where(e => e.Visible).Last().IsFrozen == true && _visibleFrozenheader[0]?.Cells?[0]?.IsFocused != true && _visibleFrozenheader[0]?.Cells?.Count != 1)
            {
                nRow = _visibleFrozenheader[newRIndex];
            }
            int cellCount = (int)(_parent.EnableColumnVirtualization && nRow?.Index == null ? _parent.Columns?.Count : nRow?.Cells?.Count)!;

            if (newCIndex < 0)
            {
                newCIndex = 0;
            }

            if (cellCount <= newCIndex)
            {
                newCIndex = cellCount - 1 < 0 ? 0 : cellCount - 1;
            }

            if (_parent.EnableColumnVirtualization &&  nRow?.Index == null)
            {
                List<GridColumn>? visibleColumns = orderedColumns?.Where(x => x.Visible).ToList();
                newCIndex = !isArrowKeys ? nRow!.Cells.FindIndex(x => x.Column != null && !string.IsNullOrEmpty(x.Column.Field) && x.Column.Field == visibleColumns?[newCIndex].Field) : newCIndex;
                if(newCIndex < 0 && (action?.Equals("MoveLeftCell", StringComparison.Ordinal) == true 
                    || action?.Equals("MoveRightCell", StringComparison.Ordinal) == true))
                {
                    return (nRow!, nRow?.Cells?.LastOrDefault()!);
                }
            }
            var nCell = nRow?.Cells?[newCIndex];
            if (nCell?.Index == null && nCell?.CellType == CellType.RowDrag)
            {
                nCell = nRow?.Cells?.Where(_ => _.Visible && _.CellType != CellType.RowDrag && _.CellType != CellType.Indent).FirstOrDefault();
            }
            var oRow = nRow;
            var oCell = nCell;

            // If cell is invisible or indent cell, then find next visible cell.
            (nRow, nCell) = CheckInVisibleOrIndentCell(nCell!, oRow!, oCell!, nRow!, e, navigator, action);

            //If grid has single column Shift + Tab from last cell not working scenario
            var lastVisibleDataCell = _visiblerows != null && _visiblerows.Count > 0 ? _visiblerows[_visiblerows.Count - 1]?.Cells?.Where(x => x.Visible && x.IsDataCell).ToList() : null;
            bool singleColumnGrid = lastVisibleDataCell != null && lastVisibleDataCell.Count > 0 && nRow?.IsLastRow == true && _visiblerows?[_visiblerows.Count - 1]?.Cells?.Where(x => x.Visible)?.LastOrDefault()?.Equals(cell) == true && lastVisibleDataCell.FirstOrDefault()!.Equals(cell);

            // same cell is focused then go to next row
            // also swap the action as we need to search cell in opposite direction.
            if (!(action == "MoveDownCell" && _currentRows?.LastOrDefault()?.Equals(nRow) == true) && nRow?.Equals(row) == true && nCell?.Equals(cell) == true && (e.IsTab() || e.IsShiftTab())
                && !(_visiblerows?.Count > 0 && _visiblerows.Last().Equals(row) && _visiblerows[_visiblerows.Count - 1]?.Cells?.Where(x => x.Visible).Last().Equals(cell) == true) || (singleColumnGrid && e.IsShiftTab() && !(nRow?.Index == 0 && nCell?.Index == 0)))
            {
                (nRow, nCell) = GetNextRowCell(nRow!, nCell!, e,
                    navigator: (e.IsShiftTab() ? -1 : 1, e.IsShiftTab() ? cellCount : -cellCount),
                    action: e.IsShiftTab() ? "MoveRightCell" : "MoveLeftCell");
            }

            return (nRow!, nCell!);
        }

        #endregion

        #region Visible Cell and Spanned Cell Handling

        private ValueTuple<Row<object>, Cell<object>> CheckInVisibleOrIndentCell(Cell<object> nCell, Row<object> oRow, Cell<object> oCell,
     Row<object> nRow, KeyboardEventArgs e, ValueTuple<int, int> navigator, string? action = null)
        {
            if (nCell.Visible == false || nCell.IsSpanned == true || nCell.IsRowSpanned == true || (nCell.CellType.Equals(CellType.Indent) || nCell.CellType.Equals(CellType.DetailIndent)))
            {
                if (action == "MoveLeftCell" || action == "MoveRightCell")
                {
                    if (action == "MoveLeftCell" && (nCell.CellType.Equals(CellType.Indent) || nCell.CellType.Equals(CellType.DetailIndent)))
                    {
                        (nRow, nCell) = GetNextRowCell(oRow, oCell, e, (0, 1), action);
                    }
                    else
                    {
                        if (action == "MoveLeftCell" && nCell.Index.HasValue && nCell.Index.Value == 0)
                        {
                            (nRow, nCell) = GetNextRowCell(oRow, oCell, e, (0, 1), action);
                        }
                        else if (action == "MoveRightCell" && nCell.Index.HasValue && nRow.Cells.Last().Equals(nCell))
                        {
                            (nRow, nCell) = GetNextRowCell(oRow, oCell, e, (0, -1), action);
                        }
                        else
                        {
                            (nRow, nCell) = GetNextRowCell(oRow, oCell, e, navigator, action);
                        }
                    }
                }

                if (action == "MoveUpCell" || action == "MoveDownCell")
                {
                    (nRow, nCell) = GetNextRowCell(oRow, oCell, e, (0, 1), action);
                    if (nCell.Visible == false)
                    {
                        (nRow, nCell) = GetNextRowCell(oRow, oCell, e, (0, -1), action);
                    }
                }
            }
            return (nRow, nCell);
        }

        #endregion

        #region Focus Area Management

        private void SetCurrentArea(Row<object> row, Cell<object> cell,
   KeyboardEventArgs e, ValueTuple<int, int> navigator, string? action = null)
        {
            var eventArgs = e;
            var Navigator = navigator;
            var cellObject = cell;
            if (_isHeader)
            {
                if (action == "MoveDownCell")
                {
                    bool isLast = _currentRows?.LastOrDefault()?.Equals(row) == true;
                    if (isLast && _parent.Rows?.Count != 0)
                    {
                        _isHeader = false;
                        _isJump = true;
                    }
                }
            }
            else
            {
                if (action == "MoveUpCell")
                {
                    bool isFirst = _currentRows?.FirstOrDefault()?.Equals(row) == true;
                    if (isFirst)
                    {
                        _isHeader = true;
                        _isJump = true;
                    }
                }
            }
        }

        #endregion

        #region Child Control Focus Handling

        private async Task<bool> FocusChild(Row<object> row, Cell<object> cell, KeyboardEventArgs e)
        {
            string keyComb = e.GetKeyCombination();
            bool hasChild = false;
            if (!IsChildFocused && (keyComb == "Enter")
                && (cell.IsTemplate || cell.CellType.Equals(CellType.CommandColumn)) && !cell.IsEdit)
            {
                hasChild = await FocusFirstChild(row.Uid!, cell.Uid, cell.Index + 1 ?? -1).ConfigureAwait(true);
            }

            if (IsChildFocused && keyComb == "Escape")
            {
                hasChild = false;
            }

            return IsChildFocused = hasChild;
        }

        #endregion

        #region Row Selection Focus Management

        /// <summary>
        /// Handles focus management when a row is selected.
        /// Determines the cell to focus based on grouping settings and updates focus state.
        /// </summary>
        internal async Task HandleRowSelectionFocus(Row<object> rowToSelect, bool isScrollIntoView, bool isSelectionMethodInvoked, bool isCellClicked, bool isAdd, bool isCancelAction)
        {
            if (rowToSelect == null || SelectedCellIndex == null || rowToSelect.Cells == null)
                return;

            Cell<object>? cell = rowToSelect.Cells.FirstOrDefault(c => c.Index == (int)SelectedCellIndex);
            bool isGrouped = _parent.AllowGrouping && _parent.GroupSettings?.Columns != null && _parent.GroupSettings.Columns?.Length > 0 && !_parent.GroupSettings.ShowGroupedColumn && !cell!.Visible;
            var cellToSelect = isGrouped ? rowToSelect.Cells.FirstOrDefault(c => c.Visible && c.Index > (int)SelectedCellIndex) : null;
            string? cellUid = isGrouped ? cellToSelect?.Uid! : cell?.Uid;
            
            if (!(isCellClicked && isAdd))
            {
                ClearCurrent();
                var cellToFocus = rowToSelect.Cells?.FirstOrDefault(_ => _.Index == (int)SelectedCellIndex)!;
                SetCurrent(rowToSelect, cellToFocus, outline: true);
            }
            
            int colIndex = isGrouped ? (cellToSelect?.Index + 1) ?? -1 : (cell?.Index + 1) ?? -1;
            string? focusMode = isAdd ? "UpdateRecord" : isScrollIntoView ? "ScrollSelect" : null;
            
            if (!isCellClicked && !isCancelAction)
            {
                await Focus(rowToSelect.Uid!, cellUid!, focusMode, cellColIndex: colIndex, isSelectionMethodInvoked: isSelectionMethodInvoked).ConfigureAwait(true);
            }
        }

        #endregion


        #region Header Focus Management

        internal async Task ToIndexHasValue(int? tIndex, int frIndx, BeforeCellFocus evt)
        {
            if (tIndex.HasValue)
            {
                evt.Cancel = true;
                var fromColumns = new List<GridColumn> { _parent!.Columns?[frIndx]! } ;
                var toColumn = _parent.Columns?[(int)tIndex];
                var ar = new ActionEventArgs<T>()
                {
                    RequestType = Grids.Action.Reorder,
                    Cancel = false,
                    Parent = _parent,
                    FromColumns = fromColumns,
                    ToColumn = toColumn!
                };
                var reorderEventArgs = new ColumnReorderingEventArgs() {ReorderingColumns = fromColumns , ToColumn = toColumn!, Cancel = false, Parent = _parent };
                await _parent.ModelChanged(ar, new ActionArgs() { FromIndex = frIndx, ToIndex = (int)tIndex }, eventArgs: reorderEventArgs, requestType:"Reorder").ConfigureAwait(true);
                await SfBaseUtils.InvokeEvent<ActionEventArgs<T>>(_parent.GridEvents?.OnActionComplete, ar).ConfigureAwait(true);
                await _parent.EventAggregator.NotifyAsync("ActionComplete", ar).ConfigureAwait(true);
                await SfBaseUtils.InvokeEvent<ColumnReorderedEventArgs>(_parent.GridEvents?.ColumnReordered, reorderEventArgs).ConfigureAwait(true);
                await _parent.EventAggregator.NotifyAsync("ColumnReordered", reorderEventArgs).ConfigureAwait(true);
                await _parent.InvokeSuccessAsync(ar, requestType: "Reorder").ConfigureAwait(true);
            }
        }

        internal async Task ShiftEnterHandler(GridColumn _col, BeforeCellFocus evt)
        {
            if (_parent!.AllowSorting)
            {
                evt.Cancel = true;
                string sortIcon = _parent.SortModule?.GetSortIconClass(_col.Field!) ?? string.Empty;
                await (_parent?.SortModule?.SortClickHandler(
                    _col,
                    sortIcon,
                    new MouseEventArgs() { CtrlKey = evt.KeyArgs!.CtrlKey, ShiftKey = evt.KeyArgs.ShiftKey, MetaKey = evt.KeyArgs.MetaKey }
                ))!.ConfigureAwait(true)!;
            }
        }

        internal async Task CtrlSpaceHandler(BeforeCellFocus evt, GridColumn _col)
        {
            if (_parent != null && _parent.AllowGrouping)
            {
                evt.Cancel = true;
                string[] _groups = _parent.GroupSettings?.Columns ?? Array.Empty<string>();
                if (!string.IsNullOrEmpty(_col?.Field) && !_col.FixedColumn)
                {
                    if (_groups.Contains(_col.Field))
                    {
                        await _parent.UngroupColumnAsync(_col.Field).ConfigureAwait(true);
                    }
                    else
                    {
                        await _parent.GroupColumnAsync(_col.Field).ConfigureAwait(true);
                    }
                }
            }
        }

        #endregion
    }

    #region Event Argument Classes

    /// <summary>
    /// BeforeCell focus event argument class.
    /// </summary>
    /// <exclude/>
    internal class BeforeCellFocus
    {
        public bool Cancel { get; set; }

        public KeyboardEventArgs? KeyArgs { get; set; }

        public string? KeyCombination { get; set; }

        public string? Action { get; set; }

        public Row<object>? Row { get; set; }

        public Cell<object>? Cell { get; set; }

        public bool IsKeyEvent { get; set; }

        public bool IsHeader { get; set; }

        public ValueTuple<Row<object>, Cell<object>> Tuple { get; set; }
    }

    /// <summary>
    /// Cell focus event argument class.
    /// </summary>
    /// <exclude/>
    public class CellFocused
    {

        /// <summary>
        /// Gets or sets a value indicating whether the focus action should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the keyboard event arguments associated with the focus action.
        /// </summary>
        public KeyboardEventArgs? KeyArgs { get; set; }

        /// <summary>
        /// Gets or sets the key combination used during the focus action.
        /// </summary>
        public string? KeyCombination { get; set; }

        /// <summary>
        /// Gets or sets the action performed during the focus event.
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the row that contains the focused cell.
        /// </summary>
        public Row<object>? Row { get; set; }

        /// <summary>
        /// Gets or sets the cell that is currently focused.
        /// </summary>
        public Cell<object>? Cell { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the row has changed during the focus event.
        /// </summary>
        public bool IsRowChanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the focus event was triggered by a keyboard action.
        /// </summary>
        public bool IsKeyEvent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the focused cell belongs to the header row.
        /// </summary>
        public bool IsHeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the focus involves jumping to another cell.
        /// </summary>
        public bool IsJump { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DOM focus should be prevented for the cell.
        /// </summary>
        public bool PreventDOMFocus { get; set; }
    }

    #endregion
}
