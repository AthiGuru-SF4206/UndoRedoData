using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Blazor.Data;
using System.Threading.Tasks;
using Syncfusion.Blazor.Internal;
using System.Globalization;
using System.Reflection;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Data module.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class DataGenerator<T>
    {
        public SfGrid<T> Parent;

        public DataGenerator(SfGrid<T> parent) => Parent = parent;

        internal virtual List<GridColumn>? _flattenedColumns { get; set; }

        #region Grid Query Generation
        public virtual Query GenerateQuery(bool skipPage = false, int VirtualStartIndex = 0, int VirtualEndIndex = 0)
        {
            Query query = Parent.Query?.Clone() ?? new Query();
            query.Queries.RequiresFilteredRecords = Parent.SelectionSettings!.PersistSelection &&
                (Parent.AllowFiltering && Parent.FilterSettings!.Columns?.Count > 0) || (Parent.SearchSettings!.Key?.Length > 0) || (Parent?.Query?.Queries?.Where != null && !Parent.IsRenderedFromTreeGrid);
            _flattenedColumns = GridUtils.GetColumns(Parent!);

            if (Parent?.ColumnQueryMode == ColumnQueryModeType.ExcludeHidden)
            {
                List<string> columns = _flattenedColumns.Where(col
                    => !(col.IsPrimaryKey != true && col.Visible == false || string.IsNullOrEmpty(col.Field)))
                    .Select(col => col.Field).ToList();
                query.Select(columns);
            }
            else if (Parent?.ColumnQueryMode == ColumnQueryModeType.Schema)
            {
                List<string> columns = _flattenedColumns.Select(col => col.Field).ToList();
                query.Select(columns);
            }

            FilterQuery(query);

            SearchQuery(query);

            AggregateQuery(query);

            SortQuery(query);

            if ((Parent != null && Parent.AllowPaging && !Parent.GroupSettings!.EnableLazyLoading) || (Parent != null && Parent.AllowPaging && (Parent.GroupSettings!.EnableLazyLoading && (Parent.GroupSettings.Columns == null || Parent.GroupSettings.Columns.Length == 0))) || (Parent != null && Parent.EnableVirtualization && (Parent.GroupSettings!.Columns == null || Parent.GroupSettings.Columns.Length == 0)) || (Parent != null && Parent.EnableInfiniteScrolling && (Parent.GroupSettings?.Columns == null || Parent.GroupSettings.Columns.Length == 0)))
            {
                if (Parent.EnableInfiniteScrolling && !skipPage && Parent.InfiniteScrollModule != null)
                {
                    Parent.InfiniteScrollModule.IntialInfinitePageQuery(query);
                }
                else
                {
                    PageQuery(query, skipPage, VirtualStartIndex, VirtualEndIndex);
                }
            }

            GroupQuery(query);

            return query;
        }
        #endregion

        #region Grid Actions Query Generation
        public void SearchQuery(Query query)
        {
            var settings = Parent.SearchSettings;
            if (settings != null && settings.Key?.Length > 0)
            {
                var foreignColumns = ForeignKey<T>.GetForeignKeyColumnsAsync(Parent.Columns!);
                foreignColumns = foreignColumns.Where(column => (column.Visible || (!column.Visible && Parent.GroupSettings?.Columns != null && Parent.GroupSettings.Columns.Contains(column.Field)))).ToList();
                var fields = settings.Fields?.Length > 0 ? settings.Fields.ToList() :
                   _flattenedColumns?.Where(column => column.AllowSearching && !string.IsNullOrEmpty(column.Field) && (column.Visible || (!column.Visible && Parent.GroupSettings?.Columns != null && Parent.GroupSettings.Columns.Contains(column.Field))))
                   .Select(column => column.Field).ToList();

                if (foreignColumns != null && foreignColumns.Count > 0)
                {
                    List<WhereFilter> finalPredicateList = new List<WhereFilter>();
                    var columnData = (foreignColumns[0].ColumnData as IEnumerable<object>)?.Count();
                    var gridColumns = Parent.Columns;
                    WhereFilter filters = new WhereFilter();
                    if (Parent.DataManager!.Adaptor == Adaptors.ODataV4Adaptor || Parent.DataManager.Adaptor == Adaptors.ODataV4Adaptor)
                    {
                        List<WhereFilter> predicate = new List<WhereFilter>();
                        predicate = IterateGridColumns(gridColumns!, predicate, settings);

                        if (predicate.Count > 0)
                        {
                            finalPredicateList.Add(WhereFilter.Or(predicate));
                        }
                    }
                    else
                    {
                        List<WhereFilter> Predicates = new List<WhereFilter>();
                        for (int i = 0; i < fields?.Count; i++)
                        {
                            Predicates.Add(new WhereFilter()
                            {
                                Field = fields[i],
                                Operator = "contains",
                                value = Parent.SearchSettings!.Key,
                                IgnoreCase = Parent.SearchSettings.IgnoreCase,
                                IgnoreAccent = Parent.SearchSettings.IgnoreAccent
                            });
                        }

                        finalPredicateList.Add(WhereFilter.Or(Predicates));
                    }

                    Parent.ForeignKeyModule?.IterateForeignColumns(foreignColumns, new List<WhereFilter>(), finalPredicateList, new List<WhereFilter>(), new List<WhereFilter>(), query, filters);
                }
                else
                {
                    // TODO: Foreign key search handling: TsLine: Data.ts->221
                    query.Search(settings.Key, fields ?? new List<string>(), settings.Operator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture), settings.IgnoreCase, settings.IgnoreAccent);
                }
            }
        }
        public void AggregateQuery(Query query)
        {
            Parent.Aggregates?.ForEach(row =>
                row.Columns?.ForEach(column =>
                {
                    AggregateType[]? types = column!.Type!.GetType().IsEnum ? new AggregateType[] { (AggregateType)column.Type }
                        : (column.Type as IEnumerable<AggregateType>)?.ToArray();
                    if (types != null)
                    {
                        foreach (var type in types)
                        {
                            query.Aggregates(column.Field!, type.ToString());
                        }
                    }
                }));
        }

        public void SortQuery(Query query)
        {
            List<GridSortColumn> columns = Parent.SortSettings?.Columns?.ToList() ?? new List<GridSortColumn>();
            var groupedColumns = Parent.GroupSettings?.Columns?.ToList();
            if (Parent.AllowGrouping && groupedColumns?.Count > 0)
            {
                for (var i = 0; i < groupedColumns.Count; i++)
                {
                    if (columns.Where(col => col.Field == groupedColumns[i]).FirstOrDefault() == null)
                    {
#pragma warning disable BL0005
                        columns.Add(new GridSortColumn() { Field = groupedColumns[i] });
#pragma warning restore BL0005
                    }
                }
            }

            var count = columns.Count;
            if ((Parent.AllowSorting || Parent.AllowGrouping) && count != 0)
            {
                List<GridColumn> gridColumns = GridUtils.GetColumns(Parent);
                List<GridSortColumn> cols = new List<GridSortColumn>();
                for (var i = count - 1; i > -1; i--)
                {
                    var field = columns[i].Field;
                    var dir = columns[i].Direction;
                    if (groupedColumns?.Where(name => name == field).FirstOrDefault() != null)
                    {
                        if (gridColumns.Any(column => column.Field == field))
                        {
#pragma warning disable BL0005
                            cols.Add(new GridSortColumn() { Field = field, Direction = dir });
#pragma warning restore BL0005
                        }
                    }
                    else
                    {
                        List<GridColumn> comp = gridColumns.Where(x => !string.IsNullOrEmpty(x.Field) ? x.Field.Equals(field, StringComparison.Ordinal) : false).ToList();
                        object? comparer = null;
                        if (comp != null && comp.Count > 0 && comp[0].ForeignKeySorting != null)
                        {
                            comparer = (!(Parent.DataManager!.DataAdaptor!.IsRemote()) && Parent.DataSource != null && !(Parent.DataSource.GetType().Name.Contains("DbSet", StringComparison.Ordinal))) ? comp[0].ForeignKeySorting : null!;
                        }
                        else
                        {
                            comparer = (comp?.Count > 0 && !(Parent.DataManager!.DataAdaptor!.IsRemote()) && Parent.DataSource != null && !(Parent.DataSource.GetType().Name.Contains("DbSet", StringComparison.Ordinal))) ? comp[0].SortComparer : null!;
                        }
                        query.Sort(field, dir.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture), comparer!);
                    }
                }

                cols.ForEach(sorts => {
                    List<GridColumn> comp = gridColumns.Where(x => !string.IsNullOrEmpty(x.Field) ? x.Field.Equals(sorts.Field, StringComparison.Ordinal) : false).ToList();
                    object? comparer = null;
                    if (comp != null && comp.Count > 0 && comp[0].ForeignKeySorting != null)
                    {
                        comparer = (!(Parent.DataManager!.DataAdaptor!.IsRemote()) && Parent.DataSource != null && !(Parent.DataSource.GetType().Name.Contains("DbSet", StringComparison.Ordinal))) ? comp[0].ForeignKeySorting : null!;
                    }
                    else
                    {
                        comparer = (comp != null && comp.Count > 0 && !(Parent.DataManager!.DataAdaptor!.IsRemote()) && Parent.DataSource != null && !(Parent.DataSource.GetType().Name.Contains("DbSet", StringComparison.Ordinal))) ? comp[0].SortComparer : null!;
                    }
                    query.Sort(sorts.Field, sorts.Direction.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture), comparer!);
                });
            }
        }

        public void PageQuery(Query query, bool skipPage, int VirtualStartIndex = 0, int VirtualEndIndex = 0)
        {
            if (Parent.EnableVirtualization && VirtualEndIndex != 0)
            {
                query.Range(VirtualStartIndex, VirtualEndIndex - VirtualStartIndex);
            }
            else if ((Parent.AllowPaging || Parent.EnableVirtualization) && !skipPage && Parent.PageSettings != null)
            {
                query.Page(Parent.PageSettings.CurrentPage, Parent.PageSettings.PageSize);
            }
        }

        public void GroupQuery(Query query)
        {
            var columns = Parent.GroupSettings?.Columns?.ToList();
            if (Parent.GroupSettings != null && Parent.GroupSettings.EnableLazyLoading)
            {
                query.Queries.LazyLoad = (Parent.GroupModule != null && Parent.GroupModule.IsLazyExpandAll) ? false : Parent.GroupSettings.EnableLazyLoading;
                query.Queries.LazyExpandAllGroup = (Parent.GroupModule != null && Parent.GroupModule.IsLazyExpandAll);
            }
            if (Parent.AllowGrouping && columns != null && columns.Count != 0)
            {
                var gCols = new List<string>();
                var groupFormatter = new Dictionary<string, string>();
                columns.ForEach(col =>
                {
                    var gColumn = _flattenedColumns?.FirstOrDefault(_ => _.Field == col);
                    if (gColumn != null)
                    {
                        gCols.Add(gColumn.Field);
                        if (gColumn.EnableGroupByFormat)
                        {
                            groupFormatter.Add(gColumn.Field, gColumn.Format!);
                        }
                    }
                });
                if (gCols.Count != 0)
                {
                    query.Group(gCols, groupFormatter);
                }
            }
        }

        public void FilterQuery(Query query, List<GridFilterColumn>? column = null, bool skipForeign = false)
        {
            var PredicateList = new List<WhereFilter>();
            List<GridFilterColumn> filteredColumns = new List<GridFilterColumn>();
            if (Parent.AllowFiltering && Parent.FilterSettings != null && Parent.FilterSettings.Columns?.Count > 0)
            {
                var columns = column ?? Parent.FilterSettings.Columns;
                Dictionary<string, object> colType = new Dictionary<string, object>();
                foreach (var col in GridUtils.GetColumns(Parent))
                {
                    colType.AddOrUpdateItem(col.Field, Parent.FilterSettings.Type.ToString());
                }
                if (Parent.IsStackedHeader)
                {
                    foreach (var col in Parent.FilterSettings.Columns)
                    {
                        foreach (var cols in GridUtils.GetColumns(Parent))
                        {
                            if (cols.Field == col.Field && col.Uid == null)
                            {
                                col.Uid = cols.Uid;
                            }
                        }
                    }
                }
                var foreignCols = new List<GridFilterColumn>();
                var defaultFltrCols = new List<GridFilterColumn>();
                AddColumns(columns, foreignCols, defaultFltrCols);

                if (defaultFltrCols.Count > 0)
                {
                    for (int i = 0, len = defaultFltrCols.Count; i < len; i++)
                    {
                        if (defaultFltrCols[i].Value is System.Text.Json.JsonElement jsonElement)
                        {
                            var field = defaultFltrCols[i].Field;
                            var value = defaultFltrCols[i].Value;
                            var gridColumn = GridUtils.GetColumnByField(field, Parent.Columns!);
                            var type = gridColumn?.IsForeignColumn() == true ? gridColumn.ActualType : gridColumn?.ValueType;
                            if (type != null && (type.GetTypeInfo() == typeof(DateTime) || type.GetTypeInfo() == typeof(DateTime?)
                                || type.GetTypeInfo() == typeof(DateOnly) || type.GetTypeInfo() == typeof(DateOnly?)
                                || type.GetTypeInfo() == typeof(TimeOnly) || type.GetTypeInfo() == typeof(TimeOnly?)
                                || type.GetTypeInfo() == typeof(DateTimeOffset) || type.GetTypeInfo() == typeof(DateTimeOffset?)
                                || jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number || type.GetTypeInfo() == typeof(bool) || type.GetTypeInfo() == typeof(bool?)))
                            {
#pragma warning disable BL0005 //Component parameter should not be set outside of its component.
                                defaultFltrCols[i].Value = GridUtils.ParseJsonElementToEnum(defaultFltrCols[i], Parent)!;
#pragma warning restore BL0005
                            }
                            else
                            {
#pragma warning disable BL0005 //Component parameter should not be set outside of its component.
                                defaultFltrCols[i].Value = GridUtils.ParseJsonElementToEnum(defaultFltrCols[i], Parent)!.ToString()!;
#pragma warning restore BL0005
                            }
                        }

                        if (string.Equals(colType[defaultFltrCols[i].Field], "FilterBar") || string.Equals(colType[defaultFltrCols[i].Field], "Menu"))
                        {
                            defaultFltrCols[i].Uid = GridUtils.grabColumnByUidOrField(null!, Parent, defaultFltrCols[i].Field)?.Uid ?? defaultFltrCols[i].Uid;
                        }
                    }
                    Dictionary<string, WhereFilter> excelPredicate = GetPredicate(defaultFltrCols);
                    foreach (var prop in excelPredicate.Keys)
                    {
                        PredicateList.Add(excelPredicate[prop]);
                    }
                }
                if (foreignCols.Count > 0)
                {
                    GridColumn? col = null;
                    for (int i = 0, len = foreignCols.Count; i < len; i++)
                    {
                        if (string.Equals(Parent.FilterSettings.Type.ToString(), "FilterBar", StringComparison.Ordinal) || string.Equals(Parent.FilterSettings.Type.ToString(), "Menu", StringComparison.Ordinal))
                        {
                            foreignCols[i].Uid = foreignCols[i].Uid ?? GridUtils.grabColumnByUidOrField(null!, Parent, foreignCols[i].Field)?.Uid!;
                        }
                    }
                    Dictionary<string, WhereFilter> excelPredicate = GetPredicate(foreignCols);
                    var foreignColumns = ForeignKey<T>.GetForeignKeyColumnsAsync(this.Parent.Columns!);
                    PredicateList = UpdatePredicateList(PredicateList, excelPredicate, col!, foreignColumns, skipForeign);
                }

                if (PredicateList.Count > 0)
                {
                    query.Where(WhereFilter.And(PredicateList));
                }
                else
                    Parent.IsEmptyGrid = true; //we can return the emptygrid by enabling this value
                if (defaultFltrCols.Count > 0 || foreignCols.Count > 0)
                {
                    // Combine and create the set of fields in one step
                    HashSet<string> columnSet = new HashSet<string>(defaultFltrCols.Select(f => f.Field).Concat(foreignCols.Select(f => f.Field)));
                    // Filter the columns using a single iteration over FilterSettings.Columns
                    filteredColumns = Parent.FilterSettings.Columns
                                               .Where(col => columnSet.Contains(col.Field))
                                               .ToList();
                    if (filteredColumns.Count > 0)
                    {
                        Parent.PageModule?.UpdateFilterMessage(filteredColumns, "and");
                    }
                }
            }
        }
        #endregion

        #region Filter Query Generation Helper Methods
        private static Dictionary<string, WhereFilter> GetPredicate(List<GridFilterColumn> columns)
        {
            var cols = Distinct(columns, "Uid", true);
            List<GridFilterColumn> collection;
            Dictionary<string, WhereFilter> pred = new Dictionary<string, WhereFilter>();
            foreach (var col in cols)
            {
                string Key;
                if (col.Uid != null)
                {
                    collection = columns.Where(c => c.Uid == col.Uid).ToList();
                    Key = col.Uid;
                }
                else
                {
                    collection = columns.Where(c => c.Field == col.Field).ToList();
                    Key = col.Field;
                }
                if (collection.Count != 0)
                {
                    pred[Key] = GeneratePredicate(collection);
                }
            }
            return pred;
        }

        private List<WhereFilter> UpdatePredicateList(List<WhereFilter> PredicateList, Dictionary<string, WhereFilter> excelPredicate, GridColumn col, List<GridColumn> foreignColumns, bool skipForeign)
        {
            List<WhereFilter> predicates = new List<WhereFilter>();
            foreach (var prop in excelPredicate.Keys)
            {
                col = GridUtils.GetColumnByFColUidOrField(prop, foreignColumns)!;
                if (col != null && !skipForeign)
                {
                    predicates = Parent.ForeignKeyModule?.ForeignKeyPredicates(col, predicates) ?? predicates;
                    if (predicates.Count > 0)
                    {
                        excelPredicate[prop].Condition = "and";
                        excelPredicate[prop].IsComplex = true;
                        excelPredicate[prop].predicates = predicates;
                        PredicateList.Add(excelPredicate[prop]);
                    }
                    else
                    {
                        PredicateList.Add(excelPredicate[prop]);
                    }
                }
                else
                {
                    PredicateList.Add(excelPredicate[prop]);
                }
            }
            return PredicateList;
        }

        private static List<WhereFilter> ValidateTypes(object searchValue, List<WhereFilter> predicate, List<GridColumn> gridColumns, Type type, GridSearchSettings settings, int i)
        {

            if (settings.Key == null)
                return predicate;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type)!;

            try
            {
                var convertedValue = Convert.ChangeType(settings.Key, type!, CultureInfo.InvariantCulture);
                predicate.Add(new WhereFilter() { Field = gridColumns[i].Field, IgnoreCase = false, value = convertedValue, Operator = "equal" });
            }
            catch (FormatException e)
            {
                // ignore invalid values
                Console.WriteLine(e);
            }

            return predicate;
        }

        private static List<WhereFilter> IterateGridColumns(List<GridColumn> gridColumns, List<WhereFilter> predicate, GridSearchSettings settings)
        {
            int i = -1;
            foreach (var column in gridColumns)
            {
                var type = column.IsForeignColumn() ? column.ActualType : column.ValueType;

                if (type == typeof(string) && !Guid.TryParse(settings?.Key?.ToString(), out Guid gs))
                {
                    predicate.Add(new WhereFilter() { Field = column.Field, IgnoreCase = true, value = settings?.Key, Operator = "contains" });
                }
                else if (type == typeof(int) || type == typeof(int?) || type == typeof(uint) || type == typeof(uint?) || type == typeof(double) || type == typeof(double?) || type == typeof(decimal) || type == typeof(decimal?) || type == typeof(Guid) || type == typeof(Guid?) || type == typeof(bool) || type == typeof(bool?))
                {
                    object? searchValue = settings?.Key;
                    var valueAdded = false;

                    if (type == typeof(int) || type == typeof(int?))
                    {
                        if (int.TryParse(settings?.Key?.ToString(), out int intValue))
                        {
                            searchValue = Convert.ToInt32(settings.Key, CultureInfo.InvariantCulture);
                            valueAdded = true;
                        }
                    }
                    else if (type == typeof(uint) || type == typeof(uint?))
                    {
                        if (uint.TryParse(settings?.Key?.ToString(), out uint uintValue))
                        {
                            searchValue = Convert.ToUInt32(settings.Key, CultureInfo.InvariantCulture);
                            valueAdded = true;
                        }
                    }
                    else if (type == typeof(double) || type == typeof(double?))
                    {
                        if (double.TryParse(settings?.Key?.ToString(), out double doubleValue))
                        {
                            searchValue = Convert.ToDouble(settings.Key, CultureInfo.InvariantCulture);
                            valueAdded = true;
                        }
                    }

                    if (valueAdded)
                        predicate.Add(new WhereFilter() { Field = column.Field, IgnoreCase = false, value = searchValue, Operator = "equal" });
                    else
                        predicate = ValidateTypes(searchValue!, predicate, gridColumns, type, settings!, i++);
                }
                else if (type != null && type.IsEnum && Enum.TryParse(typeof(T).GetProperty(column.Field)?.PropertyType!, settings?.Key?.ToString(), out var newenum))
                    predicate.Add(new WhereFilter() { Field = column.Field, IgnoreCase = true, value = Enum.Parse(typeof(T).GetProperty(column.Field)?.PropertyType!, settings?.Key!), Operator = "equal" });
            }

            return predicate;
        }

        private void AddColumns(List<GridFilterColumn> columns, List<GridFilterColumn> foreignCols, List<GridFilterColumn> defaultFltrCols)
        {
            foreach (var col in columns)
            {
                foreach (var cols in GridUtils.GetColumns(Parent))
                {
                    if (cols.IsForeignColumn() && cols.Uid == col.Uid)
                    {
                        if (cols.ForeignKeyValue == col.Field)
                        {
                            foreignCols.Add(col);
                        }
                        else
                            defaultFltrCols.Add(col);
                    }
                    else if (cols.Field == col.Field && cols.Uid == col.Uid)
                    {
                        defaultFltrCols.Add(col);
                    }
                }
            }
        }

        public static List<GridFilterColumn> Distinct(List<GridFilterColumn> json, string fieldName, Boolean requiresCompleteRecord = false)
        {
            var result = new List<GridFilterColumn>();
            object val;
            var tmp = new List<string>();
            foreach (var col in json)
            {
                val = fieldName != null ? DataUtil.GetObject(fieldName, col) : col;
                if (val == null)
                {
                    fieldName = fieldName == "Uid" ? "Field" : "Uid";
                    val = DataUtil.GetObject(fieldName, col);
                }
                if (val?.ToString() != null && !tmp.Contains(val.ToString()!))
                {
                    result.Add(col);
                    tmp.Add(val.ToString()!);
                }
            }
            return result;
        }

        public static WhereFilter GeneratePredicate(List<GridFilterColumn> cols)
        {
            var length = cols?.Count ?? 0;
            if (length == 0) return null!;

            var first = cols![0];
            var predicate = new WhereFilter()
            {
                Field = first.Field,
                Operator = first.Operator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture),
                value = first.Value,
                IgnoreCase = !first.MatchCase,
                IgnoreAccent = first.IgnoreAccent,
                ColumnType = first.ColumnType,
            };

            for (var p = 1; p < length; p++)
            {
                if (length > 2 && p > 1 && cols[p]?.Predicate == "or")
                {
                    predicate.predicates?.Add(new WhereFilter()
                    {
                        Field = cols[p].Field,
                        Operator = cols[p].Operator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture),
                        value = cols[p].Value,
                        IgnoreCase = !cols[p].MatchCase,
                        IgnoreAccent = cols[p].IgnoreAccent,
                        ColumnType = cols[p].ColumnType
                    });
                }
                else
                {
                    if (cols[p]?.Predicate == "and")
                    {
                        if (cols[p].ColumnType == "DateTime")
                        {
                            predicate = predicate.And(cols[p].Field, cols[p].Operator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture), cols[p].Value, !cols[p].MatchCase, cols[p].IgnoreAccent, cols[p].ColumnType);
                        }
                        else
                        {
                            predicate = predicate.And(cols[p].Field, cols[p].Operator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture), cols[p].Value!, !cols[p].MatchCase, cols[p].IgnoreAccent);
                        }
                    }
                    else if (cols[p]?.Predicate == "or")
                    {
                        if (cols[p].ColumnType == "DateTime")
                        {
                            predicate = predicate.Or(cols[p].Field, cols[p].Operator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture), cols[p].Value, !cols[p].MatchCase, cols[p].IgnoreAccent, cols[p].ColumnType);
                        }
                        else
                        {
                            predicate = predicate.Or(cols[p].Field, cols[p].Operator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture), cols[p].Value!, !cols[p].MatchCase, cols[p].IgnoreAccent);
                        }
                    }
                }
            }
            return predicate;
        }

        #endregion

        #region CRUD Query Helper Methods
        public async Task<bool> GetData(ActionEventArgs<T> args = null!, object? eventArgs = null, string? requestType = null)
        {
            switch (args?.RequestType.ToString() ?? requestType)
            {
                case "Save":
                    try
                    {
                        if ((args != null && args.Action == "Add") || (eventArgs is RowUpdatingEventArgs<T> saveEventArgs && Parent.EditModule!.IsAdd))
                        {
                            if (Parent.GridEvents?.OnActionBegin.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
                            {
                                ActionEventArgs<object> updateArgs = new ActionEventArgs<object>() { Data = args!.Data! };
                                Parent.EventAggregator.Trigger("DataManagerCRUD", updateArgs);
                                await this.Parent!.DataManager!.Insert<T>(updateArgs.Data, Parent.Query?.FromTable!, Parent.Query!, (int)args.Index).ConfigureAwait(true);
                            }
                            else if (eventArgs != null && eventArgs is RowUpdatingEventArgs<T> savedEventArgs)
                            {
                                RowUpdatingEventArgs<object> savingEventArgs = new RowUpdatingEventArgs<object>() { Data = savedEventArgs.Data!, Action = SaveActionType.Added };
                                Parent.EventAggregator.Trigger("DataManagerCRUD", savingEventArgs);
                                await this.Parent!.DataManager!.Insert<T>(savingEventArgs.Data, Parent.Query?.FromTable!, Parent.Query!, (int)savedEventArgs.Index).ConfigureAwait(true);
                            }

                        }
                        else if ((args != null && args.Action == "Edit") || (eventArgs is RowUpdatingEventArgs<T> saveArgs))
                        {
                            if (Parent.GridEvents?.OnActionBegin.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
                            {
                                ActionEventArgs<object> updateArgs = new ActionEventArgs<object>() { Data = this.Parent?.EditModule?.CloneData!, PreviousData = args!.PreviousData! };
                                Parent!.EventAggregator.Trigger("DataManagerCRUD", updateArgs);
                                await this.Parent!.DataManager!.Update<T>(args.PrimaryKeys![0], updateArgs.Data, Parent.Query?.FromTable!, Parent.Query!, updateArgs.PreviousData).ConfigureAwait(true)!;
                            }
                            else if (eventArgs != null && eventArgs is RowUpdatingEventArgs<T> savingEvent)
                            {
                                RowUpdatingEventArgs<object> savingEventArgs = new RowUpdatingEventArgs<object>() { Data = this.Parent.EditModule!.CloneData!, PreviousData = savingEvent.PreviousData!, Action = SaveActionType.Edited };
                                Parent.EventAggregator.Trigger("DataManagerCRUD", savingEventArgs);
                                await this.Parent!.DataManager!.Update<T>(savingEvent.PrimaryKeys![0], savingEventArgs.Data, Parent.Query?.FromTable!, Parent.Query!, savingEventArgs.PreviousData).ConfigureAwait(true);
                            }
                        }
                        this.Parent.IsEdit = false;
                        Parent.Rows.ForEach(Row => Row.IsEdit = false);
                        return true;
                    }
                    catch (Exception e) when (HandleException(e))
                    {
                        return false;
                    }
                case "Delete":
                    try
                    {
                        var primaryKey = (await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true))[0];
                        var primaryKeyValues = new List<object>();
                        RowDeletingEventArgs<T>? deletingEvent = eventArgs as RowDeletingEventArgs<T>;
                        primaryKeyValues.Add(Parent.PropHelper!.GetObject(primaryKey, args != null ? args.Data! : deletingEvent!.Datas![0]));
                        if ((Parent.SelectedRecords.Count == 1 || (Parent.SelectedRecords.Count == 0 && ((args != null && args.Data != null) || ((deletingEvent?.Datas != null) && deletingEvent.Datas.Count > 0)))) && !(Parent.SelectionModule?.ClonedSelectedRowRecords?.Count > 1))
                        {
                            await this.Parent!.DataManager!.Remove<T>(primaryKey, primaryKeyValues[0], Parent.Query?.FromTable!, Parent.Query!).ConfigureAwait(true);
                        }
                        else
                        {
                            await this.Parent!.DataManager!.SaveChanges<T>(new List<T>(), new List<T>(), Parent.SelectionModule?.ClonedSelectedRowRecords!, primaryKey, null, Parent.Query?.FromTable!, Parent.Query!).ConfigureAwait(true);
                        }
                        return true;
                    }
                    catch (Exception e) when (HandleException(e))
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }

        public async Task<bool> SaveChanges(BatchChanges<T> Changes, string Key)
        {
            try
            {
                await this.Parent!.DataManager!.SaveChanges<T>(Changes.ChangedRecords,
                Changes.AddedRecords, Changes.DeletedRecords, Key, null, Parent.Query?.FromTable!, Parent.Query!).ConfigureAwait(true);
                return true;
            }
            catch (Exception e) when (HandleException(e))
            {
                return false;
            }
        }
        #endregion

        #region Exception Handling
        private bool HandleException(Exception e)
        {
            // Fire and forget pattern for async event notification
            _ = NotifyExceptionAsync(e);
            return true;
        }
        private async Task NotifyExceptionAsync(Exception e)
        {
            if (Parent.GridEvents?.OnActionFailure.HasDelegate == true)
                await Parent.GridEvents.OnActionFailure.InvokeAsync(new FailureEventArgs() { Error = e, Parent = Parent }).ConfigureAwait(true);
            else if (Parent.IsRenderedFromTreeGrid)
                await Parent.EventAggregator.NotifyAsync("ActionFailure", new FailureEventArgs() { Error = e, Parent = Parent }).ConfigureAwait(true);
        }
        #endregion
    }
}
