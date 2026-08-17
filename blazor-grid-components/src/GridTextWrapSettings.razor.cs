using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid text wrapping.
    /// </summary>
    public partial class GridTextWrapSettings : SfOwningComponentBase
    {
        #region Cascading Context
        [CascadingParameter]
        internal IGrid? Parent { get; set; }

        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }
        #endregion
        
        #region Parameters
        /// <summary>
        /// Defines the WrapMode` of the Grid. The available modes are:
        /// <list type="bullet">
        /// <item>
        /// <term>Both</term>
        /// <description>Default. Wraps both header and content.</description>
        /// </item>
        /// <item>
        /// <term>Header</term>
        /// <description>Wraps header alone.</description>
        /// </item>
        /// <item>
        /// <term>Content</term>
        /// <description>Wraps content alone.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public WrapMode WrapMode { get; set; } = WrapMode.Both;
        #endregion

        #region Private Fields
        private WrapMode _wrapMode { get; set; }
        #endregion

        #region Initialization
        internal static async Task<GridTextWrapSettings> Initialize(SfBaseComponent baseComponent)
        {
            var gridTextWrapSettings = new GridTextWrapSettings();
            gridTextWrapSettings.Parent = (IGrid)baseComponent;
            gridTextWrapSettings.BaseParent = (IGrid)baseComponent;
            await gridTextWrapSettings.OnInitializedAsync().ConfigureAwait(true);
            return gridTextWrapSettings;
        }
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// Invoked when the component is initialized.
        /// Updates child properties, sets the wrap mode, and refreshes the parent element's class names.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(IGrid.TextWrapSettings), this);
            _wrapMode = WrapMode;

            // Update grid parent element class names
            BaseParent?.GetClass();
        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Ensures the base parameter logic runs and updates the internal wrap mode if it has changed.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            if(!SfBaseUtils.Equals(WrapMode, _wrapMode))
            {
                _wrapMode = WrapMode;
            }
        }
        #endregion
    }
}
