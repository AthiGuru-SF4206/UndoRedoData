using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid grouping.
    /// </summary>
    public partial class GridGroupSettings : SfDataBoundComponent
    {
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

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
        /// If AllowReordering is set to true, Grid allows the grouped elements to be reordered.
        /// </summary>
        [Parameter]
        public bool AllowReordering { get; set; }

        private bool _allowReordering { get; set; }

        /// <summary>
        /// The Caption Template allows user to display custom group caption.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.CaptionTemplateContext"/> of the grid.
        /// </remarks>
        [Parameter]
        [JsonIgnore]
        public RenderFragment<object>? CaptionTemplate { get; set; }

        /// <summary>
        /// Specifies the column names to group at initial rendering of the Grid.
        /// You can also get the currently grouped columns.
        /// </summary>
        [Parameter]
        public string[]? Columns { get; set; }

        private string[]? _columns { get; set; }

        /// <summary>
        /// If DisablePageWiseAggregates set to true, then the group aggregate value will
        /// be calculated from the whole data instead of paged data and two requests will be made for each page
        /// when Grid bound with remote service.
        /// </summary>
        [Parameter]
        public bool DisablePageWiseAggregates { get; set; }

        private bool _disablePageWiseAggregates { get; set; }

        /// <summary>
        /// If ShowDropArea is set to true, the group drop area element will be visible at the top of the Grid.
        /// </summary>
        [Parameter]
        public bool ShowDropArea { get; set; } = true;

        private bool _showDropArea { get; set; }

        /// <summary>
        /// If ShowGroupedColumn is set to false, it hides the grouped column after grouping.
        /// </summary>
        [Parameter]
        public bool ShowGroupedColumn { get; set; }

        private bool _showGroupedColumn { get; set; }

        /// <summary>
        /// If ShowToggleButton set to true, then the toggle button will be showed in the column headers which can be used to group
        /// or ungroup columns by clicking them.
        /// </summary>
        [Parameter]
        public bool ShowToggleButton { get; set; }

        private bool _showToggleButton { get; set; }

        /// <summary>
        /// If ShowUngroupButton set to false, then ungroup button is hidden in dropped element.
        /// It can be used to ungroup the grouped column when click on ungroup button.
        /// </summary>
        [Parameter]
        public bool ShowUngroupButton { get; set; } = true;

        private bool _showUngroupButton { get; set; }

        /// <summary>
        /// The Lazy load grouping, allows the Grid to render only the initial level caption rows in collapsed state while grouping.
        /// The child rows of each caption will render only when we expand the captions.
        /// </summary>
        [Parameter]
        public bool EnableLazyLoading { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether all groups in the grid are expanded
        /// by default during the initial render, when row virtualization is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> to expand all groups initially; otherwise, <c>false</c>. The default is <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property only affects the grid if <see cref="SfGrid{TValue}.EnableVirtualization"/> is set to <c>true</c>.
        /// If <see cref="GridGroupSettings.EnableLazyLoading"/> is <c>true</c>, this property is ignored, and groups are not auto expanded.
        ///
        /// <para>
        /// <b>Performance Note:</b> Expanding all groups on large, virtualized datasets may have a noticeable impact on grid rendering performance.
        /// Consider leaving this property as <c>false</c> to optimize load times with large data sources.
        /// </para>
        /// </remarks>
        /// <seealso cref="GridGroupSettings.EnableLazyLoading"/>
        /// <seealso cref="SfGrid{TValue}.EnableVirtualization"/>
        [Parameter]
        [JsonIgnore]
        public bool ExpandAllGroups { get; set; }
        private bool _expandAllGroups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the expand or collapse state of grouped rows is retained during <see cref="SfGrid{TValue}"/> operations such as sorting, filtering, paging, editing, and <see cref="SfGrid{TValue}.Refresh"/>.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> indicating whether the expand or collapse state of grouped rows is preserved during <see cref="SfGrid{TValue}"/> operations. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// When set to <c>true</c>, the <see cref="SfGrid{TValue}"/> preserves the expanded or collapsed state of groups using a <c>GroupKey</c>. The <c>GroupKey</c> represents the common value under which rows are grouped, such as a specific <c>CustomerID</c> when grouping by that column. For example, if grouped by <c>CustomerID</c> with a value of <c>ALFKI</c>, all rows with <c>CustomerID = ALFKI</c> are grouped under the header <c>Customer ID: ALFKI</c>, and their expand or collapse state is retained after operations like sorting or filtering.
        /// </para>
        /// <para>
        /// The expand or collapse state is reset when grouping or ungrouping actions are performed, as these actions alter the grid's grouping structure.
        /// </para>
        /// <para>
        /// Grouping state persistence is distinct from grid persistence. While <see cref="SfGrid{TValue}.EnablePersistence"/> may preserve other grid settings across sessions, <see cref="PersistGroupState"/> does not maintain the grouping state when the browser page is reloaded, navigated, or when the data source is dynamically changed.
        /// </para>
        /// <para>
        /// Performance note: Enabling this feature may increase memory usage for large datasets due to the storage of group states.
        /// </para>
        /// </remarks>
        /// <example>
        /// Demonstrates how to enable <see cref="PersistGroupState"/> to retain the expand or collapse state of grouped rows in a <see cref="SfGrid{TValue}"/>.
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" AllowGrouping="true" AllowSorting="true">
        ///     <GridGroupSettings PersistGroupState="true" Columns="@(new string[] { 'CustomerID' })">
        ///     </GridGroupSettings>
        /// </SfGrid>
        /// @code {
        ///     public List<Order> Orders { get; set; } = new List<Order>
        ///     {
        ///         new Order { CustomerID = "ALFKI", OrderID = 1001 },
        ///         new Order { CustomerID = "ALFKI", OrderID = 1002 },
        ///         new Order { CustomerID = "BENDR", OrderID = 1003 }
        ///     };
        ///
        ///     public class Order
        ///     {
        ///         public string CustomerID { get; set; }
        ///         public int OrderID { get; set; }
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        [JsonIgnore]
        public bool PersistGroupState { get; set; }
        private bool _persistGroupState { get; set; }

        internal static async Task<GridGroupSettings> Initialize(SfDataBoundComponent baseComponent)
        {
            var GridGroupSettings = new GridGroupSettings();
            GridGroupSettings.Parent = (IGrid)baseComponent;
            GridGroupSettings.BaseParent = (IGrid)baseComponent;
            await GridGroupSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridGroupSettings;
        }

        /// <summary>
        /// Invoked during component initialization.
        /// Updates the parent grid with the current group settings and initializes internal state fields.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            if (BaseParent is SfBaseComponent baseComponent)
            {
                MainParent = baseComponent;
            }
            Parent?.UpdateChildProperties(nameof(IGrid.GroupSettings), this);
            _allowReordering = AllowReordering;
            _columns = Columns;
            _disablePageWiseAggregates = DisablePageWiseAggregates;
            _showDropArea = ShowDropArea;
            _showGroupedColumn = ShowGroupedColumn;
            _showToggleButton = ShowToggleButton;
            _showUngroupButton = ShowUngroupButton;
        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Synchronizes grouping-related properties such as column reordering, drop area visibility,
        /// toggle buttons, and persistence of group state, then notifies the parent component if changes occur.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            AllowReordering = _allowReordering = await UpdateProperty(nameof(AllowReordering), AllowReordering, _allowReordering).ConfigureAwait(true);
            Columns = _columns = await UpdateProperty(nameof(Columns), Columns, _columns).ConfigureAwait(true);
            DisablePageWiseAggregates = _disablePageWiseAggregates = await UpdateProperty(nameof(DisablePageWiseAggregates), DisablePageWiseAggregates, _disablePageWiseAggregates).ConfigureAwait(true);
            ShowDropArea = _showDropArea = await UpdateProperty(nameof(ShowDropArea), ShowDropArea, _showDropArea).ConfigureAwait(true);
            ShowGroupedColumn = _showGroupedColumn = await UpdateProperty(nameof(ShowGroupedColumn), ShowGroupedColumn, _showGroupedColumn).ConfigureAwait(true);
            ShowToggleButton = _showToggleButton = await UpdateProperty(nameof(ShowToggleButton), ShowToggleButton, _showToggleButton).ConfigureAwait(true);
            ShowUngroupButton = _showUngroupButton = await UpdateProperty(nameof(ShowUngroupButton), ShowUngroupButton, _showUngroupButton).ConfigureAwait(true);
            ExpandAllGroups = _expandAllGroups = await UpdateProperty(nameof(ExpandAllGroups), ExpandAllGroups, _expandAllGroups).ConfigureAwait(true);
            PersistGroupState = _persistGroupState = await UpdateProperty(nameof(PersistGroupState), PersistGroupState, _persistGroupState).ConfigureAwait(true);

            if (PropertyChanges.Count > 0 && BaseParent != null)
            {
                if (PropertyChanges.ContainsKey("PersistGroupState"))
                {
                    ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd("PersistGroupState", this.PersistGroupState);
                }
                ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd(nameof(IGrid.GroupSettings), this);
                PropertyChanges.Clear();
                await BaseParent.PropertyChanged().ConfigureAwait(true);
            }
            // Track telemetry for grouping features
            GridTelemetryHelper.LogTelemetry(true, "Grouping");
        }

        internal async Task UpdateProperties(string key, string[] value)
        {
            if (key == nameof(Columns))
            {
                var columns = DirectParameters.TryGetValue("Columns", out object? val) ? (string[])val : Columns;
                Columns = _columns = await UpdateProperty(nameof(Columns), columns, value).ConfigureAwait(true);
            }
        }
    }
}
