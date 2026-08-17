using System;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Defines the grid mouse down details.
    /// </summary>
    internal class GridMouseDown
    {
        public GridColumn? Column { get; set; }

        public Row<object>? Row { get; set; }

        public Cell<object>? Cell { get; set; }

        public string? Target { get; set; }
    }
}
