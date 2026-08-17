using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids.Internal;
using System.Text.Json.Serialization;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid edit settings.
    /// </summary>
    public partial class GridEditSettings : SfOwningComponentBase
    {

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
        /// If AllowAdding is set to true, new records can be added to the Grid.
        /// </summary>
        [Parameter]
        public bool AllowAdding { get; set; }

        private bool _allowAdding { get; set; }

        /// <summary>
        /// If AllowDeleting is set to true, existing record can be deleted from the Grid.
        /// </summary>
        [Parameter]
        public bool AllowDeleting { get; set; }

        private bool _allowDeleting { get; set; }

        /// <summary>
        /// If AllowEditOnDblClick is set to false, Grid will not allow editing of a record on double click.
        /// </summary>
        [Parameter]
        public bool AllowEditOnDblClick { get; set; } = true;

        private bool _allowEditOnDblClick { get; set; }

        /// <summary>
        /// Specifies whether a cell enters edit mode on a single mouse click.
        /// </summary>
        /// <value>
        /// <c>true</c> to enable single-click editing; otherwise, <c>false</c>.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// When enabled, a cell enters edit mode on a single click. Otherwise, double-click is required.
        /// This property is applicable only when <see cref="GridEditSettings.Mode"/> is set to <see cref="EditMode.Batch"/>.
        /// It has no effect in other editing modes.
        /// </remarks>
        [Parameter]
        public bool AllowEditOnSingleClick { get; set; }

        private bool _allowEditOnSingleClick { get; set; }

        /// <summary>
        /// If AllowEditing is set to true, values can be updated in the existing record.
        /// </summary>
        [Parameter]
        public bool AllowEditing { get; set; }

        private bool _allowEditing { get; set; }

        /// <summary>
        /// If allowNextRowEdit is set to true, editing is done to next row. By default allowNextRowEdit is set to false.
        /// </summary>
        [Parameter]
        public bool AllowNextRowEdit { get; set; }

        private bool _allowNextRowEdit { get; set; }

        /// <summary>
        /// Defines the dialog params to edit.
        /// </summary>
        [Parameter]
        public DialogSettings? Dialog { get; set; }

        private DialogSettings? _dialog { get; set; }

        /// <summary>
        /// Defines the custom footer for the edit dialog.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type TValue of the grid.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? FooterTemplate { get; set; }

        /// <summary>
        /// Defines the custom header for the edit dialog.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type TValue of the grid.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? HeaderTemplate { get; set; }

        /// <summary>
        /// Defines the mode to edit. The available editing modes are:
        /// <list type="bullet">
        /// <item>
        /// <term>Normal</term>
        /// <description>Default. Editing is done in an inline form. Edit form is rendered inline as one of the table rows.</description>
        /// </item>
        /// <item>
        /// <term>Dialog</term>
        /// <description>Editing is done in a Dialog/Pop component.</description>
        /// </item>
        /// <item>
        /// <term>Batch</term>
        /// <description>Enables cell editing. Multiple cells can be edited, added or deleted and saved.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public EditMode Mode { get; set; } = EditMode.Normal;

        private EditMode _mode { get; set; }

        /// <summary>
        /// Defines the position of adding a new row. The available position are:
        /// <list type="bullet">
        /// <item>
        /// <term>Top</term>
        /// <description>Default. Add form is placed at the first row of the grid.</description>
        /// </item>
        /// <item>
        /// <term>Bottom</term>
        /// <description>Add form is placed at the last row of the grid</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public NewRowPosition NewRowPosition { get; set; } = NewRowPosition.Top;

        private NewRowPosition _newRowPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display a new blank row during grid initialization, facilitating the addition of new records.
        /// </summary>
        /// <value>
        /// <c>true</c>, a new blank row is displayed on the grid content. The default value is <c>false</c>.
        /// </value>
        /// <remarks>    
        /// By default, the new blank row is displayed at the top of the grid content. A new blank row can be displayed either at the top or bottom of the corresponding page, depending on the setting of the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.NewRowPosition"/> property. 
        /// However, it's important to note that the <c>ShowAddNewRow</c> property solely pertains to the display of a new blank row in the <see cref="EditMode.Normal"/> editing mode. 
        /// If the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.AllowAdding"/> property is set to false, the new blank row will be disabled.
        /// Additionally, if any of the grid column's <see cref="Syncfusion.Blazor.Grids.GridColumn.AllowAdding"/> properties is set to false, the corresponding column cell will also be disabled.
        /// </remarks>
        [Parameter]
        public bool ShowAddNewRow { get; set; }

        /// <summary>
        /// Enables Undo/Redo functionality for batch editing operations.
        /// Only works in EditMode.Batch. Default: false (opt-in).
        /// </summary>
        [Parameter]
        public bool EnableUndoRedo { get; set; } = false;

        private bool _enableUndoRedoPrevious { get; set; }

        /// <summary>
        /// Maximum number of undo/redo steps to maintain in memory.
        /// When exceeded, oldest actions are discarded. Default: 20.
        /// </summary>
        [Parameter]
        public int UndoRedoLimit { get; set; } = 20;

        private int _undoRedoLimitPrevious { get; set; }
        
        /// <summary>
        /// If ShowConfirmDialog is set to false, confirm dialog does not show when batch changes are saved or discarded.
        /// </summary>
        [Parameter]
        public bool ShowConfirmDialog { get; set; } = true;

        private bool _showConfirmDialog { get; set; }

        /// <summary>
        /// If ShowDeleteConfirmDialog is set to true, confirm dialog will show delete action. You can also cancel delete command.
        /// </summary>
        [Parameter]
        public bool ShowDeleteConfirmDialog { get; set; }

        private bool _showDeleteConfirmDialog { get; set; }

        /// <summary>
        /// Defines the custom content and edit elements for the edit dialog.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type TValue of the grid.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? Template { get; set; }

        /// <summary>
        /// Defines the custom validator component for the built-in EditForm. Use this to override built-in
        /// validation components.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.ValidatorTemplateContext"/>.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? Validator { get; set; }

        internal static async Task<GridEditSettings> Initialize(SfBaseComponent baseComponent)
        {
            var GridEditSettings = new GridEditSettings();
            GridEditSettings.Parent = (IGrid)baseComponent;
            GridEditSettings.BaseParent = (IGrid)baseComponent;

            // GridEditSettings.IsAutoInitialized = true;
            await GridEditSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridEditSettings;
        }

        /// <summary>
        /// Initializes the GridEditSettings, applies edit configuration, and updates the parent grid.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(IGrid.EditSettings), this);
            _allowAdding = AllowAdding;
            _allowDeleting = AllowDeleting;
            _allowEditOnDblClick = AllowEditOnDblClick;
            _allowEditOnSingleClick = AllowEditOnSingleClick;
            _allowEditing = AllowEditing;
            _allowNextRowEdit = AllowNextRowEdit;
            _dialog = Dialog;
            _mode = Mode;
            _newRowPosition = NewRowPosition;
            _showConfirmDialog = ShowConfirmDialog;
            _showDeleteConfirmDialog = ShowDeleteConfirmDialog;
            _enableUndoRedoPrevious = EnableUndoRedo;
            _undoRedoLimitPrevious = UndoRedoLimit;

            // Initialize UndoRedoManager on first initialization if EnableUndoRedo is true
            if (EnableUndoRedo && Mode == EditMode.Batch)
            {
                dynamic parentDynamic = Parent;
                if (parentDynamic?.UndoRedoManager != null)
                {
                    parentDynamic.UndoRedoManager.Enable(UndoRedoLimit);
                }
            }
        }

        /// <summary>
        /// Handles parameter updates for GridEditSettings and synchronizes internal state when values change.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            if(!SfBaseUtils.Equals(AllowAdding, _allowAdding) || !SfBaseUtils.Equals(AllowDeleting, _allowDeleting)
            || !SfBaseUtils.Equals(AllowEditOnDblClick, _allowEditOnDblClick) || !SfBaseUtils.Equals(AllowEditOnSingleClick, _allowEditOnSingleClick) || !SfBaseUtils.Equals(AllowEditing, _allowEditing)
            || !SfBaseUtils.Equals(AllowNextRowEdit, _allowNextRowEdit) || !SfBaseUtils.Equals(Dialog, _dialog)
            || !SfBaseUtils.Equals(Mode, _mode) || !SfBaseUtils.Equals(NewRowPosition, _newRowPosition)
            || !SfBaseUtils.Equals(ShowConfirmDialog, _showConfirmDialog) || !SfBaseUtils.Equals(ShowDeleteConfirmDialog, _showDeleteConfirmDialog)
            || !SfBaseUtils.Equals(EnableUndoRedo, _enableUndoRedoPrevious) || !SfBaseUtils.Equals(UndoRedoLimit, _undoRedoLimitPrevious))
            {
                _allowAdding = AllowAdding;
                _allowDeleting = AllowDeleting;
                _allowEditOnDblClick = AllowEditOnDblClick;
                _allowEditOnSingleClick = AllowEditOnSingleClick;
                _allowEditing = AllowEditing;
                _allowNextRowEdit = AllowNextRowEdit;
                _dialog = Dialog;
                _mode = Mode;
                _newRowPosition = NewRowPosition;
                _showConfirmDialog = ShowConfirmDialog;
                _showDeleteConfirmDialog = ShowDeleteConfirmDialog;

                // Handle UndoRedo enable/disable when EnableUndoRedo or UndoRedoLimit settings change
                if (EnableUndoRedo != _enableUndoRedoPrevious ||
                    UndoRedoLimit != _undoRedoLimitPrevious)
                {
                    // Access UndoRedoManager safely using reflection or dynamic binding
                    try
                    {
                        dynamic parentDynamic = Parent;
                        if (parentDynamic?.UndoRedoManager != null)
                        {
                            // Enable undo/redo if EnableUndoRedo is true AND Mode is Batch
                            if (EnableUndoRedo && Mode == EditMode.Batch)
                            {
                                parentDynamic.UndoRedoManager.Enable(UndoRedoLimit);
                            }
                            else
                            {
                                parentDynamic.UndoRedoManager.Disable();
                            }
                        }
                        
                        // Trigger toolbar state refresh to reflect the EnableUndoRedo change in UI
                        if (parentDynamic?.EventAggregator != null)
                        {
                            parentDynamic.EventAggregator.Trigger("ToolbarStateChanged", null!);
                        }
                    }
                    catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                    {
                        // UndoRedoManager not available on this parent type - silently ignore
                    }
                    _enableUndoRedoPrevious = EnableUndoRedo;
                    _undoRedoLimitPrevious = UndoRedoLimit;
                }
            }
            // Telemetry for editing modes
            GridTelemetryHelper.LogTelemetry(true, "Editing");
        }
    }
}