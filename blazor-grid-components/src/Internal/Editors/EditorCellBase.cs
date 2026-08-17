using System;
using System.Collections.Generic;
using System.Text;
using Syncfusion.Blazor.Data;
using System.Dynamic;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Syncfusion.Blazor.Inputs;

namespace Syncfusion.Blazor.Grids.Internal
{

    /// <summary>
    /// Represents the base class for an editable cell in a grid.
    /// </summary>
    public class EditorCellBase<T> : ComponentBase
    {

        /// <summary>
        /// Gets or sets the edit context for the current editing operation.
        /// </summary>
        [CascadingParameter]
        public EditContext? EditContext { get; set; }

        /// <summary>
        /// Gets or sets the parent grid component.
        /// </summary>
        [CascadingParameter]
        public SfGrid<T>? Parent { get; set; }

        /// <summary>
        /// Gets or sets the column associated with the cell.
        /// </summary>
        [Parameter]
        public GridColumn? Column { get; set; }

        /// <summary>
        /// Gets or sets the data for the current row.
        /// </summary>
        [Parameter]
        public T? RowData { get; set; }

        internal object? Value { get; set; }

        /// <summary>
        /// Used to disable the add form while editing any other row in the grid when the ShowAddNewRow property is set to true.
        /// </summary>
        [Parameter]
        public bool PreventAddForm { get; set; }
        
        #region ValueExpression
        internal Expression<Func<string>>? StringExp;

        internal Expression<Func<DateTime?>>? NullableDateExp;

        internal Expression<Func<DateTime>>? DateExp;

        internal Expression<Func<DateOnly?>>? NullableDateOnlyExp;

        internal Expression<Func<DateOnly>>? DateOnlyExp;

        internal Expression<Func<TimeOnly?>>? NullableTimeOnlyExp;

        internal Expression<Func<TimeOnly>>? TimeOnlyExp;

        internal Expression<Func<double>>? DoubleExp;

        internal Expression<Func<double?>>? NullableDoubleExp;

        internal Expression<Func<int>>? IntExp;

        internal Expression<Func<long>>? IntExp1;

        internal Expression<Func<int?>>? NullableIntExp;

        internal Expression<Func<float>>? FloatExp;

        internal Expression<Func<float?>>? NullableFloatExp;

        internal Expression<Func<decimal>>? DecimalExp;

        internal Expression<Func<object>>? ObjectExp;

        internal Expression<Func<decimal?>>? NullableDecimalExp;

        internal Expression<Func<long>>? LongExp;

        internal Expression<Func<long?>>? NullableLongExp;

        internal Expression<Func<ulong>>? UlongExp;

        internal Expression<Func<ulong?>>? NullableUlongExp;

        internal Expression<Func<short>>? ShortExp;

        internal Expression<Func<short?>>? NullableShortExp;

        internal Expression<Func<ushort>>? UshortExp;

        internal Expression<Func<ushort?>>? NullableUshortExp;

        internal Expression<Func<uint>>? UintExp;

        internal Expression<Func<uint?>>? NullableUintExp;

        internal Expression<Func<byte>>? ByteExp;

        internal Expression<Func<byte?>>? NullableByteExp;

        internal Expression<Func<sbyte>>? SbyteExp;

        internal Expression<Func<sbyte?>>? NullableSbyteExp;

        internal Expression<Func<DateTimeOffset>>? OffsetExp;

        internal Expression<Func<DateTimeOffset?>>? NullableOffsetExp;

        internal Expression<Func<Guid>>? GuidExp;

        internal Expression<Func<Guid?>>? NullableGuidExp;

        #endregion
        #region ValueChanged
        internal void ValueChanged<TVal>(TVal newValue)
        {
            Value = newValue!;
            if (Parent != null && Parent.EditModule != null)
            {
                Parent.EditModule.SetValue<TVal>(newValue, Column?.Field!);
            }
        }

        #endregion

        internal string? FieldName;

        internal string PlaceHolder =>
            Parent!.EditSettings?.Mode == EditMode.Dialog ? Column!.HeaderText! : string.Empty;

        internal FloatLabelType FloatLabelType =>
            Parent!.EditSettings?.Mode == EditMode.Dialog ? FloatLabelType.Always : FloatLabelType.Never;

        internal bool Enabled =>
            Parent!.EditModule!.isEditable(Column!);

        internal Dictionary<string, object> InputAttr =>
            new Dictionary<string, object>() { { "data-sf-style", $"text-align: {Column!.TextAlign}" } };

        /// <summary>
        /// Initializes the component and sets up dynamic expressions and validation for the editable cell.
        /// </summary>
        protected override void OnInitialized()
        {
            if (Parent != null && Parent.EditModule != null)
            Parent.EditModule.EditContext = EditContext;            
            var ComplexLength = Column!.Field.Split('.').Length;
            FieldName = ComplexLength > 1 ? Edit<T>.GetComplexName(Column?.Field!) : Column?.Field;
            if (string.IsNullOrEmpty(Column?.Field))
            {
                return;
            }

            if (RowData is ExpandoObject || RowData?.GetType()?.FullName?.Contains("ExpandoObject", StringComparison.Ordinal) == true)
            {
                DynamicExpressions<ExpandoObject>();
            }
            else if (RowData is DynamicObject || (Parent!.IsRenderedFromTreeGrid && RowData?.GetType()?.GenericTypeArguments?.Length > 0 && RowData?.GetType()?.GenericTypeArguments[0]?.BaseType?.FullName?.Contains("DynamicObject", StringComparison.Ordinal) == true))
            {
                DynamicExpressions<DynamicObject>();
            }
            else
            {
                var constant = Expression.Constant(this);
                var exp = Expression.PropertyOrField(constant, nameof(RowData));
                MemberExpression? exp1 = null;
                if (ComplexLength > 1)
                {
                    var ComplexExpression = exp;
                    for (var i = 0; i < ComplexLength; i++)
                    {
                        if (ComplexExpression?.Type?.Name?.Equals(nameof(ExpandoObject), StringComparison.Ordinal) == true)
                        {
                            DynamicExpressions<ExpandoObject>();
                        }
                        else if (ComplexExpression?.Type?.Name?.Equals(nameof(DynamicObject), StringComparison.Ordinal) == true || ComplexExpression?.Type?.IsSubclassOf(typeof(DynamicObject)) == true)
                        {
                            DynamicExpressions<DynamicObject>();
                        }
                        else
                        {
                            if (i == ComplexLength - 1)
                            {
                                exp1 = Expression.PropertyOrField(ComplexExpression!, Column?.Field?.Split('.')[i]!);
                            }
                            else
                            {
                                ComplexExpression = Expression.PropertyOrField(ComplexExpression!, Column?.Field?.Split('.')[i]!);
                            }
                        }
                    }
                }
                else
                {
                    exp1 = Expression.PropertyOrField(exp, Column?.Field!);
                }
                ValidateCheckType(exp1!);
            }
        }

        private void ValidateCheckType(MemberExpression exp1)
        {
            var CheckType = Column!.ValueType = exp1?.Type!;
            if (CheckType == typeof(double?))
            {
                NullableDoubleExp = Expression.Lambda<Func<double?>>(exp1!);
            }
            else if (CheckType == typeof(double))
            {
                DoubleExp = Expression.Lambda<Func<double>>(exp1!);
            }
            else if (CheckType == typeof(DateTime))
            {
                DateExp = Expression.Lambda<Func<DateTime>>(exp1!);
            }
            else if (CheckType == typeof(DateTime?))
            {
                NullableDateExp = Expression.Lambda<Func<DateTime?>>(exp1!);
            }
            else if (CheckType == typeof(DateOnly))
            {
                DateOnlyExp = Expression.Lambda<Func<DateOnly>>(exp1!);
            }
            else if (CheckType == typeof(DateOnly?))
            {
                NullableDateOnlyExp = Expression.Lambda<Func<DateOnly?>>(exp1!);
            }
            else if (CheckType == typeof(TimeOnly))
            {
                TimeOnlyExp = Expression.Lambda<Func<TimeOnly>>(exp1!);
            }
            else if (CheckType == typeof(TimeOnly?))
            {
                NullableTimeOnlyExp = Expression.Lambda<Func<TimeOnly?>>(exp1!);
            }
            else if (CheckType == typeof(int))
            {
                IntExp = Expression.Lambda<Func<int>>(exp1!);
            }
            else if (CheckType == typeof(int?))
            {
                NullableIntExp = Expression.Lambda<Func<int?>>(exp1!);
            }
            else if (CheckType == typeof(long))
            {
                IntExp1 = Expression.Lambda<Func<long>>(exp1!);
            }
            else if (CheckType == typeof(string))
            {
                StringExp = Expression.Lambda<Func<string>>(exp1!);
            }
            else if (CheckType == typeof(float))
            {
                FloatExp = Expression.Lambda<Func<float>>(exp1!);
            }
            else if (CheckType == typeof(float?))
            {
                NullableFloatExp = Expression.Lambda<Func<float?>>(exp1!);
            }
            else if (CheckType == typeof(decimal))
            {
                DecimalExp = Expression.Lambda<Func<decimal>>(exp1!);
            }
            else if (CheckType == typeof(object))
            {
                ObjectExp = Expression.Lambda<Func<object>>(exp1!);
            }
            else if (CheckType == typeof(decimal?))
            {
                NullableDecimalExp = Expression.Lambda<Func<decimal?>>(exp1!);
            }
            else if (CheckType == typeof(long))
            {
                LongExp = Expression.Lambda<Func<long>>(exp1!);
            }
            else if (CheckType == typeof(long?))
            {
                NullableLongExp = Expression.Lambda<Func<long?>>(exp1!);
            }
            else if (CheckType == typeof(ulong))
            {
                UlongExp = Expression.Lambda<Func<ulong>>(exp1!);
            }
            else if (CheckType == typeof(ulong?))
            {
                NullableUlongExp = Expression.Lambda<Func<ulong?>>(exp1!);
            }
            else if (CheckType == typeof(short))
            {
                ShortExp = Expression.Lambda<Func<short>>(exp1!);
            }
            else if (CheckType == typeof(short?))
            {
                NullableShortExp = Expression.Lambda<Func<short?>>(exp1!);
            }
            ValidateCheckTypes(exp1!);
        }

        private void ValidateCheckTypes(MemberExpression exp1)
        {
            var CheckType = Column!.ValueType = exp1?.Type!;

            if (CheckType == typeof(ushort))
            {
                UshortExp = Expression.Lambda<Func<ushort>>(exp1!);
            }
            else if (CheckType == typeof(ushort?))
            {
                NullableUshortExp = Expression.Lambda<Func<ushort?>>(exp1!);
            }
            else if (CheckType == typeof(uint))
            {
                UintExp = Expression.Lambda<Func<uint>>(exp1!);
            }
            else if (CheckType == typeof(uint?))
            {
                NullableUintExp = Expression.Lambda<Func<uint?>>(exp1!);
            }
            else if (CheckType == typeof(byte))
            {
                ByteExp = Expression.Lambda<Func<byte>>(exp1!);
            }
            else if (CheckType == typeof(byte?))
            {
                NullableByteExp = Expression.Lambda<Func<byte?>>(exp1!);
            }
            else if (CheckType == typeof(sbyte))
            {
                SbyteExp = Expression.Lambda<Func<sbyte>>(exp1!);
            }
            else if (CheckType == typeof(sbyte?))
            {
                NullableSbyteExp = Expression.Lambda<Func<sbyte?>>(exp1!);
            }
            else if (CheckType == typeof(DateTimeOffset))
            {
                OffsetExp = Expression.Lambda<Func<DateTimeOffset>>(exp1!);
            }
            else if (CheckType == typeof(DateTimeOffset?))
            {
                NullableOffsetExp = Expression.Lambda<Func<DateTimeOffset?>>(exp1!);
            }
            else if (CheckType == typeof(Guid?))
            {
                NullableGuidExp = Expression.Lambda<Func<Guid?>>(exp1!);
            }
            else if (CheckType == typeof(Guid))
            {
                GuidExp = Expression.Lambda<Func<Guid>>(exp1!);
            }
        }

        /// <summary>
        /// Builds dynamic expression trees for the specified column type to support data binding and editing.
        /// </summary>
        public void DynamicExpressions<TArg>()
        {
            var constant = Expression.Constant(this);
            var exp = Expression.Property(constant, nameof(RowData));
            Expression memExp;
            if (Column!.IsForeignColumn())
            {
                SetForeignKeyValueExpression(exp);
            }
            else if (Column.ValueType == typeof(string))
            {
                memExp = MemberTypeExp<string>(exp);
                StringExp = Expression.Lambda<Func<string>>(memExp);
            }
            else if (Column?.ValueType == typeof(DateTime?))
            {
                memExp = MemberTypeExp<DateTime?>(exp);
                NullableDateExp = Expression.Lambda<Func<DateTime?>>(memExp);
            }
            else if (Column?.ValueType == typeof(double?))
            {
                memExp = MemberTypeExp<double?>(exp);
                NullableDoubleExp = Expression.Lambda<Func<double?>>(memExp);
            }
            else if (Column?.ValueType == typeof(int?))
            {
                memExp = MemberTypeExp<int?>(exp);
                NullableIntExp = Expression.Lambda<Func<int?>>(memExp);
            }
            else if (Column?.ValueType == typeof(float?))
            {
                memExp = MemberTypeExp<float?>(exp);
                NullableFloatExp = Expression.Lambda<Func<float?>>(memExp);
            }
            else if (Column?.ValueType == typeof(DateTimeOffset?))
            {
                memExp = MemberTypeExp<DateTimeOffset?>(exp);
                NullableOffsetExp = Expression.Lambda<Func<DateTimeOffset?>>(memExp);
            }
            else if (Column?.ValueType == typeof(decimal?))
            {
                memExp = MemberTypeExp<decimal?>(exp);
                NullableDecimalExp = Expression.Lambda<Func<decimal?>>(memExp);
            }
            else if (Column?.ValueType == typeof(long?))
            {
                memExp = MemberTypeExp<long?>(exp);
                NullableLongExp = Expression.Lambda<Func<long?>>(memExp);
            }
            else if (Column?.ValueType == typeof(ulong?))
            {
                memExp = MemberTypeExp<ulong?>(exp);
                NullableUlongExp = Expression.Lambda<Func<ulong?>>(memExp);
            }
            else if (Column?.ValueType == typeof(short?))
            {
                memExp = MemberTypeExp<short?>(exp);
                NullableShortExp = Expression.Lambda<Func<short?>>(memExp);
            }
            else if (Column?.ValueType == typeof(ushort?))
            {
                memExp = MemberTypeExp<ushort?>(exp);
                NullableUshortExp = Expression.Lambda<Func<ushort?>>(memExp);
            }
            else if (Column?.ValueType == typeof(uint?))
            {
                memExp = MemberTypeExp<uint?>(exp);
                NullableUintExp = Expression.Lambda<Func<uint?>>(memExp);
            }
            else if (Column?.ValueType == typeof(Guid?))
            {
                memExp = MemberTypeExp<Guid?>(exp);
                NullableGuidExp = Expression.Lambda<Func<Guid?>>(memExp);
            }
        }

        private void SetForeignKeyValueExpression(MemberExpression exp)
        {
            Expression memExp;
            switch (Column?.ActualType)
            {
                case Type t when t == typeof(int?):
                    memExp = MemberTypeExp<int?>(exp);
                    NullableIntExp = Expression.Lambda<Func<int?>>(memExp);
                    break;

                case Type t when t == typeof(string):
                    memExp = MemberTypeExp<string>(exp);
                    StringExp = Expression.Lambda<Func<string>>(memExp);
                    break;

                case Type t when t == typeof(double?):
                    memExp = MemberTypeExp<double?>(exp);
                    NullableDoubleExp = Expression.Lambda<Func<double?>>(memExp);
                    break;

                case Type t when t == typeof(decimal?):
                    memExp = MemberTypeExp<decimal?>(exp);
                    NullableDecimalExp = Expression.Lambda<Func<decimal?>>(memExp);
                    break;

                case Type t when t == typeof(long?):
                    memExp = MemberTypeExp<long?>(exp);
                    NullableLongExp = Expression.Lambda<Func<long?>>(memExp);
                    break;

                case Type t when t == typeof(Guid?):
                    memExp = MemberTypeExp<Guid?>(exp);
                    NullableGuidExp = Expression.Lambda<Func<Guid?>>(memExp);
                    break;
            }
        }

        internal MemberExpression MemberTypeExp<TExp>(MemberExpression exp)
        {
            return Expression.Property(
                exp,
                new DynamicInfo<TExp>()
                         {
                             FieldName = Column!.Field,
                             FieldType = typeof(TExp),
                             DynamicType = typeof(T)
                         });
        }
    }
}
