using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles pagination operations for the grid.
    /// Responsibility: Manages page state, navigation, index calculations, and pager UI synchronization.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal class Paging<T>
    {
        #region Private Fields
        /// <summary>
        /// Reference to the parent SfGrid component.
        /// </summary>
        private SfGrid<T> _parent { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Paging class.
        /// </summary>
        /// <param name="parent">The parent grid component.</param>
        public Paging(SfGrid<T> parent) => _parent = parent;

        #endregion

        #region Pager UI Synchronization

        /// <summary>
        /// Updates the page sizes in the pager dropdown based on filtered data.
        /// Ensures that the current page size is available in the pager dropdown options.
        /// If not available, updates the pager properties with the total item count.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task UpdatePageSizes()
        {
            if (_parent.AllowPaging && _parent.PagerRef?.PageSizes != null && !_parent.PagerRef.PageSizes.Contains(_parent.PagerRef.PageSize) && _parent.TotalItemCount != 0)
            {
                _parent.PagerRef.UpdatePagerProperties("PageSize", _parent.TotalItemCount);
                await _parent.PagerRef.RefreshAsync().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Updates the pager filter status message based on applied filters.
        /// Displays filter information in the pager's external message area when filter bar status is enabled.
        /// Supports both 'and' and 'or' filter predicates with appropriate symbols.
        /// Handles formatting of date/time columns and manages JsonElement type conversions.
        /// </summary>
        /// <param name="filterColumns">The list of applied filter columns.</param>
        /// <param name="predicate">The filter predicate type: "and" or "or".</param>
        internal async void UpdateFilterMessage(List<GridFilterColumn> filterColumns, string predicate)
        {
            if (_parent.FilterSettings?.Type == FilterType.FilterBar && _parent.FilterSettings.ShowFilterBarStatus && _parent.PagerRef != null)
            {
                _parent.PagerRef.ShowExternalMessage = true;
                _parent.PageSettings!.EnableExternalMessage = _parent.PagerRef.ShowExternalMessage;
                string Symbol = predicate == "or" ? "||" : "&&";
                if (filterColumns?.Count > 0)
                {
                    for (var i = 0; i < filterColumns.Count; i++)
                    {
                        List<GridColumn> GridColumns = GridUtils.GetColumns(_parent);
                        var filteredColumn = GridUtils.GetColumnByFColUidOrField(filterColumns[i].Uid!, GridColumns);
                        if (filterColumns[i].Value is System.Text.Json.JsonElement)
                        {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
                            filterColumns[i].Value = SfBaseUtils.ChangeType(filterColumns[i].Value, filteredColumn?.ValueType);
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
                        }

                        if (i == 0)
                        {
                            _parent.PagerRef.ExternalMessage = filteredColumn?.HeaderText + ": " + (!string.IsNullOrEmpty(filterColumns[i].RawInputValue) ? filterColumns[i].RawInputValue : filteredColumn?.Format != null && (filteredColumn?.Type == ColumnType.Date || filteredColumn?.Type == ColumnType.DateTime 
                                || filteredColumn?.Type == ColumnType.DateOnly || filteredColumn?.Type == ColumnType.TimeOnly) ? UpdateColumnFormat(filterColumns[i]) : filterColumns[i].Value);
                            _parent.PageSettings.ExternalMessage = _parent.PagerRef.ExternalMessage;
                        }
                        else
                        {
                            _parent.PagerRef.ExternalMessage = _parent.PagerRef.ExternalMessage + " " + Symbol + " " + filteredColumn?.HeaderText + ": " + (!string.IsNullOrEmpty(filterColumns[i].RawInputValue) ? filterColumns[i].RawInputValue : filteredColumn?.Format != null && (filteredColumn?.Type == ColumnType.Date || filteredColumn?.Type == ColumnType.DateTime
                                || filteredColumn?.Type == ColumnType.DateOnly || filteredColumn?.Type == ColumnType.TimeOnly) ? UpdateColumnFormat(filterColumns[i]) : filterColumns[i].Value);
                            _parent.PageSettings.ExternalMessage = _parent.PagerRef.ExternalMessage;
                        }
                    }
                }
                else
                {
                    _parent.PagerRef.ExternalMessage = string.Empty;
                }
#if NET10_0_OR_GREATER
                await _parent.PagerRef.RefreshAsync().ConfigureAwait(true);
#endif
            }
        }

        /// <summary>
        /// Helper method to format column values for filter message display.
        /// Handles special formatting for date/time columns based on column format settings.
        /// </summary>
        /// <param name="filteredColumn">The filter column to format.</param>
        /// <returns>The formatted column value as string.</returns>
        private string UpdateColumnFormat(GridFilterColumn filteredColumn)
        {
            var gridColumn = GridUtils.GetColumnByFColUidOrField(filteredColumn.Uid!, _parent.Columns!, _parent.IsStackedHeader);
            if (filteredColumn.Value is DateTimeOffset)
            {
                return ((DateTimeOffset)filteredColumn.Value).ToString(gridColumn?.Format, CultureInfo.CurrentCulture);
            }
            else if (filteredColumn.Value is DateTime)
            {
                return ((DateTime)filteredColumn.Value).ToString(gridColumn?.Format, CultureInfo.CurrentCulture);
            }
            else if (filteredColumn.Value is DateOnly)
            {
                return ((DateOnly)filteredColumn.Value).ToString(gridColumn?.Format, CultureInfo.CurrentCulture);
            }
            else if (filteredColumn.Value is TimeOnly)
            {
                return ((TimeOnly)filteredColumn.Value).ToString(gridColumn?.Format, CultureInfo.CurrentCulture);
            }
            return filteredColumn.Value?.ToString() ?? "";
        }

        /// <summary>
        /// Refreshes the pager UI component to reflect current grid state.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task RefreshPagerAsync()
        {
            if (_parent.PagerRef != null)
            {
                await _parent.PagerRef.RefreshAsync().ConfigureAwait(true);
            }
        }

        #endregion

        #region Page Navigation

        /// <summary>
        /// Navigates to the specified page number with validation.
        /// </summary>
        /// <param name="pageNumber">The page number to navigate to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task GoToPageAsync(int pageNumber)
        {
            if (!_parent.AllowPaging)
            {
                return;
            }

            if (_parent.PagerRef != null)
            {
                _parent.PagerRef.suppressFocus = true;
                int prevNo = _parent.PageSettings!.CurrentPage;
                await _parent.PagerRef.GoToPageAsync(pageNumber).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Navigates to the specified page number with validation.
        /// </summary>
        /// <param name="page">The page to navigate to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task EllipsisButtonClickHandler(string page)
        {
            await (_parent.PagerRef?.EllipsisButtonClickHandler(page))!.ConfigureAwait(true)!;
        }

        #endregion

        #region Index Calculations

        /// <summary>
        /// Calculates the pager index (offset) for the current page.
        /// Formula: (CurrentPage - 1) * PageSize
        /// This represents the zero-based row offset for the current page.
        /// </summary>
        /// <returns>The pager index for data positioning.</returns>
        internal int CalculatePagerIndex()
        {
            return _parent.AllowPaging ? (_parent.PageSettings!.CurrentPage - 1) * _parent.PageSettings.PageSize : 0;
        }

        /// <summary>
        /// Calculates the total number of pages based on total item count and page size.
        /// Formula: Math.Ceiling((double)TotalItemCount / PageSize)
        /// </summary>
        /// <returns>The total number of pages.</returns>
        internal int CalculateTotalPages()
        {
            if (_parent.PageSettings?.PageSize == 0)
                return 1;

            int pageSize = _parent.PageSettings?.PageSize ?? 1;
            return (int)((_parent.TotalItemCount % pageSize == 0) ?
                        (_parent.TotalItemCount / pageSize) : Math.Ceiling((double)_parent.TotalItemCount / pageSize));
        }

        #endregion
    }
}