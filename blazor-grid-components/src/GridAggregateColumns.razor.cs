using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid aggregate columms.
    /// </summary>
    public partial class GridAggregateColumns : SfOwningComponentBase
    {

        [CascadingParameter]
        internal GridAggregate? Parent { get; set; }

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
        /// Defines the aggregate columns.
        /// </summary>
        public List<GridAggregateColumn> Columns { get; set; } = new List<GridAggregateColumn>();

        internal int UpdateChildProperty(GridAggregateColumn value)
        {
            Columns.Add(value);
            return Columns.Count - 1;
        }

        /// <summary>
        /// Initializes the component asynchronously and updates parent references.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(Columns), Columns);
            if(BaseParent != null)
            {
                BaseParent.HasAggregateChanges = true;
            }

        }
    }
}
