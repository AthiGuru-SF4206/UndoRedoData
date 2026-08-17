using System;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid aggregate column.
    /// </summary>
    public partial class GridAggregateColumn : SfOwningComponentBase
    {

        [CascadingParameter]
        internal GridAggregateColumns? Parent { get; set; }

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
        /// Defines the column name to display the aggregate value. If ColumnName is not defined,
        /// then Field name value will be assigned to the ColumnName` property.
        /// </summary>
        [Parameter]
        public string? ColumnName { get; set; }

        private string? _columnName { get; set; }

        /// <summary>
        /// Defines the column name to perform aggregation.
        /// </summary>
        [Parameter]
        public string? Field { get; set; }

        private string? _field { get; set; }

        /// <summary>
        /// Defines the cell template for the footer aggregate column.
        /// The Type name should be used to access aggregate values inside the template.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.AggregateTemplateContext"/>.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? FooterTemplate { get; set; }

        /// <summary>
        /// Format is applied to a calculated value before it is displayed.
        /// Gets the format from the user, which can be standard or custom formats.
        /// </summary>
        [Parameter]
        public string? Format { get; set; }

        private string? _format { get; set; }

        /// <summary>
        /// Defines the cell template for the group caption aggregate column.
        /// The Type name should be used to access aggregate values inside the template.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.AggregateTemplateContext"/>.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? GroupCaptionTemplate { get; set; }

        /// <summary>
        /// Defines the cell template for the group footer aggregate column.
        /// The Type name should be used to access aggregate values inside the template.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.AggregateTemplateContext"/>.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? GroupFooterTemplate { get; set; }

        /// <summary>
        /// Defines the aggregate type of a particular column.
        /// Types of aggregate supported in-built are,.
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="Syncfusion.Blazor.Grids.AggregateType.Sum"/></term>
        /// <description>Performes sum aggregation.</description>
        /// </item>
        /// <item><term><see cref="Syncfusion.Blazor.Grids.AggregateType.Average"/></term>
        /// <description>Performes average aggregation.</description>
        /// </item>
        /// <item><term><see cref="Syncfusion.Blazor.Grids.AggregateType.Count"/></term>
        /// <description>Performes count aggregation.</description>
        /// </item>
        /// <item><term><see cref="Syncfusion.Blazor.Grids.AggregateType.FalseCount"/></term>
        /// <description>Performes false count aggregation.</description>
        /// </item>
        /// <item><term><see cref="Syncfusion.Blazor.Grids.AggregateType.TrueCount"/></term>
        /// <description>Performes true count aggregation.</description>
        /// </item>
        /// <item><term><see cref="Syncfusion.Blazor.Grids.AggregateType.Max"/></term>
        /// <description>Performes max aggregation.</description>
        /// </item>
        /// <item><term><see cref="Syncfusion.Blazor.Grids.AggregateType.Min"/></term>
        /// <description>Performes min aggregation.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public object? Type { get; set; }

        private object? _type { get; set; }

        /// <summary>
        /// Initializes the component asynchronously and updates parent references.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperty(this);
            _columnName = ColumnName ?? Field;
            _field = Field;
            _format = Format;
            _type = Type;
            BaseParent!.HasAggregateChanges = false;
        }

        /// <summary>
        /// Handles parameter updates asynchronously and refreshes internal state when bound values change.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            if (!SfBaseUtils.Equals(ColumnName, _columnName) || !SfBaseUtils.Equals(Field, _field)
                || !SfBaseUtils.Equals(Format, _format) || !SfBaseUtils.Equals(Type, _type))
            {
                _columnName = ColumnName;
                _field = Field;
                _format = Format;
                _type = Type;
            }
        }
    }
}
