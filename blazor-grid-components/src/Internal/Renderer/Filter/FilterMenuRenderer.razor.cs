using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Internal;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Syncfusion.Blazor.Grids.Internal
{

    /// <summary>
    /// Represents a renderer for the filter menu in a grid.
    /// </summary>
    public partial class FilterMenuRenderer<TContent>
    {
        #region UI Component References

        private SfDialog? MenuDialog { get; set; }
        private SfDialog? MenuAdaptiveDialog { get; set; }

        private SfAutoComplete<string, TContent>? AutoComplete { get; set; }
        private SfDatePicker<DateTime?>? DatePicker { get; set; }
        private SfDatePicker<DateOnly?>? DateOnlyPicker { get; set; }
        private SfTimePicker<TimeOnly?>? TimeOnlyPicker { get; set; }
        private SfDateTimePicker<DateTime?>? DateTimePicker { get; set; }

        private SfNumericTextBox<double?>? NumericValueasDouble { get; set; }
        private SfNumericTextBox<int?>? NumericValueasInt { get; set; }
        private SfNumericTextBox<long?>? NumericValueasLong { get; set; }

        private SfDropDownList<bool?, DropdownBoolean>? BoolDropDown { get; set; }
        private SfDropDownList<string, object>? NumberOperatorDropDown { get; set; }
        private SfDropDownList<string, object>? BoolOperatorDropDown { get; set; }
        private SfDropDownList<string, object>? StringOperatorDropDown { get; set; }

        #endregion

        #region Component Parameters

        /// <summary>
        /// Gets or sets the parent grid.
        /// </summary>
        [CascadingParameter]
        public SfGrid<TContent>? Parent { get; set; }

        /// <summary>
        /// Gets or sets the horizontal position value for the filter dialog.
        /// </summary>
        [Parameter]
        public string Xvalue { get; set; } = "right";

        /// <summary>
        /// Gets or sets the vertical position value for the filter dialog.
        /// </summary>
        [Parameter]
        public string Yvalue { get; set; } = "bottom";

        /// <summary>
        /// Gets or sets visibility for the filter dialog.
        /// </summary>
        [Parameter]
        public bool IsVisible { get; set; } = false;

        /// <summary>
        /// Gets or sets the column associated with the filter menu.
        /// </summary>
        [Parameter]
        public GridColumn? Column { get; set; }

        #endregion

        #region Component State Properties

        private List<object>? NumberDropDown { get; set; }
        private List<object>? BooleanDropDown { get; set; }
        private List<object>? StringDropDown { get; set; }

        private string? Foperator { get; set; }
        private FilterAutoCompleteModel model = null!;

        private int? _stringIndex { get; set; }
        private int? _numberIndex { get; set; }
        private int? _booleanIndex { get; set; }

        private GridFilterColumn? GFilterColumn { get; set; }
        private object? ModelInstance { get; set; }
        private TContent? CustomFilterItem { get; set; }

        private bool _isInteger { get; set; }
        private bool _isLong { get; set; }

        #endregion

        #region Filter Dialog Position State

        private string XPosition { get; set; } = "center";
        private string YPosition { get; set; } = "center";

        #endregion

        #region Filter Value State

        private string? menuFilterValue { get; set; }
        private string inputValues = string.Empty;

        /// <summary>
        /// Defined to get the values typed in the Autocomplete input and set it to the variable bounded
        /// to the Value property of Autocomplete during rendering.
        /// </summary>
        private object? autoCompleteFilterValue { get; set; }

        #endregion

        #region UI Control Configuration

        private IDictionary<string, object> autoCompleteAttributes = new Dictionary<string, object>();
        private IDictionary<string, object> numericTextBoxAttributes = new Dictionary<string, object>();
        private IDictionary<string, object> dateTimePickerAttributes = new Dictionary<string, object>();
        private IDictionary<string, object> datePickerAttributes = new Dictionary<string, object>();
        private IDictionary<string, object> timePickerAttributes = new Dictionary<string, object>();
        private IDictionary<string, object> dropDownListAttributes = new Dictionary<string, object>();
        private Dictionary<string, object> title = new Dictionary<string, object>()
        {
           { "title", "Close"}
        };
        private Dictionary<string, object> MaxHeight = new Dictionary<string, object>()
        {
           { "data-sf-style", "max-height:100%"}
        };

        private bool enableAutoFill { get; set; } = true;
        private object? step { get; set; }
        private object? minValue { get; set; }
        private object? maxValue { get; set; }

        #endregion

        #region Component Services

        private ISyncfusionStringLocalizer? Localizer;

        #endregion

        #region Utility Properties

        private int Index { get; set; }
        private List<DropdownBoolean>? DropData { get; set; }
        /// <summary>
        /// Applies animation effects to the filter dialog.
        /// </summary>
        public DialogEffect Effects { get; set; } = DialogEffect.None;

        #endregion

        #region Component Lifecycle

        /// <summary>
        /// Initializes the component and registers event handlers for filter menu dialogs.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            Localizer = Parent?.Localizer!;
            Type colType = Column?.ValueType!;
            Type myGeneric = typeof(PredicateModel<>);
            if (colType != null)
            {
                Type constructedClass = myGeneric.MakeGenericType(colType);
                ModelInstance = Activator.CreateInstance(constructedClass);
            }
            DropData = new List<DropdownBoolean> {
            new DropdownBoolean() { Value= true, Text= Localizer.GetText(GridLocaleKeys.True)},
            new DropdownBoolean() {Value= false, Text= Localizer.GetText(GridLocaleKeys.False) },
        };
            GetData();
            if (Column?.FilterEditorSettings is AutoCompleteFilterParams autoParam && autoParam.AutoCompleteParams != null)
            {
                AutoCompleteModel model = autoParam.AutoCompleteParams;
                autoCompleteAttributes = new Dictionary<string, object>()
            {
                { nameof(model.MinLength), model.MinLength },
                { nameof(model.ShowClearButton),model.ShowClearButton},
                { nameof(model.ShowPopupButton),model.ShowPopupButton},
                { nameof(model.DebounceDelay),model.DebounceDelay},
                { nameof(model.Highlight),model.Highlight},
                { nameof(model.EnableVirtualization),model.EnableVirtualization},
                { nameof(model.SuggestionCount),model.SuggestionCount}



            };
                enableAutoFill = model.Autofill;

            }

            else if (Column?.FilterEditorSettings is DateTimeFilterParams dateTimeParam && dateTimeParam.DateTimePickerParams != null)
            {

                DateTimePickerModel<object> model = dateTimeParam.DateTimePickerParams;
                dateTimePickerAttributes = new Dictionary<string, object>
            {
                {nameof(model.CssClass),model.CssClass},
                {nameof(model.EnableRtl),model.EnableRtl},
                {nameof(model.HtmlAttributes),model.HtmlAttributes},
                {nameof(model.Readonly),model.Readonly},
                {nameof(model.Format),model.Format},
                {nameof(model.ShowClearButton),model.ShowClearButton},
                { nameof(model.TimeFormat), model.TimeFormat },
                { nameof(model.FirstDayOfWeek), model.FirstDayOfWeek }

            };
            }

            else if (Column?.FilterEditorSettings is TimeFilterParams timeParam && timeParam.TimePickerParams != null)
            {
                TimePickerModel<object> model = timeParam.TimePickerParams;
                timePickerAttributes = new Dictionary<string, object>
            {
                {nameof(model.CssClass),model.CssClass},
                {nameof(model.EnableRtl),model.EnableRtl},
                {nameof(model.HtmlAttributes),model.HtmlAttributes},
                {nameof(model.Readonly),model.Readonly},
                {nameof(model.ShowClearButton),model.ShowClearButton},
                {nameof(model.Step),model.Step},
                { nameof(model.Format), model.Format },
                { nameof(model.Min), model.Min },
                { nameof(model.Max), model.Max }
            };
            }
            else if (Column?.FilterEditorSettings is DateFilterParams dateParam && dateParam.DatePickerParams != null)
            {
                DatePickerModel model = dateParam.DatePickerParams;
                datePickerAttributes = new Dictionary<string, object>
            {
                {nameof(model.CssClass),model.CssClass},
                {nameof(model.EnableRtl),model.EnableRtl},
                {nameof(model.Readonly),model.Readonly},
                {nameof(model.ShowClearButton),model.ShowClearButton},
                {nameof(model.Format),model.Format},
                {nameof(model.FirstDayOfWeek),model.FirstDayOfWeek},
                {nameof(model.HtmlAttributes),model.HtmlAttributes}
            };
            }

            else if (Column?.FilterEditorSettings is NumericFilterParams numericParam && numericParam.NumericTextBoxParams != null)
            {
                NumericTextBoxModel<object> model = numericParam.NumericTextBoxParams;
                numericTextBoxAttributes = new Dictionary<string, object>
            {
                {nameof(model.CssClass),model.CssClass},
                {nameof(model.EnableRtl),model.EnableRtl},
                {nameof(model.Readonly),model.Readonly},
                {nameof(model.ShowClearButton),model.ShowClearButton},
                {nameof(model.HtmlAttributes),model.HtmlAttributes},
                {nameof(model.Currency),model.Currency},
                {nameof(model.Decimals),model.Decimals!},
                { nameof(model.ValidateDecimalOnType), model.ValidateDecimalOnType }

            };
                step = model.Step;
                minValue = model.Min;
                maxValue = model.Max;
            }
            else if (Column?.FilterEditorSettings is DropDownFilterParams dropParam && dropParam.DropDownListParams != null)
            {
                DropDownListModel<object, object> model = dropParam.DropDownListParams;
                dropDownListAttributes = new Dictionary<string, object>
            {
                {nameof(model.CssClass), model.CssClass},
                {nameof(model.EnableRtl), model.EnableRtl},
                {nameof(model.Readonly) , model.Readonly},
                {nameof(model.ShowClearButton), model.ShowClearButton},
                {nameof(model.DebounceDelay),model.DebounceDelay},
                {nameof(model.HtmlAttributes),model.HtmlAttributes},
            };
            }
        }

        /// <summary>
        /// Determines whether the component should be rendered.
        /// </summary>
        protected override bool ShouldRender()
        {
            return this.Parent?.FilterModule != null ? !this.Parent.FilterModule!.IsCustomFilterApplied : false;
        }

        #endregion

        #region Event Handlers

        private async Task CustomValueHandler(CustomValueSpecifierEventArgs<TContent> args)
        {
            menuFilterValue = args.Text;
            await CustomFilterValueMaintain().ConfigureAwait(true);
            args.Item = CustomFilterItem!;
        }

        private async void BeginHandler(ActionBeginEventArgs args)
        {
            args.EnableFullLookup = false;
            if (Parent!.FilterSettings != null && Parent.FilterModule != null && Parent.FilterSettings.Columns?.Count > 0)
            {
                this.Parent.FilterModule.IsCustomFilterApplied = true;
            }
        }

        private async Task CustomFilterQuery(Syncfusion.Blazor.DropDowns.FilteringEventArgs args)
        {
            inputValues = args.Text;
            if (Parent?.FilterModule != null)
            {
                this.Parent.FilterModule.IsCustomFilterApplied = true;
            }
            AutoComplete!.FilterOperator = StringOperatorDropDown?.Value;
        }

        private async Task AutoCompleteDataBound(Syncfusion.Blazor.DropDowns.DataBoundEventArgs args)
        {
            if (Parent!.FilterModule?.FilterIconColumn != null)
            {
                var isComplex = Parent.FilterModule.FilterIconColumn?.Field?.Contains('.', StringComparison.Ordinal) == true;
                if (Parent.IsRenderedFromTreeGrid || isComplex)
                {
                    if (!AutoComplete!.ListData!.Any())
                    {
                        AutoComplete.IsFilterQueryUpdated = true;
                        if (!string.IsNullOrEmpty(inputValues))
                        {
#pragma warning disable BL0005
                            AutoComplete.Value = inputValues;
#pragma warning restore BL0005
                        }
                    }
                    menuFilterValue = inputValues;
                    await CustomFilterValueMaintain().ConfigureAwait(true);
                    AutoComplete.IsFilterQueryUpdated = false;
                }
            }
        }

        private async Task CreatedHandler(Object args)
        {
            if ((Parent!.FilterSettings!.Columns != null && Parent.FilterSettings.Columns.Count > 0 && Parent.EnablePersistence) || (Parent.FilterSettings.Columns != null && Parent.FilterSettings.Columns.Count > 0))
            {
                var actualValue = Parent.FilterSettings.Columns[0]?.ActualValue;
                menuFilterValue = actualValue != null ? actualValue.ToString()! : null!;
            }
            await CustomFilterValueMaintain().ConfigureAwait(true);
        }

        #endregion

        #region Filter Dialog Management

        private async Task Rendered()
        {
            Parent!.FilterModule!.FilterIconIsClicked = true;
            Parent.FilterModule.FilterDialogInstance = this.MenuDialog!;
            if (Parent.FilterModule.FilterIconIsClicked && Parent.FilterModule.FilterIconColumn != null)
            {
                if (Parent.FilterModule.IsColumnMenuFilter)
                {
                    var dlgID = Column?.Uid + "-flmdlg";
                    string[] positions = await Parent.InvokeMethod<string[]>("sfBlazor.Grid.filterPopupRender", false, new object[] { Parent.DataId, dlgID, Column?.Uid!, "menu", Parent.FilterModule.IsColumnMenuFilter }).ConfigureAwait(true);
                    if (positions != null)
                    {
                        XPosition = positions[0];
                        YPosition = positions[1];
                        await OpenFilterDialog(XPosition, YPosition).ConfigureAwait(true);
                    }
                }
                else
                {
                    await OpenFilterDialog().ConfigureAwait(true);
                }
            }
            this.Parent.FilterModule.IsCustomFilterApplied = false;
        }

        internal void Closed()
        {
            this.Parent!.FilterModule!.FilterIconIsClicked = false;
            NumberOperatorDropDown?.Dispose();
            StringOperatorDropDown?.Dispose();
            BoolOperatorDropDown?.Dispose();
            this.Parent.EventAggregator.Trigger("FilterComponentUpdate", true);
            if (Parent.ShowColumnMenu)
            {
                Parent.FocusModule?.Focus("", "", headerUid: Column?.Uid).GetAwaiter();
            }
        }

        private async Task AdaptiveRendered()
        {
            await Parent!.InvokeMethod("sfBlazor.Grid.customFilterDialog", new object[] { Parent.DataId, $"{Parent.ID}custommenufilter" }).ConfigureAwait(true);
        }

        internal async Task CloseHandler()
        {

            await this.MenuAdaptiveDialog!.HideAsync().ConfigureAwait(true);
            if (Parent!.FilterModule != null)
            {
                Parent.FilterModule.FilterIconIsClicked = false;
            }
            Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
        }

        private async Task CustomFilterValueMaintain()
        {
            if (menuFilterValue != null && typeof(TContent).GetConstructor(Type.EmptyTypes) != null)
            {
                var filterValue = menuFilterValue;
                var splits = this.Column?.Field?.Split(".");
                var item = Activator.CreateInstance<TContent>()!;
                bool treegridInstance = (splits?.Contains("DataItem") == true);
                dynamic data = Activator.CreateInstance<TContent>()!;
                string column = splits?.Length > 1 ? splits[splits.Length - 1] : this.Column?.Field!;
                dynamic treeComplexData = Activator.CreateInstance<TContent>()!;
                dynamic complexData = Activator.CreateInstance<TContent>()!;
                if (splits?.Length > 1)
                {
                    string complexColumn = splits[splits.Length - 2];
                    string treeComplexColumn = splits.Length == 3 ? splits[splits.Length - 3] : "";
                    if (splits.Contains("DataItem"))
                    {
                        var propInfo = item?.GetType()?.GetProperty(!string.IsNullOrEmpty(treeComplexColumn) ? treeComplexColumn : complexColumn);
                        data = ReflectionExtension.TryCreateInstance(propInfo?.PropertyType);
                        treeComplexData = CloneUtils.Clone(data, propInfo?.GetType());
                    }
                    if (data is DynamicObject)
                    {
                        if (!treegridInstance)
                        {
                            ReflectionExtension.SetValueToDynamicObject(data, column, filterValue);
                            ReflectionExtension.SetValueToDynamicObject(complexData, complexColumn, data);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(treeComplexColumn))
                            {
                                ReflectionExtension.SetValueToDynamicObject(treeComplexData, column, filterValue);
                                ReflectionExtension.SetValueToDynamicObject(data, complexColumn, treeComplexData);
                                ReflectionExtension.SetValue(complexData, treeComplexColumn, data);
                            }
                            else
                            {
                                ReflectionExtension.SetValueToDynamicObject(data, column, filterValue);
                                ReflectionExtension.SetValue(complexData, complexColumn, data);
                            }
                        }
                    }
                    else if (data is ExpandoObject)
                    {
                        if (!treegridInstance)
                        {
                            ReflectionExtension.SetValueToExpandoObject((IDictionary<string, object>)data, column, filterValue);
                            ReflectionExtension.SetValueToExpandoObject((IDictionary<string, object>)complexData, complexColumn, data);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(treeComplexColumn))
                            {
                                ReflectionExtension.SetValueToExpandoObject((IDictionary<string, object>)treeComplexData, column, filterValue);
                                ReflectionExtension.SetValueToExpandoObject((IDictionary<string, object>)data, complexColumn, treeComplexData);
                                ReflectionExtension.SetValue(complexData, treeComplexColumn, data);
                            }
                            else
                            {
                                ReflectionExtension.SetValueToExpandoObject((IDictionary<string, object>)data, column, filterValue);
                                ReflectionExtension.SetValue(complexData, complexColumn, data);
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(treeComplexColumn))
                        {
                            var propInfo = item?.GetType()?.GetProperty(treeComplexColumn);
                            var tempObj = ReflectionExtension.TryCreateInstance(propInfo?.PropertyType);
                            var prop = tempObj?.GetType()?.GetProperty(complexColumn);
                            treeComplexData = ReflectionExtension.TryCreateInstance(prop?.PropertyType);
                            ReflectionExtension.SetValue(treeComplexData, column, filterValue);
                            ReflectionExtension.SetValue(tempObj, complexColumn, treeComplexData);
                            ReflectionExtension.SetValue(complexData, treeComplexColumn, tempObj);
                        }
                        else
                        {
                            var propInfo = item?.GetType()?.GetProperty(splits[splits.Length - 2]);
                            var tempObj = ReflectionExtension.TryCreateInstance(propInfo?.PropertyType);
                            tempObj?.GetType()?.GetProperty(column)?.SetValue(tempObj, filterValue);
                            ReflectionExtension.SetValue(complexData, complexColumn, tempObj);
                        }
                    }
                    item = (TContent)complexData;
                }
                else
                {
                    if (data is DynamicObject)
                    {
                        ReflectionExtension.SetValueToDynamicObject(data, column, filterValue);
                    }
                    else if (data is ExpandoObject)
                    {
                        ReflectionExtension.SetValueToExpandoObject((IDictionary<string, object>)data, column, filterValue);
                    }
                    else
                    {
                        ReflectionExtension.SetValue(data, column, filterValue);
                    }
                    item = (TContent)data;
                }
                CustomFilterItem = item;
                if (!(AutoComplete != null && AutoComplete.IsFilterQueryUpdated))
                {
                    await (AutoComplete?.AddItemsAsync(new List<TContent>() { item })!).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Opens the filter dialog at the specified position and triggers related events before rendering.
        /// </summary>
        public async Task OpenFilterDialog(string XPosition = "", string YPosition = "")
        {
            GetData();
            if ((Parent!.GridEvents?.FilterDialogOpening.HasDelegate == true) || Parent.IsRenderedFromTreeGrid)
            {
                var dialogOpeningArgs = new FilterDialogOpeningEventArgs() { Cancel = false, ColumnName = this.Column?.Field!, Parent = Parent };
                List<object> defaultFilterOperators = GetFilterOperators(Column!);
                List<IFilterOperator> filterOperators = new List<IFilterOperator>();
                foreach (var item in defaultFilterOperators)
                {
                    filterOperators.Add(new FilterOperators { Value = ((string)DataUtil.GetObject("Value", item)), Text = ((string)DataUtil.GetObject("Text", item)) });
                }
                dialogOpeningArgs.FilterOperators = filterOperators;
                if (Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("FilterDialogOpening", dialogOpeningArgs).ConfigureAwait(true);
                else
                    await SfBaseUtils.InvokeEvent<FilterDialogOpeningEventArgs>(Parent?.GridEvents?.FilterDialogOpening, dialogOpeningArgs).ConfigureAwait(true);
                if (dialogOpeningArgs.Cancel)
                {
                    EventCancelingOperation();
                    return;
                }
                SetFilterOperators(Column!, dialogOpeningArgs);
            }
            if ((Parent?.GridEvents?.OnActionBegin.HasDelegate == true) || (Parent != null && Parent.IsRenderedFromTreeGrid))
            {
                var actionArgs = new ActionEventArgs<TContent>() { RequestType = Grids.Action.FilterBeforeOpen, Cancel = false, ColumnName = this.Column?.Field!, Parent = Parent };
                actionArgs.FilterOperators = (List<object>)GetFilterOperators(Column!);

                if (Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("ActionBegin", actionArgs).ConfigureAwait(true);
                else
                    await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent.GridEvents?.OnActionBegin, actionArgs).ConfigureAwait(true);

                if (actionArgs.Cancel)
                {
                    EventCancelingOperation();
                    return;
                }
                SetFilterOperators(Column!, actionArgs: actionArgs);
            }

            if (Parent?.FilterModule != null && Parent.FilterModule.IsColumnMenuFilter)
            {
                Xvalue = XPosition;
                Yvalue = YPosition;
            }

            StateHasChanged();
            await (this.MenuDialog?.ShowAsync()!).ConfigureAwait(true);
            var CompleteArgs = new ActionEventArgs<TContent>() { RequestType = Grids.Action.FilterAfterOpen, Cancel = false, ColumnName = this.Column?.Field ?? string.Empty, Parent = Parent! };

            if (Parent != null && Parent.IsRenderedFromTreeGrid)
                await Parent.EventAggregator.NotifyAsync("ActionComplete", CompleteArgs).ConfigureAwait(true);
            else
                await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent?.GridEvents?.OnActionComplete, CompleteArgs).ConfigureAwait(true);
            if ((Parent?.GridEvents?.FilterDialogOpened.HasDelegate == true) || (Parent != null && Parent.IsRenderedFromTreeGrid))
            {
                var dialogOpenedArgs = new FilterDialogOpenedEventArgs() { ColumnName = this.Column?.Field!, Parent = Parent };
                if (Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("FilterDialogOpened", dialogOpenedArgs).ConfigureAwait(true);
                else
                    await SfBaseUtils.InvokeEvent<FilterDialogOpenedEventArgs>(Parent.GridEvents?.FilterDialogOpened, dialogOpenedArgs).ConfigureAwait(true);
            }
            if (Parent != null && Parent.FilterModule != null)
            {
                Parent.FilterModule.FilterIconIsClicked = true;
            }

            Foperator = null!;
        }

        private void EventCancelingOperation()
        {
            if (Parent != null && Parent.FilterModule != null)
                this.Parent.FilterModule.FilterIconIsClicked = false;
            this.Parent!.EventAggregator.Trigger("FilterComponentUpdate", null!);
            return;
        }

        private void SetFilterOperators(GridColumn column, FilterDialogOpeningEventArgs? filterArgs = null, ActionEventArgs<TContent>? actionArgs = null)
        {
            List<object> customFilterOperator = new List<object>();

            if (filterArgs != null)
            {
                List<IFilterOperator>? filterOperators = filterArgs.FilterOperators;
                if (filterOperators != null)
                {
                    customFilterOperator = filterOperators
                        .Select(item => new Operator { Value = item.Value, Text = item.Text })
                        .Cast<object>()
                        .ToList();
                }
            }
            else if (actionArgs != null)
            {
                customFilterOperator = actionArgs.FilterOperators!;
            }
            if ((column.Type.Equals(ColumnType.String)) || (column.ValueType != null && (column.ValueType.IsEnum)))
            {
                _stringIndex = (StringDropDown != customFilterOperator) ? null : _stringIndex;
                StringDropDown = customFilterOperator;
            }
            if (Column != null && Column.Type.Equals(ColumnType.Boolean))
            {
                _booleanIndex = (BooleanDropDown != customFilterOperator) ? null : _booleanIndex;
                BooleanDropDown = customFilterOperator;
            }
            else
            {
                _numberIndex = (NumberDropDown != customFilterOperator) ? null : _numberIndex;
                NumberDropDown = customFilterOperator;
            }
        }

        #endregion

        #region Filter Operator Retrieval

        private List<object> GetFilterOperators(GridColumn column)
        {
            switch (column?.Type)
            {
                case ColumnType.Boolean:
                    return BooleanDropDown!;
                case ColumnType.String:
                    return StringDropDown!;
                default:
                    return NumberDropDown!;
            }
        }

        #endregion

        #region Filter Value Extraction

        private async Task FilterBtnClick()
        {
            object value = "";
            var filterOperator = "";
            if (Parent?.FilterModule != null)
            {
                this.Parent.FilterModule.IsCustomFilterApplied = false;
            }
            if (Column?.FilterTemplate != null && ModelInstance != null)
            {
                value = ModelInstance.GetType()?.GetProperty("Value")?.GetValue(ModelInstance)!;
                filterOperator = GetColumnFilterOperator();
            }
            else
            {
                switch (Column?.Type)
                {
                    case ColumnType.Integer:
                    case ColumnType.Double:
                    case ColumnType.Long:
                    case ColumnType.Decimal:
                        value = _isInteger ? NumericValueasInt?.Value! : _isLong ? NumericValueasLong?.Value! : NumericValueasDouble?.Value!;
                        filterOperator = NumberOperatorDropDown?.Value;
                        break;

                    case ColumnType.Date:
                        value = DatePicker?.Value!;
                        if (Column?.ValueType?.Name == "DateTime" && DateTime.TryParse(value.ToString(), out var dateTimeValue) && dateTimeValue.TimeOfDay != TimeSpan.Zero)
                        {
                            var result = Parent!.DataManager?.Json?.Where(item => item?.GetType()?.GetProperty(Column?.Field!)?.PropertyType == typeof(DateTime))?.Where(item =>
                            {
                                var dateValue = (DateTime)item?.GetType()?.GetProperty(Column?.Field!)?.GetValue(item, null!)!;
                                var resultDate = new DateTime(dateValue.Year, dateValue.Month, dateValue.Day, 0, 0, 0) == (DateTime)value;
                                return resultDate;
                            })
                            ?.ToList();
                            value = result?.Count > 0 ? DataUtil.GetObject(Column?.Field!, result[0]) : value;
                        }
                        filterOperator = NumberOperatorDropDown?.Value;
                        break;
                    case ColumnType.DateOnly:
                        value = DateOnlyPicker?.Value!;
                        filterOperator = NumberOperatorDropDown?.Value;
                        break;
                    case ColumnType.TimeOnly:
                        value = TimeOnlyPicker?.Value!;
                        filterOperator = NumberOperatorDropDown?.Value;
                        break;
                    case ColumnType.DateTime:
                        value = DateTimePicker?.Value!;
                        if (Column?.ValueType?.Name == "DateTime" && DateTime.TryParse(value.ToString(), out var dateTime) && dateTime.TimeOfDay != TimeSpan.Zero)
                        {
                            var result = Parent!.DataManager?.Json?.Where(item => item?.GetType()?.GetProperty(Column?.Field!)?.PropertyType == typeof(DateTime))?.Where(item =>
                            {
                                var dateValue = (DateTime)item?.GetType()?.GetProperty(Column.Field)?.GetValue(item, null!)!;
                                var resultDate = new DateTime(dateValue.Year, dateValue.Month, dateValue.Day, dateValue.Hour, dateValue.Minute, 0) == (DateTime)value;
                                return resultDate;
                            })?.ToList();
                            value = result?.Count > 0 ? DataUtil.GetObject(Column?.Field!, result[0]) : value;
                        }

                        filterOperator = NumberOperatorDropDown?.Value;
                        break;
                    case ColumnType.Boolean:
                        value = BoolDropDown?.Value!;
                        filterOperator = BoolOperatorDropDown?.Value;
                        break;
                    case ColumnType.String:
                        value = (Column?.IsForeignColumn() == true) && model != null ? model.FilterValue! ?? AutoComplete?.Value! : AutoComplete?.Value!;
                        filterOperator = StringOperatorDropDown?.Value;
                        break;
                }
            }
            if (!Parent!.EnableAdaptiveUI || (Parent.EnableAdaptiveUI && (Parent.AdaptiveUIMode.Equals(AdaptiveMode.Mobile) && !Parent.SyncfusionService.IsDeviceMode) || (Parent.AdaptiveUIMode.Equals(AdaptiveMode.Desktop) && Parent.SyncfusionService.IsDeviceMode)))
            {
             #if NET10_0_OR_GREATER
                if (AutoComplete != null)
                    {
                        await AutoComplete.DisposeAsync().ConfigureAwait(true);
                    }
            #else
                AutoComplete?.Dispose();
            #endif

                if ((Column?.IsForeignColumn() == true) && (Column?.IsGridForeignColumn == true))
                {
                    Column?.AutoCompleteDispose();
                }
                await (this.MenuDialog?.HideAsync()!).ConfigureAwait(true);
            }

            else
            {
                await (this.MenuAdaptiveDialog?.HideAsync()!).ConfigureAwait(true);
                Parent.EventAggregator.Trigger("CloseAdaptiveDialog", null!);
            }
            if (Parent.ColumnMenuInstance != null)
            {
#pragma warning disable BL0005
                Parent.ColumnMenuInstance.CssClass = $"e-hide-menu e-{Parent.ID}-column-menu e-grid-column-menu e-grid-menu";
#pragma warning restore BL0005
                Parent.ColumnMenuClass = $"e-hide-menu e-{Parent.ID}-column-menu e-grid-column-menu e-grid-menu";
                Parent.IsColumnMenuFilter = false;
                Parent.ColumnMenuInstance?.Close();
            }
            if (this.Parent != null && this.Parent.FilterModule != null)
            {
                this.Parent.FilterModule.FilterIconColumn = null!;
                this.Parent.FilterModule.FilterIconIsClicked = false;
            }

            await Parent!.FilterModule!.FilterByColumn(Column?.Field ?? string.Empty, Filter<TContent>.GetOperator(filterOperator ?? string.Empty), value!, null!, Parent.FilterSettings!.EnableCaseSensitivity, Parent.FilterSettings.IgnoreAccent, null!, null!, Column?.Uid).ConfigureAwait(true);
            model = null!;
        }


        private string? GetColumnFilterOperator()
        {

            switch (Column!.Type)
            {
                case ColumnType.Integer:
                case ColumnType.Double:
                case ColumnType.Long:
                case ColumnType.Decimal:
                case ColumnType.Date:
                case ColumnType.DateOnly:
                case ColumnType.TimeOnly:
                case ColumnType.DateTime:
                    return NumberOperatorDropDown?.Value?.ToString();
                case ColumnType.Boolean:
                    return BoolOperatorDropDown?.Value?.ToString();
                default:
                    return StringOperatorDropDown?.Value?.ToString();

            }

        }

        #endregion

        #region Filter Data Processing

        private void GetData()
        {
            List<object> NumberData = new List<object>();
            NumberData.Add(new Operator { Value = "equal", Text = Localizer!.GetText(GridLocaleKeys.Equal) });
            NumberData.Add(new Operator { Value = "notequal", Text = Localizer.GetText(GridLocaleKeys.NotEqual) });
            NumberData.Add(new Operator { Value = "greaterthan", Text = Localizer.GetText(GridLocaleKeys.GreaterThan) });
            NumberData.Add(new Operator { Value = "greaterthanorequal", Text = Localizer.GetText(GridLocaleKeys.GreaterThanOrEqual) });
            NumberData.Add(new Operator { Value = "lessthan", Text = Localizer.GetText(GridLocaleKeys.LessThan) });
            NumberData.Add(new Operator { Value = "lessthanorequal", Text = Localizer.GetText(GridLocaleKeys.LessThanOrEqual) });
            NumberData.Add(new Operator { Value = "isnull", Text = Localizer.GetText(GridLocaleKeys.IsNull) });
            NumberData.Add(new Operator { Value = "isnotnull", Text = Localizer.GetText(GridLocaleKeys.IsNotNull) });
            this.NumberDropDown = NumberData;

            List<object> BooleanData = new List<object>();
            BooleanData.Add(new Operator { Value = "equal", Text = Localizer.GetText(GridLocaleKeys.Equal) });
            BooleanData.Add(new Operator { Value = "notequal", Text = Localizer.GetText(GridLocaleKeys.NotEqual) });
            this.BooleanDropDown = BooleanData;

            List<object> StringData = new List<object>();

            StringData.Add(new Operator { Value = "startswith", Text = Localizer.GetText(GridLocaleKeys.StartsWith) });
            StringData.Add(new Operator { Value = "doesnotstartwith", Text = Localizer.GetText(GridLocaleKeys.DoesNotStartWith) });
            StringData.Add(new Operator { Value = "endswith", Text = Localizer.GetText(GridLocaleKeys.EndsWith) });
            StringData.Add(new Operator { Value = "doesnotendwith", Text = Localizer.GetText(GridLocaleKeys.DoesNotEndWith) });
            StringData.Add(new Operator { Value = "contains", Text = Localizer.GetText(GridLocaleKeys.Contains) });
            StringData.Add(new Operator { Value = "doesnotcontain", Text = Localizer.GetText(GridLocaleKeys.DoesNotContain) });
            StringData.Add(new Operator { Value = "equal", Text = Localizer.GetText(GridLocaleKeys.Equal) });
            StringData.Add(new Operator { Value = "notequal", Text = Localizer.GetText(GridLocaleKeys.NotEqual) });
            StringData.Add(new Operator { Value = "isempty", Text = Localizer.GetText(GridLocaleKeys.IsEmpty) });
            StringData.Add(new Operator { Value = "isnotempty", Text = Localizer.GetText(GridLocaleKeys.IsNotEmpty) });
            StringData.Add(new Operator { Value = "like", Text = Localizer.GetText(GridLocaleKeys.Like) });
            this.StringDropDown = StringData;
            this.StringDropDown = StringData;

        }

        #endregion

        #region UI Helper Methods

        private async Task ClearBtnClick()
        {
            if (Parent?.FilterModule != null)
            {
                this.Parent.FilterModule.IsCustomFilterApplied = false;
            }
            model = null!;
            if (!Parent!.EnableAdaptiveUI || (Parent.EnableAdaptiveUI && (Parent.AdaptiveUIMode.Equals(AdaptiveMode.Mobile) && !Parent.SyncfusionService.IsDeviceMode) || (Parent.AdaptiveUIMode.Equals(AdaptiveMode.Desktop) && Parent.SyncfusionService.IsDeviceMode)))
            {
                await (this.MenuDialog?.HideAsync()!).ConfigureAwait(true);
            }
            else
            {
                await (this.MenuAdaptiveDialog?.HideAsync()!).ConfigureAwait(true);
                Parent.EventAggregator.Trigger("CloseAdaptiveDialog", null!);
            }
            if (Parent.ColumnMenuInstance != null)
            {
#pragma warning disable BL0005
                Parent.ColumnMenuInstance.CssClass = $"e-hide-menu e-{Parent.ID}-column-menu e-grid-column-menu e-grid-menu";
#pragma warning restore BL0005
                Parent.ColumnMenuClass = $"e-hide-menu e-{Parent.ID}-column-menu e-grid-column-menu e-grid-menu";
                Parent.IsColumnMenuFilter = false;
                Parent.ColumnMenuInstance.Close();
            }
            if (this.Parent != null && this.Parent.FilterModule != null)
            {
                this.Parent.FilterModule.FilterIconColumn = null!;
                this.Parent.FilterModule.FilterIconIsClicked = false;
                await Parent.FilterModule.RemoveFilterColumnByField(this.Column?.Field ?? string.Empty, this.Column?.Uid ?? string.Empty).ConfigureAwait(true);
            }
        }

        private static bool GetClass(int? stringIndex, List<object> StringDropDownValue)
        {
            if (stringIndex.HasValue && stringIndex >= 0 && stringIndex < StringDropDownValue?.Count)
            {
                if (StringDropDownValue[stringIndex.Value] is Operator operatorItem)
                {
                    string value = operatorItem.Value!;
                    string text = operatorItem.Text!;

                    if (value == "isempty" || value == "isnotempty")
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool GetNumberClass(int? stringIndex, List<object> NumberDropDown)
        {
            if (stringIndex.HasValue && stringIndex >= 0 && stringIndex < NumberDropDown?.Count)
            {
                if (NumberDropDown[stringIndex.Value] is Operator operatorItem)
                {
                    string value = operatorItem.Value!;
                    string text = operatorItem.Text!;

                    if (value == "isnull" || value == "isnotnull")
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion

        #region Helper Methods

        private static bool IsNullOrEmptyOperator(string OperatorValue)
        {
            switch (OperatorValue?.ToUpperInvariant())
            {
                case "ISNULL":
                case "ISNOTNULL":
                case "ISEMPTY":
                case "ISNOTEMPTY":
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Inner Classes

        private class DropdownBoolean
        {
            public bool Value { get; set; }
            public string? Text { get; set; }
        }

        private class Operator : IFilterOperator
        {
            public string Value { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
        }

        private class FilterOperators : IFilterOperator
        {
            public string Value { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
        }

        #endregion
    }
}
