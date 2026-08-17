using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles searching feature operations for the Grid component.
    /// This class is responsible for building search queries, managing search predicates,
    /// and coordinating search operations with foreign key columns.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class Searching<T>
    {
        #region Private Fields
        
        /// <summary>
        /// Reference to the parent SfGrid component.
        /// </summary>
        private SfGrid<T> _parent { get; set; }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Searching class.
        /// </summary>
        /// <param name="parent">The parent SfGrid component.</param>
        public Searching(SfGrid<T> parent) => _parent = parent;

        #endregion

        #region Foreign Key Search Handling

        /// <summary>
        /// Processes foreign key column data by generating appropriate query and retrieving column data.
        /// Handles both search-based and data-based query generation based on complexity flag.
        /// </summary>
        /// <param name="filteredColumn">The foreign key column to process.</param>
        /// <param name="isComplex">Flag indicating if complex query generation is needed (search-based).</param>
        /// <param name="data">The data source for query generation when not complex.</param>
        /// <param name="foreignKeyModule">Reference to ForeignKey module for non-search query generation.</param>
        internal async Task ProcessForeignKeyColumnData(GridColumn filteredColumn, bool isComplex, object data, ForeignKey<T> foreignKeyModule)
        {
            if ((!string.IsNullOrEmpty(_parent.SearchSettings!.Key) && filteredColumn.AllowSearching) || string.IsNullOrEmpty(_parent.SearchSettings.Key))
            {
                Query? query = isComplex ? await foreignKeyModule.GenerateColumnQuery(filteredColumn).ConfigureAwait(true) : foreignKeyModule.GenerateQuery(filteredColumn, (data as IEnumerable<object>)!, false, true) as Query;
                query!.Params = _parent.Query?.Params;
                filteredColumn.ColumnData = await filteredColumn.GetData(query.Queries).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Checks if any foreign key columns are filtered or being searched.
        /// </summary>
        /// <param name="columns">List of foreign key columns.</param>
        /// <returns>True if any foreign key column has active filter or search.</returns>
        internal bool IsForeignKeyFiltered(List<GridColumn> columns)
        {
            var isForeignKey = false;
            isForeignKey = columns.Any(column =>
            {
                isForeignKey = (_parent.FilterSettings != null && _parent.FilterSettings.Columns != null) 
                    ? _parent.FilterSettings.Columns.Any(col =>
                    {
                        return col.Uid == column.Uid && 
                            (col.Field == column.ForeignKeyField || col.Field == column.ForeignKeyValue);
                    }) 
                    : false;
                return isForeignKey || !string.IsNullOrEmpty(_parent.SearchSettings!.Key);
            });
            return isForeignKey;
        }

        /// <summary>
        /// Applies foreign key search filtering to the query based on search settings.
        /// Builds search predicates and updates the foreign key column's search status.
        /// </summary>
        /// <param name="foreignKeyColumn">The foreign key column to apply search filtering to.</param>
        /// <param name="query">The query object to update with search predicates.</param>
        /// <returns>A Task representing the asynchronous search operation.</returns>
        internal async Task ApplyForeignKeySearch(GridColumn foreignKeyColumn, Query query)
        {
            if (!string.IsNullOrEmpty(_parent.SearchSettings!.Key))
            {
                var PredicateList = new List<WhereFilter>();
                PredicateList.Add(new WhereFilter()
                {
                    Field = foreignKeyColumn.ForeignKeyValue,
                    Operator = "contains",
                    Condition = "or",
                    value = _parent.SearchSettings.Key,
                    IgnoreCase = _parent.SearchSettings.IgnoreCase,
                    IgnoreAccent = _parent.SearchSettings.IgnoreAccent
                });
                Query searchQuery = new Query().Where(WhereFilter.Or(PredicateList));
                IEnumerable<object> result = (IEnumerable<object>)await foreignKeyColumn.GetData(searchQuery.Queries).ConfigureAwait(true);
                foreignKeyColumn.IsSearchQueryRequired = false;
                if (result.Any())
                {
                    foreignKeyColumn.IsSearchQueryRequired = true;
                    query.Where(WhereFilter.Or(PredicateList));
                }
            }
        }

        #endregion

        #region Search Execution

        /// <summary>
        /// Performs the search operation with the given search string.
        /// Updates search settings, handles icon state, and triggers model changes.
        /// </summary>
        /// <param name="searchString">The search string to apply.</param>
        /// <returns>A Task representing the asynchronous search operation.</returns>
        internal async Task PerformSearch(string searchString)
        {
            _parent.SearchClearIcon = string.IsNullOrEmpty(searchString) ? string.Empty : "e-clear-icon";
            searchString = searchString ?? string.Empty;
            if (searchString != _parent.SearchSettings!.Key)
            {
#pragma warning disable BL0005
                _parent.SearchSettings.Key = searchString;
                await _parent.SearchSettings.UpdateProperties("Key", _parent.SearchSettings.Key).ConfigureAwait(true);
#pragma warning restore BL0005
                await _parent.ModelChanged(new ActionEventArgs<T>() { RequestType = Action.Searching, SearchString = _parent.SearchSettings.Key }, eventArgs: new SearchingEventArgs() { SearchText = _parent.SearchSettings.Key }, requestType: "Searching").ConfigureAwait(true);
                await (_parent.PageModule?.UpdatePageSizes())!.ConfigureAwait(true)!;
            }
        }

        /// <summary>
        /// Handles keyboard events (Enter and Escape) for the search input field.
        /// Processes search key submission and adaptive UI escape behavior.
        /// </summary>
        /// <param name="keyEventArgs">The keyboard event arguments containing the key information.</param>
        /// <param name="searchInput">The current search input value.</param>
        /// <param name="isEscapeKeyPressed">Reference parameter to track escape key press state.</param>
        /// <returns>A Task containing a tuple with the updated search input value and escape key state.</returns>
        internal async Task<(string searchInput, bool isEscapeKeyPressed)> HandleSearchKeyboardEvent(KeyboardEventArgs keyEventArgs, string searchInput, bool isEscapeKeyPressed)
        {
            if (keyEventArgs?.Key == "Enter")
            {
                string searchKey = searchInput!;
                if (isEscapeKeyPressed && _parent!.EnableAdaptiveUI && searchInput != null)
                {
#pragma warning disable BL0005
                    _parent.SearchSettings!.Key = searchInput;
#pragma warning restore BL0005
                    searchKey = string.Empty;
                    isEscapeKeyPressed = false;
                }
                await (_parent?.SearchAsync(searchKey)!).ConfigureAwait(true);
            }

            if (keyEventArgs?.Key == "Escape")
            {
                _parent!.SearchClearIcon = string.Empty;
                if (_parent.EnableAdaptiveUI)
                {
                    isEscapeKeyPressed = true;
#pragma warning disable BL0005
                    _parent.SearchSettings!.Key = string.Empty;
#pragma warning restore BL0005
                }
                else
                {
                    searchInput = string.Empty;
                }
            }

            return (searchInput!, isEscapeKeyPressed!);
        }

        /// <summary>
        /// Handles the clear icon click for the search input field.
        /// Clears the search input and invokes JavaScript to clear the search box element.
        /// </summary>
        /// <returns>A Task representing the asynchronous clear operation.</returns>
        internal async Task CancelIconClickSearch()
        {
            _parent!.SearchClearIcon = string.Empty;
            await _parent.SearchAsync(string.Empty).ConfigureAwait(true);
            await _parent.InvokeMethod("sfBlazor.Grid.searchClear", new object[]
            {
                _parent.DataId, $"{_parent.ID}_ToolbarSearchBox"
            }).ConfigureAwait(true);
        }

        #endregion

    }
}
