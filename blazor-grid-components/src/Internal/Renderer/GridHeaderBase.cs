using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using System.Linq;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids.Internal
{

    /// <summary>
    /// Represents the base class for the grid header in the Syncfusion Blazor Grid.
    /// Provides access to the parent grid instance.
    /// </summary>
    public class GridHeaderBase<TContent> : ComponentBase
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the parent grid instance associated with this header.
        /// </summary>
        [CascadingParameter]
        public SfGrid<TContent>? Parent { get; set; }

        #endregion

        #region Sort UI Utilities
        internal string GetSortIconOrAriaValue(GridColumn column, string attributeType = "")
        {
            if (Parent!.AllowSorting || (Parent.AllowGrouping && Parent.GroupSettings != null && Parent.GroupSettings.ShowGroupedColumn))
            {
                if (attributeType == "sortIcon")
                {
                    return Parent.SortModule?.GetSortIconClass(column.Field!) ?? string.Empty;
                }
                else if (attributeType == "sortAria")
                {
                    return Parent.SortModule?.GetSortAriaLabel(column.Field!) ?? "none";
                }
            }
            return attributeType == "sortIcon" ? string.Empty : "none";
        }
        #endregion

        #region Header Cell Focus Handling
        internal void HeaderFocused(object args) => HeaderKeyHandler(args).GetAwaiter();

        internal async Task HeaderKeyHandler(object args)
        {
            BeforeCellFocus evt = (args as BeforeCellFocus)!;
            string? keyCom = evt?.KeyCombination;
            if (evt != null && (!evt.IsHeader || !evt.IsKeyEvent))
            {
                return;
            }

            GridColumn? _col = evt?.Cell?.Column;
            switch (keyCom)
            {
                case "Enter":
                case "CtrlEnter":
                case "ShiftEnter":
                    await Parent!.FocusModule!.ShiftEnterHandler(_col!, evt!).ConfigureAwait(true);
                    break;
                case "Space":
                    if (_col != null && _col.Type.Equals(ColumnType.CheckBox))
                    {
                        evt!.Cancel = true;
                        if (Parent != null && Parent.SelectionModule != null)
                        {
                            await Parent.SelectionModule.HeaderClickHandler(null!, Parent.CheckBoxState).ConfigureAwait(true);
                        }
                    }

                    break;
                case "CtrlRight":
                    if (!Parent!.AllowReordering || (_col != null && !_col!.AllowReordering))
                    {
                        break;
                    }

                    int fromIndx = (int)await Parent.GetColumnIndexByUidAsync(_col?.Uid!).ConfigureAwait(true);
                    List<GridColumn> _vCols = await Parent.GetColumnsAsync().ConfigureAwait(true);

                    int? toIndex = null;
                    for (var i = 0; i < _vCols.Count; i++)
                    {
                        if (i > fromIndx && _vCols[i].Visible)
                        {
                            toIndex = i;
                            break;
                        }
                    }

                    await Parent.FocusModule!.ToIndexHasValue(toIndex, fromIndx, evt!).ConfigureAwait(true);
                    break;

                case "CtrlLeft":
                    if (!Parent!.AllowReordering || (_col != null && !_col!.AllowReordering))
                    {
                        break;
                    }

                    int frIndx = (int)await Parent.GetColumnIndexByUidAsync(_col?.Uid!).ConfigureAwait(true);
                    List<GridColumn> _cols = await Parent.GetColumnsAsync().ConfigureAwait(true);

                    int? tIndex = null;
                    for (var i = _cols.Count - 1; i >= 0; i--)
                    {
                        if (i < frIndx && _cols[i].Visible)
                        {
                            tIndex = i;
                            break;
                        }
                    }

                    await Parent.FocusModule!.ToIndexHasValue(tIndex, frIndx, evt!).ConfigureAwait(true);
                    break;

                case "CtrlSpace":
                    await Parent!.FocusModule!.CtrlSpaceHandler(evt!, _col!).ConfigureAwait(true);
                    break;
                case "AltDown":
                    // Open menu, checkbox, excel and column menu
                    break;
            }
        }
        
        #endregion
    }
}
