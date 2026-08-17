using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Represents the base class for a grid row in the Syncfusion Blazor Grid.
    /// </summary>
    public partial class GridRowBase<TRow> : SfOwningComponentBase
    {

        /// <summary>
        /// Gets or sets the parameters associated with the grid row.
        /// </summary>
        [Parameter]
        public GridRowParameters? RowParameters { get; set; }

        private Row<object>? _row { get; set; }

        /// <summary>
        /// Gets or sets the parent grid instance for this row.
        /// </summary>
        [CascadingParameter]
        public SfGrid<TRow>? Parent { get; set; }

        /// <summary>
        /// Gets or sets the list of CSS classes applied to the row.
        /// </summary>
        public List<string> ClassList { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of inline styles applied to the row.
        /// </summary>
        public List<string> StyleList { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the combined CSS class names for the row.
        /// </summary>
        public string ClassNames { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the combined inline style text for the row.
        /// </summary>
        public string StyleText { get; set; } = string.Empty;

        internal bool IsEventTriggered { get; set; }

        internal CellDOM? _cellDom { get; set; }

        internal double _rowHeight { get; set; }

        /// <summary>
        /// Gets or sets the additional HTML attributes applied to the row.
        /// </summary>
        public IDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Initializes the grid row and subscribes to row state change events.
        /// </summary>
        protected override void OnInitialized()
            => Parent!.EventAggregator.Add("RowStateChanged", InvokeStateChange);

        private void UpdateRowHeight()
        {
            if (Parent!.RowHeight != _rowHeight)
            {
                StyleList = StyleList ?? FetchStyleList();
                int styleIndex = StyleList.FindIndex(style => style.StartsWith("height", StringComparison.Ordinal));
                if (styleIndex > -1)
                {
                    StyleList.RemoveAt(styleIndex);
                }

                StyleList.AddOrSkip($"height: {Parent.RowHeight}px");
                _rowHeight = Parent.RowHeight;
                StyleText = string.Join(";", StyleList.ToArray());
            }
        }

        /// <summary>
        /// Invoked after the component has rendered. Handles row drag selection completion and triggers related events.
        /// </summary>
        /// <param name="firstRender">Indicates whether this is the first render of the component.</param>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var rowIndex = RowParameters!.Row!.Index;
            var dragStopIndex = Parent!.DragStopIndex;

            if (rowIndex == dragStopIndex && Parent.HasDragSelectionCompleted)
            {
                Parent.HasDragSelectionCompleted = false;
                Parent.DragStopIndex = 0;
                if (Parent.GridEvents?.RowDragSelectionCompleted.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
                {

                    if (Parent.IsRenderedFromTreeGrid)
                    {
                        await Parent.EventAggregator.NotifyAsync("RowDragSelectionCompleted", Parent.DragSelectionEventArgs!).ConfigureAwait(false);
                    }
                    else
                    {
                        if (Parent.GridEvents != null)
                        {
                            await Parent.GridEvents.RowDragSelectionCompleted.InvokeAsync(Parent.DragSelectionEventArgs).ConfigureAwait(false);
                            Parent.DragSelectionEventArgs = null;
                        }
                    }
                }

            }
            if (Parent.IsRenderedFromTreeGrid && RowParameters?.Row is { } row)
            {
                var payload = new Dictionary<string, object>
                {
                    ["Row"] = RowParameters.Row!,
                    ["Attributes"] = Attributes,
                    ["RefreshRow"] = false,
                };
                await Parent.EventAggregator.NotifyAsync("TreeExpandedStateUpdated", payload).ConfigureAwait(true);
                if ((bool)payload["RefreshRow"])
                {
                    Parent.EventAggregator.Trigger("RowStateChanged", payload["Row"]);
                }
            }
            await base.OnAfterRenderAsync(firstRender).ConfigureAwait(false);
            return;
        }

        /// <summary>
        /// Called when component parameters are set or updated.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            UpdateRowHeight();
            if (RowParameters != null && RowParameters.Row != null && (RowParameters.Row != _row || IsEventTriggered || Parent!.SoftRefresh || RowParameters.Row.HasDataChanges))
            {
                if ((RowParameters.Row != _row || RowParameters.Row.HasDataChanges) && IsEventTriggered)
                {
                    IsEventTriggered = false;
                }

                if (IsEventTriggered == false)
                {
                    ClassList = FetchClassNames()!;
                    StyleList = FetchStyleList();
                    Attributes = FetchAttributes();
                    _row = RowParameters.Row;
                    _cellDom = new CellDOM(ClassList!, StyleList, Attributes);
                    if (RowParameters.Row.IsDataRow)
                    {
                        if ((Parent!.GridEvents != null && Parent.GridEvents.RowDataBound.HasDelegate) || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromFileManager)
                        {
                            IsEventTriggered = true;
                            Parent.IsFreezeLineMoved = false;

                            var args = new RowDataBoundEventArgs<TRow>()
                            {
                                Data = (TRow)RowParameters.Row.Data!,
                                Row = _cellDom,
                                Parent = Parent
                            };

                            if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromFileManager)
                                await Parent.EventAggregator.NotifyAsync("RowDataBoundMock", args).ConfigureAwait(true);
                            else
                                await Parent.GridEvents!.RowDataBound.InvokeAsync(args).ConfigureAwait(true);
                        }
                    }
                }

                bool frozenRightColumn = Parent!.Columns!.Any(_ => _.IsFrozen && _.Freeze == FreezeDirection.Right);
                if ((frozenRightColumn) || ((RowParameters.IsFrozen == false || RowParameters.IsFrozen == null) && RowParameters.Row.HasDataChanges && !frozenRightColumn))
                {
                    RowParameters.Row.HasDataChanges = false;
                }

                if (RowParameters.Row.IsAltRow)
                {
                    ClassList.AddOrSkip("e-altrow");
                }
                // Add wrap class in vertical mode when Text wrap is enabled 
                if (Parent.EnableAdaptiveUI && Parent.RowRenderingMode == RowDirection.Vertical && Parent.AllowTextWrap && (Parent!.TextWrapSettings!.WrapMode.Equals(WrapMode.Header)
                    || Parent.TextWrapSettings.WrapMode.Equals(WrapMode.Both)))
                {
                    ClassList.AddOrSkip("e-verticalwrap");
                }
                bool remoteDataPersistSelection = Parent.DataSource == null && Parent.CheckBoxState.Equals(CheckState.Intermediate) && Parent.SelectionModule != null && Parent.SelectionModule.IsHeaderCheckboxChecked && !Parent.SelectionModule.IsDataInDeselectedCollection(RowParameters.Row.Data!);
                if ((RowParameters.Row.IsDataRow && !RowParameters.Row.RowType.Equals("DetailRow", StringComparison.Ordinal))
                    && ((Parent.CheckBoxState.Equals(CheckState.Check) && RowParameters.Row.State != "UnSelected")
                    || (Parent.EnableVirtualization && Parent.CheckBoxState.Equals(CheckState.Check) && RowParameters.Row.State != "UnSelected") && RowParameters.Row.Index != null && Array.IndexOf(Parent.VirtualScrollModule!.SelectRowsMethodIndexes, (int)RowParameters.Row.Index) != -1)
                    || (remoteDataPersistSelection))
                {
                    RowParameters.Row.IsSelected = true;
                }

                if (Parent.IsRenderedFromTreeGrid && ((RowParameters.IsLastRow != RowParameters.Row.IsLastRow) || (Parent.EnablePersistence && Parent.SortModule?.LastSortedCols?.Count > 0 && !RowParameters.IsLastRow && !RowParameters.Row.IsLastRow && Parent.EditModule?.LastVisibleRow?.IsLastRow == true)))
                {
                    await Parent.EventAggregator.NotifyAsync("RowParameter", RowParameters).ConfigureAwait(true);
                }

                if (RowParameters.IsLastRow || (!RowParameters.IsLastRow && RowParameters.Row.IsLastRow && RowParameters.Row.Uid == Parent.EditModule!.LastVisibleRow?.Uid))
                {
                    RowParameters.Row.IsLastRow = true;
                }
                else
                {
                    RowParameters.Row.IsLastRow = false;
                }

                if (RowParameters.Row.IsSelected)
                {
                    Attributes.AddOrUpdateItem("aria-selected", "true");
                }
                else
                {
                    Attributes.Remove("aria-selected");
                }

                if (!Parent.EnableHover)
                {
                    ClassList.AddOrSkip("e-disable-gridhover");
                }
                else
                {
                    if (Parent.IsRenderedFromTreeGrid)
                    {
                        ClassList.Remove("e-disable-gridhover");
                    }
                }

                if (RowParameters.Row.IsDirty)
                {
                    if (RowParameters.Row.Action.Equals(EditAction.Deleted))
                    {
                        RowParameters.Row.CssClass = string.Concat(RowParameters.Row.CssClass, "e-hiddenrow").Trim();
                        ClassList.AddOrSkip("e-hiddenrow e-updatedtd");

                        if (RowParameters.Row.IsLastRow)
                        {
                            RowParameters.Row.IsLastRow = false;
                        }
                    }
                    else
                    {
                        if (RowParameters.Row.Action.Equals(EditAction.Added))
                        {
                            ClassList.AddOrSkip("e-insertedrow");
                        }

                        if (Parent.IsEdit)
                        {
                            ClassList.AddOrSkip("e-editedrow e-batchrow");
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(RowParameters.Row.CssClass) && RowParameters.Row.CssClass.Contains("e-hiddenrow", StringComparison.Ordinal))
                    {
                        RowParameters.Row.CssClass = RowParameters.Row.CssClass
                            .Replace("e-hiddenrow", string.Empty, StringComparison.Ordinal)
                            .Trim();
                    }
                    ClassList.Remove("e-hiddenrow e-updatedtd");
                    ClassList.Remove("e-insertedrow");
                    ClassList.Remove("e-editedrow e-batchrow");
                }
                if(Parent.AllowGrouping && Parent!.GroupSettings!.Columns?.Length > 0)
                {
                    Attributes.AddOrUpdateItem("caption-uid", RowParameters.Row.ParentUid!);
                }
                Attributes.AddOrUpdateItem("data-uid", RowParameters.Row.Uid!);
                Attributes.AddOrUpdateItem("aria-rowindex", RowParameters.Row.Index! + 1);
                if(RowParameters.Row.CssClass != null && RowParameters.Row.CssClass.Contains("e-lazyload-middle-down", StringComparison.Ordinal))
                {
                    ClassList.AddOrSkip("e-lazyload-middle-down");
                }
                if (RowParameters.Row.CssClass != null && RowParameters.Row.CssClass.Contains("e-lazyload-last-down", StringComparison.Ordinal))
                {
                    ClassList.AddOrSkip("e-lazyload-last-down");
                }
                ClassNames = string.Join(" ", ClassList.ToArray());
                StyleText = string.Join(";", StyleList.ToArray());
            }
        }

        /// <summary>
        /// Retrieves a collection of cells within the specified column index range.
        /// </summary>
        public List<Cell<object>> GetCells(int? StartColumnIndex = 0, int? EndColumnIndex = 0)
        {
            List<Cell<object>> Cells = new List<Cell<object>>();
            if (!Parent!.EnableColumnVirtualization)
            {
                if (Parent.AllowFreezeLineMoving)
                {
                    Cells = RowParameters!.Row!.Cells;
                    Parent.FreezeModule!.SetEnableFrozenLineCursorByCells(Cells);
                }
                else
                {
                    Cells = RowParameters!.Row!.Cells;
                }
            }
            else
            {
                if (Parent.EditSettings!.Mode.Equals(EditMode.Normal))
                {
                    StartColumnIndex = StartColumnIndex != Parent.VirtualScrollModule!.StartColumnIndex ? Parent.VirtualScrollModule!.StartColumnIndex : StartColumnIndex;
                    EndColumnIndex = EndColumnIndex != Parent.VirtualScrollModule.EndColumnIndex ? Parent.VirtualScrollModule.EndColumnIndex : EndColumnIndex;
                }
                if (RowParameters!.IsFrozen.HasValue && RowParameters.IsFrozen == true && RowParameters.IsFrozenRight)
                {
                    Cells = RowParameters.Row!.Cells.Where(_ => _.Freeze.Equals(FreezeDirection.Right) && _.IsFrozen && _.Visible).ToList();
                    Parent.FreezeModule!.SetEnableFrozenLineCursorByCells(Cells, "first");
                }
                else if (RowParameters.IsFrozen.HasValue && RowParameters.IsFrozen == true && !RowParameters.IsFrozenRight)
                {
                    Cells = RowParameters.Row!.Cells.Where(_ => _.IsFrozen && _.Visible && _.Freeze.Equals(FreezeDirection.Left)).ToList();
                    Parent.FreezeModule!.SetEnableFrozenLineCursorByCells(Cells, "last");
                }
                else
                {
                    int? startIndex = StartColumnIndex;
                    int bufferCellCount = Parent.AllowRowDragAndDrop && string.IsNullOrEmpty(Parent!.RowDropSettings!.TargetID) ? 2 : 1;
                    if (Parent.AllowGrouping && Parent!.GroupSettings!.Columns?.Length == StartColumnIndex)
                    {
                        startIndex = 0;
                    }
                    if (Parent.AllowGrouping && Parent!.GroupSettings!.Columns?.Length != null && Parent!.GroupSettings!.Columns?.Length != 0 && RowParameters.Row!.Cells.Where(cell => cell.CellType == CellType.Data && cell.Visible).Count() - 1 == EndColumnIndex)
                    {
                        bufferCellCount = bufferCellCount + Parent.GroupSettings.Columns!.Length;
                    }
                    var FrozenLeftCells = RowParameters?.Row?.Cells?.Where(_ => _.IsFrozen && _.Freeze.Equals(FreezeDirection.Left)).ToList();
                    var FrozenRightCells = RowParameters?.Row?.Cells?.Where(_ => _.IsFrozen && _.Freeze.Equals(FreezeDirection.Right)).ToList();
                    Cells = StartColumnIndex != null ? RowParameters!.Row!.Cells.Where(_ => (!_.IsFrozen || _.IsFrozen && _.Freeze.Equals(FreezeDirection.Fixed)) && _.Visible).Skip((int)startIndex!).Take(((int)EndColumnIndex! - (int)StartColumnIndex + bufferCellCount)).ToList() : RowParameters!.Row!.Cells.Where(_ => !_.IsFrozen && _.Visible).ToList();
                    Cells = FrozenLeftCells!.Concat(Cells).Concat(FrozenRightCells!).ToList();
                    Parent.FreezeModule!.SetEnableFrozenLineCursorByCellsExceptFirstAndLast(Cells);
                }
            }

            return Cells;
        }



        /// <summary>
        /// Gets the ARIA role for the current row.
        /// Returns an empty string if the row is added at the top or bottom.
        /// </summary>
        public virtual string GetRole() => (RowParameters!.Row!.IsAddedBottom || RowParameters.Row.IsAddedTop) ? string.Empty : "row";

        /// <summary>
        /// Retrieves a list of CSS class names for the current row based on its type.
        /// </summary>
        public virtual List<string>? FetchClassNames() => GridUtils.RowStaticClasses[RowParameters!.Row!.RowType].Clone<string>();

        /// <summary>
        /// Gets the collection of HTML attributes applied to the current row.
        /// </summary>
        public virtual IDictionary<string, object> FetchAttributes() => Attributes;

        /// <summary>
        /// Builds and returns a list of inline style strings for the current row.
        /// Includes row height if specified in the parent.
        /// </summary>
        public virtual List<string> FetchStyleList()
        {
            var list = new List<string>();
            if (Parent!.RowHeight > 0)
            {
                list.AddOrSkip($"height: {Parent.RowHeight}px");
            }

            return list;
        }

        internal bool IsDummyRowNeeded()
        {
            bool IsDummyRowNeeded = Parent!.EditSettings!.Mode.Equals(EditMode.Batch) && Parent.Rows.Count - Parent.GetBatchChangesAsync().Result.DeletedRecords.Count <= 2;
            return (Parent.TotalItemCount <= 2 || IsDummyRowNeeded) && Parent.EditModule!.ErrorResult.Count > 0 && RowParameters!.Row != null && RowParameters.Row.IsLastRow && (Parent.IsAdd || Parent.IsEdit) && Parent.AllowPaging && Parent!.Columns!.Any(x => x.ValidationRules != null)
                && !Parent.EditSettings.Mode.Equals(EditMode.Dialog) && (Parent.EditModule.IsAdd || Parent.EditModule.IsLastRow || IsDummyRowNeeded && Parent.IsEdit);
        }

        internal bool IsExpanded(object record, bool isFirstParent)
        {
            var parentRecord = Parent?.PropHelper?.GetObject("ParentRecord", record);

            if (parentRecord != null)
            {
                bool parentExpanded = IsExpanded(parentRecord, false);
                if (!parentExpanded)
                {
                    return false;
                }
            }
            else
            {
                if (isFirstParent)
                {
                    return true;
                }
            }

            // Check the expand state of the current record
            bool currentExpanded = (bool)Parent!.PropHelper!.GetObject("Expanded", record);

            // If the current record is collapsed but the parent is expanded, return true
            if (!currentExpanded && isFirstParent)
            {
                return true;
            }

            return currentExpanded;
        }

        /// <summary>
        /// Invokes a state change for the grid row when its state is updated or a dummy row is required.
        /// </summary>
        public virtual void InvokeStateChange(object args)
        {
            Row<object>? row = args as Row<object>;
            if (row != null && RowParameters!.Row != null && row.Uid!.Equals(RowParameters.Row.Uid, StringComparison.Ordinal) || IsDummyRowNeeded())
            {
                OnParametersSetAsync().ConfigureAwait(false);
                InvokeAsync(() => StateHasChanged());
            }
        }

        /// <summary>
        /// Dispose unmanaged resources in the RowParameters.Row component.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose unmanaged resources in the RowParameters.Row component.
        /// </summary>
        /// <param name="disposing">Boolean value to dispose the object.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(true);
            if (disposing)
            {
                Parent?.EventAggregator?.Remove("RowStateChanged", InvokeStateChange);
                propertyHelper.Dispose();
            }
        }
    }
}
