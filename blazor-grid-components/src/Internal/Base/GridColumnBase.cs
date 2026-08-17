using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using Syncfusion.Blazor.Data;
using Microsoft.AspNetCore.Components;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures a grid column.
    /// </summary>
    public partial class GridColumn : SfDataBoundComponent
    {
        /// <summary>
        /// Provides the data for the grid columns.
        /// </summary>
        [JsonIgnore]
        public object? ColumnData { get; set; }

        [JsonIgnore]
        internal Type? ValueType { get; set; }

        [JsonIgnore]
        internal Type? ActualType { get; set; }

        internal bool IsGridForeignColumn { get; set; }

        internal bool IsSearchQueryRequired { get; set; }

        internal bool ColumnVisible { get; set; }

        internal string? FilterClearIcon { get; set; }

        /// <summary>
        /// Gets or sets the text to be displayed whether the column is Left frozen, right frozen and movable column.
        /// </summary>
        internal string? FrozenMovableLabel { get; set; }

        /// <summary>
        /// Gets or sets the text to be displayed when the value of the property is null.
        /// This annotation attribute is used for UI display purposes only and does not affect the underlying data source or grid actions.
        /// </summary>
        internal string? NullDisplayText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether empty string values should be converted to null in UI.
        /// </summary>
        /// <value>
        /// <c>true</c> if empty string values should be converted to null; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When this property is set to true, if a column has an empty string value, it will be automatically converted to null in the UI.
        /// This conversion is specific to the UI and does not affect the underlying data source or other operations.
        /// </remarks>
        internal bool ConvertEmptyStringToNull { get; set; }

        /// <summary>
        /// Gets or sets the text to be displayed for the tooltip.
        /// </summary>
        /// <value>
        /// The text to be used as the tooltip content.
        /// </value>
        /// <remarks>
        /// By default, the header text is used as the tooltip content. However, if a description is specified in the model's annotation, the description value is applied as the tooltip content instead of the header text.
        /// </remarks>
        /// <exclude />
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column format should be applied in edit mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if the format should be applied in edit mode; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When this property is set to true, the specified column format will be applied in edit mode. This allows for consistent formatting of the column value during editing.
        /// </remarks>
        internal bool ApplyFormatInEditMode { get; set; } = true;

        internal virtual object GetForeignDataSource(GridColumn column, object data, IEnumerable<object> fkData) => data;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridColumn"/> class and assigns a unique identifier if not provided.
        /// </summary>
        public GridColumn()
        {
            Uid = string.IsNullOrEmpty(Uid) ? GetColumnUid("grid-column") : Uid;
        }

        /// <summary>
        /// Returns true if the column is foreign key column.
        /// </summary>
        /// <returns>bool.</returns>
        public virtual bool IsForeignColumn()
        {
            return ForeignKeyValue != null;
        }

        internal virtual List<object> GetForeignkeyFilteredData(IEnumerable<object> data, List<WhereFilter> filterQuery)
            => DataOperations.PerformFiltering(data, filterQuery, "and").ToList();

        internal virtual List<object> GetForeignKeyData(IEnumerable<object> foreignKeyData, List<WhereFilter> query)
        {
            if (query is null || query.Count == 0)
                return foreignKeyData?.ToList() ?? new List<object>();

            foreach (var filter in query)
            {
                foreignKeyData = foreignKeyData?.Where(item =>
                {
                    var itemValue = DataUtil.GetObject(filter.Field ?? "", item);
                    return Equals(itemValue, filter.value);
                })!;
            }            
            return foreignKeyData?.ToList() ?? new List<object>();
        }

        /// <summary>
        /// Returns true if the Foreignkeyfield and Field property is not same is foreign key column.
        /// </summary>
        /// <returns>bool.</returns>
        public virtual bool IsForeignKeyField()
        {
            return ForeignKeyField != null && !ForeignKeyField.ToString().Equals(Field.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Performs data operation in the foreign key column based on the given query and returns data.
        /// </summary>
        /// <param name="dataManagerRequest">Query value to be used for data fetching.</param>
        /// <returns>object.</returns>
        public virtual async Task<object> GetData(DataManagerRequest dataManagerRequest)
        {
            if (Parent != null && !Parent.IsRendered && DataManager != null && DataManager.DataAdaptor != null)
            {
                DataManager.DataAdaptor.SetRunSyncOnce(true);
            }
            return DataManager != null? await DataManager.ExecuteQuery<object>(dataManagerRequest).ConfigureAwait(true): null!;
        }

        internal static int sequence { get; set; }

        internal object? GridParent { get; set; }
       
        internal static string GetColumnUid(string prefix)
        {
            return $"{prefix}{sequence++}";
        }

        /// <summary>
        /// Set column visibility.
        /// </summary>
        /// <param name="visibility">Value to be set in the <c>Visible</c> property.</param>
        public void SetVisibility(bool visibility) => Visible = _visible = visibility;

        /// <summary>
        /// Set column width.
        /// </summary>
        /// <param name="width">Value to be set in the <c>Width</c> property.</param>
        public void SetWidth(string width) => Width = _width = width;

        internal void SetFreeze(FreezeDirection freeze) => Freeze = _freeze = freeze;
        
        internal void SetIsFrozen(bool isFrozen) => IsFrozen = _isFrozen = isFrozen;

        internal void SetIndex(int index) => Index = _index = index;

        /// <summary>
        /// Set column uid.
        /// </summary>
        /// <param name="uid">Value to be set in the <c>Uid</c> property.</param>
        public void SetUid(string uid) => Uid = _uid = uid;

        /// <summary>
        /// Get Foreign Data.
        /// </summary>
        public virtual object GetForeignData()
        {
            return null!;
        }

        internal void SetColumnEditType()
        {
            if (ValueType != null)
            {
                var type = ValueType;
                var IsInteger = new List<Type>()
                {
                    typeof(int), typeof(int?), typeof(short), typeof(short?),
                    typeof(ushort), typeof(ushort?), typeof(byte), typeof(byte?),
                    typeof(sbyte), typeof(sbyte?), typeof(uint), typeof(uint?)
                }.Any(x => x == type);

                var IsLong = new List<Type>()
                {
                    typeof(long), typeof(long?), typeof(ulong), typeof(ulong?)
                }.Any(x => x == type);

                var IsDecimal = new List<Type>()
                {
                    typeof(decimal), typeof(decimal?)
                }.Any(x => x == type);

                var IsDouble = new List<Type>()
                {
                    typeof(double), typeof(double?), typeof(float), typeof(float?)
                }.Any(x => x == type);

                var IsDateTime = new List<Type>()
                {
                    typeof(DateTime?), typeof(DateTimeOffset?),
                    typeof(DateTime), typeof(DateTimeOffset)
                }.Any(x => x == type);

                var IsDateOnly = new List<Type>() { typeof(DateOnly?), typeof(DateOnly) }.Any(x => x == type);
                var IsTimeOnly = new List<Type>() { typeof(TimeOnly?), typeof(TimeOnly) }.Any(x => x == type);
                var IsBoolean = new List<Type>() { typeof(bool), typeof(bool?) }.Any(x => x == type);

                if (IsInteger)
                {
#pragma warning disable BL0005
                    Type = Type.Equals(ColumnType.None) ? ColumnType.Integer : Type;
                }
                else if (IsLong)
                {
                    Type = Type.Equals(ColumnType.None) ? ColumnType.Long : Type;
                }
                else if (IsDecimal)
                {
                    Type = Type.Equals(ColumnType.None) ? ColumnType.Decimal : Type;
                }
                else if (IsDouble)
                {
                    Type = Type.Equals(ColumnType.None) ? ColumnType.Double : Type;
                }
                else if (IsDateTime)
                {
                    Type = Type.Equals(ColumnType.None) ? ColumnType.DateTime : Type;
                }
                else if (IsDateOnly)
                {
                    Type = Type.Equals(ColumnType.None) ? ColumnType.DateOnly : Type;
                }
                else if (IsTimeOnly)
                {
                    Type = Type.Equals(ColumnType.None) ? ColumnType.TimeOnly : Type;
                }
                else if (type == typeof(string))
                {
                    Type = Type.Equals(ColumnType.None) ? ColumnType.String : Type;
                }
                else if (IsBoolean)
                {
                    Type = Type.Equals(ColumnType.None) ? ColumnType.Boolean : Type;
                }

                if (EditType == EditType.DefaultEdit)
                {
                    if (IsForeignColumn())
                    {
                        EditType = EditType.DropDownEdit;
                    }
                    else if (IsInteger || IsLong || IsDecimal ||IsDouble)
                    {
                        EditType = EditType.NumericEdit;
                    }
                    else if (IsDateTime || IsDateOnly)
                    {
                        EditType = EditType.DatePickerEdit;
                    }
                    else if (IsTimeOnly)
                    {
                        EditType = EditType.TimePickerEdit;
                    }
                    else if (IsBoolean)
                    {
                        EditType = EditType.BooleanEdit;
                    }
                }
                else
                {
                    if (ValueType == typeof(Guid))
                    {
                        EditType = EditType.Equals(EditType.DefaultEdit) ? EditType.DefaultEdit : EditType;
                    }
                    else if (ValueType.IsEnum)
                    {
                        EditType = EditType.Equals(EditType.DefaultEdit) ? EditType.DropDownEdit : EditType;
#pragma warning restore BL0005
                    }
                }
            }

            _editType = EditType;
            _type = Type;
        }

        internal TextAlign GetTextAlign()
        {
            if ((Commands != null) && directParamKeys?.Contains(nameof(TextAlign)) == true)
            {
                return TextAlign == TextAlign.None ? TextAlign.Right : TextAlign;   
            }
            else if (Commands != null)
            {
                return TextAlign.Right;
            }
            return TextAlign == TextAlign.None ? TextAlign.Left : TextAlign;
        }
    }
}
