using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids.Internal;
using Syncfusion.Blazor.Internal;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid sort column.
    /// </summary>
    public partial class GridSortColumn : SfOwningComponentBase
    {
        #region Cascading Context
        [CascadingParameter]
        internal GridSortColumns? Parent { get; set; }

        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }
        #endregion

        #region Sorting Parameters
        /// <summary>
        /// Defines the direction of sort column. Available directions are,.
        /// <list type="bullet">
        /// <item>
        /// <term>Ascending</term>
        /// <description>Default. Sorts records in ascending order.</description>
        /// </item>
        /// <item>
        /// <term>Descending</term>
        /// <description>Sorts records in descending order.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public SortDirection Direction { get; set; }

        /// <summary>
        /// Defines the field name of sort column.
        /// </summary>
        [Parameter]
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Defines the sorted column whether or from grouping operation.
        /// </summary>
        [Parameter]
        public bool IsFromGroup { get; set; }
        #endregion

        #region Internal State
        private SortDirection _direction { get; set; }

        private string? _field { get; set; }

        private bool _isFromGroup { get; set; }
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// Invoked during component initialization.
        /// Updates the parent grid with the current sort settings, initializes internal state fields,
        /// and triggers a UI refresh if a base parent exists.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperty(this);
            _direction = Direction;
            _field = Field;
            _isFromGroup = IsFromGroup;
            if (BaseParent != null)
            {
                BaseParent.HasSortColumnChanges = false;
            }

        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Updates internal state for direction, field, and group flag if any of these values have changed.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            if(!SfBaseUtils.Equals(Direction, _direction) || !SfBaseUtils.Equals(Field, _field)
            || !SfBaseUtils.Equals(IsFromGroup, _isFromGroup))
            {
                _direction = Direction;
                _field = Field;
                _isFromGroup = IsFromGroup;
            }
            // Telemetry for Sorting
            GridTelemetryHelper.LogTelemetry(true, "Sorting");
        }
        #endregion
    }
}
