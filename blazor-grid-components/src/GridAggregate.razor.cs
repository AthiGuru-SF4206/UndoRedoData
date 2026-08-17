using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Syncfusion.Blazor.Grids.Internal;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid aggregate.
    /// </summary>
    public partial class GridAggregate : SfDataBoundComponent
    {
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

        [CascadingParameter]
        internal GridAggregates? Parent { get; set; }

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
        /// Configures the aggregate columns.
        /// </summary>
        [Parameter]
        public List<GridAggregateColumn>? Columns { get; set; }

        private List<GridAggregateColumn>? _columns { get; set; }

        /// <summary>
        /// Updates the child properties of the aggregate based on the specified key.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void UpdateChildProperties(string key, List<GridAggregateColumn> value)
        {
            if (key == nameof(Columns))
            {
                Columns = _columns = value;
            }

            DirectParameters.AddOrUpdateItem(key, value);
        }

        /// <summary>
        /// Initializes the component asynchronously and updates parent references.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            MainParent = (SfBaseComponent)BaseParent!;
            Parent?.UpdateChildProperty(this);
            _columns = Columns;
        }

        /// <summary>
        /// Sets component parameters asynchronously and updates column properties.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            Columns = _columns = (await UpdateProperty(nameof(Columns), Columns, _columns).ConfigureAwait(true))!;

            //Telementry for Aggregates
            GridTelemetryHelper.LogTelemetry(true, "Aggregates");
        }
    }
}
