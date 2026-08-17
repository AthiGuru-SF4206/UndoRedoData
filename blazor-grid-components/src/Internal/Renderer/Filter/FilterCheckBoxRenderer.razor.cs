using System;
using System.Collections.Generic;
using System.Text;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Syncfusion.Blazor.Internal;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Syncfusion.Blazor.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001002382fcb1069523ce72d849497a557a445c151eaf4007aa79adef551a8204ca7f728e5378607d85695b16f129ec35bf4af15dcf6d3581deb8bb0debb239c33e7f1271a37c7f60f1044ae417730f5082abee5f9ec568a8a4cef04074394755706376e982dc6f9d15430faaad385ae8f00a77ef1c97517f1a1517004ce78028b9ce")]
namespace Syncfusion.Blazor.Grids.Internal
{

    /// <summary>
    /// Represents a renderer for filter checkboxes in a grid.
    /// </summary>
    public partial class FilterCheckBoxRenderer<TContent>
    {
        private SfDialog? ExcelDialog { get; set; }

        /// <summary>
        /// Gets or sets the dialog animation effects for the filter checkbox.
        /// </summary>
        public DialogEffect Effects { get; set; } = DialogEffect.None;

        private bool VisibleProperty { get; set; }

        private ExcelBase<TContent>? ExcelBaseRef { get; set; }

        /// <summary>
        /// Gets or sets the parent grid.
        /// </summary>
        [CascadingParameter]
        public SfGrid<TContent>? Parent { get; set; }

        /// <summary>
        /// Gets or sets the horizontal position value for the filter dialog.
        /// </summary>
        public string Xvalue { get; set; } = "right";

        /// <summary>
        /// Gets or sets the vertical position value for the filter dialog.
        /// </summary>
        public string Yvalue { get; set; } = "bottom";

        /// <summary>
        /// Gets or sets the column associated with the filter checkbox.
        /// </summary>
        [Parameter]
        public GridColumn? Column { get; set; }

        private object? InputValue { get; set; }

        private bool SelectAllChk { get; set; }

        private bool IsCurrentSelectionChecked { get; set; }

        private bool FullDataEmpty { get; set; }

        private string InputFocus { get; set; } = string.Empty;

        private string XPosition { get; set; } = "center";

        private string YPosition { get; set; } = "center";

        private List<CheckBoxModel> FilteredData { get; set; } = new List<CheckBoxModel>();

        private List<CheckBoxModel> AddPredicate { get; set; } = new List<CheckBoxModel>();

        private Dictionary<string,List<CheckBoxModel>> DistinctPredicate { get; set; } = new Dictionary<string, List<CheckBoxModel>>();

        private bool Intermediate { get; set; }

        private bool isCurrentSelecitonFilter { get; set; }

        private PropertyInfoHelper PropertyHelper { get; set; } = new PropertyInfoHelper();
        private ISyncfusionStringLocalizer? Localizer;

        private Dictionary<string, object> CheckedData { get; set; } = new Dictionary<string, object>();

        private IEnumerable<object>? CheckboxListData { get; set; }

        private Dictionary<string, object> InitialBindedData { get; set; } = new Dictionary<string, object>();

        private bool FilterDialogInitialRendering { get; set; }

        private bool IsClearIconPressed { get; set; }


        private bool IsOkButtonDisabled { get; set; }

        private int FilterChoiceCount { get; set; }

        private string? NullKey { get; set; }

        private object? NullData { get; set; }

        private bool NullChecked { get; set; }

        private bool isBlank { get; set; }

        private bool isSearch { get; set; }

        private string CancelIcon { get; set; } = string.Empty;

        private string SearchValue { get; set; } = string.Empty;

        private string excelSearchOperator { get; set; } = "none";

        private List<PredicateModel<object>> predicateModels { get; set; } = new List<PredicateModel<object>>();


        //AdaptiveDialog Section Started
        private SfDialog? CheckBoxAdaptiveDialog { get; set; }

        private bool IsContextMenuItemSelected { get; set; } = true;
        private bool IsBackButtonTriggered { get; set; }        
      

        private Dictionary<string, object> MaxHeight = new Dictionary<string, object>()
     {
        { "data-sf-style", "max-height:100%"}
    };
       
        private async Task AdaptiveRendered()
        {
            VisibleProperty = true;
            await OpenFilterDialog(XPosition, YPosition).ConfigureAwait(true);
            Parent!.FilterModule!.FilterDialogInstance = CheckBoxAdaptiveDialog;
            await Parent.InvokeMethod("sfBlazor.Grid.customFilterDialog", new object[] { Parent.DataId, $"{Column?.Uid}customcheckboxfilter", Parent.FilterModule.GetFilterType(Column!) == "Excel" }).ConfigureAwait(true);
            VisibleProperty = false;
        }

        internal void CloseHandler()
        {
            IsBackButtonTriggered = true;
            if(Parent!.FilterModule != null)
            {
                Parent.FilterModule.FilterIconIsClicked = false;
            }
                      
            Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
        }

        internal string GetContentDialogClassNames() 
        {
            string styleText = "e-filter-popup";

            if (Parent?.FilterModule != null && Parent.FilterModule.GetFilterType(Column!) == "Excel")
            {
                styleText = string.Concat(styleText, " e-excelfilter");
            }
            else
            {
                styleText = string.Concat(styleText, " e-checkboxfilter");
            }

            return styleText;
        }

        internal string GetAdaptiveDialogClassNames() 
        {
            string styleText = " e-resfilterdiv e-bigger e-responsive-dialog  e-resfilter e-row-responsive-filter e-dlg-fullscreen";

            if (Parent!.FilterModule != null &&  Parent.FilterModule.GetFilterType(Column!) == "Excel")
            {
                styleText = string.Concat(styleText, " e-excelfilter");
            }
            else
            {
                styleText = string.Concat(styleText, " e-bigger e-checkboxfilter");
            }
            return styleText;
        }

        private void OpenCheckboxDialog(object args)
        {
            IsContextMenuItemSelected = true;
            StateHasChanged();
        }

        private void CloseCheckboxDialog(object args)
        {
            IsContextMenuItemSelected = false;
            StateHasChanged();
        }

        //AdaptiveDialog Section Ended

        /// <summary>
        /// Initializes the component and registers event handlers for checkbox dialogs.
        /// </summary>
        protected override void OnInitialized()
        {
            Localizer = Parent!.Localizer;
            CheckedData?.Clear();
            Parent.EventAggregator.Add("CloseAdaptiveCheckBoxDialog", CloseCheckboxDialog);
            Parent.EventAggregator.Add("OpenAdaptiveCheckBoxDialog", OpenCheckboxDialog);
        }

        private static Operator GetOperator(string value)
        {
            return Filter<object>.GetOperator(value);
        }
        private async Task FilterBtnClick()
        {
            if(Parent!.FilterModule != null)
            {
                Parent.FilterModule.ExcelDialog = true;
            }      
            var CheckedCount = GetCheckedDataCount();
            if(Parent.FilterModule != null)
            {
                Parent.FilterModule.FilterIconIsClicked = false;
            }          
            if (!Parent.EnableAdaptiveUI)
            {
                await (ExcelDialog?.HideAsync()!).ConfigureAwait(true);
            }
            else
            {
                IsBackButtonTriggered = true;
                Parent.EventAggregator.Trigger("CloseAdaptiveDialog", null!);
                Parent.EventAggregator.Trigger("CloseColumnMenuAdaptiveDialog", null!);
            }
            if( Parent.SelectionModule != null &&!string.IsNullOrEmpty(Parent.SelectionModule.IsSelectFilteredField))
            {
                Parent.SelectionModule.IsSelectFilteredField = string.Empty;
                Parent.SelectionModule.IsHeaderCheckboxChecked = false;
            }
            if (CheckedCount == FilteredData?.Count && Parent.FilterModule != null)
            {
                await Parent.FilterModule.RemoveFilterColumnByField(Column?.Field!).ConfigureAwait(true);
            }
            else
            {
                var isNotEqual = FilteredData?.Count != CheckedCount && FilteredData?.Count - CheckedCount < CheckedCount;
                if (isNotEqual)
                {
                    var unCheckCount = GetUnCheckedDataCount();
                    await GeneratePredicate("notequal", unCheckCount).ConfigureAwait(true);
                    await Parent.ModelChanged(new ActionEventArgs<TContent>() { Columns = predicateModels, CurrentFilteringColumn = Column!.Field, RequestType = Grids.Action.Filtering }, requestType: "Filtering", eventArgs: new FilteringEventArgs() { FilterPredicates = predicateModels, ColumnName = Column.Field }).ConfigureAwait(true);
                }
                else
                {
                    await GeneratePredicate("equal", CheckedCount).ConfigureAwait(true);
                    await Parent.ModelChanged(new ActionEventArgs<TContent>() { Columns = predicateModels, CurrentFilteringColumn = Column!.Field, RequestType = Grids.Action.Filtering }, requestType: "Filtering", eventArgs: new FilteringEventArgs() { FilterPredicates = predicateModels, ColumnName = Column.Field }).ConfigureAwait(true);
                }
            }

            if (Parent.ColumnMenuInstance != null && Parent.FilterModule != null)
            {
                Parent.FilterModule.HideColumnMenuPopup();
            }

            Parent.FilterModule!.ExcelDialog = false;
        }

        internal object? GetActualValue(object value)
        {
            if (Column?.ValueType?.Namespace != "System")
                return value?.ToString();
            switch (Column?.Type)
            {
                case ColumnType.Integer:
                case ColumnType.Double:
                case ColumnType.Long:
                case ColumnType.Decimal:
                    var IsInteger = Parent!.FilterModule!.IntConvertedList.ContainsKey(Column.ValueType);
                    var isLong = Parent.FilterModule.LongConvertedList.ContainsKey(Column.ValueType);
                    if (IsInteger)
                    {
                        if (int.TryParse(value?.ToString(), out int temp) == true)
                        {
                            value = SfBaseUtils.ChangeType(value, Column?.ValueType);
                        }
                    }
                    else if (isLong)
                    {
                        if (long.TryParse(value?.ToString(), out long temp) == true)
                        {
                            value = SfBaseUtils.ChangeType(value, Column?.ValueType);
                        }
                    }
                    else if (double.TryParse(value?.ToString(), out double temp) == true)
                    {
                        value = SfBaseUtils.ChangeType(value, Column?.ValueType);
                    }
                    else
                    {
                        value = "grd-search-notvalid";
                        isSearch = true;
                        CheckedData?.Clear();
                    }
                    break;
                case ColumnType.Date:
                case ColumnType.DateTime:
                    if (DateTime.TryParseExact(value?.ToString(), Column?.Format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime Temp) == true)
                    {
                        value = Temp;
                    }
                    else if(value != null && Parent!.DataManager != null && Parent!.DataManager.DataAdaptor != null && !Parent!.DataManager.DataAdaptor.IsRemote())
                    {
                        value = value.ToString()!;
                    }
                    else
                    {
                        value = "grd-search-notvalid";
                        CheckedData?.Clear();
                        isSearch = true;
                    }
                    break;
                case ColumnType.Boolean:
                    if (bool.TryParse((string)value, out bool b))
                    {
                        value = SfBaseUtils.ChangeType(value, Column?.ValueType);
                    }
                    else
                    {
                        value = "grd-search-notvalid";
                        CheckedData?.Clear();
                        isSearch = true;
                    }
                    break;
                case ColumnType.None:
                case ColumnType.String:
                    value = SfBaseUtils.ChangeType(value, Column?.ValueType);
                    break;
            }
            return value;
        }

        private async Task GeneratePredicate(string Operator, int checkcount = 0)
        {
            // Use a local non-null column reference to avoid redundant null-condition checks
            var column = Column;
            if (column == null)
            {
                // If Column is not available, nothing to generate
                return;
            }

            var checkCount = checkcount;
            var Data = GetCheckChangeValues(Operator);
            var predicate = Operator == "notequal" ? "and" : "or";
            PredicateModel<object> predicateModel = new PredicateModel<object>();
#pragma warning disable BL0005
            if (Parent!.FilterSettings!.Columns == null)
            {
                Parent.FilterSettings.Columns = new List<GridFilterColumn>();
            }
#pragma warning restore BL0005
            if (IsCurrentSelectionChecked)
            {
                var values = DistinctPredicate[column.Uid];
                foreach (var v in Data)
                {
                    var temp = values?.Find(x => x.Value != null && x.Value.Equals(v.Value));
                    if (temp != null)
                    {
                        values?.Remove(temp);
                        values?.Add(v);
                    }
                }

                var unCheckDatas = GetUnCheckChangeValues();
                foreach (var v in unCheckDatas)
                {
                    var temp = values?.Find(x => x.Value?.Equals(v.Value) == true);
                    if (temp != null)
                    {
                        values?.Remove(temp);
                        values?.Add(v);
                    }
                }

                DistinctPredicate?.Remove(column.Uid);
                DistinctPredicate?.Add(column.Uid, values!);
                RemoveExistingPredicate();

                if (DistinctPredicate?.TryGetValue(column.Uid, out var value) == true)
                {
                    foreach (var val in DistinctPredicate[column.Uid])
                    {
                        var operators = val.isChecked ? "equal" : "notequal";
                        var predict = val.isChecked ? "or" : "and";
                        if (val != null && val.isChecked && (val.FUid == column.Uid))
                        {
                            using var filterColumn = new GridFilterColumn()
                            {
#pragma warning disable BL0005
                                Field = column.IsForeignColumn() == true ? column.ForeignKeyValue! : column.Field!,
                                Operator = Filter<TContent>.GetOperator(operators),
                                Value = val.Value,
                                MatchCase = true,
                                IgnoreAccent = Parent.FilterSettings.IgnoreAccent,
                                Uid = column.Uid!,
                                Predicate = "or",
                                ColumnType = Filter<TContent>.GetColumnType(column.Type)!
#pragma warning restore BL0005
                            };

                            predicateModel = new PredicateModel<object>()
                            {
                                Field = (column.IsForeignColumn() == true ? column.ForeignKeyValue : column.Field)!,
                                Operator = Filter<TContent>.GetOperator(operators),
                                Value = val.Value,
                                MatchCase = true,
                                IgnoreAccent = Parent.FilterSettings.IgnoreAccent,
                                Uid = column.Uid!,
                                Predicate = "or"
                            };
                            
                            predicateModels?.Add(predicateModel);
                            Parent.FilterSettings.Columns?.Add(filterColumn);
                        }
                    }
                }
            }     
            else
            {
                RemoveExistingPredicate();

                foreach (var Fval in Data)
                {
                    if (Fval != null && Fval.Value != null && Fval.Value.ToString() != "null" && !string.IsNullOrEmpty(Fval.Value.ToString()))
                    {
                        using var filterColumn = new GridFilterColumn()
                        {
#pragma warning disable BL0005
                            Field = (column.IsForeignColumn() == true ? column.ForeignKeyValue : column.Field)!,
                            Operator = Filter<TContent>.GetOperator(Operator),
                            Value = Fval.Value,
                            MatchCase = true,
                            IgnoreAccent = Parent.FilterSettings.IgnoreAccent,
                            Uid = column.Uid!,
                            Predicate = predicate,
                            ColumnType = Filter<TContent>.GetColumnType(column.Type)!
#pragma warning restore BL0005
                        };
                        predicateModel = new PredicateModel<object>()
                        {
                            Field = (column.IsForeignColumn() == true ? column.ForeignKeyValue : column.Field)!,
                            Operator = Filter<TContent>.GetOperator(Operator),
                            Value = Fval.Value,
                            MatchCase = true,
                            IgnoreAccent = Parent.FilterSettings.IgnoreAccent,
                            Uid = column.Uid!,
                            Predicate = predicate
                        };                        
                        predicateModels?.Add(predicateModel);
                        Parent.FilterSettings.Columns?.Add(filterColumn);
                    }
                    else
                    {
                        NullValueFiltering(Operator, column.ValueType!);
                    }
                }
            }
            
            await Parent.FilterSettings.UpdateProperties("Columns", Parent.FilterSettings.Columns!).ConfigureAwait(true);
        }

        internal List<CheckBoxModel> GetCheckChangeValues(string Operator)
        {
            bool equal = Operator == "notequal" ? false : true;
            List<CheckBoxModel> Model = new List<CheckBoxModel>();
            foreach (var val in CheckedData)
            {
                var data = (CheckBoxModel)val.Value;
                if (data?.isChecked == equal)
                {
                    Model.Add(data);
                }
            }
            return Model;
        }

        internal List<CheckBoxModel> GetUnCheckChangeValues()
        {
            List<CheckBoxModel> Model = new List<CheckBoxModel>();
            foreach (var val in CheckedData)
            {
                var data = (CheckBoxModel)val.Value;
                if (!data.isChecked)
                {
                    Model.Add(data);
                }
            }
            return Model;
        }

        internal void NullValueFiltering(string opr, Type valueType)
        {
            object[] Values = new object[] { null! };
            int nullCount = 1;
            var predicate = opr == "notequal" ? "and" : "or";
            if (Column!.Type == ColumnType.String)
            {
                Values = new object[] { null!, string.Empty };
                nullCount = 2;
            }

            PredicateModel<object> predicateModel = new PredicateModel<object>();
            for (var n = 0; n < nullCount; n++)
            {
                using var filterColumn = new GridFilterColumn()
                {
#pragma warning disable BL0005
                    Field = (Column?.IsForeignColumn() == true ? Column.ForeignKeyValue : Column?.Field)!,
                    Operator = Filter<TContent>.GetOperator(opr),
                    Value = Values[n],
                    MatchCase = true,
                    IgnoreAccent = Parent!.FilterSettings!.IgnoreAccent,
                    Uid = Column?.Uid!,
                    Predicate = predicate
#pragma warning restore BL0005
                };
                predicateModel = new PredicateModel<object>()
                {
                    Field = (Column?.IsForeignColumn() == true ? Column.ForeignKeyValue : Column?.Field)!,
                    Operator = Filter<TContent>.GetOperator(opr),
                    Value = Values[n],
                    MatchCase = true,
                    IgnoreAccent = Parent.FilterSettings.IgnoreAccent,
                    Uid = Column?.Uid!,
                    Predicate = predicate
                };
                predicateModels?.Add(predicateModel);
                Parent.FilterSettings.Columns?.Add(filterColumn);
            }
        }

        internal void RemoveExistingPredicate()
        {
            if (Parent!.FilterSettings!.Columns != null)
            {
                int Colcount = Parent.FilterSettings.Columns.Count;
                for (var i = Colcount - 1; i >= 0; i--)
                {
                    if (Parent.FilterSettings.Columns[i]?.Uid == Column?.Uid)
                    {
                        Parent.FilterSettings.Columns.RemoveAt(i);
                    }
                }
            }
        }

        private async Task ClearBtnClick()
        {
            Parent!.FilterModule!.ExcelDialog = true;
            Parent.FilterModule.FilterIconIsClicked = false;
            if (!Parent.EnableAdaptiveUI)
            {
                if (ExcelDialog != null)
                {
                    await ExcelDialog.HideAsync().ConfigureAwait(true);
                }                
            }
            else
            {
                IsBackButtonTriggered = true;
                Parent.EventAggregator.Trigger("CloseAdaptiveDialog", null!);
            }
            if (Parent.ColumnMenuInstance != null)
            {
                Parent.FilterModule.HideColumnMenuPopup();
            }

            var filterType = Parent.FilterModule.GetFilterType(Column!);
            if (filterType == "Excel" || filterType == "CheckBox")
            {
                if (filterType == "Excel")
                {
                    Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                }
                Parent.FilterModule.ExcelDialog = false;
            }
            if (filterType != "Excel" || Parent.EnableAdaptiveUI)
            {
                await Parent.FilterModule.RemoveFilterColumnByField(Column?.Field!, Column?.Uid!, foreginKeyFieldName: Column!.ForeignKeyValue!).ConfigureAwait(true);
            }
        }

        private async Task DialogCreated()
        {
            if (Parent!.FocusModule != null)
            {
                Parent.FocusModule.IsSelectAllClicked = false;
            }
            VisibleProperty = true;
            if (Parent.FilterModule != null)
            {
                Parent.FilterModule.FilterIconIsClicked = true;
                Parent.FilterModule.FilterDialogInstance = ExcelDialog;
            }

            if (Parent.FilterModule != null && Parent.FilterModule.FilterIconIsClicked && Parent.FilterModule.FilterIconColumn != null)
            {
                if (Parent.FilterModule.IsColumnMenuFilter)
                {
                    var dlgID = Column?.Uid + "_excelDlg";
                    string[] positions = await Parent.InvokeMethod<string[]>("sfBlazor.Grid.filterPopupRender", false, new object[] { Parent.DataId, dlgID, Column?.Uid!, "excel", Parent.FilterModule.IsColumnMenuFilter }).ConfigureAwait(true);
                    if (positions != null)
                    {
                        XPosition = positions[0];
                        YPosition = positions[1];
                        await OpenFilterDialog(XPosition, YPosition).ConfigureAwait(true);
                    }
                    VisibleProperty = false;
                }
                else
                {
                    await OpenFilterDialog().ConfigureAwait(true);
                    VisibleProperty = false;
                }
            }
        }
        
        internal void Closed()
        {
            if (Parent!.FilterModule != null && Parent.FocusModule != null && !Parent.FilterModule.IsSubMenuClick)
            {
                Parent.FilterModule.FilterIconIsClicked = false;
                Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                Parent.FocusModule.Focus("", "", headerUid: Column?.Uid).GetAwaiter();
            }
        }

        internal void OncloseDialog(BeforeCloseEventArgs args)
        {
            if(args?.ClosedBy == "Escape" && Parent!.FilterSettings!.Type == FilterType.Excel)
            {
                ExcelBaseRef?.Dispose();
            }
        }

        /// <summary>
        /// Opens the filter dialog at the specified position and triggers related events before rendering.
        /// </summary>
        public async Task OpenFilterDialog(string XPosition = "", string YPosition = "")
        {
            FilterDialogInitialRendering = true;
            var actionArgs = new ActionEventArgs<TContent>() { RequestType = Grids.Action.FilterBeforeOpen, Cancel = false, ColumnName = Column?.Field!, Parent = Parent, CheckboxListData = null!};
            await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent!.GridEvents?.OnActionBegin, actionArgs).ConfigureAwait(true);
            await Parent.EventAggregator.NotifyAsync("ActionBegin", actionArgs).ConfigureAwait(true);
            FilterDialogOpeningEventArgs dialogOpeningArgs = new FilterDialogOpeningEventArgs()
            {
                ColumnName = this.Column?.Field!,
                Parent = Parent,
                FilterChoiceCount = FilterChoiceCount,
            };
            if ((Parent.GridEvents != null && Parent.GridEvents!.FilterDialogOpening.HasDelegate) || Parent.IsRenderedFromTreeGrid)
            {
                if(Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("FilterDialogOpening", dialogOpeningArgs).ConfigureAwait(true);
                else if(Parent.GridEvents != null)
                    await Parent.GridEvents.FilterDialogOpening.InvokeAsync(dialogOpeningArgs).ConfigureAwait(true);
                if (dialogOpeningArgs.Cancel)
                {
                    Parent.FilterModule!.FilterIconIsClicked = false;
                    Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                    return;
                }
            }
            if (dialogOpeningArgs.Cancel)
            {
                Parent.FilterModule!.FilterIconIsClicked = false;
                Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                return;
            }
            if (actionArgs.Cancel)
            {
                Parent.FilterModule!.FilterIconIsClicked = false;
                Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                return;
            }

            if (Parent.FilterModule!.IsColumnMenuFilter)
            {
                Xvalue = XPosition;
                Yvalue = YPosition;
            }

            await UpdateDataSource(false, actionArgs.CheckboxListData ?? dialogOpeningArgs.CheckboxListData, dialogOpeningArgs.FilterChoiceCount).ConfigureAwait(true);
            if (CheckBoxAdaptiveDialog != null && Parent.EnableAdaptiveUI && ((Parent.AdaptiveUIMode.Equals(AdaptiveMode.Both)) || (Parent.SyncfusionService.IsDeviceMode && Parent.AdaptiveUIMode.Equals(AdaptiveMode.Mobile)) || (Parent.AdaptiveUIMode.Equals(AdaptiveMode.Desktop) && Parent.SyncfusionService?.IsDeviceMode != true) )) 
            {
                await CheckBoxAdaptiveDialog.ShowAsync().ConfigureAwait(true);
            }
            else {
                StateHasChanged();
                if (ExcelDialog != null)
                await ExcelDialog.ShowAsync().ConfigureAwait(true);              
            }
            
            if (Parent.FilterModule != null && Column != null && Parent.FilterModule.GetFilterType(Column) == "Excel")
            {
                await Parent.InvokeMethod("sfBlazor.Grid.focusExcelInput", new object[] { Parent.DataId, Column.Uid}).ConfigureAwait(true);
            }

            var CompleteArgs = new ActionEventArgs<TContent>() { RequestType = Grids.Action.FilterAfterOpen, Cancel = false, ColumnName = Column?.Field!, Parent = Parent };
            await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent.GridEvents?.OnActionComplete, CompleteArgs).ConfigureAwait(true);
            await Parent.EventAggregator.NotifyAsync("ActionComplete", CompleteArgs).ConfigureAwait(true);
            if (actionArgs.Cancel)
            {
                Parent.FilterModule!.FilterIconIsClicked = false;
                Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                return;
            }
            if ((Parent.GridEvents != null && Parent.GridEvents.FilterDialogOpened.HasDelegate) || Parent.IsRenderedFromTreeGrid)
            {
                var dialogOpenedArgs = new FilterDialogOpenedEventArgs() 
                { 
                    ColumnName = this.Column?.Field!, 
                    Parent = Parent, 
                    FilterChoiceCount = dialogOpeningArgs.FilterChoiceCount, 
                    CheckboxListData = dialogOpeningArgs.CheckboxListData 
                };
                if(Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("FilterDialogOpened", dialogOpenedArgs).ConfigureAwait(true);
                else
                    await Parent.GridEvents!.FilterDialogOpened.InvokeAsync(dialogOpenedArgs).ConfigureAwait(true);
            }
            if(Parent.FilterModule != null)
            {
                Parent.FilterModule.FilterIconIsClicked = true;
            }
            FilterDialogInitialRendering = false;
        }

        private async void CurrentSelectionClickHandler(MouseEventArgs args)
        {
            Parent!.FocusModule!.ClickedCheckBoxId = "addCurrentSelection";
            IsCurrentSelectionChecked = !IsCurrentSelectionChecked;
            if (IsCurrentSelectionChecked)
            {
                IsOkButtonDisabled = false;
            }
            else if (!IsCurrentSelectionChecked && !(SelectAllChk || Intermediate))
            {
                IsOkButtonDisabled = true;
            }
                    
        }

        private async void CheckBoxClickHandler(MouseEventArgs args, string Key)
        {
            Parent!.FocusModule!.IsSelectAllClicked = false;
            var argument = args;
            var data = (CheckBoxModel)CheckedData?.GetValueOrDefault(Key)!;
            var CheckValue = data?.Value?.ToString();
            bool isNull = string.IsNullOrEmpty(CheckValue) || CheckValue == "null";
            Parent.FocusModule.ClickedCheckBoxId = Key;
            if (isNull || Column?.IsForeignColumn() == true)
            {
                foreach (var cData in CheckedData!)
                {
                    var nullData = (CheckBoxModel)cData.Value;
                    var nullCheckValue = nullData?.Value?.ToString();
                    bool isDataNull = string.IsNullOrEmpty(nullCheckValue) || nullCheckValue == "null";
                    if (isDataNull && isNull && nullData != null)
                    {
                        nullData.isChecked = !nullData.isChecked;
                    }
                    else if (!isDataNull && CheckValue == nullData?.Value?.ToString() && nullData != null)
                    {
                        nullData.isChecked = !nullData.isChecked;
                    }
                }
            }
            else
            {
                if(data != null)
                {
                    data.isChecked = !data.isChecked;
                }
            }

            SelectAllHandler();
        }

        internal List<WhereFilter> getPredicate(List<GridFilterColumn> columns)
        {
            var cols = DataGenerator<TContent>.Distinct(columns, "Uid", true);
            List<GridFilterColumn> collection;
            List<WhereFilter> predicate = new List<WhereFilter>();
            List<WhereFilter> foreignPred = new List<WhereFilter>();
            List<WhereFilter> predicateList = new List<WhereFilter>();
            foreach (var col in cols)
            {
                if (col?.Uid != null && col.Uid != Column?.Uid)
                {
                    var ForeignKeyFCol = GridUtils.GetColumnByFColUidOrField(col.Uid,  Parent!.IsStackedHeader ? Parent.GetColumnsAsync().Result : (List<GridColumn>)Parent.Columns!);
                    if (ForeignKeyFCol?.IsForeignColumn() == true)
                    {
                        if (col != null && Parent.DataModule != null)
                        {
                            predicateList = Parent.ForeignKeyModule!.ForeignKeyPredicates(ForeignKeyFCol, predicateList);
                        }

                        predicate.Add(new WhereFilter() { Condition = "and", IsComplex = true, predicates = predicateList });
                    }
                    else
                    {
                        collection = columns.Where(c => c.Uid == col.Uid).ToList();
                        if (collection.Count != 0)
                        {
                            predicate.Add(DataGenerator<TContent>.GeneratePredicate(collection));
                        }
                    }
                }
            }

            return predicate;
        }

        internal async Task<Query> GenerateWherePredicate(bool isForeignDataEmpty)
        {
            var isForeignKeyDataEmpty = isForeignDataEmpty;
            Query query = new Query();
            Query Foreignquery = new Query();

            Query ForeignDataquery = new Query();
            query.Distincts = new List<string>() { Column?.Field!};
            List<WhereFilter> WhereFilters = new List<WhereFilter>();
            List<WhereFilter> ForeignFilters = new List<WhereFilter>();
            List<WhereFilter> InputFilters = new List<WhereFilter>();
            FullDataEmpty = false;
            if (Parent!.FilterSettings!.Columns != null)
            {
                List<WhereFilter> CheckBoxPredicate = getPredicate(Parent.FilterSettings.Columns);
                if (CheckBoxPredicate.Count > 0)
                {
                    WhereFilters.Add(new WhereFilter() { Condition = "and", IsComplex = true, predicates = CheckBoxPredicate });
                }
            }

            if (InputValue != null && !string.IsNullOrEmpty(InputValue.ToString()))
            {
                var Field = Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue : Column?.Field;
                if (Column?.IsForeignColumn() == true)
                {
                    ForeignFilters.Add(new WhereFilter() { Field = Field, Operator = excelSearchOperator != "none" ? excelSearchOperator : Column?.FilterSettings?.Operator?.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? "contains", value = InputValue, IgnoreCase = true, Condition = "and" });
                    ForeignDataquery.Where(new WhereFilter() { Condition = "or", IsComplex = true, predicates = ForeignFilters });
                    await GenerateForeignPredicate(ForeignDataquery, Foreignquery).ConfigureAwait(true);
                    if (Foreignquery.Queries?.Where != null)
                    {
                        InputFilters.Add(new WhereFilter() { Condition = "and", IsComplex = true, predicates = Foreignquery.Queries.Where });
                    }
                    else
                    {
                        FullDataEmpty = true;
                    }
                }
                else
                {
                    if ((Parent.DataManager?.Adaptor == Adaptors.ODataV4Adaptor || Parent.DataManager?.Adaptor == Adaptors.ODataAdaptor) && Column?.ValueType != typeof(string))
                    {
                        InputFilters.Add(new WhereFilter() { Field = Field, Operator = excelSearchOperator != "none" ? excelSearchOperator : Column?.FilterSettings?.Operator?.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? "equal", value = InputValue, IgnoreCase = true, Condition = "and" });
                    }
                    else
                    {
                        InputFilters.Add(new WhereFilter() { Field = Field, Operator = excelSearchOperator != "none" ? excelSearchOperator : Column?.FilterSettings?.Operator?.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? "contains", value = InputValue, IgnoreCase = true, Condition = "and" });
                    }
                }
            }

            if (WhereFilters.Count != 0 && InputFilters.Count == 0)
            {
                var condition = isSearch ? "or" : "and";
                query.Where(new WhereFilter() { Condition = condition, IsComplex = true, predicates = WhereFilters });
            }
            else if (WhereFilters.Count != 0 || InputFilters.Count != 0)
            {
                if (WhereFilters.Count != 0)
                {
                    InputFilters.Add(new WhereFilter() { Condition = "or", IsComplex = true, predicates = WhereFilters });
                }

                query.Where(new WhereFilter() { Condition = "and", IsComplex = true, predicates = InputFilters });
            }

            return query;
        }

        private async Task UpdateDataSource(bool isInputSearch = false, IEnumerable<object>? customFilterData = null, int filterChoiceCount = 0)
        {
            isSearch = isInputSearch;
            isCurrentSelecitonFilter = true;
            IEnumerable<object>? CheckboxData = null;
            bool isForeignDataEmpty = false;
            var actionArgs = new ActionEventArgs<TContent>() { RequestType = Grids.Action.FilterChoiceRequest, ColumnName = Column!.Field, Parent = Parent };
            var filterSearchingArgs = new CheckboxFilterSearchingEventArgs() { SearchText = SearchValue, ColumnName = Column.Field, Parent = Parent, CheckboxListData = null! };

            if (!isInputSearch)
            {

                isCurrentSelecitonFilter = false;

                if ((Parent!.GridEvents != null && Parent.GridEvents.OnActionBegin.HasDelegate) || Parent.IsRenderedFromTreeGrid)
                {
                    if(Parent.IsRenderedFromTreeGrid)
                        await Parent.EventAggregator.NotifyAsync("ActionBegin", actionArgs).ConfigureAwait(true);
                    else
                        await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent.GridEvents!.OnActionBegin, actionArgs).ConfigureAwait(true);
                }
                FilterChoiceCount = (int)actionArgs.FilterChoiceCount > 0 ? (int)actionArgs.FilterChoiceCount : filterChoiceCount;
                customFilterData = actionArgs.CheckboxListData ?? customFilterData;

            }

            if (!(InputValue != null && !string.IsNullOrEmpty(InputValue.ToString()))) 
            {
                IsCurrentSelectionChecked = false;
                isCurrentSelecitonFilter = false;
            }

            if (customFilterData == null)
            {
                Query Foreignquery = new Query();
                object? FColDataSource = null;
                if (Column?.IsForeignColumn() == true)
                {
                    FColDataSource = await (Column?.GetData(Foreignquery.Queries)!).ConfigureAwait(true);
                    isForeignDataEmpty = !((IEnumerable<object>)Column.ColumnData!).Any();
                }

                Query DataManagerQuery = await GenerateWherePredicate(isForeignDataEmpty).ConfigureAwait(true);
                Query GridQuery = Column?.IsForeignColumn() == true && isForeignDataEmpty ? new Query() : Parent!.DataModule!.GenerateQuery().RequiresCount();
                var SortField = Column?.IsForeignColumn() == true ? Column?.IsForeignKeyField() == true ? Column?.Field : Column?.ForeignKeyField : Column?.Field;
                if(SortField != null)
                {
                    DataManagerQuery.Sort(SortField, "ascending");
                }
                
                var needSort = GridQuery?.Queries?.Sorted?.FindAll(x => x?.Name == SortField);
                if (SortField != null && (needSort == null || needSort.Count == 0))
                {
                    GridQuery?.Sort(SortField, "ascending");
                }

                DataManagerQuery.Queries.Search = GridQuery?.Queries?.Search;
                DataManagerQuery.Queries.RequiresCounts = true;
                if (Parent!.Query != null && Parent.Query.Queries.Where != null)
                {
                    if (DataManagerQuery.Queries.Where == null)
                    {
                        DataManagerQuery.Queries.Where = Parent.Query.Queries.Where;
                    }
                    else
                    {
                        foreach (var where in Parent.Query.Queries.Where)
                        {
                            DataManagerQuery.Queries.Where.Add(where);
                        }
                    }
                }
                DataManagerQuery.Queries.Params = GridQuery?.Queries?.Params;
                DataManagerQuery.Queries.Expand = GridQuery?.Queries?.Expand;
                GridQuery!.Queries.Select = DataManagerQuery.Queries.Select = new List<string>() { SortField! };
                GridQuery.Queries.RequiresCounts = true;
                IEnumerable tmpFullData = ((DataResult)await (Parent?.DataManager?.ExecuteQuery<TContent>(DataManagerQuery)!).ConfigureAwait(true))?.Result!;
                var fullData = (IEnumerable<object>)tmpFullData?.OfType<TContent>().ToList()!;
                if (DataManagerQuery.Queries.Distinct != null)
                {
                    CheckboxData = fullData;
                }
                else
                {
                    object? result = null;
                    if (Parent.DataManager.Adaptor != Adaptors.BlazorAdaptor)
                    {
#pragma warning disable BL0005
                        using DataManager customDataManager = new SfDataManager() { Json = fullData, Adaptor = Adaptors.BlazorAdaptor };
#pragma warning restore BL0005
                        result = await customDataManager.ExecuteQuery<TContent>(GridQuery).ConfigureAwait(true);
                    }
                    else
                    {
                        result = await Parent.DataManager.ExecuteQuery<TContent>(GridQuery).ConfigureAwait(true);
                    }

                    IEnumerable<object>? Data = result is DataResult ? (IEnumerable<object>)((DataResult)result).Result! : (IEnumerable<object>)result;
                    Data = ForeignKey<TContent>.GetForeignKeyDataSource(Column!, Data) as IEnumerable<object>;

                    IEnumerable<object>? FullData = FullDataEmpty ? new List<IEnumerable<object>>() : fullData;
                    FullData = ForeignKey<TContent>.GetForeignKeyDataSource(Column!, FullData) as IEnumerable<object>;

                    var propertyName = Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue : Column?.Field;
                    var DistinctData = (Data != null && propertyName != null) ? DataUtil.GetDistinct(Data, propertyName) : Enumerable.Empty<object>();
                    var DistinctFullData = FullData != null && propertyName != null && ((FullData.Count() >= DistinctData?.Cast<object>().Count()) || isSearch) ? DataUtil.GetDistinct(FullData, propertyName) : DistinctData;
                    DistinctFullData = DistinctFullData?.Take(FilterChoiceCount == 0 ? 1000 : FilterChoiceCount);
                    CheckboxData = DistinctFullData;
                    if(CheckboxListData == null)
                    {
                        CheckboxListData = CheckboxData;
                    }
                }
            }
            else
            {
                if(isInputSearch && Column?.FilterItemTemplate == null)
                {
                    CheckboxData = customFilterData
                    .Where(val =>
                    {
                        var value = Parent!.PropHelper?.GetValue(val, Column?.Field)?.ToString()?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                        var searchText = SearchValue?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                        return value != null && searchText != null && value.Contains(searchText, StringComparison.Ordinal);
                    })
                    ?.ToList();
                    CheckboxListData = CheckboxData;
                }
                else{
                    CheckboxData = customFilterData.Take(FilterChoiceCount == 0 ? 1000 : FilterChoiceCount); // bind the custom datasource from searchbegin event
                }
                if (CheckboxListData == null)
                {
                    CheckboxListData = CheckboxData;
                }
            }

            //For Blanks input search
            if (CheckboxListData != null && isInputSearch && customFilterData == null)
            {
                var blanksText = Localizer?.GetText(GridLocaleKeys.Blanks)?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                var distinctCheckboxData = CheckboxListData
                    .Where(val =>
                    {
                        var value = Parent!.PropHelper?.GetValue(val, Column?.Field);
                        var searchText = SearchValue?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                        return value == null && !string.IsNullOrEmpty(searchText) && blanksText?.Contains(searchText, StringComparison.Ordinal) == true;
                    })
                    ?.ToList();

                if (distinctCheckboxData?.Count > 0)
                {
                    CheckboxData = distinctCheckboxData.Concat(CheckboxData!.Except(distinctCheckboxData)).AsQueryable();
                    CheckboxData = CheckboxData.Where(list => CheckboxListData.Contains(list));                }
            }

            CheckedData?.Clear();

            if (CheckboxData != null && !CheckboxData.Any() && Column?.Format != null && (!Column.Format.Contains('C', StringComparison.Ordinal) || !Column.Format.Contains('N', StringComparison.Ordinal)))
            {
                var FilteredValues = InitialBindedData?.Values?.Where(e => ((CheckBoxModel)e)?.FormattedValue?.ToString()?.Contains(SearchValue, StringComparison.Ordinal) == true)?.ToList();

                foreach (var data in FilteredValues!)
                {
                    var val = (CheckBoxModel)data;
                    var uniqueId = "cbox" + Guid.NewGuid().ToString();
                    var FilterValue = PropertyHelper?.GetObject(Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue!: Column?.Field!, val?.Data!);

                    var checkBoxModelData = new CheckBoxModel()
                    {
                        Data = val?.Data!,
                        Value = val?.Value!,
                        FormattedValue = val?.FormattedValue!,
                        isChecked = isForeignDataEmpty ? false : UpdateResult(FilterValue!),
                        guid = uniqueId,
                        FUid = Column?.Uid!
                    };
                    FilteredData?.Add(checkBoxModelData);
                    CheckedData?.AddOrUpdateItem(uniqueId, checkBoxModelData);
                }
            }
            else
            {
                UpdateCheckBoxDataItems(CheckboxData!, isForeignDataEmpty, isInputSearch);
            }
            SelectAllHandler(CheckboxData);
            await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent!.GridEvents?.OnActionComplete, actionArgs).ConfigureAwait(true);
            await Parent.EventAggregator.NotifyAsync("ActionComplete", actionArgs).ConfigureAwait(true);
        }

        private void UpdateCheckBoxDataItems(IEnumerable<object> CheckboxData, bool isForeignDataEmpty, bool isInputSearch = false)
        {
            foreach (var value in CheckboxData)
            {
                var FilterValue = PropertyHelper?.GetObject(Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue! : Column?.Field!, value);
                var uniqueId = "cbox" + Guid.NewGuid().ToString();
                var val = Column?.IsForeignColumn() == true ? FilterValue : Parent!.PropHelper?.GetValue(value, Column?.Field);
                object? CheckBoxValue = null;
                var currentCulture = CultureInfo.CurrentCulture?.Name;

                if (val != null && val.ToString() != "null" && !string.IsNullOrEmpty(val.ToString()))
                {
                    if ((Column?.Type == ColumnType.Date || Column?.Type == ColumnType.DateTime) || Column?.Type == ColumnType.DateOnly || Column?.Type == ColumnType.TimeOnly)
                    {
                        if(val.GetType() == typeof(string))
                        { 
                            CheckBoxValue = val;
                        }
                        else if (val is DateTimeOffset)
                        {
                            CheckBoxValue = ((DateTimeOffset)val).ToString(Column?.Format, CultureInfo.CurrentCulture);
                        }
                        else if (val is DateOnly)
                        {
                            CheckBoxValue = ((DateOnly)val).ToString(Column?.Format, CultureInfo.CurrentCulture);
                        }
                        else if (val is TimeOnly)
                        {
                            CheckBoxValue = ((TimeOnly)val).ToString(Column?.Format, CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            CheckBoxValue = ((DateTime)val).ToString(Column?.Format, CultureInfo.CurrentCulture);
                        } 
                    }
                    if (Column?.Type == ColumnType.Integer || Column?.Type == ColumnType.Double || Column?.Type == ColumnType.Long || Column?.Type == ColumnType.Decimal)
                    {
                        CheckBoxValue = Convert.ToDouble(val, CultureInfo.CurrentCulture).ToString(Column?.Format, CultureInfo.CurrentCulture);
                    }
                    if (Column?.Type == ColumnType.Boolean)
                    {
                        CheckBoxValue = (bool)val ? Localizer?.GetText(GridLocaleKeys.FilterTrue) : Localizer?.GetText(GridLocaleKeys.FilterFalse);
                    }
                    if ((Column?.Type != ColumnType.Integer && Column?.Type != ColumnType.Double && Column?.Type != ColumnType.Long && Column?.Type != ColumnType.Decimal) && Column?.Type != ColumnType.Date && Column?.Type != ColumnType.DateTime && Column?.Type != ColumnType.Boolean && Column?.Type != ColumnType.DateOnly && Column?.Type != ColumnType.TimeOnly)
                    {
                        CheckBoxValue = val;
                    }
                }
                var data = new CheckBoxModel()
                {
                    Data = value,
                    Value = FilterValue!,
                    FormattedValue = CheckBoxValue!,
                    isChecked = isForeignDataEmpty ? false : UpdateResult(FilterValue!, isInputSearch),
                    guid = uniqueId,
                    FUid = Column?.Uid!
                };
                FilteredData?.Add(data);

                if (!isInputSearch)
                {
                    CheckBoxModel tempModel = AddPredicate?.Find(e => e?.Value != null && e.Value.Equals(data.Value))!;

                    if (tempModel != null)
                    {
                        AddPredicate?.Remove(tempModel);
                    }
                    AddPredicate?.Add(data);
                    if (DistinctPredicate?.ContainsKey(Column?.Uid!) != true)
                    {
                        DistinctPredicate?.Add(Column?.Uid!, AddPredicate!);
                    }
                    else if (DistinctPredicate?.ContainsKey(Column?.Uid!) == true)
                    {
                        DistinctPredicate[Column?.Uid!] = AddPredicate!;
                    }
                }

                var checkBoxVal = CheckBoxValue?.ToString();
                if ((Column?.Type != ColumnType.Integer && Column?.Type != ColumnType.Double && Column?.Type != ColumnType.Long && Column?.Type != ColumnType.Decimal) || IsClearIconPressed || (Parent!.FilterModule!.FilterIconIsClicked && SearchValue?.Length == 0))
                {
                    if (FilterDialogInitialRendering)
                    {
                        InitialBindedData?.AddOrUpdateItem(uniqueId, data);
                    }
                    CheckedData?.AddOrUpdateItem(uniqueId, data);
                }
                else if (CheckBoxValue != null &&
                    (Column?.FilterItemTemplate != null || ((currentCulture == "en-US" || currentCulture == "zh" || currentCulture == "ar") && (checkBoxVal?.Replace(",", "", StringComparison.Ordinal)?.Contains(SearchValue!, StringComparison.Ordinal) == true))
                    || (currentCulture == "de" && (checkBoxVal?.Replace(".", "", StringComparison.Ordinal)?.Contains(SearchValue!, StringComparison.Ordinal) == true))
                    || (currentCulture == "fr" && (string.Concat(checkBoxVal!.Where(c => !char.IsWhiteSpace(c))!.ToArray())!.Contains(SearchValue!, StringComparison.Ordinal) == true))
                    || (currentCulture != "en-US" && (checkBoxVal?.Contains(SearchValue!, StringComparison.Ordinal) == true))
                    ) || CheckBoxValue == null)
                {
                    CheckedData?.AddOrUpdateItem(uniqueId, data);
                }
            }
        }

        internal bool UpdateResult(object fValue, bool isInputSearch = false)
        {
            bool isTrue = true;
            bool isFiltered = false;
            if (Parent!.FilterSettings!.Columns != null)
            {
                foreach (GridFilterColumn FCol in Parent.FilterSettings.Columns)
                {
                    if (FCol?.Uid == Column?.Uid)
                    {
                        if (Parent.FilterSettings.Columns?.Where(col => col?.Uid == Column?.Uid)?.Count() == 2)
                        {
                            var primaryFilter = Parent.FilterSettings.Columns?.Where(col => col?.Uid == Column?.Uid)?.ToList()[0];
                            var secondaryFilter = Parent.FilterSettings.Columns?.Where(col => col?.Uid == Column?.Uid)?.ToList()[1];

                            if (primaryFilter?.Operator == secondaryFilter?.Operator && primaryFilter?.Operator == Operator.NotEqual && primaryFilter?.Predicate == secondaryFilter?.Predicate && (primaryFilter?.Predicate?.Equals("or", StringComparison.Ordinal) == true))
                            {
                                return true;
                            }
                            else if (primaryFilter?.Operator == secondaryFilter?.Operator && primaryFilter?.Operator == Operator.Equal && primaryFilter?.Predicate == secondaryFilter?.Predicate && (primaryFilter?.Predicate?.Equals("and", StringComparison.Ordinal) == true))
                            {
                                return false;
                            }
                        }

                        isFiltered = true;
#pragma warning disable BL0005
                        FCol!.Value = FCol.Value?.GetType()?.Name == "JsonElement" ? SfBaseUtils.ChangeType(FCol.Value, Column?.ValueType) : FCol.Value!;
#pragma warning restore BL0005
                        var CurrentFilterValue = FCol.Value == null ? "null" : FCol.Value;
                        fValue = fValue == null ? "null" : fValue;
                        var actualString = fValue;
                        var filterString = CurrentFilterValue;
                        if(actualString.ToString() == "null" || filterString?.ToString() == "null")
                        {
                            actualString = actualString.ToString();
                            filterString = filterString?.ToString();
                        }
                        else
                        {
                            var (actualValue, fitlerValue) = GetActualAndFilterValue(fValue, CurrentFilterValue);
                            actualString = actualValue;
                            filterString = fitlerValue;
                        }
                        if (!Parent.FilterSettings.EnableCaseSensitivity && !FCol.MatchCase && fValue is string && CurrentFilterValue is string)
                        {
                            filterString = CurrentFilterValue?.ToString()?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                            actualString = fValue.ToString()?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                        }
                        IComparable comparableActualValue = (IComparable)actualString!;
                        IComparable comparableCurrentFilterValue = (IComparable)filterString!;
                        int integerValue = 0;
                        if (FCol.Value != null && IsNumericValue(filterString!) && double.TryParse(FCol.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double result) && actualString != null && IsNumericValue(actualString) && double.TryParse(actualString.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double resultValue))
                        {
                            integerValue = (double.Parse(actualString.ToString()!, CultureInfo.InvariantCulture)).CompareTo(double.Parse(filterString?.ToString()!, CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            integerValue = comparableActualValue!.CompareTo(comparableCurrentFilterValue);
                        }
                        if (integerValue != 0 && integerValue != 1 && integerValue != -1)
                        {
                            if (Convert.ToInt32(actualString, CultureInfo.InvariantCulture) < Convert.ToInt16(filterString, CultureInfo.InvariantCulture))
                            {
                                integerValue = -1;
                            }
                            else
                            {
                                integerValue = 1;
                            }
                        }

                        if (FCol.Operator != Operator.Equal && FCol.Operator != Operator.NotEqual)
                        {
                            if (Parent.FilterSettings.Columns?.IndexOf(FCol) == 0 && string.Equals(FCol?.Predicate, "or", StringComparison.OrdinalIgnoreCase))
                            {
                                isTrue = false;
                            }
                            isFiltered = false;
                            isTrue = ColOperator(isTrue, FCol!, integerValue, actualString?.ToString()!, filterString?.ToString()!);
                        }
                        else
                        {
                            isTrue = FCol.Operator == Operator.NotEqual ? false : true;
                            if (actualString is DateTime actual && filterString is DateTime filter)
                            {
                                if (actualString is DateTime)
                                {
                                    actualString = actual.ToString(Column?.Format, CultureInfo.CurrentCulture);
                                }
                                if (filterString is DateTime)
                                {
                                    filterString = filter.ToString(Column?.Format, CultureInfo.CurrentCulture);
                                }
                            }
                            if (actualString?.ToString() == filterString?.ToString() && !isInputSearch)
                            {
                                return isTrue;
                            }
                            else if (actualString?.ToString() == filterString?.ToString() && isInputSearch && !(InputValue != null && !string.IsNullOrEmpty(InputValue.ToString())))
                            {
                                return isTrue;
                            }

                        }
                    }
                }
                if (isInputSearch && (InputValue != null && !string.IsNullOrEmpty(InputValue.ToString())))
                {
                    return true;
                }

                return isFiltered ? !isTrue : isTrue;
            }
            return isTrue;
        }
        private static bool IsNumericValue(object filterValue)
        {
            TypeCode filterTypeCode = Type.GetTypeCode(Nullable.GetUnderlyingType(filterValue.GetType()) ?? filterValue.GetType());
            return filterTypeCode >= TypeCode.SByte && filterTypeCode <= TypeCode.Decimal;
        }
        internal (object,object) GetActualAndFilterValue(object fValue, object CurrentFilterValue)
        {
            var actualString = fValue;
            var filterString = CurrentFilterValue;
            if (fValue is int)
            {
                filterString = Convert.ToInt32(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is long)
            {
                filterString = Convert.ToInt64(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is decimal)
            {
                filterString = Convert.ToDecimal(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is float)
            {
                filterString = Convert.ToSingle(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if(fValue is short)
            {
                filterString = Convert.ToInt16(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is ushort)
            {
                filterString = Convert.ToUInt16(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is uint)
            {
                filterString = Convert.ToUInt32(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is ulong)
            {
                filterString = Convert.ToUInt64(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is byte)
            {
                filterString = Convert.ToByte(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is sbyte)
            {
                filterString = Convert.ToSByte(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (Column?.Type == ColumnType.Date || Column?.Type == ColumnType.DateTime)
            {
                if (fValue is DateTimeOffset)
                {
                    actualString = GetDateTimeFromDateTimeOffset(fValue);
                }
                if (CurrentFilterValue is DateTimeOffset)
                {
                    filterString = GetDateTimeFromDateTimeOffset(CurrentFilterValue);
                }
            }
            else
            {
                actualString = fValue?.ToString();
                filterString = CurrentFilterValue?.ToString();
            }
            return (actualString!, filterString!);
        }

        private static DateTime GetDateTimeFromDateTimeOffset(object value)
        {
            DateTimeOffset fOffset = (DateTimeOffset)value;
            DateTime actualDateTime = fOffset.DateTime;
            return actualDateTime;
        }
        private static bool ColOperator(bool isTrue, GridFilterColumn FCol, int integerValue, string actualString, string filterString)
        {
            bool operatorResult;
            switch (FCol?.Operator)
            {
                case Operator.LessThan:
                    operatorResult = integerValue < 0;
                    break;
                case Operator.LessThanOrEqual:
                    operatorResult = integerValue <= 0;
                    break;
                case Operator.GreaterThan:
                    operatorResult = integerValue > 0;
                    break;
                case Operator.GreaterThanOrEqual:
                    operatorResult = integerValue >= 0;
                    break;
                case Operator.Contains:
                    operatorResult = actualString.Contains(filterString, StringComparison.Ordinal);
                    break;
                case Operator.DoesNotContain:
                    operatorResult = !actualString.Contains(filterString, StringComparison.Ordinal);
                    break;
                case Operator.StartsWith:
                    operatorResult = actualString.StartsWith(filterString, StringComparison.Ordinal);;
                    break;
                case Operator.DoesNotStartWith:
                    operatorResult = !actualString.StartsWith(filterString, StringComparison.Ordinal);
                    break;
                case Operator.EndsWith:
                    operatorResult = actualString.EndsWith(filterString, StringComparison.Ordinal);
                    break;
                case Operator.DoesNotEndWith:
                    operatorResult = !actualString.EndsWith(filterString, StringComparison.Ordinal);
                    break;
                case Operator.IsNull:
                    operatorResult = actualString == null || actualString == "null";
                    break;
                case Operator.IsNotNull:
                    operatorResult = !(actualString == null || actualString == "null");
                    break;
                case Operator.IsEmpty:
                    operatorResult = actualString != null && actualString.Length == 0;
                    break;
                case Operator.IsNotEmpty:
                    operatorResult = !(actualString == null || actualString == "null" || actualString.Length == 0);
                    break;
                default:
                    operatorResult = true;
                    break;
            }
            return string.Equals(FCol?.Predicate, "or", StringComparison.OrdinalIgnoreCase) ? (isTrue || operatorResult) : isTrue && operatorResult;
        }

        internal async Task GenerateForeignPredicate(Query DataManagerQuery, Query Foreignquery)
        {
            DataManagerQuery.Queries.RequiresCounts = true;
            IEnumerable<object> ForeignData = (IEnumerable<object>)((DataResult)await (Column?.DataManager?.ExecuteQuery<TContent>(DataManagerQuery))!.ConfigureAwait(true))?.Result!;
            Foreignquery.Distincts = new List<string>() { Column?.IsForeignColumn() == true ? Column?.ForeignKeyField! : Column?.Field! };
            Foreignquery.Queries.RequiresCounts = true;
            List<WhereFilter> WhereFilters = new List<WhereFilter>();
            foreach (var data in ForeignData!)
            {
                var FKeyValue = PropertyHelper?.GetObject(Column?.IsForeignColumn() == true ? Column?.ForeignKeyField! : Column?.Field!, data);
                WhereFilters.Add(new WhereFilter() { Field = Column?.Field, Operator = "equal", Condition = "or", value = FKeyValue, IgnoreCase = true, IgnoreAccent = true });
            }

            if (WhereFilters.Count > 0)
            {
                Foreignquery.Where(new WhereFilter() { Condition = "or", IsComplex = true, predicates = WhereFilters });
            }
        }

        internal object? GetForeignKeyValue(object data, object fdata)
        {
            var FData = GridUtils.GetForeignData(Column!, data, fdata);
            object? KeyName = null;
            foreach (var val in (List<object>)FData)
            {
                KeyName = PropertyHelper?.GetObject(Column?.ForeignKeyValue!, val);
            }

            return KeyName;
        }

        internal void SelectAllHandler(IEnumerable<object>? checkboxData = null)
        {
            int CheckCount = GetCheckedDataCount();
            int UnCheckCount = GetUnCheckedDataCount();
            
            SelectAllChk = CheckedData?.Count == CheckCount ? true : false;
            Intermediate = CheckedData?.Count != CheckCount && CheckedData?.Count != UnCheckCount ? true : false;

            if (Intermediate || SelectAllChk || IsCurrentSelectionChecked)
            {
                IsOkButtonDisabled = false;
            }
            else
            {
                IsOkButtonDisabled = !IsOkButtonDisabled;
            }

            if (checkboxData != null && !checkboxData.Any())
            {
                IsOkButtonDisabled = true;
            }
        }

        internal async void SelectAllClickHandler(string uid)
        {
            if(Parent!.FocusModule != null)
            {
                Parent.FocusModule.ClickedCheckBoxId = string.Empty;
                Parent.FocusModule.IsSelectAllClicked = true;
            }
            if (Intermediate || SelectAllChk)
            {
                IsOkButtonDisabled = true;
                SelectAllChk = false;
                Intermediate = false;
            }
            else
            {
                IsOkButtonDisabled = false;
                SelectAllChk = true;
            }

            foreach (var data in CheckedData)
            {
                var CheckData = (CheckBoxModel)data.Value;
                CheckData.isChecked = SelectAllChk;
            }
        }

        internal int GetUnCheckedDataCount()
        {
            var checkcount = 0;
            foreach (var val in CheckedData)
            {
                var data = (CheckBoxModel)val.Value;
                if (!data.isChecked)
                {
                    checkcount++;
                }
            }

            return checkcount;
        }

        internal int GetCheckedDataCount()
        {
            var checkcount = 0;
            foreach (var val in CheckedData)
            {
                var data = (CheckBoxModel)val.Value;
                if (data.isChecked)
                {
                    checkcount++;
                }
            }

            return checkcount;
        }

        private async Task InputArgs(Microsoft.AspNetCore.Components.ChangeEventArgs args)
        {
            SearchValue = args?.Value?.ToString()!;
            VisibleProperty = true;
            var actionArgs = new ActionEventArgs<TContent>() { RequestType = Grids.Action.FilterSearchBegin, SearchString = SearchValue, ColumnName = Column?.Field!, Parent = Parent, CheckboxListData = null! };
            await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent!.GridEvents?.OnActionBegin, actionArgs).ConfigureAwait(true);
            await Parent.EventAggregator.NotifyAsync("ActionBegin", actionArgs).ConfigureAwait(true);
            var filterSearchingArgs = new CheckboxFilterSearchingEventArgs() { SearchText = SearchValue!, ColumnName = Column?.Field!, Parent = Parent, CheckboxListData = null! };
            if ((Parent.GridEvents?.CheckboxFilterSearching.HasDelegate == true) || Parent.IsRenderedFromTreeGrid)
            {
                if(Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("CheckboxFilterSearching", filterSearchingArgs).ConfigureAwait(true);
                else
                    await SfBaseUtils.InvokeEvent<CheckboxFilterSearchingEventArgs>(Parent.GridEvents?.CheckboxFilterSearching, filterSearchingArgs).ConfigureAwait(true);
                
                excelSearchOperator = actionArgs.ExcelSearchOperator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                if(!SearchValue!.Equals(filterSearchingArgs.SearchText, StringComparison.Ordinal))
                {
                    await Parent.InvokeMethod("sfBlazor.Grid.searchClear", new object[] { Parent.DataId, $"{Parent.ID}_SearchBox", filterSearchingArgs.SearchText}).ConfigureAwait(true);
                }
                SearchValue = filterSearchingArgs.SearchText;
            }
            if (Parent.GridEvents?.OnActionBegin.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                if (!SearchValue.Equals(actionArgs.SearchString, StringComparison.Ordinal))
                {
                    await Parent.InvokeMethod("sfBlazor.Grid.searchClear", new object[] { Parent.DataId, $"{Parent.ID}_SearchBox", actionArgs.SearchString }).ConfigureAwait(true);
                }
                SearchValue = actionArgs.SearchString;
            }
            excelSearchOperator = actionArgs.ExcelSearchOperator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture);
            SearchValue = ((Column?.ValueType == typeof(Guid) || Column?.ValueType == typeof(Guid?)) && !Guid.TryParse(SearchValue, out var newguid)) ? null! : SearchValue;
            InputValue = (!string.IsNullOrEmpty(SearchValue) && Column?.ValueType != null) ? GetActualValue(SearchValue)! : null!;
            if (string.IsNullOrEmpty(SearchValue))
            {
                CancelIcon = string.Empty;
                SelectAllHandler();
            }
            else if (Parent.DataManager != null && Parent.DataManager.DataAdaptor != null && Parent.DataManager.DataAdaptor.IsRemote())
            {

            }
            else
            {
                CancelIcon = "e-chkcancel-icon";
                SelectAllHandler();
            }

            if (InputValue == null || InputValue.ToString() != "grd-search-notvalid" || (actionArgs.CheckboxListData != null && actionArgs.CheckboxListData.Any()) || (filterSearchingArgs.CheckboxListData != null && filterSearchingArgs.CheckboxListData.Any()) )
            {
                await UpdateDataSource(true, actionArgs.CheckboxListData ?? filterSearchingArgs.CheckboxListData).ConfigureAwait(true);
            }
            VisibleProperty = false;
        }

        private string GetClassName()
        {
            var filterType = Parent!.FilterModule!.GetFilterType(Column!);
            if (filterType == "Excel")
            {
                return "e-filter-popup e-excelfilter";
            }
            else
            {
                return "e-filter-popup e-checkboxfilter";
            }
        }

        private string GetLabelClassName()
        {
            return Parent!.FilterSettings!.AllowTextWrap ? "e-fltrcheck e-wrapfilter" : "e-fltrcheck";
        }

        private void OnBlur()
        {
            InputFocus = string.Empty;
        }

        private void OnFocus()
        {
            InputFocus = "e-input-focus";
        }

        private async Task CancelIconClick(MouseEventArgs e)
        {
            IsClearIconPressed = true;
            var eventArgs = e;
            CancelIcon = string.Empty;
            InputValue = null!;
            VisibleProperty = true;
            await UpdateDataSource().ConfigureAwait(true);
            await Parent!.InvokeMethod("sfBlazor.Grid.searchClear", new object[] { Parent.DataId, $"{Parent.ID}_SearchBox" }).ConfigureAwait(true);
            VisibleProperty = false;
            IsClearIconPressed = false;
        }

        internal class CheckBoxModel
        {
            public object? Value { get; set; }

            public object? Data { get; set; }

            public object? FormattedValue { get; set; }

            public bool isChecked { get; set; } = true;

            public string? guid { get; set; }

            public string? FUid { get; set; }
        }
    }
}