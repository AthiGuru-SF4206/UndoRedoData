using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Syncfusion.Blazor.Data;
using System.Dynamic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles all foreign key column operations including data retrieval, querying,
    /// filtering, searching, and sorting. This module centralizes FK logic to improve
    /// maintainability and enable better unit testing.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class ForeignKey<T>
    {
        #region Private Fields
        /// <summary>
        /// Reference to the parent SfGrid component.
        /// </summary>
        private SfGrid<T> Parent { get; set; }

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the Paging class.
        /// </summary>
        /// <param name="parent">The parent grid component.</param>
        public ForeignKey(SfGrid<T> parent) => Parent = parent;

        #endregion

        #region Query Generation

        /// <summary>
        /// Retrieves foreign key data for all configured foreign key columns in the grid.
        /// This method handles complex nested data scenarios and applies filtering based on 
        /// current search and filter settings.
        /// </summary>
        /// <typeparam name="TModel">The model type of the foreign key data source.</typeparam>
        /// <param name="data">The grid data source (can be grouped or ungrouped records).</param>
        /// <param name="isComplex">Flag indicating if query should handle complex data scenarios 
        /// (search-based queries). When true, uses GenerateColumnQuery; otherwise uses data-based queries.</param>
        /// <param name="isFiltered">Flag indicating if foreign key data should respect active filters.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetForeignKeyData<TModel>(object data, bool isComplex = false, bool isFiltered = false)
        {
            List<GridColumn> foreignKeyColumns = GetForeignKeyColumnsAsync(Parent.Columns!);
            List<GridColumn> listOfForeginKeyColumns = foreignKeyColumns;
            List<GridFilterColumn>? filterSettingsColumns = Parent.FilterSettings?.Columns;
            if (filterSettingsColumns != null && filterSettingsColumns.Count > 0 && isFiltered)
            {
                foreach (GridFilterColumn filteredColumn in filterSettingsColumns)
                {
                    IQueryable<GridColumn> matchingColumns = foreignKeyColumns.Where(f => f.ForeignKeyValue == filteredColumn.Field).AsQueryable();
                    if (matchingColumns.Any())
                    {
                        listOfForeginKeyColumns = isComplex ? matchingColumns.ToList() : foreignKeyColumns.Where(f => f.ForeignKeyValue != filteredColumn.Field).ToList();
                    }
                }
            }

            for (int i = 0; i < listOfForeginKeyColumns.Count; i++)
            {
                GridColumn filteredColumn = listOfForeginKeyColumns[i];
                await Parent.SearchModule!.ProcessForeignKeyColumnData(filteredColumn, isComplex, data, this).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Generates a query or predicate for foreign key data based on the provided parameters.
        /// Handles both simple field mapping and complex nested data scenarios.
        /// </summary>
        /// <param name="column">The GridColumn instance representing the foreign key column. 
        /// Must have ForeignKeyField and ForeignKeyValue properties configured.</param>
        /// <param name="data">Source data for query generation. Can be IEnumerable&lt;object&gt; 
        /// for flat data or Group&lt;T&gt; for grouped data.</param>
        /// <param name="fromData">Determines query strategy:
        /// - true: Generate query from current row data
        /// - false: Generate query for column data retrieval</param>
        /// <param name="needQuery">Controls return type:
        /// - true: Returns Query object
        /// - false: Returns WhereFilter predicate</param>
        /// <returns>
        /// Returns object that can be cast to:
        /// - Query: When needQuery=true. Contains WHERE conditions for FK data filtering.
        /// - WhereFilter: When needQuery=false. Contains filter predicate for FK matching.
        /// </returns>
        public object GenerateQuery(GridColumn column, IEnumerable<object> data, bool fromData, bool needQuery)
        {
            var dataList = data?.ToList();
            if (dataList != null && dataList.Count > 0 && (dataList[0] is ExpandoObject || dataList[0] is DynamicObject))
            {
                return new Query();
            }

            var query = new Query();
            var field = fromData ? (column.ForeignKeyField ?? column.Field) : column.Field;
            List<WhereFilter> predicates = new List<WhereFilter>();
            WhereFilter predicate = new WhereFilter();
            var Field = field.Split('.');
            var complex = Field.Length;
            if (Parent.AllowPaging || Parent.EnableVirtualization || fromData)
            {
                IEnumerable<object>? result = null;
                if (complex == 1)
                {
                    if (Parent.AllowGrouping && Parent.GroupSettings != null && Parent.GroupSettings.Columns?.Length > 0 && !fromData)
                    {
                        result = ((data as Group<T>)?.Records as IEnumerable<object>)?
                          .Select(res => res?.GetType().GetProperty(field)?.GetValue(res))!;
                    }
                    else
                    {
                        result = (data as IEnumerable<object>)?.Select(res => res?.GetType().GetProperty(field)?.GetValue(res))!;

                    }
                }
                var filteredValue = result?.ToList().Distinct();
                field = fromData ? column.Field : column.ForeignKeyField ?? column.Field;
                foreach (var obj in filteredValue ?? Enumerable.Empty<object>())
                {
                    predicates.Add(new WhereFilter()
                    {
                        Field = field,
                        Operator = "equal",
                        value = obj,
                        IgnoreCase = false
                    });
                }
            }

            if (needQuery)
            {
                return predicates.Count > 0 ? query.Where(WhereFilter.Or(predicates)) : query;
            }

            predicate = predicates.Count > 0 ? WhereFilter.Or(predicates) : new WhereFilter();
            return predicate;
        }

        /// <summary>
        /// Generates an optimized query for a specific foreign key column with active filters and search applied.
        /// This method combines filter and search criteria into a single query for efficient data retrieval.
        /// </summary>
        /// <param name="foreignKeycolumn">The foreign key column to generate query for.</param>
        /// <returns>A Task containing the generated Query object with all applicable filters and search criteria.</returns>
        internal async Task<Query> GenerateColumnQuery(GridColumn foreignKeycolumn)
        {
            var query = new Query();
            var queryColumn = IsFiltered(foreignKeycolumn);
            if (queryColumn.IsFiltered)
            {
                Parent.DataModule?.FilterQuery(query, queryColumn.Columns, true);
            }
            if (Parent.SearchModule != null)
            {
                await Parent.SearchModule.ApplyForeignKeySearch(foreignKeycolumn, query).ConfigureAwait(true);
            }

            return query;
        }

        /// <summary>
        /// Generates a where filter predicate from foreign key column data.
        /// This is used for building filter conditions during data binding operations.
        /// </summary>
        /// <param name="column">The foreign key column to generate predicate from.</param>
        /// <param name="predicate">Reference parameter that will contain the generated WhereFilter predicate.</param>
        public void GenerateQueryFormData(GridColumn column, ref WhereFilter predicate)
        {
            predicate = (GenerateQuery(column, (column.ColumnData as IEnumerable<object>)!, true, false) as WhereFilter)!;
        }

        #endregion

        #region Predicate Building

        /// <summary>
        /// Builds a list of WHERE filter predicates for foreign key columns.
        /// This method generates predicates that are used in filtering foreign key column data.
        /// </summary>
        /// <param name="column">The foreign key column to build predicates for.</param>
        /// <param name="predicateList">The existing list of predicates to append to.</param>
        /// <returns>The updated list of WhereFilter predicates including the foreign key predicate if valid.</returns>
        public List<WhereFilter> ForeignKeyPredicates(GridColumn column, List<WhereFilter> predicateList)
        {
            var fkPredicate = new WhereFilter();
            if (column != null)
            {
                this.Parent.ForeignKeyModule?.GenerateQueryFormData(column, ref fkPredicate);
                if (fkPredicate != null && (fkPredicate.predicates != null || fkPredicate.Operator != null)) //Prevent the empty predicate to avoid the exception while foreignkey data was empty
                {
                    predicateList.Add(fkPredicate);
                }
            }
            return predicateList;
        }

        #endregion

        #region Search Query Building

        /// <summary>
        /// Iterates through foreign key columns and builds search query predicates.
        /// This method processes columns that have search enabled and generates corresponding
        /// WHERE filter conditions that are combined into the main query.
        /// </summary>
        /// <param name="foreignColumns">List of foreign key columns to process.</param>
        /// <param name="predicateList">List of predicates for individual search matches.</param>
        /// <param name="finalPredicateList">List of final predicates to be combined.</param>
        /// <param name="filterpredicates">List of filter predicates from the query.</param>
        /// <param name="finalFilterpredicates">List of final filter predicates to apply.</param>
        /// <param name="query">The query object to update with combined predicates.</param>
        /// <param name="filters">The WHERE filter object to apply.</param>
        internal void IterateForeignColumns(List<GridColumn> foreignColumns, List<WhereFilter> predicateList, List<WhereFilter> finalPredicateList, List<WhereFilter> filterpredicates, List<WhereFilter> finalFilterpredicates, Query query, WhereFilter filters)
        {
            foreach (var foreignColumn in foreignColumns.Where(x => x.AllowSearching && (GridUtils.GetColumnByFColUidOrField(x.Field, foreignColumns) != null || x.ColumnData != null)))
            {
                if (foreignColumn.IsSearchQueryRequired)
                {
                    predicateList = Parent.ForeignKeyModule?.ForeignKeyPredicates(foreignColumn, predicateList) ?? predicateList;
                    if (predicateList.Count > 0 && finalPredicateList.Count > 0)
                    {
                        finalPredicateList[0].predicates?.Add(WhereFilter.Or(predicateList));
                    }
                }
            }

            if (finalPredicateList.Count > 0)
            {
                if (query.Queries.Where?.Count > 0)
                {
                    filterpredicates = query.Queries.Where;
                    filters = new WhereFilter() { Condition = "or", IsComplex = true, predicates = finalPredicateList };
                    filterpredicates.Add(filters);
                    finalFilterpredicates.Add(WhereFilter.And(filterpredicates));
                    query.Queries.Where = finalFilterpredicates;
                }
                else
                {
                    query.Where(WhereFilter.Or(finalPredicateList));
                }
            }
        }

        #endregion

        #region Validation & Checking

        /// <summary>
        /// Checks if a foreign key column currently has active filter criteria applied.
        /// Matches columns by field name or unique identifier (UID) depending on availability.
        /// </summary>
        /// <param name="foreignKeycolumn">The foreign key column to check for filter status.</param>
        /// <returns>A FilteredColumn struct containing the list of matching filter columns and filter status flag.</returns>
        internal FilteredColumn IsFiltered(GridColumn foreignKeycolumn)
        {
            var filterColumn = (Parent.FilterSettings != null && Parent.FilterSettings.Columns != null) ? Parent.FilterSettings.Columns.Where(filteredColumn =>
            {
                if (filteredColumn.Uid == null)
                {
                    return filteredColumn.Field == foreignKeycolumn.Field;
                }
                else
                {
                    return filteredColumn.Uid == foreignKeycolumn.Uid;
                }
            }).ToList() : null;
            return new FilteredColumn() { Columns = filterColumn!, IsFiltered = filterColumn != null ? filterColumn.Count > 0 : false };
        }

        /// <summary>
        /// Determines if any foreign key action (filtering or searching) needs to be performed.
        /// This method checks if there are active filters or search criteria that apply to foreign key columns.
        /// </summary>
        /// <returns>True if foreign key filtering or searching action is needed; otherwise false.</returns>
        public bool isNeedForeignKeyAction()
        {
            bool isFiltered = Parent.AllowFiltering && Parent.FilterSettings != null && Parent.FilterSettings.Columns?.Count > 0;
            bool isSearched = !string.IsNullOrEmpty(Parent.SearchSettings!.Key);
            bool fliterOrSearch = isFiltered || isSearched;
            bool needForeignKeyAction = fliterOrSearch && (Parent.SearchModule?.IsForeignKeyFiltered(GetForeignKeyColumnsAsync(Parent.Columns!)) ?? false);
            return needForeignKeyAction;
        }

        #endregion

        #region Helper Structures

        /// <summary>
        /// Asynchronously retrieves all foreign key columns from the grid.
        /// This method filters the grid columns to return only those with ForeignKeyValue configured.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning a list of GridColumn objects that have foreign key values.</returns>
        internal async Task<List<GridColumn>> GetForeignKeyColumnsAsync()
        {
            List<GridColumn> _columns = GridUtils.GetColumns(Parent);
            return await Task.FromResult<List<GridColumn>>(
                _columns.Where(_ => !string.IsNullOrEmpty(_.ForeignKeyValue)).ToList()).ConfigureAwait(true);
        }

        /// <summary>
        /// Retrieves all foreign key columns from a collection of grid columns.
        /// This static method filters columns to return only those with ForeignKeyValue configured.
        /// </summary>
        /// <param name="gridColumns">The collection of GridColumn objects to filter. Should be cast-able to List&lt;GridColumn&gt;.</param>
        /// <returns>A list of GridColumn objects that have ForeignKeyValue configured.</returns>
        internal static List<GridColumn> GetForeignKeyColumnsAsync(object gridColumns)
        {
            var foreignKeyColumns = GridUtils.GetColumns(columns: (gridColumns as List<GridColumn>)!)
                .Where(column => column.ForeignKeyValue != null).ToList();
            return foreignKeyColumns;
        }

        /// <summary>
        /// Fetches and populates foreign key data for a specific row.
        /// This method retrieves the foreign key data for each foreign key column in the row and caches it
        /// to avoid redundant lookups for the same key values.
        /// </summary>
        /// <param name="row">The row object to populate with foreign key data.</param>
        /// <param name="item">The data item containing the foreign key field values.</param>
        /// <param name="GridColumns">The list of GridColumn objects defining the grid structure and foreign key configurations.</param>
        /// <param name="distinctForeignKeyValue">A cache dictionary mapping column keys to item keys and their corresponding foreign key data.</param>
        internal static void FetchForeignKeyRow(Row<object> row, object item, List<GridColumn> GridColumns, Dictionary<object, Dictionary<object, IEnumerable<object>>> distinctForeignKeyValue)
        {
            var columns = ForeignKey<T>.GetForeignKeyColumnsAsync(GridColumns);

            foreach (var column in columns)
            {
                var columnKey = column.Field;
                var itemKey = DataUtil.GetObject(columnKey, item);
                IEnumerable<object>? foreignData = null;
                if (itemKey == null)
                {
                    continue;
                }
                if (!distinctForeignKeyValue.TryGetValue(columnKey, out var itemCache))
                {
                    itemCache = new Dictionary<object, IEnumerable<object>>();
                    distinctForeignKeyValue[columnKey] = itemCache;
                }

                if (!itemCache.TryGetValue(itemKey, out foreignData))
                {
                    if (column.ColumnData != null && column.ColumnData is IEnumerable<object> columnData && columnData.Any())
                    {
                        foreignData = GridUtils.GetForeignData(column, item, columnData, true) as IEnumerable<object>;
                    }
                    else if (column.GetForeignData() != null)
                    {
                        IEnumerable<object> foreignColumnData = (IEnumerable<object>)column.GetForeignData();
                        foreignData = GridUtils.GetForeignData(column, item, foreignColumnData, true) as IEnumerable<object>;
                    }
                    itemCache[itemKey] = foreignData!;
                }
                row.ForeignKeyData![column.Uid] = foreignData!;
            }
        }

        /// <summary>
        /// Refreshes the foreign key data for a specific row.
        /// This method re-fetches and updates all foreign key data in the row based on the current item data,
        /// clearing any cached foreign key information and repopulating it with fresh data.
        /// </summary>
        /// <param name="row">The row object whose foreign key data should be refreshed.</param>
        /// <param name="item">The data item containing the current foreign key field values.</param>
        public void RefreshForeignKeyRow(Row<object> row, object item)
        {
            var columns = ForeignKey<T>.GetForeignKeyColumnsAsync(Parent.Columns!);
            foreach (var column in columns)
            {
                var data = new object();
                if (column.ColumnData != null && (column.ColumnData as IEnumerable<object>)!.Any())
                {
                    data = GridUtils.GetForeignData(column, item, column.ColumnData);
                }

                row.ForeignKeyData?.Add(column.Uid, (data as IEnumerable<object>)!);
            }
        }

        /// <summary>
        /// Retrieves and filters the foreign key data source for a specific column.
        /// This method applies the column's foreign key configuration to extract and filter the appropriate
        /// data from the provided data set, returning only the relevant foreign key records.
        /// </summary>
        /// <param name="Column">The GridColumn object with foreign key configuration.</param>
        /// <param name="data">The source data to filter and process. Can be null or an enumerable collection of objects.</param>
        /// <returns>The filtered foreign key data source for the column, or the original data if no foreign key processing is needed.</returns>
        internal static object? GetForeignKeyDataSource(GridColumn Column, object? data)
        {
            if (Column.IsForeignColumn() && ((data as IEnumerable<object>)?.Any() ?? false))
            {
                var columnData = Column?.DataManager?.IsDataManager == true ? Column?.ColumnData : Column?.DataManager?.Json;
                var fkData = columnData as IEnumerable<object> ?? Enumerable.Empty<object>();
                data = Column!.GetForeignDataSource(Column, data!, fkData);
            }

            return data;
        }

        /// <summary>
        /// Internal structure used to represent the filtered status and associated filter columns for a foreign key column.
        /// </summary>
        internal struct FilteredColumn
        {
            public bool IsFiltered;

            public List<GridFilterColumn> Columns;
        }

        #endregion
    }
}