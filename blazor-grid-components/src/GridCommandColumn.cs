using System.ComponentModel;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid command column.
    /// </summary>
    public partial class GridCommandColumn : SfOwningComponentBase
    {

        [CascadingParameter]
        internal GridCommandColumns? Parent { get; set; }

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
        /// Define the button model.
        /// </summary>
        [Parameter]
        public CommandButtonOptions ButtonOption { get; set; } = new CommandButtonOptions();

        private CommandButtonOptions? _buttonOption { get; set; }

        /// <summary>
        /// Define the command button tooltip.
        /// </summary>
        [Parameter]
        public string? Title { get; set; }

        private string? _title { get; set; }

        /// <summary>
        /// Define the command button ID.
        /// </summary>
        [Parameter]
        public string? ID { get; set; }

        private string? _ID { get; set; }

        /// <summary>
        /// Define the command button type.
        /// <list type="bullet">
        /// <item>
        /// <term>None</term>
        /// <description>Default. A command button with no default action. Use this for custom command actions.</description>
        /// </item>
        /// <item>
        /// <term>Edit</term>
        /// <description>A edit command button that edit current record.</description>
        /// </item>
        /// <item>
        /// <term>Delete</term>
        /// <description>A delete command button that delete current record.</description>
        /// </item>
        /// <item>
        /// <term>Save</term>
        /// <description>A save command button that saves the current edited record.</description>
        /// </item>
        /// <item>
        /// <term>Cancel</term>
        /// <description>A cancel command button that cancels the edit state.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// To use custom command button, set Type as <see cref="Syncfusion.Blazor.Grids.CommandButtonType.None"/> and use
        /// <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CommandClicked"/> event to perform custom action.
        /// </remarks>
        [Parameter]
        public CommandButtonType Type { get; set; } = CommandButtonType.None;

        private CommandButtonType _type { get; set; }

        internal static int sequence { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier for the command column instance.
        /// </summary>
        [Parameter]
        public string? Uid { get; set; }

        /// <summary>
        /// Initializes the grid command column and registers it with the parent grid.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperty(this);
            _buttonOption = ButtonOption;
            _title = Title;
            _ID = ID;
            _type = Type;
            if(BaseParent != null)
            {
                BaseParent.HasColumnChanges = true;
                await BaseParent.CallStateHasChangedAsync().ConfigureAwait(true);
            }
           

            Uid = GetCommandUid("gridcommand");
        }

        internal static string GetCommandUid(string prefix)
        {
            return $"{prefix}{sequence++}";
        }

        /// <summary>
        /// Updates cached button properties when parameters change.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            if(!SfBaseUtils.Equals(ButtonOption, _buttonOption) || !SfBaseUtils.Equals(Title, _title)
            || !SfBaseUtils.Equals(ID, _ID) || !SfBaseUtils.Equals(Type, _type))
            {
                _buttonOption = ButtonOption;
                _title = Title;
                _ID = ID;
                _type = Type;
            }
        }
    }
}