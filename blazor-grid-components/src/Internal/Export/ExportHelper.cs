using System;
using System.Collections.Generic;
using Syncfusion.Blazor.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using System.Drawing;
using System.Dynamic;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Export helper.
    /// </summary>
    /// <typeparam name="T">TValue of grid.</typeparam>
    internal class ExportHelper<T>
    {
        internal static async Task<DataResult> DataProcess(SfGrid<T> GridModel, bool AllPages = true, ActionArgs action = null!)
        {
            try
            {
                var Grid = GridModel;
                var query = new Query();
                if (Grid.EnableInfiniteScrolling)
                {
                    if (!AllPages)
                    {
                        if (Grid.InfiniteScrollSettings != null && !Grid.InfiniteScrollSettings.EnableCache && Grid.InfiniteScrollModule != null)
                        {
                            await Grid.InfiniteScrollModule.ResetInfiniteProperties(Grid.InfiniteScrollModule.RequestType).ConfigureAwait(true);
                        }                      
                    }
                }
                query = Grid?.DataModule?.GenerateQuery(AllPages).RequiresCount();
                query!.Queries.LazyLoad = false;
                if(Grid != null && Grid.DataManager != null && Grid.DataManager.Adaptor.Equals(Adaptors.CustomAdaptor) && AllPages && query.Queries.Aggregates == null)
                {
                    query.Queries.RequiresCounts = false;
                }
                bool IsQueryGenerated = true;
                List<int> QueryStartIndexes = new List<int>();
                var isForeignKeyAction = (Grid != null && Grid.ForeignKeyModule!.isNeedForeignKeyAction());
                if (isForeignKeyAction && Grid?.ForeignKeyModule != null)
                {
                    await Grid.ForeignKeyModule.GetForeignKeyData<T>(null!, true).ConfigureAwait(true);
                }

                if (Grid != null &&(!Grid.EnableVirtualization || Grid.CurrentViewData == null || (Grid.EnableVirtualization && Grid.GroupSettings!.Columns?.Length > 0)))
                {
                    if (Grid.EnableVirtualization && Grid.GroupSettings!.Columns == null)
                    {
                        action.VirtualEndIndex = (int)action?.VirtualEndIndex! > 0 ? (int)action?.VirtualEndIndex! : Grid.PageSettings!.PageSize;
                        await Grid.GenerateAndExecuteQuery(query, isForeignKeyAction, (int)action?.VirtualStartIndex!, (int)action?.VirtualEndIndex!, IsQueryGenerated).ConfigureAwait(true);
                    }
                    else
                    {
                        if (Grid.EnableVirtualization && Grid.GroupSettings!.Columns?.Length > 0 && action != null)
                        {
                            action.VirtualEndIndex = (int)action.VirtualEndIndex > 0 ? (int)action.VirtualEndIndex : Grid.PageSettings!.PageSize;
                        }

                        await Grid.GenerateAndExecuteQuery(query, isForeignKeyAction, 0, 0, IsQueryGenerated).ConfigureAwait(true);
                    }
                }

                if (Grid != null && Grid.EnableVirtualization && action == null && AllPages)
                {
                    await Grid.GenerateAndExecuteQuery(query, isForeignKeyAction, 0, 0, IsQueryGenerated).ConfigureAwait(true);
                }

                DataResult dataResult = (DataResult)Grid!.Data!;
                DataReadyArgs<T> eventArgs = new DataReadyArgs<T>() { Data = (IEnumerable<object>?)dataResult.Result, Grid = GridModel, Query = query };
                GridModel.EventAggregator.Trigger("DataReady", eventArgs);
                dataResult.Result = eventArgs.Data;
                dataResult.Aggregates = eventArgs.Aggregates != null ? eventArgs.Aggregates : dataResult.Aggregates;
                return dataResult;
            }
#pragma warning disable BL0005
            catch (Exception exception)
            {
                if (GridModel.GridEvents?.OnActionFailure.HasDelegate == true)
                    await GridModel.GridEvents.OnActionFailure.InvokeAsync(new FailureEventArgs() { Error = exception, Parent = GridModel }).ConfigureAwait(true);
                else if(GridModel.IsRenderedFromTreeGrid)
                    await GridModel.EventAggregator.NotifyAsync("ActionFailure", new FailureEventArgs() { Error = exception, Parent = GridModel }).ConfigureAwait(true);
                return null!;
                throw;
            }
        }

        internal static int GetGroupColumnsCount(SfGrid<T> Grid)
        {
            return (Grid.AllowGrouping && Grid.GroupSettings != null && Grid.GroupSettings.Columns != null) ? Grid.GroupSettings.Columns.Length : 0;
        }


        internal static void SetColumnType(object rowData, GridColumn column, SfGrid<T> GridModel)
        {
            var data = rowData;
            IDictionary<string, Type>? type = null;
            if (data is ExpandoObject)
            {
                type = DataUtil.GetColumnType(new List<object>() { data }, true);
            }
            else if (data is DynamicObject && GridModel.EditModule != null)
            {
                type = GridModel.EditModule.GetDynamicColType();
            }

            if (data is ExpandoObject || data is DynamicObject)
            {
                if (type?.TryGetValue(column.Field, out Type? value) == true)
                {
                    column.ValueType = value;
                }
            }
            else
            {
                Type ? _ref = null;
                var valueType = GridModel.EditModule!.GetColumnType(column, ref _ref!, null!, data);
                column.ValueType = valueType != null ? valueType : column.ValueType;
                column.ActualType = _ref;
            }

            column.SetColumnEditType();
        }

        internal static void GetSummaryAndCount(List<GridColumn> Columns, Group<T> context, GridAggregateColumn summaryColumn, bool isHideColumnInclude, bool isTemplateColumnInclude, bool isCustomCommandColumnInclude, ref object SummaryValue, ref int count)
        {
            IDictionary<string, object> aggregates = (IDictionary<string, object>)(context.Aggregates ?? new Dictionary<string, object>());
            string GroupKeyValue = summaryColumn.Field + " " + "-" + " " + summaryColumn.Type!.ToString();
            if(aggregates != null)
            foreach (var SummaryData in aggregates)
            {
                if (SummaryData.Key == GroupKeyValue)
                {
                    SummaryValue = SummaryData.Value;
                    break;
                }
            }

            foreach (var column in Columns)
            {
                bool visColumn = column.Visible || isHideColumnInclude;
                bool customCommands = (column.Commands != null && isCustomCommandColumnInclude) || column.Commands == null;
                bool tempColumn = ((column.Template != null) && isTemplateColumnInclude) || (column.Template == null);
                if (visColumn && tempColumn && customCommands && column.Type != ColumnType.CheckBox)
                {
                    count++;
                }

                if (column.Field == summaryColumn.ColumnName)
                {
                    break;
                }
            }
        }

        internal static Color GetDrawingColorFromHexString(string hexString)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(hexString, @"[#]([0-9]|[a-f]|[A-F]){6}\b"))
            {
                return GetColor(hexString);
            }

            int red = int.Parse(hexString.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int green = int.Parse(hexString.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int blue = int.Parse(hexString.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return System.Drawing.Color.FromArgb(red, green, blue);
        }

        internal static string GetHexValueFromColor(string ColorName)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(ColorName, @"[#]([0-9]|[a-f]|[A-F]){6}\b"))
            {
                return ColorName;
            }
            else
            {
                int ColorValue = Color.FromName(ColorName).ToArgb();
                string ColorHex = string.Format(CultureInfo.InvariantCulture, "{0:x6}", ColorValue);
                return ColorHex;
            }
        }

        private static Color GetColor(string colorCode)
        {
            return Color.FromName(colorCode);
        }

        internal static bool IsGroupingEnabled(SfGrid<T> GridModel)
        {
            return GridModel.AllowGrouping &&  GridModel.GroupSettings != null && GridModel.GroupSettings.Columns != null;
        }

        internal static int GetCurrentCellIndex(List<SpannedRow> SpannedCellIndex, int CurrentCellIndex, int RowIndex, int ColIndex)
        {
            var colIndex = ColIndex;
            if (SpannedCellIndex?.Count > 0)
            {
                if (SpannedCellIndex[RowIndex]?.RowIndex == RowIndex)
                {
                    CurrentCellIndex = CurrentCellIndex + SpannedCellIndex[RowIndex].ColumnIndex;
                    return CurrentCellIndex;
                }
            }

            return CurrentCellIndex;
        }

        internal static int GetCurrentPdfCellIndex(List<PdfSpannedRow> SpannedCellIndex, int CurrentCellIndex, int RowIndex, int ColIndex)
        {
            var colIndex = ColIndex;
            if (SpannedCellIndex?.Count > 0)
            {
                if (SpannedCellIndex[RowIndex]?.RowIndex == RowIndex)
                {
                    CurrentCellIndex = CurrentCellIndex + SpannedCellIndex[RowIndex].ColumnIndex;
                    return CurrentCellIndex;
                }
            }

            return CurrentCellIndex;
        }

        internal static int GetColSpan(GridColumn Column, int Count, SfGrid<T> GridModel)
        {
            List<GridColumn> cols = (List<GridColumn>)Column.Columns!;

            if (cols != null && cols.Count != 0)
            {
                for (var i = 0; i < cols.Count; i++)
                {
                    Count = ExportHelper<T>.GetColSpan(cols[i], Count, GridModel);
                }
            }
            else
            {
                if (Column.Visible && IsGroupVisible(Column, GridModel))
                {
                    Count++;
                }
            }

            return Count;
        }
        internal static bool IsGroupVisible(GridColumn Col, SfGrid<T> GridModel)
        {
            bool Value = true;
            if (GridModel.AllowGrouping && GridModel.GroupSettings!.Columns != null && Array.IndexOf(GridModel.GroupSettings.Columns, Col.Field) > -1)
            {
                return GridModel.GroupSettings.ShowGroupedColumn;
            }

            return Value;
        }

        internal static int MeasureColumnDepth(List<GridColumn> Columns)
        {
            var Max = 0;
            for (var i = 0; i < Columns?.Count; i++)
            {
                var depth = ExportHelper<T>.CheckDepth(Columns[i], 0);
                if (Max < depth)
                {
                    Max = depth;
                }
            }

            return Max;
        }

        internal static int CheckDepth(GridColumn Col, int Index)
        {
            var Max = Index;
            List<int> Indices = new List<int>();
            if (Col.Columns != null)
            {
                Index++;
                for (var i = 0; i < ((List<GridColumn>)Col.Columns).Count; i++)
                {
                    var Cols = (List<GridColumn>)Col.Columns;
                    Indices.Add(ExportHelper<T>.CheckDepth(Cols[i], Index));
                }

                for (var j = 0; j < Indices.Count; j++)
                {
                    if (Max < Indices[j])
                    {
                        Max = Indices[j];
                    }
                }

                Index = Max;
            }

            return Index;
        }

        internal static object FormatDConverstion(string columnFormat, object value, Type? valueType)
        {
            object convertedValue = value;
            HashSet<Type>? integerTypes = new HashSet<Type>
            {
                typeof(Int16), typeof(Int16?),
                typeof(Int32), typeof(Int32?),
                typeof(Int64), typeof(Int64?),
                typeof(UInt16), typeof(UInt16?),
                typeof(UInt32), typeof(UInt32?),
                typeof(byte), typeof(byte?),
                typeof(sbyte), typeof(sbyte?)
            };
            if (integerTypes.Contains(valueType!))
            {
                convertedValue = Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(columnFormat, CultureInfo.CurrentCulture);
            }
            else if (valueType == typeof(UInt64) || valueType == typeof(UInt64?))
            {
                convertedValue = Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(columnFormat, CultureInfo.CurrentCulture);
            }
            return convertedValue;
        }
    }
}