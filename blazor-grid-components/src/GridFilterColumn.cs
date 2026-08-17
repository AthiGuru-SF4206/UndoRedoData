using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid filter column.
    /// </summary>
    public partial class GridFilterColumn : SfOwningComponentBase
    {

        [CascadingParameter]
        internal GridFilterColumns? Parent { get; set; }

        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// Defines the child content.
        /// </summary>
        /// <exclude/>
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Defines the field name of the filter column.
        /// </summary>
        [Parameter]
        public string Field { get; set; } = string.Empty;

        private string? _field { get; set; }

        /// <summary>
        /// If ignoreAccent is set to true, then filter ignores the diacritic characters or accents while filtering.
        /// </summary>
        [Parameter]
        public bool IgnoreAccent { get; set; }

        private bool _ignoreAccent { get; set; }

        /// <summary>
        /// If match case set to true, then filter records with exact match or else
        /// filter records with case insensitive(uppercase and lowercase letters treated as same).
        /// </summary>
        [Parameter]
        public bool MatchCase { get; set; }

        private bool _matchCase { get; set; }

        /// <summary>
        /// Defines the operator to filter records.
        /// </summary>
        [Parameter]
        public Syncfusion.Blazor.Operator Operator { get; set; }

        private Syncfusion.Blazor.Operator _operator { get; set; }

         /// <summary>
        /// Defines the RawInputValue to filter records.
        /// </summary>
        internal string? RawInputValue { get; set; }
        
        /// <summary>
        /// Defines the relationship between one filter query and another by using AND or OR predicate.
        /// </summary>
        [Parameter]
        public string? Predicate { get; set; }

        private string? _predicate { get; set; }

        /// <summary>
        /// Defines the UID of filter column.
        /// </summary>
        public string? Uid { get; set; }

        private string? _uid { get; set; }

        /// <summary>
        /// Defines the value used to filter records.
        /// </summary>
        [Parameter]
        public object? Value { get; set; }

        private object? _value { get; set; }

        /// <summary>
        /// Defines the Actual value used to filter records.
        /// </summary>
        public object? ActualValue { get; set; }

        private object? _actualValue { get; set; }

        /// <summary>
        /// The value set to the PreventFilterQuery property in OnActionBegin event handler is maintained by using this property.
		/// This helps to prevent the default filter query generation for previously filtered columns during the multiple column filtering.
        /// </summary>
        internal bool PreventFilterQuery { get; set; }

        /// <summary>
        /// Specifies the column type of the <see cref="Syncfusion.Blazor.Grids.GridColumn"/>, denoting the type of data it displays. 
        /// </summary>
        internal string? ColumnType { get; set; }


        /// <summary>
        /// Initializes the GridFilterColumn, updates its properties in the parent grid, 
        /// and synchronizes internal state for filtering configuration.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperty(this);
            _field = Field;
            _ignoreAccent = IgnoreAccent;
            _matchCase = MatchCase;
            _operator = Operator;
            _predicate = Predicate;
            _uid = Uid = Field != null ? Syncfusion.Blazor.Grids.Internal.GridUtils.GetColumnByField(Field, (List<GridColumn>)BaseParent!.Columns!)?.Uid! : null!;
            _value = Value;
            _actualValue = ActualValue;
            if (BaseParent != null)
            {
                BaseParent.HasFilterColumnChanges = false;
            }

        }

        /// <summary>
        /// Handles parameter updates for GridFilterColumn and synchronizes internal state when filter-related values change.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            if(!SfBaseUtils.Equals(Field, _field) || !SfBaseUtils.Equals(IgnoreAccent, _ignoreAccent)
            || !SfBaseUtils.Equals(MatchCase, _matchCase) || !SfBaseUtils.Equals(Operator, _operator)
            || !SfBaseUtils.Equals(Predicate, _predicate) || !SfBaseUtils.Equals(Uid, _uid)
            || !SfBaseUtils.Equals(Value, _value) || !SfBaseUtils.Equals(ActualValue, _actualValue))
            {
                _field = Field;
                _ignoreAccent = IgnoreAccent;
                _matchCase = MatchCase;
                _operator = Operator;
                _predicate = Predicate;
                _uid = Uid;
                _value = Value;
                _actualValue = ActualValue;
            }
        }
    }
}
