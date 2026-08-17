using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid aggregates.
    /// </summary>
    public partial class GridAggregates : SfOwningComponentBase
    {

        [CascadingParameter]
        internal IGrid? Parent { get; set; }

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
        /// Gets or sets the aggregate row collection.
        /// </summary>
        public List<GridAggregate> Aggregates { get; set; } = new List<GridAggregate>();

        internal int UpdateChildProperty(GridAggregate value)
        {
            Aggregates.Add(value);
            return Aggregates.Count - 1;
        }

        /// <summary>
        /// Initializes the component asynchronously and updates parent references.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(Aggregates), Aggregates);
            if(BaseParent != null)
            {
                BaseParent.HasAggregateChanges = true;
            }

        }
    }
}
