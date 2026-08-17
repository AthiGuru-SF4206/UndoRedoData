using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using Syncfusion.Blazor.Internal;
using Syncfusion.Blazor.Grids.Internal;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid infinite scrolling.
    /// </summary>
    public partial class GridInfiniteScrollSettings : SfOwningComponentBase
    {
        [CascadingParameter]
        internal IGrid? Parent { get; set; }

        /// <summary>
        /// Defines the child content.
        /// </summary>
        /// <exclude/>
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the number of blocks to be initially rendered when the Grid is loaded. Each block corresponds to a page size of the Grid, resulting in the rendering of a certain number of <c>tr</c> elements determined by multiplying the initial block size with the page size.
        /// </summary>
        /// <value>
        /// The default value is 3, indicating that the Grid will display three blocks, which translates to a specific number of <c>tr</c> elements, during the initial rendering.
        /// </value>
        /// <remarks>
        /// The <c>InitialBlocks</c> property determines the quantity of blocks, each equivalent to a page size, to display during the first load. Additional blocks will be dynamically loaded as the user scrolls through the Grid. For example, when the bottom of the scrollbar is reached, one block of records will be fetched and loaded to display new set of data's.
        /// If the <see cref="Syncfusion.Blazor.Grids.GridPageSettings.PageSize"/> is not explicitly provided, it will be calculated based on the viewport height to ensure an optimal user experience.
        /// The <c>InitialBlocks</c> size should be set greater than the viewport height to guarantee a smooth and seamless user experience.
        /// </remarks>
        [Parameter]
        public int InitialBlocks { get; set; } = 3;
        private int _initialBlocks { get; set; }

        /// <summary>
        /// Gets or sets the number of blocks to be rendered in the Grid during scrolling when <c>EnableCache</c> is set to true. 
        /// This caching mode optimizes performance by storing blocks in a cache for large data sets.
        /// </summary>
        /// <value>
        /// The default value is 3.
        /// </value>
        /// <remarks>
        /// When <c>EnableCache</c> is set to true, the <c>MaximumBlocks</c> property determines the maximum number of blocks to be kept in the cache. Exceeding this limit will remove the oldest blocks from the cache to make space for new ones.
        /// </remarks>
        [Parameter]
        public int MaximumBlocks { get; set; } = 3;
        private int _maximumBlocks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Grid will cache visited blocks of data, allowing reuse of previously loaded block data when revisiting the same block. This reduces the frequency of data requests while navigating through the same block.
        /// </summary>
        /// <value>
        /// <c>true</c> to enable data caching for visited blocks; <c>false</c> by default.
        /// </value>      
        /// <remarks>
        /// Enabling this caching mode maintains row elements in the DOM based on the <see cref="Syncfusion.Blazor.Grids.GridInfiniteScrollSettings.MaximumBlocks"/> count. If the number of maintained rows exceeds this limit during scrolling, the Grid removes rows from the DOM to accommodate new row elements.
        /// </remarks>
        [Parameter]
        public bool EnableCache { get; set; }
        private bool _enableCache { get; set; }

        internal static async Task<GridInfiniteScrollSettings> Initialize(SfBaseComponent baseComponent)
        {
            var GridInfiniteScrollSettings = new GridInfiniteScrollSettings();
            GridInfiniteScrollSettings.Parent = (IGrid)baseComponent;
            GridInfiniteScrollSettings.Parent = (IGrid)baseComponent;
            await GridInfiniteScrollSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridInfiniteScrollSettings;
        }


        /// <summary>
        /// Invoked during component initialization.
        /// Updates the parent grid with the current infinite scroll settings and initializes internal state fields.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(IGrid.InfiniteScrollSettings), this);
            _enableCache = EnableCache;
            _initialBlocks = InitialBlocks;
            _maximumBlocks = MaximumBlocks;
        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Updates internal state for virtualization settings such as initial blocks, cache enablement,
        /// and maximum blocks if any of these values have changed.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);
            if (!SfBaseUtils.Equals(InitialBlocks, _initialBlocks) || !SfBaseUtils.Equals(EnableCache, _enableCache)
                || !SfBaseUtils.Equals(MaximumBlocks, _maximumBlocks))
            {
                _initialBlocks = InitialBlocks;
                _enableCache = EnableCache;
                _maximumBlocks = MaximumBlocks;
            }
            // Telemetry for infinite scroll
            GridTelemetryHelper.LogTelemetry(true, "InfiniteScroll");
        }
    }
}
