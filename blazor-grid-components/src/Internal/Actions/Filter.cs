using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Internal;
using Syncfusion.Blazor.Popups;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles filter operation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Filter<T>
    {
        #region Private Fields

        private SfGrid<T> Parent { get; set; }

        private PredicateModel<object>? _currentFilterObject { get; set; }

        #endregion

        #region Internal Properties

        internal bool ExcelDialog { get; set; }

        internal bool FilterIconIsClicked { get; set; }

        internal bool IsColumnMenuFilter { get; set; }

        internal SfDialog? FilterDialogInstance { get; set; }

        internal GridColumn? FilterIconColumn { get; set; }

        internal bool IsSubMenuClick { get; set; }

        internal bool IsAdaptiveFilter { get; set; }
        internal bool IsAdaptiveSort { get; set; }
        internal bool IsAdaptiveToolbarMenu { get; set; }
        internal bool ShouldTemplateDispose { get; set; }
        internal bool IsCustomFilterApplied { get; set; }

        internal string FilteredValue { get; set; } = string.Empty;

        internal string RawInputValue { get; set; } = string.Empty;

        internal string FilterOperatorProperty { get; set; } = string.Empty;

        internal Dictionary<Type, Type> IntConvertedList = new Dictionary<Type, Type>()
        {
            {typeof(int),typeof(int)},
            {typeof(int?),typeof(int?)},
        };
        internal Dictionary<Type, Type> LongConvertedList = new Dictionary<Type, Type>()
        {
            {typeof(Int64),typeof(Int64)},
            {typeof(Int64?),typeof(Int64?)}
        };

        #endregion

        #region Constructor

        public Filter(SfGrid<T> parent) => Parent = parent;

        #endregion

        #region Core Filter Operations

        internal async Task FilterByColumn(string FieldName, Operator FilterOperator, object FilterValue,
                string? Predicate = null, Nullable<bool> MatchCase = null, Nullable<bool> IgnoreAccent = null,
                object? ActualFilterValue = null, object? ActualOperator = null, string? Uid = null, string? InputField = null)
        {
            if (Parent.SelectedRecords?.Count > 0 && !Parent.SelectionSettings!.PersistSelection)
            {
                await Parent.ClearSelectionAsync().ConfigureAwait(true);
            }

            if (string.Equals(FilterOperator.ToString(), "IsEmpty", StringComparison.Ordinal) || string.Equals(FilterOperator.ToString(), "IsNotEmpty", StringComparison.Ordinal))
            {
                FilterValue = "";
            }

            object? actualFilerValue = ActualFilterValue;
            object? actualOperator = ActualOperator;
            List<GridColumn> GridColumns = GridUtils.GetColumns(Parent);
            GridColumn? _column = Uid != null ? GridUtils.GetColumnByFColUidOrField(Uid, GridColumns) : GridUtils.GetColumnByField(FieldName, GridColumns);
            object _value = FilterValue;
            bool _matchCase = (bool)(MatchCase ?? Parent?.FilterSettings?.EnableCaseSensitivity ?? false);
            bool _ignoreAccent = IgnoreAccent != null ? (bool)IgnoreAccent : false;
            Operator _operator = FilterOperator;
            List<GridFilterColumn> FilterColumn = new List<GridFilterColumn>();
            List<PredicateModel<object>> PredicateModels = new List<PredicateModel<object>>();
            List<object> FCollection = [_value];
            bool isCollection = _value != null && IsCollection(_value.GetType());
            var filterValues = isCollection ? ((IEnumerable)_value!) : FCollection;
#pragma warning disable BL0005
            if (Parent != null && Parent.FilterSettings!.Columns == null)
            {
                Parent.FilterSettings.Columns = new List<GridFilterColumn>();
            }

            Predicate = !string.IsNullOrEmpty(Predicate) ? Predicate : (isCollection && _operator != Operator.NotEqual ? "or" : "and");
            var field = _column?.IsForeignColumn() == true ? _column.ForeignKeyValue : FieldName;
            var removeFilterColumns = Parent?.FilterSettings!.Columns?.Where(col => col.Field == field && col.Uid == _column?.Uid).ToList();
            foreach (var col in removeFilterColumns!)
            {
                Parent?.FilterSettings!.Columns?.Remove(col);
            }
            foreach (var Fval in filterValues)
            {
                var filterColumn = BuildFilterColumn(field!, FilterOperator, Fval, _matchCase, _ignoreAccent, _column, Predicate, _value!, InputField);
                _currentFilterObject = BuildPredicateModel(field!, FilterOperator, Fval, _matchCase, _ignoreAccent, _column, Predicate, _value!);
                FilterColumn.Add(filterColumn);
                PredicateModels.Add(_currentFilterObject);
                var index = GetFilteredColsIndexByField(filterColumn);
                Parent?.FilterSettings?.Columns?.Add(filterColumn);
            }
            await UpdateFilterColumnsAsync().ConfigureAwait(true);
            if (Parent?.PagerRef != null && Parent?.FilterSettings?.Columns is { } columns)
            {
                Parent.PageModule?.UpdateFilterMessage(Parent.FilterSettings.Columns!, Predicate);
            }
            await Parent!.ModelChanged(new ActionEventArgs<T>() { CurrentFilterObject = _currentFilterObject!, CurrentFilteringColumn = _column?.Field!, RequestType = Action.Filtering }, eventArgs: new FilteringEventArgs() { FilterPredicates = PredicateModels, ColumnName = _column?.Field! }, requestType: "Filtering").ConfigureAwait(true);
            if (Parent.PageModule != null)
            {
                await Parent.PageModule.UpdatePageSizes().ConfigureAwait(true);
            }
        }

        internal async Task ClearFiltering(object Fields = null!)
        {
            FilteredValue = string.Empty;
            if (Fields != null)
            {
                bool isCollection = IsCollection(Fields.GetType());
                List<object> FCollection = new List<object>();
                FCollection.Add(Fields);
                var FilterValues = isCollection ? ((IEnumerable)Fields) : FCollection;
                foreach (var Fval in FilterValues)
                {
                    await RemoveFilterColumnByField(Fval?.ToString()!).ConfigureAwait(true);
                }
            }
            else if (Parent.FilterSettings != null && Parent.FilterSettings.Columns != null)
            {
                List<string> filteredColumns = new List<string>();
                foreach (var filedName in Parent.FilterSettings.Columns)
                {
                    filteredColumns.Add(filedName.Field);
                }

#pragma warning disable BL0005
                Parent.FilterSettings.Columns = new List<GridFilterColumn>();
                if (Parent.FilterSettings.Type == FilterType.FilterBar && Parent.PagerRef != null)
                {
                    Parent.PagerRef.ExternalMessage = string.Empty;
                }

                await UpdateFilterColumnsAsync().ConfigureAwait(true);
                if (Parent.SelectionModule != null && Parent.SelectionModule.IsSelectFilteredField.Equals((string)Fields!, StringComparison.Ordinal))
                {
                    Parent.SelectionModule.IsHeaderCheckboxChecked = false;
                    Parent.SelectionModule.IsSelectFilteredField = string.Empty;
                }
                await Parent.ModelChanged(new ActionEventArgs<T>() { RequestType = Action.ClearFiltering }, requestType: "ClearFiltering", eventArgs: new FilteringEventArgs() { }).ConfigureAwait(true);
            }
        }

        internal async Task ApplyPreventFilterQueryAsync(ActionEventArgs<T>? args, object? eventArgs = null, string? requestType = null)
        {
            if ((args?.RequestType.Equals(Action.Filtering) == true || requestType == "Filtering")
                && (args?.CurrentFilteringColumn != null || (eventArgs as FilteringEventArgs)?.ColumnName != null)
                && Parent.FilterSettings?.Columns != null)
            {
                FilteringEventArgs? filteringEventArgs = eventArgs as FilteringEventArgs;

                var columnName = args?.CurrentFilteringColumn
                                 ?? filteringEventArgs?.ColumnName;

                var fGridcolumn = await Parent.GetColumnByFieldAsync(columnName!).ConfigureAwait(true);

                if (fGridcolumn != null)
                {
                    var prevent = args?.PreventFilterQuery
                                  ?? filteringEventArgs?.PreventFilterQuery
                                  ?? false;

                    fGridcolumn.PreventFilterQuery = prevent;

                    foreach (var col in Parent.FilterSettings.Columns)
                    {
                        if (col.Field == fGridcolumn.Field || col.Field == fGridcolumn.ForeignKeyValue)
                        {
                            col.PreventFilterQuery = prevent;
                        }
                    }

                    Parent.FilteredColumns = Parent.FilterSettings.Columns.ToList();
                }
            }


            if (((args != null && args.RequestType == Action.Refresh) || requestType == "Refresh") && Parent.Columns?.Any(f => f.FilterTemplate != null) == true && Parent.FilterSettings?.Columns != null)
            {
                Parent.FilteredColumns = Parent.FilterSettings.Columns.ToList();
            }

            var filteredCols = Parent.FilterSettings?.Columns?.Where(col => col.PreventFilterQuery).ToList();

            if (filteredCols != null)
            {
                foreach (var col in filteredCols)
                {
                    Parent.FilterSettings?.Columns?.Remove(col);
                }
            }
        }


        #endregion

        #region Filter Model Binding

        internal static void UpdateFilterModel(object modelInstance, GridFilterColumn Col, object? value = null, Type? columnValueType = null, bool hasPersistence = false)
        {
            Type? filterModelType = modelInstance?.GetType();
            var filterValue = filterModelType?.GetProperty("Value")?.GetValue(modelInstance);

            if (filterValue == null || columnValueType?.BaseType != null && columnValueType.BaseType.Name.Equals("Enum", StringComparison.Ordinal) && ((int)filterValue) == 0)
            {
                var enumName = hasPersistence && columnValueType?.BaseType?.Name.Equals("Enum", StringComparison.Ordinal) == true ? Enum.GetName(columnValueType, Convert.ToInt32(value, CultureInfo.CurrentCulture)) : null;
                PropertyInfo? enumValueProperty = filterModelType?.GetProperty("Value");
                if (enumValueProperty?.PropertyType.IsEnum == true && enumName != null)
                {
                    var enumObject = Enum.Parse(enumValueProperty.PropertyType, enumName);
                    enumValueProperty.SetValue(modelInstance, enumObject);
                }
                else
                {
                    enumValueProperty?.SetValue(modelInstance, value);
                }
            }
            Col.ActualValue = Col.ActualValue?.GetType().Name == "JsonElement" ? SfBaseUtils.ChangeType(Col.ActualValue, columnValueType) : Col.ActualValue!;
            filterModelType?.GetProperty("ActualValue")?.SetValue(modelInstance, Col.ActualValue);
            filterModelType?.GetProperty("Field")?.SetValue(modelInstance, Col.Field);
            filterModelType?.GetProperty("Operator")?.SetValue(modelInstance, Col.Operator);
            filterModelType?.GetProperty("MatchCase")?.SetValue(modelInstance, Col.MatchCase);
            filterModelType?.GetProperty("IgnoreAccent")?.SetValue(modelInstance, Col.IgnoreAccent);
            filterModelType?.GetProperty("Predicate")?.SetValue(modelInstance, Col.Predicate);
            filterModelType?.GetProperty("Uid")?.SetValue(modelInstance, Col.Uid);
        }

        #endregion

        #region Type Detection

        private static bool IsCollection(Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition().Equals(typeof(List<>)) || type.GetGenericTypeDefinition().Equals(typeof(ICollection<>)) || type.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)) || type.IsAssignableFrom(typeof(IEnumerable))))
            {
                return true;
            }
            else if (type.IsArray)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region UI & Pager Integration

        internal void HideColumnMenuPopup()
        {
            Parent.EventAggregator.Trigger("HideColumnMenuPopup", null!);
        }

        internal async Task RemoveFilterColumnByField(string FieldName, string uid = null!, string foreginKeyFieldName = null!)
        {
            List<GridColumn> GridColumns = GridUtils.GetColumns(Parent);
            var column = uid != null ? GridUtils.GetColumnByFColUidOrField(uid, GridColumns) : GridUtils.GetColumnByField(FieldName, GridColumns);
            var FilterUid = column?.Uid;
            if (Parent.FilterSettings?.Columns != null)
            {
                var count = Parent.FilterSettings.Columns.Count;
                while (count > 0)
                {
                    count--;
                    var GCol = Parent.FilterSettings.Columns[count];
                    if (GCol.Uid == FilterUid)
                    {
                        _currentFilterObject = new PredicateModel<object>()
                        {
#pragma warning disable BL0005
                            Field = GCol.Field,
                            Operator = GCol.Operator,
                            Value = GCol.Value,
                            IgnoreAccent = GCol.IgnoreAccent,
                            Uid = GCol.Uid,
                            Predicate = GCol.Predicate,
                            ActualValue = null!
#pragma warning restore BL0005
                        };
                        Parent.FilterSettings.Columns.Remove(GCol);
                        Parent.PageModule?.UpdateFilterMessage(Parent.FilterSettings.Columns, GCol.Predicate!);
                    }
                }
            }

            await UpdateFilterColumnsAsync().ConfigureAwait(true);
            if (!Parent.SelectionSettings!.PersistSelection && Parent.SelectionModule != null && Parent.Rows?.Where(_ => _.IsSelected).Any() == true)
            {
                await Parent.SelectionModule.ClearSelection().ConfigureAwait(true);
            }

            if (Parent.SelectionModule != null && Parent.SelectionModule.IsSelectFilteredField.Equals(foreginKeyFieldName ?? FieldName, StringComparison.Ordinal) && !string.IsNullOrEmpty(Parent.SelectionModule.IsSelectFilteredField))
            {
                Parent.SelectionModule.IsHeaderCheckboxChecked = false;
                Parent.SelectionModule.IsSelectFilteredField = string.Empty;
            }
            await Parent.ModelChanged(new ActionEventArgs<T>() { CurrentFilterObject = _currentFilterObject!, RequestType = Action.ClearFiltering }, requestType: "ClearFiltering", eventArgs: new FilteringEventArgs() { ColumnName = FieldName }).ConfigureAwait(true);
            if (Parent.PageModule != null)
            {
                await Parent.PageModule.UpdatePageSizes().ConfigureAwait(true);
            }
        }

        private string UpdateColumnFormat(GridFilterColumn filteredColumn)
        {
            var gridColumn = GridUtils.GetColumnByFColUidOrField(filteredColumn.Uid!, Parent.Columns!, Parent.IsStackedHeader);
            if (filteredColumn.Value is DateTimeOffset)
            {
                return ((DateTimeOffset)filteredColumn.Value).ToString(gridColumn?.Format, CultureInfo.CurrentCulture);
            }
            else if (filteredColumn.Value is DateTime)
            {
                return ((DateTime)filteredColumn.Value).ToString(gridColumn?.Format, CultureInfo.CurrentCulture);
            }
            else if (filteredColumn.Value is DateOnly)
            {
                return ((DateOnly)filteredColumn.Value).ToString(gridColumn?.Format, CultureInfo.CurrentCulture);
            }
            else if (filteredColumn.Value is TimeOnly)
            {
                return ((TimeOnly)filteredColumn.Value).ToString(gridColumn?.Format, CultureInfo.CurrentCulture);
            }
            return filteredColumn.Value?.ToString() ?? string.Empty;
        }

        private async Task UpdateFilterColumnsAsync()
        {
            Parent.FilteredColumns = Parent.FilterSettings?.Columns?.ToList();
            if (Parent.FilterSettings != null)
            {
                await Parent.FilterSettings.UpdateProperties("Columns", Parent.FilterSettings.Columns!).ConfigureAwait(true);
            }
        }

        private string GetFilterValueDisplay(GridFilterColumn filterColumn, GridColumn? filteredColumn)
        {
            if (!string.IsNullOrEmpty(filterColumn.RawInputValue))
            {
                return filterColumn.RawInputValue;
            }

            if (filteredColumn?.Format != null && (filteredColumn?.Type == ColumnType.Date || filteredColumn?.Type == ColumnType.DateTime
                || filteredColumn?.Type == ColumnType.DateOnly || filteredColumn?.Type == ColumnType.TimeOnly))
            {
                return UpdateColumnFormat(filterColumn);
            }

            return filterColumn.Value?.ToString() ?? string.Empty;
        }
        #endregion

        #region Filter Column Building

        private static GridFilterColumn BuildFilterColumn(string field, Operator filterOperator, object filterValue, bool matchCase, bool ignoreAccent, GridColumn? column, string predicate, object actualValue, string? inputField)
        {
            return new GridFilterColumn()
            {
#pragma warning disable BL0005
                Field = field,
                Operator = filterOperator,
                Value = filterValue,
                MatchCase = matchCase,
                IgnoreAccent = ignoreAccent,
                Uid = column?.Uid!,
                Predicate = predicate,
                ActualValue = actualValue,
                RawInputValue = inputField,
                ColumnType = GetColumnType(column?.Type)!
#pragma warning restore BL0005
            };
        }

        private static PredicateModel<object> BuildPredicateModel(string field, Operator filterOperator, object filterValue, bool matchCase, bool ignoreAccent, GridColumn? column, string predicate, object actualValue)
        {
            return new PredicateModel<object>()
            {
#pragma warning disable BL0005
                Field = field,
                Operator = filterOperator,
                Value = filterValue,
                MatchCase = matchCase,
                IgnoreAccent = ignoreAccent,
                Uid = column?.Uid!,
                Predicate = predicate,
                ActualValue = actualValue
#pragma warning restore BL0005
            };
        }

        #endregion

        #region Operator Conversion & Type Detection

        internal static Operator GetOperator(string value)
        {
            switch (value)
            {
                case "isnull":
                    return Operator.IsNull;
                case "isnotnull":
                    return Operator.IsNotNull;
                case "isempty":
                    return Operator.IsEmpty;
                case "isnotempty":
                    return Operator.IsNotEmpty;
                case "contains":
                    return Operator.Contains;
                case "doesnotcontain":
                    return Operator.DoesNotContain;
                case "startswith":
                    return Operator.StartsWith;
                case "doesnotstartwith":
                    return Operator.DoesNotStartWith;
                case "endswith":
                    return Operator.EndsWith;
                case "doesnotendwith":
                    return Operator.DoesNotEndWith;
                case "greaterthan":
                    return Operator.GreaterThan;
                case "greaterthanorequal":
                    return Operator.GreaterThanOrEqual;
                case "lessthan":
                    return Operator.LessThan;
                case "lessthanorequal":
                    return Operator.LessThanOrEqual;
                case "notequal":
                    return Operator.NotEqual;
                case "equal":
                    return Operator.Equal;
                case "like":
                    return Operator.Like;
                case "wildcard":
                    return Operator.WildCard;
                default:
                    return Operator.None;
            }
        }

        internal static Operator GetEnumOperator(string value)
        {
            return GetOperator(value);
        }

        internal static string UpdateDropDownStringOperator(string value)
        {
            switch (value)
            {
                case "doesnotstartwith":
                    return "startswith";
                case "doesnotendwith":
                    return "endswith";
                case "doesnotcontain":
                    return "contains";
                case "notequal":
                    return "equal";
                default:
                    return value;
            }
        }

        internal static Syncfusion.Blazor.Operator GetFilterOperator(string filterOperator)
        {
            Syncfusion.Blazor.Operator currentOperator = Blazor.Operator.None;
            if (Enum.TryParse(typeof(Syncfusion.Blazor.Operator), filterOperator, true, out object? enumValue))
            {
                currentOperator = (Syncfusion.Blazor.Operator)enumValue;
            }
            return currentOperator;
        }

        #endregion

        #region Type Utilities & Conversion

        internal static string GetFilterType(string filterOperator)
        {
            switch (filterOperator)
            {
                case "isempty":
                    return "isempty";
                case "isnotempty":
                    return "isnotempty";
                case "startswith":
                    return "startswith";
                case "endswith":
                    return "endswith";
                case "doesnotstartwith":
                    return "doesnotstartwith";
                case "doesnotendwith":
                    return "doesnotendwith";
                case "doesnotcontain":
                    return "doesnotcontain";
                case "like":
                    return "like";
                case "wildcard":
                    return "wildcard";
                default:
                    return "contains";

            }
        }

        internal string? GetFilterType(GridColumn FColumn)
        {
            var value = Parent.FilterSettings?.Type.ToString();
            if (FColumn?.FilterSettings != null && FColumn.FilterSettings.Type.HasValue)
            {
                return FColumn.FilterSettings.Type.ToString();
            }

            return value;
        }

        internal static string? GetColumnType(ColumnType? columnType)
        {
            return columnType?.ToString();
        }

        internal static object TryParseNullable(string val, bool isInteger = false, bool isLong = false)
        {
            if (isInteger)
            {
                return int.TryParse(val, out int Value) ? (int?)Value : null!;
            }
            else if (isLong)
            {
                return long.TryParse(val, out long Value) ? (long?)Value : null!;
            }

            return double.TryParse(val, out double outValue) ? (double?)outValue : null!;
        }

        #endregion

        #region Filter Lookup & Indexing

        internal static int GetOperatorIndex(List<object> filterOperators, string operatorName)
        {
            int count = filterOperators?.Count ?? 0;
            int index = 0;
            for (var i = 0; i < count; i++)
            {
                var value = filterOperators?[i]?.GetType().GetProperty("Value")?.GetValue(filterOperators[i]);
                if (value != null && (string.Equals((string)value, operatorName, StringComparison.Ordinal)))
                {
                    return i;
                }
            }

            return index;
        }

        private int GetFilteredColsIndexByField(GridFilterColumn col)
        {
            var colCount = Parent.FilterSettings?.Columns?.Count ?? 0;
            var cols = Parent.FilterSettings!.Columns;
            if (cols != null)
            {
                for (var i = 0; i < colCount; i++)
                {
                    if (cols[i]?.Uid == col?.Uid)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        #endregion

        #region FilterBar Input Processing

        /// <summary>
        /// Converts typed user input to the actual filter value with operator detection.
        /// </summary>
        internal object? GetActualFilterValue(GridColumn Col, string StringValue)
        {
            object value = string.Empty;
            int index = 0;
            string Condition = string.Empty;
            FilterOperatorProperty = string.Empty;
            switch (Col?.Type)
            {
                case ColumnType.Integer:
                case ColumnType.Double:
                case ColumnType.Long:
                case ColumnType.Decimal:
                    string[] SkipInputs = new string[] { ">", "<", "=", "!" };
                    for (var s = 0; s < SkipInputs.Length; s++)
                    {
                        if (StringValue?.IndexOf(SkipInputs[s], StringComparison.Ordinal) > -1)
                        {
                            if (index < 3)
                            {
                                index++;
                                Condition = Condition + StringValue[index - 1];
                            }
                        }
                    }

                    if (index != 0)
                        value = StringValue?.Substring(index)!;
                    else
                        value = System.Text.RegularExpressions.Regex.Replace(StringValue ?? "", "[%*]+", "");
                    MapOperatorString(Condition, Col);
                    value = (Col?.ValueType != null) ? SfBaseUtils.ChangeType(value, Col.ValueType) : value;
                    break;
                case ColumnType.Date:
                case ColumnType.DateTime:
                case ColumnType.DateOnly:
                case ColumnType.TimeOnly:
                    MapOperatorString(Condition, Col);
                    if (Col?.ValueType == typeof(DateTimeOffset) || Col?.ValueType == typeof(DateTimeOffset?))
                    {
                        if (DateTimeOffset.TryParse(StringValue, out var timer))
                        {
                            value = timer;
                        }
                    }
                    else if (Col?.ValueType != null)
                    {
                        value = SfBaseUtils.ChangeType(StringValue, Col.ValueType);
                    }
                    break;
                case ColumnType.Boolean:
                    MapOperatorString(Condition, Col);
                    value = (Col?.ValueType != null) ? SfBaseUtils.ChangeType(StringValue, Col.ValueType) : StringValue;
                    break;
                case ColumnType.None:
                case ColumnType.String:
                    string? filterType = GetColumnOperator(Col);
                    bool isWildcard = !string.IsNullOrEmpty(filterType) ? filterType.Equals("wildcard", StringComparison.Ordinal) : false;
                    bool isLike = !string.IsNullOrEmpty(filterType) ? filterType.Equals("like", StringComparison.Ordinal) : false;
                    if (isWildcard || isLike)
                    {
                        value = StringValue;
                        FilterOperatorProperty = filterType!;
                    }
                    else if (StringValue != null && (StringValue.StartsWith('*') || StringValue.StartsWith('%')))
                    {
                        Condition = StringValue.StartsWith('*') ? "*" : "%";
                        MapOperatorString(Condition, Col);
                        value = StringValue.Substring(1);
                    }
                    else
                    {
                        MapOperatorString(Condition, Col);
                        value = System.Text.RegularExpressions.Regex.Replace(StringValue ?? "", "[<!=>=]+", "");
                    }
                    value = (StringValue == "null" || StringValue == "blanks") ? null! : (Col?.ValueType != null) ? SfBaseUtils.ChangeType(value, Col.ValueType)! : value!;
                    break;
            }
            RawInputValue = !string.IsNullOrEmpty(Condition) ? StringValue! : string.Empty;
            return value;
        }

        /// <summary>
        /// Converts date filter value to the desired format for processing.
        /// </summary>
        internal static string ConvertToDesiredDateFormat(string inputDate, string inputDateFormat, ColumnType columnType, bool defaultFormat = true)
        {
            if (DateTime.TryParseExact(inputDate, inputDateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                return columnType == ColumnType.DateOnly ? parsedDate.Date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : parsedDate.ToString(CultureInfo.InvariantCulture);
            }
            else if (!defaultFormat && DateTime.TryParseExact(inputDate, "M/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime columnParsedDate))
            {
                return columnParsedDate.Date.ToString(inputDateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                return inputDate;
            }
        }

        /// <summary>
        /// Gets formatted display values for all previously filtered columns.
        /// </summary>
        internal async Task<List<string>> GetPreviousFilteredValues()
        {
            List<string> values = new List<string>();
            var columnsAndFormats = new Dictionary<string, string>();
            if (Parent!.IsStackedHeader)
            {
                foreach (var column in Parent?.Columns!)
                {
                    if (!string.IsNullOrEmpty(column?.Field))
                    {
                        columnsAndFormats[column.Field] = column?.Format!;
                    }
                    if (column?.Columns != null && column.Columns.Count > 0)
                    {
                        SetValuesRecursively(column.Columns, columnsAndFormats);
                    }
                }
            }
            else
            {
                foreach (var column in Parent.Columns!)
                {
                    var key = column?.IsForeignColumn() == true ? column.ForeignKeyValue! : column?.Field!;
                    if (!columnsAndFormats.ContainsKey(key) && column?.Format != null)
                    {
                        columnsAndFormats[key] = column.Format;
                    }
                }

            }
            foreach (var column in Parent.FilteredColumns!)
            {
                if (columnsAndFormats?.TryGetValue(column?.Field!, out var format) == true && column?.Value != null)
                {
                    var type = (column.ActualValue ?? column.Value)?.GetType();
                    var typeName = type?.Name;
                    switch (typeName)
                    {
                        case "DateTime":
                        case "DateTimeOffset":
                        case "DateOnly":
                        case "TimeOnly":
                            var actualValue = column.ActualValue ?? column.Value ?? string.Empty;
                            string valueToAdd = DataUtil.GetFormattedValue(actualValue, format);
                            values.Add(valueToAdd);
                            break;
                        case "JsonElement":
                            var foreignKeyColumn = ForeignKey<T>.GetForeignKeyColumnsAsync(Parent.Columns!);
                            GridColumn filteredGridColumn = await Parent.GetColumnByFieldAsync(column.Field).ConfigureAwait(true);
                            object convertedValue = SfBaseUtils.ChangeType(column.ActualValue,
                                foreignKeyColumn != null && foreignKeyColumn.Count > 0 ? foreignKeyColumn.Where(col => col.Uid == column.Uid).FirstOrDefault()!.ValueType : filteredGridColumn.ValueType);
                            string formattedValue = DataUtil.GetFormattedValue(convertedValue, format);
                            values.Add(formattedValue);
                            break;
                        default:
                            string? valueToAddInList = !string.IsNullOrEmpty(column.RawInputValue) ? column.RawInputValue : type != null && type.IsEnum ? MetadataExtension.GetDisplayName((column.ActualValue as Enum)!) : (column.ActualValue ?? column.Value)?.ToString();
                            values.Add(valueToAddInList!);
                            break;
                    }
                }
                else
                {
                    values.Add("null");
                }
            }
            return values;
        }

        /// <summary>
        /// Recursively sets column format values for stacked headers.
        /// </summary>
        private static void SetValuesRecursively(List<GridColumn> columns, Dictionary<string, string> columnsAndFormats)
        {
            foreach (var column in columns)
            {
                if (!string.IsNullOrEmpty(column?.Field))
                {
                    columnsAndFormats[column.Field] = column?.Format!;
                }
                if (column?.Columns != null && column.Columns.Count > 0)
                {
                    SetValuesRecursively(column.Columns, columnsAndFormats);
                }
            }
        }

        /// <summary>
        /// Maps operator string to FilterOperator property.
        /// </summary>
        private void MapOperatorString(string OperatorString, GridColumn? filteredColumn = null)
        {
            switch (OperatorString)
            {
                case "!=":
                    FilterOperatorProperty = "notequal";
                    break;
                case "=":
                    FilterOperatorProperty = "equal";
                    break;
                case "<":
                    FilterOperatorProperty = "lessthan";
                    break;
                case "<=":
                    FilterOperatorProperty = "lessthanorequal";
                    break;
                case ">":
                    FilterOperatorProperty = "greaterthan";
                    break;
                case ">=":
                    FilterOperatorProperty = "greaterthanorequal";
                    break;
                case "%":
                    FilterOperatorProperty = "endswith";
                    break;
                case "*":
                    FilterOperatorProperty = "startswith";
                    break;
                default:
                    if (filteredColumn?.FilterSettings != null)
                    {
                        var ColumnOperator = GetColumnOperator(filteredColumn);
                        FilterOperatorProperty = (ColumnOperator != null && !string.IsNullOrEmpty(ColumnOperator.ToString())) ? (string)ColumnOperator : "equal";
                    }
                    else
                        FilterOperatorProperty = filteredColumn?.Type == ColumnType.String ? "startswith" : "equal";
                    break;
            }
        }

        #endregion

        #region Enhanced FilterBar Functionalities

        internal static string? GetColumnOperator(GridColumn Column)
        {
            if (Column.FilterSettings != null && Column.FilterSettings.Operator.HasValue)
            {
                return Column.FilterSettings.Operator.ToString()?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
            }
            else
            {
                return null!;
            }
        }

        internal static bool IsNullOrEmptyOperator(string? op)
        {
            return string.Equals(op, "isnull", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(op, "isnotnull", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(op, "isempty", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(op, "isempty", StringComparison.OrdinalIgnoreCase);
        }

        internal static string GetDefaultOperator(ColumnType? type)
        {
            return type switch
            {
                ColumnType.String => "startswith",
                ColumnType.Integer or ColumnType.Double or ColumnType.Long or ColumnType.Decimal => "equal",
                ColumnType.Date or ColumnType.DateTime or ColumnType.DateOnly or ColumnType.TimeOnly => "equal",
                ColumnType.Boolean => "equal",
                _ => "equal"
            };
        }

        internal static bool IsNumericDecimal(GridColumn? column)
        {
            if (column?.ValueType == typeof(decimal) ||
                column?.ValueType == typeof(decimal?))
                return true;

            if (column?.Type == ColumnType.Decimal)
                return true;

            return false;
        }

        internal static bool IsNumericInteger(GridColumn? column)
        {
            if (column?.ValueType == typeof(int) ||
                column?.ValueType == typeof(int?))
                return true;

            if (column?.Type == ColumnType.Integer)
                return true;

            return false;
        }

        internal static bool IsNumericLong(GridColumn? column)
        {
            if (column?.ValueType == typeof(long) ||
                column?.ValueType == typeof(long?))
                return true;

            if (column?.Type == ColumnType.Long)
                return true;

            return false;
        }

        internal static bool IsDateColumn(GridColumn? column)
        {
            // Priority 1: Check ColumnType first
            if (column?.Type == ColumnType.Date || column?.Type == ColumnType.DateTime)
                return true;

            // Priority 2: If ColumnType is null or not date, check ValueType
            if (column?.Type == null && (column?.ValueType == typeof(DateTime) ||
                column?.ValueType == typeof(DateTime?)))
                return true;

            return false;
        }

        internal static bool IsDateOnlyColumn(GridColumn? column)
        {
            // Priority 1: Check ColumnType first
            if (column?.Type == ColumnType.DateOnly)
                return true;

            // Priority 2: If ColumnType is null, check ValueType
            if (column?.Type == null && (column?.ValueType == typeof(DateOnly) ||
                column?.ValueType == typeof(DateOnly?)))
                return true;

            return false;
        }

        internal static bool IsDateTimeColumn(GridColumn? column)
        {
            // Priority 1: Check ColumnType first
            if (column?.Type == ColumnType.DateTime)
                return true;

            // Priority 2: If ColumnType is null, check ValueType
            if (column?.Type == null && (column?.ValueType == typeof(DateTime) ||
                column?.ValueType == typeof(DateTime?)))
                return true;

            return false;
        }

        internal static bool IsTimeOnlyColumn(GridColumn? column)
        {
            // Priority 1: Check ColumnType first
            if (column?.Type == ColumnType.TimeOnly)
                return true;

            // Priority 2: If ColumnType is null, check ValueType
            if (column?.Type == null && (column?.ValueType == typeof(TimeOnly) ||
                column?.ValueType == typeof(TimeOnly?)))
                return true;

            return false;
        }

        internal static bool IsDateTimeOffsetColumn(GridColumn? column)
        {
            // Priority 1: Check ColumnType first (if available)
            // Note: DateTimeOffset may not have a direct ColumnType equivalent

            // Priority 2: Check ValueType
            if (column?.ValueType == typeof(DateTimeOffset) ||
                column?.ValueType == typeof(DateTimeOffset?))
                return true;

            return false;
        }

        internal bool ShouldTriggerImmediateFilter()
        {
            return Parent?.FilterSettings?.Mode == FilterBarMode.Immediate;
        }

        internal List<OperatorItem> GetOperatorsByColumnType(ColumnType? type)
        {
            if (type == null)
            {
                return new List<OperatorItem>
                {
                    new() { Value = "equal", Text = LocalizeOperator("equal") },
                    new() { Value = "notequal", Text = LocalizeOperator("notequal") }
                };
            }

            return type switch
            {
                ColumnType.String => new List<OperatorItem>
                {
                    new() { Value = "contains", Text = LocalizeOperator("contains") },
                    new() { Value = "doesnotcontain", Text = LocalizeOperator("doesnotcontain") },
                    new() { Value = "startswith", Text = LocalizeOperator("startswith") },
                    new() { Value = "endswith", Text = LocalizeOperator("endswith") },
                    new() { Value = "equal", Text = LocalizeOperator("equal") },
                    new() { Value = "notequal", Text = LocalizeOperator("notequal") },
                    new() { Value = "isempty", Text = LocalizeOperator("isempty") },
                    new() { Value = "isnotempty", Text = LocalizeOperator("isnotempty") }
                },
                ColumnType.Integer or ColumnType.Double or ColumnType.Long or ColumnType.Decimal => new List<OperatorItem>
                {
                    new() { Value = "equal", Text = LocalizeOperator("equal") },
                    new() { Value = "notequal", Text = LocalizeOperator("notequal") },
                    new() { Value = "greaterthan", Text = LocalizeOperator("greaterthan") },
                    new() { Value = "greaterthanorequal", Text = LocalizeOperator("greaterthanorequal") },
                    new() { Value = "lessthan", Text = LocalizeOperator("lessthan") },
                    new() { Value = "lessthanorequal", Text = LocalizeOperator("lessthanorequal") },
                    new() { Value = "isnull", Text = LocalizeOperator("isnull") },
                    new() { Value = "isnotnull", Text = LocalizeOperator("isnotnull") }
                },
                ColumnType.Date or ColumnType.DateTime or ColumnType.DateOnly or ColumnType.TimeOnly => new List<OperatorItem>
                {
                    new() { Value = "equal", Text = LocalizeOperator("equal") },
                    new() { Value = "notequal", Text = LocalizeOperator("notequal") },
                    new() { Value = "greaterthan", Text = LocalizeOperator("greaterthan") },
                    new() { Value = "greaterthanorequal", Text = LocalizeOperator("greaterthanorequal") },
                    new() { Value = "lessthan", Text = LocalizeOperator("lessthan") },
                    new() { Value = "lessthanorequal", Text = LocalizeOperator("lessthanorequal") },
                    new() { Value = "isnull", Text = LocalizeOperator("isnull") },
                    new() { Value = "isnotnull", Text = LocalizeOperator("isnotnull") }
                },
                ColumnType.Boolean => new List<OperatorItem>
                {
                    new() { Value = "equal", Text = LocalizeOperator("equal") },
                    new() { Value = "notequal", Text = LocalizeOperator("notequal") }
                },
                _ => new List<OperatorItem>
                {
                    new() { Value = "equal", Text = LocalizeOperator("equal") },
                    new() { Value = "notequal", Text = LocalizeOperator("notequal") }
                }
            };
        }

        private string LocalizeOperator(string op)
        {
            op = op ?? string.Empty;

            return op switch
            {
                _ when op.Equals("equal", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.Equal) ?? "Equal",
                _ when op.Equals("notequal", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.NotEqual) ?? "Not Equal",
                _ when op.Equals("greaterthan", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.GreaterThan) ?? "Greater Than",
                _ when op.Equals("greaterthanorequal", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.GreaterThanOrEqual) ?? "Greater Than Or Equal",
                _ when op.Equals("lessthan", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.LessThan) ?? "Less Than",
                _ when op.Equals("lessthanorequal", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.LessThanOrEqual) ?? "Less Than Or Equal",
                _ when op.Equals("contains", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.Contains) ?? "Contains",
                _ when op.Equals("doesnotcontain", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.DoesNotContain) ?? "Does Not Contain",
                _ when op.Equals("startswith", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.StartsWith) ?? "Starts With",
                _ when op.Equals("endswith", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.EndsWith) ?? "Ends With",
                _ when op.Equals("isempty", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.IsEmpty) ?? "Is Empty",
                _ when op.Equals("isnotempty", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.IsNotEmpty) ?? "Is Not Empty",
                _ when op.Equals("isnull", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.IsNull) ?? "Is Null",
                _ when op.Equals("isnotnull", StringComparison.OrdinalIgnoreCase) =>
                    Parent?.Localizer?.GetText(GridLocaleKeys.IsNotNull) ?? "Is Not Null",
                _ => op
            };
        }

        #endregion

        #region JS interop
        internal async Task FilterMouseOverHandler(string uid, bool showDialog)
        {
            if (showDialog)
            {
                var column = await Parent.GetColumnByUidAsync(uid).ConfigureAwait(true);
                FilterIconIsClicked = true;
                IsColumnMenuFilter = true;
                FilterIconColumn = column;
            }
            else
            {
                FilterIconIsClicked = false;
                IsColumnMenuFilter = false;
            }

            Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
        }

        internal async Task FilterPopupClose()
        {
            if (FilterIconColumn != null && (Parent.FilterSettings!.Type == FilterType.Menu || (FilterIconColumn.FilterSettings != null && FilterIconColumn.FilterSettings.Type == FilterType.Menu)) && FilterIconColumn.FilterTemplate != null)
            {
                ShouldTemplateDispose = true;
                Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                ShouldTemplateDispose = false;
            }
            if (Parent.FilterModule != null && FilterDialogInstance != null)
            {
                if (GetFilterType(FilterIconColumn!) == "Excel")
                {
                    ExcelDialog = true;
                    Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                }

                FilterIconIsClicked = false;
                Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                ExcelDialog = false;
            }

            if (Parent.ChooserDialogInstance != null && !Parent.ShowChooser)
            {
                Parent.EventAggregator.Trigger("HideColumnChooser", null!);
            }

            if (Parent.ColumnMenuInstance != null && Parent.FilterModule != null)
            {
                HideColumnMenuPopup();
            }
        }

        internal async Task CloseEnhancedOperatorDropdown()
        {
            if (Parent != null && Parent.FilterSettings != null && Parent.FilterSettings.ShowFilterBarOperator)
            {
                // Trigger component update to close all open operator dropdowns in the filter bar
                Parent.EventAggregator.Trigger("CloseOperatorDropdown", null!);
                await Task.CompletedTask.ConfigureAwait(true);
            }
        }

        #endregion

    }

    /// <summary>
    /// Operator item for filter dropdown
    /// </summary>
    internal class OperatorItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Boolean option item for boolean filter dropdown
    /// </summary>
    internal class BooleanOption
    {
        public bool? Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
