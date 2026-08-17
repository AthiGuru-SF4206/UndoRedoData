using System;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid events.
    /// </summary>
    public partial class GridEvents<TValue> : SfOwningComponentBase
    {

        /// <summary>
        /// Gets or sets the parent grid instance to which the event handlers are associated.
        /// </summary>
        [CascadingParameter]
        protected SfGrid<TValue>? Parent { get; set; }


        /// <summary>
        /// Gets or sets the event callback that is raised before paging action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a <see cref="Syncfusion.Blazor.Grids.GridPageChangingEventArgs"/> object, which provides details about the before paging action in the grid.
        ///</remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the PageChanging event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" PageChanging="PageChangingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code {
        /// public async Task PageChangingHandler (GridPageChangingEventArgs args)
        /// {
        ///      args.CurrentPage = 2; // Sets the current page number.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<GridPageChangingEventArgs> PageChanging { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after paging action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.GridPageChangedEventArgs"/> object, which provides details about the after paging action in the grid.
        ///</remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the PageChanged event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" PageChanged="PageChangedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task PageChangedHandler (GridPageChangedEventArgs args)
        /// {
        ///      int pagenumber = args.CurrentPage; // Gets the current page number.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<GridPageChangedEventArgs> PageChanged { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is invoked before a sorting action is performed or a column is removed from sorting in the grid or when the sort column direction changes from Ascending to Descending or vice versa for the same column.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.SortingEventArgs"/> object, which provides details about the before sorting action or a column is removed from sorting in the grid or when the sort column direction changes from <c>Ascending</c> to <c>Descending</c> or vice versa for the same column.
        ///</remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the Sorting event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" Sorting="SortingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task SortingHandler(SortingEventArgs args)
        /// {
        ///     args.Cancel = true; // To cancel the sorting action.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<SortingEventArgs> Sorting { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after sorting action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a <see cref="Syncfusion.Blazor.Grids.SortedEventArgs"/> object, which provides details about the after sorting action in the grid.
        ///</remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the Sorted event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" Sorted="SortedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task SortedHandler(SortedEventArgs args)
        /// {
        ///    var direction = args.Direction; // Gets the current sorting direction.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<SortedEventArgs> Sorted { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after grouping action or un-grouping action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.GroupingEventArgs"/> object, which provides details about the before grouping action or un-grouping action in the grid.
        ///</remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the Grouping event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" Grouping="GroupingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task GroupingHandler(GroupingEventArgs args)
        /// {
        ///     args.Cancel = true; // To cancel the grouping action.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<GroupingEventArgs> Grouping { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after grouping or ungrouping action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.GroupedEventArgs"/> object, which provides details about the after grouping or ungrouping action in the grid.
        ///</remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the Grouped event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" Grouped="GroupedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task GroupedHandler(GroupedEventArgs args)
        /// {
        ///      var groupedColumns = args.ColumnName; // Gets the grouped columns.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<GroupedEventArgs> Grouped { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised before the search action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.SearchingEventArgs"/> object, 
        /// which provides details about the search action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the Searching event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" Searching ="SearchingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code {
        /// public async Task SearchingHandler (SearchingEventArgs args)
        /// {
        ///     args.Cancel = true; // To cancel the search begin action.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<SearchingEventArgs> Searching { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after the search action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.SearchedEventArgs"/> object, 
        /// which provides details about the after-search action in the grid.
        ///</remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the Searched event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" Searched="SearchedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task SearchedHandler (SearchedEventArgs args)
        /// {
        ///   var searchResult = args.SearchString; // Gets the search result.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<SearchedEventArgs> Searched { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised before the add action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.RowCreatingEventArgs{TValue}"/> object, 
        /// which provides details about the before add action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the RowCreating event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" RowCreating ="RowAddingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowAddingHandler (RowCreatingEventArgs<Order> args)
        /// {
        ///    args.Cancel = true; // To cancel the add action.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowCreatingEventArgs<TValue>> RowCreating { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after the add action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.RowCreatedEventArgs{TValue}"/> object, 
        /// which provides details about the after add action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the RowCreated event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" RowCreated ="RowCreatedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowCreatedHandler (RowCreatedEventArgs<Order> args)
        /// {
        ///    var addedRecord = args.Data; // Gets the added record.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowCreatedEventArgs<TValue>> RowCreated { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised before the save action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.RowUpdatingEventArgs{TValue}"/> object, 
        /// which provides details about the before save action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the RowUpdating event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" RowUpdating ="RowUpdatingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowUpdatingHandler (RowUpdatingEventArgs<Order> args)
        /// {
        ///    args.Cancel = true; // To cancel the save action.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowUpdatingEventArgs<TValue>> RowUpdating { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after the save action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.RowUpdatedEventArgs{TValue}"/> object, 
        /// which provides details about the after save action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the RowUpdated event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" RowUpdated ="RowUpdatedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowUpdatedHandler (RowUpdatedEventArgs<Order> args)
        /// {
        ///    var rowIndex = args.Index; // Gets the row index of the saved record.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowUpdatedEventArgs<TValue>> RowUpdated { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised before the delete action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.RowDeletingEventArgs{TValue}"/> object, 
        /// which provides details about the before delete action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the RowDeleting event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" RowDeleting ="RowDeletingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowDeletingHandler(RowDeletingEventArgs<Order> args)
        /// {
        ///    if(args.Data.OrderID == 10248) // To cancel the delete action for a specific record.
        ///    { 
        ///       args.Cancel = true; 
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowDeletingEventArgs<TValue>> RowDeleting { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after the delete action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.RowDeletedEventArgs{TValue}"/> object, 
        /// which provides details about the after delete action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the RowDeleted event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" RowDeleted ="RowDeletedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowDeletedHandler(RowDeletedEventArgs<Order> args)
        /// {
        ///    var rowIndex = args.RowIndex; // Gets the row index of the deleted record.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowDeletedEventArgs<TValue>> RowDeleted { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is invoked before the cancel action is performed in the grid, specifically when using <see cref="Syncfusion.Blazor.Grids.EditMode.Normal"/> and <see cref="Syncfusion.Blazor.Grids.EditMode.Dialog"/> edit modes.
        /// </summary>        
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.EditCancelingEventArgs{TValue}"/> object, 
        /// which provides details about the before cancel action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the EditCanceling event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" EditCanceling ="CancelingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task CancelingHandler(EditCancelingEventArgs<Order> args)
        /// {
        ///    if(args.PreviousData.OrderID == 10248) 
        ///    {
        ///       args.Cancel = true; // To cancel the cancel action for a specific record.
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<EditCancelingEventArgs<TValue>> EditCanceling { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is invoked after the cancel action is performed in the grid, specifically when using <see cref="Syncfusion.Blazor.Grids.EditMode.Normal"/> and <see cref="Syncfusion.Blazor.Grids.EditMode.Dialog"/> edit modes.
        /// </summary>  
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.EditCanceledEventArgs{TValue}"/> object, 
        /// which provides details about the after cancel action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the EditCanceled event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" EditCanceled ="CanceledHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task CanceledHandler(EditCanceledEventArgs<Order> args)
        /// {
        ///    var data = args.Data; // Gets the data of the Canceled record.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<EditCanceledEventArgs<TValue>> EditCanceled { get; set; }


        /// <summary>
        /// Gets or sets the event callback that is raised before the edit action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a <see cref="Syncfusion.Blazor.Grids.RowEditingEventArgs{TValue}"/> object, which provides details about 
        /// the before edit action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the RowEditing event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" RowEditing ="RowEditingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowEditingHandler(RowEditingEventArgs<<Order> args)
        /// {
        ///    args.Cancel = true ; // To cancel the editing action
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowEditingEventArgs<TValue>> RowEditing { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after the edit action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a <see cref="Syncfusion.Blazor.Grids.RowEditedEventArgs{TValue}"/> object, which provides details about 
        /// the after edit action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the RowEdited event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" RowEdited ="RowEditedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowEditedHandler(RowEditedEventArgs<Order> args)
        /// {
        ///    var editedData = args.Data; // Gets the data of the edited record.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowEditedEventArgs<TValue>> RowEdited { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised before an editing action is performed in the grid.
        /// This event is primarily used to enable or disable the <c>PreventDataClone</c> argument, which controls
        /// whether the <c>Data</c> argument belonging to the <see cref="RowEditing"/> event will be cloned or not.
        /// </summary>        /// <remarks>
        /// The event handler receives a <see cref="Syncfusion.Blazor.Grids. OnRowEditStartEventArgs"/> object, which provides details about the 
        /// before the edit action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the OnRowEditStart event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" OnRowEditStart ="BeforeRowEditingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task BeforeRowEditingHandler(OnRowEditStartEventArgs<Order> args)
        /// {
        ///    args.PreventDataClone = true; // To prevent the data from being cloned.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<OnRowEditStartEventArgs> OnRowEditStart { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised before the filtering or clear filtering action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a <see cref="Syncfusion.Blazor.Grids.FilteringEventArgs"/> object,
        /// which contains details about filtering or clearing the filtering action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the Filtering event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" Filtering="FilteringHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task FilteringHandler(FilteringEventArgs args)
        /// {
        ///    
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<FilteringEventArgs> Filtering { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after the filtered or clear filtered action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a <see cref="Syncfusion.Blazor.Grids.FilteredEventArgs"/> object,
        /// which contains details about filtering or clearing the filtering action in the grid.        
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the Filtered event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" Filtered ="FilteredHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task FilteredHandler(FilteredEventArgs args)
        /// {
        ///    
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<FilteredEventArgs> Filtered { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised before the filter dialog is opened in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.FilterDialogOpeningEventArgs"/> object, 
        /// which provides details about the filter dialog opening in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the FilterDialogOpening event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" FilterDialogOpening ="FilterDialogOpeningHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task FilterDialogOpeningHandler(FilterDialogOpeningEventArgs args)
        /// {
        ///    args.Cancel = true; // To cancel the filter dialog opening action.
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<FilterDialogOpeningEventArgs> FilterDialogOpening { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised after the filter dialog is opened in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.FilterDialogOpenedEventArgs"/> object, 
        /// which provides details about the filter dialog opened in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the FilterDialogOpened event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" FilterDialogOpened="FilterDialogOpenedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task FilterDialogOpenedHandler(FilterDialogOpenedEventArgs args)
        /// {
        ///    
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<FilterDialogOpenedEventArgs> FilterDialogOpened { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised when values get filtered using search bar in <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/> and <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/> filter.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.CheckboxFilterSearchingEventArgs"/> object, 
        /// which provides details about the values get filtered using search bar in checkbox filter and excel filter in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the CheckboxFilterSearching event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" CheckboxFilterSearching="CheckboxFilterSearchHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task CheckboxFilterSearchingHandler(CheckboxFilterSearchEventArgs args)
        /// {
        ///    
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<CheckboxFilterSearchingEventArgs> CheckboxFilterSearching { get; set; }


        /// <summary>
        /// Gets or sets the event callback that is raised when columns reordering action is performed in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.ColumnReorderingEventArgs"/> object, 
        /// which provides details about the columns reordering action in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the ColumnReordering event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" ColumnReordering ="ColumnReorderingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task ColumnReorderingHandler(ColumnReorderingEventArgs args)
        /// {
        ///    var fromColumn = args.FromColumn; // To get the from columns list
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ColumnReorderingEventArgs> ColumnReordering { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised when columns are reordered in the grid.
        /// </summary>
        /// <remarks>
        /// The event handler receives a  <see cref="Syncfusion.Blazor.Grids.ColumnReorderedEventArgs"/> object, 
        /// which provides details about the columns reordered in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the ColumnReordered event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" ColumnReordered ="ColumnReorderedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task ColumnReorderedHandler(ColumnReorderedEventArgs args)
        /// {
        ///    var toColumn = args.ToColumn; // To get the to columns list
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ColumnReorderedEventArgs> ColumnReordered { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised when the grid's column visibility is changing.
        /// </summary>
        /// <remarks>
        /// The event handler receives a <see cref="Syncfusion.Blazor.Grids.ColumnVisibilityChangingEventArgs"/> object, 
        /// which provides details about the columns and the action performed (show or hide) in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the ColumnVisibilityChanging event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" ColumnVisibilityChanging ="ColumnVisibilityChangingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task ColumnVisibilityChangingHandler(ColumnVisibilityChangingEventArgs args)
        /// {
        ///    var vivibleColumns = args.VisibleColumns; // To get the visible columns list
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ColumnVisibilityChangingEventArgs> ColumnVisibilityChanging { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised when the grid's column visibility is changed.
        /// </summary>
        /// <remarks>
        /// The event handler receives a <see cref="Syncfusion.Blazor.Grids.ColumnVisibilityChangedEventArgs"/> object, 
        /// which provides details about the columns and the action performed (show or hide) in the grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// This example shows how to handle the ColumnVisibilityChanged event:
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" ColumnVisibilityChanged ="ColumnVisibilityChangedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task ColumnVisibilityChangedHandler(ColumnVisibilityChangedEventArgs args)
        /// {
        ///    
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ColumnVisibilityChangedEventArgs> ColumnVisibilityChanged { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when the grid actions such as sorting, paging, grouping, ungrouping, reorder, rowdraganddrop, filtering, add, edit, delete, save and cancel action begins.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.ActionEventArgs{TValue}"/> object which provides the details of the current grid action.
        /// You can differentiate the actions using <see cref="Syncfusion.Blazor.Grids.ActionEventArgs{TValue}.RequestType"/>.
        /// To cancel the current action, set the <see cref="Syncfusion.Blazor.Grids.ActionEventArgs{TValue}.Cancel"/> property to true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents OnActionBegin="ActionBeginHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void ActionBeginHandler(ActionEventArgs<Order> args)
        ///     {
        ///         args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ActionEventArgs<TValue>> OnActionBegin { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when the grid actions such as sorting, paging, grouping, ungrouping, reorder, rowdraganddrop, filtering, add, edit, delete, save and cancel action completed.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.ActionEventArgs{TValue}"/> object that provides details of the current grid action.
        /// An event triggered after the grid action has completed so you cannot prevent the current grid action using <see cref="Syncfusion.Blazor.Grids.ActionEventArgs{TValue}.Cancel"/>.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents OnActionComplete="ActionCompletedHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void ActionCompletedHandler(ActionEventArgs<Order> args)
        ///     {
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ActionEventArgs<TValue>> OnActionComplete { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised when a grid action fails to achieve the desired results. For example, if the provided URL in the dataSource property is incorrect, it will throw an exception in the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnActionFailure"/> event.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.FailureEventArgs"/> object that provides details of the error in the grid, including a stack trace of any exceptions that occurred.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents OnActionFailure="ActionFailureHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void ActionFailureHandler(FailureEventArgs args)
        ///     {
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<FailureEventArgs> OnActionFailure { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before new records are added to the UI when a user clicks the add toolbar item or presses the insert key.
        /// </summary>
        /// <remarks>
        /// This event will be raised only for <see cref="Syncfusion.Blazor.Grids.EditMode.Batch"/> mode.
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.BeforeBatchAddArgs{TValue}"/> object which provides access to the added records and allows for cancellation of the batch add operation using the <see cref="Syncfusion.Blazor.Grids.BeforeBatchAddArgs{TValue}.Cancel"/> property.
        /// Within this event handler, you can customize the default data that is being added to the grid element before it is added.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" Toolbar="@(new List<string>() { "Add", "Delete", "Update", "Cancel" })">
        /// <GridEvents OnBatchAdd="BatchAddHandler" TValue="Order"></GridEvents>
        /// <GridEditSettings AllowAdding="true" AllowDeleting="true" AllowEditing="true" Mode="EditMode.Batch"></GridEditSettings>
        /// </SfGrid>
        /// @code {
        ///     public void BatchAddHandler(BeforeBatchAddArgs<Order> args)
        ///     {
        ///       args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<BeforeBatchAddArgs<TValue>> OnBatchAdd { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before records are deleted in the grid element.
        /// You can perform delete action by click delete toolbar item or pressing the delete key. If no rows have been selected for deletion, a popup will be displayed allowing the user to select the rows they wish to delete before the operation is performed.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.BeforeBatchDeleteArgs{TValue}"/> object which provides details of the records to be deleted in Grid.
        /// You can prevent the batch delete action by setting the <see cref="Syncfusion.Blazor.Grids.BeforeBatchDeleteArgs{TValue}.Cancel"/> property to true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" Toolbar="@(new List<string>() { "Add", "Delete", "Update", "Cancel" })">
        /// <GridEvents OnBatchDelete="BatchDeleteHandler" TValue="Order"></GridEvents>
        /// <GridEditSettings AllowAdding="true" AllowDeleting="true" AllowEditing="true" Mode="EditMode.Batch"></GridEditSettings>
        /// </SfGrid>
        /// @code {
        ///     public void BatchDeleteHandler(BeforeBatchDeleteArgs<Order> args)
        ///     {
        ///       args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<BeforeBatchDeleteArgs<TValue>> OnBatchDelete { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before batch changes(such as added, edited and deleted data) are saved in dataSource. The edited data can be saved by clicking the <C>Update</C> button on the grid toolbar.
        /// When the Update button on the toolbar is clicked, a confirmation popup is displayed to confirm the save action to be performed in the grid.
        /// </summary>
        /// <remarks>
        /// The <see cref="Syncfusion.Blazor.Grids.BeforeBatchSaveArgs{TValue}.BatchChanges"/> property contains the batch changes so that you can customize them within the event handler..
        /// You can prevent the batch save action by setting <see cref="Syncfusion.Blazor.Grids.BeforeBatchSaveArgs{TValue}.Cancel"/> to true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" Toolbar="@(new List<string>() { "Add", "Delete", "Update", "Cancel" })">
        /// <GridEvents OnBatchSave="BatchSaveHandler" TValue="Order"></GridEvents>
        /// <GridEditSettings AllowAdding="true" AllowDeleting="true" AllowEditing="true" Mode="EditMode.Batch"></GridEditSettings>
        /// </SfGrid>
        /// @code {
        ///     public void BatchSaveHandler(BeforeBatchSaveArgs<Order> args)
        ///     {
        ///         args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<BeforeBatchSaveArgs<TValue>> OnBatchSave { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before batch changes are canceled from the grid element.
        /// The edited cell will be highlighted in the grid and it will be removed and returned to its original state after canceling the batch action.
        /// When the Cancel button on the toolbar is clicked, a confirmation popup is displayed to confirm the cancel action to be performed in the grid.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.BeforeBatchCancelArgs{TValue}"/> object which provides the details of the batch changes being canceled.
        /// You can customize the cancel action using this event handler.
        /// To cancel the this action, set the <see cref="Syncfusion.Blazor.Grids.BeforeBatchCancelArgs{TValue}.Cancel"/> property to true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" Toolbar="@(new List<string>() { "Add", "Delete", "Update", "Cancel" })">
        /// <GridEvents OnBatchCancel="BatchcancelHandler" TValue="Order"></GridEvents>
        /// <GridEditSettings AllowAdding="true" AllowDeleting="true" AllowEditing="true" Mode="EditMode.Batch"></GridEditSettings>
        /// </SfGrid>
        /// @code {
        ///     public void BatchcancelHandler(BeforeBatchCancelArgs<Order> args)
        ///     {
        ///       args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<BeforeBatchCancelArgs<TValue>> OnBatchCancel { get; set; }

        /// <summary>
        /// An event that is raised before copy or paste action in the Grid cells. You can cancel this entire copy or paste action by using this event.
        /// </summary>
        /// <remarks>
        /// This event triggers before <see cref="Syncfusion.Blazor.Grids.BeforeCellPasteEventArgs{TValue}"/> event, so you can cancel entire pasting operation by using this event.
        /// Also, this event handler receives a <see cref="Syncfusion.Blazor.Grids.BeforeCopyPasteEventArgs"/> object which provides the details of before paste/copy action.
        ///</remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" BeforeCopyPaste="Copy"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task Copy(BeforeCopyPasteEventArgs args)
        /// {
        ///      //you can cancel the entire copy action here
        ///       if(args.Action == "Copy"){
        ///          args.Cancel = true;
        ///       }
        ///     // you can cancel the entire paste action here
        ///       if(args.Action == "Paste"){
        ///          args.Cancel = true;
        ///       }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        [JsonIgnore]
        public EventCallback<BeforeCopyPasteEventArgs> BeforeCopyPaste{get; set;}

        /// <summary>
        /// An event that is raised before pasting the copied cell value for each cell. You can cancel the pasting action for particular cell or change the value by using this event.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.BeforeCellPasteEventArgs{TValue}"/> object which provides the details of before pasting the copied cell value in the current cell.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" BeforeCellPaste="Paste"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task Paste(BeforeCellPasteEventArgs<Orders> args)
        /// {
        ///   if(ColumnIndex == 1 && RowIndex == 4){
        ///       //you can modified the content to be paste here.
        ///      args.CellValue = "Modified value"; 
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        [JsonIgnore]
        public EventCallback<BeforeCellPasteEventArgs<TValue>> BeforeCellPaste{get; set;}

        /// <summary>
        /// Gets or sets an event callback that is raised before data is bound to the grid, allowing you to customize the current view data in this event handler.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.BeforeDataBoundArgs{TValue}"/> object, which contains current view data and total records count.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents OnDataBound="DataBoundHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void void DataBoundHandler(BeforeDataBoundArgs<Order> args)
        ///     {
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<BeforeDataBoundArgs<TValue>> OnDataBound { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised before a row enters edit mode in the UI, such as when a user double-clicks a cell or presses F2 / edit toolbar item to enter edit mode.
        /// </summary>
        /// <remarks>
        /// This event is raised when <see cref="Syncfusion.Blazor.Grids.EditMode"/> as Normal or Dialog.
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.BeginEditArgs{TValue}"/> object, that contains information about the record being edited. This allows you to customize the data before it enters into edit mode.
        /// You can prevent the edit action by setting <see cref="Syncfusion.Blazor.Grids.BeginEditArgs{TValue}.Cancel"/> to true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents OnBeginEdit="BeginEditHandler" TValue="Order"></GridEvents>
        ///     <GridEditSettings AllowAdding="true" AllowEditing="true" AllowDeleting="true" Mode="EditMode.Normal"></GridEditSettings>
        /// </SfGrid>
        /// @code {
        ///     public void BeginEditHandler(BeginEditArgs<Order> args)
        ///     {
        ///         args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<BeginEditArgs<TValue>> OnBeginEdit { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised before a cell enters edit mode in the UI, such as when a user double-clicks a cell or presses F2 to enter edit mode.
        /// </summary>
        /// <remarks>
        /// This event is raised when <see cref="Syncfusion.Blazor.Grids.EditMode.Batch"/> as Batch.
        /// Use this event to prevent the edit action by setting <see cref="Syncfusion.Blazor.Grids.CellEditArgs{TValue}.Cancel"/> to true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" Toolbar="@(new List<string>() { "Add", "Delete", "Update", "Cancel" })">
        /// <GridEvents OnCellEdit="CellEditHandler" TValue="Order"></GridEvents>
        /// <GridEditSettings AllowAdding="true" AllowDeleting="true" AllowEditing="true" Mode="EditMode.Batch"></GridEditSettings>
        /// </SfGrid>
        /// @code {
        ///     public void CellEditHandler(CellEditArgs<Order> args)
        ///     {
        ///       args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<CellEditArgs<TValue>> OnCellEdit { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before cell changes are updated in the UI. The save action will happen when the cell is in edit state and the user performs an action such as pressing Enter key, clicking or navigating to a new cell.
        /// </summary>
        /// <remarks>
        /// Within this event handler, you can customize the edited value before update it in grid UI.
        /// You can prevent the save action using <see cref="Syncfusion.Blazor.Grids.CellSaveArgs{TValue}.Cancel"/>.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" Toolbar="@(new List<string>() { "Add", "Delete", "Update", "Cancel" })">
        /// <GridEvents OnCellSave="CellSaveHandler" TValue="Order"></GridEvents>
        /// <GridEditSettings AllowAdding="true" AllowDeleting="true" AllowEditing="true" Mode="EditMode.Batch"></GridEditSettings>
        /// </SfGrid>
        /// @code {
        ///     public void CellSaveHandler(CellSaveArgs<Order> args)
        ///     {
        ///       args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<CellSaveArgs<TValue>> OnCellSave { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised after cell changes are updated in the grid user interface and the edited values are highlighted in the grid.
        /// </summary>
        /// <remarks>
        /// The cell save action is prevented using the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnCellSave"/> event, then the <c>CellSaved</c> event will not be raised.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" Toolbar="@(new List<string>() { "Add", "Delete", "Update", "Cancel" })">
        /// <GridEvents CellSaved="CellSavedHandler" TValue="Order"></GridEvents>
        /// <GridEditSettings AllowAdding="true" AllowDeleting="true" AllowEditing="true" Mode="EditMode.Batch"></GridEditSettings>
        /// </SfGrid>
        /// @code {
        ///     public void CellSavedHandler(CellSavedArgs<Order> args)
        ///     {
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<CellSavedArgs<TValue>> CellSaved { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised after a cell is selected in the grid. Cell selection can be done by click on the cell or pressing arrow keys with/without pressing Shift or Ctrl keys or programmatically.
        /// </summary>
        /// <remarks>        
        /// This event is raised when <see cref="Syncfusion.Blazor.Grids.SelectionMode"/> as Cell or Both.
        /// The selection of a cell is prevented using the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CellSelecting"/> event, then the <c>CellSelected</c> event will not be raised.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents CellSelected="CellselectHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void CellselectHandler(CellSelectEventArgs<Order> args)
        ///     {
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<CellSelectEventArgs<TValue>> CellSelected { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before any cell deselection occurs in the grid.
        /// If a cell is selected and click on to any other cell or pressing Tab or arrow keys without pressing Ctrl or Shift key or programmatically, then the previously selected cell will be deselected.
        /// </summary>
        /// <remarks>
        /// This event is raised when <see cref="Syncfusion.Blazor.Grids.SelectionMode"/> is set to Cell or Both.
        /// You can prevent the cell deselection action by setting <see cref="Syncfusion.Blazor.Grids.CellDeselectEventArgs{TValue}.Cancel"/> to true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents CellDeselecting="CellDeselectingHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void CellDeselectingHandler(CellDeselectEventArgs<Order> args)
        ///     {
        ///       args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<CellDeselectEventArgs<TValue>> CellDeselecting { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised after a selected cell is deselected in the grid. 
	    /// If a cell is selected and click on to any other cell or pressing Tab or arrow keys without pressing Ctrl or Shift key or programmatically, then the previously selected cell will be deselected.
        /// </summary>
        /// <remarks>
        /// This event is raised when <see cref="Syncfusion.Blazor.Grids.SelectionMode"/> as Cell or Both.
        /// The deselection of a cell is prevented using the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.CellDeselecting"/> event, then the <c>CellDeselected</c> event will not be raised.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents CellDeselected="CellDeselectHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void CellDeselectHandler(CellDeselectEventArgs<Order> args)
        ///     {
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<CellDeselectEventArgs<TValue>> CellDeselected { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before any cell selection occurs in the grid. Cell selection can be done by click on the cell or pressing Shift/Ctrl and click on the cell or pressing arrow keys with or without pressing Shift/Ctrl keys after selecting any cell.
        /// </summary>
        /// <remarks>
        /// This event is raised when <see cref="Syncfusion.Blazor.Grids.SelectionMode"/> as Cell and Both.
        /// You can prevent the cell selection action by setting <see cref="Syncfusion.Blazor.Grids.CellSelectingEventArgs{TValue}.Cancel"/> to true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents CellSelecting="CellselectingHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void CellselectingHandler(CellSelectingEventArgs<Order> args)
        ///     {
        ///        args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<CellSelectingEventArgs<TValue>> CellSelecting { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised after clicking on a column menu item in a grid.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.ColumnMenuClickEventArgs"/> object, which contains corresponding menu item and <see cref="Syncfusion.Blazor.Grids.GridColumn"/>.
        /// You can perform custom actions for column menu items within this event handler.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" AllowGrouping="true" AllowFiltering="true" AllowPaging="true" ShowColumnMenu="true">
        ///     <GridEvents ColumnMenuItemClicked="ColumnMenuItemClickedHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void ColumnMenuItemClickedHandler(ColumnMenuClickEventArgs args)
        ///     {
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ColumnMenuClickEventArgs> ColumnMenuItemClicked { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when the column menu is opened by clicking the column menu icon in the grid column.
        /// This event is also triggered when opening sub-menu items within the column menu.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.ColumnMenuOpenEventArgs"/> object, which contains the column menu instance. You can customize the column menu item properties within this event handler.
        /// To prevent the default action, set the <see cref="Syncfusion.Blazor.Grids.ColumnMenuOpenEventArgs.Cancel"/> property to true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" AllowGrouping="true" AllowFiltering="true" AllowPaging="true" ShowColumnMenu="true">
        ///     <GridEvents OnColumnMenuOpen ="ColumnMenuOpenHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void ColumnMenuOpenHandler(ColumnMenuOpenEventArgs args)
        ///     {
        ///         args.Cancel = true; // Prevents the column menu from opening
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ColumnMenuOpenEventArgs> OnColumnMenuOpen { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised after command column button is clicked in the grid.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.CommandClickEventArgs{TValue}"/> object, which contains corresponding command column and row details.
        /// With this event handler, you can perform a custom action based on the row and command column details.
        /// You can prevent the action by setting <see cref="Syncfusion.Blazor.Grids.CommandClickEventArgs{TValue}.Cancel"/> as true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" AllowGrouping="true" AllowFiltering="true" AllowPaging="true" ShowColumnMenu="true">
        ///     <GridEvents CommandClicked="OnCommandClicked" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void OnCommandClicked(CommandClickEventArgs<Order> args)
        ///     {
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<CommandClickEventArgs<TValue>> CommandClicked { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised after clicking an item in context menu of grid. To enable the context menu, you can define either default or custom items in the ContextMenuItems property.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.ContextMenuClickEventArgs{TValue}"/> object, which contains details about the corresponding menu item, column, and row information. 
        /// You can perform custom actions for context menu items within this event handler.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid ContextMenuItems="@(new List<object>() { "AutoFit", "AutoFitAll", "SortAscending", "SortDescending","Copy", "Edit", 
        ///   "Delete", "Save", "Cancel","PdfExport", "ExcelExport", "CsvExport", "FirstPage", "PrevPage","LastPage", "NextPage"})" >
        ///   <GridEvents ContextMenuItemClicked="ContextMenuItemClickedHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void ContextMenuItemClickedHandler(ContextMenuClickEventArgs args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ContextMenuClickEventArgs<TValue>> ContextMenuItemClicked { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before the context menu is opened by right-clicking anywhere on the grid. The context menu items displayed will depend on the target of the right-click performs in the grid like "header", "content" or "pager".
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.ContextMenuOpenEventArgs{TValue}"/> object, which contains context menu instance so you can customize the context menu item property, within this event handler.
        /// If you want to prevent the default action, you can set the <see cref="Syncfusion.Blazor.Grids.ContextMenuOpenEventArgs{TValue}.Cancel"/> property as true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid ContextMenuItems="@(new List<object>() { "AutoFit", "AutoFitAll", "SortAscending", "SortDescending","Copy", "Edit", 
        ///   "Delete", "Save", "Cancel","PdfExport", "ExcelExport", "CsvExport", "FirstPage", "PrevPage","LastPage", "NextPage"})" >
        ///   <GridEvents ContextMenuOpen="ContextMenuOpenHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void ContextMenuOpenHandler(ContextMenuOpenEventArgs<Order> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ContextMenuOpenEventArgs<TValue>> ContextMenuOpen { get; set; }

        /// <summary>
        /// An event that is raised when the component is created. Event can be used to perform any necessary initialization logic before the component is rendered.
        /// </summary>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents Created="CreatedHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void CreatedHandler()
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<object> Created { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised after a grid component finished rendering. This event can be used to perform any custom logic after data is populated in grid component.
        /// </summary>
        /// <remarks>
        /// This event is invoked after the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnDataBound"/> event.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents DataBound="DataBoundHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void DataBoundHandler()
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<object> DataBound { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when the grid component is destroyed. This can happen when the component is removed from the DOM or when the page is refreshed.
        /// </summary>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents Destroyed="DestroyHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void DestroyHandler()
        ///  {
        ///  }
        /// }
        /// ]]></code>
        /// </example>
        [Parameter]
        public EventCallback<object> Destroyed { get; set; }

        /// <summary>
        /// Get or set an event callback that is raised when a detail row is expanded. The purpose of this event is to bind values or data to the detail template element that will be used to render the contents of the detail row.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.DetailDataBoundEventArgs{TValue}"/> object that provides details about the selected row.
	/// Based on the selected row information, perform any action and display any template element within the detailed row of grid.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents DetailDataBound="DetailDataBoundHandler" TValue="Employee"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void DetailDataBoundHandler(DetailDataBoundEventArgs<Employee> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<DetailDataBoundEventArgs<TValue>> DetailDataBound { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when detail row is expanding by click on the collapsed icon of the corresponding row or programmatically expanding the detail row.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.DetailsExpandingEventArgs{TValue}"/> object which provides details about the expanding row.
        /// You can prevent the expand action using the <see cref="Syncfusion.Blazor.Grids.DetailsExpandingEventArgs{TValue}.Cancel"/> property.
	    /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" DetailsExpanding="DetailsExpand"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task DetailsExpand(DetailsExpandingEventArgs<Orders> args)
        /// {
        ///   args.Cancel = true;
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<DetailsExpandingEventArgs<TValue>> DetailsExpanding {get; set;}

        /// <summary>
        /// Gets or sets an event callback that is raised after a detail row is expanded by click on the collapsed icon of the corresponding row or programmatically expanding the detail row to display the detail template content.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.DetailsExpandedEventArgs{TValue}"/> object which provides the details of the expanded detail row.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" DetailsExpanded="DetailsExpanded"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task DetailsExpanded(DetailsExpandedEventArgs<Orders> args)
        /// {
        ///   ...........
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<DetailsExpandedEventArgs<TValue>> DetailsExpanded {get; set;}

        /// <summary>
        /// Gets or sets an event callback that is raised when the detail template row is collapsing by click on the expanded icon in the grid or programmatically collapsed the detail row.  
        /// </summary>
        /// <remarks>
        ///  You can prevent collapse action using <see cref="Syncfusion.Blazor.Grids.DetailsCollapsingEventArgs{TValue}.Cancel"/>
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" DetailsCollapsing="DetailsCollapse"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task DetailsCollapse(DetailsCollapsingEventArgs<Orders> args)
        /// {
        ///   args.Cancel = true;
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<DetailsCollapsingEventArgs<TValue>> DetailsCollapsing {get; set;}

        /// <summary>
        /// Gets or sets an event callback that is raised after the detail template row is collapsed by click on the expanded icon in the grid or programmatically collapsed the detail row.  
        /// </summary>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" DetailsCollapsed="DetailsCollapsed"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task DetailsCollapsed(DetailsCollapsedEventArgs<Orders> args)
        /// {
        ///   ...........
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<DetailsCollapsedEventArgs<TValue>> DetailsCollapsed {get; set;}

        /// <summary>
        /// Gets or sets the event callback that is raised before the autofill action. 
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.BeforeAutoFill"/> object which provides the details of before autofill action.
        /// Also,this event triggers when you release the dragged fill handle icon. You can cancel the entire cells getting automatically filled in the cell.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" BeforeAutoFill="BeforeAutoFillAction"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task BeforeAutoFillAction(BeforeAutoFillEventArgs args)
        /// {     
        ///       //you can cancel the autofill action here.
        ///       args.Cancel = true;
        ///   }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        [JsonIgnore]
        public EventCallback<BeforeAutoFillEventArgs> BeforeAutoFill { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is raised before the autofill action sets the value for each cell. You can cancel the autofill action for particular cell or change the value by using this event.
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.BeforeAutoFill"/> event if that event is not canceled.
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.BeforeAutoFillCell"/> object which provides the details of before autofill action.    
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        /// <GridEvents TValue="Orders" BeforeAutoFillCell="BeforeAutoFillCellAction"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task BeforeAutoFillCellAction(BeforeAutoFillCellEventArgs<Orders> args)
        /// {
        ///   if(args.ColumnIndex == 1 && args.RowIndex == 5)
        ///   { 
        ///       //you can modified the content to be paste here.
        ///       args.Value = "Modified Value";
        ///   }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        [JsonIgnore]
        public EventCallback<BeforeAutoFillCellEventArgs<TValue>> BeforeAutoFillCell { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before a request is made to access the grid header cell information.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.HeaderCellInfo"/> object which provides the details of header cells.
        /// This event allows you to customize the header cells by adding classes, changing their header text, etc...
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// The following example demonstrates how to handle the HeaderCellInfo event to customize the header cells.
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" @ref="DefaultGrid" >
        ///   <GridEvents TValue="Orders" HeaderCellInfo="HeaderCell"></GridEvents>
        ///   ........
        /// </SfGrid>
        /// 
        /// @code{
        ///   public async Task HeaderCell(HeaderCellInfoEventArgs args)
        ///   {
        ///     if (args.Column.Field == "OrderID")
        ///     {
        ///       // You can customize the header cell.
        ///       args.Cell.AddClass(new string[] { "newclass" });
        ///     }
        ///   }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<HeaderCellInfoEventArgs> HeaderCellInfo { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before rendering of the grid, and it provides a callback method that you can use to customize the grid properties.
        /// </summary>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents TValue="Orders" OnLoad="LoadHandler"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void LoadHandler(object args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<object> OnLoad { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before the cell element is appended to the grid element. And the event is raised whenever a grid cell is rendered or refreshed.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.QueryCellInfoEventArgs{TValue}"/> object, which provides grid row and cell details. 
        /// Within this event handler, you can customize the cell element.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents QueryCellInfo="QueryCellInfoHandler" TValue="Orders"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void QueryCellInfoHandler(QueryCellInfoEventArgs<Orders> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<QueryCellInfoEventArgs<TValue>> QueryCellInfo { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when a cell is clicked in the grid.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.RecordClickEventArgs{TValue}"/> object, which provides the information about the clicked cell and row information.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents OnRecordClick="RecordClickHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void RecordClickHandler(RecordClickEventArgs<Order> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RecordClickEventArgs<TValue>> OnRecordClick { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when a cell is double clicked in grid.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.RecordDoubleClickEventArgs{TValue}"/> object, which provides the information about the clicked cell and row information.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents OnRecordDoubleClick="RecordDoubleClickHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void RecordDoubleClickHandler(RecordDoubleClickEventArgs<Order> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RecordDoubleClickEventArgs<TValue>> OnRecordDoubleClick { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when column resizing is starts.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.ResizeArgs"/> object which provides the details of the resizing column.
        /// You can prevent the resize action using <see cref="Syncfusion.Blazor.Grids.ResizeArgs.Cancel"/>.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents OnResizeStart="OnResizeStartHanlder" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void OnResizeStartHanlder(ResizeArgs args)
        ///  {
        ///     args.Cancel = true;
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ResizeArgs> OnResizeStart { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when the column resizing is ends.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.ResizeArgs"/> object which provides the details of the resized column.
        /// You can prevent the resize action using <see cref="Syncfusion.Blazor.Grids.ResizeArgs.Cancel"/>.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents ResizeStopped="ResizeStoppedHanlder" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void ResizeStoppedHanlder(ResizeArgs args)
        ///  {
        ///     args.Cancel = true;
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ResizeArgs> ResizeStopped { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised whenever a request is made to access row information, element, or data.
        /// This will be triggered before the row element is appended to the grid.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.RowDataBoundEventArgs{TValue}"/> object, which provides information about the corresponding row.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents RowDataBound="RowDataBoundHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void RowDataBoundHandler(RowDataBoundEventArgs<Order> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowDataBoundEventArgs<TValue>> RowDataBound { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised after a selected row is deselected in the grid.
        /// When a row is selected in the grid, if the same row is clicked again or if arrow keys are pressed to move to another row, the previously selected row will be deselected.
        /// </summary>
        /// <remarks>
        /// This event is raised when <see cref="Syncfusion.Blazor.Grids.SelectionMode"/> as Row or Both.
        /// The deselection of a row is prevented using the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowDeselecting"/> event, then the <c>RowDeselected</c> event will not be raised.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents RowDeselected="RowDeselectHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void RowDeselectHandler(RowDeselectEventArgs<Order> args)
        ///     {
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowDeselectEventArgs<TValue>> RowDeselected { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before any row deselection occurs in the grid.
        /// When a row is selected in the grid, if the same row is clicked again or if arrow keys are pressed to move to another row, the previously selected row will be deselected.
        /// </summary>
        /// <remarks>
        /// You can prevent the row deselection by setting <see cref="Syncfusion.Blazor.Grids.RowDeselectEventArgs{TValue}.Cancel"/> as true.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents RowDeselecting="RowDeselectingHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void RowDeselectingHandler(RowDeselectEventArgs<Order> args)
        ///     {
        ///      args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowDeselectEventArgs<TValue>> RowDeselecting { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when we start dragging the rows to perform row reordering.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.RowDragStartingEventArgs{TValue}"/> object which provides the details of the rows from which it is dragged.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders">
        /// <GridEvents TValue="Orders" RowDragStarting="RowDragStartingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowDragStartingHandler(RowDragStartingEventArgs<Orders> args)
        /// {
        ///      //you can get the dragged row data's here
        ///      List<Orders> Data = args.Data;
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        [JsonIgnore]
        public EventCallback<RowDragStartingEventArgs<TValue>> RowDragStarting { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised when the row elements are dropping on the target element. You can cancel the dropping action using this event.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.RowDroppingEventArgs{TValue}"/> object which provides the details of the rows which are dropping and the target where the rows are dropping.
        /// If the dropping action is prevented using the <c>Cancel</c> argument, then the RowDropped event doesn't trigger.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders">
        /// <GridEvents TValue="Orders" RowDropping="RowDroppingHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowDroppingHandler(RowDroppingEventArgs<Orders> args)
        /// {
        ///      //you can cancel the dropping action here
        ///      args.Cancel = true;
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        [JsonIgnore]
        public EventCallback<RowDroppingEventArgs<TValue>> RowDropping { get; set; }
    
        /// <summary>
        /// Gets or sets an event callback that is raised when row elements are dropped on the target element.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.RowDroppedEventArgs{TValue}"/> object which provides the details of the rows which are dropped and the target where the rows are dropped.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders">
        /// <GridEvents TValue="Orders" RowDropped="RowDroppedHandler"></GridEvents>
        /// ........
        /// </SfGrid>
        /// @code{
        /// public async Task RowDroppedHandler(RowDroppedEventArgs<Orders> args)
        /// {
        ///      //you can get the dropped row data's here
        ///      List<Orders> Data = args.Data;
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        [JsonIgnore]
        public EventCallback<RowDroppedEventArgs<TValue>> RowDropped { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is triggered when the drag selection starts in the Grid.
        /// The selection begins when the mouse is pressed and dragged across rows or cells.
        /// </summary>
        /// <value>An event callback of type <see cref="RowDragSelectionEventArgs{TValue}"/>.</value>
        /// <remarks>
        /// This event is raised for all selection modes: Row, Cell, and Both.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents RowDragSelectionStarting="OnDragSelectionStart" TValue="Order" />
        /// </SfGrid>
        /// @code {
        ///   public void OnDragSelectionStart(RowDragSelectionEventArgs<Order> args) { }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <summary>
        /// Gets or sets the event callback that is triggered when the drag selection starts in the Grid.
        /// The selection begins when the mouse is pressed and dragged across rows or cells.
        /// </summary>
        /// <value>An event callback of type <see cref="RowDragSelectionEventArgs{TValue}"/>.</value>
        /// <remarks>
        /// This event is raised for all selection modes: Row, Cell, and Both.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents RowDragSelectionStarting="OnDragSelectionStart" TValue="Order" />
        /// </SfGrid>
        /// @code {
        ///   public void OnDragSelectionStart(RowDragSelectionEventArgs<Order> args) { }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowDragSelectionEventArgs<TValue>> RowDragSelectionStarting { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is triggered when the mouse is released to complete the drag selection.
        /// </summary>
        /// <value>An event callback delegate with <see cref="RowDragSelectedEventArgs{TValue}"/>.</value>
        /// <remarks>
        /// This event provides an instance of <see cref="RowDragSelectedEventArgs{TValue}"/>, which contains details such as the target Grid ID 
        /// and the selected range of rows and cells.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents RowDragSelectionCompleting="OnDragSelectionCompleting" TValue="Order" />
        /// </SfGrid>
        /// @code {
        ///   public void OnDragSelectionCompleting(RowDragSelectedEventArgs<Order> args) { }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowDragSelectedEventArgs<TValue>> RowDragSelectionCompleting { get; set; }

        /// <summary>
        /// Gets or sets the event callback that is triggered after the drag selection is completed in the Grid.
        /// </summary>
        /// <value>An event callback of type <see cref="RowDragSelectedEventArgs{TValue}"/>.</value>
        /// <remarks>
        /// This event provides an instance of <see cref="RowDragSelectedEventArgs{TValue}"/>, which contains details such as the target Grid ID 
        /// and the selected range of rows and cells.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents RowDragSelectionCompleted="OnDragSelectionCompleted" TValue="Order" />
        /// </SfGrid>
        /// @code {
        ///   public void OnDragSelectionCompleted(RowDragSelectedEventArgs<Order> args) { }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowDragSelectedEventArgs<TValue>> RowDragSelectionCompleted { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised after a row is selected in the grid. Row selection can be done by click on the row or presssing arrow keys with or wihtout Shift or Ctrl keys or doing drag selection or programmatically.
        /// </summary>
        /// <remarks>
        /// This event is raised when <see cref="Syncfusion.Blazor.Grids.SelectionMode"/> as Row or Both.
        /// The selection of a row is prevented using the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.RowSelecting"/> event, then the <c>RowSelected</c> event will not be raised.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents RowSelected="RowselectHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void RowselectHandler(RowSelectEventArgs<Order> args)
        ///     {
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowSelectEventArgs<TValue>> RowSelected { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before any row selection occurs in the grid. Row selection can be done by click on the row or presssing arrow keys with or wihtout Shift or Ctrl keys or doing drag selection or programmatically.
        /// </summary>
        /// <remarks>
        /// This event is raised when <see cref="Syncfusion.Blazor.Grids.SelectionMode"/> as Row or Both. 
        /// You can prevent the cell selection action using <see cref="Syncfusion.Blazor.Grids.RowSelectingEventArgs{TValue}.Cancel"/>.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEvents RowSelecting="RowselectingHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///     public void RowselectingHandler(RowSelectingEventArgs<Order> args)
        ///     {
        ///       args.Cancel = true;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<RowSelectingEventArgs<TValue>> RowSelecting { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised on moving freeze line.
        /// </summary>
        /// <remarks> 
        /// You can prevent the freeze action using <see cref="Syncfusion.Blazor.Grids.FreezeLineMovingEventArgs.Cancel"/>.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example> 
        /// <code> 
        /// <![CDATA[ 
        /// <SfGrid DataSource="@Orders"> 
        ///     <GridEvents FreezeLineMoving="FreezeLineMovingHandler" TValue="Order"></GridEvents> 
        /// </SfGrid> 
        /// @code { 
        ///     public void FreezeLineMovingHandler(FreezeLineMoving args) 
        ///     { 
        ///         args.Cancel = true;
        ///     } 
        /// } 
        /// ]]> 
        /// </code> 
        /// </example>
        [Parameter]
        public EventCallback<FreezeLineMovingEventArgs> FreezeLineMoving { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised after moved freeze line.
        /// </summary>
        /// <remarks> 
        /// This event handler receives a  <see cref="Syncfusion.Blazor.Grids.FreezeLineMovedEventArgs" /> object which provides frozen columns details.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example> 
        /// <code> 
        /// <![CDATA[ 
        /// <SfGrid DataSource="@Orders"> 
        ///     <GridEvents FreezeLineMoved="FreezeLineMovedHandler" TValue="Order"></GridEvents> 
        /// </SfGrid> 
        /// @code { 
        ///     public void FreezeLineMovedHandler(FreezeLineMoving args) 
        ///     { 
        ///     } 
        /// } 
        /// ]]> 
        /// </code> 
        /// </example>
        [Parameter]
        public EventCallback<FreezeLineMovedEventArgs> FreezeLineMoved { get; set; }


        /// <summary>
        /// Gets or sets an event callback that is raised when a toolbar item is clicked or the Enter key is pressed after focusing on the toolbar item.
        /// </summary>
        /// <remarks>
        /// This event handler receives a object which provides the details about the toolbar items.
        /// Within this event handler, you can use custom actions for toolbar items.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid DataSource="@Orders" Toolbar="@(new List<string>() { "Add", "Edit", "Delete", "Cancel", "Update" })">
        ///   <GridEvents OnToolbarClick="ToolbarClickHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<Syncfusion.Blazor.Navigations.ClickEventArgs> OnToolbarClick { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before the column chooser dialog is open while click the columns icon in the toolbar. The column chooser allows the user to show or hide columns by changing the state of the checkbox.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.ColumnChooserEventArgs"/> object which provide details about <see cref="Syncfusion.Blazor.Grids.GridColumns"/>.
	    /// You can customize the column chooser dialog elements using this event.
        /// You can prevent the column chooser action using <see cref="Syncfusion.Blazor.Grids.ColumnChooserEventArgs.Cancel"/>.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid ShowColumnChooser="true" Toolbar="@(new List<string>() { "ColumnChooser" })">
        ///   <GridEvents BeforeOpenColumnChooser="BeforeOpenColumnChooserHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  public void BeforeOpenColumnChooserHandler(ColumnChooserEventArgs Args)
        ///  {
        ///     args.Cancel = true;
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<ColumnChooserEventArgs> BeforeOpenColumnChooser { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised just before the grid is exported to a PDF document. This event is triggered when the user clicks on the PDF exporting icon in the toolbar.
        /// </summary>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents OnPdfExport="PdfExportHandler" OnToolbarClick="ToolbarClickHandler" TValue="Order"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "PDF Export")
        ///   {
        ///    await this.Grid.PdfExport();
        ///   }
        ///  }
        ///  public void PdfExportHandler(object args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<object>? OnPdfExport { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised whenever a grid data cell is exported into PDF document. This event is triggered when the user clicks on the PDF exporting icon in the toolbar.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.PdfQueryCellInfoEventArgs{TValue}"/> object, which provides corresponding row and cell informations.
        /// Within this event handler, you can customize the appearance and contents of individual data cells in the exported PDF document.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid AllowPdfExport="true" Toolbar="@(new List<string>() { "PdfExport" })">
        ///   <GridEvents PdfQueryCellInfoEvent="PdfQueryCellInfoHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "PDF Export")
        ///   {
        ///    await this.Grid.PdfExport();
        ///   }
        ///  }
        ///  public void PdfQueryCellInfoHandler(PdfQueryCellInfoEventArgs<BusinessObject> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<PdfQueryCellInfoEventArgs<TValue>>? PdfQueryCellInfoEvent { get; set; }
        
        /// <summary>
        /// Gets or sets an event callback that is raised whenever grid header cell is exported into PDF document. This event is triggered when the user clicks on the PDF exporting icon in the toolbar.
        /// </summary>
        /// <remarks>
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.PdfHeaderQueryCellInfoEventArgs"/> object, which provides corresponding column and cell informations.
        /// Within this event handler, you can customize the appearance and contents of individual header cells in the exported PDF document.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid AllowPdfExport="true" Toolbar="@(new List<string>() { "PdfExport" })">
        ///   <GridEvents PdfHeaderQueryCellInfoEvent="PdfHeaderQueryCellInfoHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "PDF Export")
        ///   {
        ///    await this.Grid.PdfExport();
        ///   }
        ///  }
        ///  public void PdfHeaderQueryCellInfoHandler(PdfHeaderQueryCellInfoEventArgs args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<PdfHeaderQueryCellInfoEventArgs>? PdfHeaderQueryCellInfoEvent { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised whenever a caption, footer, or group footer aggregate row is created on the PDF document. This event is triggered when the user clicks on the PDF exporting icon in the toolbar.
        /// </summary>
        /// <remarks>
        /// Within this event handler, you can customize the appearance and contents of caption, footer, or group footer aggregate rows in the exported PDF document.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid AllowPdfExport="true" Toolbar="@(new List<string>() { "PdfExport" })">
        ///   <GridEvents PdfAggregateTemplateInfo="PdfAggregateTemplateInfoHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "PDF Export")
        ///   {
        ///    await this.Grid.PdfExport();
        ///   }
        ///  }
        ///  public void PdfAggregateTemplateInfoHandler(PdfAggregateEventArgs<Order> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<PdfAggregateEventArgs>? PdfAggregateTemplateInfo { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised whenever a group caption template is created on the PDF document. This event is triggered when the user clicks on the PDF exporting icon in the toolbar.
        /// </summary>
        /// <remarks>
        /// Within this event handler, you can customize the appearance and contents of group caption templates in the exported PDF document.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid AllowPdfExport="true" Toolbar="@(new List<string>() { "PdfExport" })">
        ///   <GridEvents PdfGroupCaptionTemplateInfo="PdfGroupCaptionHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "PDF Export")
        ///   {
        ///    await this.Grid.PdfExport();
        ///   }
        ///  }
        ///  public void PdfGroupCaptionHandler(PdfCaptionTemplateArgs Args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<PdfCaptionTemplateArgs>? PdfGroupCaptionTemplateInfo { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before a detail template is append to Pdf file format.
        /// </summary>
        /// <remarks>
        /// This event will be triggered only when the <c>PdfDetailRowMode</c> is set to <c>Expand</c> in <see cref="Syncfusion.Blazor.Grids.PdfExportProperties"/>.
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.PdfDetailTemplateEventArgs{TValue}"/> object, which provides details about the corresponding parent row, along with additional customization options for the PDF detail template.
        /// Within this event handler, you can customize the appearance and content of the PDF document before a detail template added. Additionally, this event supports achieving nested grid exporting.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid AllowPdfExport="true" Toolbar="@(new List<string>() { "PdfExport" })">
        ///   <GridEvents PdfDetailTemplateExporting="PdfDetailTemplateEventHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "PDF Export")
        ///   {
        ///    await this.Grid.PdfExport();
        ///   }
        ///  }
        ///  public void PdfDetailTemplateEventHandler(PdfDetailTemplateEventArgs<Order> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<PdfDetailTemplateEventArgs<TValue>>? PdfDetailTemplateExporting { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised before the grid data is exported to an Excel/CSV file. This event is triggered when the user clicks on the Excel exporting icon in the toolbar.
        /// </summary>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents OnExcelExport="ExcelExportHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "Excel Export")
        ///   {
        ///    await this.Grid.ExcelExport();
        ///   }
        ///  }
        ///  public void ExcelExportHandler(object args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<object>? OnExcelExport { get; set; }

         /// <summary>
        /// Gets or sets an event callback that is raised whenever a caption, footer, or group footer aggregate row is created on the excel sheet.  This event is triggered when the user clicks on the Excel exporting icon in the toolbar.
        /// </summary>
        /// <remarks>
        /// Within this event handler, you can customize the appearance and contents of caption, footer, or group footer aggregate rows in the exported Excel file.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents ExcelAggregateTemplateInfo="ExcelAggregateTemplateInfoHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "Excel Export")
        ///   {
        ///    await this.Grid.ExcelExport();
        ///   }
        ///  }
        ///  public void ExcelAggregateTemplateInfoHandler(ExcelAggregateEventArgs args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<ExcelAggregateEventArgs>? ExcelAggregateTemplateInfo { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised whenever a group caption template is created on the excel sheet. This event is triggered when the user clicks on the Excel exporting icon in the toolbar.
        /// </summary>
        /// <remarks>
        /// Within this event handler, you can customize the appearance and contents of group caption templates in the exported Excel file.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents ExcelGroupCaptionTemplateInfo="ExcelGroupCaptionHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "Excel Export")
        ///   {
        ///    await this.Grid.ExcelExport();
        ///   }
        ///  }
        ///  public void ExcelGroupCaptionHandler(ExcelCaptionTemplateArgs Args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<ExcelCaptionTemplateArgs>? ExcelGroupCaptionTemplateInfo { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised whenever a data is entered into a cell of the Excel sheet. This event is triggered when the user clicks on the Excel exporting icon in the toolbar.
        /// </summary>
        /// <remarks>
        /// Within this event handler, you can customize the appearance and contents of individual data cells in the exported Excel file.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents ExcelQueryCellInfoEvent="ExcelQueryCellInfoHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "Excel Export")
        ///   {
        ///    await this.Grid.ExcelExport();
        ///   }
        ///  }
        ///  public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<Order> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<ExcelQueryCellInfoEventArgs<TValue>>? ExcelQueryCellInfoEvent { get; set; }

        /// <summary>
        /// Gets or sets an event callback that is raised whenever a data entered into a header cell of the excel sheet. This event is triggered when the user clicks on the Excel exporting icon in the toolbar.
        /// </summary>
        /// <remarks>
        /// Within this event handler, you can customize the appearance and contents of individual header cells in the exported Excel file.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents ExcelHeaderQueryCellInfoEvent="ExcelHeaderQueryCellInfoHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "Excel Export")
        ///   {
        ///    await this.Grid.ExcelExport();
        ///   }
        ///  }
        ///  public void ExcelHeaderQueryCellInfoHandler(ExcelHeaderQueryCellInfoEventArgs<Order> args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<ExcelHeaderQueryCellInfoEventArgs>? ExcelHeaderQueryCellInfoEvent { get; set; }


        /// <summary>
        /// Gets or sets an event callback that is raised before a detail template is appended to an Excel sheet.
        /// </summary>
        /// <remarks>
        /// This event will be triggered only when the <see cref="Syncfusion.Blazor.Grids.ExcelDetailRowMode"/> is set to <c>Expand</c> or <c>Collapse</c> in <see cref="Syncfusion.Blazor.Grids.ExcelExportProperties"/>.
        /// This event handler receives a <see cref="Syncfusion.Blazor.Grids.ExcelDetailTemplateEventArgs{TValue}"/> object, which provides details about the corresponding parent row along with additional customization options for the Excel detail template.
        /// Within this event handler, you can customize the appearance and content of the exported Excel file before a detail template added. Additionally, this event supports achieving nested grid exporting.
        /// </remarks>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents ExcelDetailTemplateExporting="ExcelDetailTemplateEventHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "Excel Export")
        ///   {
        ///    ExcelExportProperties ExportProperties = new ExcelExportProperties();
        ///    ExportProperties.ExcelDetailRowMode = ExcelDetailRowMode.Expand;
        ///    await this.Grid.ExcelExport(ExportProperties);
        ///   }
        ///  }
        ///  public void ExcelDetailTemplateEventHandler(ExcelDetailTemplateEventArgs<Order> args)
        ///  {
        ///    . . . .
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<ExcelDetailTemplateEventArgs<TValue>>? ExcelDetailTemplateExporting { get; set; }


        /// <summary>
        /// Gets or sets an event callback that is raised when the export process is completed.
        /// </summary>
        /// <value>
        /// An event callback function.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid>
        ///   <GridEvents ExportComplete="ExportCompleteHandler" OnToolbarClick="ToolbarClickHandler" TValue="BusinessObject"></GridEvents>
        /// </SfGrid>
        /// @code {
        ///  SfGrid<BusinessObject> Grid;
        ///  public async Task ToolbarClickHandler(Syncfusion.Blazor.Navigations.ClickEventArgs Args)
        ///  {
        ///   if (Args.Item.Text == "Excel Export")
        ///   {
        ///    await this.Grid.ExcelExport();
        ///   }
        ///  }
        ///  public void ExportCompleteHandler(object args)
        ///  {
        ///  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [Parameter]
        public Action<object>? ExportComplete { get; set; }

        /// <summary>
        /// Initializes the GridEvents and assigns it to the parent grid instance.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);

            Parent!.GridEvents = this;
        }
    }
}