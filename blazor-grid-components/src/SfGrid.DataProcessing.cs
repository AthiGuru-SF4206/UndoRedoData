using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids.Internal;
using Syncfusion.Blazor.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;


namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Partial class containing data processing and query generation logic for the SfGrid component.
    /// Handles orchestration of data pipeline including query generation, execution, and event aggregation.
    /// </summary>
    public partial class SfGrid<TValue> : SfDataBoundComponent, IGrid, ISfCircularComponent
    {
        #region Private Fields

        private Query? query { get; set; }

        #endregion

        #region Data Processing Pipeline Orchestration

        /// <summary>
        /// Orchestrates the primary data processing pipeline, serving as the main entry point for all data operations.
        /// Shows the loading spinner, coordinates child processors, manages selection state, and triggers completion events.
        /// </summary>
        /// <param name="action">The action arguments containing virtual scroll indices and request metadata.</param>
        /// <param name="actionArgs">The action event arguments containing request type, data, and context information.</param>
        /// <param name="eventArgs">Additional event-specific arguments (paging, sorting, filtering, etc.).</param>
        /// <param name="requestType">The string identifier of the request type (e.g., "Paging", "Sorting", "Filtering", "Save", "Delete").</param>
        /// <param name="isResetData">Indicates whether the data should be reset to its initial state.</param>
        /// <param name="isDeleteAction">Indicates whether the current action is a delete operation.</param>
        /// <param name="groupedKey">Dictionary containing grouped key information for group-aware operations.</param>
        internal async Task DataProcess(ActionArgs? action = null, ActionEventArgs<TValue>? actionArgs = null, object? eventArgs = null, string? requestType = null, bool isResetData = false, bool isDeleteAction = false, Dictionary<object, object>? groupedKey = null)
        {
            try
            {
                if (IsRendered && !HideGridSpinner && (!EnableVirtualization || (EnableVirtualization && action == null)))
                {
                    await ShowSpinnerAsync().ConfigureAwait(true);
                }
                bool cancel = await DataProcessChild(action!, actionArgs!, actionEventArgs: eventArgs!, requestType: requestType!, isDeleteAction: isDeleteAction).ConfigureAwait(true);

                if (cancel == true)
                {
                    return;
                }
                if (IsFirstEventRender && DataSource == null && SfBaseUtils.IsObservableCollection(CurrentViewData))
                {
                    ((INotifyCollectionChanged)CurrentViewData!).CollectionChanged += CollectionChangedMethod!;
                    IsFirstEventRender = false;
                }

                if (actionArgs != null && (actionArgs.RequestType == Action.Save || requestType == "Save") && actionArgs.Action == "Add" && (EnableVirtualization || EnableColumnVirtualization) && FilterSettings != null && FilterSettings.Type.Equals(FilterType.FilterBar) && IsAdd)
                {
                    IsAdd = false;
                }

                AddOrDeleteArgs = (actionArgs != null || eventArgs != null) && ((actionArgs!.RequestType == Action.Save || requestType == "Save") || (actionArgs.RequestType == Action.Delete || requestType == "Delete")) ? actionArgs : null;
                await CallStateHasChangedAsync().ConfigureAwait(true);
                SelectionModule?.UpdateSelectionAfterDataProcess();
                if (actionArgs != null || eventArgs != null)
                {
                    if (VirtualScrollModule != null) VirtualScrollModule.RequestType = actionArgs?.RequestType.ToString() ?? requestType!;
                    if (EnableInfiniteScrolling && InfiniteScrollModule != null)
                    {
                        InfiniteScrollModule.RequestType = actionArgs?.RequestType.ToString() ?? requestType!;
                    }
                    if (actionArgs != null) actionArgs.Parent = this;
                    if (FilteredColumns != null && FilteredColumns.Count != 0)
                    {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
                        FilterSettings!.Columns = FilteredColumns.ToList();
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
                    }
                    if (actionArgs != null) actionArgs.Type = "ActionComplete";
                    await SfBaseUtils.InvokeEvent<ActionEventArgs<TValue>>(GridEvents?.OnActionComplete, actionArgs!).ConfigureAwait(true);
                    await EventAggregator.NotifyAsync("ActionComplete", actionArgs!).ConfigureAwait(true);
                    await InvokeDataProcessCompletionEvents(requestType, eventArgs).ConfigureAwait(true);
                    await NotifyFilteringStateChange(requestType).ConfigureAwait(true);
                }

                await InvokeSuccessAsync(actionArgs, requestType: requestType, isResetData: isResetData).ConfigureAwait(true);
            }
            catch (Exception exception) when (HandleException())
            {
                await InvokeFailureAsync(exception).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// First-level data processor handling foreign key resolution and virtualization routing.
        /// Determines whether to execute full query processing or use cached virtual scroll data.
        /// </summary>
        /// <param name="action">The action arguments containing virtual scroll indices and request metadata.</param>
        /// <param name="actionArgs">The action event arguments containing request type and data context.</param>
        /// <param name="actionEventArgs">Additional event-specific arguments for the current operation.</param>
        /// <param name="requestType">The string identifier of the request type (e.g., "Paging", "Sorting").</param>
        /// <param name="isDeleteAction">Indicates whether the current action is a delete operation.</param>
        /// <returns>
        /// Returns true if processing should be cancelled (e.g., when paging to a different page), false to continue.
        /// </returns>
        internal async Task<bool> DataProcessChild(ActionArgs action = null!, ActionEventArgs<TValue> actionArgs = null!, object actionEventArgs = null!, string requestType = null!, bool isDeleteAction = false)
        {
            bool cancel = false;
            query = new Query();
            DataReadyArgs<TValue>? eventArgs = null;
            List<int> QueryStartIndexes = new List<int>();
            bool foreignKeyHandle = false;
            IsEmptyGrid = false;
            var isForeignKeyAction = ForeignKeyModule != null && ForeignKeyModule.isNeedForeignKeyAction();
            if (isForeignKeyAction)
            {
                foreignKeyHandle = true;
                bool isFilteringOrClearFiltering = (actionArgs?.RequestType == Action.Filtering || actionArgs?.RequestType == Action.ClearFiltering) || (requestType != null && (requestType == "Filtering" || requestType == "ClearFiltering"));
                if (ForeignKeyModule != null)
                {
                    await ForeignKeyModule.GetForeignKeyData<TValue>(null!, true, isFiltered: isFilteringOrClearFiltering).ConfigureAwait(true);
                }
            }

            if (action == null)
            {
                action = new ActionArgs
                {
                    VirtualStartIndex = DataSource != null && DataSource is INotifyPropertyChanged ? VirtualScrollModule!.RowStartIndex : 0,
                    VirtualEndIndex = DataSource != null && DataSource is INotifyPropertyChanged ? VirtualScrollModule!.RowEndIndex : 0
                };
            }

            if (!EnableVirtualization || CurrentViewData == null || (EnableVirtualization && GroupSettings != null && GroupSettings.Columns?.Length > 0) || (EnableVirtualization && _updateVirtualPageSize) || (EnableVirtualization && Reset) || (EnableVirtualization && (AllowFiltering && FilterSettings?.Columns?.Count > 0 && requestType != "Save" && requestType != "Delete") || (SearchSettings!.Key?.Length > 0) || (EnablePersistence && requestType != "Save" && requestType != "Delete")) && (actionArgs?.RequestType != Action.Sorting || requestType != "Sorting"))
            {
                _updateVirtualPageSize = false;
                cancel = await DataProcessChildContent(action, actionArgs!, isForeignKeyAction, eventArgs: actionEventArgs, requestType: requestType!, isDeleteAction: isDeleteAction).ConfigureAwait(true);
                if (cancel == true)
                {
                    return cancel;
                }

                if (EnableVirtualization && !Reset)
                {
                    eventArgs = await SetupVirtualScrollData(action, actionArgs).ConfigureAwait(true);
                }
                if (EnableVirtualization && Reset && VirtualScrollModule != null)
                {
                    VirtualScrollModule.QueriedCurrentViewData = (IEnumerable<object>?)((DataResult)Data!).Result;
                }
            }
            else
            {
                if (EnableVirtualization && (actionArgs != null || actionEventArgs != null) && ((actionArgs!.RequestType == Action.RowDragAndDrop || requestType == "RowDragAndDrop") || (actionArgs.RequestType == Action.Sorting || requestType == "Sorting")
                    || (VirtualScrollModule != null && VirtualScrollModule.RefreshByMethod) || (IsRenderedFromTreeGrid && VirtualScrollModule != null && !VirtualScrollModule.RefreshByMethod && (DataSource?.GetType() == typeof(ObservableCollection<TValue>))) || (EditSettings != null && !EditSettings.Mode.Equals(EditMode.Batch) &&
                    (actionArgs.Action == "Edit" || actionArgs.Action == "Add" || (actionEventArgs is RowUpdatingEventArgs<TValue> savingEventArgs) || (actionArgs.RequestType.Equals(Action.Delete) || requestType == "Delete")))))
                {
                    VirtualScrollModule!.RefreshByMethod = false;
                    action.VirtualEndIndex = VirtualScrollModule.RowEndIndex;
                    action.VirtualStartIndex = VirtualScrollModule.RowStartIndex;
                }

                if (PageSettings != null && EnableVirtualization && actionArgs != null
                    && actionArgs.RequestType == Action.Save && VirtualScrollModule!.IsBottomAddForm(VirtualScrollModule.RowEndIndex))
                {
                    int totalItemCount = TotalItemCount + 1;
                    VirtualScrollModule.RowEndIndex = action.VirtualEndIndex = totalItemCount;
                    int gridPageSize = (totalItemCount) < PageSettings.PageSize ? totalItemCount : PageSettings.PageSize;
                    VirtualScrollModule.RowStartIndex = action.VirtualStartIndex = VirtualScrollModule.RowEndIndex - gridPageSize;
                }
                QueryStartIndexes = await (VirtualScrollModule?.GenerateQueryAndSetQueryIndex(query, foreignKeyHandle, isForeignKeyAction, (int)action.VirtualStartIndex, (int)action.VirtualEndIndex, saveAction: requestType == "Save" || requestType == "Delete"))!.ConfigureAwait(true)!;
                ReactiveAggregateModule?.UpdateAggregateFromDataResult((DataResult)Data!, IsEmptyGrid);
                TotalItemCount = ((DataResult)Data!).Count;
                CurrentFilteredRecords = IsEmptyGrid ? null : ((DataResult)Data).FilteredRecords?.Cast<TValue>().ToList();
            }
            cancel = await VirtualScrollModule!.VirtualDataProcess(QueryStartIndexes, action, eventArgs!, requestType: actionArgs?.RequestType.ToString()!, query).ConfigureAwait(true);
            return cancel;

        }

        /// <summary>
        /// Coordinates the data loading and query generation process for all grid data operations.
        /// Generates the appropriate query for the request and updates grid properties with result data.
        /// </summary>
        /// <param name="action">The action arguments containing virtual scroll indices for data range.</param>
        /// <param name="actionArgs">The action event arguments containing request type and data context.</param>
        /// <param name="isForeignKeyAction">Indicates whether the current action requires foreign key handling.</param>
        /// <param name="eventArgs">Additional event-specific arguments for the current operation.</param>
        /// <param name="requestType">The string identifier of the request type (e.g., "Paging", "Filtering").</param>
        /// <param name="isDeleteAction">Indicates whether the current action is a delete operation.</param>
        /// <returns>
        /// Returns true if processing should be cancelled (e.g., after page number adjustment), false to continue.
        /// </returns>
        private async Task<bool> DataProcessChildContent(ActionArgs action, ActionEventArgs<TValue> actionArgs, bool isForeignKeyAction, object eventArgs = null!, string requestType = null!, bool isDeleteAction = false)
        {
            bool cancel = false;
            bool isFilteringOrClearFiltering = (actionArgs?.RequestType == Action.Filtering || actionArgs?.RequestType == Action.ClearFiltering) || (requestType != null && (requestType == "Filtering" || requestType == "ClearFiltering"));
            await TriggerGenerateAndExecuteQuery(action, isFilteringOrClearFiltering, isForeignKeyAction).ConfigureAwait(true);

            if (EnableInfiniteScrolling && InfiniteScrollModule != null)
            {
                InfiniteScrollModule.RefreshInfiniteCurrentViewData((IEnumerable<object>)((DataResult)Data!).Result!);
            }

            CurrentViewData = IsEmptyGrid ? null! : (IEnumerable<object>?)((DataResult)Data!).Result;
            ReactiveAggregateModule?.UpdateAggregateFromDataResult((DataResult)Data!, IsEmptyGrid);
            TotalItemCount = IsEmptyGrid ? 0 : ((DataResult)Data!).Count;
            CurrentFilteredRecords = IsEmptyGrid ? null : ((DataResult)Data!).FilteredRecords?.Cast<TValue>().ToList();
            
            await HandleSelectionPersistence(actionArgs, requestType, isDeleteAction).ConfigureAwait(true);

            if (IsRenderedFromTreeGrid && EnablePersistence && AllowPaging && IsSingleRootData && TotalItemCount == 1 && actionArgs != null)
            {
                TotalItemCount = actionArgs.RequestType.Equals(Action.Paging) ? (DataSource?.Count() ?? 0) : 0;
            }

            // When data filter/searched, current page data could be null if so move to last available page.
            cancel = await AdjustPage(actionArgs, eventArgs, requestType).ConfigureAwait(true);
            return cancel;

        }

        /// <summary>
        /// Invokes completion events based on the request type after data processing completes.
        /// Handles Paging, Sorting, Grouping, Searching, Save, Delete, and Filtering events.
        /// </summary>
        /// <param name="requestType">The request type string (e.g., "Paging", "Sorting", "Filtering").</param>
        /// <param name="eventArgs">The event arguments containing request-specific data.</param>
        private async Task InvokeDataProcessCompletionEvents(string? requestType, object? eventArgs)
        {
            if (requestType == null)
            {
                return;
            }

            switch (requestType)
            {
                case "Paging":
                    await InvokePageChangedEvent(eventArgs as GridPageChangingEventArgs).ConfigureAwait(true);
                    break;
                case "Sorting":
                    await InvokeSortedEvent(eventArgs as SortingEventArgs).ConfigureAwait(true);
                    break;
                case "Grouping":
                case "UnGrouping":
                    await InvokeGroupedEvent(eventArgs as GroupingEventArgs).ConfigureAwait(true);
                    break;
                case "Searching":
                    await InvokeSearchedEvent(eventArgs as SearchingEventArgs).ConfigureAwait(true);
                    break;
                case "Save":
                    await InvokeRowUpdatedEvent(eventArgs as RowUpdatingEventArgs<TValue>).ConfigureAwait(true);
                    break;
                case "Delete":
                    await InvokeRowDeletedEvent(eventArgs as RowDeletingEventArgs<TValue>).ConfigureAwait(true);
                    break;
                case "Filtering":
                case "ClearFiltering":
                    await InvokeFilteredEvent(eventArgs as FilteringEventArgs).ConfigureAwait(true);
                    break;
                case "RowDragAndDrop":
                    if (!IsRenderedFromTreeGrid){
                        await InvokeRowDroppedEvent(eventArgs as RowDroppedEventArgs<TValue>).ConfigureAwait(true);
                    }
                    break;
            }
        }

        /// <summary>
        /// Invokes the PageChanged event for paging operations.
        /// </summary>
        private async Task InvokePageChangedEvent(GridPageChangingEventArgs? pagingEventArgs)
        {
            if (pagingEventArgs == null)
            {
                return;
            }

            GridPageChangedEventArgs pageEventArgs = new GridPageChangedEventArgs()
            {
                CurrentPage = pagingEventArgs.CurrentPage,
                PreviousPage = pagingEventArgs.PreviousPage,
                CurrentPageSize = pagingEventArgs.CurrentPageSize,
                TotalPages = pagingEventArgs.TotalPages,
            };
            await EventAggregator.NotifyAsync("PageChanged", pageEventArgs).ConfigureAwait(true);
            if (GridEvents?.PageChanged.HasDelegate == true)
            {
                await GridEvents.PageChanged.InvokeAsync(pageEventArgs).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Invokes the Sorted event for sorting operations.
        /// </summary>
        private async Task InvokeSortedEvent(SortingEventArgs? sortingEventArgs)
        {
            if (sortingEventArgs == null)
            {
                return;
            }

            SortedEventArgs sortedEventArgs = new SortedEventArgs()
            {
                ColumnName = sortingEventArgs.ColumnName!,
                Direction = sortingEventArgs.Direction,
                Action = sortingEventArgs.Action,
                SortedColumns = sortingEventArgs.SortedColumns,
                Parent = this
            };
            await EventAggregator.NotifyAsync("Sorted", sortedEventArgs).ConfigureAwait(true);
            if (GridEvents?.Sorted.HasDelegate == true)
            {
                await GridEvents.Sorted.InvokeAsync(sortedEventArgs).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Invokes the Grouped event for grouping operations.
        /// </summary>
        private async Task InvokeGroupedEvent(GroupingEventArgs? groupingEventArgs)
        {
            if (groupingEventArgs == null)
            {
                return;
            }

            GroupedEventArgs afterGroupingEventArgs = new GroupedEventArgs()
            {
                ColumnName = groupingEventArgs.ColumnName!,
                Action = groupingEventArgs.Action,
                Parent = this
            };
            if (GridEvents?.Grouped.HasDelegate == true)
            {
                await GridEvents.Grouped.InvokeAsync(afterGroupingEventArgs).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Invokes the Searched event for search operations.
        /// </summary>
        private async Task InvokeSearchedEvent(SearchingEventArgs? searchArgs)
        {
            if (searchArgs == null)
            {
                return;
            }

            SearchedEventArgs searchCompleteEventArgs = new SearchedEventArgs() { SearchText = searchArgs.SearchText!, Parent = this };
            await EventAggregator.NotifyAsync("Searched", searchCompleteEventArgs).ConfigureAwait(true);
            if (GridEvents?.Searched.HasDelegate == true)
            {
                await GridEvents.Searched.InvokeAsync(searchCompleteEventArgs).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Invokes the RowUpdated event for save operations.
        /// </summary>
        private async Task InvokeRowUpdatedEvent(RowUpdatingEventArgs<TValue>? savingArgs)
        {
            if (savingArgs == null)
            {
                return;
            }

            RowUpdatedEventArgs<TValue> savedEventArgs = new RowUpdatedEventArgs<TValue>()
            {
                Data = savingArgs.Data,
                Index = savingArgs.Index,
                PreviousData = savingArgs.PreviousData,
                PrimaryKeys = savingArgs.PrimaryKeys,
                PrimaryKeyValue = savingArgs.PrimaryKeyValue,
                Action = savingArgs.Action,
                Parent = this
            };
            if (GridEvents?.RowUpdated.HasDelegate == true)
            {
                await GridEvents.RowUpdated.InvokeAsync(savedEventArgs).ConfigureAwait(true);
            }
            await EventAggregator.NotifyAsync("RowUpdated", savedEventArgs).ConfigureAwait(true);
        }

        /// <summary>
        /// Invokes the RowDeleted event for delete operations.
        /// </summary>
        private async Task InvokeRowDeletedEvent(RowDeletingEventArgs<TValue>? rowDeletingArgs)
        {
            if (rowDeletingArgs == null)
            {
                return;
            }

            RowDeletedEventArgs<TValue> deletedEventArgs = new RowDeletedEventArgs<TValue>()
            {
                PrimaryKeys = rowDeletingArgs.PrimaryKeys,
                Datas = rowDeletingArgs.Datas,
                Parent = this
            };
            if (GridEvents?.RowDeleted.HasDelegate == true)
            {
                await GridEvents.RowDeleted.InvokeAsync(deletedEventArgs).ConfigureAwait(true);
            }
            await EventAggregator.NotifyAsync("RowDeleted", deletedEventArgs).ConfigureAwait(true);
        }

        /// <summary>
        /// Invokes the Filtered event for filtering and clear filtering operations.
        /// </summary>
        private async Task InvokeFilteredEvent(FilteringEventArgs? filteringArgs)
        {
            if (filteringArgs == null)
            {
                return;
            }

            FilteredEventArgs filteredEventArgs = new FilteredEventArgs()
            {
                FilterPredicates = filteringArgs.FilterPredicates,
                ColumnName = filteringArgs.ColumnName,
                Parent = this
            };
            await EventAggregator.NotifyAsync("Filtered", filteredEventArgs).ConfigureAwait(true);
            if (GridEvents != null && GridEvents.Filtered.HasDelegate)
            {
                await GridEvents.Filtered.InvokeAsync(filteredEventArgs).ConfigureAwait(true);
            }
        }
        
        /// <summary>
        /// Invokes the RowDragAndDropped event for Row drag and drop operation.
        /// </summary>
        private async Task InvokeRowDroppedEvent(RowDroppedEventArgs<TValue>? droppedEventArgs)
        {
            if (droppedEventArgs == null)
            {
                return;
            }
            droppedEventArgs!.Parent = this;
            if (GridEvents != null && GridEvents.RowDropped.HasDelegate)
            {
                await GridEvents.RowDropped.InvokeAsync(droppedEventArgs).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Notifies header state change if filtering with columns that have PreventFilterQuery flag.
        /// </summary>
        private async Task NotifyFilteringStateChange(string? requestType)
        {
            if (requestType != null && (requestType == "Filtering" || requestType == "ClearFiltering"))
            {
                var hasPreventFilterQueryCols = FilterSettings!.Columns?.Where(col => col.PreventFilterQuery).Any();
                if (hasPreventFilterQueryCols.HasValue && (bool)hasPreventFilterQueryCols)
                {
                    EventAggregator.Trigger("HeaderStateChanged", null!);
                }
            }
        }

        /// <summary>
        /// Sets up virtual scroll data by triggering DataReady event and populating generated data.
        /// Updates CurrentViewData and VirtualScrollModule cache with queried data.
        /// Returns DataReady event arguments for event processing.
        /// </summary>
        /// <param name="action">Action arguments containing virtual scroll indices.</param>
        /// <param name="actionArgs">Action event arguments for request type information.</param>
        /// <returns>DataReady event arguments populated with query and data information.</returns>
        private async Task<DataReadyArgs<TValue>> SetupVirtualScrollData(ActionArgs action, ActionEventArgs<TValue>? actionArgs)
        {
            var eventArgs = new DataReadyArgs<TValue>() { Data = (IEnumerable<object>?)((DataResult)Data!).Result, Grid = this, Query = query, StartIndex = (int)action.VirtualStartIndex, EndIndex = (int)action.VirtualEndIndex };
            EventAggregator.Trigger("DataReady", eventArgs);
            
            CurrentViewData = eventArgs.Data;
            VirtualScrollModule!.QueriedCurrentViewData = CurrentViewData;
            action.VirtualEndIndex = (int)eventArgs.EndIndex;
            
            await PopulateGeneratedVirtualScrollData(action, actionArgs).ConfigureAwait(true);
            
            return eventArgs;
        }

        /// <summary>
        /// Populates generated data in VirtualScrollModule based on request type.
        /// Uses RowDragAndDrop indices if applicable, otherwise uses action indices.
        /// </summary>
        private async Task PopulateGeneratedVirtualScrollData(ActionArgs action, ActionEventArgs<TValue>? actionArgs)
        {
            if (actionArgs != null && actionArgs.RequestType.Equals(Action.RowDragAndDrop))
            {
                VirtualScrollModule!.SetGeneratedData((int)VirtualScrollModule!.RowStartIndex, (int)VirtualScrollModule!.RowEndIndex, VirtualScrollModule.QueriedCurrentViewData!);
            }
            else
            {
                VirtualScrollModule!.SetGeneratedData((int)action.VirtualStartIndex, (int)action.VirtualEndIndex, VirtualScrollModule.QueriedCurrentViewData!);
            }
        }

        /// <summary>
        /// Executes data query with virtualization handling.
        /// Adjusts indices and calls GenerateAndExecuteQuery based on virtualization and grouping settings.
        /// </summary>
        private async Task TriggerGenerateAndExecuteQuery(ActionArgs action, bool isFilteringOrClearFiltering, bool isForeignKeyAction)
        {
            if (EnableVirtualization && (GroupSettings?.Columns == null || GroupSettings.Columns.Length == 0) && !Reset && PageSettings != null)
            {
                action.VirtualEndIndex = (int)action.VirtualEndIndex > 0 ? (int)action.VirtualEndIndex : PageSettings.PageSize;
                int pageSize = OverscanCount > 0 ? (PageSettings.PageSize * 2) : PageSettings.PageSize;
                var endIndex = VirtualScrollModule != null && VirtualScrollModule.IsLocal() && PageSettings.PageSize > OverscanCount ? action.VirtualEndIndex + pageSize : action.VirtualEndIndex + (OverscanCount * 2);
                await GenerateAndExecuteQuery(query!, isForeignKeyAction, (int)action.VirtualStartIndex, endIndex, foreginKeyFilter: isFilteringOrClearFiltering).ConfigureAwait(true);
            }
            else
            {
                if (EnableVirtualization && GroupSettings?.Columns?.Length > 0 && PageSettings != null)
                {
                    action.VirtualEndIndex = (int)action.VirtualEndIndex > 0 ? (int)action.VirtualEndIndex : PageSettings.PageSize;
                }

                await GenerateAndExecuteQuery(query!, isForeignKeyAction, foreginKeyFilter: isFilteringOrClearFiltering, action: action).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handles selection persistence for filtering, searching, delete, and checkbox selection scenarios.
        /// Coordinates with SelectionModule to maintain or update selection state based on action type.
        /// </summary>
        private async Task HandleSelectionPersistence(ActionEventArgs<TValue>? actionArgs, string? requestType, bool isDeleteAction)
        {
            if (actionArgs == null && (SearchSettings?.Key?.Length > 0 || FilterSettings?.Columns?.Count > 0 || Query?.Queries?.Where?.Count > 0) && SelectionSettings != null && SelectionSettings.PersistSelection && SelectionModule != null)
            {
                var currentInitialAction = SearchSettings?.Key?.Length > 0 ? "Searching" : (FilterSettings?.Columns?.Count > 0 || Query?.Queries?.Where?.Count > 0) ? "Filtering" : "";
                SelectionModule.GetCurrentFilterData(requestType: currentInitialAction);
            }

            if (actionArgs != null && !actionArgs.RequestType.Equals(Action.Paging))
            {
                await HandleDeleteActionSelection(actionArgs, requestType, isDeleteAction).ConfigureAwait(true);
                await HandleCheckBoxPersistSelection(actionArgs).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handles selection state for delete operations.
        /// </summary>
        private async Task HandleDeleteActionSelection(ActionEventArgs<TValue>? actionArgs, string? requestType, bool isDeleteAction)
        {
            if ((actionArgs?.Action == "Delete" || requestType == "Delete") && SelectionModule != null)
            {
                await SelectionModule.HandleDeleteActionSelection(isDeleteAction, actionArgs!).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handles checkbox selection persistence based on action type and data state.
        /// </summary>
        private async Task HandleCheckBoxPersistSelection(ActionEventArgs<TValue>? actionArgs)
        {
            if (actionArgs != null && !((actionArgs.RequestType.Equals(Action.Filtering) || actionArgs.RequestType.Equals(Action.ClearFiltering)) && (DataManager!.DataAdaptor!.IsRemote() || DataManager.Adaptor == Adaptors.CustomAdaptor) && !Columns!.Any(x => x.Type == ColumnType.CheckBox)) && !actionArgs.RequestType.Equals(Action.Sorting))
            {
                if (SelectionModule != null)
                {
                    SelectionModule.HandleCheckBoxPersistSelection(actionArgs.RequestType, AllowFiltering, TotalItemCount, FilteredColumns!);
                }
            }
        }

        /// <summary>
        /// Adjusts current page to last available page if current page data is empty after filtering/searching.
        /// Returns true if page adjustment was triggered and processing should be cancelled.
        /// </summary>
        private async Task<bool> AdjustPage(ActionEventArgs<TValue>? actionArgs, object? eventArgs, string? requestType)
        {
            if (PageSettings == null || !AllowPaging)
            {
                return false;
            }

            bool isEmptyDataAfterAction = actionArgs != null && !actionArgs.RequestType.Equals(Action.Paging) || eventArgs != null && requestType != "Paging";
            bool isPersistenceExceedingPage = EnablePersistence && TotalItemCount > 0 && PageSettings.CurrentPage > Math.Ceiling((double)TotalItemCount / PageSettings.PageSize);

            if (!((isEmptyDataAfterAction && (CurrentViewData == null || !CurrentViewData.Any()) && TotalItemCount > 0) || isPersistenceExceedingPage))
            {
                return false;
            }

            double pageNo = (TotalItemCount % PageSettings.PageSize == 0) ? (TotalItemCount / PageSettings.PageSize) :
                Math.Ceiling((double)TotalItemCount / PageSettings.PageSize);
            int prevPage = PageSettings.CurrentPage;
            await PageSettings.UpdateProperties("CurrentPage", (int)pageNo).ConfigureAwait(true);
            
            GridPageChangingEventArgs pageChangingEventArgs = new GridPageChangingEventArgs()
            {
                CurrentPage = PageSettings.CurrentPage,
                PreviousPage = prevPage,
                TotalPages = PagerRef!.TotalPages,
                CurrentPageSize = PagerRef.PageSize,
            };
            
            await ModelChanged(new ActionEventArgs<TValue>()
            {
                RequestType = Action.Paging,
                CurrentPage = PageSettings.CurrentPage,
                PreviousPage = prevPage
            }, eventArgs: pageChangingEventArgs, requestType: "Paging").ConfigureAwait(true);

            if ((actionArgs != null && actionArgs.RequestType.Equals(Action.Paging)) || (eventArgs != null && requestType == "Paging"))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Persistence State Management

        /// <summary>
        /// Handles loading and applying persisted grid state from a serialized properties string.
        /// </summary>
        internal async Task HandleSetPersistData(string properties)
        {
            Reset = true;
            IsSetPersistDataCalled = true;
            await PersistProperties(properties).ConfigureAwait(true);
            if (EnableInfiniteScrolling && InfiniteScrollModule != null)
            {
                InfiniteScrollModule.RequestType = "Refresh";
                await InfiniteScrollModule.ResetInfiniteProperties("Refresh").ConfigureAwait(true);
            }
            if (VirtualScrollModule != null)
            {
                VirtualScrollModule.CheckAndResetCache("Refresh").GetAwaiter();
            }
            IsDataLoaded = true;
            await DataProcess().ConfigureAwait(true);
            IsSetPersistDataCalled = false;
            Reset = false;
        }

        internal async Task SetLocalStorage()
        {
            if (!SkipLocalStorageSet)
            {
                await InvokeMethod("window.localStorage.setItem", new object[] { $"grid{ID}", SerializeModel(this) }).ConfigureAwait(true);
            }

            SkipLocalStorageSet = false;
        }

        /// <summary>
        /// Defines the properties of persisting component’s state between page reloads.
        /// </summary>
        private async Task PersistProperties(string properties, bool isResetPersistData = false)
        {
            try
            {
                if (string.IsNullOrEmpty(properties))
                {
                    return;
                }

                var columns = IsStackedHeader ? Columns : GridUtils.GetColumns(this);
                var PersistProp = JsonSerializer.Deserialize<Dictionary<string, object>>(properties.ToString());
                PersistProp!["columns"] = HandleDeprecateColumnType(PersistProp);
                var PersistColumns = JsonSerializer.Deserialize<List<GridColumn>>(PersistProp?["columns"]?.ToString()!);
                var PersitAutoFit = PersistColumns?.FirstOrDefault()?.IsPersistAutoFit;
                var PersistPage = JsonSerializer.Deserialize<GridPageSettings>(PersistProp?["pageSettings"]?.ToString()!);
                var PersistFilter = JsonSerializer.Deserialize<GridFilterSettings>(PersistProp?["filterSettings"]?.ToString()!);
                var PersistSort = JsonSerializer.Deserialize<GridSortSettings>(PersistProp?["sortSettings"]?.ToString()!);
                var PersistGroup = JsonSerializer.Deserialize<GridGroupSettings>(PersistProp?["groupSettings"]?.ToString()!);
                var PersistSearch = JsonSerializer.Deserialize<GridSearchSettings>(PersistProp?["searchSettings"]?.ToString()!);
                await RestoreColumnPersistence(columns!, PersistColumns!, PersitAutoFit).ConfigureAwait(true);
                await RestoreFilterColumnValues(PersistFilter).ConfigureAwait(true);
                await RestoreSortGroupSearchSettings(PersistSort, PersistFilter, PersistGroup, PersistSearch).ConfigureAwait(true);
                await RestorePagingSettings(PersistPage).ConfigureAwait(true);
                if (GroupModule != null)
                {
                    SortModule?.InitialGroupSort(null!);
                }
                IsPersist = true;
                await HandleResetPersistData(isResetPersistData, PersistFilter, PersistColumns).ConfigureAwait(true);
                if (EnableInfiniteScrolling)
                {
                    await PageSettings!.UpdateProperties("CurrentPage", 1).ConfigureAwait(true);
                }
            }
            catch (Exception exception)
            {
                await InvokeFailureAsync(exception).ConfigureAwait(true);
                throw;
            }
        }

        /// <summary>
        /// Handles resetting persisted grid state back to original configuration.
        /// </summary>
        internal async Task HandleResetPersistData()
        {
            if (!_setOnce && PageSettings != null)
            {
                Reset = true;
                await InvokeMethod("sfBlazor.Grid.removePersistItem", new object[] { DataId, $"grid{ID}" }).ConfigureAwait(true);
                await PersistProperties(_originalProp!, true).ConfigureAwait(true);
                if (EnableInfiniteScrolling && InfiniteScrollModule != null)
                {
                    InfiniteScrollModule.RequestType = "Refresh";
                    await InfiniteScrollModule.ResetInfiniteProperties("Refresh").ConfigureAwait(true);
                }
                if (VirtualScrollModule != null)
                {
                    VirtualScrollModule.CheckAndResetCache("Refresh").GetAwaiter();
                }
                await DataProcess(isResetData: true).ConfigureAwait(true);
                PageSettings.EnableExternalMessage = false;
                Reset = false;
                if (!EnableColumnVirtualization && EnableVirtualization && (Width == "100%" || Width == "auto") && AllowResizing)
                {
                    await InvokeMethod("sfBlazor.Grid.syncTableWidthsAfterReset", new object[] { DataId }).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Restores column persistence including visibility, width, and freeze state.
        /// </summary>
        private async Task RestoreColumnPersistence(List<GridColumn> columns, List<GridColumn> PersistColumns, bool? PersitAutoFit)
        {
            bool persistedInOldVersion = PersistColumns?.Where(i => i.OriginalIndex == 0).Count() == PersistColumns?.Count;
            Columns = _columns = columns = columns!.OrderBy(a => PersistColumns?.Select(i => persistedInOldVersion ? i.Index : i.OriginalIndex).ToList().IndexOf(persistedInOldVersion ? a.Index : a.OriginalIndex)).ToList();
            List<GridColumn> gridForeignKeyColumns = new List<GridColumn>();
            SetValuesRecursively(columns, PersistColumns!);
            await ApplyPersistedColumnProperties(columns, PersistColumns!, persistedInOldVersion, gridForeignKeyColumns).ConfigureAwait(true);
            if (PersitAutoFit == true)
            {
                Columns.ForEach(col => col.IsPersistAutoFit = true);
            }
        }

        /// <summary>
        /// Applies persisted properties to each column including width, visibility, and freeze state.
        /// </summary>
        private async Task ApplyPersistedColumnProperties(List<GridColumn> columns, List<GridColumn> PersistColumns, bool persistedInOldVersion, List<GridColumn> gridForeignKeyColumns)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                await RestoreIndividualColumnState(columns, PersistColumns, i, persistedInOldVersion, gridForeignKeyColumns).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Restores visibility, width, and freeze state for an individual column.
        /// </summary>
        private async Task RestoreIndividualColumnState(List<GridColumn> columns, List<GridColumn> PersistColumns, int index, bool persistedInOldVersion, List<GridColumn> gridForeignKeyColumns)
        {
            var foreignKeyColumn = ForeignKey<TValue>.GetForeignKeyColumnsAsync(Columns ?? new List<GridColumn>())?.Where(x => x.Uid == columns[index].Uid)?.FirstOrDefault();

            // Refactored foreign key duplicate check
            bool hasForeignKeyColumn = foreignKeyColumn != null;
            bool isDuplicateForeignkeyField = hasForeignKeyColumn && (gridForeignKeyColumns?.Any(x => x.Field == columns?[index]?.Field) ?? false);

            var foreignKeyColumnByIndex = PersistColumns?.FirstOrDefault(c => c.Index == columns[index].Index);
            var Column = isDuplicateForeignkeyField ? foreignKeyColumnByIndex : PersistColumns?.FirstOrDefault(c => (!string.IsNullOrEmpty(c.Field) || SfGrid<TValue>.CheckColumnType(c, columns[index])) ? c.Field == columns[index].Field : c.Uid == columns[index].Uid) ?? columns[index];

            // Refactored column width changed check
            bool isWidthDifferent = (columns[index].Width?.ToUpperInvariant()) != Column?.Width?.ToUpperInvariant();
            bool shouldMarkColumnResized = AllowResizing && isWidthDifferent;

            if (shouldMarkColumnResized)
            {
                _isColumnResized = true;
            }

            bool hasPersistAutoFit = PersistColumns?.FirstOrDefault()?.IsPersistAutoFit == true;
            bool shouldUpdatePersistAutoFit = isWidthDifferent && !columns[index].AutoFit && hasPersistAutoFit;

            if (shouldUpdatePersistAutoFit)
            {
                _isPersistAutoFit = true;
                _targetColumns.Add(Column?.Field!);
            }

            // Refactored stacked columns persistence check
            bool hasComplexColumns = PersistColumns != null && index < PersistColumns.Count;
            bool hasPersistedChildren = hasComplexColumns && PersistColumns?[index].Columns != null;
            bool hasCurrentChildren = hasComplexColumns && columns[index].Columns != null;
            bool shouldProcessComplexColumns = hasPersistedChildren && hasCurrentChildren;

            if (shouldProcessComplexColumns)
            {
                for (int j = 0; j < PersistColumns?[index].Columns?.Count && j < columns[index].Columns?.Count; j++)
                {
#pragma warning disable BL0005
                    Columns![index].Columns = _columns![index].Columns = columns[index].Columns = columns[index].Columns?.OrderBy(a => PersistColumns[index].Columns?.Select(i => persistedInOldVersion ? i.Index : i.OriginalIndex).ToList().IndexOf(persistedInOldVersion ? a.Index : a.OriginalIndex)).ToList();
#pragma warning restore BL0005
                }
            }

            columns[index].SetVisibility(Column != null && Column.Visible);
            columns[index].SetWidth(Column?.Width!);
            columns[index].SetUid(Column?.Uid!);
            columns[index].SetIsFrozen(Column != null && Column.IsFrozen);
            columns[index].SetFreeze(Column!.Freeze);
            columns[index].TableWidth = Column.TableWidth;
            columns[index].LeftFrozenTableWidth = Column.LeftFrozenTableWidth;
            columns[index].RightFrozenTableWidth = Column.RightFrozenTableWidth;
            await Task.CompletedTask.ConfigureAwait(true);
        }

        /// <summary>
        /// Restores filter column values, handling number and date type conversions.
        /// </summary>
        private async Task RestoreFilterColumnValues(GridFilterSettings? PersistFilter)
        {
            if (PersistFilter != null && PersistFilter.Columns != null)
            {
                for (int i = 0; i < PersistFilter!.Columns?.Count; i++)
                {
                    await ConvertFilterValue(PersistFilter, i).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Converts filter value types to match column type (number, date, etc.).
        /// </summary>
        private async Task ConvertFilterValue(GridFilterSettings? PersistFilter, int columnIndex)
        {
            var filteredValue = PersistFilter?.Columns?[columnIndex].Value;

            // Refactored filter value type check
            bool isJsonElementValue = filteredValue is JsonElement;

            if (isJsonElementValue)
            {
                var column = IsStackedHeader ? GridUtils.GetColumnByFColUidOrField(PersistFilter?.Columns?[columnIndex].Uid!, Columns!, true) : Columns!.Find(col => col.Field == PersistFilter?.Columns?[columnIndex].Field);
                string? value = filteredValue?.ToString();

                // Refactored number column detection
                double result = 0;
                bool isNumberType = column != null && (column.Type == ColumnType.Integer || column.Type == ColumnType.Long || column.Type == ColumnType.Double || column.Type == ColumnType.Decimal);
                bool isNumberColumnWithValidParse = isNumberType && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

                if (isNumberColumnWithValidParse)
                {
#pragma warning disable BL0005
                    if (PersistFilter?.Columns?[columnIndex] != null) PersistFilter.Columns[columnIndex]!.Value = result;
                    if (PersistFilter?.Columns?[columnIndex] != null) PersistFilter.Columns[columnIndex]!.ActualValue = result;
#pragma warning restore BL0005
                }

                // Refactored datetime column check
                bool isDateTimeColumn = column != null && (column.Type == ColumnType.DateTime || column.Type == ColumnType.Date);

                if (isDateTimeColumn)
                {
#pragma warning disable BL0005
                    if (PersistFilter?.Columns?[columnIndex] != null) PersistFilter.Columns[columnIndex]!.ColumnType = Filter<TValue>.GetColumnType(column?.Type)!;
#pragma warning restore BL0005
                }
            }
            await Task.CompletedTask.ConfigureAwait(true);
        }

        /// <summary>
        /// Restores sort, group, search, and filter settings from persisted state.
        /// </summary>
        private async Task RestoreSortGroupSearchSettings(GridSortSettings? PersistSort, GridFilterSettings? PersistFilter, GridGroupSettings? PersistGroup, GridSearchSettings? PersistSearch)
        {
#pragma warning disable BL0005
            GroupSettings!.Columns = PersistGroup?.Columns!;
            GroupSettings.GetType().GetProperty("_columns", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(GroupSettings, GroupSettings.Columns);
            await SortSettings!.UpdateProperties("Columns", PersistSort?.Columns!).ConfigureAwait(true);
            await FilterSettings!.UpdateProperties("Columns", PersistFilter?.Columns!).ConfigureAwait(true);
            SearchSettings!.Fields = PersistSearch?.Fields!;
            await SearchSettings.UpdateProperties("Key", PersistSearch?.Key!).ConfigureAwait(true);
#pragma warning restore BL0005
        }

        /// <summary>
        /// Restores paging settings including page size, current page, and external messages.
        /// </summary>
        private async Task RestorePagingSettings(GridPageSettings? PersistPage)
        {
            if (!EnableVirtualization)
            {
                await PageSettings!.UpdateProperties("PageSize", PersistPage?.PageSize!).ConfigureAwait(true);
                PagerRef?.UpdatePagerProperties("PageSize", PersistPage?.PageSize!);
            }
            await PageSettings!.UpdateProperties("CurrentPage", PersistPage?.CurrentPage!).ConfigureAwait(true);
            await PageSettings.UpdateProperties("PageCount", PersistPage?.PageCount!).ConfigureAwait(true);
            if (PersistPage?.ExternalMessage != null || PagerRef?.ExternalMessage != null)
            {
                if (PagerRef != null) PagerRef.ShowExternalMessage = PersistPage?.EnableExternalMessage ?? false;
                if (PagerRef != null) PagerRef.ExternalMessage = PersistPage?.ExternalMessage;
                PageSettings.EnableExternalMessage = PersistPage?.EnableExternalMessage ?? false;
                if (PersistPage != null) PageSettings.ExternalMessage = PersistPage.ExternalMessage;
            }
        }

        /// <summary>
        /// Handles reset persist data scenario including media columns and filtered columns restoration.
        /// </summary>
        private async Task HandleResetPersistData(bool isResetPersistData, GridFilterSettings? PersistFilter, List<GridColumn>? PersistColumns)
        {
            if (isResetPersistData)
            {
                FilteredColumns = PersistFilter?.Columns;

                // Refactored media columns check
                var MediaColumns = PersistColumns?.Where(Col => !string.IsNullOrEmpty(Col.HideAtMedia)).ToList();
                bool hasMediaColumns = MediaColumns?.Count > 0;

                if (hasMediaColumns)
                {
                    foreach (var col in MediaColumns!)
                    {
                        _mediaColumnsUid.AddOrUpdateItem(col.Uid, col.Visible);
                    }
                    await InvokeMethod("sfBlazor.Grid.setMediaColumns", new object[] { DataId, isResetPersistData }).ConfigureAwait(true);
                }
            }
        }

        private static string HandleDeprecateColumnType(Dictionary<string, object> persistProp)
        {
            JsonArray? columnsArray = null;
            if (persistProp.TryGetValue("columns", out var columnsValue) && columnsValue is not null)
            {
                if (columnsValue is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                {
                    columnsArray = JsonNode.Parse(jsonElement.GetRawText()) as JsonArray;
                }
                else if (columnsValue is string columnsString && !string.IsNullOrWhiteSpace(columnsString))
                {
                    columnsArray = JsonNode.Parse(columnsString) as JsonArray;
                }
            }
            if (columnsArray is not null && columnsArray.Count > 0)
            {
                RemoveDeprecatedNumberType(columnsArray);
            }
            return columnsArray?.ToJsonString()!;
        }

        private static void RemoveDeprecatedNumberType(JsonArray columnsJsonArray)
        {
            foreach (var columnObject in columnsJsonArray)
            {
                if (columnObject is not JsonObject columnJsonObject) continue;
                if (columnJsonObject.TryGetPropertyValue("type", out var type))
                {
                    var typeValue = type?.ToString();
                    if (!string.IsNullOrEmpty(typeValue) && string.Equals(typeValue, "number", StringComparison.OrdinalIgnoreCase))
                    {
                        columnJsonObject.Remove("type");
                    }
                }
                if (columnJsonObject.TryGetPropertyValue("columns", out var nestedColumns))
                {
                    if (nestedColumns is JsonArray nestedColumnsArray && nestedColumnsArray.Count > 0)
                    {
                        RemoveDeprecatedNumberType(nestedColumnsArray);
                    }
                }
            }
        }

        private static void SetValuesRecursively(List<GridColumn> columns, List<GridColumn> persistColumns)
        {
            var persistColumnDictionary = persistColumns.ToDictionary(pc => $"{pc.OriginalIndex}_{pc.Field}_{pc.HeaderText}", pc => pc);

            foreach (var column in columns)
            {
                var key = $"{column.OriginalIndex}_{column.Field}_{column.HeaderText}";
                if (persistColumnDictionary.TryGetValue(key, out var persistColumn))
                {
                    column.SetVisibility(persistColumn.Visible);
                    column.SetWidth(persistColumn.Width);
                    column.SetUid(persistColumn.Uid);

                    if (column.Columns != null && persistColumn.Columns != null)
                    {
                        SetValuesRecursively(column.Columns, persistColumn.Columns);
                    }
                }
            }
        }

        #endregion

        #region Query Generation & Execution

        /// <summary>
        /// Generates the data query based on grid filters, sorting, grouping, and virtualization settings,
        /// then executes the query against the DataManager to retrieve data.
        /// </summary>
        /// <param name="query">The Query object to populate with filter, sort, and paging constraints.</param>
        /// <param name="isForeignKeyAction">Indicates whether foreign key data needs to be fetched during this operation.</param>
        /// <param name="VirtualStartIndex">The starting row index for virtual scroll viewport (default: 0 for non-virtual grids).</param>
        /// <param name="VirtualEndIndex">The ending row index for virtual scroll viewport (default: 0 for non-virtual grids).</param>
        /// <param name="isQueryGenerated">If true, skips query generation and uses the provided Query object directly.</param>
        /// <param name="preventForeign">If true, suppresses foreign key data fetching even when available.</param>
        /// <param name="foreginKeyFilter">Indicates whether the operation is a filter/search requiring foreign key lookup.</param>
        /// <param name="action">The action arguments for special operations like grouping with virtual scroll.</param>
        internal async Task GenerateAndExecuteQuery(Query query, bool isForeignKeyAction, int VirtualStartIndex = 0, int VirtualEndIndex = 0, bool isQueryGenerated = false, bool preventForeign = false, bool foreginKeyFilter = false, ActionArgs action = null!)
        {
            if (!isQueryGenerated && DataModule != null)
            {
                query = await GenerateQuery(query, VirtualStartIndex, VirtualEndIndex).ConfigureAwait(true);
            }
            Data = await DataManager!.ExecuteQuery<TValue>(query!).ConfigureAwait(true);

            await ProcessGroupedData(action).ConfigureAwait(true);

            await WrapCustomAdaptorData(query).ConfigureAwait(true);

            await ProcessForeignKeyData(isForeignKeyAction, foreginKeyFilter, preventForeign).ConfigureAwait(true);

            EnsureDataResultIsNotNull();
        }

        /// <summary>
        /// Generates the query based on grid state (filters, sorts, grouping, paging, virtualization).
        /// For TreeGrid local data, notifies GenerateQuery event; otherwise uses DataModule.GenerateQuery.
        /// </summary>
        private async Task<Query> GenerateQuery(Query query, int VirtualStartIndex, int VirtualEndIndex)
        {
            if (IsRenderedFromTreeGrid && !(DataManager!.DataAdaptor!.IsRemote() || DataManager!.Adaptor.Equals(Adaptors.CustomAdaptor)))
            {
                var updateArgs = new QueryArgs(
                    Query: query,
                    VirtualStartIndex: VirtualStartIndex,
                    VirtualEndIndex: VirtualEndIndex
                );
                updateArgs.VirtualStartIndex = VirtualStartIndex;
                updateArgs.VirtualEndIndex = VirtualEndIndex;
                await EventAggregator.NotifyAsync("GenerateQuery", updateArgs).ConfigureAwait(true);
                query = (Query)updateArgs.Query!;
            }
            else
            {
                query = DataModule!.GenerateQuery(false, (int)VirtualStartIndex, (int)VirtualEndIndex).RequiresCount();
            }
            return query;
        }

        /// <summary>
        /// Processes grouped data for virtualization with grouping enabled.
        /// Sets current grouped data and calculates visible grouped data count.
        /// </summary>
        private async Task ProcessGroupedData(ActionArgs? action)
        {
            if (AllowGrouping && EnableVirtualization && GroupSettings != null && !GroupSettings.EnableLazyLoading && GroupSettings.Columns != null && GroupSettings.Columns.Length > 0 && action != null && action.RequestType != "virtualscroll")
            {
                VirtualScrollModule!.SetCurrentGroupedData();
                VisibleGroupedDataCount = Grouping<TValue>.GetVisibleGroupeddataCountInternal(VirtualScrollModule.CurrentGroupedData!, GroupStates, GroupSettings.PersistGroupState, VirtualScrollModule.CurrentGroupedDataCaptionRowMap);
            }
        }

        /// <summary>
        /// Wraps data in DataResult if using CustomAdaptor without RequiresCounts.
        /// </summary>
        private async Task WrapCustomAdaptorData(Query query)
        {
            if (DataManager!.Adaptor.Equals(Adaptors.CustomAdaptor) && !query.Queries.RequiresCounts)
            {
                Data = new DataResult { Result = (IEnumerable?)Data };
            }
        }

        /// <summary>
        /// Processes foreign key data resolution if needed.
        /// Populates foreign key columns with display values from foreign data sources.
        /// </summary>
        private async Task ProcessForeignKeyData(bool isForeignKeyAction, bool foreginKeyFilter, bool preventForeign)
        {
            if ((Data as DataResult)?.Result != null && ForeignKey<TValue>.GetForeignKeyColumnsAsync(Columns!).Count > 0 && (!isForeignKeyAction || !string.IsNullOrEmpty(SearchSettings?.Key) || foreginKeyFilter) && !preventForeign)
            {
                await ForeignKeyModule!.GetForeignKeyData<TValue>((Data as DataResult)!.Result!, isFiltered: foreginKeyFilter).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Ensures Data.Result is not null, initializing with empty list if necessary.
        /// </summary>
        private void EnsureDataResultIsNotNull()
        {
            if ((Data as DataResult)?.Result == null)
            {
                (Data as DataResult)!.Result = new List<object>();
            }
        }

        #endregion
        
        #region Exception Handling
        private bool HandleException()
        {
            CurrentViewData = Enumerable.Empty<object>().ToList();
            StateHasChanged();
            return true;
        }
        #endregion
    }

    /// <summary>
	/// Class QueryArgs handles the arguments for SfTreeGrid component to generate query.
	/// </summary>
	internal sealed record class QueryArgs
    {
        Query query;
        int virtualStartIndex;
        int virtualEndIndex;

        public QueryArgs(Query Query, int VirtualStartIndex, int VirtualEndIndex)
        {
            query = Query;
            virtualStartIndex = VirtualStartIndex;
            virtualEndIndex = VirtualEndIndex;
        }

        public object? Query { get; internal set; }
        public object? VirtualStartIndex { get; internal set; }
        public object? VirtualEndIndex { get; internal set; }
    }
}