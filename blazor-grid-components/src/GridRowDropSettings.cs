using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid row drag and drop.
    /// </summary>
    public partial class GridRowDropSettings : SfOwningComponentBase
    {

        [CascadingParameter]
        internal IGrid? Parent { get; set; }

        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// Defines the ID of droppable component on which row drop should occur.
        /// </summary>
        [Parameter]
        public string? TargetID { get; set; }

        private string? _targetID { get; set; }

        /// <summary>
        /// Gets or sets a value that enables dropping rows into empty areas of the grid content during drag-and-drop operations.
        /// </summary>
        /// <value>
        /// A boolean value indicating whether rows can be dropped into blank areas within the grid content.
        /// The default value is <c>true</c>.
        /// </value>
        /// <remarks>
        /// When set to <c>true</c>, users can drop rows not only above or below existing rows, 
        /// but also into empty spaces within the grid. If the grid contains rows and a row is dropped into an empty area, 
        /// it will be appended as the last row in the grid.
        /// When set to <c>false</c>, dropping is restricted to valid drop indicators displayed between rows only.
        /// <para>
        /// This setting applies to row reordering both within the grid and outside the grid.
        /// </para>
        /// </remarks>
        [Parameter]
        public bool AllowEmptyAreaDrop { get; set; } = true;

        private bool _allowEmptyAreaDrop { get; set; }

        internal static async Task<GridRowDropSettings> Initialize(SfBaseComponent baseComponent)
        {
            var GridRowDropSettings = new GridRowDropSettings();
            GridRowDropSettings.Parent = (IGrid)baseComponent;
            GridRowDropSettings.BaseParent = (IGrid)baseComponent;
            await GridRowDropSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridRowDropSettings;
        }

        /// <summary>
        /// Invoked during component initialization.
        /// Updates the parent grid with the current row drop settings and initializes internal state fields.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(IGrid.RowDropSettings), this);
            _targetID = TargetID;
            _allowEmptyAreaDrop = AllowEmptyAreaDrop;
        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Updates internal state for target ID and empty area drop settings if they have changed.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            if(!SfBaseUtils.Equals(TargetID, _targetID))
            {
                _targetID = TargetID;
            }
            if(!SfBaseUtils.Equals(AllowEmptyAreaDrop, _allowEmptyAreaDrop))
            {
                _allowEmptyAreaDrop = AllowEmptyAreaDrop;
            }
        }
    }
}
