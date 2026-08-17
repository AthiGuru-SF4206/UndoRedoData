using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Syncfusion.Blazor.Internal;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid selection.
    /// </summary>
    public partial class GridSelectionSettings : SfOwningComponentBase
    {

        [CascadingParameter]
        internal IGrid? Parent { get; set; }

        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// The cell selection modes are flow and box.
        /// It requires the <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.Mode"/> to be either cell or both.
        /// <list type="bullet">
        /// <item>
        /// <term>Flow</term>
        /// <description>Default. Selects the range of cells between start index and end index that also includes the other cells of the selected rows..</description>
        /// </item>
        /// <item>
        /// <term>Box</term>
        /// <description>Selects the range of cells within the start and end column indexes that includes in between cells of rows within the range</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public CellSelectionMode CellSelectionMode { get; set; } = CellSelectionMode.Flow;

        private CellSelectionMode _cellSelectionMode { get; set; }

        /// <summary>
        /// Defines options for checkbox selection Mode. They are,.
        /// <list type="bullet">
        /// <item>
        /// <term>Default</term>
        /// <description>Default. In this mode, user can select multiple rows by clicking rows one by one.</description>
        /// </item>
        /// <item>
        /// <term>ResetOnRowClick</term>
        /// <description>In ResetOnRowClick mode, on clicking a row it will reset previously selected row and also multiple
        ///  rows can be selected by using CTRL or SHIFT key.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public CheckboxSelectionType CheckboxMode { get; set; } = CheckboxSelectionType.Default;

        private CheckboxSelectionType _checkboxMode { get; set; }

        /// <summary>
        /// If CheckboxOnly set to true, then the Grid selection is allowed only through checkbox.
        /// </summary>
        [Parameter]
        public bool CheckboxOnly { get; set; }

        private bool _checkboxOnly { get; set; }

        /// <summary>
        /// If EnableSimpleMultiRowSelection set to true, then the user can able to perform multiple row selection with single clicks.
        /// </summary>
        [Parameter]
        public bool EnableSimpleMultiRowSelection { get; set; }

        private bool _enableSimpleMultiRowSelection { get; set; }

        /// <summary>
        /// If EnableToggle set to true, then the user can able to perform toggle for the selected row.
        /// </summary>
        [Parameter]
        public bool EnableToggle { get; set; } = true;

        private bool _enableToggle { get; set; }

        /// <summary>
        /// Specifies the selection mode. Available modes are,.
        /// <list type="bullet">
        /// <item>
        /// <term>Row</term>
        /// <description>Default. Row selection is enabled</description>
        /// </item>
        /// <item>
        /// <term>Cell</term>
        /// <description>Cell selection is enabled.</description>
        /// </item>
        /// <item>
        /// <term>Both</term>
        /// <description>Both Row and Cell selection is enabled.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public SelectionMode Mode { get; set; } = SelectionMode.Row;

        private SelectionMode _mode { get; set; }

        /// <summary>
        /// If PersistSelection set to true, then the Grid selection is persisted on all operations.
        /// For persisting selection in the Grid, any one of the column must be enabled as a primary key.
        /// </summary>
        [Parameter]
        public bool PersistSelection { get; set; }

        private bool _persistSelection { get; set; }

        /// <summary>
        /// Defines options for selection type. They are.
        /// <list type="bullet">
        /// <item>
        /// <term>Single</term>
        /// <description>Default. Allows selection of only a row or a cell.</description>
        /// </item>
        /// <item>
        /// <term>Multiple</term>
        /// <description>Allows user to select a multiple rows or cells.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public SelectionType Type { get; set; } = SelectionType.Single;

        private SelectionType _type { get; set; }

        /// <summary>
        /// Gets or sets whether to enable selection of multiple rows and cells by dragging mouse.
        /// </summary>
        /// <value>
        /// <c>true</c>, if the row and cell selection enabled when dragging mouse. Otherwise, false.
        /// </value>
        /// <remarks>
        /// To perform drag selection in blazor grid, set <see cref="GridSelectionSettings.Type"/> as should be <c>Multiple</c>.
        /// </remarks>
        [Parameter]
        [DefaultValue(false)]
        [JsonPropertyName("allowDragSelection")]
        public bool AllowDragSelection { get; set; }

        private bool _allowDragSelection { get; set; }

        internal static async Task<GridSelectionSettings> Initialize(SfBaseComponent baseComponent)
        {
            var GridSelectionSettings = new GridSelectionSettings();
            GridSelectionSettings.Parent = (IGrid)baseComponent;
            GridSelectionSettings.BaseParent = (IGrid)baseComponent;
            await GridSelectionSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridSelectionSettings;
        }

        /// <summary>
        /// Invoked during component initialization.
        /// Updates the parent grid with the current selection settings and initializes internal state fields.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(IGrid.SelectionSettings), this);
            _cellSelectionMode = CellSelectionMode;
            _checkboxMode = CheckboxMode;
            _checkboxOnly = CheckboxOnly;
            _enableSimpleMultiRowSelection = EnableSimpleMultiRowSelection;
            _enableToggle = EnableToggle;
            _mode = Mode;
            _persistSelection = PersistSelection;
            _type = Type;
            _allowDragSelection = AllowDragSelection;
        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Detects changes in the selection mode and notifies the parent component to handle the update.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            if (!SfBaseUtils.Equals(Mode, _mode)) 
            { 
                var sfBaseComponent = (SfBaseComponent?)BaseParent;
                switch (_mode) 
                { 
                    case SelectionMode.Cell: sfBaseComponent?.PropertyChanges.Add("CellSelectionModeChanged", null!);
                        break; 
                    case SelectionMode.Row: sfBaseComponent?.PropertyChanges.Add("RowSelectionModeChanged", null!); 
                        break;
                    case SelectionMode.Both: sfBaseComponent?.PropertyChanges.Add("BothSelectionModeChanged", null!);
                        break; 
                }
                if(BaseParent != null)
                {
                    await BaseParent.PropertyChanged().ConfigureAwait(true);
                }
            }

            if(!SfBaseUtils.Equals(CellSelectionMode, _cellSelectionMode) || !SfBaseUtils.Equals(CheckboxMode, _checkboxMode)
            || !SfBaseUtils.Equals(CheckboxOnly, _checkboxOnly) || !SfBaseUtils.Equals(EnableSimpleMultiRowSelection, _enableSimpleMultiRowSelection)
            || !SfBaseUtils.Equals(EnableToggle, _enableToggle)
            || !SfBaseUtils.Equals(Mode, _mode) || !SfBaseUtils.Equals(PersistSelection, _persistSelection)
            || !SfBaseUtils.Equals(Type, _type) || !SfBaseUtils.Equals(AllowDragSelection, _allowDragSelection))
            {
                _cellSelectionMode = CellSelectionMode;
                _checkboxMode = CheckboxMode;
                _checkboxOnly = CheckboxOnly;
                _enableSimpleMultiRowSelection = EnableSimpleMultiRowSelection;
                _enableToggle = EnableToggle;
                _mode = Mode;
                _persistSelection = PersistSelection;
                _type = Type;
                _allowDragSelection = AllowDragSelection;
            }
            // Telemetry for selection
            GridTelemetryHelper.LogTelemetry(true, "Selection");
        }
    }
}
