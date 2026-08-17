using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid column chooser settings.
    /// </summary>
    public partial class GridColumnChooserSettings : SfDataBoundComponent
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
        /// Defines the search operator for Column Chooser.
        /// By default search operator is <see cref="Syncfusion.Blazor.Operator"/>.
        /// </summary>
        [Parameter]
        public Syncfusion.Blazor.Operator Operator { get; set; } = Operator.StartsWith;

        private Syncfusion.Blazor.Operator _operator { get; set; } = Operator.StartsWith;
        /// <summary>
        /// Defines the custom content for the column chooser dialog. This can be used to introduce own UI inside the column chooser dialog content.
        /// The checkboxes can be rendered using <see cref="Syncfusion.Blazor.Grids.GridColumnChooserItem"/> component.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.ColumnChooserTemplateContext"/> of the grid.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? Template { get; set; }
        /// <summary>
        /// Defines the custom footer content for the column chooser.This can be used to introduce own UI inside the column chooser dialog footer content.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.ColumnChooserFooterTemplateContext"/> of the grid.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? FooterTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value that determines whether the header text in the column chooser dialog wraps to the next line when it exceeds the width of the dialog.
        /// </summary>
        /// <value>
        /// <b>true</b> to enable text wrapping; otherwise, <b>false</b>. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// Text wrapping is not applicable when using the <see cref="Syncfusion.Blazor.Grids.ColumnChooserTemplateContext"/>.
        /// </remarks>
        [Parameter]
        public bool AllowTextWrap { get; set; }

        private bool _allowTextWrap { get; set; }

        internal static async Task<GridColumnChooserSettings> Initialize(SfDataBoundComponent baseComponent)
        {
            var gridColumnChooserSettings = new GridColumnChooserSettings();
            gridColumnChooserSettings.Parent = (IGrid)baseComponent;
            gridColumnChooserSettings.BaseParent = (IGrid)baseComponent;
            await gridColumnChooserSettings.OnInitializedAsync().ConfigureAwait(true);
            return gridColumnChooserSettings;
        }

        /// <summary>
        /// Initializes and sets default values.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            MainParent = (SfBaseComponent)BaseParent!;
            Parent?.UpdateChildProperties(nameof(IGrid.ColumnChooserSettings), this);
            _operator = Operator;
            _allowTextWrap = AllowTextWrap;
        }

        /// <summary>
        /// Updates property values when component parameters are set.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            Operator = _operator = await UpdateProperty(nameof(Operator), Operator, _operator).ConfigureAwait(true);
            AllowTextWrap = _allowTextWrap = await UpdateProperty(nameof(AllowTextWrap), AllowTextWrap, _allowTextWrap).ConfigureAwait(true);
        }
    }
}
