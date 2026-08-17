using Syncfusion.Blazor.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles Reactive Aggregation in Grid.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal partial class ReactiveAggregate<T>
    {
        #region Private Fields
        private SfGrid<T> Parent { get; set; }
        #endregion

        #region Internal Properties
        internal Dictionary<string, List<Cell<object>>> OriginalCells = new Dictionary<string, List<Cell<object>>>();
        #endregion

        #region Constructor
        public ReactiveAggregate(SfGrid<T> parent)
        {
            Parent = parent;
        }
        #endregion
    }
}