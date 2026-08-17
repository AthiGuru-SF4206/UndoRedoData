using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid sort columns.
    /// </summary>
    public partial class GridSortColumns : SfOwningComponentBase
    {

        [CascadingParameter]
        internal GridSortSettings? Parent { get; set; }

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
        /// Gets the sorted columns collection, use <see cref="Syncfusion.Blazor.Grids.GridSortColumn"/> component
        /// to set initial sort columns.
        /// </summary>
        public List<GridSortColumn> Columns { get; set; } = new List<GridSortColumn>();

        internal int UpdateChildProperty(GridSortColumn value)
        {
            Columns.Add(value);
            return Columns.Count - 1;
        }

        /// <summary>
        /// Invoked during component initialization.
        /// Updates the parent grid with the current column settings and triggers a UI refresh if a base parent exists.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(Columns), Columns);
            if(BaseParent != null)
            {
                BaseParent.HasSortColumnChanges = true;
            }

        }
    }
}
