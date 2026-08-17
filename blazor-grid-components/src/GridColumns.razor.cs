using System.Collections.Generic;
using Syncfusion.Blazor.Internal;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid columns.
    /// </summary>
    public partial class GridColumns : SfDataBoundComponent
    {
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

        [CascadingParameter]
        internal object? Parent { get; set; }

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
        /// Gets or sets the collection of grid columns.
        /// </summary>
        public List<GridColumn> Columns { get; set; } = new List<GridColumn>();

        internal int UpdateChildProperty(GridColumn value)
        {
            Columns.Add(value);
            return Columns.Count - 1;
        }

        /// <summary>
        /// Initializes the GridColumn and updates its column properties in the parent grid.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            MainParent = (SfBaseComponent)BaseParent!;
            ((ISfCircularComponent)Parent!).UpdateChildProperties(nameof(Columns), Columns);
        }

        // Remove column from collection on dynamic removal.
        internal int RemoveChildProperty(GridColumn value)
        {
            Columns.Remove(value);
            return Columns.Count - 1;
        }
    }
}
