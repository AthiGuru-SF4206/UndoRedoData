using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids.Internal;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid sorting.
    /// </summary>
    public partial class GridSortSettings : SfDataBoundComponent
    {
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

        private bool isAutoInitialized { get; set; }

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
        /// If AllowUnsort set to false the user can not get the grid in unsorted state by clicking the sorted column header.
        /// </summary>
        [Parameter]
        public bool AllowUnsort { get; set; } = true;

        private bool _allowUnsort { get; set; }

        /// <summary>
        /// Specifies the columns to sort at initial rendering of Grid.
        /// Also user can get current sorted columns, use <see cref="Syncfusion.Blazor.Grids.GridSortColumn"/> component
        /// to set initial sort columns.
        /// </summary>
        [Parameter]
        public List<GridSortColumn>? Columns { get; set; }

        private List<GridSortColumn>? _columns { get; set; }

        /// <summary>
        /// Updates the child property for the specified key and value.
        /// Synchronizes the <see cref="Columns"/> property if the key matches.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void UpdateChildProperties(string key, List<GridSortColumn> value)
        {
            if (key == nameof(Columns))
            {
                Columns = _columns = value;
            }

            DirectParameters.AddOrUpdateItem(key, value);
        }

        internal static async Task<GridSortSettings> Initialize(SfDataBoundComponent baseComponent)
        {
            var GridSortSettings = new GridSortSettings();
            GridSortSettings.Parent = (IGrid)baseComponent;
            GridSortSettings.BaseParent = (IGrid)baseComponent;
            GridSortSettings.isAutoInitialized = true;
            await GridSortSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridSortSettings;
        }

        /// <summary>
        /// Invoked during component initialization.
        /// Sets the main parent reference, updates sort settings in the parent grid, and initializes internal state.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            if (BaseParent is SfBaseComponent baseComponent)
            {
                MainParent = baseComponent;
            }
            Parent?.UpdateChildProperties(nameof(IGrid.SortSettings), this);
            _allowUnsort = AllowUnsort;
            _columns = Columns;
            if (!isAutoInitialized && BaseParent != null)
            {
                BaseParent.HasSortColumnChanges = true;
            }
        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Synchronizes properties such as <see cref="AllowUnsort"/> and <see cref="Columns"/> with their previous values,
        /// and resets sort column change tracking if no child content is provided.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            AllowUnsort = _allowUnsort = await UpdateProperty(nameof(AllowUnsort), AllowUnsort, _allowUnsort).ConfigureAwait(true);
            Columns = _columns = await UpdateProperty(nameof(Columns), Columns, _columns!).ConfigureAwait(true);

            if (ChildContent == null && BaseParent != null)
            {
                BaseParent.HasSortColumnChanges = false;
            }
            // Telementry for Sorting
            GridTelemetryHelper.LogTelemetry(true, "Sorting");
        }
        
        internal async Task UpdateProperties(string key, List<GridSortColumn> value)
        {
            if (key == nameof(Columns))
            {
                var columns = DirectParameters.TryGetValue("Columns", out object? Value) ? (List<GridSortColumn>)Value : Columns;
                Columns = _columns = await UpdateProperty(nameof(Columns), columns, value).ConfigureAwait(true);
            }
        }

        internal void UpdateColumns(string key, List<GridSortColumn> value)
        {
            if (key == nameof(Columns))
            {
                Columns = _columns = value;
            }
        }
    }
}
