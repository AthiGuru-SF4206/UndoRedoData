﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles row drag and drop action.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class RowReorder<T>
    {
        #region Private Properties
        private SfGrid<T> _parent { get; set; }

        private bool _isDataDeleted { get; set; }

        private Dimension _targetDimension { get; set; }

        private bool _isDropCancelled { get; set; }

        private List<T> _rowsToReorder = new List<T>();

        private Row<object> DragRow;

        private Row<object> DropRow;

        #endregion

        #region Constructor & Initialization

        

        public RowReorder(SfGrid<T> parent)
        {
            _parent = parent;
            _targetDimension = null!;
            DragRow = null!;
            DropRow = null!;
            DropRowData = null!;
            AddedRow = null!;
            target = null!;
        }

        #endregion

        #region Internal Properties

        internal bool IsReorderByInteraction;

        internal object DropRowData { get; set; }

        internal List<T> AddedRow { get; set; }

        internal DOM target { get; set; }

        internal string RowReorderIndentWidth { get; set; } = string.Empty;

        internal bool HasAddedRecord { get; set; }

        #endregion

        #region Private class SelectionState
        private class SelectedRecordsIndexes
        {
            public List<T>? SelectedRecords { get; set; }
            public List<int>? SelectedIndexes { get; set; }
        }
        #endregion
        
        #region Private Methods
        private async Task<SelectedRecordsIndexes> GetSelectedRecordsAndIndexes(List<T> SelectedRecords, string action, List<int> SelectedIndexes, int fromIndex, bool isDragWithinGrid, DotNetObjectReference<GridJSInteropAdaptor<T>>? destInstance = null, bool withinBothGrid = false)
        {
            if (!IsReorderByInteraction && !_parent.EnableVirtualization)
            {
                SelectedRecords = _rowsToReorder;
                _rowsToReorder = (action == "add") ? new List<T>() : _rowsToReorder;
            }
            else if (destInstance == null)
            {
                SelectedRecords = await _parent.GetSelectedRecordsAsync().ConfigureAwait(true);
                SelectedIndexes = await _parent.GetSelectedRowIndexesAsync().ConfigureAwait(true);
            }
            else
            {
                List<T> currentViewRecords = await destInstance.Value.Parent.GetCurrentViewRecordsAsync().ConfigureAwait(true);
                SelectedRecords = withinBothGrid ? (_parent.EnableVirtualization && destInstance.Value.Parent.VirtualScrollModule != null && destInstance.Value.Parent.VirtualScrollModule.GeneratedData?.Count > 0 ?
                                  new List<T> { (T)destInstance.Value.Parent.VirtualScrollModule.GeneratedData[fromIndex].FirstOrDefault()! } :
                                  new List<T> { currentViewRecords[fromIndex] }) : await destInstance.Value.Parent.GetSelectedRecordsAsync().ConfigureAwait(true);
                SelectedIndexes = withinBothGrid ? new List<int> { fromIndex } : await destInstance.Value.Parent.GetSelectedRowIndexesAsync().ConfigureAwait(true);
            }

            if (SelectedIndexes?.Count != 0 && SelectedRecords.Count != 0 && isDragWithinGrid)
            {
                SelectedRecords = (SelectedIndexes != null && SelectedIndexes.Contains(fromIndex)) ? SelectedRecords : new List<T>();
            }

            if (SelectedRecords.Count == 0)
            {
                List<T> CurrentRecords = await _parent.GetCurrentViewRecordsAsync().ConfigureAwait(true);
                if (_parent.EnableVirtualization && !(_parent.AllowGrouping && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0))
                {
                    if (_parent.DataManager!.DataAdaptor!.IsRemote() || _parent.DataManager.Adaptor == Adaptors.CustomAdaptor)
                    {
                        IEnumerable<object> alreadyAdded;
                        if (action == "delete" && _parent.VirtualScrollModule != null && _parent.VirtualScrollModule.GeneratedData?.TryGetValue(fromIndex, out alreadyAdded!) == true)
                        {
                            SelectedRecords.Add((T)alreadyAdded.FirstOrDefault()!);
                            _rowsToReorder = SelectedRecords;
                        }
                        else
                        {
                            SelectedRecords = _rowsToReorder;
                            _rowsToReorder = new List<T>();
                        }
                    }
                    else
                    {
                        CurrentRecords = new List<T>(_parent.DataSource!);
                        if (CurrentRecords.Count >= fromIndex)
                        {
                            if (action == "delete")
                            {
                                SelectedRecords.Add(CurrentRecords[fromIndex]);
                                _rowsToReorder = SelectedRecords;
                            }
                            else
                            {
                                SelectedRecords = _rowsToReorder;
                                _rowsToReorder = new List<T>();
                            }
                        }
                    }
                }
                if (CurrentRecords.Count >= fromIndex && !_parent.EnableVirtualization && !(_parent.EnableInfiniteScrolling && _parent.InfiniteScrollSettings!.EnableCache) && !(_parent.AllowGrouping && _parent.GroupSettings!.Columns?.Length > 0))
                {
                    if (_parent.DataManager!.Adaptor == Adaptors.CustomAdaptor)
                    {
                        if (action == "delete")
                        {
                            SelectedRecords.Add(CurrentRecords[fromIndex]);
                            _rowsToReorder = SelectedRecords;
                        }
                        else
                        {
                            SelectedRecords = _rowsToReorder;
                            _rowsToReorder = new List<T>();
                        }
                    }
                    else
                    {
                        if (CurrentRecords.Count > 0)
                        {
                            SelectedRecords.Add(CurrentRecords[fromIndex]);
                        }
                    }
                }
                else if (_parent.AllowGrouping && _parent.GroupSettings!.Columns?.Length > 0)
                {
                    SelectedRecords.Add((T)DragRow?.Data!);
                    _rowsToReorder = SelectedRecords;
                }
            }

            return new SelectedRecordsIndexes() { SelectedRecords = SelectedRecords, SelectedIndexes = SelectedIndexes! };
        }

        private async Task ReorderRowsAction(bool outsideGrid, List<T> added, List<T> deleted, bool isDragWithinGrid, string action, int selectedDataIndex, int fromIndex, int toIndex, string Primarykey, string targetClass = "", string targetId = "", string? fromUid = null, string? toUid = null, double clientX = 0, double clientY = 0)
        {
            Dictionary<object, object> groupedKeyCollection = new Dictionary<object, object>();

            DropRow = _parent.Rows?.Find(x => x?.Uid == toUid)!;

            if (((!_parent.DataManager!.DataAdaptor!.IsRemote() || _parent.DataManager.Adaptor == Adaptors.GraphQLAdaptor) && !(isDragWithinGrid && _parent.AllowGrouping && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0)) && !(_parent.DataSource != null && isDragWithinGrid && _parent.AllowGrouping && _parent.GroupSettings!.Columns?.Length > 0))
            {
                if (!outsideGrid)
                {
                    var AddedRecords = _parent.Rows?.Where(x => x.Action == EditAction.Added);
                    if (added.Count > 0 && HasAddedRecord && _parent.EditSettings!.NewRowPosition == NewRowPosition.Top)
                    {
                        HasAddedRecord = false;
                        selectedDataIndex = selectedDataIndex - 1;
                    }
                    if (added.Count > 0 && AddedRecords != null && AddedRecords.Any())
                    {
                        added.RemoveAll(item => AddedRecords.Any(addedItem => Equals(addedItem.Data, item)));
                        selectedDataIndex = selectedDataIndex - AddedRecords.AsQueryable().Count();
                    }
                    await _parent.DataManager.SaveChanges<T>(new List<T>(), added, deleted, Primarykey, action == "add" && isDragWithinGrid && _parent.DataSource != null && selectedDataIndex >= 0 ? selectedDataIndex : toIndex).ConfigureAwait(true);
                }
            }
            else if (!(_parent.AllowGrouping && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0))
            {
                await SwapCurrentViewData(added, deleted, isDragWithinGrid, action, fromIndex, selectedDataIndex >= 0 ? selectedDataIndex : toIndex, Primarykey, clientX, clientY).ConfigureAwait(true);
            }

            if (action == "add" && (((!_parent.DataManager.DataAdaptor.IsRemote() || _parent.DataSource != null) || _parent.DataManager.Adaptor == Adaptors.GraphQLAdaptor) && isDragWithinGrid && _parent.AllowGrouping && _parent.GroupSettings!.Columns?.Length > 0))
            {
                var groupedColumns = _parent.GroupSettings.Columns?.ToList();
                var dropRowParentUId = _parent.Rows?.Find(x => x?.Uid == toUid)?.ParentUid;
                List<T> keyChangedDataRow = new List<T>();
                SelectedRecordsIndexes updatedRecord = new SelectedRecordsIndexes();

                if (DropRow != null && (bool)!(DragRow?.ParentUid?.Equals(DropRow.ParentUid, StringComparison.Ordinal) == true))
                {
                    DropRowData = DropRow.Data!;
                    AddedRow = added;
                    foreach (var dragRows in added)
                    {
                        foreach (var column in groupedColumns!)
                        {
                            var gridColumn = GridUtils.GetColumnByField(column, _parent.Columns!);
                            var dropKeyValue = _parent.PropHelper?.GetObject(column, DropRow.Data!);
                            _parent.EditModule!.SetValue(dropKeyValue, column, dragRows!);
                            var DraggedRow = _parent.Rows?.FirstOrDefault(row => row.Data!.Equals(dragRows));
                            if (_parent.EditSettings != null && _parent.EditSettings.Mode == EditMode.Batch && DraggedRow?.EditedData != null)
                            {
                                _parent.EditModule.SetValue(dropKeyValue, column, DraggedRow.EditedData);
                            }

                            if (gridColumn?.Format != null)
                            {
                                dropKeyValue = DataUtil.GetFormattedValue(dropKeyValue!, gridColumn.Format);
                            }

                            if (!groupedKeyCollection.TryGetValue(dropKeyValue!, out object? _value) && dropKeyValue != null)
                            {
                                groupedKeyCollection.Add(dropKeyValue, column);
                            }
                        }
                        keyChangedDataRow.Add(dragRows);
                    }
                    updatedRecord.SelectedRecords = keyChangedDataRow;
                    await _parent.DataManager.SaveChanges<T>(changed: updatedRecord.SelectedRecords, new List<T>(), new List<T>(), keyField: Primarykey, null).ConfigureAwait(true);
                }
            }
            if ((!_parent.DataManager.DataAdaptor.IsRemote() || _parent.DataManager.Adaptor == Adaptors.GraphQLAdaptor) && ((isDragWithinGrid && action == "add") || !isDragWithinGrid))
            {
                _parent.IsRowReordered = true;
                var dropEventArgs = new RowDroppedEventArgs<T>
                {
                    FromIndex = fromIndex,
                    DropIndex = selectedDataIndex >= 0 ? selectedDataIndex : toIndex,
                    Data = action == "add" ? added : deleted,
                    Parent = _parent,
                    Action = isDragWithinGrid ? null! : (action == "add" ? "Add" : "Delete"),
                    Target = target,
                    TargetDimension = _targetDimension,
                    ClientX = clientX,
                    ClientY = clientY
                };
                await _parent.ModelChanged(new ActionEventArgs<T>() { Cancel = false, RequestType = Action.RowDragAndDrop }, requestType: "RowDragAndDrop", eventArgs: dropEventArgs, groupedKey : groupedKeyCollection).ConfigureAwait(true);
                _parent.IsRowReordered = false;
            }

            if (isDragWithinGrid && action == "delete")
            {
                await ReorderRows(fromIndex, toIndex, "add", true, targetClass, targetId, fromUid: fromUid, toUid: toUid, clientX: clientX, clientY: clientY).ConfigureAwait(true);
            }
        }

        private async Task SwapCurrentViewData(List<T> added, List<T> deleted, bool isDragWithinGrid, string action, int fromIndex, int toIndex, string primaryKey, double clientX = 0, double clientY = 0)
        {
            List<T> CurrentViewData = _parent.CurrentViewData!.Cast<T>().ToList();
            if (isDragWithinGrid)
            {
                if (action == "delete")
                {
                    var count = CurrentViewData.Count;
                    foreach (var item in deleted)
                    {
                        CurrentViewData.Remove(item);
                    }
                    _isDataDeleted = CurrentViewData.Count != count;
                    _parent.CurrentViewData = (IEnumerable<object>)CurrentViewData;
                }
                else
                {
                    int dragIndex = fromIndex;
                    int dropIndex = toIndex;
                    fromIndex -= _parent.VirtualScrollModule?.RowStartIndex ?? 0;
                    toIndex -= _parent.VirtualScrollModule?.RowStartIndex ?? 0;
                    int index = 0;
                    if (_isDataDeleted)
                    {
                        index = fromIndex < toIndex ? toIndex - added.Count + 1 : toIndex;
                    }
                    else
                    {
                        index = fromIndex < toIndex ? toIndex + 1 : toIndex;
                    }
                    CurrentViewData.InsertRange(index, added);
                    _parent.CurrentViewData = (IEnumerable<object>)CurrentViewData;
                    await RefreshGridContent().ConfigureAwait(true);
                    await InvokeRowDroppedEvent(added, dragIndex, dropIndex, action, isDragWithinGrid, clientX: clientX, clientY: clientY).ConfigureAwait(true);
                }
            }
            else
            {
                int dropIndex = toIndex;
                if (action == "delete")
                {
                    foreach (var item in deleted)
                    {
                        CurrentViewData.Remove(item);
                    }
                }
                else
                {
                    toIndex -= _parent.VirtualScrollModule?.RowStartIndex ?? 0;
                    CurrentViewData.InsertRange(toIndex, added);
                }
                _parent.CurrentViewData = (IEnumerable<object>)CurrentViewData;
                await RefreshGridContent().ConfigureAwait(true);
                await InvokeRowDroppedEvent(action == "add" ? added : deleted, fromIndex, dropIndex, action, isDragWithinGrid, clientX: clientX, clientY: clientY).ConfigureAwait(true);
            }
        }

        private async Task RefreshGridContent()
        {
            if (_parent.EnableVirtualization && _parent.VirtualScrollModule != null)
            {
                await _parent.VirtualScrollModule.CheckAndResetCache("RowDragAndDrop").ConfigureAwait(true);
                _parent.VirtualScrollModule.QueriedCurrentViewData = _parent.CurrentViewData;
                _parent.VirtualScrollModule.SetGeneratedData(_parent.VirtualScrollModule.RowStartIndex, _parent.VirtualScrollModule.RowEndIndex, _parent.VirtualScrollModule.QueriedCurrentViewData!);
            }
            _parent.ForceUpdate = true;
            _parent.IsRowReordered = true;
            string content = _parent.EnableVirtualization ? "VirtualComponentUpdate" : "ContentStateChanged";
            _parent.EventAggregator.Trigger(content, null!);
            _parent.IsRowReordered = false;
        }

        private async Task InvokeRowDroppedEvent(List<T> data, int dragIndex, int dropIndex, string action, bool isDragWithinGrid, double clientX = 0, double clientY = 0)
        {
            if (_parent.GridEvents?.RowDropped.HasDelegate == true)
            {
                var args = new RowDroppedEventArgs<T>
                {
                    FromIndex = dragIndex,
                    DropIndex = dropIndex,
                    Data = data,
                    Parent = _parent,
                    Action = isDragWithinGrid ? null! : (action == "add" ? "Add" : "Delete"),
                    Target = target,
                    TargetDimension = _targetDimension,
                    ClientX = clientX,
                    ClientY = clientY
                };
                await _parent.GridEvents.RowDropped.InvokeAsync(args).ConfigureAwait(true);
            }
        }

        #endregion

        #region Internal Methods

        internal async Task ReorderRows(int fromIndex, int toIndex, string action = "delete", bool isDragWithinGrid = true, string targetClass = "", string targetId = "", object? targetDimension = null, DotNetObjectReference<GridJSInteropAdaptor<T>>? destInstance = null, bool outsideGrid = false, bool differentTValue = false, string? fromUid = null, string? toUid = null, bool withinBothGrid = false, double clientX = 0, double clientY = 0)
        {
            var Primarykey = (await _parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true))?.FirstOrDefault();
            List<T> SelectedRecords = new List<T>();
            int selectedDataIndex = toIndex;
            List<int>? SelectedIndexes = null;
            DropRow = _parent.Rows?.Find(x => x?.Uid == toUid)!;
            await _parent.EditModule!.HandleEditStateBeforeRowReorder(this).ConfigureAwait(true);
            if (action == "add" && _parent.DataSource != null && isDragWithinGrid && !(_parent.AllowGrouping && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0))
            {
                IEnumerable<object>? Current = _parent.CurrentViewData;
                object? CurrentData;

                if (_parent.EnableVirtualization)
                {
                    var virtualModule = _parent.VirtualScrollModule;
                    CurrentData = virtualModule?.GeneratedData.Count == 0
                        ? virtualModule.CurrentViewDataLookup[toIndex]
                        : virtualModule?.GeneratedData[toIndex].FirstOrDefault();
                }
                else
                {
                    CurrentData = Current!.ToList()[toIndex];
                }

                var value = _parent.PropHelper?.GetObject(Primarykey!, CurrentData!)?.ToString();
                IEnumerable<T> dataSource = _parent.DataSource;

                selectedDataIndex = dataSource.ToList().FindIndex(x => _parent.PropHelper?.GetObject(Primarykey!, x)?.ToString() == value?.ToString());
                selectedDataIndex = selectedDataIndex < 0 && dataSource?.Count() >= toIndex ? toIndex : fromIndex < toIndex ? selectedDataIndex + 1 : selectedDataIndex;
            }
            SelectedRecordsIndexes selectedRecordsIndexes = new SelectedRecordsIndexes();
            selectedRecordsIndexes = await GetSelectedRecordsAndIndexes(SelectedRecords, action, SelectedIndexes!, fromIndex, isDragWithinGrid, destInstance, withinBothGrid: withinBothGrid).ConfigureAwait(true);
            SelectedRecords = selectedRecordsIndexes.SelectedRecords!;
            SelectedIndexes = selectedRecordsIndexes.SelectedIndexes;
            bool isSameGroup = _parent.AllowGrouping && _parent.GroupSettings!.Columns?.Length > 0 && DragRow?.ParentUid == DropRow?.ParentUid;
            bool isGroupCaption = DropRow?.RowType?.Equals("GroupCaption", StringComparison.Ordinal) ?? false;

            if (((isDragWithinGrid && SelectedIndexes != null && SelectedIndexes.Contains(toIndex) && !(_parent.AllowGrouping && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0))) || (!isDragWithinGrid && _isDropCancelled) || isSameGroup || (_parent.AllowGrouping && _parent.GroupSettings!.Columns?.Length > 0 && isGroupCaption))
            {
                _isDropCancelled = false;
                return;
            }

            if (IsReorderByInteraction && (isDragWithinGrid && action == "delete" || (!isDragWithinGrid && action == "add") || outsideGrid || differentTValue))
            {
                Dimension pos = targetDimension != null ? JsonSerializer.Deserialize<Dimension>(targetDimension.ToString()!)! : (Dimension)targetDimension!;
                if (_parent.GridEvents?.RowDropping.HasDelegate == true)
                {
                    var args = new RowDroppingEventArgs<T>
                    {
                        Cancel = false,
                        Target = new DOM() { ID = targetId, XPath = targetClass },
                        FromIndex = fromIndex,
                        DropIndex = toIndex,
                        Data = SelectedRecords,
                        Action = null!,
                        Parent = _parent,
                        TargetDimension = pos,
                        ClientX = clientX,
                        ClientY = clientY
                    };
                    await _parent.GridEvents.RowDropping.InvokeAsync(args).ConfigureAwait(true);
                    if (args.Cancel || outsideGrid && string.IsNullOrEmpty(targetClass)) {
                        if (!isDragWithinGrid && destInstance != null && destInstance.Value.Parent.RowReorderModule != null)
                        {
                            destInstance.Value.Parent.RowReorderModule._isDropCancelled = true;
                        }
                        return;
                    }
                }
                if (outsideGrid && string.IsNullOrEmpty(targetClass)) {
                    return;
                }
                _targetDimension = pos;
                target = new DOM() { ID = targetId, XPath = targetClass };
            }

            List<T> added = new List<T>();
            List<T> deleted = new List<T>();
            bool _isMultiSelect = _parent.SelectionSettings!.Type == SelectionType.Multiple && SelectedRecords?.Count > 0;
            if (action == "add")
            {
                added = SelectedRecords!;
                if (_isMultiSelect && !isDragWithinGrid && SelectedIndexes?.Count > 0)
                {
                    fromIndex = SelectedIndexes[0];        
                }
            }
            else if (action == "delete")
            {
                if (withinBothGrid && fromIndex >= 0 && !(_parent.AllowGrouping && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0))
                {
                    if (_parent.EnableVirtualization && _parent.VirtualScrollModule != null && _parent.VirtualScrollModule.GeneratedData?.Count > 0)
                    {
                        deleted.Add((T)_parent.VirtualScrollModule.GeneratedData[fromIndex].FirstOrDefault()!);
                    }
                    else
                    {
                        var currentViewDataList = _parent.CurrentViewData!.ToList();
                        deleted.Add((T)currentViewDataList[fromIndex]);
                    }
                }
                else
                {
                    deleted = SelectedRecords!;
                    if (_isMultiSelect && SelectedIndexes?.Count > 0 && !isDragWithinGrid)
                    {
                        fromIndex = SelectedIndexes[0];
                    }
                }
            }
           
            await ReorderRowsAction(outsideGrid, added!, deleted!, isDragWithinGrid, action, selectedDataIndex, fromIndex, toIndex, Primarykey!, targetClass, targetId, fromUid, toUid, clientX: clientX, clientY: clientY).ConfigureAwait(true);
        }
   
        internal async Task RowDragStartEvent(int FromIndex, string Uid)
        {
            DragRow = _parent.Rows.Find(r => r.Uid == Uid)!;
            if (_parent.GridEvents?.RowDragStarting.HasDelegate == true || _parent.IsRenderedFromTreeGrid)
            {
                List<T> selectedRecords = await _parent.GetSelectedRecordsAsync().ConfigureAwait(true);
                if (selectedRecords.Count == 0)
                {
                    if (_parent.AllowGrouping && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0)
                    {
                        selectedRecords.Add((T)DragRow.Data!);
                    }
                    else
                    {
                        selectedRecords.Add((T)_parent.Rows?.ElementAtOrDefault(FromIndex)?.Data!);
                    }                   

                }

                var args = new RowDragStartingEventArgs<T>
                {
                    FromIndex = FromIndex,
                    Data = selectedRecords,
                    Parent = _parent
                };
                
                if(_parent.IsRenderedFromTreeGrid)
                    await _parent.EventAggregator.NotifyAsync("RowDragStarting", args).ConfigureAwait(true);
                else
                    await (_parent.GridEvents?.RowDragStarting.InvokeAsync(args))!.ConfigureAwait(true)!;
            }  
            if (_parent.GridEvents?.RowDragStarting.HasDelegate == true || _parent.IsRenderedFromTreeGrid)

            {
                List<T> selectedRecords = await _parent.GetSelectedRecordsAsync().ConfigureAwait(true);
                if (selectedRecords.Count == 0)
                {
                    if (_parent.AllowGrouping && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0)
                    {
                        selectedRecords.Add((T)DragRow.Data!);
                    }
                    else
                    {
                        selectedRecords.Add((T)_parent.Rows?.ElementAtOrDefault(FromIndex)?.Data!);
                    }
                }

                var args = new RowDragStartingEventArgs<T>
                {
                    FromIndex = FromIndex,
                    Data = selectedRecords,
                    Parent = _parent
                };
                
                if(_parent.IsRenderedFromTreeGrid)
                    await _parent.EventAggregator.NotifyAsync("RowDragStart", args).ConfigureAwait(true);
                else
                    await _parent.GridEvents!.RowDragStarting.InvokeAsync(args).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handles disabling or enabling the add form during row drag operations and resets add form values when rows are dropped on the add form.
        /// </summary>
        /// <param name="requestType">The type of drag operation: "RowDragStart" or "RowDragStop".</param>
        /// <param name="isTargetAddForm">Indicates whether the drop target is the add form.</param>
        /// <param name="destInstance">Optional destination grid instance for cross-grid drag operations.</param>
        internal void HandleAddFormStateOnRowDrag(string requestType, bool isTargetAddForm, DotNetObjectReference<GridJSInteropAdaptor<T>>? destInstance = null)
        {
            if (requestType == "RowDragStart" || requestType == "RowDragStop")
            {
                var disableAddForm = requestType == "RowDragStart";
                if (destInstance != null)
                {
                    destInstance.Value?.Parent.EventAggregator.Trigger("DisableOrEnableAddForm", disableAddForm);
                }
                else
                {
                    _parent.EventAggregator.Trigger("DisableOrEnableAddForm", disableAddForm);
                }
                if (isTargetAddForm)
                {
                    _parent.EventAggregator.Trigger("ResetAddFormValues", "RowDropOnAddForm");
                }
            }
        }

        #endregion
    }
}
