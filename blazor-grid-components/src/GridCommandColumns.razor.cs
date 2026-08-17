using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid command columns.
    /// </summary>
    public partial class GridCommandColumns : SfOwningComponentBase
    {

        [CascadingParameter]
        internal GridColumn? Parent { get; set; }

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
        /// Defines the command button collection. Use <see cref="Syncfusion.Blazor.Grids.GridCommandColumn"/> component
        /// to provide command buttons.
        /// </summary>
        public List<GridCommandColumn> Commands { get; set; } = new List<GridCommandColumn>();

        internal int UpdateChildProperty(GridCommandColumn value)
        {
            Commands.Add(value);
            return Commands.Count - 1;
        }

        /// <summary>
        /// Initializes the GridCommandColumn and updates its command properties in the parent grid.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(Commands), Commands);
        }
    }
}
