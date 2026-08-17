using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid filter columns.
    /// </summary>
    public partial class GridFilterColumns : SfOwningComponentBase
    {

        [CascadingParameter]
        internal GridFilterSettings? Parent { get; set; }

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
        /// Defines the filter column collection. Use <see cref="Syncfusion.Blazor.Grids.GridFilterColumn"/> component
        /// to define filter criteria.
        /// </summary>
        public List<GridFilterColumn> Columns { get; set; } = new List<GridFilterColumn>();

        internal int UpdateChildProperty(GridFilterColumn value)
        {
            Columns.Add(value);
            return Columns.Count - 1;
        }

        /// <summary>
        /// Initializes the GridFilterColumn and updates its column properties in the parent grid.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(Columns), Columns);
            if (BaseParent != null)
            {
                BaseParent.HasFilterColumnChanges = true;
            }
            
        }
    }
}
