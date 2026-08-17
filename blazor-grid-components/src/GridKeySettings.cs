using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures keyboard navigation for grid.
    /// </summary>
    public partial class GridKeySettings : SfOwningComponentBase
    {

        [CascadingParameter]
        internal IGrid? Parent { get; set; }

        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// Defines the action keys for the left cell movement.
        /// <list type="bullet">
        /// <item>
        /// <term>ShiftTab</term>
        /// <description>Moves to left cell on pressing Shift+Tab key</description>
        /// </item>
        /// <item>
        /// <term>ArrowLeft</term>
        /// <description>Moves to left cell on pressing left arrow key</description>
        /// </item>
        /// <item>
        /// <term>ShiftLeft</term>
        /// <description>Moves to left cell on pressing shift+left arrow key</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public string MoveLeftCell { get; set; } = "ShiftTab,ArrowLeft,ShiftLeft";

        private string _moveLeftCell { get; set; } = "ShiftTab,ArrowLeft,ShiftLeft";

        /// <summary>
        /// Defines the action keys for the right cell movement.
        /// <list type="bullet">
        /// <item>
        /// <term>Tab</term>
        /// <description>Moves to right cell on pressing Tab key</description>
        /// </item>
        /// <item>
        /// <term>ArrowRight</term>
        /// <description>Moves to right cell on pressing right arrow key</description>
        /// </item>
        /// <item>
        /// <term>ShiftRight</term>
        /// <description>Moves to right cell on pressing shift+right arrow key</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public string MoveRightCell { get; set; } = "Tab,ArrowRight,,ShiftRight";

        private string _moveRightCell { get; set; } = "Tab,ArrowRight,ShiftRight";

        /// <summary>
        /// Defines the action keys for the up cell movement.
        /// <list type="bullet">
        /// <item>
        /// <term>ShiftEnter</term>
        /// <description>Moves to top cell on pressing Shit+Enter key</description>
        /// </item>
        /// <item>
        /// <term>ArrowUp</term>
        /// <description>Moves to top cell on pressing up arrow key</description>
        /// </item>
        /// <item>
        /// <term>ShiftUp</term>
        /// <description>Moves to top cell on pressing shift+up arrow key</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public string MoveUpCell { get; set; } = "ShiftEnter,ArrowUp,ShiftUp";

        private string _moveUpCell { get; set; } = "ShiftEnter,ArrowUp,ShiftUp";

        /// <summary>
        /// Defines the action keys for the down cell movement.
        /// <list type="bullet">
        /// <item>
        /// <term>Enter</term>
        /// <description>Moves to down cell on pressing Enter key</description>
        /// </item>
        /// <item>
        /// <term>ArrowDown</term>
        /// <description>Moves to down cell on pressing down arrow key</description>
        /// </item>
        /// <item>
        /// <term>ShiftDown</term>
        /// <description>Moves to down cell on pressing shift+down arrow key</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public string MoveDownCell { get; set; } = "Enter,ArrowDown,ShiftDown";

        private string _moveDownCell { get; set; } = "Enter,ArrowDown,ShiftDown";

        internal Dictionary<string, string> PropKeys
        {
            get
            {
                return new Dictionary<string, string>()
            {
                { nameof(MoveUpCell), MoveUpCell }, { nameof(MoveDownCell), MoveDownCell },
                { nameof(MoveLeftCell), MoveLeftCell }, { nameof(MoveRightCell), MoveRightCell }
            };
            }
        }

        internal static async Task<GridKeySettings> Initialize(SfBaseComponent baseComponent)
        {
            var GridColumnChooserSettings = new GridKeySettings();
            GridColumnChooserSettings.Parent = (IGrid)baseComponent;
            GridColumnChooserSettings.BaseParent = (IGrid)baseComponent;
            await GridColumnChooserSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridColumnChooserSettings;
        }

        /// <summary>
        /// Invoked during component initialization.
        /// Updates the parent grid with the current key navigation settings and initializes internal state fields.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(IGrid.KeySettings), this);
            _moveDownCell = MoveDownCell;
            _moveLeftCell = MoveLeftCell;
            _moveRightCell = MoveRightCell;
            _moveUpCell = MoveUpCell;
        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Updates internal navigation settings for cell movement (up, down, left, right) if any of these values have changed.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            if(!SfBaseUtils.Equals(MoveDownCell, _moveDownCell) || !SfBaseUtils.Equals(MoveUpCell, _moveUpCell)
            || !SfBaseUtils.Equals(MoveLeftCell, _moveLeftCell) || !SfBaseUtils.Equals(MoveRightCell, _moveRightCell))
            {
                _moveDownCell = MoveDownCell;
                _moveUpCell = MoveUpCell;
                _moveLeftCell = MoveLeftCell;
                _moveRightCell = MoveRightCell;
            }
        }

        internal string[] GetAction(string keyCombination)
        {
            List<string> possibleActions = new List<string>();
            foreach (var item in PropKeys)
            {
                if (item.Value.Split(",").Contains(keyCombination))
                {
                    possibleActions.Add(item.Key);
                }
            }

            return possibleActions.ToArray();
        }
    }
}
