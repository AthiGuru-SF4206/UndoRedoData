using Microsoft.AspNetCore.Components.Forms;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Internal;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections;
using System.Globalization;
using Syncfusion.Blazor.Popups;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Syncfusion.Blazor.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001002382fcb1069523ce72d849497a557a445c151eaf4007aa79adef551a8204ca7f728e5378607d85695b16f129ec35bf4af15dcf6d3581deb8bb0debb239c33e7f1271a37c7f60f1044ae417730f5082abee5f9ec568a8a4cef04074394755706376e982dc6f9d15430faaad385ae8f00a77ef1c97517f1a1517004ce78028b9ce")]
namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles CRUD operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Edit<T>
    {
        #region Fields

        public IDictionary<string, object> position = new Dictionary<string, object>();

        public string ArrowPosition = string.Empty;

        public bool ValueChanged;

        public bool HasBatchChanges;

        internal bool EditNextCell;

        internal bool ForceValidate;

        internal bool ShouldPreventEditFormRender { get; set; }

        internal bool IsBatchReorderPending { get; set; }

        /// <summary>
        /// It prevents the clear selection when pressing the "Esc" key in the editable state (batch editing) while enabling PersistSelection.
        /// </summary>
        internal bool ClearSelection;

        internal int? EditRowIndex;

        internal string? KeyCode { get; set; }

        internal bool IsShiftKey { get; set; }

        internal Dictionary<string, EditContext> ComplexEditContext = new Dictionary<string, EditContext>() { };

        internal string AlertMessage { get; set; } = string.Empty;

        internal object? CloneData { get; set; }

        internal bool IsEmptyRowRendered { get; set; }

        #endregion

        #region Private Properties

        private SfGrid<T> Parent { get; set; }

        #endregion

        #region Internal Properties

        internal bool IsAdd { get; set; }

        internal ActionEventArgs<T>? BatchActionArgs { get; set; }

        internal bool KeyPressed { get; set; }

        internal Row<object>? OriginalRow { get; set; }

        internal Cell<object>? OriginalCell { get; set; }

        internal ActionArgs? BatchAdditionalArgs { get; set; }

        internal bool IsLastRow { get; set; }

        internal bool IsCancelAction { get; set; }

        internal Row<object>? EditedRow;

        internal EditContext? EditContext;

        internal List<ValidationResult> ErrorResult { get; set; } = new List<ValidationResult>();

        internal SfDialog? EditDialogInstance { get; set; }

        internal Row<object>? LastVisibleRow { get; set; }

        internal T? RowData => (T)CloneData!;

        #endregion

        #region Constructors

        public Edit(SfGrid<T> parent)
        {
            Parent = parent;
            parent.EventAggregator.Add("BeforeCellFocus", CurrentFocused);
            Parent.EventAggregator.Add("CellFocused", NextCellFocus);
        }

        #endregion

        #region Cell CRUD Operations
        internal async Task AddRecord(object data = null!, Nullable<double> index = null)
        {
            Nullable<double> rowIndex = index;
            if (Parent.EditSettings != null && Parent.EditSettings.Mode.Equals(EditMode.Batch))
            {
                await BulkAddRow(data).ConfigureAwait(true);
            }
            else
            {
                if (Parent.IsEdit || (Parent.EditSettings != null && Parent.EditSettings.ShowAddNewRow && data == null))
                {
                    return;
                }

                if (Parent.EnableVirtualization || Parent.EnableInfiniteScrolling)
                {
                    Parent.VirtualScrollModule!.CurrentRowIndex = Parent.InfiniteScrollModule!.CurrentRowIndex = 0;
                }

                if (data != null)
                {
                    if (Parent.InfiniteScrollModule != null && Parent.EnableInfiniteScrolling)
                    {
                        Parent.InfiniteScrollModule.IsInfiniteInitialRender = true;
                    }
                    Parent.IsAdd = Parent.EnableVirtualization;
                    await Parent.ModelChanged(new ActionEventArgs<T>()
                    {
                        RequestType = Action.Save,
                        Type = "ActionBegin",
                        Data = (T)data,
                        SelectedRow = 0,
                        Action = "Add",
                        Index = (int?)index ?? 0,
                        Parent = Parent
                    },
                    eventArgs: new RowUpdatingEventArgs<T>()
                    {
                        Data = (T)data,
                        Index = (int?)index ?? 0,
                        Parent = Parent,
                        Action = SaveActionType.Added
                    }, requestType: "Save").ConfigureAwait(true);
                    Parent.IsAdd = false;
                    return;
                }
                var ind = 0;
                var Row = GetModelGenerator(true);
                Row.Action = EditAction.Added;
                EditedRow = Row;
                CloneData = (T)Row.Data!;
                //Used to split the treegrid field name to get string "dataitem"
                var split = GridUtils.GetColumns(Parent)[0].Field.Split('.');
                if (split.Length > 1)
                {
                    var propInfo = CloneData?.GetType().GetProperty(split[0]);
                    var isDynamic = typeof(IDynamicMetaObjectProvider).IsAssignableFrom(propInfo?.PropertyType);
                    if (isDynamic)
                    {
                        object tempObj = ReflectionExtension.TryCreateInstance(propInfo?.PropertyType);
                        propInfo?.SetValue(CloneData, tempObj);
                    }
                }
                SetDefaultValue();
                EditContext = CloneData != null ? new EditContext(CloneData) : null!;
                var args = new ActionEventArgs<T>()
                {
                    Cancel = false,
                    RequestType = Action.Add,
                    Data = (T)CloneData!,
                    Index = ind,
                    RowData = (T)CloneData!,
                    Type = "ActionBegin",
                    EditContext = EditContext,
                    Parent = Parent
                };
                var eventArgs = new RowCreatingEventArgs<T>()
                {
                    Cancel = false,
                    Data = (T)CloneData!,
                    Index = ind,
                    EditContext = EditContext,
                    Parent = Parent
                };
                await SfBaseUtils.InvokeEvent<ActionEventArgs<T>>(Parent.GridEvents?.OnActionBegin, args).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("ActionBegin", args).ConfigureAwait(true);
                await SfBaseUtils.InvokeEvent<RowCreatingEventArgs<T>>(Parent.GridEvents?.RowCreating, eventArgs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("RowCreating", eventArgs).ConfigureAwait(true);
                if (Parent.EnableVirtualization)
                {
                    if (Parent.GridEvents?.OnActionBegin.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
                    {
                        index = args.Index == 0 ? index : (int)args.Index;
                    }
                    else
                    {
                        rowIndex = eventArgs.Index == 0 ? rowIndex : (int)eventArgs.Index;
                    }

                }
                if (args.Cancel || eventArgs.Cancel)
                {
                    return;
                }

                var cData = CloneData;
                EnsureDataAndEditContext(ref cData!, args, eventArgs: eventArgs);
                CloneData = cData;
                if (Parent.EditSettings != null && Parent.EditSettings.Mode != EditMode.Dialog)
                {
                    if (!IsPersistSelection())
                    {
                        await Parent.ClearSelectionAsync().ConfigureAwait(true);
                    }
                    if (Parent.EditSettings.NewRowPosition == NewRowPosition.Top)
                    {
                        if (Parent.EnableVirtualization && index == null && rowIndex == null)
                        {
                            await Parent.InvokeMethod("sfBlazor.Grid.scrollIntoView", new object[] { Parent.DataId, -1, 0, -1, false, true }).ConfigureAwait(true);
                        }
                        else
                        {
                            if (Parent.EnableInfiniteScrolling && Parent.PageSettings != null && Parent.PageSettings.CurrentPage > 1 && (Parent.InfiniteScrollSettings!.EnableCache || (Parent.AllowGrouping && Parent.GroupSettings?.Columns?.Length > 0)))
                            {
                                Parent.InfiniteScrollModule!.RequestType = args.RequestType.ToString();
                                Parent.InfiniteScrollModule.IsInfiniteInitialRender = true;
                                await Parent.InfiniteScrollModule.ResetInfiniteProperties(Parent.InfiniteScrollModule.RequestType).ConfigureAwait(true);
                                await Parent.DataProcess().ConfigureAwait(true);
                            }
                            if (Parent.GridEvents?.OnActionBegin.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
                            {
                                if (Parent.IsRenderedFromTreeGrid && (Parent as IGrid).GridTemplates?.DetailTemplate != null && Parent.Rows.Where(row => row.IsAddedTop).ToList().Count > 0)
                                {
                                    Parent.Rows.Insert((int)args.Index * 2, Row);
                                    Row.IsAddedTop = true;
                                    ind = (int)args.Index * 2;
                                }
                                else
                                {
                                    Parent.Rows.Insert((int)args.Index, Row);
                                    Row.IsAddedTop = true;
                                    ind = (int)args.Index;
                                }
                            }
                            else
                            {
                                Parent.Rows.Insert((int)eventArgs.Index, Row);
                                Row.IsAddedTop = true;
                                ind = (int)eventArgs.Index;
                            }
                        }
                    }
                    else
                    {
                        if (Parent.EnableVirtualization && (index == null || rowIndex == null))
                        {
                            if (Parent.AllowGrouping && Parent.GroupSettings?.Columns?.Length > 0)
                            {
                                await Parent.InvokeMethod("sfBlazor.Grid.scrollIntoView", new object[] { Parent.DataId, -1, Parent.VirtualScrollModule!.VisibleGroupRows.Count, -1, true }).ConfigureAwait(true);
                            }
                            else
                            {
                                await Parent.InvokeMethod("sfBlazor.Grid.scrollIntoView", new object[] { Parent.DataId, -1, Parent.TotalItemCount, -1 }).ConfigureAwait(true);
                            }
                        }
                        else
                        {
                            Row.IsAddedBottom = true;
                            ind = Parent.Rows.Count;
                            Parent.Rows.Add(Row);
                            EditedRow.Index = Parent.AllowPaging ? (Parent.PageSettings?.PageSize > ind ? ind : ind - 1) : ind;
                        }
                    }
                    if (!Parent.EnableVirtualization)
                    {
                        Parent.Rows[ind].IsEdit = true;
                    }
                }
                Parent.IsEdit = true;
                Parent.IsAdd = true;
                if (Parent.FocusModule != null)
                {
                    Parent.FocusModule.IsChildFocused = false;
                }
                Parent.PreventRender();
                IsAdd = true;
                if (Parent.EditSettings != null && Parent.EditSettings.Mode.Equals(EditMode.Dialog))
                {
                    Parent.EventAggregator.Trigger("ShowDialog", null!);
                }
                else
                {
                    Parent.ForceUpdate = false;
                    if (Parent.FrozenRows > 0)
                    {
                        Parent.EventAggregator.Trigger("HeaderStateChanged", null!);
                    }
                    else if ((Parent.EnableVirtualization || Parent.EnableColumnVirtualization) && Parent.VirtualScrollModule != null)
                    {
                        Parent.VirtualScrollModule.HasAddOrCancelAction = Parent.VirtualScrollModule.IsBottomAddForm(Parent.VirtualScrollModule.RowEndIndex);
                        Parent.EventAggregator.Trigger("VirtualComponentUpdate", null!);
                    }
                    else
                    {
                        Parent.FocusModule!.ClearCurrent();
                        Parent.EventAggregator.Trigger("ContentStateChanged", null!);
                    }
                }
                if (Parent.SelectionModule != null)
                {
                    Parent.SelectionModule.UpdateCheckBoxStateOnAdd(IsAdd, EditedRow.Action);
                }
                Parent.EventAggregator.Trigger("HeaderStateChanged", null!);
                Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
                args.Type = "actionComplete";
                await SfBaseUtils.InvokeEvent<ActionEventArgs<T>>(Parent.GridEvents?.OnActionComplete, args).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("ActionComplete", args).ConfigureAwait(true);
                var rowAddedArgs = new RowCreatedEventArgs<T>()
                {
                    Data = eventArgs.Data,
                    Index = eventArgs.Index,
                    EditContext = eventArgs.EditContext,
                };
                await SfBaseUtils.InvokeEvent<RowCreatedEventArgs<T>>(Parent.GridEvents?.RowCreated, rowAddedArgs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("RowCreated", rowAddedArgs).ConfigureAwait(true);
            }
            if (Parent.FreezeModule!.GetFrozenCount() > 0)
            {
                await Task.Delay(1).ConfigureAwait(true);
                await Parent.InvokeMethod("sfBlazor.Grid.frozenHeight", new object[] { Parent.DataId, Parent.GetClientOption(), null! }).ConfigureAwait(true);
            }
            if (Parent.EnableInfiniteScrolling || (Parent.EnableVirtualization && Parent.EditSettings != null && Parent.EditSettings.NewRowPosition == NewRowPosition.Bottom && Parent.FreezeModule!.GetFrozenCount() == 0))
            {
                await Parent.InvokeMethod("sfBlazor.Grid.updateOptions", new object[] { Parent.DataId, Parent.GetClientOption() }).ConfigureAwait(true);
            }
        }

        internal async Task EditCell(Row<object> Row, Cell<object> Cell, bool focusFirstCellOnShiftTab = false)
        {
            if (Parent.IsEdit)
            {
                await SaveCell(focusFirstCellOnShiftTab: focusFirstCellOnShiftTab).ConfigureAwait(true);
            }

            if (Parent.IsEdit)
            {
                return;
            }
            IsAdd = Row.Action != EditAction.Added ? false : IsAdd;
            var Data = Row.EditedData ?? Row.Data;
            CloneRowData(Data!);
            OriginalRow = EditedRow = Row;
            OriginalCell = Cell;
            IsLastRow = Parent.Rows?.OrderByDescending(x => x.Index).FirstOrDefault() == Row;
            EditContext = CloneData != null ? new EditContext(CloneData) : null!;
            var Keys = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            if ((((Keys.Count != 0 && Keys[0].Equals(Cell.Column?.Field, StringComparison.Ordinal) && !Cell.IsDirty) || (!Cell.Column!.AllowEditing && !Cell?.IsDirty == true && Row?.Action != EditAction.Added)) && !IsAdd) || (!Cell!.Column.AllowAdding && Row?.Action == EditAction.Added))
            {
                Cell!.EditDisabled = true;
                return;
            }

            var args = new CellEditArgs<T>()
            {
                ColumnName = Cell.Column.Field,
                IsForeignKey = Cell.IsForeignKey,
                Data = (T)CloneData! ?? (T)Row?.EditedData!,
                PrimaryKey = Keys?.ToArray()!,
                RowData = (T)Row!.Data!,
                ValidationRules = Cell.Column.ValidationRules,
                Cancel = false,
                ForeignKeyData = Row.ForeignKeyData,
                EditContext = EditContext,
                Column = Cell.Column,
                Parent = Parent
            };
            await SfBaseUtils.InvokeEvent<CellEditArgs<T>>(Parent.GridEvents?.OnCellEdit, args).ConfigureAwait(true);
            await Parent.EventAggregator.NotifyAsync("CellEdit", args).ConfigureAwait(true);
            if (args.Cancel)
            {
                return;
            }

            var cData = CloneData;
            EnsureDataAndEditContext(ref cData!, cellEditArgs: args);
            CloneData = cData;
            if (!IsPersistSelection())
            {
                await Parent.ClearSelectionAsync().ConfigureAwait(true);
            }
            Row.IsDirty = true;
            Cell.IsEdit = true;
            Parent.IsEdit = true;
            Parent.PreventRender();
            Parent.SoftRefresh = true;
            if (Parent.AllowSelection && !Parent.SelectionSettings!.PersistSelection && !IsCheckBoxOnly())
            {
                await Parent.SelectRowAsync((int)Row.Index!).ConfigureAwait(true);
            }
            else
            {
                Parent.EventAggregator.Trigger("RowStateChanged", Row);
            }
        }

        internal async Task UpdateRow(double index, object data)
        {
            CloneRowData(data);
            var PrimaryKeys = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            var args = new ActionEventArgs<T>()
            {
                RequestType = Action.Save,
                PrimaryKeys = PrimaryKeys?.ToArray()!,
                Type = "ActionBegin",
                Data = (T)data,
                RowData = (await Parent.GetCurrentViewRecordsAsync().ConfigureAwait(true))[(int)index],
                Cancel = false,
                PreviousData = (await Parent.GetCurrentViewRecordsAsync().ConfigureAwait(true))[(int)index],
                Action = "Edit"
            };
            var saveEventArgs = new RowUpdatingEventArgs<T>()
            {
                PrimaryKeys = PrimaryKeys?.ToArray()!,
                Data = (T)data,
                Cancel = false,
                PreviousData = (await Parent.GetCurrentViewRecordsAsync().ConfigureAwait(true))[(int)index],
                Action = SaveActionType.Edited
            };
            if (Parent.GridEvents?.RowUpdating.HasDelegate == true)
            {
                await (Parent.GridEvents?.RowUpdating.InvokeAsync(saveEventArgs))!.ConfigureAwait(true)!;
            }
            await Parent.EventAggregator.NotifyAsync("RowUpdating", saveEventArgs).ConfigureAwait(true);
            Parent.EditModule!.IsAdd = false;
            if (Parent.DataModule != null)
                await Parent.DataModule.GetData(args, eventArgs: saveEventArgs, requestType: "Save").ConfigureAwait(true);
            Parent.EditModule.IsAdd = Parent.EditSettings!.ShowAddNewRow ? true : false;
            await Parent.ModelChanged(new ActionEventArgs<T>() { Cancel = false, RequestType = Action.Refresh }, requestType: "Save", eventArgs: saveEventArgs, isSavingTriggered: true).ConfigureAwait(true);
        }

        internal async Task SaveCell(bool ForceSave = false, bool isDelete = false, bool isEscapeKey = false, bool focusLastGridCell = false, bool focusFirstCellOnShiftTab = false)
        {
            if (!Parent.IsEdit && !ForceSave || OriginalCell == null)
            {
                return;
            }

            if ((IsAdd || OriginalCell.IsEdit) && !ForceSave && !isDelete)
            {
                ForceValidate = true;
                if (EditContext != null && !EditContext.Validate() && OriginalCell.IsEdit)
                {
                    ForceValidate = false;
                    var Result = ErrorResult.Find(_ => _.FieldName == OriginalCell!.Column?.Field);
                    Result = Result == null ? ErrorResult.Find(_ => OriginalCell!.Column!.Field.Contains(_.FieldName ?? "", StringComparison.CurrentCulture)) : Result;
                    if (ErrorResult.Count != 0 && Result != null)
                    {
                        var Row = IsAdd ?
                            Parent.EditSettings!.NewRowPosition.Equals(NewRowPosition.Bottom) ?
                            Parent.Rows[Parent.Rows.Count - 1] : Parent.Rows[0] : OriginalRow;
                        var Cell = Row?.Cells.Find(_ => _.Column?.Field == Result.FieldName);
                        
                        if (Parent.EditModule != null && Parent.EditSettings != null && Parent.EditSettings.Mode.Equals(EditMode.Batch) && Parent.AllowPaging && Parent.EditModule.IsAdd && Parent.EditModule.IsLastRow && Parent.TotalItemCount == 0 && Parent.EditModule.EditNextCell)
                        {
                            IsEmptyRowRendered = true;
                            Parent.EventAggregator.Trigger("RowStateChanged", Parent.Rows);
                        }
                        if (!position.ContainsKey(GetComplexName(Result.Uid ?? Result.FieldName!)))
                        {
                            await InvokeValidation(new List<ValidationResult>() { Result }).ConfigureAwait(true);
                            await Parent.InvokeMethod(
                                "sfBlazor.Grid.focusCell",
                                Parent.DataId, OriginalCell?.Column?.Field, IsAdd).ConfigureAwait(true);
                        }
                        return;
                    }
                    if (Parent.EditSettings?.Validator != null)
                    {
                        return; // if removed custom validation will not work.
                    }
                }

                ForceValidate = false;
            }

            CellSaveArgs<T> args = null!;
            CellSavedArgs<T>? cellSavedArgs = null;
            if (!ForceSave)
            {
                // FIXED: Get the previous value from EditedData (if exists from previous edit) or from OriginalRow.Data
                // This ensures that when editing the same cell multiple times in batch mode:
                // - 1st edit: vinet → vinets uses OriginalRow.Data (original value)
                // - 2nd edit: vinets → vinetss uses OriginalRow.EditedData (intermediate value from 1st edit)
                // This way the undo stack correctly records each step: vinets, vinet
                var PreviousVal = Parent.PropHelper?.GetObject(OriginalCell!.Column!.Field, OriginalRow!.EditedData ?? OriginalRow!.Data);
                
                OriginalRow!.EditedData = CloneData!;
                var EditedValue = Parent.PropHelper?.GetObject(OriginalCell!.Column!.Field, OriginalRow.EditedData);
                
                args = new CellSaveArgs<T>()
                {
                    ColumnName = OriginalCell!.Column!.Field,
                    Value = EditedValue!,
                    PreviousValue = PreviousVal!,
                    RowData = (T)OriginalRow.Data!,
                    Cancel = false,
                    Data = (T)CloneData!,
                    IsForeignKey = OriginalCell.Column.IsForeignColumn(),
                    Column = OriginalCell.Column,
                    CellInfo = new CellDOM(OriginalCell.ClassList, OriginalCell.StyleList, OriginalCell.AttributeList),
                    Parent = Parent
                };
                if (!OriginalRow.Cells.Any(_ => _.IsDirty))
                {
                    OriginalRow.IsDirty = GridUtils.CompareValues<object>(args.PreviousValue, args.Value);
                }
                OriginalRow.EditedData = OriginalRow.IsDirty ? CloneData! : null!;
                await SfBaseUtils.InvokeEvent<CellSaveArgs<T>>(Parent.GridEvents?.OnCellSave, args).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("CellSave", args).ConfigureAwait(true);
                if (args.Cancel)
                {
                    return;
                }
                cellSavedArgs = new CellSavedArgs<T>()
                {
                    ColumnName = args.ColumnName,
                    Value = args.Value,
                    PreviousValue = args.PreviousValue,
                    RowData = args.RowData,
                    Data = args.Data,
                    IsForeignKey = args.IsForeignKey,
                    Column = args.Column,
                    CellInfo = new CellDOM(OriginalCell.ClassList, OriginalCell.StyleList, OriginalCell.AttributeList),
                    Parent = args.Parent
                };
                if (Parent.Aggregates?.Count > 0 && Parent.ReactiveAggregateModule != null)
                {
                    await Parent.ReactiveAggregateModule.RefreshFooterAggregate().ConfigureAwait(true);
                    await Parent.ReactiveAggregateModule.UpdateGroupCaptionFooterAggregates().ConfigureAwait(true);
                }
                if (GridUtils.CompareValues<object>(EditedValue!, args.Value))
                {
                    SetValue(args.Value, OriginalCell.Column.Field);
                    OriginalRow.EditedData = CloneData!;
                }

                var checkboxColumn = OriginalCell.Column.Type.Equals(ColumnType.CheckBox);
                if (OriginalRow.Action != EditAction.Added && !checkboxColumn)
                {
                    OriginalCell.IsDirty = !OriginalCell.IsDirty ?
                        GridUtils.CompareValues<object>(args.PreviousValue, args.Value) : true;
                    IsDirtyHandler();
                }
                else if (!checkboxColumn)
                {
                    OriginalCell.IsDirty = true;
                }
            }

            Parent.IsEdit = false;
            OriginalCell.IsEdit = false;
            Parent.SoftRefresh = true;
            ClearSelection = true;
            Parent.EventAggregator.Trigger("RowStateChanged", OriginalRow!);
            var selectedRow = Parent.FocusModule?.SelectedRowIndex != null ? Parent.Rows?.Find(_ => _.Index == Parent.FocusModule.SelectedRowIndex) : null;
            var cellIndex = Parent.FocusModule?.SelectedCellIndex;
            var lastCell = Parent.FocusModule?.GetLastVisibleCell();
            if ((isEscapeKey || (lastCell != null && lastCell.IsFocused)) && selectedRow != null && cellIndex != null && !Parent.SelectionSettings!.Mode.Equals(SelectionMode.Cell) || focusLastGridCell || focusFirstCellOnShiftTab)
            {
                bool isLastBatchEditCell = Parent.EditSettings?.Mode == EditMode.Batch && (lastCell != null && lastCell.IsFocused || focusLastGridCell);
                await (Parent.FocusModule?.Focus(selectedRow!.Uid!, selectedRow.Cells[(int)cellIndex!].Uid, "SaveCell", cellColIndex: selectedRow.Cells[(int)cellIndex!].Index + 1 ?? -1,
                    isLastBatchEditCell: isLastBatchEditCell)!).ConfigureAwait(true);
            }
            if (!ForceSave)
            {
                await SfBaseUtils.InvokeEvent<CellSavedArgs<T>>(Parent.GridEvents?.CellSaved, cellSavedArgs!).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("CellSaved", cellSavedArgs!).ConfigureAwait(true);

                // Record cell edit action for undo/redo
                // IMPORTANT: Skip recording CellEdit for newly added rows.
                // Newly added rows should only have RowAdd action, so undoing removes the entire row.
                // This prevents the issue where undoing an edit on a new row reverts it to default values
                // instead of removing the row completely. Matches EJ2 behavior.
                bool isNewlyAddedRow = (OriginalRow?.Action ?? EditAction.None) == EditAction.Added;
                
                if (!isNewlyAddedRow && cellSavedArgs != null && Parent.UndoRedoManager != null)
                {
                    // Use helper method to record cell edit action
                    Parent.UndoRedoManager.RecordCellEditAction(
                        OriginalRow.Index ?? -1,
                        OriginalCell.Index ?? -1,
                        OriginalCell.Column?.Field,
                        cellSavedArgs.PreviousValue,
                        cellSavedArgs.Value,
                        OriginalCell.Column);
                }
                else if (isNewlyAddedRow &&
                         Parent.EditSettings?.EnableUndoRedo == true &&
                         Parent.EditSettings?.Mode == EditMode.Batch &&
                         Parent.UndoRedoManager != null &&
                         Parent.UndoRedoManager.IsEnabled &&
                         OriginalRow != null)
                {
                    // CRITICAL FIX: Update the RowAdd action with the latest edited data
                    // When editing a newly added row multiple times, each edit should update
                    // the RowAdd action's rowData so that Redo restores with all accumulated edits
                    // This follows the EJ2 pattern: "If row already in undo stack, just update rowData"
                    var rowIndex = OriginalRow.Index ?? -1;
                    var wasUpdated = Parent.UndoRedoManager.UpdateLastRowAddAction(rowIndex, (T)OriginalRow.EditedData!);
                    
                    if (wasUpdated)
                    {
                        Parent.UndoRedoManager.TriggerUndoRedoStackChanged();
                    }
                }
            }
        }

        internal async Task DeleteRecord(string fieldname = null!, object data = null!)
        {
            Parent.FocusModule!.SelectedCellIndex = await Parent.EditModule!.GetSelectedCellIndex().ConfigureAwait(true);
            if (Parent.EditSettings?.Mode == EditMode.Batch)
            {
                await SaveCell(isDelete: true).ConfigureAwait(true);
                await BulkDelete(fieldname, data, true).ConfigureAwait(true);
                if (ErrorResult.Count > 0)
                {
                    ClearRules();
                }
                if (Parent.Aggregates?.Count > 0 && Parent.ReactiveAggregateModule != null)
                {
                    await Parent.ReactiveAggregateModule.RefreshFooterAggregate().ConfigureAwait(true);

                    if (Parent.GroupSettings?.Columns != null)
                    {
                        await Parent.ReactiveAggregateModule.UpdateGroupCaptionFooterAggregates().ConfigureAwait(true);
                    }
                }

                IEnumerable<Row<object>> VisibleRow = Parent.Rows
                .Select((row, index) => new { Row = row, Index = index })
                .Where(x => x.Row.CssClass == null || !x.Row.CssClass.Contains("e-hiddenrow", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Row);

                LastVisibleRow = VisibleRow?.AsQueryable().LastOrDefault()!;
                if (LastVisibleRow != null)
                {
                    if (!Parent.RequireLastRowBorder && VisibleRow?.AsQueryable().Count() <= Parent.MaxVisibleRowsCount)
                    {
                        Parent.RequireLastRowBorder = true;
                    }
                    LastVisibleRow.IsLastRow = true;

                    Parent.EventAggregator.Trigger("RowStateChanged", LastVisibleRow);
                }
            }
            else
            {
                // 🔧 FIXED: Use data parameter to find deletedRow instead of relying on SelectionModule
                // This prevents null deletedRow when selection has been cleared during SaveCell
                
                // Fetch primary keys once and reuse throughout this block
                var primaryKeys = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
                Row<object>? deletedRow = null;

                // Strategy 1: Try to find row by data parameter (primary key matching)
                if (data != null && primaryKeys?.Count > 0)
                {
                    var primaryKeyField = primaryKeys!.FirstOrDefault();
                    
                    if (primaryKeyField != null)
                    {
                        var dataKeyValue = Parent.PropHelper?.GetObject(primaryKeyField, data);
                        deletedRow = Parent.Rows?.FirstOrDefault(row =>
                            row.Data != null &&
                            GridUtils.CompareValues<object>(
                                Parent.PropHelper?.GetObject(primaryKeyField, row.Data)!,
                                dataKeyValue!
                            )
                        );
                    }
                }

                // Strategy 2: Fallback to SelectionModule if data not provided
                if (deletedRow == null && Parent.SelectionModule != null)
                {
                    deletedRow = Parent.SelectionModule.SelectedRow();
                }

                var columns = GridUtils.GetColumns(Parent);
                EditRowIndex = columns.Any(x => x.Type == ColumnType.CheckBox) ? -1 : deletedRow?.Index;
                
                // Now deletedRow should be properly found (not null)
                if (deletedRow != null)
                {
                    deletedRow.Action = EditAction.Deleted;
                    
                }
                
                var args = new ActionEventArgs<T>()
                {
                    RequestType = Action.Delete,
                    Type = "ActionBegin",
                    Cancel = false,
                    RowData = (T)data ?? Parent.SelectedRecords[0],
                    Data = (T)data! ?? Parent.SelectedRecords[0],
                    PrimaryKeys = primaryKeys?.ToArray()!,
                    Action = "Delete"
                };
                var deleteEventArgs = new RowDeletingEventArgs<T>()
                {
                    Cancel = false,
                    Datas = (T)data! != null ? new List<T>() { (T)data } : Parent.SelectedRecords,
                    PrimaryKeys = primaryKeys?.ToArray()!
                };
                if (Parent.EnableInfiniteScrolling)
                {
                    var deletedRows = Parent.Rows?.Where(x => x.IsSelected).ToList();
                }
                if (Parent.FocusModule != null)
                    Parent.FocusModule.IsChildFocused = false;
                await Parent.ModelChanged(args, eventArgs: deleteEventArgs, requestType: "Delete", isDeleteAction: fieldname != null && data != null).ConfigureAwait(true);
            }
        }

        internal bool isEditable(GridColumn Column)
        {
            if (Column != null && Column.IsIdentity)
            {
                return false;
            }
            if (Column != null && (!Column.Visible || !Column.AllowEditing || Column.IsPrimaryKey && !IsAdd || !Column.AllowAdding && IsAdd))
            {
                if (!Column.AllowEditing && Column.Visible && Column.AllowAdding && IsAdd)
                {
                    return true;
                }
                else if (!Column.IsPrimaryKey && !Parent.GroupSettings!.ShowGroupedColumn && !Column.Visible && Parent.GroupSettings.Columns != null && Parent.GroupSettings.Columns.Any(col => col.Equals(Column.Field, StringComparison.Ordinal)))
                {
                    return true;
                }
                else if (Parent.EditSettings!.Mode.Equals(EditMode.Batch) && ((Column.IsPrimaryKey && Column.Visible && Column.AllowAdding)
                    || (Parent.EditModule!.OriginalCell?.IsDirty == true && Parent.EditModule.OriginalRow?.Action == EditAction.Added
                    && !IsAdd && !Column.AllowEditing)))
                {
                    return true;
                }
                return false;
            }

            return true;
        }

        #endregion

        #region Batch Operations
        private async Task BulkAddRow(object data = null!)
        {
            if (Parent.IsEdit)
            {
                await SaveCell().ConfigureAwait(true);
                await ValidateNextCell().ConfigureAwait(true);
            }

            if (Parent.IsEdit)
            {
                return;
            }

            var Row = GetModelGenerator(true);
            CloneData = data ?? (T)Row.Data!;
            SetDefaultValue(data != null);
            if ((Parent.CurrentViewData == null || !Parent.CurrentViewData.Any()) && Parent.Rows.Count == 0 && Parent.Aggregates?.Count > 0 && Parent.Aggregates.Where(e => e.Columns!.Any(_ => _.FooterTemplate != null)).Any() && Parent.ReactiveAggregateModule != null)
            {
                Parent.ReactiveAggregateModule.UpdateEmptyList(CloneData);
            }
            EditContext = CloneData != null ? new EditContext(CloneData) : null!;
            var args = new BeforeBatchAddArgs<T>()
            {
                DefaultData = (T)CloneData!,
                PrimaryKey = (await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true))?.ToArray()!,
                Cancel = false,
                EditContext = EditContext,
                Index = 0,
                Parent = Parent
            };
            await SfBaseUtils.InvokeEvent<BeforeBatchAddArgs<T>>(Parent.GridEvents?.OnBatchAdd, args).ConfigureAwait(true);
            await Parent.EventAggregator.NotifyAsync("BatchAdd", args).ConfigureAwait(true);
            if (args.Cancel)
            {
                return;
            }

            var cData = CloneData;
            EnsureDataAndEditContext(ref cData!, batchAddArgs: args);
            CloneData = cData;
            Row.Data = cData;
            Row.EditedData = CloneData;
            Parent.IsAdd = IsAdd = HasBatchChanges = true;
            await Parent.ClearSelectionAsync().ConfigureAwait(true);
            var addedRowIndex = AddRows(Row, args.Index);
            var Column = GridUtils.GetColumns(Parent).Find(col => isEditable(col));
            if (Parent.IsAdd && Column != null && !Column.AllowAdding && Parent.Columns!.Where(e => e.IsIdentity).Any())
            {
                Column = GridUtils.GetColumns(Parent).Where(e => e.AllowAdding).FirstOrDefault();
            }
            var Cell = Row.Cells.Find(_ => _.Column!.Field == Column?.Field);
            EditedRow = Row;
            if (Cell != null && data == null)
            {
                await EditCell(Row, Cell).ConfigureAwait(true);
            }

            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
            if (Parent.FrozenRows > 0)
            {
                Parent.EventAggregator.Trigger("HeaderStateChanged", null!);
            }
            else
            {
                Parent.EventAggregator.Trigger("ContentStateChanged", null!);
            }
            await Parent.SelectRowAsync(Parent.IsRenderedFromTreeGrid ? args.Index : addedRowIndex).ConfigureAwait(true);
        }

        internal async Task BatchSave()
        {
            await SaveCell().ConfigureAwait(true);
            var PrimaryFields = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            var PrimaryKey = PrimaryFields.Count != 0 ? PrimaryFields[0] : null;
            var BatchChanges = GetBatchChanges();

            if (Parent.GridEvents?.OnBatchSave.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                var args = new BeforeBatchSaveArgs<T>()
                {
                    BatchChanges = BatchChanges,
                    Cancel = false,
                    Parent = Parent
                };

                if(Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("BatchSave", args).ConfigureAwait(true);
                else
                    await Parent.GridEvents!.OnBatchSave.InvokeAsync(args).ConfigureAwait(true);
                if (args.Cancel)
                    return;
            }

            var ChangesUpdated = await Parent.DataModule!.SaveChanges(BatchChanges, PrimaryKey!).ConfigureAwait(true);
            if (!ChangesUpdated)
            {
                return;
            }

            // IMPORTANT: Clear undo/redo stacks on successful batch save (EJ2 behavior)
            // This matches EJ2 grid behavior where undo/redo history is cleared after batch save
            // to prevent undoing changes that have already been persisted to the server
            if (Parent.UndoRedoManager != null && Parent.UndoRedoManager.IsEnabled)
            {
                Parent.UndoRedoManager.Clear();
            }

            HasBatchChanges = false;
            Parent.Rows?.ForEach(row =>
            {
                row.IsDirty = false;
                row.IsAddedTop = false;
                row.IsAddedBottom = false;
                row.Cells?.ForEach(_ =>
                {
                    _.IsDirty = false;
                    _.EditDisabled = false;
                });
            });

            Parent.PreventRender(false);
            await Parent.DataProcess(actionArgs: new ActionEventArgs<T>() { RequestType = Action.BatchSave }).ConfigureAwait(true);
            if(Parent.SelectionModule != null)
            {
                Parent.SelectionModule.AutofillChanges();
            }
            IsCheckBoxColumn();
            if (Parent.Aggregates?.Count > 0 && Parent.ReactiveAggregateModule != null)
            {
                Parent.ReactiveAggregateModule.OriginalCells.Clear();
            }
        }
        
        private int AddRows(Row<object> row, int index = 0)
        {
            row.Cells?.ForEach(_ => _.IsDirty = true);
            var addedRowIndex = 0;
            if (Parent.EditSettings != null && Parent.EditSettings.NewRowPosition == NewRowPosition.Top)
            {
                row.IsAddedTop = true;
                Parent.Rows?.Insert(index, row);
            }
            else
            {
                row.IsAddedBottom = true;
                addedRowIndex = Parent.Rows.Count;
                Parent.Rows?.Add(row);
            }

            RefreshRowIndex();
            row.IsDirty = true;
            row.Action = EditAction.Added;

            // Record row addition action for undo/redo using helper method
            if (Parent.UndoRedoManager != null && CloneData != null)
            {
                Parent.UndoRedoManager.RecordRowAddAction(
                    (T)CloneData,
                    addedRowIndex >= 0 ? addedRowIndex : row.Index ?? -1,
                    Parent.EditSettings?.NewRowPosition ?? NewRowPosition.Bottom);
            }
            return addedRowIndex;
        }

        internal async Task ApplyBatchChanges(BatchChanges<T> value)
        {
            var rows = Parent.Rows?.FindAll(_ => _.IsDataRow);
            if (Parent.IsEdit && rows?.Count > 0)
            {
                await SaveCell().ConfigureAwait(true);
                await ValidateNextCell().ConfigureAwait(true);
            }
            if (Parent.IsEdit)
            {
                return;
            }
            await Parent.ClearSelectionAsync().ConfigureAwait(true);
            Parent.FocusModule?.ClearCurrent();
            var changedReords = value.ChangedRecords;
            var deletedRecords = value.DeletedRecords;
            var addedRecords = value.AddedRecords;
            if (addedRecords.Count > 0)
            {
                foreach (var data in addedRecords)
                {
                    var Row = GetModelGenerator(true);
                    CloneData = data ?? (T)Row.Data!;

                    if ((Parent.CurrentViewData == null || !Parent.CurrentViewData.Any()) && Parent.Rows?.Count == 0 && Parent.Aggregates?.Count > 0 && Parent.Aggregates.Where(e => e.Columns!.Any(_ => _.FooterTemplate != null)).Any())
                    {
                        Parent.ReactiveAggregateModule?.UpdateEmptyList(CloneData);
                    }

                    EditContext = CloneData != null ? new EditContext(CloneData) : null!;
                    var cData = CloneData;
                    CloneData = cData!;
                    Row.Data = cData!;
                    Row.EditedData = CloneData!;
                    Parent.IsAdd = IsAdd = HasBatchChanges = true;
                    AddRows(Row);
                }
            }

            List<T> currentViewData = await Parent.GetCurrentViewRecordsAsync().ConfigureAwait(true);
            if (changedReords.Count > 0)
            {
                await BatchUpdates(changedReords, "Edit", currentViewData).ConfigureAwait(true);
            }

            if (deletedRecords.Count > 0)
            {
                await BatchUpdates(deletedRecords, "Delete", currentViewData).ConfigureAwait(true);
            }
            HasBatchChanges = true;
            Parent.SoftRefresh = true;
            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
            if (Parent.FrozenRows > 0 && addedRecords.Count > 0)
            {
                Parent.EventAggregator.Trigger("FrozenHeaderStateChanged", null!);
            }
            else
            {
                Parent.EventAggregator.Trigger("ContentStateChanged", null!);
            }
            if (addedRecords.Count > 0 && Parent.FreezeModule!.GetFrozenCount() > 0)
            {

                await Parent.InvokeMethod("sfBlazor.Grid.frozenHeight", new object[] { Parent.DataId, Parent.GetClientOption(), null! }).ConfigureAwait(true);
            }
        }
        
        private async Task BatchUpdates(List<T> batchChanges, String action, List<T> Data)
        {
            var primaryKeyFields = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            var primaryKeyField = primaryKeyFields.Count > 0 ? primaryKeyFields[0] : null;
            foreach (var data in batchChanges)
            {
                var key = Parent.PropHelper?.GetObject(primaryKeyField!, data);
                var index = Data.FindIndex(item => Parent.PropHelper?.GetObject(primaryKeyField!, item)?.ToString() == key?.ToString());
                var row = index >= 0 && index < Parent.Rows?.Count ? Parent.Rows[index] : null;
                if (row != null)
                {
                    if (string.Equals("Delete", action, StringComparison.Ordinal))
                    {
                        row.IsDirty = true;
                        row.Action = EditAction.Deleted;
                    }
                    else
                    {
                        CloneRowData(row.EditedData! ?? row.Data!);
                        foreach (var cell in row.Cells)
                        {
                            cell.IsDirty = cell.Column!.AllowEditing ? GridUtils.CompareValues<object>(Parent.PropHelper?.GetObject(cell.Column.Field, row.Data!)!, Parent.PropHelper!.GetObject(cell.Column.Field, data)) : false;
                            if (cell.IsDirty)
                            {
                                SetValue(DataUtil.GetObject(cell.Column.Field, data!), cell.Column.Field);
                            }
                        }
                        row.IsDirty = row.Cells.Any(_ => _.IsDirty);
                        row.HasDataChanges = true;
                        row.EditedData = CloneData!;
                    }
                }
            }
        }
        
        internal bool IsDirty()
        {
            foreach (var row in Parent.Rows)
            {
                if (row.IsDirty)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task BulkDelete(string Field, object data, bool isDelete = false)
        {
            var field = Field;
            if (!isDelete && EditContext != null && !EditContext.Validate())
            {
                await SaveCell(true, isDelete).ConfigureAwait(true);
            }

            var rows = Parent.Rows?.FindAll(_ => _.IsDataRow);
            var dataRow = new Row<object>();
            if (data != null && rows != null)
            {
                foreach (var row in rows)
                {
                    var primaryKey = (await GetPrimaryKeyValue(row.Data!).ConfigureAwait(true)).Count != 0 ? (await GetPrimaryKeyValue(row.Data!).ConfigureAwait(true))[0] : string.Empty;
                    var dataKey = (await GetPrimaryKeyValue(data).ConfigureAwait(true)).Count != 0 ? (await GetPrimaryKeyValue(data).ConfigureAwait(true))[0] : string.Empty;
                    if (primaryKey.Equals(dataKey.ToString(), StringComparison.Ordinal))
                    {
                        dataRow = row;
                        break;
                    }
                }
            }

            var args = new BeforeBatchDeleteArgs<T>()
            {
                PrimaryKey = (await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true))?.ToArray()!,
                RowIndex = (int)(dataRow.Index ?? Parent.SelectionModule?.SelectedRow()?.Index ?? -1),
                RowData = (T)data! ?? Parent.SelectedRecords[0],
                Cancel = false,
                Parent = Parent
            };
            EditRowIndex = EditRowIndex ?? (int?)args.RowIndex;
            await SfBaseUtils.InvokeEvent<BeforeBatchDeleteArgs<T>>(Parent.GridEvents?.OnBatchDelete, args).ConfigureAwait(true);
            await Parent.EventAggregator.NotifyAsync("BatchDelete", args).ConfigureAwait(true);
            if (args.Cancel)
            {
                return;
            }

            var Rows = Parent.Rows?.FindAll(_ => _.IsSelected || _.Index == args.RowIndex);
            if (Parent.DetailRowModule != null)
            {
                Parent.DetailRowModule.IncludeDetailRowsInBatchDelete(Rows);
            }

            HasBatchChanges = true;
            ClearSelection = true;
            var addedRows = Parent.Rows?.FindAll(row => row.Action == EditAction.Added && row.IsAddedTop);
            var batchChanges = Parent.Rows?.FindAll(row => row.EditedData != null && row.Action != EditAction.Deleted && row.Action != EditAction.Added);
            if (addedRows?.Count == 1 && batchChanges?.Count == 0)
            {
                HasBatchChanges = !(Rows?.Any(row => row.Uid == addedRows[0].Uid) == true);
            }
            Rows?.ForEach(_ =>
            {
                _.IsDirty = true;
                _.Action = EditAction.Deleted;

                // Record row deletion action for undo/redo using helper method
                // 🔧 FIXED: Store current edited state (EditedData) if available, else original (Data)
                // This ensures undo delete restores the row with all user edits, not the original value
                // E.g. Edit: vinet→vinets, Delete, Ctrl+Z should restore "vinets" not "vinet"
                if (Parent.UndoRedoManager != null && (_.EditedData != null || _.Data != null))
                {
                    var rowDataToStore = (T?)(_.EditedData ?? _.Data);
                    Parent.UndoRedoManager.RecordRowDeleteAction(rowDataToStore, _.Index ?? -1);
                }
            });
            
            // Trigger stack changed events
            if (Parent.UndoRedoManager != null)
            {
                Parent.UndoRedoManager.TriggerUndoRedoStackChanged();
            }
            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
            await Parent.SelectRowAsync(args.RowIndex + 1).ConfigureAwait(true);
        }

        internal async Task BatchClose(bool escapeKey = false)
        {
            // Clear both undo and redo stacks on batch cancel (EJ2 behavior)
            // When user cancels batch edits, we clear the history to reflect that changes were discarded
            if (Parent.EditSettings?.EnableUndoRedo == true &&
                Parent.UndoRedoManager != null)
            {
                Parent.UndoRedoManager.Clear();
                Parent.EventAggregator.Trigger("UndoRedoStackChanged", null!);
            }

            var batchChanges = GetBatchChanges();
            var editedRow = Parent.Rows?.Where(s => s.Cells?.Any(c => c.IsEdit) ?? false).FirstOrDefault();
            EditRowIndex = editedRow?.Index ?? EditRowIndex;
            if (Parent.Aggregates?.Any(e => e.Columns?.Any(e => e.FooterTemplate != null) ?? false) == true)
            {
                Parent.ReactiveAggregateModule?.RefreshAggregateAfterBatchCancel(); 
            }
            if (Parent.Aggregates?.Count > 0 && Parent.AllowGrouping && Parent.GroupSettings?.Columns != null && Parent.GroupSettings.Columns.Length > 0)
            {
                Parent.ReactiveAggregateModule?.HandleBatchCancel();
            }
            if (Parent.GridEvents?.OnBatchCancel.HasDelegate == true)
            {
                var cancelArgs = new BeforeBatchCancelArgs<T>()
                {
                    BatchChanges = batchChanges,
                    Cancel = false,
                    Parent = Parent
                };
                await Parent.GridEvents.OnBatchCancel.InvokeAsync(cancelArgs).ConfigureAwait(true);
                if (cancelArgs.Cancel)
                    return;
            }
            if (!escapeKey)
            {
                IsAdd = false;
            }

            var args = new ActionEventArgs<T>()
            {
                RequestType = Action.Cancel,
                Type = "BatchCancel"
            };
            Parent.EventAggregator.Trigger("BatchCancel", args);
            if (args.Cancel) { return; }
            if (Parent.IsEdit)
            {
                await SaveCell(true, false, escapeKey).ConfigureAwait(true);
            }
            if (!escapeKey && Parent.Rows != null)
            {
                Parent.Rows.RemoveAll(_ => _.IsAddedTop || _.IsAddedBottom);
                if (Parent.IsColumnHeaderChange && !Parent._shouldRender)
                {
                    Parent._shouldRender = true;
                }
                foreach (var row in Parent.Rows)
                {
                    row.EditedData = null!;
                    row.IsDirty = false;
                    row.Action = EditAction.None;
                    row.Cells?.ForEach(_ =>
                    {
                        _.EditDisabled = false;
                        if (_.IsDirty)
                        {
                            _.IsDirty = false;
                            _.Changes = true;
                        }
                    });
                }
            }
            else if (escapeKey && OriginalRow?.IsDirty == true && !(OriginalRow.Cells?.Any(_ => _.IsDirty) ?? false))
            {
                OriginalRow.IsDirty = false;
            }

            ClearRules();
            OriginalCell = null!;
            OriginalRow = null!;
            Parent.SoftRefresh = true;
            HasBatchChanges = Parent.Rows?.Any(_ => _.IsDirty) ?? false;
            if (Parent.FrozenRows != 0)
            {
                Parent.ForceUpdate = true;
            }

            LastVisibleRow = new Row<object>();

            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
            if (Parent.FrozenRows > 0)
            {
                Parent.EventAggregator.Trigger("HeaderStateChanged", null!);
            }
            Parent.EventAggregator.Trigger("ContentStateChanged", null!);

            if (!IsCheckBoxOnly())
            {
                await Parent.SelectRowAsync((int)(EditRowIndex ?? -1)).ConfigureAwait(true);
            }
            EditRowIndex = null;
            Parent.SelectionModule?.AutofillChanges();
            IsCheckBoxColumn();
            if (Parent.FreezeModule!.GetFrozenCount() > 0)
            {

                await Parent.InvokeMethod("sfBlazor.Grid.frozenHeight", new object[] { Parent.DataId, Parent.GetClientOption(), null! }).ConfigureAwait(true);
            }
            if (Parent.Aggregates?.Count > 0)
            {
                Parent.ReactiveAggregateModule?.OriginalCells.Clear();
            }
            if (Parent.AllowGrouping && Parent.GroupSettings?.Columns?.Length > 0)
            {
                Parent.PreventRender(false);
            }
        }

        internal BatchChanges<T> GetBatchChanges()
        {
            var BatchChanges = new BatchChanges<T>();
            Parent.Rows?.ForEach(Row =>
            {
                if (Row.IsDirty && !Row.IsDetailRow)
                {
                    switch (Row.Action)
                    {
                        case EditAction.Deleted:
                            BatchChanges.DeletedRecords.Add((T)(Row.EditedData ?? Row.Data)!);
                            break;
                        case EditAction.Added:
                            BatchChanges.AddedRecords.Add((T)Row.EditedData!);
                            break;
                        default:
                            if (((T)Row.EditedData!) != null)
                            {
                                BatchChanges.ChangedRecords.Add((T)Row.EditedData);
                            }
                            break;
                    }
                }
            });

            return BatchChanges;
        }

        internal async Task HandleBatchEditDuringReorder()
        {
            if (Parent.EditSettings?.Mode == EditMode.Batch && Parent.IsEdit && Parent.EditModule != null)
            {
                await Parent.EditModule.SaveCell(ForceSave: true).ConfigureAwait(true);
                Parent.EditModule.OriginalCell = null!;
                Parent.EditModule.OriginalRow = null!;
            }
        }

        internal async Task PerformBatchActions(ActionEventArgs<T> args = null!, ActionArgs? additionalArguments = null)
        {
            await BatchClose().ConfigureAwait(true);
            args = args ?? BatchActionArgs!;
            await Parent.ModelChanged(args, additionalArgs: additionalArguments).ConfigureAwait(true);
            if (args != null && args.RequestType == Action.Reorder && Parent.EditSettings != null && Parent.EditSettings.ShowConfirmDialog)
            {
                await Parent.InvokeMethod("sfBlazor.Grid.updateOptions", new object[] { Parent.DataId, Parent.GetClientOption() }).ConfigureAwait(true);
            }
            BatchAdditionalArgs = null;
        }
        internal async Task ShowAlertDialog(ActionEventArgs<T> args, ActionArgs? additionalArgs = null)
        {
            BatchActionArgs = args;
            BatchAdditionalArgs = additionalArgs;
            AlertMessage = "BatchSaveLostChanges";
            if (Parent.IsEdit)
            {
                await SaveCell().ConfigureAwait(true);

                if (args?.RequestType != Action.Reorder)
                    return;
            }
            Parent.EventAggregator.Trigger("ShowValidationDialog", null!);
        }

        private void IsDirtyHandler()
        {
            if (OriginalCell != null && OriginalCell.IsDirty)
            {
                OriginalRow!.IsDirty = true;
                if (!HasBatchChanges)
                {
                    HasBatchChanges = true;
                    Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
                }
            }
        }

        #endregion

        #region Validation Operation

        internal bool ValidateDeleteOperation()
        {
            if (Parent.SelectedRecords.Count == 0)
            {
                AlertMessage = "DeleteAlert";
                Parent.EventAggregator.Trigger("ShowValidationDialog", null!);
                return false;
            }

            if (Parent.EditSettings!.ShowDeleteConfirmDialog)
            {
                AlertMessage = "DeleteConfirmAlert";
                Parent.EventAggregator.Trigger("ShowValidationDialog", null!);
                return false;
            }

            return true;
        }

        internal bool ValidateDeleteOperation(object data)
        {
            if (data == null)
            {
                if (Parent.SelectedRecords.Count == 0)
                {
                    AlertMessage = "DeleteAlert";
                    Parent.EventAggregator.Trigger("ShowValidationDialog", null!);
                    return false;
                }
            }

            if (Parent.EditSettings != null && Parent.EditSettings.ShowDeleteConfirmDialog)
            {
                AlertMessage = "DeleteConfirmAlert";
                Parent.EventAggregator.Trigger("ShowValidationDialog", data!);
                return false;
            }

            return true;
        }

        internal void ClearValidationErrors()
        {
            if (ErrorResult?.Count > 0)
            {
                ErrorResult = new List<ValidationResult>();
                position = new Dictionary<string, object>();
                Parent.EventAggregator.Trigger("ShowValidationMessage", null!);
            }
        }

        internal async Task ApplyFormValidation(ValidationResult result)
        {
            var Columns = GridUtils.GetColumns(Parent);
            var TemplateColumn = Columns.Find(x => x.Field == result.FieldName)?.EditTemplate != null;
            var TemplateProp = Parent.EditSettings!.Template != null;
            int deletedRecordsCount = 0;
            if (Parent.EditSettings.Mode.Equals(EditMode.Batch) && Parent.Rows?.Count > 0)
            {
                var batchChanges = await Parent.GetBatchChangesAsync().ConfigureAwait(true);
                deletedRecordsCount = batchChanges.DeletedRecords.Count;
            }
            bool IsDummyRowNeeded = Parent.EditSettings.Mode.Equals(EditMode.Batch) && Parent.Rows?.Count - deletedRecordsCount <= 2;
            if (Columns != null)
            {
                if (ValueChanged || ForceValidate || TemplateColumn || TemplateProp || ErrorResult.Count > 0 || result.Message != null)
                {
                    var editable = isEditable(Columns.Find(x => x.Field == result.FieldName)!) &&
                        (Columns.Find(x => x.Field == result.FieldName)?.Visible ?? true);
                    if (result != null && !result.IsValid && !string.IsNullOrEmpty(result.FieldName))
                    {
                        if (ErrorResult.Any(x => x.Uid == result.Uid) || ErrorResult.Any(x => result.Uid != null && result.Uid.Contains(x.Uid!, StringComparison.CurrentCulture)))
                        {
                            if (ErrorResult.Any(x => x.Message == result.Message) && position.ContainsKey(GetComplexName(result.Uid ?? result.FieldName)))
                            {
                                return;
                            }

                            ErrorResult.Remove(ErrorResult.Find(x => x.Uid != null ? x.Uid == result.Uid : x.FieldName == result.FieldName)!);
                            if (Columns.Any(x => x.Field.Contains('.', StringComparison.Ordinal)) && result?.Uid != null && ErrorResult.Any(x => result.Uid.Contains(x.Uid!, StringComparison.CurrentCulture)))
                            {
                                ErrorResult.Remove(result);
                            }
                        }

                        if (editable)
                        {
                            ErrorResult.Add(result!);
                        }

                        if (Parent.EditSettings.Mode.Equals(EditMode.Normal) || (Parent.EditSettings.Mode.Equals(EditMode.Batch) && (Parent.TotalItemCount == 0 && !Parent.EditModule!.EditNextCell) || (Parent.TotalItemCount != 0 && Parent.TotalItemCount <= 2) || IsDummyRowNeeded))
                        {
                            if (ErrorResult.Count > 0 && Parent.AllowPaging && (Parent.TotalItemCount == 0 && Parent.EditModule!.IsAdd)
                                || (Parent.TotalItemCount <= 2 && !Parent.EditModule!.IsAdd && Parent.EditModule.IsLastRow) || IsDummyRowNeeded)
                            {
                                IsEmptyRowRendered = true;
                                Parent.EventAggregator.Trigger("RowStateChanged", Parent.Rows!);
                            }
                        }

                        if ((TemplateColumn || ValueChanged || TemplateProp || result?.Message != null) && editable && !ForceValidate)
                        {
                            await InvokeValidation(new List<ValidationResult>() { result! }).ConfigureAwait(true);
                            return;
                        }
                    }
                    else if (result!.IsValid)
                    {
                        if (ErrorResult.Any(x => x.FieldName == result.FieldName) || (Columns.Any(x => x.Field.Contains('.', StringComparison.Ordinal)) && ErrorResult.Any(x => result.FieldName!.Contains(x.FieldName ?? "", StringComparison.CurrentCulture))))
                        {
                            var Count = ErrorResult.Count;
                            ErrorResult.Remove(ErrorResult.Find(x => x.Uid == result.Uid)!);
                            if (Columns.Any(x => x.Field.Contains('.', StringComparison.Ordinal)) && ErrorResult.Any(x => result.Uid != null && result.Uid.Contains(x.Uid ?? "", System.StringComparison.CurrentCulture)))
                            {
                                ErrorResult.Remove(result);
                            }
                            position.Remove(GetComplexName(result.Uid ?? result.FieldName!));
                            if (ValueChanged || TemplateColumn || TemplateProp || Count > 0)
                            {
                                Parent.EventAggregator.Trigger("ShowValidationMessage", null!);
                            }
                            if (position.Count == 0 && IsEmptyRowRendered)
                            {
                                IsEmptyRowRendered = false;
                                Parent.EventAggregator.Trigger("RowStateChanged", Parent.Rows!);
                            }
                        }
                    }

                    ValueChanged = false;
                }
            }
        }

        internal IDictionary<string, object> ValidationRules()
        {
            Dictionary<string, object> Rules = new Dictionary<string, object>();
            foreach (var Column in GridUtils.GetColumns(Parent))
            {
                if (Column.ValidationRules != null && Column.Visible)
                {
                    if (!Rules.ContainsKey(Column.Uid))
                    {
                        if (Column.ValidationRules is ValidationRules _rule)
                        {
                            Rules.Add(Column.Uid, _rule.ToDictionary());
                        }
                        else
                        {
                            Rules.Add(Column.Uid, Column.ValidationRules);
                        }
                    }
                }
            }

            return Rules;
        }

        internal async Task InvokeValidation(List<ValidationResult> results)
        {
            await Parent.InvokeMethod(
                "sfBlazor.Grid.validation",
                new object[] { Parent.DataId, results, IsAdd, Parent.GetClientOption().newRowPosition! }).ConfigureAwait(true);
        }

        internal async Task ValidateNextCell()
        {
            if (IsAdd && !Parent.IsEdit)
            {
                ForceValidate = true;
                if (EditContext != null && !EditContext.Validate())
                {
                    ForceValidate = false;
                    if (ErrorResult?.Count > 0)
                    {
                        var Row = EditedRow;
                        var Cell = Row?.Cells?.Find(_ => _.Column?.Field?.Split('.')?.LastOrDefault() == ErrorResult[0].FieldName);
                        if (Row != null && Cell != null)
                        {
                            await EditCell(Row, Cell).ConfigureAwait(true);
                        }
                    }
                }
                else
                {
                    IsAdd = false;
                }

                ForceValidate = false;
            }
        }

        internal void ClearRules()
        {
            ErrorResult = new List<ValidationResult>();
            position = new Dictionary<string, object>();
        }
        
        #endregion

        #region Add Form Management

        private async void EditContextDetails(object args = null!)
        {
            var ind = 0;
            var Row = GetModelGenerator(true);
            CloneData = (T)Row.Data!;
            EditedRow = Row;
            SetDefaultValue();
            EditContext = CloneData != null ? new EditContext(CloneData) : null!;
            if (Parent.EditSettings?.NewRowPosition == NewRowPosition.Bottom && EditedRow.Action == EditAction.Added)
            {
                Row.IsAddedBottom = true;
                ind = Parent.Rows.Count;
                EditedRow.Index = Parent.AllowPaging ? (Parent.PageSettings?.PageSize > ind ? ind : ind - 1) : ind;
            }
            else
            {
                Row.IsAddedTop = true;
                if ((Parent.EnableVirtualization || Parent.EnableInfiniteScrolling) && (string)args == "Add")
                {
                    await Parent.InvokeMethod("sfBlazor.Grid.scrollIntoView", new object[] { Parent.DataId, -1, 0, -1, false, true }).ConfigureAwait(true);
                }
            }
        }

        internal void AddFormDetails(object args = null!)
        {
            Parent!.EditModule?.EditContextDetails(args);
            Parent.SetColumnValueType();
            if (Parent.EditModule != null)
            {
                Parent.EditModule.IsAdd = true;
            }
            Parent.IsAdd = true;
        }

        #endregion

        #region Click Handling

        internal async Task<bool> InvokeSingleClickHandler(Row<object> row, Cell<object> cell)
        {
            if (Parent != null && Parent.EditSettings != null && Parent.EditSettings.Mode.Equals(EditMode.Batch) && Parent.EditSettings.AllowEditOnSingleClick && !cell.Column!.IsPrimaryKey && !(cell.Column?.Type == ColumnType.CheckBox))
            {
                await SingleClickHandler(row, cell).ConfigureAwait(true);
                return true;
            }
            return false;
        }
        internal async Task SingleClickHandler(Row<object> row, Cell<object> cell)
        {
            var keys = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            if (Parent != null && Parent.IsEdit)
            {
                await SaveCell().ConfigureAwait(true);
                await ValidateNextCell().ConfigureAwait(true);
                if (Parent.IsEdit)
                {
                    return;
                }
            }
            if (keys != null && keys.Count != 0 && keys[0].Equals(cell.Column?.Field, StringComparison.Ordinal) && !cell.IsDirty && !IsAdd)
            {
                cell.EditDisabled = true;
                return;
            }

            if (cell != null && cell.Column != null && !cell.Column.AllowEditing)
            {
                return;
            }



            await EditCell(row, cell!).ConfigureAwait(true);
        }

        internal async Task DblClickHandler(Row<object> Row, Cell<object> Cell)
        {
            if (Parent.GridEvents?.OnRecordDoubleClick.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromFileManager)
            {
                var args = new RecordDoubleClickEventArgs<T>()
                {
                    CellIndex = (int)Cell.Index!,
                    RowData = (T)Row.Data!,
                    RowIndex = (int)Row.Index!,
                    Column = Cell.Column,
                    Parent = Parent
                };

                if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromFileManager)
                {
                    await Parent.EventAggregator.NotifyAsync("DoubleClick", args).ConfigureAwait(true);
                }
                else
                    await (Parent.GridEvents?.OnRecordDoubleClick.InvokeAsync(args)!).ConfigureAwait(true);

            }

            if (Parent.EditSettings != null && Parent.EditSettings.AllowEditing && Parent.EditSettings.AllowEditOnDblClick)
            {
                if (Parent.EditSettings.Mode != EditMode.Batch)
                {
                    await StartEdit(Row).ConfigureAwait(true);
                }
                else
                {
                    if (IsAdd && ErrorResult.Count != 0 && Parent.IsEdit)
                    {
                        return;
                    }
                    if (Parent.SelectionModule != null)
                    {
                        Parent.SelectionModule.IsBatchModeDoubleClick = true;
                    }
                    await EditCell(Row, Cell).ConfigureAwait(true);
                    if (Parent.SelectionModule != null)
                    {
                        Parent.SelectionModule.IsBatchModeDoubleClick = false;
                    }
                    if (Cell.Column?.IsPrimaryKey == true && Parent.EditSettings.Mode.Equals(EditMode.Batch) && Parent.FocusModule != null)
                    {
                        await Parent.FocusModule.Focus(Row.Uid!, Cell.Uid, cellColIndex: Cell.Index + 1 ?? -1).ConfigureAwait(true);
                    }
                }
            }
        }

        /// <summary>
        /// Handles cell click behavior when grid is in edit mode
        /// </summary>
        /// <param name="row">The row that was clicked</param>
        /// <param name="cell">The cell that was clicked</param>
        /// <returns>True if the calling code should return early, false otherwise</returns>
        internal async Task<bool> HandleCellClickInEditMode(Row<object> row, Cell<object> cell)
        {
            if (Parent.IsEdit)
            {
                if (Parent.EditSettings != null && Parent.EditSettings.Mode != EditMode.Batch)
                {
                    if (row.IsEdit != true)
                    {
                        Parent.IsCellClicked = true;
                        if (Parent.FocusModule != null)
                        {
                            Parent.FocusModule.SetCurrent(row, cell, outline: true);
                        }
                        var addedRow = Parent.EditModule!.IsAdd;
                        Parent.EditModule.EditRowIndex = row.Index;
                        await Parent.EditModule.EndEdit(cell, true).ConfigureAwait(true);
                        Parent.IsCellClicked = false;
                        if (Parent.FocusModule != null && addedRow)
                        {
                            Parent.FocusModule.SetCurrent(row, cell, outline: true);
                        }
                    }
                    return true;
                }
                else
                {
                    await Parent.EditModule!.SaveCell().ConfigureAwait(true);
                    if (!Parent.IsEdit)
                    {
                        await Parent.EditModule.ValidateNextCell().ConfigureAwait(true);
                    }

                    if (Parent.EditModule != null && Parent.EditModule.ErrorResult?.Count > 0)
                    {
                        return true;
                    }
                }
            }
            else if (Parent.EditSettings != null && Parent.EditModule != null && Parent.EditSettings.Mode == EditMode.Batch)
            {
                Parent.EditModule.EditRowIndex = row.Index;
            }

            return false;
        }

        internal async Task HandleContentClickEdit()
        {
            if (Parent.PreventEndEdit && Parent.EditSettings?.Mode == EditMode.Normal && !Parent.EditSettings.ShowAddNewRow)
            {
                Parent.PreventEndEdit = false;
                return;
            }
            if (Parent.IsEdit && Parent.EditSettings?.Mode != EditMode.Batch && !Parent.EditSettings!.ShowAddNewRow)
            {
                await EndEdit().ConfigureAwait(true);
            }
            else if (Parent.IsEdit)
            {
                await SaveCell().ConfigureAwait(true);
                if (!Parent.IsEdit)
                {
                    await ValidateNextCell().ConfigureAwait(true);
                }
            }
            else
            {
                Parent.PreventRender();
            }
        }

        #endregion

        #region Focus Handling

        private void NextCellFocus(object args) => NextCellFocused(args).GetAwaiter();

        private void CurrentFocused(object args) => CurrentFocus(args).GetAwaiter();

        internal async Task FocusEditableCell(bool frozenEdit = false)
        {
            var focusCell = string.Empty;
            foreach (var col in Parent.Columns!)
            {
                if (!(string.IsNullOrEmpty(col.Field) || col.Type.Equals(ColumnType.CheckBox)) && isEditable(col))
                {
                    focusCell = col.Field;
                    break;
                }
            }

            Parent.FocusEditableCellArgs = new
            {
                EditableCellIsAdd = IsAdd,
                EditableCellField = focusCell,
                EditableCellFrozenEdit = frozenEdit
            };
            if (Parent.IsClientInitialized)
            {
                await Task.Yield();
                await Parent.InvokeMethod("sfBlazor.Grid.focusCell", Parent.DataId, focusCell, IsAdd, frozenEdit).ConfigureAwait(true);
            }
        }

        private async Task CurrentFocus(object args)
        {
            if (!Parent.EditSettings!.Mode.Equals(EditMode.Batch))
            {
                return;
            }

            BeforeCellFocus? focus = (args as BeforeCellFocus)!;
            var Key = focus?.KeyCombination;
            if (focus?.Cell?.IsEdit != true && focus?.Cell?.EditDisabled != true)
            {
                EditNextCell = false;
                if (Key != null && Key.Equals("F2", StringComparison.Ordinal) && focus != null && focus.KeyArgs != null && !focus.KeyArgs.CtrlKey && !focus.KeyArgs.ShiftKey && !focus.KeyArgs.AltKey && Parent.EditSettings.AllowEditing)
                {
                    await Parent.FocusModule!.ClearFocus(focus.Row, focus.Cell).ConfigureAwait(true);
                    await EditCell(focus.Row!, focus.Cell!).ConfigureAwait(true);
                }

                return;
            }

            if (Key?.Equals("Escape", StringComparison.Ordinal) == true)
            {
                focus.Cancel = true;
            }

            EditNextCell = true;
            var lastRow = Parent.Rows?.LastOrDefault();
            var lastRowCell = lastRow?.Cells?.FindAll(_ => _.Visible).Last();
            ForceValidate = true;
            KeyPressed = new List<string> { "Tab", "ShiftTab", "Enter", "ShiftEnter" }.Contains(Key!);
            if (!KeyPressed)
            {
                KeyPressed = false;
                return;
            }

            if (EditContext != null && !EditContext.Validate())
            {
                ForceValidate = false;
                var result = ErrorResult?.Find(_ => _.FieldName == focus.Cell?.Column?.Field);
                if (result != null)
                {
                    focus.Cancel = true;
                    if (!position.ContainsKey(GetComplexName(result.FieldName!)))
                    {
                        await InvokeValidation(new List<ValidationResult>() { result }).ConfigureAwait(true);
                    }

                    await Parent.InvokeMethod("sfBlazor.Grid.focusCell", Parent.DataId, focus.Cell?.Column?.Field, IsAdd).ConfigureAwait(true);
                }
            }

            ForceValidate = false;
            var VisibleCells = focus.Row?.Cells?.FindAll(_ => _.Visible);
            if (Key?.Equals("Tab", StringComparison.Ordinal) == true)
            {
                if (VisibleCells != null && VisibleCells.LastOrDefault()!.Equals(focus.Cell))
                {
                    bool focusLastGridCell = (focus.Row == lastRow && focus.Cell == lastRowCell);
                    bool isLastRowCell = lastRowCell?.Equals(focus.Cell) == true;
                    if (ErrorResult?.Count == 0 && isLastRowCell)
                    {
                        ShouldPreventEditFormRender = true;
                    }
                    await SaveCell(focusLastGridCell: focusLastGridCell).ConfigureAwait(true);
                    if (isLastRowCell)
                    {
                        Parent.FocusModule?.ClearCurrent();
                        ShouldPreventEditFormRender = false;
                    }

                    ClearSelection = !IsPersistSelection();
                    await Parent.SelectRowAsync((int)focus.Row!.Index! + 1).ConfigureAwait(true);
                }
            }
            else if (Key?.Equals("ShiftTab", StringComparison.Ordinal) == true)
            {
                if (VisibleCells != null && VisibleCells.FirstOrDefault()!.Equals(focus.Cell))
                {
                    if (!Parent.EditSettings.AllowNextRowEdit)
                    {
                        await SaveCell().ConfigureAwait(true);
                    }

                    await Parent.SelectRowAsync((int)(focus.Row?.Index ?? 0) - 1).ConfigureAwait(true);
                }
            }
        }

        internal async Task NextCellFocused(object args)
        {
            CellFocused focus = (args as CellFocused)!;
            var Key = focus?.KeyCombination;
            var VisibleCells = focus!.Row?.Cells?.FindAll(_ => _.Visible);
            if (!EditNextCell || (Key == "Tab" && VisibleCells?.FirstOrDefault()?.Equals(focus?.Cell) == true) ||
                (Key == "ShiftTab" && VisibleCells?.LastOrDefault()?.Equals(focus?.Cell) == true))
            {

                if (!Parent.EditSettings!.AllowNextRowEdit || (!EditNextCell && Parent.EditSettings.AllowNextRowEdit))
                {
                    return;
                }
            }

            switch (Key)
            {
                case "Tab":
                case "ShiftTab":
                    if (focus?.Row != null && focus?.Cell != null)
                    {
                        await EditCell(focus.Row, focus.Cell, focusFirstCellOnShiftTab: focus.Row.Cells[0] == focus.Cell).ConfigureAwait(true);
                        if (Parent.EditSettings!.Mode.Equals(EditMode.Batch))
                        {
                            focus.PreventDOMFocus = true;
                        }
                    }
                    break;
                case "Enter":
                case "ShiftEnter":
                    if (focus?.Cell?.IsEdit == true)
                    {
                        await SaveCell().ConfigureAwait(true);
                    }
                    else if (focus?.Row != null && focus?.Cell != null)
                    {
                        await EditCell(focus.Row, focus.Cell).ConfigureAwait(true);
                    }
                    if (Parent.EditSettings!.Mode.Equals(EditMode.Batch) && focus != null)
                    {
                        focus.PreventDOMFocus = true;
                    }
                    break;
            }
        }


        #endregion

        #region Complex Field Analysis

        private static ValueTuple<object, Type> ComplexFieldIsDynamicObject(ValueTuple<object, Type> values, int i, int Complex, string[] Fields, object data)
        {
            object dynamicData = values.Item1;
            Type type = values.Item2;

            if (i != 0)
            { // Generic with dynamic complex data
                dynamicData = DataUtil.GetDynamicValue((dynamicData != null ? dynamicData as DynamicObject : data?.GetType().GetProperty(Fields?[i - 1]!)?.GetValue(data) as DynamicObject)!, Fields?[i]!);
                if (i == (Complex - 1) && dynamicData != null)
                {
                    Type dynamicDataType = dynamicData.GetType();
                    if (dynamicDataType.IsValueType)
                    {
                        dynamicDataType = typeof(Nullable<>).MakeGenericType(dynamicDataType);
                    }

                    type = dynamicDataType;
                }
            }
            else
            {
                dynamicData = DataUtil.GetDynamicValue((dynamicData != null ? dynamicData as DynamicObject : data as DynamicObject)!, Fields?[i]!);

                if (i == (Complex - 1) && dynamicData != null)
                {
                    Type dynamicDataType = dynamicData.GetType();
                    if (dynamicDataType.IsValueType)
                    {
                        dynamicDataType = typeof(Nullable<>).MakeGenericType(dynamicDataType);
                    }

                    type = dynamicDataType;
                }
                else
                {
                    values.Item1 = dynamicData!;
                    type = typeof(object);
                }
            }
            values.Item2 = type;
            return values;
        }

        private static ValueTuple<object, IDictionary<string, Type>, Type> ComplexFieldIsExpandoObject(ValueTuple<object, IDictionary<string, Type>, Type> values, int i, string[] Fields, GridColumn Column, object data)
        {
            object customData = values.Item1;
            IDictionary<string, Type> dynamicType = values.Item2;
            Type type = values.Item3;

            if (i != 0)
            { // Generic with expandoObject complex data
                customData = customData != null ? (customData is ExpandoObject ? DataUtil.GetExpandoValue((customData as ExpandoObject)!, Fields[i - 1]) : customData.GetType().GetProperty(Fields[i - 1])?.GetValue(customData))! : data.GetType().GetProperty(Fields[i - 1])?.GetValue(data)!;
                dynamicType = customData != null ? DataUtil.GetColumnType(new List<object>() { customData }, true) : null!;
                if (dynamicType != null && dynamicType.TryGetValue(Fields[i], out Type? value))
                {
                    type = value;
                }
                values.Item1 = customData!;
                values.Item2 = dynamicType!;
            }
            else
            {
                var expandoData = (IDictionary<string, object>)data;
                var expandoValue = DataUtil.GetObject(Column.Field, expandoData);
                type = expandoValue != null ? expandoValue.GetType() : typeof(object);
                if (type.IsValueType)
                {
                    type = typeof(Nullable<>).MakeGenericType(type);
                }
            }
            values.Item3 = type;

            return values;

        }

        private static Type? DataIsExpandoOrDynamic(Row<object> Row, Type actualType, Type gridType, GridColumn Column)
        {
            if (!(Row?.Data is ExpandoObject || Row?.Data is DynamicObject))
            {
                actualType = gridType.GetProperty(Column.Field)?.PropertyType!;
            }
            return actualType;
        }

        internal static string GetComplexName(string Field) => Field.Replace(".", "___", StringComparison.Ordinal);

        #endregion

        #region Internal Helper Methods

        internal async Task HandleEditStateBeforeRowReorder(RowReorder<T> rowReorderModule)
        {
            if (Parent.IsEdit)
            {
                if (Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Normal && Parent.Rows?.Any(x => x.Action == EditAction.Added) == true)
                {
                    rowReorderModule.HasAddedRecord = true;
                    await CloseEdit().ConfigureAwait(true);
                }
                else if (Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Batch)
                {
                    await SaveCell().ConfigureAwait(true);
                    if (ErrorResult.Count > 0)
                    {
                        return;
                    }
                }
            }
        }

        internal async Task<bool> HandleBatchChangesWithConfirmDialog(ActionEventArgs<T>? args, object? additionalArgument, string? requestType)
        {
            if (Parent.EditSettings != null && Parent.EditSettings.ShowConfirmDialog)
            {
                if (args != null && requestType == "Reorder")
                {
                    IsBatchReorderPending = true;
                }

                await ShowAlertDialog(args!, additionalArgument as ActionArgs).ConfigureAwait(true);
                return true;
            }
            if (requestType == "Reorder")
            {
                await PerformBatchActions(args!, additionalArgument as ActionArgs).ConfigureAwait(true);
                return true;
            }
            await PerformBatchActions(args!).ConfigureAwait(true);
            return false;
        }

        internal void HandleVirtualScrollEditState(List<Row<object>> addRow, List<Row<object>> currentRows, int rowStartIndex, int rowEndIndex, bool isDataSourceChanged, int i)
        {
            if (Parent.EditSettings != null && Parent.EditSettings.Mode.Equals(EditMode.Normal))
            {
                if (!Parent.IsEdit && addRow[0].IsEdit)
                {
                    addRow[0].IsEdit = false;
                }
                else if (Parent.IsEdit && addRow[0].Index == EditedRow?.Index && !IsAdd)
                {
                    addRow[0].IsEdit = Parent.Rows?.FirstOrDefault(x => x.Data == addRow[0].Data)?.IsEdit ?? true;
                }
            }
            currentRows.Add(addRow[0]);
            if (Parent.IsEdit && IsAdd && Parent.EditSettings != null && Parent.EditSettings.Mode.Equals(EditMode.Normal)
                && !isDataSourceChanged && EditedRow != null)
            {
                EditedRow.IsEdit = true;
                if (Parent.EditSettings.NewRowPosition == NewRowPosition.Top && rowStartIndex == 0
                    && i == 0)
                {
                    EditedRow.IsAddedTop = true;
                    currentRows.Insert(0, EditedRow);
                }
                else if (Parent.EditSettings.NewRowPosition == NewRowPosition.Bottom
                    && rowEndIndex == Parent.TotalItemCount && i == Parent.TotalItemCount - 1)
                {
                    EditedRow.IsAddedBottom = true;
                    currentRows.Add(EditedRow);
                }
            }
        }

        internal async Task HandleContextMenuEdit(object targetRowdata, Row<object> rowObject, GridColumn targetColumn)
        {
            Parent.EventAggregator.Trigger("UpdateEditMode", null!);
            if (Parent.EditSettings!.Mode == EditMode.Batch)
            {
                if (targetRowdata != null && rowObject != null && targetColumn != null)
                {
                    await Parent.EditCellAsync((int)rowObject.Index!, targetColumn.Field).ConfigureAwait(true);
                }
            }
            else
            {
                if (!Parent.EditSettings.ShowAddNewRow)
                {
                    await Parent.EndEditAsync().ConfigureAwait(true);
                }
                await StartEdit(rowObject).ConfigureAwait(true);
            }
        }

        internal async Task HandleContextMenuDelete(GridColumn targetColumn, T? targetRowdata)
        {
            if (Parent.EditSettings!.Mode != EditMode.Batch && !Parent.EditSettings.ShowAddNewRow)
            {
                await Parent.EndEditAsync().ConfigureAwait(true);
            }
            if (Parent.SelectedRecords.Count == 1)
            {
                await Parent.DeleteRecordAsync(targetColumn.Field, targetRowdata!).ConfigureAwait(true);
            }
            else
            {
                await Parent.DeleteRecordAsync().ConfigureAwait(true);
            }
        }

        internal Row<object>? GetSelectedRowForEdit()
        {
            Row<object> Row = new Row<object>();
            if (Parent.SelectedRecords.Count > 0)
            {
                var selectedRowIndex = Parent.Rows.Where(_ => _.IsSelected).Select(x => x.Index).FirstOrDefault();
                if (selectedRowIndex != null && selectedRowIndex > -1)
                {
                    Row = Parent.Rows.Find(_ => _.Index == selectedRowIndex)!;
                }
                else
                {
                    Row = Parent.SelectionModule!.SelectedRow()!;
                }
            }
            else
            {
                AlertMessage = "EditAlert";
                Parent.EventAggregator.Trigger("ShowValidationDialog", null!);
                return null;
            }

            return Row;
        }

        internal (List<string> EnableItems, List<string> DisableItems) GetToolbarItemStates()
        {
            var EnableItems = new List<string>();
            var DisableItems = new List<string>();
            var Edit = Parent.EditSettings;
            var HasData = Parent.TotalItemCount > 0;

            if (Edit != null && Edit.AllowAdding && !Parent.EditSettings!.ShowAddNewRow)
            {
                EnableItems.Add("Add");
            }
            else
            {
                DisableItems.Add("Add");
            }

            if (Edit != null && Edit.AllowEditing && HasData)
            {
                EnableItems.Add("Edit");
            }
            else
            {
                DisableItems.Add("Edit");
            }

            if (Edit != null && Edit.AllowDeleting && HasData)
            {
                EnableItems.Add("Delete");
            }
            else
            {
                DisableItems.Add("Delete");
            }

            if (Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Batch)
            {
                if (HasBatchChanges && (Edit!.AllowAdding || Edit.AllowEditing || Edit.AllowDeleting))
                {
                    EnableItems.Add("Update");
                    EnableItems.Add("Cancel");
                }
                else
                {
                    DisableItems.Add("Update");
                    DisableItems.Add("Cancel");
                }

                // Add Undo/Redo toolbar button states for Batch Edit mode
                if (Edit != null && Edit.EnableUndoRedo && Parent.UndoRedoManager != null)
                {
                    if (Parent.UndoRedoManager.IsUndoAvailable)
                    {
                        EnableItems.Add("Undo");
                    }
                    else
                    {
                        DisableItems.Add("Undo");
                    }

                    if (Parent.UndoRedoManager.IsRedoAvailable)
                    {
                        EnableItems.Add("Redo");
                    }
                    else
                    {
                        DisableItems.Add("Redo");
                    }
                }
                else
                {
                    DisableItems.Add("Undo");
                    DisableItems.Add("Redo");
                }
            }
            else
            {
                if (Parent.IsEdit && (Edit!.AllowAdding || Edit.AllowEditing))
                {
                    EnableItems = new List<string>() { "Update", "Cancel" };
                    DisableItems = new List<string>() { "Add", "Edit", "Delete" };
                }
                else if (!Parent.EditSettings!.ShowAddNewRow)
                {
                    DisableItems.Add("Update");
                    DisableItems.Add("Cancel");
                }

                // Undo/Redo not supported in Normal or Dialog mode
                DisableItems.Add("Undo");
                DisableItems.Add("Redo");
            }

            return (EnableItems, DisableItems);
        }

        internal async Task EditCellByIndexAndField(int index, string fieldName)
        {
            var row = Parent.Rows?.Find(_ => _.Index == index);
            var cell = row?.Cells?.Find(_ => _.Column!.Field == fieldName);
            if (Parent.EditSettings != null && Parent.EditSettings.Mode.Equals(EditMode.Batch) &&
                Parent.EditSettings.AllowEditing && ((cell?.Column?.AllowEditing ?? false) || (cell?.IsDirty == true && row?.Action == EditAction.Added)))
            {
                if (row != null && cell != null && Parent.SelectionModule != null)
                {
                    Parent.SelectionModule.IsBatchModeDoubleClick = true;
                    await Task.Yield();
                    await EditCell(row, cell).ConfigureAwait(true);
                    Parent.SelectionModule.IsBatchModeDoubleClick = false;
                }
            }
        }

        #endregion

        #region Row Index Management

        private int GetRowIndex()
        {
            bool isBottomAdd = IsAdd && Parent.EditSettings!.NewRowPosition.Equals(NewRowPosition.Bottom);
            if (isBottomAdd && Parent.Rows?.Count != 0 && !Parent.EnableVirtualization)
            {
                int dataRowsCount = Parent.Rows!.Count(x => x.IsDataRow && !x.IsDetailRow);
                int TotalIndexCount = dataRowsCount - 1 + (Parent.EditSettings!.ShowAddNewRow ? 1 : 0);
                if (Parent.AllowPaging && Parent.PageSettings != null)
                {
                    return Math.Min(TotalIndexCount, Parent.PageSettings.PageSize - 1);
                }
                return TotalIndexCount;
            }
            return isBottomAdd && Parent.EnableVirtualization ? 0 : (int)EditedRow!.Index!;
        }

        internal void RefreshRowIndex()
        {
            var Index = 0;
            Parent.Rows?.ForEach(Row =>
            {
                Row.Index = Index;
                Index++;
            });
        }

        #endregion

        #region Edit State Management
        internal async Task StartEdit(Row<object> Row)
        {
            if (!Parent.IsEdit && Parent.EditSettings!.AllowEditing && ErrorResult.Count == 0)
            {
                var startArgs = new ActionEventArgs<T>()
                {
                    Type = "ActionBegin",
                    RequestType = Action.BeforeBeginEdit,
                    PreventDataClone = false,
                    Parent = Parent
                };
                var startEditargs = new OnRowEditStartEventArgs()
                {
                    Cancel = false,
                    PreventDataClone = false,
                    Parent = Parent
                };
                await SfBaseUtils.InvokeEvent<ActionEventArgs<T>>(Parent.GridEvents?.OnActionBegin, startArgs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("ActionBegin", startArgs).ConfigureAwait(true);
                await SfBaseUtils.InvokeEvent<OnRowEditStartEventArgs>(Parent.GridEvents?.OnRowEditStart, startEditargs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("BeforeRowEditing", startEditargs).ConfigureAwait(true);
                if (startEditargs.Cancel)
                {
                    return;
                }
                Parent.EditModule!.IsAdd = false;
                CloneRowData(Row.Data!, startArgs.PreventDataClone || startEditargs.PreventDataClone);
                EditedRow = Row;
                IsLastRow = Parent.Rows?.OrderByDescending(x => x.Index).FirstOrDefault() == Row;
                var primaryKeys = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
                var primaryKeyValues = await GetPrimaryKeyValue(CloneData!).ConfigureAwait(true);
                EditContext = CloneData != null ? new EditContext(CloneData) : null!;
                var args = new BeginEditArgs<T>()
                {
                    PrimaryKey = primaryKeys?.ToArray()!,
                    PrimaryKeyValue = primaryKeyValues?.ToArray()!,
                    RowData = (T)CloneData!,
                    RowIndex = (int)Row.Index!,
                    Cancel = false,
                    Parent = Parent
                };
                await SfBaseUtils.InvokeEvent<BeginEditArgs<T>>(Parent.GridEvents?.OnBeginEdit, args).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("BeginEdit", args).ConfigureAwait(true);
                var actionArgs = new ActionEventArgs<T>()
                {
                    PrimaryKeys = args.PrimaryKey,
                    PrimaryKeyValue = args.PrimaryKeyValue,
                    RowData = (T)Row.Data!,
                    Data = args.RowData,
                    RowIndex = args.RowIndex,
                    Cancel = args.Cancel,
                    Type = "ActionBegin",
                    PreviousData = (T)Row.Data!,
                    RequestType = Action.BeginEdit,
                    ForeignKeyData = Row.ForeignKeyData,
                    EditContext = EditContext,
                    Parent = Parent
                };

                var editingEventArgs = new RowEditingEventArgs<T>()
                {
                    PrimaryKeys = args.PrimaryKey,
                    PrimaryKeyValue = args.PrimaryKeyValue,
                    Data = args.RowData,
                    Index = args.RowIndex,
                    Cancel = args.Cancel,
                    ForeignKeyData = Row.ForeignKeyData,
                    EditContext = EditContext,
                    Parent = Parent
                };
                await SfBaseUtils.InvokeEvent<ActionEventArgs<T>>(Parent.GridEvents?.OnActionBegin, actionArgs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("ActionBegin", actionArgs).ConfigureAwait(true);
                await SfBaseUtils.InvokeEvent<RowEditingEventArgs<T>>(Parent.GridEvents?.RowEditing, editingEventArgs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("RowEditing", editingEventArgs).ConfigureAwait(true);
                if (actionArgs.Cancel || editingEventArgs.Cancel)
                {
                    if (Parent.EditSettings.ShowAddNewRow)
                    {
                        Parent.EventAggregator.Trigger("ResetAddFormValues", null!);
                    }
                    return;
                }

                var cData = CloneData;
                EnsureDataAndEditContext(ref cData!, actionArgs, eventArgs: editingEventArgs);
                CloneData = cData;
                Parent.IsEdit = true;
                Row.IsEdit = true;
                if (Parent.EditSettings.ShowAddNewRow)
                {
                    ClearRules();
                    Parent.EventAggregator.Trigger("DisableOrEnableAddForm", null!);
                }
                Parent.PreventRender();
                if (Parent.EditSettings.Mode != EditMode.Dialog && Parent.FocusModule != null)
                {
                    Parent.FocusModule.ClearCurrent();
                    Parent.FocusModule.SetCurrent(Row, Row.Cells?.Where(e => !e.Column!.IsPrimaryKey && e.Visible && e.CellType != CellType.RowDrag && e.Column.Type != ColumnType.CheckBox && e.Column.AllowEditing).FirstOrDefault()!, false);
                    if (!IsPersistSelection())
                    {
                        await Parent.ClearSelectionAsync().ConfigureAwait(true);
                    }
                    if (Parent.Rows?.LastOrDefault() != null)
                    {
                        Parent.EventAggregator.Trigger("RowStateChanged", Parent.Rows.Last());
                    }
                    Parent.EventAggregator.Trigger("RowStateChanged", Row);
                }
                else
                {
                    Parent.EventAggregator.Trigger("ShowDialog", null!);
                }
                var firstRow = Parent.Rows?.Where(_ => _.Visible).FirstOrDefault();
                Parent.EventAggregator.Trigger("RowStateChanged", firstRow!);
                Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
                actionArgs.Type = "ActionComplete";
                await SfBaseUtils.InvokeEvent<ActionEventArgs<T>>(Parent.GridEvents?.OnActionComplete, actionArgs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("ActionComplete", actionArgs).ConfigureAwait(true);
                var editedArgs = new RowEditedEventArgs<T>()
                {
                    PrimaryKeys = editingEventArgs.PrimaryKeys,
                    PrimaryKeyValue = editingEventArgs.PrimaryKeyValue,
                    Data = editingEventArgs.Data,
                    Index = editingEventArgs.Index,
                    ForeignKeyData = editingEventArgs.ForeignKeyData,
                    EditContext = editingEventArgs.EditContext,
                    Parent = Parent
                };
                await SfBaseUtils.InvokeEvent<RowEditedEventArgs<T>>(Parent.GridEvents?.RowEdited, editedArgs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("RowEdited", editedArgs).ConfigureAwait(true);

            }
            if (Parent.EnableInfiniteScrolling)
            {
                await Parent.InvokeMethod("sfBlazor.Grid.updateOptions", new object[] { Parent.DataId, Parent.GetClientOption() }).ConfigureAwait(true);
            }
        }

        internal async Task EndEdit(Cell<object>? cell = null, bool ignoreFocus = false, string keyCombination = null!)
        {
            if (IsAdd && Parent.SelectedRecords.Count > 0 && Parent.SelectionSettings != null && !Parent.SelectionSettings.PersistSelection && Parent.EditSettings != null && !Parent.EditSettings.Mode.Equals(EditMode.Dialog))
            {
                await Parent.ClearSelectionAsync().ConfigureAwait(true);
            }
            if (Parent.EditSettings != null && Parent.EditSettings.ShowAddNewRow)
            {
                if (Parent.IsEdit)
                {
                    IsAdd = false;
                    Parent.IsAdd = true;
                }
                else
                {
                    Parent.IsEdit = true;
                }
            }
            if (Parent.EditSettings?.Mode == EditMode.Batch)
            {
                ForceValidate = true;
                if (Parent.IsEdit)
                {
                    await SaveCell().ConfigureAwait(true);
                    await ValidateNextCell().ConfigureAwait(true);
                }

                if (Parent.IsEdit)
                {
                    return;
                }

                if (!Parent.IsEdit)
                {
                    await ValidateNextCell().ConfigureAwait(true);
                }

                if (Parent.IsEdit && Parent.EditModule!.ErrorResult.Count > 0)
                {
                    return;
                }

                if (Parent.EditSettings.ShowConfirmDialog)
                {
                    Parent.SoftRefresh = true;
                    AlertMessage = "BatchSaveConfirm";
                    Parent.EventAggregator.Trigger("ShowValidationDialog", null!);
                }
                else
                {
                    await BatchSave().ConfigureAwait(true);
                }
            }
            else
            {
                Parent.FocusModule!.SelectedCellIndex = await Parent.EditModule!.GetSelectedCellIndex().ConfigureAwait(true);
                if (!Parent.IsEdit || EditContext == null)
                {
                    return;
                }
                ForceValidate = true;
                bool isTabKey = keyCombination != null && keyCombination.Equals("Tab", StringComparison.Ordinal);
                if (!EditContext.Validate())
                {
                    ForceValidate = false;
                    if (ErrorResult.Count > 0) // Handled for identity and cols hidden
                    {
                        var ValidateFields = new List<ValidationResult>();
                        foreach (var key in ErrorResult)
                        {
                            if (!position.ContainsKey(GetComplexName(key.Uid ?? key.FieldName!)))
                            {
                                ValidateFields.Add(key);
                            }
                        }
                        if (Parent.EnableInfiniteScrolling && Parent.InfiniteScrollModule != null && Parent.EditModule.IsAdd && Parent.InfiniteScrollSettings != null && Parent.InfiniteScrollSettings.EnableCache && Parent.PageSettings != null && Parent.PageSettings.CurrentPage > 1)
                        {
                            Parent.InfiniteScrollModule.RequestType = "Save";
                            await Parent.InfiniteScrollModule.ResetInfiniteProperties(Parent.InfiniteScrollModule.RequestType).ConfigureAwait(true);
                            await Parent.DataProcess().ConfigureAwait(true);
                        }

                        if (ValidateFields.Count > 0 || (Parent.EnableVirtualization && Parent.IsEdit) || (Parent.EnableInfiniteScrolling && Parent.IsEdit))
                        {
                            if (Parent.EnableInfiniteScrolling)
                            {
                                if (Parent.IsEdit && Parent.EditModule.IsAdd)
                                {
                                    await InvokeValidation(ValidateFields).ConfigureAwait(true);
                                    int editableRowIndex = Parent.EditSettings != null && Parent.EditSettings.Mode.Equals(EditMode.Normal) && Parent.EditSettings.NewRowPosition == NewRowPosition.Top ? 0 :
                                        Parent.AllowGrouping && Parent.GroupSettings != null && Parent.GroupSettings.Columns?.Length > 0 ? Parent.VirtualScrollModule!.VisibleGroupRows.Count : Parent.TotalItemCount;
                                    await Parent.InvokeMethod("sfBlazor.Grid.scrollIntoView", new object[] { Parent.DataId, -1, editableRowIndex, -1, Parent.EditSettings?.NewRowPosition == NewRowPosition.Bottom }).ConfigureAwait(true);
                                }
                                else if (Parent.GroupSettings?.Columns == null || Parent.GroupSettings.Columns.Length == 0)
                                {
                                    var editedRowIndex = Parent.Rows.FindIndex(x => x.Index == EditedRow!.Index);
                                    await Parent.InvokeMethod("sfBlazor.Grid.scrollIntoView", new object[] { Parent.DataId, -1, editedRowIndex, -1 }).ConfigureAwait(true);
                                }
                            }
                            else if ((Parent.EnableVirtualization && Parent.VirtualScrollModule != null && Parent.Rows.Find(_ => _.Index == (double)EditedRow!.Index!) == null) && !(Parent.EditSettings != null && Parent.EditSettings.ShowAddNewRow && Parent.GroupSettings?.Columns?.Length > 0))
                            {
                                await Parent.VirtualScrollModule.ScrollToEditedRowAsync(ValidateFields).ConfigureAwait(true);
                            }
                            else
                            {
                                await InvokeValidation(ValidateFields).ConfigureAwait(true);
                            }
                        }
                        if (Parent.EditSettings != null && Parent.EditSettings.ShowAddNewRow && isTabKey && EditedRow != null && IsAdd && Parent.EditSettings.NewRowPosition == NewRowPosition.Top)
                        {
                            var nextRow = Parent.Rows.FirstOrDefault(row => row.Index == EditedRow.Index);
                            var firstVisibleCell = nextRow?.Cells.FirstOrDefault(cell => cell.Visible);
                            if (nextRow != null && firstVisibleCell != null && Parent.FocusModule != null)
                            {
                                await Parent.FocusModule.Focus(nextRow.Uid!, firstVisibleCell.Uid, cellColIndex: firstVisibleCell.Index + 1 ?? -1).ConfigureAwait(true);
                            }
                        }
                        Parent.IsEdit = Parent.EditSettings!.ShowAddNewRow ? false : Parent.IsEdit; // For adding records while displaying validation tooltips on the add form
                        EditRowIndex = null;
                        return;
                    }

                    Parent.IsEdit = Parent.EditSettings!.ShowAddNewRow ? false : Parent.IsEdit;
                    return; // if removed CustomValidation will not work
                }
                ForceValidate = false;
                var Primarykeys = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
                var primaryKeyValues = await GetPrimaryKeyValue(CloneData!).ConfigureAwait(true);
                var args = new ActionEventArgs<T>()
                {
                    RowData = (T)EditedRow?.Data!,
                    Data = (T)CloneData!,
                    RowIndex = GetRowIndex(),
                    PreviousData = (T)EditedRow?.Data!,
                    RequestType = Action.Save,
                    IsShiftKeyPressed = IsShiftKey,
                    Code = KeyCode!,
                    Cancel = false,
                    PrimaryKeys = Primarykeys?.ToArray() ?? new List<String>().ToArray(),
                    PrimaryKeyValue = primaryKeyValues != null && primaryKeyValues.Count > 0 ? primaryKeyValues[0] : null!
                };
                var saveEventArgs = new RowUpdatingEventArgs<T>()
                {
                    Data = (T)CloneData!,
                    Index = GetRowIndex(),
                    PreviousData = (T)EditedRow?.Data!,
                    IsShiftKeyPressed = IsShiftKey,
                    KeyCode = KeyCode!,
                    Cancel = false,
                    PrimaryKeys = Primarykeys?.ToArray() ?? new List<String>().ToArray(),
                    PrimaryKeyValue = primaryKeyValues != null && primaryKeyValues.Count > 0 ? primaryKeyValues[0] : null!
                };
                bool isBottomNewRowPosition = Parent.EditSettings!.NewRowPosition.Equals(NewRowPosition.Bottom);
                args.Index = saveEventArgs.Index = (IsAdd && isBottomNewRowPosition) ? args.RowIndex : args.Index;
                if (IsAdd)
                {
                    if (isBottomNewRowPosition && Parent.EnableVirtualization && EditedRow != null)
                    {
                        EditedRow.Index = args.Index;
                    }
                    args.Action = "Add";
                    saveEventArgs.Action = SaveActionType.Added;
                }
                else
                {
                    args.Action = "Edit";
                    saveEventArgs.Action = SaveActionType.Edited;
                }

                await Parent.ModelChanged(args, eventArgs: saveEventArgs, requestType: "Save").ConfigureAwait(true);
                if (args.Cancel || saveEventArgs.Cancel)
                {
                    if (Parent.EditSettings.ShowAddNewRow)
                    {
                        Parent.IsEdit = args.Action == "Edit" ? true : false;
                    }
                    return;
                }

                int? rowIndex = null;
                bool isShiftTabKey = keyCombination != null && keyCombination.Equals("ShiftTab", StringComparison.Ordinal);
                bool isFirstCellFocused = !IsAdd && EditedRow!.Cells.Where(e => e.Visible && e.CellType != CellType.RowDrag).FirstOrDefault()!.IsFocused;
                bool isLastCellFocused = !IsAdd && EditedRow!.Cells.Where(e => e.Visible && e.CellType != CellType.RowDrag).LastOrDefault()!.IsFocused;
                bool isFirstRowEdited = false;
                bool isLastRowEdited = false;
                bool isCellMode = (Parent.SelectionModule != null && Parent.SelectionModule.IsCellMode());
                bool isBothMode = (Parent.SelectionModule != null && Parent.SelectionModule.IsBothMode());
                if (Parent.Rows != null && Parent.Rows.Count > 0)
                {
                    isFirstRowEdited = Parent.Rows.FirstOrDefault()!.Index == args.RowIndex;
                    isLastRowEdited = Parent.Rows.LastOrDefault()!.Index == args.RowIndex;
                }
                if (IsAdd && args.RequestType == Action.Save && isFirstRowEdited && isShiftTabKey)
                {
                    isFirstCellFocused = EditedRow!.Cells.Where(e => e.Visible).FirstOrDefault()!.IsFocused;
                }
                if (IsAdd)
                {
                    IsAdd = false;
                }
                else
                {
                    if (Parent.EditSettings.Mode != EditMode.Dialog)
                    {
                        if (Parent.SelectionSettings != null && Parent.SelectionSettings.CheckboxMode == CheckboxSelectionType.ResetOnRowClick && Parent.SelectionSettings.PersistSelection)
                        {
                            await Parent.ClearSelectionAsync().ConfigureAwait(true);

                        }
                        if (!(Parent.GroupSettings?.Columns?.Length > 0) && ((isTabKey && isLastCellFocused && !isLastRowEdited) || (isShiftTabKey && isFirstCellFocused && !isFirstRowEdited)))
                        {
                            rowIndex = isTabKey ? args.RowIndex + 1 : args.RowIndex - 1;
                            if (!Parent.SelectionSettings!.CheckboxOnly)
                            {
                                await Parent.SelectRowAsync(EditRowIndex > -1 ? (int)EditRowIndex : (int)rowIndex).ConfigureAwait(true);
                            }
                        }
                        else if (!Parent.SelectionSettings!.CheckboxOnly && Parent.SelectionModule != null)
                        {
                            await Parent.SelectionModule.SelectRowAndCell(EditRowIndex, args.RowIndex, cell!, isCellMode, isBothMode).ConfigureAwait(true);
                        }
                        EditRowIndex = null;
                        if (isTabKey && isLastRowEdited)
                        {
                            Parent.FocusModule?.ClearCurrent();
                            Parent.FocusModule!.ChangeLastCellTabIndex = true;
                            Parent.EventAggregator.Trigger("RowStateChanged", Parent.Rows?.Last()!);
                            if (Parent.EditSettings.ShowAddNewRow)
                            {
                                Parent.EventAggregator.Trigger("DisableOrEnableAddForm", null!);
                                Parent.EventAggregator.Trigger("ResetAddFormValues", null!);
                            }
                            return;
                        }
                    }
                    else if (Parent.EditSettings.Mode == EditMode.Dialog && !Parent.SelectionSettings!.CheckboxOnly && Parent.SelectionModule != null && EditedRow != null)
                    {
                        await Parent.SelectionModule.SelectRowAndCell(EditRowIndex, args.RowIndex, cell!, isCellMode, isBothMode).ConfigureAwait(true);
                    }                    
                }

                var selectedRow = keyCombination != null && rowIndex != null ? Parent.Rows!.Find(_ => _.Index == rowIndex) : Parent.Rows!.Find(_ => _.Index == args.RowIndex);
                var cellIndex = rowIndex != null && isShiftTabKey && selectedRow != null ? selectedRow.Cells.Where(e => e.Visible).LastOrDefault()!.Index : Parent.FocusModule!.SelectedCellIndex;
                bool isFilterBar = Parent.AllowFiltering && Parent.FilterSettings?.Type == FilterType.FilterBar;
                bool isFirstContentCellFocused = isFirstRowEdited && isFirstCellFocused;

                if (selectedRow != null && EditedRow != null && EditedRow.Cells.Where(e => e.Visible && e.IsFocused).Any())
                {
                    int? focusedCellIndex = EditedRow.Cells.Where(e => e.Visible && e.IsFocused).First().Index;
                    if (!isFirstCellFocused && isShiftTabKey && focusedCellIndex != null)
                    {
                        cellIndex = focusedCellIndex - 1;
                    }
                    else if (!isLastCellFocused && isTabKey && focusedCellIndex != null && !(focusedCellIndex + 1 >= selectedRow.Cells.Count))
                    {
                        cellIndex = focusedCellIndex + 1;
                    }

                    if (isFirstRowEdited && isFirstCellFocused && isShiftTabKey && !isFilterBar)
                    {
                        selectedRow = Parent.FocusModule?.HeaderRows.LastOrDefault();
                        cellIndex = selectedRow?.Cells.Where(e => e.Visible).LastOrDefault()?.Index;
                    }

                    if (isFirstRowEdited && isFirstCellFocused && isShiftTabKey && isFilterBar)
                    {
                        Parent.FocusModule?.ClearCurrent();
                        bool isFilterTemplate = Parent.Columns!.Where(e => e.Visible).Any() && Parent.Columns!.Where(e => e.Visible).LastOrDefault()?.FilterTemplate != null;
                        await Parent.InvokeMethod("sfBlazor.Grid.focusFilterBar", new object[] { Parent.DataId, keyCombination!, isFilterTemplate, -1 }).ConfigureAwait(true);
                    }

                    if (!(Parent.GroupSettings?.Columns?.Length > 0) && cellIndex != null && !ignoreFocus && !(isFirstContentCellFocused && isFilterBar && isShiftTabKey))
                    {
                        if ((isShiftTabKey || isTabKey) && !Parent.SelectionSettings!.CheckboxOnly && (isCellMode || isBothMode) && Parent.SelectionModule != null)
                        {
                            await Parent.SelectionModule.SelectCellByRow(selectedRow!, (int)cellIndex).ConfigureAwait(true);
                        }
                    }
                }

                if (selectedRow != null && cellIndex != null && !ignoreFocus && args.Action != "Add" && !(isFirstContentCellFocused && isFilterBar && isShiftTabKey))
                {
                    var cellObject = selectedRow.Cells.Where(_ => _.Index == (int)cellIndex).FirstOrDefault();
                    await Parent.FocusModule!.Focus(selectedRow.Uid!, cellObject?.Uid!, "UpdateRecord", cellColIndex: cellObject?.Index + 1 ?? -1).ConfigureAwait(true);
                    if ((!(cellObject != null && cellObject.CellType.Equals(CellType.CommandColumn) || (cellObject != null && cellObject.IsTemplate))) && Parent.FocusModule.IsChildFocused)
                    {
                        Parent.FocusModule.IsChildFocused = false;
                    }

                }
                if (Parent.EditSettings.ShowAddNewRow)
                {
                    Parent.EventAggregator.Trigger("DisableOrEnableAddForm", null!);
                    Parent.EventAggregator.Trigger("ResetAddFormValues", args.Action);
                }
                if (ErrorResult.Count > 0)
                {
                    ClearRules();
                }
            }
        }

        internal async Task CloseEdit(bool escapeKey = false)
        {
            Parent.PreventRender(false);
            bool isFromAddForm = false;
            Parent.FocusModule!.SelectedCellIndex = await Parent.EditModule!.GetSelectedCellIndex().ConfigureAwait(true);
            if (Parent.EditSettings != null && Parent.EditSettings.ShowAddNewRow)
            {
                if (!Parent.IsEdit)
                {
                    isFromAddForm = ErrorResult.Count == 0 || IsAdd;
                    Parent.IsEdit = true;
                }
            }
            if (Parent.EditSettings != null && Parent.EditSettings.Mode == EditMode.Batch)
            {
                if (Parent.EditSettings.ShowConfirmDialog && HasBatchChanges && !escapeKey)
                {
                    AlertMessage = "CancelEdit";
                    if (Parent.IsEdit)
                    {
                        await SaveCell().ConfigureAwait(true);
                    }

                    Parent.EventAggregator.Trigger("ShowValidationDialog", null!);
                    return;
                }

                await BatchClose(escapeKey).ConfigureAwait(true);
            }
            else
            {
                if (!Parent.IsEdit)
                {
                    return;
                }
                var row = Parent.Rows?.Find(_ => _.IsEdit);
                if (row != null && row.IsAddedBottom)
                {
                    row.Index = Parent.Rows!.IndexOf(row) - 1;
                }
                var args = new ActionEventArgs<T>()
                {
                    RequestType = Action.Cancel,
                    Type = "ActionBegin",
                    Parent = Parent,
                    Data = (T)CloneData!,
                    RowData = (T)row?.Data!,
                    PreviousData = (T)row?.Data!,
                    RowIndex = (int)(row?.Index ?? -1),
                    PrimaryKeys = (await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true))?.ToArray()!
                };
                var cancelEventArgs = new EditCancelingEventArgs<T>()
                {
                    Data = (T)CloneData!,
                    PreviousData = (T)row?.Data!,
                    Index = (int)(row?.Index ?? -1),
                    PrimaryKeys = (await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true))?.ToArray()!,
                    Parent = Parent
                };
                await SfBaseUtils.InvokeEvent<ActionEventArgs<T>>(Parent.GridEvents?.OnActionBegin, args).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("ActionBegin", args).ConfigureAwait(true);
                await SfBaseUtils.InvokeEvent<EditCancelingEventArgs<T>>(Parent.GridEvents?.EditCanceling, cancelEventArgs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("Cancelling", cancelEventArgs).ConfigureAwait(true);
                if (args.Cancel || cancelEventArgs.Cancel)
                {
                    return;
                }
                if (Parent.EditSettings?.Dialog != null && Parent.EditSettings.Dialog.AnimationEffect != null)
                {
                    await (EditDialogInstance?.HideAsync())!.ConfigureAwait(true)!;
                    await Task.Delay(250).ConfigureAwait(true);
                }

                ClearRules();
                if (IsAdd && Parent.EditSettings!.Mode.Equals(EditMode.Normal))
                {
                    Parent.Rows?.Remove(row!);
                }
                var selectedRecordIndex = Parent.Rows?.Where(_ => _.IsSelected)?.Select(x => x.Index).FirstOrDefault();
                var selectedRowIndex = selectedRecordIndex != null || (GridUtils.GetColumns(Parent).Any(x => x.Type == ColumnType.CheckBox) && IsAdd) ? selectedRecordIndex : (row?.Index ?? -1);
                if (Parent.IsRenderedFromTreeGrid)
                {
                    selectedRowIndex = selectedRecordIndex != null || (GridUtils.GetColumns(Parent).Any(x => x.Type == ColumnType.CheckBox) && IsAdd) ? selectedRecordIndex : (Parent.FocusModule!.SelectedRowIndex);
                }
                if (Parent.EnableVirtualization && IsAdd)
                {
                    Parent.VirtualScrollModule!.HasAddOrCancelAction = Parent.VirtualScrollModule.IsBottomAddForm(Parent.VirtualScrollModule.RowEndIndex);
                }
                IsAdd = false;
                Parent.IsEdit = false;
                Parent.Rows?.ForEach(Row => Row.IsEdit = false);
                args.Type = "ActionComplete";
                if (Parent.EditSettings != null && Parent.EditSettings.Mode.Equals(EditMode.Dialog))
                {
                    Parent.EventAggregator.Trigger("ShowDialog", null!);
                }
                else
                {
                    if (Parent.IsEdit && !IsPersistSelection())
                    {
                        await Parent.SelectRowAsync(row?.Index ?? -1).ConfigureAwait(true);
                    }
                    else if (!IsPersistSelection() && !isFromAddForm && !IsCheckBoxOnly())
                    {
                        IsCancelAction = true;
                        await Parent.SelectRowAsync(selectedRowIndex ?? -1).ConfigureAwait(true);
                        IsCancelAction = false;
                    }
                    if (Parent.FrozenRows > 0)
                    {
                        Parent.EventAggregator.Trigger("HeaderStateChanged", null!);
                    }
                    else if (Parent.EnableVirtualization && Parent.VirtualScrollModule != null)
                    {
                        Parent.EventAggregator.Trigger("VirtualComponentUpdate", null!);
                    }
                    else
                    {
                        Parent.EventAggregator.Trigger("ContentStateChanged", null!);
                    }
                }

                Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
                await SfBaseUtils.InvokeEvent<ActionEventArgs<T>>(Parent.GridEvents?.OnActionComplete, args).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("ActionComplete", args).ConfigureAwait(true);
                await SfBaseUtils.InvokeEvent<EditCanceledEventArgs<T>>(Parent.GridEvents?.EditCanceled, cancelEventArgs).ConfigureAwait(true);
                await Parent.EventAggregator.NotifyAsync("Cancelled", cancelEventArgs).ConfigureAwait(true);
                bool isFocused = await Parent.InvokeMethod<bool>("sfBlazor.Grid.focusNextFrame", false, new object[] { Parent.DataId }).ConfigureAwait(true);
                var cellIndex = Parent.FocusModule?.SelectedCellIndex;
                if (cellIndex != null && selectedRowIndex != null && !isFromAddForm)
                {
                    var selectedRow = Parent.Rows?.Find(_ => _.Index == selectedRowIndex);
                    if (selectedRow != null && Parent.FocusModule != null)
                    {
                        await Parent.FocusModule.Focus(selectedRow.Uid!, selectedRow.Cells[(int)cellIndex].Uid, "CancelEdit", cellColIndex: selectedRow.Cells[(int)cellIndex].Index + 1 ?? -1).ConfigureAwait(true);
                    }
                }
            }
            if (Parent.FreezeModule!.GetFrozenCount() > 0)
            {
                await Parent.InvokeMethod("sfBlazor.Grid.frozenHeight", new object[] { Parent.DataId, Parent.GetClientOption(), null! }).ConfigureAwait(true);
            }
            if (Parent.EnableInfiniteScrolling)
            {
                await Parent.InvokeMethod("sfBlazor.Grid.updateOptions", new object[] { Parent.DataId, Parent.GetClientOption() }).ConfigureAwait(true);
            }
            if (Parent.EditSettings != null && Parent.EditSettings.ShowAddNewRow)
            {
                Parent.EventAggregator.Trigger("DisableOrEnableAddForm", null!);
                Parent.EventAggregator.Trigger("ResetAddFormValues", isFromAddForm ? "Cancel" : null!);
            }
            if (Parent.SelectionModule != null && Parent.SelectionModule.IsCheckBoxPersistSelection() && Parent.SelectionModule.PersistedData.Count == Parent.TotalItemCount && EditedRow?.Action == EditAction.Added)
            {
                Parent.CheckBoxState = Parent.CheckBoxState == CheckState.Intermediate ? CheckState.Check : Parent.CheckBoxState;
                Parent.EventAggregator.Trigger("HeaderStateChanged", null!);
            }
        }

        internal async Task EditComplete(ActionEventArgs<T> args, object? eventArgs = null, string? requestType = null)
        {
            Parent.SoftRefresh = true;
            switch (args.RequestType.ToString())
            {
                // TODO: checkboxselection, persistselection
                case "Save":
                    if ((Parent.SelectionSettings!.Type != SelectionType.Multiple && Parent.SelectionModule != null && !Parent.SelectionModule.HasCheckBoxColumn()) || !Parent.SelectionSettings.PersistSelection)
                    {
                        if (args.Action == "Add")
                        {
                            if (Parent.SelectionSettings.Type == SelectionType.Single && Parent.SelectionModule != null)
                            {
                                await Parent.SelectionModule.ClearSelection().ConfigureAwait(true);
                            }
                            if (!Parent.EditSettings!.ShowAddNewRow && !(Parent.EnableVirtualization
                                && Parent.EditSettings.NewRowPosition == NewRowPosition.Bottom
                                && Parent.EditSettings.Mode == EditMode.Normal))
                            {
                                await Parent.SelectRowAsync(args.Index).ConfigureAwait(true);
                            }
                            if (Parent.EnableAdaptiveUI && Parent.Toolbar != null)
                            {
                                Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
                            }
                        }
                    }
                    break;
                case "Delete":
                    if (Parent.EditModule!.AlertMessage != null && Parent.EditModule.AlertMessage.Equals("DeleteAlert", StringComparison.Ordinal))
                    {
                        return;
                    }
                    await Parent.SelectRowAsync((int)(EditRowIndex ?? -1)).ConfigureAwait(true);
                    EditRowIndex = null;
                    break;
            }
        }

        #endregion

        #region Set Value Managerment

        internal async Task<List<string>> GetPrimaryKeyValue(object Data)
        {
            var primaryKeys = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            var primaryKeyValues = new List<string>();
            foreach (var key in primaryKeys)
            {
                primaryKeyValues.Add(Parent.PropHelper?.GetObject(key, Data)?.ToString()!);
            }

            return primaryKeyValues;
        }
        
        internal void SetValue<TValue>(TValue Value, string Field, object cloneData = null!)
        {
            cloneData = cloneData ?? CloneData!;
            if (cloneData == null)
            {
                return;
            }
            ValueChanged = true;

            ReflectionExtension.SetValue(cloneData, Field, Value, true, options: new SetOptions()
            {
                CreateInstanceForComplexType = true
            });

            Parent.EventAggregator.Trigger("UpdateEditContext", Field);
        }
        
        private object GetValueForNullable(DynamicObject item, object value, string property)
        {
            var rowData = Parent.Rows?.FirstOrDefault(x => x.Data is DynamicObject dynamicObj && dynamicObj != null && DataUtil.GetDynamicValue(dynamicObj, property) != null);
            item = (rowData?.Data as DynamicObject)!;
            value = DataUtil.GetDynamicValue(item, property);
            return value;
        }

        private void SetDefaultValue(bool isAddMethod = false)
        {
            foreach (var col in GridUtils.GetColumns(Parent))
            {

                var complexFields = col.Field.Split('.');
                var complexLength = complexFields.Length;
                if (col.DefaultValue != null || (complexLength > 1 && !isAddMethod))
                {
                    SetValue(col.DefaultValue, col.Field);
                }
            }

            ValueChanged = false;
        }

        internal async Task SetCellValue(object key, string field, object value)
        {
            Row<object>? TargetRow = null;
            var Rows = Parent.Rows?.FindAll(_ => _.IsDataRow);
            if (Rows != null)
            {
                foreach (var Row in Rows)
                {
                    var PrimaryKey = (await GetPrimaryKeyValue(Row.Data!).ConfigureAwait(true))?.FirstOrDefault();
                    if (PrimaryKey?.Equals(key?.ToString(), StringComparison.Ordinal) == true)
                    {
                        TargetRow = Row;
                        break;
                    }
                }
            }

            if (TargetRow == null)
            {
                return;
            }

            CloneRowData(TargetRow.Data!);
            SetValue(value, field);
            var Changes = new BatchChanges<T>();
            Changes.ChangedRecords = new List<T>() { (T)CloneData! };
            if (Parent.DataModule != null)
            {

                await Parent.DataModule.SaveChanges(Changes, (await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true))?.FirstOrDefault()!).ConfigureAwait(true);
            }
            await Parent.DataProcess().ConfigureAwait(true);
        }

        #endregion

        #region Next Cell Validation

        internal async Task UpdateCopyCell(double rowIndex, string fieldName, string value, string columnname = null!, double valueIndex = 0)
        {
            var dataField = await Parent.GetColumnByFieldAsync(fieldName).ConfigureAwait(true);
            var dropField = await Parent.GetColumnByFieldAsync(columnname).ConfigureAwait(true);
            if (dropField?.Format != null && dataField?.ValueType == dropField?.ValueType)
            {
                var Row = Parent.Rows?.FirstOrDefault(_ => _.Index == valueIndex);
                CloneRowData(Row?.EditedData ?? Row?.Data!);
                value = DataUtil.GetObject(columnname, CloneData!)?.ToString()!;
            }
            if (dataField != null && !dataField.IsPrimaryKey)
            {
                if (dataField.ValueType == typeof(string))
                {
                    await UpdateCell(rowIndex, fieldName, value).ConfigureAwait(true);
                }
                else if ((dataField.ValueType == typeof(bool) || dataField.ValueType == typeof(bool?)) && bool.TryParse(value, out var newboolean))
                {
                    await UpdateCell(rowIndex, fieldName, newboolean).ConfigureAwait(true);
                }
                else if (dataField.ValueType?.IsEnum == true && Enum.TryParse(typeof(T).GetProperty(fieldName)?.PropertyType!, value, out var newenum))
                {
                    await UpdateCell(rowIndex, fieldName, newenum).ConfigureAwait(true);
                }
                else if (dataField.ValueType == typeof(char) && char.TryParse(value, out var newChar))
                {
                    await UpdateCell(rowIndex, fieldName, newChar).ConfigureAwait(true);
                }
                else if ((dataField.ValueType == typeof(DateTime) || dataField.ValueType == typeof(DateTime?)))
                {
                    if (DateTime.TryParseExact(value, dataField.Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var newformatdatetime))
                    {
                        await UpdateCell(rowIndex, fieldName, newformatdatetime).ConfigureAwait(true);
                    }
                    else if (DateTime.TryParse(value, out var newdatetime))
                    {
                        await UpdateCell(rowIndex, fieldName, newdatetime).ConfigureAwait(true);
                    }
                }
                else if ((dataField.ValueType == typeof(Guid) || dataField.ValueType == typeof(Guid?)) && Guid.TryParse(value, out var newguid))
                {
                    await UpdateCell(rowIndex, fieldName, newguid).ConfigureAwait(true);
                }
                else if ((dataField.ValueType == typeof(DateTimeOffset) || dataField.ValueType == typeof(DateTimeOffset?)))
                {
                    if (DateTimeOffset.TryParse(value, out var newdateTimeOffset))
                    {
                        await UpdateCell(rowIndex, fieldName, newdateTimeOffset).ConfigureAwait(true);
                    }
                    else if (DateTimeOffset.TryParseExact(value, dataField.Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var newformatteddateTimeOffset))
                    {
                        await UpdateCell(rowIndex, fieldName, newformatteddateTimeOffset).ConfigureAwait(true);
                    }
                }
                else if ((dataField.ValueType == typeof(DateOnly) || dataField.ValueType == typeof(DateOnly?)))
                {
                    if (DateOnly.TryParse(value, out var newdateonly))
                    {
                        await UpdateCell(rowIndex, fieldName, newdateonly).ConfigureAwait(true);
                    }
                    else if (DateOnly.TryParseExact(value, dataField.Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var newformatdateonly))
                    {
                        await UpdateCell(rowIndex, fieldName, newformatdateonly).ConfigureAwait(true);
                    }
                }
                else if ((dataField.ValueType == typeof(TimeOnly) || dataField.ValueType == typeof(TimeOnly?)) && TimeOnly.TryParse(value, out var newtimeonly))
                {
                    await UpdateCell(rowIndex, fieldName, newtimeonly).ConfigureAwait(true);
                }
                else if ((dataField.ValueType == typeof(int) || dataField.ValueType == typeof(int?)) && int.TryParse(value, out var newint))
                {
                    await UpdateCell(rowIndex, fieldName, newint).ConfigureAwait(true);
                }
                else if ((dataField.ValueType == typeof(long) || dataField.ValueType == typeof(long?)) && long.TryParse(value, out var newint64))
                {
                    await UpdateCell(rowIndex, fieldName, newint64).ConfigureAwait(true);
                }
                else
                {
                    await UpdateCellValidation(dataField, rowIndex, fieldName, value).ConfigureAwait(true);
                }
            }

            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
        }

        private async Task UpdateCellValidation(GridColumn dataField, double rowIndex, string fieldName, string value)
        {
            if ((dataField.ValueType == typeof(float) || dataField.ValueType == typeof(float?)) && float.TryParse(value, out var newfloat))
            {
                await UpdateCell(rowIndex, fieldName, newfloat).ConfigureAwait(true);
            }
            else if ((dataField.ValueType == typeof(decimal) || dataField.ValueType == typeof(decimal?)) && decimal.TryParse(value, out var newdecimal))
            {
                await UpdateCell(rowIndex, fieldName, newdecimal).ConfigureAwait(true);
            }
            else if ((dataField.ValueType == typeof(double) || dataField.ValueType == typeof(double?)) && double.TryParse(value, out var newdouble))
            {
                await UpdateCell(rowIndex, fieldName, newdouble).ConfigureAwait(true);
            }
            else if ((dataField.ValueType == typeof(long) || dataField.ValueType == typeof(long?)) && long.TryParse(value, out var newlong))
            {
                await UpdateCell(rowIndex, fieldName, newlong).ConfigureAwait(true);
            }
            else if ((dataField.ValueType == typeof(ulong) || dataField.ValueType == typeof(ulong?)) && ulong.TryParse(value, out var newulong))
            {
                await UpdateCell(rowIndex, fieldName, newulong).ConfigureAwait(true);
            }
            else if ((dataField.ValueType == typeof(ushort) || dataField.ValueType == typeof(ushort?)) && ushort.TryParse(value, out var newushort))
            {
                await UpdateCell(rowIndex, fieldName, newushort).ConfigureAwait(true);
            }
            else if ((dataField.ValueType == typeof(short) || dataField.ValueType == typeof(short?)) && short.TryParse(value, out var newshort))
            {
                await UpdateCell(rowIndex, fieldName, newshort).ConfigureAwait(true);
            }
            else if ((dataField.ValueType == typeof(uint) || dataField.ValueType == typeof(uint?)) && uint.TryParse(value, out var newuint))
            {
                await UpdateCell(rowIndex, fieldName, newuint).ConfigureAwait(true);
            }
            else
            {
                await UpdateCell(rowIndex, fieldName, null!).ConfigureAwait(true);
            }
        }
        internal async Task UpdateAutofillCell(double rowIndex, string fieldName, string columnName, double valueIndex)
        {
            var Row = Parent.Rows?.FirstOrDefault(_ => _.Index == valueIndex);
            var dataField = await Parent.GetColumnByFieldAsync(fieldName).ConfigureAwait(true);
            var valueField = await Parent.GetColumnByFieldAsync(columnName).ConfigureAwait(true);
            if (Row != null && !dataField.IsPrimaryKey && dataField.AllowEditing)
            {
                CloneRowData(Row.EditedData! ?? Row.Data!);
                var data = DataUtil.GetObject(columnName, CloneData!);
                if (dataField.ValueType == valueField.ValueType)
                {
                    await UpdateCell(rowIndex, fieldName, data).ConfigureAwait(true);
                }
                else if (dataField.ValueType == typeof(string))
                {
                    await UpdateCell(rowIndex, fieldName, data?.ToString()!).ConfigureAwait(true);
                }
                else
                {
                    await UpdateCell(rowIndex, fieldName, null!).ConfigureAwait(true);
                }
            }

            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
        }

        internal async Task UpdateCell(double rowIndex, string field, object value, bool isUndoRedoAction = false)
        {
            var Row = Parent.Rows?.Find(_ => _.Index == rowIndex);
            var Cell = Row?.Cells?.Find(_ => _.Column?.Field?.Equals(field, StringComparison.Ordinal) == true);
            if (Parent.GridEvents?.BeforeAutoFillCell.HasDelegate == true)
            {
                var args = new BeforeAutoFillCellEventArgs<T>
                {
                    Cancel = false,
                    ColumnName = field,
                    Value = value,
                    RowIndex = (int)rowIndex,
                    ColumnIndex = (int)Cell?.Index!,
                    Data = (T)Row!.Data!,
                };
                await Parent.GridEvents.BeforeAutoFillCell.InvokeAsync(args).ConfigureAwait(true);
                value = args.Value;
                if (args.Cancel)
                {
                    return;
                }
            }
            if (Row != null && Cell != null)
            {
                // CRITICAL FIX: For Undo/Redo, always clone from Row.Data (the original)
                // NOT from Row.EditedData (which may already contain the edited value)
                // This ensures both Undo and Redo work correctly
                var sourceData = isUndoRedoAction ? Row.Data! : (Row.EditedData! ?? Row.Data!);
                CloneRowData(sourceData);
                SetValue(value, field);

                // Recompute dirty state against the ORIGINAL data (Row.Data), not the
                // previously edited value. This ensures undo (which restores the old
                // value) clears the green "modified" indicator, while redo (which
                // re-applies the new value) re-marks the cell as dirty and shows green.
                // NOTE: GridUtils.CompareValues returns TRUE when values DIFFER and
                // FALSE when they are the SAME (see Utils.cs: !EqualityComparer.Equals).
                // So the result maps directly to IsDirty (true = changed = dirty).
                // The variable name below reflects the actual return semantics.
                var originalCellValue = Parent.PropHelper?.GetObject(field, Row.Data!);
                var valueDiffersFromOriginal = GridUtils.CompareValues<object>(originalCellValue!, value!);
                Cell.IsDirty = valueDiffersFromOriginal;

                // Flag cell for re-rendering to update CSS classes (e.g., dirty indicator).
                // This ensures the renderer rebuilds the cell's ClassList, which recalculates
                // whether to include "e-updatedtd" class based on the newly updated Cell.IsDirty state.
                Cell.Changes = true;

                // Recompute row-level dirty state: a row is dirty if any of its cells is dirty.
                // This MUST stay active (not commented out). When it was commented out,
                // Row.IsDirty retained its previous value; for a previously-clean row it
                // stayed false, which then triggered the else-branch below that clears
                // EditedData — discarding the value autofill just wrote into CloneData.
                Row.IsDirty = Row.Cells?.Any(c => c.IsDirty) ?? false;

                // Keep EditedData only while the row is dirty; clear it when fully restored.
                if (Row.IsDirty)
                {
                    Row.EditedData = CloneData!;
                }
                else
                {
                    Row.EditedData = null!;
                }

                // Only flag batch changes if at least one row remains dirty.
                HasBatchChanges = Parent.Rows?.Any(r => r.IsDirty) ?? false;
                Parent.SoftRefresh = true;
                Parent.EventAggregator.Trigger("ContentStateChanged", null!);
                Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
            }
            await Task.CompletedTask.ConfigureAwait(true);
        }

        internal async Task UpdateBatchRow(object Data)
        {
            var rows = Parent.Rows?.FindAll(_ => _.IsDataRow);
            var primaryKeyFields = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            var primaryKeyField = primaryKeyFields.Count != 0 ? primaryKeyFields[0] : null;
            var key = Parent.PropHelper?.GetObject(primaryKeyField!, Data);
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var primaryKeyValues = await GetPrimaryKeyValue(row.Data!).ConfigureAwait(true);
                    var primaryKeyValue = primaryKeyValues.Count != 0 ? primaryKeyValues[0] : null;
                    if (!string.IsNullOrEmpty(primaryKeyValue) && primaryKeyValue.Equals(key?.ToString(), StringComparison.Ordinal))
                    {
                        if (row.Cells != null)
                        {
                            foreach (var cell in row.Cells)
                            {
                                cell.IsDirty = GridUtils.CompareValues<object>(Parent.PropHelper?.GetObject(cell.Column?.Field!, row.Data!)!, Parent.PropHelper?.GetObject(cell.Column?.Field!, Data)!);
                            }
                        }
                        Parent.SoftRefresh = true;
                        row.HasDataChanges = true;
                        Parent.EventAggregator.Trigger("RowStateChanged", row!);
                        break;
                    }
                }
            }
        }

        internal async Task SetRowData(object key, object rowData, bool suppressDataUpdate = false)
        {
            var rows = Parent.Rows?.FindAll(_ => _.IsDataRow);
            var primaryKeyFields = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            var primaryKeyField = primaryKeyFields?.Count > 0 ? primaryKeyFields[0] : null;
            if (string.IsNullOrEmpty(primaryKeyField))
            {
                throw new InvalidOperationException("Primary key column is required to set row data");
            }

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var primaryKeyValues = await GetPrimaryKeyValue(row.Data!).ConfigureAwait(true);
                    var primaryKeyValue = primaryKeyValues.Count != 0 ? primaryKeyValues[0] : null;
                    if (!string.IsNullOrEmpty(primaryKeyValue) && primaryKeyValue.Equals(key?.ToString(), StringComparison.Ordinal))
                    {
                        try
                        {
                            var result = !suppressDataUpdate && Parent.DataManager != null ? await Parent.DataManager!.Update<T>(primaryKeyField, rowData, Parent.Query?.FromTable!, Parent.Query!).ConfigureAwait(true) : null;
                            row.Data = result ?? rowData;
                            row.Cells?.ForEach(_ => _.Changes = true);
                            Parent.SoftRefresh = true;
                            row.HasDataChanges = true;
                            Parent.EventAggregator.Trigger("RowStateChanged", row);
                            break;
                        }
                        catch (Exception e)
                        {
                            if (Parent.GridEvents?.OnActionFailure.HasDelegate == true)
                                await Parent.GridEvents.OnActionFailure.InvokeAsync(new FailureEventArgs() { Error = e, Parent = Parent }).ConfigureAwait(true);
                            else if (Parent.IsRenderedFromTreeGrid)
                                await Parent.EventAggregator.NotifyAsync("ActionFailure", new FailureEventArgs() { Error = e, Parent = Parent }).ConfigureAwait(true);
                            throw;
                        }
                    }
                }
            }
        }
        internal FieldIdentifier ModelExpressions(string field)
        {
            var column = Parent.Columns?.Find(_ => _.Field?.Equals(field, StringComparison.Ordinal) == true);
            var complexLength = field.Split('.').Length;
            var constant = Expression.Constant(this);
            var exp = Expression.PropertyOrField(constant, "RowData");
            MemberExpression? exp1 = null;
            if (complexLength <= 1 || string.IsNullOrEmpty(field))
            {
                return new FieldIdentifier();
            }

            if (complexLength > 1)
            {
                var complexExpression = exp;
                var fieldParts = field.Split('.');
                for (var i = 0; i < complexLength; i++)
                {
                    if (i == complexLength - 1)
                    {
                        exp1 = Expression.PropertyOrField(complexExpression, fieldParts?[i]!);
                    }
                    else
                    {
                        complexExpression = Expression.PropertyOrField(complexExpression, fieldParts?[i]!);
                    }
                }
            }
            else
            {
                exp1 = Expression.PropertyOrField(exp, field);
            }

            var checkType = column!.ValueType = exp1?.Type!;
            Expression<Func<string>> stringExp;
            Expression<Func<DateTime?>> nullableDateExp;
            Expression<Func<DateTime>> dateExp;
            Expression<Func<double>> doubleExp;
            Expression<Func<double?>> nullableDoubleExp;
            Expression<Func<int>> intExp;
            Expression<Func<long>> intExp1;
            Expression<Func<int?>> nullableIntExp;
            Expression<Func<float>> floatExp;
            Expression<Func<float?>> nullableFloatExp;
            Expression<Func<decimal>> decimalExp;
            Expression<Func<decimal?>> nullableDecimalExp;
            Expression<Func<DateTimeOffset>> offsetExp;
            Expression<Func<DateTimeOffset?>> nullableOffsetExp;
            FieldIdentifier fieldIdentifier = new FieldIdentifier();
            if (checkType == typeof(double?))
            {
                nullableDoubleExp = Expression.Lambda<Func<double?>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<double?>(nullableDoubleExp);
            }
            else if (checkType == typeof(double))
            {
                doubleExp = Expression.Lambda<Func<double>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<double>(doubleExp);
            }
            else if (checkType == typeof(DateTime))
            {
                dateExp = Expression.Lambda<Func<DateTime>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<DateTime>(dateExp);
            }
            else if (checkType == typeof(DateTime?))
            {
                nullableDateExp = Expression.Lambda<Func<DateTime?>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<DateTime?>(nullableDateExp);
            }
            else if (checkType == typeof(int))
            {
                intExp = Expression.Lambda<Func<int>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<int>(intExp);
            }
            else if (checkType == typeof(int?))
            {
                nullableIntExp = Expression.Lambda<Func<int?>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<int?>(nullableIntExp);
            }
            else if (checkType == typeof(long))
            {
                intExp1 = Expression.Lambda<Func<long>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<long>(intExp1);
            }
            else if (checkType == typeof(string))
            {
                stringExp = Expression.Lambda<Func<string>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<string>(stringExp);
            }
            else if (checkType == typeof(float))
            {
                floatExp = Expression.Lambda<Func<float>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<float>(floatExp);
            }
            else if (checkType == typeof(float?))
            {
                nullableFloatExp = Expression.Lambda<Func<float?>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<float?>(nullableFloatExp);
            }
            else if (checkType == typeof(decimal))
            {
                decimalExp = Expression.Lambda<Func<decimal>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<decimal>(decimalExp);
            }
            else if (checkType == typeof(decimal?))
            {
                nullableDecimalExp = Expression.Lambda<Func<decimal?>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<decimal?>(nullableDecimalExp);
            }
            else if (checkType == typeof(DateTimeOffset))
            {
                offsetExp = Expression.Lambda<Func<DateTimeOffset>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<DateTimeOffset>(offsetExp);
            }
            else if (checkType == typeof(DateTimeOffset?))
            {
                nullableOffsetExp = Expression.Lambda<Func<DateTimeOffset?>>(exp1!);
                fieldIdentifier = FieldIdentifier.Create<DateTimeOffset?>(nullableOffsetExp);
            }

            return fieldIdentifier;
        }

        #endregion

        #region Selection Utilities

        internal bool IsPersistSelection()
        {
            return Parent.SelectionSettings!.PersistSelection && Parent.SelectionModule != null && Parent.SelectionModule.PersistedData.Count > 0;
        }
        private bool IsCheckBoxOnly()
        {
            return Parent.SelectionSettings!.CheckboxOnly && (Parent.SelectionModule != null && Parent.SelectionModule.HasCheckBoxColumn());
        }
        private void IsCheckBoxColumn()
        {
            var columns = GridUtils.GetColumns(Parent);
            if (columns?.Any(x => x.Type == ColumnType.CheckBox) == true)
            {
                Parent.SelectionModule?.SetHeaderCheckState();
                Parent.EventAggregator.Trigger("HeaderStateChanged", null!);
            }
        }
        internal async Task<int> GetSelectedCellIndex()
        {
            if (Parent.Rows == null || Parent.Rows.Count == 0)
            {
                return await Task.FromResult<int>(0).ConfigureAwait(true);
            }
            return await Task.FromResult<int>(Parent.Rows.FirstOrDefault(row => row.IsDataRow)?.Cells?.Where(cell => cell.Visible && cell.CellType != CellType.RowDrag).First().Index ?? 0).ConfigureAwait(true);
        }

        #endregion

        #region Column Type Retrieval

        internal IDictionary<string, Type> GetDynamicColType()
        {
            var columnTypes = new Dictionary<string, Type>();
            DynamicObject? item = Parent.Rows?.Find(_ => _.IsDataRow)?.Data as DynamicObject;

            if (Parent.IsRenderedFromTreeGrid)
            {
                var rowData = Parent.Rows?
                .FirstOrDefault(_ => _.IsDataRow)?
                .Data;
                if (rowData == null)
                    return columnTypes;

                var rowType = rowData.GetType();
                if (rowType.IsGenericType && rowType.GetProperty("DataItem") != null)
                {
                    var dataItem = rowType.GetProperty("DataItem")!.GetValue(rowData);
                    item = dataItem as DynamicObject;
                }
            }
            var properties = item?.GetDynamicMemberNames()?.ToArray();
            foreach (var property in properties!)
            {
                var value = DataUtil.GetDynamicValue(item!, property);
                value = value ?? GetValueForNullable(item!, value!, property);
                if (value != null)
                {
                    Type type = value.GetType();
                    if (type.IsValueType)
                    {
                        type = typeof(Nullable<>).MakeGenericType(type);
                    }
                    columnTypes.Add(property, type);
                }
                else
                {
                    columnTypes.Add(property, typeof(object));
                }
            }

            return columnTypes;
        }

        internal Type? GetColumnType(GridColumn Column, ref Type actualType, string field = null!, object data = null!)
        {
            var Fields = string.IsNullOrEmpty(field) ? Column.Field?.Split('.') : field.Split('.');
            Type type = typeof(Nullable<>);
            IDictionary<string, Type>? dynamicType = null;
            var Complex = Fields?.Length ?? 0;
            Type gridType = typeof(T);
            object? customData = null;
            object? dynamicData = null;
            if (Complex > 1)
            {
                Type complexType = gridType;
                for (var i = 0; i < Complex; i++)
                {
                    PropertyInfo? info = complexType?.GetProperty(Fields?[i]!);
                    if (string.Equals(complexType?.Name!, "ExpandoObject", StringComparison.Ordinal) && data != null)
                    {
                        ValueTuple<object, IDictionary<string, Type>, Type> values = ComplexFieldIsExpandoObject((customData!, dynamicType!, type!), i, Fields!, Column, data);
                        customData = values.Item1;
                        dynamicType = values.Item2;
                        type = values.Item3;
                    }
                    else if (data != null && (data is DynamicObject || (complexType != null && complexType.IsSubclassOf(typeof(DynamicObject)))))
                    {
                        ValueTuple<object, Type> values = ComplexFieldIsDynamicObject((dynamicData!, type), i, Complex, Fields!, data);
                        dynamicData = values.Item1;
                        type = values.Item2;
                    }
                    else if (i == Complex - 1)
                    {
                        type = info?.PropertyType!;
                    }
                    else
                    {
                        if (info != null && i < Complex - 1)
                        {
                            complexType = info.PropertyType;
                        }
                    }
                }
            }
            else
            {
                // Data = Column.ForeignKeyValue != null && Row?.ForeignKeyData[Column.Field] != null ? Row?.ForeignKeyData[Column.Field].ToList()[0] : Data;
                if (Column.ForeignKeyValue != null)
                {
                    var Row = Parent.Rows?.Find(_ => _.IsDataRow && _.ForeignKeyData != null && _.ForeignKeyData.ContainsKey(Column.Uid) && _.ForeignKeyData[Column.Uid] != null &&
                            _.ForeignKeyData[Column.Uid].Any() && _.ForeignKeyData[Column.Uid].ToList()[0] != null);
                    var Data = Row?.Data;
                    if (Data != null)
                    {
                        Data = Row?.ForeignKeyData![Column.Uid]?.ToList()?[0];
                        type = Data?.GetType()?.GetProperty(Column.ForeignKeyValue)?.PropertyType!;
                    }

                    type = type?.Equals(typeof(Nullable<>)) == true ? Column.ValueType! : type!;
                    actualType = DataIsExpandoOrDynamic(Row!, actualType, gridType, Column)!;
                }
                else
                {
                    type = gridType.GetProperty(field ?? Column?.Field!)?.PropertyType!;
                }
            }

            return type;
        }

        #endregion

        #region Private Helper Method
        private void EnsureDataAndEditContext(ref object data, ActionEventArgs<T>? actionArgs = null,
    BeforeBatchAddArgs<T> batchAddArgs = null!, CellEditArgs<T> cellEditArgs = null!, object eventArgs = null!, string requestType = null!)
        {
            if (actionArgs != null && (Parent.GridEvents?.OnActionBegin.HasDelegate == true || Parent.IsRenderedFromTreeGrid))
            {
                data = actionArgs.Data!;
                EditContext = actionArgs.EditContext ?? EditContext;
            }
            else if (batchAddArgs != null)
            {
                data = batchAddArgs.DefaultData!;
                EditContext = batchAddArgs.EditContext ?? EditContext;
            }
            else if (cellEditArgs != null)
            {
                data = cellEditArgs.Data!;
                EditContext = cellEditArgs.EditContext ?? EditContext;
            }
            else if (eventArgs != null && eventArgs is RowCreatingEventArgs<T> addEventArgs)
            {
                data = addEventArgs.Data!;
                EditContext = addEventArgs.EditContext ?? EditContext;
            }
            else if (eventArgs != null && eventArgs is RowEditingEventArgs<T> editEventargs)
            {
                data = editEventargs.Data!;
                EditContext = editEventargs.EditContext ?? EditContext;
            }
            else
            {
                data = null!;
                EditContext = null!;
            }

            if (EditContext == null && data != null)
            {
                EditContext = new EditContext(data);
            }
            var Cols = GridUtils.GetColumns(Parent);
            var field = new List<string>();
            Cols.ForEach(x =>
            {
                if (x.Field.Contains('.', StringComparison.Ordinal))
                {
                    field.Add(x.Field);
                }
            });
            ComplexEditContext.Clear();
            if (field.Count > 0 && ComplexEditContext.Count == 0)
            {
                var ComplexData = data;
                field.ForEach(x =>
                {
                    var splitField = x.Split('.');
                    var fieldCount = splitField.Length - 1;
                    var complexData = ComplexData;
                    for (int i = 0; i < fieldCount; i++)
                    {
                        complexData = Parent.PropHelper?.GetObject(splitField[i], complexData!);
                    }
                    if (complexData != null)
                    {
                        EditContext ComplexModel = new EditContext(complexData);
                        ComplexEditContext.Add(x, ComplexModel);
                    }
                });
            }
            if (EditContext == null || data == null)
            {
                throw new InvalidOperationException(
                    $"Unable to create EditContext from type {typeof(T).FullName}, handle object creation or give EditContext using " +
                    $"OnActionBegin event. For batch editing use OnBatchAdd and OnCellEdit events to provide new item.");
            }
        }

        private void CloneRowData(object Data, bool PreventDataClone = false)
        {
            if (Data is ExpandoObject)
            {
                CloneData = CloneUtils.CloneExpandoObject(Data as ExpandoObject, PreventDataClone);
            }
            else if (Data is DynamicObject)
            {
                CloneData = CloneUtils.CloneDynamicObject(Data as DynamicObject, typeof(T), PreventDataClone);
            }
            else
            {
                CloneData = (T)CloneUtils.CloneStaticObjectType(Data, typeof(T), PreventDataClone);

                // Iterate and clone complex field and set default value
                List<GridColumn> columns = GridUtils.GetColumns(Parent);
                string[] complexFields = columns.Where(_ => _.Field.IndexOf('.', StringComparison.Ordinal) > -1).Select(_ => _.Field).ToArray();

                foreach (var prop in complexFields)
                {
                    var value = DataUtil.GetObject(prop, Data) ?? (T)default!;
                    if (value == null)
                    {
                        DataUtil.isNullableComplexField = true;
                        continue;
                    }
                    SetValue(value, prop);
                }
            }
        }

        #endregion
        
        #region Row Model Generation

        internal Row<object> GetModelGenerator(bool isAdd = false)
        {
            if (Parent.AllowGrouping && Parent.GroupSettings!.Columns != null && Parent.GroupSettings.Columns.Length > 0)
            {
                return new GroupModelGenerator<T>(Parent).GenerateRow(ReflectionExtension.TryCreateInstance<T>(), 0, indent: Parent.GroupSettings.Columns.Length);
            }
            else
            {
                return new RowModelGenerator<T>(Parent).GenerateRow(ReflectionExtension.TryCreateInstance<T>(), 0, isAdd);
            }
        }

        #endregion


    }
}
